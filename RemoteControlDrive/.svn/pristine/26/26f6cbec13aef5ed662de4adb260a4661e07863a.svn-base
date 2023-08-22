using CommWrapper;
using RcdCmn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using static CommWrapper.CommUdpSrv;
using RcdOperationSystemConst;
using static RcdOperation.Control.ControlMain;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RcdOperation.Control
{
    /// <summary>
    /// 人認識機能との通信を行う処理を記載する
    /// </summary>
    public class CommDetect : IDisposable
    {
        #region ### イベント ###
        public delegate void EmergencyEndProcessEventHundler(EmergencyEndProcessEventArgs e);

        public event EmergencyEndProcessEventHundler Emergency;
        #endregion

        #region ### クラス変数 ###
        AppLog LOGGER = AppLog.GetInstance();

        private CommUdpCl m_jsoncl;
        System.Threading.Timer m_timer_json;
        private CommUdpSrv m_jUdpSrv;

        /// <summary>
        /// 認識対象カメラリスト
        /// </summary>
        private List<int> m_camlist = new List<int>();
        #endregion

        #region ### プロパティ ###
        public List<RecognitionObjectData> DetectObjectList { get; private set; }

        public bool RcvStatus { get; private set; }
        #endregion

        #region  ### コンストラクタ ###
        public CommDetect(ConnectionConst connection)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            RcvStatus = false;

            m_jsoncl = new CommUdpCl(connection.ObjectDetect_IP, ConnectionConst.ObjectDetect_Port_Snd);
            m_jsoncl.LogWriterSnd = new LogWriteComm("侵入検知", "Snd");

            m_jUdpSrv = new CommUdpSrv(ConnectionConst.ObjectDetect_Port_Rcv);
            m_jUdpSrv.ReceiveMessage += OnUdpCamMsgReceive;
            m_jUdpSrv.LogWriterRcv = new LogWriteComm("侵入検知", "Rcv");
            m_jUdpSrv.StartListen();

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region ### Dispose ###
        public void Dispose()
        {
            if (m_jUdpSrv != null) m_jUdpSrv.EndListen();
        }
        #endregion

        /// <summary>
        /// 認識を行うカメラ指定の際に送るカメラ数を変更する
        /// </summary>
        /// <param name="num"></param>
        public void ChangeSndCamNum(List<int> list)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_camlist = list;

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        #region ### Snd Start/End ###

        public void SndTimerStart()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_timer_json = new System.Threading.Timer(new TimerCallback(Callback_SndCarStatus), null, Timeout.Infinite, Timeout.Infinite);
            m_timer_json.Change(0, ConnectionConst.ObjectDetect_SndRate);

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        public void SndTimerStop()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (m_timer_json != null)
            {
                m_timer_json.Dispose();
                m_timer_json = null;
            }

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }

        #endregion

        #region ### 送信 ###

        /// <summary>
        /// 人認識機能へ検知カメラIDを定期送信
        /// 定期的にタイマーより呼び出される
        /// </summary>
        /// <param name="sender"></param>
        private void Callback_SndCarStatus(object sender)
        {
            try
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                lock (m_timer_json)
                {
                    try
                    {
                        //タイマー停止
                        if (m_timer_json == null) { return; }
                        m_timer_json.Change(Timeout.Infinite, Timeout.Infinite);

                        // 送信処理
                        // 制御中のみ、送信を行う
                        SndCamNo sndmsg = new SndCamNo()
                        {
                            jsonrpc = "2.0",
                            method = SndCamNo.MethodVal
                        };

                        //現測位カメラから3つのカメラIDを送信する
                        CamNo camno = new CamNo();
                        //カメラリストを追加
                        camno.cam_ids = m_camlist;

                        sndmsg.timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        sndmsg.param = new CamNo();
                        sndmsg.param.cam_ids = new List<int>();
                        sndmsg.param.cam_ids = camno.cam_ids;
                        m_jsoncl.SendJsonMessage(sndmsg);

                    }
                    catch (Exception ex)
                    {
                        LOGGER.Error("送信に失敗しました", ex);
                        ExceptionProcess.ComnExceptionConsoleProcess(ex);
                    }
                    finally
                    {
                        if (m_timer_json != null) { m_timer_json.Change(ConnectionConst.ObjectDetect_SndRate, ConnectionConst.ObjectDetect_SndRate); }
                    }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region ### 受信 ###

        /// <summary>
        /// 人認識機能より受信した際の処理
        /// </summary>
        /// <param name="e"></param>
        private void OnUdpCamMsgReceive(ReceiveMessageEventArgs e)
        {
            try
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                string method = JsonMsgBase.GetMsgMethot(e.Message);

                switch (method)
                {
                    case RcvRecognitionInfo.MethodVal:
                        // 認識情報通知を受信
                        RcvRecognitionInfo rcvRecognitionInfo = new RcvRecognitionInfo();
                        m_jUdpSrv.DeSerializRcvJson(e.Message, ref rcvRecognitionInfo);
                        RcvRecognitionInfoProc(rcvRecognitionInfo);
                        break;
                    default:

                        break;
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 認識情報通知受信時の処理
        /// </summary>
        /// <param name="rcvmsg"></param>
        private void RcvRecognitionInfoProc(RcvRecognitionInfo rcvmsg)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (rcvmsg.param.error_info != "00000")
            {
                //エラーフラグ
                EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                {
                    errcd = rcvmsg.param.error_info.ToString()
                };
                if (Emergency != null) { Emergency(ea); }
            }

            // 検知情報保持
            DetectObjectList = rcvmsg.param.object_data;

            // 受信済みフラグ
            RcvStatus = true;

            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region ### 受信電文定義 ###

        #region ## 認識情報通知 ##
        /// <summary>
        /// 認識情報通知
        /// </summary>
        [DataContract]
        public class RcvRecognitionInfo : JsonMsgBase
        {
            public const string MethodVal = "MOT-API-ODF-SMF-001";

            [DataMember(Name = "params", Order = 2)]
            public RecognitionInfo param { get; set; }
        }
        /// <summary>
        /// 認識情報
        /// </summary>
        [DataContract]
        public class RecognitionInfo
        {
            /// <summary>
            /// 使用カメラ番号
            /// </summary>
            [DataMember(Order = 0)]
            public List<int> cam_ids { get; set; }
            /// <summary>
            /// 検知物評数
            /// </summary>
            [DataMember(Order = 1)]
            public int object_num { get; set; }
            [DataMember(Name = "data", Order = 2)]
            public List<RecognitionObjectData> object_data { get; set; }
            /// <summary>
            /// エラーフラグ
            /// 0:異常なし 非0:異常あり
            /// </summary>
            [DataMember(Order = 3)]
            public string error_info { get; set; }
            /// <summary>
            /// タイムスタンプ（画像入力時）
            /// フォーマット：「yyyyMMddHHmmssfff」
            /// タイムゾーン：JST (GMT+0900)
            /// </summary>
            [DataMember(Order = 4)]
            public string timestamp { get; set; }
        }
        /// <summary>
        /// 物評データ
        /// </summary>
        [DataContract, Serializable]
        public class RecognitionObjectData : ICloneable
        {
            /// <summary>
            /// 物評種別
            /// </summary>
            [DataMember(Order = 0)]
            public string label { get; set; }
            /// <summary>
            /// 物評位置
            /// </summary>
            [DataMember(Order = 1)]
            public List<int> pos { get; set; }

            public object Clone()
            {

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, this);
                    ms.Position = 0;
                    return (RecognitionObjectData)bf.Deserialize(ms);
                }
            }
        }
        #endregion

        #endregion

        #region ### 送信電文定義 ###

        #region ## カメラ指示 ##
        [DataContract]
        public class SndCamNo : JsonMsgBase
        {
            public const string MethodVal = "MOT-API-SMF-ODF-001";

            [DataMember(Name = "params", Order = 2)]
            public CamNo param { get; set; }
        }

        [DataContract]
        public class CamNo
        {
            /// <summary>
            /// 使用カメラ番号
            /// </summary>
            [DataMember(Order = 0)]
            public List<int> cam_ids { get; set; }
        }

        #endregion

        #endregion

        #region　### 送受信ログ出力 ###
        public class LogWriteComm : ICommLogWriter
        {
            private string m_name = "";
            private string m_string = "";

            public LogWriteComm(string name, string s)
            {
                m_name = name;
                m_string = s;
            }

            public void WriteCommLog(byte[] data)
            {
                AppLog.GetInstance().Info($"[{m_name},{m_string}]：{Encoding.ASCII.GetString(data)}");
            }
        }
        #endregion
    }
}
