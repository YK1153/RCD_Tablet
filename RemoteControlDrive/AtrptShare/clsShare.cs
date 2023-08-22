using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AtrptShare
{

    #region RouteNodeクラス
    [Serializable]
    public class RouteNode : ICloneable
    {
        public int ID;
        public float X;
        public float Y;
        public string Name;

        public object Clone()
        {
            return (RouteNode)MemberwiseClone();
        }
    }
    #endregion

    public class receive_Viewer : ICloneable
    {
        public int SID;
        public float facilityCoordinate_X;
        public float facilityCoordinate_Y;
        public string facilityName;
        public int facilityTypeID;
        public int visible;

        public object Clone()
        {
            return (receive_Viewer)MemberwiseClone();
        }
    }

    #region # SpeedAreaクラス #
    [Serializable]
    public class SpeedArea : ICloneable
    {
        public int ID;
        public string Name;
        public PointF[] Area;
        public double LeftWidth;
        public double RightWidth;
        public double Speed;
        public float Acceleration; // 加速度
        public float Deceleration; // 減速度
        public string GetRadian; //前方角度
        public string MinDistance; //最小距離
        public string MaxDistance; //最大距離

        // 減速エリア
        public string SlowLeftWidth;
        public string SlowRightWidth;
        public string SlowFrontDistance;
        public string SlowBackDistance;
        public PointF[] SlowArea;
        public string SlowCoefficient;
        // 一時停止エリア
        public string PauseLeftWidth;
        public string PauseRightWidth;
        public string PauseFrontDistance;
        public string PauseBackDistance;
        public PointF[] PauseArea;
        // 走行停止エリア
        public string StopLeftWidth;
        public string StopRightWidth;
        public string StopFrontDistance;
        public string StopBackDistance;
        public PointF[] StopArea;

        public object Clone()
        {
            SpeedArea sc = (SpeedArea)MemberwiseClone();

            if (sc.Area != null)
            {
                sc.Area = (PointF[])sc.Area.Clone();
            }

            return sc;
        }
    }
    #endregion

    #region CarInfo
    [Serializable]
    public class CarInfo : CamInfo
    {

        /// <summary>
        ///  車両現在舵角
        ///  </summary>
        public float angle;
        /// <summary>
        ///  車両現在速度
        ///  </summary>
        public float speed;
        /// <summary>
        /// 車両ヨーレートセンサ値
        ///  </summary>
        public double Yawrate;
        /// <summary>
        ///  ヨーレート取得時間
        ///  </summary>
        public int YawrateTime;
       
        /// <summary>
        ///  ヨーレート積分値
        ///  </summary>
        public double calcYawrate;

        /// <summary>
        /// 自車制御状態
        /// </summary>
        public int Act4;
        /// <summary>
        /// XSTOP
        /// </summary>
        public int XSTOP;

        public object Clone()
        {
            return (CarInfo)MemberwiseClone();
        }
    }

    public class CamInfo
    {

        /// <summary>
        /// 測位点X座標
        /// </summary>
        public float xPosition;
        /// <summary>
        ///  測位点Y座標
        ///  </summary>
        public float yPosition;
        /// <summary>
        ///  画像認識開始時刻
        ///  </summary>
        public string camStartTime;
        /// <summary>
        /// 車両マスク信頼度
        /// </summary>
        public string Reliability;
        /// <summary>
        /// カメラID(制御中=最後に受信したカメラID、制御外=初期カメラID)
        /// </summary>
        public string CamID;
        /// <summary>
        /// 認識処理時間
        /// </summary>
        public string ProcTime;
        /// <summary>
        ///  車両ベクトルX（画像解析測位）
        ///  </summary>
        public double xVector;
        /// <summary>
        ///  車両ベクトルY（画像解析測位）
        ///  </summary>
        public double yVector;
        /// <summary>
        ///  車両ベクトル計算値
        ///  </summary>
        public double Vector;
        /// <summary>
        ///  移動方向ベクトルX（画像解析測位）
        ///  </summary>
        public double MovexVector;
        /// <summary>
        ///  移動方向ベクトルY（画像解析測位）
        ///  </summary>
        public double MoveyVector;
        /// <summary>
        ///  車両・移動方向ベクトル計算値
        ///  </summary>
        public double MoveVector;

        /// <summary>
        /// 
        /// </summary>
        public uint observed_data_timestamp;

    }


    #endregion

    #region CalcInfo
    [Serializable]
    public class CalcInfo
    {
        /// <summary>
        ///  エリア始点ノード座標
        ///  </summary>
        public PointF StartPoint;
        /// <summary>
        ///  エリア終点ノード座標
        ///  </summary>
        public PointF EndPoint;
        /// <summary>
        ///  エリア速度
        ///  </summary>
        public double Speed;
        /// <summary>
        ///  ゴール地点判断
        ///  </summary>
        public bool InGoal;
        /// <summary>
        ///  エリア加速度
        ///  </summary>
        public float Acceleration;
        /// <summary>
        ///  エリア減速度
        ///  </summary>
        public float Deceleration;
        /// <summary>
        /// 走行エリア外であるか True:走行エリア外
        /// </summary>
        public Boolean OutOfArea;
        /// <summary>
        /// 目標軌道の旋回半径の制御項計算用
        /// </summary>
        public List<PointF> FFareaList;
        /// <summary>
        /// エリア移動変更有無
        /// </summary>
        public bool AreaChange;
    }

    public class ControlFacilityArea
    {
        public string ID;
        public string Command;
    }
    #endregion

    #region OrderInfo
    [Serializable]
    public class OrderInfo
    {
        /// <summary>
        ///  指示舵角
        ///  </summary>
        public double Angle;
        /// <summary>
        ///  目標舵角(閾値適応前)
        ///  </summary>
        public double Angleori;
        /// <summary>
        ///  舵角が舵角最大値を超えていないか　範囲内：True
        ///  </summary>
        public bool f_maxangle;
        /// <summary>
        ///  舵角が舵角変化閾値を超えていないか　範囲内：True
        ///  </summary>
        public bool f_moveangle;
        /// <summary>
        ///  指示加速度
        ///  </summary>
        public float Acceleration;
        /// <summary>
        ///  舵角計算用乖離量
        ///  </summary>
        public double L;
        /// <summary>
        ///  垂線と理想経路の交点のX座標
        ///  </summary>
        public float xCalcPoint;
        /// <summary>
        ///  垂線と理想経路の交点のY座標
        ///  </summary>
        public float yCalcPoint;
        /// <summary>
        ///  予測点X座標（ログ出力用）
        ///  </summary>
        public float xPosi;
        /// <summary>
        ///  予測点Y座標（ログ出力用）
        ///  </summary>
        public float yPosi;
        /// <summary>
        ///  舵角計算時刻
        ///  </summary>
        public DateTime newTime;
        /// <summary>
        ///  Lの総和（最新）
        ///  </summary>
        public double newsL;
        /// <summary>
        ///  ログ出力用舵角計算値
        ///  </summary>
        public calcLog log;
        /// <summary>
        ///  現在エリア目標速度（ログ出力用）
        ///  </summary>
        public double areaspeed; // 現在エリア速度
    }

    [Serializable]
    public class calcLog
    {
        public double p;
        public double v;
        public double a;
        public double d;
        public double i;

        public double p1;
        public double d1;
        public double i1;
        public double f1;
    }
    #endregion

    #region ConfigDataクラス
    [Serializable]
    public class ConfigData
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        public ConnectValue ConnectVal { get; set; }
        /// <summary>
        /// 演算係数
        /// </summary>
        public CalcCoefficient CalcCoefficient { get; set; }
        /// <summary>
        /// 前方注視点距離(測位点～予測地点距離)
        /// </summary>
        public double ForwardDistance { get; set; }
        /// <summary>
        /// 車両全幅
        /// </summary>
        public double WidthDistance { get; set; }
        /// <summary>
        /// FF項取得開始位置
        /// </summary>
        public int FFGetStartPosition { get; set; }
        /// <summary>
        /// 閾値
        /// </summary>
        public LimitValue LimitVal { get; set; }
        /// <summary>
        /// グローバル座標
        /// </summary>
        public GlobalCoordinate GlobalCoordinate { get; set; }
        /// <summary>
        /// 共通減速度
        /// </summary>
        public Acceleration Acceleration { get; set; }
        /// <summary>
        /// 測位点取得条件
        /// </summary>
        public GetCondition GetCondition { get; set; }
        /// <summary>
        /// 理想経路ノード
        /// </summary>
        public RouteNodeList RouteNodeList { get; set; }
        /// <summary>
        /// 理想経路走行エリア
        /// </summary>
        public SpeedAreaList SpeedAreaList { get; set; }
        /// <summary>
        /// その他エリア
        /// </summary>
        public List<OtherArea> OtherAreas { get; set; }
        /// <summary>
        /// ゴールするエリアの番号
        /// </summary>
        public int GoalNo { get; set; }
        /// <summary>
        /// 無線端末ポート選択 1or2
        /// </summary>
        public int UsePortNo { get; set; }
        /// <summary>
        /// ギア比初期値
        /// </summary>
        public double GearRate { get; set; }
        /// <summary>
        /// ホイールベース初期値
        /// </summary>
        public double Wheelbase { get; set; }
        public List<facility> trafficLights { get; set; }
        public List<facility> shutters { get; set; }
        public List<facility> facilitys { get; set; }
        /// <summary>
        /// 画像解析スコア
        /// </summary>
        public double camscore { get; set; }
        /// <summary>
        /// 補正個数
        /// </summary>
        public int fixcnt { get; set; }
        /// <summary>
        /// 無補正カメラ
        /// </summary>
        public string nofixcam { get; set; }

        // 減速エリア
        public string SlowLeftWidth { get; set; }
        public string SlowRightWidth { get; set; }
        public string SlowFrontDistance { get; set; }
        public string SlowBackDistance { get; set; }
        public string SlowCoefficient { get; set; }
        // 一時停止エリア
        public string PauseLeftWidth { get; set; }
        public string PauseRightWidth { get; set; }
        public string PauseFrontDistance { get; set; }
        public string PauseBackDistance { get; set; }
        // 走行停止エリア
        public string StopLeftWidth { get; set; }
        public string StopRightWidth { get; set; }
        public string StopFrontDistance { get; set; }
        public string StopBackDistance { get; set; }

        public object Clone()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Position = 0;
                return (ConfigData)bf.Deserialize(ms);
            }
        }
    }

    #region ConnectValue
    /// <summary>
    /// 接続文字列
    /// </summary>
    [Serializable]
    public class ConnectValue
    {
        public string mClConnectIP { get; set; }
        public int mClConnectPORT { get; set; }
        //public int mSrvConnectPORT { get; set; }
        public int mUdpConnectPORT { get; set; }
        public string pConnectIP { get; set; }
        public int pConnectPORT { get; set; }
        public string tClConnectIP { get; set; }
        public int tClConnectPORT { get; set; }
        public int tSrvConnectPORT { get; set; }

        public object Clone()
        {
            return (ConnectValue)MemberwiseClone();
        }
    }
    #endregion

    #region CalcCoefficient
    /// <summary>
    /// 演算パラメータ
    /// </summary>
    [Serializable]
    public class CalcCoefficient
    {
        public double Kp;
        public double Ki;
        public double Kd;
        public double Kf;

        public object Clone()
        {
            return (CalcCoefficient)MemberwiseClone();
        }
    }
    #endregion

    #region LimitValue
    /// <summary>
    /// 閾値
    /// </summary>
    [Serializable]
    public class LimitValue
    {
        /// <summary>
        /// 舵角最大値
        /// </summary>
        public double AngleLimit { get; set; }
        /// <summary>
        /// 舵角変動最大値
        /// </summary>
        public double AngleMoveLimit { get; set; }

        public object Clone()
        {
            return (LimitValue)MemberwiseClone();
        }
    }
    #endregion

    #region GlobalCoordinate
    /// <summary>
    /// グローバル座標
    /// </summary>
    [Serializable]
    public class GlobalCoordinate
    {
        /// <summary>
        /// 最小値
        /// </summary>
        public PointF minCoordinate { get; set; }
        /// <summary>
        /// 最大値
        /// </summary>
        public PointF maxCoordinate { get; set; }

        public object Clone()
        {
            return (GlobalCoordinate)MemberwiseClone();
        }
    }
    #endregion

    #region Acceleration
    /// <summary>
    /// 加速度設定
    /// </summary>
    [Serializable]
    public class Acceleration
    {
        /// <summary>
        /// 走行停止時減速度
        /// </summary>
        public float Abnormal { get; set; }
        /// <summary>
        /// 一時停止時減速度
        /// </summary>
        public float Pause { get; set; }
        /// <summary>
        /// ゴールエリア減速度
        /// </summary>
        public float Goal { get; set; }
        /// <summary>
        /// 加速度変化量
        /// </summary>
        public float AcceAmount { get; set; }
        /// <summary>
        /// 減速度変化量
        /// </summary>
        public float DeceAmount { get; set; }
        /// <summary>
        /// 速度閾値
        /// </summary>
        public float Threshold { get; set; }

        public object Clone()
        {
            return (Acceleration)MemberwiseClone();
        }
    }
    #endregion

    #region GetCondition
    /// <summary>
    /// 測位点取得条件
    /// </summary>
    [Serializable]
    public class GetCondition
    {
        /// <summary>
        /// 取得前方角度
        /// </summary>
        public float GetRadian { get; set; }
        /// <summary>
        /// 取得最小距離
        /// </summary>
        public float MinDistance { get; set; }
        /// <summary>
        /// 取得最大距離
        /// </summary>
        public float MaxDistance { get; set; }
        /// <summary>
        /// 異常値除外最大個数
        /// </summary>
        public int ErrorCount { get; set; }

        public object Clone()
        {
            return (GetCondition)MemberwiseClone();
        }
    }
    #endregion

    #region RouteNodeValue
    [Serializable]
    public class RouteNodeList
    {
        public List<RouteNode> RouteNode { get; set; }

        public object Clone()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Position = 0;
                return (RouteNodeList)bf.Deserialize(ms);
            }
        }
    }
    #endregion

    #region SpeedAreaValue
    [Serializable]
    public class SpeedAreaList
    {
        public List<SpeedArea> SpeedArea { get; set; }

        public object Clone()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Position = 0;
                return (SpeedAreaList)bf.Deserialize(ms);
            }
        }
    }
    #endregion

    #region OtherArea
    [Serializable]
    public class OtherArea
    {
        /// <summary>
        /// エリアの種類
        /// </summary>
        public OtherAreaCode AreaCode;
        /// <summary>
        /// 番号
        /// </summary>
        public int Number;
        /// <summary>
        /// エリア名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 設備の種類
        /// </summary>  
        public FacilityCode FaciCode;
        /// <summary>
        /// 設備ID
        /// </summary>
        public string FacilityID;
        /// <summary>
        /// 設備ID
        /// </summary>
        public int FacilitySID;
        /// <summary>
        /// 指示内容
        /// </summary>
        public int Command;
        /// <summary>
        /// 判定エリア
        /// </summary>
        public PointF[][] area;
        /// <summary>
        /// 在籍センサ確認有無 True：有
        /// </summary>
        public bool Sensor;
        /// <summary>
        /// 縦ガイド
        /// </summary>
        public int verticalGuide;
        /// <summary>
        /// 横ガイド
        /// </summary>
        public int besideGuide;

        public object Clone()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Position = 0;
                return (SpeedAreaList)bf.Deserialize(ms);
            }
        }
    }

    public enum OtherAreaCode
    {
        /// <summary>
        /// ゴールエリア設定
        /// </summary>
        GoalArea = 0,

        /// <summary>
        /// 周辺設備設定
        /// </summary>
        FacilityArea = 2,
        /// <summary>
        /// ゴール変更禁止開始エリア設定
        /// </summary>
        GoalChangeStopArea = 3,
        /// <summary>
        /// ガイドエリア
        /// </summary>
        GuideArea = 4
    }

    public enum FacilityCode
    {
        /// <summary>
        /// 信号機
        /// </summary>
        TrafficLight = 0,
        /// <summary>
        /// シャッター
        /// </summary>
        Shutter = 1,
        /// <summary>
        /// その他設備
        /// </summary>
        Facility = 2
    }
    #endregion

    [Serializable]
    public class facility
    {
        /// <summary>
        /// 設備ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 設備名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int sid { get; set; }
    }

    #endregion

    #region # CarSpec #

    [Serializable]
    public class CarSpec
    {
        public double Wheelbase { get; set; }

        public double GearRate { get; set; }

        public double ForwardDistance { get; set; }

        public double WidthDistance { get; set; }

        public double CenterFrontLength { get; set; }

        public double RearOverhang { get; set; }

        public object Clone()
        {
            return (CarSpec)MemberwiseClone();
        }
    }
    #endregion

    #region # SettingInfo #
    public class SettingInfo
    {
        /// <summary>
        /// 車種ID
        /// </summary>
        public char[] vehicle_type_id { get; set; }
    }
    #endregion

    #region # VehicleData #
    public class VehicleData
    {
        /// <summary>
        /// 自車停車フラグ
        /// </summary>
        public bool is_vehicle_stop { get; set; }
        /// <summary>
        /// 車速(SP1)
        /// </summary>
        public double vehicle_speed { get; set; }
        /// <summary>
        /// 舵角(ピニオン角)
        /// </summary>
        public double steer_angle { get; set; }
        /// <summary>
        /// GL1,GXセンサ信号
        /// </summary>
        public double gx { get; set; }
        /// <summary>
        /// GL2,GYセンサ信号
        /// </summary>
        public double gy { get; set; }
        /// <summary>
        /// ヨーレートセンサ
        /// </summary>
        public double yr { get; set; }
        /// <summary>
        /// 車両信号タイムスタンプ
        /// </summary>
        public uint vehicle_data_timestamp { get; set; }
        /// <summary>
        /// 操舵トルク
        /// </summary>
        public double steer_torque { get; set; }
        /// <summary>
        /// 操舵トルク無効フラグ
        /// </summary>
        public bool is_steer_torque_invalid { get; set; }
        /// <summary>
        /// 操舵トルクLSB拡張信号
        /// </summary>
        public double steer_torque_enhaned { get; set; }
        /// <summary>
        /// EPSモータトルク
        /// </summary>
        public double eps_motor_torque { get; set; }
        /// <summary>
        /// 推定車体速無効
        /// </summary>
        public bool is_vehicle_speed_from_vsc_invalid { get; set; }
        /// <summary>
        /// 車速パルス信号積算値
        /// </summary>
        public uint sp1_pulse{ get; set; }
        /// <summary>
        /// 推定車体速
        /// </summary>
        public double vehicle_speed_from_vsc { get; set; }
        /// <summary>
        /// ブレーキペダルドライバー要求加速度
        /// </summary>
        public double request_gx_from_b_pedal { get; set; }
        /// <summary>
        /// アクセルペダル要求加速度
        /// </summary>
        public double request_gx_from_a_pedal { get; set; }
        /// <summary>
        /// 推定車体加速度
        /// </summary>
        public double estimated_gx_form_vsc { get; set; }
        /// <summary>
        /// GVC無効フラグ
        /// </summary>
        public bool gvc_invalid { get; set; }
        /// <summary>
        /// EPBﾛｯｸ状態
        /// </summary>
        public uint epb_lock_state { get; set; }
        /// <summary>
        /// VMC認識シフトレンジ
        /// </summary>
        public uint shift_range_from_vmc { get; set; }
        /// <summary>
        /// ヨーレイトセンサ１無効/有効
        /// </summary>
        public bool yawrate_sensor_1_invalid { get; set; }
        /// <summary>
        /// ヨーレイトセンサ2無効/有効
        /// </summary>
        public bool yawrate_sensor_2_invalid { get; set; }
        /// <summary>
        /// センサ電源電圧（ＩＧ）無効／有効
        /// </summary>
        public bool yaw_g_sensor_power_invalid { get; set; }
        /// <summary>
        /// GL1,GX無効／有効
        /// </summary>
        public bool g_sensor_1_invalid { get; set; }
        /// <summary>
        /// センサ識別
        /// </summary>
        public uint yaw_g_sensor_type_identification { get; set; }
        /// <summary>
        /// GL2,GY無効／有効
        /// </summary>
        public bool g_sensor_2_invalid { get; set; }
        /// <summary>
        /// ヨーレートセンサ１高分解能信号
        /// </summary>
        public double yaw_senser_1_value { get; set; }

        /// <summary>
        /// ヨーレートセンサ２高分解能信号
        /// </summary>
        public double yaw_senser_2_value { get; set; }

    }
    #endregion

    #region # ObservedData #
    public class ObservedData
    {
        /// <summary>
        /// 車両左後ろx座標
        /// </summary>
        public double observed_vehicle_x { get; set; }
        /// <summary>
        /// 車両左後ろy座標
        /// </summary>
        public double observed_vehicle_y { get; set; }
        /// <summary>
        /// 車両左後ろyaw座標
        /// </summary>
        public double observed_vehicle_yaw { get; set; }
        /// <summary>
        /// 認識結果タイムスタンプ
        /// </summary>
        public uint observed_data_timestamp { get; set; }
        ///// <summary>
        ///// 分散共分散行列個数
        ///// </summary>
        //public int observed_varcov_count { get; set; }
        ///// <summary>
        ///// 分散共分散行列
        ///// </summary>
        //public double[] observed_varcov_matrix { get; set; }

    }
    #endregion

    #region # ControlCenterData #
    public class ControlCenterData
    {
        /// <summary>
        /// タイムスタンプ
        /// </summary>
        public uint timestamp { get; set; }
        /// <summary>
        /// ゴール座標
        /// </summary>
        public PointF[] goalarea { get; set; }
        /// <summary>
        /// 制御状態
        /// </summary>
        public uint request_control_status { get; set; }
        /// <summary>
        /// 減速率
        /// </summary>
        public double deceleration_rate { get; set; }

        public ControlCenterData()
        {
            deceleration_rate = 1.0;
        }
    }

    #endregion

    #region # AVPAppOutPutData #
    [Serializable]
    public class AVPAppOutputData
    {
        /// <summary>
        /// 目標舵角
        /// </summary>
        public double target_steering_angle { get; set; }
        /// <summary>
        /// 目標加速度
        /// </summary>
        public double target_gx { get; set; }
        /// <summary>
        /// 制御点
        /// </summary>
        public ControlPoint control_point { get; set; }
        /// <summary>
        /// 描画情報
        /// </summary>
        public DrawInfo draw_info { get; set; }
        /// <summary>
        /// アプリ制御状態
        /// </summary>
        public uint avp_app_state { get; set; }
        /// <summary>
        /// エラーコード配列
        /// </summary>
        public char[] avp_error_code { get; set; }
        /// <summary>
        /// ガイドエリア情報
        /// </summary>
        public uint guideareaInfo { get; set; }
        /// <summary>
        /// Pana出力情報
        /// </summary>
        public PanaOutputData panaOutputData { get; set; }


        public object Clone()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Position = 0;
                return (AVPAppOutputData)bf.Deserialize(ms);
            }
        }
    }
    #endregion

    #region  # DrawInfo #
    [Serializable]
    public class DrawInfo
    {
        /// <summary>
        /// ヨーレート積算値
        /// </summary>
        public double yawrate { get; set; }
        /// <summary>
        /// 最大舵角ガード
        /// </summary>
        public bool angle_guard { get; set; }
        /// <summary>
        /// ガード前舵角
        /// </summary>
        public double guard_front_angle { get; set; }
    }
    #endregion

    #region # ControlPoint #
    [Serializable]
    public class ControlPoint
    {
        /// <summary>
        /// X座標
        /// </summary>
        public double x { get; set; }
        /// <summary>
        /// Y座標
        /// </summary>
        public double y { get; set; }
    }
    #endregion

    #region # PanaSettingData #

    [Serializable]
    public class PanaSettingData
    {
        /// <summary>
        /// コンフィグ情報
        /// </summary>
        public ConstData constData { get; set; }
        /// <summary>
        /// 初期ベクトル
        /// </summary>
        public double first_vector { get; set; }
        /// <summary>
        /// 車両仕様
        /// </summary>
        public CarSpec carSpec { get; set; }
        public object Clone()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Position = 0;
                return (PanaSettingData)bf.Deserialize(ms);
            }
        }
    }

    #endregion

    #region # ConstData #

    [Serializable]
    public class ConstData
    {
        /// <summary>
        /// 制御復帰後前回指示値送信時間
        /// </summary>
        public int actual_angle_wait { get; set; }
        /// <summary>
        /// 初期ベクトル固定値仕様有無
        /// </summary>
        public bool use_start_yawrate { get; set; }
        /// <summary>
        /// 初期ベクトル固定値
        /// </summary>
        public int start_yawrate { get; set; }
        /// <summary>
        /// 初回取得座標X
        /// </summary>
        public double first_pos_x { get; set; }
        /// <summary>
        /// 初回取得座標Y
        /// </summary>
        public double first_pos_y { get; set; }
        /// <summary>
        /// 初回取得範囲X
        /// </summary>
        public double first_diff_x { get; set; }
        /// <summary>
        /// 初回取得範囲Y
        /// </summary>
        public double first_diff_y { get; set; }
        /// <summary>
        /// 初回取得エラー限界数
        /// </summary>
        public int first_errmax_cnt { get; set; }
        /// <summary>
        /// 補正信頼度毎回確認
        /// </summary>
        public bool score_all_use { get; set; }
        /// <summary>
        /// 舵角制限モード
        /// </summary>
        public int angle_limit_mode { get; set; }
        /// <summary>
        /// 加速度モード
        /// </summary>
        public int acceleration_mode { get; set; }
        /// <summary>
        /// 測位点エリア内法判定判定エリア数（前後）
        /// </summary>
        public int include_judge_area_count { get; set; }

        public bool rcdlogger_use { get; set; }

        public string rcdlogger_path { get; set; }

    }
    #endregion

    #region # PanaInputData #
    public class PanaInputData
    {
        /// <summary>
        /// ヨーレートティックカウント
        /// </summary>
        public int tickcount { get; set; }
        /// <summary>
        /// カメラID
        /// </summary>
        public string camera_id { get; set; }
        /// <summary>
        /// 信頼度
        /// </summary>
        public string reliability { get; set; }
        /// <summary>
        /// 自走制御フラグ
        /// </summary>
        public bool act4 { get; set; }

        public double yr { get; set; }
        /// <summary>
        /// ゴール番号
        /// </summary>
        public int goalno { get; set; }
    }
    #endregion

    #region # PanaOutputData #
    [Serializable]

    public class PanaOutputData
    {
        public double L { get; set; }
        public calcLog log { get; set; }
        public bool get { get; set; }
    }
    #endregion

    #region # CarCornerinfo #

    //public class CarCornerinfo
    //{
    //    public bool OutOfArea { get; set; }
    //    public string cornerNumber { get; set; }
    //    public PointF cornerPoint { get; set; }
    //}
    #endregion

    /// <summary>
    /// 車両４隅位置情報
    /// </summary>
    public class CarCornerinfo
    {
        public bool OutOfArea { get; set; }
        public int containAreaidx { get; set; }
        public string cornerName { get; set; }
        public PointF cornerPoint { get; set; }
    }

    #region カメラマスタ
    //※見直し対象
    public class CamList
    {
        /// <summary>
        /// カメラID
        /// </summary>
        public string CamID { get; set; }


    }
    #endregion

    #region ガイドエリア判定用
    public class GuideareaJudge
    {
        /// <summary>
        /// ガイドエリア
        /// </summary>
        public OtherArea guidearea { get; set; }
        /// <summary>
        /// エリア内在フラグ
        /// </summary>
        public bool inareaflg { get; set; }
    }
    #endregion
}
