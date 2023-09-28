using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using RcdDao;
using static RcdDao.MFacilityDao;
using RcdCmn;
using MapViewer;
using static RcdDao.MStationDao;
using static RcdDao.DRcdStatusDao;
using CommWrapper;
using static RcdTablet.clsTabletMsg;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;
using static RcdDao.DCarStatus;
using static RcdTablet.Utils;
using System.Runtime.InteropServices;
namespace RcdTablet
{
    public partial class clsTablet : Form
    {
        #region ### クラス定数 ###
        /// <summary>
        /// 接続状況
        /// </summary>
        private enum StsConn
        {
            /// <summary>
            /// 未接続
            /// </summary>
            DisConnecting = 0,
            /// <summary>
            /// 接続
            /// </summary>
            Connnecting = 1
        };

        /// <summary>
        /// 管制制御部 管制モード
        /// </summary>
        private enum ManagerMode
        {
            /// <summary>
            /// 各個
            /// </summary>
            Manual,

            /// <summary>
            /// 連続
            /// </summary>
            Auto,

            /// <summary>
            /// 不明
            /// </summary>
            Err
        }

        private enum ManagerStatus
        {            
            /// <summary>
            /// 原位置復帰前
            /// </summary>
            WaitOPBotton,
            /// <summary>
            /// 原位置復帰後
            /// </summary>
            PushOPBotton
            /// <summary>
            /// 異常処置中
            /// </summary>
            //HundlingError
        }

        private enum btnStatus
        {
            /// <summary>
            /// 運転準備OFF、連続OFF
            /// </summary>
            OFF,
            /// <summary>
            /// 運転準備ON,連続ON
            /// </summary>
            ON
        }

        /// <summary>
        /// シャッターステータス
        /// </summary>
        private enum shutterstatus
        {
            Close = 0,
            Open = 1,
            Working = 2,
            Unknown = 99,
            Error = -1
        }

        private enum TriadSignalstatus
        {
            Blue = 0,
            Red = 1,
            Yellow = 2,
            Unknown = 99,
            Error = -1
        }

        private enum DuoSignalstatus
        {
            Blue = 0,
            Red = 1,
            Lights_out = 3,
            Unknown = 99,
            Error = -1
        }

        private enum elsefacilitystatus
        {
            Negative = 0,
            Positive = 1,
            Unknown = 99,
            Error = -1
        }
        private enum FacilityType { TriadSignal = 0, DuoSignal = 1, Shutter = 2};

        private class C_FACILITY_COL
        {
            internal const string ID = "FacilityID";
            internal const string Name = "FacilityName";
            internal const string Status = "Status1";
            internal const string StatusMsg = "StatusMsg";
            internal const string BtnCtrl0 = "BtnCtrl0";
            internal const string BtnCtrl1 = "BtnCtrl1";
        }

        private const string C_ALLOW_FACILITY = "0";
        private const string C_NOT_CONTROL = "1";

        public const int C_DEFAULT_BTN_WAIT = 1000;
        public readonly int C_FONT_SIZE = 16;
        #endregion

        #region ### config ###


        #endregion

        #region ### クラス変数 ###
        private int formSizeX;
        private int formSizeY;
        private AppLog LOGGER = AppLog.GetInstance();
        private System.Timers.Timer m_statInfoTimer;
        private System.Timers.Timer m_EmerStopTImer;

        private List<FacilityStatusMsg> m_facilityStatusMsgs;

        private List<TouchPoint> touchPoints = new List<TouchPoint>();

        List<MStationDao.Station> m_stationDaoList;

        private SortableBindingList<FacilityStatus_Tablet> m_facilityBindingList = new SortableBindingList<FacilityStatus_Tablet>();
        private SortableBindingList<FacilityStatus_Tablet> m_facilityALBindingList = new SortableBindingList<FacilityStatus_Tablet>();
        private SortableBindingList<FacilityStatus_Tablet> m_facilityNCBindingList = new SortableBindingList<FacilityStatus_Tablet>();
        private SortableBindingList<CarStatus_Tablet> m_carBindingList = new SortableBindingList<CarStatus_Tablet>();
        //private DRcdStatus m_managerMode = new DRcdStatus();
        //private DRcdStatusDao d_RcdStatusDao = new DRcdStatusDao();
        //private MStationDao m_StationDao = new MStationDao();
        private List<DRcdStatus> m_managerMode;
        private Res.CarStatus m_carStatus = new Res.CarStatus();
        private DRcdStatusDao d_RcdStatusDao;
        private MStationDao m_StationDao;
        private MFacilityDao m_FacilityDao;
        public CommCl m_rcdCl;
        private MPlantDao.Plant m_plant;
        private MStationDao.Station m_station;

        private string m_IP_ADDRESS = ConfigurationManager.AppSettings["m_IP_ADDRESS"];
        private int m_CL_PORT = int.Parse(ConfigurationManager.AppSettings["m_CL_PORT"]);
        private bool m_hasErrors = false;
        private bool errorflag = false;

        private string prepath_image = ConfigurationManager.AppSettings["IMAGE_PATH"];

        public int m_STATUS_UPDATE_INTERVAL = int.Parse(ConfigurationManager.AppSettings["STATUS_UPDATE_INTERVAL"]);
        public int m_PLANT_ID = int.Parse(ConfigurationManager.AppSettings["C_PLANT_ID"]);
        public int m_STATION_ID = int.Parse(ConfigurationManager.AppSettings["C_STATION_ID"]);
        public int C_UDP_PORT = int.Parse(ConfigurationManager.AppSettings["C_UDP_PORT"]);

        private CommUdpSrv m_imageSrv;
        public string ImageNo;

        /// <summary>
        /// RCDstatus関連(電文用）
        /// </summary>
        private int m_preparationStatus;
        private int m_onesecStatus;
        private int m_continueStatus;

        private List<ControlList> clist = new List<ControlList>();

        /// <summary>
        /// 確認用
        /// </summary>
        bool confirmed;




        private int WC;
        private int HC;



        #endregion

        #region ### コンストラクター ###
        public clsTablet()
        {
            InitializeComponent();
            ConnectDB();

            //m_managerMode = new DRcdStatus();
            d_RcdStatusDao = new DRcdStatusDao();
            m_StationDao = new MStationDao();
            m_FacilityDao = new MFacilityDao();
            

            m_stationDaoList = m_StationDao.GetSelectInfo(m_PLANT_ID);

            List<string> m_stationNameList = m_stationDaoList.Select(m =>
            {
                return m.Name;
            }
                ).ToList();
            cb_stationlist.DataSource = m_stationNameList;
            m_station = m_stationDaoList.FirstOrDefault(s => s.SID == m_STATION_ID);
            manageViewer2.setStation(m_station);
            SetMapViewer(m_stationDaoList.SingleOrDefault(s => s.SID == m_station.SID).Image);

            userControl11.WpfButtonClick += btn_userControl_Click;
            onesec_btnctrl1.WpfButtonClick += btn_onesec_Click;

            tb_allowControl.TabPages.Add(new TabPage("制御可能"));
            tb_allowControl.TabPages.Add(new TabPage("制御不可"));

            //tp_allowControl.Controls.Add(dgvfacility_allow);
            //tb_allowControl.ItemSize = new Size(80, 120);
            //tp_uncontrol.Controls.Add(dgvFacility_noControl);
            m_imageSrv = new CommUdpSrv(C_UDP_PORT);
            m_imageSrv.ReceiveMessage += new CommUdpSrv.ReceiveMessageEventHandler(IsrvReceiveMessage);
            m_imageSrv.StartListen();

        }
        #endregion

