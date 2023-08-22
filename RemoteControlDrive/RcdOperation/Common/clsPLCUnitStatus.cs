using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RcdDao.MPLCDao;
using CommWrapper;
using RcdCmn;
using System.Reflection;
using System.Threading;
using RcdDao;
using System.Configuration;
using static RcdCmn.Res;
using RcdOperation;

namespace RcpOperation.Common
{
    public class PLCUnitStatus : IDisposable
    {
        #region ## クラス定数 ##
        private const string C_READ_CMD_HEADER = "000005001E";
        private const int C_READ_CMD_START_POSITION = 11;

        private const string C_WRITE_CMD_HEAD = "0000";
        private const string C_WRITE_CMD_CODE = "1F";

        internal readonly int C_STAUTS_READ_INTERVAL = int.Parse(ConfigurationManager.AppSettings["STATUS_READ_INTERVAL"]) >= 40 ? int.Parse(ConfigurationManager.AppSettings["STATUS_READ_INTERVAL"]) - 30 : 10;
        internal readonly int C_STAUTS_WRITE_INTERVAL = int.Parse(ConfigurationManager.AppSettings["STATUS_WRITE_INTERVAL"]) >= 40 ? int.Parse(ConfigurationManager.AppSettings["STATUS_WRITE_INTERVAL"]) - 30 : 10;
        private const int C_SEND_TIMEOUT = 50;

        public const string C_MESSAGE_SUCCESS = "0000";
        public const string C_MESSAGE_RESEIVE_ERR = "1001";
        #endregion

        #region ## クラス変数 ##
        private AppLog LOGGER = AppLog.GetInstance();

        private Timer m_status_read_timer;
        private Timer m_status_write_timer;
        private object m_Conn_lock = new object();
        private object m_Write_lock = new object();

        // WatchDog Write
        private Timer WriteTimer;
        public int m_WriteInterval;
        public int WriteStatus;

        // WatchDog Read
        private Timer ReadTimer;
        public int m_ReadInterval;
        public byte? LastBit;

        private bool IsDisposed = false;
        #endregion

        #region ## プロパティ ##

        public MPlcDevice PLCInfo { get; private set; }
        public CommPlcCl m_plccl { get; private set; }

        public string ReadCommand { get; private set; }
        public string WriteCommandHeader { get; private set; }

        public List<byte[]> ReadContactStatus { get; private set; }
        public List<byte[]> WriteContactStatus { get; private set; }

        public bool bWriteWait { get; set; }

        public DateTime LastChanged { get; private set; }

        public ConnectStatus Connected { get; private set; } = ConnectStatus.Disconnected;

        private ResMsg _status = PLCStatus.UNDETECTED;
        public ResMsg Status
        {
            get
            {
                return _status;
            }
            private set
            {
                if (!value.In(PLCStatus.Assignable))
                {
                    LOGGER.Error($"[PLCWatching] Set Status: unassignable status: {value.Msg}");
                    return;
                }

                ResMsg prev = _status;
                _status = value;

                if (prev != value)
                {
                    //PLCStatusChanged?.Invoke(this, new PLCStatusChangedEventArgs(prev, value));
                    LOGGER.Info($"[PLCステータス変化] {PLCInfo.Name}: {prev.Msg} -> {value.Msg}");
                }
            }
        }
        #endregion

        #region ## コンストラクタ ##
        public PLCUnitStatus(MPlcDevice plcinfo)
        {
            PLCInfo = plcinfo;

            m_plccl = new CommPlcCl(plcinfo.IPAddr, plcinfo.PortNo, C_SEND_TIMEOUT);
            m_plccl.Connected += new CommPlcCl.ClientConnectedEventHandler(OnConnected);
            m_plccl.Disconnected += new CommPlcCl.ClientConnectedEventHandler(OnDisConnected);
            m_plccl.Connect();

            ReadContactStatus = new List<byte[]>();
            WriteContactStatus = new List<byte[]>();

            CreateReadCommand();
            CreateWriteCommandHeader();

            PLCWatchInisialize();
        }

