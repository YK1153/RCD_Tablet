using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommWrapper;
using RcdCmn;

namespace RcdManagement
{
    /// <summary>
    /// OperationPCとの通信処理を行う
    /// </summary>
    public class clsCommOperation
    {

        #region ### クラス変数 ###
        // logger
        private AppLog LOGGER = RcdCmn.AppLog.GetInstance();

        // Comm
        private CommSrv m_srv;
        // ↓ machii 20230511 ↓
        public int m_connect_id = 0;
        public delegate void SendMessageEventHandler(SendMessageEventArgs e);
        public event SendMessageEventHandler SendMessage;
        public event SendMessageEventHandler SendMessageNotify;
        private string m_SenderLogicalName = "";
        private string m_PointCode = "";
        private string m_Mode = "";
        // ↑ machii 20230511 ↑
        #endregion

        #region ### プロパティ ###
        /// <summary>
        /// 接続状態
        /// </summary>
        public bool ConnectionStatus { get; private set; }
        // ↓ machii 20230511 ↓
        public string SenderLogicalName

        {
            get { return this.m_SenderLogicalName; }
        }
        public string PointCode

        {
            get { return this.m_PointCode; }
        }
        public string Mode

        {
            get { return this.m_Mode; }
        }
        // ↑ machii 20230511 ↑
        #endregion

        #region ### コンストラクタ ###
        public clsCommOperation()
        {
            m_srv = new CommSrv(ManagementConst.C_OPERATION_COMM_PORT);
            m_srv.Connected += new CommSrv.ConnectedEventHandler(SrvConnected);
            m_srv.Disconnected += new CommSrv.ConnectedEventHandler(SrvDisConnected);
            m_srv.RecieveMessage += new CommSrv.RecieveMessageEventHandler(tSrvReceiveMessage);
            m_srv.LogWriterRcv = new LogWriteComm("OperationPCSrv", "Rcv");
            m_srv.LogWriterSnd = new LogWriteComm("OperationPCSrv", "Snd");
            m_srv.StartListen();
        }
        // ↓ machii 20230511 ↓
        public clsCommOperation(int p_port)
        {
            m_srv = new CommSrv(p_port);
            m_srv.Connected += new CommSrv.ConnectedEventHandler(SrvConnected);
            m_srv.Disconnected += new CommSrv.ConnectedEventHandler(SrvDisConnected);
            m_srv.RecieveMessage += new CommSrv.RecieveMessageEventHandler(tSrvReceiveMessage);
            m_srv.LogWriterRcv = new LogWriteComm("OperationPCSrv", "Rcv");
            m_srv.LogWriterSnd = new LogWriteComm("OperationPCSrv", "Snd");
            m_srv.StartListen();
        }
        // ↑ machii 20230511 ↑
        #endregion

