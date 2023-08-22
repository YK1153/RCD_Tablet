
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using RcdCmn;

namespace CommWrapper
{

    public class CommCl
    {

        #region ### 定数 ###

        private const int C_DEFAULT_RECONNECT_INTERVAL = 1000;
        private const int C_DEFAULT_RESPONSE_TIMEOUT = 10000;
        private const int C_READ_BUFFER_SIZE = 1024;

        #endregion

        #region ### クラス変数 ###

        private AppLog LOGGER = AppLog.GetInstance();
        private Thread m_tConnect;
        private ManualResetEvent m_evEnd;
        private ManualResetEvent m_evRcvWait;
        private TcpClient m_cl;
        private MsgAnalyzer m_analyzer;
        private Object m_tcp_access_lock;

        private string m_ipAddr;
        private int m_port;
        private string m_rcv_msg;

        #endregion

        #region ### プロパティ ###

        /// <summary>
        /// サーバへの再接続間隔(msec)
        /// </summary>
        /// <returns></returns>
        public int ReConnectInterval { get; set; }
        /// <summary>
        /// サーバからの応答メッセージ待機時間(msec)
        /// </summary>
        /// <returns></returns>
        public int ResponseTimeout { get; set; }
        /// <summary>
        /// 接続状態を取得
        /// </summary>
        /// <returns></returns>
        public bool IsConnected
        {
            get
            {
                bool rtn = false;
                lock (m_tcp_access_lock)
                {
                    if (m_cl != null)
                    {
                        rtn = m_cl.Connected;
                    }
                }
                return rtn;
            }
        }

        /// <summary>
        /// 受信ログ出力用クラス
        /// </summary>
        public ICommLogWriter LogWriterRcv { get; set; }
        /// <summary>
        /// 送信ログ出力用クラス
        /// </summary>
        public ICommLogWriter LogWriterSnd { get; set; }

        #endregion

        #region ### イベント ###

        public delegate void ClientConnectedEventHandler(ClientConnectedEventArgs e);
        public delegate void RecieveServerMessageEventHandler(RecieveServerMessageEventArgs e);
        /// <summary>
        /// 接続イベント
        /// </summary>
        public event ClientConnectedEventHandler Connected;
        /// <summary>
        /// 切断イベント
        /// </summary>
        public event ClientConnectedEventHandler Disconnected;
        /// <summary>
        /// サーバメッセージ受信イベント
        /// </summary>
        public event RecieveServerMessageEventHandler RecieveServerMessage;

        #endregion

        #region ### コンストラクタ ###

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public CommCl(string ip, int port)
        {
            m_tConnect = null;
            m_evEnd = new ManualResetEvent(false);
            m_evRcvWait = new ManualResetEvent(true);
            m_analyzer = new MsgAnalyzer();
            m_cl = null;
            m_tcp_access_lock = new Object();

            ReConnectInterval = C_DEFAULT_RECONNECT_INTERVAL;
            ResponseTimeout = C_DEFAULT_RESPONSE_TIMEOUT;

            m_ipAddr = ip;
            m_port = port;
            m_rcv_msg = null;

            LogWriterRcv = null;
            LogWriterSnd = null;
        }

        #endregion

        #region ### 接続/切断 ###

        /// <summary>
        /// サーバへの接続処理
        /// Disconnect()が呼ばれるまで自動的に再接続処理を行う
        /// </summary>


        public void Connect()
        {
            if (m_tConnect != null)
            {
                throw new Exception("サーバへの接続は既に開始されています");
            }
            m_evEnd.Reset();
            m_tConnect = new Thread(new ThreadStart(ConnectThread));
            m_tConnect.Start();
        }

        /// <summary>
        /// 接続スレッドを強制終了
        /// </summary>
        public void ForceStop()
        {
            if (m_tConnect.IsAlive)
            {
                m_tConnect.Abort();
            }
        }

        /// <summary>
        /// 接続処理スレッド
        /// </summary>
        private void ConnectThread()
        {
            LOGGER.Info($"サーバ接続スレッド開始 (ip={m_ipAddr}, port={m_port})");
            do
            {
                try
                {
                    //サーバ接続処理
                    ConnectToServer();
                }
                catch (Exception ex)
                {
                    LOGGER.Error("予期しないエラーが発生しました", ex);
                    DisconnectFromServer();
                }
            }
            while (!m_evEnd.WaitOne(ReConnectInterval));

            //サーバ切断処理
            DisconnectFromServer();
            LOGGER.Info($"サーバ接続スレッド終了 (ip={m_ipAddr}, port={m_port})");
        }

