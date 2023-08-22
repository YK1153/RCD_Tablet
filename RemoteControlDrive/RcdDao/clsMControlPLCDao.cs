using RcdCmn;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RcdDao.DaoCommon;

namespace RcdDao
{
    public class MControlPLCDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion


        /// <summary>
        /// PLC条件情報取得
        /// </summary>
        /// <returns></returns>
        public List<MControlPlc> GetAllMControlPlcInfo()
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
                            , GroupNo
                            , SerialNumber
                            , PLCDeviceID
                            , ReadAddr
                            , ReadBit
                            , Value
                            , BindingCondition
                        FROM
                            dbo.M_CONTROL_PLC
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MControlPlc> list = m_daoCommon.ConvertToListOf<MControlPlc>(dataTable);

                    return m_daoCommon.ConvertToListOf<MControlPlc>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Add(MControlPlc controlPlc)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        INSERT dbo.M_CONTROL_PLC (
                              Created
                            , Updated
                            , DelFlg
                            , PlantSID
                            , GroupNo
                            , SerialNumber
                            , PLCDeviceID
                            , ReadAddr
                            , ReadBit
                            , Value
                            , BindingCondition
                        ) VALUES (
                              GETDATE()
                            , GETDATE()
                            , 0
                            , @PlantSID
                            , @GroupNo
                            , @SerialNumber
                            , @PLCDeviceID
                            , @ReadAddr
                            , @ReadBit
                            , @Value
                            , @BindingCondition
                        );
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, controlPlc);
                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Delete(MControlPlc controlPlc)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            M_CONTROL_PLC
                        SET
                            Updated = GETDATE()
                            , DelFlg = 1
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, controlPlc);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Update(MControlPlc controlPlc)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            M_CONTROL_PLC
                        SET
                            Updated = GETDATE()
                            , PlantSID = @PlantSID
                            , GroupNo = @GroupNo
                            , SerialNumber = @SerialNumber
                            , PLCDeviceID = @PLCDeviceID
                            , ReadAddr = @ReadAddr
                            , ReadBit = @ReadBit
                            , Value = @Value
                            , BindingCondition = @BindingCondition
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, controlPlc);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public class MControlPlc : Result
        {
            public MControlPlc() { }

            public int? SID { get; set; }

            /// <summary>
            /// 工場SID
            /// <summary>
            public int PlantSID { get; set; }
            /// <summary>
            /// グループNo
            /// <summary>
            public int GroupNo { get; set; }
            /// <summary>
            /// 番号
            /// <summary>
            public int SerialNumber { get; set; }
            /// <summary>
            /// PLCデバイスID
            /// <summary>
            public int PLCDeviceID { get; set; }
            /// <summary>
            /// アドレス番号
            /// <summary>
            public int ReadAddr { get; set; }
            /// <summary>
            /// ビット番号
            /// <summary>
            public int ReadBit { get; set; }
            /// <summary>
            /// 接点値
            /// <summary>
            public bool Value { get; set; }
            /// <summary>
            /// 結合条件
            /// <summary>
            public int BindingCondition { get; set; }
        }

    }
}
