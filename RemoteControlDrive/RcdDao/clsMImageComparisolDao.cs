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
    public class MImageComparisolDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion


        #region ### データ操作 ###

        /// <summary>
        /// ローカライズアルゴ比較情報取得
        /// </summary>
        /// <returns></returns>
        public List<MImageComparisol> GetAllMImageComparisolInfo()
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
                            , CourseSID
                            , CamType
                            , DeviationX
                            , DeviationY
                            , DeviationVecter
                        FROM
                            dbo.M_IMAGE_COMPARISOL
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MImageComparisol> list = m_daoCommon.ConvertToListOf<MImageComparisol>(dataTable);

                    return m_daoCommon.ConvertToListOf<MImageComparisol>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Add(MImageComparisol imageComparisol)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        INSERT dbo.M_IMAGE_COMPARISOL (
                              Created
                            , Updated
                            , DelFlg
                            , PlantSID
                            , CourseSID
                            , CamType
                            , DeviationX
                            , DeviationY
                            , DeviationVecter
                        ) VALUES (
                              GETDATE()
                            , GETDATE()
                            , 0
                            , @PlantSID
                            , @CourseSID
                            , @CamType
                            , @DeviationX
                            , @DeviationY
                            , @DeviationVecter
                        );
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, imageComparisol);
                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Delete(MImageComparisol imageComparisol)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            M_IMAGE_COMPARISOL
                        SET
                            Updated = GETDATE()
                            , DelFlg = 1
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, imageComparisol);

                    int effectedRowCount = helper.ExecuteNonQuery(cmd, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public int Update(MImageComparisol imageComparisol)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        UPDATE
                            M_IMAGE_COMPARISOL
                        SET
                            Updated = GETDATE()
                            , PlantSID = @PlantSID
                            , CourseSID = @CourseSID
                            , CamType = @CamType
                            , DeviationX = @DeviationX
                            , DeviationY = @DeviationY
                            , DeviationVecter = @DeviationVecter
                        WHERE
                            sid = @SID;
                    ";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, imageComparisol);

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

        public class MImageComparisol : Result
        {
            public MImageComparisol() { }

            public int? SID { get; set; }

            /// <summary>
            /// 工場SID
            /// <summary>
            public int PlantSID { get; set; }
            /// <summary>
            /// コースSID
            /// <summary>
            public int CourseSID { get; set; }
            /// <summary>
            /// 認識種別
            /// <summary>
            public int CamType { get; set; }
            /// <summary>
            /// 画像認識_X軸誤差
            /// <summary>
            public float DeviationX { get; set; }
            /// <summary>
            /// 画像認識_Y軸誤差
            /// <summary>
            public float DeviationY { get; set; }
            /// <summary>
            /// ベクトル値誤差
            /// <summary>
            public float DeviationVecter { get; set; }
        }


        #endregion

    }
}
