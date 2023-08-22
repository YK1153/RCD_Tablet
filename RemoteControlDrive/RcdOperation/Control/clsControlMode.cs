using AtrptShare;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static RcdOperation.Control.ControlMain;

namespace RcdOperation.Control
{

    /// <summary>
    /// 制御に関するモードやステータスの管理周辺を記載する
    /// ・モード
    /// ・自走前工程車両や自走工程車両の情報
    /// </summary>
    public partial class ControlMain
    {

        private Mode mode = new Mode
        {
            Ready = false,
            Selector = ModeSelector.Manual,
            ContinueON = false
        };


        /// <summary>
        /// 運転準備状態変更
        /// </summary>
        /// <param name="state"></param>
        internal void ChangeReady(bool state)
        {
            if (mode.Ready != state)
            {
                switch (state)
                {
                    case true:
                        if (mode.Ready==false)
                        {
                            mode.Ready = true;

                        }
                        break;
                    case false:
                        if (mode.Ready == true)
                        {
                            mode.Ready = false;

                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 各個連続切替
        /// </summary>
        /// <param name="state"></param>
        internal void ChangeSelector(ModeSelector state)
        {
            if (mode.Selector != state)
            {
                switch (state)
                {
                    case ModeSelector.Manual:
                        if (mode.Selector == ModeSelector.Auto)
                        {
                            mode.Selector = ModeSelector.Manual;
                            //連続状態がONであればOFFにする=>OFFにするとき制御中であれば走行停止する
                            if (mode.ContinueON)
                            {
                                ChangeContimueON(false);
                            }

                        }
                        break;
                    case ModeSelector.Auto:
                        if (mode.Selector == ModeSelector.Manual)
                        {
                            mode.Selector = ModeSelector.Auto;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 連続ON状態変更
        /// </summary>
        /// <param name="state"></param>
        internal void ChangeContimueON(bool state)
        {
            if (mode.ContinueON != state)
            {
                switch (state)
                {
                    case true:
                        if (mode.ContinueON == false)
                        {
                            mode.ContinueON = true;
                            //生産指示情報取得開始
                            m_ProductionInstructionsTimer.Start();
                        }
                        break;
                    case false:
                        if (mode.ContinueON == true)
                        {
                            mode.ContinueON = false;
                            //走行中であれば走行停止

                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 異常発生時状態遷移
        /// </summary>
        /// <param name="level">異常レベル</param>
        private void ChangeErrorMode(int level)
        {
            switch (level)
            {
                case 1:
                    // 軽度の異常(通常)
                    // 連続OFF
                    ChangeContimueON(false);
                    break;
                case 2:
                    // 重度の異常
                    // 連続OFF かつ 運転準備OFF 
                    ChangeContimueON(false);
                    ChangeReady(false);
                    break;
            }

        }

    }

    public enum ModeSelector
    {
        /// <summary>
        /// 各個
        /// </summary>
        Manual,
        /// <summary>
        /// 連続
        /// </summary>
        Auto
    }

    public class Mode
    {
        /// <summary>
        /// 運転準備 false:OFF true:ON
        /// </summary>
        internal bool Ready { get; set; }
        /// <summary>
        /// 各個連続状態 0:各個 1:連続
        /// </summary>
        internal ModeSelector Selector { get; set; }
        /// <summary>
        /// 連続状態 fale:OFF true:ON
        /// </summary>
        internal bool ContinueON { get; set; }
    }



    /// <summary>
    /// 自走前工程にいる車両の情報を格納する
    /// </summary>
    public class PreVehicle
    {
        private static PreVehicle instance = new PreVehicle();

        public VehicleInfo Vehicle { get; set; }

        public PreVehicleStatus Status { get; set; }
        public string ErrorMsg { get; set; }


        private PreVehicle()
        {
            Status = PreVehicleStatus.Preparation;
            //Vehicle = new VehicleInfo();
        }

        private void Clear()
        {
            instance = new PreVehicle();
        }

        public static PreVehicle Instance
        {
            get
            {
                return instance;
            }
        }
    }

    public enum PreVehicleStatus 
    {
        /// <summary>
        /// 生産指示待ち
        /// </summary>
        Preparation,
        /// <summary>
        /// 動作中
        /// </summary>
        Moving,
        /// <summary>
        /// 準備起動待ち
        /// </summary>
        WaitReadyBoot,
        /// <summary>
        /// 起動待ち
        /// </summary>
        WaitStartup,
        /// <summary>
        /// 前車両退出待ち
        /// </summary>
        WaitFrontVehicle,
        /// <summary>
        /// 異常完了
        /// </summary>
        ErrorEnd
    }



    /// <summary>
    /// 制御中または異常対処中の車両を格納する
    /// </summary>
    public class ControlVehicle
    {
        private static ControlVehicle instance = new ControlVehicle();

        public VehicleInfo Vehicle { get; set; }
        public ControlVehicleStatus Status { get; set; }
        public string ErrorMsg { get; set; }

        private ControlVehicle() 
        {
            Status = ControlVehicleStatus.Preparation;
            //Vehicle = new VehicleInfo();
        }

        public static ControlVehicle Instance
        {
            get
            {
                return instance;
            }
        }

        private void Clear()
        {
            instance = new ControlVehicle();
        }

        public enum ControlVehicleStatus
        {
            /// <summary>
            /// 制御準備中
            /// </summary>
            Preparation,
            /// <summary>
            /// 制御中
            /// </summary>
            Moving,
            /// <summary>
            /// 正常完了
            /// </summary>
            CompleteEnd,
            /// <summary>
            /// 異常完了
            /// </summary>
            ErrorEnd
        }
    }

    /// <summary>
    /// 車両情報
    /// </summary>
    [Serializable]
    public class VehicleInfo :ICloneable
    {
        /// <summary>
        /// ボデーNo
        /// </summary>
        public string BodyNo;
        /// <summary>
        /// 車両仕様番号
        /// </summary>
        public string CarSpecNo;
        /// <summary>
        /// 仕様内容
        /// </summary>
        public CarSpec CarSpec;
        /// <summary>
        /// IPアドレス
        /// </summary>
        public string IpAddr;
        /// <summary>
        /// PORT番号
        /// </summary>
        public string Port;

        public VehicleInfo() 
        { 
            CarSpec = new CarSpec();
        }

        public object Clone()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Position = 0;
                return (VehicleInfo)bf.Deserialize(ms);
            }
        }

    }


}
