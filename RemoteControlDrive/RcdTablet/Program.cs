using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace RcdTablet
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = "AtrptManager";
            Mutex mutex = new Mutex(false, mutexName);

            bool hasHandle = false;
            try
            {
                try
                {
                    //ミューテックスの所有権を要求する
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (System.Threading.AbandonedMutexException)
                {
                    //別のアプリケーションがミューテックスを解放しないで終了した時
                    hasHandle = true;
                }
                //ミューテックスを得られたか
                if (hasHandle == false)
                {
                    //得られなかった場合終了
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new clsTablet());
                return;
            }
            finally
            {
                if (hasHandle)
                {
                    Application.Exit();
                    Environment.Exit(0);
                    //ミューテックスを解放する
                    mutex.ReleaseMutex();
                }
                mutex.Close();                
            }
        }
    }
}
