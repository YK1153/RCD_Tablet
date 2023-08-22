using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using AtrptCalc;
using AtrptShare;
using RcdCmn;
using RcdDao;
using RcdOperation.PlcControl;
using RcdOperationSystemConst;
using RcpOperation.Common;
using static RcdDao.MCourseAreaDao;
using static RcdDao.MCourseDao;
using static RcdDao.MCourseNodeDao;
using static RcdDao.MFacilityDao;
using static RcdDao.MPLCDao;

namespace RcdOperation.Control
{
    public partial class ControlMain
    {
        #region 定数※DB設定値にする
        private readonly string C_CONFIG_FILE_PATH = ConfigurationManager.AppSettings["C_CONFIG_FILE_PATH"];

        
        private readonly int C_INCLUDE_JUDGE_AREA_COUNT = int.Parse(ConfigurationManager.AppSettings["C_INCLUDE_JUDGE_AREA_COUNT"]);

        private readonly int C_CC_CALC_WAITTIME = int.Parse(ConfigurationManager.AppSettings["C_CC_CALC_WAITTIME"]);
        private readonly int C_CC_SEND_INTERVAL = int.Parse(ConfigurationManager.AppSettings["C_CC_SEND_INTERVAL"]);
        private readonly int C_CARCORNER_OUTAREA_ALLOW_NUM = int.Parse(ConfigurationManager.AppSettings["C_CARCORNER_OUTAREA_ALLOW_NUM"]);

        private readonly int C_CAR_WAITTIME = int.Parse(ConfigurationManager.AppSettings["C_CAR_WAITTIME"]);
        private readonly bool C_SURVEILLANCE_CAR = bool.Parse(ConfigurationManager.AppSettings["C_SURVEILLANCE_CAR"]);
        private readonly int C_SURVEILLANCE_INTERVALP_CAR = int.Parse(ConfigurationManager.AppSettings["C_SURVEILLANCE_INTERVALP_CAR"]);
        //private readonly int C_VCONTROLFLG_THROUGH_NUM = int.Parse(ConfigurationManager.AppSettings["C_VCONTROLFLG_THROUGH_NUM"]);
        private readonly bool C_SNDRSESULT_CAR = bool.Parse(ConfigurationManager.AppSettings["C_SNDRSESULT_CAR"]);
        //private readonly int C_PORT1 = int.Parse(ConfigurationManager.AppSettings["C_PORT1"]);
        //private readonly int C_PORT2 = int.Parse(ConfigurationManager.AppSettings["C_PORT2"]);
        private readonly int C_CARSTOP_SENDCOUNT = int.Parse(ConfigurationManager.AppSettings["C_CARSTOP_SENDCOUNT"]);
        private readonly int C_CARSTOP_SENDDEREY = int.Parse(ConfigurationManager.AppSettings["C_CARSTOP_SENDDEREY"]);
        private readonly int C_SURVEILLANCE_INTERVALFS_CAR = int.Parse(ConfigurationManager.AppSettings["C_SURVEILLANCE_INTERVALFS_CAR"]);
        private readonly int C_SURVEILLANCE_INTERVALS_CAR = int.Parse(ConfigurationManager.AppSettings["C_SURVEILLANCE_INTERVALS_CAR"]);

        private readonly int C_CI_INTERVAL = int.Parse(ConfigurationManager.AppSettings["C_CI_INTERVAL"]);

        // private readonly int C_SENSORCHECK_DELAY = int.Parse(ConfigurationManager.AppSettings["C_SENSORCHECK_DELAY"]);

        internal readonly int m_MAX_IO_READ_WAIT = int.Parse(ConfigurationManager.AppSettings["IO_READ_MAX_WAIT"]);
        internal readonly int m_STATUS_UPDATE_INTERVAL = int.Parse(ConfigurationManager.AppSettings["STATUS_UPDATE_INTERVAL"]);

        private readonly int C_COMPARISONPOSITION = int.Parse(ConfigurationManager.AppSettings["C_COMPARISONPOSITION"]);

        private readonly double C_CONTROLCARRADIUS = double.Parse(ConfigurationManager.AppSettings["C_CONTROLCARRADIUS"]);

        public readonly string C_START_CAM_ID = ConfigurationManager.AppSettings["C_START_CAM_ID"];
        public readonly string C_START_CAM_IP = ConfigurationManager.AppSettings["C_START_CAM_IP"];
        #endregion

        /// <summary>
        /// 初期動作
        /// </summary>
        private void Dummy_firstprocess()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //設定ファイル読み込み
            LoadFile();

            CommonProc.ReadFile("xml/mCarSpec.xml",ref m_carSpecs);

