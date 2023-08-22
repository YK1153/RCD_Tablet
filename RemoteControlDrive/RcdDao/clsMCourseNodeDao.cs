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
    public class MCourseNodeDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        #region ### データ操作 ###
        public List<MCourseNode> GetAllInfo(int plantsid, int stationsid, int coursesid)
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
                            , NodeSequence
                            , Name
                            , Xcoordinate
                            , Ycoordinate
                        FROM
                            dbo.M_COURSE_NODE
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MCourseNode> list = m_daoCommon.ConvertToListOf<MCourseNode>(dataTable);

                    return m_daoCommon.ConvertToListOf<MCourseNode>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        #endregion


        #region ### 結果セット ###

        public class MCourseNode : Result
        {
            public MCourseNode() { }

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
            /// ノード順序
            /// <summary>
            public int NodeSequence { get; set; }
            /// <summary>
            /// 名称
            /// <summary>
            public string Name { get; set; }
            /// <summary>
            /// x座標
            /// <summary>
            public float Xcoordinate { get; set; }
            /// <summary>
            /// y座標
            /// <summary>
            public float Ycoordinate { get; set; }


            public RouteNode ToRoutenode()
            {
                RouteNode rn = new RouteNode();
                rn.ID = (int)SID;
                rn.Name = Name;
                rn.X = Xcoordinate;
                rn.Y = Ycoordinate;

                return rn;
            }
        }

        #endregion
    }
}
