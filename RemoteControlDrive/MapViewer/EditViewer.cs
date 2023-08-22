using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AtrptCmn;
using System.Reflection;
using AtrptShare;


namespace MapViewer
{
    public partial class EditViewer : ViewerBace
    {
        #region イベント
        public delegate void RouteNodeEventHundler(RouteNodeEventArgs e);
        public delegate void SpeedAreaEventHandler(SpeedAreaEventArgs e);
        public delegate void FacilityAreaEventHandler(FacilityAreaEventArgs e);

        public event RouteNodeEventHundler NodeAdd;
        public event RouteNodeEventHundler NodeChanged;
        public event RouteNodeEventHundler NodeDeleted;
        public event RouteNodeEventHundler NodeSelectChanged;

        public event SpeedAreaEventHandler AreaAdd;
        public event SpeedAreaEventHandler AreaChanegd;
        public event SpeedAreaEventHandler AreaDeleted;
        public event SpeedAreaEventHandler AreaSelectChanged;

        public event FacilityAreaEventHandler FacilityAdd;
        public event FacilityAreaEventHandler FacilityChanged;
        public event FacilityAreaEventHandler FacilityDeleted;
        #endregion

        #region 変数
        /// <summary>
        /// 選択されているノードのキー情報
        /// </summary>
        public int m_SelectedNode = -1;
        /// <summary>
        /// 選択されているエリアのキー情報
        /// </summary>
        public int m_SelectedArea = -1;
        /// <summary>
        /// クリックされたエリアノードの番号
        /// </summary>
        private int m_SelectedAreaNode = -1;
        /// <summary>
        /// 選択された周辺設備エリアのインデックス番号
        /// </summary>
        public int m_SelectedFacilityIdx = -1;
        /// <summary>
        /// 選択された新規作成中エリアのインデックス番号
        /// </summary>
        private AtrptAddMode m_SelectedNewArea = AtrptAddMode.mode0;
        /// <summary>
        /// クリック位置調整座標
        /// </summary>
        private PointF m_ClickCorrection = new PointF(0, 0);
        public AtrptMode m_Mode;
        private AtrptEditmode m_EditMode;
        public AtrptAddMode m_AddMode;

        Dictionary<int, RouteNode> m_SelectedAreaNodes = new Dictionary<int, RouteNode>();
        //マウス操作中フラグ
        Boolean m_MouseDrag = false;
        //マウス操作中フラグ
        Boolean m_AreaNodeDrag = false;

        /// <summary>
        /// 周辺設備エリア作成中一時保管
        /// </summary>
        private PointF[][] m_NewFacilityArea;

        #endregion

        #region 定数
        /// <summary>
        /// 範囲変更用ノードサイズ
        /// </summary>
        private const int C_AREANODE_SIZE = 7;
        /// <summary>
        /// エリア幅初期値
        /// </summary>
        private const int C_EARLY_AREA_WIDTH = 1;
        private const double C_MINIMUM_AREA_WIDTH = 0.01;
        /// <summary>
        /// エリア速度初期値
        /// </summary>
        private const int C_EARLY_SPEED = 10;
        /// <summary>
        /// エリア加速度初期値
        /// </summary>
        private const int C_ACCELERATION = 1;
        /// <summary>
        /// エリア減速度初期値
        /// </summary>
        private const int C_DECELERATION = 1;
        /// <summary>
        /// ノード座標画面表示桁数
        /// </summary>
        private const int C_NODEVAL_DIGIT = 3;
        /// <summary>
        /// エリア幅画面表示桁数
        /// </summary>
        private const int C_AREAWIDTH_DIGIT = 3;
        //範囲変更用ノードID
        private const int C_CHANGE_WIDTH_LEFT = 4;
        private const int C_CHANGE_WIDTH_RIGHT = 5;
        /// <summary>
        /// その他エリア描画色
        /// </summary>
        private Color C_OTHERAREA_COLOR = Color.Orange;
        #endregion

        #region enum
        public enum AtrptMode : int
        {
            /// <summary>
            /// 理想経路設定
            /// </summary>
            Route = 1,
            /// <summary>
            /// その他エリア設定
            /// </summary>
            Other = 2,
            /// <summary>
            /// その他エリアのエリア新規作成 or エリア修正中
            /// </summary>
            OtherAdd = 3,
        }

        public enum AtrptEditmode : int
        {

            Node = 1,
            SpeedArea = 2
        }

        public enum AtrptAddMode : int
        {
            /// <summary>
            /// 
            /// </summary>
            mode0 = -1,
            /// <summary>
            /// 動作エリア１
            /// </summary>
            area1 = 0,
            /// <summary>
            /// 動作エリア２
            /// </summary>
            area2 = 1,
            /// <summary>
            /// 動作確認エリア
            /// </summary>
            area3 = 2
        }
        #endregion

        public EditViewer()
        {
            InitializeComponent();
            this.pnl_view_OnPaint += new PaintEventHandler(OnPaint);
            this.pnl_view_OnMouseDown += new MouseEventHandler(EditViewer_MouseDown);
            this.pnl_view_OnMouseMove += new MouseEventHandler(EditViewer_MouseMove);
            this.pnl_view_OnMouseUp += new MouseEventHandler(EditViewer_MouseUp);
        }

