using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RcdDao.DDrivingResultDao;
using RcdCmn;
using System.Reflection;
using static RcdTablet.Utils;
using RcdDao;

namespace RcdTablet
{
    public partial class DrivingHistory : Form
    {
        private clsTablet m_parent;

        private AppLog LOGGER = AppLog.GetInstance();
        private SortableBindingList<ResultList> m_errorResultBindingList = new SortableBindingList<ResultList>();
        private SortableBindingList<ResultList> m_allResultBindingList = new SortableBindingList<ResultList>();
        private SortableBindingList<ResultList> m_successResultBindingList = new SortableBindingList<ResultList>();

        private SortableBindingList<ResultList> m_errorinvisibleBindingList = new SortableBindingList<ResultList>();
        private SortableBindingList<ResultList> m_allinvisibleBindingList = new SortableBindingList<ResultList>();
        private SortableBindingList<ResultList> m_successinvisibleBindingList = new SortableBindingList<ResultList>();

        private const string C_ERROR_RESULT = "失敗一覧";
        private const string C_ALL_RESULT = "結果一覧";
        private const string C_SUCCESS_RESULT = "成功一覧";
        private const string C_ASC_STRING = "ASC";
        private const string C_DESC_STRING = "DESC";
        DDrivingResultDao d_DrivingResultDao;
        private List<ResultList> fac;
        //private List<ResultList> invisiblefac;
        ListSortDirection direction;
        bool sortAscending = true;
        private int m_sort_column_index = -1;
        private int ROWAUTOSCALL = 31;

        public DrivingHistory(clsTablet _parent)
        {
            InitializeComponent();
            m_parent = _parent;
            d_DrivingResultDao = new DDrivingResultDao();
            fac = d_DrivingResultDao.GetAllResultList(m_parent.m_PLANT_ID,m_parent.m_STATION_ID);
            //invisiblefac = d_DrivingResultDao.GetAllResultList(m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
            foreach (ResultList row in fac)
            {
                m_allResultBindingList.Add(row);
            }
            //foreach (ResultList row in invisiblefac)
            //{
            //    m_allinvisibleBindingList.Add(row);
            //}
            fac = d_DrivingResultDao.GetErrorResultList(m_parent.m_PLANT_ID, m_parent.m_STATION_ID, 1);
            //invisiblefac = d_DrivingResultDao.GetErrorResultList();
            foreach (ResultList row in fac)
            {
                m_errorResultBindingList.Add(row);
            }
            //foreach (ResultList row in invisiblefac)
            //{
            //    m_errorinvisibleBindingList.Add(row);
            //}
            fac = d_DrivingResultDao.GetErrorResultList(m_parent.m_PLANT_ID, m_parent.m_STATION_ID,0);
            //invisiblefac = d_DrivingResultDao.GetSuccessResultList();
            foreach (ResultList row in fac)
            {
                m_successResultBindingList.Add(row);
            }
            //foreach (ResultList row in invisiblefac)
            //{
            //    m_successinvisibleBindingList.Add(row);
            //}
            cmbAllOrError.SelectedIndex = 2;
            dgv_drivingHistory.CellFormatting += formResultList_CellFormatting;
            dgv_drivingHistory.Font = new Font("MS UI Gothic", m_parent.C_FONT_SIZE);

            dgv_drivingHistory.RowsDefaultCellStyle.BackColor = Color.White;
            dgv_drivingHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245,245,245);
        }

