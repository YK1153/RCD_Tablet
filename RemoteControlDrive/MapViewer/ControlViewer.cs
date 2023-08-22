using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RcdCmn;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Threading;
using AtrptShare;
using AtrptCalc;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlClient;

namespace MapViewer
{
    public partial class ControlViewer : ViewerBace
    {
        public List<InstrusionDetection> m_detectdatas = new List<InstrusionDetection>();

        #region 変数
        /// <summary>
        /// 車の位置情報等の情報を蓄積する
        /// </summary>
        private List<CarInfo> m_CarStatusList = new List<CarInfo>();
        /// <summary>
        /// ローカル座標に変換した車の位置情報を蓄積する
        /// </summary>
        private List<PointF> m_CarStatusListLPoint = new List<PointF>();
        /// <summary>
        /// 描画用に制御点を保持する
        /// </summary>
        private List<PointF> m_CarYawrateLPoint = new List<PointF>();
        /// <summary>
        /// 車へ指示する情報を入れる
        /// </summary>
        public OrderInfo m_CarOrder = new OrderInfo();
        /// <summary>
        /// 進行方向角度を求める底辺
        /// </summary>
        private double m_base;
        /// <summary>
        /// 進行方向角度を求める高さ
        /// </summary>
        private double m_Vertical;
        /// <summary>
        /// 現在予測点
        /// </summary>
        private PointF m_newPPoint = new PointF();
        /// <summary>
        /// 現在予測点(ローカル座標)
        /// </summary>
        private PointF m_newLPPoint = new PointF();
        /// <summary>
        /// 測位点前回取得値
        /// </summary>
        private PointF m_OldPosition = new PointF();
        /// <summary>
        /// ヨーレート積算値(進行方向)
        /// </summary>
        private double m_oRad = 0;
        /// <summary>
        /// ヨーレート基準値
        /// </summary>
        //private double m_YawrateStandart = 0;
        /// <summary>
        /// 現在車速
        /// </summary>
        private double m_CarSpeed = 0;
        /// <summary>
        /// 現在舵角
        /// </summary>
        private double m_CarDakaku = 0;
        /// <summary>
        /// 異常値測位点
        /// </summary>
        private PointF m_ErrPoint;
        /// <summary>
        /// 異常値測位点(制御点)
        /// </summary>
        private PointF m_ErrPoint1;
        /// <summary>
        /// 異常値測位点取得有無 True:取得範囲外
        /// </summary>
        private Boolean m_ErrPFlg = false;
        /// <summary>
        /// 連続異常測位点カウント
        /// </summary>
        private int m_ErrCnt = 0;
        /// <summary>
        /// ヨーレート取得時間
        /// </summary>
        //private double m_YawrateTime = 0;
        /// <summary>
        /// 
        /// </summary>
        private int m_rad_flg = 0;
        /// <summary>
        /// 
        /// </summary>
        private double m_SetSpeed;
        /// <summary>
        /// 舵角描画を行うか true:描画する
        /// </summary>
        public bool m_draw_Dakaku = true;
        /// <summary>
        /// 測位点取得範囲の描画を行うか true:描画する
        /// </summary>
        public bool m_draw_GetArea = false;
        /// <summary>
        /// 
        /// </summary>
        //private StringFormat m_StringFormat = new StringFormat();

        private CarSpec carSpec = new CarSpec();

        private double carAngle = new double();

        private string[] cornername = { "左後端", "左前端" ,"右前端","右後端"};

        public List<bool> m_DrawOtherArea = new List<bool>();

        public int m_contain_idx = -1;

        private bool m_CarOrderSet;

        List<CarCornerinfo> m_CarcornerInfo = new List<CarCornerinfo>();

        SafetyMonitoringIndex m_SafetyMonitoringIndex = new SafetyMonitoringIndex();

        #endregion

        #region 定数
        /// <summary>
        /// 軌跡線の太さ
        /// </summary>
        private const int C_PASSEDROAD_WIDTH = 1;
        #endregion

        public ControlViewer()
        {
            InitializeComponent();
            this.pnl_view_OnPaint += new PaintEventHandler(OnPaint);
        }

        #region 描画情報取得
        /// <summary>
        ///  
        ///  </summary>
        ///  <param name=""></param>
        public void SetDrawInfo(CarInfo info, AVPAppOutputData avpappOutputData, int nowidx, List<CarCornerinfo> carCornerinfos,VehicleData vehicleData,SafetyMonitoringIndex safetyindex)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            // 車のステータスセット
            //PointF p = new PointF(info.xPosition, info.yPosition);
            PointF p1 = new PointF((float)avpappOutputData.control_point.x, (float)avpappOutputData.control_point.y);

