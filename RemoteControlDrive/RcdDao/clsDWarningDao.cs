using RcdCmn;
using RcdDao.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RcdDao.DaoCommon;
using static RcdDao.DEmergencyDao;

namespace RcdDao
{
    public  class DWarningDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion


        #region ### データ操作 ###

        /// <summary>
        /// 異常検知テーブル情報取得
        /// </summary>
        /// <returns></returns>
        public List<DWarning> GetAllDWarningInfo(int plantsid, int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT
                              SID
                            , PlantSID
                            , StationSID
                            , SolvedFlg
                            , StartTime
                            , EndTime
                            , BodyNo
                            , CamID
                            , StatusMsg
                            , ErrCode
                        FROM
                            dbo.D_WARNING
                        WHERE
                            DelFlg=0
                        AND
                            PlantSID = {plantsid}
                        AND
                            StationSID = {stationsid}
                        AND
                            SolvedFlg = 0;
                    ";

                    //SqlParameter[] p = new SqlParameter[2];
                    //p[0] = new SqlParameter("@PlantSID", plantsid);
                    //p[1] = new SqlParameter("@StationSID", stationsid);

                    //DataTable dataTable = helper.Execute(cmd, CommandType.Text, p);
                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<DWarning> list = m_daoCommon.ConvertToListOf<DWarning>(dataTable);

                    return m_daoCommon.ConvertToListOf<DWarning>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Add(DWarning warning)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        INSERT dbo.D_WARNING (
                              Created
                            , Updated
                            , DelFlg
                            , PlantSID
                            , StationSID
                            , SolvedFlg
                            , StartTime
                            , EndTime
                            , BodyNo
                            , CamID
                            , StatusMsg
                            , ErrCode
                        ) VALUES (
                              GETDATE()
                            , GETDATE()
                            , 0
                            , @PlantSID
                            , @StationSID
                            , @SolvedFlg
                            , @StartTime
                            , @EndTime
                            , @BodyNo
                            , @CamID
                            , @StatusMsg
                            , @ErrCode
                        );
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, warning);
                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Delete(DWarning warning)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            D_WARNING
                        SET
                            Updated = GETDATE()
                            , DelFlg = 1
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, warning);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Update(DWarning warning)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            D_WARNING
                        SET
                            Updated = GETDATE()
                            , PlantSID = @PlantSID
                            , StationSID = @StationSID
                            , SolvedFlg = @SolvedFlg
                            , StartTime = @StartTime
                            , EndTime = @EndTime
                            , BodyNo = @BodyNo
                            , CamID = @CamID
                            , StatusMsg = @StatusMsg
                            , ErrCode = @ErrCode
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, warning);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Solved(DWarning dWarning)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            D_WARNING
                        SET
                              Updated = GETDATE()
                            , SolvedFlg = 1
                            , EndTime = @EndTime
                        WHERE
                            PlantSID = @PlantSID
                        AND
                            StationSID = @StationSID
                        AND
                            ErrCode = @ErrCode
                        AND
                            SolvedFlg = 0
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, dWarning);

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

        public class DWarning : Result
        {
            public DWarning() { }

            [SqlParam(queryNames: new string[] { "Delete", "Update" })]
            public int? SID { get; set; }

            /// <summary>
            /// 工場SID
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update", "Solved" })]
            public int PlantSID { get; set; }
            /// <summary>
            /// 工程SID
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update", "Solved" })]
            public int StationSID { get; set; }
            /// <summary>
            /// 解決フラグ
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update", "Solved" })]
            public bool SolvedFlg { get; set; }
            /// <summary>
            /// 発生時刻
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            public DateTime StartTime { get; set; }
            /// <summary>
            /// 解決時刻
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update", "Solved" })]
            public DateTime? EndTime { get; set; }
            /// <summary>
            /// ボデーNo
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            public string BodyNo { get; set; }
            /// <summary>
            /// カメラID
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            public string CamID { get; set; }
            /// <summary>
            /// 異常内容
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update" })]
            public string StatusMsg { get; set; }
            /// <summary>
            /// エラーコード
            /// <summary>
            [SqlParam(queryNames: new string[] { "Add", "Update", "Solved" })]
            public string ErrCode { get; set; }
        }


        #endregion

    }
}
