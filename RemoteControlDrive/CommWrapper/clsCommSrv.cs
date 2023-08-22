using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using RcdCmn;

namespace CommWrapper
{
    public class CommSrv
    {
        #region ### 定数 ####

        private const int C_READ_BUFFER_SIZE = 1024;
        private const int C_SEARCH_MODE_STX = 1;
        private const int C_SEARCH_MODE_ETX = 2;

        #endregion

        #region ### クラス変数 ###

        private AppLog LOGGER = AppLog.GetInstance();    //ログ出力用オブジェクト
        private ManualResetEvent m_evEnd;                               //終了イベント
        private ManualResetEvent m_evConnected;                         //クライアント接続イベント
        private TcpListener m_listener;                                 //Listenerクラスオブジェクト
        private Thread m_tListen;                                       //待ち受けスレッド
        private Dictionary<int, TcpClient> m_dicTcpClient;              //接続クライアント管理用

        private int m_port;         //待ち受けポート
        private int m_connect_id;

        #endregion

        #region ### プロパティ ###

        /// <summary>
        /// 受信ログ出力用クラス
        /// </summary>
        public ICommLogWriter LogWriterRcv { get; set; }
        /// <summary>
        /// 送信ログ出力用クラス
        /// </summary>
        public ICommLogWriter LogWriterSnd { get; set; }

        #endregion

        #region ### イベント###

        public delegate void ConnectedEventHandler(ConnectedEventArgs e);
        public delegate void DiscconectedEventHandler(ConnectedEventArgs e);
        public delegate void RecieveMessageEventHandler(RecieveMessageEventArgs e);

        /// <summary>
        /// クライアントからの接続時に発生するイベント
        /// </summary>
        public event ConnectedEventHandler Connected;
        /// <summary>
        /// クライアントとの通信が切断されたときに発生するイベント
        /// </summary>
        public event ConnectedEventHandler Disconnected;
        /// <summary>
        /// クライアントからメッセージを受信したときに発生するイベント
        /// </summary>
        public event RecieveMessageEventHandler RecieveMessage;

        # endregion

        #region ### コンストラクタ ###

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="port">待ち受けポート</param>
        public CommSrv(int port)
        {
            //クラス変数初期化
            m_evEnd = new ManualResetEvent(false);
            m_evConnected = new ManualResetEvent(false);
            m_tListen = null;
            m_dicTcpClient = new Dictionary<int, TcpClient>();
            m_port = 0;
            m_connect_id = 0;

            m_port = port;

            LogWriterRcv = null;
            LogWriterSnd = null;
        }
        #endregion

        #region ### クライアント接続待ち受け処理 ###

        /// <summary>
        /// 待ち受けの開始
        /// </summary>
        public void StartListen()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            //既に開始されていればエラーとする
            if (m_listener != null)
            {
                throw new Exception("ポートの待ち受けは既に開始されています");
            }
            //Listenerオブジェクト初期化
            m_listener = new TcpListener(System.Net.IPAddress.Any, m_port);
            //待ち受けスレッドの開始
            m_evEnd.Reset();
            m_tListen = new Thread(new ThreadStart(ListenThread));
            m_tListen.Start();

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        /// <summary>
        /// クライアント待ち受けスレッド
        /// </summary>
        private void ListenThread()
        {
            LOGGER.Info($"待ち受けスレッド開始 (port={m_port})");

            //LISTEN開始
            m_listener.Start();
            do
            {
                try
                {
                    //接続待機
                    AcceptClient();                    

                } catch (Exception ex)
                {
                    LOGGER.Error("予期しないエラーが発生しました", ex);
                }
            } while (!m_evEnd.WaitOne());

            //LISTEN終了
            m_listener.Stop();
            m_listener = null;
            //接続中のクライアントを切断
            foreach (int key in m_dicTcpClient.Keys)
            {
                DisposeClient(key);
            }
            m_dicTcpClient.Clear();

            LOGGER.Info("待ち受けスレッド終了");
        }

        private async void AcceptClient()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            //接続待機
            TcpClient cl = null;
            do
            {
                try
                {
                    cl = await m_listener.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException) {
                    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
                    return;
                }

                //クライアントから接続
                string ipAddr = ((System.Net.IPEndPoint)cl.Client.RemoteEndPoint).Address.ToString();
                m_connect_id = m_connect_id + 1;
                LOGGER.Info($"{ipAddr}({m_connect_id})より接続されました");
                m_dicTcpClient.Add(m_connect_id, cl);

                //接続イベント通知
                ConnectedEventArgs e = new ConnectedEventArgs();
                e.ID = m_connect_id;
                e.IpAddr = ipAddr;
                if (Connected != null) { Connected(e); }

                //データ受信開始
                Task t =  Task.Run(() => OnRecieveMessage(m_connect_id, cl));
            } while (true);
        }