            m_oRad = avpappOutputData.draw_info.yawrate;
            m_CarSpeed = vehicleData.vehicle_speed;
            m_CarDakaku = vehicleData.steer_angle;

            //測位点取得(追加)
            AddCarInfo(info, p1);
            
            m_newPPoint = p1;
            m_newLPPoint = toLocalCoordinate(p1); ;
            m_CarYawrateLPoint.Add(m_newLPPoint);

            SetOrder(avpappOutputData, nowidx);

            lock (m_CarcornerInfo)
            {
                m_CarcornerInfo.Clear();
                foreach (CarCornerinfo cornerinfo in carCornerinfos)
                {
                    m_CarcornerInfo.Add(cornerinfo);
                }
            }

            m_SafetyMonitoringIndex = (SafetyMonitoringIndex)safetyindex.Clone();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #region AddCarInfo
        /// <summary>
        /// 測位点を取得する
        /// </summary>
        /// <param name="info"></param>
        /// <param name="p"></param>
        private void AddCarInfo(CarInfo info, PointF p)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            CarInfo CarStatus = new CarInfo();
            CarStatus.angle = info.angle;
            CarStatus.xPosition = p.X;
            CarStatus.yPosition = p.Y;
            CarStatus.speed = info.speed;
            CarStatus.Yawrate = info.Yawrate;
            CarStatus.YawrateTime = info.YawrateTime;
            PointF LPoint = toLocalCoordinate(new PointF(p.X, p.Y));

            m_CarStatusList.Add(CarStatus);
            m_CarStatusListLPoint.Add(LPoint);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region SetOrder
        /// <summary>
        /// 指示情報をセット＋再描画
        /// </summary>
        /// <param name="order"></param>
        public void SetOrder(AVPAppOutputData avpappOutputData, int idx)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_rad_flg = 1;

            if (avpappOutputData.control_point != null && idx != -1)
            {
                PointF StartPoint = new PointF(m_RouteNodes.ElementAt(idx).Value.X, m_RouteNodes.ElementAt(idx).Value.Y);
                PointF EndPoint = new PointF(m_RouteNodes.ElementAt(idx + 1).Value.X, m_RouteNodes.ElementAt(idx + 1).Value.Y);

                double a = 0;
                double b = 0;
                double c = 0;
                //a = y2-y1
                a = EndPoint.Y - StartPoint.Y;
                //b = -(x2-x1)
                b = -(EndPoint.X - StartPoint.X);
                //c=(x2-x1)y1-(y2-y1)x1
                c = (EndPoint.X - StartPoint.X) * StartPoint.Y - (EndPoint.Y - StartPoint.Y) * StartPoint.X;

                double d = 0;
                double e = 0;
                double f = 0;
                d = b;
                e = -a;
                f = -b * avpappOutputData.control_point.x + a * avpappOutputData.control_point.y;

                // 指示情報セット
                m_CarOrder.Angle = avpappOutputData.target_steering_angle;
                m_CarOrder.f_maxangle = avpappOutputData.draw_info.angle_guard;
                m_CarOrder.Angleori = avpappOutputData.draw_info.guard_front_angle;
                m_CarOrder.xCalcPoint = (float)((b * f - e * c) / (a * e - d * b));
                m_CarOrder.yCalcPoint = (float)((d * c - a * f) / (a * e - d * b));
                m_CarOrder.Acceleration = (float)avpappOutputData.target_gx;
                m_CarOrder.areaspeed = m_SpeedAreas.ElementAt(idx).Value.Speed;
            }
            if (avpappOutputData.panaOutputData != null)
            {
                if (avpappOutputData.panaOutputData.log != null)
                {
                    m_CarOrder.log = avpappOutputData.panaOutputData.log;
                }
            }

            m_CarOrderSet = true;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #endregion

        #region 描画

        #region 予測線の描画
        /// <summary>
        /// 予測線の描画
        /// </summary>
        /// <param name="p"></param>
        /// <param name="e"></param>
        private void PredictionGrid(Pen p, PaintEventArgs e)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            p.Color = Color.Yellow;
            AdjustableArrowCap myarrow = new AdjustableArrowCap(3, 2);
            p.CustomEndCap = myarrow;

