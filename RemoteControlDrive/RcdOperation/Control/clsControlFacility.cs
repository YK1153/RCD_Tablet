using CommWrapper;
using RcdCmn;
using RcdOperation.PlcControl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RcdDao.MFacilityDao;
using static RcdOperation.PlcControl.PLCControl;

namespace RcdOperation.Control
{
    public partial class ControlMain
    {

        #region ## 設備制御処理 ##

        /// <summary>
        /// 設備ステータスを要求指示ステータスに変更<para/>
        /// 変更完了後、指示受信Tcpサーバーより完了電文を返信
        /// </summary>
        /// <param name="facilityID">対象設備ID</param>
        /// <param name="reqStatus">要求ステータス</param>
        /// <param name="rcvSrv">指示受信Tcpサーバー</param>
        /// <param name="connID">接続ID</param>
        internal void SetFacilityRequestedStatus(string facilityID, int reqStatus)//, CommSrv rcvSrv, int connID, string clientName)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                // 設備ID 存在チェック
                if (!m_dicFacilityStatus.Keys.Contains(facilityID))
                {
                    LOGGER.Warn($"[設備制御不可] 存在しない設備IDが制御指示されました。[設備ID: {facilityID}]");
                    return;
                }

                //FacilityControl currentControl = new FacilityControl(rcvSrv, connID, facilityID, reqStatus, clientName);
                FacilityControl currentControl = new FacilityControl(facilityID, reqStatus);

                // 設備制御中チェック
                FacilityControl control = null;
                lock (m_facilityControlThreads)
                {
                    control = m_facilityControlThreads
                        .SingleOrDefault(c => c.FacilityID == facilityID && c.CtrlValue == reqStatus);
                }

                if (control != null)
                {
                    if (control.m_reset != null)
                    {
                        lock (control.m_reset)
                        {
                            control.m_reset.Set();
                        }

                    }
                    LOGGER.Warn($"既 制御中の設備制御 受信: 設備ID: {facilityID}, 制御指定値: {reqStatus}");
                }


                // 設備制御実施
                LOGGER.Info($"設備制御開始　　: 設備ID: {facilityID}, 制御指定値: {reqStatus}");

                lock (m_facilityControlThreads)
                {
                    m_facilityControlThreads.Add(currentControl);
                }

                int facilityTypeID = m_dicFacilityStatus[facilityID].FacilityTypeID;

                int result = ControlSetFacility(facilityID, facilityTypeID, reqStatus, true);

                bool ctrlDone = false;
                switch (result)
                {
                    case PLC_CONTROL_RESULT.C_FACILITY_CTRL_ERROR:
                        LOGGER.Error($"設備制御失敗　　: 設備ID: {facilityID}, 制御指定値: {reqStatus}");

                        //GetCtrlIDListFromFacilityID(facilityID)
                        //.ForEach((unitID) =>
                        //{
                        //    CtrlUnitState ctrlUnit = m_ctrlUnitStates.SingleOrDefault(unit => unit.CtrlUnit.ID == unitID);
                        //    if (ctrlUnit != null && ctrlUnit.IsRunning())
                        //    {
                        //        LOGGER.Info($"[走行停止] 設備制御失敗のため、対象経路生成部の走行停止処理を行います。{Res.C_CTRL_UNIT}ID:{unitID}");
                        //        ErrorPreProcess(ctrlUnit, Res.ErrorStatus.FACILITY_ERR.Code, ErroredFrom.Mng, GetFacilityCtrlFailMsg(facilityID, reqStatus));
                        //    }
                        //});
                        LOGGER.Info($"[走行停止] 設備制御失敗のため、走行停止処理を行います。");
                        EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}", Res.ErrorStatus.FACILITY_ERR.Code);
                        break;
                    case PLC_CONTROL_RESULT.C_FACILITY_CTRL_INTERRUPTED:
                        // TODO: 他の設備制御指示発生
                        LOGGER.Warn($"設備制御取消　　: 設備ID: {facilityID}, 制御指定値: {reqStatus}");
                        break;
                    case PLC_CONTROL_RESULT.C_FACILITY_CTRL_NORMAL_END:
                        ctrlDone = CheckFacCtrlDone(currentControl, facilityID, reqStatus);
                        break;
                    default:
                        break;
                }

