using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using RcdCmn;
using System.Runtime;
using static RcdDao.MFacilityDao;
using static RcdDao.MStationDao;
using System.Runtime.InteropServices;


namespace MapViewer
{
    public partial class ManageViewer : ManageBace
    {
        //private AppLog2 LOGGER;
        //private Bitmap m_bitmap;
        //private Bitmap BACKIMAGE;
        private double m_imagerate = 1;
        private double m_ratechange = 0.1;

        //private List<TerminalStatus> m_TerminalStatus = new List<TerminalStatus>();
        private List<CarStatus> m_CarStatus = new List<CarStatus>();
        private List<CarStatus> m_CarStatusL = new List<CarStatus>();
        //private PointF m_CommonCordinate;
        //private double m_SizeRatioToL;
        List<Facility_Coordinate> view_fac = new List<Facility_Coordinate>();
        List<FacilityStatus> view_sta = new List<FacilityStatus>();
        private List<TouchPoint> touchPoints = new List<TouchPoint>();

        //private int? C_POSITION_WIDTH = int.Parse(ConfigurationManager.AppSettings["CAR_PAINTSIZE"]);
        //private const int C_POSITION_WIDTH = 10;
        private Int32 result;
        private const double C_RATE_LIMIT_MINIMUM = 0.1;
        private const double C_RATE_LIMIT_MAXIMUM = 4;
        private PointF send_cp = new PointF();

        private Station m_station = new Station();

        public delegate void FacilityCoordinateEventHundler(FacilityCoordinateEventArgs e);
        public event FacilityCoordinateEventHundler FacCoFormSend;

        public Facility_Coordinate onfacilityCoordinate = new Facility_Coordinate();

        public List<StatusDraw> m_statusdraws = new List<StatusDraw>()
        {
            new StatusDraw(0,Brushes.Gray),
            new StatusDraw(1,Brushes.LawnGreen),
            new StatusDraw(2,Brushes.Yellow),
            new StatusDraw(3,Brushes.Red),
            new StatusDraw(4,Brushes.Red),
            new StatusDraw(5,Brushes.Red),
            new StatusDraw(6,Brushes.LawnGreen),
            new StatusDraw(7,Brushes.Gray),
            new StatusDraw(99,Brushes.Red)
        };

        public ManageViewer()
        {
            InitializeComponent();
            int.TryParse(ConfigurationManager.AppSettings["CAR_PAINTSIZE"],out result);
            pnl_view_OnPaint += new PaintEventHandler(ManageViewer_OnPaint);
            pnl_view_OnMouseDown += new MouseEventHandler(ManageViewer_MouseDown);
            pnl_view_OnMouseMove += new MouseEventHandler(ManageViewer_MouseMove);
            pnl_view_OnMouseUp += new MouseEventHandler(ManageViewer_MouseUp);
            //this.pnl_view.MouseWheel += new MouseEventHandler(pnl_view_MouseWheel);
        }

        #region 描画
        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ManageViewer_OnPaint(object sender, PaintEventArgs e)
        {
            //if (BACKIMAGE != null)
            //{
            //    // 画像をぼかして描写
            //    ColorMatrix cm = new ColorMatrix
            //    {
            //        Matrix00 = 1,
            //        Matrix11 = 1,
            //        Matrix22 = 1,
            //        Matrix33 = 0.4F,
            //        Matrix44 = 1
            //    };
            //    ImageAttributes ia = new ImageAttributes();
            //    ia.SetColorMatrix(cm);
            //    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //    e.Graphics.DrawImage(BACKIMAGE, new Rectangle(0, 0, (int)Math.Round(BACKIMAGE.Width * m_imagerate), (int)Math.Round(BACKIMAGE.Height * m_imagerate)), 0, 0, BACKIMAGE.Width, BACKIMAGE.Height, GraphicsUnit.Pixel, ia);

            

            if(view_fac.Count > 0)
            {
                for(int i = view_fac.Count - 1; i >= 0; i--)
                {
                    if (view_fac[i].visible == 1)
                    {
                        paintFacilityCoordinate(view_fac[i], e);
                    }
                }
            }
            if (onfacilityCoordinate.FacilityName != null)
            {
                paintFacilityName(onfacilityCoordinate, e);
                //paintFacilityLight(onImagefacility);
            }

            //車両情報の描画
            if (m_CarStatusL.Count > 0)
            {
                for (int i = 0; i <= m_CarStatusL.Count - 1; i++)
                {
                    PaintCarStatus(m_CarStatusL[i], e);
                }
            }


            //}
        }

