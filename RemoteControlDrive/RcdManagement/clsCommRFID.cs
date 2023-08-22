using CommWrapper;
using RcdCmn;
using RcdOperationSystemConst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RcdManagement.clsCommOperation;

namespace RcdManagement
{
    public class clsCommRFID
    {
        #region ### クラス変数 ###
        CommClRFID m_cl;

        /// <summary>
        /// RFID接続状態
        /// </summary>
        public bool RFIDCommClStatus;

        //public const string C_ReceiverLogicalName = "ABCDEF";//※送信先論理名(RFIDサーバー論理名)
        public string m_receiverLogicalName = "ABCDEF";//※送信先論理名(RFIDサーバー論理名)
        public string m_senderLogicalName = "M1A011";

        public string m_pointCode = "";

        public delegate void ReceiveMessageEventHandler(ReceiveMessageEventArgs e);
        public delegate void SendMessageEventHandler(SendMessageEventArgs e);
        public event ReceiveMessageEventHandler ReceiveMessage;
        public event SendMessageEventHandler SendMessage;
        // ↓ machii 20230511 ↓
        public event SendMessageEventHandler RFIDClSendMessageNotify;
        // ↑ machii 20230511 ↑
        #endregion

        #region ### コンストラクタ ###
        // ↓ machii 20230511 ↓
        // DEL machii public clsCommRFID(string ip, int port, int TimeOut,string senderLogicalName, string receiverLogicalName)
        public clsCommRFID(string ip, int port, int TimeOut, string senderLogicalName, string receiverLogicalName, string p_pointCode)
        // ↑ machii 20230511 ↑
        {
            m_cl = new CommClRFID(ip, port);
            m_cl.Connected += new CommClRFID.ClientConnectedEventHandler(RFIDClConnected);
            m_cl.Disconnected += new CommClRFID.ClientConnectedEventHandler(RFIDClDisConnected);
            m_cl.RecieveServerMessage += new CommClRFID.RecieveServerMessageEventHandler(tclReceiveMessage);
            m_cl.LogWriterRcv = new LogWriteComm("RFIDCl", "Rcv");
            m_cl.LogWriterSnd = new LogWriteComm("RFIDCl", "Snd");
            m_cl.ResponseTimeout = TimeOut;
            m_senderLogicalName = senderLogicalName;
            m_receiverLogicalName = receiverLogicalName;
            // ↓ machii 20230511 ↓
            m_pointCode = p_pointCode;
            // ↑ machii 20230511 ↑
        }
        #endregion

        #region ### RFIDサーバ接続処理 ###
        public void RFIDClConnect()
        {
            if (!m_cl.IsConnected)
            {
                m_cl.Connect();
            }
            else
            {
                throw new UserException("接続開始済みです");
            }
            
        }
        #endregion

        #region ### RFIDサーバ切断処理 ###
        public void RFIDClDisConnect()
        {
            m_cl.Disconnect(true);
        }
        #endregion

