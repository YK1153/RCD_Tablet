using CommWrapper;
using RcdCmn;
using RcdOperationSystemConst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RcdOperation.Control.CommCam;
using static RcdOperationSystemConst.OperationConst;

namespace RcdOperation.Control
{
    public partial class ControlMain
    {
        public CommSrv m_TerminalSrv;

        public string C_NM_TERMINAL="Terminal";

        public void ConstructorCommTerminal()
        {
            m_TerminalSrv = new CommSrv(ConnectionConst.Terminal_Srv_Port);
            m_TerminalSrv.RecieveMessage += new CommSrv.RecieveMessageEventHandler(terminalReceiveMessage);
            m_TerminalSrv.LogWriterRcv = new LogWriteComm("Terminal", "Rcv");
            m_TerminalSrv.LogWriterSnd = new LogWriteComm("Terminal", "Snd");
            m_TerminalSrv.StartListen();
        }

        public void CommTerminalDispose()
        {
            if (m_TerminalSrv != null)
            {
                m_TerminalSrv.EndListen(true);
                m_TerminalSrv = null;
            }
        }

        public void terminalReceiveMessage(RecieveMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.ConnectID}:{e.Message}");

                string rcvid = CommMsgBase.GetMsgID(e.Message);

                switch (rcvid)
                {
                    case RcvChangeOpePreparation.ID:
                        RcvChangeOpePreparationProc(e);
                        break;
                    case RcvChangeOpeMode.ID:
                        RcvChangeOpeModeProc(e);
                        break;
                    case RcvChangeContinueStatus.ID:
                        RcvChangeContinueStatusProc(e);
                        break;
                    case RcvCarStop.ID:
                        RcvCarStopProc(e);
                        break;
                    case RcvStopBuzzer.ID:
                        RcvStopBuzzerProc(e);
                        break;
                    case RcvResolution.ID:
                        RcvResolutionProc(e);
                        break;
                    case RcvInitFacility.ID:
                        RcvInitFacilityProc(e);
                        break;
                    case RcvShiftCheck.ID:
                        RcvShiftCheckProc(e);
                        break;
                    case RcvFacilityCtrl.ID:
                        RcvFacilityCtrlProc(e);
                        break;
                    default:
                        {
                            LOGGER.Warn($"{C_NM_TERMINAL}不明な電文を受信：{e.Message}", $"{e.ConnectID}");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                ExceptionProcess.UserExceptionProcess(ex);
            }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #region ## 受信時処理 ##
        /// <summary>
        /// 運転準備切替処理
        /// </summary>
        public void RcvChangeOpePreparationProc(RecieveMessageEventArgs e)
        {
            RcvChangeOpePreparation rcvmsg = new RcvChangeOpePreparation(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvChangeOpePreparation.NameStr}:{e.Message}", $"{e.ConnectID}");

            ChangeReady(rcvmsg.Value == "1" ? true : false);
        }
        /// <summary>
        /// 各個連続切替処理
        /// </summary>
        /// <param name="e"></param>
        public void RcvChangeOpeModeProc(RecieveMessageEventArgs e)
        {
            RcvChangeOpeMode rcvmsg = new RcvChangeOpeMode(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvChangeOpeMode.NameStr}:{e.Message}", $"{e.ConnectID}");

            ChangeSelector(rcvmsg.Value == "1" ? ModeSelector.Auto : ModeSelector.Manual);
        }
        /// <summary>
        /// 連続OK処理
        /// </summary>
        /// <param name="e"></param>
        public void RcvChangeContinueStatusProc(RecieveMessageEventArgs e)
        {
            RcvChangeContinueStatus rcvmsg = new RcvChangeContinueStatus(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvChangeContinueStatus.NameStr}:{e.Message}", $"{e.ConnectID}");

            // 運転準備ON かつ 連続 のとき
            if (mode.Ready == true && mode.Selector == ModeSelector.Auto)
            {
                ChangeContimueON(rcvmsg.Value == "1" ? true : false);
            }
        }
        /// <summary>
        /// 走行停止
        /// </summary>
        /// <param name="e"></param>
        public void RcvCarStopProc(RecieveMessageEventArgs e)
        {
            RcvCarStop rcvmsg = new RcvCarStop(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvCarStop.NameStr}:{e.Message}", $"{e.ConnectID}");

            if (mode.ContinueON == true && mode.Selector == ModeSelector.Auto)
            {
                // 制御中車両 走行停止 
                EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}", Res.ErrorStatus.VIEWER_ERR.Code);
            }
        }
        public void RcvStopBuzzerProc(RecieveMessageEventArgs e)
        {
            RcvStopBuzzer rcvmsg = new RcvStopBuzzer(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvStopBuzzer.NameStr}:{e.Message}", $"{e.ConnectID}");

            //※ 各個のとき　運転準備OFFでもアンドン停止可能なのであれば
            if ( mode.Selector == ModeSelector.Manual)
            {

            }
        }
        public void RcvResolutionProc(RecieveMessageEventArgs e)
        {
            RcvResolution rcvmsg = new RcvResolution(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvResolution.NameStr}:{e.Message}", $"{e.ConnectID}");

            if (mode.Ready == true && mode.Selector == ModeSelector.Manual)
            {

            }
        }
        public void RcvInitFacilityProc(RecieveMessageEventArgs e)
        {
            RcvInitFacility rcvmsg = new RcvInitFacility(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvInitFacility.NameStr}:{e.Message}", $"{e.ConnectID}");

            if (mode.Ready == true && mode.Selector == ModeSelector.Manual)
            {
                // 原位置復帰の実施
                InitFacilityProc();
            }
        }
        public void RcvShiftCheckProc(RecieveMessageEventArgs e)
        {
            RcvShiftCheck rcvmsg = new RcvShiftCheck(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvShiftCheck.NameStr}:{e.Message}", $"{e.ConnectID}");

            if (mode.Ready == true && mode.Selector == ModeSelector.Auto && mode.ContinueON == true)
            {

            }
        }
        /// <summary>
        /// 設備制御指示受信時動作
        /// </summary>
        /// <param name="e"></param>
        public void RcvFacilityCtrlProc(RecieveMessageEventArgs e)
        {
            RcvFacilityCtrl rcvmsg = new RcvFacilityCtrl(e.Message);
            LOGGER.InfoPlus($"受信→{C_NM_TERMINAL}{RcvFacilityCtrl.NameStr}:{e.Message}", $"{e.ConnectID}");

            if (mode.Ready == true && mode.Selector == ModeSelector.Manual)
            {
                // 運転準備ON かつ 各個の時
                LOGGER.InfoPlus($"受信→{C_NM_TERMINAL},設備ID:{rcvmsg.FacilitySID},制御値:{rcvmsg.CtrlValue}", $"{e.ConnectID}");
                SetFacilityRequestedStatus(rcvmsg.FacilitySID,rcvmsg.CtrlValue);
            }
        }
        #endregion