        /// <summary>
        /// 車両位置の描画
        /// </summary>
        /// <param name="status"></param>
        /// <param name="e"></param>
        private void PaintCarStatus(CarStatus status, PaintEventArgs e)
        {
            if (!status.Position.IsEmpty)
            {
                //車両位置
                StatusDraw statusDraw = GetCurrentStatusDraw(status.Status);
                e.Graphics.FillEllipse(statusDraw.color, status.Position.X - (float)(result / 2), status.Position.Y - (float)(result / 2), (float)result, (float)result);

                //ボデーNo
                using (Font font = new Font("MS UI Gothic", 8, FontStyle.Bold, GraphicsUnit.Point))
                {
                    Brush b = Brushes.Black;
                    float x = status.Position.X - (float)(result / 2) + (float)result;
                    SizeF sizeF = e.Graphics.MeasureString("A", font);
                    float y = status.Position.Y - (float)(result / 2) - sizeF.Height;
                    e.Graphics.DrawString(status.BodyNo, Font, b, new PointF(x, y));
                }
            }
        }
        #endregion

        private void paintFacilityCoordinate(Facility_Coordinate fac, PaintEventArgs e)
        {
            PointF p = toLocalCoordinate(new PointF(fac.Xcoordinate, fac.Ycoordinate));
            FacilityStatus facstatus = view_sta.Where(t => t.FacilitySID == fac.SID).SingleOrDefault();

            if (facstatus == null) { return; }

            float Imgrealsize_X;
            float Imgrealsize_Y;

            if (facstatus.FacilityTypeID == 0)
            {
                //Imgrealsize_X = C_IMAGESIZE_CARSIG_X * m_SizeRatioToG;
                //Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y * m_SizeRatioToG;
                Imgrealsize_X = C_IMAGESIZE_CARSIG_X;
                Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y;

            }
            else if (facstatus.FacilityTypeID == 1)
            {
                Imgrealsize_X = C_IMAGESIZE_WARKERSIG_X;
                Imgrealsize_Y = C_IMAGESIZE_WARKERSIG_Y;
            }
            else if (facstatus.FacilityTypeID == 2)
            {
                Imgrealsize_X = C_IMAGESIZE_SHUTTER_X;
                Imgrealsize_Y = C_IMAGESIZE_SHUTTER_Y;
            }
            else
            {
                Imgrealsize_X = C_IMAGESIZE_LIGHT_X;
                Imgrealsize_Y = C_IMAGESIZE_LIGHT_Y;
            }

            float x = p.X - Imgrealsize_X / 2;
            float y = p.Y - Imgrealsize_Y / 2;

            //float x = p.X - C_IMAGESIZE_WIDTH / 2;
            //float y = p.Y - C_IMAGESIZE_HIGHT / 2;
            if (facstatus == null)
            {
                if (fac.FacilityTypeID == 2)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_shatter_move_darkandred, x, y, Imgrealsize_X, Imgrealsize_Y);
                    //selectedImage_Paint(fac, Color.Red, e);
                }
                else if (fac.FacilityTypeID == 0)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_shatter_move_darkandred, x, y, Imgrealsize_X, Imgrealsize_Y);
                    //selectedImage_Paint(fac, Color.Red, e);
                }
                else if (fac.FacilityTypeID == 1)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_wakersig_nolightverticalandred, x, y, Imgrealsize_X, Imgrealsize_Y);
                    //selectedImage_Paint(fac, Color.Red, e);
                }
                else
                {
                    e.Graphics.DrawImage(Properties.Resources.nolightandred, x, y, Imgrealsize_X, Imgrealsize_Y);
                    //selectedImage_Paint(fac, Color.Red, e);
                }
                return;
            }
            if (fac.FacilityTypeID == 2)
            {
                if(facstatus.Status1 == 0)
                { 
                e.Graphics.DrawImage(Properties.Resources.bt_close_onlyimg_orange, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else if(facstatus.Status1 == 1)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_open_onlyimg_orange, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else if (facstatus.Status1 == 2)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_shatter_move_orange, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_shatter_move_darkandred_orange, x, y, Imgrealsize_X, Imgrealsize_Y);
                    //selectedImage_Paint(fac, Color.Red, e);
                }
            }
            else if (fac.FacilityTypeID == 0)
            {
                if(facstatus.Status1 == 0)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_carsig_blue_short, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else if(facstatus.Status1 == 1)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_carsig_red_short, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else if(facstatus.Status1 == 2)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_carsig_yellow_short, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_carsig_no_light_shortandred, x, y, Imgrealsize_X, Imgrealsize_Y);
                    //selectedImage_Paint(fac, Color.Red, e);
                }
            }
            else if (fac.FacilityTypeID == 1)
            {
                if(facstatus.Status1 == 0)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_wakersig_vertical, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else if(facstatus.Status1 == 3)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_wakersig_nolightvertical, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else if(facstatus.Status1 == 1)
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_wakersig_redvertical, x, y, Imgrealsize_X, Imgrealsize_Y);
                } 
                else
                {
                    e.Graphics.DrawImage(Properties.Resources.bt_wakersig_nolightverticalandred, x, y, Imgrealsize_X, Imgrealsize_Y);
                    //selectedImage_Paint(fac, Color.Red, e);
                }
            }
            else
            {
                if (facstatus.Status1 == 0)
                {
                    e.Graphics.DrawImage(Properties.Resources.nolight, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else if (facstatus.Status1 == 1)
                {
                    e.Graphics.DrawImage(Properties.Resources.greenlight, x, y, Imgrealsize_X, Imgrealsize_Y);
                }
                else
                {
                    e.Graphics.DrawImage(Properties.Resources.nolightandred, x, y, Imgrealsize_X, Imgrealsize_Y);
                    //selectedImage_Paint(fac, Color.Red, e);
                }
            }
        }
        public void selectedImage_Paint(Facility_Coordinate facCo, Color color, PaintEventArgs e)
        {
            PointF p = toLocalCoordinate(new PointF(facCo.Xcoordinate, facCo.Ycoordinate));
            float Imgrealsize_X;
            float Imgrealsize_Y;

            if (facCo.FacilityTypeID == 0)
            {
                //Imgrealsize_X = C_IMAGESIZE_CARSIG_X * m_SizeRatioToG;
                //Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y * m_SizeRatioToG;
                Imgrealsize_X = C_IMAGESIZE_CARSIG_X;
                Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y;

            }
            else if (facCo.FacilityTypeID == 1)
            {
                Imgrealsize_X = C_IMAGESIZE_WARKERSIG_X;
                Imgrealsize_Y = C_IMAGESIZE_WARKERSIG_Y;
            }
            else if (facCo.FacilityTypeID == 2)
            {
                Imgrealsize_X = C_IMAGESIZE_SHUTTER_X;
                Imgrealsize_Y = C_IMAGESIZE_SHUTTER_Y;
            }
            else
            {
                Imgrealsize_X = C_IMAGESIZE_LIGHT_X;
                Imgrealsize_Y = C_IMAGESIZE_LIGHT_Y;
            }

            float x = p.X - Imgrealsize_X / 2;
            float y = p.Y - Imgrealsize_Y / 2;
            Pen pen = new Pen(color, 1);
            e.Graphics.DrawRectangle(pen, x - 1, y - 1, Imgrealsize_X + 1, Imgrealsize_Y + 1);
        }

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="log"></param>
        /// <param name="filepath">画像ファイルパス</param>
        /// <param name="CommonCordinate">画像座標(右上)左下原点</param>
        /// <param name="terminals">制御端末情報</param>
        //public void FirstSet(AppLog2 log, string filepath, PointF CommonCordinate, List<TerminalStatus> terminals)
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
        /// 情報更新＋再描画
        /// </summary>
        /// <param name="state"></param>
        public void ReDraw(List<CarStatus> state)
        {

            //車両情報の更新
            m_CarStatus.Clear();
            m_CarStatusL.Clear();
            for (int i = 0; i < state.Count; i++)
            {
                m_CarStatus.Add((CarStatus)state[i].Clone());
                m_CarStatusL.Add((CarStatus)state[i].Clone());
                m_CarStatusL[i].Position = ToCommonCordinate(m_CarStatusL[i].Position);
            }

            //描画更新
            pnl_view.Refresh();
        }

        #region 内部処理
        public StatusDraw GetCurrentStatusDraw(int code)
        {
            return m_statusdraws.SingleOrDefault(status => status.code == code);
        }

        /// <summary>
        /// 画像に合わせてパネルのサイズ変更
        /// </summary>
        //private void PanelResize()
        //{
        //    Size ControlSize = new Size((int)Math.Round(BACKIMAGE.Width * m_imagerate), (int)Math.Round(BACKIMAGE.Height * m_imagerate));
        //    pnl_view.Size = ControlSize;
        //}

        //private void ChangeRatio()
        //{
        //    m_SizeRatioToL = (BACKIMAGE.Size.Width * m_imagerate) / m_CommonCordinate.X;
        //}

        //private void RaitoCheck(PointF CommonCordinate)
        //{
        //    if (BACKIMAGE.Size.Width / m_CommonCordinate.X != BACKIMAGE.Size.Height / m_CommonCordinate.Y)
        //    {
        //        LOGGER.Warn("画像と座標の比率が違います");
        //    }
        //}

        public PointF ToCommonCordinate(PointF p)
        {

            PointF point = PointF.Empty;
            PointF tpoint = PointF.Empty;
            if (!p.IsEmpty)
            {
                point = new PointF(p.X + m_station.XaxisMIN, p.Y + m_station.YaxisMIN);
                point = toLocalCoordinate(point);


                //foreach (TerminalStatus terminal in m_TerminalStatus)
                //{
                //    if (terminal.TerminalID == no)
                //    {
                //        tpoint = terminal.OriginPosition;
                //        break;
                //    }
                //}

                //point = new PointF(p.X + tpoint.X, p.Y + tpoint.Y);
                //point = toLocalCoordinate(point);
            }

            return point;
        }

        public void setFacilityCoordinate(List<Facility_Coordinate> fac)
        {
            view_fac = new List<Facility_Coordinate>();
            foreach(Facility_Coordinate o_o_vfac in fac)
            {
                view_fac.Add(o_o_vfac);
            }
        }

        //public PointF toLocalCoordinate(PointF p)
        //{
        //    PointF lp = new PointF(0, 0);

        //    //原点ずれ調整
        //    lp.X = (float)(p.X * m_SizeRatioToL);
        //    lp.Y = (float)(p.Y * m_SizeRatioToL);

        //    lp.Y = (float)(BACKIMAGE.Height * m_imagerate) - lp.Y;
        //    return lp;
        //}
        #endregion

        #region マウス操作
        private bool m_mousedown = false;
        private Point m_mousedown_point;
        private void ManageViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (BACKIMAGE != null)
            {
                //m_mousedown = true;

                //Point sp = Cursor.Position;
                //Point cp = this.PointToClient(sp);

                //m_mousedown_point = cp;

                Point sp = Cursor.Position;
                Point ssp = this.PointToClient(sp);
                PointF cp = (PointF)this.PointToClient(sp);
                cp.X = cp.X - pnl_view.Location.X;
                cp.Y = cp.Y - pnl_view.Location.Y;
                m_mousedown_point = ssp;

                //if( listToPnl )
                m_mousedown = true;
                //Point scrollPosition = this.AutoScrollPosition;
                //cp.X = cp.X - scrollPosition.X;
                //cp.Y = cp.Y - scrollPosition.Y;
                cp = (PointF)toGlobalCoordinate(cp);

                send_cp = cp;
            }
        }

        private void ChangeImage(Facility_Coordinate fac,PaintEventArgs e)
        {

            PointF p = toLocalCoordinate(new PointF(fac.Xcoordinate, fac.Ycoordinate));
            float x = p.X - C_IMAGESIZE_WIDTH / 2;
            float y = p.Y - C_IMAGESIZE_HIGHT / 2;
            if (fac.FacilityTypeID == 2)
            {
                e.Graphics.DrawImage(Properties.Resources.Map_shatter, x, y, C_IMAGESIZE_WIDTH, C_IMAGESIZE_HIGHT);
            }
            else if (fac.FacilityTypeID == 0)
            {
                e.Graphics.DrawImage(Properties.Resources.Map_trafficlights_three, x, y, C_IMAGESIZE_WIDTH, C_IMAGESIZE_HIGHT);
            }
            else if (fac.FacilityTypeID == 1)
            {
                e.Graphics.DrawImage(Properties.Resources.Map_trafficlights_three, x, y, C_IMAGESIZE_WIDTH, C_IMAGESIZE_HIGHT);
            }
            else
            {
                e.Graphics.DrawImage(Properties.Resources.Map_lights, x, y, C_IMAGESIZE_WIDTH, C_IMAGESIZE_HIGHT);
            }
        }

        private void ManageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (BACKIMAGE != null)
            {
                if (m_mousedown)
                {
                    Point sp = Cursor.Position;
                    Point cp = this.PointToClient(sp);

                    cp = MoveLimitCheck(cp);

                    int x = pnl_view.Location.X + cp.X - m_mousedown_point.X;
                    int y = pnl_view.Location.Y + cp.Y - m_mousedown_point.Y;

                    pnl_view.Location = new Point(x, y);

                    m_mousedown_point = cp;
                }
                else
                {
                    Point sp_over = Cursor.Position;
                    Point ssp_over = this.PointToClient(sp_over);
                    PointF cp_over = (PointF)this.PointToClient(sp_over);
                    cp_over.X = cp_over.X - pnl_view.Location.X;
                    cp_over.Y = cp_over.Y - pnl_view.Location.Y;

                    cp_over = (PointF)toGlobalCoordinate(cp_over);
                    onfacilityCoordinate = new Facility_Coordinate();
                    foreach (Facility_Coordinate kvp in view_fac)
                    {
                        PointF Imgrealsize = getImageSize(kvp.FacilityTypeID);
                        //float Imgrealsize = 40 * m_SizeRatioToG;
                        RectangleF round = new RectangleF((float)(kvp.Xcoordinate - (double)Imgrealsize.X / (double)2), (float)(kvp.Ycoordinate - (double)Imgrealsize.Y / (double)2), Imgrealsize.X, Imgrealsize.Y);

                        if (round.Contains(cp_over.X, cp_over.Y) && kvp.visible == 1)
                        {
                            onfacilityCoordinate = new Facility_Coordinate();
                            onfacilityCoordinate = kvp;
                            pnl_view.Refresh();
                            return;
                        }
                    }
                    pnl_view.Refresh();
                }
            }
        }

        private void ManageViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (BACKIMAGE != null)
            {
                m_mousedown = false;
                foreach (Facility_Coordinate o_o_vfac in view_fac)
                {
                    float Imgrealsize_X;
                    float Imgrealsize_Y;
                    if (o_o_vfac.FacilityTypeID == 0)
                    {
                        Imgrealsize_X = C_IMAGESIZE_CARSIG_X * m_SizeRatioToG;
                        Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y * m_SizeRatioToG;
                    }
                    else if(o_o_vfac.FacilityTypeID == 1)
                    {
                        Imgrealsize_X = C_IMAGESIZE_WARKERSIG_X * m_SizeRatioToG;
                        Imgrealsize_Y = C_IMAGESIZE_WARKERSIG_Y * m_SizeRatioToG;
                    }
                    else if(o_o_vfac.FacilityTypeID == 2)
                    {
                        Imgrealsize_X = C_IMAGESIZE_SHUTTER_X * m_SizeRatioToG;
                        Imgrealsize_Y = C_IMAGESIZE_SHUTTER_Y * m_SizeRatioToG;
                    }
                    else
                    {
                        Imgrealsize_X = C_IMAGESIZE_LIGHT_X * m_SizeRatioToG;
                        Imgrealsize_Y = C_IMAGESIZE_LIGHT_Y * m_SizeRatioToG;
                    }
                    //float Imgrealsize = 40 * m_SizeRatioToG;
                    RectangleF round = new RectangleF((float)(o_o_vfac.Xcoordinate - (double)Imgrealsize_X / (double)2), (float)(o_o_vfac.Ycoordinate - (double)Imgrealsize_Y / (double)2), Imgrealsize_X, Imgrealsize_Y);
                    //RectangleF round = new RectangleF((float)(o_o_vfac.facilityCoordinate_X), (float)(o_o_vfac.facilityCoordinate_Y), Imgrealsize_X, Imgrealsize_Y);
                    if (round.Contains(send_cp.X, send_cp.Y) && o_o_vfac.visible == 1)
                    {
                        sendFacCo(o_o_vfac);
                    }
                }
            }
        }

        public void setFacilityStatus(List<FacilityStatus> facsta)
        {
            view_sta = new List<FacilityStatus>();
            foreach (FacilityStatus o_o_vfacsta in facsta)
            {
                view_sta.Add(o_o_vfacsta);
            }
        }

        private void sendFacCo(Facility_Coordinate facco)
        {
            FacilityCoordinateEventArgs e = new FacilityCoordinateEventArgs
            {
                fac = facco,
                index = 1
            };
            if (FacCoFormSend != null) { FacCoFormSend(e); }
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

                //車両位置変更
                for (int i = 0; i < m_CarStatusL.Count; i++)
                {
                    m_CarStatusL[i].Position = ToCommonCordinate(m_CarStatus[i].Position);
                }
                //描画更新
                pnl_view.Refresh();
            }
        }

        public void setStation(Station _station)
        {
            m_station = _station;
        }

        // タッチ操作の情報を取得する関数
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize);

        // タッチ操作のハンドルを閉じる関数
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseTouchInputHandle(IntPtr lParam);

        // WndProcメソッドをオーバーライド
        protected override void WndProc(ref Message m)
        {
            // WM_TOUCHメッセージを処理
            if (m.Msg == 0x0240) // WM_TOUCH
            {
                int inputCount = (short)m.WParam.ToInt32();
                TOUCHINPUT[] inputs = new TOUCHINPUT[inputCount];
                if (GetTouchInputInfo(m.LParam, inputCount, inputs, Marshal.SizeOf(new TOUCHINPUT())))
                {
                    // タッチ操作の情報を処理（後述）
                    for (int i = 0; i < inputCount; i++)
                    {
                        Point pt = new Point(inputs[i].x, inputs[i].y);
                        int id = inputs[i].dwID;

                        if ((inputs[i].dwFlags & 0x02) != 0) // TOUCHEVENTF_DOWN
                        {
                            touchPoints.Add(new TouchPoint { ID = id, Position = pt, PreviousPosition = pt });
                        }
                        else if ((inputs[i].dwFlags & 0x04) != 0) // TOUCHEVENTF_UP
                        {
                            touchPoints.RemoveAll(p => p.ID == id);
                        }
                        else if ((inputs[i].dwFlags & 0x01) != 0) // TOUCHEVENTF_MOVE
                        {
                            var touchPoint = touchPoints.Find(p => p.ID == id);
                            if (touchPoint != null)
                            {
                                touchPoint.PreviousPosition = touchPoint.Position;
                                touchPoint.Position = pt;
                            }
                        }

                        if (touchPoints.Count == 2)
                        {
                            double deltaX1 = touchPoints[0].Position.X - touchPoints[0].PreviousPosition.X;
                            double deltaY1 = touchPoints[0].Position.Y - touchPoints[0].PreviousPosition.Y;
                            double deltaX2 = touchPoints[1].Position.X - touchPoints[1].PreviousPosition.X;
                            double deltaY2 = touchPoints[1].Position.Y - touchPoints[1].PreviousPosition.Y;

                            double deltaWidth = (Math.Abs(deltaX1) + Math.Abs(deltaX2)) / 2;
                            double deltaHeight = (Math.Abs(deltaY1) + Math.Abs(deltaY2)) / 2;

                            pnl_view.Width += (int)deltaWidth;
                            pnl_view.Height += (int)deltaHeight;
                        }
                    }
                }
                CloseTouchInputHandle(m.LParam);
            }
            else
            {
                base.WndProc(ref m);
            }
        }




        #endregion

        //private void pnl_view_MouseDown(object sender, MouseEventArgs e)
        //{

        //}

        //private void pnl_view_MouseMove(object sender, MouseEventArgs e)
        //{

        //}

        //private void pnl_view_MouseUp(object sender, MouseEventArgs e)
        //{

        //}
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

    public class TerminalStatus
    {
        /// <summary>
        /// 制御端末ID
        /// </summary>
        public string TerminalID { get; set; }
        /// <summary>
        /// 制御端末の原点位置
        /// </summary>
        public PointF OriginPosition { get; set; }
    }

    public class CarStatus : ICloneable
    {
        /// <summary>
        /// 制御端末ID
        /// </summary>
        //public string TerminalID { get; set; }
        /// <summary>
        /// ボデーNo.
        /// </summary>
        public string BodyNo { get; set; }
        /// <summary>
        /// 車両ステータス
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 車両位置
        /// </summary>
        public PointF Position { get; set; }

        public object Clone()
        {
            return (CarStatus)MemberwiseClone();
        }
    }

    public class FacilityCoordinateEventArgs
    {
        public Facility_Coordinate fac;
        public int index;
    }


    public class StatusDraw
    {
        public int code { get; private set; }
        public Brush color { get; private set; }

        public StatusDraw(int code, Brush color)
        {
            this.code = code;
            this.color = color;
        }
    }
}
