using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using RcdCmn;
using System.Configuration;
using static RcdDao.MFacilityDao;
using System.Reflection;
using AtrptShare;

namespace MapViewer
{
    public partial class ManageBace : UserControl
    {
        private AppLog LOGGER;
        private Bitmap m_bitmap;
        protected Bitmap BACKIMAGE;
        private double m_imagerate = 1;
        private double m_ratechange = 0.1;

        protected List<TerminalStatus> m_TerminalStatus = new List<TerminalStatus>();
        private List<CarStatus> m_CarStatus = new List<CarStatus>();
        private List<CarStatus> m_CarStatusL = new List<CarStatus>();

        public SortedList<int, receive_Viewer> receive_Viewers = new SortedList<int, receive_Viewer>();

        //public delegate int DataUpdateEventHandler(object sender, DataUpdateEventArgs<T> e);

        protected PointF m_CommonCordinate;
        protected double m_SizeRatioToL;
        protected float m_SizeRatioToG;

        private const int C_POSITION_WIDTH = 10;

        protected const int C_IMAGESIZE_WIDTH = 50;
        protected const int C_IMAGESIZE_HIGHT = 30;

        protected const int C_IMAGESIZE_SHUTTER_X = 50;
        protected const int C_IMAGESIZE_SHUTTER_Y = 30;

        protected const int C_IMAGESIZE_CARSIG_X = 55;
        protected const int C_IMAGESIZE_CARSIG_Y = 25;

        protected const int C_IMAGESIZE_WARKERSIG_X = 25;
        protected const int C_IMAGESIZE_WARKERSIG_Y = 30;

        protected const int C_IMAGESIZE_LIGHT_X = 30;
        protected const int C_IMAGESIZE_LIGHT_Y = 30;

        private const double C_RATE_LIMIT_MINIMUM = 0.1;
        private const double C_RATE_LIMIT_MAXIMUM = 4;

        public event PaintEventHandler pnl_view_OnPaint;
        public event MouseEventHandler pnl_view_OnMouseDown;
        public event MouseEventHandler pnl_view_OnMouseMove;
        public event MouseEventHandler pnl_view_OnMouseUp;
        public event MouseEventHandler pnl_view_ONMouseWheel;

        int MAP_MOJI_COLOR_R;
        int MAP_MOJI_COLOR_G;
        int MAP_MOJI_COLOR_B;
        int MAP_MOJI_SIZE;


        protected GraphicsState transState;
        protected int m_mouseHover = 0;
        protected receive_Viewer onImagefacility= new receive_Viewer();         

        public ManageBace()
        {
            InitializeComponent();
            //this.Load += new System.EventHandler(this.ManageBace_Load);
            pnl_view.Paint += new PaintEventHandler(OnPaint);
            pnl_view.MouseDown += new MouseEventHandler(pnl_view_MouseDown);
            pnl_view.MouseMove += new MouseEventHandler(pnl_view_MouseMove);
            pnl_view.MouseUp += new MouseEventHandler(pnl_view_MouseUp);
            //pnl_view.MouseWheel += new MouseEventHandler(pnl_view_MouseWheel);
            //pnl_view.mouse += new EventHandler(pnl_view_MouseHover);
        }

        #region 描画
        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnPaint(object sender, PaintEventArgs e)
        {
            if (BACKIMAGE != null)
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
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(BACKIMAGE, new Rectangle(0, 0, (int)Math.Round(BACKIMAGE.Width * m_imagerate), (int)Math.Round(BACKIMAGE.Height * m_imagerate)), 0, 0, BACKIMAGE.Width, BACKIMAGE.Height, GraphicsUnit.Pixel, ia);

                if (pnl_view_OnPaint != null) { pnl_view_OnPaint(this, e); }

                

                ////車両情報の描画
                //if (m_CarStatusL.Count > 0)
                //{
                //    for (int i = 0; i <= m_CarStatusL.Count - 1; i++)
                //    {
                //        PaintCarStatus(m_CarStatusL[i], e);
                //    }
                //}
            }
        }