            CommonProc.ReadFile("xml/LabelList.xml", ref m_labelList);
            CommonProc.ReadFile("xml/mImageComparisol.xml", ref m_deviations);

            // PLC情報取得
            //MPLCDao fPLCDao = new MPLCDao();
            //m_plc_debice = fPLCDao.GetAllPLCDevice();
            CommonProc.ReadFile("xml/plcdevice.xml", ref m_plc_debice);
            m_plcunit = new List<PLCUnitStatus>();
            foreach (MPlcDevice plc in m_plc_debice)
            {
                m_plcunit.Add(new PLCUnitStatus(plc));
            }

            //設備情報取得
            //MFacilityDao fDao = new MFacilityDao();
            m_facilityControlThreads = new List<FacilityControl>();

            //List<SettingMap> settingMaps = fDao.GetAllrFacilityCtrlMap();
            List<RFacilityCtrlMap> mapList = new List<RFacilityCtrlMap>();
            CommonProc.ReadFile("xml/settingMap.xml", ref mapList);
            List<SettingMap> settingMaps = mapList
                    .GroupBy(map => new { map.FacilityTypeID, map.FacilityTypeName, map.StatusCode, map.StatusMsg, map.ReadByOutput })
                    .Select(mapGroup =>
                        new SettingMap(
                            mapGroup.Key.FacilityTypeID,
                            mapGroup.Key.FacilityTypeName,
                            mapGroup.Key.StatusCode,
                            mapGroup.Key.StatusMsg,
                            mapGroup
                                .Where(map => map.IOType == (int)IOTYPE.Output)
                                .Select(map => map.FuncID)
                                .ToArray(),
                            mapGroup
                                .Where(map => map.IOType == (int)IOTYPE.Input)
                                .Select(map => map.FuncID)
                                .ToArray(),
                            mapGroup.Key.ReadByOutput))
                                .ToList();

            //m_facilityTypes = fDao.GetAllFacilityTypes();
            CommonProc.ReadFile("xml/facilityTypes.xml", ref m_facilityTypes);
            //m_facilityStatusMsgs = fDao.GetFacilityStatusMsgs();
            CommonProc.ReadFile("xml/facilityStatusMsgs.xml", ref m_facilityStatusMsgs);

            //List<RFacility> rFacilities = fPLCDao.GetAllrFacilityInfo();
            List<RFacility> rFacilities = new List<RFacility>();
            CommonProc.ReadFile("xml/rFacilities.xml", ref rFacilities);
            //List<RPLCBitSetting> rPLCBitSettings = fPLCDao.GetAllrPLCBitSettings();
            List<RPLCBitSetting> rPLCBitSettings = new List<RPLCBitSetting>();
            CommonProc.ReadFile("xml/rPLCBitSettings.xml", ref rPLCBitSettings);

            PLCControl.Initialize(rFacilities, rPLCBitSettings, settingMaps, m_plcunit, m_MAX_IO_READ_WAIT);
            m_PLCControl = PLCControl.GetInstance();

            CommonProc.ReadFile("xml/systemMessage.xml", ref m_SystemMsg);

            m_dicFacilityStatus = new Dictionary<string, FacilityStatus>();


            //PLCステータスDB記録
            m_plcstatusLogTimer = new System.Timers.Timer
            {
                Interval = m_STATUS_UPDATE_INTERVAL
            };
            m_plcstatusLogTimer.Elapsed += OnPlcStatusLogTimerElapsed;
            m_plcstatusLogTimer.Start();


