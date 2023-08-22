using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using RcdCmn;
using static RcdDao.DaoCommon;
using RcdDao.Attributes;
using AtrptShare;


namespace RcdDao
{
    public class MCourseAreaDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        #region ### データ操作 ###
        public List<MCourseArea> GetAllMCourseAreaInfo(int plantsid, int stationsid, int coursesid)
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
                            , CourseSID
                            , AreaSeqence
                            , Name
                            , LeftWidth
                            , RightWidth
                            , AreaSpeed
                            , AccelerationPlus
                            , AccelerationMinus
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
                        FROM
                            dbo.M_COURSE_AREA
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MCourseArea> list = m_daoCommon.ConvertToListOf<MCourseArea>(dataTable);

                    return m_daoCommon.ConvertToListOf<MCourseArea>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        #endregion


        #region ### 結果セット ###

        public class MCourseArea : Result
        {
            public MCourseArea() { }
            
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
            /// コースSID
            /// <summary>
            public int CourseSID { get; set; }
            /// <summary>
            /// エリア順序
            /// <summary>
            public int AreaSeqence { get; set; }
            /// <summary>
            /// 名称
            /// <summary>
            public string Name { get; set; }
            /// <summary>
            /// 左幅
            /// <summary>
            public float LeftWidth { get; set; }
            /// <summary>
            /// 右幅
            /// <summary>
            public float RightWidth { get; set; }
            /// <summary>
            /// 目標速度
            /// <summary>
            public float AreaSpeed { get; set; }
            /// <summary>
            /// 加速度+
            /// <summary>
            public float AccelerationPlus { get; set; }
            /// <summary>
            /// 加速度-
            /// <summary>
            public float AccelerationMinus { get; set; }
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


            public SpeedArea ToSpeedArea()
            {
                SpeedArea sa = new SpeedArea();
                sa.ID = (int)SID;
                sa.Name = Name;
                sa.LeftWidth = LeftWidth;
                sa.RightWidth = RightWidth;
                sa.Speed = AreaSpeed;
                sa.Acceleration = AccelerationPlus;
                sa.Deceleration = AccelerationMinus;

                return sa;
            }

        }

        #endregion
    }
}
