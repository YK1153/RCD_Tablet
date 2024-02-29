using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RcdCmn;
using System.Reflection;
using static RcdDao.DEmergencyDao;
using System.Diagnostics;

namespace RcdTablet
{
    public partial class EventHistory : Form
    {
        private clsTablet m_parent;
        private AppLog LOGGER = AppLog.GetInstance();
        private RcdDao.DEmergencyDao d_DEmergencyDao;
        private SortableBindingList<ErrorStatus> m_partEmergencyBindingList = new SortableBindingList<ErrorStatus>();
        private System.Timers.Timer m_Warningtimer;

        public EventHistory(clsTablet _parent)
        {
            InitializeComponent();
            m_parent = _parent;
            d_DEmergencyDao = new RcdDao.DEmergencyDao();
            List<ErrorStatus> DEmergencyList = d_DEmergencyDao.GetAllErrorStatus_Tablet(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            //invisiblefac = d_DrivingResultDao.GetAllResultList(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            foreach (ErrorStatus row in DEmergencyList)
            {
                m_partEmergencyBindingList.Add(row);
            }

            m_Warningtimer = new System.Timers.Timer
            {
                Interval = 10000,
                Enabled = false
            };
            m_Warningtimer.Elapsed += OnStatWarningTimerElapsed;

            m_Warningtimer.Start();
            dgv_EventHistry.Font = new Font("MS UI Gothic", m_parent.C_FONT_SIZE);
        }

        private class C_WARNING_LIST
        {
            internal const string StationSID = "StationSID";
            internal const string StationName = "Name";
            internal const string StartTime = "StartTIme";
            internal const string EndTime = "EndTime";
            internal const string boddyNumber = "BodyNo";
            internal const string CamID = "CamID";
            internal const string StatusMsg = "StatusMsg";
            internal const string BtnCtrl0 = "DetailInfo";
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void EventHistory_Load(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                PrepareDataGridView();
                //dgv_warningList.CellPainting += dgv_warningList_CellPainting;
                //dgv_warningList.SelectionChanged += ClearDgvSelection;
                //dgv_warningList.CellClick += dgv_warningList_CellClick;

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void PrepareDataGridView()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                dgv_EventHistry.DataSource = m_partEmergencyBindingList;
                //invisibleDgvList.DataSource = m_errorinvisibleBindingList;

                List<DataGridViewColumn> emergencyListCols = new List<DataGridViewColumn>()
                {
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_WARNING_LIST.StationName,
                                HeaderText = "工程名称",
                                Name = C_WARNING_LIST.StationName,
                                MinimumWidth = 80,
                                Width = 80,
                                ReadOnly = true,
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_WARNING_LIST.StartTime,
                                HeaderText = "発生時刻",
                                Name = C_WARNING_LIST.StartTime,
                                MinimumWidth = 110,
                                Width = 110,
                                ReadOnly = true,
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_WARNING_LIST.EndTime,
                                HeaderText = "解決時刻",
                                Name = C_WARNING_LIST.EndTime,
                                MinimumWidth = 80,
                                Width = 80,
                                ReadOnly = true,
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_WARNING_LIST.boddyNumber,
                                HeaderText = "ボデーNo",
                                Name = C_WARNING_LIST.boddyNumber,
                                MinimumWidth = 80,
                                Width = 80,
                                ReadOnly = true,Visible = false,
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_WARNING_LIST.CamID,
                                HeaderText = "カメラID",
                                Name = C_WARNING_LIST.CamID,
                                MinimumWidth = 80,
                                Width = 80,
                                ReadOnly = true,Visible = false,
                                //SortMode = DataGridViewColumnSortMode.NotSortable
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_WARNING_LIST.StatusMsg,
                                HeaderText = "エラー内容",
                                Name = C_WARNING_LIST.StatusMsg,
                                MinimumWidth = 100,
                                Width = 500,
                                ReadOnly = true,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewDisableButtonColumn()
                            {
                                HeaderText = "",
                                Name = C_WARNING_LIST.BtnCtrl0,
                                MinimumWidth = 100,
                                ReadOnly = false, Visible = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },

                };
                dgv_EventHistry.Columns.Clear();
                dgv_EventHistry.Columns.AddRange(emergencyListCols.ToArray());

                //List<DataGridViewColumn> invisiListCols = new List<DataGridViewColumn>()
                //{
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_RESULT_LIST.transporTime,
                //                HeaderText = "搬送時刻",
                //                Name = C_RESULT_LIST.transporTime,
                //                MinimumWidth = 50,
                //                Width = 50,
                //                ReadOnly = true,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                //                SortMode = DataGridViewColumnSortMode.Programmatic
                //            },
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_RESULT_LIST.boddyNumber,
                //                HeaderText = "ボデーNo",
                //                Name = C_RESULT_LIST.boddyNumber,
                //                MinimumWidth = 50,
                //                Width = 50,
                //                ReadOnly = true,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //            },
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_RESULT_LIST.ctrlUnitSID,
                //                HeaderText = "制御端末",
                //                Name = C_RESULT_LIST.ctrlUnitSID,
                //                MinimumWidth = 50,
                //                Width = 50,
                //                ReadOnly = true,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //            },
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_RESULT_LIST.result,
                //                HeaderText = "搬送結果",
                //                Name = C_RESULT_LIST.result,
                //                MinimumWidth = 50,
                //                Width = 50,
                //                ReadOnly = true,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //            },
                //            new DataGridViewTextBoxColumn()
                //            {
                //                DataPropertyName = C_RESULT_LIST.StatusMsg,
                //                HeaderText = "エラー内容",
                //                Name = C_RESULT_LIST.StatusMsg,
                //                MinimumWidth = 100,
                //                Width = 200,
                //                ReadOnly = true,
                //                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //            }
                //};

                //dgv_drivingHistory.Columns.Clear();
                //dgv_drivingHistory.Columns.AddRange(invisiListCols.ToArray());
                dgv_EventHistry.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }


        private void OnStatWarningTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<ErrorStatus> DWarningList = d_DEmergencyDao.GetAllErrorStatus_Tablet(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            foreach (ErrorStatus row in DWarningList)
            {
                //m_allWarningBindingList = ne;
            }
            Invoke(new Action(() => { dgv_EventHistry.Refresh(); }));
        }

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

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
