using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RcdCmn;
using static RcdDao.DaoCommon;
using System.Reflection;
using System.Data;

namespace RcdDao
{
    public class DCarStatus
    {
        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = DaoCommon.GetInstance();

        #endregion

        #region ## 車両ステータス ##

        public List<CarStatus> GetAllCarStatus(int plantsid, int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = $@"
                        SELECT
		                    stat.BodyNo
		                    , stat.CarSpecID		                    
		                    , stat.Xpos
		                    , stat.Ypos
                            , stat.FirstAdmissionTime
                            , stat.Status
	                    FROM
		                    dbo.D_CAR_STATUS stat	                    
	                    WHERE
		                    stat.DelFlg = 0
                            stat.PlantSID = '{plantsid}'
                            stat.StationSID = '{stationsid}';
                        ";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    List<CarStatus> list = m_daoCommon.ConvertToListOf<CarStatus>(dataTable);

                    return m_daoCommon.ConvertToListOf<CarStatus>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public List<CarStatus_Tablet> GetAllCarStatus_Tablet(int plantsid, int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = $@"
                        SELECT
		                    stat.BodyNo
		                    , stat.CarSpecID
                            , stat.CarType
		                    , stat.Xpos
		                    , stat.Ypos
                            , stat.FirstAdmissionTime
                            , stat.SystemStatus
	                    FROM
		                    dbo.D_CAR_STATUS stat	                    
	                    WHERE
		                    stat.DelFlg = 0
                        AND    
                            stat.PlantSID = '{plantsid}'
                        AND
                            stat.StationSID = '{stationsid}'
                        ";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    List<CarStatus_Tablet> list = m_daoCommon.ConvertToListOf<CarStatus_Tablet>(dataTable);

                    return m_daoCommon.ConvertToListOf<CarStatus_Tablet>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public class CarStatus : Result
        {
            public string CarNo { get; set; }

            public string SpecNo { get; set; }

            public string IP { get; set; }

            public string Port1 { get; set; }

            public string Port2 { get; set; }

            public int? CtrlUnitSID { get; set; }

            public string CtrlUnitID { get; set; }

            public string Status { get; set; }

            public string Xpos { get; set; }

            public string Ypos { get; set; }

            public CarStatus() { }

            public CarStatus(string _CarNo, string _SpecNo, string _IP, string _Port1, string _Port2, string _CtrlUnit, string _Status, string _StatusMsg, string _Xpos, string _Ypos)
            {
                CarNo = _CarNo;
                SpecNo = _SpecNo;
                IP = _IP;
                Port1 = _Port1;
                Port2 = _Port2;
                CtrlUnitID = _CtrlUnit;
                Status = _Status;
                Xpos = _Xpos;
                Ypos = _Ypos;
            }

            public void UpdateStatus(CarStatus newStatus)
            {
                CarNo = newStatus.CarNo;
                SpecNo = newStatus.SpecNo;
                IP = newStatus.IP;
                Port1 = newStatus.Port1;
                Port2 = newStatus.Port2;
                CtrlUnitID = newStatus.CtrlUnitID;
                Status = newStatus.Status;
                Xpos = newStatus.Xpos;
                Ypos = newStatus.Ypos;
            }
        }

        public class CarStatus_Tablet : Result
        {
            public int CarType { get; set; }

            public string BodyNo { get; set; }

            public string CarSpecID { get; set; }

            public string ProductionInfo { get; set;}

            public string SystemStatus { get; set; }

            public DateTime FirstAdmissionTime { get; set; }

            public DateTime EndTime { get; set; }

            public int CouseID { get; set; }

            public string ErrorCodeStr { get; set; }

            public string ErrorMsg { get; set; }

            public string IpAddr { get; set; }

            public string Port { get; set; }

            public string Xpos { get; set; }

            public string Ypos { get; set; }

            public CarStatus_Tablet() { }

            public CarStatus_Tablet(int _CarType ,string _BodyNo, string _CarSpecID, string _ProductionInfo, string _SystemStatus, DateTime _FirstAdmissionTime,DateTime _EndTime, int _CouseID , string _ErrorCodeStr, string _ErrorMsg , string _IpAddr, string _Port, string _Xpos, string _Ypos , string _StatusMsg = null)
            {
                CarType = _CarType;
                BodyNo = _BodyNo;
                CarSpecID = _CarSpecID;
                ProductionInfo = _ProductionInfo;
                SystemStatus = _SystemStatus;
                FirstAdmissionTime = _FirstAdmissionTime;
                EndTime = _EndTime;
                CouseID = CouseID;
                ErrorCodeStr = _ErrorCodeStr;
                ErrorMsg = _ErrorMsg;
                IpAddr = _IpAddr;
                Port = _Port;
                Xpos = _Xpos;
                Ypos = _Ypos;

            }

            public void UpdateStatus(CarStatus_Tablet newStatus)
            {
                CarType = newStatus.CarType;
                BodyNo = newStatus.BodyNo;
                CarSpecID = newStatus.CarSpecID;
                ProductionInfo = newStatus.ProductionInfo;
                SystemStatus = newStatus.SystemStatus;
                FirstAdmissionTime = newStatus.FirstAdmissionTime;
                EndTime = newStatus.EndTime;
                CouseID = newStatus.CouseID;
                ErrorCodeStr = newStatus.ErrorCodeStr;
                ErrorMsg = newStatus.ErrorMsg;
                IpAddr = newStatus.IpAddr;
                Port = newStatus.Port;
                Xpos = newStatus.Xpos;
                Ypos = newStatus.Ypos;
            }            
        }

        #endregion

    }
}
