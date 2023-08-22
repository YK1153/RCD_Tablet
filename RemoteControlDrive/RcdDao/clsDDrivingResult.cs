using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System;
using RcdCmn;
using static RcdDao.DaoCommon;

namespace RcdDao
{
    public class DDrivingResultDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        /// <summary>
        /// 異常検知情報を取得
        /// </summary>
        /// <returns>異常検知リスト</returns>
        public List<ResultList> GetAllResultList(int plantsid,int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT 
                            drire.StartTime
                            , drire.EndTime
                            , drire.StationSID
                            , drire. BodyNo
                            , drire.Result
                            , drire.StatusMsg
                        FROM 
                            D_DRIVING_RESULT drire
                        Where 
                            drire.PlantSID = '{plantsid}'
                        And 
                            drire.StationSID = '{stationsid}'
                        ORDER BY 
                            drire.StartTime DESC;
                        ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<ResultList> list = m_daoCommon.ConvertToListOf<ResultList>(dataTable);

                    return list;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public List<ResultList> GetErrorResultList(int plantsid, int stationsid ,int wantresult)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT 
                            drire.StartTime
                            , drire.EndTime
                            , drire.StationSID
                            , drire. BodyNo
                            , drire.Result
                            , drire.StatusMsg
                        FROM 
                            D_DRIVING_RESULT drire                        
                        WHERE 
                            drire.PlantSID = '{plantsid}'
                        AND 
                            drire.StationSID = '{stationsid}'
                        AND
                            drire.Result = '{wantresult}'
                        ORDER BY 
                            drire.StartTime DESC;
                        ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<ResultList> list = m_daoCommon.ConvertToListOf<ResultList>(dataTable);

                    return list;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        //public List<ResultList> GetSuccessResultList()
        //{
        //    LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
        //    try
        //    {
        //        using (SqlHelper helper = m_daoCommon.getSqlHelper())
        //        {
        //            string cmd = @"
        //                SELECT 
        //                    ercr.Created
        //                    , ercr.CarNo
        //                    , ercr.CtrlUnitSID 
        //                    , ercr.result
        //                    , ercr.StatusMsg
        //                FROM 
        //                    D_ERROR_CAR_LOG ercr
        //                WHERE
        //                    ercr.result = 0
        //                ORDER BY ercr.Created DESC;
        //                ";

        //            DataTable dataTable = helper.Execute(cmd, CommandType.Text);

        //            List<ResultList> list = m_daoCommon.ConvertToListOf<ResultList>(dataTable);

        //            return list;
        //        }
        //    }
        //    finally
        //    {
        //        LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
        //    }
        //}

        public List<ResultList> GetSortResultList(string WhereStrig, string OrderColumnName, string OrderMode)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT 
                            drire.Created
                            , drire.CarNo
                            , drire.CtrlUnitSID 
                            , drire.result
                            , sysme.StatusMsg
                        FROM 
                            D_DRIVING_RESULT drire
                        LEFT JOIN
                            M_SYSTEM_MESSAGE sysme
                        WHERE
                            {WhereStrig}
                        ORDER BY ercr.{OrderColumnName} {OrderMode};
                        ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<ResultList> list = m_daoCommon.ConvertToListOf<ResultList>(dataTable);

                    return list;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public void InsertResultList(List<ResultList> resultListList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    SqlParameter[] sqlParameters = m_daoCommon.ConvertListToSqlParams(resultListList);
                    StringBuilder query = new StringBuilder();

                    query.AppendLine(@"INSERT INTO [dbo].[D_ERROR_CAR_LOG] ");
                    query.AppendLine(@"           ([Created] ");
                    query.AppendLine(@"           ,[Updated] ");
                    query.AppendLine(@"           ,[CarNo] ");
                    query.AppendLine(@"           ,[CtrlUnitSID] ");
                    query.AppendLine(@"           ,[result] ");
                    query.AppendLine(@"           ,[StatusMsg]) ");
                    query.AppendLine(@"     VALUES ");
                    query.AppendLine(@"           (GETDATE() ");
                    query.AppendLine(@"           ,GETDATE() ");
                    for (int i = 0; i < resultListList.Count; i++)
                    {
                        ResultList stat = resultListList[i];
                        string row = $",@CarNo{i.ToString()}, @CtrlUnitSID{i.ToString()}, @result{i.ToString()}, @StatusMsg{i.ToString()}";
                        if (i < resultListList.Count - 1)
                        {
                            row += ", ";
                        }
                        query.AppendLine(row);
                    }
                    query.AppendLine(@"           ); ");

                    helper.ExecuteNonQuery(query.ToString(), CommandType.Text, sqlParameters);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        

        public void DeleteResultList(int date)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        DELETE FROM dbo.D_ERROR_CAR_LOG
                            WHERE Created < dateadd(day,{-date},getdate()) ;
                        ";

                    helper.Execute(cmd, CommandType.Text);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }


        /// <summary>
        /// 結果情報セット
        /// </summary>
        public class ResultList : Result
        {
            public ResultList() { }

            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public String BodyNo { get; set; }

            public int? StationSID { get; set; }

            public int result { get; set; }

            public String StatusMsg { get; set; }

        }

    }
}
