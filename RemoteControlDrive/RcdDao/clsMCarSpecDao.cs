using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using RcdCmn;
using static RcdDao.DaoCommon;
using System.ComponentModel;
using RcdDao.Attributes;

namespace RcdDao
{
    public class MCarSpecDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        /// <summary>
        /// 車両仕様マスタ情報取得
        /// </summary>
        /// <returns></returns>
        public List<MCarSpec> GetAllMCarSpecInfo()
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
                            , SpecCd
                            , ControlFlg
                            , CarWidth
                            , FrontOverhang
                            , Wheelbase
                            , RearOverhang
                            , Tread
                            , StabilityFactor
                            , AngleMax
                            , GearRatio
                            , ElectronicPlatform
                        FROM
                            dbo.M_CAR_SPEC
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MCarSpec> list = m_daoCommon.ConvertToListOf<MCarSpec>(dataTable);

                    return m_daoCommon.ConvertToListOf<MCarSpec>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Add(MCarSpec carSpec)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        INSERT dbo.M_CAR_SPEC (
                              Created
                            , Updated
                            , DelFlg
                            , PlantSID
                            , SpecCd
                            , ControlFlg
                            , CarWidth
                            , FrontOverhang
                            , Wheelbase
                            , RearOverhang
                            , Tread
                            , StabilityFactor
                            , AngleMax
                            , GearRatio
                            , ElectronicPlatform
                        ) VALUES (
                              GETDATE()
                            , GETDATE()
                            , 0
                            , @PlantSID
                            , @SpecCd
                            , @ControlFlg
                            , @CarWidth
                            , @FrontOverhang
                            , @Wheelbase
                            , @RearOverhang
                            , @Tread
                            , @StabilityFactor
                            , @AngleMax
                            , @GearRatio
                            , @ElectronicPlatform
                        );
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, carSpec);
                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Delete(MCarSpec carSpec)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            M_CAR_SPEC
                        SET
                            Updated = GETDATE()
                            , DelFlg = 1
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, carSpec);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Update(MCarSpec carSpec)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            M_CAR_SPEC
                        SET
                            Updated = GETDATE()
                            , PlantSID = @PlantSID
                            , SpecCd = @SpecCd
                            , ControlFlg = @ControlFlg
                            , CarWidth = @CarWidth
                            , FrontOverhang = @FrontOverhang
                            , Wheelbase = @Wheelbase
                            , RearOverhang = @RearOverhang
                            , Tread = @Tread
                            , StabilityFactor = @StabilityFactor
                            , AngleMax = @AngleMax
                            , GearRatio = @GearRatio
                            , ElectronicPlatform = @ElectronicPlatform
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, carSpec);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public class MCarSpec : Result
        {
            public MCarSpec() { }

            public int? SID { get; set; }

            /// <summary>
            /// 工場SID
            /// <summary>
            public int PlantSID { get; set; }
            /// <summary>
            /// 車両仕様番号
            /// <summary>
            public string SpecCd { get; set; }
            /// <summary>
            /// 自走可否
            /// <summary>
            public bool ControlFlg { get; set; }
            /// <summary>
            /// 全幅
            /// <summary>
            public float CarWidth { get; set; }
            /// <summary>
            /// フロントオーバーハング
            /// <summary>
            public float FrontOverhang { get; set; }
            /// <summary>
            /// ホイールベース
            /// <summary>
            public float Wheelbase { get; set; }
            /// <summary>
            /// リアオーバーハング
            /// <summary>
            public float RearOverhang { get; set; }
            /// <summary>
            /// トレッド
            /// <summary>
            public float Tread { get; set; }
            /// <summary>
            /// スタビリティファクタ
            /// <summary>
            public float StabilityFactor { get; set; }
            /// <summary>
            /// 最大舵角
            /// <summary>
            public float AngleMax { get; set; }
            /// <summary>
            /// ギア比
            /// <summary>
            public float GearRatio { get; set; }
            /// <summary>
            /// 電子プラットフォーム
            /// <summary>
            public float ElectronicPlatform { get; set; }
        }

    }
}