        private class C_RESULT_LIST
        {
            internal const string StartTime = "StartTime";
            internal const string EndTime = "EndTime";
            //internal const string Name = "Updated";
            internal const string boddyNumber = "BodyNo";
            internal const string StationSID = "StationSID";
            internal const string StationName = "Name";
            internal const string result = "result";
            internal const string StatusMsg = "StatusMsg";
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void DrivingHistory_Load(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                PrepareDataGridView(m_errorResultBindingList);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void PrepareDataGridView(SortableBindingList<ResultList> Rlist)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                dgv_drivingHistory.DataSource = Rlist;
                //invisibleDgvList.DataSource = m_errorinvisibleBindingList;

                List<DataGridViewColumn> resutListCols = new List<DataGridViewColumn>()
                {
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_RESULT_LIST.StationName,
                                HeaderText = "工程名称",
                                Name = C_RESULT_LIST.StationName,
                                MinimumWidth = 80,
                                Width = 80,
                                ReadOnly = true,
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_RESULT_LIST.StartTime,
                                HeaderText = "エラー発生時刻",
                                Name = C_RESULT_LIST.StartTime,
                                MinimumWidth = 110,
                                Width = 110,
                                ReadOnly = true,
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_RESULT_LIST.EndTime,
                                HeaderText = "エラー終了時刻",
                                Name = C_RESULT_LIST.EndTime,
                                MinimumWidth = 110,
                                Width = 110,
                                ReadOnly = true,
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_RESULT_LIST.boddyNumber,
                                HeaderText = "ボデーNo",
                                Name = C_RESULT_LIST.boddyNumber,
                                MinimumWidth = 80,
                                Width = 80,
                                ReadOnly = true,
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },                            
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_RESULT_LIST.result,
                                HeaderText = "搬送結果",
                                Name = C_RESULT_LIST.result,
                                MinimumWidth = 80,
                                Width = 80,
                                ReadOnly = true,
                                //SortMode = DataGridViewColumnSortMode.NotSortable
                                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            },
                            new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = C_RESULT_LIST.StatusMsg,
                                HeaderText = "エラー内容",
                                Name = C_RESULT_LIST.StatusMsg,
                                MinimumWidth = 100,
                                Width = 500,
                                ReadOnly = true,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                SortMode = DataGridViewColumnSortMode.Programmatic
                            }
                };
                dgv_drivingHistory.Columns.Clear();
                dgv_drivingHistory.Columns.AddRange(resutListCols.ToArray());

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
                dgv_drivingHistory.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void cmbALLOrError_SelectionChangeCommitted(object sender, EventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                ComboBox cmb = (ComboBox)sender;
                string val = cmb.SelectedItem == null ? null : cmb.SelectedItem.ToString();

                if (val == null)
                {
                    throw new UserException($"種別フィルター値取得失敗");
                }

                PrepareDataGridView(GetBindingListResult(val));
                dgv_drivingHistory.Refresh();

                //invisibleDgvList.DataSource = GetBindingListResult_invisible(val);
                //invisibleDgvList.Refresh();
                //dgvResultList.Columns.Clear();
                //dgvResultList.Columns.AddRange(PrepareDataGridView().ToArray());
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        private SortableBindingList<ResultList> GetBindingListResult(string result)
        {
            switch (result)
            {
                case (C_ERROR_RESULT):
                    return m_errorResultBindingList;
                case (C_ALL_RESULT):
                    return m_allResultBindingList;
                case (C_SUCCESS_RESULT):
                    return m_successResultBindingList;
                default:
                    return null;
            }
        }

        private SortableBindingList<ResultList> GetBindingListResult_invisible(string result)
        {
            switch (result)
            {
                case (C_ERROR_RESULT):
                    return m_errorinvisibleBindingList;
                case (C_ALL_RESULT):
                    return m_allinvisibleBindingList;
                case (C_SUCCESS_RESULT):
                    return m_successinvisibleBindingList;
                default:
                    return null;
            }
        }

        private void formResultList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // If the column is the Artist column, check the
            // value.
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (this.dgv_drivingHistory.Columns[e.ColumnIndex].Name == C_RESULT_LIST.result)
                {
                    if (e.Value != null)
                    {
                        // Check for the string "pink" in the cell.
                        if ((int)e.Value == 1)
                        {
                            e.Value = "失敗";
                        }
                        else if ((int)e.Value == 0)
                        {
                            e.Value = "成功";
                        }
                    }
                }
                else if (this.dgv_drivingHistory.Columns[e.ColumnIndex].Name == C_RESULT_LIST.StartTime)
                {
                    if (e.Value != null)
                    {
                        e.Value = e.Value;
                    }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        private void formResultList_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                //選択列取得
                string Column = "";
                switch (e.ColumnIndex)
                {
                    case 0:
                        Column = "Name";
                        break;
                    case 1:
                        Column = "StartTime";
                        break;
                    case 2:
                        Column = "EndTime";
                        break;
                    case 3:
                        Column = "BodyNo";
                        break;
                    case 4:
                        Column = "result";
                        break;
                    case 5:
                        Column = "StatusMsg";
                        break;
                }

                if (m_sort_column_index == e.ColumnIndex)
                {
                    sortAscending = sortAscending == true ? false : true;
                }
                else { sortAscending = true; }
                string order = sortAscending ? C_ASC_STRING : C_DESC_STRING;
                m_sort_column_index = e.ColumnIndex;

                //値格納
                string where = "";
                SortableBindingList<ResultList> list = new SortableBindingList<ResultList>();
                switch (cmbAllOrError.SelectedItem.ToString())
                {
                    case (C_ERROR_RESULT):
                        // m_errorResultBindingList;
                        where = "drire.result = 1";
                        fac = d_DrivingResultDao.GetSortResultList(where, Column, order, m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
                        foreach (ResultList row in fac)
                        {
                            list.Add(row);
                        }
                        m_errorResultBindingList = list;
                        PrepareDataGridView(m_errorResultBindingList);
                        break;
                    case (C_ALL_RESULT):
                        // m_allResultBindingList;
                        where = "drire.result <> -1";
                        fac = d_DrivingResultDao.GetSortResultList(where, Column, order, m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
                        foreach (ResultList row in fac)
                        {
                            list.Add(row);
                        }
                        m_allResultBindingList = list;
                        PrepareDataGridView(m_allResultBindingList);
                        break;
                    case (C_SUCCESS_RESULT):
                        // m_successResultBindingList;
                        where = "drire.result = 0";
                        fac = d_DrivingResultDao.GetSortResultList(where, Column, order, m_parent.m_PLANT_ID, m_parent.m_STATION_ID);
                        foreach (ResultList row in fac)
                        {
                            list.Add(row);
                        }
                        m_successResultBindingList = list;
                        PrepareDataGridView(m_successResultBindingList);
                        break;
                }

                dgv_drivingHistory.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = sortAscending ? SortOrder.Ascending : SortOrder.Descending;
                dgv_drivingHistory.Refresh();

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
    }
}

