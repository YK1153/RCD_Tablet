using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RcdOperation
{
    public static class Utils
    {
        public static bool In<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }

        public static bool In<T>(this T item, List<T> list)
        {
            return In(item, list.ToArray());
        }

        /// <summary>
        /// KeyValuePairがDefault(値未設定)か確認
        /// </summary>
        /// <returns>Default(値未設定)の場合 True</returns>
        public static bool IsDefault<T, TU>(this KeyValuePair<T, TU> pair)
        {
            return pair.Equals(new KeyValuePair<T, TU>());
        }
    }
}