            // 進行方向線
            float x =(float)( m_newLPPoint.X + m_config.C_DIRECTION_LINE_LENGTH * Math.Cos(-m_oRad));
            float y = (float)(m_newLPPoint.Y + m_config.C_DIRECTION_LINE_LENGTH * Math.Sin(-m_oRad));
            e.Graphics.DrawLine(p, m_newLPPoint.X, m_newLPPoint.Y, x, y);

            // 進行方向予測線
            //p.Color = Color.Orange;
            //PointF point2 = GetRollPointView(m_newLPPoint, new PointF(x, y), -m_CarOrder.Angle / m_config.GearRate);
            //e.Graphics.DrawLine(p, m_newLPPoint.X, m_newLPPoint.Y, point2.X, point2.Y);

            p.Color = Color.Black;
            p.EndCap = LineCap.NoAnchor;

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region チェックされたその他エリアの描画
        private void DrawOtherArea(PaintEventArgs e)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //エリアの描画
            for (int i = 0; i <= m_config.OtherAreas.Count() - 1; i++)
            {
                if (i <= m_DrawOtherArea.Count() - 1)
                {
                    if (m_DrawOtherArea[i])
                    {
                        OtherAreaPaint_Three(i, e);
                    }
                }
            }

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        public void ChangeDrawOtherArea(int idx,bool draw, bool Redraw)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_DrawOtherArea[idx] = draw;

