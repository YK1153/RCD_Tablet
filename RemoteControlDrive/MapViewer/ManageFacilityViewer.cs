using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using AtrptCmn;
using AtrptDao;
using static AtrptDao.MFacilityDao;
using System.Reflection;
using AtrptShare;

namespace MapViewer
{
    public partial class ManageFacilityViewer : ManageBace
    {
        //private AppLog2 LOGGER;
        //private Bitmap m_bitmap;
        //private Bitmap BACKIMAGE;
        private double m_imagerate = 1;
        private double m_ratechange = 0.1;

       // private List<TerminalStatus> m_TerminalStatus = new List<TerminalStatus>();
        private List<CarStatus> m_CarStatus = new List<CarStatus>();
        private List<CarStatus> m_CarStatusL = new List<CarStatus>();

        //private PointF m_CommonCordinate;
        //private double m_SizeRatioToL;
        //public float m_SizeRatioToG = 0;
        public float m_backimage_sizeh;
        public Facility listToPnl;

        private const int C_POSITION_WIDTH = 10;

        private const double C_RATE_LIMIT_MINIMUM = 0.1;
        private const double C_RATE_LIMIT_MAXIMUM = 4;
        private const int C_NODEVAL_DIGIT = 3;

        public int m_SelectedImage = -1;
        private PointF m_ClickCorrection = new PointF(0, 0);
        public bool dataGridSelect = false;
        private int first_Key;

        private receive_Viewer recView = new receive_Viewer();
        private MFacilityDao mFacilityDao;// = new AtrptDao.MFacilityDao();
        List<Facility_Coordinate> FacCoords = new List<Facility_Coordinate>();
        List<Facility> exfac = new List<Facility>();
        public int m_SelectedSid;
        bool imgClickFlg = false;

        public event PaintEventHandler drowacilityimage;

        public delegate void FacilityImageEventHundler(FacilityImageEventArgs e);

        public event FacilityImageEventHundler ImgAdd;
        public event FacilityImageEventHundler ImgChanged;
        public event FacilityImageEventHundler ImgSelectChanged;

        private DMngModeDao m_DMngModeDao;

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

        public ManageFacilityViewer()
        {
            InitializeComponent();
            this.pnl_view_OnPaint += new PaintEventHandler(ManageFacilityViewer_OnPaint);
            this.pnl_view_OnMouseDown += new MouseEventHandler(ManageFacilityViewer_MouseDown);
            this.pnl_view_OnMouseMove += new MouseEventHandler(ManageFacilityViewer_MouseMove);
            this.pnl_view_OnMouseUp += new MouseEventHandler(ManageFacilityViewer_MouseUp);

            //mFacilityDao = new AtrptDao.MFacilityDao();

            //FacCoords = mFacilityDao.GetALLFacilityCoordinate();
            //first_Key = 0;
            //foreach(Facility_Coordinate facility_Coordinate in FacCoords)
            //{
            //    if(facility_Coordinate.visible)
            //    {
            //        receive_Viewer TEMP_recview = new receive_Viewer();
            //        TEMP_recview = TEMP_recview.setRecView(facility_Coordinate);
            //        receive_Viewers.Add(first_Key, TEMP_recview);
            //        first_Key++;
            //    }
            //}

            //this.pnl_view.MouseWheel += new MouseEventHandler(pnl_view_MouseWheel);
        }

        #region 描画
        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ManageFacilityViewer_OnPaint(object sender, PaintEventArgs e)
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

            //車両情報の描画
            //if (m_CarStatusL.Count > 0)
            //{
            //    for (int i = 0; i <= m_CarStatusL.Count - 1; i++)
            //    {
            //        PaintCarStatus(m_CarStatusL[i], e);
            //    }
            //}
            //}

            //if(!(recView.facilityName == null))
            //{
            //    Facility_Coordinate temp_FACO = new Facility_Coordinate();

            //    temp_FACO = FacCoords.SingleOrDefault(f => f.SID == recView.ID);
            //    temp_FACO.visible = true;
            //    temp_FACO.facilityCoordinate_X = recView.facilityCoordinate_X;
            //    temp_FACO.facilityCoordinate_Y = recView.facilityCoordinate_Y;
            //}