        private void EditViewer_Load(object sender, EventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_Mode = AtrptMode.Route;
            m_EditMode = AtrptEditmode.Node;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #region ノードの追加
        public void AddRouteNode(float x, float y)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                int key = 1;

                //現在設定されているKeyの最大値の＋１をKeyに設定
                key = (RouteNodes.Count > 0) ? RouteNodes.Keys.Last() + 1 : 1;

                //要素のセット
                //PointF p = toGlobalCoordinate(new PointF(x, y));
                PointF p = new PointF(x, y);
                RouteNode setnode = new RouteNode
                {
                    ID = key,
                    X = p.X,
                    Y = p.Y,
                    Name = "New Node"
                };
                //ノードの追加
                RouteNodes.Add(key, setnode);
                //ノード追加イベント発生
                RouteNodeEventArgs e = new RouteNodeEventArgs
                {
                    rn = (RouteNode)setnode.Clone(),
                    index = RouteNodes.IndexOfKey(key)
                };
                p = new PointF(e.rn.X, e.rn.Y);
                //p = point_substr(toGlobalCoordinate(p), C_NODEVAL_DIGIT);
                p = point_substr(p, C_NODEVAL_DIGIT);
                e.rn.X = p.X;
                e.rn.Y = p.Y;
                if (NodeAdd != null) { NodeAdd(e); }

                //エリアの追加
                if (RouteNodes.Count - 1 > 0) { AddSpeedArea(); }

                //追加したノードを選択状態にする
                m_EditMode = AtrptEditmode.Node;
                m_SelectedNode = key;
                //ノード選択イベント発生
                e.rn = (RouteNode)RouteNodes[key].Clone();
                e.index = RouteNodes.IndexOfKey(key);
                p = new PointF(e.rn.X, e.rn.Y);
                p = point_substr(p, C_NODEVAL_DIGIT);
                e.rn.X = p.X;
                e.rn.Y = p.Y;
                if (NodeSelectChanged != null) { NodeSelectChanged(e); }

                this.Refresh();

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region 設備エリアの更新
        public void UpdateOtherArea(PointF point)
        {
            point = MoveLimits(point);
            //移動距離算出
            SizeF move = new SizeF(PointF.Subtract(point, new SizeF(m_ClickCorrection)));
            //移動距離反映
            PointF[] fs = m_config.OtherAreas[m_SelectedFacilityIdx].area[(int)m_AddMode];
            for (int i = 0; i <= fs.Count() - 1; i++)
            {
                fs[i] = PointF.Add(fs[i], move);
            }
            m_config.OtherAreas[m_SelectedFacilityIdx].area[(int)m_AddMode] = fs;
            m_ClickCorrection = point;
        }

        public void UpdateNewOtherArea(PointF point)
        {
            point = MoveLimits(point);
            //移動距離算出
            SizeF move = new SizeF(PointF.Subtract(point, new SizeF(m_ClickCorrection)));
            //移動距離反映
            PointF[] fs = (PointF[])m_NewFacilityArea[(int)m_SelectedNewArea].Clone();
            for (int i = 0; i <= fs.Count() - 1; i++)
            {
                fs[i] = PointF.Add(fs[i], move);
            }
            m_NewFacilityArea[(int)m_SelectedNewArea] = fs;
            m_ClickCorrection = point;
        }
        #endregion

        #region ノードの更新
        /// <summary>
        /// ノードの更新（座標値のみ）
        /// </summary>
        public void UpdateRouteNode(int key, float x, float y)
        {
            try
            {
                // 値の更新
                RouteNodes[key].X = x;
                RouteNodes[key].Y = y;

                // ノード変更イベント発生
                RouteNodeEventArgs nodee = new RouteNodeEventArgs
                {
                    rn = (RouteNode)RouteNodes[m_SelectedNode].Clone(),
                    index = RouteNodes.IndexOfKey(m_SelectedNode)
                };
                PointF p = new PointF(nodee.rn.X, nodee.rn.Y);
                p = point_substr(p, C_NODEVAL_DIGIT);
                nodee.rn.X = p.X;
                nodee.rn.Y = p.Y;
                if (NodeChanged != null) { NodeChanged(nodee); }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { }
        }

        /// <summary>
        /// ノードの更新(名前も変更)
        /// </summary>
        public void UpdateRouteNode(int key, RouteNode node)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                // 値の更新
                PointF po = new PointF(node.X, node.Y);
                //po = toLocalCoordinate(po);
                RouteNodes[key].Name = node.Name;
                RouteNodes[key].X = po.X;
                RouteNodes[key].Y = po.Y;
                // ノード変更イベント発生
                RouteNodeEventArgs nodee = new RouteNodeEventArgs
                {
                    rn = (RouteNode)RouteNodes[m_SelectedNode].Clone(),
                    index = RouteNodes.IndexOfKey(m_SelectedNode)
                };
                PointF p = new PointF(nodee.rn.X, nodee.rn.Y);
                p = point_substr(p, 5);
                nodee.rn.X = p.X;
                nodee.rn.Y = p.Y;
                if (NodeChanged != null) { NodeChanged(nodee); }

                UpdateSpeedArea();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region ノードとエリアの削除
        public void DeleteRouteNode(int key)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                // ノード削除イベント発生
                RouteNodeEventArgs nodee = new RouteNodeEventArgs
                {
                    rn = (RouteNode)RouteNodes[key].Clone(),
                    index = RouteNodes.IndexOfKey(key)
                };
                PointF p = new PointF(nodee.rn.X, nodee.rn.Y);
                p = point_substr(toGlobalCoordinate(p), C_NODEVAL_DIGIT);
                nodee.rn.X = p.X;
                nodee.rn.Y = p.Y;
                if (NodeDeleted != null) { NodeDeleted(nodee); }

                int index = RouteNodes.IndexOfKey(key);
                if (index == -1) { index = 0; }

                // エリアの削除
                DeleteSpeedArea(index);
                // ノードの削除
                RouteNodes.Remove(RouteNodes[key].ID);

                if (RouteNodes.Count > 0)
                {
                    m_SelectedNode = (index == 0) ? RouteNodes.ElementAt(index).Key : RouteNodes.ElementAt(index - 1).Key;
                }
                else
                {
                    m_SelectedNode = -1;
                }

                if (m_SelectedNode != -1)
                {
                    // ノード選択イベント発生
                    nodee.rn = (RouteNode)RouteNodes[m_SelectedNode].Clone();
                    nodee.index = RouteNodes.IndexOfKey(m_SelectedNode);
                    p = new PointF(nodee.rn.X, nodee.rn.Y);
                    p = point_substr(toGlobalCoordinate(p), C_NODEVAL_DIGIT);
                    nodee.rn.X = p.X;
                    nodee.rn.Y = p.Y;
                    if (NodeSelectChanged != null) { NodeSelectChanged(nodee); }
                }

                // エリアの更新
                UpdateSpeedArea();

                this.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region エリアの追加
        public void AddSpeedArea()
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                int key = 0;

                // 現在設定されているKeyの最大値の＋１をKeyに設定
                key = (SpeedAreas.Count > 0) ? SpeedAreas.Keys.Last() + 1 : 1;

                // 要素のセット
                SpeedArea setarea = new SpeedArea
                {
                    ID = key,
                    Name = "New Area"
                };
                if (SpeedAreas.Count == 0)
                {
                    // 初期値を設定
                    setarea.LeftWidth = C_EARLY_AREA_WIDTH;
                    setarea.RightWidth = C_EARLY_AREA_WIDTH;
                    setarea.Speed = C_EARLY_SPEED;
                    setarea.Acceleration = C_ACCELERATION;
                    setarea.Deceleration = C_DECELERATION;
                }
                else
                {
                    // 前エリアの幅とスピードを受け継ぐ
                    setarea.LeftWidth = SpeedAreas.Values.Last().LeftWidth;
                    setarea.RightWidth = SpeedAreas.Values.Last().RightWidth;
                    setarea.Speed = SpeedAreas.Values.Last().Speed;
                    setarea.Acceleration = SpeedAreas.Values.Last().Acceleration;
                    setarea.Deceleration = SpeedAreas.Values.Last().Deceleration;

                    setarea.SlowRightWidth = SpeedAreas.Values.Last().SlowRightWidth;
                    setarea.SlowLeftWidth = SpeedAreas.Values.Last().SlowLeftWidth;
                    setarea.SlowFrontDistance = SpeedAreas.Values.Last().SlowFrontDistance;
                    setarea.SlowBackDistance = SpeedAreas.Values.Last().SlowBackDistance;
                    setarea.SlowCoefficient = SpeedAreas.Values.Last().SlowCoefficient;

                    setarea.PauseRightWidth = SpeedAreas.Values.Last().PauseRightWidth;
                    setarea.PauseLeftWidth = SpeedAreas.Values.Last().PauseLeftWidth;
                    setarea.PauseFrontDistance = SpeedAreas.Values.Last().PauseFrontDistance;
                    setarea.PauseBackDistance = SpeedAreas.Values.Last().PauseBackDistance;

                    setarea.StopRightWidth = SpeedAreas.Values.Last().StopRightWidth;
                    setarea.StopLeftWidth = SpeedAreas.Values.Last().StopLeftWidth;
                    setarea.StopFrontDistance = SpeedAreas.Values.Last().StopFrontDistance;
                    setarea.StopBackDistance = SpeedAreas.Values.Last().StopBackDistance;
                }
                setarea.Area = AreaSet(SpeedAreas.Count(), setarea, 0);
                setarea.SlowArea = AreaSet(SpeedAreas.Count(), setarea, 1);
                setarea.PauseArea = AreaSet(SpeedAreas.Count(), setarea, 2);
                setarea.StopArea = AreaSet(SpeedAreas.Count(), setarea, 3);

                // エリアの追加
                SpeedAreas.Add(key, setarea);

                // エリア追加イベント発生
                SpeedAreaEventArgs e = new SpeedAreaEventArgs
                {
                    sa = (SpeedArea)setarea.Clone(),
                    index = SpeedAreas.Count - 1
                };
                e.sa.LeftWidth = areawidth_substr(e.sa.LeftWidth, C_AREAWIDTH_DIGIT);
                e.sa.RightWidth = areawidth_substr(e.sa.RightWidth, C_AREAWIDTH_DIGIT);
                if (AreaAdd != null) { AreaAdd(e); }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region エリアの更新
        /// <summary>
        ///  エリアの幅やスピードの更新
        ///  </summary>
        ///  <param name="i"></param>
        ///  <param name="area"></param>
        public void UpdateSpeedArea(int key, SpeedArea area)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                // エリアの値更新
                SpeedAreas[key].Name = area.Name;
                SpeedAreas[key].LeftWidth = area.LeftWidth;
                SpeedAreas[key].RightWidth = area.RightWidth;
                SpeedAreas[key].Speed = area.Speed;
                SpeedAreas[key].Acceleration = area.Acceleration;
                SpeedAreas[key].Deceleration = area.Deceleration;
                SpeedAreas[key].GetRadian = area.GetRadian;
                SpeedAreas[key].MinDistance = area.MinDistance;
                SpeedAreas[key].MaxDistance = area.MaxDistance;

                SpeedAreas[key].SlowLeftWidth = area.SlowLeftWidth;
                SpeedAreas[key].SlowRightWidth = area.SlowRightWidth;
                SpeedAreas[key].SlowFrontDistance = area.SlowFrontDistance;
                SpeedAreas[key].SlowBackDistance = area.SlowBackDistance;
                SpeedAreas[key].SlowCoefficient = area.SlowCoefficient;
                SpeedAreas[key].PauseLeftWidth = area.PauseLeftWidth;
                SpeedAreas[key].PauseRightWidth = area.PauseRightWidth;
                SpeedAreas[key].PauseFrontDistance = area.PauseFrontDistance;
                SpeedAreas[key].PauseBackDistance = area.PauseBackDistance;
                SpeedAreas[key].StopLeftWidth = area.StopLeftWidth;
                SpeedAreas[key].StopRightWidth = area.StopRightWidth;
                SpeedAreas[key].StopFrontDistance = area.StopFrontDistance;
                SpeedAreas[key].StopBackDistance = area.StopBackDistance;

                // エリア全体更新
                for (int j = 0; j <= SpeedAreas.Count - 1; j++)
                {
                    SpeedAreas.ElementAt(j).Value.Area = AreaSet(j, SpeedAreas.ElementAt(j).Value, 0);
                }
                //
                for (int j = 0; j <= SpeedAreas.Count - 1; j++)
                {
                    SpeedAreas.ElementAt(j).Value.SlowArea = AreaSet(j, SpeedAreas.ElementAt(j).Value, 1);
                }
                for (int j = 0; j <= SpeedAreas.Count - 1; j++)
                {
                    SpeedAreas.ElementAt(j).Value.PauseArea = AreaSet(j, SpeedAreas.ElementAt(j).Value, 2);
                }
                for (int j = 0; j <= SpeedAreas.Count - 1; j++)
                {
                    SpeedAreas.ElementAt(j).Value.StopArea = AreaSet(j, SpeedAreas.ElementAt(j).Value, 3);
                }

                // エリア更新イベント発生
                SpeedAreaEventArgs areae = new SpeedAreaEventArgs
                {
                    sa = (SpeedArea)SpeedAreas[key].Clone(),
                    index = SpeedAreas.IndexOfKey(key)
                };
                areae.sa.LeftWidth = areawidth_substr(areae.sa.LeftWidth, C_AREAWIDTH_DIGIT);
                areae.sa.RightWidth = areawidth_substr(areae.sa.RightWidth, C_AREAWIDTH_DIGIT);
                if (AreaChanegd != null) { AreaChanegd(areae); }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// ノードの移動によるエリアの更新
        /// </summary>
        public void UpdateSpeedArea()
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                // エリア全体更新
                for (var i = 0; i <= SpeedAreas.Count - 1; i++)
                {
                    SpeedAreas.ElementAt(i).Value.Area = AreaSet(i, SpeedAreas.ElementAt(i).Value, 0);
                }
                for (var i = 0; i <= SpeedAreas.Count - 1; i++)
                {
                    SpeedAreas.ElementAt(i).Value.SlowArea = AreaSet(i, SpeedAreas.ElementAt(i).Value, 1);
                }
                for (var i = 0; i <= SpeedAreas.Count - 1; i++)
                {
                    SpeedAreas.ElementAt(i).Value.PauseArea = AreaSet(i, SpeedAreas.ElementAt(i).Value, 2);
                }
                for (var i = 0; i <= SpeedAreas.Count - 1; i++)
                {
                    SpeedAreas.ElementAt(i).Value.StopArea = AreaSet(i, SpeedAreas.ElementAt(i).Value, 3);
                }

                if (m_EditMode == AtrptEditmode.SpeedArea)
                {
                    // エリア更新イベント発生
                    SpeedAreaEventArgs areae = new SpeedAreaEventArgs
                    {
                        sa = SpeedAreas[m_SelectedArea],
                        index = SpeedAreas.IndexOfKey(m_SelectedArea)
                    };
                    areae.sa.LeftWidth = areawidth_substr(areae.sa.LeftWidth, C_AREAWIDTH_DIGIT);
                    areae.sa.RightWidth = areawidth_substr(areae.sa.RightWidth, C_AREAWIDTH_DIGIT);
                    if (AreaChanegd != null) { AreaChanegd(areae); }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region エリアの削除
        public void DeleteSpeedArea(int i)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                if (SpeedAreas.Count != 0)
                {
                    // 削除されたノードの前エリアを削除する
                    if (i == 0)
                    {
                        SpeedAreas.Remove(SpeedAreas.ElementAt(i).Value.ID);
                    }
                    else
                    {
                        SpeedAreas.Remove(SpeedAreas.ElementAt(i - 1).Value.ID);
                    }

                    m_SelectedArea = -1;
                    // エリア削除イベント発生
                    SpeedAreaEventArgs areae = new SpeedAreaEventArgs();
                    if (i == 0) { areae.index = i; }
                    else { areae.index = i - 1; }
                    if (AreaDeleted != null) { AreaDeleted(areae); }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region エリアのサイズ変更
        /// <summary>
        /// エリアのサイズを変更する
        /// </summary>
        /// <param name="x">マウス位置x</param>
        ///  <param name="y">マウス位置y</param>
        private void AreaSizeMove(float x, float y)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            PointF HarfNode = new PointF();

            int SelectIndex = SpeedAreas.IndexOfKey(m_SelectedArea);

            HarfNode.X = (float)((RouteNodes.ElementAt(SelectIndex).Value.X + RouteNodes.ElementAt(SelectIndex + 1).Value.X) / (double)2);
            HarfNode.Y = (float)((RouteNodes.ElementAt(SelectIndex).Value.Y + RouteNodes.ElementAt(SelectIndex + 1).Value.Y) / (double)2);

            // 2点を通る直線の方程式
            // ax+by+c = 0
            double a = 0;
            double b = 0;
            double c = 0;
            // a = y2-y1
            a = m_SelectedAreaNodes[m_SelectedAreaNode].Y - HarfNode.Y;
            // b = -(x2-x1)
            b = -(m_SelectedAreaNodes[m_SelectedAreaNode].X - HarfNode.X);
            // c=(x2-x1)y1-(y2-y1)x1
            c = (m_SelectedAreaNodes[m_SelectedAreaNode].X - HarfNode.X) * HarfNode.Y - (m_SelectedAreaNodes[m_SelectedAreaNode].Y - HarfNode.Y) * HarfNode.X;

            // 直線に垂直である点を通る方程式(dx+ey+f=0)
            // bx-ay-bx0+ay0=0
            double d = 0;
            double e = 0;
            double f = 0;
            d = b;
            e = -a;
            f = -b * x + a * y;

            // 2直線の交点を求める
            double newX = (b * f - e * c) / (a * e - d * b);
            double newY = (d * c - a * f) / (a * e - d * b);

            double L = Math.Sqrt(Math.Pow((HarfNode.X - newX), 2) + Math.Pow((HarfNode.Y - newY), 2));

            PointF O = new PointF(RouteNodes.ElementAt(SelectIndex).Value.X, RouteNodes.ElementAt(SelectIndex).Value.Y);
            PointF P = new PointF(RouteNodes.ElementAt(SelectIndex + 1).Value.X, RouteNodes.ElementAt(SelectIndex + 1).Value.Y);
            double Z = GaisekiView(O, P, new PointF(x, y));
            if (m_SelectedAreaNode == C_CHANGE_WIDTH_LEFT)
            {
                if (Z > 0)
                    SpeedAreas.ElementAt(SelectIndex).Value.LeftWidth = L;
                else
                    SpeedAreas.ElementAt(SelectIndex).Value.LeftWidth = C_MINIMUM_AREA_WIDTH;
            }
            else if (m_SelectedAreaNode == C_CHANGE_WIDTH_RIGHT)
            {
                if (Z < 0)
                    SpeedAreas.ElementAt(SelectIndex).Value.RightWidth = L;
                else
                    SpeedAreas.ElementAt(SelectIndex).Value.RightWidth = C_MINIMUM_AREA_WIDTH;
            }

            UpdateSpeedArea();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region 描画
        private new void OnPaint(object sender, PaintEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                //RCD
                // 減速エリアの描画
                if (m_config.SlowLeftWidth != null && m_config.SlowRightWidth != null)
                {
                    foreach (int key in SpeedAreas.Keys) { Area_Paint(SpeedAreas[key], Color.Blue, e, 1); }
                }
                if (m_config.PauseLeftWidth != null && m_config.PauseRightWidth != null)
                {
                    foreach (int key in SpeedAreas.Keys) { Area_Paint(SpeedAreas[key], Color.Yellow, e, 2); }
                }
                if (m_config.StopLeftWidth != null && m_config.StopRightWidth != null)
                {
                    foreach (int key in SpeedAreas.Keys) { Area_Paint(SpeedAreas[key], Color.IndianRed, e, 3); }
                }
                //RCD

                switch (m_Mode)
                {
                    case AtrptMode.Route:
                        if (m_EditMode == AtrptEditmode.Node)
                        {
                            if (RouteNodes.ContainsKey(m_SelectedNode))
                            {
                                // 選択されているノードの色変更
                                Node_Paint(RouteNodes[m_SelectedNode], Brushes.Red, e);
                            }
                        }
                        else if (m_EditMode == AtrptEditmode.SpeedArea)
                        {
                            if (SpeedAreas.ContainsKey(m_SelectedArea))
                            {
                                // 選択されているエリアの色変更
                                Area_Paint(SpeedAreas[m_SelectedArea], Color.Red, e);
                                // 選択されているエリアノードの描写
                                foreach (int Keys in m_SelectedAreaNodes.Keys) { AreaNode_Paint(m_SelectedAreaNodes[Keys], e); }
                            }
                        }
                        break;
                    case AtrptMode.Other:
                        if (m_SelectedFacilityIdx != -1)
                        {
                            ////エリアの描画
                            //OtherAreaPaint_Three(m_SelectedFacilityIdx, e);

                            ////選択エリアの色変更
                            //if (m_AddMode != AtrptAddMode.mode0) OtherAreaPaint_One(e, Color.Red, m_config.OtherAreas[m_SelectedFacilityIdx].area[(int)m_AddMode]);

                            //作成済みのエリアの描画
                            NewOtherAreaCreated_Paint(e, m_config.OtherAreas[m_SelectedFacilityIdx].AreaCode);
                        }
                        break;
                    case AtrptMode.OtherAdd:
                        //作成中のエリアの描画
                        if (m_AddMode != AtrptAddMode.mode0)
                        {
                            NewFacilityArea_Paint(e);
                            NewFacilityArea_Connect(e);
                        }
                        //作成済みのエリアの描画
                        NewOtherAreaCreated_Paint(e, OtherAreaCode.FacilityArea);

                        break;
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #region 新設備エリアノードの描画
        /// <summary>
        /// 点を描画する
        /// </summary>
        public void NewFacilityArea_Paint(PaintEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (m_NewFacilityArea[(int)m_AddMode] != null)
            {
                foreach (PointF p in m_NewFacilityArea[(int)m_AddMode])
                {
                    PointF Lp = toLocalCoordinate(p);

                    float x = Lp.X - m_configconst.C_NODE_WIDTH / 4;
                    float y = Lp.Y - m_configconst.C_NODE_WIDTH / 4;

                    e.Graphics.FillEllipse(Brushes.Red, x, y, m_configconst.C_NODE_WIDTH / 2, m_configconst.C_NODE_WIDTH / 2);
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region ノードをつなぐ
        public void NewFacilityArea_Connect(PaintEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            float FroX = 0;
            float FroY = 0;
            if (m_NewFacilityArea[(int)m_AddMode] != null)
            {
                using (Pen p = new Pen(Color.Red, 1))
                {
                    for (int i = 0; i <= m_NewFacilityArea[(int)m_AddMode].Count() - 1; i++)
                    {
                        PointF Lp = toLocalCoordinate(m_NewFacilityArea[(int)m_AddMode][i]);
                        if (i != 0) e.Graphics.DrawLine(p, FroX, FroY, Lp.X, Lp.Y);
                        FroX = Lp.X;
                        FroY = Lp.Y;
                    }
                }
            }
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region 作成済み新規エリアの描画
        public void NewOtherAreaCreated_Paint(PaintEventArgs e, OtherAreaCode code)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //作成済みのエリアの描画
            if (m_NewFacilityArea != null)
            {
                for (int i = 0; i <= m_NewFacilityArea.Count() - 1; i++)
                {
                    if (i == (int)m_SelectedNewArea)
                    {
                        OtherAreaPaint_One(e, Color.Red, m_NewFacilityArea[i]);
                    }
                    else if (i != (int)m_AddMode)
                    {
                        Color c = m_configconst.C_COLOR_FACILITYAREA;
                        switch (code)
                        {
                            case OtherAreaCode.GoalArea:
                                {
                                    OtherAreaPaint_One(e, m_configconst.C_COLOR_GOALAREA, m_NewFacilityArea[i]);
                                    break;
                                }
                            case OtherAreaCode.TakeoverArea:
                                {
                                    OtherAreaPaint_One(e, m_configconst.C_COLOR_TAKEOVERAREA, m_NewFacilityArea[i]);
                                    break;
                                }
                            case OtherAreaCode.FacilityArea:
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            OtherAreaPaint_One(e, ControlPaint.Light(c), m_NewFacilityArea[i]);
                                            break;
                                        case 1:
                                            OtherAreaPaint_One(e, ControlPaint.Dark(c), m_NewFacilityArea[i]);
                                            break;
                                        case 2:
                                            OtherAreaPaint_One(e, c, m_NewFacilityArea[i]);
                                            break;
                                    }
                                    break;
                                }
                            case OtherAreaCode.GoalChangeStopArea:
                                {
                                    OtherAreaPaint_One(e, m_configconst.C_COLOR_GOALCHANGESTOPAREA, m_NewFacilityArea[i]);
                                    break;
                                }
                            case OtherAreaCode.GuideArea:
                                {
                                    OtherAreaPaint_One(e, m_configconst.C_COLOR_GUIDEAREA, m_NewFacilityArea[i]);
                                    break;
                                }
                        }
                    }
                }
            }
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #endregion

        #region 1ノードエリアの描写
        public void AreaNode_Paint(RouteNode Paintnode, PaintEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            using (Pen p = new Pen(Color.Black, 1))
            {
                PointF pf = toLocalCoordinate(new PointF(Paintnode.X, Paintnode.Y));
                float x = (float)(pf.X - C_AREANODE_SIZE / (double)2);
                float y = (float)(pf.Y - C_AREANODE_SIZE / (double)2);
                e.Graphics.FillEllipse(Brushes.White, x, y, C_AREANODE_SIZE - 1, C_AREANODE_SIZE - 1);
                e.Graphics.DrawEllipse(p, x, y, C_AREANODE_SIZE, C_AREANODE_SIZE);
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region マウス操作
        private void EditViewer_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                //クリック位置の補正と変換
                Point sp = Cursor.Position;
                PointF cp = this.PointToClient(sp);
                Point scrollPosition = this.AutoScrollPosition;
                cp.X = cp.X - scrollPosition.X;
                cp.Y = cp.Y - scrollPosition.Y;
                cp = toGlobalCoordinate(cp);

                switch (m_Mode)
                {
                    case AtrptMode.Route:
                        MouseDown_Route(cp, e);
                        break;
                    case AtrptMode.Other:
                        MouseDown_Other(cp, e);
                        break;
                    case AtrptMode.OtherAdd:
                        MouseDown_OtherAdd(cp, e);
                        break;
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void MouseDown_Route(PointF cp, MouseEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            float node_width = (float)toGlobalLength((double)m_configconst.C_NODE_WIDTH);

            // クリック位置とノードを比較
            foreach (KeyValuePair<int, RouteNode> kvp in RouteNodes)
            {
                RectangleF round = new RectangleF((float)(kvp.Value.X - (double)node_width / (double)2), (float)(kvp.Value.Y - (double)node_width / (double)2), node_width, node_width);
                if (round.Contains(cp.X, cp.Y))
                {
                    LOGGER.Info("ノードがクリックされました");
                    if (m_EditMode == AtrptEditmode.Node)
                    {
                        m_MouseDrag = true;
                    }
                    else
                    {
                        m_EditMode = AtrptEditmode.Node;
                    }

                    m_ClickCorrection = new PointF(kvp.Value.X - cp.X, kvp.Value.Y - cp.Y);
                    m_SelectedNode = kvp.Key;

                    // ノード選択イベント発生
                    RouteNodeEventArgs nodee = new RouteNodeEventArgs
                    {
                        rn = (RouteNode)kvp.Value.Clone(),
                        index = RouteNodes.IndexOfKey(kvp.Key)
                    };
                    PointF p = new PointF(nodee.rn.X, nodee.rn.Y);
                    p = point_substr(p, C_NODEVAL_DIGIT);
                    nodee.rn.X = p.X;
                    nodee.rn.Y = p.Y;
                    if (NodeSelectChanged != null) { NodeSelectChanged(nodee); }

                    this.Refresh();
                    return;
                }
            }

            // クリック位置とエリアノードを比較
            foreach (KeyValuePair<int, RouteNode> kvp in m_SelectedAreaNodes)
            {
                float areanode_width = (float)toGlobalLength((double)C_AREANODE_SIZE);
                RectangleF round = new RectangleF((float)(kvp.Value.X - (areanode_width / 2)), (float)(kvp.Value.Y - (areanode_width / 2)), areanode_width, areanode_width);
                if (round.Contains(cp.X, cp.Y))
                {
                    LOGGER.Info("エリアノードがクリックされました");
                    m_AreaNodeDrag = true;
                    m_SelectedAreaNode = m_SelectedAreaNodes[kvp.Key].ID;
                    this.Refresh();
                    return;
                }
            }

            // クリック位置とエリアを比較
            foreach (KeyValuePair<int, SpeedArea> kvp in SpeedAreas)
            {
                if (PointInPolygonView(cp, kvp.Value.Area))
                {
                    LOGGER.Info("エリアがクリックされました");
                    m_EditMode = AtrptEditmode.SpeedArea;
                    m_SelectedArea = kvp.Key;
                    // エリア範囲変更ノードセット
                    AreaNodeSet(kvp.Value.Area);
                    // エリア選択イベント発生
                    SpeedAreaEventArgs areae = new SpeedAreaEventArgs
                    {
                        sa = kvp.Value,
                        index = SpeedAreas.IndexOfKey(kvp.Key)
                    };
                    if (AreaSelectChanged != null) { AreaSelectChanged(areae); }

                    this.Refresh();
                    return;
                }
            }

            // クリックでノードを追加
            if (m_config.GlobalCoordinate.minCoordinate.X <= cp.X & cp.X <= m_config.GlobalCoordinate.maxCoordinate.X & m_config.GlobalCoordinate.minCoordinate.Y <= cp.Y & cp.Y <= m_config.GlobalCoordinate.maxCoordinate.Y)
            {
                LOGGER.Info("ノード追加がクリックされました");
                m_EditMode = AtrptEditmode.Node;
                AddRouteNode(cp.X, cp.Y);
                m_MouseDrag = true;
                return;
            }

            //m_SelectedNode = -1;
            //m_SelectedArea = -1;
            //this.Refresh();
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private void MouseDown_Other(PointF cp, MouseEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (m_NewFacilityArea != null)
            {
                for (int i = 0; i <= m_NewFacilityArea.Count() - 1; i++)
                {
                    if (PointInPolygonView(cp, m_NewFacilityArea[i]))
                    {
                        LOGGER.Info("作成中のその他エリアがクリックされました。");
                        m_MouseDrag = true;
                        m_SelectedNewArea = (AtrptAddMode)i;
                        m_ClickCorrection = cp;
                        return;
                    }
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private void MouseDown_OtherAdd(PointF cp, MouseEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (m_AddMode == AtrptAddMode.mode0)
            {
                for (int i = 0; i <= m_NewFacilityArea.Count() - 1; i++)
                {
                    if (PointInPolygonView(cp, m_NewFacilityArea[i]))
                    {
                        LOGGER.Info("作成中のその他エリアがクリックされました。");
                        m_MouseDrag = true;
                        m_SelectedNewArea = (AtrptAddMode)i;
                        m_ClickCorrection = cp;
                        return;
                    }
                }
            }
            else
            {
                LOGGER.Info("作成中のその他エリアの点追加がクリックされました。");
                //配列のリサイズ
                Array.Resize(ref m_NewFacilityArea[(int)m_AddMode], m_NewFacilityArea[(int)m_AddMode].Count() + 1);
                //点の追加
                m_NewFacilityArea[(int)m_AddMode][m_NewFacilityArea[(int)m_AddMode].Count() - 1] = cp;
            }
            this.Refresh();
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private void EditViewer_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                m_MouseDrag = false;
                m_AreaNodeDrag = false;

                switch (m_Mode)
                {
                    case AtrptMode.Route:
                        if (m_EditMode == AtrptEditmode.Node)
                        {
                            if (m_SelectedNode >= 0)
                                Focus();
                        }
                        else if (m_EditMode == AtrptEditmode.SpeedArea)
                        {
                            if (m_SelectedArea >= 0)
                                Focus();
                        }
                        break;
                    case AtrptMode.Other:
                        m_SelectedNewArea = AtrptAddMode.mode0;
                        break;
                    case AtrptMode.OtherAdd:
                        m_SelectedNewArea = AtrptAddMode.mode0;
                        break;
                }
                this.Refresh();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void EditViewer_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (m_MouseDrag || m_AreaNodeDrag)
                {
                    Point sp = Cursor.Position;
                    PointF cp = this.PointToClient(sp);
                    Point scrollPosition = this.AutoScrollPosition;
                    cp.X = cp.X - scrollPosition.X;
                    cp.Y = cp.Y - scrollPosition.Y;
                    cp = toGlobalCoordinate(cp);

                    switch (m_Mode)
                    {
                        case AtrptMode.Route:
                            if (m_MouseDrag)
                            {
                                // ノードの移動
                                PointF node = new PointF();
                                node = PointF.Add(cp, new SizeF(m_ClickCorrection));
                                node = MoveLimits(node);
                                // ノードの更新
                                UpdateRouteNode(m_SelectedNode, node.X, node.Y);
                                // エリアの更新
                                UpdateSpeedArea();
                            }
                            if (m_AreaNodeDrag)
                            {
                                if (m_SelectedAreaNode != 0)
                                {
                                    // エリアのサイズ変更
                                    AreaSizeMove(cp.X, cp.Y);
                                    // エリア範囲変更ノードセット
                                    AreaNodeSet(SpeedAreas[m_SelectedArea].Area);
                                }
                            }
                            this.Refresh();
                            break;
                        case AtrptMode.Other:
                            if (m_MouseDrag)
                            {
                                //エリアの移動
                                //UpdateOtherArea(cp);
                                UpdateNewOtherArea(cp);
                            }
                            this.Refresh();
                            break;
                        case AtrptMode.OtherAdd:
                            if (m_MouseDrag)
                            {
                                //エリアの移動
                                UpdateNewOtherArea(cp);
                            }
                            this.Refresh();
                            break;
                    }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { }
        }
        #endregion

        #region　移動範囲制限の移動
        /// <summary>
        /// クリック位移動置を画像の範囲内に制限する
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private PointF MoveLimits(PointF node)
        {
            // X軸
            if (node.X <= m_config.GlobalCoordinate.minCoordinate.X)
                node.X = m_config.GlobalCoordinate.minCoordinate.X;
            else if (node.X >= m_config.GlobalCoordinate.maxCoordinate.X)
                node.X = m_config.GlobalCoordinate.maxCoordinate.X;
            else
                node.X = node.X;
            // Y軸
            if (node.Y <= m_config.GlobalCoordinate.minCoordinate.Y)
                node.Y = m_config.GlobalCoordinate.minCoordinate.Y;
            else if (node.Y >= m_config.GlobalCoordinate.maxCoordinate.Y)
                node.Y = m_config.GlobalCoordinate.maxCoordinate.Y;
            else
                node.Y = node.Y;

            return node;
        }

        #endregion

        #region エリアの範囲変更用ノードをセット
        public void AreaNodeSet(PointF[] AreasVal)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                m_SelectedAreaNodes.Clear();

                // 左ノード
                RouteNode nodeleft = new RouteNode
                {
                    ID = C_CHANGE_WIDTH_LEFT,
                    X = (AreasVal[2].X + AreasVal[3].X) / (float)2,
                    Y = (AreasVal[2].Y + AreasVal[3].Y) / (float)2
                };
                m_SelectedAreaNodes.Add(nodeleft.ID, nodeleft);

                // 右ノード
                RouteNode noderight = new RouteNode
                {
                    ID = C_CHANGE_WIDTH_RIGHT,
                    X = (AreasVal[0].X + AreasVal[1].X) / (float)2,
                    Y = (AreasVal[0].Y + AreasVal[1].Y) / (float)2
                };
                m_SelectedAreaNodes.Add(noderight.ID, noderight);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region キーによる削除
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            //理想経路設定中にdeleteキーが押されたとき
            if (e.KeyData == Keys.Delete)
            {
                if (m_Mode == AtrptMode.Route)
                {
                    if (m_EditMode == AtrptEditmode.Node)
                    {
                        if (m_SelectedNode >= 0)
                            DeleteRouteNode(m_SelectedNode);
                    }
                }
            }

            //その他エリア作成中にescまたはdeleteキーが押されたとき
            if (e.KeyData == Keys.Escape || e.KeyData == Keys.Escape)
            {
                if (m_Mode == AtrptMode.OtherAdd)
                {
                    if (m_NewFacilityArea[(int)m_AddMode].Count() >= 1)
                    {
                        Array.Resize(ref m_NewFacilityArea[(int)m_AddMode], m_NewFacilityArea[(int)m_AddMode].Count() - 1);
                        this.Refresh();
                    }
                }
            }
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region その他エリアの作成・変更・削除

        public void ChangeOtherArea(PointF[][] pointFs)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            AddNewFacilityArea();

            m_NewFacilityArea = pointFs;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        /// <summary>
        /// 新規エリアの初期化
        /// </summary>
        public void AddNewFacilityArea()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_NewFacilityArea = new PointF[3][];
            m_NewFacilityArea[0] = new PointF[0];
            m_NewFacilityArea[1] = new PointF[0];
            m_NewFacilityArea[2] = new PointF[0];

            this.Refresh();
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        public void ResetFacilityArea(AtrptAddMode addmode)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_NewFacilityArea[(int)addmode] = new PointF[0];

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// エリアの追加
        /// </summary>
        /// <param name="fa"></param>
        public void EnterNewFacilityArea(OtherArea fa)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //追加する
            fa.area = (PointF[][])m_NewFacilityArea.Clone();
            m_config.OtherAreas.Add(fa);
            m_SelectedFacilityIdx = m_config.OtherAreas.Count() - 1;

            FacilityAreaEventArgs e = new FacilityAreaEventArgs
            {
                fa = fa,
                index = m_SelectedFacilityIdx
            };
            if (FacilityAdd != null) { FacilityAdd(e); }

            this.Refresh();
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// エリアの変更
        /// </summary>
        /// <param name="fa"></param>
        /// <param name="idx"></param>
        public void ModifyFacilityArea(OtherArea fa, int idx)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //変更する
            //fa.area = m_config.OtherAreas[idx].area;
            fa.area = m_NewFacilityArea;
            m_config.OtherAreas[idx].area = fa.area;
            m_config.OtherAreas[idx].Name = fa.Name;
            m_config.OtherAreas[idx].AreaCode = fa.AreaCode;
            m_config.OtherAreas[idx].Number = fa.Number;
            m_config.OtherAreas[idx].FacilityID = fa.FacilityID;
            m_config.OtherAreas[idx].FacilitySID = fa.FacilitySID;

            FacilityAreaEventArgs e = new FacilityAreaEventArgs
            {
                fa = fa,
                index = idx
            };
            if (FacilityChanged != null) { FacilityChanged(e); }

            this.Refresh();
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// エリアの削除
        /// </summary>
        /// <param name="idx"></param>
        public void DeleteFacilityArea(int idx)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_config.OtherAreas.RemoveAt(idx);
            m_SelectedFacilityIdx = -1;
            //m_config.GearRate = 100;

            FacilityAreaEventArgs e = new FacilityAreaEventArgs
            {
                index = idx
            };
            if (FacilityDeleted != null) { FacilityDeleted(e); }

            this.Refresh();
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 新規エリア作成入力チェック
        /// </summary>
        /// <returns></returns>
        public Boolean NewFaclityAreaCheck(int code)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            Boolean check = false;
            if (m_AddMode == AtrptAddMode.mode0)
            {
                if (code == (int)OtherAreaCode.FacilityArea)
                {
                    if (m_NewFacilityArea[0].Count() >= 3 && m_NewFacilityArea[1].Count() >= 3 && m_NewFacilityArea[2].Count() >= 3)
                    {
                        check = true;
                    }
                }
                else
                {
                    if (m_NewFacilityArea[0].Count() >= 3)
                    {
                        check = true;
                    }
                }
            }
            else
            {
                if (m_NewFacilityArea[(int)m_AddMode].Count() >= 3)
                {
                    check = true;
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return check;
        }

        /// <summary>
        /// モード切替
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="addmode"></param>
        public void AddModeChange(AtrptMode mode, AtrptAddMode addmode)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_Mode = mode;
            m_AddMode = addmode;

            this.Refresh();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// その他エリア作成完了時
        /// </summary>
        public void EntNewArea()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (m_AddMode != AtrptAddMode.mode0)
            {
                //ポリゴンを閉じる
                Array.Resize(ref m_NewFacilityArea[(int)m_AddMode], m_NewFacilityArea[(int)m_AddMode].Count() + 1);
                m_NewFacilityArea[(int)m_AddMode][m_NewFacilityArea[(int)m_AddMode].Count() - 1] = m_NewFacilityArea[(int)m_AddMode][0];
            }

            this.Refresh();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region ログ用結合
        public string Node_Union(RouteNode node)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return "ID：" + node.ID + "　X：" + node.X + "　Y：" + node.Y;
        }
        #endregion

        #region 理想経路一括更新
        public void UpdeteIdealRoute(RouteNodeList rnl, SpeedAreaList sal)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            RouteNodes.Clear();
            SpeedAreas.Clear();
            for (int i = 0; i <= rnl.RouteNode.Count - 1; i++) { RouteNodes.Add(rnl.RouteNode[i].ID, rnl.RouteNode[i]); }
            for (int i = 0; i <= rnl.RouteNode.Count - 2; i++)
            {
                SpeedAreas.Add(sal.SpeedArea[i].ID, sal.SpeedArea[i]);
            }
            for (int i = 0; i <= SpeedAreas.Count() - 1; i++)
            {
                SpeedAreas.ElementAt(i).Value.Area = AreaSet(i, SpeedAreas.ElementAt(i).Value,0);
            }
            for (int i = 0; i <= SpeedAreas.Count() - 1; i++)
            {
                SpeedAreas.ElementAt(i).Value.SlowArea = AreaSet(i, SpeedAreas.ElementAt(i).Value, 1);
            }
            for (int i = 0; i <= SpeedAreas.Count() - 1; i++)
            {
                SpeedAreas.ElementAt(i).Value.PauseArea = AreaSet(i, SpeedAreas.ElementAt(i).Value, 2);
            }
            for (int i = 0; i <= SpeedAreas.Count() - 1; i++)
            {
                SpeedAreas.ElementAt(i).Value.StopArea = AreaSet(i, SpeedAreas.ElementAt(i).Value, 3);
            }
            UpdateSpeedArea();

            m_SelectedNode = -1;
            m_SelectedAreaNode = -1;
            m_SelectedArea = -1;
            this.Refresh();

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

    }


}