        public void paintFacilityName(receive_Viewer onImagefacility, PaintEventArgs e)
        {
            Font font = new Font("MS UI Gothic", MAP_MOJI_SIZE);
            SolidBrush drawBrush = new SolidBrush(Color.FromArgb(MAP_MOJI_COLOR_R, MAP_MOJI_COLOR_G, MAP_MOJI_COLOR_B));
            //Font font = new Font("MS UI Gothic", 11);
            //SolidBrush drawBrush = new SolidBrush(Color.FromArgb(255, 127, 0));
            PointF p = toLocalCoordinate(new PointF(onImagefacility.facilityCoordinate_X, onImagefacility.facilityCoordinate_Y));
            float imagesize_X;
            float imagesize_Y;
            if (onImagefacility.facilityTypeID == 0)
            {
                imagesize_X = C_IMAGESIZE_CARSIG_X;
                imagesize_Y = C_IMAGESIZE_CARSIG_Y;
            }
            else if (onImagefacility.facilityTypeID == 1)
            {
                imagesize_X = C_IMAGESIZE_WARKERSIG_X;
                imagesize_Y = C_IMAGESIZE_WARKERSIG_Y;
            }
            else if (onImagefacility.facilityTypeID == 2)
            {
                imagesize_X = C_IMAGESIZE_SHUTTER_X;
                imagesize_Y = C_IMAGESIZE_SHUTTER_Y;
            }
            else
            {
                imagesize_X = C_IMAGESIZE_LIGHT_X;
                imagesize_Y = C_IMAGESIZE_LIGHT_Y;
            }

            float x = p.X - imagesize_X / 2;
            float y = p.Y - imagesize_Y / 2 + imagesize_Y;
            e.Graphics.DrawString(onImagefacility.facilityName, font, drawBrush, x, y);
            //Font font = new Font("MS UI Gothic", 8);
            //SolidBrush drawBrush = new SolidBrush(Color.Red);
            //PointF p = toLocalCoordinate(new PointF(onImagefacility.facilityCoordinate_X, onImagefacility.facilityCoordinate_Y));
            //PointF Imgsize = getImageSize(onImagefacility.SID);
            //float x = p.X - C_IMAGESIZE_WIDTH / 2;
            //float y = p.Y + 30 - C_IMAGESIZE_HIGHT / 2;
            //e.Graphics.DrawString(onImagefacility.facilityName, font, drawBrush, x, y);
        }
        public void paintFacilityName(Facility_Coordinate onfacilityCoordinate, PaintEventArgs e)
        {
            Font font = new Font("MS UI Gothic", MAP_MOJI_SIZE);
            SolidBrush drawBrush = new SolidBrush(Color.FromArgb(MAP_MOJI_COLOR_R, MAP_MOJI_COLOR_G, MAP_MOJI_COLOR_B));
            //Font font = new Font("MS UI Gothic", 11);
            //SolidBrush drawBrush = new SolidBrush(Color.FromArgb(255, 127, 0));
            PointF p = toLocalCoordinate(new PointF(onfacilityCoordinate.Xcoordinate, onfacilityCoordinate.Ycoordinate));
            float imagesize_X;
            float imagesize_Y;
            if (onfacilityCoordinate.FacilityTypeID == 0)
            {
                imagesize_X = C_IMAGESIZE_CARSIG_X;
                imagesize_Y = C_IMAGESIZE_CARSIG_Y;
            }
            else if (onfacilityCoordinate.FacilityTypeID == 1)
            {
                imagesize_X = C_IMAGESIZE_WARKERSIG_X;
                imagesize_Y = C_IMAGESIZE_WARKERSIG_Y;
            }
            else if (onfacilityCoordinate.FacilityTypeID == 2)
            {
                imagesize_X = C_IMAGESIZE_SHUTTER_X;
                imagesize_Y = C_IMAGESIZE_SHUTTER_Y;
            }
            else
            {
                imagesize_X = C_IMAGESIZE_LIGHT_X;
                imagesize_Y = C_IMAGESIZE_LIGHT_Y;
            }

            float x = p.X - imagesize_X / 2;
            float y = p.Y - imagesize_Y / 2 + imagesize_Y;
            e.Graphics.DrawString(onfacilityCoordinate.FacilityName, font, drawBrush, x, y);
        }

