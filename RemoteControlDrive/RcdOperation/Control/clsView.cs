using AtrptShare;
using MapViewer;
using RcdCmn;
using RcdOperationSystemConst;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapViewer.UserStatusPanel;
using static RcdDao.MCarSpecDao;
using static RcdDao.MCourseDao;
using static RcdOperation.Control.CommCam;

namespace RcdOperation.Control
{
    public partial class ControlMain : Form
    {
        /// 
        /// UIの更新系を記載する
        /// 表示していないときに無駄な処理を行わないように注意
        /// 


        // ※
        private const int C_DIRECTION_LINE_LENGTH = 60;
        private const int C_NODE_WIDTH = 6;
        private const int C_ARROW_SIZE = 3;
        private Color C_COLOR_GOALAREA = Color.Yellow;
        private Color C_COLOR_FACILITYAREA = Color.Green;
        private Color C_COLOR_GOALCHANGESTOPAREA = Color.Orange;
        private Color C_COLOR_GUIDEAREA = Color.Blue;
        // ※

        /// <summary>
        /// 再描画タイマー
        /// </summary>
        System.Threading.Timer m_timer_rd;

        public bool m_AutoScroll = true;

        private const int C_RD_INTERVAL = 100;

        #region ### 表示初期設定 ###
        /// <summary>
        /// 描画初期設定
        /// </summary>
        private void SetViewConfig()
        {
            ViewerConfig viewerConfig = new ViewerConfig();
            viewerConfig.RouteNodeList = new SortedList<int, RouteNode>();
            viewerConfig.SpeedAreaList = new SortedList<int, SpeedArea>();
            if (m_RouteNodes != null)
            {
                for (int i = 0; i <= m_RouteNodes.Count - 1; i++) { viewerConfig.RouteNodeList.Add(m_RouteNodes[i].ID, m_RouteNodes[i]); }
                for (int i = 0; i <= m_SpeedAreas.Count - 1; i++) { viewerConfig.SpeedAreaList.Add(m_SpeedAreas[i].ID, m_SpeedAreas[i]); }
            }

            viewerConfig.OtherAreas = m_ConfigData.OtherAreas;

            viewerConfig.GlobalCoordinate = new GlobalCoordinate()
            {
                minCoordinate = m_ConfigData.GlobalCoordinate.minCoordinate,
                maxCoordinate = m_ConfigData.GlobalCoordinate.maxCoordinate
            };

            viewerConfig.SlowLeftWidth = m_ConfigData.SlowLeftWidth;
            viewerConfig.SlowRightWidth = m_ConfigData.SlowRightWidth;
            viewerConfig.PauseLeftWidth = m_ConfigData.PauseLeftWidth;
            viewerConfig.PauseRightWidth = m_ConfigData.PauseRightWidth;
            viewerConfig.StopLeftWidth = m_ConfigData.StopLeftWidth;
            viewerConfig.StopRightWidth = m_ConfigData.StopRightWidth;

            // ※
            viewerConfig.C_NODE_WIDTH = C_NODE_WIDTH;
            viewerConfig.C_ARROW_SIZE = C_ARROW_SIZE;
            viewerConfig.C_COLOR_GOALAREA = C_COLOR_GOALAREA;
            viewerConfig.C_COLOR_FACILITYAREA = C_COLOR_FACILITYAREA;
            viewerConfig.C_COLOR_GOALCHANGESTOPAREA = C_COLOR_GOALCHANGESTOPAREA;
            viewerConfig.C_COLOR_GUIDEAREA= C_COLOR_GUIDEAREA;
            viewerConfig.C_DIRECTION_LINE_LENGTH = C_DIRECTION_LINE_LENGTH;
            // ※

            // 描画初期設定
            Viewer.ViewSetConfig(Image.FromFile(@"map.png"), (ViewerConfig)viewerConfig.Clone(), LOGGER);

            // 
            lblReset();

            // 再描画スタート
            m_timer_rd = new System.Threading.Timer(Callback_ReDraw, null, C_RD_INTERVAL, C_RD_INTERVAL);
        }
        #endregion

