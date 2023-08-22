using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RcdManagement
{
    public partial class RcdManagement : Form
    {
        public clsRcdManagement m_RCDmanagerSrv;
        bool m_dbInitialized;

        public RcdManagement()
        {
            //icon setting
            this.ShowInTaskbar = false;
            setComponents();

            OnStart();

        }

        private void DBConnectionConfirm()
        {
            // DB接続情報確認(Control Center)
            string C_IP = ManagementConst.C_CC_DB_IP;
            string C_DB = ManagementConst.C_CC_DB_NAME;
            string C_USER = ManagementConst.C_CC_DB_USER;
            string C_PASSWORD = ManagementConst.C_CC_DB_PASSWORD;
            m_dbInitialized = RcdDao.DaoCommon.Initialize(C_IP, C_DB, C_USER, C_PASSWORD);

            if (m_dbInitialized == false)
            {
                ApplicationExit();
            }

            // DB接続情報確認(Management PC)
            C_IP = ManagementConst.C_MP_DB_IP;
            C_DB = ManagementConst.C_MP_DB_NAME;
            C_USER = ManagementConst.C_MP_DB_USER;
            C_PASSWORD = ManagementConst.C_MP_DB_PASSWORD;
            m_dbInitialized = RcdDao.DaoCommon.Initialize(C_IP, C_DB, C_USER, C_PASSWORD);

            if (m_dbInitialized == false)
            {
                ApplicationExit();
            }
        }

        protected void OnStart()
        {
            //LOGGER.Debug($"管制サービス Start");
            m_RCDmanagerSrv = clsRcdManagement.GetInstance();
        }

        protected void OnStop()
        {
            //LOGGER.Debug($"管制サービス End");
            //m_managerSrv.Dispose();
            m_RCDmanagerSrv.Dispose();
        }

        NotifyIcon notifyIcon;
        private void setComponents()
        {
            notifyIcon = new NotifyIcon();
            Bitmap ico = new Bitmap(Properties.Resources.icon);
            // icon settings
            notifyIcon.Icon = Icon.FromHandle(ico.GetHicon());
            // show icon
            notifyIcon.Visible = true;
            // text on hover
            notifyIcon.Text = "Management";

            // context menu
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Text = "&Exit";
            toolStripMenuItem.Click += ToolStripMenuItem_Click;
            contextMenuStrip.Items.Add(toolStripMenuItem);
            notifyIcon.ContextMenuStrip = contextMenuStrip;
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationExit();
        }


        private void ApplicationExit()
        {
            OnStop();
            // exit application
            Application.Exit();
            this.Close();
        }
    }
}
