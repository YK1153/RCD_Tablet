using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using RcdCmn;
using static RcdDao.DaoCommon;

namespace RcdDao
{
    public class DErrorStatus
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        /// <summary>
        /// 異常検知情報を取得
        /// </summary>
        /// <returns>異常検知リスト</returns>
        public List<ErrorStatus> GetAllErrorStatus()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        WITH W_GROUP_ERR_ORDERED AS
                            (SELECT 
                                ID
                                ,GroupSID
                                ,CtrlUnitSID
                                ,CarNo
                                ,Status
                                ,StatusMsg
                                ,Updated
                                ,ROW_NUMBER() OVER (
                                    PARTITION BY 
                                        GroupSID 
                                    ORDER BY 
                                        Updated Desc) 
                                AS SEQ
                            FROM 
                                D_ERROR_STATUS
                            WHERE
                                DelFlg = 0)

                        SELECT 
                            err.ID              AS ID
                            , grp.ID            AS GroupID
                            , grp.Name          AS GroupName
                            , ctrl.ID           AS CtrlUnitID
                            , ctrl.UnitName     AS CtrlUnitName
                            , err.CarNo         AS CarNo
                            , err.Status        AS Status
                            , err.StatusMsg     AS StatusMsg
                        FROM 
                            W_GROUP_ERR_ORDERED err
                        INNER JOIN
                            M_GROUP grp
                        ON 
                            err.GroupSID = grp.sid
                            AND grp.DelFlg = 0
                        LEFT OUTER JOIN
                            M_CONTROL_UNITS ctrl
                        ON
                            err.CtrlUnitSID = ctrl.sid
                            AND ctrl.DelFlg = 0
                        WHERE 
                            SEQ = 1
                        ORDER BY 
                            grp.ID Asc;
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
        public bool HasErrors()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        SELECT 
                            TOP 1 err.ID
                        FROM
                            D_ERROR_STATUS err
                        INNER JOIN
                            M_GROUP grp
                        ON
                            err.GroupSID = grp.sid
                            AND grp.DelFlg = 0
                        WHERE
                            err.DelFlg = 0;
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

        public List<ErrorStatus> errorCtrlUnitSID()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        SELECT 
                            err.ID
                        FROM
                            D_CTRL_UNIT_STATUS err
                        WHERE
                            err.DelFlg = 0;
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
        /// 異常検知データを更新・追加
        /// </summary>
        /// <param name="errorStatusList"></param>
        /// <returns></returns>
        public int UpdateErrorStatus(List<ErrorStatus> errorStatusList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    SqlParameter[] sqlParameters = m_daoCommon.ConvertListToSqlParams(errorStatusList);

                    StringBuilder query = new StringBuilder();
                    query.AppendLine(@"MERGE INTO ");
                    query.AppendLine(@"        dbo.D_ERROR_STATUS as Target ");
                    query.AppendLine(@"    USING (VALUES ");
                    for (int i = 0; i < errorStatusList.Count; i++)
                    {
                        ErrorStatus stat = errorStatusList[i];
                        string row = $"(@GroupSID{i.ToString()}, @CtrlUnitSID{i.ToString()}, @CarNo{i.ToString()}, @Status{i.ToString()}, @StatusMsg{i.ToString()})";
                        if (i < errorStatusList.Count - 1)
                        {
                            row += ", ";
                        }
                        query.AppendLine(row);
                    }
                    query.AppendLine(@"          ) as Source (GroupSID, CtrlUnitSID, CarNo, Status, StatusMsg) ");
                    query.AppendLine(@"    ON ");
                    query.AppendLine(@"        (Target.GroupSID = Source.GroupSID ");
                    query.AppendLine(@"         AND Target.CtrlUnitSID = Source.CtrlUnitSID ");
                    query.AppendLine(@"         AND Target.DelFlg = 0) ");
                    query.AppendLine(@"    WHEN MATCHED THEN ");
                    query.AppendLine(@"        UPDATE SET ");
                    query.AppendLine(@"            Target.Updated = GETDATE(), ");
                    query.AppendLine(@"            Target.GroupSID = Source.GroupSID, ");
                    query.AppendLine(@"            Target.CtrlUnitSID = Source.CtrlUnitSID, ");
                    query.AppendLine(@"            Target.CarNo = Source.CarNo, ");
                    query.AppendLine(@"            Target.Status = Source.Status, ");
                    query.AppendLine(@"            Target.StatusMsg = Source.StatusMsg ");
                    query.AppendLine(@"    WHEN NOT MATCHED BY Target THEN ");
                    query.AppendLine(@"        INSERT( Created, ");
                    query.AppendLine(@"                Updated, ");
                    query.AppendLine(@"                GroupSID, ");
                    query.AppendLine(@"                CtrlUnitSID, ");
                    query.AppendLine(@"                CarNo, ");
                    query.AppendLine(@"                Status, ");
                    query.AppendLine(@"                StatusMsg) ");
                    query.AppendLine(@"        VALUES( GETDATE(), ");
                    query.AppendLine(@"                GETDATE(), ");
                    query.AppendLine(@"                Source.GroupSID, ");
                    query.AppendLine(@"                Source.CtrlUnitSID, ");
                    query.AppendLine(@"                Source.CarNo, ");
                    query.AppendLine(@"                Source.Status, ");
                    query.AppendLine(@"                Source.StatusMsg) ");
                    query.AppendLine(@"    WHEN NOT MATCHED BY Source THEN ");
                    query.AppendLine(@"        DELETE; ");

                    int effectedRowCount = helper.ExecuteNonQuery(query.ToString(), CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 異常検知データをすべて削除
        /// </summary>
        /// <returns></returns>
        public int RemoveErrorStatus()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        DELETE FROM D_ERROR_STATUS
                        ";

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text);

                    return effectedRowCount;
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

            public int? ID { get; set; }

            public int? GroupSID { get; set; }

            public string GroupID { get; set; }

            public string GroupName { get; set; }

            public int? CtrlUnitSID { get; set; }

            public string CtrlUnitID { get; set; }

            public string CtrlUnitName { get; set; }

            public string CarNo { get; set; }

            public string Status { get; set; }

            public string StatusMsg { get; set; }

        }

    }
}