            if (Redraw) { this.Refresh(); }

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        private new void OnPaint(object sender, PaintEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            
            // 侵入検知エリアの描画
            if (m_config.SlowLeftWidth != null && m_config.SlowRightWidth != null)
            {
                for(int i = m_SafetyMonitoringIndex.SlowBackIdx;i <= m_SafetyMonitoringIndex.SlowFrontIdx ; i++) { Area_Paint(m_SpeedAreas.ElementAt(i).Value, Color.Blue, e, 1); }
            }
            if (m_config.PauseLeftWidth != null && m_config.PauseRightWidth != null)
            {
                for (int i = m_SafetyMonitoringIndex.PauseBackIdx; i <= m_SafetyMonitoringIndex.PauseFrontIdx ; i++) { Area_Paint(m_SpeedAreas.ElementAt(i).Value, Color.Yellow, e, 2); }
            }
            if (m_config.StopLeftWidth != null && m_config.StopRightWidth != null)
            {
                for (int i = m_SafetyMonitoringIndex.StopBackIdx; i <= m_SafetyMonitoringIndex.StopFrontIdx ; i++) { Area_Paint(m_SpeedAreas.ElementAt(i).Value, Color.Red, e, 3); }
            }

            // その他エリアの描画
            if (m_DrawOtherArea.Count() != 0) { DrawOtherArea(e); }

            // 制御車両描画
            using (Pen p = new Pen(Color.Black, C_PASSEDROAD_WIDTH))
            {
                // 通過軌跡
                float FroX = 0;
                float FroY = 0;
                for (int i = 0; i <= m_CarYawrateLPoint.Count - 1; i++)
                {
                    if (i != 0)
                    {
                        e.Graphics.DrawLine(p, FroX, FroY, m_CarYawrateLPoint.ElementAt(i).X, m_CarYawrateLPoint.ElementAt(i).Y);
                    }
                    FroX = m_CarYawrateLPoint.ElementAt(i).X;
                    FroY = m_CarYawrateLPoint.ElementAt(i).Y;
                }

                if (m_CarStatusListLPoint.Count != 0)
                {
                    float xOri = 0;
                    float yOri = 0;

                    // 予測点(制御点)
                    if (m_rad_flg == 1)
                    {
                        PredictionGrid(p, e);
                        xOri = m_newLPPoint.X;
                        yOri = m_newLPPoint.Y;
                        // 現在位置
                        float xnow = m_CarStatusListLPoint.Last().X - m_config.C_NODE_WIDTH/ 2;
                        float ynow = m_CarStatusListLPoint.Last().Y - m_config.C_NODE_WIDTH / 2;
                        float w = m_config.C_NODE_WIDTH;
                        e.Graphics.FillEllipse(Brushes.Black, xnow, ynow, w, w);
                    }
                    else
                    {
                        xOri = m_CarStatusListLPoint.Last().X;
                        yOri = m_CarStatusListLPoint.Last().Y;
                    }

                    float x = (float)(xOri - m_config.C_NODE_WIDTH / 2);
                    float y = (float)(yOri - m_config.C_NODE_WIDTH / 2);
                    float r = m_config.C_DIRECTION_LINE_LENGTH;

                    // 現在地（後輪軸中心）の描画
                    e.Graphics.FillEllipse(Brushes.Green, x, y, m_config.C_NODE_WIDTH, m_config.C_NODE_WIDTH);
                    // 車両4隅の描画
                    foreach (CarCornerinfo carcornerInfo in m_CarcornerInfo)
                    {
                        PointF corners = toLocalCoordinate(new PointF(carcornerInfo.cornerPoint.X, carcornerInfo.cornerPoint.Y));
                        e.Graphics.FillEllipse(Brushes.Green, corners.X - m_config.C_NODE_WIDTH/2, corners.Y - m_config.C_NODE_WIDTH / 2, m_config.C_NODE_WIDTH, m_config.C_NODE_WIDTH);
                    }

                    // 垂線
                    if (m_CarOrderSet)
                    {
                        PointF calcpoint = toLocalCoordinate(new PointF(m_CarOrder.xCalcPoint, m_CarOrder.yCalcPoint));
                        e.Graphics.DrawLine(p, xOri, yOri, calcpoint.X, calcpoint.Y);
                    }

                    if (m_rad_flg == 1 || m_CarStatusListLPoint.Count - 1 >= 1 )
                    {
                        // 角度を求める
                        AdjustableArrowCap myarrow = new AdjustableArrowCap(3, 2);
                        p.CustomEndCap = myarrow;

                        float xNew = (float)(m_newLPPoint.X + m_config.C_DIRECTION_LINE_LENGTH * Math.Cos(-m_oRad));
                        float yNew = (float)(m_newLPPoint.Y + m_config.C_DIRECTION_LINE_LENGTH * Math.Sin(-m_oRad));

                        int cnt = m_CarStatusListLPoint.Count - 1;
                        if (m_rad_flg == 1)
                        {
                            m_base = xNew-m_newLPPoint.X  ;  // 底辺
                            m_Vertical = yNew -m_newLPPoint.Y; // 高さ
                        }
                        else if (m_CarStatusListLPoint.ElementAt(cnt - 1).X != m_CarStatusListLPoint.ElementAt(cnt).X & m_CarStatusListLPoint.ElementAt(cnt - 1).Y != m_CarStatusListLPoint.ElementAt(cnt).Y)
                        {
                            m_base = m_CarStatusListLPoint.ElementAt(cnt).X - m_CarStatusListLPoint.ElementAt(cnt - 1).X; // 底辺
                            m_Vertical = m_CarStatusListLPoint.ElementAt(cnt).Y - m_CarStatusListLPoint.ElementAt(cnt - 1).Y; // 高さ
                        }

                        double rad = Math.Atan2(m_Vertical, m_base); // ラジアン
                        float θ = (float)CommonProc.ToDegree(rad); // 角度

                        if (m_draw_Dakaku)
                        {
                            // 進行方向
                            p.Width = 1;
                            e.Graphics.DrawLine(p, m_newLPPoint.X, m_newLPPoint.Y, xNew, yNew);
                            // 舵角
                            p.Width = 2;
                            var AngleBrush = new SolidBrush(Color.FromArgb(150, Color.Green));
                            e.Graphics.FillPie(AngleBrush, m_newLPPoint.X - r, m_newLPPoint.Y - r, r * 2, r * 2, θ, (float)(-CommonProc.ToDegree(m_CarDakaku)));
                            // 舵角線
                            p.Color = Color.Green;
                            rad = -m_CarDakaku;
                            Draw_DakakuArrow(p, xNew, yNew, m_newLPPoint.X, m_newLPPoint.Y, rad, e);

                            //舵角変化閾値
                            if (!m_CarOrder.f_moveangle)
                            {
                                p.Color = Color.Blue;
                                p.Width = 1;
                                if (m_CarOrder.Angleori.Equals(float.NaN))
                                {
                                    rad = 0;
                                }
                                else
                                {
                                    rad = -(m_CarOrder.Angleori);
                                }
                                Draw_DakakuArrow(p, xNew, yNew, m_newLPPoint.X, m_newLPPoint.Y, rad, e);
                            }

                            if (!m_CarOrder.f_maxangle)
                            {
                                // 指示舵角
                                AngleBrush.Color = Color.FromArgb(100, Color.DodgerBlue);
                                // 指示舵角線
                                p.Color = Color.Blue;
                            }
                            else
                            {
                                //最大舵角
                                AngleBrush.Color = Color.FromArgb(100, Color.Red);
                                p.Color = Color.Red;
                                p.Width = 1;
                                if (m_CarOrder.Angleori.Equals(float.NaN))
                                { 
                                    rad = 0;
                                }
                                else
                                { 
                                    rad = -(m_CarOrder.Angleori);
                                }
                                Draw_DakakuArrow(p, xNew, yNew, m_newLPPoint.X, m_newLPPoint.Y, rad, e);
                            }
                            // 指示舵角
                            if (m_CarOrder.Angle.Equals(float.NaN))
                            {
                                e.Graphics.FillPie(AngleBrush, m_newLPPoint.X - r, m_newLPPoint.Y - r, r * 2, r * 2, θ, 0);
                            }
                            else
                            {
                                e.Graphics.FillPie(AngleBrush, m_newLPPoint.X - r, m_newLPPoint.Y - r, r * 2, r * 2, θ, (float)(-CommonProc.ToDegree(m_CarOrder.Angle)));
                            }
                            // 指示舵角線
                            if (m_CarOrder.Angle.Equals(float.NaN))
                            { 
                                rad = 0;
                            }
                            else
                            { 
                                rad = -(m_CarOrder.Angle);
                            }
                            Draw_DakakuArrow(p, xNew, yNew, m_newLPPoint.X, m_newLPPoint.Y, rad, e);
                        }

                        p.EndCap = LineCap.NoAnchor;

                    }
                }


                // 侵入物検知範囲の描画
                if (m_detectdatas.Count != 0)
                {
                    //float width =  10/ 2;
                    foreach (InstrusionDetection instrusion in m_detectdatas)
                    {
                        PointF dp = toLocalCoordinate(instrusion.Position);
                        float r = (float)toLocalLength(instrusion.Radius);
                        dp.X = dp.X - r;
                        dp.Y = dp.Y - r;
                        //var AngleBrush = new SolidBrush(Color.FromArgb(150, Color.White));
                        //e.Graphics.FillEllipse(AngleBrush, dp.X, dp.Y, r * 2f, r * 2f);
                        e.Graphics.FillEllipse(Brushes.White, dp.X, dp.Y, r * 2f, r * 2f);
                    }
                }

            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region 舵角矢印の描画
        /// <summary>
        /// 舵角矢印を描画する
        /// </summary>
        /// <param name="xNew">進行方向矢印先端x座標</param>
        /// <param name="yNew">進行方向矢印先端y座標</param>
        /// <param name="xOri">現在地x座標</param>
        /// <param name="yOri">現在地y座標</param>
        /// <param name="rad">舵角ラジアン</param>
        private void Draw_DakakuArrow(Pen p, float xNew, float yNew, float xOri, float yOri, double rad, PaintEventArgs e)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            float x = (float)((xNew - xOri) * Math.Cos(rad) - (yNew - yOri) * Math.Sin(rad) + xOri);
            float y = (float)((xNew - xOri) * Math.Sin(rad) + (yNew - yOri) * Math.Cos(rad) + yOri);
            e.Graphics.DrawLine(p, xOri, yOri, x, y);

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region 点の回転
        // <summary>
        // 回転した角度を求めます
        // </summary>
        // <param name="pCenter">回転の中心座標</param>
        // <param name="pTarget">回転対象の座標</param>
        // <param name="sin">sinθの値</param>
        // <param name="cos">cosθの値</param>
        // <returns>回転後の座標</returns>
        private PointF GetRollPointView(PointF pCenter, PointF pTarget, double rad)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            float x = 0;
            float y = 0;

            // 中心点を原点として平行移動
            float oX = pTarget.X - pCenter.X;
            float oY = pTarget.Y - pCenter.Y;

            // 平行移動した状態での回転位置を算出
            float oX2 = oX * System.Convert.ToSingle(Math.Cos(rad)) - oY * System.Convert.ToSingle(Math.Sin(rad));
            float oY2 = oX * System.Convert.ToSingle(Math.Sin(rad)) + oY * System.Convert.ToSingle(Math.Cos(rad));

            // 平行移動した分を戻す
            x = pCenter.X + oX2;
            y = pCenter.Y + oY2;

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return new PointF(x, y);
        }
        #endregion

        #region 初期化
        public void ResetView()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            m_CarStatusList.Clear();
            m_CarStatusListLPoint.Clear();
            m_rad_flg = 0;
            //m_YawrateTime = 0;
            //m_YawrateStandart = 0;

            //m_oRad = CommonProc.ToRadian(m_configconst.C_START_YAWRATE);

            m_newPPoint = PointF.Empty;
            m_CarYawrateLPoint.Clear();
            m_contain_idx = -1;
            m_CarcornerInfo = new List<CarCornerinfo>();

            m_CarOrderSet = false;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion  

        private void ControlViewer_Load(object sender, EventArgs e)
        {
            
        }

        public void DrawSet()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            for (int i = 0; i <= m_config.OtherAreas.Count() - 1; i++)
            {
                m_DrawOtherArea.Add(false);
            }
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        public void AngleDrawChange(bool angle,bool getarea)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            m_draw_Dakaku = angle;
            m_draw_GetArea = getarea;
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #region 自動スクロール
        /// <summary>
        /// 自動スクロール
        /// 制御点が画面から外れた際に制御点が中心になるようにスクロールさせる
        /// </summary>
        public void AutoScrollView()
        {
            if (m_CarStatusListLPoint.Count != 0)
            {
                //float xnow = m_CarStatusListLPoint.Last().X - m_configconst.C_NODE_WIDTH / 2;
                //float ynow = m_CarStatusListLPoint.Last().Y - m_configconst.C_NODE_WIDTH / 2;
                float xnow = m_newLPPoint.X - m_config.C_NODE_WIDTH / 2;
                float ynow = m_newLPPoint.Y - m_config.C_NODE_WIDTH / 2;

                //縦スクロール
                if (-this.AutoScrollPosition.Y >= ynow || -this.AutoScrollPosition.Y + this.Size.Height <= ynow)
                {
                    int halfY = this.Size.Height / 2;
                    if(ynow+halfY >= pnl_view.Size.Height)
                    {
                        this.AutoScrollPosition = new Point(this.AutoScrollPosition.X, pnl_view.Size.Height - this.Size.Height);
                    }
                    else if(ynow -halfY <=0 )
                    {
                        this.AutoScrollPosition = new Point(this.AutoScrollPosition.X, 0);
                    }
                    else
                    {
                        this.AutoScrollPosition = new Point(this.AutoScrollPosition.X, (int)ynow - halfY);
                    }
                }

                //横スクロール
                if (-this.AutoScrollPosition.X >= xnow || -this.AutoScrollPosition.X + this.Size.Width <= xnow)
                {
                    int halfX = this.Size.Width / 2;
                    if (xnow + halfX >= pnl_view.Size.Width)
                    {
                        this.AutoScrollPosition = new Point(pnl_view.Size.Width - this.Size.Width, this.AutoScrollPosition.Y);
                    }
                    else if (xnow - halfX <= 0)
                    {
                        this.AutoScrollPosition = new Point(0, this.AutoScrollPosition.X);
                    }
                    else
                    {
                        this.AutoScrollPosition = new Point(this.AutoScrollPosition.Y, (int)xnow - halfX);
                    }
                }
            }
        }
        #endregion

        #region エリア幅調整

        public void SpeedAreaAdjust(double val)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            foreach (KeyValuePair<int, SpeedArea> area in m_SpeedAreas)
            {
                area.Value.LeftWidth = area.Value.LeftWidth + val >= 0 ? area.Value.LeftWidth + val : 0;
                area.Value.RightWidth = area.Value.RightWidth + val >= 0 ? area.Value.RightWidth + val : 0;
            }

            // エリア全体更新
            for (var i = 0; i <= m_SpeedAreas.Count - 1; i++)
            {
                m_SpeedAreas.ElementAt(i).Value.Area =AreaSet(i, m_SpeedAreas.ElementAt(i).Value, 0);
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        public void SpeedAreaReset(ConfigData conf)
        {
            m_SpeedAreas.Clear();
            if (conf.SpeedAreaList != null)
            {
                for (int i = 0; i <= conf.SpeedAreaList.SpeedArea.Count - 1; i++) { m_SpeedAreas.Add(conf.SpeedAreaList.SpeedArea[i].ID, conf.SpeedAreaList.SpeedArea[i]); }
            }
        }

        #endregion


        #region エリアの各座標値のセット
        public PointF[] AreaSet(int i, SpeedArea NowSpeedArea, int num)
        {
            List<PointF> area = new List<PointF>();
            int ncnt = i + 1;

            double uedge = 0;
            double vertical = 0;

            uedge = m_RouteNodes[ncnt].X - m_RouteNodes[ncnt - 1].X; // 底辺
            vertical = m_RouteNodes[ncnt].Y - m_RouteNodes[ncnt - 1].Y; // 高さ
            double rad = Math.Atan2(vertical, uedge); // ラジアン
            double cosθ = Math.Cos(rad);
            double sinθ = Math.Sin(rad);

            double rightwidth = AreaSelecterRight(NowSpeedArea, num);
            double leftwidth = AreaSelecterLeft(NowSpeedArea, num);

            // エリアの頂点座標の計算
            area.Add(new PointF((float)(m_RouteNodes[ncnt - 1].X + rightwidth * sinθ), (float)(m_RouteNodes[ncnt - 1].Y - rightwidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt].X + rightwidth * sinθ), (float)(m_RouteNodes[ncnt].Y - rightwidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt].X - leftwidth * sinθ), (float)(m_RouteNodes[ncnt].Y + leftwidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt - 1].X - leftwidth * sinθ), (float)(m_RouteNodes[ncnt - 1].Y + leftwidth * cosθ)));
            area.Add(area[0]);

