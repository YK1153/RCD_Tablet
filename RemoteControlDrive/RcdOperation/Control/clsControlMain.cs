using RcdCmn;
using RcdDao;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AtrptShare;
using AtrptCalc;
using System.Threading;
using CommWrapper;
using System.Drawing;
using static RcdDao.MPLCDao;
using RcpOperation.Common;
using RcdOperationSystemConst;
using static RcdDao.MCourseDao;
using static RcdDao.MCourseAreaDao;
using static RcdDao.MCourseNodeDao;
using static RcdDao.MCarSpecDao;
using static RcdDao.MFacilityDao;
using RcdOperation.PlcControl;
using System.Timers;
using static RcdDao.MSystemValue;
using System.Windows.Forms;
using MapViewer;
using static RcdDao.MObjctLabelDao;
using System.Diagnostics;
using static RcdDao.MControlSettingDao;
using static RcdDao.MControlPLCDao;
using static RcdOperationSystemConst.OperationConst;
using RcdOperation.Hazard;

namespace RcdOperation.Control
{
    public partial class ControlMain : Form
    {
        List<GuideareaJudge> guideAreaList = new List<GuideareaJudge>();
        private DrawConfigForm m_drawform = new DrawConfigForm();

        private System.Threading.Timer m_timer_cc;
        private clsCalculator m_calc;

        /// <summary>
        /// 車両情報送信タイマー
        /// </summary>
        private System.Threading.Timer m_timer_ci;
        private int m_ci_interval = 100;

        private PointF m_newPPoint = new PointF();
        /// <summary>
        /// 連続異常測位点カウント
        /// </summary>
        private int m_ErrCnt = 0;

        DateTime m_now;
        /// <summary>
        /// 車両制御状態 1:制御中 ※見直し
        /// </summary>
        private int m_CarControlStatus = 0;
        /// <summary>
        /// 異常検知状態 true:異常検知中
        /// </summary>
        private bool m_EmergencyStop;
        /// <summary>
        /// 車両制御送信開始 0:未
        /// </summary>
        private int m_calcstarted = 0;
        private bool m_GoalCheckSnd = false;
        /// <summary>
        /// 制御状態 0:正常 1:異常
        /// </summary>
        private int m_error_flg = 0;
        /// <summary>
        /// ゴール状況
        /// </summary>
        private int m_goal_flg = 0;
        /// <summary>
        /// 一時停止状態 0:正常 1:一時停止
        /// </summary>
        private int m_pause_flg = 0;
        /// <summary>
        /// 一時停止状態(ゴール判定) 0:正常 1:一時停止
        /// </summary>
        private int m_pause_goal_flg = 0;
        /// ゴール後停止状況
        /// </summary>
        private int m_goalcomp_flg = 0;
        /// <summary>
        /// 在籍センサ確認
        /// </summary>
        private bool m_senser_check = true;
        /// <summary>
        /// 範囲外回数
        /// </summary>
        int m_outarea;
        private OtherAreaRetention[] m_otherArea;
        private ConfigData m_ConfigData = new ConfigData();
        private ControlCenterData m_controlCenterData = new ControlCenterData();
        private string m_TerminalStatus;
        private object m_carcontrollock = new object();
        private AVPAppOutputData m_avpappOutputData = new AVPAppOutputData();
        private ObservedData m_observedData = new ObservedData();

        private TimeSpan m_sertimespan;


        private AtrptShare.CarSpec m_carSpec = new AtrptShare.CarSpec();

        //private double m_carAngle = new double();

        private string[] m_cornername = { "左後端", "左前端", "右前端", "右後端" };

        public List<RouteNode> m_RouteNodes = new List<RouteNode>();
        public List<SpeedArea> m_SpeedAreas = new List<SpeedArea>();
        
        private int m_contain_idx = -1;
        List<CarCornerinfo> m_CarCornerinfo = new List<CarCornerinfo>();

        #region ### 共通クラス変数 ###
        AppLog LOGGER = AppLog.GetInstance(); 
        int m_PlantNo;
        int m_stationNo;
        public ConnectionConst m_connectionConst;

        //Cam
        private OperationConst.CamType MainCamType = OperationConst.CamType.AVP; //　※
        private OperationConst.CamType? SubCamType = OperationConst.CamType.Pana; //　※

        CommManagement m_ManagementCl;
        CommCam m_camClPana;
        CommCam m_camClAvp;

        /// <summary> 全コースマスタ情報 </summary>
        List<CourseInfo> m_CourseInfoList;

        // Status
        CarInfo m_CarInfo = new CarInfo();

        /// <summary> CarSpec DB master information </summary>
        internal List<MCarSpec> m_carSpecs;

        // PLC
        internal List<MPlcDevice> m_plc_debice;
        internal List<PLCUnitStatus> m_plcunit;
        internal PLCControl m_PLCControl;

        // Facility
        internal List<FacilityType> m_facilityTypes;
        internal List<FacilityStatusMsg> m_facilityStatusMsgs;
        internal Dictionary<string, FacilityStatus> m_dicFacilityStatus;

        // Threads
        internal List<FacilityControl> m_facilityControlThreads;
        internal List<FacilityControl> m_facControlWaits;

        // Setting
        internal List<MControlPlc> m_controlPLC;
        internal MControlSetting m_controlSetting;

        // Message
        private List<MSystemMessage> m_SystemMsg;

        // SystemTimer
        /// <summary> PLC + 設備ステータス更新タイマー</summary>
        private System.Timers.Timer m_plcstatusLogTimer;
        /// <summary> 生産指示取得タイマー </summary>
        private System.Timers.Timer m_ProductionInstructionsTimer;
        /// <summary> 準備起動判定取得タイマー </summary>
        private System.Timers.Timer m_ReadyBootTimer;
        /// <summary> 起動判定取得タイマー </summary>
        private System.Timers.Timer m_ReadyOnTimer;

        // Status
        internal List<FacilityStatus> m_FacilityStatus;

        //
        private List<CamList> m_camList;
        #endregion

        #region ### クラス変数 ###
        /// <summary>
        /// 制御中車両ボデーNo
        /// </summary>
        private string m_CarNo = "";
        private string m_CarSpec = "";

        #endregion

        #region ### コンストラクタ ###
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="StationNo">工程番号</param>
        public ControlMain(int PlantSid, int StationNo,ConnectionConst Const)
        {
            AppLog.GetInstance().Debug($"OperationMain StationoNo:{StationNo} constructor start");

            InitializeComponent();

            LOGGER.Info($"起動");

            // DBデータ取得ー初期化
            // InitStatus();

            // ※初期処理　※DBデータ取得(InitStatus)の代わり
            Dummy_firstprocess();

            // 値受取
            m_PlantNo = PlantSid;
            m_stationNo = StationNo;
            m_connectionConst = Const;

            // 接続セット～接続
            ConnectSet();

            // 現在の制御モード同期


            // 生産開始読み取り開始
            m_ProductionInstructionsTimer = new System.Timers.Timer { Interval = m_STATUS_UPDATE_INTERVAL };
            m_ProductionInstructionsTimer.Elapsed += OnProductionInstructionsTimerElapsed;
            m_ProductionInstructionsTimer.Start();
            // 起動準備タイマー準備
            m_ReadyBootTimer = new System.Timers.Timer { Interval = m_STATUS_UPDATE_INTERVAL };
            m_ReadyBootTimer.Elapsed += OnReadyBootTimerElapsed;
            // 起動タイマー準備
            m_ReadyOnTimer = new System.Timers.Timer { Interval = m_STATUS_UPDATE_INTERVAL };
            m_ReadyOnTimer.Elapsed += OnReadyOnTimerElapsed;

            // 表示画像セット
            SetViewConfig();
            //カメラ状態表示のためのデータグリッドビューの設定
            SetGridView();

            AppLog.GetInstance().Debug($"OperationMain StationoNo:{StationNo} constructor end");
        }


        /// <summary>
        /// DB情報取得
        /// </summary>
        private void InitStatus()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //マスタ情報取得

            //車両仕様情報取得
            m_carSpecs = new MCarSpecDao().GetAllMCarSpecInfo();

            // PLC情報取得
            MPLCDao fPLCDao = new MPLCDao();
            m_plc_debice = fPLCDao.GetAllPLCDevice();
            m_plcunit = new List<PLCUnitStatus>();
            foreach (MPlcDevice plc in m_plc_debice)
            {
                m_plcunit.Add(new PLCUnitStatus(plc));
            }

            // 全コース情報取得
            List<MCourse> courselist = new List<MCourse>();
            MCourseDao courseDao = new MCourseDao();
            courselist = courseDao.GetAllMCourseInfo(m_PlantNo , m_stationNo);
            m_CourseInfoList = new List<CourseInfo>();
            foreach (MCourse course in courselist)
            {
                m_CourseInfoList.Add(new CourseInfo(course));
            }

            //設備情報取得
            MFacilityDao fDao = new MFacilityDao();
            m_facilityControlThreads = new List<FacilityControl>();

            m_dicFacilityStatus = fDao.GetAllFacilityStatus().ToDictionary(fac => fac.FacilityID);

            List<SettingMap> settingMaps = fDao.GetAllrFacilityCtrlMap();

            m_facilityTypes = fDao.GetAllFacilityTypes();
            m_facilityStatusMsgs = fDao.GetFacilityStatusMsgs();

            List<RFacility> rFacilities = fPLCDao.GetAllrFacilityInfo();
            List<RPLCBitSetting> rPLCBitSettings = fPLCDao.GetAllrPLCBitSettings();

            PLCControl.Initialize(rFacilities, rPLCBitSettings, settingMaps, m_plcunit, m_MAX_IO_READ_WAIT);
            m_PLCControl = PLCControl.GetInstance();

            //PLCステータスDB記録
            m_plcstatusLogTimer = new System.Timers.Timer
            {
                Interval = m_STATUS_UPDATE_INTERVAL
            };
            m_plcstatusLogTimer.Elapsed += OnPlcStatusLogTimerElapsed;
            m_plcstatusLogTimer.Start();

            // 侵入検知ラベル取得
            m_labelList = new List<ObjectLabel>();
            List<MObjLabel> mObjLabel = new MObjctLabelDao().GetAllMObjLabelInfo();
            foreach (MObjLabel obj in mObjLabel)
            {
                m_labelList.Add(new ObjectLabel(obj));
            }

            // 設定値取得
            m_controlPLC = new MControlPLCDao().GetAllMControlPlcInfo();
            m_controlSetting = new MControlSettingDao().GetAllMControlSettingInfo()[0];

            // 安全監視
            m_deviations = new MImageComparisolDao().GetAllMImageComparisolInfo();


            //システムメッセージ取得
            m_SystemMsg = new MSystemValue().GetSystemMsgOf();


            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 接続情報初期化
        /// </summary>
        private void ConnectSet()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            // ManagementPCへ接続
            m_ManagementCl = new CommManagement(LOGGER, m_controlSetting.StationLogicalName, m_controlSetting.StationPointCode);

            // 自走TVECSへ接続
            SetCommTvecs();

            // Pana画像解析へ接続
            m_camClPana = new CommCam(m_connectionConst,OperationConst.CamType.Pana);
            m_camClPana.Emergency += new CommCam.EmergencyEndProcessEventHundler(EventChatchEmergency);
            m_camClPana.StartErr += new CommCam.EmergencyEndProcessEventHundler(EventChatchControlStartStatus);
            m_camClPana.CamChange += new CommCam.AnalyzerCamNoChangedEventHundler(SafetyMonitoringCamNoChange);

            // WCo画像解析へ接続
            m_camClAvp = new CommCam(m_connectionConst, OperationConst.CamType.AVP);
            m_camClAvp.Emergency += new CommCam.EmergencyEndProcessEventHundler(EventChatchEmergency);
            m_camClAvp.StartErr += new CommCam.EmergencyEndProcessEventHundler(EventChatchControlStartStatus);
            m_camClAvp.CamChange += new CommCam.AnalyzerCamNoChangedEventHundler(SafetyMonitoringCamNoChange);

            // 物体検知へ接続
            InitializeSafetyMonitoring();

            // 操作端末ポートオープン
            ConstructorCommTerminal();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// データグリッドビュー初期設定
        /// </summary>
        private void SetGridView()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            dgv_MainCamHealth.ColumnCount = 2;
            dgv_MainCamHealth.Columns[0].HeaderText = "カメラID";
            dgv_MainCamHealth.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv_MainCamHealth.Columns[0].Width = 60;
            dgv_MainCamHealth.Columns[1].HeaderText = "状態";
            dgv_MainCamHealth.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv_MainCamHealth.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgv_SubCamHealth.ColumnCount = 2;
            dgv_SubCamHealth.Columns[0].HeaderText = "カメラID";
            dgv_SubCamHealth.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv_SubCamHealth.Columns[0].Width = 60;
            dgv_SubCamHealth.Columns[1].HeaderText = "状態";
            dgv_SubCamHealth.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv_SubCamHealth.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region ### Dispose ###
        public void Dispose()
        {
            if (m_ManagementCl != null) m_ManagementCl.Dispose();

            m_camClPana.Dispose();
            m_camClAvp.Dispose();

            if (m_tcl != null) m_tcl.Disconnect(true);
            if (m_tsrv != null) m_tsrv.EndListen(true);

            if (m_detectCl != null) m_detectCl.Dispose();

            CommTerminalDispose();

            if (m_facilityControlThreads != null) m_facilityControlThreads.ForEach(cThread => cThread.m_reset.Dispose());



            //if (m_PLCCheckStatus != null) m_PLCCheckStatus.ForEach(plc => plc.Dispose());
            //if (m_PLCWatchStatus != null) m_PLCWatchStatus.ForEach(plc => plc.Dispose());
            //m_msgs = null;
            //if (m_DIOControl != null) m_DIOControl.Dispose();
            //m_cUdpSrv.EndListen();
            //m_ctrlTcpController.Dispose();
            //m_tvecsTcpController.Dispose();
            //m_viewerTcpController.Dispose();
            //m_statusLogTimer.Dispose();
            //m_statusLogTimer = null;
            if (m_plcstatusLogTimer != null) m_plcstatusLogTimer.Dispose();
            m_plcstatusLogTimer = null;
            //m_FacilityStatusLogTimer.Dispose();
            //m_FacilityStatusLogTimer = null;
            //m_ctrlUnitStates.ForEach(ctrlUnitstate => ctrlUnitstate.Dispose());
            m_facilityControlThreads.ForEach(cThread => cThread.m_reset.Dispose());

            m_plcunit.ForEach(PLCUnitStatus => PLCUnitStatus.Dispose());
            if (m_PLCControl != null) m_PLCControl.Dispose();
            if (m_PLCControl != null) m_PLCControl.Dispose();
        }
        #endregion

        #region ### フォームロード / クローズイベント ###
        private void ControlMain_Load(object sender, EventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_TerminalStatus = Res.CtrlStatus.IDLE.Code;

            //その他エリア描画設定
            // OtherAreaDrawSet();
            m_drawform.Visible = true;
            m_drawform.OtherAreaDrawList(m_ConfigData, m_otherArea);
            m_drawform.Visible = false;
            Viewer.DrawSet();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private void ControlMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }
        #endregion

        #region ### 走行状態管理 ###
        private string TerminalHealthCheck()
        {
            //制御していないときのみ開始できるか確認する
            if (m_CarControlStatus == OperationConst.ControlStatusType.C_NOT_IN_CONTROL)
            {
                string status = Res.CtrlStatus.CTRL_AVAILABLE.Code;

                //画像接続確認
                if (!GetCamCommStatus(MainC)) { return Res.CtrlStatus.CAM_CONNECTION_ERR.Code; }
                if (!GetCamCommStatus(SubC)) { return Res.CtrlStatus.CAM_CONNECTION_ERR.Code; }

                //カメラ確認
                if (GetCamHeakthStatus(MainC) == 1) { return Res.CtrlStatus.CAM_ERR.Code; }
                if (GetCamHeakthStatus(SubC) == 1) { return Res.CtrlStatus.CAM_ERR.Code; }

                //車両接続確認
                if (TvecsCommClStatus || TvecsCommSrvStatus) { return Res.CtrlStatus.CAR_CONNECTION_ERR.Code; }

                //走行停止中か
                if (m_EmergencyStop) { return Res.CtrlStatus.STOPPING.Code; }

                return status;
            }
            //制御中
            if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
            {
                uint status = GetPauseStatus();

                if (status == OperationConst.AccelerationStatus.C_ACCELERATION_PAUSE)
                {
                    if (m_CarInfo.speed == 0)
                    {
                        //StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_PAUSE);
                        return Res.CtrlStatus.PAUSED.Code;
                    }
                    else
                    {
                        //StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_PAUSE);
                        return Res.CtrlStatus.PAUSING.Code;
                    }
                }
                else if(status == OperationConst.AccelerationStatus.C_ACCELERATION_EMERGENCY)
                {
                    //StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_ERROR);
                    return Res.CtrlStatus.STOPPING.Code;
                }
                else
                {
                    //StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_RUNNING);
                }
            }

            return m_TerminalStatus;
        }
        #endregion

