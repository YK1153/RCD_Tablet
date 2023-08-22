using RcdCmn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static RcdDao.DaoCommon;
using static RcdDao.MObjctLabelDao;

namespace RcdDao
{
    public class MObjectLabelAreaDao
    {

        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion


        /// <summary>
        /// 侵入検知ラベルエリアマスタ情報取得
        /// </summary>
        /// <returns></returns>
        public List<MObjLabelArea> GetAllMObjLabelAreaInfo(MObjLabel mObj)
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
                            , LabelSID
                            , RadiusCoefficient
                        FROM
                            dbo.M_OBJ_LABEL_AREA
                        WHERE
                             LabelSID = @SID
                        AND  DelFlg=0
                    ";

                    SqlParameter p = new SqlParameter("@SID", mObj.SID);

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MObjLabelArea> list = m_daoCommon.ConvertToListOf<MObjLabelArea>(dataTable);

                    return m_daoCommon.ConvertToListOf<MObjLabelArea>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 侵入検知ラベルエリアマスタ情報取得
        /// </summary>
        /// <returns></returns>
        public List<MObjLabelAreaPoint> GetAllMObjLabelAreaPointInfo(MObjLabelArea area)
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
                            , LabelAreaSID
                            , Xcoordinate
                            , Ycoordinate
                        FROM
                            dbo.M_OBJ_LABEL_AREA
                        WHERE
                             LabelAreaSID = @SID
                        AND  DelFlg=0
                    ";

                    SqlParameter p = new SqlParameter("@SID", area.SID);

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MObjLabelAreaPoint> list = m_daoCommon.ConvertToListOf<MObjLabelAreaPoint>(dataTable);

                    return m_daoCommon.ConvertToListOf<MObjLabelAreaPoint>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }



        public class MObjLabelArea : Result
        {
            public MObjLabelArea() { }

            public int? SID { get; set; }

            /// <summary>
            /// 工場SID
            /// <summary>
            public int PlantSID { get; set; }
            /// <summary>
            /// ラベルSID
            /// <summary>
            public int LabelSID { get; set; }
            /// <summary>
            /// エリア半径係数
            /// <summary>
            public float RadiusCoefficient { get; set; }
        }

        public class MObjLabelAreaPoint : Result
        {
            public MObjLabelAreaPoint() { }

            public int? SID { get; set; }

            /// <summary>
            /// 工場SID
            /// <summary>
            public int PlantSID { get; set; }
            /// <summary>
            /// ラベルエリアSID
            /// <summary>
            public int LabelAreaSID { get; set; }
            /// <summary>
            /// X座標
            /// <summary>
            public float Xcoordinate { get; set; }
            /// <summary>
            /// Y座標
            /// <summary>
            public float Ycoordinate { get; set; }
        }
    }
}
