using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using RcdCmn;
using static RcdDao.DaoCommon;
using RcdDao.Attributes;
using static RcdDao.MCourseNodeDao;
using static RcdDao.MCourseAreaDao;

namespace RcdDao
{
    public class MCourseDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        #region ### データ操作 ###
        public List<MCourse> GetAllMCourseInfo(int plantsid,int stationsid)
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
                            , Name
                            , MainCamNo
                            , SubCamNo
                            , SlowLeftWidth
                            , SlowRightWidth
                            , SlowFrontDistance
                            , SlowBackDistance
                            , PauseLeftWidth
                            , PauseRightWidth
                            , PauseFrontDistance
                            , PauseBackDistance
                            , StopLeftWidth
                            , StopRightWidth
                            , StopFrontDistance
                            , StopBackDistance
                            , minCoordinateX
                            , maxCoordinateX
                            , minCoordinateY
                            , maxCoordinateY
                            , AccelerationAbnomal
                            , AccelerationPause
                        FROM
                            dbo.M_COURSE
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MCourse> list = m_daoCommon.ConvertToListOf<MCourse>(dataTable);

                    return m_daoCommon.ConvertToListOf<MCourse>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        #endregion

        #region ### 結果セット ###

        public class MCourse : Result
        {
            public MCourse() { }

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
            /// コース名
            /// <summary>
            public string Name { get; set; }
            /// <summary>
            /// メイン測位
            /// <summary>
            public int MainCamNo { get; set; }
            /// <summary>
            /// サブ測位
            /// <summary>
            public int SubCamNo { get; set; }
            /// <summary>
            /// 減速エリア左幅
            /// <summary>
            public string SlowLeftWidth { get; set; }
            /// <summary>
            /// 減速エリア右幅
            /// <summary>
            public string SlowRightWidth { get; set; }
            /// <summary>
            /// 減速エリア前距離
            /// <summary>
            public string SlowFrontDistance { get; set; }
            /// <summary>
            /// 減速エリア後距離
            /// <summary>
            public string SlowBackDistance { get; set; }
            /// <summary>
            /// 一時停止エリア左幅
            /// <summary>
            public string PauseLeftWidth { get; set; }
            /// <summary>
            /// 一時停止エリア右幅
            /// <summary>
            public string PauseRightWidth { get; set; }
            /// <summary>
            /// 一時停止エリア前距離
            /// <summary>
            public string PauseFrontDistance { get; set; }
            /// <summary>
            /// 一時停止エリア後距離
            /// <summary>
            public string PauseBackDistance { get; set; }
            /// <summary>
            /// 走行停止エリア左幅
            /// <summary>
            public string StopLeftWidth { get; set; }
            /// <summary>
            /// 走行停止エリア右幅
            /// <summary>
            public string StopRightWidth { get; set; }
            /// <summary>
            /// 走行停止エリア前距離
            /// <summary>
            public string StopFrontDistance { get; set; }
            /// <summary>
            /// 走行停止エリア後距離
            /// <summary>
            public string StopBackDistance { get; set; }
            /// <summary>
            /// グローバル座標x表示最小
            /// <summary>
            public float minCoordinateX { get; set; }
            /// <summary>
            /// グローバル座標x表示最大
            /// <summary>
            public float maxCoordinateX { get; set; }
            /// <summary>
            /// グローバル座標y表示最小
            /// <summary>
            public float minCoordinateY { get; set; }
            /// <summary>
            /// グローバル座標y表示最大
            /// <summary>
            public float maxCoordinateY { get; set; }
            /// <summary>
            /// 走行停止時減速度
            /// <summary>
            public float AccelerationAbnomal { get; set; }
            /// <summary>
            /// 一時停止時減速度
            /// <summary>
            public float AccelerationPause { get; set; }
        }
        #endregion

        /// <summary>
        /// コース情報
        /// </summary>
        public class CourseInfo : Result
        {
            public CourseInfo(MCourse course)
            {
                //初期化
                this.CourseNode = new List<MCourseNode>();
                this.CourseArea = new List<MCourseArea>();

                //コース情報をセット
                Course = course;
                //コースに紐づく情報をセット
                MCourseNodeDao nodedao = new MCourseNodeDao();
                CourseNode = nodedao.GetAllInfo(course.PlantSID, course.StationSID, (int)course.SID);
                MCourseAreaDao areadao = new MCourseAreaDao();
                CourseArea = areadao.GetAllMCourseAreaInfo(course.PlantSID, course.StationSID, (int)course.SID);

            }


            //※いづれ削除
            public CourseInfo()
            {
                this.CourseNode = new List<MCourseNode>();
                this.CourseArea = new List<MCourseArea>();
            }

            public MCourse Course { get; set; }
            public List<MCourseNode> CourseNode { get; set; }
            public List<MCourseArea> CourseArea { get; set; }
                
        }
    }
}
