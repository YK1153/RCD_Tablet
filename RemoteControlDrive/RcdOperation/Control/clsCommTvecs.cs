﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AtrptShare;
using CommWrapper;
using RcdCmn;
using RcdOperationSystemConst;

namespace RcdOperation.Control
{
    public partial class ControlMain
    {
        private PanaInputData m_panaInputData = new PanaInputData();
        private VehicleData m_vehicleData = new VehicleData();
        private ManualResetEvent m_evRcvWait;
        /// <summary>
        /// 自走終了通知受信状態 false:未受信
        /// </summary>
        private bool m_cARendrcv = true;
        /// <summary>
        /// 一時停止状態(TVECS制御) 0:正常 1:一時停止
        /// </summary>
        private int m_pause_tvecs_control_state_flg = 0;
        /// <summary>
        /// 自車停車フラグ-TVECS制御中連続受信回数
        /// </summary>
        private int m_Act4Out_cnt = 0;
        /// <summary>
        /// 無線遅延時ベクトル再取得処理開始有無 (true:実施中,false:未実施)
        /// </summary>
        private bool m_vectorreget_start = false;
        /// <summary>
        /// 車両前進タイマー
        /// </summary>
        System.Threading.Timer m_timer_carmove = null;
        #region ### クラス変数 ###
        private CommCl m_tcl;
        private CommSrv m_tsrv;
        /// <summary>
        /// TVECSクライアント側接続状態
        /// </summary>
        private bool TvecsCommClStatus;//m_tcl_flag
        /// <summary>
        /// TVECSサーバー側接続状態
        /// </summary>
        private bool TvecsCommSrvStatus;//m_tSrv_flag
        /// <summary>
        /// 制御開始経過時間
        /// </summary>
        private TimeSpan m_cartimespan;

        /// <summary>
        /// TVECSクライアント側ヘルスチェック用Ping監視
        /// </summary>
        private System.Net.NetworkInformation.Ping m_sender = new System.Net.NetworkInformation.Ping();

        /// <summary>
        /// 車両情報受信間隔タイマー
        /// </summary>
        System.Threading.Timer m_timer_tRcv_pause;

        /// <summary>
        /// 車両情報受信間隔タイマー
        /// </summary>
        System.Threading.Timer m_timer_tRcv_stop;

        /// <summary>
        /// 自走TVECSヘルスチェックタイマー
        /// </summary>
        System.Threading.Timer m_tpin_timer;

        #endregion

        #region ### フラグ ###

        /// <summary>
        /// 車両制御開始受信状況 0:未
        /// </summary>
        private int m_CarStartRes = 0;
        /// <summary>
        /// 車両状態通知受信 0:未
        /// </summary>
        private int m_carstarted = 0;
        /// <summary>
        /// 一時停止状態(通信) 0:正常 1:一時停止
        /// </summary>
        private int m_pause_commT_flg = 0;
        /// <summary>
        /// 車両進行フラグ false:車両前進開始前
        /// </summary>
        private bool m_carmovestart_flg = false;

        #endregion

        public void SetCommTvecs()
        {
            // クライアント側定義
            m_tcl = new CommCl(m_connectionConst.Tvecs_IP,ConnectionConst.Tvecs_Cl_Port);
            m_tcl.Connected += new CommCl.ClientConnectedEventHandler(TvecsClConnected);
            m_tcl.Disconnected += new CommCl.ClientConnectedEventHandler(TvecsClDisConnected);
            m_tcl.RecieveServerMessage += new CommCl.RecieveServerMessageEventHandler(tclReceiveMessage);
            m_tcl.LogWriterRcv = new LogWriteComm("TVECSCl", "Rcv");
            m_tcl.LogWriterSnd = new LogWriteComm("TVECSCl", "Snd");
            m_tcl.ResponseTimeout = C_CAR_WAITTIME;
            // クライアント側接続
            m_tcl.Connect();

            // サーバー側定義
            m_tsrv = new CommSrv(ConnectionConst.Tvecs_Srv_Port);
            m_tsrv.Connected += new CommSrv.ConnectedEventHandler(tSrvConnected);
            m_tsrv.Disconnected += new CommSrv.ConnectedEventHandler(tSrvDisConnected);
            m_tsrv.RecieveMessage += new CommSrv.RecieveMessageEventHandler(tSrvReceiveMessage);
            m_tsrv.LogWriterRcv = new LogWriteComm("TVECSSrv", "Rcv");
            m_tsrv.LogWriterSnd = new LogWriteComm("TVECSSrv", "Snd");
            // サーバー側ポートオープン
            m_tsrv.StartListen();

            m_evRcvWait = new ManualResetEvent(true);
        }

        #region ### 接続・切断 イベント ###

