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
using static RcdDao.DWarningDao;
using System.Reflection;
using System.Diagnostics;

namespace RcdTablet
{
    public partial class FormWarninig : Form
    {
        private AppLog LOGGER = AppLog.GetInstance();
        private clsTablet m_parent;
        private RcdDao.DWarningDao d_DWarningDao;
        private SortableBindingList<DWarning> m_allWarningBindingList = new SortableBindingList<DWarning>();
        private System.Timers.Timer m_Warningtimer;

        public FormWarninig(clsTablet _parent)
        {
            InitializeComponent();
            m_parent = _parent;
            d_DWarningDao = new RcdDao.DWarningDao();
            List<DWarning> DWarningList = d_DWarningDao.GetAllDWarningInfo(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            //invisiblefac = d_DrivingResultDao.GetAllResultList(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            foreach (DWarning row in DWarningList)
            {
                m_allWarningBindingList.Add(row);
            }

            dgv_warningList.CellClick += dgv_warningList_CellClick;

            m_Warningtimer = new System.Timers.Timer
            {
                Interval = 10000,
                Enabled = false
            };
            m_Warningtimer.Elapsed += OnStatWarningTimerElapsed;

            m_Warningtimer.Start();

        }
        private class C_WARNING_LIST
        {
            internal const string StationSID = "StationSID";
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
        private void DrivingHistory_Load(object sender, EventArgs e)
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
                dgv_warningList.DataSource = m_allWarningBindingList;
                //invisibleDgvList.DataSource = m_errorinvisibleBindingList;

                List<DataGridViewColumn> resutListCols = new List<DataGridViewColumn>()
                {
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_WARNING_LIST.StationSID,
                                HeaderText = "工程番号",
                                Name = C_WARNING_LIST.StationSID,
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
                                ReadOnly = true,
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
                                ReadOnly = true,
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
                                ReadOnly = false,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                            },

                };
                dgv_warningList.Columns.Clear();
                dgv_warningList.Columns.AddRange(resutListCols.ToArray());

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
                dgv_warningList.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// DGV設備Cellクリック時
        /// </summary>
        private void dgv_warningList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                DataGridView senderDgv = (DataGridView)sender;
                int colIndex = e.ColumnIndex;
                int rowIndex = e.RowIndex;

                if (rowIndex < 0) return;

                bool btn0clicked = senderDgv.Columns.Contains(C_WARNING_LIST.BtnCtrl0)
                    && colIndex == senderDgv.Columns[C_WARNING_LIST.BtnCtrl0].Index;

                if (!btn0clicked)
                {
                    return;
                }

                DataGridViewDisableButtonCell btnCell = (DataGridViewDisableButtonCell)dgv_warningList.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (btnCell.Enabled == false)
                {
                    LOGGER.Debug($"非活性設備制御ボタン押下: 処理終了");
                    return;
                }

                DWarning Warn = (DWarning)m_allWarningBindingList[e.RowIndex];

                //SortableBindingList<FacilityStatus_Tablet> bindings = (SortableBindingList<FacilityStatus_Tablet>)senderDgv.DataSource;
                if (Warn.ErrCode.Equals("10114"))
                {
                    clsTabletMsg.CamShiftCheckSndMsg sndmsg = new clsTabletMsg.CamShiftCheckSndMsg();
                    m_parent.m_rcdCl.SendMessage(sndmsg, false);
                }

                //if (m_managerMode != ManagerMode.Manual)
                //{
                //    MessageBox.Show("管制制御部が各個制御中でないため、制御できません。", "設備制御不可", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    manageViewer1.onfacilityCoordinate = new Facility_Coordinate();
                //    return;
                //}

                //if (!(m_managerMode[0].PreoarationStatus == (int)btnStatus.ON && m_managerMode[0].ControlStatus == (int)ManagerMode.Manual && m_managerMode[0].ContinueStatus == (int)btnStatus.OFF))
                //{
                //    MessageBox.Show("管制制御部が各個制御中でないため、制御できません。", "設備制御不可", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    //manageViewer1.onfacilityCoordinate = new Facility_Coordinate();
                //    return;
                //}


                //string faclityID = bindings[e.RowIndex].FacilityID;
                //int facilityStatus = bindings[e.RowIndex].Status;
                ////int facilityCtrlVal = facilityStatus.Equals("赤") || facilityMsg.Equals("開") || facilityMsg.Equals("ON") || facilityMsg.Equals("点灯") ? 0 : 1;
                //int facilityCtrlVal = facilityStatus == 1 ? 0 : 1;
                //string ctrlValName = "";

                //FacilityStatusMsg statMsg = m_facilityStatusMsgs
                //        .Where(fStatus => fStatus.FacilityTypeSID == facilityType_ID)
                //        .Where(fStatus => fStatus.StatusCode == facilityCtrlVal)
                //        .SingleOrDefault();

                //ctrlValName = statMsg == null ? $"制御値:{facilityCtrlVal}" : statMsg.StatusMsg;
                //if (ctrlValName == null)
                //{
                //    ctrlValName = $"制御値:{facilityCtrlVal}";
                //}

                //LOGGER.Info($"設備制御ボタン押下: 設備ID: {faclityID}, 制御値({facilityCtrlVal})");

                ////動作確認
                //string strTile = "周辺設備　動作確認";
                //string strMsg = $"{facilityName}を {ctrlValName}にしますか？";
                //DialogResult result = MessageBox.Show(strMsg, strTile, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                //manageViewer2.onfacilityCoordinate = new Facility_Coordinate();
                //if (result == DialogResult.Cancel)
                //{
                //    LOGGER.Info($"設備制御ボタン押下: 設備ID: {faclityID}, 制御値({facilityCtrlVal}):キャンセルが押下されました");
                //    manageViewer1.onfacilityCoordinate = new Facility_Coordinate();
                //    return;
                //}

                //FacCtrlSndMsg msg = new FacCtrlSndMsg();
                //msg.facID = faclityID;
                //msg.ctrlVal = facilityCtrlVal;
                //SendTcp<FacCtrlSndMsg>(msg);

                //DWarning d_warn = m_allWarningBindingList[rowIndex];
                //string btnKey = C_WARNING_LIST.BtnCtrl0;

                // 連打防止待ち
                Task.Run(() =>
                {
                    PreventRepeatBtn(dgv_warningList, 1000);
                });
                List<DWarning> DWarningList = d_DWarningDao.GetAllDWarningInfo(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
                foreach (DWarning row in DWarningList)
                {
                    m_allWarningBindingList = new SortableBindingList<DWarning>();
                    m_allWarningBindingList.Add(row);
                }
                Invoke(new Action(() => { dgv_warningList.Refresh(); }));
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void OnStatWarningTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<DWarning> DWarningList = d_DWarningDao.GetAllDWarningInfo(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            foreach (DWarning row in DWarningList)
            {
                //m_allWarningBindingList = ne;
            }
            Invoke(new Action(() => { dgv_warningList.Refresh(); }));
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

    }

}
