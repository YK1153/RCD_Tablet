﻿using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using RcdCmn;
using static RcdDao.DaoCommon;
using System.ComponentModel;

namespace RcdDao
{
    public class MSystemValue
    {
        #region ### クラス定数 ###
        public static class CODE
        {
            public const string C_GROUP_MASTER = "MGRP";
        }

        public static class SUBCODE
        {
            public const string C_CTRL_UNIT = "_CUNIT";
            public const string C_CTRL_PROCESS = "_CPROC";
            public const string C_IO_FACILITY = "_IOFAC";
            public const string C_DETECT_CAM = "_SKCAM";
            public const string C_PLC_CHECK = "_PLC";
            public const string C_PLC_WATCH_CHECK = "_WATCHR";
        }
        #endregion

        #region ### クラス変数 ###

        AppLog LOGGER = AppLog.GetInstance();
        DaoCommon m_daoCommon = GetInstance();

        #endregion

        public List<SystemValue> GetSystemValueOf(string code)
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string query = @"
                        SELECT 
                            Code + SubCode AS SystemCode
                            Val
                        FROM
                            M_SYSTEM_VALUE
                        WHERE
                            Code = @Code;
                        ";

                    DataTable dataTable = helper.Execute(query, CommandType.Text, new SqlParameter("@Code", code));

                    List<SystemValue> list = m_daoCommon.ConvertToListOf<SystemValue>(dataTable);

                    return m_daoCommon.ConvertToListOf<SystemValue>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public class SystemValue : Result
        {
            public string SystemCode { get; set; }
            public string Val { get; set; }

            public SystemValue() { }
        }

        public List<MSystemMessage> GetSystemMsgOf()
        {
            LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} start");

            try
            {
                using (SqlHelper helper = m_daoCommon.getSqlHelper())
                {
                    string cmd = @"
                        SELECT
                              SID
                            , MessageCd
                            , Text
                            , Level
                        FROM
                            dbo.M_SYSTEM_MESSAGE
                        WHERE
                            DelFlg=0
                    ";

                    DataTable dataTable = helper.Execute(cmd, CommandType.Text);

                    return m_daoCommon.ConvertToListOf<MSystemMessage>(dataTable);
                }
            }
            finally
            {
                LOGGER.Debug($"{MethodBase.GetCurrentMethod().Name} end");
            }
        }

        public class MSystemMessage : Result
        {
            public MSystemMessage() { }

            public int? SID { get; set; }

            /// <summary>
            /// メッセージコード
            /// <summary>
            public string MessageCd { get; set; }
            /// <summary>
            /// 内容
            /// <summary>
            public string Text { get; set; }
            /// <summary>
            /// 異常レベル
            /// <summary>
            public int Level { get; set; }
        }
    }
}
