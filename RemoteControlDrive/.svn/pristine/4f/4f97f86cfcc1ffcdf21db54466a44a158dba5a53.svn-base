using System;
using System.Reflection;

namespace RcdDao.Attributes
{
    internal class SqlParamAttribute : Attribute
    {
        private string[] queryNames;

        public SqlParamAttribute(string[] queryNames)
        {
            this.queryNames = queryNames;
        }

        /// <summary>
        /// SqlParamAttributesの設定値取得
        /// </summary>
        /// <param name="prop">取得対象プロパティ</param>
        /// <returns>SqlParamAttributesの設定値行列、設定されていない場合は空の行列</returns>
        public static string[] GetSqlParamAttributes(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                SqlParamAttribute sqlParamAttr = attr as SqlParamAttribute;
                if (sqlParamAttr != null)
                {
                    return sqlParamAttr.queryNames;
                }
            }
            return new string[0];
        }
    }
}