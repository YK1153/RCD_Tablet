namespace RcdManagement
{
    partial class TestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisConnect = new System.Windows.Forms.Button();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.tboxLog = new System.Windows.Forms.TextBox();
            this.tbIP = new System.Windows.Forms.TextBox();
            this.lblIP = new System.Windows.Forms.Label();
            this.tbPORT = new System.Windows.Forms.TextBox();
            this.lblPORT = new System.Windows.Forms.Label();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.tbSender = new System.Windows.Forms.TextBox();
            this.lblSender = new System.Windows.Forms.Label();
            this.tbReceiver = new System.Windows.Forms.TextBox();
            this.lblReceiver = new System.Windows.Forms.Label();
            this.tbPointCode = new System.Windows.Forms.TextBox();
            this.lblPointCode = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(28, 21);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "接続";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Location = new System.Drawing.Point(109, 21);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisConnect.TabIndex = 0;
            this.btnDisConnect.Text = "切断";
            this.btnDisConnect.UseVisualStyleBackColor = true;
            this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Location = new System.Drawing.Point(13, 111);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(75, 23);
            this.btnSendMessage.TabIndex = 1;
            this.btnSendMessage.Text = "電文送信";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
            // 
            // tboxLog
            // 
            this.tboxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tboxLog.Location = new System.Drawing.Point(94, 111);
            this.tboxLog.Multiline = true;
            this.tboxLog.Name = "tboxLog";
            this.tboxLog.Size = new System.Drawing.Size(726, 204);
            this.tboxLog.TabIndex = 2;
            // 
            // tbIP
            // 
            this.tbIP.Location = new System.Drawing.Point(283, 21);
            this.tbIP.Name = "tbIP";
            this.tbIP.Size = new System.Drawing.Size(100, 19);
            this.tbIP.TabIndex = 3;
            this.tbIP.Text = "172.19.98.156";
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(242, 24);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(15, 12);
            this.lblIP.TabIndex = 4;
            this.lblIP.Text = "IP";
            // 
            // tbPORT
            // 
            this.tbPORT.Location = new System.Drawing.Point(282, 44);
            this.tbPORT.Name = "tbPORT";
            this.tbPORT.Size = new System.Drawing.Size(100, 19);
            this.tbPORT.TabIndex = 3;
            this.tbPORT.Text = "30000";
            // 
            // lblPORT
            // 
            this.lblPORT.AutoSize = true;
            this.lblPORT.Location = new System.Drawing.Point(242, 47);
            this.lblPORT.Name = "lblPORT";
            this.lblPORT.Size = new System.Drawing.Size(35, 12);
            this.lblPORT.TabIndex = 4;
            this.lblPORT.Text = "PORT";
            // 
            // tbTimeout
            // 
            this.tbTimeout.Location = new System.Drawing.Point(464, 23);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(100, 19);
            this.tbTimeout.TabIndex = 3;
            this.tbTimeout.Text = "2000";
            // 
            // lblTimeout
            // 
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new System.Drawing.Point(412, 26);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(46, 12);
            this.lblTimeout.TabIndex = 4;
            this.lblTimeout.Text = "Timeout";
            // 
            // tbSender
            // 
            this.tbSender.Location = new System.Drawing.Point(720, 23);
            this.tbSender.Name = "tbSender";
            this.tbSender.Size = new System.Drawing.Size(100, 19);
            this.tbSender.TabIndex = 3;
            this.tbSender.Text = "M1P001";
            // 
            // lblSender
            // 
            this.lblSender.AutoSize = true;
            this.lblSender.Location = new System.Drawing.Point(601, 26);
            this.lblSender.Name = "lblSender";
            this.lblSender.Size = new System.Drawing.Size(105, 12);
            this.lblSender.TabIndex = 4;
            this.lblSender.Text = "SenderLogicalName";
            // 
            // tbReceiver
            // 
            this.tbReceiver.Location = new System.Drawing.Point(720, 44);
            this.tbReceiver.Name = "tbReceiver";
            this.tbReceiver.Size = new System.Drawing.Size(100, 19);
            this.tbReceiver.TabIndex = 3;
            this.tbReceiver.Text = "M1M012";
            // 
            // lblReceiver
            // 
            this.lblReceiver.AutoSize = true;
            this.lblReceiver.Location = new System.Drawing.Point(601, 47);
            this.lblReceiver.Name = "lblReceiver";
            this.lblReceiver.Size = new System.Drawing.Size(115, 12);
            this.lblReceiver.TabIndex = 4;
            this.lblReceiver.Text = "ReceiverLogicalName";
            // 
            // tbPointCode
            // 
            this.tbPointCode.Location = new System.Drawing.Point(720, 69);
            this.tbPointCode.Name = "tbPointCode";
            this.tbPointCode.Size = new System.Drawing.Size(100, 19);
            this.tbPointCode.TabIndex = 3;
            this.tbPointCode.Text = "01";
            // 
            // lblPointCode
            // 
            this.lblPointCode.AutoSize = true;
            this.lblPointCode.Location = new System.Drawing.Point(601, 76);
            this.lblPointCode.Name = "lblPointCode";
            this.lblPointCode.Size = new System.Drawing.Size(61, 12);
            this.lblPointCode.TabIndex = 4;
            this.lblPointCode.Text = "Point Code";
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 324);
            this.Controls.Add(this.lblPointCode);
            this.Controls.Add(this.lblPORT);
            this.Controls.Add(this.lblReceiver);
            this.Controls.Add(this.lblSender);
            this.Controls.Add(this.lblTimeout);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.tbPointCode);
            this.Controls.Add(this.tbPORT);
            this.Controls.Add(this.tbReceiver);
            this.Controls.Add(this.tbSender);
            this.Controls.Add(this.tbTimeout);
            this.Controls.Add(this.tbIP);
            this.Controls.Add(this.tboxLog);
            this.Controls.Add(this.btnSendMessage);
            this.Controls.Add(this.btnDisConnect);
            this.Controls.Add(this.btnConnect);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestForm_FormClosing);
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisConnect;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.TextBox tboxLog;
        private System.Windows.Forms.TextBox tbIP;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.TextBox tbPORT;
        private System.Windows.Forms.Label lblPORT;
        private System.Windows.Forms.TextBox tbTimeout;
        private System.Windows.Forms.Label lblTimeout;
        private System.Windows.Forms.TextBox tbSender;
        private System.Windows.Forms.Label lblSender;
        private System.Windows.Forms.TextBox tbReceiver;
        private System.Windows.Forms.Label lblReceiver;
        private System.Windows.Forms.TextBox tbPointCode;
        private System.Windows.Forms.Label lblPointCode;
    }
}