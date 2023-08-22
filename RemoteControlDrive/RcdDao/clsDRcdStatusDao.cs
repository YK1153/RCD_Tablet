using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RcdDao.DaoCommon;
using RcdCmn;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace RcdDao
{
    public class DRcdStatusDao
    {

        #region ### クラス定数 ###


        #endregion

        #region ### config ###


        #endregion

        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        #region ### コンストラクター ###


        #endregion

        #region ### フォームイベント ###


        #endregion

        #region ### クラスイベント ###


        #endregion

        #region ### 状態表示更新処理 ###


        #endregion

        #region ### 処理 ###


        #endregion

        /// <summary>
        /// 車両仕様情報取得
        /// </summary>
        /// <returns>車両仕様リスト</returns>
        public List<DRcdStatus> GetMngMode(int plantSID, int stationSID)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT
                            SID
                            ,PreoarationStatus
                            ,ControlStatus
                            ,ContinueStatus
                            ,OriPosiStatus
                        FROM
                            dbo.D_RCD_STATUS
                        WHERE
                            DelFlg = 0
                        AND
                            PlantSID = '{plantSID}'
                        AND
                            StationSID = '{stationSID}'
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    if (dataTable.Rows.Count != 1)
                    {
                        throw new UserException("[DB構成不正] 管制モードテーブルのデータが1件でないです");
                    }

                    return m_daoCommon.ConvertToListOf<DRcdStatus>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }
        public class DRcdStatus : Result
        {
            public DRcdStatus() { }
            
            public int? SID { get; set; }

            public int PreoarationStatus { get; set; }

            public int ControlStatus { get; set; }

            public int ContinueStatus { get; set; }

            public int OriPosiStatus { get; set; }

        }

    }
}
