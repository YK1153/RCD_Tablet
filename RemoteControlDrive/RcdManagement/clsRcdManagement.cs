using RcdCmn;
using RcdOperationSystemConst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RcdManagement
{
    public class clsRcdManagement : IDisposable
    {
        #region ## クラス変数 ##

        // logger
        private AppLog LOGGER = RcdCmn.AppLog.GetInstance();

        // Singleton
        private static clsRcdManagement m_manager = null;
        private static object m_lockobj = new object();
        // ↓ machii 20230511 ↓
        private const string C_SETTING_FILE_NM = "setting.xml";
        private Setting m_setting = null;
        private clsCommOperation m_operation;
        private clsCommRFID m_rfidCL;
        private string m_bodyno = "";
        public readonly ConnectionConst m_connectionConst = new ConnectionConst(0);
        // ↑ machii 20230511 ↑

        #endregion

        #region ## インスタンス生成・破棄 ##
        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal static clsRcdManagement GetInstance()
        {
            if (m_manager == null)
            {
                lock (m_lockobj)
                {
                    if (m_manager == null)
                    {
                        m_manager = new clsRcdManagement();
                    }
                }

            }
            return m_manager;
        }

        public clsRcdManagement()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                // show version
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                LOGGER.Info($"{Res.C_SYSTEM} {Res.C_MNG_SRV} Ver.{version.ToString()}");
                LOGGER.Info($"Initializing App Start...");

                // 各種設定値読み込み
                string strPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), C_SETTING_FILE_NM);
                System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(Setting));
                using (FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    m_setting = (Setting)xml.Deserialize(fs);
                }

                // 上位DBとの同期(担当工場分のデータコピー)

                // DBデータ取得(マスタデータ取得)

                // OperationPCと通信用ポート開設
                // ↓ machii 20230511 ↓
                m_operation = new clsCommOperation(ConnectionConst.ManagementPC_Port);
                m_operation.SendMessageNotify += callRFID;
                // ↑ machii 20230511 ↑

                // IP管理

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        // ↓ machii 20230511 ↓
        private void callRFID(SendMessageEventArgs p_SendMessageEventArgs)
        {
            if (m_operation.Mode == "RFID")
            {
                // RFIDサーバ接続
                m_rfidCL = new clsCommRFID(m_setting.RFIDServerIP, m_setting.RFIDServerPort, m_setting.RFIDServerTimeOut, m_operation.SenderLogicalName, m_setting.ReceiverLogicalName, m_operation.PointCode);

                m_rfidCL.SendMessage += new clsCommRFID.SendMessageEventHandler(RFIDSendMessage);
                m_rfidCL.ReceiveMessage += new clsCommRFID.ReceiveMessageEventHandler(RFIDReceiveMessage);
                m_rfidCL.RFIDClSendMessageNotify += RFIDSendMessageNotify;
                m_rfidCL.RFIDClConnect();
            }
            else if (m_operation.Mode == "INSPECTION")
            {
                callInspection(p_SendMessageEventArgs);
            }
        }
        private async void callInspection(SendMessageEventArgs e)
        {
            // 検査情報システムから検査結果読み出し
            clsInspection inspection = new clsInspection();
            var inspectionList = await inspection.getInspection(m_bodyno);
            // 検査情報システムから検査結果読み出し結果送信
            m_operation.tSrvSendMessage(inspectionList);
        }
        private void RFIDSendMessage(SendMessageEventArgs e)
        {

        }
        private void RFIDReceiveMessage(ReceiveMessageEventArgs e)
        {

        }
        private void RFIDSendMessageNotify(SendMessageEventArgs e)
        {
            m_bodyno = e.BodyNo;
            // RFID問い合わせ結果送信
            m_operation.tSrvSendMessage(e);
        }
        public class Setting
        {
            public string RFIDServerIP;
            public int RFIDServerPort;
            public int RFIDServerTimeOut;
            public string ReceiverLogicalName;
        }
        // ↑ machii 20230511 ↑
        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_operation.tSrvEnd();
                    LOGGER.End();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #endregion
    }
}