        #region ### 再描画 ###
        /// <summary>
        /// 再描画タイマー処理
        /// </summary>
        /// <param name="state"></param>
        private void Callback_ReDraw(object state)
        {

            try
            {
                // タイマー停止
                if (m_timer_rd == null) { return; }
                m_timer_rd.Change(Timeout.Infinite, Timeout.Infinite);



                // 表示されていなければ、画面の更新は行わない
                if (this.Visible)
                {
                    Invoke(new Action(lblResetCam));

                    // 接続状態の更新
                    Invoke(new Action(CommlblCheck));

                    // 走行ステータス表示
                    StatusPanel();

                    // 待機車両ステータス表示
                    WaitStatusPanel();

                    // 走行停止時、メッセージ表示
                    StatusDisplay statusDisplay = panel_Status.GetCurrentStatusDisplay();
                    if (statusDisplay.statusCode == C_PANEL_STATUS_ERROR)
                    {
                        if (ControlVehicle.Instance.Vehicle != null)
                        {
                            Invoke(new Action(() => 
                            {
                                lbl_errorlabel.Enabled = true;
                                lbl_errorval.Text = $"{ ControlVehicle.Instance.ErrorMsg}"; 
                            }));
                        }
                    }
                    else
                    {
                        Invoke(new Action(() =>
                        {
                            lbl_errorlabel.Enabled = false;
                        }));
                    }
                }

                // TVECSに加速度舵角通知を送信するまで再描画を行わない
                if (m_calcstarted == 0) { return; }

                // 車両情報削除後は行わない
                if(ControlVehicle.Instance.Vehicle == null) { return; }

                // ラベルの更新
                if (this.Visible) Invoke(new Action(CalcVal_lblset));

                // 描画のデータ更新
                ControlViewer.SafetyMonitoringIndex safety = new ControlViewer.SafetyMonitoringIndex()
                {
                    SlowBackIdx = m_slowbackidx,
                    SlowFrontIdx = m_slowfrontidx,
                    PauseBackIdx = m_pausebackidx,
                    PauseFrontIdx = m_pausefrontidx,
                    StopBackIdx = m_stopbackidx,
                    StopFrontIdx = m_stopfrontidx,
                };
                Viewer.SetDrawInfo(m_CarInfo, m_avpappOutputData, m_contain_idx, m_CarCornerinfo, m_vehicleData, safety);

                // 侵入検知データ更新
                List<MapViewer.ControlViewer.InstrusionDetection> HandOverData = new List<MapViewer.ControlViewer.InstrusionDetection>();
                lock (m_ObjectDatas)
                {
                    foreach (InstrusionDetection objects in m_ObjectDatas)
                    {
                        if (objects.JudgesTarget)
                        {
                            MapViewer.ControlViewer.InstrusionDetection instrusion = new MapViewer.ControlViewer.InstrusionDetection();
                            instrusion.objectData = new MapViewer.ControlViewer.RecognitionObjectData();
                            instrusion.objectData.label = objects.objectData.label;
                            instrusion.Position = objects.Position;
                            instrusion.Radius = objects.Radius;
                            instrusion.JudgesTarget = objects.JudgesTarget;
                            HandOverData.Add(instrusion);
                        }
                    }
                }
                Viewer.m_detectdatas = new List<ControlViewer.InstrusionDetection>( HandOverData);


                if (this.Visible)
                {
                    if (m_AutoScroll) Viewer.AutoScrollView();

                    // 描画の更新
                    Invoke((Action)(() =>
                    {
                        //if (C_AUTO_SCROLL && m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL) { Viewer.AutoScrollView(); }
                        Viewer.Refresh();
                    }));
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally
            {
                if (m_timer_rd != null) { m_timer_rd.Change(C_RD_INTERVAL, C_RD_INTERVAL); }

            }
        }


        private void CommlblCheck()
        {
            if (m_ManagementCl.CommStatus) { lbl_sign_mcl.BackColor = Color.LawnGreen; }
            else { lbl_sign_mcl.BackColor = Color.Red; }

            if (GetCamCommStatus(MainC)) { lbl_sign_MainCamCl.BackColor = Color.LawnGreen;}
            else { lbl_sign_MainCamCl.BackColor = Color.Red; }

            if (GetCamCommStatus(SubC) && SubCamType != null) { lbl_sign_SubCamCl.BackColor = Color.LawnGreen; }
            else { lbl_sign_SubCamCl.BackColor = Color.Red; }

            if (TvecsCommClStatus) { lbl_sign_tvecscl.BackColor = Color.LawnGreen; }
            else { lbl_sign_tvecscl.BackColor = Color.Red; }

            if (TvecsCommSrvStatus) { lbl_sign_tvecssrv.BackColor = Color.LawnGreen; }
            else { lbl_sign_tvecssrv.BackColor = Color.Red; }

            //※カメラヘルスチェックの状態更新
            lstv_camstatus_add(MainC);
            if(SubCamType != null) lstv_camstatus_add(SubC);
            lbl_sign_MainCamHealth.BackColor = (GetCamHeakthStatus(MainC) == OperationConst.C_NORMAL) ? Color.Lime : Color.Red;
            if (SubCamType != null) lbl_sign_SubCamHealth.BackColor = (GetCamHeakthStatus(SubC) == OperationConst.C_NORMAL) ? Color.Lime : Color.Red;
        }

        public void CalcVal_lblset()
        {
            lbl_CarAngle.Text = CommonProc.ToDegree(m_CarInfo.angle).ToString("F2");
            lbl_CarSpeed.Text = m_CarInfo.speed.ToString();
            string yawrad = CommonProc.ToDegree(m_CarInfo.calcYawrate).ToString();
            lbl_YawRad.Text = (yawrad.Length > 7) ? yawrad.Substring(0, 7) : yawrad;
            lbl_Yawrate.Text = m_CarInfo.Yawrate.ToString();

            lbl_xPosi.Text = Math.Round(m_avpappOutputData.control_point.x, 4).ToString();
            lbl_yPosi.Text = Math.Round(m_avpappOutputData.control_point.y, 4).ToString();

            lbl_xCalcPoint.Text = Math.Round(Viewer.m_CarOrder.xCalcPoint, 4).ToString();
            lbl_yCalcPoint.Text = Math.Round(Viewer.m_CarOrder.yCalcPoint, 4).ToString();

            lbl_AngleOri.Text = Math.Round(CommonProc.ToDegree(m_avpappOutputData.draw_info.guard_front_angle), 4).ToString();
            lbl_Angle.Text = Math.Round(CommonProc.ToDegree(m_avpappOutputData.target_steering_angle), 4).ToString();

            if (m_contain_idx != -1) lbl_AimSpeed.Text = m_SpeedAreas[m_contain_idx].Speed.ToString();
            lbl_Acceleration.Text = Math.Round(m_avpappOutputData.target_gx, 3).ToString("0.00");

            if (m_avpappOutputData.panaOutputData.log != null)
            {
                lbl_L_calc.Text = Math.Round(m_avpappOutputData.panaOutputData.L, 4).ToString();

                lbl_kp_calc.Text = Math.Round(m_avpappOutputData.panaOutputData.log.p1, 4).ToString();
                lbl_kd_calc.Text = Math.Round(m_avpappOutputData.panaOutputData.log.d1, 4).ToString();
                lbl_ki_calc.Text = Math.Round(m_avpappOutputData.panaOutputData.log.i1, 4).ToString();
                lbl_ff_calc.Text = Math.Round(m_avpappOutputData.panaOutputData.log.f1, 4).ToString();
            }

            lbl_CamId.Text = m_CarInfo.CamID;
            lbl_camCordinateX.Text = GetCamRcvInfo(MainC).xPosition.ToString();
            lbl_camCordinateY.Text = GetCamRcvInfo(MainC).yPosition.ToString();
            lbl_CamTime.Text = GetCamRcvInfo(MainC).ProcTime;
            lbl_CamRadx.Text = GetCamRcvInfo(MainC).MovexVector.ToString();
            lbl_CamRady.Text = GetCamRcvInfo(MainC).MoveyVector.ToString();
            lbl_CamRad.Text = (GetCamRcvInfo(MainC).MoveVector.ToString().Length > 7) ? GetCamRcvInfo(MainC).MoveVector.ToString().Substring(0, 7) : GetCamRcvInfo(MainC).MoveVector.ToString();

            lbl_FirstAngle.Text = m_CarInfo.Vector.ToString("F3");
            lbl_FirstVectorX.Text = m_CarInfo.xVector.ToString();
            lbl_FirstVectorY.Text = m_CarInfo.yVector.ToString();


            lbl_BodyNo.Text = m_CarNo;
            lbl_CarSpec.Text = m_CarSpec;
            lbl_GearRate.Text = m_ConfigData.GearRate.ToString();
            lbl_Wheelbase.Text = m_ConfigData.Wheelbase.ToString();
            lbl_GoalNo.Text = m_ConfigData.GoalNo.ToString();
        }

        private void lstv_camstatus_add(bool type)
        {
            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            List<HealthInfo> Healths = GetCamHeakthStatusInfo(type);

            //if (m_CamList.Count() <= 0)
            //{
            //    foreach (HealthInfo info in infos)
            //    {
            //        CamMaster cammaster = new CamMaster();
            //        cammaster.CamID = info.camID;
            //        m_CamList.Add(cammaster);
            //    }
            //    m_rows = new DataGridViewRow[infos.Count()];
            //}

            DataGridViewRow[] rows = new DataGridViewRow[Healths.Count()];

            //ステータス変換
            string status = "";
            for (int i = 0; i <= Healths.Count - 1; i++)
            {
                status = ConvCamStatus(Healths[i].status);

                //リスト追加
                string[] item = { Healths[i].camID.ToString(), status };
                DataGridViewRow row = new DataGridViewRow();
                if (type)
                {
                    row.CreateCells(dgv_MainCamHealth);
                }
                else
                {
                    row.CreateCells(dgv_SubCamHealth);
                }
                row.SetValues(item);
                rows[i] = row;
            }

            //bool change_flg = false;
            //for (int i = 0; i < m_rows.Count(); i++)
            //{
            //    if (m_rows[i] == null)
            //    {
            //        change_flg = true;
            //        break;
            //    }
            //    else if ((string)m_rows[i].Cells[0].Value != (string)rows[i].Cells[0].Value || m_rows[i].Cells[1].Value != rows[i].Cells[1].Value)
            //    {
            //        change_flg = true;
            //        break;
            //    }
            //}

            ////if (m_rows != rows)
            //if (change_flg)
            //{
            //    LOGGER.Info($"カメラのステータスを変更します");
            int rowIndex = type? dgv_MainCamHealth.FirstDisplayedScrollingRowIndex: dgv_SubCamHealth.FirstDisplayedScrollingRowIndex;
            //m_rows = rows;

            //if (m_FormClosing == false && this.IsDisposed == false)
            if (this.IsDisposed == false)
            {
                try
                {
                    if (type)
                    {
                        Invoke((Action)(() =>
                        {
                            dgv_MainCamHealth?.Rows.Clear();
                            dgv_MainCamHealth?.Rows.AddRange(rows);
                            if (rowIndex != -1) { dgv_MainCamHealth.FirstDisplayedScrollingRowIndex = rowIndex; }

                        }));
                    }
                    else
                    {
                        Invoke((Action)(() =>
                        {
                            dgv_SubCamHealth?.Rows.Clear();
                            dgv_SubCamHealth?.Rows.AddRange(rows);
                            if (rowIndex != -1) { dgv_SubCamHealth.FirstDisplayedScrollingRowIndex = rowIndex; }

                        }));
                    }
                    
                }
                catch (ObjectDisposedException e)
                {
                    LOGGER.Info(e.Message);
                }
            }
            //}

            //AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private string ConvCamStatus(int stcd)
        {
            string status = "";

            switch (stcd)
            {
                case 0:
                    status = "正常";
                    break;
                case 1:
                    status = "起動中";
                    break;
                case 2:
                    status = "映像断";
                    break;
                case 9:
                    status = "不明";
                    break;
            }

            return status;
        }
        #endregion

        #region ## 走行状態管理 ##
        private void StatusPanel()
        {
            if (m_CarControlStatus==OperationConst.ControlStatusType.C_NOT_IN_CONTROL)
            {
                if (m_error_flg==OperationConst.C_NORMAL)
                {
                    StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_IDLE);
                }
                else
                {
                    StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_ERROR);
                }
                
            }
            else if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
            {
                if (m_error_flg == OperationConst.C_NORMAL)
                {
                    uint status = GetPauseStatus();
                    if (status == OperationConst.AccelerationStatus.C_ACCELERATION_PAUSE || status == OperationConst.AccelerationStatus.C_ACCELERATION_PAUSE_DISCONNECTION)
                    {
                        StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_PAUSE);
                    }
                    else if (status == OperationConst.AccelerationStatus.C_ACCELERATION_EMERGENCY)
                    {
                        StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_ERROR);
                    }
                    else if (m_controlCenterData.deceleration_rate != 1.0)
                    {
                        StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_SLOW);
                    }
                    else
                    {
                        StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_RUNNING);
                    }
                }
                else
                {
                    StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_ERROR);
                }

            }
            else if (m_CarControlStatus == OperationConst.ControlStatusType.C_WAIT_CONTROL_END)
            {
                StatusPanelChange(UserStatusPanel.C_PANEL_STATUS_RUNNING);
            }

        }

        private void StatusPanelChange(int code)
        {
            UserStatusPanel.StatusDisplay status = panel_Status.GetCurrentStatusDisplay();
            if (status.statusCode != code)
            {
                Invoke(new Action<int>(panel_status_change), code);
            }
        }

        /// <summary>
        /// パネル表示ステータス変更
        /// </summary>
        private void panel_status_change(int statusCode)
        {
            panel_Status.ChangeStatus(statusCode);
        }


        /// <summary>
        /// 待機車両ステーパネル変更
        /// </summary>
        private void WaitStatusPanel()
        {
            PreVehicle preVehicle = PreVehicle.Instance;

            if (preVehicle.Status == PreVehicleStatus.Preparation)
            {
                WaitStatusPanelChange(UserStatusPanel.C_PANEL_STATUS_WAIT_IDOL);
            }
            else if (preVehicle.Status == PreVehicleStatus.Moving)
            {
                WaitStatusPanelChange(UserStatusPanel.C_PANEL_STATUS_WAIT_RUNNING);
            }
            else if (preVehicle.Status == PreVehicleStatus.WaitReadyBoot)
            {
                WaitStatusPanelChange(UserStatusPanel.C_PANEL_STATUS_WAIT_READYBOOT);
            }
            else if (preVehicle.Status == PreVehicleStatus.WaitStartup)
            {
                WaitStatusPanelChange(UserStatusPanel.C_PANEL_STATUS_WAIT_STARTUP);
            }
            else if (preVehicle.Status == PreVehicleStatus.WaitFrontVehicle)
            {
                WaitStatusPanelChange(UserStatusPanel.C_PANEL_STATUS_WAIT_LEAVING);
            }
            else if (preVehicle.Status == PreVehicleStatus.ErrorEnd)
            {
                WaitStatusPanelChange(UserStatusPanel.C_PANEL_STATUS_WAIT_ERROR);
            }

            if (preVehicle.Status == PreVehicleStatus.ErrorEnd)
            {
                Invoke(new Action(() =>
                {
                    lbl_errorlabel.Enabled = true;
                    lbl_errorval.Text = $"{PreVehicle.Instance.ErrorMsg}";
                }));
            }
            else
            {
                Invoke(new Action(() =>
                {
                    lbl_errorlabel.Enabled = false;
                    lbl_errorval.Text = $"";
                }));
            }
        }
        private void WaitStatusPanelChange(int code)
        {
            UserStatusPanel.StatusDisplay status = panel_WaitStatus.GetCurrentStatusDisplay();
            if (status.statusCode != code)
            {
                Invoke(new Action<int>(wait_panel_status_change), code);
            }
        }

        /// <summary>
        /// パネル表示ステータス変更
        /// </summary>
        private void wait_panel_status_change(int statusCode)
        {
            panel_WaitStatus.ChangeStatus(statusCode);
        }
        #endregion



        #region ### 初期化 ###
        private void DrowInitialize()
        {

            // 描画初期化
            Viewer.ResetView();
        }

        /// <summary>
        /// ラベル値初期化
        /// </summary>
        private void lblReset()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            // ManagementPC 接続情報
            lbl_mclip.Text = ConnectionConst.ManagementPC_IP;
            lbl_mclport.Text = ConnectionConst.ManagementPC_Port.ToString(); ;

            lblResetCam();

             // TVECS接続情報
            lbl_tclip.Text = m_connectionConst.Tvecs_IP;
            lbl_tclport.Text = ConnectionConst.Tvecs_Cl_Port.ToString();
            lbl_tsrvport.Text = ConnectionConst.Tvecs_Srv_Port.ToString(); ;

            //座標
            lbl_camCordinateX.Text = "";
            lbl_camCordinateY.Text = "";
            lbl_xPosi.Text = "";
            lbl_yPosi.Text = "";
            lbl_xCalcPoint.Text = "";
            lbl_yCalcPoint.Text = "";

            //舵角
            lbl_CarAngle.Text = "";
            lbl_AngleOri.Text = "";
            lbl_Angle.Text = "";

            //進行方向
            lbl_FirstAngle.Text = "";
            lbl_FirstVectorX.Text = "";
            lbl_FirstVectorY.Text = "";
            lbl_CamRad.Text = "";
            lbl_YawRad.Text = "";
            lbl_Yawrate.Text = "";

            //車速
            lbl_CarSpeed.Text = "";
            lbl_AimSpeed.Text = "";
            lbl_Acceleration.Text = "";

            //舵角計算
            lbl_kp.Text = (m_ConfigData.ConnectVal != null) ? m_ConfigData.CalcCoefficient.Kp.ToString() : "";
            lbl_kd.Text = (m_ConfigData.ConnectVal != null) ? m_ConfigData.CalcCoefficient.Kd.ToString() : "";
            lbl_ki.Text = (m_ConfigData.ConnectVal != null) ? m_ConfigData.CalcCoefficient.Ki.ToString() : "";
            lbl_L_calc.Text = "";
            lbl_kp_calc.Text = "";
            lbl_kd_calc.Text = "";
            lbl_ki_calc.Text = "";
            lbl_ff_calc.Text = "";

            //画像解析装置
            lbl_CamId.Text = "";
            lbl_CamRadx.Text = "";
            lbl_CamRady.Text = "";
            lbl_CamTime.Text = "";

            //TVECS
            lbl_BodyNo.Text = "";
            lbl_CarSpec.Text = "";
            lbl_GearRate.Text = "";
            lbl_Wheelbase.Text = "";
            lbl_GoalNo.Text = "";

            // エラー内容
            lbl_errorlabel.Enabled = false;
            lbl_errorval.Text = "";
            // エラー内容
            lbl_waiterrorlabel.Enabled = false;
            lbl_waiterrorval.Text = "";

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private void lblResetCam()
        {
            // 画像解析 接続情報
            if (MainCamType == OperationConst.CamType.Pana)
            {
                lbl_MainCamIp.Text = m_connectionConst.PanaImageAnalysis_IP;
                lbl_MainCamPort.Text = ConnectionConst.PanaImageAnalysis_Port.ToString();
                lbl_MainCamPortUdp_lbl.Visible = false;
                lbl_MainCamPortUdp_semi.Visible = false;
                lbl_MainCamPortUdp.Visible = false;
                if (SubCamType != null)
                {
                    lbl_SubCamIp.Text = m_connectionConst.AvpImageAnalysis_IP;
                    lbl_SubCamPort.Text = ConnectionConst.AvpImageAnalysis_Port_Tcp.ToString();
                    lbl_SubCamPortUdp_lbl.Visible = true;
                    lbl_SubCamPortUdp_semi.Visible = true;
                    lbl_SubCamPortUdp.Visible = true;
                    lbl_SubCamPortUdp.Text = ConnectionConst.AvpImageAnalysis_Port_Udp.ToString();
                }
                else
                {
                    lbl_SubCamIp.Text ="";
                    lbl_SubCamPort.Text = "";
                    lbl_SubCamPortUdp_lbl.Visible = false;
                    lbl_SubCamPortUdp_semi.Visible = false;
                    lbl_SubCamPortUdp.Visible = false;
                    lbl_SubCamPortUdp.Text = "";
                }
                
            }
            else if (MainCamType == OperationConst.CamType.AVP)
            {
                if (SubCamType != null)
                {
                    lbl_SubCamIp.Text = m_connectionConst.PanaImageAnalysis_IP;
                    lbl_SubCamPort.Text = ConnectionConst.PanaImageAnalysis_Port.ToString();
                    lbl_SubCamPortUdp_lbl.Visible = false;
                    lbl_SubCamPortUdp_semi.Visible = false;
                    lbl_SubCamPortUdp.Visible = false;
                }
                else
                {
                    lbl_SubCamIp.Text = "";
                    lbl_SubCamPort.Text = "";
                    lbl_SubCamPortUdp_lbl.Visible = false;
                    lbl_SubCamPortUdp_semi.Visible = false;
                    lbl_SubCamPortUdp.Visible = false;
                    lbl_SubCamPortUdp.Text = "";
                }

                lbl_MainCamIp.Text = m_connectionConst.AvpImageAnalysis_IP;
                lbl_MainCamPort.Text = ConnectionConst.AvpImageAnalysis_Port_Tcp.ToString();
                lbl_MainCamPortUdp_lbl.Visible = true;
                lbl_MainCamPortUdp_semi.Visible = true;
                lbl_MainCamPortUdp.Visible = true;
                lbl_MainCamPortUdp.Text = ConnectionConst.AvpImageAnalysis_Port_Udp.ToString();
            }

        }
        #endregion
    }
}
