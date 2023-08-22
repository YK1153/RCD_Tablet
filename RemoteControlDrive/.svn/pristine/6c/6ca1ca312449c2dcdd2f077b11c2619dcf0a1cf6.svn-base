using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RcdCmn;
using System.Reflection;
using System.Configuration;
using AtrptShare;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MapViewer
{
    public partial class ViewerBace : UserControl
    {
        public SortedList<int, RouteNode> m_RouteNodes = new SortedList<int, RouteNode>();
        public SortedList<int, SpeedArea> m_SpeedAreas = new SortedList<int, SpeedArea>();

        public AppLog LOGGER;
        public Image BACKIMAGE; 

        public event PaintEventHandler pnl_view_OnPaint;
        public event MouseEventHandler pnl_view_OnMouseDown;
        public event MouseEventHandler pnl_view_OnMouseMove;
        public event MouseEventHandler pnl_view_OnMouseUp;

        public float m_SizeRatioToG = 0;
        public float m_SizeRatioToL = 0;
        public Image m_backimage;
        public float m_backimage_sizeh;
        
        public ViewerConfig m_config;

        #region ### コンストラクタ ###

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ViewerBace()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.ViewerBace_Load);
            pnl_view.Paint += new PaintEventHandler(OnPaint);
        }

        #endregion

        #region ### 設定値受け取り ###
        public void ViewSetConfig(Image image, ViewerConfig conf, AppLog log)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            LOGGER = log;

            BACKIMAGE = image;
            ResizeBackimage();

            m_config = conf;

            // 理想経路設定
            m_RouteNodes.Clear();
            m_SpeedAreas.Clear();
            //if (conf.RouteNodeList != null)
            //{
            //    for (int i = 0; i <= conf.RouteNodeList.RouteNode.Count - 1; i++) { m_RouteNodes.Add(conf.RouteNodeList.RouteNode[i].ID, (RouteNode)conf.RouteNodeList.RouteNode[i]); }
            //    for (int i = 0; i <= conf.SpeedAreaList.SpeedArea.Count - 1; i++) { m_SpeedAreas.Add(conf.SpeedAreaList.SpeedArea[i].ID, (SpeedArea)conf.SpeedAreaList.SpeedArea[i]); }
            //}
            m_RouteNodes = m_config.RouteNodeList;
            m_SpeedAreas = m_config.SpeedAreaList;

            // グローバル-ローカル変換レート計算
            m_SizeRatioToG = (m_config.GlobalCoordinate.maxCoordinate.X - m_config.GlobalCoordinate.minCoordinate.X) / m_backimage.Width;
            m_SizeRatioToL = m_backimage.Size.Width / (m_config.GlobalCoordinate.maxCoordinate.X - m_config.GlobalCoordinate.minCoordinate.X);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// コントロールに合わせて背景画像をリサイズ
        /// </summary>
        private void ResizeBackimage()
        {
            Size imagesize = new Size(0, 0);

            imagesize = BACKIMAGE.Size;
            m_backimage = (Image)(new Bitmap(BACKIMAGE, imagesize));
            m_backimage_sizeh = m_backimage.Size.Height;

        }
        #endregion


        #region ### ロード ###
        private void ViewerBace_Load(object sender, EventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                PanelResize();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 描画用のパネルをコントロールのサイズに合わせる
        /// </summary>
        private void PanelResize()
        {
            //Size ControlSize = new Size(this.Size.Width, this.Size.Height);

            // pnl_draw.Size = ControlSize;

            Size ControlSize = new Size(this.Size.Width, this.Size.Height);
            if (BACKIMAGE.Width > this.Size.Width)
            {
                ControlSize.Width = BACKIMAGE.Width + 10;
            }
            if (BACKIMAGE.Height > this.Size.Height)
            {
                ControlSize.Height = BACKIMAGE.Height + 10;
            }
            pnl_view.Size = ControlSize;
        }

        #endregion


        #region ### 描画 ###
        public void OnPaint(object sender, PaintEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                using (Pen p = new Pen(Color.FromArgb(100, Color.Gray), 1))
                {
                    // 画像をぼかして描写
                    ColorMatrix cm = new ColorMatrix
                    {
                        Matrix00 = 1,
                        Matrix11 = 1,
                        Matrix22 = 1,
                        Matrix33 = 0.4F,
                        Matrix44 = 1
                    };
                    ImageAttributes ia = new ImageAttributes();
                    ia.SetColorMatrix(cm);
                    e.Graphics.DrawImage(m_backimage, new Rectangle(0, 0, m_backimage.Width, m_backimage.Height), 0, 0, m_backimage.Width, m_backimage.Height, GraphicsUnit.Pixel, ia);

                    p.Color = Color.DimGray;
                    // ノードの描写
                    foreach (int key in m_RouteNodes.Keys) { Node_Paint(m_RouteNodes[key], Brushes.DimGray, e); }
                    // ノードをつなぐ
                    if (m_RouteNodes.Count != 0) { Node_Connect(p, e); }
                    // エリアの描写
                    foreach (int key in m_SpeedAreas.Keys) { Area_Paint(m_SpeedAreas[key], Color.DimGray, e); }

                    //継承先にイベント発生させる
                    if (pnl_view_OnPaint != null) { pnl_view_OnPaint(this, e); }
            }
        }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ## １ノードの描画 ##
        /// <summary>
        /// 点を描画する
        /// </summary>
        /// <param name="Paintnode"></param>
        /// <param name="color">点を塗りつぶす色</param>
        /// <param name="e"></param>
        public void Node_Paint(RouteNode Paintnode, Brush color, PaintEventArgs e)
        {
            PointF p = toLocalCoordinate(new PointF(Paintnode.X, Paintnode.Y));

            float x = p.X - m_config.C_NODE_WIDTH / 2;
            float y = p.Y - m_config.C_NODE_WIDTH / 2;
            e.Graphics.FillEllipse(color, x, y, m_config.C_NODE_WIDTH, m_config.C_NODE_WIDTH);
        }
        #endregion

        #region ## ノードをつなぐ ##
        public void Node_Connect(Pen p, PaintEventArgs e)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            float FroX = 0;
            float FroY = 0;

            AdjustableArrowCap myarrow = new AdjustableArrowCap((float)m_config.C_ARROW_SIZE, (float)m_config.C_ARROW_SIZE);
            p.CustomEndCap = myarrow;
            int i = 0;
            foreach (KeyValuePair<int, RouteNode> kvp in m_RouteNodes)
            {
                PointF pt = toLocalCoordinate(new PointF(kvp.Value.X, kvp.Value.Y));
                if (i != 0) e.Graphics.DrawLine(p, FroX, FroY, pt.X, pt.Y);
                FroX = pt.X;
                FroY = pt.Y;
                i++;
            }

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region  ## １エリアの描画 ##
        public void Area_Paint(SpeedArea PaintArea, Color color, PaintEventArgs e)
        {
            Pen p = new Pen(color, 1);
            PointF[] pf = new PointF[PaintArea.Area.Count() - 1];
            for (int i = 0; i < PaintArea.Area.Count() - 1; i++)
            {
                pf[i] = toLocalCoordinate(PaintArea.Area[i]);
            }
            e.Graphics.DrawPolygon(p, pf);
            p.Dispose();
        }

        public void Area_Paint(SpeedArea PaintArea, Color color, PaintEventArgs e,int type)
        {
            Pen p = new Pen(Color.FromArgb(90, color), 1);
            PointF[] pf = new PointF[PaintArea.Area.Count() - 1];

            switch (type)
            {
                case 1:
                    for (int i = 0; i < PaintArea.SlowArea.Count() - 1; i++)
                    {
                        pf[i] = toLocalCoordinate(PaintArea.SlowArea[i]);
                    }
                    break;
                case 2:
                    for (int i = 0; i < PaintArea.PauseArea.Count() - 1; i++)
                    {
                        pf[i] = toLocalCoordinate(PaintArea.PauseArea[i]);
                    }
                    break;
                case 3:
                    for (int i = 0; i < PaintArea.StopArea.Count() - 1; i++)
                    {
                        pf[i] = toLocalCoordinate(PaintArea.StopArea[i]);
                    }
                    break;
            }

            e.Graphics.DrawPolygon(p, pf);
            p.Dispose();
        }
        #endregion

        #region ## その他エリアの描画 ##
        /// <summary>
        /// 最大3エリアを描画
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="e"></param>
        public void OtherAreaPaint_Three(int idx, PaintEventArgs e)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            for (int j = 0; j <= 2; j++)
            {
                switch (m_config.OtherAreas[idx].AreaCode)
                {
                    case OtherAreaCode.GoalArea:
                        {
                            OtherAreaPaint_One(e, m_config.C_COLOR_GOALAREA, m_config.OtherAreas[idx].area[j]);
                            break;
                        }
                    case OtherAreaCode.FacilityArea:
                        {
                            Color c = m_config.C_COLOR_FACILITYAREA;
                            switch (j)
                            {
                                case 0:
                                    OtherAreaPaint_One(e, ControlPaint.Light(c), m_config.OtherAreas[idx].area[j]);
                                    break;
                                case 1:
                                    OtherAreaPaint_One(e, ControlPaint.Dark(c), m_config.OtherAreas[idx].area[j]);
                                    break;
                                case 2:
                                    OtherAreaPaint_One(e, c, m_config.OtherAreas[idx].area[j]);
                                    break;
                            }
                            break;
                        }
                    case OtherAreaCode.GoalChangeStopArea:
                        {
                            OtherAreaPaint_One(e, m_config.C_COLOR_GOALCHANGESTOPAREA, m_config.OtherAreas[idx].area[j]);
                            break;
                        }
                    case OtherAreaCode.GuideArea:
                        {
                            OtherAreaPaint_One(e, m_config.C_COLOR_GUIDEAREA, m_config.OtherAreas[idx].area[j]);
                            break;
                        }
                }
            }

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        /// <summary>
        /// その他エリアの1エリアの描画
        /// </summary>
        /// <param name="e"></param>
        /// <param name="color"></param>
        /// <param name="points"></param>
        public void OtherAreaPaint_One(PaintEventArgs e, Color color, PointF[] points)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            using (Pen p = new Pen(color, 1))
            {
                if (points != null && points.Count() != 0)
                {
                    PointF[] pf = (PointF[])points.Clone();
                    for (int i = 0; i < pf.Count(); i++)
                    {
                        pf[i] = toLocalCoordinate(pf[i]);
                    }
                    e.Graphics.DrawPolygon(p, pf);
                }
            }
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion


        #region ### グローバル-ローカル 変換 ###
        public PointF toLocalCoordinate(PointF p)
        {
            PointF lp = new PointF(0, 0);

            //原点ずれ調整
            lp.X = (p.X - m_config.GlobalCoordinate.minCoordinate.X) * m_SizeRatioToL;
            lp.Y = (p.Y - m_config.GlobalCoordinate.minCoordinate.Y) * m_SizeRatioToL;

            //lp.Y = m_backimage.Size.Height - lp.Y;
            lp.Y = m_backimage_sizeh - lp.Y;
            return lp;
        }

        public PointF toGlobalCoordinate(PointF p)
        {
            PointF gp = new PointF(0, 0);

            //p.Y = m_backimage.Size.Height - p.Y;
            p.Y = m_backimage_sizeh - p.Y;

            //原点ずれ調整
            gp.X = p.X * m_SizeRatioToG + m_config.GlobalCoordinate.minCoordinate.X;
            gp.Y = p.Y * m_SizeRatioToG + m_config.GlobalCoordinate.minCoordinate.Y;

            return gp;
        }

        public double toLocalLength(double glen)
        {
            return glen * m_SizeRatioToL;
        }
        public double toGlobalLength(double llen)
        {
            return llen * m_SizeRatioToG;
        }
        #endregion

        #region ### 桁数調整 ###
        public PointF point_substr(PointF p, int num)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            p.X = (float)Math.Round(p.X, num, MidpointRounding.ToEven);
            p.Y = (float)Math.Round(p.Y, num, MidpointRounding.ToEven);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return p;
        }

        public double areawidth_substr(double width, int num)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            width = Math.Round(width, num, MidpointRounding.ToEven);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return width;
        }
        #endregion

        #region ### 継承先へイベント発生させる ###
        private void pnl_view_MouseDown(object sender, MouseEventArgs e)
        {
            if (pnl_view_OnMouseDown != null) { pnl_view_OnMouseDown(this, e); }
        }

        private void pnl_view_MouseMove(object sender, MouseEventArgs e)
        {
            if (pnl_view_OnMouseMove != null) { pnl_view_OnMouseMove(this, e); }
        }

        private void pnl_view_MouseUp(object sender, MouseEventArgs e)
        {
            if (pnl_view_OnMouseUp != null) { pnl_view_OnMouseUp(this, e); }
        }
        #endregion

    }

    public class MyPanel : Panel
    {
        public MyPanel()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }
    }

    
    #region Config設定値
    [Serializable]
    public class ViewerConfig
    {
        /// <summary>
        /// グローバル座標
        /// </summary>
        public GlobalCoordinate GlobalCoordinate { get; set; }

        /// <summary>
        /// 理想経路ノード
        /// </summary>
        public SortedList<int, RouteNode> RouteNodeList { get; set; }
        /// <summary>
        /// 理想経路走行エリア
        /// </summary>
        public SortedList<int,SpeedArea> SpeedAreaList { get; set; }
        /// <summary>
        /// その他エリア
        /// </summary>
        public List<OtherArea> OtherAreas { get; set; }


        //    /// <summary>
        //    /// 初期ベクトル固定値
        //    /// </summary>
        //    public int C_START_YAWRATE { get; set; }
        //    /// <summary>
        //    /// 初期ベクトル固定値を使用するか True:使用する
        //    /// </summary>
        //    public bool C_B_START_YAWRATE_USE { get; set; }

        /// <summary>
        /// ゴールエリアのエリア色
        /// </summary>
        public Color C_COLOR_GOALAREA { get; set; }

        ///// <summary>
        ///// 引継ぎエリアのエリア色
        ///// </summary>
        //public Color C_COLOR_TAKEOVERAREA { get; set; }

        /// <summary>
        /// 設備動作エリアのエリア色
        /// </summary>
        public Color C_COLOR_FACILITYAREA { get; set; }
        /// <summary>
        /// ゴール確定エリアの色
        /// </summary>
        public Color C_COLOR_GOALCHANGESTOPAREA { get; set; }
        /// <summary>
        /// ガイドエリアの色
        /// </summary>
        public Color C_COLOR_GUIDEAREA { get; set; }

        //    /// <summary>
        //    /// 設備動作エリアの動作判定する点 0:測位点 1:前方注視点
        //    /// </summary>
        //    public int C_OTHERAREA_JUDG_POINT { get; set; }
        /// <summary>
        /// 進行方向矢印長さ
        /// </summary>
        public int C_DIRECTION_LINE_LENGTH { get; set; }
        //    /// <summary>
        //    /// 速度値描画距離
        //    /// </summary>
        //    public int C_SPEEDLBL_LENGHT { get; set; }
        //    /// <summary>
        //    /// 計算値描画距離
        //    /// </summary>
        //    public int C_CALCLBL_LENGHT { get; set; }
        //    /// <summary>
        //    /// 計算値描画桁数
        //    /// </summary>
        //    public string C_CALCLBL_DIGIT { get; set; }
        /// <summary>
        /// ノードサイズ
        /// </summary>
        public int C_NODE_WIDTH { get; set; }
        /// <summary>
        /// 矢印サイズ
        /// </summary>
        public int C_ARROW_SIZE { get; set; }
        //    /// <summary>
        //    /// 測位点エリア内包判定判定エリア数(前後)
        //    /// </summary>
        //    public int C_INCLUDE_JUDGE_AREA_COUNT { get; set; }
        //    /// <summary>
        //    /// 制御中の自動スクロール
        //    /// </summary>
        //    public bool C_AUTO_SCROLL { get; set; }
        //    /// <summary>
        //    /// スタートカメラID
        //    /// </summary>
        //    public string C_START_CAM_ID { get; set; }
        //    /// <summary>
        //    /// カメラ補正毎回使用有無
        //    /// </summary>
        //    public bool C_SCORE_ALLUSE { get; set; }
        //    public double C_FIRST_POS_X { get; set; }
        //    public double C_FIRST_POS_Y { get; set; }
        //    public double C_DIFF_X { get; set; }
        //    public double C_DIFF_Y { get; set; }
        //    public int C_ERRMAX_COUNT { get; set; }

        //    /// <summary>
        //    /// 舵角制限モード
        //    /// </summary>
        //    public int C_ANGLE_LIMIT_MODE { get; set; }
        //    /// <summary>
        //    /// 加減速度モード
        //    /// </summary>
        //    public int C_ACCELERATION_MODE { get; set; }

        //    public int C_ACTUAL_ANGLE_WAIT { get; set; }

        //減速エリア
        public string SlowLeftWidth { get; set; }
        public string SlowRightWidth { get; set; }
        // 一時停止エリア
        public string PauseLeftWidth { get; set; }
        public string PauseRightWidth { get; set; }
        // 走行停止エリア
        public string StopLeftWidth { get; set; }
        public string StopRightWidth { get; set; }

        public object Clone()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Position = 0;
                return (ViewerConfig)bf.Deserialize(ms);
            }
        }
    }

    #endregion

}