        #region ### RFIDサーバ接続イベント ###
        public void RFIDClConnected(ClientConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                RFIDCommClStatus = true;
                // ↓ machii 20230511 ↓
                RFIDInquiry();
                // ↑ machii 20230511 ↑
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ### RFIDサーバ切断イベント ###
        public void RFIDClDisConnected(ClientConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                RFIDCommClStatus = false;

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ### RFIDサーバから受信イベント ###
        public void tclReceiveMessage(RecieveServerMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.Message}");

                // RFIDサーバーからの生産指示受信
                RcvInstructions rcvmsg = new RcvInstructions(e.Message);

                //受信イベント発生 ※一時
                ReceiveMessageEventArgs es = new ReceiveMessageEventArgs();
                es.Message = e.Message;
                if (ReceiveMessage != null) { ReceiveMessage(es); }
                // ↓ machii 20230511 ↓
                if (rcvmsg.BodyNumber == "" || rcvmsg.BodyNumber == "     ") { 
                    return;
                }
                // ↑ machii 20230511 ↑
                // 送信論理名の確認(送受信想定のものであるか)


                // 応答の内容確認 異常に応じた対処を行う
                // 処理結果に応じた応答を返信する
                //SndInstructionsRes sndmsg = new SndInstructionsRes(C_ReceiverLogicalName, m_senderLogicalName);
                SndInstructionsRes sndmsg = new SndInstructionsRes(m_receiverLogicalName, m_senderLogicalName);
                // ↓ machii 20230511 ↓
                // DEL sndmsg.ProcessType = "00"; // ※定数化する
                sndmsg.ProcessType = OperationConst.C_PROCTYPE_NORMAL;
                // ↑ machii 20230511 ↑
                sndmsg.PointCode = m_pointCode; // ※後で変更
                //送信イベント発生 ※一時
                SendMessageEventArgs er = new SendMessageEventArgs();
                er.Message = Encoding.ASCII.GetString(sndmsg.GetBytes());
                if (SendMessage != null) { SendMessage(er); }

                m_cl.SendMessage(sndmsg, false);

                // ↓ machii 20230511 ↓
                er.Message = rcvmsg.DCData;
                er.BodyNo = rcvmsg.BodyNumber;
                RFIDClSendMessageNotify(er);

                RFIDClDisConnect();
                // ↑ machii 20230511 ↑


                // BCデータを設定に応じて分解を行う


            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ### RFIDサーバへ送信 ###
        /// <summary>
        /// RFID要求
        /// </summary>
        /// <param name="pointcode"></param>
        /// <returns>要求結果</returns>
        public void RFIDInquiry()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (RFIDCommClStatus)
            {
                // ↓ machii 20230511 ↓
                // machii DEL m_pointCode = pointcode;
                // ↑ machii 20230511 ↑

                // A通知を送信
                //SndRFIDInquiry sndmsg = new SndRFIDInquiry(C_ReceiverLogicalName, m_senderLogicalName);
                SndRFIDInquiry sndmsg = new SndRFIDInquiry(m_receiverLogicalName, m_senderLogicalName);
                sndmsg.PointCode = m_pointCode; // ※後で変更
                // ↓ machii 20230511 ↓
                // DEL sndmsg.ProcessType = "00"; // ※定数化する
                sndmsg.ProcessType = OperationConst.C_PROCTYPE_NORMAL;
                sndmsg.Mode = "1";
                sndmsg.ProcessResult = "  ";
                sndmsg.BodyNumber = "     ";
                // ↑ machii 20230511 ↑

                //送信イベント発生 ※一時
                SendMessageEventArgs e = new SendMessageEventArgs();
                e.Message = Encoding.ASCII.GetString(sndmsg.GetBytes()); 
                if (SendMessage != null) { SendMessage(e); }

                string msg = "";
                try
                {
                     msg = m_cl.SendMessage(sndmsg, true);
                }
                catch
                {
                    throw new UserException("応答タイムアウト");
                }


                //送信して受信した旨の応答を受信する
                RcvRFIDInquiry rcvmsg = new RcvRFIDInquiry(msg);

                //受信イベント発生 ※一時
                ReceiveMessageEventArgs es = new ReceiveMessageEventArgs();
                es.Message = msg;
                if (ReceiveMessage != null) { ReceiveMessage(es); }


                // 送信論理名の確認(送受信想定のものであるか)



            }
            else
            {
                throw new UserException("接続されていません");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion
    }

    #region ### 電文定義 ###

    #region ## 送信 ##
    /// <summary>
    /// A通知,C通知：進入信号で、設備から生産指示サーバへ送るフォーマット
    /// </summary>
    public class SndRFIDInquiry : SndRFIDCommBase
    {
        /// <summary>
        /// ライン length:1
        /// </summary>
        public string Line = "1";

        /// <summary>
        /// TP length:2
        /// </summary>
        public string PointCode { get; set; }

        /// <summary>
        /// BC連番 length:3 全て半角スペース埋め
        /// </summary>
        public string BCSequenceNumber = "   ";

        /// <summary>
        /// ボデーNo. length:5 全て半角スペース埋め
        /// </summary>
        public string BodyNumber = "     ";

        /// <summary>コンストラクタ</summary>
        public SndRFIDInquiry(string receiverLogicalName, string senderLogicalName)
        {
            ReceiverLogicalName = receiverLogicalName;
            SenderLogicalName = senderLogicalName;
            SerialNo = "0000";
            Line = "1";
            BCSequenceNumber = "   ";
            Length = "00011";
            // machii DEL Mode = "1";
            // machii DEL ProcessResult = "  ";
        }

        protected override string GetBodyString()
        {
            string msg = "";

            msg = $"{Line}{PointCode}{BCSequenceNumber}{BodyNumber}";

            return msg;
        }  
    }

    /// <summary>
    /// 第２応答,F応答：設備から生産指示サーバへの生産指示受信応答
    /// </summary>
    public class SndInstructionsRes : SndRFIDCommBase
    {
        /// <summary>
        /// ライン length:1
        /// </summary>
        public string Line = "1";

        /// <summary>
        /// TP length:2
        /// </summary>
        public string PointCode { get; set; }

        /// <summary>
        /// BC連番 length:3 全て半角スペース埋め
        /// </summary>
        public string BCSequenceNumber = "   ";

        /// <summary>
        /// ボデーNo. length:5 全て半角スペース埋め
        /// </summary>
        public string BodyNumber { get; set; }

        /// <summary>コンストラクタ</summary>
        public SndInstructionsRes(string receiverLogicalName, string senderLogicalName)
        {
            ReceiverLogicalName = receiverLogicalName;
            SenderLogicalName = senderLogicalName;
            SerialNo = "0000";
            Mode = "0";
        }

        protected override string GetBodyString()
        {
            string msg = "";

            msg = $"{Line}{PointCode}{BCSequenceNumber}{BodyNumber}";

            return msg;
        }
    }


    public abstract class SndRFIDCommBase : RFIDCommBase, ISndMsgBase
    {
        public string GetHeaderString()
        {
            string msg = "";

            msg = $"{ReceiverLogicalName}{SenderLogicalName}{SerialNo}{Mode}";

            return msg;
        }

        protected abstract string GetBodyString();

        public byte[] GetBytes()
        {
            StringBuilder sb = new StringBuilder();
            string header = GetHeaderString();
            string body = GetBodyString();
            int body_len = body?.Length ?? 0;

            sb.Append(header);
            sb.Append((body_len.ToString("D5")));
            sb.Append(ProcessType);
            sb.Append(ProcessResult);
            sb.Append(body);
            //sb.Append((char)CommConst.ETX);

            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }   

    #endregion

    #region ## 受信 ##
    /// <summary>
    /// B第１応答,D第１応答：進入信号で、生産指示サーバから設備への応答フォーマット
    /// </summary>
    public class RcvRFIDInquiry : RFIDCommBase
    {
        /// <summary>
        /// ライン length:1
        /// </summary>
        [Order(8), Length(1)]
        public string Line { get; set; }

        /// <summary>
        /// TP length:2
        /// </summary>
        [Order(9), Length(2)]
        public string PointCode { get; set; }

        /// <summary>
        /// BC連番 length:3 全て半角スペース埋め
        /// </summary>
        [Order(10), Length(3)]
        public string BCSequenceNumber { get; set; }

        /// <summary>
        /// ボデーNo. length:5 全て半角スペース埋め
        /// </summary>
        [Order(11), Length(5)]
        public string BodyNumber { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rcv_msg"></param>
        public RcvRFIDInquiry(string rcv_msg)
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


    /// <summary>
    /// E通知：生産指示サーバから設備へのBCデータ送信ﾌｫｰﾏｯﾄ
    /// </summary>
    public class RcvInstructions : RFIDCommBase
    {
        /// <summary>
        /// ライン length:1
        /// </summary>
        [Order(8), Length(1)]
        public string Line { get; set; }

        /// <summary>
        /// TP length:2
        /// </summary>
        [Order(9), Length(2)]
        public string PointCode { get; set; }

        /// <summary>
        /// BC連番 length:3 全て半角スペース埋め
        /// </summary>
        [Order(10), Length(3)]
        public string BCSequenceNumber { get; set; }

        /// <summary>
        /// ボデーNo. length:5 全て半角スペース埋め
        /// </summary>
        [Order(11), Length(5)]
        public string BodyNumber { get; set; }


        /// <summary>
        /// BCデータ
        /// </summary>
        public string DCData;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rcv_msg"></param>
        public RcvInstructions(string rcv_msg)
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

            DCData = rcv_msg.Substring(substringFrom);
        }
    }



    #endregion


    #region ## 共通 ##
    public abstract class RFIDCommBase
    {
        /// <summary>
        /// 送信先論理名 length:6
        /// </summary>
        [Order(1), Length(6)]
        public string ReceiverLogicalName { get; set; }

        /// <summary>
        /// 送信元論理名 length:6
        /// </summary>
        [Order(2), Length(6)]
        public string SenderLogicalName { get; set; }

        /// <summary>
        /// シリアルNo length:4 通信連番(0000固定)
        /// </summary>
        [Order(3), Length(4)]
        public string SerialNo { get; set; }

        /// <summary>
        /// モード length:1
        /// </summary>
        [Order(4), Length(1)]
        public string Mode { get; set; }

        /// <summary>
        /// データ長 length:5 左0埋め
        /// </summary>
        [Order(5), Length(5)]
        public string Length { get; set; }

        /// <summary>
        /// 処理区分 length:2 [00]:通常　[01]:再送
        /// </summary>
        [Order(6), Length(2)]
        public string ProcessType { get; set; }

        /// <summary>
        /// 処理結果 length:2 
        /// </summary>
        [Order(7), Length(2)]
        public string ProcessResult { get; set; }

    }
    #endregion

    #endregion


    #region ### ReceiveMessageEventArgs ###

    public class ReceiveMessageEventArgs
    {
        public string Message { get; set; }
    }

    #endregion

    #region ### SendMessageEventArgs ###

    public class SendMessageEventArgs
    {
        public string Message { get; set; }
        public string BodyNo { get; set; }
    }

    #endregion
}
