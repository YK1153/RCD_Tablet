using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using RcdCmn;
using static RcdDao.DaoCommon;
using RcdDao.Attributes;


namespace RcdDao
{
    public class MPlantDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        #region ### データ操作 ###
        /// <summary>
        /// 工場データ取得
        /// </summary>
        /// <param name="plantIdentifyid">工場識別ID</param>
        /// <returns></returns>
        public Plant GetAllInfo(string plantIdentifyid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT
		                      SID
		                    , Name
		                    , PlantCd
	                    FROM
		                    dbo.M_PLANT
	                    WHERE
		                    DelFlg = 0
                        And PlantCd = '{plantIdentifyid}';
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<Plant> plants = m_daoCommon.ConvertToListOf<Plant>(dataTable);

                    return plants.Single();
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Add(Plant plant)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        INSERT M_PLANT (
                            Created
                            , Updated
                            , DelFlg
                            , Name
                            , PlantCd
                        ) VALUES (
                            GETDATE()
                            , GETDATE()
                            , 0
                            , @Name
                            , @PlantCd
                            
                        );
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, plant);
                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Delete(Plant plant)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE 
                            M_PLANT 
                        SET
                            Updated = GETDATE()
                            , DelFlg = 1
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, plant);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Update(Plant plant)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE 
                            M_PLANT
                        SET
                            Updated = GETDATE()
                            , Name = @Name
                            , PlantCd = @PlantCd

                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, plant);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }
        #endregion

        #region ### 結果セット ###
        public class Plant : Result
        {
            public Plant() { }

            [SqlParam(queryNames: new string[] { "Delete", "Update" })]
            public int? SID { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            public string Name { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            public string PlantCd { get; set; }

        }

        #endregion

    }
}
