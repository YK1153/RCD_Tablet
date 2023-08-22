using CommWrapper;
using RcdCmn;
using RcdOperationSystemConst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static RcdOperation.Control.ControlMain;

namespace RcdOperation.Control
{
    public class CommManagement
    {
        #region ※ 一時定数 ※ なくす必要あり
        private const int C_MANAGEMENTCOMM_WAITTIME = 500;

        #endregion

        private const string C_MANAGEMENT_NM = "Management";

        #region ### クラス変数 ###
        AppLog LOGGER;
        private object m_sendlock= new object();
        private ManualResetEvent m_evRvcWait;

        private CommCl m_managementcl;
        /// <summary>
        /// Management接続状態
        /// </summary>
        private bool ManagementCommStatus;

        private string m_LogicalName;
        private string m_PointCode;

        #endregion

        #region ### プロパティ ###
        /// <summary>
        /// 接続状態
        /// </summary>
        public bool CommStatus { get { return ManagementCommStatus; } }

        public RcvRFIDRequest GetRFIDInfo { get; private set; }

        #endregion

        #region ### コンストラクタ ###
        public CommManagement(AppLog logger,string logicalname,string pointcode)
        {
            LOGGER = logger;
            m_LogicalName = logicalname;
            m_PointCode = pointcode;
            m_evRvcWait = new ManualResetEvent(true);

            m_managementcl = new CommCl( ConnectionConst.ManagementPC_IP, ConnectionConst.ManagementPC_Port );
            m_managementcl.Connected += new CommCl.ClientConnectedEventHandler(ManagementClConnected);
            m_managementcl.Disconnected += new CommCl.ClientConnectedEventHandler(ManagementClDisConnected);
            m_managementcl.RecieveServerMessage += new CommCl.RecieveServerMessageEventHandler(ManagementclReceiveMessage);
            m_managementcl.LogWriterRcv = new LogWriteComm("ManagementPCCl", "Rcv");
            m_managementcl.LogWriterSnd = new LogWriteComm("ManagementPCCl", "Snd");
            m_managementcl.ResponseTimeout = C_MANAGEMENTCOMM_WAITTIME;

            m_managementcl.Connect();
        }

        #endregion

        #region ### Dispose ###
        public void Dispose()
        {
            m_managementcl.Disconnect(true);
            m_managementcl=null;
        }

        #endregion

