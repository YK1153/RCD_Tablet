using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace CommWrapper
{
    /// <summary>
    /// Json共通部
    /// </summary>
    [DataContract]
    public class JsonMsgBase
    {
        /// <summary>
        /// JSON-RPCバージョン
        /// "2.0"固定
        /// </summary>
        [DataMember(Order = 0)]
        public string jsonrpc { get; set; }

        /// <summary>
        /// API識別子
        /// </summary>
        [DataMember(Order = 1)]
        public string method { get; set; }

        //Order=2 params

        /// <summary>
        /// タイムスタンプ（データ送信時）
        /// フォーマット：「yyyyMMddHHmmssfff」
        /// タイムゾーン：JST (GMT+0900)
        /// </summary>
        [DataMember(Order = 3)]
        public string timestamp { get; set; }

        /// <summary>
        /// API識別子の取得
        /// </summary>
        /// <param name="rcv_msg">メッセージ内容</param>
        /// <returns>API識別子</returns>
        public static string GetMsgMethot(string rcv_msg)
        {
            JsonMsgBase info = new JsonMsgBase();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(rcv_msg)))
            {
                DataContractJsonSerializer sr = new DataContractJsonSerializer(typeof(JsonMsgBase));
                info = (JsonMsgBase)sr.ReadObject(ms);
            }

            return info.method;
        }
    }

}
