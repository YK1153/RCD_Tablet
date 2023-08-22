using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RcdCmn;

namespace CommWrapper
{
    public class CommUdpSrv
    {
        private AppLog LOGGER = AppLog.GetInstance();

        int m_port;
        UdpClient m_udp;
        Thread m_receiveThread;
        private MsgAnalyzer m_analyzer;

        public delegate void ReceiveMessageEventHandler(ReceiveMessageEventArgs e);
        public event ReceiveMessageEventHandler ReceiveMessage;

        /// <summary>
        /// 受信ログ出力用クラス
        /// </summary>
        public ICommLogWriter LogWriterRcv { get; set; }

        public CommUdpSrv(int port)
        {
            m_port = port;
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            m_udp = new UdpClient(ep);
            m_receiveThread = new Thread(new ThreadStart(ListenTread));
            m_analyzer = new MsgAnalyzer();
        }

        public void StartListen()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            m_receiveThread.Start();

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        public void PauseListen()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            m_receiveThread.Interrupt();

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        public void EndListen()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            m_udp.Close();
            m_receiveThread.Abort();

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        public async void ListenTread()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            do
            {
                UdpReceiveResult receiveResult;
                try
                {
                    receiveResult = await m_udp.ReceiveAsync();
                }
                catch (ObjectDisposedException)
                {
                    Console.Write("disposed");
                    break;
                }
                
                OnReceiveMessage(receiveResult);
            } while (true);

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        } 

        private void OnReceiveMessage(UdpReceiveResult receiveResult)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            byte[] buffer = receiveResult.Buffer;
            System.IO.MemoryStream ms = null;
            MsgAnalyzer m_analyzer = new MsgAnalyzer();
            try
            {
                if (m_analyzer.Analyze(buffer, buffer.Length, ref ms))
                {
                    //受信イベント
                    ReceiveMessageEventArgs re = new ReceiveMessageEventArgs();
                    re.Message = m_analyzer.RcvMessage;
                    ReceiveMessage?.Invoke(re);
                }
                else
                {
                    ReceiveMessageEventArgs re = new ReceiveMessageEventArgs();
                    LogWriterRcv?.WriteCommLog(buffer.ToArray());
                    re.Message = Encoding.UTF8.GetString(buffer); 
                    ReceiveMessage?.Invoke(re);
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error("予期しないエラーが発生しました", ex);
            }
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        /// <summary>
        /// JSONデシリアライズを行う
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg">受信メッセージ</param>
        /// <param name="data">デシリアライズ後格納先</param>
        public void DeSerializRcvJson<T>(string msg, ref T data)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(msg)))
            {
                DataContractJsonSerializer sr = new DataContractJsonSerializer(typeof(T));
                data = (T)sr.ReadObject(ms);
            }
        }
    }

    public class ReceiveMessageEventArgs
    {
        public string Message { get; set; }
    }
}