        #region ### 電文定義 ###

        #region ## 受信 ##
        /// <summary>
        /// 運転準備切替
        /// </summary>
        public class RcvChangeOpePreparation : RcvMsgBase
        {
            public const string ID = "C1";
            public const string NameStr = "運転準備切替通知";

            /// <summary>
            /// 切替要求
            /// </summary>
            [Order(3), Length(1)]
            public string Value { get; set; }

            public RcvChangeOpePreparation(string rcv_msg) : base(rcv_msg) { }
        }
        /// <summary>
        /// 各個連続切替
        /// </summary>
        public class RcvChangeOpeMode : RcvMsgBase
        {
            public const string ID = "C2";
            public const string NameStr = "各個連続切替通知";

            /// <summary>
            /// 切替要求
            /// </summary>
            [Order(3), Length(1)]
            public string Value { get; set; }

            public RcvChangeOpeMode(string rcv_msg) : base(rcv_msg) { }
        }
        /// <summary>
        /// 連続切替通知
        /// </summary>
        public class RcvChangeContinueStatus : RcvMsgBase
        {
            public const string ID = "C3";
            public const string NameStr = "運転切替通知";


            /// <summary>
            /// 切替要求
            /// </summary>
            [Order(3), Length(1)]
            public string Value { get; set; }

            public RcvChangeContinueStatus(string rcv_msg) : base(rcv_msg) { }
        }
        /// <summary>
        /// 走行停止通知
        /// </summary>
        public class RcvCarStop : RcvMsgBase
        {
            public const string ID = "C4";
            public const string NameStr = "走行停止通知";

            public RcvCarStop(string rcv_msg) : base(rcv_msg) { }
        }
        /// <summary>
        /// ブザー停止通知
        /// </summary>
        public class RcvStopBuzzer : RcvMsgBase
        {
            public const string ID = "C5";
            public const string NameStr = "ブザー停止通知";

            public RcvStopBuzzer(string rcv_msg) : base(rcv_msg) { }
        }
        /// <summary>
        /// 異常解消通知
        /// </summary>
        public class RcvResolution : RcvMsgBase
        {
            public const string ID = "C6";
            public const string NameStr = "異常解消通知";

            public RcvResolution(string rcv_msg) : base(rcv_msg) { }
        }
        /// <summary>
        /// 原位置復帰通知
        /// </summary>
        public class RcvInitFacility : RcvMsgBase
        {
            public const string ID = "C7";
            public const string NameStr = "原位置復帰通知";

            public RcvInitFacility(string rcv_msg) : base(rcv_msg) { }
        }
        /// <summary>
        /// 画角ズレ実施通知
        /// </summary>
        public class RcvShiftCheck : RcvMsgBase
        {
            public const string ID = "C8";
            public const string NameStr = "画角ズレ実施通知";

            public RcvShiftCheck(string rcv_msg) : base(rcv_msg) { }
        }

        public class RcvFacilityCtrl : RcvMsgBase
        {
            public const string ID = "C9";
            public const string NameStr = "設備制御指示";

            [Order(4), Length(2)]
            public string FacilitySID { get; set; }

            [Order(5), Length(1)]
            public int CtrlValue { get; set; }

            public RcvFacilityCtrl(string rcv_msg) : base(rcv_msg) { }
        }

        #endregion

        #endregion

    }
}
