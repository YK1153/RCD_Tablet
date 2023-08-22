using RcdCmn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CommWrapper
{
    public abstract class clsCommMsgBaseMO
    {
        /// <summary>
        /// 電文の連番
        /// </summary>
        [Order(0), Length(4)]
        public int SerialNo { get; protected set; }

        /// <summary>
        /// データ種別
        /// </summary>
        [Order(1), Length(2)]
        public string MSGID { get; protected set; } = "0";

        /// <summary>
        /// データ長
        /// </summary>
        [Order(2), Length(5)]
        public int DataLength { get; protected set; }

        /// <summary>
        /// メッセージID取得
        /// </summary>
        /// <param name="rcv_msg">メッセージ内容</param>
        /// <returns>メッセージID</returns>
        public static string GetMsgID(string rcv_msg)
        {
            PropertyInfo[] props = typeof(clsCommMsgBaseMO).GetProperties();
            PropertyInfo msgIdProp = typeof(clsCommMsgBaseMO).GetProperty("MSGID");

            // msgIdよりOrderが低いプロパティのLength合計を計算して，msIdの開始Indexを取得
            int startIndex = new List<PropertyInfo>(props)
                .OrderBy(prop => OrderAttribute.GetOrder(prop))
                .Where(prop => OrderAttribute.GetOrder(prop) < OrderAttribute.GetOrder(msgIdProp))
                .Sum(prop => LengthAttribute.GetLength(prop));

            return rcv_msg.Substring(startIndex, LengthAttribute.GetLength(msgIdProp));
        }
    }

    public class RcvResultSendMsgMO : clsCommMsgBaseMO, ISndMsgBase
    {
        public const string msgId = "00";

        [Order(3), Length(5), Body()]
        public int result { get; private set; }

        private string GetBodyString()
        {
            // Body，Order Attributeを持っているプロパティをOrder 順でリスト化
            List<PropertyInfo> orderedBodyProps = new List<PropertyInfo>(GetType().GetProperties())
                .Where(prop => Attribute.IsDefined(prop, typeof(BodyAttribute)))
                .OrderBy(prop => OrderAttribute.GetOrder(prop))
                .ToList();

            StringBuilder sb = new StringBuilder();
            foreach (PropertyInfo prop in orderedBodyProps)
            {
                var propType = prop.PropertyType;
                var converter = TypeDescriptor.GetConverter(propType);

                int length = LengthAttribute.GetLength(prop);
                switch (prop.GetValue(this))
                {
                    case int intVal:
                        string format = "D" + length.ToString();
                        if (intVal.ToString().Length > length)
                        {
                            throw new UserException($"Property Length over: {prop.Name}: {intVal.ToString()}");
                        }
                        sb.Append(intVal.ToString(format));
                        break;
                    case string strVal:
                        if (strVal.Length > length)
                        {
                            throw new UserException($"Property Length over: {prop.Name}: {strVal}");
                        }
                        sb.Append(strVal.PadRight(length, ' '));
                        break;
                    default:
                        throw new UserException("Invalid Type");
                }
            }
            return sb.ToString();
        }

        public byte[] GetBytes()
        {
            StringBuilder sb = new StringBuilder();
            string body = GetBodyString();
            int body_len = body?.Length ?? 0;

            sb.Append((char)CommConst.STX);
            sb.Append(SerialNo.ToString("D4"));
            sb.Append(MSGID);
            sb.Append(body_len.ToString("D5"));
            sb.Append(body);
            sb.Append((char)CommConst.ETX);

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        public static RcvResultSendMsgMO GetOK()
        {
            RcvResultSendMsgMO msg = new RcvResultSendMsgMO();
            msg.result = 00000;

            return msg;
        }

        public static RcvResultSendMsgMO GetOK(int serialNo)
        {
            RcvResultSendMsgMO msg = new RcvResultSendMsgMO();
            msg.SerialNo = serialNo;
            msg.result = 00000;

            return msg;
        }

        public static RcvResultSendMsgMO GetModeNotAvailableMsg(int serialNo)
        {
            RcvResultSendMsgMO msg = new RcvResultSendMsgMO();
            msg.SerialNo = serialNo;
            msg.result = 11111;

            return msg;
        }

        public RcvResultSendMsgMO()
        {
            MSGID = msgId;
        }
    }

    public abstract class CommSndMsgBaseMO : clsCommMsgBaseMO, ISndMsgBase
    {
        // 送信メッセージID別シリアル採番リスト
        private static Dictionary<string, int> m_msgSerials = new Dictionary<string, int>();

        private const int C_MIN_SERIAL = 0;
        private const int C_MAX_SERIAL = 9999;
        private object dictlock = new object();

        protected CommSndMsgBaseMO(string msg_id)
        {
            MSGID = msg_id;
            lock (dictlock)
            {
                // シリアル番号採番
                int prevSerialNo = m_msgSerials.ContainsKey(msg_id) ? m_msgSerials[msg_id] : C_MAX_SERIAL;
                int newSerialNo = prevSerialNo == C_MAX_SERIAL ? C_MIN_SERIAL : prevSerialNo + 1;
                SerialNo = newSerialNo;
                m_msgSerials[msg_id] = newSerialNo;
            }
        }

        protected abstract string GetBodyString();

        public byte[] GetBytes()
        {
            StringBuilder sb = new StringBuilder();
            string body = GetBodyString();
            int body_len = body?.Length ?? 0;

            sb.Append((char)CommConst.STX);
            sb.Append(SerialNo.ToString("D4"));
            sb.Append(MSGID);
            sb.Append((body_len.ToString("D5")));
            sb.Append(body);
            sb.Append((char)CommConst.ETX);

            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }

    public class RcvRsultRcvMsgMO : clsCommMsgBaseMO
    {
        public const string msgId = "00";

        [Order(3), Length(5)]
        public string result { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rcv_msg"></param>
        public RcvRsultRcvMsgMO(string rcv_msg)
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
        }
    }

    public class RcvMsgBaseMO : clsCommMsgBaseMO
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rcv_msg"></param>
        public RcvMsgBaseMO(string rcv_msg)
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
        }
    }
}