        #region ### 接続・切断 ###
        private void ManagementClConnected(ClientConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                ManagementCommStatus = true;
                LOGGER.Info("ManagementPCと接続しました");

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void ManagementClDisConnected(ClientConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                ManagementCommStatus = false;
                LOGGER.Info("ManagementPCと切断しました");

                //制御中に切断された場合に走行停止
                //if (m_CarControlStatus == C_UNDER_CONTROL)
                //{
                //    EmergencyEndProcess(Res.ErrorStatus.C_ERR.Code);
                //}
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region ### 受信 ###
        private void ManagementclReceiveMessage(RecieveServerMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.Message}");

                string rcvid = CommMsgBase.GetMsgID(e.Message);

                switch (rcvid)
                {
                    case RcvRFIDRequest.ID:
                        {
                            GetRFIDInfo = new RcvRFIDRequest(e.Message);
                            LOGGER.Info($"受信→[{C_MANAGEMENT_NM}]{RcvRFIDRequest.NameStr}:{e.Message}");
                            m_evRvcWait.Set();
                            break;
                        }
                    default:
                        {

                            break;
                        }
                }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region ### 送信 ###

        #region ## RFID取得要求 ##
        /// <summary>
        /// RFID取得要求
        /// </summary>
        /// <returns>RFID情報</returns>
        private int GetRFID()
        {
            lock (m_sendlock)
            {
                // ManagementPCへ要求
                SndRFIDRequest sndmsg = new SndRFIDRequest()
                {
                    SenderLogicalName = m_LogicalName,
                    PointCode = m_PointCode
                };

                m_evRvcWait.Reset();
                LOGGER.Info($"[{C_MANAGEMENT_NM}]送信→{SndRFIDRequest.NameStr}");
                string rcvmsg = m_managementcl.SendMessage(sndmsg, true);
                LOGGER.Info($"[{C_MANAGEMENT_NM}]受信→{SndRFIDRequest.NameStr}送信応答");

                //　ManagementPC処理待ち
                if (m_evRvcWait.WaitOne(20000))
                {
                    return GetRFIDInfo.ProcResult;
                }
                else
                {
                    // Timeout
                    LOGGER.Error("timeout");
                }
            }

            return 1;
        }
        #endregion

        private string GetInspection()
        {
            lock (m_sendlock)
            {
                // ManagementPCへ要求
            }
            return "";
        }

        private string GetIPAddr()
        {
            lock (m_sendlock)
            {
                // ManagementPCへ要求
            }
            return "";
        }

        #endregion

        /// <summary>
        /// RFID読み込み処理
        /// ManagementPCとやり取りを行い、取得した車両の情報を返却する
        /// </summary>
        public bool InquiryRFID()
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                VehicleInfo vehicle = new VehicleInfo();

                //RFID情報取得
                int s = GetRFID();
                if (s != 0)
                {
                    return false;
                }

                //RFID情報分解
                vehicle.BodyNo = GetRFIDInfo.BodyNo;
                LOGGER.Info($"[送信内容]→ボデーNo:{vehicle.BodyNo}");

                return true;
            }
            catch (Exception ex)
            {
                ExceptionProcess.ComnExceptionConsoleProcess(ex);
                return false;
            }finally 
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        /// <summary>
        /// 検査情報取得処理
        /// </summary>  
        public bool InquiryInspection()
        {
            //検査情報取得
            GetInspection();

            //検査情報分解

            //走行車両判定

            //走行NG車両の場合※リターンの後に行う？？？

            return true;
        }


        public string InquiryIPAddr()
        {
            // IPアドレス取得
            GetIPAddr();

            return "";
        }

        #region ### 電文定義 ###

        #region ## 送信 ##

        public class SndRFIDRequest : CommSndMsgBaseMO
        {
            public const string ID = "M1";
            public const string NameStr = "RFIDサーバー問い合わせ";

            [Order(3),Length(6)]
            public string SenderLogicalName { get; set; }

            [Order(4),Length(2)]
            public string PointCode { get; set; }

            public SndRFIDRequest() : base(ID) { }

            protected override string GetBodyString()
            {
                return $"{SenderLogicalName}{PointCode}";
            }
        }

        #endregion

        #region ## 受信 ##
        /// <summary>
        /// RFIDサーバー問い合わせ結果
        /// </summary>
        public class RcvRFIDRequest : clsCommMsgBaseMO
        {
            public const string ID = "M6";
            public const string NameStr = "RFIDサーバー問い合わせ結果";

            [Order(3), Length(1)]
            public int ProcResult { get; set; }

            [Order(4),Length(5)]
            public string BodyNo { get; set; }

            public string BCData;

            public RcvRFIDRequest(string rcv_msg) 
            {
                List<PropertyInfo> orderedProps = new List<PropertyInfo>(GetType().GetProperties())
                    .OrderBy(prop => OrderAttribute.GetOrder(prop))
                    .ToList();

                int substringFrom = 0;
                foreach (PropertyInfo prop in orderedProps)
                {
                    var propType = prop.PropertyType;
                    var converter = TypeDescriptor.GetConverter(propType);

                    int length = LengthAttribute.GetLength(prop);
                    string strVal = rcv_msg.Substring(substringFrom, length);

                    if (prop.PropertyType == typeof(int))
                    {
                        prop.SetValue(this, converter.ConvertFromString(strVal));
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(this, strVal.Trim());
                    }
                    else
                    {
                        throw new UserException("Invalid type property");
                    }
                    substringFrom += length;
                }

                BCData = rcv_msg.Substring(substringFrom);
            }

        }
        #endregion

        #endregion
    }

}