            if (i > 0)
            {
                PointF O = new PointF(m_RouteNodes[ncnt - 2].X, m_RouteNodes[ncnt - 2].Y);
                PointF A = new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y);
                PointF B = new PointF(m_RouteNodes[ncnt].X, m_RouteNodes[ncnt].Y);

                double Z = CommonProc.Gaiseki(O, A, B);
                if (Z > 0)
                {
                    area.Insert(4, new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y));
                    switch (num)
                    {
                        case 0:
                            area.Insert(5, m_SpeedAreas[i - 1].Area[1]);
                            break;
                        case 1:
                            area.Insert(5, m_SpeedAreas[i - 1].SlowArea[1]);
                            break;
                        case 2:
                            area.Insert(5, m_SpeedAreas[i - 1].PauseArea[1]);
                            break;
                        case 3:
                            area.Insert(5, m_SpeedAreas[i - 1].StopArea[1]);
                            break;
                    }

                }
                else if (Z < 0)
                {
                    switch (num)
                    {
                        case 0:
                            area.Insert(4, m_SpeedAreas[i - 1].Area[2]);
                            break;
                        case 1:
                            area.Insert(4, m_SpeedAreas[i - 1].SlowArea[2]);
                            break;
                        case 2:
                            area.Insert(4, m_SpeedAreas[i - 1].PauseArea[2]);
                            break;
                        case 3:
                            area.Insert(4, m_SpeedAreas[i - 1].StopArea[2]);
                            break;
                    }
                    area.Insert(5, new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y));
                }
            }

            return (PointF[])area.ToArray().Clone();
        }

        private double AreaSelecterRight(SpeedArea NowSpeedArea, int num)
        {
            double width = 0;

            switch (num)
            {
                case 0:
                    width = NowSpeedArea.RightWidth;
                    break;
                case 1:
                    if (NowSpeedArea.SlowRightWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.SlowRightWidth);
                    }
                    else
                    {
                        width = double.Parse(m_config.SlowRightWidth);
                    }
                    break;
                case 2:
                    if (NowSpeedArea.PauseRightWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.PauseRightWidth);
                    }
                    else
                    {
                        width = double.Parse(m_config.PauseRightWidth);
                    }
                    break;
                case 3:
                    if (NowSpeedArea.StopRightWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.StopRightWidth);
                    }
                    else
                    {
                        width = double.Parse(m_config.StopRightWidth);
                    }
                    break;
            }

            return width;
        }
        private double AreaSelecterLeft(SpeedArea NowSpeedArea, int num)
        {
            double width = 0;

            switch (num)
            {
                case 0:
                    width = NowSpeedArea.LeftWidth;
                    break;
                case 1:
                    if (NowSpeedArea.SlowLeftWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.SlowLeftWidth);
                    }
                    else
                    {
                        width = double.Parse(m_config.SlowLeftWidth);
                    }
                    break;
                case 2:
                    if (NowSpeedArea.PauseLeftWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.PauseLeftWidth);
                    }
                    else
                    {
                        width = double.Parse(m_config.PauseLeftWidth);
                    }
                    break;
                case 3:
                    if (NowSpeedArea.StopLeftWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.StopLeftWidth);
                    }
                    else
                    {
                        width = double.Parse(m_config.StopLeftWidth);
                    }
                    break;
            }

            return width;
        }
        #endregion


        //RCD

        #region ## 判定エリア数計算 ##
        /// <summary>
        /// 後方半径距離から判定エリア数を算出
        /// </summary>
        /// <param name="nowidx">リア軸中心が属するインデックス</param>
        /// <param name="distance">後方判定距離</param>
        /// <returns></returns>
        private int CalcBackIndex(int nowidx, double distance)
        {
            double suml = 0;
            int i = 0;
            // 合計値が距離を超えるか、先頭まで合計したら終了
            for (i = 0; i < nowidx; i++)
            {
                // ノード間の距離の計算
                double l = Math.Sqrt(Math.Pow((m_RouteNodes.ElementAt(nowidx - i).Value.X - m_RouteNodes.ElementAt(nowidx - i - 1).Value.X), 2) + Math.Pow((m_RouteNodes.ElementAt(nowidx - i).Value.Y - m_RouteNodes.ElementAt(nowidx - i - 1).Value.Y), 2));

                suml += l;
                // ノード間の合計値が判定距離を超えたことを確認
                if (suml >= distance)
                {
                    break;
                }
            }
            if (nowidx - i - 1 < 0)
            {
                return nowidx - i;
            }
            else
            {
                return nowidx - i - 1;
            }
        }
        /// <summary>
        /// 前方半径距離から判定エリア数を算出
        /// </summary>
        /// <param name="nowidx">リア軸中心が属するインデックス</param>
        /// <param name="distance">前方判定距離</param>
        /// <returns></returns>
        private int CalcFrontIndex(int nowidx, double distance)
        {
            double suml = 0;
            int i = 0;
            for (i = 0; i < m_SpeedAreas.Count - 2- nowidx; i++)
            {
                // ノード間の距離の計算
                double l = Math.Sqrt(Math.Pow((m_RouteNodes.ElementAt(nowidx + i + 2).Value.X - m_RouteNodes.ElementAt(nowidx + i + 1).Value.X), 2) + Math.Pow((m_RouteNodes.ElementAt(nowidx + i + 2).Value.Y - m_RouteNodes.ElementAt(nowidx + i + 1).Value.Y), 2));

                suml += l;
                // ノード間の合計値が判定距離を超えたことを確認
                if (suml >= distance)
                {
                    break;
                }
            }
            if (nowidx + i + 1 >= m_SpeedAreas.Count)
            {
                return nowidx + i;
            }
            else
            {
                return nowidx + i + 1;
            }
        }
        #endregion

        
        [Serializable]
        public class InstrusionDetection : ICloneable
        {
            /// <summary>
            /// 検知情報
            /// </summary>
            public RecognitionObjectData objectData { get; set; }
            /// <summary>
            /// 検知判定状態 True:判定対象 False:判定対象外
            /// </summary>
            public bool JudgesTarget { get; set; }
            /// <summary>
            /// 認識範囲半径
            /// </summary>
            public double Radius { get; set; }
            /// <summary>
            /// 認識中心座標
            /// </summary>
            public PointF Position { get; set; }


            public InstrusionDetection()
            {
                JudgesTarget = true;
            }

            public object Clone()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, this);
                    ms.Position = 0;
                    return (InstrusionDetection)bf.Deserialize(ms);
                }
            }
        }
        /// <summary>
        /// 物評データ
        /// </summary>
        [Serializable]
        public class RecognitionObjectData : ICloneable
        {
            /// <summary>
            /// 物評種別
            /// </summary>
            public string label { get; set; }
            /// <summary>
            /// 物評位置
            /// </summary>
            public List<int> pos { get; set; }

            public object Clone()
            {

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, this);
                    ms.Position = 0;
                    return (RecognitionObjectData)bf.Deserialize(ms);
                }
            }
        }

        [Serializable]
        public class SafetyMonitoringIndex
        {
            public int SlowFrontIdx { get; set; }
            public int SlowBackIdx { get; set; }
            public int PauseFrontIdx { get; set; }
            public int PauseBackIdx { get; set; }
            public int StopFrontIdx { get; set; }
            public int StopBackIdx { get; set; }
            public object Clone()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, this);
                    ms.Position = 0;
                    return (SafetyMonitoringIndex)bf.Deserialize(ms);
                }
            }
        }

        
    }

}
