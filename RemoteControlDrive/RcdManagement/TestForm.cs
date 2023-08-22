﻿using CommWrapper;
using RcdCmn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RcdManagement
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private clsCommRFID m_rfidCL;
        //public bool RFIDCommClStatus = true;

        private void TestForm_Load(object sender, EventArgs e)
        {
            //m_rfidCL = new clsCommRFID(tbIP.Text,int.Parse(tbPORT.Text), int.Parse(tbTimeout.Text), tbSender.Text, tbReceiver.Text);

            //m_rfidCL.SendMessage += new clsCommRFID.SendMessageEventHandler(RFIDSendMessage);
            //m_rfidCL.ReceiveMessage += new clsCommRFID.ReceiveMessageEventHandler(RFIDReceiveMessage);

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // RFIDサーバ接続
            m_rfidCL = new clsCommRFID(tbIP.Text, int.Parse(tbPORT.Text), int.Parse(tbTimeout.Text), tbSender.Text, tbReceiver.Text, "01");

            m_rfidCL.SendMessage += new clsCommRFID.SendMessageEventHandler(RFIDSendMessage);
            m_rfidCL.ReceiveMessage += new clsCommRFID.ReceiveMessageEventHandler(RFIDReceiveMessage);
            m_rfidCL.RFIDClConnect();
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            m_rfidCL.RFIDClDisConnect();

            m_rfidCL = null;
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            SendMessage();
        }


        private void SendMessage()
        {

            try
            {
                m_rfidCL.RFIDInquiry();
            }
            catch (UserException ue)
            {
                MessageBox.Show(ue.Message);
            }
        }

        private List<string> m_loglist = new List<string>();
        private int C_LineCount = 30;
        private void RFIDSendMessage(SendMessageEventArgs e)
        {
            m_loglist.Add($"S → {e.Message}");

            logRefresh();
        }

        private void RFIDReceiveMessage(ReceiveMessageEventArgs e)
        {
            m_loglist.Add($"R ← {e.Message}");

            logRefresh();
        }

        private void logRefresh()
        {
            string s = "";
            for (int i = 0; i < C_LineCount; i++)
            {
                if (m_loglist.Count - 1 - i >= 0) { s = s + m_loglist[m_loglist.Count - 1 - i] + Environment.NewLine; }
            }

            Invoke(new Action(() => { tboxLog.Text = s; }));
        }

        private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(m_rfidCL != null)
            {
                if (m_rfidCL.RFIDCommClStatus)
                {
                    m_rfidCL.RFIDClDisConnect();
                }
                m_rfidCL = null;
            }
        }
    }
}
