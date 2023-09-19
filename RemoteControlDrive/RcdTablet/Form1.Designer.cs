namespace RcdTablet
{
    partial class clsTablet
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(clsTablet));
            this.dgvFacility_noControl = new System.Windows.Forms.DataGridView();
            this.dgvfacility_allow = new System.Windows.Forms.DataGridView();
            this.btn_sysstatus = new System.Windows.Forms.Button();
            this.btn_EventHistory = new System.Windows.Forms.Button();
            this.btn_DrivingHistory = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.manageViewer2 = new MapViewer.ManageViewer();
            this.btn_OriPosi = new System.Windows.Forms.Button();
            this.btn_CarResolveSndMsg = new System.Windows.Forms.Button();
            this.btn_andonStop = new System.Windows.Forms.Button();
            this.btn_continue = new System.Windows.Forms.Button();
            this.dgvCarStatus = new System.Windows.Forms.DataGridView();
            this.btn_close = new System.Windows.Forms.Button();
            this.cb_stationlist = new System.Windows.Forms.ComboBox();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.userControl11 = new RcdTablet.UserControl1();
            this.elementHost2 = new System.Windows.Forms.Integration.ElementHost();
            this.onesec_btnctrl1 = new RcdTablet.onesec_btnctrl();
            this.tb_allowControl = new System.Windows.Forms.TabControl();
            this.tp_allowControl = new System.Windows.Forms.TabPage();
            this.tp_uncontrol = new System.Windows.Forms.TabPage();
            this.btn_preparetion = new System.Windows.Forms.Button();
            this.btn_warning = new System.Windows.Forms.Button();
            this.btn_errorDetect = new System.Windows.Forms.Button();
            this.lbl_black = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFacility_noControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvfacility_allow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarStatus)).BeginInit();
            this.tb_allowControl.SuspendLayout();
            this.tp_allowControl.SuspendLayout();
            this.tp_uncontrol.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvFacility_noControl
            // 
            this.dgvFacility_noControl.AllowUserToAddRows = false;
            this.dgvFacility_noControl.AllowUserToDeleteRows = false;
            this.dgvFacility_noControl.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFacility_noControl.Location = new System.Drawing.Point(0, 0);
            this.dgvFacility_noControl.Name = "dgvFacility_noControl";
            this.dgvFacility_noControl.RowTemplate.Height = 21;
            this.dgvFacility_noControl.Size = new System.Drawing.Size(663, 901);
            this.dgvFacility_noControl.TabIndex = 28;
            // 
            // dgvfacility_allow
            // 
            this.dgvfacility_allow.AllowUserToAddRows = false;
            this.dgvfacility_allow.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvfacility_allow.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvfacility_allow.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvfacility_allow.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvfacility_allow.Location = new System.Drawing.Point(0, 0);
            this.dgvfacility_allow.Name = "dgvfacility_allow";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvfacility_allow.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvfacility_allow.RowHeadersVisible = false;
            this.dgvfacility_allow.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvfacility_allow.Size = new System.Drawing.Size(674, 903);
            this.dgvfacility_allow.TabIndex = 6;
            // 
            // btn_sysstatus
            // 
            this.btn_sysstatus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_sysstatus.Location = new System.Drawing.Point(1572, 72);
            this.btn_sysstatus.Name = "btn_sysstatus";
            this.btn_sysstatus.Size = new System.Drawing.Size(156, 46);
            this.btn_sysstatus.TabIndex = 9;
            this.btn_sysstatus.Text = "システムステータス";
            this.btn_sysstatus.UseVisualStyleBackColor = true;
            // 
            // btn_EventHistory
            // 
            this.btn_EventHistory.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_EventHistory.Location = new System.Drawing.Point(1410, 72);
            this.btn_EventHistory.Name = "btn_EventHistory";
            this.btn_EventHistory.Size = new System.Drawing.Size(156, 46);
            this.btn_EventHistory.TabIndex = 10;
            this.btn_EventHistory.Text = "イベント履歴";
            this.btn_EventHistory.UseVisualStyleBackColor = true;
            this.btn_EventHistory.Click += new System.EventHandler(this.btnEventHistory_Click);
            // 
            // btn_DrivingHistory
            // 
            this.btn_DrivingHistory.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_DrivingHistory.Location = new System.Drawing.Point(1248, 72);
            this.btn_DrivingHistory.Name = "btn_DrivingHistory";
            this.btn_DrivingHistory.Size = new System.Drawing.Size(156, 46);
            this.btn_DrivingHistory.TabIndex = 11;
            this.btn_DrivingHistory.Text = "走行履歴";
            this.btn_DrivingHistory.UseVisualStyleBackColor = true;
            this.btn_DrivingHistory.Click += new System.EventHandler(this.btnDrivingHistory_Click);
            // 
            // manageViewer2
            // 
            this.manageViewer2.Location = new System.Drawing.Point(370, 124);
            this.manageViewer2.Name = "manageViewer2";
            this.manageViewer2.Size = new System.Drawing.Size(830, 753);
            this.manageViewer2.TabIndex = 20;
            // 
            // btn_OriPosi
            // 
            this.btn_OriPosi.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_OriPosi.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_OriPosi.Location = new System.Drawing.Point(193, 566);
            this.btn_OriPosi.Name = "btn_OriPosi";
            this.btn_OriPosi.Size = new System.Drawing.Size(148, 92);
            this.btn_OriPosi.TabIndex = 13;
            this.btn_OriPosi.Text = "原位置復帰";
            this.btn_OriPosi.UseVisualStyleBackColor = true;
            this.btn_OriPosi.Click += new System.EventHandler(this.btn_OriPosi_Click);
            // 
            // btn_CarResolveSndMsg
            // 
            this.btn_CarResolveSndMsg.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_CarResolveSndMsg.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_CarResolveSndMsg.Location = new System.Drawing.Point(196, 705);
            this.btn_CarResolveSndMsg.Name = "btn_CarResolveSndMsg";
            this.btn_CarResolveSndMsg.Size = new System.Drawing.Size(145, 92);
            this.btn_CarResolveSndMsg.TabIndex = 17;
            this.btn_CarResolveSndMsg.Text = "異常リセット";
            this.btn_CarResolveSndMsg.UseVisualStyleBackColor = true;
            this.btn_CarResolveSndMsg.Click += new System.EventHandler(this.btnCarResolveSndMsg_Click);
            // 
            // btn_andonStop
            // 
            this.btn_andonStop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_andonStop.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_andonStop.Location = new System.Drawing.Point(42, 705);
            this.btn_andonStop.Name = "btn_andonStop";
            this.btn_andonStop.Size = new System.Drawing.Size(148, 92);
            this.btn_andonStop.TabIndex = 16;
            this.btn_andonStop.Text = "ブザー停止";
            this.btn_andonStop.UseVisualStyleBackColor = true;
            this.btn_andonStop.Click += new System.EventHandler(this.btn_andonStop_Click);
            // 
            // btn_continue
            // 
            this.btn_continue.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_continue.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_continue.Location = new System.Drawing.Point(39, 566);
            this.btn_continue.Name = "btn_continue";
            this.btn_continue.Size = new System.Drawing.Size(148, 92);
            this.btn_continue.TabIndex = 4;
            this.btn_continue.Text = "連続";
            this.btn_continue.UseVisualStyleBackColor = true;
            this.btn_continue.Click += new System.EventHandler(this.btn_Continue_Click);
            // 
            // dgvCarStatus
            // 
            this.dgvCarStatus.AllowUserToAddRows = false;
            this.dgvCarStatus.AllowUserToDeleteRows = false;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCarStatus.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvCarStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCarStatus.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvCarStatus.Location = new System.Drawing.Point(370, 901);
            this.dgvCarStatus.Name = "dgvCarStatus";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCarStatus.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvCarStatus.RowHeadersVisible = false;
            this.dgvCarStatus.Size = new System.Drawing.Size(830, 146);
            this.dgvCarStatus.TabIndex = 7;
            // 
            // btn_close
            // 
            this.btn_close.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_close.Location = new System.Drawing.Point(1734, 72);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(156, 46);
            this.btn_close.TabIndex = 24;
            this.btn_close.Text = "閉じる";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // cb_stationlist
            // 
            this.cb_stationlist.FormattingEnabled = true;
            this.cb_stationlist.Location = new System.Drawing.Point(39, 52);
            this.cb_stationlist.Name = "cb_stationlist";
            this.cb_stationlist.Size = new System.Drawing.Size(330, 20);
            this.cb_stationlist.TabIndex = 25;
            // 
            // elementHost1
            // 
            this.elementHost1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.elementHost1.BackColor = System.Drawing.Color.Transparent;
            this.elementHost1.Location = new System.Drawing.Point(42, 821);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(299, 228);
            this.elementHost1.TabIndex = 18;
            this.elementHost1.ChildChanged += new System.EventHandler<System.Windows.Forms.Integration.ChildChangedEventArgs>(this.elementHost1_ChildChanged);
            this.elementHost1.Child = this.userControl11;
            // 
            // elementHost2
            // 
            this.elementHost2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.elementHost2.Location = new System.Drawing.Point(42, 366);
            this.elementHost2.Name = "elementHost2";
            this.elementHost2.Size = new System.Drawing.Size(299, 194);
            this.elementHost2.TabIndex = 23;
            this.elementHost2.Text = "elementHost2";
            this.elementHost2.Child = this.onesec_btnctrl1;
            // 
            // tb_allowControl
            // 
            this.tb_allowControl.Controls.Add(this.tp_allowControl);
            this.tb_allowControl.Controls.Add(this.tp_uncontrol);
            this.tb_allowControl.Location = new System.Drawing.Point(1232, 124);
            this.tb_allowControl.Name = "tb_allowControl";
            this.tb_allowControl.SelectedIndex = 0;
            this.tb_allowControl.Size = new System.Drawing.Size(674, 941);
            this.tb_allowControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tb_allowControl.TabIndex = 27;
            // 
            // tp_allowControl
            // 
            this.tp_allowControl.AutoScroll = true;
            this.tp_allowControl.Controls.Add(this.dgvfacility_allow);
            this.tp_allowControl.Location = new System.Drawing.Point(4, 22);
            this.tp_allowControl.Name = "tp_allowControl";
            this.tp_allowControl.Padding = new System.Windows.Forms.Padding(3);
            this.tp_allowControl.Size = new System.Drawing.Size(666, 915);
            this.tp_allowControl.TabIndex = 0;
            this.tp_allowControl.Text = "tabPage1";
            this.tp_allowControl.UseVisualStyleBackColor = true;
            // 
            // tp_uncontrol
            // 
            this.tp_uncontrol.Controls.Add(this.dgvFacility_noControl);
            this.tp_uncontrol.Location = new System.Drawing.Point(4, 22);
            this.tp_uncontrol.Name = "tp_uncontrol";
            this.tp_uncontrol.Padding = new System.Windows.Forms.Padding(3);
            this.tp_uncontrol.Size = new System.Drawing.Size(666, 915);
            this.tp_uncontrol.TabIndex = 1;
            this.tp_uncontrol.Text = "tabPage2";
            this.tp_uncontrol.UseVisualStyleBackColor = true;
            // 
            // btn_preparetion
            // 
            this.btn_preparetion.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_preparetion.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_preparetion.BackgroundImage")));
            this.btn_preparetion.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_preparetion.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_preparetion.Location = new System.Drawing.Point(39, 255);
            this.btn_preparetion.Name = "btn_preparetion";
            this.btn_preparetion.Size = new System.Drawing.Size(299, 96);
            this.btn_preparetion.TabIndex = 2;
            this.btn_preparetion.Text = "運転準備";
            this.btn_preparetion.UseVisualStyleBackColor = true;
            this.btn_preparetion.Click += new System.EventHandler(this.btn_preparetion_Click);
            // 
            // btn_warning
            // 
            this.btn_warning.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_warning.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_warning.BackgroundImage")));
            this.btn_warning.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_warning.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_warning.Location = new System.Drawing.Point(190, 98);
            this.btn_warning.Name = "btn_warning";
            this.btn_warning.Size = new System.Drawing.Size(145, 96);
            this.btn_warning.TabIndex = 12;
            this.btn_warning.Text = "警告";
            this.btn_warning.UseVisualStyleBackColor = true;
            this.btn_warning.Click += new System.EventHandler(this.btn_warning_Click);
            // 
            // btn_errorDetect
            // 
            this.btn_errorDetect.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_errorDetect.BackColor = System.Drawing.SystemColors.Control;
            this.btn_errorDetect.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_errorDetect.BackgroundImage")));
            this.btn_errorDetect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_errorDetect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btn_errorDetect.FlatAppearance.BorderSize = 2;
            this.btn_errorDetect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_errorDetect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_errorDetect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_errorDetect.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_errorDetect.Location = new System.Drawing.Point(39, 98);
            this.btn_errorDetect.Margin = new System.Windows.Forms.Padding(0);
            this.btn_errorDetect.Name = "btn_errorDetect";
            this.btn_errorDetect.Size = new System.Drawing.Size(148, 96);
            this.btn_errorDetect.TabIndex = 5;
            this.btn_errorDetect.Text = "!異常検知";
            this.btn_errorDetect.UseVisualStyleBackColor = false;
            this.btn_errorDetect.Click += new System.EventHandler(this.btn_errorDetect_Click);
            // 
            // lbl_black
            // 
            this.lbl_black.AutoSize = true;
            this.lbl_black.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbl_black.Image = ((System.Drawing.Image)(resources.GetObject("lbl_black.Image")));
            this.lbl_black.Location = new System.Drawing.Point(347, 826);
            this.lbl_black.Name = "lbl_black";
            this.lbl_black.Size = new System.Drawing.Size(0, 12);
            this.lbl_black.TabIndex = 22;
            // 
            // clsTablet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.ControlBox = false;
            this.Controls.Add(this.tb_allowControl);
            this.Controls.Add(this.cb_stationlist);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.btn_OriPosi);
            this.Controls.Add(this.btn_preparetion);
            this.Controls.Add(this.elementHost1);
            this.Controls.Add(this.btn_CarResolveSndMsg);
            this.Controls.Add(this.btn_warning);
            this.Controls.Add(this.btn_andonStop);
            this.Controls.Add(this.btn_sysstatus);
            this.Controls.Add(this.btn_continue);
            this.Controls.Add(this.btn_EventHistory);
            this.Controls.Add(this.btn_DrivingHistory);
            this.Controls.Add(this.elementHost2);
            this.Controls.Add(this.btn_errorDetect);
            this.Controls.Add(this.dgvCarStatus);
            this.Controls.Add(this.manageViewer2);
            this.Controls.Add(this.lbl_black);
            this.ForeColor = System.Drawing.Color.Black;
            this.MaximumSize = new System.Drawing.Size(1920, 1280);
            this.MinimumSize = new System.Drawing.Size(960, 540);
            this.Name = "clsTablet";
            this.Text = "clsTablet";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.clsTablet_FormClosed);
            this.Load += new System.EventHandler(this.clsTablet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFacility_noControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvfacility_allow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarStatus)).EndInit();
            this.tb_allowControl.ResumeLayout(false);
            this.tp_allowControl.ResumeLayout(false);
            this.tp_uncontrol.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_preparetion;
        private System.Windows.Forms.Button btn_errorDetect;
        private System.Windows.Forms.Button btn_sysstatus;
        private System.Windows.Forms.Button btn_EventHistory;
        private System.Windows.Forms.Button btn_DrivingHistory;
        private System.Windows.Forms.Button btn_warning;
        private MapViewer.ManageViewer manageViewer1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private MapViewer.ManageViewer manageViewer2;
        private System.Windows.Forms.Label lbl_black;
        private System.Windows.Forms.DataGridView dgvCarStatus;
        private System.Windows.Forms.Button btn_OriPosi;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private UserControl1 userControl11;
        private System.Windows.Forms.Button btn_CarResolveSndMsg;
        private System.Windows.Forms.Button btn_andonStop;
        private System.Windows.Forms.Button btn_continue;
        private System.Windows.Forms.Integration.ElementHost elementHost2;
        private onesec_btnctrl onesec_btnctrl1;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.ComboBox cb_stationlist;
        private System.Windows.Forms.TabControl tb_allowControl;
        private System.Windows.Forms.TabPage tp_allowControl;
        private System.Windows.Forms.TabPage tp_uncontrol;
        private System.Windows.Forms.DataGridView dgvfacility_allow;
        private System.Windows.Forms.DataGridView dgvFacility_noControl;
    }
}

