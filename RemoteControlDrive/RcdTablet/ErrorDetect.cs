using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using RcdCmn;
using System.Reflection;
using CommWrapper;
using static RcdTablet.clsTabletMsg;
using static RcdTablet.Utils;
using static RcdDao.DEmergencyDao;
using RcdDao;


namespace RcdTablet
{
    public partial class ErrorDetect : Form
    {

        private AppLog LOGGER = AppLog.GetInstance();
        private clsTablet m_parent;
        private SortableBindingList<ErrorStatus> m_CarErrorDetectBindingList = new SortableBindingList<ErrorStatus>();
        private SortableBindingList<ErrorStatus> m_ConnectErrorDetectBindingList = new SortableBindingList<ErrorStatus>();
        private System.Timers.Timer m_EDetecttimer;

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

        public ErrorDetect(clsTablet _parent)
        {
            InitializeComponent();
            m_parent = _parent;
        }

        private void FormErrorDetect_Load(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                SetDoubleBuffer(this);
                List<ErrorStatus> errorStatuses = new DEmergencyDao().GetAllErrorStatus_noSolve(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
                List<ErrorStatus> errorStatuses_Con = new DEmergencyDao().GetAllErrorStatus_Connect(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
                foreach (ErrorStatus row in errorStatuses)
                {
                    m_CarErrorDetectBindingList.Add(row);
                }

                foreach (ErrorStatus row in errorStatuses_Con)
                {
                    m_ConnectErrorDetectBindingList.Add(row);
                }
                PrepareDataGridView();

                m_EDetecttimer = new System.Timers.Timer
                {
                    Interval = 500,
                    Enabled = false
                };
                m_EDetecttimer.Elapsed += OnStatWarningTimerElapsed;

                m_EDetecttimer.Start();
                dgvCarError.Font = new Font("MS UI Gothic", m_parent.C_FONT_SIZE);
                dgvConnError.Font = new Font("MS UI Gothic", m_parent.C_FONT_SIZE);

                dgvCarError.RowsDefaultCellStyle.BackColor = Color.White;
                dgvCarError.AlternatingRowsDefaultCellStyle.BackColor = Color.Gray;

                dgvConnError.RowsDefaultCellStyle.BackColor = Color.White;
                dgvConnError.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

                //lbl_bodyNo_Value.Text = errorStatuses[0].BodyNo;
                //lbl_emergency_value.Text = errorStatuses[0].StatusMsg;
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        private void PrepareDataGridView()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                dgvCarError.DataSource = m_CarErrorDetectBindingList;
                dgvConnError.DataSource = m_ConnectErrorDetectBindingList;
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
                                ReadOnly = true,Visible = true,
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
                                ReadOnly = true,Visible = true,
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
                            }

                };
                List<DataGridViewColumn> connectListCols = new List<DataGridViewColumn>()
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
                            }

                };
                dgvCarError.Columns.Clear();
                dgvCarError.Columns.AddRange(emergencyListCols.ToArray());
                dgvConnError.Columns.Clear();
                dgvConnError.Columns.AddRange(connectListCols.ToArray());

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
                dgvCarError.Refresh();
                dgvConnError.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void OnStatWarningTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<ErrorStatus> errorStatuses = new DEmergencyDao().GetAllErrorStatus_noSolve(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            List<ErrorStatus> errorStatuses_Con = new DEmergencyDao().GetAllErrorStatus_Connect(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            foreach (ErrorStatus row in errorStatuses)
            {
                if (!m_CarErrorDetectBindingList.Any(t => t.SID == row.SID))
                {
                    m_CarErrorDetectBindingList.Add(row);
                }    
                //m_allWarningBindingList = ne;
            }
            foreach (ErrorStatus row in errorStatuses_Con)
            {
                if (!m_ConnectErrorDetectBindingList.Any(t => t.SID == row.SID))
                {
                    m_ConnectErrorDetectBindingList.Add(row);
                }
            }

            Invoke(new Action(() => { dgvCarError.Refresh(); }));
            Invoke(new Action(() => { dgvConnError.Refresh(); }));
        }

        /// <summary>
        /// 閉じるボタン押下
        /// </summary>
        private void btnExit_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                m_EDetecttimer.Dispose();
                System.Threading.Thread.Sleep(500);
                Close();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        private void btnCarOffAndon_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                m_parent.andonStop();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
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

        private void btnCarResolveSndMsg_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                //if (!carStat.Status.In(Res.CarStatus.STOPPED.Code, Res.CarStatus.ERROR.Code))
                //{
                //    LOGGER.Info($"異常解除ボタン押下: 対象ステータスでないため、処理終了。車両ステータス: {m_carStatus.GetMsg(carStat.Status)}");
                //    return;
                //}

                //LOGGER.Info($"車両異常解除ボタン押下: ボデーNo: {carStat.CarNo}, 車両ステータス: {m_carStatus.GetMsg(carStat.Status)}");
                //CarResolveSndMsg msg = new CarResolveSndMsg();
                //m_parent.SendTcp(msg);

                //// 連打防止待ち
                //Task.Run(() =>
                //{
                //    PreventRepeatBtn(btn_carResolve, C_DEFAULT_BTN_WAIT);
                //});

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void btn_carResolve_Click(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                m_parent.carResolveSndMsg();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
    }
}
