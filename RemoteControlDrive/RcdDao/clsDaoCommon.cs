using RcdCmn;
using RcdDao.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace RcdDao
{
    /// <summary>
    /// DAO共通処理
    /// </summary>
    public class DaoCommon
    {
        AppLog LOGGER = AppLog.GetInstance();

        private string m_ip;
        private string m_db_name;
        private string m_user;
        private string m_password;
        private const string C_INITIALIZE_WAIT_SECOND = "1";

        private static DaoCommon m_dao;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private DaoCommon(string ip, string name, string user, string pw)
        {
            m_ip = ip;
            m_db_name = name;
            m_user = user;
            m_password = pw;
        }

        public static bool Initialize(string ip, string name, string user, string pw)
        {
            string connString = $@"Data Source={ip};Integrated Security=False;Initial Catalog={name};User ID={user};Password={pw};Connection Timeout={C_INITIALIZE_WAIT_SECOND}";

            using (SqlConnection connection = new SqlConnection(connString))
            {
                try
                {
                   connection.Open();
                }
                catch (Exception e)
                {
                    AppLog.GetInstance().Error($"[DB接続不可]{e.Message}");
                    return false;
                }

                m_dao = new DaoCommon(ip, name, user, pw);
                return true;
            }
        }

        public static DaoCommon GetInstance()
        {
            if (m_dao == null)
            {
                AppLog.GetInstance().Error("DB Connection not initialized");
                throw new UserException("[データベース接続エラー] データベース接続初期化に失敗しました。");
            }
            else
            {
                return m_dao;
            }
        }

        private string GetConnectionString()
        {
            return $@"Data Source={m_ip};
                    Integrated Security=False;
                    Initial Catalog={m_db_name};
                    User ID={m_user};
                    Password={m_password}";
        }

        /// <summary>
        /// クエリ条件抽象クラス
        /// </summary>
        public abstract class Condition { }

        /// <summary>
        /// クエリ結果抽象クラス
        /// </summary>
        public abstract class Result
        {
            /// <summary>
            /// 基本データ整合性確認処理
            /// </summary>
            /// <param name="errorMsg">データ不正の場合のエラーメッセージ</param>
            /// <returns>Overrideされていない場合、デフォルトTrueを返す</returns>
            public virtual bool IsValid(ref string errorMsg)
            {
                return true;
            }
        }

        public abstract class Area : Result
        {
            public abstract Rectangle GetRec();
        }

        /// <summary>
        /// SQLHelper取得
        /// </summary>
        /// <returns></returns>
        public SqlHelper getSqlHelper()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                string connectionString = GetConnectionString();

                return new SqlHelper(connectionString);
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// Object PropertyをSQLパラメータ行列に変換
        /// </summary>
        /// <param name="obj">not null オブジェクト</param>
        /// <returns>パラメータリスト(パラメータ：[{@プロパティ名}, {プロパティ値}])</returns>
        public SqlParameter[] ConvertToSqlParams<T>(T obj)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                List<SqlParameter> paramList = new List<SqlParameter>();

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    paramList.Add(GetParam(obj, prop));
                }
                return paramList.ToArray();
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// sqlParameter属性:queryNameを持つObject PropertyをSQLパラメータ行列に変換
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="obj">not null オブジェクト</param>
        /// <returns>パラメータリスト(パラメータ：[{@プロパティ名}, {プロパティ値}])</returns>
        public SqlParameter[] ConvertToSqlParams<T>(string queryName, T obj)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                List<SqlParameter> paramList = new List<SqlParameter>();

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    string[] propQueryNames = SqlParamAttribute.GetSqlParamAttributes(prop);
                    if (propQueryNames.Contains(queryName))
                    {
                        paramList.Add(GetParam(obj, prop));
                    }

                }
                return paramList.ToArray();
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public SqlParameter[] ConvertListToSqlParams<T>(List<T> objList, string queryName = null)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                List<SqlParameter> paramList = new List<SqlParameter>();

                for (int i = 0; i < objList.Count; i++)
                {
                    object obj = objList[i];
                    foreach (PropertyInfo prop in typeof(T).GetProperties())
                    {
                        string[] propQueryNames = SqlParamAttribute.GetSqlParamAttributes(prop);
                        if ((queryName != null && propQueryNames.Contains(queryName)) || queryName == null)
                        {
                            paramList.Add(GetParam(obj, prop, prop.Name + i.ToString()));
                        }
                    }
                }
                return paramList.ToArray();
                
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// SQLパラメータインスタンスを取得
        /// </summary>
        /// <returns>SQLパラメータインスタンス</returns>
        private SqlParameter GetParam(object obj, PropertyInfo prop, string explicitName = null)
        {
            object value = prop.GetValue(obj) == null ? DBNull.Value : prop.GetValue(obj);
            if (explicitName != null)
            {
                return new SqlParameter("@" + explicitName, value);
            }
            else
            {
                return new SqlParameter("@" + prop.Name, value);
            }
        }

        /// <summary>
        /// データテーブルをObjectリストに変換
        /// </summary>
        /// <typeparam name="T">変換ターゲットObject型</typeparam>
        /// <param name="dt">データテーブル</param>
        /// <returns>変換リスト</returns>
        public List<T> ConvertToListOf<T>(DataTable dt) where T : Result
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                return dt.AsEnumerable()
                .Select(r => (T)ConvertTo<T>(r))
                .ToList();
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// データ行変換
        /// </summary>
        /// <typeparam name="T">クエリ結果(パラメータなしのコンストラクタが必要)</typeparam>
        /// <param name="r">データ行</param>
        /// <returns>変換結果、プロパティ名と一致するデータコラムがない場合Nullで設定</returns>
        private T ConvertTo<T>(DataRow r) where T : Result
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                var obj = Activator.CreateInstance(typeof(T));

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (r.Table.Columns.Contains(prop.Name))
                    {
                        var safeValue = r[prop.Name] == null || DBNull.Value.Equals(r[prop.Name]) ? null : Convert.ChangeType(r[prop.Name], propType);

                        prop.SetValue(obj, safeValue, null);
                    }
                }

                return (T)obj;
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public DataTable ConvertToDataTableOf<T>(List<T> list)
        {
            var properties = typeof(T).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(TableTypeColumnAttribute)))
                .ToList();
            var table = new DataTable();
            foreach (PropertyInfo prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (T item in list)
            {
                var row = table.NewRow();
                foreach (PropertyInfo prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