        #region ### 自走前工程タイマー ###
        /// <summary>
        /// 生産開始情報読み取り
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool m_ProductionInstructionFlg =false;
        private void OnProductionInstructionsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            try
            {
                // タイマー止める
                m_ProductionInstructionsTimer.Stop();

                // 生産指示情報の取得条件を取得
                MControlPlc plc = m_controlPLC.SingleOrDefault(no => no.GroupNo == m_controlSetting.ProductionGroupNo);

                // 取得条件に従い値を取得
                bool? val = m_plcunit.Single(key => key.PLCInfo.SID == plc.PLCDeviceID).GetReadBit(plc.ReadAddr, plc.ReadBit);

                if (val == null)
                {
                    // 取得条件がない場合、異常とする
                    throw new Exception("生産情報不正 - PLC情報取得不可");
                }

                // すでに開始後は、在籍がOFFになるのを確認する
                if (m_ProductionInstructionFlg)
                {
                    // 値の検証
                    if (val != plc.Value && PreVehicle.Instance.Vehicle == null)
                    {
                        // 条件に当てはまらなくなった時
                        // 生産指示確認再開
                        m_ProductionInstructionFlg = false;
                        PreVehicle.Instance.Status = PreVehicleStatus.Preparation;
                    }
                    else
                    {
                        // 当てはまる場合、タイマー再開して終了
                        m_ProductionInstructionsTimer.Start();
                        return;
                    }
                }

                // 自走前工程に車両がいないときに確認を行う
                if (PreVehicle.Instance.Vehicle == null)
                {
                    // 値の検証
                    // 条件に当てはまる場合、自走前工程スタート
                    if (val == plc.Value)
                    {
                        LOGGER.Info("生産指示の入力を検知しました");
                        SelfPropelledInspection1();
                        m_ProductionInstructionFlg = true;
                    }
                    else
                    {
                        // 当てはまらない場合、タイマー再開
                        //m_ProductionInstructionsTimer.Start();
                        PreVehicle.Instance.Status = PreVehicleStatus.Preparation;
                    }
                }
                m_ProductionInstructionsTimer.Start();

            }
            catch (Exception ex) 
            {
                // ステータス変更
                PreVehicle.Instance.Status = PreVehicleStatus.ErrorEnd;
                // 必ず異常発砲する
                ExceptionProcess.UserExceptionProcess(ex);
                // ※ 削除する
                m_ProductionInstructionsTimer.Start();
            }
            finally 
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
           
        }

        /// <summary>
        /// 生産開始情報読み取り
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReadyBootTimerElapsed(object sender, ElapsedEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            try
            {
                // タイマー止める
                m_ReadyBootTimer.Stop();

                // 準備起動判定の取得条件を取得
                MControlPlc plc = m_controlPLC.SingleOrDefault(no => no.GroupNo == m_controlSetting.ReadyBootGroupNo);

                // 取得条件に従い値を取得
                bool? val = m_plcunit.Single(key => key.PLCInfo.SID == plc.PLCDeviceID).GetReadBit(plc.ReadAddr, plc.ReadBit);

                if (val == null)
                {
                    // 取得条件がない場合、異常とする
                    throw new Exception("");
                }

                // 値の検証
                // 条件に当てはまる場合、自走前工程スタート
                if (val == plc.Value)
                {
                    LOGGER.Info("準備起動の入力を検知しました");
                    SelfPropelledInspection2();
                }
                else
                // 当てはまらない場合、タイマー再開
                {
                    m_ReadyBootTimer.Start();
                }

            }
            catch (Exception ex)
            {
                // ステータス変更
                PreVehicle.Instance.Status = PreVehicleStatus.ErrorEnd;
                // 必ず異常発砲する
                ExceptionProcess.UserExceptionProcess(ex);
                //※ 削除する
                m_ProductionInstructionsTimer.Start();
                PreVehicle.Instance.Vehicle = null;
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }

        }

