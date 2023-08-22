using AtrptShare;
using CommWrapper;
using RcdCmn;
using RcdOperationSystemConst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RcdOperation.Control.ControlMain;

namespace RcdOperation.Control
{
    public class CommCam : IDisposable
    {
        #region ### イベント ###
        public delegate void EmergencyEndProcessEventHundler(EmergencyEndProcessEventArgs e);
        public event EmergencyEndProcessEventHundler Emergency;
        public event EmergencyEndProcessEventHundler StartErr;

        public delegate void AnalyzerCamNoChangedEventHundler(AnalyzerCamNoChangedEventArgs e);
        public event AnalyzerCamNoChangedEventHundler CamChange;
        #endregion

        #region ※ 一時定数 ※ なくす必要あり
        public readonly string C_START_CAM_ID = ConfigurationManager.AppSettings["C_START_CAM_ID"];
        public readonly bool C_B_START_YAWRATE_USE = false;
        public readonly int C_START_YAWRATE = int.Parse(ConfigurationManager.AppSettings["C_ACCELERATION_MODE"]);

        private readonly int C_CAM_WAITTIME = int.Parse(ConfigurationManager.AppSettings["C_CAM_WAITTIME"]);
        private readonly int C_CHT_INTERVAL = int.Parse(ConfigurationManager.AppSettings["C_CHT_INTERVAL"]);
        private readonly bool C_SURVEILLANCE_CAM = true;
        private readonly int C_SURVEILLANCE_INTERVALP_CAM = int.Parse(ConfigurationManager.AppSettings["C_SURVEILLANCE_INTERVALP_CAM"]);
        private readonly int C_SURVEILLANCE_INTERVALS_CAM = int.Parse(ConfigurationManager.AppSettings["C_SURVEILLANCE_INTERVALS_CAM"]);
        private readonly int C_SURVEILLANCE_RESTART_LIMITNUM = int.Parse(ConfigurationManager.AppSettings["C_SURVEILLANCE_RESTART_LIMITNUM"]);
        private readonly int C_SURVEILLANCE_RESTART_WATETIME = int.Parse(ConfigurationManager.AppSettings["C_SURVEILLANCE_RESTART_WATETIME"]);
        private readonly int C_CHT_ERRCONT = int.Parse(ConfigurationManager.AppSettings["C_CHT_ERRCONT"]);
        private readonly int C_CAM_RETRY_NUM = int.Parse(ConfigurationManager.AppSettings["C_CAM_RETRY_NUM"]);
        private readonly bool C_CAM_SHIFT_CHECK = bool.Parse(ConfigurationManager.AppSettings["C_CAM_SHIFT_CHECK"]);

        private List<PointF> C_START_POS = new List<PointF>()
        {
            new PointF(float.Parse(ConfigurationManager.AppSettings["C_START_POS1"].Split(',')[0]), float.Parse(ConfigurationManager.AppSettings["C_START_POS1"].Split(',')[1])),
            new PointF(float.Parse(ConfigurationManager.AppSettings["C_START_POS2"].Split(',')[0]), float.Parse(ConfigurationManager.AppSettings["C_START_POS2"].Split(',')[1])),
            new PointF(float.Parse(ConfigurationManager.AppSettings["C_START_POS3"].Split(',')[0]), float.Parse(ConfigurationManager.AppSettings["C_START_POS3"].Split(',')[1])),
            new PointF(float.Parse(ConfigurationManager.AppSettings["C_START_POS4"].Split(',')[0]), float.Parse(ConfigurationManager.AppSettings["C_START_POS4"].Split(',')[1]))
        };

        private readonly int C_FIRST_VECTER_GET_NUM = int.Parse(ConfigurationManager.AppSettings["C_FIRST_VECTER_GET_NUM"]);

        private readonly int C_RESTART_VECTER_GET_NUM = int.Parse(ConfigurationManager.AppSettings["C_RESTART_VECTER_GET_NUM"]);

        private readonly double C_DOWNSIZE_RATE = double.Parse(ConfigurationManager.AppSettings["C_DOWNSIZE_RATE"]);

        private readonly int C_CARSEARCH_WAITTIME = int.Parse(ConfigurationManager.AppSettings["C_CARSEARCH_WAITTIME"]);

        private readonly string C_CAMSTART_STATUS_OK = "0"; //※
        #endregion

        private List<CamMaster> m_CamList = new List<CamMaster>();//※見直し対象
        /// <summary>
        /// ヘルスチェック状態 0:正常 1:異常
        /// </summary>
        private int m_camht_status = -1;
        private List<HealthInfo> m_HealthInfo = new List<HealthInfo>();
        private bool m_ResetSnd = false;
        private int m_HealthErrCnt = 0;
        private int m_HealthErrCnt9 = 0;
        private ManualResetEvent m_CamReceiveWait;
        private ManualResetEvent m_GetVectorWait;
        private string m_CheckedCamID = "";
        /// <summary>
        /// 測位終了通知監視 (true:受信,false:未受信)
        /// </summary>
        private bool m_finishobserv = true;
        private CarSearchRes m_CarSearchResult;
        /// <summary>
        /// ヨーレート積算値(進行方向)
        /// </summary>
        private double m_oRad = 0;
        private string m_error_camid = "0000";
        private DateTime m_now;
        private string m_CarNo = "";

        #region ### クラス変数 ###
        AppLog LOGGER = AppLog.GetInstance();

        private CommCl m_camTcpCl;
        private CommUdpSrv m_camUdpSrv;

        private object m_healthlock = new object();
        private object m_camsndlock = new object();

        private string m_CamShiftCheckStatus = Res.CamShiftStatus.NOT_ACTIVATE.Code;

        /// <summary>
        /// 制御状態(測位状態) 0:正常 1:異常
        /// </summary>
        private int m_error_flg = 0;

        /// <summary>
        /// 一時停止状態(通信) 0:正常 1:一時停止
        /// </summary>
        private int m_pause_commP_flg = 0;

        private TimeSpan m_CamTimespan;

        /// <summary>
        /// 測位開始応答受信状況 0:未
        /// </summary>
        //private int m_CamStartRes = 0;
        /// <summary>
        /// 測位終了通知監視 (true:受信,false:未受信)
        /// </summary>
        //private bool m_finishobserv = true;

        /// <summary>
        /// 車両位置情報(画像解析より受信)
        /// </summary>
        private List<PointF> m_CarMaskPosition = new List<PointF>();

        /// <summary>
        /// ヘルスチェックタイマー
        /// </summary>
        System.Threading.Timer m_timer_hch;
        /// <summary>
        /// 測位情報受信間隔タイマー(一時停止)
        /// </summary>
        System.Threading.Timer m_timer_pRcv_pause;
        /// <summary>
        /// 測位情報受信間隔タイマー(走行停止)
        /// </summary>
        System.Threading.Timer m_timer_pRcv_stop;

        #endregion

        #region ### プロパティ ###
        public OperationConst.CamType CamType { get; private set; }
        /// <summary>
        /// 接続状態
        /// </summary>
        public bool CommStatus { get; private set; }
        /// <summary>
        /// ヘルスチェック状態
        /// </summary>
        public int HealthStatus { get { return m_camht_status; } }
        /// <summary>
        /// ヘルスチェック情報
        /// </summary>
        public List<HealthInfo> HealthStatusInfo { get { return m_HealthInfo; } }
        /// <summary>
        /// 測位情報受信遅延発生状況
        /// 1：発生中
        /// </summary>
        public int PauseStatus { get { return m_pause_commP_flg == 0 ? 0 : 1; } }
        /// <summary>
        /// 測位開始応答受信状況 0:未
        /// </summary>
        public int CamStartRes { get; private set; }
        /// <summary>
        /// 測位情報1通目受信状況 0:未
        /// </summary>
        public int CamSarted { get; private set; }
        /// <summary>
        /// 測位情報
        /// </summary>
        public CamInfo CamRcvInfo { get; private set; }
        /// <summary>
        /// 測位情報履歴(過去数件程度)
        /// </summary>
        public List<CamInfo> CamRcvInfoHistory { get; set; }

        #endregion

        #region ### コンストラクタ ###
        /// <summary>
        /// 車両認識のコンストラクタ
        /// </summary>
        /// <param name="co"></param>
        /// <param name="type">認識の方法を指定</param>
        public CommCam(ConnectionConst co, OperationConst.CamType type)
        {
            //
            m_CamReceiveWait = new ManualResetEvent(true);
            m_GetVectorWait = new ManualResetEvent(true);

            CamType = type;

            string IP = "";
            int PORT = int.MinValue;

            switch (type)
            {
                case OperationConst.CamType.Pana:
                    {
                        IP = co.PanaImageAnalysis_IP;
                        PORT = ConnectionConst.PanaImageAnalysis_Port;
                        break;
                    }
                case OperationConst.CamType.AVP:
                    {
                        IP = co.AvpImageAnalysis_IP;
                        PORT = ConnectionConst.AvpImageAnalysis_Port_Tcp;

                        m_camUdpSrv = new CommUdpSrv(ConnectionConst.AvpImageAnalysis_Port_Udp);
                        m_camUdpSrv.ReceiveMessage += new CommUdpSrv.ReceiveMessageEventHandler(psrvReceiveMessage);
                        m_camUdpSrv.StartListen();
                        break;
                    }
            }

            m_camTcpCl = new CommCl(IP, PORT);
            m_camTcpCl.Connected += new CommCl.ClientConnectedEventHandler(ClConnected);
            m_camTcpCl.Disconnected += new CommCl.ClientConnectedEventHandler(DisConnected);
            m_camTcpCl.RecieveServerMessage += new CommCl.RecieveServerMessageEventHandler(pclReceiveMessage);
            m_camTcpCl.LogWriterRcv = new LogWriteComm($"{CamType}Analyzer", "Rcv");
            m_camTcpCl.LogWriterSnd = new LogWriteComm($"{CamType}Analyzer", "Snd");
            m_camTcpCl.ResponseTimeout = C_CAM_WAITTIME;
            //接続開始
            m_camTcpCl.Connect();


            CamRcvInfo = new CamInfo();
            CamRcvInfo.CamID = C_START_CAM_ID;

            //m_CamList
            List<CamList> list = new List<CamList>();
            ReadFile("xml/CamList.xml", ref list);
            foreach (CamList li in list)
            {
                CamMaster camMaster = new CamMaster()
                {
                    CamID = li.CamID,
                    CarList = new List<SearchPosition>()
                };
                m_CamList.Add(camMaster);
            }

            //
            CamRcvInfoHistory = new List<CamInfo>();
        }
        #endregion 

        #region ### dispose ###
        public void Dispose()
        {
            if (m_camTcpCl != null)
            {
                m_camTcpCl.Disconnect(true);
            }
            if (m_camUdpSrv != null)
            {
                m_camUdpSrv.EndListen();
            }
        }
        #endregion 