        public void facilityImage_Paint(receive_Viewer Paintrec, PaintEventArgs e)
        {
            PointF p = toLocalCoordinate(new PointF(Paintrec.facilityCoordinate_X, Paintrec.facilityCoordinate_Y));


            float imagesize_X;
            float imagesize_Y;
            if (Paintrec.facilityTypeID == 0)
            {
                imagesize_X = C_IMAGESIZE_CARSIG_X;
                imagesize_Y = C_IMAGESIZE_CARSIG_Y;
            }
            else if (Paintrec.facilityTypeID == 1)
            {
                imagesize_X = C_IMAGESIZE_WARKERSIG_X;
                imagesize_Y = C_IMAGESIZE_WARKERSIG_Y;
            }
            else if (Paintrec.facilityTypeID == 2)
            {
                imagesize_X = C_IMAGESIZE_SHUTTER_X;
                imagesize_Y = C_IMAGESIZE_SHUTTER_Y;
            }
            else
            {
                imagesize_X = C_IMAGESIZE_LIGHT_X;
                imagesize_Y = C_IMAGESIZE_LIGHT_Y;
            }
            float x = p.X - imagesize_X / 2;
            float y = p.Y - imagesize_Y / 2;

            if (Paintrec.facilityTypeID == 2)
            {
                e.Graphics.DrawImage(Properties.Resources.bt_open_onlyimg_orange_lightgray, x, y, imagesize_X, imagesize_Y);
            }
            else if (Paintrec.facilityTypeID == 0)
            {
                e.Graphics.DrawImage(Properties.Resources.bt_carsig_blue_short, x, y, imagesize_X, imagesize_Y);
            }
            else if (Paintrec.facilityTypeID == 1)
            {
                e.Graphics.DrawImage(Properties.Resources.bt_wakersig_vertical, x, y, imagesize_X, imagesize_Y);
            }
            else
            {
                e.Graphics.DrawImage(Properties.Resources.greenlight, x, y, imagesize_X, imagesize_Y);
            }
        }

        public PointF toGlobalCoordinate(PointF p)
        {
            PointF gp = new PointF(0, 0);

            //p.Y = m_backimage.Size.Height - p.Y;
            //p.Y = m_backimage_sizeh - p.Y;
            p.Y = BACKIMAGE.Height - p.Y;

            //原点ずれ調整
            gp.X = p.X * m_SizeRatioToG;
            gp.Y = p.Y * m_SizeRatioToG;

            return gp;
        }
        /// <summary>
        /// 車両位置の描画
        /// </summary>
        /// <param name="status"></param>
        /// <param name="e"></param>
        //private void PaintCarStatus(CarStatus status, PaintEventArgs e)
        //{
        //    if (!status.Position.IsEmpty)
        //    {
        //        //車両位置
        //        StatusDraw statusDraw = GetCurrentStatusDraw(status.Status);
        //        e.Graphics.FillEllipse(statusDraw.color, status.Position.X - (float)(C_POSITION_WIDTH / 2), status.Position.Y - (float)(C_POSITION_WIDTH / 2), (float)C_POSITION_WIDTH, (float)C_POSITION_WIDTH);

        //        //ボデーNo
        //        using (Font font = new Font("MS UI Gothic", 8, FontStyle.Bold, GraphicsUnit.Point))
        //        {
        //            Brush b = Brushes.Black;
        //            float x = status.Position.X - (float)(C_POSITION_WIDTH / 2) + (float)C_POSITION_WIDTH;
        //            SizeF sizeF = e.Graphics.MeasureString("A", font);
        //            float y = status.Position.Y - (float)(C_POSITION_WIDTH / 2) - sizeF.Height;
        //            e.Graphics.DrawString(status.BodyNo, Font, b, new PointF(x, y));
        //        }
        //    }
        //}
        #endregion

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="log"></param>
        /// <param name="filepath">画像ファイルパス</param>
        /// <param name="CommonCordinate">画像座標(右上)左下原点</param>
        /// <param name="terminals">制御端末情報</param>
        //public void FirstSet(AppLog log, string filepath, PointF CommonCordinate, List<TerminalStatus> terminals)
        //{
        //    LOGGER = log;
        //    m_bitmap = new Bitmap(filepath);
        //    BACKIMAGE = new Bitmap(m_bitmap, new Size(m_bitmap.Size.Width, m_bitmap.Size.Height));