        public void PLCWatchInisialize()
        {
            // 書込み
            // ウォッチドッグ処理セット
            WriteStatus = 0;
            // タイムPLC応答時間-20 タイマー誤差-10
            m_WriteInterval = PLCInfo.WDWriteInterval - 20 - 10;
            WriteTimer = new Timer(new TimerCallback(Callback_OnTimer), null, m_WriteInterval, m_WriteInterval);

            // 読込み
            ReadTimer = new Timer(new TimerCallback(Callback_OnTimeOver), null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 読取コマンド作成
        /// </summary>
        private void CreateReadCommand()
        {
            ReadCommand = "";
            ReadCommand += C_READ_CMD_HEADER;

            string address = Convert.ToString(Convert.ToInt32(PLCInfo.ReadStartAddr, 16) * 2 + 4096, 16).PadLeft(4, '0');
            ReadCommand += address.Substring(2, 2) + address.Substring(0, 2);
            string readbyte = Convert.ToString((PLCInfo.ReadByteNum * 2), 16).PadLeft(4, '0');
            ReadCommand += readbyte.Substring(2, 2) + readbyte.Substring(0, 2);
        }

        private void CreateWriteCommandHeader()
        {
            WriteCommandHeader = "";
            WriteCommandHeader += C_WRITE_CMD_HEAD;

            string commandnum = Convert.ToString((PLCInfo.WriteByteNum * 2 + 3), 16).PadLeft(4, '0');
            WriteCommandHeader += commandnum.Substring(2, 2) + commandnum.Substring(0, 2);

            WriteCommandHeader += C_WRITE_CMD_CODE;
        }
        #endregion

        #region ## 接続・切断イベント ##

        private void OnConnected(ClientConnectedEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                LOGGER.Info($"{PLCInfo.Name}と接続しました。");

                Connected = ConnectStatus.Connected;

                if (WriteContactStatus.Count == 0)
                {
                    SendMessage(SendType.WriteAddrRead);
                }
                else
                {
                    SendMessage(SendType.Write);
                }

                m_status_read_timer = new Timer(new TimerCallback(Callback_StatusCheck), null, Timeout.Infinite, 0);
                m_status_read_timer.Change(0, C_STAUTS_READ_INTERVAL);

                m_status_write_timer = new Timer(new TimerCallback(Callback_StatusWrite), null, Timeout.Infinite, 0);
                m_status_write_timer.Change(0, C_STAUTS_WRITE_INTERVAL);
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        private void OnDisConnected(ClientConnectedEventArgs e)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                LOGGER.Info($"{PLCInfo.Name}と切断しました。");

                Connected = ConnectStatus.Disconnected;

                if (m_status_read_timer != null)
                {
                    m_status_read_timer.Dispose();
                    m_status_read_timer = null;
                }

                if (m_status_write_timer != null)
                {
                    m_status_write_timer.Dispose();
                    m_status_write_timer = null;
                }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End"); }
        }

        #endregion

        #region ## 読取・書込 ##
        public bool SendMessage(SendType type)
        {
            string result = "";
            switch (type)
            {
                case SendType.Read:
                    {
                        result = ReadMessage();
                    }
                    break;
                case SendType.Write:
                    {
                        result = WriteMessage();
                    }
                    break;
                case SendType.WriteAddrRead:
                    {
                        result = WriteAddrReadMessage();
                    }
                    break;
            }

            if (result == C_MESSAGE_RESEIVE_ERR)
            {
                if (Connected == ConnectStatus.Connected)
                {
                    LOGGER.Error("再接続処理開始");
                    //一足先にエラー(未接続)状態に
                    Connected = ConnectStatus.Disconnected;
                    //再接続
                    m_plccl.Disconnect(true);
                    m_plccl.Connect();
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public string ReadMessage()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            string rcvmsg = "";
            lock (m_Conn_lock)
            {
                //読取コマンド送信
                LOGGER.Info($"{this.PLCInfo.Name} SndReadCommand:{ReadCommand}");
                if (m_plccl.IsConnected)
                {
                    rcvmsg = m_plccl.SendMessage(ReadCommand);
                }
                else { LOGGER.Info($"is not Connected"); }
            }
            LOGGER.Info($"{this.PLCInfo.Name} RcvReadCommand:{rcvmsg}");
            if (rcvmsg.Length >= 11)
            {
                ReadContactStatus = ConvertList(rcvmsg);
                return C_MESSAGE_SUCCESS;
            }
            else
            {
                return C_MESSAGE_RESEIVE_ERR;
            }
        }

        public string WriteMessage()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            string sndcmd = "";
            List<byte[]> Status;
            lock (m_Write_lock)
            {
                bWriteWait = false;
                sndcmd = WriteCommandHeader;
                Status = new List<byte[]>(WriteContactStatus);
            }

            string address = Convert.ToString(Convert.ToInt32(PLCInfo.WriteStartAddr, 16) * 2 + 4096, 16).PadLeft(4, '0');
            sndcmd += address.Substring(2, 2) + address.Substring(0, 2);

            foreach (byte[] bit in Status)
            {
                string s = "";
                for (int i = 1; i <= 8; i++)
                {
                    s += bit[8 - i];
                }

                for (int i = 0; i < 2; i++)
                {
                    int ss = Convert.ToInt32(s.Substring(i * 4, 4), 2);
                    sndcmd += Convert.ToString(ss, 16);
                }
                s = "";
                for (int i = 1; i <= 8; i++)
                {
                    s += bit[16 - i];
                }

                for (int i = 0; i < 2; i++)
                {
                    int ss = Convert.ToInt32(s.Substring(i * 4, 4), 2);
                    sndcmd += Convert.ToString(ss, 16);
                }
            }

            string rcvmsg = "";
            lock (m_Conn_lock)
            {
                //書込コマンド送信
                LOGGER.Info($"{this.PLCInfo.Name} SndWriteCommand:{sndcmd}");
                if (m_plccl.IsConnected)
                {
                    rcvmsg = m_plccl.SendMessage(sndcmd);
                }
                else { LOGGER.Info($"is not Connected"); }

            }
            LOGGER.Info($"{this.PLCInfo.Name} RcvWriteCommand:{rcvmsg}");
            if (rcvmsg.Length > 4)
            {
                return C_MESSAGE_SUCCESS;
            }
            else
            {
                return rcvmsg;
            }

        }

        public string WriteAddrReadMessage()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            string sndmsg = "";
            sndmsg += C_READ_CMD_HEADER;

            string address = Convert.ToString(Convert.ToInt32(PLCInfo.WriteStartAddr, 16) * 2 + 4096, 16).PadLeft(4, '0');
            sndmsg += address.Substring(2, 2) + address.Substring(0, 2);
            string readbyte = Convert.ToString((PLCInfo.WriteByteNum * 2), 16).PadLeft(4, '0');
            sndmsg += readbyte.Substring(2, 2) + readbyte.Substring(0, 2);

            string rcvmsg = "";
            lock (m_Conn_lock)
            {
                LOGGER.Info($"{this.PLCInfo.Name} SndWriteAddReadCommand:{sndmsg}");
                if (m_plccl.IsConnected)
                {
                    rcvmsg = m_plccl.SendMessage(sndmsg);
                }
                else { LOGGER.Info($"is not Connected"); }
            }
            LOGGER.Info($"{this.PLCInfo.Name} RcvWriteAddReadCommand:{rcvmsg}");
            if (rcvmsg.Length >= 11)
            {
                WriteContactStatus = ConvertList(rcvmsg); ReadContactStatus = ConvertList(rcvmsg);
                return C_MESSAGE_SUCCESS;
            }
            else
            {
                return C_MESSAGE_RESEIVE_ERR;
            }
        }

        public List<byte[]> ConvertList(string StatusCommand)
        {
            string Command = StatusCommand.Substring(C_READ_CMD_START_POSITION - 1);

            List<byte[]> status = new List<byte[]>();
            for (int i = 0; i < Command.Length; i += 4)
            {
                byte[] bit = new byte[16];

                string r1bit = Convert.ToString(Convert.ToInt32(Command.Substring(i, 2), 16), 2);
                r1bit = r1bit.PadLeft(8, '0');
                for (int j = 0; j < r1bit.Length; j++)
                {
                    bit[j] = byte.Parse(r1bit.Substring(r1bit.Length - 1 - j, 1));
                }

                if (Command.Length >= i + 4)
                {
                    string r2bit = Convert.ToString(Convert.ToInt32(Command.Substring(i + 2, 2), 16), 2);
                    r2bit = r2bit.PadLeft(8, '0');
                    for (int j = 0; j < r2bit.Length; j++)
                    {
                        bit[j + r1bit.Length] = byte.Parse(r2bit.Substring(r2bit.Length - 1 - j, 1));
                    }
                }

                status.Add(bit);
            }

            return status;
        }

        #endregion

        #region ## 値変更 ##
        public bool SetOutBit(int addrNo, int bitNo, int value)
        {
            if (Connected == ConnectStatus.Connected)
            {
                lock (m_Write_lock)
                {
                    WriteContactStatus[addrNo][bitNo] = (byte)value;
                    bWriteWait = true;
                }
                LOGGER.Info($"Value Change(機器:{PLCInfo.Name},アドレス:{addrNo},ビット:{bitNo},値:{value})");
            }
            else
            {
                LOGGER.Error($"{PLCInfo.Name}と未接続のため内部値の変更に失敗しました。");
                return false;
            }

            //bool ret = SendMessage(SendType.Write);
            bool ret = true;

            return ret;
        }

        #endregion

        #region ## 値取得 ##
        public bool? GetReadBit(int addrNo, int bitNo)
        {
            if (Connected == ConnectStatus.Connected)
            {
                return ReadContactStatus[addrNo][bitNo] == 1 ? true : false;
            }
            else
            {
                return null;
            }

        }

        #endregion

        #region ## 定期処理 ##
        private void Callback_StatusCheck(object state)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (m_status_read_timer == null) { return; }
                m_status_read_timer.Change(Timeout.Infinite, Timeout.Infinite);

                SendMessage(SendType.Read);

                // PLCステータス確認
                UpdatePLCWatchStatus();
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally
            {
                if (m_status_read_timer != null) m_status_read_timer.Change(C_STAUTS_READ_INTERVAL, C_STAUTS_READ_INTERVAL);
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        private void Callback_StatusWrite(object state)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                if (m_status_write_timer == null) { return; }
                m_status_write_timer.Change(Timeout.Infinite, Timeout.Infinite);
                if (bWriteWait)
                {
                    SendMessage(SendType.Write);
                }
                else { LOGGER.Info($"機器:{PLCInfo.Name}:No Status Change"); }
            }
            catch (UserException ue) { ExceptionProcess.UserExceptionConsoleProcess(ue); }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally
            {
                if (m_status_write_timer != null) m_status_write_timer.Change(C_STAUTS_WRITE_INTERVAL, C_STAUTS_WRITE_INTERVAL);
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        #endregion

        #region ## ウォッチドッグ書き換え ##
        /// <summary>
        /// ウォッチドッグ書込み更新タイマーコールバック
        /// </summary>
        /// <param name="sender"></param>
        private void Callback_OnTimer(object sender)
        {
            try
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
                WriteTimer.Change(Timeout.Infinite, Timeout.Infinite);

                WriteStatus = WriteStatus == 0 ? 1 : 0;
                if (m_plccl.IsConnected)
                {
                    LOGGER.Info($"{PLCInfo.Name}:生存出力({WriteStatus})");
                    SetOutBit(PLCInfo.WDWriteAddrNum, PLCInfo.WDWriteBitNum, WriteStatus);
                }
            }
            catch (Exception ex) { ExceptionProcess.ComnExceptionConsoleProcess(ex); }
            finally
            {
                if (!IsDisposed)
                {
                    WriteTimer.Change(m_WriteInterval, m_WriteInterval);
                }
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }
        #endregion

        #region ## ウォッチドック読み取り ##
        /// <summary>
        /// PLCデバイス接点値より、PLCステータスを更新
        /// </summary>
        /// <param name="inputs">PLCデバイス接点値リスト</param>
        private void UpdatePLCWatchStatus()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} Start");
            try
            {
                SetLastBit(ReadContactStatus[PLCInfo.WDReadAddrNum][PLCInfo.WDReadBitNum]);
            }
            catch (UserException ue)
            {
                ExceptionProcess.UserExceptionProcess(ue);
            }
            catch (Exception ex)
            {
                ExceptionProcess.ComnExceptionProcess(ex);
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} End");
            }
        }