        #region ### フォームイベント ###
        private void clsTablet_Load(object sender, EventArgs e)
        {
            //this.ResizeEnd += clsTablet_ResizeEnd;
            this.SizeChanged += clsTablet_SizeChanged;
            List<Control> controls = GetAllControls<Control>(this);
            foreach (Control ctrl in Controls)
            {
                ControlList cl = new ControlList
                {
                    ControlName = ctrl.Name,
                    ControlSize = ctrl.Size,
                    ControlLoc = ctrl.Location,
                };
                clist.Add(cl);
            }
            formSizeX = this.Size.Width;
            formSizeY = this.Size.Height;

            //elementHost2.Child 
            SubscribeControlEvents();

            int sizex = this.Size.Width;

            btn_preparetion.Enabled = false;
            btn_continue.Enabled = false;

            // OperationPCへ接続
            m_rcdCl = new CommCl(m_IP_ADDRESS, m_CL_PORT);
            m_rcdCl.Connected += OnConnected;
            m_rcdCl.Disconnected += OnDisconnected;
            m_rcdCl.RecieveServerMessage += OnSrvMsgReceived;
            m_rcdCl.Connect();

            m_facilityStatusMsgs = new MFacilityDao().GetFacilityStatusMsgs();

            PrepareDataGridView();

            // 状態更新タイマー設定
            m_statInfoTimer = new System.Timers.Timer
            {
                Interval = m_STATUS_UPDATE_INTERVAL,
                Enabled = false
            };
            m_statInfoTimer.Elapsed += OnStatInfoTimerElapsed;

            m_statInfoTimer.Start(); 

            dgvfacility_allow.Font = new Font(dgvfacility_allow.Font.Name, C_FONT_SIZE);
            //dgvFacility_noControl.Font = new Font(dgvfacility_allow.Font.Name, 16);
            dgvCarStatus.Font = new Font(dgvCarStatus.Font.Name, C_FONT_SIZE);
            SetDoubleBuffer(dgvfacility_allow);
            //SetDoubleBuffer(dgvFacility_noControl);
            SetDoubleBuffer(dgvCarStatus);
        }
        //ビューモード
        private void SetSetting()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                m_STATION_ID = (int)m_stationDaoList.FirstOrDefault(s => s.Name == cb_stationlist.SelectedItem.ToString()).SID;
                m_station = m_stationDaoList.FirstOrDefault(s => s.SID == m_STATION_ID);
                manageViewer2.setStation(m_station);
                SetMapViewer(m_stationDaoList.SingleOrDefault(s => s.SID == m_station.SID).Image);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void SetMapViewer(string imagepath)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                manageViewer2.FirstSet(LOGGER, prepath_image + "/" + imagepath);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        //private void readSetting()
        //{
        //   m_plant.SID = int.Parse(ConfigurationManager.AppSettings["C_PLANT_ID"]);
        //    m_StationDao.m
        //}
        private void btn_preparetion_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (m_ope_flag == StsConn.DisConnecting)
                {
                    MessageBox.Show("OperationPCが接続されていません。", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                    return;
                }

                if(m_managerMode.Count() != 1)
                {
                    throw new Exception("該当ステータスが複数存在します。");
                }

                DRcdStatus managestatus = m_managerMode.FirstOrDefault();

                if (managestatus.PreoarationStatus == (int)btnStatus.OFF)
                {
                    m_preparationStatus = (int)btnStatus.ON;
                    confirmed = PopUpCheck($"運転準備をONにしますか?", "運転準備指示");
                }                
                else
                {
                    m_preparationStatus = (int)btnStatus.OFF;
                    confirmed = PopUpCheck($"運転準備をOFFにしますか?", "運転準備指示");
                }

                if(confirmed)
                {
                    PreparetionReqSndMsg sndMsg = new PreparetionReqSndMsg()
                    {
                        Preparetionstatus = m_preparationStatus
                    };
                    string res = m_rcdCl.SendMessage(sndMsg, true);
                    btn_preparetion.BackgroundImage = RcdTablet.Properties.Resources.運転準備_ON;
                } 
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_onesec_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (m_ope_flag == StsConn.DisConnecting)
                {
                    MessageBox.Show("OperationPCが接続されていません。", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                    return;
                }

                if (m_managerMode.Count() != 1)
                {
                    throw new Exception("該当ステータスが複数存在します。");
                }

                DRcdStatus managestatus = m_managerMode.FirstOrDefault();

                if(managestatus.PreoarationStatus != (int)ManagerMode.Auto)
                {
                    MessageBox.Show("運転準備がOFFです", "注意", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (managestatus.ControlStatus == (int)ManagerMode.Manual)
                {
                    m_onesecStatus = (int)ManagerMode.Auto;
                    confirmed = PopUpCheck($"各個から連続に変更しますか?", "各個連続指示");
                }
                else
                {
                    m_onesecStatus = (int)ManagerMode.Manual;
                    confirmed = PopUpCheck($"連続から各個に変更しますか?", "各個連続指示");
                }
                if(confirmed)
                {
                    MngModeChangeSnd sndMsg = new MngModeChangeSnd()
                    {
                        MngModeStatus = m_onesecStatus
                    };
                    string res = m_rcdCl.SendMessage(sndMsg, true);
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_Continue_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (m_ope_flag == StsConn.DisConnecting)
                {
                    MessageBox.Show("OperationPCが接続されていません。", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                    return;
                }

                if (m_managerMode.Count() != 1)
                {
                    throw new Exception("該当ステータスが複数存在します。");
                }

                DRcdStatus managestatus = m_managerMode.FirstOrDefault();

                if (managestatus.ContinueStatus == (int)btnStatus.OFF)
                {
                    m_continueStatus = (int)btnStatus.ON;
                    confirmed = PopUpCheck($"連続をONにしますか?", "連続指示");
                }
                else
                {
                    m_continueStatus = (int)btnStatus.OFF;
                    confirmed = PopUpCheck($"連続をOFFにしますか?", "連続指示");
                }

                if(confirmed)
                {
                    ContinueChangeSnd sndMsg = new ContinueChangeSnd()
                    {
                        ContinueStatus = m_continueStatus
                    };
                    string res = m_rcdCl.SendMessage(sndMsg, true);
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_CarOffAndon_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                List<DEmergencyDao.ErrorStatus> ErrStat = new DEmergencyDao().GetAllErrorStatus_Solve(m_PLANT_ID, m_STATION_ID);
                if (ErrStat.Count() == 0)
                {
                    LOGGER.Info($"異常解除ボタン押下: エラーステータスでないため、処理終了。");
                    return;
                }

                LOGGER.Info($"車両異常解除ボタン押下: ボデーNo: {ErrStat[0].BodyNo}, 車両ステータス: {m_carStatus.GetMsg(ErrStat[0].StatusMsg)}");
                CarOffAndonSndMsg msg = new CarOffAndonSndMsg();
                SendTcp(msg);

                // 連打防止待ち
                Task.Run(() =>
                {
                    PreventRepeatBtn(btn_CarResolveSndMsg, C_DEFAULT_BTN_WAIT);
                });                
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_OriPosi_Click(object sender, EventArgs e)
        {
            InitFacilitySnd msg = new InitFacilitySnd();
            LOGGER.Info($"原位置復帰指示送信");
            SendTcp(msg);
            // 連打防止待ち
            Task.Run(() =>
            {
                PreventRepeatBtn(btn_OriPosi, C_DEFAULT_BTN_WAIT);
            });
        }

        ///// <summary>
        /////　走行履歴
        ///// </summary>
        //private void btn_DrivingHistory_Click(object sender ,EventArgs e)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        using (DrivingHistory formDriHis = new DrivingHistory(this))
        //        {
        //            formDriHis.Tag = this;
        //            formDriHis.ShowDialog(this);
        //        }

        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        ///// <summary>
        /////　イベント履歴
        ///// </summary>
        //private void btn_EventHistory_Click(object sender, EventArgs e)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        using (EventHistory formEveHis = new EventHistory(this))
        //        {
        //            formEveHis.Tag = this;
        //            formEveHis.ShowDialog(this);
        //        }

        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}


        /// <summary>
        /// ボタン連打防止
        /// </summary>
        /// <param name="key">ボタンキー</param>
        /// <param name="waitTime">待ち時間</param>
        private void PreventRepeatBtn(Control ctrl, int waitTime)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                //コントロールのプロパティを変更できるか？
                ctrl.Enabled = false;
                Stopwatch btnWait = new Stopwatch();
                btnWait.Start();
                do
                {

                }
                while (btnWait.ElapsedMilliseconds < waitTime);

                ctrl.Enabled = true;
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btnCarResolveSndMsg_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                carResolveSndMsg();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        public void carResolveSndMsg()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                List<DEmergencyDao.ErrorStatus> ErrStat = new DEmergencyDao().GetAllErrorStatus_Solve(m_PLANT_ID, m_STATION_ID);
                if (ErrStat.Count() == 0)
                {
                    LOGGER.Info($"異常解除ボタン押下: エラーステータスでないため、処理終了。");
                    return;
                }               

                LOGGER.Info($"車両異常解除ボタン押下: ボデーNo: {ErrStat[0].BodyNo}, 車両ステータス: {m_carStatus.GetMsg(ErrStat[0].StatusMsg)}");
                CarResolveSndMsg msg = new CarResolveSndMsg();
                SendTcp(msg);

                // 連打防止待ち
                Task.Run(() =>
                {
                    PreventRepeatBtn(btn_CarResolveSndMsg, C_DEFAULT_BTN_WAIT);
                });
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btnerrorDetect_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (!m_hasErrors)
                {
                    return;
                }

                using (ErrorDetect formErrorsDetect = new ErrorDetect(this))
                {
                    formErrorsDetect.Tag = this;
                    formErrorsDetect.ShowDialog(this);
                }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btnOriPosi_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                //bool confirmed = PopUpCheck($"設備原位置復帰指示を送信しますか?\n工程:{ctrl.UnitID}", "原位置復帰指示");

                InitFacilitySnd msg = new InitFacilitySnd();                
                
                //LOGGER.Info($"原位置復帰指示送信 経路生成部ID:{ctrl.UnitID}");
                SendTcp(msg);


            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btnDrivingHistory_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                using (DrivingHistory formDriHis = new DrivingHistory(this))
                {
                    formDriHis.Tag = this;
                    formDriHis.ShowDialog(this);
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btnEventHistory_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                using (EventHistory formEveHis = new EventHistory(this))
                {
                    formEveHis.Tag = this;
                    formEveHis.ShowDialog(this);
                }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_errorDetect_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if(!m_hasErrors)
                {
                    return;
                }

                using (ErrorDetect ErrorDetectForm = new ErrorDetect(this))
                {
                    ErrorDetectForm.Tag = this;
                    ErrorDetectForm.ShowDialog(this);
                    errorflag = true;
                    
                    manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        //警告ボタン
        private void btn_warning_Click(object sender, EventArgs e)
        {
            using (FormWarninig FormWarninig = new FormWarninig(this))
            {
                FormWarninig.Tag = this;
                FormWarninig.ShowDialog(this);
            }
        }

        /// <summary>
        /// コントロールのイベントメソッドを設定
        /// </summary>
        private void SubscribeControlEvents()
        {
            //pMapViewerWrap.Paint += DrawBorder;

            dgvfacility_allow.CellPainting += dgvfacility_CellPainting;
            dgvfacility_allow.SelectionChanged += ClearDgvSelection;
            dgvfacility_allow.CellClick += dgvfacility_CellClick;

            //dgvFacility_noControl.CellPainting += dgvfacility_NoControl_CellPainting;
            //dgvFacility_noControl.SelectionChanged += ClearDgvSelection;
            //dgvFacility_noControl.CellClick += dgvfacility_CellClick;

            //dgvShutter.CellPainting += dgvShutter_CellPainting;
            //dgvSignal.CellPainting += dgvSignal_CellPainting;
            //dgvElseFacility.CellPainting += dgvElseFacility_CellPainting;

            //dgvCtrlStatus.SelectionChanged += ClearDgvSelection;
            dgvCarStatus.SelectionChanged += ClearDgvSelection;
            dgvCarStatus.CellPainting += dgvCarStatus_CellPainting;
            //dgvShutter.SelectionChanged += ClearDgvSelection;
            //dgvSignal.SelectionChanged += ClearDgvSelection;
            //dgvElseFacility.SelectionChanged += ClearDgvSelection;

            //dgvShutter.CellClick += dgvFacility_CellClick;
            //dgvSignal.CellClick += dgvFacility_CellClick;
            //dgvElseFacility.CellClick += dgvFacility_CellClick;

            //dgvCarStatus.CellClick += dgvCarStatus_CellClick;
            //dgvCtrlStatus.CellClick += dgvCtrlStatus_CellClick;

            //pStsmCl.MouseHover += ShowConnToolTip;
            //pStsmCl.MouseLeave += HideConnToolTip;
        }

        /// <summary>
        /// データグリッドビュー選択を無効化
        /// </summary>
        private void ClearDgvSelection(object sender, EventArgs e)
        {
            // 表示一覧は選択なしにする
            ((DataGridView)sender).ClearSelection();
        }

        private void dgvCarStatus_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                {
                    return;
                }
                CarStatus_Tablet car = (CarStatus_Tablet)m_carBindingList[e.RowIndex];
                int carType = car.CarType;
                if (e.ColumnIndex == dgvfacility_allow.Columns["StatusMsg"].Index)
                {
                    if (carType == 0)
                    {
                        if (car.SystemStatus == Res.PreVehicleStatus.Preparation.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.PreVehicleStatus.Preparation.Msg;
                        }
                        else if (car.SystemStatus == Res.PreVehicleStatus.Moving.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.PreVehicleStatus.Moving.Msg;
                        }
                        else if (car.SystemStatus == Res.PreVehicleStatus.WaitReadyBoot.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.PreVehicleStatus.WaitReadyBoot.Msg;
                        }
                        else if (car.SystemStatus == Res.PreVehicleStatus.WaitStartup.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.PreVehicleStatus.WaitStartup.Msg;
                        }
                        else if (car.SystemStatus == Res.PreVehicleStatus.WaitFrontVehicle.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.PreVehicleStatus.WaitFrontVehicle.Msg;
                        }
                        else if (car.SystemStatus == Res.PreVehicleStatus.ErrorEnd.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.PreVehicleStatus.ErrorEnd.Msg;
                        }
                    }
                    else if (carType == 1)
                    {
                        if (car.SystemStatus == Res.CarStatus.IDLE.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.IDLE.Msg;
                        }
                        else if (car.SystemStatus == Res.CarStatus.RUNNING.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.RUNNING.Msg;
                        }
                        else if (car.SystemStatus == Res.CarStatus.PAUSED.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.PAUSED.Msg;
                        }
                        else if (car.SystemStatus == Res.CarStatus.STOPPED.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.STOPPED.Msg;
                        }
                        else if (car.SystemStatus == Res.CarStatus.STOPPING.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.STOPPING.Msg;
                        }
                        else if (car.SystemStatus == Res.CarStatus.RESOLVING.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.RESOLVING.Msg;
                        }
                        else if (car.SystemStatus == Res.CarStatus.HANDOVER_CHECKING.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.HANDOVER_CHECKING.Msg;
                        }
                        else if (car.SystemStatus == Res.CarStatus.END_IDLE.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.END_IDLE.Msg;
                        }
                        else if (car.SystemStatus == Res.CarStatus.ERROR.Code)
                        {
                            dgvCarStatus.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Res.CarStatus.ERROR.Msg;
                        }
                    }

                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }


        /// <summary>
        /// 設備一覧再表示
        /// </summary>
        private void dgvfacility_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                {
                    return;
                }
                if(tb_allowControl.SelectedIndex == int.Parse(C_ALLOW_FACILITY))
                {
                    m_facilityBindingList = m_facilityALBindingList;
                }
                else if(tb_allowControl.SelectedIndex == int.Parse(C_NOT_CONTROL))
                {
                    m_facilityBindingList = m_facilityNCBindingList;
                }
                
                FacilityStatus_Tablet fac = (FacilityStatus_Tablet)m_facilityBindingList[e.RowIndex];                
                int facType = fac.FacilityTypeID;
                if (e.ColumnIndex == dgvfacility_allow.Columns[C_FACILITY_COL.BtnCtrl0].Index)
                {
                    DataGridViewDisableButtonCell dgvscell = (DataGridViewDisableButtonCell)dgvfacility_allow.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    int facilityType_ID = m_facilityBindingList.Where(f => f.FacilityID == (string)dgvfacility_allow.Rows[e.RowIndex].Cells[0].Value).SingleOrDefault().FacilityTypeID;
                    int facilityCtrlVal = m_facilityBindingList.Where(f => f.FacilityID == (string)dgvfacility_allow.Rows[e.RowIndex].Cells[0].Value).SingleOrDefault().Status;

                    if (facType == (int)FacilityType.Shutter)
                    {
                        if (fac.Status == (int)shutterstatus.Close)
                        {
                            dgvscell.Enabled = true;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_close_noframe);
                            dgvscell.FlatStyle = FlatStyle.Standard;
                            dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)shutterstatus.Open)
                        {
                            dgvscell.Enabled = true;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_open_noframe);

                            dgvscell.FlatStyle = FlatStyle.Standard;
                            dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)shutterstatus.Working)
                        {
                            //最初から動作中
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_working_noB);
                            dgvscell.FlatStyle = FlatStyle.Popup;
                            dgvscell.Style.BackColor = Color.White;
                            dgvscell.Enabled = false;
                        }
                        else if (fac.Status == (int)shutterstatus.Unknown)
                        {
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_unknown);
                            dgvscell.Style.BackColor = Color.White;
                            dgvscell.Enabled = false;
                        }
                        else if (fac.Status == (int)shutterstatus.Error)
                        {
                            dgvscell.Enabled = false;
                            dgvscell.Style.BackColor = Color.Red;
                        }
                    }
                    else if (facType == (int)FacilityType.TriadSignal)
                    {
                        if (fac.Status == (int)TriadSignalstatus.Blue)
                        {
                            dgvscell.Enabled = true;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_carsig_blue_up);
                            dgvscell.FlatStyle = FlatStyle.Standard;
                            dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)TriadSignalstatus.Yellow)
                        {
                            dgvscell.Enabled = false;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_carsig_yellow_up);
                            dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)TriadSignalstatus.Red)
                        {
                            dgvscell.Enabled = true;
                            dgvscell.FlatStyle = FlatStyle.Standard;
                            dgvscell.Style.BackColor = Color.White;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_carsig_red_up);
                        }
                        else if (fac.Status == (int)TriadSignalstatus.Unknown)
                        {
                            dgvscell.Enabled = false;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_carsig_no_light);
                            dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)TriadSignalstatus.Error)
                        {
                            dgvscell.Enabled = false;
                            dgvscell.Style.BackColor = Color.Red;
                        }
                    }
                    else if (facType == (int)FacilityType.DuoSignal)
                    {
                        if (fac.Status == (int)DuoSignalstatus.Blue)
                        {
                            dgvscell.Enabled = true;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_wakersig_blue_up);
                            dgvscell.FlatStyle = FlatStyle.Standard;
                            dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)DuoSignalstatus.Lights_out)
                        {                            
                                dgvscell.Enabled = false;
                                DrawImageDgvCell(e, 10, Properties.Resources.bt_wakersig_nolight);
                                dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)DuoSignalstatus.Red)
                        {
                            dgvscell.Enabled = true;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_wakersig_red_up);
                            dgvscell.FlatStyle = FlatStyle.Standard;
                            dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)DuoSignalstatus.Unknown)
                        {
                            dgvscell.Enabled = false;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_wakersig_nolight);
                            dgvscell.Style.BackColor = Color.White;
                        }
                        else if (fac.Status == (int)DuoSignalstatus.Error)
                        {
                            dgvscell.Enabled = false;
                            dgvscell.Style.BackColor = Color.Red;
                        }
                    }
                    else
                    {
                        //OFF or 消灯 or 各個
                        if (fac.Status == (int)elsefacilitystatus.Negative)
                        {
                            dgvscell.Enabled = true;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_lamp_off);

                            dgvscell.FlatStyle = FlatStyle.Standard;
                            dgvscell.Style.BackColor = Color.White;
                        }
                        //ON or 点灯 or 連続
                        else if (fac.Status == (int)elsefacilitystatus.Positive)
                        {
                            dgvscell.Enabled = true;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_lamp_onright);
                            dgvscell.FlatStyle = FlatStyle.Standard;
                            dgvscell.Style.BackColor = Color.White;
                            //btnCell.Value = "ON";
                        }
                        //不明
                        else if (fac.Status == (int)elsefacilitystatus.Unknown)
                        {
                            dgvscell.Enabled = false;
                            DrawImageDgvCell(e, 10, Properties.Resources.bt_unknown);
                            dgvscell.Style.BackColor = Color.White;
                        }
                        //エラー
                        else if (fac.Status == (int)elsefacilitystatus.Error)
                        {
                            dgvscell.Enabled = false;
                            dgvscell.Style.BackColor = Color.Red;
                        }

                    }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        //private void dgvfacility_NoControl_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        //        {
        //            return;
        //        }
        //        FacilityStatus_Tablet fac = (FacilityStatus_Tablet)m_facilityNCBindingList[e.RowIndex];
        //        int facType = fac.FacilityTypeID;
        //        if (e.ColumnIndex == dgvFacility_noControl.Columns[C_FACILITY_COL.BtnCtrl0].Index)
        //        {
        //            DataGridViewDisableButtonCell dgvscell = (DataGridViewDisableButtonCell)dgvFacility_noControl.Rows[e.RowIndex].Cells[e.ColumnIndex];
        //            int facilityType_ID = m_facilityNCBindingList.Where(f => f.FacilityID == (string)dgvFacility_noControl.Rows[e.RowIndex].Cells[0].Value).SingleOrDefault().FacilityTypeID;
        //            int facilityCtrlVal = m_facilityNCBindingList.Where(f => f.FacilityID == (string)dgvFacility_noControl.Rows[e.RowIndex].Cells[0].Value).SingleOrDefault().Status;

        //            if (facType == (int)FacilityType.Shutter)
        //            {
        //                if (fac.Status == (int)shutterstatus.Close)
        //                {
        //                    dgvscell.Enabled = true;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_close_noframe);
        //                    dgvscell.FlatStyle = FlatStyle.Standard;
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)shutterstatus.Open)
        //                {
        //                    dgvscell.Enabled = true;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_open_noframe);

        //                    dgvscell.FlatStyle = FlatStyle.Standard;
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)shutterstatus.Working)
        //                {
        //                    //最初から動作中
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_working_noB);
        //                    dgvscell.FlatStyle = FlatStyle.Popup;
        //                    dgvscell.Style.BackColor = Color.White;
        //                    dgvscell.Enabled = false;
        //                }
        //                else if (fac.Status == (int)shutterstatus.Unknown)
        //                {
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_unknown);
        //                    dgvscell.Style.BackColor = Color.White;
        //                    dgvscell.Enabled = false;
        //                }
        //                else if (fac.Status == (int)shutterstatus.Error)
        //                {
        //                    dgvscell.Enabled = false;
        //                    dgvscell.Style.BackColor = Color.Red;
        //                }
        //            }
        //            else if (facType == (int)FacilityType.TriadSignal)
        //            {
        //                if (fac.Status == (int)TriadSignalstatus.Blue)
        //                {
        //                    dgvscell.Enabled = true;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_carsig_blue_up);
        //                    dgvscell.FlatStyle = FlatStyle.Standard;
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)TriadSignalstatus.Yellow)
        //                {
        //                    dgvscell.Enabled = false;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_carsig_yellow_up);
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)TriadSignalstatus.Red)
        //                {
        //                    dgvscell.Enabled = true;
        //                    dgvscell.FlatStyle = FlatStyle.Standard;
        //                    dgvscell.Style.BackColor = Color.White;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_carsig_red_up);
        //                }
        //                else if (fac.Status == (int)TriadSignalstatus.Unknown)
        //                {
        //                    dgvscell.Enabled = false;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_carsig_no_light);
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)TriadSignalstatus.Error)
        //                {
        //                    dgvscell.Enabled = false;
        //                    dgvscell.Style.BackColor = Color.Red;
        //                }
        //            }
        //            else if (facType == (int)FacilityType.DuoSignal)
        //            {
        //                if (fac.Status == (int)DuoSignalstatus.Blue)
        //                {
        //                    dgvscell.Enabled = true;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_wakersig_blue_up);
        //                    dgvscell.FlatStyle = FlatStyle.Standard;
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)DuoSignalstatus.Lights_out)
        //                {
        //                    dgvscell.Enabled = false;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_wakersig_nolight);
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)DuoSignalstatus.Red)
        //                {
        //                    dgvscell.Enabled = true;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_wakersig_red_up);
        //                    dgvscell.FlatStyle = FlatStyle.Standard;
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)DuoSignalstatus.Unknown)
        //                {
        //                    dgvscell.Enabled = false;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_wakersig_nolight);
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                else if (fac.Status == (int)DuoSignalstatus.Error)
        //                {
        //                    dgvscell.Enabled = false;
        //                    dgvscell.Style.BackColor = Color.Red;
        //                }
        //            }
        //            else
        //            {
        //                //OFF or 消灯 or 各個
        //                if (fac.Status == (int)elsefacilitystatus.Negative)
        //                {
        //                    dgvscell.Enabled = true;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_lamp_off);

        //                    dgvscell.FlatStyle = FlatStyle.Standard;
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                //ON or 点灯 or 連続
        //                else if (fac.Status == (int)elsefacilitystatus.Positive)
        //                {
        //                    dgvscell.Enabled = true;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_lamp_onright);
        //                    dgvscell.FlatStyle = FlatStyle.Standard;
        //                    dgvscell.Style.BackColor = Color.White;
        //                    //btnCell.Value = "ON";
        //                }
        //                //不明
        //                else if (fac.Status == (int)elsefacilitystatus.Unknown)
        //                {
        //                    dgvscell.Enabled = false;
        //                    DrawImageDgvCell(e, 10, Properties.Resources.bt_unknown);
        //                    dgvscell.Style.BackColor = Color.White;
        //                }
        //                //エラー
        //                else if (fac.Status == (int)elsefacilitystatus.Error)
        //                {
        //                    dgvscell.Enabled = false;
        //                    dgvscell.Style.BackColor = Color.Red;
        //                }

        //            }
        //        }
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        /// <summary>
        /// DGV設備Cellクリック時
        /// </summary>
        private void dgvfacility_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                DataGridView senderDgv = (DataGridView)sender;
                int colIndex = e.ColumnIndex;
                int rowIndex = e.RowIndex;

                if (rowIndex < 0) return;

                bool btn0clicked = senderDgv.Columns.Contains(C_FACILITY_COL.BtnCtrl0)
                    && colIndex == senderDgv.Columns[C_FACILITY_COL.BtnCtrl0].Index;

                bool btn1clicked = senderDgv.Columns.Contains(C_FACILITY_COL.BtnCtrl1)
                    && colIndex == senderDgv.Columns[C_FACILITY_COL.BtnCtrl1].Index;


                if (!btn0clicked && !btn1clicked)
                {
                    return;
                }

                DataGridViewDisableButtonCell btnCell = (DataGridViewDisableButtonCell)dgvfacility_allow.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (btnCell.Enabled == false)
                {
                    LOGGER.Debug($"非活性設備制御ボタン押下: 処理終了");
                    return;
                }

                FacilityStatus_Tablet fac = (FacilityStatus_Tablet)m_facilityBindingList[e.RowIndex];

                SortableBindingList<FacilityStatus_Tablet> bindings = (SortableBindingList<FacilityStatus_Tablet>)senderDgv.DataSource;
                string facilityName = bindings[e.RowIndex].FacilityName;
                int facilityType_ID = bindings[e.RowIndex].FacilityTypeID;
                if (fac.FacilityTypeID != (int)FacilityType.TriadSignal && fac.FacilityTypeID != (int)FacilityType.DuoSignal && fac.FacilityTypeID != (int)FacilityType.Shutter)
                {
                    string strReadOnly = "周辺設備　動作確認";
                    string strFacMsg = $"{facilityName}は操作できません";
                    DialogResult resultTypeID = MessageBox.Show(strFacMsg, strReadOnly, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    manageViewer1.onfacilityCoordinate = new Facility_Coordinate();
                    return;
                }

                //if (m_managerMode != ManagerMode.Manual)
                //{
                //    MessageBox.Show("管制制御部が各個制御中でないため、制御できません。", "設備制御不可", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    manageViewer1.onfacilityCoordinate = new Facility_Coordinate();
                //    return;
                //}

                if (!(m_managerMode[0].PreoarationStatus == (int)btnStatus.ON && m_managerMode[0].ControlStatus == (int)ManagerMode.Manual && m_managerMode[0].ContinueStatus == (int)btnStatus.OFF))
                {
                    MessageBox.Show("管制制御部が各個制御中でないため、制御できません。", "設備制御不可", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //manageViewer1.onfacilityCoordinate = new Facility_Coordinate();
                    return;
                }


                string faclityID = bindings[e.RowIndex].FacilityID;
                int facilityStatus = bindings[e.RowIndex].Status;
                //int facilityCtrlVal = facilityStatus.Equals("赤") || facilityMsg.Equals("開") || facilityMsg.Equals("ON") || facilityMsg.Equals("点灯") ? 0 : 1;
                int facilityCtrlVal = facilityStatus == 1 ? 0 : 1;
                string ctrlValName = "";

                FacilityStatusMsg statMsg = m_facilityStatusMsgs
                        .Where(fStatus => fStatus.FacilityTypeSID == facilityType_ID)
                        .Where(fStatus => fStatus.StatusCode == facilityCtrlVal)
                        .SingleOrDefault();
                
                ctrlValName = statMsg == null ? $"制御値:{facilityCtrlVal}" : statMsg.StatusMsg;
                if (ctrlValName == null)
                {
                    ctrlValName = $"制御値:{facilityCtrlVal}";
                }

                LOGGER.Info($"設備制御ボタン押下: 設備ID: {faclityID}, 制御値({facilityCtrlVal})");

                //動作確認
                string strTile = "周辺設備　動作確認";
                string strMsg = $"{facilityName}を {ctrlValName}にしますか？";
                DialogResult result = MessageBox.Show(strMsg, strTile, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                if (result == DialogResult.Cancel)
                {
                    LOGGER.Info($"設備制御ボタン押下: 設備ID: {faclityID}, 制御値({facilityCtrlVal}):キャンセルが押下されました");
                    manageViewer1.onfacilityCoordinate = new Facility_Coordinate();
                    return;
                }

                FacCtrlSndMsg msg = new FacCtrlSndMsg();
                msg.facID = faclityID;
                msg.ctrlVal = facilityCtrlVal;
                SendTcp<FacCtrlSndMsg>(msg);

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void clsTablet_FormClosed(object sender, FormClosedEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                //SaveSplitPosition();

                Controls.Clear(true);
                manageViewer2.Dispose();
                //btnErrorDetect.Dispose();
                //btnExit.Dispose();
                //btnMasterManage.Dispose();

                if (m_statInfoTimer != null)
                {
                    m_statInfoTimer.Stop();
                    m_statInfoTimer.Dispose();
                }

                if (m_rcdCl != null)
                {
                    // 管制サービス接続のイベント削除
                    m_rcdCl.Connected -= OnConnected;
                    m_rcdCl.Disconnected -= OnDisconnected;
                    m_rcdCl.RecieveServerMessage -= OnSrvMsgReceived;

                    if (m_rcdCl.IsConnected)
                    {
                        m_rcdCl.Disconnect(true);
                    }
                    else
                    {
                        m_rcdCl.ForceStop();
                    }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }


        #endregion

        #region ### クラスイベント ###
        /// <summary>
        /// 管制サーバー接続時
        /// </summary>
        private void OnConnected(ClientConnectedEventArgs e)
        {
            LOGGER.Info($"OperationPC 接続完了");
            try
            {
                //管制制御部 接続状態表示:接続中
                SetStsmCl(StsConn.Connnecting);

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void OnDisconnected(ClientConnectedEventArgs e)
        {
            LOGGER.Info($"OperationPC disconnected");

            try
            {
                //管制制御部 接続状態表示:切断中
                SetStsmCl(StsConn.DisConnecting);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 管制制御部よりTcpメッセージ受信時
        /// </summary>
        private void OnSrvMsgReceived(RecieveServerMessageEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                string rawMsg = e.Message;

                string rcvid = CommMsgBase.GetMsgID(e.Message);

                //switch (rcvid)
                //{
                //    case:
                //}
                    //LOGGER.Info(GetMsgRcvLog(Res.C_MNG_SRV, rawMsg));

                    //string msgId = CtrlManageMsgBase.GetMsgID(rawMsg);
                    //MsgCase msgCase = m_msgs.GetMsgCase(msgId);

                    //switch (msgCase)
                    //{
                    //}
                }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 管制制御部情報Timerコールバック設定
        /// </summary>
        private void OnStatInfoTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //UpdateStatusInfo();
            Invoke(new Action(() =>
            {
                UpdateStatusInfo();
            }));
        }

        private void OnEmarStopTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                emergencyStop();
            }));
        }

        #endregion

        #region ### 状態表示更新処理 ###


        #endregion

        #region ### 処理 ###
        /// <summary>
        /// データベース接続初期化
        /// </summary>
        private void ConnectDB()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                string C_IP = ConfigurationManager.AppSettings["DB_IP"];
                string C_DB = ConfigurationManager.AppSettings["DB_NAME"];
                string C_USER = ConfigurationManager.AppSettings["DB_USER"];
                string C_PASSWORD = ConfigurationManager.AppSettings["DB_PASSWORD"];
                bool connected = RcdDao.DaoCommon.Initialize(C_IP, C_DB, C_USER, C_PASSWORD);
                if (!connected)
                {
                    DialogResult res = MessageBox.Show("データベースに接続できません。設定ファイルを修正し再起動してください。");
                    manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                    Close();
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }


        /// <summary>
        /// TCPメッセージ送信
        /// </summary>
        public void SendTcp<T>(T sndMsg) where T : CtrlTabletSndMsgBase
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (!m_rcdCl.IsConnected)
                {
                    string errTarget = $"{GetDisplayName(sndMsg)}送信失敗";
                    string errMsg = $"管制制御部に接続されていないため、送信できません";
                    LOGGER.Warn($"[{errTarget}] {errMsg}");
                    MessageBox.Show(errMsg, errTarget, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                    return;
                }

                LOGGER.Info(GetMsgSndLog(Res.C_MNG_SRV, sndMsg));
                m_rcdCl.SendMessage(sndMsg, false);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private bool PopUpCheck(string checkMsg, string title = null)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                DialogResult res = MessageBox.Show(
                    checkMsg
                    , title == null ? "確認" : title
                    , MessageBoxButtons.YesNo
                    , MessageBoxIcon.Question);
                manageViewer2.onfacilityCoordinate = new Facility_Coordinate();

                if (res == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }

            return false;
        }

        #endregion

        /// <summary>
        /// OperationPC 接続状態
        /// </summary>
        private StsConn m_ope_flag = StsConn.DisConnecting;

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void clsTablet_SizeChanged(object sender, EventArgs e)
        {

            double nextformSizeX = (double)this.Size.Width / formSizeX;
            double nextformSizeY = (double)this.Size.Height / formSizeY;

            List<Control> controls = GetAllControls<Control>(this);
            //List<Control> panalcontrol = GetAllControls<Control>(panel1);
            foreach (Control ctrl in Controls)
            {
                ControlList cl = clist.SingleOrDefault(c => c.ControlName == ctrl.Name);

                Size controlSize = ctrl.Size;
                ctrl.Size = new Size((int)(cl.ControlSize.Width * nextformSizeX), (int)(cl.ControlSize.Height * nextformSizeY));
                Point ctrlLoc = ctrl.Location;
                ctrlLoc.X = (int)(cl.ControlLoc.X * nextformSizeX);
                ctrlLoc.Y = (int)(cl.ControlLoc.Y * nextformSizeY);
                ctrl.Location = ctrlLoc;
            }
            //formSizeX = this.Size.Width;
            //formSizeY = this.Size.Height;
        }

        private void clsTablet_ResizeEnd(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            //コントロールをすべて抽出する
            // コントロール全てを列挙
            //int WH = HC + WC;

            double nextformSizeX = (double)this.Size.Width / formSizeX;
            double nextformSizeY = (double)this.Size.Height / formSizeY;

            List<Control> controls = GetAllControls<Control>(this);
            foreach (Control ctrl in Controls)
            {
                ControlList cl = clist.SingleOrDefault(c => c.ControlName == ctrl.Name);

                //Size controlSize = ctrl.Size;
                ctrl.Size = new Size((int)(cl.ControlSize.Width * nextformSizeX), (int)(cl.ControlSize.Height * nextformSizeY));
                Point ctrlLoc = ctrl.Location;
                ctrlLoc.X = (int)(cl.ControlLoc.X * nextformSizeX);
                ctrlLoc.Y = (int)(cl.ControlLoc.Y * nextformSizeY);
            }
        }

        /// <summary>
        /// 指定のコントロール上の全てのジェネリック型 Tコントロールを取得する。
        /// </summary>
        /// <typeparam name="T">対象となる型</typeparam>
        /// <param name="top">指定のコントロール</param>
        /// <returns>指定のコントロール上の全てのジェネリック型 Tコントロールのインスタンス</returns>
        public static List<T> GetAllControls<T>(Control top) where T : Control
        {

            List<T> buf = new List<T>();
            foreach (Control ctrl in top.Controls)
            {
                if (ctrl is T) buf.Add((T)ctrl);
                buf.AddRange(GetAllControls<T>(ctrl));
            }
            return buf;
        }

        #region ## 状態表示更新処理 ##

        /// <summary>
        /// ステータス表示情報をすべて更新
        /// </summary>
        private void UpdateStatusInfo()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                m_statInfoTimer.Stop();

                //　設備状態情報取得
                List<FacilityStatus_Tablet> facilityStatusList = new MFacilityDao().GetAllFacilityStatus_Tablet(m_PLANT_ID,m_STATION_ID);
                List<FacilityStatus_Tablet> facilityStatusAllowList = new MFacilityDao().GetAllFacilityStatus_Tablet_TYPE(m_PLANT_ID, m_STATION_ID);
                List<FacilityStatus_Tablet> facilityStatusNCList = new MFacilityDao().GetAllFacilityStatus_Tablet_NC(m_PLANT_ID, m_STATION_ID);

                //#if DEBUG
                //                facilityStatusList.Add(new FacilityStatus_Tablet(1,"1",0,1,"3灯信号機",1,"赤",1,1,"ステータスオプション"));

                //#else
                //                List<FacilityStatus_Tablet> facilityStatusList = new MFacilityDao().GetAllFacilityStatus_Tablet(m_PLANT_ID,m_STATION_ID);

                //#endif

                if (tb_allowControl.SelectedIndex == 0)
                {
                    foreach (FacilityStatus_Tablet newStatus in facilityStatusList)
                    {
                        FacilityStatus_Tablet matchedStatus = m_facilityALBindingList.SingleOrDefault(s => s.FacilityID == newStatus.FacilityID);
                        if (matchedStatus != null)
                        {
                            matchedStatus.UpdatedStatus(newStatus);
                        }
                        else
                        {
                            if (newStatus.FacilityTypeID == (int)FacilityType.DuoSignal || newStatus.FacilityTypeID == (int)FacilityType.Shutter || newStatus.FacilityTypeID == (int)FacilityType.TriadSignal)
                            {
                                m_facilityALBindingList.Add(newStatus);                                
                            }
                            //m_facilityBindingList.Add(newStatus);
                        }
                    }
                }
                else if(tb_allowControl.SelectedIndex == 1)
                {
                    foreach (FacilityStatus_Tablet newStatus in facilityStatusList)
                    {
                        FacilityStatus_Tablet matchedStatus = m_facilityNCBindingList.SingleOrDefault(s => s.FacilityID == newStatus.FacilityID);
                        if (matchedStatus != null)
                        {
                            matchedStatus.UpdatedStatus(newStatus);
                        }
                        else
                        {
                            if (newStatus.FacilityTypeID != (int)FacilityType.DuoSignal || newStatus.FacilityTypeID != (int)FacilityType.Shutter || newStatus.FacilityTypeID != (int)FacilityType.TriadSignal)
                            {
                                m_facilityNCBindingList.Add(newStatus);
                            }
                            //m_facilityBindingList.Add(newStatus);
                        }
                    }

                }

                

                dgvfacility_allow.Refresh();
                //dgvFacility_noControl.Refresh();

                //設備配置情報取得
                //List<Facility_Coordinate> m_Facility_Coordinate = new MFacilityDao().GetALLFacilityCoordinate((int)m_plant.SID,(int)m_station.SID);
                //this.manageViewer1.setFacilityCoordinate(m_Facility_Coordinate);
                //this.manageViewer1.setFacilityStatus(facilityStatusList);

                // 車両ステータス情報情報取得・表示
                List<CarStatus_Tablet> carStatusList = new DCarStatus().GetAllCarStatus_Tablet(m_PLANT_ID,m_STATION_ID);

                DrawCarStatusList(carStatusList);

                foreach (CarStatus_Tablet CSable in carStatusList)
                {
                    if(CSable.CarType == 0)
                    {
                       if(CSable.SystemStatus.Equals(Res.PreVehicleStatus.Preparation.Code))
                       {
                            CSable.SystemStatus = Res.PreVehicleStatus.Preparation.Msg;
                       }
                       else if (CSable.SystemStatus.Equals(Res.PreVehicleStatus.Moving.Code))
                       {
                            CSable.SystemStatus = Res.PreVehicleStatus.Moving.Msg;
                       }
                        else if (CSable.SystemStatus.Equals(Res.PreVehicleStatus.WaitReadyBoot.Code))
                        {
                            CSable.SystemStatus = Res.PreVehicleStatus.WaitReadyBoot.Msg;
                        }
                        else if (CSable.SystemStatus.Equals(Res.PreVehicleStatus.WaitStartup.Code))
                        {
                            CSable.SystemStatus = Res.PreVehicleStatus.WaitStartup.Msg;
                        }
                        else if (CSable.SystemStatus.Equals(Res.PreVehicleStatus.WaitFrontVehicle.Code))
                        {
                            CSable.SystemStatus = Res.PreVehicleStatus.WaitFrontVehicle.Msg;
                        }
                        else if (CSable.SystemStatus.Equals(Res.PreVehicleStatus.ErrorEnd.Code))
                        {
                            CSable.SystemStatus = Res.PreVehicleStatus.ErrorEnd.Msg;
                        }
                    }
                    if(CSable.CarType == 1)
                    {
                        if (CSable.SystemStatus.Equals(Res.CtrlVehicleStatus.Preparation.Code))
                        {
                            CSable.SystemStatus = Res.CtrlVehicleStatus.Preparation.Msg;
                        }
                        else if (CSable.SystemStatus.Equals(Res.CtrlVehicleStatus.PreMoving.Code))
                        {
                            CSable.SystemStatus = Res.CtrlVehicleStatus.PreMoving.Msg;
                        }
                        else if (CSable.SystemStatus.Equals(Res.CtrlVehicleStatus.Moving.Code))
                        {
                            CSable.SystemStatus = Res.CtrlVehicleStatus.Moving.Msg;
                        }
                        else if (CSable.SystemStatus.Equals(Res.CtrlVehicleStatus.CompleteEnd.Code))
                        {
                            CSable.SystemStatus = Res.CtrlVehicleStatus.CompleteEnd.Msg;
                        }
                        else if (CSable.SystemStatus.Equals(Res.CtrlVehicleStatus.ErrorEnd.Code))
                        {
                            CSable.SystemStatus = Res.CtrlVehicleStatus.ErrorEnd.Msg;
                        }                        
                    }
                }

//#if DEBUG
//                carStatusList.Add(new CarStatus_Tablet("11111", "00032", "0000.0000", "0000.0000", DateTime.Now,"0"));
//#else
//                List<DCarStatus.CarStatus_Tablet> carStatusList = new DCarStatus().GetAllCarStatus_Tablet(m_PLANT_ID,m_STATION_ID);
//#endif

                //foreach (CarStatus_Tablet newStatus in carStatusList)
                //{
                //    CarStatus_Tablet matchedStatus = m_carBindingList.SingleOrDefault(s => s.BodyNo == newStatus.BodyNo);
                //    if (matchedStatus != null)
                //    {
                //        matchedStatus.UpdatedStatus(newStatus);
                //    }
                //    else
                //    {
                //        m_facilityBindingList.Add(newStatus);
                //    }
                //}

                SetCarStatusList(carStatusList);
                

                // 設備状態情報表示
                //SetFacilityInfoList(facilityStatusList);

                // ステータス確認
                m_managerMode = d_RcdStatusDao.GetMngMode(m_PLANT_ID, m_STATION_ID);

                statusbtn_enable();

                //m_managerMode = GetManagerMode(facilityStatusList);
                //UpdateMngModeToggle();

                //m_managerStatus = (ManagerMode)m_DMngModeDao.GetMngStatus();

                //m_Facility_Coordinate = new MFacilityDao().GetALLFacilityCoordinate();

                m_hasErrors = new DEmergencyDao().HasErrors(m_PLANT_ID,m_STATION_ID);
                //errorCtrlUnit = new DErrorStatus().errorCtrlUnitSID();

                SetDetectBtnColor();
                if (m_hasErrors && !errorflag)
                {
                    btn_errorDetect.BackgroundImage = RcdTablet.Properties.Resources.異常検知;
                    using (ErrorDetect ErrorDetectForm = new ErrorDetect(this))
                    {
                        ErrorDetectForm.Tag = this;
                        ErrorDetectForm.ShowDialog(this);
                        errorflag = true;
                        manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                    }
                }
                if (!m_hasErrors)
                {
                    btn_errorDetect.BackgroundImage = RcdTablet.Properties.Resources.異常検知_検知なし;
                    errorflag = false;
                }

                m_statInfoTimer.Start();
                //m_statInfoTimer.Start();
            }
            catch (SqlException e)
            {
                LOGGER.Info(e.Message);
                DialogResult res = MessageBox.Show(
                    "データベースへ接続できません。プログラムを終了しますか?"
                    , "DB接続エラー"
                    , MessageBoxButtons.YesNo
                    , MessageBoxIcon.Error);
                manageViewer2.onfacilityCoordinate = new Facility_Coordinate();

                if (res == DialogResult.Yes)
                {
                    Close();
                }
                else
                {
                    m_statInfoTimer.Start();
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex);
}
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 設備ステータスより、管制制御部の管制モードを判定・取得する。
        /// </summary>
        /// <param name="facilityStatusList">設備ステータスリスト</param>
        /// <returns>管制モード</returns>
        private ManagerMode GetManagerMode(List<FacilityStatus> facilityStatusList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (m_ope_flag == StsConn.DisConnecting)
                {
                    ManagerMode managerMode = new ManagerMode();
                    //managerMode
                }

                //return (ManagerMode)m_DRCDStatus.GetMngMode();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
            return ManagerMode.Err;
        }

        /// <summary>
        /// 管制制御部 各個・連続表示を更新
        /// </summary>
        //private void UpdateMngModeToggle()
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        if (m_managerMode[0].ControlStatus == (int)ManagerMode.Manual)
        //        {
        //            btn_onesec.Enabled = false;
        //        }
        //        else if (m_managerMode[0].ControlStatus == (int)ManagerMode.Auto)
        //        {
        //            btn_onesec.Enabled = true;
        //        }

        //        if (m_managerMode[0].PreoarationStatus == (int)btnStatus.ON)
        //        {
        //            btn_preparetion.Enabled = true;
        //        }
        //        else if (m_managerMode[0].PreoarationStatus == (int)btnStatus.OFF )
        //        {
        //            btn_preparetion.Enabled = false;
        //        }

        //        if (m_managerMode[0].ContinueStatus == (int)btnStatus.ON)
        //        {
        //            btn_continue.Enabled = true;
        //        }
        //        else if (m_managerMode[0].ContinueStatus == (int)btnStatus.OFF)
        //        {
        //            btn_continue.Enabled = false;
        //        }
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        /// <summary>
        /// 設備状態画面更新
        /// </summary>
        private void SetFacilityInfoList(List<FacilityStatus> facilityStatusList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                List<string> sourceID = facilityStatusList.Select(s => s.FacilityID).ToList();

                // 設備機器マスタ情報取得
                foreach (FacilityStatus newStatus in facilityStatusList)
                {
                    SortableBindingList<FacilityStatus> TargetToAddStatus;
                }
                //    switch (newStatus.FacilityTypeID)
                //    {
                //        case (int)FacilityType.TriadSignal:
                //        case (int)FacilityType.DuoSignal:
                //            TargetToAddStatus = m_signalBindingList;
                //            break;
                //        case (int)FacilityType.Shutter:
                //            TargetToAddStatus = m_shutterBindingList;
                //            break;
                //        default:
                //            TargetToAddStatus = m_elseBindingList;
                //            break;
                //    }

                //    FacilityStatus matchedStatus = m_shutterBindingList.Cast<FacilityStatus>()
                //        .Concat(m_signalBindingList.Cast<FacilityStatus>())
                //        .Concat(m_elseBindingList.Cast<FacilityStatus>())
                //        .SingleOrDefault(s => s.FacilityID == newStatus.FacilityID);

                //    if (matchedStatus != null) // update
                //    {
                //        matchedStatus.UpdatedStatus(newStatus);
                //    }
                //    else // add
                //    {
                //        TargetToAddStatus.Add(newStatus);
                //    }
                //}

                //UpdateFacilityErrorDisplay();

                //dgvShutter.Refresh();
                //dgvSignal.Refresh();
                //dgvElseFacility.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 車両状態画面更新
        /// </summary>
        private void SetCarStatusList(List<CarStatus_Tablet> carStatusList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                List<string> sourceID = carStatusList.Select(s => s.BodyNo).ToList();
                // 既存の情報を削除
                IReadOnlyList<CarStatus_Tablet> unmatchedCars = m_carBindingList.Cast<CarStatus_Tablet>().Where(t => !sourceID.Contains(t.BodyNo)).ToList();
                foreach (CarStatus_Tablet unmatchedCar in unmatchedCars)
                {
                    m_carBindingList.Remove(unmatchedCar);
                }

                foreach (CarStatus_Tablet newStatus in carStatusList)
                {
                    CarStatus_Tablet matchedStatus = m_carBindingList.Cast<CarStatus_Tablet>()
                        .SingleOrDefault(s => s.BodyNo == newStatus.BodyNo);

                    if (matchedStatus != null) // update
                    {
                        matchedStatus.UpdateStatus(newStatus);
                    }
                    else // add
                    {
                        m_carBindingList.Add(newStatus);
                    }
                }

                UpdateCarErrorDisplay();
                dgvCarStatus.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void UpdateCarErrorDisplay()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                for (int rowIndex = 0; rowIndex < dgvCarStatus.Rows.Count; rowIndex++)
                {
                    CarStatus_Tablet car = (CarStatus_Tablet)m_carBindingList[rowIndex];
                    DataGridViewRow row = dgvCarStatus.Rows[rowIndex];
                    if (car.SystemStatus.In(Res.CarStatus.STOPPED.Code, Res.CarStatus.RESOLVING.Code, Res.CarStatus.ERROR.Code))
                    {
                        SetDGVRowColor(row, Color.Red);
                        //btn_andonStop.Enabled = true;
                    }
                    else
                    {
                        SetDGVRowColor(row, SystemColors.Window);
                        //btn_andonStop.Enabled = false;
                    }
                }                
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void SetDGVRowColor(DataGridViewRow row, Color color)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = color;
            }
        }

        /// <summary>
        /// 車両状態をMAPに表示
        /// </summary>
        /// <param name="carStatusList"></param>
        private void DrawCarStatusList(List<CarStatus_Tablet> carStatusList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                List<MapViewer.CarStatus> statusList = carStatusList.Select(carStat => new MapViewer.CarStatus()
                {
                    //TerminalID = carStat.CtrlUnitID ?? "",
                    BodyNo = carStat.BodyNo,
                    Position = new PointF()
                    {
                        X = carStat.Xpos == null ? 0.0f : float.Parse(carStat.Xpos),
                        Y = carStat.Ypos == null ? 0.0f : float.Parse(carStat.Ypos)
                    },
                    Status = int.Parse(carStat.SystemStatus)

                }).ToList();
                LOGGER.Debug($"車両システムステータス", carStatusList.Select(carStat => { return carStat.SystemStatus; }).ToString());
                manageViewer2.ReDraw(statusList);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void UpdateStatusbtn(DRcdStatus rcdstatus)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (m_ope_flag == StsConn.DisConnecting)
                {
                    btn_preparetion.Enabled = false;
                    //btn_onesec.Enabled = false;
                    btn_continue.Enabled = false;
                }
                else
                {
                    btn_preparetion.Enabled = true;
                    //btn_onesec.Enabled = true;
                    btn_continue.Enabled = true;
                }
                if(m_preparationStatus == (int)btnStatus.OFF)
                {
                    //btn_preparetion.
                }


            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

#endregion

        /// <summary>
        /// ステータスDGV設定
        /// </summary>
        private void PrepareDataGridView()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                dgvfacility_allow.DataSource = m_facilityBindingList;
                dgvCarStatus.DataSource = m_carBindingList;
                //dgvFacility_noControl.DataSource = m_facilityNCBindingList;

                List<DataGridViewColumn> carStatusCols = new List<DataGridViewColumn>()
                        {
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = "CarSpecID",
                                HeaderText = "車両スペック",
                                Name = "CarSpecID",
                                MinimumWidth = 80,
                                ReadOnly = true,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = "BodyNo",
                                HeaderText = "ボデーNo",
                                Name = "BodyNo",
                                MinimumWidth = 80,
                                ReadOnly = true,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = "SystemStatus",
                                HeaderText = "ステータス",
                                Name = "StatusMsg",
                                MinimumWidth = 100,
                                ReadOnly = true,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            //new DataGridViewTextBoxColumn()
                            //{
                            //    DataPropertyName = "Status",
                            //    HeaderText = "制御状態",
                            //    Name = "CtrlUnitID",
                            //    MinimumWidth = 80,
                            //    ReadOnly = true,
                            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            //},
                            //new DataGridViewDisableButtonColumn()
                            //{
                            //    HeaderText = "",
                            //    MinimumWidth = 100,
                            //    Name = C_ANDON_STOP_COL,
                            //    ReadOnly = true,
                            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            //},
                            //new DataGridViewButtonColumn()
                            //{
                            //    HeaderText = "",
                            //    MinimumWidth = 100,
                            //    Name = C_CAR_STOP_COL,
                            //    ReadOnly = true,
                            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            //}
                        };
                List<DataGridViewColumn> facilityCols = new List<DataGridViewColumn>()
                        {
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_FACILITY_COL.ID,
                                HeaderText = "設備ID",
                                Name = C_FACILITY_COL.ID,
                                MinimumWidth = 60,
                                ReadOnly = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_FACILITY_COL.Name,
                                HeaderText = "設備名称",
                                Name = C_FACILITY_COL.Name,
                                MinimumWidth = 100,
                                ReadOnly = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_FACILITY_COL.StatusMsg,
                                HeaderText = "ステータス",
                                Name = C_FACILITY_COL.StatusMsg,
                                MinimumWidth = 100,
                                ReadOnly = false, Visible = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_FACILITY_COL.Status,
                                HeaderText = "Status",
                                Name = C_FACILITY_COL.Status,
                                MinimumWidth = 100,
                                ReadOnly = false, Visible = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            new DataGridViewDisableButtonColumn()
                            {
                                HeaderText = "",
                                Name = C_FACILITY_COL.BtnCtrl0,
                                MinimumWidth = 100,
                                ReadOnly = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            new DataGridViewDisableButtonColumn()
                            {
                                HeaderText = "",
                                Name = C_FACILITY_COL.BtnCtrl1,
                                MinimumWidth = 100,
                                ReadOnly = false, Visible = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            }
                        };
                //List<DataGridViewColumn> facilityNCCols = new List<DataGridViewColumn>()
                //        {
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_FACILITY_COL.ID,
                //                HeaderText = "設備ID",
                //                Name = C_FACILITY_COL.ID,
                //                MinimumWidth = 60,
                //                ReadOnly = false,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                //            },
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_FACILITY_COL.Name,
                //                HeaderText = "設備名称",
                //                Name = C_FACILITY_COL.Name,
                //                MinimumWidth = 100,
                //                ReadOnly = false,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                //            },
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_FACILITY_COL.StatusMsg,
                //                HeaderText = "ステータス",
                //                Name = C_FACILITY_COL.StatusMsg,
                //                MinimumWidth = 100,
                //                ReadOnly = false, Visible = false,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //            },
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_FACILITY_COL.Status,
                //                HeaderText = "Status",
                //                Name = C_FACILITY_COL.Status,
                //                MinimumWidth = 100,
                //                ReadOnly = false, Visible = false,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //            },
                //            new DataGridViewDisableButtonColumn()
                //            {
                //                HeaderText = "",
                //                Name = C_FACILITY_COL.BtnCtrl0,
                //                MinimumWidth = 100,
                //                ReadOnly = false,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //            },
                //            new DataGridViewDisableButtonColumn()
                //            {
                //                HeaderText = "",
                //                Name = C_FACILITY_COL.BtnCtrl1,
                //                MinimumWidth = 100,
                //                ReadOnly = false, Visible = false,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //            }
                //        };

                dgvCarStatus.Columns.Clear();
                dgvCarStatus.Columns.AddRange(carStatusCols.ToArray());
                dgvfacility_allow.Columns.Clear();
                dgvfacility_allow.Columns.AddRange(facilityCols.ToArray());
                //dgvFacility_noControl.Columns.Clear();
                //dgvFacility_noControl.Columns.AddRange(facilityNCCols.ToArray());

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void FacilityDataGridView(SortableBindingList<FacilityStatus_Tablet> BlistF)
        {
            dgvfacility_allow.DataSource = BlistF;
            List<DataGridViewColumn> facilityCols = new List<DataGridViewColumn>()
                        {
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_FACILITY_COL.ID,
                                HeaderText = "設備ID",
                                Name = C_FACILITY_COL.ID,
                                MinimumWidth = 60,
                                ReadOnly = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_FACILITY_COL.Name,
                                HeaderText = "設備名称",
                                Name = C_FACILITY_COL.Name,
                                MinimumWidth = 100,
                                ReadOnly = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_FACILITY_COL.StatusMsg,
                                HeaderText = "ステータス",
                                Name = C_FACILITY_COL.StatusMsg,
                                MinimumWidth = 100,
                                ReadOnly = false, Visible = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_FACILITY_COL.Status,
                                HeaderText = "Status",
                                Name = C_FACILITY_COL.Status,
                                MinimumWidth = 100,
                                ReadOnly = false, Visible = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            new DataGridViewDisableButtonColumn()
                            {
                                HeaderText = "",
                                Name = C_FACILITY_COL.BtnCtrl0,
                                MinimumWidth = 100,
                                ReadOnly = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },
                            new DataGridViewDisableButtonColumn()
                            {
                                HeaderText = "",
                                Name = C_FACILITY_COL.BtnCtrl1,
                                MinimumWidth = 100,
                                ReadOnly = false, Visible = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            }
                        };
            dgvfacility_allow.Columns.Clear();
            dgvfacility_allow.Columns.AddRange(facilityCols.ToArray());
        }


            /// <summary>
            /// DataGridViewCellに指定した画像を追加する
            /// </summary>
            private void DrawImageDgvCell(DataGridViewCellPaintingEventArgs e, int padding, Bitmap img)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                // fit image by cell height
                float imgAspect = img.Width / (float)img.Height;
                int h = e.CellBounds.Height - padding;
                int w = (int)(h * imgAspect);

                if (w >= e.CellBounds.Width - padding) // if image width overs cell width
                {
                    // fit image by cell width
                    w = e.CellBounds.Width - padding;
                    h = (int)(w / imgAspect);
                }

                int x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                int y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

                e.Graphics.DrawImage(img, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 管制制御部_接続状態表示 変更
        /// </summary>
        private void SetStsmCl(StsConn stsConn_flg)
        {
            try
            {
                //接続状態が変更なしの場合、処理不要
                if (stsConn_flg == m_ope_flag)
                {
                    return;
                }

                //接続状態の更新
                m_ope_flag = stsConn_flg;
                if (m_ope_flag == StsConn.Connnecting)
                {
                    Invoke((Action)(() =>
                    {
                        //OperationPC 接続状態表示：接続中
                        btn_preparetion.Enabled = true;
                        //btn_onesec.Enabled = true;
                        btn_continue.Enabled = true;
                    }));
                }
                else
                {
                    Invoke((Action)(() =>
                    {
                        //OperationPC 接続状態表示：切断中
                        btn_preparetion.Enabled = false;
                        //btn_onesec.Enabled = false;
                        btn_continue.Enabled = false;
                    }));
                }
            }
        catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
        catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
        finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        /// <summary>
        /// 異常検知ボタン色変更
        /// </summary>
        private void SetDetectBtnColor()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (m_hasErrors)
                {
                    btn_errorDetect.BackColor = Color.Red;

                }
                else
                {
                    btn_errorDetect.BackColor = SystemColors.ButtonShadow;
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        public void emergencyStop()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                CarStopSndMsg msg = new CarStopSndMsg();
                LOGGER.Info($"走行停止ボタン押下");
                SendTcp(msg);
                // 連打防止待ち
                //Task.Run(() =>
                //{
                //    PreventRepeatBtn(btn_OriPosi, C_DEFAULT_BTN_WAIT);
                //});
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_userControl_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if(m_carBindingList.Count(cs => cs.BodyNo != null && !cs.SystemStatus.Equals("0")) == 0)
                {
                    MessageBox.Show("走行停止対象車両が見つかりません。", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if(m_EmerStopTImer == null)
                {
                    m_EmerStopTImer = new System.Timers.Timer
                    {
                        Interval = m_STATUS_UPDATE_INTERVAL,
                        Enabled = false
                    };
                    m_EmerStopTImer.Elapsed += OnEmarStopTimerElapsed;
                    m_EmerStopTImer.Start();
                }
                else
                {
                    m_EmerStopTImer.Stop();
                    m_EmerStopTImer.Dispose();
                    m_EmerStopTImer = null;
                }
                
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        //private void btn_emergencystop_Click(object sender, EventArgs e)
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        CarStopSndMsg msg = new CarStopSndMsg();
        //        LOGGER.Info($"走行停止ボタン押下");
        //        SendTcp(msg);
        //        // 連打防止待ち
        //        Task.Run(() =>
        //        {
        //            PreventRepeatBtn(btn_OriPosi, C_DEFAULT_BTN_WAIT);
        //        });
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
        //    finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        private void btn_andonStop_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                andonStop();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                this.Close();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }


        public void andonStop()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                CarOffAndonSndMsg msg = new CarOffAndonSndMsg();
                LOGGER.Info($"ブザー停止ボタン押下");
                SendTcp(msg);
                // 連打防止待ち
                Task.Run(() =>
                {
                    PreventRepeatBtn(btn_OriPosi, C_DEFAULT_BTN_WAIT);
                });
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        private void statusbtn_enable()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if(m_managerMode[0].PreoarationStatus == (int)btnStatus.OFF)
                {
                    btn_preparetion.BackgroundImage = RcdTablet.Properties.Resources.blackimage;
                    //btn_onesec.Enabled = false;
                    btn_OriPosi.Enabled = false;
                    btn_continue.Enabled = false;
                }
                else
                {
                    btn_preparetion.BackgroundImage = RcdTablet.Properties.Resources.運転準備_ON;
                    //btn_onesec.Enabled = true;
                    if (m_managerMode[0].ControlStatus == (int)ManagerMode.Manual)
                    {
                        btn_OriPosi.Enabled = true;
                        btn_continue.Enabled = false;
                    }
                    else if(m_managerMode[0].ControlStatus == (int)ManagerMode.Auto && m_managerMode[0].OriPosiStatus == (int)ManagerStatus.PushOPBotton)
                    {
                        btn_OriPosi.Enabled = false;
                        btn_continue.Enabled = true;
                    }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #region ### tabControl ###
        private void tballowControl_Selected(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                TabControl tab = (TabControl)sender;
                string val = tab.SelectedTab == null ? null : tab.SelectedIndex.ToString();

                if (val == null)
                {
                    throw new UserException($"種別フィルター値取得失敗");
                }
                FacilityDataGridView(GetBindingListFacility(val));
                //dgvfacility_allow.DataSource = GetBindingListFacility(val);
                dgvfacility_allow.Refresh();

                //invisibleDgvList.DataSource = GetBindingListResult_invisible(val);
                //invisibleDgvList.Refresh();
                //dgvResultList.Columns.Clear();
                //dgvResultList.Columns.AddRange(PrepareDataGridView().ToArray());
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private SortableBindingList<FacilityStatus_Tablet> GetBindingListFacility(string tabindex)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            switch (tabindex)
            {
                case (C_ALLOW_FACILITY):
                    return m_facilityALBindingList;
                case (C_NOT_CONTROL):
                    return m_facilityNCBindingList;
                default:
                    return null;
            }
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region ### コンボボックス変形 ###
        private void cb_stationlist_SelectionChangeCommitted(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                SetSetting();

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }

        }
        #endregion

        #region ###　画像表示指示受信用 ###
        private void IsrvReceiveMessage(ReceiveMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.Message}");

                string rcvid = CommMsgBase.GetMsgID(e.Message);

                switch (rcvid)
                {
                    case RcvImageDisplay.ID:
                        {
                            LOGGER.Info($"受信→[OperationPC]画像切替指示:{e.Message}");

                            ImgDis(e.Message);

                            break;
                        }
                    default:
                        {
                            LOGGER.Info($"受信→[OperationPC]不正なIDを受信しました。:{e.Message}");

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

        private void ImgDis(string msg)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
                ImageNo = msg.Substring(6, 2);
                if (ImageNo.Equals("11"))
                {
                    using (FormImageDisplay formErrorsDetect = new FormImageDisplay(this))
                    {
                        formErrorsDetect.Tag = this;
                        formErrorsDetect.Show(this);
                    }
                }
                else
                {
                    for (int i = 0; i < Application.OpenForms.Count; i++)
                    {
                        Form f = Application.OpenForms[i];
                        if (f.Text == "FormImageDisplay")
                        {
                            f.Close();
                        }
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

        #endregion
        //// タッチ操作の情報を取得する関数
        //[DllImport("user32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize);

        //// タッチ操作のハンドルを閉じる関数
        //[DllImport("user32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool CloseTouchInputHandle(IntPtr lParam);

        //// WndProcメソッドをオーバーライド
        //protected override void WndProc(ref Message m)
        //{
        //    // WM_TOUCHメッセージを処理
        //    if (m.Msg == 0x0240) // WM_TOUCH
        //    {
        //        int inputCount = (short)m.WParam.ToInt32();
        //        TOUCHINPUT[] inputs = new TOUCHINPUT[inputCount];
        //        if (GetTouchInputInfo(m.LParam, inputCount, inputs, Marshal.SizeOf(new TOUCHINPUT())))
        //        {
        //            // タッチ操作の情報を処理（後述）
        //            for (int i = 0; i < inputCount; i++)
        //            {
        //                Point pt = new Point(inputs[i].x, inputs[i].y);
        //                int id = inputs[i].dwID;

        //                if ((inputs[i].dwFlags & 0x02) != 0) // TOUCHEVENTF_DOWN
        //                {
        //                    touchPoints.Add(new TouchPoint { ID = id, Position = pt, PreviousPosition = pt });
        //                }
        //                else if ((inputs[i].dwFlags & 0x04) != 0) // TOUCHEVENTF_UP
        //                {
        //                    touchPoints.RemoveAll(p => p.ID == id);
        //                }
        //                else if ((inputs[i].dwFlags & 0x01) != 0) // TOUCHEVENTF_MOVE
        //                {
        //                    var touchPoint = touchPoints.Find(p => p.ID == id);
        //                    if (touchPoint != null)
        //                    {
        //                        touchPoint.PreviousPosition = touchPoint.Position;
        //                        touchPoint.Position = pt;
        //                    }
        //                }

        //                if (touchPoints.Count == 2)
        //                {
        //                    double deltaX1 = touchPoints[0].Position.X - touchPoints[0].PreviousPosition.X;
        //                    double deltaY1 = touchPoints[0].Position.Y - touchPoints[0].PreviousPosition.Y;
        //                    double deltaX2 = touchPoints[1].Position.X - touchPoints[1].PreviousPosition.X;
        //                    double deltaY2 = touchPoints[1].Position.Y - touchPoints[1].PreviousPosition.Y;

        //                    double deltaWidth = (Math.Abs(deltaX1) + Math.Abs(deltaX2)) / 2;
        //                    double deltaHeight = (Math.Abs(deltaY1) + Math.Abs(deltaY2)) / 2;

        //                    this.Width += (int)deltaWidth;
        //                    this.Height += (int)deltaHeight;
        //                }
        //            }
        //        }
        //        CloseTouchInputHandle(m.LParam);
        //    }
        //    else
        //    {
        //        base.WndProc(ref m);
        //    }
        //}
    }

    class ControlList
    {
        /// <summary>
        /// コントロール名称
       /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// コントロールサイズ
        /// </summary>
        public Size ControlSize { get; set; }

        /// <summary>
        /// コントロール位置
        /// </summary>
        public Point ControlLoc { get; set; }
    }

    public class TouchPoint
    {
        public int ID { get; set; }
        public Point Position { get; set; }
        public Point PreviousPosition { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOUCHINPUT
    {
        public int x;
        public int y;
        public IntPtr hSource;
        public int dwID;
        public int dwFlags;
        public int dwMask;
        public int dwTime;
        public IntPtr dwExtraInfo;
        public int cxContact;
        public int cyContact;
    }

    #region ## 受信 ##
    public class RcvImageDisplay : RcvMsgBase
    {
        public const string ID = "D1";
        public const string NameStr = "画像表示指示";

        [Order(4), Length(2)]
        public string order { get; set; }

        public RcvImageDisplay(string rcv_msg) : base(rcv_msg) { }
    }

    #endregion
    //class ControlList
    //{
    //    /// <summary>
    //    /// コントロール名称
    //    /// </summary>
    //    public string ControlName { get; set; }

    //    /// <summary>
    //    /// コントロールサイズ
    //    /// </summary>
    //    public Size ControlSize { get; set; }

    //    /// <summary>
    //    /// コントロール位置
    //    /// </summary>
    //    public Point ControlLoc { get; set; }
    //}
    //class settingLocation
    //{
    //    /// <summary>
    //    /// 工場ID
    //    /// </summary>
    //    private int PlantID { get; set; }
    //    /// <summary>
    //    /// 工場名
    //    /// </summary>
    //    private string PlantName { get; set; }
    //    /// <summary>
    //    /// 工場ID
    //    /// </summary>
    //    private int StationID { get; set; }
    //    /// <summary>
    //    /// 工場名
    //    /// </summary>
    //    private string StationName { get; set; }

    //}
}
