using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RcdCmn;

namespace CommWrapper
{
    public class MsgAnalyzer
    {
        private AppLog LOGGER = AppLog.GetInstance();
        private int m_startIndex = 0;
        private bool m_completeFlg = false;
        private string m_rcvMsg = null;

        public bool AnalyzeComplete
        {
            get { return m_completeFlg; }
        }

        public string RcvMessage
        {
            get { return m_rcvMsg; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MsgAnalyzer() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="accessBytes"></param>
        /// <param name="ms"></param>
        /// <returns>STX～ETXを受信するとtrueを返し、RcvMessageに受信したメッセージがセットされる。</returns>
        public bool Analyze(byte[] buffer, int accessBytes, ref System.IO.MemoryStream ms)
        {
            byte searchText = CommConst.STX;
            bool bRet = false;
            int i = 0;

            m_completeFlg = false;

            if (buffer[0] != CommConst.STX)
            {
                if (ms != null)
                {
                    searchText = CommConst.ETX;
                }
            }

            for (i = m_startIndex; i < accessBytes; i++)
            {
                switch (buffer[i])
                {
                    case CommConst.STX:   //STXを受信
                        {
                            string strDispose = null;
                            if (ms != null || m_startIndex != i)
                            {
                                //それ以前のデータを破棄
                                if (ms != null)
                                {
                                    strDispose = Encoding.ASCII.GetString(ms.ToArray());
                                    ms.Dispose();
                                    ms = null;
                                }
                                strDispose = strDispose + Encoding.ASCII.GetString(buffer, 0, i);
                                LOGGER.Warn($"不正なデータを破棄 (data={strDispose})");
                            }
                            m_startIndex = i + 1;
                            searchText = CommConst.ETX;
                        }
                        break;
                    case CommConst.ETX:   //ETXを受信
                        {
                            string strRcvMessage = null;
                            if (ms != null)
                            {
                                strRcvMessage = Encoding.ASCII.GetString(ms.ToArray());
                                ms.Dispose();
                                ms = null;
                            }
                            strRcvMessage = strRcvMessage + Encoding.ASCII.GetString(buffer, m_startIndex, i - m_startIndex);
                            m_startIndex = i + 1;
                            if (searchText != CommConst.ETX)
                            {
                                //予期せずETXを受信した場合はそれまでのデータを破棄
                                LOGGER.Warn($"不正なデータを破棄 (data={strRcvMessage})");
                                continue;
                            }
                            searchText = CommConst.STX;
                            bRet = true;
                            m_rcvMsg = strRcvMessage;
                            i = m_startIndex;
                            goto END_PROC;
                        }
                }
            }
        END_PROC:
            if (i == accessBytes)
            {
                m_completeFlg = true;
                if (searchText == CommConst.ETX)
                {
                    //ETXを受信しなかった場合はMemoryStreamに退避
                    if (ms == null)
                    {
                        ms = new System.IO.MemoryStream();
                    }
                    ms.Write(buffer, m_startIndex, accessBytes - m_startIndex);
                }
                else if (m_startIndex < accessBytes)
                {
                    // STXが含まれないまま読み取りが完了した場合は不正データとみなし破棄
                    string strDispose = Encoding.ASCII.GetString(buffer, m_startIndex, accessBytes - m_startIndex);
                    LOGGER.Warn($"不正なデータを破棄 (data={strDispose})");
                }
                m_startIndex = 0;
            }
            return bRet;
        }
    }
}