        /// <summary>
        /// ウォッチドック読込みステータス変更
        /// </summary>
        /// <param name="bit"></param>
        public void SetLastBit(byte bit)
        {
            if (LastBit == bit)
            {
                return;
            }

            bool isFirstBit = LastBit == null;

            LastBit = bit;
            LastChanged = DateTime.Now;

            if (isFirstBit)
            {
                return;
            }


            if (Status == PLCStatus.UNDETECTED || Status.In(PLCStatus.Abnormals))
            {
                // タイマー開始
                Status = PLCStatus.NORMAL;
                ReadTimer.Change(PLCInfo.WDReadTimeout, PLCInfo.WDReadTimeout);
            }

            if (Status == PLCStatus.NORMAL)
            {
                // タイマーリセット
                ReadTimer.Change(PLCInfo.WDReadTimeout, PLCInfo.WDReadTimeout);
            }

        }
        /// <summary>
        /// ヘルスチェックタイムアウト
        /// </summary>
        /// <param name="sender"></param>
        private void Callback_OnTimeOver(object sender)
        {
            if (Status == PLCStatus.NORMAL)
            {
                Status = PLCStatus.TIMEOUT;
                ReadTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        #endregion

        #region ## Dispose ##
        public void Dispose()
        {
            IsDisposed = true;

            //WDwirte
            if (WriteTimer != null)
            {
                WriteTimer.Dispose();
            }

            //Device
            if (m_status_read_timer != null)
            {
                m_status_read_timer.Dispose();
                m_status_read_timer = null;
            }
            if (m_status_write_timer != null)
            {
                m_status_write_timer.Dispose();
                m_status_write_timer = null;
            }
            if (m_plccl != null)
            {
                m_plccl.Disconnect(true);
                m_plccl = null;
            }
        }
        #endregion

        #region ## 内部クラス ##
        public enum SendType { Read = 0, Write = 1, WriteAddrRead = 2 }
        public enum ConnectStatus { Disconnected = 0, Connected = 1 }
        #endregion  

    }
}