            // 設定値取得
            m_controlPLC = new List<MControlPLCDao.MControlPlc>();
            CommonProc.ReadFile("xml/mControlPlc.xml", ref m_controlPLC);
            m_controlSetting = new MControlSettingDao.MControlSetting();
            CommonProc.ReadFile("xml/mControlSetting.xml",ref m_controlSetting);


            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        /// <summary>
        /// 設定ファイル読み込み
        /// </summary>
        private void LoadFile()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            try
            {
                if (File.Exists(C_CONFIG_FILE_PATH))
                {
                    //ConfigData m_ConfigData = new ConfigData();

                    System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(ConfigData));
                    using (System.IO.FileStream fs = new System.IO.FileStream(C_CONFIG_FILE_PATH, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        m_ConfigData = (ConfigData)xml.Deserialize(fs);
                    }

                    //設定値反映
                    SetConfig();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw new UserException($"設定ファイルが不正です。{ex.Message}");
            }

            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        private void SetConfig()
        {
            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} Start");

            //経路設定
            CourseInfo courseInfo = new CourseInfo();
            courseInfo.Course = new MCourse();
            courseInfo.CourseArea = new List<MCourseArea>();
            courseInfo.CourseNode = new List<MCourseNode>();

            MCourse course = new MCourse();
            CommonProc.ReadFile("xml/mCourse.xml", ref course);
            courseInfo.Course = course;

            //List<MCourseArea> arealist = new List<MCourseArea>();
            //ReadFile("xml/mCourseArea.xml", ref arealist);
            //courseInfo.CourseArea = arealist;

            //List<MCourseNode> nodelist = new List<MCourseNode>();
            //ReadFile("xml/mCourseNode.xml", ref nodelist);
            //courseInfo.CourseNode = nodelist;

            foreach (SpeedArea area in m_ConfigData.SpeedAreaList.SpeedArea)
            {
                MCourseArea coursearea = new MCourseArea()
                {
                    SID = area.ID,
                    Name = area.Name,
                    LeftWidth = (float)area.LeftWidth,
                    RightWidth = (float)area.RightWidth,
                    AreaSpeed = (float)area.Speed,
                    AccelerationPlus = area.Acceleration,
                    AccelerationMinus = area.Deceleration,
                };

                courseInfo.CourseArea.Add(coursearea);
            }
            foreach (RouteNode node in m_ConfigData.RouteNodeList.RouteNode)
            {
                MCourseNode coursenode = new MCourseNode()
                {
                    SID = node.ID,
                    Name = node.Name,
                    Xcoordinate = node.X,
                    Ycoordinate = node.Y
                };

                courseInfo.CourseNode.Add(coursenode);
            }


            m_CourseInfoList = new List<CourseInfo>
            {
                courseInfo
            };

            foreach (MCourseNode routeNode in m_CourseInfoList[0].CourseNode)
            {
                m_RouteNodes.Add(routeNode.ToRoutenode()); ;
            }
            //foreach(CourseArea courseArea in courseInfo.CourseArea) 
            for (int i = 0; i < m_CourseInfoList[0].CourseArea.Count; i++)
            {
                SpeedArea speedArea = m_CourseInfoList[0].CourseArea[i].ToSpeedArea();
                speedArea.Area = AreaSet(i, speedArea, 0, m_ConfigData);
                speedArea.SlowArea = AreaSet(i, speedArea, 1, m_ConfigData);
                speedArea.PauseArea = AreaSet(i, speedArea, 2, m_ConfigData);
                speedArea.StopArea = AreaSet(i, speedArea, 3, m_ConfigData);

                m_SpeedAreas.Add(speedArea);
            }

            //その他エリアセット
            m_otherArea = new OtherAreaRetention[m_ConfigData.OtherAreas.Count()];
            for (int i = 0; i <= m_ConfigData.OtherAreas.Count() - 1; i++)
            {
                m_otherArea[i] = new OtherAreaRetention
                {
                    otherarea = new OtherArea()
                };
                m_otherArea[i].otherarea = m_ConfigData.OtherAreas[i];
                m_otherArea[i].Done = OperationConst.FacilityAreaStage.C_FACILITY_START;
                if (m_ConfigData.OtherAreas[i].AreaCode == OtherAreaCode.GuideArea)
                {
                    GuideareaJudge Gaj = new GuideareaJudge();
                    Gaj.guidearea = m_ConfigData.OtherAreas[i];
                    guideAreaList.Add(Gaj);
                }
            }


            AppLog.GetInstance().Debug($"{MethodBase.GetCurrentMethod().Name} End");
        }

        

        #region エリアの各座標値のセット
        public PointF[] AreaSet(int i, SpeedArea NowSpeedArea, int num, ConfigData m_ConfigData)
        {
            List<PointF> area = new List<PointF>();
            int ncnt = i + 1;

            double uedge = 0;
            double vertical = 0;

            uedge = m_RouteNodes[ncnt].X - m_RouteNodes[ncnt - 1].X; // 底辺
            vertical = m_RouteNodes[ncnt].Y - m_RouteNodes[ncnt - 1].Y; // 高さ
            double rad = Math.Atan2(vertical, uedge); // ラジアン
            double cosθ = Math.Cos(rad);
            double sinθ = Math.Sin(rad);

            double rightwidth = AreaSelecterRight(NowSpeedArea, num, m_ConfigData);
            double leftwidth = AreaSelecterLeft(NowSpeedArea, num, m_ConfigData);

            // エリアの頂点座標の計算
            area.Add(new PointF((float)(m_RouteNodes[ncnt - 1].X + rightwidth * sinθ), (float)(m_RouteNodes[ncnt - 1].Y - rightwidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt].X + rightwidth * sinθ), (float)(m_RouteNodes[ncnt].Y - rightwidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt].X - leftwidth * sinθ), (float)(m_RouteNodes[ncnt].Y + leftwidth * cosθ)));
            area.Add(new PointF((float)(m_RouteNodes[ncnt - 1].X - leftwidth * sinθ), (float)(m_RouteNodes[ncnt - 1].Y + leftwidth * cosθ)));
            area.Add(area[0]);

