using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RcdTablet
{
    public static class Utils
    {
        public static bool In<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }

        public static void Clear(this Control.ControlCollection controls, bool dispose)
        {
            for (int ix = controls.Count - 1; ix >= 0; --ix)
            {
                if (dispose) controls[ix].Dispose();
                else controls.RemoveAt(ix);
            }
        }

        /// <summary>
        /// 指定コントロールをDoubleBufferに設定する
        /// </summary>
        internal static void SetDoubleBuffer<T>(T targetControl) where T : Control
        {
            typeof(T).InvokeMember("DoubleBuffered",
                   BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                   null, targetControl, new object[] { true });
        }

    }
}
