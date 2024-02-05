using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using CommWrapper;
using RcdCmn;

namespace RcdTablet
{
    public class clsTabletMsg
    {
        #region ## 送信メッセージ ##
        /// <summary>
        /// 管制端末一般送信メッセージ
        /// </summary>
        public class ManagerSndMsg : CtrlTabletSndMsgBase
        {
            public ManagerSndMsg(string msg_id) : base(msg_id) { }

            /// <summary>
            /// メッセージBodyをまとめた文字列で取得
            /// </summary>
            /// <returns>Body，Order，Length Attributeを持っているプロパティの値をまとめた文字列</returns>
            protected override string GetBodyString()
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
        }

        /// <summary>
        /// 共通 取得応答送信メッセージ
        /// </summary>
        public class RcvResSndMsg : ManagerSndMsg
        {
            private const string msgId = "99";

            [Order(4), Length(1), Body()]
            public string rcvRes { get; set; }

            public RcvResSndMsg() : base(msgId) { }
        }
        [DisplayName("運転準備切替通知")]
        public class PreparetionReqSndMsg : ManagerSndMsg
        {
            private const string msgId = "C1";

            [Order(3), Length(1), Body()]
            public int Preparetionstatus { get; set; }

            public PreparetionReqSndMsg() : base(msgId) { }
        }
        [DisplayName("各個連続切り替え通知")]
        public class MngModeChangeSnd : ManagerSndMsg
        {
            private const string msgId = "C2";

            [Order(3), Length(1), Body()]
            public int MngModeStatus { get; set; }

            public MngModeChangeSnd() : base(msgId) { }
        }
        [DisplayName("連続切替通知")]
        public class ContinueChangeSnd : ManagerSndMsg
        {
            private const string msgId = "C3";

            [Order(3), Length(1), Body()]
            public int ContinueStatus { get; set; }

            public ContinueChangeSnd() : base(msgId) { }
        }
        [DisplayName("走行停止通知")]
        public class CarStopSndMsg : ManagerSndMsg
        {
            private const string msgId = "C4";

            public CarStopSndMsg() : base(msgId) { }
        }
        [DisplayName("ブザー停止通知")]
        public class CarOffAndonSndMsg : ManagerSndMsg
        {
            private const string msgId = "C5";

            public CarOffAndonSndMsg() : base(msgId) { }
        }
        [DisplayName("異常解消通知")]
        public class CarResolveSndMsg : ManagerSndMsg
        {
            private const string msgId = "C6";

            public CarResolveSndMsg() : base(msgId) { }
        }
        [DisplayName("原位置復帰通知")]
        public class InitFacilitySnd : ManagerSndMsg
        {
            private const string msgId = "C7";

            public InitFacilitySnd() : base(msgId) { }
        }
        [DisplayName("画角ズレ実施通知")]
        public class CamShiftCheckSndMsg : ManagerSndMsg
        {
            private const string msgId = "C8";

            public CamShiftCheckSndMsg() : base(msgId) { }
        }
        [DisplayName("設備制御指示")]
        public class FacCtrlSndMsg : ManagerSndMsg
        {
            private const string msgId = "C9";

            [Order(3), Length(2), Body()]
            public string facID { get; set; }

            [Order(4), Length(1), Body()]
            public int ctrlVal { get; set; }

            public FacCtrlSndMsg() : base(msgId) { }
        }
        [DisplayName("コンベア停止解消通知")]
        public class CovMoveSnd:ManagerSndMsg
        {
            private const string msgId = "CA";

            public CovMoveSnd() : base(msgId) { }

        }
        #endregion

        /// <summary>
        /// 電文送信ログメッセージ取得
        /// </summary>
        /// <param name="to">送信先</param>
        /// <param name="sndMsg">受信電文インスタンス</param>
        /// <returns>
        ///     送信→[{to}]: {電文名}電文送信"
        ///         [Raw Message]: {raw}
        ///         [送信内容]: 電文プロパティ名: 電文プロパティ値、...
        /// </returns>
        public static string GetMsgSndLog<T>(string to, T sndMsg) where T : CtrlTabletSndMsgBase
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"送信→[{to}]: {GetDisplayName(sndMsg)}電文送信");
            sb.AppendLine();
            sb.Append($"   [Raw Message]: {Encoding.ASCII.GetString(sndMsg.GetBytes())}");

            List<PropertyInfo> orderedBodyProps = GetOrderedProps(sndMsg);

            if (orderedBodyProps.Count > 0)
            {
                string divider = ", ";
                sb.AppendLine();

                sb.Append($"   [ 送信 内容 ]: ");
                foreach (PropertyInfo prop in orderedBodyProps)
                {
                    var displayName = prop
                      .GetCustomAttributes(typeof(DisplayNameAttribute), true)
                      .FirstOrDefault() as DisplayNameAttribute;

                    string strDisplayName = displayName == null ? prop.Name : displayName.DisplayName;

                    sb.Append($"{strDisplayName}: {prop.GetValue(sndMsg).ToString()}{divider}");
                }

                sb.Remove(sb.Length - divider.Length, divider.Length);
            }

            return sb.ToString();
        }

        /// <summary>
        /// DisplayName属性取得
        /// </summary>
        /// <returns>DisplayName属性、ない場合 ""</returns>
        public static string GetDisplayName<T>(T obj)
        {
            var displayName = obj.GetType()
              .GetCustomAttributes(typeof(DisplayNameAttribute), true)
              .FirstOrDefault() as DisplayNameAttribute;

            if (displayName != null)
                return displayName.DisplayName;

            return "";
        }

        public static List<PropertyInfo> GetOrderedProps<T>(T obj)
        {
            return new List<PropertyInfo>(obj.GetType().GetProperties())
                .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                .OrderBy(prop => OrderAttribute.GetOrder(prop))
                .ToList();
        }
    }

}