            if (i > 0)
            {
                PointF O = new PointF(m_RouteNodes[ncnt - 2].X, m_RouteNodes[ncnt - 2].Y);
                PointF A = new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y);
                PointF B = new PointF(m_RouteNodes[ncnt].X, m_RouteNodes[ncnt].Y);

                double Z = CommonProc.Gaiseki(O, A, B);
                if (Z > 0)
                {
                    area.Insert(4, new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y));
                    switch (num)
                    {
                        case 0:
                            area.Insert(5, m_SpeedAreas[i - 1].Area[1]);
                            break;
                        case 1:
                            area.Insert(5, m_SpeedAreas[i - 1].SlowArea[1]);
                            break;
                        case 2:
                            area.Insert(5, m_SpeedAreas[i - 1].PauseArea[1]);
                            break;
                        case 3:
                            area.Insert(5, m_SpeedAreas[i - 1].StopArea[1]);
                            break;
                    }

                }
                else if (Z < 0)
                {
                    switch (num)
                    {
                        case 0:
                            area.Insert(4, m_SpeedAreas[i - 1].Area[2]);
                            break;
                        case 1:
                            area.Insert(4, m_SpeedAreas[i - 1].SlowArea[2]);
                            break;
                        case 2:
                            area.Insert(4, m_SpeedAreas[i - 1].PauseArea[2]);
                            break;
                        case 3:
                            area.Insert(4, m_SpeedAreas[i - 1].StopArea[2]);
                            break;
                    }
                    area.Insert(5, new PointF(m_RouteNodes[ncnt - 1].X, m_RouteNodes[ncnt - 1].Y));
                }
            }

            return (PointF[])area.ToArray().Clone();
        }

        private double AreaSelecterRight(SpeedArea NowSpeedArea, int num, ConfigData m_ConfigData)
        {
            double width = 0;

            switch (num)
            {
                case 0:
                    width = NowSpeedArea.RightWidth;
                    break;
                case 1:
                    if (NowSpeedArea.SlowRightWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.SlowRightWidth);
                    }
                    else
                    {
                        width = double.Parse(m_ConfigData.SlowRightWidth);
                    }
                    break;
                case 2:
                    if (NowSpeedArea.PauseRightWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.PauseRightWidth);
                    }
                    else
                    {
                        width = double.Parse(m_ConfigData.PauseRightWidth);
                    }
                    break;
                case 3:
                    if (NowSpeedArea.StopRightWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.StopRightWidth);
                    }
                    else
                    {
                        width = double.Parse(m_ConfigData.StopRightWidth);
                    }
                    break;
            }

            return width;
        }
        private double AreaSelecterLeft(SpeedArea NowSpeedArea, int num, ConfigData m_ConfigData)
        {
            double width = 0;

            switch (num)
            {
                case 0:
                    width = NowSpeedArea.LeftWidth;
                    break;
                case 1:
                    if (NowSpeedArea.SlowLeftWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.SlowLeftWidth);
                    }
                    else
                    {
                        width = double.Parse(m_ConfigData.SlowLeftWidth);
                    }
                    break;
                case 2:
                    if (NowSpeedArea.PauseLeftWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.PauseLeftWidth);
                    }
                    else
                    {
                        width = double.Parse(m_ConfigData.PauseLeftWidth);
                    }
                    break;
                case 3:
                    if (NowSpeedArea.StopLeftWidth != null && NowSpeedArea.SlowRightWidth != "")
                    {
                        width = double.Parse(NowSpeedArea.StopLeftWidth);
                    }
                    else
                    {
                        width = double.Parse(m_ConfigData.StopLeftWidth);
                    }
                    break;
            }

            return width;
        }
        #endregion


    }


    public class ControlStartClass
    {
        //public OperationConst.CamType MainCamType { get; set; }

        //public string CarNo { get; set; }
        //= "01234";
        public string CarSpecNo { get; set; }
        // = "00032";
        public string IPaddr { get; set; }
        // = "192.168.0.101";
        public int GoalNo { get; set; }
        // = 1;
        //public string GearRate { get; set; }
        //// = "";
        //public string Wheelbase { get; set; }
        //// = "";
        //public string WidthDistance { get; set; }
        //// = "";
        public string ForwardDistance { get; set; }
        // = "";
        public string AreaAdjust { get; set; }
        // = "";
        public int PORT1 { get; set; }
        // = 10000;
        //public double CenterFrontLength { get; set; }
        //public double RearOverhang { get; set; }
    }

}
