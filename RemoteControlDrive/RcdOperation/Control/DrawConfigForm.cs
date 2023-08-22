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
using MapViewer;
using AtrptShare;
using static RcdOperation.Control.ControlMain;

namespace RcdOperation.Control
{
    public partial class DrawConfigForm : Form
    {
        public delegate void OtherAreaChangeDrawEventHandler(OtherAreaChangeDrawEventArgs e);
        public event OtherAreaChangeDrawEventHandler OtherAreaChangeDrawEvent;

        public delegate void AngleDrawEventHandler(AngleDrawEventArgs e);
        public event AngleDrawEventHandler AngleDrawChangeEvent;

        public delegate void AutoDrawEventHandler(AutoDrawEventArgs e);
        public event AutoDrawEventHandler AutoDrawChangeEvent;

        private const int C_DRAW_ANGLE = 1;
        private const int C_DRAW_GETAREA = 2;

        public DrawConfigForm()
        {
            InitializeComponent();
        }

        private void DrawConfigFormcs_Load(object sender, EventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();
            dgv_otherarea.ColumnCount = 1;
            dgv_otherarea.Columns.Insert(0, column);
            dgv_otherarea.Columns[0].Width = 30;
            dgv_otherarea.Columns[1].HeaderText = "設備名称";
            dgv_otherarea.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv_otherarea.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            //描画設定
            //switch (Properties.Settings.Default.C_DRAW_MODE)
            //{
            //    case C_DRAW_ANGLE:
                    rbtn_drawangle.Checked = true;
                    rbtn_drawgetarea.Checked = false;
            //        break;
            //    case C_DRAW_GETAREA:
            //        rbtn_drawangle.Checked = false;
            //        rbtn_drawgetarea.Checked = true;
            //        break;
            //}

            //追尾モード
            //bool b = Properties.Settings.Default.C_DRAW_AUTO;
            bool b = true;
            cbox_drawauto.Checked = b;
            //cbox_drawauto.Checked = true;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private void DrawConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            e.Cancel = true;
            this.Visible = false;
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private void dgv_otherarea_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)dgv_otherarea[0, e.RowIndex];
            if (cell.Value.ToString() == "False")
            {
                dgv_otherarea.Rows[e.RowIndex].SetValues(true);
                OtherAreaChangeDrawEventArgs ea = new OtherAreaChangeDrawEventArgs
                {
                    index = e.RowIndex,
                    draw = true,
                    redraw = ""
                };
                if (OtherAreaChangeDrawEvent != null) { OtherAreaChangeDrawEvent(ea); }
            }
            else
            {
                dgv_otherarea.Rows[e.RowIndex].SetValues(false);
                OtherAreaChangeDrawEventArgs ea = new OtherAreaChangeDrawEventArgs
                {
                    index = e.RowIndex,
                    draw = false,
                    redraw = ""
                };
                if (OtherAreaChangeDrawEvent != null) { OtherAreaChangeDrawEvent(ea); }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        public void OtherAreaDrawList(ConfigData configData, OtherAreaRetention[] otherArea)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //その他エリア描画チェックリスト
            //string[] array = Properties.Settings.Default.C_DRAW_OTHERAREA.Split(',');
            string[] array = new string[0];

            //データ追加
            dgv_otherarea.Rows.Clear();
            for (int i = 0; i <= configData.OtherAreas.Count() - 1; i++)
            {
                dgv_otherarea.Rows.Add(array.Contains(i.ToString()), configData.OtherAreas[i].Name);
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }


        //private void rbtn_drawangle_CheckedChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

        //        AppLog.GetInstance().Info("[描画設定-舵角指示]がクリックされました");
        //        if (Properties.Settings.Default.C_DRAW_MODE == C_DRAW_GETAREA)
        //        {
        //            AppLog.GetInstance().Info("描画設定を変更します");

        //            AngleDrawEventArgs ea = new AngleDrawEventArgs
        //            {
        //                Angle = true,
        //                GetArea = false
        //            };
        //            if (AngleDrawChangeEvent != null) { AngleDrawChangeEvent(ea); }
        //            //Properties.Settings.Default.C_DRAW_MODE = C_DRAW_ANGLE;
        //        }
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
        //    finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        //private void rbtn_drawgetarea_CheckedChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

        //        AppLog.GetInstance().Info("[描画設定-測位取得範囲表示]がクリックされました");
        //        if (Properties.Settings.Default.C_DRAW_MODE == C_DRAW_ANGLE)
        //        {
        //            AppLog.GetInstance().Info("描画設定を変更します");
        //            AngleDrawEventArgs ea = new AngleDrawEventArgs
        //            {
        //                Angle = false,
        //                GetArea = true
        //            };
        //            if (AngleDrawChangeEvent != null) { AngleDrawChangeEvent(ea); }
        //            //Properties.Settings.Default.C_DRAW_MODE = C_DRAW_GETAREA;
        //        }
        //    }
        //    catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
        //    catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
        //    finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        //}

        private void cbox_drawauto_CheckedChanged(object sender, EventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            //Properties.Settings.Default.C_DRAW_AUTO = cbox_drawauto.Checked;
            AutoDrawEventArgs ea = new AutoDrawEventArgs
            {
                Auto = cbox_drawauto.Checked
            };
            if (AutoDrawChangeEvent != null) { AutoDrawChangeEvent(ea); }
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
    }

    public class OtherAreaChangeDrawEventArgs
    {
        public int index;
        public bool draw;
        public string redraw;
    }
    public class AngleDrawEventArgs
    {
        public bool Angle;
        public bool GetArea;
    }
    public class AutoDrawEventArgs
    {
        public bool Auto;
    }
}
