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
    public class MStationDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        #region ### データ操作 ###
        /// <summary>
        /// 工程情報取得
        /// </summary>
        /// <returns>工程リスト</returns>
        public List<Station> GetAllInfo(int plantsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT
		                    SID
		                    , PlantID
		                    , StationNo
	                    FROM
		                    dbo.M_STATION
	                    WHERE
		                    DelFlg = 0;
                            And PlantSID = '{plantsid}'
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    return m_daoCommon.ConvertToListOf<Station>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }
        /// <summary>
        /// 工程情報取得（工場ごと）
        /// </summary>
        /// <returns>工程リスト</returns>
        public List<Station> GetSelectInfo(int plantsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT
		                    SID
		                    , StationNo
                            , Name
                            , Image
                            , XaxisMIN
                            , XaxisMAX
                            , YaxisMIN
                            , YaxisMAX
	                    FROM
		                    dbo.M_STATION
	                    WHERE
                            DelFlg = 0
		                And PlantSID = '{plantsid}'
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<Station> dataList = m_daoCommon.ConvertToListOf<Station>(dataTable);

                    return dataList;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Add(Station process)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        INSERT M_STATION (
                            Created
                            , Updated
                            , DelFlg
                            , PlantID
                            , StationNp
                            , Image
                            , XaxisMIN
                            , XaxisMAX
                            , YaxisMIN
                            , YaxisMIN
                        ) VALUES (
                            GETDATE()
                            , GETDATE()
                            , 0
                            , @PlantID
                            , @StationNo
                            , @Image
                            , @XaxisMIN
                            , @XaxisMAX
                            , @YaxisMIN
                            , @YaxisMAX
                        );
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, process);
                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Delete(Station process)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE 
                            M_STATION 
                        SET
                            Updated = GETDATE()
                            , DelFlg = 1
                        WHERE
                            SID = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, process);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Update(Station process)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE 
                            M_STATION
                        SET
                            Updated = GETDATE()
                            , PlantID = @PlantID
                            , StationNo = @StationNo

                        WHERE
                            SID = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, process);

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
        public class Station : Result
        {
            public Station() { }

            [SqlParam(queryNames: new string[] { "Delete", "Update" })]
            public int? SID { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            [Pattern("[0-9]{1,4}", "1~4桁の数字")]
            public int PlantID { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            [Pattern("[0-9]{1,4}", "1~4桁の数字")]
            public int StationNo { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            public string Name { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            public string Image { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            [Pattern("[0-9]{1,8}", "1~8桁の数字")]
            public float XaxisMIN { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            [Pattern("[0-9]{1,8}", "1~8桁の数字")]
            public float IaxisMAX { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            [Pattern("[0-9]{1,8}", "1~8桁の数字")]
            public float YaxisMIN { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            [Pattern("[0-9]{1,8}", "1~8桁の数字")]
            public float YaxisMAX { get; set; }

        }

        public class ImageInfo : Result
        {
            public ImageInfo() { }

            [SqlParam(queryNames: new string[] { "Delete", "Update" })]
            public int? SID { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            [Pattern("[0-9]{1,4}", "1~4桁の数字")]
            public string PlantID { get; set; }

            [UserInput]
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            [Pattern("[0-9]{1,3}", "1~3桁の数字")]
            public string StationNo { get; set; }
        }


        #endregion

    }
}