                // 設備制御完了処理
                if (ctrlDone)
                {
                    LOGGER.Info($"設備制御正常終了: 設備ID: {facilityID}, 制御指定値: {reqStatus}");
                    List<FacilityControl> targetToSendDone = m_facControlWaits
                                .Where(wait => wait.FacilityID == facilityID && wait.CtrlValue == reqStatus)
                                .ToList();
                    bool alreadyListed = targetToSendDone.SingleOrDefault(wait => wait.Equals(currentControl)) != null;
                    if (!alreadyListed)
                    {
                        targetToSendDone.Add(currentControl);
                    }

                    foreach (FacilityControl toSend in targetToSendDone)
                    {
                        SendFacCtrlNormalEndMsg(toSend);
                        m_facControlWaits.Remove(toSend);
                    }
                }
                else
                {
                    // 設備が指示値に変更できず、確認処理終了
                    bool alreadyWaiting = m_facControlWaits.SingleOrDefault(wait => wait.Equals(currentControl)) != null;
                    if (!alreadyWaiting)
                    {
                        m_facControlWaits.Add(currentControl);
                    }
                }

                lock (currentControl.m_reset)
                {
                    currentControl.m_reset.Dispose();
                }

                lock (m_facilityControlThreads)
                {
                    m_facilityControlThreads.Remove(currentControl);
                }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 設備IDで指定された設備を種別、指示値に合わせて制御する。
        /// </summary>
        /// <param name="facilityID">設備ID</param>
        /// <param name="facilityTypeID">設備種別</param>
        /// <param name="ctrlValue">指示値</param>
        /// <param name="forceCtrl">強制制御フラグ</param>
        /// <returns>制御結果</returns>
        /// <see cref="DIO_CONTROL_RESULT"/>
        private int ControlSetFacility(string facilityID, int facilityTypeID, int ctrlValue, bool forceCtrl)
        {
            // 3灯信号機: 赤、2灯信号機: 赤 の場合、別処理
            if (facilityTypeID == 0 && ctrlValue == 1) // 3灯信号機 赤
            {
                // 黄色 → 赤
                List<FacCtrlStep> steps = new List<FacCtrlStep>() { FacCtrlStep.OneShot(2), FacCtrlStep.OneShot(1) };
                int interval = GetFaciliyIntervalConfig();

                return m_PLCControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, interval);
            }
            else if (facilityTypeID == 1 && ctrlValue == 1) // 2灯信号機 赤
            {
                int interval = GetFaciliyIntervalConfig();
                int repeatInterval = GetFaciliyRepeatIntervalConfig();
                // 青点滅(青→消灯: 繰り返し) → 赤
                List<FacCtrlStep> steps = new List<FacCtrlStep>() { FacCtrlStep.Repeat(new int[] { 0, 3 }, repeatInterval), FacCtrlStep.OneShot(1) };

                return m_PLCControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, interval);
            }
            else // 一般制御
            {
                List<FacCtrlStep> steps = new List<FacCtrlStep>() { FacCtrlStep.OneShot(ctrlValue) };

                return m_PLCControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, 0);
            }
        }

        /// <summary>
        /// 設備IDで指定された設備が指示値になるまで繰り返し確認する。
        /// </summary>
        /// <returns>設備が指示値に変更された場合True、
        /// 指示値に変更される前に繰り返し確認がキャンセルされた場合False</returns>
        /// </summary>
        private bool CheckFacCtrlDone(FacilityControl targetControl, string facilityID, int ctrlValue)
        {
            do
            {
                if (m_dicFacilityStatus[facilityID].Status1 == ctrlValue)
                {
                    return true;
                }
            }
            while (!targetControl.m_reset.WaitOne(m_STATUS_UPDATE_INTERVAL));

            return false;
        }


        /// <summary>
        /// 対象設備の異常時設定ステータス値を取得
        /// </summary>
        /// <param name="facilityIDs">対象設備IDリスト</param>
        /// <returns>対象設備情報と異常時設定ステータス値</returns>
        private Dictionary<FacilityStatus, int> GetFacilityErrorSetValues(List<string> facilityIDs)
        {
            Dictionary<FacilityStatus, int> facilityErrorSetValues = new Dictionary<FacilityStatus, int>();

            foreach (string facilityID in facilityIDs)
            {
                FacilityStatus fac = m_dicFacilityStatus[facilityID];
                if (fac == null)
                {
                    LOGGER.Warn($"[異常時設定値 取得不可] 指定されたIO設備が存在しません。設備ID: {facilityID}");
                    continue;
                }
                FacilityType facType = m_facilityTypes.SingleOrDefault(typeInfo => typeInfo.SID == fac.FacilityTypeID);
                if (facType == null)
                {
                    LOGGER.Warn($"[異常時設定値 取得不可] 指定されたIO設備の種別を判定できません。設備ID: {facilityID}, 設備種別ID: {fac.FacilityTypeID}");
                    continue;
                }
                int? ErrorStatus = facType.ErrorStatus;
                if (ErrorStatus != null)
                {
                    facilityErrorSetValues.Add(fac, (int)ErrorStatus);
                }
            }
            return facilityErrorSetValues;
        }

        /// <summary>
        /// 対象設備の異常解除時設定ステータス値を取得
        /// </summary>
        /// <param name="facilityIDs">対象設備IDリスト</param>
        /// <returns>対象設備情報と異常解除時設定ステータス値</returns>
        internal Dictionary<FacilityStatus, int> GetFacilityErrorResolveValues(List<string> facilityIDs)
        {
            Dictionary<FacilityStatus, int> facilityErrorResolveValues = new Dictionary<FacilityStatus, int>();

            foreach (string facilityID in facilityIDs)
            {
                FacilityStatus fac = m_dicFacilityStatus[facilityID];
                if (fac == null)
                {
                    LOGGER.Warn($"[異常解除] グループメンバーIO設備が存在しません。設備ID: {facilityID}");
                    continue;
                }
                FacilityType facType = m_facilityTypes.SingleOrDefault(typeInfo => typeInfo.SID == fac.FacilityTypeID);
                int? resolveStatus = facType.ResolveStatus;
                if (resolveStatus != null)
                {
                    facilityErrorResolveValues.Add(fac, (int)resolveStatus);
                }
            }
            return facilityErrorResolveValues;
        }

        /// <summary>
        /// 並行設備制御
        /// </summary>
        /// <param name="controlList">制御指示リスト</param>
        /// <returns>制御結果リスト(設備ID、制御処理結果)</returns>
        internal Dictionary<string, int> ParallelFacilityControl(Dictionary<FacilityStatus, int> controlList)
        {
            Dictionary<string, int> ctrlResults = new Dictionary<string, int>();

            for (int i = 0; i < controlList.Count; i++)
            {
                try
                {
                    FacilityStatus facility = controlList.ElementAt(i).Key;
                    int ctrlValue = controlList.ElementAt(i).Value;

                    LOGGER.Info($"[設備ステータスを変更] 設備ID: {facility.FacilityID}, 設備名: {facility.FacilityName}, 設定値: {ctrlValue}");
                    int ctrlRes = ControlSetFacility(facility.FacilityID, facility.FacilityTypeID, ctrlValue, true);
                    ctrlResults.Add(facility.FacilityID, ctrlRes);
                }
                catch (Exception ex)
                {
                    ExceptionProcess.ComnExceptionConsoleProcess(ex);
                    FacilityStatus facility = controlList.ElementAt(i).Key;
                    ctrlResults.Add(facility.FacilityID, PLC_CONTROL_RESULT.C_FACILITY_CTRL_ERROR);
                }
            }

            return ctrlResults;
        }

        /// <summary>
        /// 設備制御失敗メッセージ作成
        /// </summary>
        /// <param name="FacilityID">対象設備ID</param>
        /// <param name="FailedCtrlVal">制御失敗指示値</param>
        /// <returns>設備制御失敗エラーメッセージ</returns>
        internal string GetFacilityCtrlFailMsg(string FacilityID, int FailedCtrlVal)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                FacilityStatus erroredFacility = m_dicFacilityStatus.Values.SingleOrDefault(f => f.FacilityID == FacilityID);
                string facilityName = "";
                string ctrlValName = "";
                if (erroredFacility == null)
                {
                    LOGGER.Warn($"[メッセージ作成失敗] 該当する設備情報が見つけれません。" +
                        $"異常内容はID、指示値で表示されます。設備ID:{FacilityID}, 制御値:{FailedCtrlVal}");
                    facilityName = $"設備ID:{FacilityID}";
                    ctrlValName = $"制御値:{FailedCtrlVal}";
                }
                else
                {
                    facilityName = erroredFacility.FacilityName;

                    FacilityStatusMsg statMsg = m_facilityStatusMsgs
                            .Where(fStatus => fStatus.FacilityTypeID == erroredFacility.FacilityTypeID)
                            .Where(fStatus => fStatus.StatusCode == FailedCtrlVal)
                            .SingleOrDefault();
                    ctrlValName = statMsg == null ? $"制御値:{FailedCtrlVal}" : statMsg.StatusMsg;
                }

                return string.Format(ErrorStatusGetMsg(Res.ErrorStatus.FACILITY_ERR.Code), facilityName, ctrlValName);
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        /// <summary>
        /// 設備制御指示の正常終了応答送信
        /// </summary>
        /// <param name="connID">指示受信接続ID</param>
        /// <param name="facilityID">制御設備ID</param>
        /// <param name="ctrlValue">制御指示値</param>
        private void SendFacCtrlNormalEndMsg(FacilityControl facCtrlInfo)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {               
                LOGGER.Info($"[受信内容]設備ID：{facCtrlInfo.FacilityID}");
                for (int i = 0; i <= m_otherArea.Count() - 1; i++)
                {
                    if (m_otherArea[i].otherarea.FacilityID == facCtrlInfo.FacilityID)
                    {
                        m_otherArea[i].status = 1;
                        m_otherArea[i].stop = false;
                    }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 設備シーケンス制御時間間隔取得
        /// </summary>
        private int GetFaciliyIntervalConfig()
        {
            string strInterval = ConfigurationManager.AppSettings["FACILITY_CTRL_INTERVAL"];
            int interval = 2000;
            bool isValidConfig = int.TryParse(strInterval, out interval);
            if (!isValidConfig)
            {
                throw new UserException($"設定値 FACILITY_CTRL_INTERVAL: {strInterval}を整数に変更できませんでした。");
            }
            return interval;
        }

        /// <summary>
        /// 設備繰り返し制御時間間隔取得
        /// </summary>
        private int GetFaciliyRepeatIntervalConfig()
        {
            string strInterval = ConfigurationManager.AppSettings["FACILITY_CTRL_REPEAT_INTERVAL"];
            int interval = 300;
            bool isValidConfig = int.TryParse(strInterval, out interval);
            if (!isValidConfig)
            {
                throw new UserException($"設定値 FACILITY_CTRL_REPEAT_INTERVAL: {strInterval}を整数に変更できませんでした。");
            }
            return interval;
        }

        /// <summary>
        /// 管制モードに合うように対象設備を更新制御
        /// </summary>
        /// <param name="facStatusList">現在設備ステータスリスト</param>
        private void UpdateManagerModeFacility(List<FacilityStatus> facStatusList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                LOGGER.Debug($"管制モード表示設備更新");

                Dictionary<FacilityStatus, int> unmatchingFacilities =
                    facStatusList
                    .Where(f => f.FacilityTypeID == (int)FacilityTypes.MngModeViewer)
                    .Where(f => f.Status1 != (int)mode.Selector)
                    .ToDictionary(f => f, f => (int)mode.Selector);

                if (unmatchingFacilities.Count <= 0)
                {
                    LOGGER.Debug($"設備ステータスが管制モードに一致するため処理スキップ");
                    return;
                }
                else
                {
                    ParallelFacilityControl(unmatchingFacilities);
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion



        #region # FacilityControl #
        public class FacilityControl
        {
            //public FacilityControl(CommSrv rcvdServer, int connId, string facilityID, int ctrlValue, string clientName = null)
            public FacilityControl(string facilityID, int ctrlValue)
            {
                //ReceivedServer = rcvdServer;
                //ConnectionID = connId;
                FacilityID = facilityID;
                CtrlValue = ctrlValue;
                //ClientName = clientName ?? "";
                m_reset = new ManualResetEvent(false);
            }

            //public CommSrv ReceivedServer { get; private set; }
            //public int ConnectionID { get; set; }
            public string FacilityID { get; private set; }
            public int CtrlValue { get; private set; }
            //public string ClientName { get; private set; }

            public ManualResetEvent m_reset { get; private set; }

            public override bool Equals(object obj)
            {
                if (!(obj is FacilityControl))
                {
                    return false;
                }

                FacilityControl tarControl = (FacilityControl)obj;
                bool ret = true;

                //if (ClientName != tarControl.ClientName)
                //{
                //    ret = false;
                //}
                //if (ConnectionID != tarControl.ConnectionID)
                //{
                //    ret = false;
                //}
                if (FacilityID != tarControl.FacilityID)
                {
                    ret = false;
                }
                if (CtrlValue != tarControl.CtrlValue)
                {
                    ret = false;
                }
                return ret;
            }

            public override int GetHashCode()
            {
                var hashCode = -898392680;
                //hashCode = hashCode * -1521134295 + EqualityComparer<CommSrv>.Default.GetHashCode(ReceivedServer);
                //hashCode = hashCode * -1521134295 + ConnectionID.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FacilityID);
                hashCode = hashCode * -1521134295 + CtrlValue.GetHashCode();
                //hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClientName);
                hashCode = hashCode * -1521134295 + EqualityComparer<ManualResetEvent>.Default.GetHashCode(m_reset);
                return hashCode;
            }
        }
        #endregion
    }
}
