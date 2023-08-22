using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;

namespace RcdManagement
{
    static class Program
    {
        private static string m_mode = "";
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (m_mode == "")
            {
                string assemblyName = Assembly.GetExecutingAssembly().FullName;
                Mutex app_mutex = new Mutex(false, assemblyName);
                if (app_mutex.WaitOne(0, false) == false)
                {
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                new RcdManagement();
                Application.Run();
            }
            else if (m_mode == "1") {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TestForm());
            }
            else if (m_mode == "2")
            {
                try
                {
                    // 検査情報システム 接続テスト用
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Test1());
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }
    }
}