            //foreach(Facility_Coordinate facility_Coordinate in FacCoords)
            //{
            //    if(facility_Coordinate.visible)
            //    {
            //        recView = recView.setRecView(facility_Coordinate);
            //        facilityImage_Paint(recView, e);
            //    }
            //}
            Color blue = new Color();
            blue = Color.Blue;
            for(int i = receive_Viewers.Count - 1; i >= 0; i--)
                    
            if(receive_Viewers[i].visible == 1)
            {
                facilityImage_Paint(receive_Viewers[i], e);
                if(receive_Viewers[i].SID == m_SelectedSid)
                {
                    selectedImage_Paint(receive_Viewers[i], blue, e);
                }
            }
            if (onImagefacility.facilityName != null)
            {
                paintFacilityName(onImagefacility, e);
                //paintFacilityLight(onImagefacility);
            }
            //foreach(int key in )
            //receive_Viewers.Add()

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
                e.Graphics.FillEllipse(statusDraw.color, status.Position.X - (float)(C_POSITION_WIDTH / 2), status.Position.Y - (float)(C_POSITION_WIDTH / 2), (float)C_POSITION_WIDTH, (float)C_POSITION_WIDTH);

                //ボデーNo
                using (Font font = new Font("MS UI Gothic", 8, FontStyle.Bold, GraphicsUnit.Point))
                {
                    Brush b = Brushes.Black;
                    float x = status.Position.X - (float)(C_POSITION_WIDTH / 2) + (float)C_POSITION_WIDTH;
                    SizeF sizeF = e.Graphics.MeasureString("A", font);
                    float y = status.Position.Y - (float)(C_POSITION_WIDTH / 2) - sizeF.Height;
                    e.Graphics.DrawString(status.BodyNo, Font, b, new PointF(x, y));
                }
            }
        }
        #endregion

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
                m_CarStatusL[i].Position = ToCommonCordinate(m_CarStatusL[i].Position, m_CarStatusL[i].TerminalID);
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
        //    m_SizeRatioToG = m_CommonCordinate.X / (BACKIMAGE.Size.Width * (float)m_imagerate);
        //}

        //private void RaitoCheck(PointF CommonCordinate)
        //{
        //    if (BACKIMAGE.Size.Width / m_CommonCordinate.X != BACKIMAGE.Size.Height / m_CommonCordinate.Y)
        //    {
        //        LOGGER.Warn("画像と座標の比率が違います");
        //    }
        //}

        public PointF ToCommonCordinate(PointF p, string no)
        {

            PointF point = PointF.Empty;
            PointF tpoint = PointF.Empty;
            if (!p.IsEmpty)
            {
                foreach (TerminalStatus terminal in m_TerminalStatus)
                {
                    if (terminal.TerminalID == no)
                    {
                        tpoint = terminal.OriginPosition;
                        break;
                    }
                }

                point = new PointF(p.X + tpoint.X, p.Y + tpoint.Y);
                point = toLocalCoordinate(point);
            }

            return point;
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
        private void ManageFacilityViewer_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                //クリック位置の補正と変換
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
                
                foreach (KeyValuePair<int, receive_Viewer> kvp in receive_Viewers)
                {
                    //float Imgrealsize_X;
                    //float Imgrealsize_Y;
                    PointF Imgrealsize = new PointF();

                    Imgrealsize = getImageSize(kvp.Value.facilityTypeID);

                    //if (kvp.Value.facilityTypeID == 0)
                    //{
                    //    Imgrealsize_X = C_IMAGESIZE_CARSIG_X * m_SizeRatioToG;
                    //    Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y * m_SizeRatioToG;
                    //}
                    //else if (kvp.Value.facilityTypeID == 1)
                    //{
                    //    Imgrealsize_X = C_IMAGESIZE_WARKERSIG_X * m_SizeRatioToG;
                    //    Imgrealsize_Y = C_IMAGESIZE_WARKERSIG_Y * m_SizeRatioToG;
                    //}
                    //else if (kvp.Value.facilityTypeID == 2)
                    //{
                    //    Imgrealsize_X = C_IMAGESIZE_SHUTTER_X * m_SizeRatioToG;
                    //    Imgrealsize_Y = C_IMAGESIZE_SHUTTER_Y * m_SizeRatioToG;
                    //}
                    //else
                    //{
                    //    Imgrealsize_X = C_IMAGESIZE_LIGHT_X * m_SizeRatioToG;
                    //    Imgrealsize_Y = C_IMAGESIZE_LIGHT_Y * m_SizeRatioToG;
                    //}
                    //float Imgrealsize = 40 * m_SizeRatioToG;
                    RectangleF round = new RectangleF((float)(kvp.Value.facilityCoordinate_X - (double)Imgrealsize.X / (double)2), (float)(kvp.Value.facilityCoordinate_Y - (double)Imgrealsize.Y / (double)2), Imgrealsize.X, Imgrealsize.Y);
                    if (round.Contains(cp.X, cp.Y) && kvp.Value.visible == 1)
                    {
                        m_ClickCorrection = new PointF(kvp.Value.facilityCoordinate_X - cp.X, kvp.Value.facilityCoordinate_Y - cp.Y);
                        m_SelectedImage = kvp.Key;
                        m_SelectedSid = kvp.Value.SID;
                        //画像クリック
                        imgClickFlg = true;
                        // ノード選択イベント発生
                        FacilityImageEventArgs facilityImg = new FacilityImageEventArgs
                        {
                            rec = (receive_Viewer)kvp.Value.Clone(),
                            index = receive_Viewers.IndexOfKey(kvp.Key)
                        };
                        PointF p = new PointF (facilityImg.rec.facilityCoordinate_X, facilityImg.rec.facilityCoordinate_Y);
                        p = point_substr(p, C_NODEVAL_DIGIT);
                        facilityImg.rec.facilityCoordinate_X = p.X;
                        facilityImg.rec.facilityCoordinate_Y = p.Y;
                        if (ImgSelectChanged != null) { ImgSelectChanged(facilityImg); }

                        this.Refresh();
                        return;
                    }
                }

                if (dataGridSelect)
                {
                    recView.facilityCoordinate_X = cp.X;
                    recView.facilityCoordinate_Y = cp.Y;
                    AddImage(cp.X, cp.Y);
                    this.Refresh();
                    //DrawImageView(Properties.Resources.Map_shatter, rectangle);
                }

                //switch (m_Mode)
                //{
                //    case AtrptMode.Route:
                //        MouseDown_Route(cp, e);
                //        break;
                //    case AtrptMode.Other:
                //        MouseDown_Other(cp, e);
                //        break;
                //    case AtrptMode.OtherAdd:
                //        MouseDown_OtherAdd(cp, e);
                //        break;
                //}
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }



        public void AddImage(float x, float y)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                int key = 1;

                //現在設定されているKeyの最大値の＋１をKeyに設定
                key = (receive_Viewers.Count > 0) ? receive_Viewers.Keys.Last() + 1 : 1;

                //要素のセット
                //PointF p = toGlobalCoordinate(new PointF(x, y));
                PointF p = new PointF(x, y);
                p = point_substr(p, C_NODEVAL_DIGIT);
                receive_Viewer setcoord = new receive_Viewer
                {
                    SID = recView.SID,
                    facilityTypeID = recView.facilityTypeID,
                    facilityCoordinate_X = p.X,
                    facilityCoordinate_Y = p.Y,
                    facilityName = recView.facilityName,
                    visible = 1
                };
                //ノードの追加
                receive_Viewer o_o_t_m_rec = new receive_Viewer();
                o_o_t_m_rec = receive_Viewers.Values.SingleOrDefault(t => t.SID == setcoord.SID);
                if (o_o_t_m_rec == null)
                {
                    receive_Viewers.Add(key, setcoord);
                    m_SelectedImage = key;
                }
                else if(o_o_t_m_rec.visible == 0)
                {
                    o_o_t_m_rec.facilityCoordinate_X = setcoord.facilityCoordinate_X;
                    o_o_t_m_rec.facilityCoordinate_Y = setcoord.facilityCoordinate_Y;
                    o_o_t_m_rec.visible = setcoord.visible;
                }
                //ノード追加イベント発生
                FacilityImageEventArgs e = new FacilityImageEventArgs
                {
                    rec = (receive_Viewer)setcoord.Clone(),
                    index = receive_Viewers.IndexOfKey(key)
                };
                p = new PointF(e.rec.facilityCoordinate_X, e.rec.facilityCoordinate_Y);
                //p = point_substr(toGlobalCoordinate(p), C_NODEVAL_DIGIT);
                p = point_substr(p, C_NODEVAL_DIGIT);
                e.rec.facilityCoordinate_X = p.X;
                e.rec.facilityCoordinate_Y = p.Y;
                if (ImgAdd != null) { ImgAdd(e); }

                //エリアの追加
                //if (RouteNodes.Count - 1 > 0) { AddSpeedArea(); }

                //追加したノードを選択状態にする
                //m_EditMode = AtrptEditmode.Node;
                
                ////ノード選択イベント発生
                //e.rn = (RouteNode)RouteNodes[key].Clone();
                //e.index = RouteNodes.IndexOfKey(key);
                //p = new PointF(e.rn.X, e.rn.Y);
                //p = point_substr(p, C_NODEVAL_DIGIT);
                //e.rn.X = p.X;
                //e.rn.Y = p.Y;
                //if (NodeSelectChanged != null) { NodeSelectChanged(e); }

                this.Refresh();

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        public void UpdateImage(int key, float x, float y)
        {
            try
            {
                // 値の更新
                PointF p = new PointF(x, y);
                p = point_substr(p, C_NODEVAL_DIGIT);
                receive_Viewers[key].facilityCoordinate_X = p.X;
                receive_Viewers[key].facilityCoordinate_Y = p.Y;

                //保留
                //recView.facilityCoordinate_X = x;
                //recView.facilityCoordinate_Y = y;
                

                // ノード変更イベント発生
                FacilityImageEventArgs facilityImg = new FacilityImageEventArgs
                { 
                    rec = (receive_Viewer)receive_Viewers[key].Clone(),
                    index = receive_Viewers.IndexOfKey(key)
                };
                
                //facilityImg.rec.facilityCoordinate_X = p.X;
                //facilityImg.rec.facilityCoordinate_Y = p.Y;
                if (ImgChanged != null) { ImgChanged(facilityImg); }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { }
        }

        
        //private void ManageViewer_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (BACKIMAGE != null)
        //    {
        //        m_mousedown = true;

        //        Point sp = Cursor.Position;
        //        Point cp = this.PointToClient(sp);

        //        m_mousedown_point = cp;
        //    }
        //}

        //private void ManageFacilityViewer_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (BACKIMAGE != null)
        //    {
        //        if (m_mousedown)
        //        {
        //            Point sp = Cursor.Position;
        //            Point cp = this.PointToClient(sp);

        //            cp = MoveLimitCheck(cp);

        //            int x = pnl_view.Location.X + cp.X - m_mousedown_point.X;
        //            int y = pnl_view.Location.Y + cp.Y - m_mousedown_point.Y;

        //            pnl_view.Location = new Point(x, y);

        //            m_mousedown_point = cp;
        //        }
        //    }
        //}

        private void ManageFacilityViewer_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (m_mousedown && imgClickFlg)
                {
                    Point sp = Cursor.Position;
                    Point ssp = this.PointToClient(sp);
                    PointF cp = (PointF)this.PointToClient(sp);
                    cp.X = cp.X - pnl_view.Location.X;
                    cp.Y = cp.Y - pnl_view.Location.Y;
                    cp = toGlobalCoordinate(cp);
                 
                    // 画像の移動
                    PointF node = new PointF();
                    node = PointF.Add(cp, new SizeF(m_ClickCorrection));
                    node = MoveLimits(node);
                    // 画像の更新
                    UpdateImage(m_SelectedImage, node.X, node.Y);
                    this.Refresh();
                }
                else if(m_mousedown)
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
                    onImagefacility = new receive_Viewer();
                    foreach (KeyValuePair<int, receive_Viewer> kvp in receive_Viewers)
                    {
                        PointF Imgrealsize = getImageSize(kvp.Value.facilityTypeID);

                        //float Imgrealsize = 40 * m_SizeRatioToG;
                        RectangleF round = new RectangleF((float)(kvp.Value.facilityCoordinate_X - (double)Imgrealsize.X / (double)2), (float)(kvp.Value.facilityCoordinate_Y - (double)Imgrealsize.Y / (double)2), Imgrealsize.X, Imgrealsize.Y);

                        if (round.Contains(cp_over.X, cp_over.Y) && kvp.Value.visible == 1)
                        {
                            onImagefacility = new receive_Viewer();
                            onImagefacility = kvp.Value;
                            m_mouseHover = 1;
                            pnl_view.Refresh();
                            return;
                        }
                    }
                    pnl_view.Refresh();
                }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { }
        }

        //private void ManageFacilityViewer_MouseUp(object sender, MouseEventArgs e)
        //{
        //    if (BACKIMAGE != null)
        //    {
        //        m_mousedown = false;
        //    }
        //}

        private void ManageFacilityViewer_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                m_mousedown = false;
                imgClickFlg = false;
                dataGridSelect = false;

                        //if (m_EditMode == AtrptEditmode.Node)
                        //{
                        //    if (m_SelectedNode >= 0)
                        //        Focus();
                        //}
                        //else if (m_EditMode == AtrptEditmode.SpeedArea)
                        //{
                        //    if (m_SelectedArea >= 0)
                        //        Focus();
                        //}

                this.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
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
                    m_CarStatusL[i].Position = ToCommonCordinate(m_CarStatus[i].Position, m_CarStatus[i].TerminalID);
                }
                //描画更新
                pnl_view.Refresh();
            }
        }
        #endregion

        private PointF MoveLimits(PointF node)
        {
            // X軸
            if (node.X <= 0)
                node.X = 0;
            else if (node.X >= m_CommonCordinate.X)
                node.X = m_CommonCordinate.X;
            else
                node.X = node.X;
            // Y軸
            if (node.Y <= 0)
                node.Y = 0;
            else if (node.Y >= m_CommonCordinate.Y)
                node.Y = m_CommonCordinate.Y;
            else
                node.Y = node.Y;

            return node;
        }

        private void pnl_view_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void pnl_view_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void pnl_view_MouseUp(object sender, MouseEventArgs e)
        {

        }

        public void dgvtoviewer(DataGridViewCellCollection dataGridViewCellCollection)
        {
            //recView.setrec("", "", "", "", "");
            recView.SID = int.Parse(dataGridViewCellCollection[5].Value.ToString());
            recView.facilityName = dataGridViewCellCollection[1].Value.ToString();
            recView.facilityTypeID = int.Parse(dataGridViewCellCollection[2].Value.ToString());
            recView.facilityCoordinate_X = 0;
            recView.facilityCoordinate_Y = 0;
            dataGridSelect = true;
            //if (!confirmationrecview(recView.SID))
            //{
            //    dataGridSelect = true;
            //}
            //recView.facilityTypeID = int.Parse(dataGridViewCellCollection[2].Value.ToString());
            //recView.facilityCoordinate_X = (float)dataGridViewCellCollection[3].Value;
            //recView.facilityCoordinate_Y = (float)dataGridViewCellCollection[4].Value;
            //recView.facilityTypeID = dataGridViewCellCollection[2].Value.ToString();
            //recView.facilityCoordinate_X = dataGridViewCellCollection[3].Value.ToString();
            //recView.facilityCoordinate_Y = dataGridViewCellCollection[4].Value.ToString();
        }
        public List<Facility_Coordinate> listToForm()
        {
            //recView.setrec("", "", "", "", "");
            List<Facility_Coordinate> listfac = new List<Facility_Coordinate>();
            foreach(receive_Viewer o_o_rec in receive_Viewers.Values)
            {
                Facility_Coordinate o_o_fac = new Facility_Coordinate();
                PointF transCoord = new PointF(o_o_rec.facilityCoordinate_X, o_o_rec.facilityCoordinate_Y);
                transCoord = point_substr(transCoord, C_NODEVAL_DIGIT);
                o_o_fac.SID = o_o_rec.SID;
                o_o_fac.facilityCoordinate_X = transCoord.X;
                o_o_fac.facilityCoordinate_Y = transCoord.Y;
                o_o_fac.FacilityName = o_o_rec.facilityName;
                o_o_fac.FacilityTypeID = o_o_rec.facilityTypeID;
                o_o_fac.visible = o_o_rec.visible;
                listfac.Add(o_o_fac);
            }
            return listfac;
            //recView.facilityTypeID = int.Parse(dataGridViewCellCollection[2].Value.ToString());
            //recView.facilityCoordinate_X = (float)dataGridViewCellCollection[3].Value;
            //recView.facilityCoordinate_Y = (float)dataGridViewCellCollection[4].Value;
            //recView.facilityTypeID = dataGridViewCellCollection[2].Value.ToString();
            //recView.facilityCoordinate_X = dataGridViewCellCollection[3].Value.ToString();
            //recView.facilityCoordinate_Y = dataGridViewCellCollection[4].Value.ToString();
        }
        public int deleteFormForm()
        {
            //receive_Viewers[m_SelectedImage].visible = false;
            //receive_Viewers[m_SelectedImage].facilityCoordinate_X = 0;
            //receive_Viewers[m_SelectedImage].facilityCoordinate_Y = 0;
            ////recView.setrec("", "", "", "", "");
            //List<Facility_Coordinate> listfac = new List<Facility_Coordinate>();
            //Facility_Coordinate o_o_fac = new Facility_Coordinate();
            //foreach (receive_Viewer o_o_rec in receive_Viewers.Values)
            //{
            //    PointF transCoord = new PointF(o_o_rec.facilityCoordinate_X, o_o_rec.facilityCoordinate_Y);
            //    transCoord = point_substr(transCoord, C_NODEVAL_DIGIT);
            //    o_o_fac.SID = o_o_rec.ID;
            //    o_o_fac.facilityCoordinate_X = transCoord.X;
            //    o_o_fac.facilityCoordinate_Y = transCoord.Y;
            //    o_o_fac.FacilityName = o_o_rec.facilityName;
            //    o_o_fac.FacilityTypeID = o_o_rec.facilityTypeID;
            //    listfac.Add(o_o_fac);
            //}
            //return listfac;
            //recView.facilityTypeID = int.Parse(dataGridViewCellCollection[2].Value.ToString());
            //recView.facilityCoordinate_X = (float)dataGridViewCellCollection[3].Value;
            //recView.facilityCoordinate_Y = (float)dataGridViewCellCollection[4].Value;
            //recView.facilityTypeID = dataGridViewCellCollection[2].Value.ToString();
            //recView.facilityCoordinate_X = dataGridViewCellCollection[3].Value.ToString();
            //recView.facilityCoordinate_Y = dataGridViewCellCollection[4].Value.ToString();
            return 0;
        }

        private void ManageFacilityViewer1_Load(object sender, EventArgs e)
        {
            //mFacilityDao = new MFacilityDao();
            //FacCoords = new MFacilityDao().GetALLFacilityCoordinate();
            //first_Key = 0;
            //foreach (Facility_Coordinate facility_Coordinate in FacCoords)
            //{
            //    if (facility_Coordinate.visible)
            //    {
            //        receive_Viewer TEMP_recview = new receive_Viewer();
            //        TEMP_recview = TEMP_recview.setRecView(facility_Coordinate);
            //        receive_Viewers.Add(first_Key, TEMP_recview);
            //        first_Key++;
            //    }
            //}
        }

        public void alterDaoFromForm(Facility_Coordinate facility_Coordinate)
        {
            //mFacilityDao = new MFacilityDao();
            //if (facility_Coordinate.visible == 1)
            //{
                receive_Viewer TEMP_recview = new receive_Viewer();
                TEMP_recview = SetRecView(facility_Coordinate);
                receive_Viewers.Add(first_Key, TEMP_recview);
                first_Key++;
            //}
            this.Refresh();
        }

        public List<receive_Viewer> plzRecView()
        {
            List<receive_Viewer> trans_View = new List<receive_Viewer>();
            foreach(receive_Viewer p_o_recviw in receive_Viewers.Values)
            {
                trans_View.Add(p_o_recviw);
            }
            return trans_View;
        }
        public void SelectedImage(int sid)
        {
            if(confirmationrecview(sid))
            {
                m_SelectedImage = receive_Viewers.IndexOfKey(receive_Viewers.IndexOfValue(receive_Viewers.Values.SingleOrDefault(t => t.SID == sid)));
            }
            else
            {
                if (receive_Viewers.Keys.Count == 0)
                {
                    m_SelectedImage = 1;
                }
                else
                {
                    m_SelectedImage = receive_Viewers.Keys.Last() + 1;
                }
            }
        }

        public bool confirmationrecview(int sid)
        {
            if(receive_Viewers.Values.SingleOrDefault(t => t.SID == sid) == null)
            {
                return false;
            }else
            {
                return true;
            }
        }
        public void selectedImage_Paint(receive_Viewer Paintrec,Color color ,PaintEventArgs e)
        {
            PointF p = toLocalCoordinate(new PointF(Paintrec.facilityCoordinate_X, Paintrec.facilityCoordinate_Y));

            float Imgrealsize_X;
            float Imgrealsize_Y;
            if (Paintrec.facilityTypeID == 0)
            {
                //Imgrealsize_X = C_IMAGESIZE_CARSIG_X * m_SizeRatioToG;
                //Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y * m_SizeRatioToG;
                Imgrealsize_X = C_IMAGESIZE_CARSIG_X;
                Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y;
            }
            else if (Paintrec.facilityTypeID == 1)
            {
                Imgrealsize_X = C_IMAGESIZE_WARKERSIG_X;
                Imgrealsize_Y = C_IMAGESIZE_WARKERSIG_Y;
            }
            else if (Paintrec.facilityTypeID == 2)
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
            if (Paintrec.facilityTypeID == 2)
            {
                e.Graphics.DrawImage(Properties.Resources.bt_open_onlyimgandblue_orange_lightgray, x, y, Imgrealsize_X, Imgrealsize_Y);
            }
            else if (Paintrec.facilityTypeID == 0)
            {
                e.Graphics.DrawImage(Properties.Resources.bt_carsig_blue_shortandblue, x, y, Imgrealsize_X, Imgrealsize_Y);
            }
            else if (Paintrec.facilityTypeID == 1)
            {
                e.Graphics.DrawImage(Properties.Resources.bt_wakersig_verticalandblue, x, y, Imgrealsize_X, Imgrealsize_Y);
            }
            else
            {
                e.Graphics.DrawImage(Properties.Resources.greenlightandblue, x, y, Imgrealsize_X, Imgrealsize_Y);
            }
            //PointF p = toLocalCoordinate(new PointF(Paintrec.facilityCoordinate_X, Paintrec.facilityCoordinate_Y));

            //float Imgrealsize_X;
            //float Imgrealsize_Y;
            //if (Paintrec.facilityTypeID == 0)
            //{
            //    //Imgrealsize_X = C_IMAGESIZE_CARSIG_X * m_SizeRatioToG;
            //    //Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y * m_SizeRatioToG;
            //    Imgrealsize_X = C_IMAGESIZE_CARSIG_X;
            //    Imgrealsize_Y = C_IMAGESIZE_CARSIG_Y;
            //}
            //else if (Paintrec.facilityTypeID == 1)
            //{
            //    Imgrealsize_X = C_IMAGESIZE_WARKERSIG_X;
            //    Imgrealsize_Y = C_IMAGESIZE_WARKERSIG_Y;
            //}
            //else if (Paintrec.facilityTypeID == 2)
            //{
            //    Imgrealsize_X = C_IMAGESIZE_SHUTTER_X;
            //    Imgrealsize_Y = C_IMAGESIZE_SHUTTER_Y;
            //}
            //else
            //{
            //    Imgrealsize_X = C_IMAGESIZE_LIGHT_X;
            //    Imgrealsize_Y = C_IMAGESIZE_LIGHT_Y;
            //}

            //float x = p.X - Imgrealsize_X / 2;
            //float y = p.Y - Imgrealsize_Y / 2;
            //Pen pen = new Pen(color, 1);
            //e.Graphics.DrawRectangle(pen, x - 1, y - 1, Imgrealsize_X + 1, Imgrealsize_Y + 1);
        }

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