        //    PanelResize();

        //    m_CommonCordinate = CommonCordinate;
        //    RaitoCheck(CommonCordinate);
        //    ChangeRatio();

        //    m_TerminalStatus = terminals;
        //}

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="log"></param>
        /// <param name="filepath">画像ファイルパス</param>
        /// <param name="CommonCordinate">画像座標(右上)左下原点</param>
        /// <param name="terminals">制御端末情報</param>
        public void FirstSet(AppLog log, string filepath)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                LOGGER = log;
                if(m_bitmap != null)
                {
                    m_bitmap = null;
                    BACKIMAGE = null;
                }
                m_bitmap = new Bitmap(filepath);
                BACKIMAGE = new Bitmap(m_bitmap, new Size(m_bitmap.Size.Width, m_bitmap.Size.Height));

                PanelResize();

                //m_CommonCordinate = new PointF(m_bitmap.Size.Width / 10, m_bitmap.Size.Height / 10);
                RaitoCheck(m_CommonCordinate);
                ChangeRatio();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }


        /// <summary>
        /// 情報更新＋再描画
        /// </summary>
        /// <param name="state"></param>
        //public void ReDraw(List<CarStatus> state)
        //{

        //    //車両情報の更新
        //    m_CarStatus.Clear();
        //    m_CarStatusL.Clear();
        //    for (int i = 0; i < state.Count; i++)
        //    {
        //        m_CarStatus.Add((CarStatus)state[i].Clone());
        //        m_CarStatusL.Add((CarStatus)state[i].Clone());
        //        m_CarStatusL[i].Position = ToCommonCordinate(m_CarStatusL[i].Position, m_CarStatusL[i].TerminalID);
        //    }

        //    //描画更新
        //    pnl_view.Refresh();
        //}

        #region 内部処理
        //public StatusDraw GetCurrentStatusDraw(int code)
        //{
        //    return m_statusdraws.SingleOrDefault(status => status.code == code);
        //}

        /// <summary>
        /// 画像に合わせてパネルのサイズ変更
        /// </summary>
        protected void PanelResize()
        {
            Size ControlSize = new Size((int)Math.Round(BACKIMAGE.Width * m_imagerate), (int)Math.Round(BACKIMAGE.Height * m_imagerate));
            pnl_view.Size = ControlSize;
        }

        protected void ChangeRatio()
        {
            m_SizeRatioToL = (BACKIMAGE.Size.Width * m_imagerate) / m_CommonCordinate.X;
            m_SizeRatioToG = m_CommonCordinate.X / (BACKIMAGE.Size.Width * (float)m_imagerate);
        }

        protected void RaitoCheck(PointF CommonCordinate)
        {
            if (BACKIMAGE.Size.Width / m_CommonCordinate.X != BACKIMAGE.Size.Height / m_CommonCordinate.Y)
            {
                LOGGER.Warn("画像と座標の比率が違います");
            }
        }

        //public PointF ToCommonCordinate(PointF p, string no)
        //{

        //    PointF point = PointF.Empty;
        //    PointF tpoint = PointF.Empty;
        //    if (!p.IsEmpty)
        //    {
        //        foreach (TerminalStatus terminal in m_TerminalStatus)
        //        {
        //            if (terminal.TerminalID == no)
        //            {
        //                tpoint = terminal.OriginPosition;
        //                break;
        //            }
        //        }

        //        point = new PointF(p.X + tpoint.X, p.Y + tpoint.Y);
        //        point = toLocalCoordinate(point);
        //    }

        //    return point;
        //}

        public PointF toLocalCoordinate(PointF p)
        {
            PointF lp = new PointF(0, 0);

            //原点ずれ調整
            lp.X = (float)(p.X * m_SizeRatioToL);
            lp.Y = (float)(p.Y * m_SizeRatioToL);

            lp.Y = (float)(BACKIMAGE.Height * m_imagerate) - lp.Y;
            return lp;
        }
        #endregion

        #region マウス操作
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

        private void pnl_view_MouseHover(object sender, EventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                //クリック位置の補正と変換
                Point sp_over = Cursor.Position;
                Point ssp_over = this.PointToClient(sp_over);
                PointF cp_over = (PointF)this.PointToClient(sp_over);
                cp_over.X = cp_over.X - pnl_view.Location.X;
                cp_over.Y = cp_over.Y - pnl_view.Location.Y;

                cp_over = (PointF)toGlobalCoordinate(cp_over);
                onImagefacility = new receive_Viewer();
                foreach (KeyValuePair<int, receive_Viewer> kvp in receive_Viewers)
                {
                    float Imgrealsize = 40 * m_SizeRatioToG;
                    RectangleF round = new RectangleF((float)(kvp.Value.facilityCoordinate_X - (double)Imgrealsize / (double)2), (float)(kvp.Value.facilityCoordinate_Y - (double)Imgrealsize / (double)2), Imgrealsize, Imgrealsize);
                    
                    if (round.Contains(cp_over.X, cp_over.Y) && kvp.Value.visible == 1)
                    {
                        onImagefacility = new receive_Viewer();
                        onImagefacility = kvp.Value;
                        pnl_view.Refresh();
                        return;
                    }
                    

                }
                
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        public void receiveConfig(int colorR,int colorG,int colorB,int moji)
        {
            MAP_MOJI_COLOR_R = colorR;
            MAP_MOJI_COLOR_G = colorG;
            MAP_MOJI_COLOR_B = colorB;
            MAP_MOJI_SIZE = moji;
        }

        private Point MoveLimitCheck(Point cp)
        {
            Point p = Point.Empty;

            if (cp.X >= this.Size.Width)
            {
                p.X = this.Size.Width;
            }
            else if (cp.X <= 0)
            {
                p.X = 0;
            }
            else
            {
                p.X = cp.X;
            }

            if (cp.Y >= this.Size.Height)
            {
                p.Y = this.Size.Height;
            }
            else if (cp.Y <= 0)
            {
                p.Y = 0;
            }
            else
            {
                p.Y = cp.Y;
            }

            return p;
        }

        private void pnl_view_MouseWheel(object sender, MouseEventArgs e)
        {
            if (BACKIMAGE != null)
            {
                //倍率変更
                m_imagerate = m_imagerate + m_ratechange * Math.Sign(e.Delta);
                if (m_imagerate > C_RATE_LIMIT_MAXIMUM)
                {
                    m_imagerate = C_RATE_LIMIT_MAXIMUM;
                }
                else if (m_imagerate < C_RATE_LIMIT_MINIMUM)
                {
                    m_imagerate = C_RATE_LIMIT_MINIMUM;
                }

                //変化前サイズ
                Size p = pnl_view.Size;

                //描画準備
                PanelResize();
                ChangeRatio();

                //変化比率
                double rate = (double)pnl_view.Size.Width / (double)p.Width;

                //パネル移動
                Point sp = Cursor.Position;
                Point cp = this.PointToClient(sp);
                Point point = Point.Empty;
                point.X = (int)Math.Round((cp.X - pnl_view.Location.X) * rate - (cp.X - pnl_view.Location.X));
                point.Y = (int)Math.Round((cp.Y - pnl_view.Location.Y) * rate - (cp.Y - pnl_view.Location.Y));
                pnl_view.Location = new Point(pnl_view.Location.X - point.X, pnl_view.Location.Y - point.Y);

                ////車両位置変更
                //for (int i = 0; i < m_CarStatusL.Count; i++)
                //{
                //    m_CarStatusL[i].Position = ToCommonCordinate(m_CarStatus[i].Position, m_CarStatus[i].TerminalID);
                //}
                //描画更新
                pnl_view.Refresh();
            }

        }
        public PointF point_substr(PointF p, int num)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            p.X = (float)Math.Round(p.X, num, MidpointRounding.ToEven);
            p.Y = (float)Math.Round(p.Y, num, MidpointRounding.ToEven);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return p;
        }
        protected void DrawImageView(PaintEventArgs e, Rectangle rectangle, Bitmap img)
        {
                //e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                
                //e.Graphics.DrawImage(img,)

                //// fit image by cell height
                //float imgAspect = img.Width / (float)img.Height;
                //int h = e.CellBounds.Height - padding;
                //int w = (int)(h * imgAspect);

                //if (w >= e.CellBounds.Width - padding) // if image width overs cell width
                //{
                //    // fit image by cell width
                //    w = e.CellBounds.Width - padding;
                //    h = (int)(w / imgAspect);
                //}

                //int x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                //int y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

                e.Graphics.DrawImage(img, rectangle);
                //e.Handled = true;
            }
        #endregion
        public receive_Viewer SetRecView(Facility_Coordinate facility_Coordinate)
        {
            receive_Viewer setRec = new receive_Viewer();
            setRec.SID = facility_Coordinate.SID;
            setRec.facilityTypeID = facility_Coordinate.FacilityTypeID;
            setRec.facilityName = facility_Coordinate.FacilityName;
            setRec.facilityCoordinate_X = facility_Coordinate.Xcoordinate;
            setRec.facilityCoordinate_Y = facility_Coordinate.Ycoordinate;
            setRec.visible = facility_Coordinate.visible;

            return setRec;
        }
        protected PointF getImageSize(int facilityTypeID)
        {
            PointF Imagesize = new PointF();

            if (facilityTypeID == 0)
            {
                Imagesize.X = C_IMAGESIZE_CARSIG_X * m_SizeRatioToG;
                Imagesize.Y = C_IMAGESIZE_CARSIG_Y * m_SizeRatioToG;
            }
            else if (facilityTypeID == 1)
            {
                Imagesize.X = C_IMAGESIZE_WARKERSIG_X * m_SizeRatioToG;
                Imagesize.Y = C_IMAGESIZE_WARKERSIG_Y * m_SizeRatioToG;
            }
            else if (facilityTypeID == 2)
            {
                Imagesize.X = C_IMAGESIZE_SHUTTER_X * m_SizeRatioToG;
                Imagesize.Y = C_IMAGESIZE_SHUTTER_Y * m_SizeRatioToG;
            }
            else
            {
                Imagesize.X = C_IMAGESIZE_LIGHT_X * m_SizeRatioToG;
                Imagesize.Y = C_IMAGESIZE_LIGHT_Y * m_SizeRatioToG;
            }
            return Imagesize;

        }

        public void setCommonCordinate(PointF Maxmindiff)
        {
            m_CommonCordinate = Maxmindiff;
            ChangeRatio();
        }
    }
    public class FacilityImageEventArgs
    {
        public receive_Viewer rec;
        public int index;
    }

    //public class TerminalStatus
    //{
    //    /// <summary>
    //    /// 制御端末ID
    //    /// </summary>
    //    public string TerminalID { get; set; }
    //    /// <summary>
    //    /// 制御端末の原点位置
    //    /// </summary>
    //    public PointF OriginPosition { get; set; }
    //}

    //public class CarStatus : ICloneable
    //{
    //    /// <summary>
    //    /// 制御端末ID
    //    /// </summary>
    //    public string TerminalID { get; set; }
    //    /// <summary>
    //    /// ボデーNo.
    //    /// </summary>
    //    public string BodyNo { get; set; }
    //    /// <summary>
    //    /// 車両ステータス
    //    /// </summary>
    //    public int Status { get; set; }
    //    /// <summary>
    //    /// 車両位置
    //    /// </summary>
    //    public PointF Position { get; set; }

    //    public object Clone()
    //    {
    //        return (CarStatus)MemberwiseClone();
    //    }
    //}

    //public class StatusDraw
    //{
    //    public int code { get; private set; }
    //    public Brush color { get; private set; }

    //    public StatusDraw(int code, Brush color)
    //    {
    //        this.code = code;
    //        this.color = color;
    //    }
    //}
}