        private void OnReadyOnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            try
            {
                // タイマー止める
                m_ReadyOnTimer.Stop();

                // 起動判定の取得条件を取得
                MControlPlc plc = m_controlPLC.SingleOrDefault(no => no.GroupNo == m_controlSetting.ReadyOnGroupNo);

                // 取得条件に従い値を取得
                bool? val = m_plcunit.Single(key => key.PLCInfo.SID == plc.PLCDeviceID).GetReadBit(plc.ReadAddr, plc.ReadBit);

                if (val == null)
                {
                    // 取得条件がない場合、異常とする
                    throw new Exception("");
                }

                // 値の検証
                // 条件に当てはまる場合、制御開始処理スタート
                if (val == plc.Value && ControlVehicle.Instance.Vehicle == null)
                {
                    LOGGER.Info("起動の入力を検知しました");
                    ControlStartProc();
                }
                else
                // 当てはまらない場合、タイマー再開
                {
                    m_ReadyOnTimer.Start();
                }

            }
            catch (Exception ex)
            {
                // ステータス変更
                PreVehicle.Instance.Status = PreVehicleStatus.ErrorEnd;
                ExceptionProcess.UserExceptionProcess(ex);
                // ※ 削除する
                m_ProductionInstructionsTimer.Start();
                PreVehicle.Instance.Vehicle = null;
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }

        }

        #endregion

        #region ### 自走前検査 ###
        /// <summary>
        /// 自走前検査シーケンス
        /// </summary>
        public void SelfPropelledInspection1()
        {
            //※ 削除対象(DBやRFIDから取得)
            ControlStartClass controlStart = new ControlStartClass();
            CommonProc.ReadFile("xml/ControlStart.xml", ref controlStart);
            //※

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            // 音声表示に状態表示をシーケンスの間に差し込んでいく

            // ステータス変更
            PreVehicle.Instance.Status = PreVehicleStatus.Moving;

            // RFID読み込み
            GetRFIDProc();

            // 検査情報確認
            // フラグにて確認の有無
            //if (m_controlSetting[0].GetInspectionInfo)
            //{
            //    GetInspectionProc();
            //}

            // コース選択


            // IPアドレス取得
            //PreVehicle.Instance.Vehicle.IpAddr = m_ManagementCl.InquiryIPAddr();
            PreVehicle.Instance.Vehicle.IpAddr = controlStart.IPaddr;
            PreVehicle.Instance.Vehicle.Port = controlStart.PORT1.ToString();

            // 準備起動判定状態読み出しタイマー開始
            m_ReadyBootTimer.Start();

            // ステータス変更
            PreVehicle.Instance.Status = PreVehicleStatus.WaitReadyBoot;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        public void SelfPropelledInspection2()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            // ステータス変更
            PreVehicle.Instance.Status = PreVehicleStatus.Moving;

            // １、TVECSへ生産指示
            string result = CarProductionInstructions();
            if (result != Res.ErrorStatus.NORMAL.Code)
            {
                // 異常
            }

            // ヨーレート取得フラグ確認
            // ヨーレート取得

            // 起動判定状態読み出し
            m_ReadyOnTimer.Start();

            // ステータス変更
            PreVehicle.Instance.Status = PreVehicleStatus.WaitStartup;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 自走検査時に走行NGとなった場合の処理を記載
        /// </summary>
        public void SelfPropelledNGProc()
        {
            //PLCに不可であることを出力


        }

        public void GetRFIDProc()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //※ 削除対象(DBやRFIDから取得)
            ControlStartClass controlStart = new ControlStartClass();
            CommonProc.ReadFile("xml/ControlStart.xml", ref controlStart);
            //※

            if (!m_ManagementCl.CommStatus)
            {
                throw new Exception("MnagementPCに接続されていません");
            }

            // RFID情報取得
            bool result = m_ManagementCl.InquiryRFID();

            if (!result)
            {
                //// 要求結果がNGの場合QRコード読取
                throw new Exception("RFID要求に失敗しました");
            }


            PreVehicle.Instance.Vehicle = new VehicleInfo();
            VehicleInfo vehicle = PreVehicle.Instance.Vehicle;
            vehicle.BodyNo = m_ManagementCl.GetRFIDInfo.BodyNo;
            //※
            vehicle.CarSpecNo = controlStart.CarSpecNo;
            //※

            // 自走車種判定
            MCarSpec carSpec = m_carSpecs.SingleOrDefault(spec => spec.SpecCd == vehicle.CarSpecNo);
            if (!carSpec.ControlFlg)
            {
               // 自走不可車種
            }

            // 車両情報取得
            PreVehicle.Instance.Vehicle.CarSpec = new CarSpec
            {
                GearRate = (double)carSpec.GearRatio,
                Wheelbase = (double)carSpec.Wheelbase / 1000,
                RearOverhang = (double)carSpec.RearOverhang /1000,
                WidthDistance = (double)carSpec.CarWidth / 1000,
                //CenterFrontLength = controlStart.CenterFrontLength,
                CenterFrontLength = (double)carSpec.Wheelbase / 1000 + (double)carSpec.FrontOverhang / 1000,
                ForwardDistance = double.Parse(controlStart.ForwardDistance)
            };

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        public void GetInspectionProc()
        {
            // 検査情報取得
            // inspectininfo = m_ManagementCl.Inspection();
        }

        #endregion

        #region 制御開始処理
        ///// <summary>
        ///// 画面上のボタンからの実行時
        ///// </summary>
        //public void DummyControlStartProc()
        //{
        //    ControlStartClass controlStart = new ControlStartClass();
        //    CommonProc. ReadFile("xml/ControlStart.xml", ref controlStart);
        //    PreVehicle.Instance.Vehicle.BodyNo = controlStart.CarNo;

        //    // カメラ切替
        //    MainCamType = (CamType)m_CourseInfoList[0].Course.MainCamNo;
        //    SubCamType = (CamType)m_CourseInfoList[0].Course.SubCamNo;


        //    ExeCamPreControlStart(MainC, PreVehicle.Instance.Vehicle.BodyNo);
        //    if (GetCamCommStatus(SubC)) ExeCamPreControlStart(SubC, PreVehicle.Instance.Vehicle.BodyNo);

        //    ControlStartProc();
        //}

        private async　void ControlStartProc()
        {
            //※ 削除対象(DBやRFIDから取得)
            ControlStartClass controlStart = new ControlStartClass();
            CommonProc.ReadFile("xml/ControlStart.xml", ref controlStart);
            int GoalNo = controlStart.GoalNo;
            string AreaAdjust = controlStart.AreaAdjust;
            //※

            m_camList = new List<CamList>();
            CommonProc.ReadFile("xml/Camlist.xml", ref m_camList);
            //※

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                ControlVehicle.Instance.Vehicle = (VehicleInfo)PreVehicle.Instance.Vehicle.Clone();
                PreVehicle.Instance.Vehicle = null;
                PreVehicle.Instance.Status = PreVehicleStatus.WaitFrontVehicle;

                ControlVehicle.Instance.Status = ControlVehicle.ControlVehicleStatus.Preparation;

                m_CarNo = ControlVehicle.Instance.Vehicle.BodyNo;
                m_CarSpec = ControlVehicle.Instance.Vehicle.CarSpecNo;


                //ハザード確認
                if (m_controlSetting.GetHazard)
                {
                    HazardConfirn hazard = new HazardConfirn(m_controlSetting.PanaWritePath, m_controlSetting.TMCWritePath);
                    hazard.Start(ControlVehicle.Instance.Vehicle.BodyNo, ControlVehicle.Instance.Vehicle.CarSpecNo, C_START_CAM_IP, C_START_CAM_ID);
                    if (hazard.Result)
                    {
                        LOGGER.Info("ハザード確認処理にて制御対象車両を確認しました");
                    }
                    else
                    {
                        //異常のため異常終了
                        EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name},Hazard", hazard.ErrCode);
                        return;
                    }
                    hazard = null;
                }


                // ２、メイン画像測位開始前処理
                var cam1task = Task<bool>.Run(() => {
                    //※ 前車両が終わるまで待機
                    //   前車両が異常終了した場合リセット

                    return ExeCamPreControlStart(MainC, ControlVehicle.Instance.Vehicle.BodyNo);
                });

                // ３、サブ画像測位開始前処理
                var cam2task = Task<bool>.Run(() => {
                    //※ 前車両が終わるまで待機
                    //   前車両が異常終了した場合リセット

                    return ExeCamPreControlStart(SubC, ControlVehicle.Instance.Vehicle.BodyNo);
                });
                // ２、３処理終了待機
                await Task.WhenAll(cam1task, cam2task);
                if (!cam1task.Result || !cam2task.Result)
                {
                    // どこかの開始準備処理において異常が発生
                }

                // ※開始前ステータスチェック ※異常があったら連続OFFになるので確認必要ない？
                string code = StartStatusCheck();
                if (code != Res.ErrorStatus.NORMAL.Code)
                {
                    if (code == Res.ErrorStatus.B_ERR.Code)
                    {
                        ControlStartStatus(code, GetCamRcvInfo(MainC).CamID);
                    }
                    else
                    {
                        ControlStartStatus(code);
                    }
                    return;
                }

                //値の初期化
                ResetVal();

                //ゴール番号更新
                m_ConfigData.GoalNo = GoalNo;
                foreach (OtherAreaRetention otherArea in m_otherArea)
                {
                    if (otherArea.otherarea.AreaCode == OtherAreaCode.GoalArea && m_ConfigData.GoalNo == otherArea.otherarea.Number)
                    {
                        m_controlCenterData.goalarea = otherArea.otherarea.area[OperationConst.FacilityAreaStage.C_FACILITY_START];
                    }
                }

                double GearRate = ControlVehicle.Instance.Vehicle.CarSpec.GearRate;
                double Wheelbase = ControlVehicle.Instance.Vehicle.CarSpec.Wheelbase;
                double WidthDistance = ControlVehicle.Instance.Vehicle.CarSpec.WidthDistance;
                double ForwardDistance = ControlVehicle.Instance.Vehicle.CarSpec.ForwardDistance;
                double CenterFrontLength = ControlVehicle.Instance.Vehicle.CarSpec.CenterFrontLength;
                double RearOverhang = ControlVehicle.Instance.Vehicle.CarSpec.RearOverhang;
                //ギア比更新
                if (GearRate != 0)
                {
                    m_ConfigData.GearRate = GearRate;
                }
                //ホイールベース更新
                if (Wheelbase != 0)
                {
                    m_ConfigData.Wheelbase = Wheelbase;
                }
                //車両全幅
                if (WidthDistance != 0)
                {
                    m_ConfigData.WidthDistance = WidthDistance;
                }
                //前方注視距離
                if (ForwardDistance != 0)
                {
                    m_ConfigData.ForwardDistance = ForwardDistance;
                }
                //車両仕様反映
                m_carSpec = new AtrptShare.CarSpec()
                {
                    GearRate = GearRate,
                    Wheelbase = Wheelbase,
                    ForwardDistance =　ForwardDistance,
                    WidthDistance = WidthDistance,
                    CenterFrontLength = CenterFrontLength,
                    RearOverhang = RearOverhang
                };

                //走行エリア調整 
                //foreach (CourseInfo course in m_CourseInfoList)
                //{
                //    SpeedAreaReset(course);
                //}
                SpeedAreaAdjust(double.Parse(AreaAdjust));

                m_TerminalStatus = Res.CtrlStatus.ON_CTRL.Code;

                //制御開始時時刻取得
                m_now = DateTime.Now;

                //制御指示送信後タイマー開始
                m_timer_ci = new System.Threading.Timer(new TimerCallback(Callback_CarStatus), null, Timeout.Infinite, 0);
                m_ci_interval = OperationConst.C_CI_FIRST_INTERVAL;
                m_timer_ci.Change(0, m_ci_interval);

                //画像解析測位開始            
                if (ExeCamControlStart(MainC, m_CarNo, m_now) == false) { return; };
                //　☆
                if (GetCamCommStatus(SubC)) if (ExeCamControlStart(SubC, m_CarNo, m_now) == false) { return; };

                //車両制御開始
                CarControlStart();

                //計算部起動
                SettingInfo settingInfo = new SettingInfo();
                PanaSettingData panaSettingData = SetPanaSettingData();
                settingInfo.vehicle_type_id =CommonProc.stringToChar(m_CarSpec);
                panaSettingData.carSpec = m_carSpec;
                LOGGER.Debug($"before[settingInfo.vehicle_type_id:{CommonProc.charToString(settingInfo.vehicle_type_id)}");
                LOGGER.Debug($"before[ConnectVal.mClConnectIP:{m_ConfigData.ConnectVal.mClConnectIP.ToString()},CalcCoefficient.Kp.ToString:{m_ConfigData.CalcCoefficient.Kp.ToString()},ForwardDistance:{m_ConfigData.ForwardDistance.ToString()},WidthDistance:{m_ConfigData.WidthDistance.ToString()},FFGetStartPosition:{m_ConfigData.FFGetStartPosition.ToString()},LimitVal.AngleLimit:{m_ConfigData.LimitVal.AngleLimit.ToString()},GlobalCoordinate.maxCoordinate:{m_ConfigData.GlobalCoordinate.maxCoordinate.ToString()},Acceleration.DeceAmount:{m_ConfigData.Acceleration.DeceAmount.ToString()},GetCondition.GetRadian:{m_ConfigData.GetCondition.GetRadian.ToString()},OtherAreas数:{m_ConfigData.OtherAreas.Count.ToString()},GoalNo:{m_ConfigData.GoalNo.ToString()},GearRate:{m_ConfigData.GearRate.ToString()},Wheelbase:{m_ConfigData.Wheelbase.ToString()},trafficLights:{m_ConfigData.trafficLights.Any().ToString()},シャッター:{m_ConfigData.shutters.Any().ToString()},facilitys:{m_ConfigData.facilitys.Any().ToString()},camscore:{m_ConfigData.camscore},fixcnt:{m_ConfigData.fixcnt},nofixcam:{m_ConfigData.nofixcam},");
                LOGGER.Debug($"before[actual_angle_wait:{panaSettingData.constData.actual_angle_wait},use_start_yawrate:{panaSettingData.constData.use_start_yawrate},start_yawrate:{panaSettingData.constData.start_yawrate},first_pos_x:{panaSettingData.constData.first_pos_x},first_pos_y:{panaSettingData.constData.first_pos_y},first_diff_x:{panaSettingData.constData.first_diff_x},first_diff_y:{panaSettingData.constData.first_diff_y},first_errmax_cnt:{panaSettingData.constData.first_errmax_cnt},score_all_use:{panaSettingData.constData.score_all_use},angle_limit_mode:{panaSettingData.constData.angle_limit_mode},acceleration_mode:{panaSettingData.constData.acceleration_mode},include_judge_area_count:{panaSettingData.constData.include_judge_area_count},first_vector:{panaSettingData.first_vector}");

                try
                {
                    m_calc = new clsCalculator(settingInfo, (ConfigData)m_ConfigData.Clone(), (PanaSettingData)panaSettingData.Clone());
                }
                catch (Exception e)
                {
                    LOGGER.Error(e.Message);
                    ControlStartStatus(Res.ErrorStatus.R_ERR.Code);
                    return;
                }

                //加速度舵角通知タイマー開始
                m_timer_cc = new System.Threading.Timer(new TimerCallback(Callback_Kasoku), null, Timeout.Infinite, 0);
                m_timer_cc.Change(0, C_CC_CALC_WAITTIME);


                ControlVehicle.Instance.Status = ControlVehicle.ControlVehicleStatus.Moving;
            }
            catch (UserException ex)
            {
                LOGGER.Error(ex.Message);

            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message);
                ControlStartStatus(Res.ErrorStatus.N_ERR.Code);
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        /// <summary>
        /// 開始前ステータスチェック
        /// </summary>
        /// <returns></returns>
        private string StartStatusCheck()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            string msg = "";
            //制御開始重複
            if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
            {
                msg = $"車両制御中です";
                LOGGER.Info(msg);
                return Res.ErrorStatus.F_ERR.Code;
            }

            //画像接続状況
            if (GetCamCommStatus(MainC) != true)
            {
                msg = $"画像解析装置との接続が不正です";
                LOGGER.Info(msg);
                return Res.ErrorStatus.CAM_CONN_ERR.Code;
            }

            //カメラ状態確認
            if (GetCamHeakthStatus(MainC) == OperationConst.C_ABNORMAL)
            {
                msg = $"カメラ状態が不正です。";
                LOGGER.Info(msg);
                return Res.ErrorStatus.B_ERR.Code;
            }

            //接続状態の確認
            if (TvecsCommClStatus != true || TvecsCommSrvStatus != true)
            {
                msg = $"TVECSとの接続が不正です。";
                LOGGER.Info(msg);
                return Res.ErrorStatus.C_ERR.Code;
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return Res.ErrorStatus.NORMAL.Code;
        }

        private PanaSettingData SetPanaSettingData()
        {
            // ※※ calcの引数が新しくなる際に変更を入れる

            PanaSettingData panaSettingData = new PanaSettingData();

            panaSettingData.constData = new ConstData();
            panaSettingData.constData.actual_angle_wait = int.Parse(ConfigurationManager.AppSettings["C_ACTUAL_ANGLE"]);
            panaSettingData.constData.use_start_yawrate = false;
            panaSettingData.constData.start_yawrate = int.Parse(ConfigurationManager.AppSettings["C_ACCELERATION_MODE"]);
            panaSettingData.constData.first_pos_x = double.Parse(ConfigurationManager.AppSettings["C_FIRST_POS_X"]);
            panaSettingData.constData.first_pos_y = double.Parse(ConfigurationManager.AppSettings["C_FIRST_POS_Y"]);
            panaSettingData.constData.first_diff_x = double.Parse(ConfigurationManager.AppSettings["C_DIFF_X"]);
            panaSettingData.constData.first_diff_y = double.Parse(ConfigurationManager.AppSettings["C_DIFF_Y"]);
            panaSettingData.constData.first_errmax_cnt = int.Parse(ConfigurationManager.AppSettings["C_ERRMAX_COUNT"]);
            panaSettingData.constData.score_all_use = bool.Parse(ConfigurationManager.AppSettings["C_SCORE_ALLUSE"]); ;
            panaSettingData.constData.angle_limit_mode = int.Parse(ConfigurationManager.AppSettings["C_ANGLE_LIMIT_MODE"]); ;
            panaSettingData.constData.acceleration_mode = int.Parse(ConfigurationManager.AppSettings["C_ACCELERATION_MODE"]); ;
            panaSettingData.constData.include_judge_area_count = C_INCLUDE_JUDGE_AREA_COUNT;
            panaSettingData.constData.rcdlogger_use = bool.Parse(ConfigurationManager.AppSettings["C_RCDLOGGER_USE"]); ;
            panaSettingData.constData.rcdlogger_path = ConfigurationManager.AppSettings["C_RCDLOGGER_PATH"]; ;

            //panaSettingData.first_vector = CommonProc.ToRadian(m_CarInfo.Vector);
            panaSettingData.first_vector = CommonProc.ToRadian(GetCamRcvInfo(MainC).Vector);

            return panaSettingData;
        }
        #endregion

        #region ### 制御開始時初期化 ###
        public void ResetVal()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_CarInfo.xPosition = -1;
            m_CarInfo.yPosition = -1;

            m_error_flg = 0;
            m_EmergencyStop = false;
            m_GoalCheckSnd = false;
            m_goalcomp_flg = 0;
            m_calcstarted = OperationConst.C_NOT_RECEIVED;

            m_contain_idx = 0;
            m_CarCornerinfo.Clear();

            m_goal_flg = 0;
            m_newPPoint = PointF.Empty;

            //その他エリア完了フラグ初期化
            for (int i = 0; i <= m_ConfigData.OtherAreas.Count() - 1; i++)
            {
                m_otherArea[i].otherarea = m_ConfigData.OtherAreas[i];
                m_otherArea[i].status = 0;
                m_otherArea[i].Done = 0;
            }

            //初期化呼び出し
            TvecsResetVal();
            ExeCamResetVal(MainC);
            DrowInitialize();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region 制御開始応答
        private void EventChatchControlStartStatus(EmergencyEndProcessEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            ControlStartStatus(e.errcd, e.arg1);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        /// <summary>
        /// 制御正常開始応答
        /// </summary>
        private void ControlStartStatus(string status, string camid = "0000")
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (status != Res.ErrorStatus.NORMAL.Code && status != Res.ErrorStatus.F_ERR.Code)
            {
                // TVECSに制御終了指示を送信する
                if (m_carstarted == OperationConst.C_RECEIVED) { CarControlEnd(); }
                //測位終了通知を送信する
                if (GetCamStartRes(MainC) == OperationConst.C_RECEIVED) { ExeCamEnd(MainC); }
                //☆
                if (GetCamCommStatus(SubC)) if (GetCamStartRes(SubC) == OperationConst.C_RECEIVED) { ExeCamEnd(SubC); }

                m_CarControlStatus = OperationConst.ControlStatusType.C_NOT_IN_CONTROL;

                // タイマーを止める
                TimerEnd();
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region 通常制御終了
        /// <summary>
        /// 制御正常終了時の処理を行う
        /// </summary>
        private void EndProcess()
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                LOGGER.Info("制御正常終了処理を開始します。");
                try
                {
                    // TVECSに制御終了指示を送信する
                    CarControlEnd();

                    //測位終了通知を送信する
                    ExeCamEnd(MainC);
                    //☆
                    if (GetCamCommStatus(SubC)) ExeCamEnd(SubC);

                    // タイマーを止める
                    TimerEnd();

                    //m_CarControlStatus = OperationConst.ControlStatusType.C_NOT_IN_CONTROL;
                    m_CarControlStatus = OperationConst.ControlStatusType.C_WAIT_CONTROL_END;
                    ControlVehicle.Instance.Status = ControlVehicle.ControlVehicleStatus.CompleteEnd;

                }
                catch
                {
                    EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}", Res.ErrorStatus.O_ERR.Code);
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
        #endregion

        #region 走行停止

        public void EventChatchEmergency(EmergencyEndProcessEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL && m_error_flg == OperationConst.C_NORMAL)
            {
                EmergencyEndProcess(e.methodname, e.errcd, e.errtype, e.arg1, e.arg2);
            }
            else
            {
                LOGGER.Info($"制御外のため走行停止終了処理イベントをスキップします。{e.methodname}");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 異常終了時の終了処理を行う(全てに通知)
        /// </summary>
        private void EmergencyEndProcess(string methodname, string errcd, string Pt = null, string arg1 = null, string arg2 = null)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                LOGGER.Info($"走行停止終了処理を開始します。[{methodname}]");
                if (m_error_flg == OperationConst.C_NORMAL)
                {
                    m_error_flg = OperationConst.C_ABNORMAL;
                    ChangeErrorMode(ErrorStatusGetLevel(errcd));

                    // TVECSへ走行停止通知を送信する
                    if (TvecsCommClStatus == true) { Task.Run(() => EmergencyStop()); }

                    // 測位終了通知を送信する
                    if (GetCamCommStatus(MainC) == true) { Task.Run(() => ExeCamEnd(MainC)); }
                    // ☆
                    if (GetCamCommStatus(SubC) == true) { Task.Run(() => ExeCamEnd(SubC)); }

                    ErrorEnd(errcd);
                }
                else
                {
                    LOGGER.Info("走行停止終了処理開始済み。");
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

        #endregion

        #region 車両からの走行停止
        /// <summary>
        /// 車両からの異常通知
        /// </summary>
        private void CarEmergencyEnd(string errcd)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                LOGGER.Info("走行停止終了処理を開始します。");
                if (m_error_flg == OperationConst.C_NORMAL)
                {
                    m_error_flg = OperationConst.C_ABNORMAL;
                    ChangeErrorMode(ErrorStatusGetLevel(errcd));

                    //測位終了通知を送信する
                    if (GetCamCommStatus(MainC) == true) { Task.Run(() => ExeCamEnd(MainC)); }
                    //☆
                    if (GetCamCommStatus(SubC) == true) { Task.Run(() => ExeCamEnd(SubC)); }

                    ErrorEnd(errcd);
                }
                else
                {
                    LOGGER.Info("走行停止終了処理開始済み。");
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


        private void ErrorEnd(string errcd)
        {
            // タイマーを止める
            TimerEnd();

            // 車両ステータス更新
            ControlVehicle.Instance.ErrorMsg = ErrorStatusGetMsg(errcd);
            ControlVehicle.Instance.Status = ControlVehicle.ControlVehicleStatus.ErrorEnd;

            //ステータス表示パネル表示
            m_EmergencyStop = true;
            m_CarControlStatus = OperationConst.ControlStatusType.C_NOT_IN_CONTROL;

            // 設備の制御
            ErrorFacilityProc();

            //待機中車両 強制完了
        }
        #endregion

        #region ### 自走終了時動作 ###
        private void ControlEndProc()
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                // 自走TVECS 自走シーケンス終了
                if (m_CarControlStatus == OperationConst.ControlStatusType.C_WAIT_CONTROL_END)
                {
                    WaitControlEndResetStatus();
                }
                m_cARendrcv = true;


                // ログに記録して、制御車両情報クリア
                if (ControlVehicle.Instance.Status == ControlVehicle.ControlVehicleStatus.CompleteEnd)
                {
                    // 正常完了


                    // 設備原位置
                    InitFacilityProc();

                    // 車両情報削除
                    ControlVehicle.Instance.Vehicle = null;
                }
                else if (ControlVehicle.Instance.Status == ControlVehicle.ControlVehicleStatus.ErrorEnd)
                {
                    // 異常完了
                    // 走行停止状態へ移行 異常通知

                }
                else
                {
                    // 想定外
                    LOGGER.Warn("想定外のシーケンス");
                }

            }
            catch (Exception e)
            {
                ExceptionProcess.ComnExceptionProcess(e);
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        #endregion

        #region ### 送受信ログ出力 ###
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

        #region ### 制御エリア判定用クラス ###
        public class OtherAreaRetention
        {
            /// <summary>
            /// その他エリア
            /// </summary>
            public OtherArea otherarea { get; set; }
            /// <summary>
            /// 次処理インデックス
            /// </summary>
            public int Done { get; set; }
            /// <summary>
            /// 設備ステータス 0:動作前 1:動作後
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// 一時停止エリア侵入
            /// </summary>
            public bool stop { get; set; }
        }
        #endregion

        #region ### 一時停止状態取得 ###
        /// <summary>
        /// 一時停止の条件を満たしているかの状態を返す
        /// センサ等による停止
        /// メイン測位受信遅延
        /// サブ測位受信遅延
        /// TVECS通信受信遅延
        /// ゴール満
        /// 侵入検知
        /// </summary>
        /// <returns></returns>
        private uint GetPauseStatus()
        {

            //状況に応じて加速度を変更する
            if (m_pause_flg == OperationConst.C_ABNORMAL || m_pause_goal_flg == OperationConst.C_ABNORMAL || m_pause_detect_flg == OperationConst.C_ABNORMAL)
            {
                LOGGER.Info($"[一時停止]{m_pause_flg}{m_pause_goal_flg}");
                return OperationConst.AccelerationStatus.C_ACCELERATION_PAUSE;
            }
            if (GetPauseStatus(MainC) == OperationConst.C_ABNORMAL || GetPauseStatus(SubC) == OperationConst.C_ABNORMAL || m_pause_commT_flg == OperationConst.C_ABNORMAL)
            {
                LOGGER.Info($"[一時停止通信途絶]{GetPauseStatus(MainC)}{GetPauseStatus(SubC)}{m_pause_commT_flg}");
                return OperationConst.AccelerationStatus.C_ACCELERATION_PAUSE_DISCONNECTION;
            }
            //if (m_goal_flg == 1)
            //{
            //    LOGGER.Info($"[ゴール範囲]{m_goal_flg}");
            //    return OperationConst.AccelerationStatus.C_ACCELERATION_GOAL;
            //}
            if (m_error_flg == 1 || GetCamHeakthStatus(MainC) == 1 || GetCamHeakthStatus(SubC) == 1)
            {
                LOGGER.Info($"[異常]");
                return OperationConst.AccelerationStatus.C_ACCELERATION_EMERGENCY;
            }

            return OperationConst.AccelerationStatus.C_ACCELERATION_NOMAL;
        }
        #endregion

        #region PLC設備ステータス記録
        /// <summary>
        /// ステータス記録タイマー処理(PLC直)
        /// </summary>
        private void OnPlcStatusLogTimerElapsed(object sender, ElapsedEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            LOGGER.Debug($"設備ステータス更新");
            try
            {
                m_plcstatusLogTimer.Stop();

                Dictionary<MPlcDevice, List<byte[]>> inputs = new Dictionary<MPlcDevice, List<byte[]>>();
                Dictionary<MPlcDevice, List<byte[]>> outputs = new Dictionary<MPlcDevice, List<byte[]>>();
                //接点読取
                foreach (PLCUnitStatus plc in m_plcunit)
                {
                    if (plc.Connected == PLCUnitStatus.ConnectStatus.Connected)
                    {
                        if (plc.ReadContactStatus.Count > 0) { inputs[plc.PLCInfo] = plc.ReadContactStatus; }
                        if (plc.ReadContactStatus.Count > 0) { outputs[plc.PLCInfo] = plc.WriteContactStatus; }
                    }
                }

                //PLC確認ステータス
                //Task.Run(() =>
                //{
                //    UpdatePLCWatchStatus(inputs);
                //    CheckPLCAll();
                //});

                //設備ステータス
                List<FacilityStatus> facStatusList = m_PLCControl.GetAllFacilityStatus(inputs, outputs);
                m_dicFacilityStatus = facStatusList.ToDictionary(fac => fac.FacilityID);

                m_FacilityStatus = facStatusList;
                

                //設備の状態から、処理を判定する
                FacilityStatusCheck(facStatusList);

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally
            {
                if (m_plcstatusLogTimer != null) m_plcstatusLogTimer.Start();
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        #region

        private void FacilityStatusCheck(List<FacilityStatus> facStatusList)
        {
            // 走行停止設備確認
            Task.Run(() =>
            {
                CheckStopBtnError(facStatusList);
            });

            //// エリアセンサ確認
            //Task.Run(() =>
            //{
            //    CheckAreaSensorPause(facStatusList);
            //});

            //// エリアセンサ解除確認
            //Task.Run(() =>
            //{
            //    CheckAreaSensorPauseCancel(facStatusList);
            //});

            //// ゴールエリア更新
            //Task.Run(() =>
            //{
            //    UpdateGoalAreaNo(facStatusList);
            //});

            //// 各個連続表示更新
            //Task.Run(() =>
            //{
            //    UpdateManagerModeFacility(facStatusList);
            //});

            //// ステータス取得不可確認
            //Task.Run(() =>
            //{
            //    CheckDIOFacilityError(facStatusList);
            //});

        }

        #endregion

        //#region 設備走行停止

        /// <summary>
        /// 設備ステータスリストより、走行停止装置の停止ステータスを確認
        /// 停止装置がONであり、関連異常が発生されていない場合、異常処理を行う
        /// </summary>
        /// <param name="facStatusList">設備ステータスリスト</param>
        private void CheckStopBtnError(List<FacilityStatus> facStatusList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                // ステータス = "ON"の走行停止装置取得
                List<FacilityStatus> erroredstopBtnList = facStatusList
                    .Where(fac => fac.FacilityTypeID == (int)FacilityTypes.StopBtn)
                    .Where(stopBtn => stopBtn.Status1 == 1)
                    .ToList();

                foreach (FacilityStatus erroredBtn in erroredstopBtnList)
                {
                    //GetGroupIDListFromMemberID(GROUP_MEMBER.IO_FACILITY, erroredBtn.FacilityID)
                    //    .ForEach(groupID =>
                    //    {
                    //        bool isAlreadyErrored = m_dicErrorInfos.ContainsKey(groupID);

                    //        if (isAlreadyErrored)
                    //        {
                    //            // グループが既に異常の場合スキップ
                    //            return;
                    //        }

                    //        string errTarCtrlUnitID = GetCtrlIDFromGroupID(groupID);

                    //        if (errTarCtrlUnitID == null)
                    //        {
                    //            // 対象制御端末を取得できない場合スキップ
                    //            return;
                    //        }

                    //        CtrlUnitState unit = m_ctrlUnitStates
                    //            .SingleOrDefault(ctrl => ctrl.CtrlUnit.ID == errTarCtrlUnitID);

                    //        if (unit == null || !unit.IsRunning())
                    //        {
                    //            // 対象制御端末が制御中でない場合、スキップ
                    //            return;
                    //        }

                            LOGGER.Info($"[走行停止] 走行停止装置ステータスがONのため、走行停止処理を行います。");
                    //        LOGGER.Info($"    設備ID: {erroredBtn.FacilityID}, 設備名称: {erroredBtn.FacilityName}, 対象グループID: {groupID}");

                    //        string explicitMsg = ErrorStatusGetMsg(Res.ErrorStatus.FACILITY_ERR_DETECT.Code) + $"({erroredBtn.FacilityID}, {erroredBtn.FacilityName})";
                    //        ErrorPreProcess(unit, Res.ErrorStatus.FACILITY_ERR_DETECT.Code, ErroredFrom.Fac, explicitMsg);
                    //    });

                    EmergencyEndProcessEventArgs e = new EmergencyEndProcessEventArgs()
                    {
                        methodname = $"{MethodBase.GetCurrentMethod().Name}",
                        errcd = Res.ErrorStatus.FACILITY_ERR_DETECT.Code
                    };

                    EventChatchEmergency(e);
                }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        ///// <summary>
        ///// 設備ステータスリストより、エリアセンサの一時停止ステータスを確認
        ///// エリアセンサがONであり、対象制御グループが一時停止でない場合、一時停止処理を行う
        ///// </summary>
        ///// <param name="facStatusList">設備ステータスリスト</param>
        //private void CheckAreaSensorPause(List<FacilityStatus> facStatusList)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        foreach (Group group in m_groups)
        //        {
        //            List<string> facilities = GetErrorSetFacilityIDs(group.ID);
        //            List<FacilityStatus> detectedAreaSensors = facStatusList
        //                                    .Where(f => facilities.Contains(f.FacilityID))
        //                                    .Where(f => f.FacilityTypeID == (int)FacilityTypes.AreaSensor)
        //                                    .Where(areaSensor => areaSensor.Status1 == 1)
        //                                    .ToList();

        //            if (detectedAreaSensors.Count > 0)
        //            {
        //                string targetCtrl = GetCtrlIDFromGroupID(group.ID);

        //                if (targetCtrl == null)
        //                {
        //                    // 対象制御グループ内に経路生成部がない場合、スキップ
        //                    continue;
        //                }

        //                CtrlUnitState unitStatus = m_ctrlUnitStates.SingleOrDefault(unit => unit.CtrlUnit.ID == targetCtrl);

        //                if (unitStatus == null || !unitStatus.IsRunning() || !unitStatus.IsOnCtrl())
        //                {
        //                    // 対象経路生成部が取得できない、または、制御中でない場合、スキップ
        //                    continue;
        //                }

        //                if (unitStatus.Status.In(Res.CtrlStatus.PAUSED, Res.CtrlStatus.PAUSING))
        //                {
        //                    // 対象経路生成部が既に一時停止中・一時停止である場合、スキップ
        //                    continue;
        //                }

        //                string sensorList = detectedAreaSensors
        //                    .Select(sensor => $"{sensor.FacilityID} {sensor.FacilityName}")
        //                    .Aggregate((curr, next) => curr + ", " + next);

        //                LOGGER.Warn($"[一時停止] エリアセンサ検出: {sensorList}");

        //                CtrlPauseSndMsg pauseMsg = new CtrlPauseSndMsg();

        //                m_ctrlTcpController.SendMessage(unitStatus.ConnectionID, pauseMsg);

        //                if (m_dicCarStatus.Keys.Contains(unitStatus.CarNo))
        //                {
        //                    CarStatus carStat = m_dicCarStatus[unitStatus.CarNo];

        //                    lock (carStat)
        //                    {
        //                        carStat.Pause();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        ///// <summary>
        ///// 設備ステータスリストより、エリアセンサの一時停止ステータスを確認
        ///// エリアセンサがOFFであり、対象制御グループが一時停止である場合、一時停止解除処理を行う
        ///// </summary>
        ///// <param name="facStatusList">設備ステータスリスト</param>
        //private void CheckAreaSensorPauseCancel(List<FacilityStatus> facStatusList)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        foreach (Group group in m_groups)
        //        {
        //            List<string> facilities = GetErrorSetFacilityIDs(group.ID);
        //            bool allSensorResolved = facStatusList
        //                                    .Where(f => facilities.Contains(f.FacilityID))
        //                                    .Where(f => f.FacilityTypeID == (int)FacilityTypes.AreaSensor)
        //                                    .All(areaSensor => areaSensor.Status1 == 0);

        //            if (allSensorResolved)
        //            {
        //                string targetCtrl = GetCtrlIDFromGroupID(group.ID);

        //                if (targetCtrl == null)
        //                {
        //                    // 対象制御グループ内に経路生成部がない場合、スキップ
        //                    continue;
        //                }

        //                CtrlUnitState unitStatus = m_ctrlUnitStates.SingleOrDefault(unit => unit.CtrlUnit.ID == targetCtrl);

        //                if (unitStatus == null)
        //                {
        //                    // 対象経路生成部が取得できない場合、スキップ
        //                    continue;
        //                }

        //                if (unitStatus.Status.In(Res.CtrlStatus.PAUSED, Res.CtrlStatus.PAUSING))
        //                {
        //                    CtrlPauseCancelSndMsg pauseCancelMsg = new CtrlPauseCancelSndMsg();

        //                    m_ctrlTcpController.SendMessage(unitStatus.ConnectionID, pauseCancelMsg);

        //                    if (m_dicCarStatus.Keys.Contains(unitStatus.CarNo))
        //                    {
        //                        CarStatus carStat = m_dicCarStatus[unitStatus.CarNo];

        //                        lock (carStat)
        //                        {
        //                            carStat.Resume();
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    continue;
        //                }
        //            }
        //        }
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        ///// <summary>
        ///// 設備ステータスリストより、全設備のステータスを確認
        ///// ステータスがエラーのものがある場合、走行停止タイマーを開始する
        ///// </summary>
        ///// <param name="facStatusList">設備ステータスリスト</param>
        //private void CheckDIOFacilityError(List<FacilityStatus> facStatusList)
        //{
        //    try
        //    {
        //        LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

        //        bool err = false;

        //        // ステータス = "エラー"の設備
        //        List<FacilityStatus> erroredfacilityList = facStatusList
        //            //.Where(fac => fac.FacilityTypeID == (int)FacilityTypes.StopBtn)
        //            .Where(stopBtn => stopBtn.Status1 == -1)
        //            .ToList();

        //        foreach (FacilityStatus erroredfacility in erroredfacilityList)
        //        {
        //            GetGroupIDListFromMemberID(GROUP_MEMBER.IO_FACILITY, erroredfacility.FacilityID)
        //                .ForEach(groupID =>
        //                {
        //                    bool isAlreadyErrored = m_dicErrorInfos.ContainsKey(groupID);

        //                    if (isAlreadyErrored)
        //                    {
        //                        // グループが既に異常の場合スキップ
        //                        return;
        //                    }

        //                    string errTarCtrlUnitID = GetCtrlIDFromGroupID(groupID);

        //                    if (errTarCtrlUnitID == null)
        //                    {
        //                        // 対象制御端末を取得できない場合スキップ
        //                        return;
        //                    }

        //                    CtrlUnitState unit = m_ctrlUnitStates
        //                        .SingleOrDefault(ctrl => ctrl.CtrlUnit.ID == errTarCtrlUnitID);

        //                    if (unit == null || !unit.IsRunning())
        //                    {
        //                        // 対象制御端末が制御中でない場合、スキップ
        //                        return;
        //                    }

        //                    err = true;
        //                    if (m_err_timer == null)
        //                    {
        //                        m_err_timer = new System.Threading.Timer(new TimerCallback(Callback_DIOfacility_Err), erroredfacilityList, m_FACILITY_STATUS_FAIL_TIMEOUT, 0);
        //                        LOGGER.Info($"[走行停止] 設備ステータス取得不可のため、タイマーを開始します。");
        //                        LOGGER.Info($"    設備ID: {erroredfacility.FacilityID}, 設備名称: {erroredfacility.FacilityName}, 対象グループID: {groupID}");
        //                    }
        //                });
        //        }

        //        if (m_err_timer != null && err == false)
        //        {
        //            m_err_timer.Dispose();
        //            m_err_timer = null;
        //            LOGGER.Info($"[走行停止] 設備ステータス取得したため、タイマーを停止します。");
        //        }
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        //#endregion

        //#region ## 残留検知処理 ##

        ///// <summary>
        ///// 設備ステータス情報からGoalNoを更新する。
        ///// GoalNoが変更されたとき、制御端末にゴールエリア変更通知を送信する。
        ///// </summary>
        ///// <param name="facilityStatus">すべての設備ステータスのリスト</param>
        //private void UpdateGoalAreaNo(List<FacilityStatus> facilityStatus)
        //{
        //    List<FacilityStatus> remainFacStatusList = new List<FacilityStatus>();
        //    List<string> remainDetectFacilityIDList = m_goalAreas.Select(area => area.FacilityID).ToList();

        //    foreach (string facId in remainDetectFacilityIDList)
        //    {
        //        FacilityStatus remainDetectFac = facilityStatus.Single(fac => fac.FacilityID == facId);
        //        remainFacStatusList.Add(remainDetectFac);
        //    }

        //    foreach (CtrlUnitState unit in m_ctrlUnitStates)
        //    {
        //        int newGoalNo = GetNewCtrlGoalArea(remainFacStatusList, unit.CtrlUnit.ID);
        //        unit.SetGoalNo(newGoalNo);
        //    }
        //}

        ///// <summary>
        ///// 制御端末のゴールエリアを取得する
        ///// </summary>
        ///// <param name="remainFacStatusList">残留検知の設備ステータスリスト</param>
        ///// <param name="unitId">確認中の制御端末</param>
        ///// <returns>制御端末のゴールエリアNo</returns>
        //private int GetNewCtrlGoalArea(List<FacilityStatus> remainFacStatusList, string unitId)
        //{
        //    // 制御端末ごとの残留検知設備ID
        //    List<string> facIds = m_goalAreas
        //        .Where(goal => goal.CtrlUnitID == unitId)
        //        .Select(goal => goal.FacilityID)
        //        .ToList();

        //    // 設定情報がない場合、デフォルトゴールエリアを返却
        //    if (facIds.Count <= 0)
        //    {
        //        return m_DEFAULT_GOAL_AREA;
        //    }

        //    // 残留検知された設備
        //    List<FacilityStatus> remainingFac = remainFacStatusList
        //        .Where(fac => facIds.Contains(fac.FacilityID))
        //        .Where(fac => fac.Status1 == 1)
        //        .ToList();

        //    // 検知された設備の設備ID
        //    List<string> remainingId = remainingFac
        //        .Select(fac => fac.FacilityID).ToList();

        //    if (remainingFac.Count == 0)
        //    {
        //        LOGGER.Debug($"[ゴールエリア取得] 残留なし制御端末 → 最小ゴールエリアNoに設定。{Res.C_CTRL_UNIT}ID: {unitId}");
        //        int minGoalNo = m_goalAreas
        //            .Where(goal => goal.CtrlUnitID == unitId)
        //            .Min(goal => goal.GoalNo) ?? m_DEFAULT_GOAL_AREA;
        //        return minGoalNo;
        //    }

        //    //最大ゴールNo( Status == 1 )取得
        //    int maxRemainGoalNo = m_goalAreas
        //        .Where(goal => remainingId.Contains(goal.FacilityID))
        //        .Where(goal => goal.CtrlUnitID == unitId)
        //        .Max(goal => goal.GoalNo) ?? m_DEFAULT_GOAL_AREA;

        //    //m_goalAreasの最後のGoalNo
        //    int lastGoalAreas = m_goalAreas
        //        .Where(goal => goal.CtrlUnitID == unitId)
        //        .Max(goal => goal.GoalNo) ?? m_DEFAULT_GOAL_AREA;

        //    if (maxRemainGoalNo == lastGoalAreas)
        //    {
        //        return maxRemainGoalNo + 1;
        //    }

        //    //最大残留検知GoalNoより大きい中の最小GoalNo
        //    int minAvailableGoalNo = m_goalAreas
        //        .Where(goal => goal.CtrlUnitID == unitId)
        //        .Where(goal => goal.GoalNo > maxRemainGoalNo)
        //        .Min(goal => goal.GoalNo) ?? m_DEFAULT_GOAL_AREA;

        //    return minAvailableGoalNo;
        //}

        //#endregion

        //#region ## 設備制御処理 ##

        ///// <summary>
        ///// 設備ステータスを要求指示ステータスに変更<para/>
        ///// 変更完了後、指示受信Tcpサーバーより完了電文を返信
        ///// </summary>
        ///// <param name="facilityID">対象設備ID</param>
        ///// <param name="reqStatus">要求ステータス</param>
        ///// <param name="rcvSrv">指示受信Tcpサーバー</param>
        ///// <param name="connID">接続ID</param>
        //internal void SetFacilityRequestedStatus(string facilityID, int reqStatus, CommSrv rcvSrv, int connID, string clientName)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        // 設備ID 存在チェック
        //        if (!m_dicFacilityStatus.Keys.Contains(facilityID))
        //        {
        //            LOGGER.Warn($"[設備制御不可] 存在しない設備IDが制御指示されました。[設備ID: {facilityID}]");
        //            return;
        //        }

        //        FacilityControl currentControl = new FacilityControl(rcvSrv, connID, facilityID, reqStatus, clientName);

        //        // 設備制御中チェック
        //        FacilityControl control = null;
        //        lock (m_facilityControlThreads)
        //        {
        //            control = m_facilityControlThreads
        //                .SingleOrDefault(c => c.FacilityID == facilityID && c.CtrlValue == reqStatus);
        //        }

        //        if (control != null)
        //        {
        //            if (control.m_reset != null)
        //            {
        //                lock (control.m_reset)
        //                {
        //                    control.m_reset.Set();
        //                }

        //            }
        //            LOGGER.Warn($"既 制御中の設備制御 受信: 設備ID: {facilityID}, 制御指定値: {reqStatus}");
        //        }


        //        // 設備制御実施
        //        LOGGER.Info($"設備制御開始　　: 設備ID: {facilityID}, 制御指定値: {reqStatus}");

        //        lock (m_facilityControlThreads)
        //        {
        //            m_facilityControlThreads.Add(currentControl);
        //        }

        //        int facilityTypeID = m_dicFacilityStatus[facilityID].FacilityTypeID;

        //        int result = ControlSetFacility(facilityID, facilityTypeID, reqStatus, true);

        //        bool ctrlDone = false;
        //        switch (result)
        //        {
        //            case DIO_CONTROL_RESULT.C_FACILITY_CTRL_ERROR:
        //                LOGGER.Error($"設備制御失敗　　: 設備ID: {facilityID}, 制御指定値: {reqStatus}");

        //                GetCtrlIDListFromFacilityID(facilityID)
        //                .ForEach((unitID) =>
        //                {
        //                    CtrlUnitState ctrlUnit = m_ctrlUnitStates.SingleOrDefault(unit => unit.CtrlUnit.ID == unitID);
        //                    if (ctrlUnit != null && ctrlUnit.IsRunning())
        //                    {
        //                        LOGGER.Info($"[走行停止] 設備制御失敗のため、対象経路生成部の走行停止処理を行います。{Res.C_CTRL_UNIT}ID:{unitID}");
        //                        ErrorPreProcess(ctrlUnit, Res.ErrorStatus.FACILITY_ERR.Code, ErroredFrom.Mng, GetFacilityCtrlFailMsg(facilityID, reqStatus));
        //                    }
        //                });
        //                break;
        //            case DIO_CONTROL_RESULT.C_FACILITY_CTRL_INTERRUPTED:
        //                // TODO: 他の設備制御指示発生
        //                LOGGER.Warn($"設備制御取消　　: 設備ID: {facilityID}, 制御指定値: {reqStatus}");
        //                break;
        //            case DIO_CONTROL_RESULT.C_FACILITY_CTRL_NORMAL_END:
        //                ctrlDone = CheckFacCtrlDone(currentControl, facilityID, reqStatus);
        //                break;
        //            default:
        //                break;
        //        }

        //        // 設備制御完了処理
        //        if (ctrlDone)
        //        {
        //            LOGGER.Info($"設備制御正常終了: 設備ID: {facilityID}, 制御指定値: {reqStatus}");
        //            List<FacilityControl> targetToSendDone = m_facControlWaits
        //                        .Where(wait => wait.FacilityID == facilityID && wait.CtrlValue == reqStatus)
        //                        .ToList();
        //            bool alreadyListed = targetToSendDone.SingleOrDefault(wait => wait.Equals(currentControl)) != null;
        //            if (!alreadyListed)
        //            {
        //                targetToSendDone.Add(currentControl);
        //            }

        //            foreach (FacilityControl toSend in targetToSendDone)
        //            {
        //                SendFacCtrlNormalEndMsg(toSend);
        //                m_facControlWaits.Remove(toSend);
        //            }
        //        }
        //        else
        //        {
        //            // 設備が指示値に変更できず、確認処理終了
        //            bool alreadyWaiting = m_facControlWaits.SingleOrDefault(wait => wait.Equals(currentControl)) != null;
        //            if (!alreadyWaiting)
        //            {
        //                m_facControlWaits.Add(currentControl);
        //            }
        //        }

        //        lock (currentControl.m_reset)
        //        {
        //            currentControl.m_reset.Dispose();
        //        }

        //        lock (m_facilityControlThreads)
        //        {
        //            m_facilityControlThreads.Remove(currentControl);
        //        }

        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        ///// <summary>
        ///// 設備IDで指定された設備を種別、指示値に合わせて制御する。
        ///// </summary>
        ///// <param name="facilityID">設備ID</param>
        ///// <param name="facilityTypeID">設備種別</param>
        ///// <param name="ctrlValue">指示値</param>
        ///// <param name="forceCtrl">強制制御フラグ</param>
        ///// <returns>制御結果</returns>
        ///// <see cref="DIO_CONTROL_RESULT"/>
        //private int ControlSetFacility(string facilityID, int facilityTypeID, int ctrlValue, bool forceCtrl)
        //{
        //    // 3灯信号機: 赤、2灯信号機: 赤 の場合、別処理
        //    if (facilityTypeID == 0 && ctrlValue == 1) // 3灯信号機 赤
        //    {
        //        // 黄色 → 赤
        //        List<FacCtrlStep> steps = new List<FacCtrlStep>() { FacCtrlStep.OneShot(2), FacCtrlStep.OneShot(1) };
        //        int interval = GetFaciliyIntervalConfig();
        //        if (m_FACILITY_CONTROL_MODE == (int)FACILITY_CONTROL_MODE.ContecDIO)
        //        {
        //            return m_DIOControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, interval);
        //        }
        //        else if (m_FACILITY_CONTROL_MODE == (int)FACILITY_CONTROL_MODE.PLC)
        //        {
        //            return m_PLCControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, interval);
        //        }
        //        else { return 2; }
        //    }
        //    else if (facilityTypeID == 1 && ctrlValue == 1) // 2灯信号機 赤
        //    {
        //        int interval = GetFaciliyIntervalConfig();
        //        int repeatInterval = GetFaciliyRepeatIntervalConfig();
        //        // 青点滅(青→消灯: 繰り返し) → 赤
        //        List<FacCtrlStep> steps = new List<FacCtrlStep>() { FacCtrlStep.Repeat(new int[] { 0, 3 }, repeatInterval), FacCtrlStep.OneShot(1) };
        //        if (m_FACILITY_CONTROL_MODE == (int)FACILITY_CONTROL_MODE.ContecDIO)
        //        {
        //            return m_DIOControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, interval);
        //        }
        //        else if (m_FACILITY_CONTROL_MODE == (int)FACILITY_CONTROL_MODE.PLC)
        //        {
        //            return m_PLCControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, interval);
        //        }
        //        else { return 2; }
        //    }
        //    else // 一般制御
        //    {
        //        List<FacCtrlStep> steps = new List<FacCtrlStep>() { FacCtrlStep.OneShot(ctrlValue) };
        //        if (m_FACILITY_CONTROL_MODE == (int)FACILITY_CONTROL_MODE.ContecDIO)
        //        {
        //            return m_DIOControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, 0);
        //        }
        //        else if (m_FACILITY_CONTROL_MODE == (int)FACILITY_CONTROL_MODE.PLC)
        //        {
        //            return m_PLCControl.FacCtrlStepSet(facilityID, facilityTypeID, steps, forceCtrl, 0);
        //        }
        //        else { return 2; }
        //    }
        //}

        ///// <summary>
        ///// 設備IDで指定された設備が指示値になるまで繰り返し確認する。
        ///// </summary>
        ///// <returns>設備が指示値に変更された場合True、
        ///// 指示値に変更される前に繰り返し確認がキャンセルされた場合False</returns>
        ///// </summary>
        //private bool CheckFacCtrlDone(FacilityControl targetControl, string facilityID, int ctrlValue)
        //{
        //    do
        //    {
        //        if (m_dicFacilityStatus[facilityID].Status1 == ctrlValue)
        //        {
        //            return true;
        //        }
        //    }
        //    while (!targetControl.m_reset.WaitOne(m_STATUS_UPDATE_INTERVAL));

        //    return false;
        //}

        ///// <summary>
        ///// 異常時、ステータスを変更する設備IDリストを取得
        ///// </summary>
        ///// <param name="ctrlUnitId">制御端末ID</param>
        ///// <returns>異常処理対象制御端末と同じグループに属している設備のIDリスト</returns>
        //internal List<string> GetErrorSetFacilityIDs(string groupID)
        //{
        //    // 制御端末と同じグループに属している設備のIDリストを取得
        //    List<string> errSetTargetFacilityies = m_groupMembers
        //        .Where(gMember => gMember.GroupID == groupID)
        //        .Where(gMember => gMember.MemberType == GROUP_MEMBER.IO_FACILITY.Code)
        //        .Select(gMember => gMember.MemberID)
        //        .ToList();

        //    return errSetTargetFacilityies;
        //}

        ///// <summary>
        ///// 対象設備の異常時設定ステータス値を取得
        ///// </summary>
        ///// <param name="facilityIDs">対象設備IDリスト</param>
        ///// <returns>対象設備情報と異常時設定ステータス値</returns>
        //private Dictionary<FacilityStatus, int> GetFacilityErrorSetValues(List<string> facilityIDs)
        //{
        //    Dictionary<FacilityStatus, int> facilityErrorSetValues = new Dictionary<FacilityStatus, int>();

        //    foreach (string facilityID in facilityIDs)
        //    {
        //        FacilityStatus fac = m_dicFacilityStatus[facilityID];
        //        if (fac == null)
        //        {
        //            LOGGER.Warn($"[異常時設定値 取得不可] 指定されたIO設備が存在しません。設備ID: {facilityID}");
        //            continue;
        //        }
        //        FacilityType facType = m_facilityTypes.SingleOrDefault(typeInfo => typeInfo.ID == fac.FacilityTypeID);
        //        if (facType == null)
        //        {
        //            LOGGER.Warn($"[異常時設定値 取得不可] 指定されたIO設備の種別を判定できません。設備ID: {facilityID}, 設備種別ID: {fac.FacilityTypeID}");
        //            continue;
        //        }
        //        int? ErrorStatus = facType.ErrorStatus;
        //        if (ErrorStatus != null)
        //        {
        //            facilityErrorSetValues.Add(fac, (int)ErrorStatus);
        //        }
        //    }
        //    return facilityErrorSetValues;
        //}

        ///// <summary>
        ///// 対象設備の異常解除時設定ステータス値を取得
        ///// </summary>
        ///// <param name="facilityIDs">対象設備IDリスト</param>
        ///// <returns>対象設備情報と異常解除時設定ステータス値</returns>
        //internal Dictionary<FacilityStatus, int> GetFacilityErrorResolveValues(List<string> facilityIDs)
        //{
        //    Dictionary<FacilityStatus, int> facilityErrorResolveValues = new Dictionary<FacilityStatus, int>();

        //    foreach (string facilityID in facilityIDs)
        //    {
        //        FacilityStatus fac = m_dicFacilityStatus[facilityID];
        //        if (fac == null)
        //        {
        //            LOGGER.Warn($"[異常解除] グループメンバーIO設備が存在しません。設備ID: {facilityID}");
        //            continue;
        //        }
        //        FacilityType facType = m_facilityTypes.SingleOrDefault(typeInfo => typeInfo.ID == fac.FacilityTypeID);
        //        int? resolveStatus = facType.ResolveStatus;
        //        if (resolveStatus != null)
        //        {
        //            facilityErrorResolveValues.Add(fac, (int)resolveStatus);
        //        }
        //    }
        //    return facilityErrorResolveValues;
        //}

        ///// <summary>
        ///// 並行設備制御
        ///// </summary>
        ///// <param name="controlList">制御指示リスト</param>
        ///// <returns>制御結果リスト(設備ID、制御処理結果)</returns>
        //internal Dictionary<string, int> ParallelFacilityControl(Dictionary<FacilityStatus, int> controlList)
        //{
        //    Dictionary<string, int> ctrlResults = new Dictionary<string, int>();

        //    for (int i = 0; i < controlList.Count; i++)
        //    {
        //        try
        //        {
        //            FacilityStatus facility = controlList.ElementAt(i).Key;
        //            int ctrlValue = controlList.ElementAt(i).Value;

        //            LOGGER.Info($"[設備ステータスを変更] 設備ID: {facility.FacilityID}, 設備名: {facility.FacilityName}, 設定値: {ctrlValue}");
        //            int ctrlRes = ControlSetFacility(facility.FacilityID, facility.FacilityTypeID, ctrlValue, true);
        //            ctrlResults.Add(facility.FacilityID, ctrlRes);
        //        }
        //        catch (Exception ex)
        //        {
        //            ExceptionProcess.ComnExceptionConsoleProcess(ex);
        //            FacilityStatus facility = controlList.ElementAt(i).Key;
        //            ctrlResults.Add(facility.FacilityID, DIO_CONTROL_RESULT.C_FACILITY_CTRL_ERROR);
        //        }
        //    }

        //    return ctrlResults;
        //}

        ///// <summary>
        ///// 設備制御失敗メッセージ作成
        ///// </summary>
        ///// <param name="FacilityID">対象設備ID</param>
        ///// <param name="FailedCtrlVal">制御失敗指示値</param>
        ///// <returns>設備制御失敗エラーメッセージ</returns>
        //internal string GetFacilityCtrlFailMsg(string FacilityID, int FailedCtrlVal)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        FacilityStatus erroredFacility = m_dicFacilityStatus.Values.SingleOrDefault(f => f.FacilityID == FacilityID);
        //        string facilityName = "";
        //        string ctrlValName = "";
        //        if (erroredFacility == null)
        //        {
        //            LOGGER.Warn($"[メッセージ作成失敗] 該当する設備情報が見つけれません。" +
        //                $"異常内容はID、指示値で表示されます。設備ID:{FacilityID}, 制御値:{FailedCtrlVal}");
        //            facilityName = $"設備ID:{FacilityID}";
        //            ctrlValName = $"制御値:{FailedCtrlVal}";
        //        }
        //        else
        //        {
        //            facilityName = erroredFacility.FacilityName;

        //            FacilityStatusMsg statMsg = m_facilityStatusMsgs
        //                    .Where(fStatus => fStatus.FacilityTypeID == erroredFacility.FacilityTypeID)
        //                    .Where(fStatus => fStatus.StatusCode == FailedCtrlVal)
        //                    .SingleOrDefault();
        //            ctrlValName = statMsg == null ? $"制御値:{FailedCtrlVal}" : statMsg.StatusMsg;
        //        }

        //        return string.Format(ErrorStatusGetMsg(Res.ErrorStatus.FACILITY_ERR.Code), facilityName, ctrlValName);
        //    }
        //    finally
        //    {
        //        LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
        //    }
        //}

        ///// <summary>
        ///// 設備制御指示の正常終了応答送信
        ///// </summary>
        ///// <param name="connID">指示受信接続ID</param>
        ///// <param name="facilityID">制御設備ID</param>
        ///// <param name="ctrlValue">制御指示値</param>
        //private void SendFacCtrlNormalEndMsg(FacilityControl facCtrlInfo)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        FacStatusResSndMsg facStatusResMsg = new FacStatusResSndMsg()
        //        {
        //            facID = facCtrlInfo.FacilityID
        //        };

        //        facCtrlInfo.ReceivedServer.SendMessage(facCtrlInfo.ConnectionID, facStatusResMsg);
        //        LOGGER.Info(GetMsgSndLog(facCtrlInfo.ClientName, facStatusResMsg));
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        ///// <summary>
        ///// 設備シーケンス制御時間間隔取得
        ///// </summary>
        //private int GetFaciliyIntervalConfig()
        //{
        //    string strInterval = ConfigurationManager.AppSettings["FACILITY_CTRL_INTERVAL"];
        //    int interval = 2000;
        //    bool isValidConfig = int.TryParse(strInterval, out interval);
        //    if (!isValidConfig)
        //    {
        //        throw new UserException($"設定値 FACILITY_CTRL_INTERVAL: {strInterval}を整数に変更できませんでした。");
        //    }
        //    return interval;
        //}

        ///// <summary>
        ///// 設備繰り返し制御時間間隔取得
        ///// </summary>
        //private int GetFaciliyRepeatIntervalConfig()
        //{
        //    string strInterval = ConfigurationManager.AppSettings["FACILITY_CTRL_REPEAT_INTERVAL"];
        //    int interval = 300;
        //    bool isValidConfig = int.TryParse(strInterval, out interval);
        //    if (!isValidConfig)
        //    {
        //        throw new UserException($"設定値 FACILITY_CTRL_REPEAT_INTERVAL: {strInterval}を整数に変更できませんでした。");
        //    }
        //    return interval;
        //}

        ///// <summary>
        ///// 管制モードに合うように対象設備を更新制御
        ///// </summary>
        ///// <param name="facStatusList">現在設備ステータスリスト</param>
        //private void UpdateManagerModeFacility(List<FacilityStatus> facStatusList)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        LOGGER.Debug($"管制モード表示設備更新");

        //        Dictionary<FacilityStatus, int> unmatchingFacilities =
        //            facStatusList
        //            .Where(f => f.FacilityTypeID == (int)FacilityTypes.MngModeViewer)
        //            .Where(f => f.Status1 != (int)Mode)
        //            .ToDictionary(f => f, f => (int)Mode);

        //        if (unmatchingFacilities.Count <= 0)
        //        {
        //            LOGGER.Debug($"設備ステータスが管制モードに一致するため処理スキップ");
        //            return;
        //        }
        //        else
        //        {
        //            ParallelFacilityControl(unmatchingFacilities);
        //        }
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        //#endregion

        #endregion

        #region 待機中への戻し
        /// <summary>
        /// 待機中への戻し
        /// </summary>
        public void WaitControlEndResetStatus()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (m_EmergencyStop == false)
            {
                LOGGER.Info($"ステータスを待機中に変更します：{m_CarControlStatus}=>{OperationConst.ControlStatusType.C_NOT_IN_CONTROL}");
            }
            m_CarControlStatus = OperationConst.ControlStatusType.C_NOT_IN_CONTROL;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region エリア幅調整

        public void SpeedAreaAdjust(double val)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            foreach (SpeedArea area in m_SpeedAreas)
            {
                area.LeftWidth = area.LeftWidth + val >= 0 ? (float)(area.LeftWidth + val) : 0;
                area.RightWidth = area.RightWidth + val >= 0 ? (float)area.RightWidth + val : 0;
            }

            // エリア全体更新
            for (var i = 0; i <= m_SpeedAreas.Count - 1; i++)
            {
                m_SpeedAreas[i].Area = AreaSet(i, m_SpeedAreas[i]);
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        public void SpeedAreaReset(CourseInfo course)
        {
            m_RouteNodes.Clear();
            if (course.CourseArea != null)
            {
                for (int i = 0; i <= course.CourseNode.Count - 1; i++) { m_RouteNodes.Add(course.CourseNode[i].ToRoutenode()); }
            }
            m_SpeedAreas.Clear();
            if (course.CourseArea != null)
            {
                for (int i = 0; i <= course.CourseArea.Count - 1; i++) { m_SpeedAreas.Add(course.CourseArea[i].ToSpeedArea()); }
            }
        }

        #region エリアの各座標値のセット
        public PointF[] AreaSet(int i, SpeedArea NowSpeedArea)
        {
            List<PointF> area = new List<PointF>();
            int ncnt = i + 1;

            double uedge = 0;
            double vertical = 0;

            uedge = m_RouteNodes[ncnt].X - m_RouteNodes[ncnt - 1].X; // 底辺
            vertical = m_RouteNodes[ncnt].Y - m_RouteNodes[ncnt - 1].Y; // 高さ
            double rad = Math.Atan2(vertical, uedge); // ラジアン
            double cosθ = Math.Cos(rad);
            double sinθ = Math.Sin(rad);

            // エリアの頂点座標の計算
            area.Add(new PointF((float)(m_RouteNodes[ncnt - 1].X + NowSpeedArea.RightWidth * sinθ), (float)(m_RouteNodes[ncnt - 1]  .Y - NowSpeedArea.RightWidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt].X + NowSpeedArea.RightWidth * sinθ), (float)(m_RouteNodes[ncnt].Y - NowSpeedArea.RightWidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt].X - NowSpeedArea.LeftWidth * sinθ), (float)(m_RouteNodes[ncnt].Y + NowSpeedArea.LeftWidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt - 1].X - NowSpeedArea.LeftWidth * sinθ), (float)(m_RouteNodes[ncnt - 1].Y + NowSpeedArea.LeftWidth * cosθ)));
            area.Add(area[0]);

            if (i > 0)
            {
                PointF O = new PointF(m_RouteNodes[ncnt - 2].X, m_RouteNodes[ncnt - 2].Y);
                PointF A = new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y);
                PointF B = new PointF(m_RouteNodes[ncnt].X, m_RouteNodes[ncnt].Y);
                
                double Z = CommonProc.Gaiseki(O, A, B);
                if (Z > 0)
                {
                    area.Insert(4, new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y));
                    area.Insert(5, m_SpeedAreas[i - 1].Area[1]);
                }
                else if (Z < 0)
                {
                    area.Insert(4, m_SpeedAreas[i - 1].Area[2]);
                    area.Insert(5, new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y));
                }
            }

            return area.ToArray();
        }

        #endregion

        #endregion

        #region 舵角加速度通知タイマー処理
        /// <summary>
        /// 舵角加速度通知タイマー処理
        /// </summary>
        /// <param name="state"></param>
        private void Callback_Kasoku(object state)
        {
            lock (m_carcontrollock)
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                try
                {
                    //タイマー停止
                    if (m_timer_cc == null) { return; }
                    m_timer_cc.Change(Timeout.Infinite, Timeout.Infinite);
                    
                    if (TvecsCommClStatus == true)
                    {
                        //車両通信端末と画像解析装置から電文を取得するまで車両制御指示は送らない
                        if (m_carstarted == 0 || GetCamStarted(MainC) == 0 || GetCamStarted(SubC) == 0)
                        {
                            LOGGER.Info("測位情報、車両状態通知受信待ち");
                            return;
                        }

                        //時間計測
                        var sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        //加速度舵角を計算して通知送信
                        bool res = KasokuDakakuSnd();

                        //100ms間隔で送信できるように待機
                        sw.Stop();
                        int t = (C_CC_SEND_INTERVAL - C_CC_CALC_WAITTIME) - sw.Elapsed.Milliseconds;
                        AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} t=>{t}");
                        if (t < 0) { t = 0; }
                        Thread.Sleep(t);

                        //舵角通知送信
                        if (res)
                        {
                            SpeedAngleSnd();
                        }
                    }
                }
                catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
                catch (Exception ex)
                {
                    LOGGER.Error(ex.Message);
                    EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}",CommonProc.charToString(m_avpappOutputData.avp_error_code));
                    ExceptionProcess.ComnExceptionConsoleProcess(ex);
                }
                finally
                {
                    int interval = C_CC_CALC_WAITTIME;
                    if (m_timer_cc != null) { m_timer_cc.Change(interval, interval); }
                    AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
                }
            }
        }

        /// <summary>
        /// 加速度舵角通知送信
        /// </summary>
        private bool KasokuDakakuSnd()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //※　各値の取得
            m_panaInputData.camera_id = GetCamRcvInfo(MainC).CamID;
            m_panaInputData.reliability = GetCamRcvInfo(MainC).Reliability;
            m_panaInputData.goalno = m_ConfigData.GoalNo;

            m_observedData.observed_vehicle_yaw = CommonProc.ToRadian(GetCamRcvInfo(MainC).MoveVector);
            m_observedData.observed_vehicle_x = GetCamRcvInfo(MainC).xPosition;
            m_observedData.observed_vehicle_y = GetCamRcvInfo(MainC).yPosition;
            m_observedData.observed_data_timestamp = GetCamRcvInfo(MainC).observed_data_timestamp;
            //※

            // 一時停止状態取得
            m_controlCenterData.request_control_status = GetPauseStatus();

            // 送信時間計算
            DateTime serdatetime = DateTime.Now;
            m_sertimespan = serdatetime - m_now;
            m_controlCenterData.timestamp = (uint)Math.Floor(m_sertimespan.TotalMilliseconds);
            LOGGER.Info($"[差分取得]timestamp：{m_controlCenterData.timestamp},");


            string goalCoordinate = null;
            foreach (PointF goalPointF in m_controlCenterData.goalarea)
            {
                goalCoordinate += $@"{goalPointF.X.ToString()},{goalPointF.Y.ToString()}, ";
            }

            LOGGER.Info($@"計測項目（入力） ,→ ,{m_vehicleData.is_vehicle_stop.ToString()} ,{m_vehicleData.vehicle_speed.ToString()} ,{m_vehicleData.steer_angle.ToString()} ,{m_vehicleData.gx.ToString()} ,{m_vehicleData.gy.ToString()} ,{m_vehicleData.yr.ToString()} ,{m_vehicleData.vehicle_data_timestamp.ToString()},{m_vehicleData.steer_torque.ToString()} ,{m_vehicleData.is_steer_torque_invalid.ToString()} ,{m_vehicleData.steer_torque_enhaned.ToString()} ,{m_vehicleData.eps_motor_torque.ToString()} ,{m_vehicleData.is_vehicle_speed_from_vsc_invalid.ToString()} ,{m_vehicleData.sp1_pulse.ToString()} ,{m_vehicleData.vehicle_speed_from_vsc.ToString()} ,{m_vehicleData.request_gx_from_b_pedal.ToString()},{m_vehicleData.request_gx_from_a_pedal.ToString()},{m_vehicleData.estimated_gx_form_vsc.ToString()} ,{m_vehicleData.gvc_invalid.ToString()} ,{m_vehicleData.epb_lock_state.ToString()} ,{m_vehicleData.shift_range_from_vmc.ToString()} ,{m_vehicleData.yawrate_sensor_1_invalid.ToString()} ,{m_vehicleData.yawrate_sensor_2_invalid.ToString()} ,{m_vehicleData.yaw_g_sensor_power_invalid.ToString()},{m_vehicleData.g_sensor_1_invalid.ToString()} ,{m_vehicleData.yaw_g_sensor_type_identification.ToString()},{m_vehicleData.g_sensor_2_invalid.ToString()},{m_observedData.observed_vehicle_x.ToString()} ,{m_observedData.observed_vehicle_y.ToString()} ,{m_observedData.observed_vehicle_yaw.ToString()} ,{m_observedData.observed_data_timestamp.ToString()} ,{m_controlCenterData.timestamp.ToString()}  ,{m_controlCenterData.request_control_status.ToString()},{goalCoordinate}");

            
            //指示値計算
            try
            {
                m_avpappOutputData = (AVPAppOutputData)m_calc.CalcCtrlTargetValue(m_vehicleData, m_observedData, m_controlCenterData, m_panaInputData).Clone();
            }
            catch (Exception e)
            {
                EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}",Res.ErrorStatus.UNKNOWN_ERR.Code);
                ExceptionProcess.ComnExceptionConsoleProcess(e);

                SetCarInfo(m_CarInfo, false, new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y));
            }

            //計算ステータス判定
            if (m_avpappOutputData.avp_app_state ==　OperationConst.CalcAppStatus.C_CONTROL_OFF || m_avpappOutputData.avp_app_state == OperationConst.CalcAppStatus.C_CONTROL_PREPARATION || m_avpappOutputData.avp_app_state == OperationConst.CalcAppStatus.C_CONTROL_READY)
            {
                SetCarInfo(m_CarInfo, false, new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y));
                LOGGER.Info($"制御準備中...：{m_avpappOutputData.avp_app_state}");
                return false;
            }
            else if (m_avpappOutputData.avp_app_state == OperationConst.CalcAppStatus.C_CONTROL_NOMAL || m_avpappOutputData.avp_app_state == OperationConst.CalcAppStatus.C_CONTROL_BLACKOUT)
            {
                //車両4隅を計算しエリア外判定をする
                List<CarCornerinfo> carCornerinfo = new List<CarCornerinfo>();
                carCornerinfo = AskContainArea_cornerpoint(new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y), new PointF((float)m_observedData.observed_vehicle_x, (float)m_observedData.observed_vehicle_y), m_avpappOutputData.draw_info.yawrate);
                AskContainArea(new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y));
                CarCornerinfo result = carCornerinfo.FirstOrDefault(c => c.OutOfArea == true);
                if (result != null)
                {
                    m_outarea++;
                    if (m_outarea > C_CARCORNER_OUTAREA_ALLOW_NUM)
                    {
                        CarCornerinfo carCorner = carCornerinfo.Find(c => c.OutOfArea == true);
                        LOGGER.Error(carCorner.cornerName + $"が範囲外です。異常通知します。x:{carCorner.cornerPoint.X.ToString()} y:{carCorner.cornerPoint.Y.ToString()}");
                        
                        EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}",Res.ErrorStatus.I_ERR.Code);
                        return false;
                    }
                }
                else
                {
                    m_outarea = 0;
                }
                LOGGER.Debug($"m_outarea値:{m_outarea}");

                // ガイド情報
                if (guideAreaList.FindAll(x => x.inareaflg == true).Count() == 0)
                {
                    m_avpappOutputData.guideareaInfo = 0;
                }
                else
                {
                    foreach (GuideareaJudge guideAJ in guideAreaList)
                    {
                        if (guideAJ.inareaflg == true)
                        {
                            m_avpappOutputData.guideareaInfo = (uint)(0 * 100 + guideAJ.guidearea.verticalGuide * 10 + guideAJ.guidearea.besideGuide);
                        }
                    }
                }

                try
                {
                    if (m_avpappOutputData.panaOutputData.log == null)
                    {
                        SetCarInfo(m_CarInfo, true, new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y));
                    }
                    else
                    {
                        SetCarInfo(m_CarInfo, m_avpappOutputData.panaOutputData.get, new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y));
                    }
                }
                catch (Exception e)
                {
                    LOGGER.Error("inexception");
                    ExceptionProcess.ComnExceptionProcess(e);
                    SetCarInfo(m_CarInfo, true, new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y));
                }

            }
            else if (m_avpappOutputData.avp_app_state == OperationConst.CalcAppStatus.C_CONTROL_COMPLETE)
            {
                if (m_goalcomp_flg == 0)
                {
                    LOGGER.Info($"制御部ゴール判定...：{m_avpappOutputData.avp_app_state}");
                    m_goalcomp_flg = 1;

                    // ADD 20230528
                    foreach(OtherAreaRetention i in m_otherArea)
                    {
                        if (i.otherarea.AreaCode == 0 && i.otherarea.Number == m_ConfigData.GoalNo)
                        {
                            m_senser_check = i.otherarea.Sensor;
                        }
                    }
                    //
                }

                if (m_senser_check)
                {
                    if (!m_GoalCheckSnd)
                    {
                        //ゴール在籍センサ状態の確認
                        m_GoalCheckSnd = true;
                        //Task.Run(() => GoalSensorStatusCheck());
                    }
                }
                else
                {
                    LOGGER.Info("在籍センサー確認なし");
                    Task.Run(() => EndProcess());
                }

                SetCarInfo(m_CarInfo, true, new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y));
            }
            else
            {
                EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}", CommonProc.charToString(m_avpappOutputData.avp_error_code));

                SetCarInfo(m_CarInfo, false, new PointF((float)m_avpappOutputData.control_point.x, (float)m_avpappOutputData.control_point.y));
            }
            
            LOGGER.Info($@"計測項目（出力） ,→ ,{m_avpappOutputData.target_steering_angle.ToString()} ,{m_avpappOutputData.target_gx.ToString()} ,{m_avpappOutputData.control_point.x.ToString()} ,{m_avpappOutputData.control_point.y.ToString()} ,{m_avpappOutputData.draw_info.yawrate.ToString()} ,{m_avpappOutputData.draw_info.angle_guard.ToString()} ,{m_avpappOutputData.draw_info.guard_front_angle.ToString()} ,{m_avpappOutputData.avp_app_state.ToString()} ");

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");

            return true;
        }

        private void SpeedAngleSnd()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            LOGGER.Debug($"carNo:{m_CarNo},Speed:{m_avpappOutputData.target_gx},Dakaku:{m_avpappOutputData.target_steering_angle},GuideareaInfo:{m_avpappOutputData.guideareaInfo}");

            SndKasokuDakaku snd = new SndKasokuDakaku
            {
                CarNo = m_CarNo,
                Speed = m_avpappOutputData.target_gx,
                Dakaku = (float)m_avpappOutputData.target_steering_angle,
                guideareaInfo = m_avpappOutputData.guideareaInfo
            };

            if (TvecsCommClStatus == true) { string strResponse = m_tcl.SendMessage(snd, false); }
            m_calcstarted = OperationConst.C_RECEIVED;
            LOGGER.Info($"[TVECS]送信→車両制御指示");

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region ### 制御指示送信後タイマー開始 ###
        /// <summary>
        /// 制御指示送信後タイマー開始
        /// </summary>
        /// <param name="state"></param>
        private void Callback_CarStatus(object state)
        {
            try
            {
                //タイマー停止
                if (m_timer_ci == null) { return; }
                m_timer_ci.Change(Timeout.Infinite, Timeout.Infinite);

                if (m_CarControlStatus == OperationConst.ControlStatusType.C_NOT_IN_CONTROL)
                {
                    if (GetCamStartRes(MainC) == OperationConst.C_NOT_RECEIVED || m_CarStartRes == OperationConst.C_NOT_RECEIVED)
                    {
                        LOGGER.Info("測位開始応答待ち");
                        return;
                    }
                    else
                    {
                        ControlStartStatus(Res.ErrorStatus.NORMAL.Code);
                        m_CarControlStatus = OperationConst.ControlStatusType.C_UNDER_CONTROL;
                        TimerStart();
                        m_ci_interval = C_CI_INTERVAL;
                    }
                }

                //if (m_calcstarted == OperationConst.C_NOT_RECEIVED) { return; }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally
            {
                if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
                {
                    m_timer_ci.Dispose();
                    m_timer_ci = null;
                }
                else
                {
                    //タイマー再開
                    if (m_timer_ci != null) { m_timer_ci.Change(m_ci_interval, m_ci_interval); }
                }
            }
        }


        private void TimerStart()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //その他エリアチェック開始
            OtherAreaCheck();

            // 安全監視
            StartSafetyMonitortringTimer();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region その他エリアチェック
        /// <summary>
        /// その他エリアの範囲チェックを行う
        /// </summary>
        private void OtherAreaCheck()
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                foreach (OtherAreaRetention i in m_otherArea)
                {
                    Task.Run(() => CheckTimer(i));
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private async void CheckTimer(OtherAreaRetention i)
        {
            try
            {
                switch (i.otherarea.AreaCode)
                {
                    case OtherAreaCode.GoalArea:
                        while (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL && i.Done != OperationConst.FacilityAreaStage.C_FACILITY_FIN)
                        {
                            if (i.otherarea.Number == m_ConfigData.GoalNo)
                            {
                                GoalAreaCheck(i);
                            }
                            await Task.Delay(OperationConst.C_OA_INTERVAL).ConfigureAwait(false);
                        }
                        break;
                    case OtherAreaCode.FacilityArea:
                        while (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL && i.Done != OperationConst.FacilityAreaStage.C_FACILITY_FIN)
                        {
                            FacilityAreaCheck(i);
                            await Task.Delay(OperationConst.C_OA_INTERVAL).ConfigureAwait(false);
                        }
                        break;
                    //case OtherAreaCode.GoalChangeStopArea:
                    //    while (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL && i.Done != OperationConst.FacilityAreaStage.C_FACILITY_FIN)
                    //    {
                    //        GoalChangeStopAreaCheck(i);
                    //        await Task.Delay(OperationConst.C_OA_INTERVAL).ConfigureAwait(false);
                    //    }
                    //    break;
                    case OtherAreaCode.GuideArea:
                        while (m_CarControlStatus ==　OperationConst.ControlStatusType.C_UNDER_CONTROL && i.Done != OperationConst.FacilityAreaStage.C_FACILITY_FIN)
                        {
                            GuideareaCheck(i);
                            //GoalChangeStopAreaCheck(i);
                            await Task.Delay(OperationConst.C_OA_INTERVAL).ConfigureAwait(false);
                        }
                        break;
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex)
            {
                ExceptionProcess.ComnExceptionConsoleProcess(ex);

                if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
                {
                    LOGGER.Info($"原因不明のエラーにより走行停止します");
                    EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}", Res.ErrorStatus.UNKNOWN_ERR.Code);
                }
            }
            finally { }
        }

        #region GoalAreaCheck（ゴールエリアの判定を行う）
        /// <summary>
        /// ゴールエリアの判定を行う
        /// </summary>
        /// <param name="i"></param>
        private void GoalAreaCheck(OtherAreaRetention i)
        {
            //車両通信端末と画像解析装置から電文を取得するまでチェックをしない
            if (m_carstarted == 0 || GetCamStarted(MainC) == 0) { return; }

            int idx = i.Done;
            if (PositionInArea(i.otherarea.area[idx]))
            {
                //管制端末からの指示とゴール番号が同じであればゴールと判定
                if (i.otherarea.Number == m_ConfigData.GoalNo)
                {
                    LOGGER.Info("ゴールエリアに侵入しました。");
                    //ゴールフラグを立て、車両状態通知で速度0が来るのを待つ
                    //m_evGoalRcvWait.Reset();
                    m_goal_flg = 1;
                    m_senser_check = i.otherarea.Sensor;
                }
                m_TerminalStatus = Res.CtrlStatus.ARRV_GOAL_AREA.Code;
                i.Done = OperationConst.FacilityAreaStage.C_FACILITY_FIN;

                //車両の停車と在籍センサの待機を待つ
                if (i.otherarea.Sensor)
                {
                    ////受信待機
                    //if (m_evGoalRcvWait.WaitOne(C_SENSORCHECK_DELAY))
                    //{
                    //    string msg = "";
                    //    if (i.otherarea.Number < m_ConfigData.GoalNo)
                    //    {
                    //        msg = "在籍センサの検知を確認しました";
                    //        LOGGER.Info($"{msg}。{m_ConfigData.GoalNo}:{m_ConfigData.GoalNo}");
                    //        Task.Run(() => EndProcess());
                    //    }
                    //    else
                    //    {
                    //        msg = $"在籍センサで検知できません。走行停止します";
                    //        LOGGER.Info($"{msg}。{m_ConfigData.GoalNo}:{m_ConfigData.GoalNo}");
                    //        EmergencyEndProcess(Res.ErrorStatus.P_ERR.Code);
                    //    }
                    //}
                    //else
                    //{
                    //    LOGGER.Info($"在籍エリア応答(ゴール変更通知)を受信できません。走行停止します。");
                    //    EmergencyEndProcess(Res.ErrorStatus.P_ERR.Code);
                    //}
                }
            }
        }
        #endregion

        #region FacilityAreaCheck（周辺設備の範囲判定を行う）
        /// <summary>
        /// 周辺設備の範囲判定を行う
        /// </summary>
        /// <param name="i"></param>
        private void FacilityAreaCheck(OtherAreaRetention i)
        {
            //車両通信端末と画像解析装置から電文を取得するまでチェックをしない
            if (m_carstarted == 0 || GetCamStarted(MainC) == 0) { return; }

            int idx = i.Done;
            if (PositionInArea(i.otherarea.area[idx]))
            {
                OtherArea area = i.otherarea;
                switch (idx)
                {
                    //指示開始エリア
                    case OperationConst.FacilityAreaStage.C_FACILITY_START:
                        //SndFacilityControl sndfcst = new SndFacilityControl();
                        //sndfcst.FacilityID = area.FacilityID;
                        //sndfcst.Command = "1";
                        //string rcvst = "";
                        //for (int count = 1; count <= C_RETRYCOUNT; count++)
                        //{
                        //    try
                        //    {
                        //        rcvst = m_mcl.SendMessage(sndfcst, m_ConfigConst.C_SNDRSESULT_MNG);
                        //        LOGGER.Info($"[管制端末]送信→設備制御指示:{area.FacilityID}.{area.Name}");
                        //        if (m_ConfigConst.C_SNDRSESULT_MNG && rcvst == null) { throw new Exception(); }
                        //        i.Done = OperationConst.FacilityAreaStage.C_FACILITY_CHECK;
                        //        break;
                        //    }
                        //    catch
                        //    {
                        //        if (count == C_RETRYCOUNT)
                        //        {
                        //            LOGGER.Info($"設備制御指示送信失敗。走行停止します。");
                        //            EmergencyEndProcess(Res.ErrorStatus.UNKNOWN_ERR.Code);
                        //            return;
                        //        }

                        //        LOGGER.Info($"設備制御指示送信失敗。再送信します。");
                        //    }
                        //}
                        //if (m_ConfigConst.C_SNDRSESULT_MNG)
                        //{
                        //    string msgid = CommMsgBase.GetMsgID(rcvst);
                        //    RcvRsultRcvMsg msg = new RcvRsultRcvMsg(rcvst);
                        //    if (msgid == RcvRsultRcvMsg.msgId)
                        //    {
                        //        LOGGER.Info($"設備制御指示応答受信:{rcvst}");
                        //    }
                        //}
                        LOGGER.Info($"設備動作開始エリアへ侵入しました。動作を開始します。設備ID:{area.FacilityID}");
                        SetFacilityRequestedStatus(area.FacilityID, 1);
                        break;
                    //指示終了エリア
                    case OperationConst.FacilityAreaStage.C_FACILITY_END:
                        //SndFacilityControl sndfcend = new SndFacilityControl();
                        //sndfcend.FacilityID = area.FacilityID;
                        //sndfcend.Command = "0";
                        //string rcvend = "";
                        //for (int count = 1; count <= C_RETRYCOUNT; count++)
                        //{
                        //    try
                        //    {
                        //        rcvend = m_mcl.SendMessage(sndfcend, m_ConfigConst.C_SNDRSESULT_MNG);
                        //        LOGGER.Info($"[管制端末]送信→設備制御指示:{area.FacilityID}.{area.Name}");
                        //        if (m_ConfigConst.C_SNDRSESULT_MNG && rcvend == null) { throw new Exception(); }
                        //        i.Done = OperationConst.FacilityAreaStage.C_FACILITY_FIN;
                        //        break;
                        //    }
                        //    catch
                        //    {
                        //        if (count == C_RETRYCOUNT)
                        //        {
                        //            LOGGER.Info($"設備制御指示送信失敗。走行停止します。");
                        //            EmergencyEndProcess(Res.ErrorStatus.FACILITY_ERR.Code);
                        //            return;
                        //        }

                        //        LOGGER.Info($"設備制御指示送信失敗。再送信します。");
                        //    }
                        //}
                        //if (m_ConfigConst.C_SNDRSESULT_MNG)
                        //{
                        //    string msgid = CommMsgBase.GetMsgID(rcvend);
                        //    RcvRsultRcvMsg msg = new RcvRsultRcvMsg(rcvend);
                        //    if (msgid == RcvRsultRcvMsg.msgId)
                        //    {
                        //        LOGGER.Info($"設備制御指示応答受信:{rcvend}");
                        //    }
                        //}
                        LOGGER.Info($"設備動作終了エリアへ侵入しました。動作を開始します。設備ID:{area.FacilityID}");
                        SetFacilityRequestedStatus(area.FacilityID, 0);
                        break;
                    //指示完了確認エリア
                    case OperationConst.FacilityAreaStage.C_FACILITY_CHECK:
                        if (i.status == 0)
                        {
                            LOGGER.Info($"設備制御未完了のため走行停止します。設備：{i.otherarea.FacilityID}.{area.Name}");
                            EmergencyEndProcess(Res.ErrorStatus.FACILITY_RES_ERR.Code, OperationConst.ErrType.C_PT_FACILITY, i.otherarea.FacilityID, "1");

                        }
                        else
                        {
                            //動作後であれば一時停止せず進める
                            i.Done = OperationConst.FacilityAreaStage.C_FACILITY_END;
                        }
                        break;
                }
            }
        }
        #endregion

        #region GoalChangeStopAreaCheck（ゴールエリア判定通知の送信を行う）
        //private void GoalChangeStopAreaCheck(OtherAreaRetention i)
        //{
        //    //車両通信端末と画像解析装置から電文を取得するまでチェックをしない
        //    if (m_carstarted == 0 || GetCamStarted(MainC) == 0) { return; }

        //    int idx = i.Done;
        //    if (PositionInArea(i.otherarea.area[idx]))
        //    {
        //        SndGoalChangeStop sndmsg = new SndGoalChangeStop();

        //        string rcvmsg = "";

        //        for (int count = 1; count <= C_RETRYCOUNT; count++)
        //        {
        //            try
        //            {
        //                LOGGER.Info($"[管制端末]送信→ゴールエリア判定通知");
        //                m_evCheckRcvWait.Reset();
        //                rcvmsg = m_mcl.SendMessage(sndmsg, m_ConfigConst.C_SNDRSESULT_MNG);
        //                if (m_ConfigConst.C_SNDRSESULT_MNG && rcvmsg == null) { throw new Exception(); }
        //                i.Done = OperationConst.FacilityAreaStage.C_FACILITY_FIN;
        //                break;
        //            }
        //            catch
        //            {
        //                if (count == C_RETRYCOUNT)
        //                {
        //                    LOGGER.Info($"ゴールエリア判定通知送信失敗。走行停止します。");
        //                    EmergencyEndProcess(Res.ErrorStatus.UNKNOWN_ERR.Code);
        //                    m_evCheckRcvWait.Set();
        //                    return;
        //                }

        //                LOGGER.Info($"ゴールエリア判定通知送信失敗。再送信します。");
        //            }
        //        }

        //        if (m_ConfigConst.C_SNDRSESULT_MNG)
        //        {
        //            string msgid = CommMsgBase.GetMsgID(rcvmsg);
        //            RcvRsultRcvMsg msg = new RcvRsultRcvMsg(rcvmsg);
        //            if (msgid == RcvRsultRcvMsg.msgId)
        //            {
        //                LOGGER.Info($"ゴールエリア判定通知応答受信:{rcvmsg}");
        //            }
        //        }

        //        //受信確認
        //        if (m_evCheckRcvWait.WaitOne(m_ConfigConst.C_GOALCHECK_DELAY))
        //        {

        //        }
        //        else
        //        {
        //            LOGGER.Info($"ゴールエリア判定結果を受信できません。走行停止します。");
        //            EmergencyEndProcess(Res.ErrorStatus.Q_ERR.Code);
        //        }
        //    }
        //}
        #endregion

        #region GoalChangeStopAreaCheck(ガイドの強さを送信する）
        private void GuideareaCheck(OtherAreaRetention i)
        {
            if (m_carstarted == 0 || GetCamStarted(MainC) == 0) { return; }

            int idx = i.Done;
            //LOGGER.Debug($"idx:{idx}");
            if (PositionInArea(i.otherarea.area[idx]))
            {
                guideAreaList.SingleOrDefault(t => t.guidearea == i.otherarea).inareaflg = true;
            }
            else
            {
                guideAreaList.SingleOrDefault(t => t.guidearea == i.otherarea).inareaflg = false;
            }
        }
        #endregion

        #region 現在測位点のエリアに対する内包判定
        public Boolean PositionInArea(PointF[] area)
        {
            bool contain = false;
            //旧前方注視点->リア軸中心？？
            if (m_newPPoint != PointF.Empty)
            {
                PointF point = new PointF(m_newPPoint.X, m_newPPoint.Y);

                contain = CommonProc.PointInPolygon(point, area);
            }

            return contain;
        }
        #endregion

        #endregion

        #region 測位点取得

        private void SetCarInfo(CarInfo info, bool get, PointF newp)
        {
            PointF p = new PointF(info.xPosition, info.yPosition);

            PointF p1 = newp;

            if (get)
            {
                m_ErrCnt = 0;

                //測位点取得(更新)
                AddCarInfo(info, p);
                m_newPPoint = p1;
            }
            else
            {
                //取得範囲外測位点情報保持
                m_ErrCnt++;
                //LOGGER.Info($"測位点取得範囲外のため測位位置をスルーします{{{m_ErrCnt}}}。x:{info.xPosition.ToString()} y:{info.yPosition.ToString()}");
            }
        }
        /// <summary>
        /// ※測位点を取得する（走行履歴をストックする）
        /// </summary>
        /// <param name="info"></param>
        /// <param name="p"></param>
        private void AddCarInfo(CarInfo info, PointF p)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            CarInfo CarStatus = new CarInfo();
            CarStatus.angle = info.angle;
            CarStatus.xPosition = p.X;
            CarStatus.yPosition = p.Y;
            CarStatus.speed = info.speed;
            CarStatus.Yawrate = info.Yawrate;
            CarStatus.YawrateTime = info.YawrateTime;

            //PointF LPoint = toLocalCoordinate(new PointF(p.X, p.Y));
            //m_CarStatusList.Add(CarStatus);
            //m_CarStatusListLPoint.Add(LPoint);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region 走行エリア判定
        /// <summary>
        ///  現在地から属するエリアのノードを返す
        /// </summary>
        /// <param name="point1">リア軸中心</param>
        /// <returns></returns>
        public void AskContainArea(PointF point1)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            bool contain = false;
            int idx = -1;

            PointF p = point1;

            //エリア内包判定
            if (m_contain_idx == -1)
            {
                //先頭から判定をする
                for (int i = 0; i <= m_SpeedAreas.Count() - 1; i++)
                {
                    if (CommonProc.PointInPolygon(p, m_SpeedAreas[i].Area))
                    {
                        contain = true;
                        idx = i;
                        break;
                    }
                }
            }
            else
            {
                int cnt = C_INCLUDE_JUDGE_AREA_COUNT;
                int fromidx = (m_contain_idx <= cnt) ? 0 : m_contain_idx - cnt;
                int toidx = (m_SpeedAreas.Count() - 1 <= m_contain_idx + cnt) ? m_SpeedAreas.Count() - 1 : m_contain_idx + cnt;
                //前回内包していたエリアの前後のみ確認する
                for (int i = toidx; i >= fromidx; i--)
                {
                    if (CommonProc.PointInPolygon(p, m_SpeedAreas[i].Area))
                    {
                        contain = true;
                        idx = i;
                        break;
                    }
                }
            }

            //エリア内であるとき
            if (contain)
            {
                m_contain_idx = idx;
            }
            //エリア外であるとき
            if (!contain)
            {
                LOGGER.Info($"現在地が設定エリア外です。制御点-X:{point1.X.ToString()} Y:{point1.Y.ToString()}");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }


        public List<CarCornerinfo> AskContainArea_cornerpoint(PointF rearPoint, PointF point2, double yawrate)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            List<PointF> cornersPoint = new List<PointF>();
            cornersPoint = CalcCorners(rearPoint.X, rearPoint.Y, yawrate);

            foreach (PointF corner in cornersPoint)
            {
                if (m_CarCornerinfo.Count() < cornersPoint.Count())
                {
                    m_CarCornerinfo.Add(new CarCornerinfo
                    {
                        cornerName = m_cornername[cornersPoint.IndexOf(corner)],
                        containAreaidx = -1,
                        OutOfArea = false,
                        cornerPoint = corner
                    });
                }

                bool contain = false;
                int idx = -1;

                PointF p = corner;

                //エリア内包判定
                if (m_CarCornerinfo[cornersPoint.IndexOf(p)].containAreaidx == -1)
                {
                    //先頭から判定をする
                    for (int i = 0; i <= m_SpeedAreas.Count() - 1; i++)
                    {
                        if (CommonProc.PointInPolygon(p, m_SpeedAreas[i].Area))
                        {
                            contain = true;
                            idx = i;
                            m_CarCornerinfo[cornersPoint.IndexOf(p)].cornerPoint = corner;
                            break;
                        }
                    }
                }
                else
                {

                    int cnt = C_INCLUDE_JUDGE_AREA_COUNT;
                    int fromidx = (m_CarCornerinfo[cornersPoint.IndexOf(p)].containAreaidx <= cnt) ? 0 : m_CarCornerinfo[cornersPoint.IndexOf(p)].containAreaidx - cnt;
                    int toidx = (m_SpeedAreas.Count() - 1 <= m_CarCornerinfo[cornersPoint.IndexOf(p)].containAreaidx + cnt) ? m_SpeedAreas.Count() - 1 : m_CarCornerinfo[cornersPoint.IndexOf(p)].containAreaidx + cnt;
                    //前回内包していたエリアの前後のみ確認する
                    for (int i = toidx; i >= fromidx; i--)
                    {
                        if (CommonProc.PointInPolygon(p, m_SpeedAreas[i].Area))
                        {
                            contain = true;
                            idx = i;
                            m_CarCornerinfo[cornersPoint.IndexOf(p)].cornerPoint = corner;
                            m_CarCornerinfo[cornersPoint.IndexOf(p)].OutOfArea = true;
                            break;
                        }
                    }
                }
                //エリア外であるとき
                m_CarCornerinfo[cornersPoint.IndexOf(p)].OutOfArea = false;
                if (!contain)
                {
                    LOGGER.Info($"現在地が設定エリア外です。制御点-X:{corner.X.ToString()} Y:{corner.Y.ToString()} 測位点-X:{point2.X.ToString()} Y:{point2.Y.ToString()}");
                    m_CarCornerinfo[cornersPoint.IndexOf(p)].OutOfArea = true;
                    m_CarCornerinfo[cornersPoint.IndexOf(p)].cornerPoint = corner;
                    //break;
                }
                else
                {
                    if (m_CarCornerinfo[cornersPoint.IndexOf(p)].containAreaidx == -1)
                    {
                        m_CarCornerinfo[cornersPoint.IndexOf(p)].containAreaidx = idx;
                    }
                    else if (cornersPoint.IndexOf(p) == 0 || cornersPoint.IndexOf(p) == 3)
                    {
                        m_CarCornerinfo[0].containAreaidx = idx;
                        m_CarCornerinfo[3].containAreaidx = idx;
                        LOGGER.Debug($"m_CarCornerinfo[0,3].containAreaidx:{idx}");
                    }
                    else if (cornersPoint.IndexOf(p) == 1 || cornersPoint.IndexOf(p) == 2)
                    {
                        m_CarCornerinfo[1].containAreaidx = idx;
                        m_CarCornerinfo[2].containAreaidx = idx;
                        LOGGER.Debug($"m_CarCornerinfo[1,2].containAreaidx:{idx}");
                    }
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return m_CarCornerinfo;
        }

        /// <summary>
        /// 後輪軸中心座標から車両4隅の計算する
        /// </summary>
        /// <returns></returns>
        public List<PointF> CalcCorners(double rearCenterx, double rearCentery, double carYawrate)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            LOGGER.Debug($"車両前長さ{m_carSpec.CenterFrontLength},車両後ろ長さ{m_carSpec.RearOverhang},車両全幅{m_carSpec.WidthDistance},車両ヨーレート{carYawrate},車両リア軸x{rearCenterx},車両リア軸y{rearCentery}");

            double LCY = m_carSpec.CenterFrontLength * Math.Cos(carYawrate);
            double LLCY = m_carSpec.RearOverhang * Math.Cos(carYawrate);
            double LSY = m_carSpec.CenterFrontLength * Math.Sin(carYawrate);
            double LLSY = m_carSpec.RearOverhang * Math.Sin(carYawrate);

            double TCY = (m_carSpec.WidthDistance / 2) * Math.Cos(carYawrate);
            double TSY = (m_carSpec.WidthDistance / 2) * Math.Sin(carYawrate);

            List<PointF> corners = new List<PointF>();
            PointF corner = new PointF();
            corner.X = (float)(rearCenterx - LLCY - TSY);
            corner.Y = (float)(rearCentery - LLSY + TCY);

            corners.Add(corner);

            corner.X = (float)(rearCenterx + LCY - TSY);
            corner.Y = (float)(rearCentery + LSY + TCY);

            corners.Add(corner);

            corner.X = (float)(rearCenterx + LCY + TSY);
            corner.Y = (float)(rearCentery + LSY - TCY);

            corners.Add(corner);

            corner.X = (float)(rearCenterx - LLCY + TSY);
            corner.Y = (float)(rearCentery - LLSY - TCY);

            corners.Add(corner);

            LOGGER.Info($"左後端:({corners[0].X.ToString()},{corners[0].Y.ToString()}), 左前端:({corners[1].X.ToString()},{corners[1].Y.ToString()}), 右前端:({corners[2].X.ToString()},{corners[2].Y.ToString()}), 右後端:({corners[3].X.ToString()},{corners[3].Y.ToString()}),");
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");

            return corners;
        }

        #endregion

        #region 制御中タイマー停止
        /// <summary>
        /// 制御中に動作しているタイマーを止める
        /// </summary>
        private void TimerEnd()
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                // 加速度舵角を送るのをやめる
                if (m_timer_cc != null)
                {
                    m_timer_cc.Dispose();
                    m_timer_cc = null;
                    LOGGER.Info("加速度舵角送信タイマー停止");
                }

                if (C_SURVEILLANCE_CAR)
                {
                    //TVECS受信間隔
                    if (m_timer_tRcv_pause != null)
                    {
                        m_timer_tRcv_pause.Dispose();
                        m_timer_tRcv_pause = null;
                        LOGGER.Info("TVECS受信タイマー停止(一時停止)");
                    }
                    if (m_timer_tRcv_stop != null)
                    {
                        m_timer_tRcv_stop.Dispose();
                        m_timer_tRcv_stop = null;
                        LOGGER.Info("TVECS受信タイマー停止(走行停止)");
                    }
                }

                if(m_timer_ci != null) m_timer_ci.Dispose();

                //安全監視タイマー停止
                StopSafetyMonitortringTimer();

                // 制御部終了
                if (m_calc != null) { m_calc.DeleteClsCalculator(); }

                m_CarControlStatus =　OperationConst.ControlStatusType.C_NOT_IN_CONTROL;

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex)
            {
                AppLog.GetInstance().Error(ex.Message);
                AppLog.GetInstance().Error(ex.StackTrace);
            }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ## エラーメッセージ取得 ##
        public string ErrorStatusGetMsg(string code)
        {
            string msg = null;
            MSystemMessage system = m_SystemMsg.SingleOrDefault(c => c.MessageCd == code);
            //msg = m_SystemMsg.SingleOrDefault(c => c.Code == code).Msg;
            //if (msg == null) { msg = new Res.ErrorStatus().GetMsg(code); }

            msg = system != null ? system.Text : new Res.ErrorStatus().GetMsg(code);

            return msg;
        }

        /// <summary>
        /// 異常レベルを取得し返す。異常コードが定義にない場合は重度のエラーとして処理する
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public int ErrorStatusGetLevel(string code)
        {
            int msg = 2;
            MSystemMessage system = m_SystemMsg.SingleOrDefault(c => c.MessageCd == code);
            msg = system != null ? system.Level : 2;

            return msg;
        }
        #endregion

        #region　走行停止解消
        /// <summary>
        /// 走行停止解消チェック
        /// </summary>
        public void EliminationCheck()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            string status = StartStatusCheck();

            // 設備解除
            ErrorResolvegroup();

            if (status != "")
            {
                
                if (status == Res.ErrorStatus.NORMAL.Code)
                {
                    // 待機中へ変更
                    m_EmergencyStop = false;
                    m_error_flg = 0;
                }
            }


            // 車両情報削除
            ControlVehicle.Instance.Vehicle = null;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion 


        #region EmergencyEndProcessEventArgsクラス
        public class EmergencyEndProcessEventArgs
        {
            public string methodname;
            public string errcd;
            public string errtype = null;
            public string arg1 = "0000";
            public string arg2 = "0";
        }
        #endregion

        #region AnalyzerCamNoChangedEventArgsクラス
        public class AnalyzerCamNoChangedEventArgs
        {
            public OperationConst.CamType camtype;
            public string camid;
        }


        #endregion

        #region ### バージョン情報表示 ###
        private void VersionOpen_Click(object sender, EventArgs e)
        {

            //自分自身のAssemblyを取得
            Assembly asm = Assembly.GetExecutingAssembly();
            //バージョンの取得
            Version ver = asm.GetName().Version;

            // ファイル情報オブジェクトを取得する
            List<FileVersion> fileVersion= new List<FileVersion>();
            fileVersion.Add(new FileVersion(@".\RcdOperation.exe"));
            fileVersion.Add(new FileVersion(@".\AtrptCalc.dll"));
            fileVersion.Add(new FileVersion(@".\AtrptShare.dll"));
            fileVersion.Add(new FileVersion(@".\CommWrapper.dll"));
            fileVersion.Add(new FileVersion(@".\MapViewer.dll"));
            fileVersion.Add(new FileVersion(@".\RcdCmn.dll"));
            fileVersion.Add(new FileVersion(@".\RcdDao.dll"));
            fileVersion.Add(new FileVersion(@".\RcdOperationSystemConst.dll"));


            // バージョン文字列合計文字列策税
            int[] SumFileVersionList = new int[4];
            foreach (FileVersion version in fileVersion)
            {
                for(int i = 0; i < version.Ver.Count(); i++)
                {
                    SumFileVersionList[i] += int.Parse(version.Ver[i]);
                }
            }
            string SumFileVersionString = "";
            for (int i = 0; i < 4; i++)
            {
                if (i>0)
                {
                    SumFileVersionString += $".";
                }

                SumFileVersionString += $"{SumFileVersionList[i]}";
            }

            // 出力文字作成
            string message = $"Ver.{SumFileVersionString}{Environment.NewLine}{Environment.NewLine}";
            foreach(FileVersion version in fileVersion) 
            {
                message += $"{version.Name} - Ver.{version.Fvi.FileVersion}{Environment.NewLine}";
            }

            // メッセージ出力
            MessageBox.Show(message, "", MessageBoxButtons.OK);

        }

        private class FileVersion
        {
            public string Path;
            public string Name;
            public FileVersionInfo Fvi;
            public string[] Ver;

            public FileVersion(string path)
            {
                Path = path;
                Name = path.Substring(path.LastIndexOf($"\\")+1);
                Fvi = FileVersionInfo.GetVersionInfo(Path);
                Ver = Fvi.FileVersion.Split('.');
            }
        }

        #endregion

        #region ### 描画設定画面起動 ###

        private void DrawSettingFormOpen_Click(object sender, EventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            // フォームを表示する
            m_drawform.OtherAreaChangeDrawEvent += delegate (OtherAreaChangeDrawEventArgs ea)
            {
                LOGGER.Info("制御エリア描画変更イベントを取得しました");
                if (ea.redraw == "")
                {
                    Viewer.ChangeDrawOtherArea(ea.index, ea.draw, m_CarControlStatus != 1);
                }
                else
                {
                    Viewer.ChangeDrawOtherArea(ea.index, ea.draw, bool.Parse(ea.redraw));
                }
            };
            m_drawform.AngleDrawChangeEvent += delegate (AngleDrawEventArgs ea)
            {
                LOGGER.Info("舵角描画変更イベントを取得しました");
                Viewer.AngleDrawChange(ea.Angle, ea.GetArea);
            };
            m_drawform.AutoDrawChangeEvent += delegate (AutoDrawEventArgs ea)
            {
                LOGGER.Info("自動追尾描画変更イベントを取得しました");
                m_AutoScroll = ea.Auto;
            };

            m_drawform.Visible = true;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion
    }

}
