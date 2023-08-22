﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using RcdCmn;
using static RcdDao.DaoCommon;
using System.Data.SqlTypes;
using System.ComponentModel;
using System.Text;
using RcdDao.Attributes;
using System.Linq;

namespace RcdDao
{
    public class MFacilityDao
    {
        public enum IOTYPE { Output = 0, Input = 1 }

        public enum FacilityTypes
        {
            TriadSignal = 0,
            DuoSignal = 1,
            Shutter = 2,
            LineLight = 3,
            Andon = 4,
            StopBtn = 5,
            AreaSensor = 6,
            RemainDetect = 7,
            MngModeViewer = 8,
            GoalOutput = 9
        }


        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = DaoCommon.GetInstance();

        #endregion

        /// <summary>
        /// 設備情報取得ベースクエリ取得
        /// </summary>
        private string GetFacilityBaseQuery()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                return @"
                    SELECT
		                sid
		                , facility.Created
		                , facility.ID
		                , facility.FacilityName
		                , facility.FacilityTypeID
		                , facilityType.TypeName
		                , isValid.IsValidSetting
	                FROM
		                dbo.M_FACILITY facility
	                LEFT OUTER JOIN
		                dbo.M_FACILITY_TYPE facilityType
	                ON
		                facility.FacilityTypeID = facilityType.ID
	                AND facilityType.DelFlg = 0
	                LEFT OUTER JOIN
		                (SELECT
			                FacilitySID,
			                CASE 
				                WHEN 
					                SUM(
						                CASE WHEN (DIODeviceID IS NOT NULL) 
								                AND (IOPort IS NOT NULL) 
								                AND (SettingValue IS NOT NULL) THEN 0
								                ELSE 1 
						                END
					                ) > 0 THEN 0
				                ELSE 1
			                END AS IsValidSetting
		                FROM
			                dbo.M_FACILITY_SETTING
		                WHERE
			                DelFlg = 0
		                GROUP BY
			                FacilitySID
		                ) isValid
	                ON
		                facility.sid = isValid.FacilitySID
	                WHERE
		                facility.DelFlg = 0";
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public List<Facility_Coordinate> GetALLFacilityCoordinate(int plantsid , int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = $@"
                        SELECT
                              facility.sid AS SID
                            , facility.ID
		                    , facility.FacilityName
		                    , facility.FacilityTypeID
		                    , facility.Xcoordinate
                            , facility.Ycoordinate
                            , facility.visible
	                    FROM
		                    dbo.M_FACILITY facility
	                    WHERE
		                    facility.DelFlg = 0
                        AND
                            facility.plantSID = '{plantsid}'
                        AND
                            facility.stationSID = '{stationsid}'";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    return m_daoCommon.ConvertToListOf<Facility_Coordinate>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 全設備情報取得
        /// </summary>
        public List<Facility> GetAllFacilityInfo()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = GetFacilityBaseQuery();

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    return m_daoCommon.ConvertToListOf<Facility>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 指定種別の設備情報を取得
        /// </summary>
        /// <param name="facilityTypeID">設備種別ID</param>
        public List<Facility> GetAllFacilityInfoOfType(int facilityTypeID)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = GetFacilityBaseQuery();

                    query += "AND facility.FacilityTypeID = @FacilityTypeID";

                    SqlParameter p = new SqlParameter("@FacilityTypeID", facilityTypeID);

                    DataTable dataTable = helper.Execute(query, CommandType.Text, p);

                    return m_daoCommon.ConvertToListOf<Facility>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備情報取得(1件)
        /// </summary>
        public Facility GetFacilityInfo(int sid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        SELECT
		                    sid
		                    , facility.Created
                            , facility.sid AS SID
		                    , facility.ID
		                    , facility.FacilityName
		                    , facility.FacilityTypeID
		                    , facilityType.TypeName
	                    FROM
		                    dbo.M_FACILITY facility
	                    LEFT OUTER JOIN
		                    dbo.M_FACILITY_TYPE facilityType
	                    ON
		                    facility.FacilityTypeID = facilityType.ID
	                    AND facilityType.DelFlg = 0
	                    WHERE
                            facility.sid = @SID
		                    AND facility.DelFlg = 0";

                    SqlParameter p = new SqlParameter("@SID", sid);

                    DataTable dataTable = helper.Execute(query, CommandType.Text, p);

                    List<Facility> result = m_daoCommon.ConvertToListOf<Facility>(dataTable);

                    if (result.Count == 0) return null;

                    return result[0];
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備IO設定情報取得
        /// </summary>
        public List<FacilitySetting> GetAllFacilitySettingInfo()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        SELECT
		                    setting.ID AS SettingID
		                    , setting.FacilityFuncID AS FacilityFuncID
                            , setting.DIODeviceID
                            , facility.sid AS FacilitySID
		                    , facility.ID AS FacilityID
		                    , facility.FacilityName
		                    , facilityFunc.SettingName
		                    , facilityFunc.IOType
		                    , dioDevice.DIODeviceName
		                    , setting.IOPort
		                    , setting.SettingValue
	                    FROM
		                    dbo.M_FACILITY_SETTING setting
	                    LEFT OUTER JOIN
		                    dbo.M_FACILITY facility
	                    ON
		                    setting.FacilitySID = facility.sid
	                    AND facility.DelFlg = 0
	                    LEFT OUTER JOIN
		                    dbo.M_FACILITY_FUNC facilityFunc
	                    ON
		                    setting.FacilityFuncID = facilityFunc.ID
	                    AND facilityFunc.DelFlg = 0
	                    LEFT OUTER JOIN
		                    dbo.M_DIO_DEVICE dioDevice
	                    ON
		                    setting.DIODeviceID = dioDevice.ID
	                    AND dioDevice.DelFlg = 0
	                    WHERE
		                    setting.DelFlg = 0";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    return m_daoCommon.ConvertToListOf<FacilitySetting>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備IO設定情報取得
        /// </summary>
        public List<FacilitySetting> GetFacilitySettingInfo(int facilitySID)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        SELECT
		                        setting.ID AS SettingID
		                        , setting.FacilityFuncID AS FacilityFuncID
                                , facility.sid As FacilitySID
		                        , facility.ID AS FacilityID
		                        , setting.DIODeviceID AS DIODeviceID
		                        , facility.FacilityName
		                        , facilityFunc.SettingName
		                        , facilityFunc.IOType
		                        , dioDevice.DIODeviceName
		                        , setting.IOPort
		                        , setting.SettingValue
	                        FROM
		                        dbo.M_FACILITY_SETTING setting
	                        LEFT OUTER JOIN
		                        dbo.M_FACILITY facility
	                        ON
		                        setting.FacilitySID = facility.sid
	                        AND facility.DelFlg = 0
	                        LEFT OUTER JOIN
		                        dbo.M_FACILITY_FUNC facilityFunc
	                        ON
		                        setting.FacilityFuncID = facilityFunc.ID
	                        AND facilityFunc.DelFlg = 0
	                        LEFT OUTER JOIN
		                        dbo.M_DIO_DEVICE dioDevice
	                        ON
		                        setting.DIODeviceID = dioDevice.ID
	                        AND dioDevice.DelFlg = 0
	                        WHERE
		                        setting.DelFlg = 0
	                        AND setting.FacilitySID = @facilitySID;";

                    SqlParameter p = new SqlParameter("@facilitySID", facilitySID);

                    DataTable dataTable = helper.Execute(query, CommandType.Text, p);

                    return m_daoCommon.ConvertToListOf<FacilitySetting>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備IO設定情報取得
        /// </summary>
        public List<FacilityType> GetAllFacilityTypes()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        	SELECT
		                        SID
		                        , TypeName
                                , ErrorStatus
                                , ResolveStatus
	                        FROM
		                        dbo.M_FACILITY_TYPE
	                        WHERE
		                        DelFlg = 0;
	                        ";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    return m_daoCommon.ConvertToListOf<FacilityType>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// DIOデバイス情報取得
        /// </summary>
        public List<DIODevice> GetAllDIODeviceInfo()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                    	SELECT
		                    ID
		                    , DIODeviceName
		                    , InputPorts
		                    , OutputPorts
	                    FROM
		                    dbo.M_DIO_DEVICE
	                    WHERE
		                    DelFlg = 0";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    return m_daoCommon.ConvertToListOf<DIODevice>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器ステータス取得
        /// </summary>
        public List<FacilityStatus> GetAllFacilityStatus()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    SELECT
		                    stat.FacilityTypeID,
                            msg.StatusCode,
		                    stat.FacilitySID,
                            fac.ID                         AS FacilityID,
		                    stat.FacilityName,
		                    stat.Status1,
		                    ISNULL(msg.StatusMsg, 'エラー')   AS StatusMsg,
		                    stat.Status2,
		                    stat.StatusOpt
	                    FROM
		                    dbo.D_FACILITY_STATUS stat
                        INNER JOIN
                            dbo.M_FACILITY fac
                        ON
                            stat.FacilitySID = fac.sid
                            AND fac.DelFlg = 0
	                    LEFT OUTER JOIN
		                    dbo.M_FACILITY_STATUS_MSG msg
	                    ON
		                    stat.FacilityTypeID = msg.FacilityTypeID
	                    AND stat.Status1 = msg.StatusCode
	                    WHERE
		                    stat.DelFlg = 0";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);
                    return m_daoCommon.ConvertToListOf<FacilityStatus>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器ステータス取得(Tablet)
        /// </summary>
        public List<FacilityStatus_Tablet> GetAllFacilityStatus_Tablet(int plantsid,int stationsid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = $@"
	                    SELECT
                            stat.SID,
		                    stat.FacilityTypeID,
                            msg.StatusCode,
		                    fac.ID As FacilityID,
                            stat.PlantSID,
                            stat.StationSID,
		                    stat.FacilityName,
		                    stat.Status,
                            stat.StatusOpt,
		                    ISNULL(msg.StatusMsg, 'エラー')   AS StatusMsg
	                    FROM
		                    dbo.D_FACILITY_STATUS stat
	                    LEFT OUTER JOIN
		                    dbo.M_FACILITY_STATUS_MSG msg
	                    ON
		                    stat.FacilityTypeID = msg.FacilityTypeID
						AND stat.Status = msg.StatusCode
                        LEFT OUTER JOIN
		                    M_FACILITY fac
	                    ON
		                    stat.FacilityName = fac.Name
	                    WHERE
		                    stat.DelFlg = 0
                        AND
                            stat.StationSID = {stationsid}
                        AND
                            stat.PlantSID = {plantsid}";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);
                    return m_daoCommon.ConvertToListOf<FacilityStatus_Tablet>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public List<FacilityBtnRGB> GetbtnRGB()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    SELECT
		                    msg.FacilityTypeID,
		                    msg.StatusCode,
                            msg.StatusMsg,
		                    msg.COLOR_R,
		                    msg.COLOR_G,
		                    msg.COLOR_B
	                    FROM
		                    dbo.M_FACILITY_STATUS_MSG msg
	                    WHERE
		                    msg.DelFlg = 0";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);
                    return m_daoCommon.ConvertToListOf<FacilityBtnRGB>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器ステータス取得
        /// </summary>
        public List<DIOPortSetting> GetAllDIOPortSettings()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    SELECT 
		                    setting.DIODeviceID,
		                    device.DIODeviceName,
		                    func.IOType,
		                    setting.IOPort,
		                    setting.SettingValue,
		                    setting.FacilityID,
		                    fac.FacilityTypeID,
		                    fac.FacilityName,
		                    func.SettingName,
		                    setting.FacilityFuncID
	                    FROM 
		                    M_FACILITY_SETTING setting
	                    INNER JOIN
		                    M_FACILITY fac
	                    ON
		                    setting.FacilityID = fac.ID
	                    AND fac.DelFlg = 0
	                    INNER JOIN
		                    M_FACILITY_FUNC func
	                    ON
		                    setting.FacilityFuncID = func.ID
	                    AND func.DelFlg = 0
	                    INNER JOIN
		                    M_DIO_DEVICE device
	                    ON
		                    setting.DIODeviceID = device.ID
	                    AND device.DelFlg = 0
	                    ORDER BY setting.DIODeviceID, func.IOType, setting.IOPort ASC;
                    ";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    List<DIOPortSetting> result = m_daoCommon.ConvertToListOf<DIOPortSetting>(dataTable);

                    return result;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器追加
        /// </summary>
        public int AddFacility(Facility facility)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string precedureName = "insertFacility";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(precedureName, facility);
                    int effectedRowCount = helper.ExecuteNonQuery(precedureName, CommandType.StoredProcedure, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// DIOデバイス追加
        /// </summary>
        public int AddDIODevice(DIODevice device)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    INSERT M_DIO_DEVICE(
		                    Created
		                    , Updated
		                    , DelFlg
		                    , DIODeviceName
		                    , InputPorts
		                    , OutputPorts
	                    )
	                    VALUES (
		                    GETDATE()
		                    , GETDATE()
		                    , 0
		                    , @DIODeviceName
		                    , @InputPorts
		                    , @OutputPorts
	                    )";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, device);

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器削除
        /// </summary>
        public int DeleteFacility(Facility facility)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    UPDATE 
		                    M_FACILITY
	                    set 
		                    DelFlg = 1
		                    , Updated = GETDATE() 
	                    WHERE 
		                    sid = @SID;

	                    UPDATE
		                    M_FACILITY_SETTING
	                    set
		                    DelFlg = 1
		                    , Updated = GETDATE()
	                    WHERE
		                    FacilitySID = @SID";

                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@SID", facility.SID));

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text, paramList.ToArray());



                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// DIOデバイス削除
        /// </summary>
        public int DeleteDIODevice(DIODevice device)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        UPDATE 
		                    M_DIO_DEVICE 
	                    set 
		                    DelFlg = 1
		                    , Updated = GETDATE() 
	                    WHERE 
		                    ID = @ID;

	                    UPDATE
		                    M_FACILITY_SETTING
	                    set
		                    DIODeviceID = NULL
		                    , Updated = GETDATE()
	                    WHERE
		                    DIODeviceID = @ID";

                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@ID", device.ID));

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text, paramList.ToArray());

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器更新
        /// </summary>
        public int UpdateFacility(Facility facility)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    UPDATE 
		                    M_FACILITY
	                    set 
		                    ID = @ID,
		                    FacilityName = @FacilityName
		                    , Updated = GETDATE() 
	                    WHERE 
		                    sid = @SID;";

                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@SID", facility.SID));
                    paramList.Add(new SqlParameter("@ID", facility.ID));
                    paramList.Add(new SqlParameter("@FacilityName", facility.FacilityName));

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text, paramList.ToArray());

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器位置更新
        /// </summary>
        public int UpdateFacilityCoordinate(Facility_Coordinate facility_coordinate)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    UPDATE 
		                    M_FACILITY
	                    set 
		                      Xcoordinate = @Xcoordinate
		                    , Ycoordinate = @Ycoordinate
                            , visible = @visible
		                    , Updated = GETDATE() 
	                    WHERE 
		                    sid = @SID;";

                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@SID", facility_coordinate.SID));
                    paramList.Add(new SqlParameter("@Xcoordinate", facility_coordinate.Xcoordinate));
                    paramList.Add(new SqlParameter("@Ycoordinate", facility_coordinate.Ycoordinate));
                    paramList.Add(new SqlParameter("@visible", facility_coordinate.visible));

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text, paramList.ToArray());

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器位置更新
        /// </summary>
        public int DeleteFacilityCoordinate(int sid)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    UPDATE 
		                    M_FACILITY
	                    set 
		                      visible = 0
		                    , Updated = GETDATE() 
	                    WHERE 
		                    sid = @SID;";

                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@SID", sid));

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text, paramList.ToArray());

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// DIOデバイス更新
        /// </summary>
        public int UpdateDIODevice(DIODevice device)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        UPDATE 
		                    M_DIO_DEVICE 
	                    set 
		                    Updated = GETDATE()
		                    , DIODeviceName = @DIODeviceName
		                    , InputPorts = @InputPorts
		                    , OutputPorts = @OutputPorts
	                    WHERE 
		                    ID = @ID;
                        ";

                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@ID", device.ID));
                    paramList.Add(new SqlParameter("@DIODeviceName", device.DIODeviceName));
                    paramList.Add(new SqlParameter("@InputPorts", device.InputPorts));
                    paramList.Add(new SqlParameter("@OutputPorts", device.OutputPorts));

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text, paramList.ToArray());
                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備機器IO設定登録・更新
        /// </summary>
        public int UpdateFacilitySetting(FacilitySetting facilitySetting)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    UPDATE 
		                    M_FACILITY_SETTING
	                    set 
		                    DIODeviceID = @DIODeviceID
		                    , IOPort = @IOPort
		                    , SettingValue = @SettingValue
		                    , Updated = GETDATE() 
	                    WHERE 
		                    ID = @SettingID";

                    SqlParameter[] sqlParameters = m_daoCommon.ConvertToSqlParams(MethodBase.GetCurrentMethod().Name, facilitySetting);

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備ステータス削除
        /// </summary>
        public int RemoveFacilityStatus()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        Delete From D_FACILITY_STATUS;
                    ";

                    int effectedRowCount = helper.ExecuteNonQuery(query, CommandType.Text);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備ステータス更新
        /// </summary>
        public int UpdateFacilityStatus(List<FacilityStatus> facilityStatusList)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    SqlParameter[] sqlParameters = m_daoCommon.ConvertListToSqlParams(facilityStatusList);

                    StringBuilder query = new StringBuilder();
                    query.AppendLine(@"MERGE INTO ");
                    query.AppendLine(@"        dbo.D_FACILITY_STATUS as Target ");
                    query.AppendLine(@"    USING (VALUES ");
                    for (int i = 0; i < facilityStatusList.Count; i++)
                    {
                        FacilityStatus stat = facilityStatusList[i];
                        string row = $"(@FacilitySID{i.ToString()}, @FacilityTypeID{i.ToString()}, @FacilityName{i.ToString()}, @Status1{i.ToString()}, " +
                            $"@Status2{i.ToString()}, @StatusOpt{i.ToString()})";
                        if (i < facilityStatusList.Count - 1)
                        {
                            row += ", ";
                        }
                        query.AppendLine(row);
                    }
                    query.AppendLine(@"          ) as Source (FacilitySID, FacilityTypeID, FacilityName, Status1, Status2, StatusOpt) ");
                    query.AppendLine(@"    ON ");
                    query.AppendLine(@"        (Target.FacilitySID = Source.FacilitySID and Target.DelFlg = 0) ");
                    query.AppendLine(@"    WHEN MATCHED THEN ");
                    query.AppendLine(@"        UPDATE SET ");
                    query.AppendLine(@"            Target.Updated = GETDATE(), ");
                    query.AppendLine(@"            Target.FacilityTypeID = Source.FacilityTypeID, ");
                    query.AppendLine(@"            Target.FacilityName = Source.FacilityName, ");
                    query.AppendLine(@"            Target.Status1 = Source.Status1, ");
                    query.AppendLine(@"            Target.Status2 = Source.Status2, ");
                    query.AppendLine(@"            Target.StatusOpt = Source.StatusOpt");
                    query.AppendLine(@"    WHEN NOT MATCHED BY Target THEN ");
                    query.AppendLine(@"        INSERT(Created, ");
                    query.AppendLine(@"                Updated, ");
                    query.AppendLine(@"                DelFlg, ");
                    query.AppendLine(@"                FacilitySID, ");
                    query.AppendLine(@"                FacilityTypeID, ");
                    query.AppendLine(@"                FacilityName, ");
                    query.AppendLine(@"                Status1, ");
                    query.AppendLine(@"                Status2, ");
                    query.AppendLine(@"                StatusOpt) ");
                    query.AppendLine(@"        VALUES(GETDATE(), ");
                    query.AppendLine(@"                GETDATE(), ");
                    query.AppendLine(@"                0, ");
                    query.AppendLine(@"                Source.FacilitySID, ");
                    query.AppendLine(@"                Source.FacilityTypeID, ");
                    query.AppendLine(@"                Source.FacilityName, ");
                    query.AppendLine(@"                Source.Status1, ");
                    query.AppendLine(@"                Source.Status2, ");
                    query.AppendLine(@"                Source.StatusOpt) ");
                    query.AppendLine(@"    WHEN NOT MATCHED BY Source THEN ");
                    query.AppendLine(@"        DELETE; ");

                    int effectedRowCount = helper.ExecuteNonQuery(query.ToString(), CommandType.Text, sqlParameters);

                    return effectedRowCount;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// DIOデバイスID重複確認、
        /// SID(データPrimaryKey)が指定された場合、指定SID以外で重複確認する
        /// </summary>
        /// <returns> DIOデバイスIDが重複する場合False、唯一の場合True</returns>
        public bool IsUnique(DIODevice dioDevice)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@DIODeviceName", dioDevice.DIODeviceName));

                    StringBuilder query = new StringBuilder();
                    query.AppendLine(@"SELECT ");
                    query.AppendLine(@"  Top (1) * ");
                    query.AppendLine(@"FROM ");
                    query.AppendLine(@"  dbo.M_DIO_DEVICE");
                    query.AppendLine(@"WHERE");
                    query.AppendLine(@"  DelFlg = 0");
                    query.AppendLine(@"  AND DIODeviceName = @DIODeviceName");
                    if (dioDevice.ID != null)
                    {
                        query.AppendLine(@"  AND ID != @ID");
                        paramList.Add(new SqlParameter("@ID", dioDevice.ID));
                    }

                    DataTable dataTable = helper.Execute(query.ToString(), CommandType.Text, paramList.ToArray());
                    return dataTable.Rows.Count <= 0;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備ID重複確認、
        /// SID(データPrimaryKey)が指定された場合、指定SID以外で重複確認する
        /// </summary>
        /// <returns> 設備IDが重複する場合False、唯一の場合True</returns>
        public bool IsUnique(Facility facility)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@ID", facility.ID));

                    StringBuilder query = new StringBuilder();
                    query.AppendLine(@"SELECT ");
                    query.AppendLine(@"  Top (1) * ");
                    query.AppendLine(@"FROM ");
                    query.AppendLine(@"  dbo.M_FACILITY");
                    query.AppendLine(@"WHERE");
                    query.AppendLine(@"  DelFlg = 0");
                    query.AppendLine(@"  AND ID = @ID");
                    if (facility.SID != null)
                    {
                        query.AppendLine(@"  AND sid != @SID");
                        paramList.Add(new SqlParameter("@SID", facility.SID));
                    }

                    DataTable dataTable = helper.Execute(query.ToString(), CommandType.Text, paramList.ToArray());
                    return dataTable.Rows.Count <= 0;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public List<FacilityStatusMsg> GetFacilityStatusMsgs()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    SELECT
		                    FacilityTypeID
                            , StatusCode
                            , StatusMsg
	                    FROM
		                    dbo.M_FACILITY_STATUS_MSG
	                    WHERE
		                    DelFlg = 0";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);
                    return m_daoCommon.ConvertToListOf<FacilityStatusMsg>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 設備情報結果セット
        /// </summary>
        public class Facility : Result
        {
            public Facility() { }

            [Browsable(false)]
            public DateTime Created { get; set; } = (DateTime)SqlDateTime.MinValue;

            [Browsable(false)]
            public DateTime Updated { get; set; } = (DateTime)SqlDateTime.MinValue;

            [Browsable(false)]
            public bool DelFlg { get; set; }

            public int? SID { get; set; }

            [DisplayName("設備ID")]
            [UserInput]
            [SqlParam(queryNames: new string[] { "insertFacility" })]
            [Pattern("[a-zA-Z0-9]{1,2}", "1~2桁英数字")]
            public string ID { get; set; }

            [DisplayName("設備名称")]
            [UserInput]
            [SqlParam(queryNames: new string[] { "insertFacility" })]
            [Pattern(".{1,50}", "1~50桁")]
            public string FacilityName { get; set; }

            [DisplayName("機器種別")]
            [AddOnly]
            [UserInput]
            [SqlParam(queryNames: new string[] { "insertFacility" })]
            [ComboInput(typeof(MFacilityDao), "GetAllFacilityTypes", "TypeName", "ID")]
            public int? FacilityTypeID { get; set; }

            public string TypeName { get; set; }

            [Browsable(false)]
            public int IsValidSetting { get; set; }

            public override bool IsValid(ref string errorMsg)
            {
                if (!new MFacilityDao().IsUnique(this))
                {
                    errorMsg = "既に登録されている設備IDです。";
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public class Facility_Coordinate : Result
        {
            public Facility_Coordinate() { }

            [Browsable(false)]
            public DateTime Created { get; set; } = (DateTime)SqlDateTime.MinValue;

            [Browsable(false)]
            public DateTime Updated { get; set; } = (DateTime)SqlDateTime.MinValue;

            [Browsable(false)]
            public bool DelFlg { get; set; }

            public int SID { get; set; }

            [DisplayName("設備ID")]
            [UserInput]
            [SqlParam(queryNames: new string[] { "insertFacility" })]
            [Pattern("[a-zA-Z0-9]{1,2}", "1~2桁英数字")]
            public string ID { get; set; }

            [DisplayName("設備名称")]
            [UserInput]
            [SqlParam(queryNames: new string[] { "insertFacility" })]
            [Pattern(".{1,50}", "1~50桁")]
            public string FacilityName { get; set; }

            [DisplayName("機器種別")]
            [AddOnly]
            [UserInput]
            [SqlParam(queryNames: new string[] { "insertFacility" })]
            [ComboInput(typeof(MFacilityDao), "GetAllFacilityTypes", "TypeName", "ID")]
            public int FacilityTypeID { get; set; }

            [DisplayName("原点X")]
            [UserInput]
            [Pattern(".{1,6}", "6桁まで")]
            [Pattern("[+-]?([0-9]*[.])?[0-9]+", "実数")]
            [SqlParam(queryNames: new string[] { "AddUnit", "UpdateFacilityCoordinate" })]
            public float Xcoordinate { get; set; }

            [DisplayName("原点Y")]
            [UserInput]
            [Pattern(".{1,6}", "6桁まで")]
            [Pattern("[+-]?([0-9]*[.])?[0-9]+", "実数")]
            [SqlParam(queryNames: new string[] { "AddUnit", "UpdateFacilityCoordinate" })]
            public float Ycoordinate { get; set; }

            [Browsable(false)]
            //[SqlParam(queryNames: new string[] { "AddUnit", "UpdateFacilityCoordinate" })]
            public int visible { get; set; }

            //public override bool IsValid(ref string errorMsg)
            //{
            //    if (!new MFacilityDao().IsUnique(this))
            //    {
            //        errorMsg = "既に登録されている設備IDです。";
            //        return false;
            //    }
            //    else
            //    {
            //        return true;
            //    }
            //}
        }

        /// <summary>
        /// 設備IO設定情報結果セット
        /// </summary>
        public class FacilitySetting : Result
        {
            public FacilitySetting() { }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public int SettingID { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public int FacilityFuncID { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public int? FacilitySID { get; set; }

            public string FacilityID { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public string facilityName { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public string SettingName { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public int IOType { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public string strIOType
            {
                get
                {
                    if (IOType == (int)IOTYPE.Output)
                    {
                        return "出力";
                    }
                    else if (IOType == (int)IOTYPE.Input)
                    {
                        return "入力";
                    }
                    else
                    {
                        throw new UserException("Invalid IOType");
                    }
                }
            }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public string DIODeviceName { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public int? DIODeviceID { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public int IOPort { get; set; }

            [SqlParam(queryNames: new string[] { "UpdateFacilitySetting" })]
            public int SettingValue { get; set; }
        }

        /// <summary>
        /// 設備種別情報結果セット
        /// </summary>
        public class FacilityType : Result
        {
            public FacilityType() { }

            public int SID { get; set; }

            public string TypeName { get; set; }

            public int? ErrorStatus { get; set; }

            public int? ResolveStatus { get; set; }
        }

        public class FacilityBtnRGB : Result
        {
            public FacilityBtnRGB() { }

            public int FacilityTypeID { get; private set; }

            public int StatusCode { get; private set; }

            public string StatusMsg { get; private set; }

            public int COLOR_R { get; set; }

            public int COLOR_G { get; set; }

            public int COLOR_B { get; set; }
        }

        /// <summary>
        /// DIOデバイス情報結果セット
        /// </summary>
        public class DIODevice : Result
        {
            //private int C_MAX_NAME_LENGTH = 50;

            public DIODevice() { }

            public DIODevice(string _dioDeviceName, int _inputPorts, int _outputPorts)
            {
                DIODeviceName = _dioDeviceName;
                InputPorts = _inputPorts;
                OutputPorts = _outputPorts;
            }

            public int? ID { get; set; }

            public short? ConnID { get; set; }

            [DisplayName("デバイス名称")]
            [UserInput]
            [SqlParam(queryNames: new string[] { "AddDIODevice" })]
            [Pattern(".{1,50}", "1~50桁")]
            public string DIODeviceName { get; set; }

            [DisplayName("入力ポート数")]
            [UserInput]
            [SqlParam(queryNames: new string[] { "AddDIODevice" })]
            [Pattern("[0-9]{1,2}", "1~2桁数値")]
            public int InputPorts { get; set; }

            [DisplayName("出力ポート数")]
            [UserInput]
            [SqlParam(queryNames: new string[] { "AddDIODevice" })]
            [Pattern("[0-9]{1,2}", "1~2桁数値")]
            public int OutputPorts { get; set; }

            public override bool IsValid(ref string errorMsg)
            {
                if (!new MFacilityDao().IsUnique(this))
                {
                    errorMsg = $"DIOデバイスIDが既に登録されています。";
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 設備ステータス
        /// </summary>
        public class FacilityStatus : Result
        {
            public int? FacilitySID { get; set; }

            public string FacilityID { get; private set; }

            public int FacilityTypeID { get; private set; }

            public int StatusCode { get; private set; }

            public string FacilityName { get; private set; }

            public int Status1 { get; set; }

            public string StatusMsg { get; private set; }

            public string Status2 { get; private set; }

            public string StatusOpt { get; private set; }

            public FacilityStatus() { }

            public FacilityStatus(int? sid, string id, int typeID, string name, int stat1, string statusMsg, string stat2, string statOpt)
            {
                FacilitySID = sid;
                FacilityID = id;
                FacilityTypeID = typeID;
                FacilityName = name;
                Status1 = stat1;
                StatusMsg = statusMsg;
                Status2 = stat2;
                StatusOpt = statOpt;
            }

            public void UpdatedStatus(FacilityStatus newStatus)
            {
                FacilitySID = newStatus.FacilitySID;
                FacilityID = newStatus.FacilityID;
                FacilityTypeID = newStatus.FacilityTypeID;
                FacilityName = newStatus.FacilityName;
                StatusCode = newStatus.StatusCode;
                if (newStatus.StatusMsg.Equals("動作中"))
                {
                    Status1 = newStatus.Status1;
                    //if (Status1 == 0)
                    //    Status1 = 5;
                    //else if (Status1 == 1)
                    //    Status1 = 6;
                    //else if (Status1 == -1 || Status1 == 99)
                    //    Status1 = newStatus.Status1;
                    //else
                    //    Status1 = newStatus.Status1;
                }
                else if (newStatus.StatusMsg.Equals("黄"))
                {
                    //if (Status1 == 0 || Status1 == 12)
                    //    Status1 = 12;
                    //else
                    Status1 = newStatus.Status1;
                }
                else if (newStatus.StatusMsg.Equals("消灯"))
                {
                    //if (Status1 == 0)
                    //    Status1 = 12;
                    //else if (Status1 == 1)
                    //    Status1 = 13;
                    //else
                    Status1 = newStatus.Status1;
                }
                else
                {
                    Status1 = newStatus.Status1;
                }
                StatusMsg = newStatus.StatusMsg;
                Status2 = newStatus.Status2;
                StatusOpt = newStatus.StatusOpt;
            }
        }

        /// <summary>
        /// 設備ステータス(tablet用)
        /// </summary>
        public class FacilityStatus_Tablet : Result
        {
            public int? SID { get; set; }

            public string FacilityID { get; private set; }

            public int FacilityTypeID { get; private set; }

            public int StatusCode { get; private set; }

            public string FacilityName { get; private set; }

            public int Status { get; set; }

            public string StatusMsg { get; private set; }

            public int StationSID { get; private set; }

            public int PlantSID { get; private set; }

            public string StatusOpt { get; private set; }

            public FacilityStatus_Tablet() { }

            public FacilityStatus_Tablet(int? sid,string facid, int typeID, string name, int stat, string statusMsg)
            {
                SID = sid;
                FacilityID = facid;
                FacilityTypeID = typeID;
                FacilityName = name;
                Status = stat;
                StatusMsg = statusMsg;
            }

            public FacilityStatus_Tablet(int? sid, string facid, int typeID, int statuscode, string name, int stat, string statusmsg,int stationsid, int plantsid, string statusopt)
            {
                SID = sid;
                FacilityID = facid;
                FacilityTypeID = typeID;
                statuscode = StatusCode;
                FacilityName = name;
                Status = stat;
                StatusMsg = statusmsg;
                StationSID = stationsid;
                PlantSID = plantsid;
                StatusOpt = statusopt; 
            }

            public void UpdatedStatus(FacilityStatus_Tablet newStatus)
            {
                SID = newStatus.SID;
                FacilityID = newStatus.FacilityID;
                FacilityTypeID = newStatus.FacilityTypeID;
                FacilityName = newStatus.FacilityName;
                StatusCode = newStatus.StatusCode;
                if (newStatus.StatusMsg.Equals("動作中"))
                {
                    Status = newStatus.Status;
                    //if (Status1 == 0)
                    //    Status1 = 5;
                    //else if (Status1 == 1)
                    //    Status1 = 6;
                    //else if (Status1 == -1 || Status1 == 99)
                    //    Status1 = newStatus.Status1;
                    //else
                    //    Status1 = newStatus.Status1;
                }
                else if (newStatus.StatusMsg.Equals("黄"))
                {
                    //if (Status1 == 0 || Status1 == 12)
                    //    Status1 = 12;
                    //else
                    Status = newStatus.Status;
                }
                else if (newStatus.StatusMsg.Equals("消灯"))
                {
                    //if (Status1 == 0)
                    //    Status1 = 12;
                    //else if (Status1 == 1)
                    //    Status1 = 13;
                    //else
                    Status = newStatus.Status;
                }
                else
                {
                    Status = newStatus.Status;
                }
                StatusMsg = newStatus.StatusMsg;
                StatusOpt = newStatus.StatusOpt;
            }
        }

        public class DIOPortSetting : Result
        {
            public int DIODeviceID { get; set; }
            public string DIODeviceName { get; set; }
            public int IOType { get; set; }
            public int IOPort { get; set; }
            public int SettingValue { get; set; }
            public int FacilityID { get; set; }
            public int FacilityTypeID { get; set; }
            public string FacilityName { get; set; }
            public string SettingName { get; set; }
            public int FacilityFuncID { get; set; }
        }

        public class DIOInput
        {
            public int DioID { get; set; }

            public int Port { get; set; }

            public int Value { get; set; }

            public DIOInput(int dioID, int port, int value)
            {
                DioID = dioID;
                Port = port;
                Value = value;
            }
        }

        public class FacilityStatusMsg : Result
        {
            public int FacilityTypeID { get;  set; }

            public int StatusCode { get;  set; }

            public string StatusMsg { get;  set; }
        }

        #region ## ReadOnly ##
        /// <summary>
        /// 読取専用設備情報取得
        /// </summary>
        public List<RFacility> GetAllrFacilityInfo()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        SELECT
		                    sid
		                    , facility.Created
                            , facility.sid AS SID
		                    , facility.ID
		                    , facility.FacilityName
		                    , facility.FacilityTypeID
		                    , facilityType.TypeName
		                    , isValid.IsValidSetting
	                    FROM
		                    dbo.M_FACILITY facility
	                    LEFT OUTER JOIN
		                    dbo.M_FACILITY_TYPE facilityType
	                    ON
		                    facility.FacilityTypeID = facilityType.ID
	                    AND facilityType.DelFlg = 0
	                    LEFT OUTER JOIN
		                    (SELECT
			                    FacilitySID,
			                    CASE 
				                    WHEN 
					                    SUM(
						                    CASE WHEN (DIODeviceID IS NOT NULL) 
								                    AND (IOPort IS NOT NULL) 
								                    AND (SettingValue IS NOT NULL) THEN 0
								                    ELSE 1 
						                    END
					                    ) > 0 THEN 0
				                    ELSE 1
			                    END AS IsValidSetting
		                    FROM
			                    dbo.M_FACILITY_SETTING
		                    WHERE
			                    DelFlg = 0
		                    GROUP BY
			                    FacilitySID
		                    ) isValid
	                    ON
		                    facility.sid = isValid.FacilitySID
	                    WHERE
		                    facility.DelFlg = 0
                        AND IsValidSetting = 1";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    return m_daoCommon.ConvertToListOf<RFacility>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        /// <summary>
        /// 読取専用設備機器bitポート設定情報取得
        /// </summary>
        public List<RDIOPortSetting> GetAllrDIOPortSettings()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
	                    SELECT 
		                    setting.DIODeviceID,
		                    device.DIODeviceName,
		                    func.IOType,
		                    setting.IOPort,
		                    setting.SettingValue,
		                    setting.FacilitySID,
                            fac.ID AS FacilityID,
		                    fac.FacilityTypeID,
		                    fac.FacilityName,
		                    func.SettingName,
		                    setting.FacilityFuncID
	                    FROM 
		                    M_FACILITY_SETTING setting
	                    INNER JOIN
		                    M_FACILITY fac
	                    ON
		                    setting.FacilitySID = fac.sid
	                    AND fac.DelFlg = 0
	                    INNER JOIN
		                    M_FACILITY_FUNC func
	                    ON
		                    setting.FacilityFuncID = func.ID
	                    AND func.DelFlg = 0
	                    INNER JOIN
		                    M_DIO_DEVICE device
	                    ON
		                    setting.DIODeviceID = device.ID
	                    AND device.DelFlg = 0
	                    ORDER BY setting.DIODeviceID, func.IOType, setting.IOPort ASC;
                    ";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    List<RDIOPortSetting> result = m_daoCommon.ConvertToListOf<RDIOPortSetting>(dataTable);

                    return result;
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public List<SettingMap> GetAllrFacilityCtrlMap()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        SELECT 
	                        map.FacilityTypeID
	                        , fType.TypeName AS FacilityTypeName
	                        , map.StatusCode
	                        , msg.StatusMsg
	                        , map.IOType
	                        , map.FuncID AS FuncID
	                        , func.Name AS FuncName
	                        , msg.ReadByOutput
                        FROM
	                        M_FACILITY_CTRL_MAP map
                        LEFT OUTER JOIN
	                        M_FACILITY_TYPE fType
                        ON
	                        map.FacilityTypeID = fType.SID
	                        AND fType.DelFlg = 0
                        LEFT OUTER JOIN
	                        M_FACILITY_STATUS_MSG msg
                        ON
	                        map.FacilityTypeID = msg.FacilityTypeID
	                        AND map.StatusCode = msg.StatusCode
	                        AND msg.DelFlg = 0
                        LEFT OUTER JOIN
	                        M_FACILITY_FUNC func
                        ON
	                        ABS(map.FuncID) = func.SID
	                        AND map.IOType = func.IOType
                    ;";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    List<RFacilityCtrlMap> mapList = m_daoCommon.ConvertToListOf<RFacilityCtrlMap>(dataTable);

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

                    return settingMaps;
                }
            }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end"); }
        }

        /// <summary>
        /// 読取専用設備情報結果セット
        /// </summary>
        public class RFacility : Result
        {
            public RFacility() { }

            public DateTime Created { get;  set; } = (DateTime)SqlDateTime.MinValue;
            public DateTime Updated { get;  set; } = (DateTime)SqlDateTime.MinValue;
            public Boolean DelFlg { get;  set; }

            public int SID { get;  set; }

            public string ID { get;  set; }

            public string Name { get;  set; }

            public int FacilityTypeID { get;  set; }

            public string TypeName { get;  set; }

            public int IsValidSetting { get;  set; }

        }

        /// <summary>
        /// 設備bitポート設定情報読取専用(Immutable)結果セット
        /// </summary>
        public class RDIOPortSetting : Result
        {
            public int? DIODeviceID { get; private set; }
            public string DIODeviceName { get; private set; }
            public int IOType { get; private set; }
            public int IOPort { get; private set; }
            public int SettingValue { get; private set; }
            public int FacilitySID { get; private set; }
            public string FacilityID { get; private set; }
            public int FacilityTypeID { get; private set; }
            public string FacilityName { get; private set; }
            public string SettingName { get; private set; }
            public int FacilityFuncID { get; private set; }
        }

        public class RFacilityCtrlMap : Result
        {
            public int FacilityTypeID { get; set; }

            public string FacilityTypeName { get; set; }

            public int StatusCode { get; set; }

            public string StatusMsg { get; set; }

            public int IOType { get; set; }

            public int FuncID { get; set; }

            public string FuncName { get; set; }

            public bool ReadByOutput { get; set; }
        }

        public class SettingMap
        {
            public int typeId { get;  set; }

            public string typeName { get;  set; }

            public int ctrlValue { get;  set; }

            public string statusMsg { get;  set; }

            public int[] SetFuncIds { get;  set; }

            public int[] GetFuncIds { get;  set; }

            public bool ReadByOutput { get;  set; }

            public SettingMap(int typeId, string typeName, int ctrlValue, string statusMsg, int[] SetFuncIds, int[] GetFuncIds, bool ReadByOutput)
            {
                this.typeId = typeId;
                this.typeName = typeName;
                this.ctrlValue = ctrlValue;
                this.statusMsg = statusMsg;
                this.SetFuncIds = SetFuncIds;
                this.GetFuncIds = GetFuncIds;
                this.ReadByOutput = ReadByOutput;
            }

            public SettingMap() { }
        }

        /// <summary>
        /// 読み取り専用設備種別リスト取得
        /// </summary>
        /// <returns></returns>
        public List<int> GetReadOnlyFacilityTypeIDList()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");
            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                                SELECT 
	                                FacilityTypeID
                                FROM M_FACILITY_FUNC 
                                GROUP BY  
	                                FacilityTypeID 
                                HAVING 
	                                SUM(
		                                CASE 
			                                WHEN IOType = 1 THEN 0 
			                                ELSE 1 
		                                END
		                                ) <= 0
                    ;";

                    DataTable dataTable = helper.Execute(query, CommandType.Text);

                    List<int> TypeIDList = new List<int>();
                    foreach (DataRow r in dataTable.Rows)
                    {
                        TypeIDList.Add((int)r[0]);
                    }

                    return TypeIDList;
                }
            }
            finally { LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end"); }
        }
        #endregion

    }
}

