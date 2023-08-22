using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using RcdCmn;

namespace CommWrapper
{
    public class CommUdpCl
    {
        private AppLog LOGGER = AppLog.GetInstance();

        string m_remoteIP;
        int m_remotePort;
        UdpClient m_udp;

        ///// <summary>
        /// 送信ログ出力用クラス
        /// </summary>
        public ICommLogWriter LogWriterSnd { get; set; }

        public CommUdpCl(string remoteIp, int remotePort)
        {
            m_remoteIP = remoteIp;
            m_remotePort = remotePort;
            m_udp = new UdpClient();
        }

        public void SendMessage(ISndMsgBase msg)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            byte[] data = msg.GetBytes();
            m_udp.Send(data, data.Length, m_remoteIP, m_remotePort);
            LogWriterSnd?.WriteCommLog(data);

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        public void SendJsonMessage<T>(T msg)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
                        
            // シリアライズ
            string jsonString = Serialize(msg);

            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonString);

            m_udp.Send(data, data.Length, m_remoteIP, m_remotePort);
            LogWriterSnd?.WriteCommLog(data);

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        /// <summary>
        /// JSONシリアライズを行う
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg">送信データ</param>
        /// <returns>シリアライズ後データ</returns>
        public string Serialize<T>(T msg)
        {
            string json="";

            using (var ms = new MemoryStream())
            using (var sr = new StreamReader(ms))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(ms, msg);
                ms.Position = 0;

                json = sr.ReadToEnd();
            }

            return json;
        }
    }
}
