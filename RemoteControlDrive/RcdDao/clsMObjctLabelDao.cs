using RcdCmn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RcdDao.DaoCommon;
using static RcdDao.MCarSpecDao;
using static RcdDao.MObjectLabelAreaDao;

namespace RcdDao
{
    public class MObjctLabelDao
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        #region ## データ取得 ##

        /// <summary>
        /// 侵入検知ラベルマスタ情報取得
        /// </summary>
        /// <returns></returns>
        public List<MObjLabel> GetAllMObjLabelInfo()
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
                            , Label
                            , LabelName
                            , CarFlag
                            , Radius
                        FROM
                            dbo.M_OBJ_LABEL
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    List<MObjLabel> list = m_daoCommon.ConvertToListOf<MObjLabel>(dataTable);

                    return m_daoCommon.ConvertToListOf<MObjLabel>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        #endregion

        #region ## 結果セット ##

        public class MObjLabel : Result
        {
            public MObjLabel() { }

            public int? SID { get; set; }

            /// <summary>
            /// 工場SID
            /// <summary>
            public int PlantSID { get; set; }
            /// <summary>
            /// ラベル
            /// <summary>
            public string Label { get; set; }
            /// <summary>
            /// ラベル名
            /// <summary>
            public string LabelName { get; set; }
            /// <summary>
            /// 車両フラグ
            /// <summary>
            public bool CarFlag { get; set; }
            /// <summary>
            /// 半径
            /// <summary>
            public float Radius { get; set; }
        }

        #endregion


        /// <summary>
        /// ラベル設定
        /// </summary>
        public class ObjectLabel
        {
            public string Label { get; set; }
            public string LabelName { get; set; }
            public float Radius { get; set; }
            public bool CarFlag { get; set; }
            public List<LabelArea> LabelAreas { get; set; }

            public ObjectLabel() { }

            public ObjectLabel(MObjLabel obj) 
            {
                Label = obj.Label;
                LabelName = obj.LabelName;
                Radius = obj.Radius;
                CarFlag = obj.CarFlag;

                MObjectLabelAreaDao dao = new MObjectLabelAreaDao();
                List<MObjLabelArea> objLabelAreaList = dao.GetAllMObjLabelAreaInfo(obj);
                foreach(MObjLabelArea area in objLabelAreaList )
                {
                    LabelArea labelArea = new LabelArea();
                    labelArea.RadiusCoefficient = area.RadiusCoefficient;

                    List<MObjLabelAreaPoint> areaPoints = dao.GetAllMObjLabelAreaPointInfo(area);
                    labelArea.Area = new PointF[areaPoints.Count];
                    for (int i = 0; i < areaPoints.Count; i++)
                    {
                        labelArea.Area[i] = new PointF(areaPoints[i].Xcoordinate, areaPoints[i].Ycoordinate);
                    }
                    LabelAreas.Add(labelArea);
                }
            }

        }


        /// <summary>
        /// ラベルのエリア設定
        /// </summary>
        public class LabelArea
        {
            public double RadiusCoefficient { get; set; }

            public PointF[] Area { get; set; }
        }

    }
}