        #region ### 接続・切断イベント ###
        private void ClConnected(ClientConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                CommStatus = true;
                LOGGER.InfoPlus("画像解析装置と接続しました", $"{CamType}");

                m_camht_status = -1;
                LOGGER.InfoPlus("ヘルスチェックを開始します", $"{CamType}");

                m_timer_hch = new System.Threading.Timer(new TimerCallback(Callback_HealthCheck), m_camTcpCl, Timeout.Infinite, 0);
                m_timer_hch.Change(0, C_CHT_INTERVAL);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void DisConnected(ClientConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                CommStatus = false;
                m_camht_status = OperationConst.C_ABNORMAL;
                LOGGER.InfoPlus("画像解析装置と切断しました", $"{CamType}");

                //制御中に切断された場合
                //if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
                //{
                //    EmergencyEndProcess(Res.ErrorStatus.CAM_CONN_ERR.Code);
                //}
                EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                {
                    methodname = $"{MethodBase.GetCurrentMethod().Name}",
                    errcd = Res.ErrorStatus.CAM_CONN_ERR.Code
                };
                if (Emergency != null) { Emergency(ea); }

                if (m_timer_hch != null)
                {
                    m_timer_hch.Dispose();
                    m_timer_hch = null;
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ### ヘルスチェック ###
        private void Callback_HealthCheck(object state)
        {
            lock (m_healthlock)
            {
                try
                {
                    //タイマー停止
                    if (m_timer_hch == null) { return; }
                    m_timer_hch.Change(Timeout.Infinite, Timeout.Infinite);

                    lock (m_camsndlock)
                    {
                        HealthCheck msg = new HealthCheck();
                        if (m_camTcpCl.IsConnected)
                        {
                            m_CamReceiveWait.Reset();
                            string strResponsmsg = m_camTcpCl.SendMessage(msg, false);
                            LOGGER.InfoPlus("画像解析装置ヘルスチェック要求を送信しました。", $"{CamType}");
                        }

                        //受信確認
                        if (m_CamReceiveWait.WaitOne(C_CHT_INTERVAL))
                        {

                        }
                        else
                        {
                            LOGGER.InfoPlus($"カメラのヘルスチェックが受信できません。異常を通知します。", $"{CamType}");
                            //ヘルスチェック受信異常
                            EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                            {
                                methodname = $"{MethodBase.GetCurrentMethod().Name}",
                                errcd = Res.ErrorStatus.B_ERR.Code
                            };
                            if (Emergency != null) { Emergency(ea); }

                            m_camht_status = OperationConst.C_ABNORMAL;
                        }
                    }
                }
                catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
                catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
                finally
                {
                    //タイマー再開
                    if (m_timer_hch != null) { m_timer_hch.Change(C_CHT_INTERVAL, C_CHT_INTERVAL); }
                }
            }
        }

        #endregion

        #region ### 受信 ###

        private void pclReceiveMessage(RecieveServerMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.Message}");

                string rcvid = CommMsgBase.GetMsgID(e.Message);

                switch (rcvid)
                {
                    case PanaMsgStatusNotifyRes.ID:
                        {
                            LOGGER.InfoPlus($"受信→[画像解析装置]測位情報:{e.Message}", $"{CamType}");

                            PanaMsgStatusNotifyResProc(e.Message);

                            break;
                        }
                    case SndEndDataRes.ID:
                        {
                            LOGGER.InfoPlus($"受信→[画像解析装置]測位終了応答:{e.Message}", $"{CamType}");
                            m_CamReceiveWait.Set();
                            SndEndDataRes semsg = new SndEndDataRes(e.Message);
                            LOGGER.InfoPlus($"[受信内容]ボデーNo：{semsg.CarNo}", $"{CamType}");

                            if (m_finishobserv)
                            {
                                //通常制御終了
                                CamRcvInfo.CamID = C_START_CAM_ID;
                                CamSarted = OperationConst.C_NOT_RECEIVED;
                            }
                            else
                            {
                                m_GetVectorWait.Set();
                                m_finishobserv = true;
                            }

                            break;
                        }
                    case HealthCheckRes.ID:
                        {
                            LOGGER.InfoPlus($"受信→[画像解析装置]ヘルスチェック応答:{e.Message}", $"{CamType}");
                            m_CamReceiveWait.Set();
                            HealthCheckResProc(e);

                            break;
                        }
                    case ViewAngleCheckRes.ID:
                        {
                            LOGGER.InfoPlus($"受信→[画像解析装置]画角ずれ検知応答を受信。:{e.Message}", $"{CamType}");
                            LOGGER.InfoPlus($"ヘルスチェックによる起動カメラリセット用。", $"{CamType}");
                            break;
                        }
                    case CarSearchRes.ID:
                        {
                            LOGGER.InfoPlus($"受信→[画像解析装置]車両検索応答:{e.Message}", $"{CamType}");

                            CarSearchRes csmsg = new CarSearchRes(e.Message);
                            LOGGER.InfoPlus($"[受信内容]制御指示種別{csmsg.CamID},結果：{csmsg.Status}", $"{CamType}");

                            m_CarSearchResult = csmsg;

                            break;
                        }
                    default:
                        {
                            LOGGER.InfoPlus($"受信→[画像解析装置]不正なIDを受信。:{e.Message}", $"{CamType}");

                            break;
                        }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex)
            {
                AppLog.GetInstance().Error(ex.Message);
                AppLog.GetInstance().Error(ex.StackTrace);
            }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void psrvReceiveMessage(ReceiveMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.Message}");

                string rcvid = CommMsgBase.GetMsgID(e.Message);

                switch (rcvid)
                {
                    case AvpMsgStatusNotifyRes.ID:
                        {
                            LOGGER.InfoPlus($"受信→[画像解析装置]測位情報:{e.Message}", $"{CamType}");

                            AvpMsgStatusNotifyResProc(e.Message);

                            break;
                        }
                    default:
                        {
                            LOGGER.InfoPlus($"受信→[画像解析装置]不正なIDを受信。:{e.Message}", $"{CamType}");

                            break;
                        }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex)
            {
                AppLog.GetInstance().Error(ex.Message);
                AppLog.GetInstance().Error(ex.StackTrace);
            }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// カメラステータス変換
        /// </summary>
        /// <param name="stcd">ステータスコード</param>
        /// <returns>ステータス文字列</returns>
        private string ConvCamStatus(int stcd)
        {
            string status = "";

            switch (stcd)
            {
                case 0:
                    status = "正常";
                    break;
                case 1:
                    status = "起動中";
                    break;
                case 2:
                    status = "映像断";
                    break;
                case 9:
                    status = "不明";
                    break;
            }

            return status;
        }

        /// <summary>
        /// 測位情報受信時処理
        /// </summary>
        private void PanaMsgStatusNotifyResProc(string message)
        {
            PanaMsgStatusNotifyRes snrmsg = new PanaMsgStatusNotifyRes(message);

            //ログ
            LOGGER.InfoPlus($"[受信内容]ボデーNo：{snrmsg.CarNo} 解析装置ID：{snrmsg.AnalystID},カメラID：{snrmsg.CameraID},画像認識開始時刻：{snrmsg.StartTime},認識処理時間：{snrmsg.ProcTime},測位車両情報数：{snrmsg.CarCount}", $"{CamType}");
            foreach (CamPositioningInfo info in snrmsg.CamPositioningInfoList)
            {
                string msg = "";
                for (int i = 0; i < info.GlovalPosition.Count; i++)
                {
                    msg += $",車両位置情報{i + 1}X：{info.GlovalPosition[i].X.ToString()},車両位置情報{i + 1}Y：{info.GlovalPosition[i].Y.ToString()}";
                }
                LOGGER.InfoPlus($"追尾状態：{info.Status},車両左後方X座標：{info.POS_X},車両左後方Y座標：{info.POS_Y},方向ベクトルX：{info.Vector_X},方向ベクトルY：{info.Vector_Y},移動ベクトルX：{info.MoveVector_X},移動ベクトルY：{info.MoveVector_Y},車両マスク信頼度：{info.Reliability}{msg}", $"{CamType}");
            }

            CamInfo camInfo = new CamInfo();
            camInfo.CamID = snrmsg.CameraID;
            camInfo.camStartTime = snrmsg.StartTime;
            camInfo.ProcTime = snrmsg.ProcTime.ToString();

            foreach (CamPositioningInfo info in snrmsg.CamPositioningInfoList)
            {
                //追尾車両の情報を取得
                if (info.Status == 0)
                {
                    //追尾車両の情報を取得できた場合のみ受信できたと判断(追尾車両がない場合、測位点が取得できていないこととする)
                    if (C_SURVEILLANCE_CAM)
                    {
                        //画像解析装置受信間隔
                        if (m_timer_pRcv_pause != null) { m_timer_pRcv_pause.Change(C_SURVEILLANCE_INTERVALP_CAM, Timeout.Infinite); }
                        if (m_timer_pRcv_stop != null) { m_timer_pRcv_stop.Change(Timeout.Infinite, Timeout.Infinite); }
                        m_pause_commP_flg = 0;
                    }

                    //進行方向ベクトル(カメラ切り替わりの1回目のみ値が入る)
                    camInfo.xVector = double.Parse(info.Vector_X);
                    camInfo.yVector = double.Parse(info.Vector_Y);
                    //移動方向ベクトル(カメラ切り替わりの1回目のみ値なし)
                    camInfo.MovexVector = double.Parse(info.MoveVector_X);
                    camInfo.MoveyVector = double.Parse(info.MoveVector_Y);

                    //取得ベクトル計算
                    if (camInfo.xVector != 0 && camInfo.yVector != 0)
                    {
                        camInfo.MoveVector = CommonProc.ToDegree(CamRcvVectorSet(camInfo.xVector, camInfo.yVector));
                    }
                    else
                    {
                        camInfo.MoveVector = CommonProc.ToDegree(CamRcvVectorSet(camInfo.MovexVector, camInfo.MoveyVector));
                    }

                    // 測位情報のXY座標を代入
                    camInfo.xPosition = float.Parse(info.POS_X);
                    camInfo.yPosition = float.Parse(info.POS_Y);
                    // 車両マスク信頼度を代入(ログ用)
                    camInfo.Reliability = info.Reliability;
                    // 車両マスク代入
                    m_CarMaskPosition = info.GlovalPosition;

                    // タイムスタンプ計算
                    DateTime now = DateTime.Now;
                    DateTime camtimespan = new DateTime(now.Year, now.Month, now.Day, int.Parse(camInfo.camStartTime.Substring(0, 2)), int.Parse(camInfo.camStartTime.Substring(2, 2)), int.Parse(camInfo.camStartTime.Substring(4, 2)), int.Parse(camInfo.camStartTime.Substring(6, 3)));
                    m_CamTimespan = camtimespan - m_now;
                    camInfo.observed_data_timestamp = (uint)Math.Floor(m_CamTimespan.TotalMilliseconds);

                    LOGGER.InfoPlus($"[差分取得]observed_data_timestamp：{camInfo.observed_data_timestamp},", $"{CamType}");

                    if (CamSarted == OperationConst.C_NOT_RECEIVED) { CamSarted = OperationConst.C_RECEIVED; }
                }
                else if (info.Status == 1)
                {
                    //追尾外車両があった際の対処があればここに記載する

                }
            }

            //カメラID変更確認
            if (CamRcvInfo.CamID != camInfo.CamID)
            {
                // 測位カメラが変更されたことを通知
                Task.Run(() =>
                {
                    AnalyzerCamNoChangedEventArgs ea = new AnalyzerCamNoChangedEventArgs
                    {
                        camtype = CamType,
                        camid = snrmsg.CameraID
                    };
                    if (CamChange != null) { CamChange(ea); }
                });
            }
            CamRcvInfo = camInfo;
            CamRcvInfoHistory.Insert(0, CamRcvInfo);
            if (CamRcvInfoHistory.Count > 10)
            {
                CamRcvInfoHistory.RemoveAt(10);
            }
        }

        /// <summary>
        /// 測位情報受信時処理
        /// </summary>
        private void AvpMsgStatusNotifyResProc(string message)
        {
            AvpMsgStatusNotifyRes snrmsg = new AvpMsgStatusNotifyRes(message);

            //ログ
            LOGGER.InfoPlus($"[受信内容]ボデーNo：{snrmsg.CarNo} 解析装置ID：{snrmsg.AnalystID},カメラID：{snrmsg.CameraID},画像認識開始時刻：{snrmsg.StartTime},認識処理時間：{snrmsg.ProcTime},測位車両情報数：{snrmsg.CarCount}", $"{CamType}");
            int terKalmanCount = 0;
            foreach (CamPositioningInfo info in snrmsg.CamPositioningInfoList)
            {
                string msg = "";
                for (int i = 0; i < info.GlovalPosition.Count; i++)
                {
                    msg += $",車両位置情報{i + 1}X：{info.GlovalPosition[i].X},車両位置情報{i + 1}Y：{info.GlovalPosition[i].Y}";
                }
                LOGGER.InfoPlus($"追尾状態：{info.Status},車両左後方X座標：{info.POS_X},車両左後方Y座標：{info.POS_Y},方向ベクトルX：{info.Vector_X},方向ベクトルY：{info.Vector_Y},移動ベクトルX：{info.MoveVector_X},移動ベクトルY：{info.MoveVector_Y},車両マスク信頼度：{info.Reliability}{msg}", $"{CamType}");
                string Kmsg = "";
                for (int j = terKalmanCount; j < snrmsg.KalmanCount[snrmsg.CamPositioningInfoList.IndexOf(info)] - 1; j++)
                {
                    Kmsg += $"分散共分散行列{j + 1}:{snrmsg.KalmanFilters[j]}";
                }
                terKalmanCount += snrmsg.KalmanCount[snrmsg.CamPositioningInfoList.IndexOf(info)];
                LOGGER.InfoPlus($"分散共分散個数{snrmsg.KalmanCount}{Kmsg}", $"{CamType}");
            }

            CamInfo camInfo = new CamInfo();
            camInfo.CamID = snrmsg.CameraID;
            camInfo.camStartTime = snrmsg.StartTime;
            camInfo.ProcTime = snrmsg.ProcTime.ToString();

            foreach (CamPositioningInfo info in snrmsg.CamPositioningInfoList)
            {
                //追尾車両の情報を取得
                if (info.Status == 0)
                {
                    //追尾車両の情報を取得できた場合のみ受信できたと判断(追尾車両がない場合、測位点が取得できていないこととする)
                    if (C_SURVEILLANCE_CAM)
                    {
                        //画像解析装置受信間隔
                        if (m_timer_pRcv_pause != null) { m_timer_pRcv_pause.Change(C_SURVEILLANCE_INTERVALP_CAM, Timeout.Infinite); }
                        if (m_timer_pRcv_stop != null) { m_timer_pRcv_stop.Change(Timeout.Infinite, Timeout.Infinite); }
                        m_pause_commP_flg = 0;
                    }

                    //進行方向ベクトル(カメラ切り替わりの1回目のみ値が入る)
                    camInfo.xVector = double.Parse(info.Vector_X);
                    camInfo.yVector = double.Parse(info.Vector_Y);
                    //移動方向ベクトル(カメラ切り替わりの1回目のみ値なし)
                    camInfo.MovexVector = double.Parse(info.MoveVector_X);
                    camInfo.MoveyVector = double.Parse(info.MoveVector_Y);

                    //取得ベクトル計算
                    if (camInfo.xVector != 0 && camInfo.yVector != 0)
                    {
                        camInfo.MoveVector = CommonProc.ToDegree(CamRcvVectorSet(camInfo.xVector, camInfo.yVector));
                    }
                    else
                    {
                        camInfo.MoveVector = CommonProc.ToDegree(CamRcvVectorSet(camInfo.MovexVector, camInfo.MoveyVector));
                    }

                    // 測位情報のXY座標を代入
                    camInfo.xPosition = float.Parse(info.POS_X);
                    camInfo.yPosition = float.Parse(info.POS_Y);
                    // 車両マスク信頼度を代入(ログ用)
                    camInfo.Reliability = info.Reliability;
                    // 車両マスク代入
                    m_CarMaskPosition = info.GlovalPosition;

                    // タイムスタンプ計算
                    DateTime now = DateTime.Now;
                    DateTime camtimespan = new DateTime(now.Year, now.Month, now.Day, int.Parse(camInfo.camStartTime.Substring(0, 2)), int.Parse(camInfo.camStartTime.Substring(2, 2)), int.Parse(camInfo.camStartTime.Substring(4, 2)), int.Parse(camInfo.camStartTime.Substring(6, 3)));
                    m_CamTimespan = camtimespan - m_now;
                    camInfo.observed_data_timestamp = (uint)Math.Floor(m_CamTimespan.TotalMilliseconds);

                    LOGGER.InfoPlus($"[差分取得]observed_data_timestamp：{camInfo.observed_data_timestamp},", $"{CamType}");

                    if (CamSarted == OperationConst.C_NOT_RECEIVED) { CamSarted = OperationConst.C_RECEIVED; }
                }
                else if (info.Status == 1)
                {
                    //追尾外車両があった際の対処があればここに記載する

                }
            }


            //カメラID変更確認
            if (CamRcvInfo.CamID != camInfo.CamID)
            {
                // 測位カメラが変更されたことを通知
                Task.Run(() =>
                {
                    AnalyzerCamNoChangedEventArgs ea = new AnalyzerCamNoChangedEventArgs
                    {
                        camtype = CamType,
                        camid = snrmsg.CameraID
                    };
                    if (CamChange != null) { CamChange(ea); }
                });
            }
            CamRcvInfo = camInfo;
            CamRcvInfoHistory.Insert(0, CamRcvInfo);
            if (CamRcvInfoHistory.Count > 10)
            {
                CamRcvInfoHistory.RemoveAt(10);
            }
        }

        /// <summary>
        /// ヘルスチェック受信時処理
        /// </summary>
        private void HealthCheckResProc(RecieveServerMessageEventArgs e)
        {
            HealthCheckRes rcvmsg = new HealthCheckRes(e.Message);

            int healthflag = OperationConst.C_NORMAL;

            for (int i = 0; i < rcvmsg.CamCount; i++)
            {
                if (rcvmsg.HealthInfoList[i].camID == CamRcvInfo.CamID)
                {
                    if (rcvmsg.HealthInfoList[i].status != 0)
                    {
                        //ステータスが正常でない
                        //if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
                        //{
                        //制御中
                        if (rcvmsg.HealthInfoList[i].status == 9 && m_HealthErrCnt9 == 0)
                        {
                            //ステータスが不明で1回目
                            LOGGER.InfoPlus($"カメラのステータスが不正です。cam{rcvmsg.HealthInfoList[i].camID}：{ConvCamStatus(rcvmsg.HealthInfoList[i].status)}", $"{CamType}");
                            m_HealthErrCnt9++;
                        }
                        else
                        {
                            //ステータスが不明以外 もしくは 不明が連続
                            if (m_HealthErrCnt < C_CHT_ERRCONT)
                            {
                                //回数未満
                                m_HealthErrCnt++;
                                string msg = $"カメラのステータスが不正です。cam{rcvmsg.HealthInfoList[i].camID}：{ConvCamStatus(rcvmsg.HealthInfoList[i].status)}";
                                LOGGER.InfoPlus(msg, $"{CamType}");
                            }
                            else
                            {
                                //回数超
                                healthflag = OperationConst.C_ABNORMAL;
                                string msg = $"カメラのステータスが不正です。走行停止します。cam{rcvmsg.HealthInfoList[i].camID}：{ConvCamStatus(rcvmsg.HealthInfoList[i].status)}";
                                LOGGER.InfoPlus(msg, $"{CamType}");
                                string camid = new string(rcvmsg.HealthInfoList[i].camID.ToCharArray());
                                string status = new string(rcvmsg.HealthInfoList[i].status.ToString().ToCharArray());
                                //Task.Run(() => EmergencyEndProcess(Res.ErrorStatus.B_ERR.Code, OperationConst.ErrType.C_PT_CAMHEALTH, camid, status));
                                Task.Run(() =>
                                {
                                    EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                                    {
                                        methodname = $"{MethodBase.GetCurrentMethod().Name}",
                                        errcd = Res.ErrorStatus.B_ERR.Code,
                                        errtype = OperationConst.ErrType.C_PT_CAMHEALTH,
                                        arg1 = camid,
                                        arg2 = status
                                    };
                                    if (Emergency != null) { Emergency(ea); }
                                });
                            }
                        }
                    }
                    else
                    {
                        //ステータス正常
                        m_CheckedCamID = rcvmsg.HealthInfoList[i].camID;
                        m_HealthErrCnt = 0;
                        m_HealthErrCnt9 = 0;
                    }
                }
            }

            if (m_ResetSnd == true && healthflag == OperationConst.C_NORMAL)
            {
                m_ResetSnd = false;
            }

            m_camht_status = healthflag;
            m_HealthInfo = rcvmsg.HealthInfoList;
        }

        #endregion

        #region ### 送信 ###
        /// <summary>
        /// 測位開始
        /// </summary>
        /// <exception cref="UserException"></exception>
        private void CamPositioningStart()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            // スタート位置のカメラID　※コースにより変わるなら引数でもらう
            string camid = C_START_CAM_ID;

            SndCamStart sndmsg = new SndCamStart()
            {
                CarNo = m_CarNo,
                CamID = camid
            };

            //カメラ毎
            bool ControlSubjectCar = false;
            foreach (CamMaster cam in m_CamList)
            {
                //測位対象車両
                if (cam.CamID == camid)
                {
                    foreach (SearchPosition sp in cam.CarList)
                    {
                        if (sp.ControlSubject)
                        {
                            sndmsg.CarPositioning = sp.CarPosition;
                            ControlSubjectCar = true;
                            break;
                        }
                    }
                    if (!ControlSubjectCar)
                    {
                        //測位対象車両がないエラー
                        throw new UserException($"[{CamType}]測位対象車両が見つかりません");
                    }
                }
                if (cam.CamID == C_START_CAM_ID)
                {
                    //マスク車両
                    SndCamStart.maskcaminfo maskcaminfo = new SndCamStart.maskcaminfo();
                    maskcaminfo.maskcarpoitions = new List<List<PointF>>();
                    foreach (SearchPosition sp in cam.CarList)
                    {
                        if (!sp.ControlSubject)
                        {
                            if (maskcaminfo.CamID == null) maskcaminfo.CamID = cam.CamID;
                            maskcaminfo.maskcarpoitions.Add(sp.CarPosition);
                        }
                    }
                }
            }

            //制御開始送信
            LOGGER.InfoPlus($"送信開始→[画像解析装置]測位開始通知", $"{CamType}");
            string mes = m_camTcpCl.SendMessage(sndmsg, true);
            LOGGER.InfoPlus($"受信→[画像解析装置]測位開始応答:{mes}", $"{CamType}");

            SndStartDataRes sdrmsg = new SndStartDataRes(mes);
            LOGGER.InfoPlus($"[受信内容]ボデーNo：{sdrmsg.CarNo}", $"{CamType}");

            string status = sdrmsg.StartStatus;

            if (status == C_CAMSTART_STATUS_OK) { CamStartRes = OperationConst.C_RECEIVED; }
            //受信間隔監視開始
            if (C_SURVEILLANCE_CAM)
            {
                if (m_timer_pRcv_pause == null)
                {
                    m_timer_pRcv_pause = new System.Threading.Timer(new TimerCallback(ReceiveExcessP_Pause), null, Timeout.Infinite, 0);
                    m_timer_pRcv_pause.Change(C_SURVEILLANCE_INTERVALP_CAM, Timeout.Infinite);
                }
                if (m_timer_pRcv_stop == null)
                {
                    m_timer_pRcv_stop = new System.Threading.Timer(new TimerCallback(ReceiveExcessP_Stop), null, Timeout.Infinite, 0);
                    m_timer_pRcv_stop.Change(Timeout.Infinite, Timeout.Infinite);
                }

                m_pause_commP_flg = 0;
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 測位終了通知
        /// </summary>
        private void CamPositioningEnd()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            lock (m_camsndlock)
            {
                SndCamEnd cemsg = new SndCamEnd();
                cemsg.CarNo = m_CarNo;

                m_CamReceiveWait.Reset();
                string strResponcemsg = m_camTcpCl.SendMessage(cemsg, false);
                LOGGER.InfoPlus($"[画像解析装置]送信→測位終了通知", $"{CamType}");
                LOGGER.InfoPlus($"[送信内容]ボデーNo：{cemsg.CarNo}", $"{CamType}");

                if (m_CamReceiveWait.WaitOne(C_CHT_INTERVAL))
                {
                    //測位終了応答確認完了
                }
                else
                {
                    // ※制御中のみ再接続を行っていたが、制御中以外でも行うべきと判断（背反があれば修正）
                    //if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL && m_error_flg == OperationConst.C_NORMAL)
                    //{
                    //制御中
                    LOGGER.InfoPlus($"測位終了応答が受信できません。", $"{CamType}");
                    //再接続
                    m_camTcpCl.Disconnect(true);
                    m_camTcpCl.Connect();
                    //}
                }
            }
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region ### 受信間隔超過 ###
        private void ReceiveExcessP_Pause(object state)
        {
            if (m_error_flg == OperationConst.C_NORMAL)
            {
                LOGGER.InfoPlus($"[画像解析装置]受信時間を超過しました。一時停止します。", $"{CamType}");

                m_pause_commP_flg += 1;

                m_timer_pRcv_stop.Change(C_SURVEILLANCE_INTERVALS_CAM, Timeout.Infinite);

                //CamReStart();
            }
        }

        #region ## 受信超過時再開始 ##
        /// <summary>
        /// 画像解析装置受信間隔オーバー時測位再開始
        /// </summary>
        private void CamReStart()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //リトライ回数を超えたら走行停止
            if (m_pause_commP_flg > C_SURVEILLANCE_RESTART_LIMITNUM)
            {
                //EmergencyEndProcess(Res.ErrorStatus.H_ERR.Code);
                EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                {
                    methodname = $"{MethodBase.GetCurrentMethod().Name}",
                    errcd = Res.ErrorStatus.H_ERR.Code
                };
                if (Emergency != null) { Emergency(ea); }

                return;
            }

            try
            {
                // ヘルスチェックを一時停止
                if (m_timer_hch == null) { return; }
                m_timer_hch.Change(Timeout.Infinite, Timeout.Infinite);

                lock (m_healthlock)
                {
                    if (m_CarMaskPosition.Count() > 0)
                    {

                        m_finishobserv = false;
                        m_GetVectorWait.Reset();


                        //測位終了
                        SndCamEnd cemsg = new SndCamEnd();
                        cemsg.CarNo = m_CarNo;
                        m_camTcpCl.SendMessage(cemsg, false);
                        LOGGER.InfoPlus($"[画像解析装置]送信→測位終了通知", $"{CamType}");
                        LOGGER.InfoPlus($"[送信内容]ボデーNo：{cemsg.CarNo}", $"{CamType}");

                        //測位終了待機
                        if (m_GetVectorWait.WaitOne(m_camTcpCl.ResponseTimeout))
                        {
                            //測位終了応答受信完了
                        }
                        else
                        {
                            LOGGER.InfoPlus($"測位終了応答を受信できませんでした。", $"{CamType}");
                            //EmergencyEndProcess(Res.ErrorStatus.CAM_STOPRES_NG.Code);
                            EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                            {
                                methodname = $"{MethodBase.GetCurrentMethod().Name}",
                                errcd = Res.ErrorStatus.CAM_STOPRES_NG.Code
                            };
                            if (Emergency != null) { Emergency(ea); }

                            m_camTcpCl.Disconnect(true);
                            m_camTcpCl.Connect();
                            return;
                        }


                        CamSndReStart();

                        // 一時停止タイマー再開
                        if (m_timer_pRcv_pause != null) { m_timer_pRcv_pause.Change(C_SURVEILLANCE_INTERVALP_CAM + C_SURVEILLANCE_RESTART_WATETIME, Timeout.Infinite); }
                    }
                    else
                    {
                        //走行停止タイマー開始
                        m_timer_pRcv_stop.Change(C_SURVEILLANCE_INTERVALS_CAM, Timeout.Infinite);
                    }

                }

            }
            catch (Exception ex)
            {
                // 測位再開始処理に失敗
                //EmergencyEndProcess(Res.ErrorStatus.CAM_RESTART_FAILURE.Code);
                EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                {
                    methodname = $"{MethodBase.GetCurrentMethod().Name}",
                    errcd = Res.ErrorStatus.CAM_RESTART_FAILURE.Code
                };
                if (Emergency != null) { Emergency(ea); }

                LOGGER.Error(ex.Message);
            }
            finally
            {
                // ヘルスチェック再開
                if (m_timer_hch != null) { m_timer_hch.Change(C_CHT_INTERVAL, C_CHT_INTERVAL); }
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }


        private void CamSndReStart()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            // 制御開始電文作成
            SndCamStart sndmsg = new SndCamStart()
            {
                CarNo = m_CarNo,
                // 最後に取得したカメラのID
                CamID = CamRcvInfo.CamID,
                CamCount = 1,
                CarPositioning = m_CarMaskPosition
            };

            // 測位開始電文送信
            string mes = m_camTcpCl.SendMessage(sndmsg, true);
            LOGGER.InfoPlus($"受信→[画像解析装置]測位開始応答:{mes}", $"{CamType}");

            //受信内容確認
            SndStartDataRes sdrmsg = new SndStartDataRes(mes);
            LOGGER.InfoPlus($"[受信内容]ボデーNo：{sdrmsg.CarNo}", $"{CamType}");

            string status = sdrmsg.StartStatus;
            if (status == C_CAMSTART_STATUS_OK)
            {
                LOGGER.InfoPlus($"測位開始応答にて正常開始が返されました。", $"{CamType}");
            }
            else
            {
                //EmergencyEndProcess(Res.ErrorStatus.CAM_RESTART_STATUS_NG.Code);
                EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                {
                    methodname = $"{MethodBase.GetCurrentMethod().Name}",
                    errcd = Res.ErrorStatus.CAM_RESTART_FAILURE.Code
                };
                if (Emergency != null) { Emergency(ea); }

                LOGGER.Error($"測位開始応答にて開始不可が返されました。");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        private void ReceiveExcessP_Stop(object state)
        {
            LOGGER.InfoPlus($"[画像解析装置]受信時間を超過しました。走行停止します。", $"{CamType}");

            //EmergencyEndProcess(Res.ErrorStatus.G_ERR.Code);
            EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
            {
                methodname = $"{MethodBase.GetCurrentMethod().Name}",
                errcd = Res.ErrorStatus.G_ERR.Code
            };
            if (Emergency != null) { Emergency(ea); }
        }

        #endregion

        #region ### 初期化 ###
        public void CamResetVal()
        {
            //車両位置情報初期化
            m_CarMaskPosition.Clear();

            CamSarted = OperationConst.C_NOT_RECEIVED;
            CamStartRes = OperationConst.C_NOT_RECEIVED;

            AnalyzerCamNoChangedEventArgs ea = new AnalyzerCamNoChangedEventArgs
            {
                camtype = CamType,
                camid = C_START_CAM_ID
            };
            if (CamChange != null) { CamChange(ea); }

            m_pause_commP_flg = 0;
        }
        #endregion

        #region ### 制御終了時動作 ###
        public void CamEnd()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            CamPositioningEnd();

            if (C_SURVEILLANCE_CAM)
            {
                //画像解析装置受信間隔
                if (m_timer_pRcv_pause != null)
                {
                    m_timer_pRcv_pause.Dispose();
                    m_timer_pRcv_pause = null;
                    LOGGER.InfoPlus("画像解析装置受信タイマー停止(一時停止)", $"{CamType}");
                }
                if (m_timer_pRcv_stop != null)
                {
                    m_timer_pRcv_stop.Dispose();
                    m_timer_pRcv_stop = null;
                    LOGGER.InfoPlus("画像解析装置受信タイマー停止(走行停止)", $"{CamType}");
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region 制御開始時動作
        public bool CamPreControlStart(string bodyno)
        {
            m_CarNo = bodyno;

            string msg = "";

            // ※Panaのみ画角ズレ確認
            if (CamType == OperationConst.CamType.Pana)
            {
                ViewAngleShiftCheckAll();

                //画角ずれ状態確認
                if (m_CamShiftCheckStatus == Res.CamShiftStatus.NORMAL.Code)
                {
                    //画角ずれ正常完了
                }
                //else if (m_CamShiftCheckStatus == Res.CamShiftStatus.IN_ACTION.Code)
                //{
                //    //画角ずれ実施中
                //    msg = $"画角ずれ実施中";
                //    LOGGER.InfoPlus(msg, $"{CamType}");
                //    for (int i = 0; i <= 80; i++)
                //    {
                //        Thread.Sleep(100);
                //        if (m_CamShiftCheckStatus == Res.CamShiftStatus.NORMAL.Code)
                //        {
                //            //画角ずれ正常完了
                //            break;
                //        }
                //        else if (m_CamShiftCheckStatus == Res.CamShiftStatus.IN_ACTION.Code)
                //        {
                //            if (i == 80)
                //            {
                //                //ControlStartStatus(Res.ErrorStatus.CAM_SHIFT_ENDRESS.Code);
                //                EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                //                {
                //                    errcd = Res.ErrorStatus.CAM_SHIFT_ENDRESS.Code
                //                };
                //                if (StartErr != null) { StartErr(ea); }
                //            }
                //        }
                //        else
                //        {
                //            //画角ずれ異常
                //            msg = $"画角ずれにて異常を検知しました。";
                //            LOGGER.InfoPlus(msg, $"{CamType}");
                //            //ControlStartStatus(m_CamShiftCheckStatus, m_error_camid);
                //            EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                //            {
                //                errcd = m_CamShiftCheckStatus,
                //                arg1 = m_error_camid
                //            };
                //            if (StartErr != null) { StartErr(ea); }
                //            return false;
                //        }
                //    }
                //}
                else
                {
                    //画角ずれ異常
                    msg = $"画角ずれにて異常を検知しました。";
                    LOGGER.InfoPlus(msg, $"{CamType}");
                    //ControlStartStatus(m_CamShiftCheckStatus, m_error_camid);
                    EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                    {
                        methodname = $"{MethodBase.GetCurrentMethod().Name}",
                        errcd = m_CamShiftCheckStatus,
                        arg1 = m_error_camid
                    };
                    if (StartErr != null) { StartErr(ea); }
                    return false;
                }

            }

            lock (m_healthlock)
            {
                try
                {
                    //ヘルスチェックを一時停止
                    if (m_timer_hch == null) { return false; }
                    m_timer_hch.Change(Timeout.Infinite, Timeout.Infinite);

                    string result = Res.ErrorStatus.UNKNOWN_ERR.Code;

                    //車両検索
                    result = CarSearch();
                    if (result != Res.ErrorStatus.NORMAL.Code)
                    {
                        msg = $"車両検索にて異常を検知しました。";
                        LOGGER.InfoPlus(msg, $"{CamType}");
                        //制御開始失敗
                        //ControlStartStatus(result);
                        EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                        {
                            methodname = $"{MethodBase.GetCurrentMethod().Name}",
                            errcd = result
                        };
                        if (StartErr != null) { StartErr(ea); }
                        return false;
                    }

                    //制御対象車両判定
                    if (!ControlCarSelect())
                    {
                        msg = $"制御対象車両を検知できませんでした。";
                        LOGGER.InfoPlus(msg, $"{CamType}");
                        //見つからなければエラー
                        //ControlStartStatus(Res.ErrorStatus.CAM_CAR_NOTFOUND.Code);
                        EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                        {
                            methodname = $"{MethodBase.GetCurrentMethod().Name}",
                            errcd = Res.ErrorStatus.CAM_CAR_NOTFOUND.Code
                        };
                        if (StartErr != null) { StartErr(ea); }
                        return false;
                    }

                    //初期ベクトル取得
                    result = FirstVectorReq();
                    if (result != Res.ErrorStatus.NORMAL.Code)
                    {
                        msg = $"初期ベクトル取得にて異常を検知しました。";
                        LOGGER.InfoPlus(msg, $"{CamType}");
                        //初期ベクトル取得失敗
                        //ControlStartStatus(result);
                        EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                        {
                            methodname = $"{MethodBase.GetCurrentMethod().Name}",
                            errcd = result
                        };
                        if (StartErr != null) { StartErr(ea); }
                        return false;
                    }

                    return true;

                }
                catch (Exception e)
                {
                    //想定外のエラー
                    LOGGER.Error(e.Message);
                    LOGGER.Error(e.StackTrace);
                    //ControlStartStatus(Res.ErrorStatus.N_ERR.Code);
                    EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                    {
                        methodname = $"{MethodBase.GetCurrentMethod().Name}",
                        errcd = Res.ErrorStatus.N_ERR.Code
                    };
                    if (StartErr != null) { StartErr(ea); }
                    return false;
                }
                finally
                {
                    //ヘルスチェック再開
                    if (m_timer_hch != null) { m_timer_hch.Change(C_CHT_INTERVAL, C_CHT_INTERVAL); }
                }
            }



        }

        public bool CamControlStart(string bodyno, DateTime now)
        {
            m_now = now;

            lock (m_healthlock)
            {
                try
                {
                    //測位開始
                    CamPositioningStart();

                    return true;

                }
                catch (Exception e)
                {
                    //想定外のエラー
                    LOGGER.Error(e.Message);
                    LOGGER.Error(e.StackTrace);
                    //ControlStartStatus(Res.ErrorStatus.N_ERR.Code);
                    EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                    {
                        methodname = $"{MethodBase.GetCurrentMethod().Name}",
                        errcd = Res.ErrorStatus.N_ERR.Code
                    };
                    if (StartErr != null) { StartErr(ea); }
                    return false;
                }
                finally
                {
                    //ヘルスチェック再開
                    if (m_timer_hch != null) { m_timer_hch.Change(C_CHT_INTERVAL, C_CHT_INTERVAL); }
                }
            }
        }


        #endregion

        #region 無線遅延時認識ベクトル取得
        /// <summary>
        /// 無線遅延時ベクトル再取得
        /// </summary>
        public void ReGetVector()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                LOGGER.InfoPlus("経路生成制御中に復帰のためベクトル取得処理開始", $"{CamType}");

                //一時停止タイマー停止
                if (m_timer_pRcv_pause != null)
                {
                    m_timer_pRcv_pause.Change(Timeout.Infinite, Timeout.Infinite);
                }

                // ヘルスチェックを一時停止
                if (m_timer_hch != null) { m_timer_hch.Change(Timeout.Infinite, Timeout.Infinite); }

                lock (m_healthlock)
                {
                    //測位終了応答待機用
                    m_GetVectorWait.Reset();
                    m_finishobserv = false;

                    //測位終了
                    SndCamEnd cemsg = new SndCamEnd();
                    cemsg.CarNo = m_CarNo;
                    m_camTcpCl.SendMessage(cemsg, false);
                    LOGGER.InfoPlus($"[画像解析装置]送信→測位終了通知", $"{CamType}");
                    LOGGER.InfoPlus($"[送信内容]ボデーNo：{cemsg.CarNo}", $"{CamType}");

                    //測位終了待機
                    if (m_GetVectorWait.WaitOne(m_camTcpCl.ResponseTimeout))
                    {
                        //測位終了応答受信完了
                    }
                    else
                    {
                        LOGGER.InfoPlus($"測位終了応答を受信できませんでした。", $"{CamType}");
                        //EmergencyEndProcess(Res.ErrorStatus.CAM_STOPRES_NG.Code);
                        EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                        {
                            methodname = $"{MethodBase.GetCurrentMethod().Name}",
                            errcd = Res.ErrorStatus.CAM_STOPRES_NG.Code
                        };
                        if (Emergency != null) { Emergency(ea); }

                        m_camTcpCl.Disconnect(true);
                        m_camTcpCl.Connect();
                        return;
                    }

                    //ベクトル取得
                    string result = ReGetVectorReq();
                    if (result == Res.ErrorStatus.NORMAL.Code)
                    {
                        //ベクトル取得正常完了
                    }
                    else
                    {
                        string msg = $"ベクトル再取得にて異常を検知しました。";
                        LOGGER.InfoPlus(msg, $"{CamType}");
                        //ベクトル取得失敗
                        //EmergencyEndProcess(result);
                        EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                        {
                            methodname = $"{MethodBase.GetCurrentMethod().Name}",
                            errcd = result
                        };
                        if (Emergency != null) { Emergency(ea); }

                        return;
                    }

                    if (m_CarMaskPosition.Count() > 0)
                    {
                        //測位開始
                        CamSndReStart();

                        //カメラ起動待機
                        LOGGER.InfoPlus($"{C_SURVEILLANCE_RESTART_WATETIME}ms待機後走行再開します。", $"{CamType}");
                        Task.Delay(C_SURVEILLANCE_RESTART_WATETIME);
                        //m_pause_tvecs_control_state_flg =　OperationConst.C_NORMAL;

                        //一時停止タイマー再開
                        if (m_timer_pRcv_pause != null) { m_timer_pRcv_pause.Change(C_SURVEILLANCE_INTERVALP_CAM, Timeout.Infinite); }
                    }
                    else
                    {
                        //入ることはないはず
                        //走行停止タイマー開始
                        m_timer_pRcv_stop.Change(C_SURVEILLANCE_INTERVALS_CAM, Timeout.Infinite);
                    }

                }

            }
            catch (Exception ex)
            {
                AppLog.GetInstance().Error(ex.Message);
                AppLog.GetInstance().Error(ex.StackTrace);
                //EmergencyEndProcess(Res.ErrorStatus.UNKNOWN_ERR.Code);
                EmergencyEndProcessEventArgs ea = new EmergencyEndProcessEventArgs
                {
                    methodname = $"{MethodBase.GetCurrentMethod().Name}",
                    errcd = Res.ErrorStatus.UNKNOWN_ERR.Code
                };
                if (Emergency != null) { Emergency(ea); }
            }
            finally
            {
                //m_vectorreget_start = false;
                //ヘルスチェック再開
                if (m_timer_hch != null) { m_timer_hch.Change(C_CHT_INTERVAL, C_CHT_INTERVAL); }
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        /// <summary>
        /// ベクトル再取得処理
        /// </summary>
        /// <returns>処理結果コード</returns>
        private string ReGetVectorReq()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                string result = Res.ErrorStatus.UNKNOWN_ERR.Code;

                List<double> vlist = new List<double>();

                for (int i = 0; i < C_RESTART_VECTER_GET_NUM; i++)
                {

                    //取得失敗時リトライ 2022/12/22 ADD
                    for (int j = 0; j < C_CAM_RETRY_NUM; j++)
                    {

                        SndFirstVectorReq vectorReq = new SndFirstVectorReq();
                        vectorReq.CamID = CamRcvInfo.CamID;
                        vectorReq.carPosi = m_CarMaskPosition;
                        //送信
                        string rcvmsg = m_camTcpCl.SendMessage(vectorReq, true);
                        FirstVectorReqRes msg = new FirstVectorReqRes(rcvmsg);
                        switch (int.Parse(msg.Status))
                        {
                            case (int)FirstVectorReqRes.Code.Normal:
                                //初期ベクトル取得
                                double vector = CamRcvVectorSet(double.Parse(msg.FirstVectorX), double.Parse(msg.FirstVectorY));
                                vlist.Add(vector);

                                CamRcvInfo.Vector = CommonProc.ToDegree(vector);
                                CamRcvInfo.xVector = double.Parse(msg.FirstVectorX);
                                CamRcvInfo.yVector = double.Parse(msg.FirstVectorY);

                                LOGGER.InfoPlus($"ベクトルを取得しました。ベクトル：{CamRcvInfo.Vector.ToString()},X：{msg.FirstVectorX.ToString()},Y：{msg.FirstVectorY.ToString()}", $"{CamType}");
                                result = Res.ErrorStatus.NORMAL.Code;
                                break;
                            case (int)FirstVectorReqRes.Code.StartFailure:
                                //開始不可
                                result = Res.ErrorStatus.CAM_START_FAILURE.Code;
                                break;
                            case (int)FirstVectorReqRes.Code.ProcFailure:
                                //処理異常
                                result = Res.ErrorStatus.CAM_PROC_FAILURE.Code;
                                break;
                            case (int)FirstVectorReqRes.Code.Failure:
                                //測位失敗
                                result = Res.ErrorStatus.CAM_DETECT_FAILURE.Code;
                                break;
                        }
                        if (result != Res.ErrorStatus.NORMAL.Code)
                        {
                            //異常であればリトライ
                            if (j == C_CAM_RETRY_NUM - 1)
                            {
                                LOGGER.InfoPlus($"ベクトル取得失敗。リトライ回数実施しました。エラーコード₌{result}", $"{CamType}");
                            }
                            else
                            {
                                LOGGER.InfoPlus($"ベクトル取得失敗。リトライします。エラーコード₌{result}", $"{CamType}");
                                continue;
                            }
                        }
                        else
                        {
                            //成功したら次
                            break;
                        }
                    }

                    if (result != Res.ErrorStatus.NORMAL.Code)
                    {
                        //異常であれば即終了
                        break;
                    }

                }

                if (result == Res.ErrorStatus.NORMAL.Code)
                {
                    vlist.Sort();
                    //終了後ベクトル取得
                    double yawrate;
                    yawrate = vlist[C_RESTART_VECTER_GET_NUM / 2];
                    CamRcvInfo.Vector = CommonProc.ToDegree(yawrate);
                    CamRcvInfo.xVector = (Math.Floor(Math.Cos(yawrate) * Math.Pow(10, 5))) / Math.Pow(10, 5);
                    CamRcvInfo.yVector = (Math.Floor(Math.Sin(yawrate) * Math.Pow(10, 5))) / Math.Pow(10, 5);
                    LOGGER.InfoPlus($"ベクトルを確定しました。ベクトル：{CamRcvInfo.Vector.ToString()},X：{CamRcvInfo.xVector.ToString()},Y：{CamRcvInfo.yVector.ToString()}", $"{CamType}");
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }
        #endregion

        #region 画角ずれ検知要求

        public void ViewAngleShiftCheckAll()
        {
            string result = Res.CamShiftStatus.NORMAL.Code;

            if (m_CamShiftCheckStatus != Res.CamShiftStatus.IN_ACTION.Code)
            {
                lock (m_healthlock)
                {
                    //画角ずれチェック
                    if (C_CAM_SHIFT_CHECK)
                    {
                        m_CamShiftCheckStatus = Res.CamShiftStatus.IN_ACTION.Code;

                        try
                        {
                            //ヘルスチェックを一時停止
                            if (m_timer_hch == null)
                            {
                                m_CamShiftCheckStatus = Res.CamShiftStatus.NOT_ACTIVATE.Code; //※接続されてないのに通知が来た場合 対処法検討
                                return;
                            }
                            m_timer_hch.Change(Timeout.Infinite, Timeout.Infinite);

                            string camid = "0000";
                            result = ViewAngleShiftCheck(ref camid);
                            if (result == Res.ErrorStatus.NORMAL.Code)
                            {
                                //画角ずれなし
                            }
                            else
                            {
                                //処理失敗
                                m_error_camid = camid;
                            }

                            m_error_camid = camid;
                        }
                        finally
                        {
                            //ヘルスチェック再開
                            if (m_timer_hch != null) { m_timer_hch.Change(C_CHT_INTERVAL, C_CHT_INTERVAL); }
                        }
                    }
                }

                m_CamShiftCheckStatus = result;
            }
            else
            {
                LOGGER.InfoPlus("画角ズレ検知すでに実行中のためスルー", $"{CamType}");
            }
        }


        private string ViewAngleShiftCheck(ref string camid)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {

                string result = Res.ErrorStatus.UNKNOWN_ERR.Code;
                LOGGER.InfoPlus("画角ずれチェック開始", $"{CamType}");

                for (int i = 0; i <= m_CamList.Count() - 1; i++)
                {
                    //取得失敗時リトライ
                    for (int j = 0; j < C_CAM_RETRY_NUM; j++)
                    {
                        //画角ずれ検知要求
                        SndViewAngleCheck sndmsg = new SndViewAngleCheck()
                        {
                            CamID = m_CamList[i].CamID
                        };
                        string rcvmsg = m_camTcpCl.SendMessage(sndmsg, true);
                        LOGGER.InfoPlus($"画角ずれ検知応答を受信しました。{m_CamList[i].CamID}", $"{CamType}");

                        //検索応答
                        ViewAngleCheckRes rcv = new ViewAngleCheckRes(rcvmsg);
                        switch (int.Parse(rcv.Status))
                        {
                            case (int)ViewAngleCheckRes.Code.Nothing:
                                //画角ずれなし
                                LOGGER.InfoPlus($"画角ずれなし", $"{CamType}");
                                result = Res.ErrorStatus.NORMAL.Code;
                                break;
                            case (int)ViewAngleCheckRes.Code.Occurred:
                                //画角ずれあり(異常)
                                result = Res.ErrorStatus.CAM_SHIFT_ERR.Code;
                                break;
                            case (int)ViewAngleCheckRes.Code.JudgeFailure:
                                //判定不可(異常)
                                result = Res.ErrorStatus.CAM_JUDGE_FAILURE.Code;
                                break;
                            case (int)ViewAngleCheckRes.Code.StartFailure:
                                //開始不可(異常)
                                result = Res.ErrorStatus.CAM_START_FAILURE.Code;
                                break;
                            case (int)ViewAngleCheckRes.Code.ProcFailure:
                                //処理異常(異常)
                                result = Res.ErrorStatus.CAM_PROC_FAILURE.Code;
                                break;
                            case (int)ViewAngleCheckRes.Code.DetectFailure:
                                //検知失敗(異常)
                                result = Res.ErrorStatus.CAM_DETECT_FAILURE.Code;
                                break;
                        }

                        if (result != Res.ErrorStatus.NORMAL.Code)
                        {
                            //異常であればリトライ
                            if (j == C_CAM_RETRY_NUM - 1)
                            {
                                LOGGER.InfoPlus($"画角ズレ検知取得失敗。リトライ回数実施しました。エラーコード₌{result}", $"{CamType}");
                            }
                            else
                            {
                                LOGGER.InfoPlus($"画角ズレ検知取得失敗。リトライします。エラーコード₌{result}", $"{CamType}");
                                continue;
                            }
                        }
                        else
                        {
                            //成功したら次
                            break;
                        }
                    }

                    //異常があったら終了する
                    if (result != Res.ErrorStatus.NORMAL.Code)
                    {
                        string msg = $"画角ずれチェックにて異常を検知しました。CamID={m_CamList[i].CamID}";
                        LOGGER.InfoPlus(msg, $"{CamType}");

                        camid = m_CamList[i].CamID;
                        break;
                    }
                }
                LOGGER.InfoPlus($"画角ずれチェック終了:{result}", $"{CamType}");
                return result;
            }
            catch (Exception e)
            {
                ExceptionProcess.ComnExceptionConsoleProcess(e);
                return Res.ErrorStatus.CAM_PROC_FAIL.Code;
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }
        #endregion

        #region 車両検索
        private string CarSearch()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                string result = Res.ErrorStatus.UNKNOWN_ERR.Code;

                //取得失敗時リトライ
                for (int j = 0; j < C_CAM_RETRY_NUM; j++)
                {
                    //車両検索
                    SndCarSearch sndcs = new SndCarSearch()
                    {
                        CamID = C_START_CAM_ID
                    };
                    string rcvmsg = m_camTcpCl.SendMessage(sndcs, true);

                    CarSearchRes csmsg = new CarSearchRes(rcvmsg);
                    LOGGER.InfoPlus($"[受信内容]制御指示種別{csmsg.CamID},結果：{csmsg.Status}", $"{CamType}");

                    //検索応答                
                    switch (int.Parse(csmsg.Status))
                    {
                        case (int)CarSearchRes.Code.Normal:
                            //正常
                            for (int i = 0; i < m_CamList.Count; i++)
                            {
                                if (m_CamList[i].CamID == C_START_CAM_ID)
                                {
                                    m_CamList[i].CarList = csmsg.SearchPositionList;
                                    break;
                                }
                            }
                            result = Res.ErrorStatus.NORMAL.Code;
                            break;
                        case (int)CarSearchRes.Code.NumOver:
                            //規定台数超過
                            result = Res.ErrorStatus.CAM_SEARCH_OVER.Code;
                            break;
                        case (int)CarSearchRes.Code.StartFailure:
                            //開始不可
                            result = Res.ErrorStatus.CAM_START_FAILURE.Code;
                            break;
                        case (int)CarSearchRes.Code.ProcFailure:
                            //処理異常
                            result = Res.ErrorStatus.CAM_PROC_FAILURE.Code;
                            break;
                        case (int)CarSearchRes.Code.Failure:
                            //検索失敗
                            result = Res.ErrorStatus.CAM_DETECT_FAILURE.Code;
                            break;
                    }


                    if (result != Res.ErrorStatus.NORMAL.Code)
                    {
                        if (j == C_CAM_RETRY_NUM - 1)
                        {
                            LOGGER.InfoPlus($"車両検索失敗。リトライ回数実施しました。エラーコード₌{result}", $"{CamType}");
                        }
                        else
                        {
                            LOGGER.InfoPlus($"車両検索失敗。リトライします。エラーコード₌{result}", $"{CamType}");
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        #endregion

        #region 制御対象車両判定
        private bool ControlCarSelect()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            bool result = false;

            for (int i = 0; i < m_CamList.Count; i++)
            {
                //スタート位置のカメラを探す
                if (m_CamList[i].CamID == C_START_CAM_ID)
                {
                    for (int j = 0; j < m_CamList[i].CarList.Count; j++)
                    {
                        if (CommonProc.CheckOverlap(m_CamList[i].CarList[j].CarPosition, C_START_POS))
                        {
                            m_CamList[i].CarList[j].ControlSubject = true;
                            result = true;
                            LOGGER.InfoPlus($"制御対象車両を発見しました。", $"{CamType}");
                            break;
                        }
                    }
                    break;
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return result;
        }
        #endregion

        #region 初期ベクトル取得
        private string FirstVectorReq()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                string result = Res.ErrorStatus.UNKNOWN_ERR.Code;
                LOGGER.InfoPlus("初期ベクトル測位開始", $"{CamType}");
                //カメラリスト
                foreach (CamMaster cm in m_CamList)
                {
                    //スタート位置カメラ判定
                    if (cm.CamID == C_START_CAM_ID)
                    {
                        //車両検索情報
                        foreach (SearchPosition fs in cm.CarList)
                        {
                            //制御対象車両判定
                            if (fs.ControlSubject)
                            {
                                List<double> vlist = new List<double>();

                                //初期ベクトル取得回数
                                for (int i = 0; i < C_FIRST_VECTER_GET_NUM; i++)
                                {
                                    //取得失敗時リトライ
                                    for (int j = 0; j < C_CAM_RETRY_NUM; j++)
                                    {
                                        SndFirstVectorReq vectorReq = new SndFirstVectorReq();
                                        vectorReq.CamID = cm.CamID;
                                        vectorReq.carPosi = fs.CarPosition;
                                        //送信
                                        string rcvmsg = m_camTcpCl.SendMessage(vectorReq, true);
                                        FirstVectorReqRes msg = new FirstVectorReqRes(rcvmsg);
                                        switch (int.Parse(msg.Status))
                                        {
                                            case (int)FirstVectorReqRes.Code.Normal:
                                                //初期ベクトル取得
                                                double vector = CamRcvVectorSet(double.Parse(msg.FirstVectorX), double.Parse(msg.FirstVectorY));
                                                vlist.Add(vector);

                                                CamRcvInfo.Vector = CommonProc.ToDegree(vector);
                                                CamRcvInfo.xVector = double.Parse(msg.FirstVectorX);
                                                CamRcvInfo.yVector = double.Parse(msg.FirstVectorY);

                                                LOGGER.InfoPlus($"初期ベクトルを取得しました。初期ベクトル：{CamRcvInfo.Vector.ToString()},X：{msg.FirstVectorX.ToString()},Y：{msg.FirstVectorY.ToString()}", $"{CamType}");
                                                result = Res.ErrorStatus.NORMAL.Code;
                                                break;
                                            case (int)FirstVectorReqRes.Code.StartFailure:
                                                //開始不可
                                                result = Res.ErrorStatus.CAM_START_FAILURE.Code;
                                                break;
                                            case (int)FirstVectorReqRes.Code.ProcFailure:
                                                //処理異常
                                                result = Res.ErrorStatus.CAM_PROC_FAILURE.Code;
                                                break;
                                            case (int)FirstVectorReqRes.Code.Failure:
                                                //測位失敗
                                                result = Res.ErrorStatus.CAM_DETECT_FAILURE.Code;
                                                break;
                                        }

                                        if (result != Res.ErrorStatus.NORMAL.Code)
                                        {
                                            //異常であればリトライ
                                            if (j == C_CAM_RETRY_NUM - 1)
                                            {
                                                LOGGER.InfoPlus($"初期ベクトル取得失敗。リトライ回数実施しました。エラーコード₌{result}", $"{CamType}");
                                            }
                                            else
                                            {
                                                LOGGER.InfoPlus($"初期ベクトル取得失敗。リトライします。エラーコード₌{result}", $"{CamType}");
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            //成功したら次
                                            break;
                                        }

                                    }

                                    if (result != Res.ErrorStatus.NORMAL.Code)
                                    {
                                        //異常であれば即終了
                                        break;
                                    }
                                }

                                if (result == Res.ErrorStatus.NORMAL.Code)
                                {
                                    vlist.Sort();
                                    //終了後初期ベクトル取得
                                    double yawrate;
                                    if (C_B_START_YAWRATE_USE)
                                    {
                                        yawrate = CommonProc.ToRadian(C_START_YAWRATE);
                                    }
                                    else
                                    {
                                        // 中央値を取得
                                        yawrate = vlist[C_FIRST_VECTER_GET_NUM / 2];
                                    }
                                    CamRcvInfo.Vector = CommonProc.ToDegree(yawrate);
                                    CamRcvInfo.xVector = (Math.Floor(Math.Cos(yawrate) * Math.Pow(10, 5))) / Math.Pow(10, 5);
                                    CamRcvInfo.yVector = (Math.Floor(Math.Sin(yawrate) * Math.Pow(10, 5))) / Math.Pow(10, 5);
                                    LOGGER.InfoPlus($"初期ベクトルを確定しました。初期ベクトル：{CamRcvInfo.Vector.ToString()},X：{CamRcvInfo.xVector.ToString()},Y：{CamRcvInfo.yVector.ToString()}", $"{CamType}");
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        #endregion

        #region ### 電文定義 ###

        #region ## 受信 ##
        /// <summary>
        ///測位開始応答
        ///</summary>
        public class SndStartDataRes : RcvMsgBase
        {
            public const string ID = "02";
            /// <summary>
            //// ボデーNO
            //// </summary>
            //// <returns></returns>
            [Order(3), Length(5)]
            public string CarNo { get; set; }
            /// <summary>
            //// 測位開始ステータス 0:正常開始 1:開始不可
            //// </summary>
            //// <returns></returns>
            [Order(4), Length(1)]
            public string StartStatus { get; set; }

            public SndStartDataRes(string rcv_msg) : base(rcv_msg) { }
        }

        /// <summary>
        /// 測位情報
        /// </summary>
        public class PanaMsgStatusNotifyRes : CommMsgBase
        {
            public const string ID = "20";

            private const int C_CARNO_LENGTH = 5;
            private const int C_ANALYSTID_LENGTH = 3;
            private const int C_CAMERAID_LENGTH = 4;
            private const int C_STARTTIME_LENGTH = 9;
            private const int C_PROCTIME_LENGTH = 5;
            private const int C_INFOCOUNT_LENGTH = 1;

            // ボデーNO
            public string CarNo { get; set; }
            // 解析装置ID
            public string AnalystID { get; set; }
            // カメラID
            public string CameraID { get; set; }
            // 画像認識開始時刻
            public string StartTime { get; set; }
            // 処理時間
            public string ProcTime { get; set; }
            // 測位車両情報数
            public int CarCount { get; set; }
            public List<CamPositioningInfo> CamPositioningInfoList { get; set; }

            public PanaMsgStatusNotifyRes(string rcv_msg)
            {
                List<PropertyInfo> orderedProps = new List<PropertyInfo>(GetType().GetProperties())
                         .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                         .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
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
                        prop.SetValue(this, strVal);
                    }
                    else
                    {
                        throw new UserException("Invalid type property");
                    }
                    substringFrom += length;
                }

                this.CarNo = rcv_msg.Substring(substringFrom, C_CARNO_LENGTH);
                substringFrom += C_CARNO_LENGTH;

                this.AnalystID = rcv_msg.Substring(substringFrom, C_ANALYSTID_LENGTH);
                substringFrom += C_ANALYSTID_LENGTH;

                this.CameraID = rcv_msg.Substring(substringFrom, C_CAMERAID_LENGTH);
                substringFrom += C_CAMERAID_LENGTH;

                this.StartTime = rcv_msg.Substring(substringFrom, C_STARTTIME_LENGTH);
                substringFrom += C_STARTTIME_LENGTH;

                this.ProcTime = rcv_msg.Substring(substringFrom, C_PROCTIME_LENGTH);
                substringFrom += C_PROCTIME_LENGTH;

                List<CamPositioningInfo> CamPositioningInfoList = new List<CamPositioningInfo>();
                string strCamPositioningInfoCount = rcv_msg.Substring(substringFrom, C_INFOCOUNT_LENGTH);
                substringFrom += C_INFOCOUNT_LENGTH;
                int CamPositioningInfoCount = int.Parse(strCamPositioningInfoCount);
                this.CarCount = CamPositioningInfoCount;
                List<PropertyInfo> CamPositioningInfoProp = new List<PropertyInfo>(typeof(CamPositioningInfo).GetProperties())
                    .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                    .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
                    .OrderBy(prop => OrderAttribute.GetOrder(prop))
                    .ToList();
                for (int i = 0; i < CamPositioningInfoCount; i++)
                {
                    CamPositioningInfo CamPositioningInfo = new CamPositioningInfo();
                    foreach (PropertyInfo prop in CamPositioningInfoProp)
                    {
                        var propType = prop.PropertyType;
                        var converter = TypeDescriptor.GetConverter(propType);

                        int length = LengthAttribute.GetLength(prop);
                        string strVal = rcv_msg.Substring(substringFrom, length);

                        if (prop.PropertyType == typeof(int))
                        {
                            prop.SetValue(CamPositioningInfo, converter.ConvertFromString(strVal));
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(CamPositioningInfo, strVal);
                        }
                        else if (prop.PropertyType == typeof(List<PointF>))
                        {
                            int C_POINT_LENGTH = 9;
                            int C_LIST_COUNT = 4;
                            int len = 0;

                            List<PointF> fs = new List<PointF>();
                            for (int j = 0; j < C_LIST_COUNT; j++)
                            {
                                fs.Add(new PointF(float.Parse(strVal.Substring(len, C_POINT_LENGTH)), float.Parse(strVal.Substring(len + C_POINT_LENGTH, C_POINT_LENGTH))));
                                len += C_POINT_LENGTH * 2;
                            }

                            prop.SetValue(CamPositioningInfo, fs);
                        }
                        else
                        {
                            throw new UserException("Invalid type property");
                        }
                        substringFrom += length;
                    }
                    CamPositioningInfoList.Add(CamPositioningInfo);
                }

                this.CamPositioningInfoList = CamPositioningInfoList;
            }
        }

        /// <summary>
        /// 測位情報
        /// </summary>
        public class AvpMsgStatusNotifyRes : CommMsgBase
        {
            public const string ID = "20";

            private const int C_CARNO_LENGTH = 5;
            private const int C_ANALYSTID_LENGTH = 3;
            private const int C_CAMERAID_LENGTH = 4;
            private const int C_STARTTIME_LENGTH = 9;
            private const int C_PROCTIME_LENGTH = 5;
            private const int C_INFOCOUNT_LENGTH = 1;
            private const int C_FILTERCOUNT_LENGTH = 2;
            private const int C_KALMAN_LENGTH = 9;


            // ボデーNO
            public string CarNo { get; set; }
            // 解析装置ID
            public string AnalystID { get; set; }
            // カメラID
            public string CameraID { get; set; }
            // 画像認識開始時刻
            public string StartTime { get; set; }
            // 処理時間
            public string ProcTime { get; set; }
            // 測位車両情報数
            public int CarCount { get; set; }
            public List<CamPositioningInfo> CamPositioningInfoList { get; set; }

            public List<int> KalmanCount { get; set; }
            public List<double> KalmanFilters { get; set; }

            public AvpMsgStatusNotifyRes(string rcv_msg)
            {
                List<PropertyInfo> orderedProps = new List<PropertyInfo>(GetType().GetProperties())
                         .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                         .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
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
                        prop.SetValue(this, strVal);
                    }
                    else
                    {
                        throw new UserException("Invalid type property");
                    }
                    substringFrom += length;
                }

                this.CarNo = rcv_msg.Substring(substringFrom, C_CARNO_LENGTH);
                substringFrom += C_CARNO_LENGTH;

                this.AnalystID = rcv_msg.Substring(substringFrom, C_ANALYSTID_LENGTH);
                substringFrom += C_ANALYSTID_LENGTH;

                this.CameraID = rcv_msg.Substring(substringFrom, C_CAMERAID_LENGTH);
                substringFrom += C_CAMERAID_LENGTH;

                this.StartTime = rcv_msg.Substring(substringFrom, C_STARTTIME_LENGTH);
                substringFrom += C_STARTTIME_LENGTH;

                this.ProcTime = rcv_msg.Substring(substringFrom, C_PROCTIME_LENGTH);
                substringFrom += C_PROCTIME_LENGTH;

                List<CamPositioningInfo> CamPositioningInfoList = new List<CamPositioningInfo>();
                List<int> KalmanCount = new List<int>();
                List<double> KalmanFilters = new List<double>();
                string strCamPositioningInfoCount = rcv_msg.Substring(substringFrom, C_INFOCOUNT_LENGTH);
                substringFrom += C_INFOCOUNT_LENGTH;
                int CamPositioningInfoCount = int.Parse(strCamPositioningInfoCount);
                this.CarCount = CamPositioningInfoCount;
                List<PropertyInfo> CamPositioningInfoProp = new List<PropertyInfo>(typeof(CamPositioningInfo).GetProperties())
                    .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                    .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
                    .OrderBy(prop => OrderAttribute.GetOrder(prop))
                    .ToList();
                for (int i = 0; i < CamPositioningInfoCount; i++)
                {
                    CamPositioningInfo CamPositioningInfo = new CamPositioningInfo();
                    foreach (PropertyInfo prop in CamPositioningInfoProp)
                    {
                        var propType = prop.PropertyType;
                        var converter = TypeDescriptor.GetConverter(propType);

                        int length = LengthAttribute.GetLength(prop);
                        string strVal = rcv_msg.Substring(substringFrom, length);

                        if (prop.PropertyType == typeof(int))
                        {
                            prop.SetValue(CamPositioningInfo, converter.ConvertFromString(strVal));
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(CamPositioningInfo, strVal);
                        }
                        else if (prop.PropertyType == typeof(List<PointF>))
                        {
                            int C_POINT_LENGTH = 9;
                            int C_LIST_COUNT = 4;
                            int len = 0;

                            List<PointF> fs = new List<PointF>();
                            for (int j = 0; j < C_LIST_COUNT; j++)
                            {
                                fs.Add(new PointF(float.Parse(strVal.Substring(len, C_POINT_LENGTH)), float.Parse(strVal.Substring(len + C_POINT_LENGTH, C_POINT_LENGTH))));
                                len += C_POINT_LENGTH * 2;
                            }

                            prop.SetValue(CamPositioningInfo, fs);
                        }
                        else
                        {
                            throw new UserException("Invalid type property");
                        }
                        substringFrom += length;
                    }
                    CamPositioningInfoList.Add(CamPositioningInfo);

                    //kondo↓20230206 change
                    string Filtercount = rcv_msg.Substring(substringFrom, C_FILTERCOUNT_LENGTH);
                    substringFrom += C_FILTERCOUNT_LENGTH;
                    KalmanCount.Add(int.Parse(Filtercount));


                    for (int j = 0; j < KalmanCount.Last() - 1; j++)
                    {
                        string strKalmanFilters = rcv_msg.Substring(substringFrom, C_KALMAN_LENGTH);
                        substringFrom += C_KALMAN_LENGTH;
                        KalmanFilters.Add(double.Parse(strKalmanFilters));
                    }
                }

                this.CamPositioningInfoList = CamPositioningInfoList;
                this.KalmanCount = KalmanCount;
                this.KalmanFilters = KalmanFilters;

            }
        }

        public class CamPositioningInfo : Info
        {
            /// <summary>
            /// 追尾状態
            /// </summary>
            [Order(1), Length(1), Body()]
            public int Status { get; set; }
            /// <summary>
            //// 位置座標X
            //// </summary>
            //// <returns></returns>
            [Order(2), Length(9), Body()]
            public string POS_X { get; set; }
            /// <summary>
            //// 位置座標Y
            //// </summary>
            //// <returns></returns>
            [Order(3), Length(9), Body()]
            public string POS_Y { get; set; }
            /// <summary>
            //// 車両方向ベクトルX
            //// </summary>
            //// <returns></returns>
            [Order(4), Length(8), Body()]
            public string Vector_X { get; set; }
            /// <summary>
            //// 車両方向ベクトルY
            //// </summary>
            //// <returns></returns>
            [Order(5), Length(8), Body()]
            public string Vector_Y { get; set; }
            /// <summary>
            //// 移動方向ベクトルX
            //// </summary>
            //// <returns></returns>
            [Order(6), Length(8), Body()]
            public string MoveVector_X { get; set; }
            /// <summary>
            //// 移動方向ベクトルY
            //// </summary>
            //// <returns></returns>
            [Order(7), Length(8), Body()]
            public string MoveVector_Y { get; set; }
            /// <summary>
            //// 車両重心X座標
            //// </summary>
            //// <returns></returns>
            [Order(8), Length(9), Body()]
            public string Centroid_X { get; set; }
            /// <summary>
            //// 車両重心Y座標
            //// </summary>
            //// <returns></returns>
            [Order(9), Length(9), Body()]
            public string Centroid_Y { get; set; }
            /// <summary>
            //// 車両面積
            //// </summary>
            //// <returns></returns>
            [Order(10), Length(7), Body()]
            public int CarArea { get; set; }
            /// <summary>
            //// 車両マスク信頼度
            //// </summary>
            //// <returns></returns>
            [Order(11), Length(7), Body()]
            public string Reliability { get; set; }

            [Order(12), Length(72), Body()]
            public List<PointF> GlovalPosition { get; set; }
        }

        /// <summary>
        /// 測位終了応答
        /// </summary>
        public class SndEndDataRes : RcvMsgBase
        {
            public const string ID = "31";
            /// <summary>
            //// ボデーNO
            //// </summary>
            //// <returns></returns>
            [Order(3), Length(5)]
            public string CarNo { get; set; }

            public SndEndDataRes(string rcv_msg) : base(rcv_msg) { }
        }


        /// <summary>
        /// ヘルスチェック応答
        /// </summary>
        public class HealthCheckRes : CommMsgBase
        {
            public const int C_CHECKID_LENGTH = 3;
            public const int C_MEASURESTATUS_LENGTH = 1;
            public const string ID = "91";

            // 解析装置ID
            public string CheckID;
            // 測位状態
            public string MeasureStatus;
            //カメラ台数
            public int CamCount;
            public List<HealthInfo> HealthInfoList { get; set; }

            public HealthCheckRes(string rcv_msg)
            {
                List<PropertyInfo> orderedProps = new List<PropertyInfo>(GetType().GetProperties())
                           .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                           .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
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
                        prop.SetValue(this, strVal);
                    }
                    else
                    {
                        throw new UserException("Invalid type property");
                    }
                    substringFrom += length;
                }

                this.CheckID = rcv_msg.Substring(substringFrom, C_CHECKID_LENGTH);
                substringFrom += C_CHECKID_LENGTH;

                this.MeasureStatus = rcv_msg.Substring(substringFrom, C_MEASURESTATUS_LENGTH);
                substringFrom += C_MEASURESTATUS_LENGTH;


                List<HealthInfo> HealthInfoList = new List<HealthInfo>();
                string strHealthInfoCount = rcv_msg.Substring(substringFrom, Info.C_InfoCountDataLength);
                substringFrom += Info.C_InfoCountDataLength;
                int HealthInfoCount = int.Parse(strHealthInfoCount);
                this.CamCount = HealthInfoCount;
                List<PropertyInfo> HealthInfoProp = new List<PropertyInfo>(typeof(HealthInfo).GetProperties())
                    .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                    .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
                    .OrderBy(prop => OrderAttribute.GetOrder(prop))
                    .ToList();
                for (int i = 0; i < HealthInfoCount; i++)
                {
                    HealthInfo HealthInfo = new HealthInfo();
                    foreach (PropertyInfo prop in HealthInfoProp)
                    {
                        var propType = prop.PropertyType;
                        var converter = TypeDescriptor.GetConverter(propType);

                        int length = LengthAttribute.GetLength(prop);
                        string strVal = rcv_msg.Substring(substringFrom, length);

                        if (prop.PropertyType == typeof(int))
                        {
                            prop.SetValue(HealthInfo, converter.ConvertFromString(strVal));
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(HealthInfo, strVal);
                        }
                        else
                        {
                            throw new UserException("Invalid type property");
                        }
                        substringFrom += length;
                    }
                    HealthInfoList.Add(HealthInfo);
                }

                this.HealthInfoList = HealthInfoList;
            }
        }

        public class HealthInfo : Info
        {
            // カメラID
            [Order(0), Length(4), Body()]
            public string camID { get; set; }
            // カメラ状態		
            [Order(1), Length(1), Body()]
            public int status { get; set; }

            public HealthInfo() { }
        }

        /// <summary>
        /// 画角ずれ検知応答
        /// </summary>
        public class ViewAngleCheckRes : RcvMsgBase
        {
            public const string ID = "41";

            public enum Code : int
            {
                Nothing = 0,
                Occurred = 1,
                JudgeFailure = 2,
                StartFailure = 3,
                ProcFailure = 4,
                DetectFailure = 9
            }

            /// <summary>
            /// カメラID
            /// </summary>
            [Order(3), Length(4)]
            public string CamID { get; set; }
            /// <summary>
            /// 画角ずれ検知ステータス
            /// </summary>
            [Order(4), Length(1)]
            public string Status { get; set; }

            public ViewAngleCheckRes(string rcv_msg) : base(rcv_msg) { }
        }

        /// <summary>
        /// 車両検索応答
        /// </summary>
        public class CarSearchRes : CommMsgBase
        {
            public const string ID = "51";
            private const int C_CAMID_LENGTH = 4;
            private const int C_STATUS_LENGTH = 1;
            private const int C_INFOCOUNT_LENGTH = 1;

            public enum Code : int
            {
                Normal = 0,
                NumOver = 1,
                StartFailure = 2,
                ProcFailure = 3,
                Failure = 9
            }

            // カメラID
            public string CamID { get; set; }
            //検索ステータス
            public string Status { get; set; }
            //カメラ台数
            public int CarCount { get; set; }
            public List<SearchPosition> SearchPositionList { get; set; }

            public CarSearchRes(string rcv_msg)
            {
                List<PropertyInfo> orderedProps = new List<PropertyInfo>(GetType().GetProperties())
                           .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                           .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
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
                        prop.SetValue(this, strVal);
                    }
                    else
                    {
                        throw new UserException("Invalid type property");
                    }
                    substringFrom += length;
                }

                this.CamID = rcv_msg.Substring(substringFrom, C_CAMID_LENGTH);
                substringFrom += C_CAMID_LENGTH;
                this.Status = rcv_msg.Substring(substringFrom, C_STATUS_LENGTH);
                substringFrom += C_STATUS_LENGTH;

                List<SearchPosition> SearchPositionList = new List<SearchPosition>();
                string strCarPositionCount = rcv_msg.Substring(substringFrom, C_INFOCOUNT_LENGTH);
                substringFrom += C_INFOCOUNT_LENGTH;
                int CarPositionCount = int.Parse(strCarPositionCount);
                this.CarCount = CarPositionCount;
                List<PropertyInfo> CarPositionProp = new List<PropertyInfo>(typeof(SearchPosition).GetProperties())
                    .Where(prop => Attribute.IsDefined(prop, typeof(OrderAttribute)))
                    .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
                    .OrderBy(prop => OrderAttribute.GetOrder(prop))
                    .ToList();
                for (int i = 0; i < CarPositionCount; i++)
                {
                    SearchPosition CarPosition = new SearchPosition();
                    foreach (PropertyInfo prop in CarPositionProp)
                    {
                        var propType = prop.PropertyType;
                        var converter = TypeDescriptor.GetConverter(propType);

                        int length = LengthAttribute.GetLength(prop);
                        string strVal = rcv_msg.Substring(substringFrom, length);

                        if (prop.PropertyType == typeof(int))
                        {
                            prop.SetValue(CarPosition, converter.ConvertFromString(strVal));
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(CarPosition, strVal);
                        }
                        else if (prop.PropertyType == typeof(List<PointF>))
                        {
                            int C_POINT_LENGTH = 9;
                            int C_LIST_COUNT = 4;
                            int len = 0;

                            List<PointF> fs = new List<PointF>();
                            for (int j = 0; j < C_LIST_COUNT; j++)
                            {
                                fs.Add(new PointF(float.Parse(strVal.Substring(len, C_POINT_LENGTH)), float.Parse(strVal.Substring(len + C_POINT_LENGTH, C_POINT_LENGTH))));
                                len += C_POINT_LENGTH * 2;
                            }

                            prop.SetValue(CarPosition, fs);
                        }
                        else
                        {
                            throw new UserException("Invalid type property");
                        }
                        substringFrom += length;
                    }
                    CarPosition.ControlSubject = false;
                    SearchPositionList.Add(CarPosition);
                }

                this.SearchPositionList = SearchPositionList;
            }
        }

        public class SearchPosition
        {
            [Order(1), Length(72), Body()]
            public List<PointF> CarPosition { get; set; }

            /// <summary>
            /// 制御対象
            /// </summary>
            public bool ControlSubject { get; set; }
        }

        /// <summary>
        /// 初期ベクトル測位応答
        /// </summary>
        public class FirstVectorReqRes : RcvMsgBase
        {
            public const string ID = "61";

            public enum Code : int
            {
                Normal = 0,
                StartFailure = 1,
                ProcFailure = 2,
                Failure = 9
            }

            /// <summary>
            /// カメラID
            /// </summary>
            [Order(3), Length(4)]
            public string CamID { get; set; }
            /// <summary>
            /// 初期ベクトル測位ステータス
            /// </summary>
            [Order(4), Length(1)]
            public string Status { get; set; }
            /// <summary>
            /// 車両方向ベクトルX
            /// </summary>
            [Order(5), Length(8)]
            public string FirstVectorX { get; set; }
            /// <summary>
            /// 車両方向ベクトルY
            /// </summary>
            [Order(6), Length(8)]
            public string FirstVectorY { get; set; }


            public FirstVectorReqRes(string rcv_msg) : base(rcv_msg) { }
        }

        #endregion

        #region ## 送信 ##
        /// <summary>
        /// 測位開始通知
        /// </summary>
        public class SndCamStart : CommSndMsgBase
        {
            public const string ID = "01";
            /// <summary>
            /// ボデーNo
            /// </summary>
            public string CarNo { get; set; }
            /// <summary>
            /// 開始カメラID
            /// </summary>
            public string CamID { get; set; }
            /// <summary>
            /// 測位対象車両位置
            /// </summary>
            public List<PointF> CarPositioning { get; set; }
            /// <summary>
            /// カメラ情報数
            /// </summary>
            public int CamCount { get; set; }
            /// <summary>
            /// カメラ情報
            /// </summary>
            public List<maskcaminfo> maskcaminfos { get; set; }

            public SndCamStart() : base(ID) { }

            protected override string GetBodyString()
            {
                string msg = "";
                msg = CarNo.PadLeft(5, '0') + CamID;
                //測位対象車両位置情報
                Camformat format = new Camformat();
                foreach (PointF pf in CarPositioning)
                {
                    msg += format.formatposition(pf.X);
                    msg += format.formatposition(pf.Y);
                }
                if (maskcaminfos != null)
                {
                    //カメラ情報数
                    msg += string.Format("{0:D2}", maskcaminfos.Count());
                    //カメラ情報
                    foreach (maskcaminfo info in maskcaminfos)
                    {
                        //カメラID
                        msg += info.CamID;
                        //マスク車両情報数
                        msg += string.Format("{0:D1}", info.maskcarpoitions.Count());
                        foreach (List<PointF> lpf in info.maskcarpoitions)
                        {
                            //マスク車両位置情報
                            foreach (PointF pf in lpf)
                            {
                                msg += format.formatposition(pf.X);
                                msg += format.formatposition(pf.Y);
                            }
                        }
                    }
                }
                else { msg += "00"; }

                return msg;
            }

            public class maskcaminfo
            {
                /// <summary>
                /// カメラID
                /// </summary>
                public string CamID { get; set; }
                /// <summary>
                /// マスク対象車両数
                /// </summary>
                public int CarCount { get; set; }

                public List<List<PointF>> maskcarpoitions { get; set; }
            }
        }

        /// <summary>
        /// 測位終了通知
        /// </summary>
        public class SndCamEnd : CommSndMsgBase
        {
            public const string ID = "30";
            /// <summary>
            /// ボデーNo
            /// </summary>
            public string CarNo { get; set; }

            public SndCamEnd() : base(ID) { }
            protected override string GetBodyString()
            {
                return CarNo;
            }
        }

        /// <summary>
        /// ヘルスチェック要求
        /// </summary>
        public class HealthCheck : CommSndMsgBase
        {
            public const string ID = "90";

            public HealthCheck() : base(ID) { }
            protected override string GetBodyString()
            {
                return "";
            }
        }

        /// <summary>
        /// 画角ずれ検知要求
        /// </summary>
        public class SndViewAngleCheck : CommSndMsgBase
        {
            public const string ID = "40";
            /// <summary>
            /// カメラID
            /// </summary>
            public string CamID { get; set; }

            public SndViewAngleCheck() : base(ID) { }
            protected override string GetBodyString()
            {
                return CamID;
            }
        }

        /// <summary>
        /// 車両検索要求
        /// </summary>
        public class SndCarSearch : CommSndMsgBase
        {
            public const string ID = "50";
            /// <summary>
            /// カメラID
            /// </summary>
            public string CamID { get; set; }

            public SndCarSearch() : base(ID) { }
            protected override string GetBodyString()
            {
                return CamID;
            }
        }

        /// <summary>
        /// 初期ベクトル測位要求
        /// </summary>
        public class SndFirstVectorReq : CommSndMsgBase
        {
            public const string ID = "60";
            /// <summary>
            /// カメラID
            /// </summary>
            public string CamID { get; set; }
            /// <summary>
            /// 取得対象車両位置
            /// </summary>
            public List<PointF> carPosi { get; set; }


            public SndFirstVectorReq() : base(ID) { }
            protected override string GetBodyString()
            {
                string msg = CamID;
                Camformat format = new Camformat();
                foreach (PointF pf in carPosi)
                {
                    msg += format.formatposition(pf.X);
                    msg += format.formatposition(pf.Y);
                }

                return msg;
            }
        }

        #endregion

        public class Camformat
        {
            public string formatposition(double val)
            {
                string sign = (val >= 0) ? "+" : "-";

                double d = Math.Abs(val);

                // 整数部書式指定
                string s = sign + ((int)d).ToString("0000");
                // 次に小数部分のみを計算して書式指定を行う
                s += (d - ((int)d)).ToString("F3").TrimStart('0');

                return s;
            }
        }

        public class Info
        {
            public static int C_InfoCountDataLength = 2;

            public string GetString()
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
                            sb.Append(strVal.PadRight(length, '0'));
                            break;
                        case null:
                            sb.Append("".PadRight(length, '0'));
                            break;
                        default:
                            throw new UserException("Invalid Type");
                    }
                }
                return sb.ToString();
            }

            public int GetDataLength()
            {
                return new List<PropertyInfo>(GetType().GetProperties())
                       .Where(prop => Attribute.IsDefined(prop, typeof(BodyAttribute)))
                       .Where(prop => Attribute.IsDefined(prop, typeof(LengthAttribute)))
                       .Sum(prop => LengthAttribute.GetLength(prop));
            }
        }

        #endregion


        #region カメラマスタ
        //※見直し対象
        public class CamMaster
        {
            /// <summary>
            /// カメラID
            /// </summary>
            public string CamID { get; set; }
            /// <summary>
            /// 検索車両情報
            /// </summary>
            public List<SearchPosition> CarList { get; set; }
        }
        #endregion

        #region FirstVectorSet
        /// <summary>
        /// 初期ベクトルからなす角θを求める
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="f">初期進行方向としての使用の有無</param>
        public double CamRcvVectorSet(double x, double y)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            double rad = 0;
            // 原点方向a（1,0）と画像解析より測位されたベクトルb(x,y)のなす角を求める
            rad = (x == 0) ? CommonProc.ToRadian(90) : Math.Atan2(y, x);

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return rad;

        }

        #endregion


        #region 削除予定
        private void ReadFile<T>(string filepath, ref T data)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            try
            {
                if (File.Exists(filepath))
                {
                    System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(T));
                    using (System.IO.FileStream fs = new System.IO.FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        data = (T)xml.Deserialize(fs);
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw new UserException($"設定ファイルが不正です。{ex.Message}");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion
    }


}