        private void ConnectToServer()
        {
            if (m_cl != null)
            {
                if (!m_cl.Connected)
                {
                    DisconnectFromServer();
                } else
                {
                    return;
                }
            }
            //サーバへの接続
            try
            {
                lock (m_tcp_access_lock)
                {
                    m_cl = new TcpClient();
                    m_cl.Connect(m_ipAddr, m_port);
                }
                LOGGER.Info($"サーバへ接続しました　(ip={m_ipAddr}, port={m_port})");
                //非同期受信開始
                RecieveMessage();
            }
            catch (Exception ex)
            {
                LOGGER.Error("サーバへの接続に失敗しました", ex);
                m_cl.Dispose();
                m_cl = null;
                return;
            }
            //接続成功
            ClientConnectedEventArgs e = new ClientConnectedEventArgs();
            if (Connected != null) { Connected(e); }
        }

        private void DisconnectFromServer()
        {
            lock (m_tcp_access_lock)
            {
                if (m_cl == null) { return; }
                try
                {
                    m_cl.Dispose();
                }
                catch (Exception ex)
                {
                    LOGGER.Error("予期しないエラーが発生しました", ex);
                }
                finally
                {
                    m_cl = null;
                    LOGGER.Info($"サーバとの接続を破棄しました　(ip={m_ipAddr}, port={m_port})");
                }
            }
            try
            {
                //切断イベント
                ClientConnectedEventArgs e = new ClientConnectedEventArgs();
                if (Disconnected != null) { Disconnected(e); }
            }
            catch (Exception ex2)
            {
                LOGGER.Error("予期しないエラーが発生しました", ex2);
            }
        }

        public void Disconnect(bool nowait)
        {
            if (m_tConnect == null)
            {
                throw new Exception("サーバへの接続は行われていません");
            }
            m_evEnd.Set();
            if (!nowait)
            {
                m_tConnect.Join();
            }
            m_tConnect = null;
        }

        #endregion

        #region ### メッセージ受信時処理 ###

        private async void RecieveMessage()
        {
            int iAccessBytes = 0;
            byte[] buffer = new byte[C_READ_BUFFER_SIZE];
            System.IO.MemoryStream ms = null;
            do
            {
                try
                {
                    iAccessBytes = await m_cl.GetStream().ReadAsync(buffer, 0, C_READ_BUFFER_SIZE);
                    if (iAccessBytes != 0)
                    {
                        //メッセージ受信
                        LogWriterRcv?.WriteCommLog(buffer.Take(iAccessBytes).ToArray());
                        do
                        {
                            if (m_analyzer.Analyze(buffer, iAccessBytes, ref ms))
                            {
                                lock (m_tcp_access_lock)
                                {
                                    m_rcv_msg = m_analyzer.RcvMessage;
                                    if (m_evRcvWait.WaitOne(0))
                                    {
                                        //受信イベント発生
                                        RecieveServerMessageEventArgs e = new RecieveServerMessageEventArgs();
                                        e.Message = m_rcv_msg;
                                        if (RecieveServerMessage != null) { RecieveServerMessage(e); }
                                    }
                                    m_evRcvWait.Set();
                                }
                            }
                        } while (!m_analyzer.AnalyzeComplete);
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LOGGER.Error("予期しないエラーが発生しました", ex);
                    break;
                }
            } while (iAccessBytes != 0);
            //サーバ側からの切断
            LOGGER.Info($"サーバ側からネットワークを切断されました (ip={m_ipAddr}, port={m_port})");
            DisconnectFromServer();
        }

        #endregion

        #region ### メッセージ送信 ###

        /// <summary>
        /// 接続中のサーバにメッセージを送信する
        /// </summary>
        /// <param name="snd">送信メッセージ</param>
        /// <param name="wait_response">応答を待機する場合はTrue</param>
        /// <returns>応答メッセージ</returns>
        public string SendMessage(ISndMsgBase msg, bool wait_response)
        {
            string strRcvMessage = null;
            try
            {
                byte[] data = msg.GetBytes();
                lock (m_tcp_access_lock)
                {
                    if (m_cl == null)
                    {
                        throw new Exception("サーバと接続されていないためデータを送信できません");
                    }
                    m_rcv_msg = null;
                    if (wait_response)
                    {
                        m_evRcvWait.Reset();
                    }
                    else
                    {
                        m_evRcvWait.Set();
                    }
                    m_cl.GetStream().Write(data, 0, data.Length);
                    LogWriterSnd?.WriteCommLog(data);
                }
                if (wait_response)
                {
                    //応答受信まで待機
                    if (m_evRcvWait.WaitOne(ResponseTimeout))
                    {
                        strRcvMessage = m_rcv_msg;
                    }
                    else
                    {
                        throw new Exception("サーバから応答メッセージ待機中にタイムアウトが発生しました");
                    }
                }
                return strRcvMessage;
            }
            catch (Exception ex)
            {
                DisconnectFromServer();
                throw ex;
            }
        }

        #endregion
    }

    #region ### ClientConnectedEventArgsクラス ###

    public class ClientConnectedEventArgs
    {
        //必要パラメータなし
    }

    #endregion

    #region ### RecieveServerMessageEventArgs ###

    public class RecieveServerMessageEventArgs
    {
        public string Message { get; set; }
    }

    #endregion

}