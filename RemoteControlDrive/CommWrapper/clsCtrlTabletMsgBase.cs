using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using RcdCmn;

namespace CommWrapper
{
    public abstract class CtrlTabletMsgBase
    {
        /// <summary>
        /// 電文の連番
        /// </summary>
        [Order(0), Length(4)]
        public int SerialNo { get; set; }

        /// <summary>
        /// データ種別
        /// </summary>
        [Order(1), Length(2)]
        public string MSGID { get; protected set; } = "0";

        /// <summary>
        /// データ長
        /// </summary>
        [Order(2), Length(4)]
        public int DataLength { get; protected set; }

        /// <summary>
        /// メッセージID取得
        /// </summary>
        /// <param name="rcv_msg">メッセージ内容</param>
        /// <returns>メッセージID</returns>
        public static string GetMsgID(string rcv_msg)
        {
            PropertyInfo[] props = typeof(CtrlTabletMsgBase).GetProperties();
            PropertyInfo msgIdProp = typeof(CtrlTabletMsgBase).GetProperty("MSGID");

            // msgIdよりOrderが低いプロパティのLength合計を計算して，msIdの開始Indexを取得
            int startIndex = new List<PropertyInfo>(props)
                .OrderBy(prop => OrderAttribute.GetOrder(prop))
                .Where(prop => OrderAttribute.GetOrder(prop) < OrderAttribute.GetOrder(msgIdProp))
                .Sum(prop => LengthAttribute.GetLength(prop));

            return rcv_msg.Substring(startIndex, LengthAttribute.GetLength(msgIdProp));
        }
    }

    public abstract class CtrlTabletSndMsgBase : CtrlTabletMsgBase, ISndMsgBase
    {
        private static Dictionary<string, int> m_msgSerials = new Dictionary<string, int>();
        private const int C_MIN_SERIAL = 0;
        private const int C_MAX_SERIAL = 9999;
        private object dictlock = new object();

        protected CtrlTabletSndMsgBase(string msgId)
        {
            MSGID = msgId;
            lock (dictlock)
            {
                // シリアル番号採番
                int prevSerialNo = m_msgSerials.ContainsKey(msgId) ? m_msgSerials[msgId] : C_MAX_SERIAL;
                int newSerialNo = prevSerialNo == C_MAX_SERIAL ? C_MIN_SERIAL : prevSerialNo + 1;
                SerialNo = newSerialNo;
                m_msgSerials[msgId] = newSerialNo;
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
            sb.Append(body_len.ToString("D4"));
            sb.Append(body);
            sb.Append((char)CommConst.ETX);

            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }

    public abstract class CtrlTabletRcvMsgBase : CtrlTabletMsgBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rcv_msg"></param>
        public CtrlTabletRcvMsgBase(string rcv_msg)
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