        #region ### OperationPC接続 ###
        private void SrvConnected(ConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
                // ↓ machii 20230511 ↓
                m_connect_id++;
                // ↑ machii 20230511 ↑
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ### OperationPC切断 ###
        private void SrvDisConnected(ConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ### OperationPCから受信 ###
        private void tSrvReceiveMessage(RecieveMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.Message}");
                // ↓ machii 20230511 ↓
                string rcvid = CommMsgBase.GetMsgID(e.Message);
                switch (rcvid)
                {
                    case RcvRfidRequest.ID:         // RFID問い合わせ
                        {
                            RcvRfidRequest rcvmsg = new RcvRfidRequest(e.Message);
                            m_SenderLogicalName = rcvmsg.SenderLogicalName;
                            m_PointCode = rcvmsg.PointCode;
                            // 受信応答を送信
                            SndRfidRequestReceiveRes sndmsg = new SndRfidRequestReceiveRes();
                            sndmsg.ResulrStatus = "00000";
                            //送信イベント発生 ※一時
                            SendMessageEventArgs er = new SendMessageEventArgs();
                            er.Message = Encoding.ASCII.GetString(sndmsg.GetBytes());
                            if (SendMessage != null) { SendMessage(er); }

                            m_srv.SendMessage(m_connect_id, sndmsg);
                            m_Mode = "RFID";
                            SendMessageNotify(er);
                            break;
                        }
                    case RcvInspectionRequest.ID:   // 検査情報問い合わせ
                        {
                            RcvInspectionRequest rcvmsg = new RcvInspectionRequest(e.Message);
                            // 受信応答を送信
                            SndRfidRequestReceiveRes sndmsg = new SndRfidRequestReceiveRes();
                            sndmsg.ResulrStatus = "00000";
                            //送信イベント発生 ※一時
                            SendMessageEventArgs er = new SendMessageEventArgs();
                            er.Message = Encoding.ASCII.GetString(sndmsg.GetBytes());
                            if (SendMessage != null) { SendMessage(er); }

                            m_srv.SendMessage(m_connect_id, sndmsg);
                            m_Mode = "INSPECTION";
                            SendMessageNotify(er);
                            break;
                        }
                }
                // ↑ machii 20230511 ↑
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        // ↓ machii 20230511 ↓
        #region ### OperationPCへ送信(RFID) ###
        public void tSrvSendMessage(SendMessageEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            // RFIDサーバーへの問い合わせ結果送信
            SndRfidRequestRes sndmsg = new SndRfidRequestRes();
            sndmsg.ResulrStatus = "0";
            sndmsg.BodyNo = e.BodyNo;
            sndmsg.BCData = e.Message;
            m_srv.SendMessage(m_connect_id, sndmsg);
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region ### OperationPCへ送信(検査情報) ###
        public void tSrvSendMessage(InspectionList p_InspectionList)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            // 検査情報問い合わせ結果送信
            SndInspectionRequestRes sndmsg = new SndInspectionRequestRes();
            sndmsg.ResulrStatus = "0";
            m_srv.SendMessage(m_connect_id, sndmsg);
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region ### OperationPC切断 ###
        public void tSrvEnd()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_srv.EndListen(true);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion
        // ↑ machii 20230511 ↑

        #region ### 送受信ログ出力 ###
        public class LogWriteComm : ICommLogWriter
        {
            private string m_name = "";
            private string m_string = "";

            public LogWriteComm(string name, string s)
            {
                m_name = name;
                m_string = s;
            }

            public void WriteCommLog(byte[] data)
            {
                AppLog.GetInstance().Info($"[{m_name},{m_string}]：{Encoding.ASCII.GetString(data)}");
            }
        }
        #endregion

        #region ### 電文定義 ###

        #region ## 受信 ##
        public class RcvRfidRequest : RcvMsgBaseMO
        {
            public const string ID = "M1";
            /// <summary>
            /// 送信元論理名
            /// </summary>
            [Order(3), Length(6)]
            public string SenderLogicalName { get; set; }
            /// <summary>
            /// TP
            /// </summary>
            [Order(4), Length(2)]
            public string PointCode { get; set; }


            public RcvRfidRequest(string rcv_msg) : base(rcv_msg) { }
        }
        // ↓ machii 20230523 ↓
        public class RcvInspectionRequest : RcvMsgBaseMO
        {
            public const string ID = "M2";
            /// <summary>
            /// 送信元論理名
            /// </summary>
            [Order(3), Length(6)]
            public string SenderLogicalName { get; set; }
            /// <summary>
            /// TP
            /// </summary>
            [Order(4), Length(2)]
            public string PointCode { get; set; }


            public RcvInspectionRequest(string rcv_msg) : base(rcv_msg) { }
        }
        // ↑ machii 20230523 ↑
        #endregion

        #region ## 送信 ##
        public class SndRfidRequestRes : CommSndMsgBaseMO
        {
            public const string ID = "M6";
            /// <summary>
            /// 処理結果
            /// </summary>
            [Order(4), Length(1)]
            public string ResulrStatus { get; set; }
            /// <summary>
            /// ボデーNo
            /// </summary>
            [Order(5), Length(5)]
            public string BodyNo { get; set; }
            /// <summary>
            /// BCデータ
            /// </summary>
            public string BCData { get; set; }

            public SndRfidRequestRes() : base(ID) { }
            protected override string GetBodyString()
            {
                return $"{ResulrStatus}{BodyNo}{BCData}";
            }
        }
        // ↓ machii 20230511 ↓
        public class SndRfidRequestReceiveRes : CommSndMsgBase
        {
            public const string ID = "00";
            public string ResulrStatus { get; set; }

            public SndRfidRequestReceiveRes() : base(ID) { }
            protected override string GetBodyString()
            {
                return $"{ResulrStatus}";
            }
        }
        // ↑ machii 20230511 ↑
        // ↓ machii 20230523 ↓
        public class SndInspectionRequestRes : CommSndMsgBaseMO
        {
            public const string ID = "M7";
            /// <summary>
            /// 処理結果
            /// </summary>
            [Order(4), Length(1)]
            public string ResulrStatus { get; set; }
            /// <summary>
            /// ボデーNo
            /// </summary>
            [Order(5), Length(5)]
            public string BodyNo { get; set; }
            /// <summary>
            /// BCデータ
            /// </summary>
            public string BCData { get; set; }

            public SndInspectionRequestRes() : base(ID) { }
            protected override string GetBodyString()
            {
                return $"{ResulrStatus}{BodyNo}{BCData}";
            }
        }
        // ↑ machii 20230523 ↑

        #endregion


        #endregion



    }
}