        #region ## クライアント側 ##
        private void TvecsClConnected(ClientConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                TvecsCommClStatus = true;
                LOGGER.Info("自走TVECSと接続しました");

                //TVECSとの通信ヘルスチェックを行う
                m_tpin_timer = new System.Threading.Timer(new TimerCallback(TVECS_HealthCheck), null, Timeout.Infinite, 0);
                m_tpin_timer.Change(0, C_CAR_WAITTIME);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void TvecsClDisConnected(ClientConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                TvecsCommClStatus = false;
                LOGGER.Info("自走TVECSと切断しました");

                //TVECSとの通信ヘルスチェック終了
                if(m_tpin_timer != null)
                {
                    m_tpin_timer.Dispose();
                    m_tpin_timer = null;
                }

                //制御中に切断された場合に走行停止
                if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
                {
                    EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}", Res.ErrorStatus.C_ERR.Code);
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }
        #endregion

        #region ## サーバー側 ##
        private void tSrvConnected(ConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                TvecsCommSrvStatus = true;
                LOGGER.Info("TVECSから接続されました");
                   
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void tSrvDisConnected(ConnectedEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                TvecsCommSrvStatus = false;
                LOGGER.Info("TVECSから切断されました");

                //制御中に切断された場合
                if (m_CarControlStatus == OperationConst.ControlStatusType.C_UNDER_CONTROL)
                {
                    EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}", Res.ErrorStatus.C_ERR.Code);
                }
                else if (m_CarControlStatus ==OperationConst.ControlStatusType.C_WAIT_CONTROL_END)
                {
                    WaitControlEndResetStatus();
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #endregion

        #region ### ヘルスチェック ###
        private void TVECS_HealthCheck(object state)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                //タイマー停止
                if (m_tpin_timer == null) { return; }
                m_tpin_timer.Change(Timeout.Infinite, Timeout.Infinite);

                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        System.Net.NetworkInformation.PingReply reply = m_sender.Send(m_connectionConst.Tvecs_IP);
                        if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        {
                            LOGGER.Debug($"{reply.Address}:{reply.Buffer.Length}:{reply.RoundtripTime}:{reply.Options.Ttl}");
                            i = i + 2;
                        }
                        else
                        {
                            LOGGER.Error($"TVECSとの通信が途絶しました。{reply.Status}");
                            throw new Exception($"TVECSとの通信が途絶しました。{reply.Status}");
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionProcess.ComnExceptionProcess(e);
                        if (i > 0)
                        {
                            m_tcl.Disconnect(true);
                            m_tcl.Connect();
                        }
                        else
                        {
                            LOGGER.Error($"TVECS通信確認リトライ。");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                AppLog.GetInstance().Error(ex.Message);
                AppLog.GetInstance().Error(ex.StackTrace);

                LOGGER.Error("TVECSとの通信が途絶しました。");
                m_tcl.Disconnect(true);
                m_tcl.Connect();
            }
            finally
            {
                if (m_tpin_timer != null) { m_tpin_timer.Change(C_CAR_WAITTIME, C_CAR_WAITTIME); }
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        #endregion

        #region ### 受信 ###

        #region ## クライアント側 ##
        private void tclReceiveMessage(RecieveServerMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.Message}");

                string rcvid = CommMsgBase.GetMsgID(e.Message);

                switch (rcvid)
                {
                    case RcvRsultRcvMsg.msgId:
                        {
                            LOGGER.Info($"受信→[TVECS]応答:{e.Message}");
                            break;
                        }
                    default:
                        {
                            LOGGER.Info($"受信→[TVECS]不正なIDを受信。:{e.Message}");
                            //SndResponceT();

                            break;
                        }
                }

            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionProcess(ex); }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region ## サーバー側 ##
        private void tSrvReceiveMessage(RecieveMessageEventArgs e)
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start {e.Message}");

                string rcvid = CommMsgBase.GetMsgID(e.Message);

                switch (rcvid)
                {
                    case RcvDartResult.ID:
                        {
                            LOGGER.Info($"受信→[TVECS]DART起動結果:{e.Message}");

                            RcvDartResult rcvdart = new RcvDartResult(e.Message);
                            SndResponceT(e.ConnectID, rcvdart.SerialNo);

                            m_evRcvWait.Set();
                            break;
                        }
                    case RcvReadyOK.ID:
                        {
                            LOGGER.Info($"受信→[TVECS]準備完了通知:{e.Message}");

                            RcvReadyOK rcvready = new RcvReadyOK(e.Message);
                            SndResponceT(e.ConnectID, rcvready.SerialNo);

                            m_evRcvWait.Set();
                            break;
                        }
                    case RcvtStatusMsg.ID:
                        {
                            LOGGER.Info($"受信→[TVECS]車両状態通知:{e.Message}");

                            RcvtStatusMsgProc(e);

                            break;
                        }
                    case RcvtEmergency.ID:
                        {
                            LOGGER.Info($"受信→[TVECS]非常停止通知:{e.Message}");

                            RcvtEmergency rcvtmsg = new RcvtEmergency(e.Message);
                            SndResponceT(e.ConnectID, rcvtmsg.SerialNo);

                            LOGGER.Info($"[受信内容]ボデーNo：{rcvtmsg.CarNo}");

                            CarEmergencyEnd($"{rcvtmsg.Errcd}");
                            break;
                        }
                    case RcvSelfRunEnd.ID:
                        {
                            LOGGER.Info($"受信→[TVECS]自走終了通知:{e.Message}");

                            RcvSelfRunEnd rcvSelfmsg = new RcvSelfRunEnd(e.Message);
                            SndResponceT(e.ConnectID, rcvSelfmsg.SerialNo);

                            LOGGER.Info($"[受信内容]ボデーNo：{rcvSelfmsg.CarNo}");

                            ControlEndProc();

                            break;
                        }
                    case RcvSelfRunEnd_old.ID:
                        {
                            LOGGER.Info($"受信→[TVECS]自走終了通知:{e.Message}");

                            RcvSelfRunEnd_old rcvSelfmsg = new RcvSelfRunEnd_old(e.Message);
                            SndResponceT(e.ConnectID, rcvSelfmsg.SerialNo);

                            LOGGER.Info($"[受信内容]ボデーNo：{rcvSelfmsg.CarNo}");

                            ControlEndProc();

                            break;
                        }
                    default:
                        {
                            LOGGER.Info($"受信→[TVECS]不正なIDを受信。:{e.Message}");
                            //SndResponceT();

                            break;
                        }
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionProcess(ue); }
            catch (Exception ex)
            {
                AppLog.GetInstance().Error(ex.Message);
                AppLog.GetInstance().Error(ex.StackTrace);
            }
            finally { AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        /// <summary>
        /// 車両状態通知受信時処理
        /// </summary>
        /// <param name="e"></param>
        public void RcvtStatusMsgProc(RecieveMessageEventArgs e)
        {
            if (C_SURVEILLANCE_CAR)
            {
                if (m_timer_tRcv_pause != null) { m_timer_tRcv_pause.Change(C_SURVEILLANCE_INTERVALP_CAR, Timeout.Infinite); }
                if (m_timer_tRcv_stop != null) { m_timer_tRcv_stop.Change(Timeout.Infinite, Timeout.Infinite); }

                m_pause_commT_flg = 0;
            }


            RcvtStatusMsg rcvMsgData = new RcvtStatusMsg(e.Message);

            LOGGER.Info($"[受信内容]ボデーNo：{rcvMsgData.CarNo},車速：{rcvMsgData.Speed},舵角：{rcvMsgData.Dakaku},ヨーレート取得時間：{rcvMsgData.YawrateGetTime},ヨーレート値：{rcvMsgData.Yawrate},Act4：{rcvMsgData.Act4},gx：{rcvMsgData.gx},gy：{rcvMsgData.gy},vehicle_data_timestamp：{rcvMsgData.vehicle_data_timestamp},");

            m_CarInfo.speed = float.Parse(rcvMsgData.Speed);
            m_CarInfo.angle = float.Parse(rcvMsgData.Dakaku);
            m_CarInfo.Yawrate = double.Parse(rcvMsgData.Yawrate);
            m_CarInfo.YawrateTime = int.Parse(rcvMsgData.YawrateGetTime);
            if (!String.IsNullOrWhiteSpace(rcvMsgData.is_vehicle_stop))
            {
                m_vehicleData.is_vehicle_stop = rcvMsgData.is_vehicle_stop == "1" ? true : false;
            }

            #region # 自車制御フラグ判定 #
            if (rcvMsgData.Act4 == "0" && m_pause_tvecs_control_state_flg ==　OperationConst.C_NORMAL)
            {
                //自車制御フラグが落ちたため、一時停止開始
                m_pause_tvecs_control_state_flg = OperationConst.C_ABNORMAL;
                m_CarInfo.Act4 = 0;
                LOGGER.Info("自走TVECS制御中のため一時停止開始");

                m_Act4Out_cnt++;
            }
            else if (rcvMsgData.Act4 == "0" && m_pause_tvecs_control_state_flg == OperationConst.C_ABNORMAL)
            {
                //自車制御フラグ落ち中
                m_CarInfo.Act4 = 0;
                m_Act4Out_cnt++;
            }
            else if (rcvMsgData.Act4 == "1" && m_pause_tvecs_control_state_flg == OperationConst.C_ABNORMAL)
            {
                //自車制御フラグ復帰
                if (!m_vectorreget_start)
                {
                    LOGGER.Info($"{rcvMsgData.is_vehicle_stop},{m_CarInfo.XSTOP}");

                    // 無線遅延後再取得なし
                    ////ベクトル取得処理未開始のためXSTOP₌1であればベクトル取得処理開始
                    //if (m_CarInfo.XSTOP == 1)
                    //{
                    //    if (m_GoalCheckSnd)
                    //    {
                    //        if (m_Act4Out_cnt >= C_VCONTROLFLG_THROUGH_NUM)
                    //        {
                    //            //車両完全停止
                    //            //ベクトル取得処理実施
                    //            m_vectorreget_start = true;
                    //            Task.Run(() => ExeReGetVector(MainC));
                    //            m_CarInfo.Act4 = 0;
                    //        }
                    //        else
                    //        {
                    //            LOGGER.Info("自車制御フラグ:0が規定値以下のため走行再開");
                    //            m_Act4Out_cnt = 0;
                    //            m_pause_tvecs_control_state_flg = OperationConst.C_NORMAL;
                    //            m_CarInfo.Act4 = 1;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        LOGGER.Info("ゴール位置停車済みのため走行再開");
                    //        m_Act4Out_cnt = 0;
                    //        m_pause_tvecs_control_state_flg = OperationConst.C_NORMAL;
                    //        m_CarInfo.Act4 = 1;
                    //    }
                    //}
                    //else
                    //{
                    //    //車両未停止
                    //    //一時停止継続
                    //    m_CarInfo.Act4 = 0;
                    //}

                    LOGGER.Info("自車制御フラグ:0に復帰したため走行再開");
                    m_Act4Out_cnt = 0;
                    m_pause_tvecs_control_state_flg = OperationConst.C_NORMAL;
                    m_CarInfo.Act4 = 1;
                }
                else
                {
                    m_CarInfo.Act4 = 0;
                }
            }
            else if (rcvMsgData.Act4 == "1" && m_pause_tvecs_control_state_flg == OperationConst.C_NORMAL)
            {
                //通常
                m_CarInfo.Act4 = 1;
            }
            #endregion

            m_panaInputData.tickcount = m_CarInfo.YawrateTime;
            if (m_CarInfo.Act4 == 0)
            {
                m_panaInputData.act4 = true;
            }
            else if (m_CarInfo.Act4 == 1)
            {
                m_panaInputData.act4 = false;
            }
            m_vehicleData.vehicle_speed = double.Parse(rcvMsgData.Speed);
            m_vehicleData.steer_angle = double.Parse(rcvMsgData.Dakaku);
            m_panaInputData.yr = m_CarInfo.Yawrate;
            m_vehicleData.yr = m_CarInfo.Yawrate;
            if (!String.IsNullOrWhiteSpace(rcvMsgData.vehicle_data_timestamp))
            {
                DateTime now = DateTime.Now;
                DateTime cardatespan = new DateTime(now.Year, now.Month, now.Day, int.Parse(rcvMsgData.vehicle_data_timestamp.Substring(0, 2)), int.Parse(rcvMsgData.vehicle_data_timestamp.Substring(2, 2)), int.Parse(rcvMsgData.vehicle_data_timestamp.Substring(4, 2)), int.Parse(rcvMsgData.vehicle_data_timestamp.Substring(6, 3)));
                m_cartimespan = cardatespan - m_now;
                m_vehicleData.vehicle_data_timestamp = (uint)Math.Floor(m_cartimespan.TotalMilliseconds);
                LOGGER.Info($"[差分取得]cartimespan：{cardatespan},");
            }
            LOGGER.Info($"[差分取得]vehicle_data_timestamp：{m_vehicleData.vehicle_data_timestamp},");

            #region 値代入
            if (!String.IsNullOrWhiteSpace(rcvMsgData.yawrate_sensor_1_invalid))
            {
                if (rcvMsgData.yawrate_sensor_1_invalid.Equals("1"))
                { m_vehicleData.yawrate_sensor_1_invalid = true; }
                else
                { m_vehicleData.yawrate_sensor_1_invalid = false; }
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.yawrate_sensor_2_invalid))
            {
                if (rcvMsgData.yawrate_sensor_2_invalid.Equals("1"))
                { m_vehicleData.yawrate_sensor_2_invalid = true; }
                else
                { m_vehicleData.yawrate_sensor_2_invalid = false; }
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.yaw_g_sensor_power_invalid))
            {
                if (rcvMsgData.yaw_g_sensor_power_invalid.Equals("1"))
                { m_vehicleData.yaw_g_sensor_power_invalid = true; }
                else
                { m_vehicleData.yaw_g_sensor_power_invalid = false; }
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.g_sensor_1_invalid))
            {
                if (rcvMsgData.g_sensor_1_invalid.Equals("1"))
                { m_vehicleData.g_sensor_1_invalid = true; }
                else
                { m_vehicleData.g_sensor_1_invalid = false; }
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.yaw_g_sensor_type_identification))
            {
                m_vehicleData.yaw_g_sensor_type_identification = uint.Parse(rcvMsgData.yaw_g_sensor_type_identification);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.yr1))
            {
                m_vehicleData.yaw_senser_1_value = double.Parse(rcvMsgData.yr1);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.yr2))
            {
                m_vehicleData.yaw_senser_2_value = double.Parse(rcvMsgData.yr2);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.gx))
            {
                m_vehicleData.gx = double.Parse(rcvMsgData.gx);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.gy))
            {
                m_vehicleData.gy = double.Parse(rcvMsgData.gy);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.steer_torque))
            {
                m_vehicleData.steer_torque = double.Parse(rcvMsgData.steer_torque);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.is_steer_torque_invalid))
            {
                if (rcvMsgData.is_steer_torque_invalid.Equals("1"))
                { m_vehicleData.is_steer_torque_invalid = true; }
                else
                { m_vehicleData.is_steer_torque_invalid = false; }
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.steer_torque_enhaned))
            {
                m_vehicleData.steer_torque_enhaned = double.Parse(rcvMsgData.steer_torque_enhaned);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.eps_motor_torque))
            {
                m_vehicleData.eps_motor_torque = double.Parse(rcvMsgData.eps_motor_torque);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.is_vehicle_speed_from_vsc_invalid))
            {
                if (rcvMsgData.is_vehicle_speed_from_vsc_invalid.Equals("1"))
                { m_vehicleData.is_vehicle_speed_from_vsc_invalid = true; }
                else
                { m_vehicleData.is_vehicle_speed_from_vsc_invalid = false; }
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.sp1_pulse))
            {
                m_vehicleData.sp1_pulse = uint.Parse(rcvMsgData.sp1_pulse);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.vehicle_speed_from_vsc))
            {
                m_vehicleData.vehicle_speed_from_vsc = double.Parse(rcvMsgData.vehicle_speed_from_vsc);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.request_gx_from_b_pedal))
            {
                m_vehicleData.request_gx_from_b_pedal = double.Parse(rcvMsgData.request_gx_from_b_pedal);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.request_gx_from_a_pedal))
            {
                m_vehicleData.request_gx_from_a_pedal = double.Parse(rcvMsgData.request_gx_from_a_pedal);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.estimated_gx_form_vsc))
            {
                m_vehicleData.estimated_gx_form_vsc = double.Parse(rcvMsgData.estimated_gx_form_vsc);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.gvc_invalid))
            {
                if (rcvMsgData.gvc_invalid.Equals("1"))
                { m_vehicleData.gvc_invalid = true; }
                else
                { m_vehicleData.gvc_invalid = false; }
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.epb_lock_state))
            {
                m_vehicleData.epb_lock_state = uint.Parse(rcvMsgData.epb_lock_state);
            }
            if (!String.IsNullOrWhiteSpace(rcvMsgData.shift_range_from_vmc))
            {
                m_vehicleData.shift_range_from_vmc = uint.Parse(rcvMsgData.shift_range_from_vmc);
            }
            #endregion

            //車両前進チェック
            if (m_timer_carmove != null)
            {
                if (m_CarInfo.XSTOP == 0)
                {
                    LOGGER.Info("車両前進OK");
                    m_timer_carmove.Dispose();
                    m_timer_carmove = null;
                }
            }

            m_carstarted = OperationConst.C_RECEIVED;
            if (!m_carmovestart_flg && m_CarInfo.speed > 0) { m_carmovestart_flg = true; }
            
        }

        #endregion

        /// <summary>
        /// TVECSへの応答
        /// </summary>
        /// <param name="id">接続id</param>
        /// <param name="serialno">受信シリアルナンバー</param>
        private void SndResponceT(int id, int serialno)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            RcvResultSendMsg msg = RcvResultSendMsg.GetOK(serialno);
            m_tsrv.SendMessage(id, msg);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        #endregion

        #region ### 送信 ###

        /// <summary>
        /// TVECSへの応答
        /// </summary>
        /// <param name="id"></param>
        private void SndResponceT(int serialno)
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            RcvResultSendMsg msg = RcvResultSendMsg.GetOK(serialno);
            m_tcl.SendMessage(msg, C_SNDRSESULT_CAR);

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 生産指示送信
        /// </summary>
        private string CarProductionInstructions()
        {
            try
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

                // 生産指示送信
                SndProductionInstructions msgpi = new SndProductionInstructions()
                {
                    CarNo = PreVehicle.Instance.Vehicle.BodyNo,
                    CarSpecNo = PreVehicle.Instance.Vehicle.CarSpecNo,
                    IP = PreVehicle.Instance.Vehicle.IpAddr,
                    PORT = int.Parse(PreVehicle.Instance.Vehicle.Port),
                    logSaveStatus = uint.Parse(m_controlSetting.LogSaveStatus.ToString())
                };  
                m_evRcvWait.Reset();
                //string rcvmsg = m_tcl.SendMessage(msgpi, true);
                LOGGER.Info($"[TVECS]送信開始→生産指示");
                m_tcl.SendMessage(msgpi, false);

                // DART起動結果受信　※タイマー起動して、受信イベントで受ける
                if (m_evRcvWait.WaitOne(OperationConst.C_SYSTEM_MAX_TIMEOUT))
                {
                    //受信

                    // 準備完了待ちのためリセット
                    m_evRcvWait.Reset();
                }
                else
                {
                    // Timeout
                    LOGGER.Error($"[TVECS]DART起動受信タイムアウト");
                    return Res.ErrorStatus.TVECS_DART_RES_ERR.Code;
                }

                // 表示状態更新

                // 準備完了通知受信　※タイマー起動して、受信イベントで受ける
                if (m_evRcvWait.WaitOne(OperationConst.C_SYSTEM_MAX_TIMEOUT))
                {
                    //受信
                }
                else
                {
                    // Timeout
                    LOGGER.Error("準備完了通知受信タイムアウト");
                    return Res.ErrorStatus.TVECS_READY_RES_ERR.Code;
                }


                return Res.ErrorStatus.NORMAL.Code;
            }
            catch (Exception ex)
            {
                ExceptionProcess.ComnExceptionProcess(ex);
                return Res.ErrorStatus.UNKNOWN_ERR.Code;
            }
            finally
            {
                AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        /// <summary>
        /// 制御開始指示
        /// </summary>
        private void CarControlStart()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            SndCarControlStart carcsmsg = new SndCarControlStart
            {
                CarNo = m_CarNo,
            };

            string strResponcemsg = "";

            LOGGER.Info($"[TVECS]送信開始→制御開始指示");
            for (int count = 0; count <= C_CARSTOP_SENDCOUNT; count++)
            {
                try
                {
                    LOGGER.Info($"[TVECS]送信→制御開始指示");
                    LOGGER.Info($"[送信内容]ボデーNo：{carcsmsg.CarNo}");//,車両仕様No：{carcsmsg.CarSpecNo},IPアドレス：{carcsmsg.IP},PORT番号：{carcsmsg.PORT}");

                    strResponcemsg = m_tcl.SendMessage(carcsmsg, C_SNDRSESULT_CAR);

                    break;
                }
                catch (Exception ex)
                {
                    if (count == C_CARSTOP_SENDCOUNT)
                    {
                        ControlStartStatus(Res.ErrorStatus.D_ERR.Code);
                        throw new UserException("[TVECS]制御開始指示送信失敗");
                    }

                    LOGGER.Error($"[TVECS]制御開始指示送信失敗。再送信します。");

                    LOGGER.Error(ex.Message);
                    Thread.Sleep(C_CARSTOP_SENDDEREY);
                }
            }

            if (C_SNDRSESULT_CAR)
            {
                RcvRsultRcvMsg rcvmsg = new RcvRsultRcvMsg(strResponcemsg);
                //受信応答不可か確認
                if (rcvmsg.result != "00000")
                {
                    LOGGER.Error($"[TVECS]制御開始異常応答受信");

                    ControlStartStatus(rcvmsg.result);
                    throw new UserException("[TVECS]制御開始異常応答受信");
                }
            }

            //受信完了
            LOGGER.Info($"[TVECS]制御開始正常応答受信");
            m_CarStartRes = OperationConst.C_RECEIVED;

            if (C_SURVEILLANCE_CAR)
            {
                if (m_timer_tRcv_pause == null) { m_timer_tRcv_pause = new System.Threading.Timer(new TimerCallback(ReceiveExcessT_Pause), null, Timeout.Infinite, 0); }
                if (m_timer_tRcv_stop == null) { m_timer_tRcv_stop = new System.Threading.Timer(new TimerCallback(ReceiveExcessT_Stop), null, Timeout.Infinite, Timeout.Infinite); }

                if (m_timer_tRcv_pause != null) { m_timer_tRcv_pause.Change(Timeout.Infinite, Timeout.Infinite); }
                if (m_timer_tRcv_stop != null) { m_timer_tRcv_stop.Change(C_SURVEILLANCE_INTERVALFS_CAR, Timeout.Infinite); }
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 制御終了指示送信
        /// </summary>
        private void CarControlEnd()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            SndControlEnd cemsg = new SndControlEnd();
            cemsg.CarNo = m_CarNo;

            try
            {
                LOGGER.Info($"[TVECS]送信開始→制御終了指示");
                string strResponcemsg = m_tcl.SendMessage(cemsg, C_SNDRSESULT_CAR);
                LOGGER.Info($"[TVECS]送信→制御終了指示");
                LOGGER.Info($"[送信内容]ボデーNo：{cemsg.CarNo}");
            }
            catch (Exception ex)
            {
                LOGGER.Error($"[TVECS]制御終了指示送信失敗");
                
                LOGGER.Error(ex.Message);
                throw new UserException("[TVECS]制御終了指示送信失敗");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 走行停止指示
        /// </summary>
        private void EmergencyStop()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            SndEmergencyStop esmsg = new SndEmergencyStop();
            esmsg.CarNo = m_CarNo;
            
            for (int count = 0; count <= C_CARSTOP_SENDCOUNT; count++)
            {
                try
                {
                    string strResponcemsg = m_tcl.SendMessage(esmsg, C_SNDRSESULT_CAR);
                    LOGGER.Info($"[TVECS]送信→走行停止指示");
                    LOGGER.Info($"[送信内容]ボデーNo：{esmsg.CarNo}");

                    if (C_SNDRSESULT_CAR && (strResponcemsg == null || (new RcvRsultRcvMsg(strResponcemsg)).result != "00000"))
                    {
                        throw new Exception();
                    }

                    break;
                }
                catch
                {

                    Thread.Sleep(C_CARSTOP_SENDDEREY);
                    LOGGER.Info($"車両走行停止応答不正。再送信します。");
                }
                
            }
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region ### 受信間隔超過 ###
        private void ReceiveExcessT_Pause(object state)
        {
            if (m_error_flg == OperationConst.C_NORMAL)
            {
                LOGGER.Info($"[TVECS]受信時間を超過しました。一時停止します。");
                
                m_pause_commT_flg = 1;
                m_timer_tRcv_stop.Change(C_SURVEILLANCE_INTERVALS_CAR, Timeout.Infinite);
            }
        }

        private void ReceiveExcessT_Stop(object state)
        {
            LOGGER.Info($"[TVECS]受信時間を超過しました。走行停止します。");

            EmergencyEndProcess($"{MethodBase.GetCurrentMethod().Name}", Res.ErrorStatus.H_ERR.Code);
        }
        #endregion

        #region ### 初期化 ###
        public void TvecsResetVal()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            m_CarStartRes = OperationConst.C_NOT_RECEIVED;
            m_carstarted = OperationConst.C_NOT_RECEIVED;
            m_pause_commT_flg = OperationConst.C_ABNORMAL;
            m_carmovestart_flg = false;

            m_pause_tvecs_control_state_flg = 0;
            m_vectorreget_start = false;
            m_Act4Out_cnt = 0;
            m_carmovestart_flg = false;

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }
        #endregion

        #region ### 電文定義 ###

        #region ## 受信 ##

        /// <summary>
        /// DART起動結果
        /// </summary>
        public class RcvDartResult : RcvMsgBase
        {
            public const string ID = "10";
            /// <summary>
            /// ボデーNo
            /// </summary>
            [Order(3), Length(5)]
            public string CarNo { get; set; }
            public RcvDartResult(string rcv_msg) : base(rcv_msg) { }
        }

        /// <summary>
        /// 準備完了通知
        /// </summary>
        public class RcvReadyOK : RcvMsgBase
        {
            public const string ID = "11";
            /// <summary>
            /// ボデーNo
            /// </summary>
            [Order(3), Length(5)]
            public string CarNo { get; set; }
            public RcvReadyOK(string rcv_msg) : base(rcv_msg) { }
        }

        /// <summary>
        ///車両状態通知(From TVECS)
        ///</summary>
        public class RcvtStatusMsg : RcvMsgBase
        {
            public const string ID = "40";

            /// <summary>
            /// ボデーNo
            /// </summary>
            [Order(3), Length(5)]
            public string CarNo { get; set; }
            /// <summary>
            /// 車速
            /// </summary>
            [Order(4), Length(6)]
            public string Speed { get; set; }
            /// <summary>
            /// 舵角
            /// </summary>
            [Order(5), Length(7)]
            public string Dakaku { get; set; }
            /// <summary>
            /// ヨーレート取得時間
            /// </summary>
            [Order(6), Length(10)]
            public string YawrateGetTime { get; set; }
            /// <summary>
            /// ヨーレート値
            /// </summary>
            [Order(7), Length(8)]
            public string Yawrate { get; set; }
            /// <summary>
            /// Act4動作状態 1:作動中(正常時) 0:作動外
            /// </summary>
            [Order(8), Length(1)]
            public string Act4 { get; set; }
            /// <summary>
            /// データ受信時間
            /// </summary>
            [Order(9), Length(9)]
            public string vehicle_data_timestamp { get; set; }
            /// <summary>
            /// ヨーレイトセンサ1無効/有効
            /// </summary>
            [Order(10), Length(1)]
            public string yawrate_sensor_1_invalid { get; set; }
            /// <summary>
            /// ヨーレイトセンサ2無効/有効
            /// </summary>
            [Order(11), Length(1)]
            public string yawrate_sensor_2_invalid { get; set; }
            /// <summary>
            /// ｾﾝｻ電源電圧(1G)無効/有効
            /// </summary>
            [Order(12), Length(1)]
            public string yaw_g_sensor_power_invalid { get; set; }
            /// <summary>
            /// GL1,GX 無効/有効
            /// </summary>
            [Order(13), Length(1)]
            public string g_sensor_1_invalid { get; set; }
            /// <summary>
            /// GL2,GX 無効/有効
            /// </summary>
            [Order(14), Length(1)]
            public string g_sensor_2_invalid { get; set; }
            /// <summary>
            /// センサ識別
            /// </summary>
            [Order(15), Length(2)]
            public string yaw_g_sensor_type_identification { get; set; }
            /// <summary>
            /// ﾖｰﾚｲﾄｾﾝｻ1 高分解能信号
            /// </summary>
            [Order(16), Length(7)]
            public string yr1 { get; set; }
            /// <summary>
            /// ﾖｰﾚｲﾄｾﾝｻ2 高分解能信号
            /// </summary>
            [Order(17), Length(7)]
            public string yr2 { get; set; }
            /// <summary>
            /// GL1,GXセンサ信号
            /// </summary>
            [Order(18), Length(6)]
            public string gx { get; set; }
            /// <summary>
            /// GL2,GYセンサ信号
            /// </summary> 
            [Order(19), Length(6)]
            public string gy { get; set; }
            /// <summary>
            /// EPS ﾄﾙｸｾﾝｻ値
            /// </summary> 
            [Order(20), Length(5)]
            public string steer_torque { get; set; }
            /// <summary>
            /// EPS ﾄﾙｸｾﾝｻ値無効
            /// </summary> 
            [Order(21), Length(1)]
            public string is_steer_torque_invalid { get; set; }
            /// <summary>
            /// EPS ﾄﾙｸｾﾝｻ値LSB 拡張信号
            /// </summary> 
            [Order(22), Length(5)]
            public string steer_torque_enhaned { get; set; }
            /// <summary>
            /// EPSトルク
            /// </summary>
            [Order(23), Length(7)]
            public string eps_motor_torque { get; set; }
            /// <summary>
            /// 推定車体速無効
            /// </summary>
            [Order(24), Length(1)]
            public string is_vehicle_speed_from_vsc_invalid { get; set; }
            /// <summary>
            /// 車速パルス信号積算値
            /// </summary>
            [Order(25), Length(2)]
            public string sp1_pulse { get; set; }
            /// <summary>
            /// 推定車体速
            /// </summary> 
            [Order(26), Length(6)]
            public string vehicle_speed_from_vsc { get; set; }
            /// <summary>
            /// 自車停止フラグ
            /// </summary> 
            [Order(27), Length(1)]
            public string is_vehicle_stop { get; set; }
            /// <summary>
            /// ﾌﾞﾚｰｷﾍﾟﾀﾞﾙﾄﾞﾗｲﾊﾞｰ要求加速度
            /// </summary> 
            [Order(28), Length(6)]
            public string request_gx_from_b_pedal { get; set; }
            /// <summary>
            /// ｱｸｾﾙﾍﾟﾀﾞﾙ要求加速度
            /// </summary> 
            [Order(29), Length(7)]
            public string request_gx_from_a_pedal { get; set; }
            /// <summary>
            /// 推定車体加速度
            /// </summary>
            [Order(30), Length(7)]
            public string estimated_gx_form_vsc { get; set; }
            /// <summary>
            /// GVC無効フラグ
            /// </summary>
            [Order(31), Length(1)]
            public string gvc_invalid { get; set; }
            /// <summary>
            /// EPBﾛｯｸ状態
            /// </summary> 
            [Order(32), Length(1)]
            public string epb_lock_state { get; set; }
            /// <summary>
            /// VMC認識シフトレンジ
            /// </summary> 
            [Order(33), Length(1)]
            public string shift_range_from_vmc { get; set; }
            /// <summary>
            /// 端末ガイド制御状態
            /// </summary> 
            [Order(34), Length(3)]
            public string guide_control_status { get; set; }
            /// <summary>
            /// 最終指令舵角
            /// </summary> 
            [Order(35), Length(7)]
            public string final_command_angle { get; set; }
            /// <summary>
            /// 最終指令加速度
            /// </summary> 
            [Order(36), Length(7)]
            public string final_command_acceleration { get; set; }
            /// <summary>
            /// ガイド制御加算目標舵角
            /// </summary> 
            [Order(37), Length(7)]
            public string guide_control_add_angle { get; set; }
            /// <summary>
            /// ガイド制御加算目標加速度
            /// </summary> 
            [Order(38), Length(7)]
            public string guide_control_add_acceleration { get; set; }
            /// <summary>
            /// 黄色端末エラーコード
            /// </summary> 
            [Order(39), Length(5)]
            public string error_code { get; set; }


            public RcvtStatusMsg(string rcv_msg) : base(rcv_msg) { }
        }

        /// <summary>
        /// 非常停止通知
        /// </summary>
        public class RcvtEmergency : RcvMsgBase
        {
            public const string ID = "41";
            /// <summary>
            /// ボデーNo
            /// </summary>
            [Order(3), Length(5)]
            public string CarNo { get; set; }
            /// <summary>
            /// 異常コード
            /// </summary>
            [Order(4), Length(5)]
            public string Errcd { get; set; }
            public RcvtEmergency(string rcv_msg) : base(rcv_msg) { }
        }

        /// <summary>
        /// 自走終了
        /// </summary>
        public class RcvSelfRunEnd_old : RcvMsgBase
        {
            public const string ID = "13";
            /// <summary>
            /// ボデーNo
            /// </summary>
            [Order(3), Length(5)]
            public string CarNo { get; set; }
            public RcvSelfRunEnd_old(string rcv_msg) : base(rcv_msg) { }
        }

        /// <summary>
        /// 自走終了
        /// </summary>
        public class RcvSelfRunEnd : RcvMsgBase
        {
            public const string ID = "42";
            /// <summary>
            /// ボデーNo
            /// </summary>
            [Order(3), Length(5)]
            public string CarNo { get; set; }
            public RcvSelfRunEnd(string rcv_msg) : base(rcv_msg) { }
        }
        #endregion

        #region ## 送信 ##
        /// <summary>
        /// 生産指示
        /// </summary>
        public class SndProductionInstructions : CommSndMsgBase
        {
            public const string ID = "20";
            /// <summary>
            /// ボデーNo
            /// </summary>
            public string CarNo { get; set; }
            /// <summary>
            /// 車両仕様No
            /// </summary>
            public string CarSpecNo { get; set; }
            /// <summary>
            /// 端末IP
            /// </summary>
            public string IP { get; set; }
            /// <summary>
            /// 端末PORT
            /// </summary>
            public int PORT { get; set; }
            /// <summary>
            /// 黄色端末ログ保存ステータス
            /// </summary>
            public uint logSaveStatus { get; set; }

            public SndProductionInstructions() : base(ID) { }
            protected override string GetBodyString()
            {
                return CarNo.PadRight(5) + CarSpecNo.PadRight(5) + IP.PadRight(15) + PORT.ToString("D5") + logSaveStatus.ToString();
            }
        }

        /// <summary>
        /// 制御開始指示(車両開始指示)
        /// </summary>
        public class SndCarControlStart : CommSndMsgBase
        {
            public const string ID = "30";
            /// <summary>
            /// ボデーNo
            /// </summary>
            public string CarNo { get; set; }


            public SndCarControlStart() : base(ID) { }
            protected override string GetBodyString()
            {
                return CarNo.PadRight(5);
            }
        }

        /// <summary>
        /// 車両制御指示
        /// </summary>
        public class SndKasokuDakaku : CommSndMsgBase
        {
            public const string ID = "31";
            /// <summary>
            /// ボデーNo
            /// </summary>
            public string CarNo { get; set; }
            /// <summary>
            /// 指示舵角
            /// </summary>
            public float Dakaku { get; set; }
            /// <summary>
            /// 指示加速度
            /// </summary>
            public double Speed { get; set; }
            /// <summary>
            /// ガイドエリア情報
            /// </summary>
            public uint guideareaInfo { get; set; }

            public SndKasokuDakaku() : base(ID) { }
            protected override string GetBodyString()
            {
                return String.Format("{0,5}", CarNo) + Speed.ToString("00.000").PadRight(7, char.Parse("0")) + Dakaku.ToString("00.000").PadRight(7, char.Parse("0")) + guideareaInfo.ToString("000");
            }
        }

        /// <summary>
        /// 制御終了指示
        /// </summary>
        public class SndControlEnd : CommSndMsgBase
        {
            public const string ID = "32";
            /// <summary>
            /// ボデーNo
            /// </summary>
            public string CarNo { get; set; }

            public SndControlEnd() : base(ID) { }
            protected override string GetBodyString()
            {
                return String.Format("{0,5}", CarNo);
            }
        }

        /// <summary>
        /// 非常停止指示
        /// </summary>
        public class SndEmergencyStop : CommSndMsgBase
        {
            public const string ID = "33";
            /// <summary>
            /// ボデーNo
            /// </summary>
            public string CarNo { get; set; }

            public SndEmergencyStop() : base(ID) { }
            protected override string GetBodyString()
            {
                return String.Format("{0,5}", CarNo);
            }
        }
        #endregion

        #endregion

    }
}