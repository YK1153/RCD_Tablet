using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RcdCmn
{
    [Serializable()]
    public class UserException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UserException() : base() { }

        public UserException(string message) : base(message) { }

        public UserException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// 逆シリアル化コンストラクタ
        /// </summary>
        protected UserException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class ExceptionProcess
    {
        public static void UserExceptionProcess(Exception uex)
        {
            AppLog.GetInstance().Error(uex.Message);
            AppLog.GetInstance().Error(uex.StackTrace);
        }

        public static void ComnExceptionProcess(Exception ex)
        {
            AppLog.GetInstance().Error(ex.Message);
            AppLog.GetInstance().Error(ex.StackTrace);
        }

        public static void UserExceptionConsoleProcess(Exception uex)
        {
            AppLog.GetInstance().Error(uex.Message);
            AppLog.GetInstance().Error(uex.StackTrace);
        }

        public static void ComnExceptionConsoleProcess(Exception ex)
        {
            AppLog.GetInstance().Error(ex.Message);
            AppLog.GetInstance().Error(ex.StackTrace);
        }
    }
}
