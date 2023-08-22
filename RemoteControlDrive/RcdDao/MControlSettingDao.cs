﻿using RcdCmn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RcdDao.DaoCommon;

namespace RcdDao
{
    /// <summary>
    /// 工程の制御の動作設定
    /// ※ DBから取得する際にListで複数取得するようになっているが、レコードは1件の想定なので、Listで返す必要はない
    /// </summary>
    public class MControlSettingDao
    {

        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion


        /// <summary>
        /// 制御設定情報取得
        /// </summary>
        /// <returns></returns>
        public List<MControlSetting> GetAllMControlSettingInfo()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        SELECT
                              SID
                            , PlantSID
                            , StationSID
                            , ProductionGroupNo
                            , StationLogicalName
                            , GetInspectionInfo
                            , ReadyBootGroupNo
                            , GetYawrate
                            , ReadyOnGroupNo
                        FROM
                            dbo.M_CONTROL_SETTING
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MControlSetting> list = m_daoCommon.ConvertToListOf<MControlSetting>(dataTable);

                    return m_daoCommon.ConvertToListOf<MControlSetting>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Add(MControlSetting controlSetting)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        INSERT dbo.M_CONTROL_SETTING (
                              Created
                            , Updated
                            , DelFlg
                            , PlantSID
                            , StationSID
                            , ProductionGroupNo
                            , StationLogicalName
                            , GetInspectionInfo
                            , ReadyBootGroupNo
                            , GetYawrate
                            , ReadyOnGroupNo
                        ) VALUES (
                              GETDATE()
                            , GETDATE()
                            , 0
                            , @PlantSID
                            , @StationSID
                            , @ProductionGroupNo
                            , @StationLogicalName
                            , @GetInspectionInfo
                            , @ReadyBootGroupNo
                            , @GetYawrate
                            , @ReadyOnGroupNo
                        );
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, controlSetting);
                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Delete(MControlSetting controlSetting)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            M_CONTROL_SETTING
                        SET
                            Updated = GETDATE()
                            , DelFlg = 1
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, controlSetting);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Update(MControlSetting controlSetting)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            M_CONTROL_SETTING
                        SET
                            Updated = GETDATE()
                            , PlantSID = @PlantSID
                            , StationSID = @StationSID
                            , ProductionGroupNo = @ProductionGroupNo
                            , StationLogicalName = @StationLogicalName
                            , GetInspectionInfo = @GetInspectionInfo
                            , ReadyBootGroupNo = @ReadyBootGroupNo
                            , GetYawrate = @GetYawrate
                            , ReadyOnGroupNo = @ReadyOnGroupNo
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, controlSetting);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public class MControlSetting : Result
        {
            public MControlSetting() { }

            public int? SID { get; set; }

            /// <summary>
            /// 工場SID
            /// <summary>
            public int PlantSID { get; set; }
            /// <summary>
            /// 工程SID
            /// <summary>
            public int StationSID { get; set; }
            /// <summary>
            /// 生産指示取得グループNo
            /// <summary>
            public int ProductionGroupNo { get; set; }
            /// <summary>
            /// 工程論理名
            /// <summary>
            public string StationLogicalName { get; set; }
            /// <summary>
            /// 検査情報取得有無
            /// <summary>
            public bool GetInspectionInfo { get; set; }
            /// <summary>
            /// 準備起動判定グループNo
            /// <summary>
            public int ReadyBootGroupNo { get; set; }
            /// <summary>
            /// ヨーレート取得有無
            /// <summary>
            public bool GetYawrate { get; set; }
            /// <summary>
            /// 起動判定状態読み出し
            /// <summary>
            public int ReadyOnGroupNo { get; set; }
        }

    }
}
