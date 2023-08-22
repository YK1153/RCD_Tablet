using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RcdCmn
{
    public class SqlHelper : IDisposable
    {
        private SqlConnection connection;
        private AppLog LOGGER = AppLog.GetInstance();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlHelper(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// DB接続
        /// </summary>
        public void openConnection()
        {
            connection.Open();
        }

        /// <summary>
        /// DBコネクションを切断
        /// </summary>
        public void closeConnection()
        {
            connection.Close();
        }

        /// <summary>
        /// 更新系　クエリ実行
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters">クエリパラメータ</param>
        /// <returns>影響行数</returns>
        public Int32 ExecuteNonQuery(String commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                {
                    connection.Open();
                }
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.CommandType = commandType;
                    cmd.Parameters.AddRange(parameters);
                    LogQuery(cmd);
                    return cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 参照系　クエリ実行
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters">クエリパラメータ</param>
        /// <returns>取得データテーブル</returns>
        public DataTable Execute(String commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                connection.Open();
            }

            using (SqlCommand cmd = new SqlCommand(commandText, connection))
            {
                cmd.CommandType = commandType;
                cmd.Parameters.AddRange(parameters);

                LogQuery(cmd);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable(string.Empty))
                    {
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        /// <summary>
        /// 実行クエリをログに記録
        /// </summary>
        /// <param name="cmd">実行予定クエリコマンド</param>
        private void LogQuery(SqlCommand cmd)
        {
            LOGGER.Debug($"[Query Execute] command text: {EraseCommandWhiteSpace(cmd.CommandText)} ");

            if (cmd.Parameters != null && cmd.Parameters.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"[Query Execute] parameters: (");

                foreach (SqlParameter parameter in cmd.Parameters)
                {
                    if ((parameter.Direction == ParameterDirection.Output) || (parameter.Direction == ParameterDirection.ReturnValue))
                    {
                        continue;
                    }
                    sb.Append($"{parameter.ParameterName}: {parameter.Value}, ");
                }
                sb.Append($" ) ");

                LOGGER.Debug(sb.ToString());
            }
        }

        /// <summary>
        /// クエリ内の改行・タブを削除
        /// </summary>
        /// <param name="str">実行クエリstring</param>
        /// <returns></returns>
        private string EraseCommandWhiteSpace(string str)
        {
            string tabErased = str.Replace("    ", "");
            string noNewLine = Regex.Replace(tabErased, @"\t|\n|\r", " ");
            return noNewLine;
        }

        /// <summary>
        /// インスタンス廃棄処理
        /// </summary>
        void IDisposable.Dispose()
        {
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
    }
}