        /// <summary>
        /// 待ち受けの終了
        /// </summary>
        public void EndListen(bool nowait)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            m_evEnd.Set();
            if (nowait == false)
            {
                LOGGER.Debug("'nowait' flg is false");
                m_tListen.Join();
            }
            //m_listener = null;
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }
        private void DisposeClient(int id)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                if (m_dicTcpClient.ContainsKey(id))
                {
                    LOGGER.Debug($"dispose connection id={id}");
                    TcpClient cl = m_dicTcpClient[id];
                    string ipAddr = ((System.Net.IPEndPoint)cl.Client.RemoteEndPoint).Address.ToString();
                    cl.Dispose();

                    //イベント通知
                    ConnectedEventArgs e = new ConnectedEventArgs();
                    e.ID = id;
                    e.IpAddr = ipAddr;
                    if (Disconnected != null) { Disconnected(e); }
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error($"クライアント接続({id})の破棄に失敗しました", ex);
            }
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        #endregion

        #region ### メッセージ受信時処理 ###

        /// <summary>
        /// クライアントからのメッセージ受信処理
        /// </summary>
        /// <param name="id"></param>
        private async Task OnRecieveMessage(int id, TcpClient cl)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start (id={id})");

            NetworkStream ns = cl.GetStream();
            int accessBytes = 0;
            byte[] buffer = new byte[C_READ_BUFFER_SIZE - 1];
            System.IO.MemoryStream ms = null;
            MsgAnalyzer analyzer = new MsgAnalyzer();
            try
            {
                do
                {
                    try
                    {
                        LOGGER.Debug($"ReadAsync start (id={id})");
                        accessBytes = await ns.ReadAsync(buffer, 0, buffer.Length);
                        LOGGER.Debug($"ReadAsync end (id={id})");
                        if (accessBytes != 0)
                        {
                            //メッセージ受信
                            LogWriterRcv?.WriteCommLog(buffer.Take(accessBytes).ToArray());
                            do
                            {
                                if (analyzer.Analyze(buffer, accessBytes, ref ms))
                                {
                                    //受信イベント
                                    RecieveMessageEventArgs re = new RecieveMessageEventArgs();
                                    re.ConnectID = id;
                                    re.Message = analyzer.RcvMessage;
                                    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} recieve {re.Message}");
                                    if (RecieveMessage != null) { RecieveMessage(re); }
                                }
                            } while (!analyzer.AnalyzeComplete);
                        }
                        else
                        {
                            LOGGER.Debug($"access bytes 0 (id={id})");
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        LOGGER.Debug($"ObjectDisposedException occured (id={id})");
                        break;
                    }
                    catch (Exception ex)
                    {
                        LOGGER.Error("予期しないエラーが発生しました", ex);
                        break;
                    }
                } while (accessBytes > 0);
                //クライアントからの切断
                string ipAddr = ((System.Net.IPEndPoint)cl.Client.RemoteEndPoint).Address.ToString();
                LOGGER.Info($"{ipAddr}({id})よりネットワークが切断されました");
                DisposeClient(id);
                m_dicTcpClient.Remove(id);
            }
            catch (Exception ex)
            {
                LOGGER.Error("予期しないエラーが発生しました", ex);
            }
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end (id={id})");
        }

        #endregion

        #region ### メッセージ送信処理 ###

        /// <summary>
        /// 接続中クライアントにメッセージを送信する
        /// </summary>
        /// <param name="id"></param>
        /// <param name="snd_msg"></param>
        public void SendMessage(int id, ISndMsgBase msg)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                byte[] data = msg.GetBytes();

                if (m_dicTcpClient.ContainsKey(id) == false)
                {
                    throw new Exception($"接続ID={id}は存在しない、または破棄されています");
                }
                TcpClient cl = m_dicTcpClient[id];
                cl.GetStream().Write(data, 0, data.Length);
                LogWriterSnd?.WriteCommLog(data);
            }
            catch (Exception ex)
            {
                DisposeClient(id);
                m_dicTcpClient.Remove(id);
                throw ex;
            }
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }
        #endregion
    }

    #region ### ConnectedEventArgsクラス ###

    public class ConnectedEventArgs
    {
        /// <summary>
        /// 接続ID
        /// </summary>
        /// <returns></returns>
        public int ID { get; set; }
        /// <summary>
        /// 接続元IPアドレス
        /// </summary>
        /// <returns></returns>
        public string IpAddr { get; set; }
    }
    #endregion

    #region ### RecieveMessageEventArgsクラス ###

    public class RecieveMessageEventArgs
    {
        /// <summary>
        /// 送信元の接続ID
        /// </summary>
        /// <returns></returns>
        public int ConnectID { get; set; }
        /// <summary>
        /// 受信メッセージ
        /// </summary>
        /// <returns></returns>
        public string Message { get; set; }
    }
#endregion

}