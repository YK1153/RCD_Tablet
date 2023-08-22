using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RcdCmn;
using static RcdDao.DaoCommon;
using System.Reflection;
using System.Data;


namespace RcdDao
{
    public class DEmergencyDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        /// <summary>
        /// 異常検知情報を取得
        /// </summary>
        /// <returns>異常検知リスト</returns>
        public List<ErrorStatus> GetAllErrorStatus(int plantsid, int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT
                            emerg.SID
                            ,emerg.PlantSID
                            ,emerg.StationSID
                            ,emerg.SolvedFlg
                            ,emerg.StartTime
                            ,emerg.EndTime
                            ,emerg.BodyNo
                            ,emerg.CamID
                            ,emerg.StatusMsg
                            ,emerg.ErrCode
                        FROM
                            D_EMERGENCY emerg
                        WHERE
                            emerg.PlantSID = '{plantsid}'
                        AND                        
                            emerg.StationSID = '{stationsid}'
                        ;
                        ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<ErrorStatus> list = m_daoCommon.ConvertToListOf<ErrorStatus>(dataTable);

                    return list;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 処置前異常情報を取得
        /// </summary>
        /// <returns>異常検知リスト</returns>
        public List<ErrorStatus> GetAllErrorStatus_noSolve(int plantsid, int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT
                            emerg.SID
                            ,emerg.PlantSID
                            ,emerg.StationSID
                            ,emerg.SolvedFlg
                            ,emerg.StartTime
                            ,emerg.EndTime
                            ,emerg.BodyNo
                            ,emerg.CamID
                            ,emerg.StatusMsg
                            ,emerg.ErrCode
                        FROM
                            D_EMERGENCY emerg
                        WHERE
                            emerg.PlantSID = '{plantsid}'
                        AND                        
                            emerg.StationSID = '{stationsid}'
                        AND 
                            emerg.SolvedFlg = 0
                        AND
                            emerg.ErrCode = 00000
                        ;
                        ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<ErrorStatus> list = m_daoCommon.ConvertToListOf<ErrorStatus>(dataTable);

                    return list;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 処置前異常情報を取得
        /// </summary>
        /// <returns>異常検知リスト</returns>
        public List<ErrorStatus> GetAllErrorStatus_Solve(int plantsid, int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT
                            emerg.SID
                            ,emerg.PlantSID
                            ,emerg.StationSID
                            ,emerg.SolvedFlg
                            ,emerg.StartTime
                            ,emerg.EndTime
                            ,emerg.BodyNo
                            ,emerg.CamID
                            ,emerg.StatusMsg
                            ,emerg.ErrCode
                        FROM
                            D_EMERGENCY emerg
                        WHERE
                            emerg.PlantSID = '{plantsid}'
                        AND                        
                            emerg.StationSID = '{stationsid}'
                        AND 
                            emerg.SolvedFlg = 0
                        ;
                        ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<ErrorStatus> list = m_daoCommon.ConvertToListOf<ErrorStatus>(dataTable);

                    return list;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }


        /// <summary>
        /// 異常検知有無取得
        /// </summary>
        /// <returns>異常ステータス(走行停止)が存在する場合True</returns>
        public bool HasErrors(int plantsid, int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = $@"
                        SELECT 
                            TOP 1 emerg.SID
                        FROM
                            D_EMERGENCY emerg
                        WHERE
                            emerg.PlantSID = '{plantsid}'
                        AND
                            emerg.StationSID = '{stationsid}'
                        AND 
                            emerg.DelFlg = 0
                        AND 
                            emerg.SolvedFlg = 0
                        AND
                            emerg.ErrCode = 00000
                    ;
                    ";

                    DataTable result = helper.Execute(cmd, CommandType.Text);

                    return result.Rows.Count >= 1;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 車両仕様情報結果セット
        /// </summary>
        public class ErrorStatus : Result
        {
            public ErrorStatus() { }

            public int? SID { get; set; }

            public int? PlantSID { get; set; }

            public int StationSID { get; set; }

            public bool SolvedFlg { get; set; }

            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public string BodyNo { get; set; }

            public string CamID { get; set; }

            public string StatusMsg { get; set; }

            public string ErrCode { get; set; }

        }
    }
}
