﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Drawing;
using System.IO;
using RcdCmn;
using AtrptShare;

namespace AtrptCalc
{
    public class clsCalculator
    {
        AppLog LOGGER = AppLog.GetInstance();

        // ADD 20230528
        private List<OtherAreaRetention> m_otherArea;
        private bool m_goalflg = false;
        //

        #region ### 変数 ###
        /// <summary>
        /// 車両進行方向
        /// </summary>

        private double YawRad;
        private SettingInfo m_settingInfo = new SettingInfo();

        private ConfigData m_configData = new ConfigData();

        private GetCondition m_getCondition = new GetCondition();

        private PanaSettingData m_panaSettingData = new PanaSettingData();

        private double m_old_observed_vehicle_x = double.MaxValue;

        private double m_old_observed_vehicle_y = double.MaxValue;

        private bool m_carmovestart_flg = false;

        private VehicleData m_vehicleData = new VehicleData();

        private ObservedData m_observedData = new ObservedData();

        private ControlCenterData m_controlCenterData = new ControlCenterData();

        private PanaInputData m_panaInputData = new PanaInputData();

        private int m_ErrCnt = 0;

        private PointF m_controlPoint;

        //private bool Acqjudge_Execution_flg = false;

        private bool getPointStates = false;

        /// <summary>
        /// 車両前進前エラー測位点カウント
        /// </summary>
        private int m_errcount = 0;
        /// <summary>
        /// 
        private double YawrateTime;

        private double YawrateStandart;

        private object YawrateLock = new object();

        private double m_old_yaw = double.MaxValue;

        private double m_beforeRad = 0;

        //現在補正回数
        private double m_fixcount = double.MaxValue;

        ////補正対象外カメラ
        //private List<string> m_nofixcam;

        //現在カメラID
        private string m_nowcamid = "";

        private CalcInfo m_CalcInfo = new CalcInfo();

        private int m_contain_idx = -1;

        private SortedList<int, SpeedArea> SpeedAreas = new SortedList<int, SpeedArea>();

        private SortedList<int, RouteNode> RouteNodes = new SortedList<int, RouteNode>();

        private double m_SetSpeed;

        AVPAppOutputData m_avpappOutputData = new AVPAppOutputData();

        private OrderInfo m_OrderInfo = new OrderInfo();

        private DateTime m_oldTime;

        private double m_oldL;

        private double m_sL;

        private double m_oldAngle;

        private bool badopen;

        /// <summary>
        /// 目標速度への到達フラグ
        /// </summary>
        private int m_Achieve_flag;
        /// <summary>
        /// 加減速モード  1:加速　-1:減速
        /// </summary>
        private int m_Change_Mode;
        /// <summary>
        /// 現在指示加速度
        /// </summary>
        private float m_Current_Acceleration;
        /// <summary>
        /// 現在エリア目標速度
        /// </summary>
        private double m_Current_Area_Speed;

        /// <summary>
        /// 前回演算時ACT4
        /// </summary>
        private bool m_oldAct4 = C_ACT4_UP;

        #endregion

        #region 定数
        private const string C_FILE_TEXT_TYPE = "utf-8";

        private const bool C_ACT4_UP = true;

        /// <summary>
        /// 平常時
        /// </summary>
        private const int C_ACCELERATION_NOMAL = 0;
        /// <summary>
        /// 一時停止時
        /// </summary>
        private const int C_ACCELERATION_PAUSE = 1;
        /// <summary>
        /// ゴールエリア侵入時
        /// </summary>
        private const int C_ACCELERATION_GOAL = 2;
        /// <summary>
        /// 走行停止時
        /// </summary>
        private const int C_ACCELERATION_EMERGENCY = 3;
        #endregion

        #region ### コンストラクタ ###
        public clsCalculator(SettingInfo settingInfo, ConfigData configData, PanaSettingData panaSettingData)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_settingInfo = settingInfo;
            m_configData = configData;
            m_panaSettingData = panaSettingData; 
            LOGGER.Debug($"after[settingInfo.vehicle_type_id:{CommonProc.charToString(settingInfo.vehicle_type_id)}]");
            LOGGER.Debug($"after[ConnectVal.mClConnectIP:{m_configData.ConnectVal.mClConnectIP},CalcCoefficient.Kp:{m_configData.CalcCoefficient.Kp},ForwardDistance:{m_configData.ForwardDistance},WidthDistance:{m_configData.WidthDistance},FFGetStartPosition:{m_configData.FFGetStartPosition},LimitVal.AngleLimit:{m_configData.LimitVal.AngleLimit},GlobalCoordinate.maxCoordinate:{m_configData.GlobalCoordinate.maxCoordinate},Acceleration.DeceAmount:{m_configData.Acceleration.DeceAmount},GetCondition.GetRadian:{m_configData.GetCondition.GetRadian},OtherAreas数:{m_configData.OtherAreas.Count},GoalNo:{m_configData.GoalNo},GearRate:{m_configData.GearRate},Wheelbase:{m_configData.Wheelbase},trafficLights:{m_configData.trafficLights.Any()},シャッター:{m_configData.shutters.Any()},facilitys:{m_configData.facilitys.Any()},camscore:{m_configData.camscore},fixcnt:{m_configData.fixcnt},nofixcam:{m_configData.nofixcam}]");
            LOGGER.Debug($"after[actual_angle_wait:{m_panaSettingData.constData.actual_angle_wait},use_start_yawrate:{m_panaSettingData.constData.use_start_yawrate},start_yawrate:{m_panaSettingData.constData.start_yawrate},first_pos_x:{m_panaSettingData.constData.first_pos_x},first_pos_y:{m_panaSettingData.constData.first_pos_y},first_diff_x:{m_panaSettingData.constData.first_diff_x},first_diff_y:{m_panaSettingData.constData.first_diff_y},first_errmax_cnt:{m_panaSettingData.constData.first_errmax_cnt},score_all_use:{m_panaSettingData.constData.score_all_use},angle_limit_mode:{m_panaSettingData.constData.angle_limit_mode},acceleration_mode:{m_panaSettingData.constData.acceleration_mode},include_judge_area_count:{m_panaSettingData.constData.include_judge_area_count},first_vector:{m_panaSettingData.first_vector}]");
            LOGGER.Debug($"[パナ設定]ホイールベース:{m_panaSettingData.carSpec.Wheelbase},車両全幅:{m_panaSettingData.carSpec.WidthDistance},前方注視点距離:{m_panaSettingData.carSpec.ForwardDistance},ギア比:{m_panaSettingData.carSpec.GearRate}");
            LOGGER.Debug($"[コンフィグ設定]ホイールベース:{m_configData.Wheelbase},車両全幅:{m_configData.WidthDistance},前方注視点距離:{m_configData.ForwardDistance},ギア比:{m_configData.GearRate}");
            if (panaSettingData.constData.use_start_yawrate)
            {
                YawRad = ToRadian(m_panaSettingData.constData.start_yawrate);
            }
            else
            {
                YawRad = m_panaSettingData.first_vector;
            }
            if (m_panaSettingData.constData.rcdlogger_use)
            {
                string msg = " ,測位座標x,測位座標y,車載ヨーレートセンサ値 rad,車載ヨーレートセンサ値 deg,カメラ測位値 rad,カメラ測位値 deg,車載ヨーレートセンサ積分値 rad,車載ヨーレートセンサ積分値 deg,調停情報（測位値、積分値）積分値固定,調停後採用値 rad,調停後採用値 deg,前方注視点座標x,前方注視点座標y,目標軌道 横偏差基準座標x,目標軌道 横偏差基準座標y,横偏差,フィードバック制御 比例項,フィードバック制御 微分項,フィードバック制御 積分項,フィードフォワード制御項,目標舵角,実指示舵角,実舵角,目標車速,実車速,目標加減速度,カメラ画像取得時間,車両制御目標値送信時刻,ヨーレート取得時間,車両マスク信頼度,カメラID,ACT4";
                //RCDLoggerOpen();
                //RCDLogger($"計測項目({DateTime.Now.ToString("yyyyMMddHHmmss")}) ,→{msg}");
                LOGGER.Info($"計算項目 ,→{msg}");
            }
            YawrateTime = 0;
            YawrateStandart = 0;

