using AtrptShare;
using RcdCmn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RcdCmn.Res;
using static RcdDao.MFacilityDao;
using static RcdDao.MSystemValue;
using static RcdOperation.PlcControl.PLCControl;

namespace RcdOperation.Control
{
    public partial class ControlMain
    {


        /// <summary>
        /// 異常時に動作させる設備の制御
        /// </summary>
        public void  ErrorFacilityProc()
        {
            // グループ内のすべてのIO設備を取得
            List<string> errSetTargetFacilityies = m_dicFacilityStatus.Keys.ToList(); ;

            Dictionary<FacilityStatus, int> facilityErrorSetValues = GetFacilityErrorSetValues(errSetTargetFacilityies);

            LOGGER.Info($"[走行停止] 対象設備を走行停止時ステータスに変更します。");
            Dictionary<string, int> ctrlResults = ParallelFacilityControl(facilityErrorSetValues);

            // check facility ctrl result
            bool facCtrlNormalEnded = ctrlResults.Values.All(result => result == PLC_CONTROL_RESULT.C_FACILITY_CTRL_NORMAL_END);
            if (!facCtrlNormalEnded)
            {
                List<string> erroredFaciliys = ctrlResults
                    .Where(facRes => facRes.Value != PLC_CONTROL_RESULT.C_FACILITY_CTRL_NORMAL_END)
                    .Select(facRes => facRes.Key)
                    .ToList();
                LOGGER.Error($"[走行停止失敗] 下記の設備の制御に失敗しました。");
                LOGGER.Error($"    失敗設備ID: {string.Join(", ", erroredFaciliys)}");
            }

        }

        /// <summary>
        /// 走行停止(異常)状態を解除
        /// </summary>
        internal void ErrorResolvegroup()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                // グループ内のすべてのIO設備のステータスを変更
                List<string> errSetTargetFacilityies = m_dicFacilityStatus.Keys.ToList();

                LOGGER.Info($"[異常解除] 制御グループ内アンドンを消灯");
                OffAllAndonInGroup();


                OnErrorResolveDone();

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// グループ内ｱﾝﾄﾞﾝ設備全消灯
        /// </summary>
        /// <param name="groupID">対象グループID</param>
        internal void OffAllAndonInGroup()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                Dictionary<FacilityStatus, int> andonOffList =
                    m_dicFacilityStatus.Keys.ToList() // グループ内の設備IDを取得
                    .Where(facId => m_dicFacilityStatus[facId].FacilityTypeID == (int)FacilityTypes.Andon) // その中でアンドンのみ取得
                    .Select(facId => m_dicFacilityStatus[facId])
                    .ToDictionary(status => status, ctrlVal => 0); // アンドン OFFの 指示リスト作成

                ParallelFacilityControl(andonOffList);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// グループの異常がすべて解除された時
        /// </summary>
        /// <param name="groupID">解除グループID</param>
        internal void OnErrorResolveDone()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {

                //List<CtrlUnitState> resolvedUnits = m_ctrlUnitStates
                //    .Where(unit => errInfo.Errors.Keys.Contains(unit.CtrlUnit.ID))
                //    .ToList();

                //foreach (CtrlUnitState resolvedUnit in resolvedUnits)
                //{
                //    string carNo = resolvedUnit.CarNo;
                //    string carSpec = m_dicCarStatus[carNo].CarSpec;

                //    if (m_dicCarStatus.ContainsKey(carNo))
                //    {
                //        m_dicCarStatus[resolvedUnit.CarNo].Resolved();
                //        m_dicCarStatus.Remove(carNo);
                //        OPBotton = DManegerStatus.WaitOPBotton;
                //    }

                //    string errMsg = errInfo.Errors[resolvedUnit.CtrlUnit.ID].StatusMsg;
                //    LOGGER.Info($",[走行結果],搬送異常,車種ID:,{carSpec},ボデーNo:,{carNo},異常内容:,{errMsg}");
                //    resolvedUnit.EndControl();
                //}

                //m_dicErrorInfos.Remove(groupID);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 設備を原位置復帰させる
        /// </summary>
        private void InitFacilityProc()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                // グループ内のすべてのIO設備を取得
                List<string> errSetTargetFacilityies = m_dicFacilityStatus.Keys.ToList(); ;

                Dictionary<FacilityStatus, int> facilityErrorSetValues = GetFacilityErrorResolveValues(errSetTargetFacilityies);

                LOGGER.Info($"[設備初期化] 対象設備を初期ステータスに変更します。");
                Dictionary<string, int> ctrlResults = ParallelFacilityControl(facilityErrorSetValues);

                // check facility ctrl result
                bool facCtrlNormalEnded = ctrlResults.Values.All(result => result == PLC_CONTROL_RESULT.C_FACILITY_CTRL_NORMAL_END);
                if (!facCtrlNormalEnded)
                {
                    List<string> erroredFaciliys = ctrlResults
                        .Where(facRes => facRes.Value != PLC_CONTROL_RESULT.C_FACILITY_CTRL_NORMAL_END)
                        .Select(facRes => facRes.Key)
                        .ToList();
                    LOGGER.Error($"[設備初期化失敗] 下記の設備の制御に失敗しました。");
                    LOGGER.Error($"    失敗設備ID: {string.Join(", ", erroredFaciliys)}");
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }


    }
}
