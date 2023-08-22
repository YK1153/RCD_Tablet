using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RcdOperation.Control.CommDetect;
using System.Windows.Forms;
using RcdCmn;
using System.Threading;
using RcdOperationSystemConst;

namespace RcdOperation.Hazard
{
    public class HazardConfirn
    {
        #region ## 変数 ##

        private ManualResetEvent m_evFinishWait;

        /// <summary>
        /// 読みとり間隔
        /// </summary>
        private int m_timerspan = 100;
        private System.Threading.Timer m_readTimer;
        /// <summary>
        /// タイムアウト計測用ストップウォッチ
        /// </summary>
        private Stopwatch m_stopwatch;

        /// <summary>
        /// 処理開始確認フラグ
        /// </summary>
        private bool m_runstart = false;

        private string m_WritePath;
        private string m_ReadPath;
        private int m_RunTimeout;
        private int m_ResultTimeout;

        private string m_BodyNo;
        private string m_SpecCd;
        private string m_CamIP;
        private string m_CamNo;

        #endregion

        #region ## プロパティ ##

        /// <summary>
        /// 取得結果
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// エラーコード
        /// 処理結果が失敗だった際に理由が取得できる
        /// </summary>
        public string ErrCode { get; private set; }

        #endregion

        #region ## コンストラクタ ##

        public HazardConfirn(string writepath, string readpath)
        {
            m_WritePath = writepath;
            m_ReadPath = readpath;

            m_RunTimeout = OperationConst.C_HAZARD_RUN_TIMEOUT;
            m_ResultTimeout = OperationConst.C_HAZARD_RESULT_TIMEOUT;

            m_evFinishWait = new ManualResetEvent(false);

            m_stopwatch = new Stopwatch();
            Result = false;
        }

        #endregion

        public bool Start(string bodyno, string speccd, string camip, string camno)
        {
            m_evFinishWait.Reset();

            m_BodyNo = bodyno;
            m_SpecCd = speccd;
            m_CamIP = camip;
            m_CamNo = int.Parse(camno).ToString();

            // 処理開始
            StartWrite();

            // 終了待機
            if (m_evFinishWait.WaitOne(OperationConst.C_SYSTEM_MAX_TIMEOUT))
            {
                // 処理終了、処理結果はResult参照


            }
            else
            {
                // タイムアウト、異常終了

                Result = false;
                ErrCode = Res.ErrorStatus.HAZARDCONFIRN_PROCTIMEOUT.Code;
            }

            return Result;
        }

        public void StartWrite()
        {
            try
            {
                PanaWrite panaWrite = new PanaWrite()
                {
                    Permition_For_Detection = true,
                    BodyNo = m_BodyNo,
                    CarType = m_SpecCd,
                    CamIP = m_CamIP,
                    CamNo = m_CamNo,
                };

                CommonProc.WriteFile(m_WritePath, panaWrite);

                m_readTimer = new System.Threading.Timer(Callback_ReadFile, null, m_timerspan, m_timerspan);
                // 時間計測開始
                m_stopwatch.Restart();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (m_readTimer != null)
                {
                    m_readTimer.Dispose();
                    m_readTimer = null;
                }
            }
        }

        private void Callback_ReadFile(object sender)
        {
            if (m_readTimer != null)
            {
                try
                {
                    // タイマーストップ
                    m_readTimer.Change(Timeout.Infinite, Timeout.Infinite);

                    DetectionWrite tMCWrite = new DetectionWrite();
                    // ファイル読み込み
                    CommonProc.ReadFile(m_ReadPath, ref tMCWrite);

                    // 動き出しを確認する
                    if (!m_runstart)
                    {
                        // 動き出し前
                        if (tMCWrite.Detection_run_condition)
                        {
                            // 動きだし確認OK
                            m_runstart = true;
                            m_stopwatch.Restart();
                        }
                        else
                        {
                            // 未動き出し
                            // 経過時間を確認
                            if (m_stopwatch.ElapsedMilliseconds > m_RunTimeout)
                            {
                                //Invoke(new Action(() => { MessageBox.Show($"Processing is terminated due to startup wait timeout.", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning); }));
                                Result = false;
                                ErrCode = Res.ErrorStatus.HAZARDCONFIRN_RUNTIMEOUT.Code;
                                EndProc();
                                return;
                            }
                        }
                    }

                    // 動き出後の結果確認
                    if (m_runstart)
                    {
                        bool result;
                        // 結果が入ったことと処理の終了を確認
                        if (bool.TryParse(tMCWrite.Result, out result) && !tMCWrite.Detection_run_condition)
                        {
                            // 結果を取得
                            //Invoke(new Action(() => { MessageBox.Show($"Acquisition of results completed.\nResult is '{bool.Parse(tMCWrite.Result)}'.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); }));
                            Result = result;
                            EndProc();
                            return;
                        }
                        else
                        // 結果の取得がまだのとき
                        {
                            // 経過時間を確認
                            if (m_stopwatch.ElapsedMilliseconds > m_ResultTimeout)
                            {
                                //Invoke(new Action(() => { MessageBox.Show($"Processing will be terminated due to timeout waiting for completion.", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning); }));
                                Result = false;
                                ErrCode = Res.ErrorStatus.HAZARDCONFIRN_RESULTTIMEOUT.Code;
                                EndProc();
                                return;
                            }

                        }

                    }

                    // タイマー再開
                    m_readTimer.Change(m_timerspan, m_timerspan);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (m_readTimer != null)
                    {
                        m_readTimer.Dispose();
                        m_readTimer = null;
                    }
                    //FormStatusChange(true);
                    m_stopwatch.Stop();
                }
            }
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        private void EndProc()
        {
            m_runstart = false;

            m_readTimer.Dispose();
            m_readTimer = null;

            // ファイル内容を戻して終了
            PanaWrite panaWrite = new PanaWrite();
            panaWrite.SetInitialValue();
            CommonProc.WriteFile(m_WritePath, panaWrite);

            m_stopwatch.Stop();

            m_evFinishWait.Set();
        }


    }
}