            for (int i = 0; i <= m_configData.SpeedAreaList.SpeedArea.Count - 1; i++) { SpeedAreas.Add(m_configData.SpeedAreaList.SpeedArea[i].ID, m_configData.SpeedAreaList.SpeedArea[i]); }
            for (int i = 0; i <= m_configData.RouteNodeList.RouteNode.Count - 1; i++) { RouteNodes.Add(m_configData.RouteNodeList.RouteNode[i].ID, m_configData.RouteNodeList.RouteNode[i]); }
            LOGGER.Debug($"speedarea数:{SpeedAreas.Count}");
            Random rd = new System.Random();
            int rdkey = rd.Next(0, m_configData.RouteNodeList.RouteNode.Count - 1);
            LOGGER.Debug($"{rdkey}番目エリア加速度{SpeedAreas.Values[rdkey].Acceleration}");
            LOGGER.Debug($"ノード数:{RouteNodes.Count}");
            LOGGER.Debug($"{rdkey}番目ノードx座標{RouteNodes.Values[rdkey].X}");


            Initialization();

            // ADD 20230528
            //m_otherArea = new OtherAreaRetention[m_configData.OtherAreas.Count()];
            m_otherArea = new List<OtherAreaRetention>();
            for (int i = 0; i < m_configData.OtherAreas.Count(); i++)
            {
                if (m_configData.OtherAreas[i].AreaCode == 0)
                {
                    // ゴールエリアのみ追加
                    OtherAreaRetention otherArea = new OtherAreaRetention()
                    {
                        otherarea = new OtherArea()
                    };
                    otherArea.otherarea = m_configData.OtherAreas[i];
                    otherArea.Done = 0;

                    m_otherArea.Add(otherArea);
                }
            }
            //

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} end");
        }
        private void Initialization()
        {
            m_avpappOutputData.control_point = new ControlPoint();
            m_avpappOutputData.draw_info = new DrawInfo();
            m_avpappOutputData.panaOutputData = new PanaOutputData();
            m_avpappOutputData.panaOutputData.log = new calcLog();
        }
        #endregion

        #region ### 指示値計算 ###
        public AVPAppOutputData CalcCtrlTargetValue(VehicleData vehicleData, ObservedData observedData, ControlCenterData controlCenterData, PanaInputData panaInputData)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {

                m_vehicleData = vehicleData;
                m_observedData = observedData;
                m_controlCenterData = controlCenterData;
                m_panaInputData = panaInputData;
                //m_avpappOutputData.panaOutputData.panaFormatFlg = true;


                LOGGER.Debug($"after[act4:{m_panaInputData.act4},vehicleData.vehicle_speed:{m_vehicleData.vehicle_speed},vehicleData.steer_angle:{m_vehicleData.steer_angle},vehicleData.gx:{m_vehicleData.gx},vehicleData.gy:{m_vehicleData.gy},vehicleData.yr:{m_vehicleData.yr},vehicleData.vehicle_data_timestamp:{m_vehicleData.vehicle_data_timestamp},is_vehicle_stop:{m_vehicleData.is_vehicle_stop}steer_torque:{m_vehicleData.steer_torque},is_steer_torque_invalid:{m_vehicleData.is_steer_torque_invalid},steer_torque_enhaned:{m_vehicleData.steer_torque_enhaned},eps_motor_torque:{m_vehicleData.eps_motor_torque},is_vehicle_speed_from_vsc_invalid:{m_vehicleData.is_vehicle_speed_from_vsc_invalid},sp1_pulse:{m_vehicleData.sp1_pulse},vehicle_speed_from_vsc:{m_vehicleData.vehicle_speed_from_vsc},request_gx_from_b_pedal:{m_vehicleData.request_gx_from_b_pedal},request_gx_from_a_pedal:{m_vehicleData.request_gx_from_a_pedal},estimated_gx_form_vsc:{m_vehicleData.estimated_gx_form_vsc},gvc_invalid:{m_vehicleData.gvc_invalid},epb_lock_state:{m_vehicleData.epb_lock_state},shift_range_from_vmc:{m_vehicleData.shift_range_from_vmc},yawrate_sensor_1_invalid:{m_vehicleData.yawrate_sensor_1_invalid},yawrate_sensor_2_invalid:{m_vehicleData.yawrate_sensor_2_invalid},yaw_g_sensor_power_invalid:{m_vehicleData.yaw_g_sensor_power_invalid},g_sensor_1_invalid:{m_vehicleData.g_sensor_1_invalid},yaw_g_sensor_type_identification:{m_vehicleData.yaw_g_sensor_type_identification},g_sensor_2_invalid:{m_vehicleData.g_sensor_2_invalid},yaw_senser_1_value:{m_vehicleData.yaw_senser_1_value},yaw_senser_2_value:{m_vehicleData.yaw_senser_2_value}");
                LOGGER.Debug($"after[observedData.observed_vehicle_x:{m_observedData.observed_vehicle_x},observedData.observed_vehicle_y:{m_observedData.observed_vehicle_y},observedData.observed_vehicle_yaw:{m_observedData.observed_vehicle_yaw},observedData.observed_data_timestamp:{m_observedData.observed_data_timestamp}");
                string goalCoordinate = null;
                foreach (PointF goalPointF in m_controlCenterData.goalarea)
                {
                    //LOGGER.Info($@"計測項目（ゴールエリア座標） ,→,{goalPointF.X},{goalPointF.Y}");
                    goalCoordinate += $@"{goalPointF.X},{goalPointF.Y}, ";
                }
                LOGGER.Debug($"after[controlCenterData.timestamp:{m_controlCenterData.timestamp},controlCenterData.goalarea:{goalCoordinate},controlCenterData.request_control_status:{m_controlCenterData.request_control_status}]");
                LOGGER.Debug($"after[panainputData.camera_id:{m_panaInputData.tickcount},panaInputData.camera_id:{m_panaInputData.camera_id},panaInputData.reliability:{m_panaInputData.reliability}]");

                double corrected_yaw;
                bool get = false;

                ////前回値比較
                //if (m_old_observed_vehicle_x != m_observedData.observed_vehicle_x || m_old_observed_vehicle_y != m_observedData.observed_vehicle_y)
                //{
                //車両がスタート位置から前進したか
                if (!m_carmovestart_flg && vehicleData.vehicle_speed != 0)
                {
                    m_carmovestart_flg = true;
                }

                if (m_carmovestart_flg)
                {
                    get = AcquisitionJudgment();
                }
                else
                {
                    get = AcquisitionJudgmentStart();
                    if (get)
                    {
                        if (!getPointStates)
                        {
                            getPointStates = true;
                        }
                    }
                    if (!get && !getPointStates)
                    {
                        m_errcount++;
                        if (m_errcount > m_panaSettingData.constData.first_errmax_cnt)
                        {
                            m_avpappOutputData.avp_app_state = 8;
                            m_avpappOutputData.avp_error_code = stringToCharCalc(Res.ErrorStatus.CAMSTART_POSITION_ERR.Code);
                            return m_avpappOutputData;
                        }
                    }
                    else
                    {
                        m_errcount = 0;
                    }
                }
                //}
                YawRad = YawrateUpdate(m_panaInputData.yr, m_panaInputData.tickcount, m_vehicleData.vehicle_speed, ToDegree(m_vehicleData.steer_angle));

                //double temp_yawrate = YawRad;

                //ヨーレート補正
                //if (!get)
                //{
                //    corrected_yaw = m_old_yaw;
                //}
                //else
                //{
                //    corrected_yaw = m_observedData.observed_vehicle_yaw;
                //}
                //一旦測位点取得しなかったら、カメラベクトル更新しないロジックは廃止
                corrected_yaw = ToDegree(m_observedData.observed_vehicle_yaw);
                YawrateAdjust(m_panaInputData.camera_id, m_panaInputData.reliability, corrected_yaw);

                if (!getPointStates)
                {
                    m_avpappOutputData.avp_app_state = 1;
                    return m_avpappOutputData;
                }

                //指示値計算＋α
                PointF[] p = new PointF[] { m_controlPoint, new PointF((float)m_observedData.observed_vehicle_x, (float)m_observedData.observed_vehicle_y) };
                try
                {
                    m_avpappOutputData = CalcAngleplus(p, (int)m_controlCenterData.request_control_status, m_controlCenterData.deceleration_rate);
                }
                catch (Exception e)
                {
                    ExceptionProcess.ComnExceptionConsoleProcess(e);
                    m_avpappOutputData.control_point.x = p[0].X;
                    m_avpappOutputData.control_point.y = p[0].Y;
                    m_avpappOutputData.target_steering_angle = m_vehicleData.steer_angle;
                    m_avpappOutputData.target_gx = -1;
                    AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
                    return m_avpappOutputData;

                }

                m_avpappOutputData.panaOutputData.get = get;
                string observed_timestamp = m_observedData.observed_data_timestamp.ToString().PadLeft(9, '0');
                if (m_panaSettingData.constData.rcdlogger_use)
                {
                    //RCDLogger($@"計測項目 ,→ ,{p[1].X} ,{p[1].Y} ,{ToRadian(m_vehicleData.yr)} ,{m_vehicleData.yr} ,{ToRadian(m_observedData.observed_vehicle_yaw)} ,{(m_observedData.observed_vehicle_yaw)} ,{YawRad} ,{ToDegree(YawRad)} ,積分値 ,{(YawRad)} ,{ToDegree(YawRad)} ,{m_OrderInfo.xPosi} ,{m_OrderInfo.yPosi} ,{m_OrderInfo.xCalcPoint} ,{m_OrderInfo.yCalcPoint} ,{m_OrderInfo.L} ,{m_OrderInfo.log.p1} ,{m_OrderInfo.log.d1} ,{m_OrderInfo.log.i1} ,{m_OrderInfo.log.f1} ,{m_OrderInfo.Angleori} ,{m_OrderInfo.Angle} ,{m_vehicleData.steer_angle} ,{m_CalcInfo.Speed} ,{m_vehicleData.vehicle_speed} ,{m_OrderInfo.Acceleration} ,{m_observedData.observed_data_timestamp.Substring(0, 2)}:{m_observedData.observed_data_timestamp.Substring(2, 2)}:{m_observedData.observed_data_timestamp.Substring(4, 2)}.{m_observedData.observed_data_timestamp.Substring(6)} ,{DateTime.Now.ToString("HH:mm:ss.fff")},{m_vehicleData.vehicle_data_timestamp},{m_panaInputData.reliability},{m_panaInputData.camera_id},{m_vehicleData.is_vehicle_stop}");
                    observed_timestamp = $"{ observed_timestamp.Substring(0, 2)}:{ observed_timestamp.Substring(2, 2)}:{ observed_timestamp.Substring(4, 2)}.{ observed_timestamp.Substring(6)}";
                    //RCDLogger($@"計測項目 ,→ ,{p[1].X} ,{p[1].Y} ,{ToRadian(m_panaInputData.yr)} ,{m_panaInputData.yr} ,{m_observedData.observed_vehicle_yaw} ,{ToDegree(m_observedData.observed_vehicle_yaw)} ,{YawRad} ,{ToDegree(YawRad)} ,積分値 ,{(YawRad)} ,{ToDegree(YawRad)} ,{m_OrderInfo.xPosi} ,{m_OrderInfo.yPosi} ,{m_OrderInfo.xCalcPoint} ,{m_OrderInfo.yCalcPoint} ,{m_OrderInfo.L} ,{m_OrderInfo.log.p1} ,{m_OrderInfo.log.d1} ,{m_OrderInfo.log.i1} ,{m_OrderInfo.log.f1} ,{m_OrderInfo.Angleori} ,{m_OrderInfo.Angle} ,{m_vehicleData.steer_angle} ,{m_CalcInfo.Speed} ,{m_vehicleData.vehicle_speed} ,{m_OrderInfo.Acceleration} ,{observed_timestamp} ,{DateTime.Now.ToString("HH:mm:ss.fff")},{m_vehicleData.vehicle_data_timestamp},{m_panaInputData.reliability},{m_panaInputData.camera_id},{m_panaInputData.act4},{m_contain_idx}");
                }
                LOGGER.Info($@"計算項目 ,→ ,{p[1].X} ,{p[1].Y} ,{ToRadian(m_panaInputData.yr)} ,{m_panaInputData.yr} ,{m_observedData.observed_vehicle_yaw} ,{ToDegree(m_observedData.observed_vehicle_yaw)} ,{YawRad} ,{ToDegree(YawRad)} ,積分値 ,{(YawRad)} ,{ToDegree(YawRad)} ,{m_OrderInfo.xPosi} ,{m_OrderInfo.yPosi} ,{m_OrderInfo.xCalcPoint} ,{m_OrderInfo.yCalcPoint} ,{m_OrderInfo.L} ,{m_OrderInfo.log.p1} ,{m_OrderInfo.log.d1} ,{m_OrderInfo.log.i1} ,{m_OrderInfo.log.f1} ,{m_OrderInfo.Angleori} ,{m_OrderInfo.Angle} ,{m_vehicleData.steer_angle} ,{m_CalcInfo.Speed} ,{m_vehicleData.vehicle_speed} ,{m_OrderInfo.Acceleration} ,{observed_timestamp} ,{DateTime.Now.ToString("HH:mm:ss.fff")},{m_vehicleData.vehicle_data_timestamp},{m_panaInputData.reliability},{m_panaInputData.camera_id},{m_panaInputData.act4}");
                LOGGER.Debug($"before[target_steering_angle:{m_avpappOutputData.target_steering_angle},target_gx:{m_avpappOutputData.target_gx},control_point.x:{m_avpappOutputData.control_point.x},control_point.y:{m_avpappOutputData.control_point.y},draw_info.yawrate:{m_avpappOutputData.draw_info.yawrate},draw_info.angle_guard{m_avpappOutputData.draw_info.angle_guard},draw_info.guard_front_angle:{m_avpappOutputData.draw_info.guard_front_angle},avp_app_state:{m_avpappOutputData.avp_app_state}");


                //前回値使用するためとして保持
                m_old_yaw = corrected_yaw;
                m_old_observed_vehicle_x = m_observedData.observed_vehicle_x;
                m_old_observed_vehicle_y = m_observedData.observed_vehicle_y;

                //前方注視点からリア軸中心へ
                m_avpappOutputData.control_point.x = m_avpappOutputData.control_point.x - (m_configData.ForwardDistance - m_panaSettingData.carSpec.RearOverhang) * Math.Cos(m_avpappOutputData.draw_info.yawrate);
                m_avpappOutputData.control_point.y = m_avpappOutputData.control_point.y - (m_configData.ForwardDistance - m_panaSettingData.carSpec.RearOverhang) * Math.Sin(m_avpappOutputData.draw_info.yawrate);
                //

            }
            catch (Exception ex)
            {
                m_avpappOutputData.avp_app_state = 8;
                m_avpappOutputData.avp_error_code = stringToCharCalc(Res.ErrorStatus.UNKNOWN_ERR.Code);
                ExceptionProcess.ComnExceptionConsoleProcess(ex);
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return m_avpappOutputData;
        }
        #endregion

        #region ### 終了処理 ###
        public void DeleteClsCalculator()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            if (m_panaSettingData.constData.rcdlogger_use && !badopen)
            {
                //m_RCDLogger.Dispose();
            }
            else
            {
                LOGGER.Info("計算ログファイルはオープンされていません");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region # 角度-ラジアン 変換 #
        private double ToRadian(double Degrees)
        {
            return ((Math.PI / 180) * Degrees);
        }
        #endregion

        #region 計算結果ログ出力
        //private StreamWriter m_RCDLogger;
        //private void RCDLoggerOpen()
        //{
        //    AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    string FileTextType = C_FILE_TEXT_TYPE;
        //    string filename = DateTime.Now.ToString("yyyyMMdd");
        //    int filenamecount = 0;
        //    bool opened = false;
        //    do
        //    {
        //        try
        //        {
        //            if (filenamecount == 0)
        //            {
        //                m_RCDLogger = new StreamWriter($"{m_panaSettingData.constData.rcdlogger_path}\\{filename}.log", true, System.Text.Encoding.GetEncoding(FileTextType));
        //            }
        //            else
        //            {
        //                m_RCDLogger = new StreamWriter($"{m_panaSettingData.constData.rcdlogger_path}\\{filename}_{filenamecount}.log", true, System.Text.Encoding.GetEncoding(FileTextType));
        //            }
        //            opened = true;
        //        }
        //        catch (Exception e)
        //        {
        //            LOGGER.Error($"{e.Message}");
        //            filenamecount++;
        //            if (filenamecount >= 10)
        //            {
        //                opened = true;
        //                badopen = true;
        //            }
        //        }
        //    } while (!opened);
        //    AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        //}

        //private void RCDLogger(string str)
        //{
        //    AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
        //    try
        //    {
        //        m_RCDLogger.WriteLine(str);
        //    }
        //    catch (Exception e)
        //    {
        //        LOGGER.Error($"{e.Message}");
        //    }
        //    AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        //}
        #endregion

        #region # 測位点取得判定 #
        /// <summary>
        /// 車両前進測位点取得判定
        /// </summary>
        /// <returns></returns>
        private bool AcquisitionJudgment()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            bool get = false;

            // 車のステータスセット
            PointF p = new PointF((float)m_observedData.observed_vehicle_x, (float)m_observedData.observed_vehicle_y);

            // 新たな測位点での予測先(制御点)を求める
            PointF p1 = YawratePoint(p);

            // 範囲判断
            if (m_ErrCnt >= m_configData.GetCondition.ErrorCount || PositionConditions(p1))
            {
                if (m_ErrCnt >= m_configData.GetCondition.ErrorCount) { AppLog.GetInstance().Info($"異常値除外限界値に達しました"); }
                AppLog.GetInstance().Info($"測位点を取得しました。x:{m_observedData.observed_vehicle_x} y:{m_observedData.observed_vehicle_y}");
                m_ErrCnt = 0;
                m_controlPoint = p1;
                get = true;

            }
            else
            {
                //取得範囲外測位点情報保持
                get = false;
                m_ErrCnt++;
                AppLog.GetInstance().Info($"測位点取得範囲外のため測位位置をスルーします{{{m_ErrCnt}}}。x:{m_observedData.observed_vehicle_x} y:{m_observedData.observed_vehicle_y}");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return get;
        }

        /// <summary>
        /// 車両前進前測位点取得判定
        /// </summary>
        /// <returns>車両ステータス情報</returns>
        private bool AcquisitionJudgmentStart()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            bool get = false;

            // 車のステータスセット
            PointF p = new PointF((float)m_observedData.observed_vehicle_x, (float)m_observedData.observed_vehicle_y);
            PointF p1 = YawratePoint(p);

            //範囲内であるか判定
            if (m_panaSettingData.constData.first_pos_x - m_panaSettingData.constData.first_diff_x <= p.X && p.X <= m_panaSettingData.constData.first_pos_x + m_panaSettingData.constData.first_diff_x)
            {
                if (m_panaSettingData.constData.first_pos_y - m_panaSettingData.constData.first_diff_y <= p.Y && p.Y <= m_panaSettingData.constData.first_pos_y + m_panaSettingData.constData.first_diff_y)
                {
                    get = true;
                    m_controlPoint = p1;
                    //範囲内であれば取得
                    AppLog.GetInstance().Info($"測位点を取得しました。x:{m_observedData.observed_vehicle_x} y:{m_observedData.observed_vehicle_y}");
                }
            }

            if (!get)
            {
                AppLog.GetInstance().Info($"測位点取得範囲外のため測位位置をスルーします。x:{m_observedData.observed_vehicle_x.ToString()} y:{m_observedData.observed_vehicle_y.ToString()}");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return get;
        }

        private PointF YawratePoint(PointF p)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //float x = (float)(p.X + m_panaSettingData.carSpec.ForwardDistance * Math.Cos(YawRad) + (m_panaSettingData.carSpec.WidthDistance / 2) * Math.Sin(YawRad));
            //float y = (float)(p.Y + m_panaSettingData.carSpec.ForwardDistance * Math.Sin(YawRad) - (m_panaSettingData.carSpec.WidthDistance / 2) * Math.Cos(YawRad));
            float x = (float)(p.X + m_configData.ForwardDistance * Math.Cos(YawRad) + (m_configData.WidthDistance / 2) * Math.Sin(YawRad));
            float y = (float)(p.Y + m_configData.ForwardDistance * Math.Sin(YawRad) - (m_configData.WidthDistance / 2) * Math.Cos(YawRad));

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return new PointF(x, y);
        }
        #endregion

        #region PositionConditions
        /// <summary>
        /// 測位点の取得判定
        /// </summary>
        /// <param name="p">新予測点(制御点)</param>
        /// <returns></returns>
        private bool PositionConditions(PointF p)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            bool include = false;

            // スルー距離より大きいか
            double r = Math.Sqrt(Math.Pow((p.X - m_controlPoint.X), 2) + Math.Pow((p.Y - m_controlPoint.Y), 2));
            //if (r >= m_config.GetCondition.MinDistance)
            if (r >= m_getCondition.MinDistance)
            {
                // スキップ距離より小さいか
                //if (r <= m_config.GetCondition.MaxDistance)
                if (r <= m_getCondition.MaxDistance)
                {
                    // 設定する角度の範囲内に入っているか
                    // 進行方向の点
                    // 前回角度（ヨー角）に舵角（ピニオン角）/ギア比 を足す
                    double rad = YawRad + (m_vehicleData.steer_angle / m_configData.GearRate);
                    PointF P0 = new PointF((float)(Math.Cos(rad) + m_controlPoint.X), (float)(Math.Sin(rad) + m_controlPoint.Y));
                    // 進行方向の点を設定角度分回す
                    PointF oldP = new PointF(m_controlPoint.X, m_controlPoint.Y);
                    //PointF p1 = GetRollPoint(oldP, P0, ToRadian(m_config.GetCondition.GetRadian / (double)2));
                    PointF p1 = GetRollPoint(oldP, P0, ToRadian(m_getCondition.GetRadian / (double)2));
                    //PointF p2 = GetRollPoint(oldP, P0, -ToRadian(m_config.GetCondition.GetRadian / (double)2));
                    PointF p2 = GetRollPoint(oldP, P0, -ToRadian(m_getCondition.GetRadian / (double)2));
                    // 外積計算
                    double Z1 = Gaiseki(oldP, p1, p);
                    double Z2 = Gaiseki(oldP, p2, p);
                    if (Z1 <= 0 & Z2 >= 0)
                    {
                        include = true;
                    }
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return include;
        }
        #endregion

        #region # 点の回転 #
        // <summary>
        // 回転した角度を求めます
        // </summary>
        // <param name="pCenter">回転の中心座標</param>
        // <param name="pTarget">回転対象の座標</param>
        // <param name="sin">sinθの値</param>
        // <param name="cos">cosθの値</param>
        // <returns>回転後の座標</returns>
        private PointF GetRollPoint(PointF pCenter, PointF pTarget, double rad)
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

        #region # 外積 #
        /// <summary>
        /// 外積を求める(正なら左側、負なら右側)
        /// </summary>
        /// <param name="O">線分始点</param>
        /// <param name="A">線分終点</param>
        /// <param name="B">点</param>
        /// <returns>外積</returns>
        private double Gaiseki(PointF O, PointF A, PointF B)
        {
            return B.X * (O.Y - A.Y) + O.X * (A.Y - B.Y) + A.X * (B.Y - O.Y);
        }
        #endregion

        #region ## ヨーレート更新 ##
        /// <summary>
        /// ヨーレート積分
        /// </summary>
        /// <param name="Yawrate"></param>
        /// <param name="YawrateTime"></param>
        /// <param name="speed"></param>
        /// <param name="dakaku"></param>
        /// <returns></returns>
        private double YawrateUpdate(double Yawrate, int Time, double speed, double dakaku)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {YawRad} , {YawrateTime}");

            // ヨー角(deg)
            double nowTime = (double)Time / (double)1000;
            double Yokaku = ToRadian(Yawrate - YawrateStandart) * (nowTime - YawrateTime);

            lock (YawrateLock)
            {

                //// 進行方向
                double rad = YawRad + Yokaku;
                double oldrad = YawRad;

                if (YawrateTime != 0)
                {
                    YawRad = rad;
                }
                else
                {
                    YawrateStandart = Yawrate;
                    AppLog.GetInstance().Info($"ヨーレート計算：,計算前ヨーレート,補正後ヨーレート,車両ヨー角,ヨーレート0点補正値,前ティックカウント,現ティックカウント,カメラID,信頼度,切り替わり時ベクトル,現画像ベクトル,画像補正回数");
                }

                AppLog.GetInstance().Info($"ヨーレート計算：,{oldrad},{YawRad},{Yawrate},{YawrateStandart},{YawrateTime},{nowTime}");
            }

            YawrateTime = nowTime;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End {YawRad} , { YawrateTime}");
            return YawRad;
        }
        #endregion

        #region # ヨーレート補正 #

        /// <summary>
        /// カメラ測位ベクトルヨーレート補正
        /// </summary>
        /// <param name="camid"></param>
        /// <param name="score"></param>
        /// <param name="camvector">ラジアン</param>
        /// <returns></returns>
        private double YawrateAdjust(string camid, string score, double camvector)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            double scored = double.Parse(score);
            double vector = ToRadian(camvector);
            LOGGER.Debug($"nowcamid:{m_nowcamid},camid:{camid},score:{scored},camscore{m_configData.camscore},nofixcam:{m_configData.nofixcam},score_all_false{m_panaSettingData.constData.score_all_use.ToString()}");
            //カメラ切り替わり時 かつ 信頼度97％以上 かつ無補正カメラでないとき、カメラ測位値で補正を実施開始
            if (m_nowcamid != camid && scored >= m_configData.camscore && !m_configData.nofixcam.Contains(int.Parse(camid).ToString()))
            {
                m_fixcount = 1;
                m_beforeRad = YawRad;
                m_nowcamid = camid;
            }
            //補正回数以下の時補正を実施する
            if (m_fixcount <= m_configData.fixcnt)
            {
                //信頼度が基準を満たしていれば補正(判定を行うかどうかは設定値出変更可能)
                lock (YawrateLock)
                {
                    double oldrad = YawRad;
                    if (!m_panaSettingData.constData.score_all_use || scored >= m_configData.camscore)
                    {
                        //補正
                        YawRad = YawRad - ((m_beforeRad - vector) / m_configData.fixcnt);
                    }

                    AppLog.GetInstance().Info($"ヨーレート計算：,{oldrad},{YawRad},,,,,{camid},{score},{m_beforeRad},{vector},{m_fixcount}");

                    m_fixcount++;
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return YawRad;
        }

        #endregion

        #region #  #

        private AVPAppOutputData CalcAngleplus(PointF[] p, int request_control_status,double speedcoefficient)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_CalcInfo = AskContainArea(p[0], p[1]);
            if (m_CalcInfo.OutOfArea)
            {
                LOGGER.Error($"範囲外です。異常通知します。x:{m_observedData.observed_vehicle_x} y:{m_observedData.observed_vehicle_y}");
                //m_logform.tlog_Insert($"範囲外のため走行停止します。");
                m_avpappOutputData.avp_app_state = 8;
                m_avpappOutputData.avp_error_code = stringToCharCalc(Res.ErrorStatus.I_ERR.Code);
                throw new Exception("範囲外です。");
            }
            m_OrderInfo = CalcAngle(m_CalcInfo, (int)m_controlCenterData.request_control_status, speedcoefficient);

            m_avpappOutputData.target_steering_angle = m_OrderInfo.Angle;
            m_avpappOutputData.target_gx = m_OrderInfo.Acceleration;
            m_avpappOutputData.control_point.x = m_controlPoint.X;
            m_avpappOutputData.control_point.y = m_controlPoint.Y;
            m_avpappOutputData.draw_info.yawrate = YawRad;
            m_avpappOutputData.draw_info.angle_guard = m_OrderInfo.f_maxangle;
            m_avpappOutputData.draw_info.guard_front_angle = m_OrderInfo.Angleori;
            m_avpappOutputData.panaOutputData.log = m_OrderInfo.log;
            m_avpappOutputData.panaOutputData.L = m_OrderInfo.L;

            if (request_control_status == 0)
            {
                m_avpappOutputData.avp_app_state = 5;
                // ADD 20230528
                if (m_goalflg && m_vehicleData.vehicle_speed == 0)
                {
                    m_avpappOutputData.avp_app_state = 7;
                }
                //
            }
            // ADD 20230528
            else if (request_control_status == 2)
            {
                m_avpappOutputData.avp_app_state = 5;
                if (m_goalflg && m_vehicleData.vehicle_speed == 0)
                {
                    m_avpappOutputData.avp_app_state = 7;
                }
            }
            // DEL 20230528
            //else if (request_control_status == 2)
            //{
            //    m_avpappOutputData.avp_app_state = 5;
            //    if (m_vehicleData.vehicle_speed == 0)
            //    {
            //        m_avpappOutputData.avp_app_state = 7;
            //    }
            //}
            //
            else if (request_control_status == 4)
            {
                m_avpappOutputData.avp_app_state = 8;
                m_avpappOutputData.avp_error_code = null;
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");

            return m_avpappOutputData;
        }

        #endregion

        #region　現在地から属するエリアのノードを返す
        /// <summary>
        ///  現在地から属するエリアのノードを返す
        /// </summary>
        /// <param name="point1">予測地点</param>
        /// <param name="point2">現在値(測位点)</param>
        /// <returns></returns>
        private CalcInfo AskContainArea(PointF point1, PointF point2)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            CalcInfo info = new CalcInfo();
            bool contain = false;
            int idx = -1;



            PointF p = point1;

            //エリア内包判定
            if (m_contain_idx == -1)
            {
                //先頭から判定をする
                for (int i = 0; i <= SpeedAreas.Count() - 1; i++)
                {
                    if (PointInPolygon(p, SpeedAreas.ElementAt(i).Value.Area))
                    {
                        contain = true;
                        idx = i;
                        break;
                    }
                }
            }
            else
            {
                int cnt = m_panaSettingData.constData.include_judge_area_count;
                int fromidx = (m_contain_idx <= cnt) ? 0 : m_contain_idx - cnt;
                int toidx = (SpeedAreas.Count() - 1 <= m_contain_idx + cnt) ? SpeedAreas.Count() - 1 : m_contain_idx + cnt;
                //前回内包していたエリアの前後のみ確認する
                for (int i = toidx; i >= fromidx; i--)
                {
                    if (PointInPolygon(p, SpeedAreas.ElementAt(i).Value.Area))
                    {
                        contain = true;
                        idx = i;
                        break;
                    }
                }
            }

            //エリア内であるとき
            if (contain)
            {
                info.StartPoint = new PointF(RouteNodes.ElementAt(idx).Value.X, RouteNodes.ElementAt(idx).Value.Y);
                info.EndPoint = new PointF(RouteNodes.ElementAt(idx + 1).Value.X, RouteNodes.ElementAt(idx + 1).Value.Y);
                SpeedArea speedarea = SpeedAreas.ElementAt(idx).Value;
                info.Speed = speedarea.Speed;
                info.Acceleration = speedarea.Acceleration;
                info.Deceleration = speedarea.Deceleration;

                //FF項計算用ノード格納
                info.FFareaList = new List<PointF>();
                if (m_configData.CalcCoefficient.Kf != 0)
                {
                    for (int i = 0; i <= 2; i++)
                    {
                        if (0 <= idx + m_configData.FFGetStartPosition + i && idx + m_configData.FFGetStartPosition + i <= RouteNodes.Count() - 1)
                        {
                            info.FFareaList.Add(new PointF(RouteNodes.ElementAt(idx + m_configData.FFGetStartPosition + i).Value.X, RouteNodes.ElementAt(idx + m_configData.FFGetStartPosition + i).Value.Y));
                        }
                    }
                }
                if (info.FFareaList.Count != 0)
                {
                    string ffpo = null;
                    foreach (PointF o_o_ff in info.FFareaList)
                    {
                        ffpo += $@"{o_o_ff.X.ToString()},{o_o_ff.Y.ToString()}, ";
                    }
                    LOGGER.Debug($"ffareaList:{ffpo}");
                }

                //測位点取得判定変更
                if (m_contain_idx != idx)
                {
                    //属するエリアが変わったときに取得エリアも変更する
                    LOGGER.Info($"Before:{m_getCondition.GetRadian},{m_getCondition.MinDistance},{m_getCondition.MaxDistance}");
                    m_getCondition.GetRadian = speedarea.GetRadian != "" && speedarea.GetRadian != null ? float.Parse(speedarea.GetRadian) : m_configData.GetCondition.GetRadian;
                    m_getCondition.MinDistance = speedarea.MinDistance != "" && speedarea.MinDistance != null ? float.Parse(speedarea.MinDistance) : m_configData.GetCondition.MinDistance;
                    m_getCondition.MaxDistance = speedarea.MaxDistance != "" && speedarea.MaxDistance != null ? float.Parse(speedarea.MaxDistance) : m_configData.GetCondition.MaxDistance;
                    LOGGER.Info($"After:{m_getCondition.GetRadian},{m_getCondition.MinDistance},{m_getCondition.MaxDistance}");

                    //CreateAngleImg();

                    info.AreaChange = true;
                }
                else
                {
                    info.AreaChange = false;
                }

                m_SetSpeed = speedarea.Speed;
                m_contain_idx = idx;
            }
            //エリア外であるとき
            info.OutOfArea = false;
            if (!contain)
            {
                LOGGER.Info($"現在地が設定エリア外です。制御点-X:{point1.X} Y:{point1.Y} 測位点-X:{point2.X} Y:{point2.Y}");
                info.OutOfArea = true;
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return info;
        }

        #endregion

        #region 点の内包判定
        /// <summary>
        /// 点の内包判定
        /// </summary>
        /// <param name="point">判定する点</param>
        /// <param name="poly">多角形</param>
        /// <returns></returns>
        /// ルール1.上向きの辺は、開始点を含み終点を含まない。
        /// ルール2.下向きの辺は、開始点を含まず終点を含む。
        /// ルール3.水平線Rと辺が水平でない(がRと重ならない)。
        /// ルール4.水平線Rと辺の交点は厳密に点Pの右側になくてはならない。
        private Boolean PointInPolygon(PointF point, PointF[] poly)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            int cnt = 0;
            for (int i = 0; i < poly.Count() - 1; i++)
            {
                // 上向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、終点は含まない。(ルール1)
                if (((poly[i].Y <= point.Y) && (poly[i + 1].Y > point.Y))
                    // 下向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、始点は含まない。(ルール2)
                    || ((poly[i].Y > point.Y) && (poly[i + 1].Y <= point.Y)))
                {
                    // ルール1,ルール2を確認することで、ルール3も確認できている。
                    // 辺は点pよりも右側にある。ただし、重ならない。(ルール4)
                    // 辺が点pと同じ高さになる位置を特定し、その時のxの値と点pのxの値を比較する。
                    float vt = (point.Y - poly[i].Y) / (poly[i + 1].Y - poly[i].Y);
                    if (point.X < (poly[i].X + (vt * (poly[i + 1].X - poly[i].X))))
                    {
                        ++cnt;
                    }
                }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return cnt % 2 == 1;

        }

        #endregion

        #region # 舵角加速度計算 #

        private OrderInfo CalcAngle(CalcInfo calc, int accelerationtype,double speedcoefficient)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            OrderInfo order = new OrderInfo();
            calcLog log = new calcLog();
            PointF p = new PointF();

            //p = genarateP;
            p = m_controlPoint;
            LOGGER.Debug($"controlPoint:{m_controlPoint}");
            double pt = 0;
            double dt = 0;
            double it = 0;

            //2点を通る直線の方程式
            //ax+by+c = 0
            double a = 0;
            double b = 0;
            double c = 0;
            //a = y2-y1
            a = calc.EndPoint.Y - calc.StartPoint.Y;
            //b = -(x2-x1)
            b = -(calc.EndPoint.X - calc.StartPoint.X);
            //c=(x2-x1)y1-(y2-y1)x1
            c = (calc.EndPoint.X - calc.StartPoint.X) * calc.StartPoint.Y - (calc.EndPoint.Y - calc.StartPoint.Y) * calc.StartPoint.X;

            //乖離量Lを求める
            if (m_configData.CalcCoefficient.Kp != 0 || m_configData.CalcCoefficient.Kd != 0 || m_configData.CalcCoefficient.Ki != 0)
            {
                //点と直線の距離を求める公式
                //L = |ax1+by1+c| / √(a*a + b*b)
                order.L = Math.Abs(a * p.X + b * p.Y + c) / Math.Sqrt(a * a + b * b);

                //Lが正か負かを判別(2つの有向線分の外積を求める)
                double Z = Gaiseki(calc.StartPoint, calc.EndPoint, p);
                //Z<0 左にある時のみ正負を反転させる
                if (Z > 0) { order.L = -order.L; }
            }

            //比例項
            if (m_configData.CalcCoefficient.Kp != 0)
            {
                pt = m_configData.CalcCoefficient.Kp * order.L;
                log.p = order.L;
            }
            else
            {
                pt = 0;
                log.p = 0;
            }
            log.p1 = pt;

            //積分項 微分項
            if (m_configData.CalcCoefficient.Kd != 0 || m_configData.CalcCoefficient.Ki != 0)
            {
                //舵角 = a*L + b*(⊿L/⊿t)+c*ΣL
                //⊿t
                TimeSpan dT = new TimeSpan();
                if (m_oldTime == DateTime.MinValue) { dT = DateTime.Now - DateTime.Now; }
                else { dT = DateTime.Now - m_oldTime; }
                order.newTime = DateTime.Now;

                LOGGER.Debug($"dT:{dT.TotalMilliseconds}");

                if (dT.TotalMilliseconds != 0)
                {
                    //微分項
                    if (m_configData.CalcCoefficient.Kd != 0)
                    {
                        //⊿L
                        double dL = 0;
                        dL = order.L - m_oldL;

                        dt = m_configData.CalcCoefficient.Kd * (dL / (dT.TotalMilliseconds * 0.001));
                        log.d = dL / (dT.TotalMilliseconds * 0.001);
                    }
                    else
                    {
                        dt = 0;
                        log.d = 0;
                    }

                    //積分項
                    if (m_configData.CalcCoefficient.Ki != 0)
                    {
                        //ΣL = (L(t)+L(t-1))/2*⊿tの積算
                        order.newsL = m_sL + (((order.L + m_oldL) / 2) * dT.Milliseconds * 0.001);

                        it = m_configData.CalcCoefficient.Ki * order.newsL;
                        log.i = order.newsL;
                    }
                    else
                    {
                        it = 0;
                        log.i = 0;
                    }
                }
                else
                {
                    dt = 0;
                    log.d = 0;
                    it = 0;
                    log.i = 0;
                }
            }
            else
            {
                dt = 0;
                log.d = 0;
                it = 0;
                log.i = 0;
            }
            log.d1 = dt;
            log.i1 = it;

            //FF項
            double ff = 0;
            if (m_configData.CalcCoefficient.Kf != 0)
            {
                ff = ffterm(calc.FFareaList);
            }
            log.f1 = ff;

            //舵角 = a*L + b*((L*⊿t)/⊿t)+c*ΣL
            double angle = 0;
            angle = pt + dt + it + ff;
            order.log = log;

            order.Angleori = angle; //目標舵角

            //LOGGER.Info($"angle ,→{angle}");
            ////舵角指示の変化量を閾値内に収める
            double movelimit = ToRadian(m_configData.LimitVal.AngleMoveLimit / 10);
            //LOGGER.Info($"m_configData.LimitVal.AngleMoveLimit ,→{m_configData.LimitVal.AngleMoveLimit}");
            //LOGGER.Info($"movelimit ,→{movelimit}");
            //LOGGER.Info($"調査:movelimit ,→{movelimit}");
            LOGGER.Debug($"angle変化前:{order.Angleori.ToString()}");
            if (m_panaSettingData.constData.angle_limit_mode == 0)
            {
                //前回指示舵角からの変化量での指示
                //ACT4が落ちていれば実舵角からの変化にする
                double beforeangle = (m_panaInputData.act4 == C_ACT4_UP) ? m_oldAngle : m_vehicleData.steer_angle;

                //LOGGER.Info($"m_vehicleData.steer_angle ,→{m_vehicleData.steer_angle}");
                //LOGGER.Info($"m_oldAngle ,→{m_oldAngle}");
                //LOGGER.Info($"調査：beforeangle ,→{beforeangle}");

                //if (CarInfo.Act4 == C_ACT4_UP)
                //{
                //    if (m_oldAct4 == C_ACT4_DOWN)
                //    {
                //        //Act4が上がったらカウント開始
                //        m_Act4Cnt = 1;
                //    }

                //    if (m_Act4Cnt >= 1 && m_Act4Cnt < m_actualanglecnt)
                //    {
                //        //カウントが1以上、設定時間/100(回)以下の場合、車両実舵角からの変化量で送信し、カウントアップ
                //        beforeangle = CarInfo.angle;
                //        m_Act4Cnt++;
                //    }
                //    else
                //    {
                //        //前回指示舵角からの変化量
                //        beforeangle = m_oldAngle;
                //    }
                //}
                //else
                //{
                //    //落ちたので実舵角からの変化量
                //    beforeangle = CarInfo.angle;
                //    m_Act4Cnt = 0;
                //}
                m_oldAct4 = m_panaInputData.act4;

                LOGGER.Debug($"前angle値:{beforeangle},oldangle:{m_oldAngle},steer_angle:{m_vehicleData.steer_angle}");
                LOGGER.Debug($"限界角:{movelimit}");
                //LOGGER.Info($"調査：angle ,→{angle}");
                //舵角指示の変化量を閾値内に収める
                if (Math.Abs(angle - beforeangle) >= movelimit)
                {
                    if (angle >= beforeangle)
                    {
                        angle = beforeangle + movelimit;
                        order.f_moveangle = false;
                    }
                    else if (angle < beforeangle)
                    {
                        angle = beforeangle - movelimit;
                        order.f_moveangle = false;
                    }
                }
                LOGGER.Debug($"angle:{angle}");
                //LOGGER.Info($"調査：angle_after ,→{angle}");

            }
            else
            {
                LOGGER.Debug($"steer_angle:{m_vehicleData.steer_angle}");
                //車両実舵角からの変化量での指示
                //実舵角からの変化量を閾値内に収める
                if (Math.Abs(angle - m_vehicleData.steer_angle) >= movelimit)
                {
                    if (angle >= m_vehicleData.steer_angle)
                    {
                        angle = m_vehicleData.steer_angle + movelimit;
                        order.f_moveangle = false;
                    }
                    else if (angle < m_vehicleData.steer_angle)
                    {
                        angle = m_vehicleData.steer_angle - movelimit;
                        order.f_moveangle = false;
                    }
                }
                LOGGER.Debug($"angle:{angle}");
                LOGGER.Debug($"m_oldAngle:{m_oldAngle}");

                //前回指示値からの変化量を閾値内に収める
                if (Math.Abs(angle - m_oldAngle) >= movelimit)
                {
                    if (angle >= m_oldAngle)
                    {
                        angle = m_oldAngle + movelimit;
                        order.f_moveangle = false;
                    }
                    else if (angle < m_oldAngle)
                    {
                        angle = m_oldAngle - movelimit;
                        order.f_moveangle = false;
                    }
                }
                LOGGER.Debug($"angle:{angle.ToString()}");
            }

            //舵角指示を舵角最大値内に収める
            double max = ToRadian(m_configData.LimitVal.AngleLimit);
            if (angle > max)
            {
                angle = max;
                order.f_maxangle = true;
            }
            else if (angle < -max)
            {
                angle = -max;
                order.f_maxangle = true;
            }
            else
            {
                order.f_maxangle = false;
            }
            //LOGGER.Info($"調査：angle ,→{angle}");
            LOGGER.Debug($"angle:{angle}");
            order.Angle = angle;

            // ADD 20230528
            // ゴール確認
            if (!m_goalflg)
            {
                foreach (OtherAreaRetention i in m_otherArea)
                {
                    int idx = i.Done;

                    //前方注視点からリア軸中心へ
                    PointF rearsenterpoint = new PointF();
                    rearsenterpoint.X = (float)(m_controlPoint.X - (m_configData.ForwardDistance - m_panaSettingData.carSpec.RearOverhang) * Math.Cos(m_avpappOutputData.draw_info.yawrate));
                    rearsenterpoint.Y = (float)(m_controlPoint.Y - (m_configData.ForwardDistance - m_panaSettingData.carSpec.RearOverhang) * Math.Sin(m_avpappOutputData.draw_info.yawrate));
                    //

                    if (i.Done != -1 && PositionInArea(i.otherarea.area[idx], rearsenterpoint))
                    {
                        //管制端末からの指示とゴール番号が同じであればゴールと判定
                        if (i.otherarea.Number == m_panaInputData.goalno)
                        {
                            LOGGER.Info("ゴールエリアに侵入しました。");
                            //ゴールフラグを立る
                            m_goalflg = true;
                        }
                        i.Done = -1;
                    }
                }
            }

            if (accelerationtype == C_ACCELERATION_NOMAL && m_goalflg)
            {
                accelerationtype = C_ACCELERATION_GOAL;
            }
            //

            //加速度判定
            switch (accelerationtype)
            {
                case C_ACCELERATION_NOMAL:
                    {
                        if (m_panaSettingData.constData.acceleration_mode == 0)
                        {
                            order.Acceleration = CalcAcceleration(calc, speedcoefficient);
                        }
                        else if (m_panaSettingData.constData.acceleration_mode == 1)
                        {
                            order.Acceleration = CalcAccelerationSub(calc);
                        }
                        else if (m_panaSettingData.constData.acceleration_mode == 2)
                        {
                            if (m_vehicleData.vehicle_speed < calc.Speed) { order.Acceleration = calc.Acceleration; }
                            else if (m_vehicleData.vehicle_speed > calc.Speed) { order.Acceleration = -calc.Deceleration; }
                            else { order.Acceleration = 0; }
                        }
                        break;
                    }
                case C_ACCELERATION_PAUSE:
                    {
                        order.Acceleration = -m_configData.Acceleration.Pause;
                        AccelerationInitialize();
                        break;
                    }
                case C_ACCELERATION_GOAL:
                    {
                        order.Acceleration = -m_configData.Acceleration.Goal;
                        break;
                    }
                case C_ACCELERATION_EMERGENCY:
                    {
                        order.Acceleration = -m_configData.Acceleration.Abnormal;
                        break;
                    }
            }

            //ログ用エリア速度
            order.areaspeed = calc.Speed;

            //横偏差基準点
            //直線に垂直である点を通る方程式(dx+ey+f=0)
            //bx-ay-bx0+ay0=0
            double d = 0;
            double e = 0;
            double f = 0;
            d = b;
            e = -a;
            f = -b * p.X + a * p.Y;
            //2直線の交点を求める
            order.xCalcPoint = (float)((b * f - e * c) / (a * e - d * b));
            order.yCalcPoint = (float)((d * c - a * f) / (a * e - d * b));

            //ログ用
            order.xPosi = p.X;
            order.yPosi = p.Y;

            //値保持
            m_oldTime = order.newTime;
            m_oldL = order.L;
            m_sL = order.newsL;
            m_oldAngle = order.Angle;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return order;
        }

        #region 現在測位点のエリアに対する内包判定
        public Boolean PositionInArea(PointF[] area,PointF p)
        {
            bool contain = false;
            //旧前方注視点->リア軸中心？？
            if (p != PointF.Empty)
            {
                PointF point = new PointF(p.X, p.Y);

                contain = CommonProc.PointInPolygon(point, area);
            }

            return contain;
        }
        #endregion

        #endregion

        #region # フィードフォワード項計算 #
        private double ffterm(List<PointF> fs)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //経路取得点不足
            if (fs.Count() == 0)
            {
                AppLog.GetInstance().Info("FF項計算に必要な経路点が不足しています。");
                AppLog.GetInstance().Info($"ff =>, null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,0");
                return 0;
            }
            else if (fs.Count() == 1)
            {
                AppLog.GetInstance().Info("FF項計算に必要な経路点が不足しています。");
                AppLog.GetInstance().Info($"ff =>,{fs[0].X},{fs[0].Y},null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,0");
                return 0;
            }
            else if (fs.Count() == 2)
            {
                AppLog.GetInstance().Info("FF項計算に必要な経路点が不足しています。");
                AppLog.GetInstance().Info($"ff =>,{fs[0].X},{fs[0].Y},{fs[1].X},{fs[1].Y},null,null,null,null,null,null,null,null,null,null,null,null,null,0");
                return 0;
            }

            //中点
            FFpoint[] p = new FFpoint[2];
            for (int i = 0; i <= 1; i++)
            {
                p[i] = new FFpoint();
                p[i].x = (fs[i].X + fs[i + 1].X) / 2;
                p[i].y = (fs[i].Y + fs[i + 1].Y) / 2;
            }

            //法線
            Normal[] n = new Normal[2];
            for (int i = 0; i <= 1; i++)
            {
                n[i] = new Normal();
                if ((fs[i + 1].Y - fs[i].Y) == 0)
                {
                    AppLog.GetInstance().Info($"ff =>,{fs[0].X},{fs[0].Y},{fs[1].X},{fs[1].Y},{fs[2].X},{fs[2].Y},{p[0].x},{p[0].y},{p[1].x},{p[1].y},null,null,null,null,null,null,null,0");
                    return 0;
                }
                n[i].a = -((fs[i + 1].X - fs[i].X) / (fs[i + 1].Y - fs[i].Y));
                n[i].b = p[i].y - n[i].a * p[i].x;
            }

            //法線の交点
            FFpoint xp = new FFpoint();
            if ((n[1].a - n[0].a) == 0)
            {
                AppLog.GetInstance().Info($"ff =>,{fs[0].X},{fs[0].Y},{fs[1].X},{fs[1].Y},{fs[2].X},{fs[2].Y},{p[0].x},{p[0].y},{p[1].x},{p[1].y},{n[0].a},{n[0].b},{n[1].a},{n[1].b},null,null,null,0");
                return 0;
            }
            xp.x = (n[1].b - n[0].b) / (n[0].a - n[1].a);
            xp.y = n[0].a * xp.x + n[0].b;

            double R = Math.Sqrt(Math.Pow(fs[1].X - xp.x, 2) + Math.Pow(fs[1].Y - xp.y, 2)) * Math.Sign(Math.Atan2(fs[2].Y - fs[1].Y, fs[2].X - fs[1].X) - Math.Atan2(fs[1].Y - fs[0].Y, fs[1].X - fs[0].X));

            double ff = (m_configData.Wheelbase / R) * m_configData.GearRate * m_configData.CalcCoefficient.Kf;

            AppLog.GetInstance().Info($"ff =>,{fs[0].X},{fs[0].Y},{fs[1].X},{fs[1].Y},{fs[2].X},{fs[2].Y},{p[0].x},{p[0].y},{p[1].x},{p[1].y},{n[0].a},{n[0].b},{n[1].a},{n[1].b},{xp.x},{xp.y},{R},{ff}");

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return ff;
        }

        private class FFpoint
        {
            public double x { get; set; }
            public double y { get; set; }
        }

        private class Normal
        {
            public double a { get; set; }
            public double b { get; set; }
        }
        #endregion

        #region # 加速度計算 #

        private float CalcAcceleration(CalcInfo calcinfo,double speedcoefficient)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            double calcspeed = calcinfo.Speed * speedcoefficient;

            //エリア速度の変化を確認
            if (m_Current_Area_Speed != calcspeed)
            {
                AppLog.GetInstance().Info("エリア目標速度が変化しました");
                m_Achieve_flag = 0;
                m_Current_Area_Speed = calcspeed;

                //加速減速モード確認
                LOGGER.Debug($"vehicle_speed:{ m_vehicleData.vehicle_speed.ToString()},areaspeed:{calcinfo.Speed.ToString()}");
                if (m_vehicleData.vehicle_speed < calcspeed)
                {
                    if (m_Change_Mode == -1 || m_Change_Mode == 0)
                    {
                        AppLog.GetInstance().Info("加速します");
                        m_Change_Mode = 1;
                        m_Current_Acceleration = 0;
                    }
                }
                else if (m_vehicleData.vehicle_speed > calcspeed)
                {
                    if (m_Change_Mode == 1 || m_Change_Mode == 0)
                    {
                        AppLog.GetInstance().Info("減速します");
                        m_Change_Mode = -1;
                        m_Current_Acceleration = 0;
                    }
                }
            }
            else
            {
                if (m_Achieve_flag == 0)
                {
                    //車速が目標速度に達したかを確認
                    if (m_Change_Mode == 1)
                    {
                        if (m_vehicleData.vehicle_speed >= calcspeed)
                        {
                            AppLog.GetInstance().Info("エリア目標速度に達しました");
                            m_Achieve_flag = 1;
                        }
                    }
                    else if (m_Change_Mode == -1)
                    {
                        if (m_vehicleData.vehicle_speed <= calcspeed)
                        {
                            AppLog.GetInstance().Info("エリア目標速度に達しました");
                            m_Achieve_flag = 1;
                        }
                    }
                }
                else if (m_Achieve_flag == 1)
                {
                    //目標速度に達していれば閾値を確認
                    if (m_Current_Area_Speed + m_configData.Acceleration.Threshold <= m_vehicleData.vehicle_speed)
                    {
                        //目標速度＋閾値より車速が高ければ減速させる
                        AppLog.GetInstance().Info("閾値を上回ったため減速します");
                        m_Achieve_flag = 0;
                        m_Change_Mode = -1;
                    }
                    else if (m_Current_Area_Speed - m_configData.Acceleration.Threshold >= m_vehicleData.vehicle_speed || m_vehicleData.vehicle_speed == 0)
                    {
                        //目標速度-閾値より車速が低ければ加速させる
                        AppLog.GetInstance().Info("閾値を下回ったため加速します");
                        m_Achieve_flag = 0;
                        m_Change_Mode = 1;
                    }
                }
            }

            //加速度計算
            LOGGER.Debug($"現在加速度:{m_Current_Acceleration},加速度変化量:{m_configData.Acceleration.AcceAmount},エリア加速度:{calcinfo.Acceleration}");
            LOGGER.Debug($"減加速度変化量:{m_configData.Acceleration.DeceAmount},エリア減加速度:{calcinfo.Deceleration}");

            if (m_Achieve_flag == 0)
            {
                if (m_Change_Mode == 1)
                {
                    //エリア加速度に達しているか
                    if (m_Current_Acceleration + (m_configData.Acceleration.AcceAmount / 10) <= calcinfo.Acceleration)
                    {
                        m_Current_Acceleration += m_configData.Acceleration.AcceAmount / 10;
                    }
                    else
                    {
                        m_Current_Acceleration = calcinfo.Acceleration;
                    }
                }
                else if (m_Change_Mode == -1)
                {
                    //エリア減速度に達しているか
                    if (m_Current_Acceleration - (m_configData.Acceleration.DeceAmount / 10) >= -calcinfo.Deceleration)
                    {
                        m_Current_Acceleration -= m_configData.Acceleration.DeceAmount / 10;
                    }
                    else
                    {
                        m_Current_Acceleration = -calcinfo.Deceleration;
                    }
                }
            }
            else
            {
                //目標速度に達していれば加速度0を送信
                m_Current_Acceleration = 0;
            }
            LOGGER.Debug($"現在加速度:{m_Current_Acceleration}");

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return m_Current_Acceleration;
        }

        private float CalcAccelerationSub(CalcInfo calcinfo)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            LOGGER.Debug($"vehicle_speed:{ m_vehicleData.vehicle_speed.ToString()},areaspeed:{calcinfo.Speed.ToString()}");
            //加速減速モード確認
            if (m_vehicleData.vehicle_speed < calcinfo.Speed)
            {
                if (m_Change_Mode == -1 || m_Change_Mode == 0)
                {
                    m_Change_Mode = 1;
                    m_Current_Acceleration = 0;
                }

                LOGGER.Debug($"現在加速度:{m_Current_Acceleration},加速度変化量:{m_configData.Acceleration.AcceAmount},エリア加速度:{calcinfo.Acceleration}");
                //エリア加速度に達しているか
                if (m_Current_Acceleration + (m_configData.Acceleration.AcceAmount / 10) <= calcinfo.Acceleration)
                {
                    m_Current_Acceleration += m_configData.Acceleration.AcceAmount / 10;
                }
                else
                {
                    m_Current_Acceleration = calcinfo.Acceleration;
                }
            }
            else if (m_vehicleData.vehicle_speed > calcinfo.Speed)
            {
                if (m_Change_Mode == 1 || m_Change_Mode == 0)
                {
                    m_Change_Mode = -1;
                    m_Current_Acceleration = 0;
                }
                LOGGER.Debug($"現在加速度:{m_Current_Acceleration},減加速度変化量:{m_configData.Acceleration.DeceAmount},エリア減加速度:{calcinfo.Deceleration}");
                //エリア減速度に達しているか
                if (m_Current_Acceleration - (m_configData.Acceleration.DeceAmount / 10) >= -calcinfo.Deceleration)
                {
                    m_Current_Acceleration -= m_configData.Acceleration.DeceAmount / 10;
                }
                else
                {
                    m_Current_Acceleration = -calcinfo.Deceleration;
                }
            }
            else
            {
                m_Current_Acceleration = 0;
            }
            LOGGER.Debug($"現在加速度:{m_Current_Acceleration}");
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            return m_Current_Acceleration;
        }
        #endregion

        #region # 加速度初期化 #
        /// <summary>
        /// 加速度の初期化
        /// </summary>
        private void AccelerationInitialize()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_Current_Acceleration = 0;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        private double ToDegree(double Radian)
        {
            return ((180 / Math.PI) * Radian);
        }

        private char[] stringToCharCalc(string ErrorString)
        {
            char[] charFive = new char[5];
            for (int i = 0; i < 5; i++) { charFive[i] = ErrorString[i]; }
            return charFive;
        }


        #region その他エリア判定用
        private class OtherAreaRetention
        {
            /// <summary>
            /// その他エリア
            /// </summary>
            public OtherArea otherarea { get; set; }
            /// <summary>
            /// 次処理インデックス
            /// </summary>
            public int Done { get; set; }
            /// <summary>
            /// 設備ステータス 0:動作前 1:動作後
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// 一時停止エリア侵入
            /// </summary>
            public bool stop { get; set; }
        }
        #endregion
    }
}