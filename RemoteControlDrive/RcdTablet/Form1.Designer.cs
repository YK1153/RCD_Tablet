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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(clsTablet));
            this.dgvfacility_allow = new System.Windows.Forms.DataGridView();
            this.btn_sysstatus = new System.Windows.Forms.Button();
            this.btn_EventHistory = new System.Windows.Forms.Button();
            this.btn_DrivingHistory = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.manageViewer2 = new MapViewer.ManageViewer();
            this.btn_OriPosi = new System.Windows.Forms.Button();
            this.dgvCarStatus = new System.Windows.Forms.DataGridView();
            this.btn_close = new System.Windows.Forms.Button();
            this.cb_stationlist = new System.Windows.Forms.ComboBox();
            this.tb_allowControl = new System.Windows.Forms.TabControl();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.rB_onesec = new RcdTablet.RoundButton();
            this.lbl_black = new System.Windows.Forms.Label();
            this.rB_Stop = new RcdTablet.RoundButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btn_tb_ope = new System.Windows.Forms.Button();
            this.btn_tabnoope = new System.Windows.Forms.Button();
            this.btn_CarResolveSndMsg = new System.Windows.Forms.Button();
            this.btn_Conveyor = new System.Windows.Forms.Button();
            this.btn_andonStop = new System.Windows.Forms.Button();
            this.btn_preparetion = new System.Windows.Forms.Button();
            this.btn_continue = new System.Windows.Forms.Button();
            this.btn_errorDetect = new System.Windows.Forms.Button();
            this.btn_warning = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvfacility_allow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvfacility_allow
            // 
            this.dgvfacility_allow.AllowUserToAddRows = false;
            this.dgvfacility_allow.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
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
            this.dgvfacility_allow.EnableHeadersVisualStyles = false;
            this.dgvfacility_allow.Location = new System.Drawing.Point(1398, 212);
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
            this.dgvfacility_allow.Size = new System.Drawing.Size(464, 817);
            this.dgvfacility_allow.TabIndex = 6;
            // 
            // btn_sysstatus
            // 
            this.btn_sysstatus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_sysstatus.BackgroundImage = global::RcdTablet.Properties.Resources.bt_status_up;
            this.btn_sysstatus.Location = new System.Drawing.Point(1516, 12);
            this.btn_sysstatus.Name = "btn_sysstatus";
            this.btn_sysstatus.Size = new System.Drawing.Size(200, 50);
            this.btn_sysstatus.TabIndex = 9;
            this.btn_sysstatus.UseVisualStyleBackColor = true;
            this.btn_sysstatus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btn_General_MouseDown);
            // 
            // btn_EventHistory
            // 
            this.btn_EventHistory.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_EventHistory.Image = global::RcdTablet.Properties.Resources.bt_event_list_up;
            this.btn_EventHistory.Location = new System.Drawing.Point(1301, 12);
            this.btn_EventHistory.Name = "btn_EventHistory";
            this.btn_EventHistory.Size = new System.Drawing.Size(200, 50);
            this.btn_EventHistory.TabIndex = 10;
            this.btn_EventHistory.UseVisualStyleBackColor = true;
            this.btn_EventHistory.Click += new System.EventHandler(this.btnEventHistory_Click);
            this.btn_EventHistory.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btn_General_MouseDown);
            // 
            // btn_DrivingHistory
            // 
            this.btn_DrivingHistory.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_DrivingHistory.BackgroundImage = global::RcdTablet.Properties.Resources.bt_run_list_up;
            this.btn_DrivingHistory.Location = new System.Drawing.Point(1071, 12);
            this.btn_DrivingHistory.Name = "btn_DrivingHistory";
            this.btn_DrivingHistory.Size = new System.Drawing.Size(200, 50);
            this.btn_DrivingHistory.TabIndex = 11;
            this.btn_DrivingHistory.UseVisualStyleBackColor = true;
            this.btn_DrivingHistory.Click += new System.EventHandler(this.btnDrivingHistory_Click);
            this.btn_DrivingHistory.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btn_General_MouseDown);
            // 
            // manageViewer2
            // 
            this.manageViewer2.Location = new System.Drawing.Point(360, 107);
            this.manageViewer2.Name = "manageViewer2";
            this.manageViewer2.Size = new System.Drawing.Size(976, 741);
            this.manageViewer2.TabIndex = 20;
            // 
            // btn_OriPosi
            // 
            this.btn_OriPosi.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_OriPosi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_OriPosi.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_OriPosi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OriPosi.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_OriPosi.Image = global::RcdTablet.Properties.Resources.bt_return_off;
            this.btn_OriPosi.Location = new System.Drawing.Point(39, 567);
            this.btn_OriPosi.Name = "btn_OriPosi";
            this.btn_OriPosi.Size = new System.Drawing.Size(121, 91);
            this.btn_OriPosi.TabIndex = 13;
            this.btn_OriPosi.UseVisualStyleBackColor = false;
            this.btn_OriPosi.Click += new System.EventHandler(this.btn_OriPosi_Click);
            this.btn_OriPosi.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btn_General_MouseDown);
            // 
            // dgvCarStatus
            // 
            this.dgvCarStatus.AllowUserToAddRows = false;
            this.dgvCarStatus.AllowUserToDeleteRows = false;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
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
            this.dgvCarStatus.EnableHeadersVisualStyles = false;
            this.dgvCarStatus.Location = new System.Drawing.Point(360, 917);
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
            this.dgvCarStatus.RowHeadersWidth = 51;
            this.dgvCarStatus.Size = new System.Drawing.Size(957, 130);
            this.dgvCarStatus.TabIndex = 7;
            // 
            // btn_close
            // 
            this.btn_close.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_close.Image = global::RcdTablet.Properties.Resources.bt_close_up;
            this.btn_close.Location = new System.Drawing.Point(1740, 14);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(156, 46);
            this.btn_close.TabIndex = 24;
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            this.btn_close.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btn_General_MouseDown);
            // 
            // cb_stationlist
            // 
            this.cb_stationlist.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.cb_stationlist.FormattingEnabled = true;
            this.cb_stationlist.Location = new System.Drawing.Point(26, 24);
            this.cb_stationlist.Name = "cb_stationlist";
            this.cb_stationlist.Size = new System.Drawing.Size(591, 37);
            this.cb_stationlist.TabIndex = 25;
            this.cb_stationlist.SelectionChangeCommitted += new System.EventHandler(this.cb_stationlist_SelectionChangeCommitted);
            // 
            // tb_allowControl
            // 
            this.tb_allowControl.Enabled = false;
            this.tb_allowControl.ImageList = this.imageList1;
            this.tb_allowControl.ItemSize = new System.Drawing.Size(140, 46);
            this.tb_allowControl.Location = new System.Drawing.Point(964, 55);
            this.tb_allowControl.Margin = new System.Windows.Forms.Padding(0);
            this.tb_allowControl.Name = "tb_allowControl";
            this.tb_allowControl.Padding = new System.Drawing.Point(0, 0);
            this.tb_allowControl.SelectedIndex = 0;
            this.tb_allowControl.Size = new System.Drawing.Size(334, 46);
            this.tb_allowControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tb_allowControl.TabIndex = 27;
            this.tb_allowControl.SelectedIndexChanged += new System.EventHandler(this.tballowControl_Selected);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tab_non_ope_off.png");
            this.imageList1.Images.SetKeyName(1, "tab_non_ope_on.png");
            this.imageList1.Images.SetKeyName(2, "tab_ope_off.png");
            this.imageList1.Images.SetKeyName(3, "tab_ope_on.png");
            // 
            // elementHost1
            // 
            this.elementHost1.Enabled = false;
            this.elementHost1.Location = new System.Drawing.Point(743, 1);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(200, 100);
            this.elementHost1.TabIndex = 29;
            this.elementHost1.Child = null;
            // 
            // rB_onesec
            // 
            this.rB_onesec.FlatAppearance.BorderSize = 0;
            this.rB_onesec.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.rB_onesec.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.rB_onesec.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rB_onesec.Image = global::RcdTablet.Properties.Resources.bt_switch_right;
            this.rB_onesec.Location = new System.Drawing.Point(107, 423);
            this.rB_onesec.Name = "rB_onesec";
            this.rB_onesec.Round = 130;
            this.rB_onesec.Size = new System.Drawing.Size(130, 130);
            this.rB_onesec.TabIndex = 30;
            this.rB_onesec.UseVisualStyleBackColor = true;
            this.rB_onesec.Click += new System.EventHandler(this.btn_onesec_Click);
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
            // rB_Stop
            // 
            this.rB_Stop.BackgroundImage = global::RcdTablet.Properties.Resources.bt_exit_on;
            this.rB_Stop.FlatAppearance.BorderSize = 0;
            this.rB_Stop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.rB_Stop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.rB_Stop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rB_Stop.Location = new System.Drawing.Point(107, 910);
            this.rB_Stop.Name = "rB_Stop";
            this.rB_Stop.Round = 130;
            this.rB_Stop.Size = new System.Drawing.Size(130, 130);
            this.rB_Stop.TabIndex = 32;
            this.rB_Stop.UseVisualStyleBackColor = true;
            this.rB_Stop.Click += new System.EventHandler(this.rB_Stop_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::RcdTablet.Properties.Resources.bt_switch_single_on;
            this.pictureBox1.Location = new System.Drawing.Point(205, 380);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(96, 37);
            this.pictureBox1.TabIndex = 34;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::RcdTablet.Properties.Resources.bt_switch_continuity_on;
            this.pictureBox2.Location = new System.Drawing.Point(39, 380);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(96, 37);
            this.pictureBox2.TabIndex = 35;
            this.pictureBox2.TabStop = false;
            // 
            // btn_tb_ope
            // 
            this.btn_tb_ope.BackColor = System.Drawing.Color.Transparent;
            this.btn_tb_ope.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_tb_ope.FlatAppearance.BorderSize = 0;
            this.btn_tb_ope.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_tb_ope.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_tb_ope.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_tb_ope.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_tb_ope.Image = global::RcdTablet.Properties.Resources.tab_ope_on;
            this.btn_tb_ope.Location = new System.Drawing.Point(1422, 150);
            this.btn_tb_ope.Margin = new System.Windows.Forms.Padding(0);
            this.btn_tb_ope.Name = "btn_tb_ope";
            this.btn_tb_ope.Size = new System.Drawing.Size(140, 46);
            this.btn_tb_ope.TabIndex = 36;
            this.btn_tb_ope.UseVisualStyleBackColor = false;
            this.btn_tb_ope.Click += new System.EventHandler(this.btn_tb_ope_Click);
            // 
            // btn_tabnoope
            // 
            this.btn_tabnoope.BackColor = System.Drawing.Color.Transparent;
            this.btn_tabnoope.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_tabnoope.FlatAppearance.BorderSize = 0;
            this.btn_tabnoope.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_tabnoope.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_tabnoope.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_tabnoope.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_tabnoope.Image = global::RcdTablet.Properties.Resources.tab_non_ope_off;
            this.btn_tabnoope.Location = new System.Drawing.Point(1590, 150);
            this.btn_tabnoope.Margin = new System.Windows.Forms.Padding(0);
            this.btn_tabnoope.Name = "btn_tabnoope";
            this.btn_tabnoope.Size = new System.Drawing.Size(140, 46);
            this.btn_tabnoope.TabIndex = 37;
            this.btn_tabnoope.UseVisualStyleBackColor = false;
            this.btn_tabnoope.Click += new System.EventHandler(this.btn_tabnoope_Click);
            // 
            // btn_CarResolveSndMsg
            // 
            this.btn_CarResolveSndMsg.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_CarResolveSndMsg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_CarResolveSndMsg.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_CarResolveSndMsg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_CarResolveSndMsg.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_CarResolveSndMsg.Image = global::RcdTablet.Properties.Resources.bt_reset_off;
            this.btn_CarResolveSndMsg.Location = new System.Drawing.Point(183, 800);
            this.btn_CarResolveSndMsg.Name = "btn_CarResolveSndMsg";
            this.btn_CarResolveSndMsg.Size = new System.Drawing.Size(122, 92);
            this.btn_CarResolveSndMsg.TabIndex = 40;
            this.btn_CarResolveSndMsg.UseVisualStyleBackColor = false;
            this.btn_CarResolveSndMsg.Click += new System.EventHandler(this.btnCarResolveSndMsg_Click);
            // 
            // btn_Conveyor
            // 
            this.btn_Conveyor.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_Conveyor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_Conveyor.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_Conveyor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Conveyor.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_Conveyor.Image = global::RcdTablet.Properties.Resources.bt_conveyor_off;
            this.btn_Conveyor.Location = new System.Drawing.Point(43, 712);
            this.btn_Conveyor.Name = "btn_Conveyor";
            this.btn_Conveyor.Size = new System.Drawing.Size(262, 82);
            this.btn_Conveyor.TabIndex = 41;
            this.btn_Conveyor.UseVisualStyleBackColor = false;
            this.btn_Conveyor.Click += new System.EventHandler(this.btn_Conveyor_Click);
            // 
            // btn_andonStop
            // 
            this.btn_andonStop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_andonStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_andonStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_andonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_andonStop.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_andonStop.Image = global::RcdTablet.Properties.Resources.bt_buzzer_off;
            this.btn_andonStop.Location = new System.Drawing.Point(43, 800);
            this.btn_andonStop.Name = "btn_andonStop";
            this.btn_andonStop.Size = new System.Drawing.Size(122, 92);
            this.btn_andonStop.TabIndex = 42;
            this.btn_andonStop.UseVisualStyleBackColor = false;
            this.btn_andonStop.Click += new System.EventHandler(this.btn_andonStop_Click);
            // 
            // btn_preparetion
            // 
            this.btn_preparetion.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_preparetion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_preparetion.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_preparetion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_preparetion.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_preparetion.Image = global::RcdTablet.Properties.Resources.bt_drive_off;
            this.btn_preparetion.Location = new System.Drawing.Point(39, 292);
            this.btn_preparetion.Name = "btn_preparetion";
            this.btn_preparetion.Size = new System.Drawing.Size(262, 82);
            this.btn_preparetion.TabIndex = 43;
            this.btn_preparetion.UseVisualStyleBackColor = false;
            this.btn_preparetion.Click += new System.EventHandler(this.btn_preparetion_Click);
            // 
            // btn_continue
            // 
            this.btn_continue.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_continue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_continue.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_continue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_continue.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_continue.Image = global::RcdTablet.Properties.Resources.bt_continuity_off;
            this.btn_continue.Location = new System.Drawing.Point(179, 567);
            this.btn_continue.Name = "btn_continue";
            this.btn_continue.Size = new System.Drawing.Size(122, 91);
            this.btn_continue.TabIndex = 44;
            this.btn_continue.UseVisualStyleBackColor = false;
            this.btn_continue.Click += new System.EventHandler(this.btn_Continue_Click);
            // 
            // btn_errorDetect
            // 
            this.btn_errorDetect.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_errorDetect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_errorDetect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_errorDetect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_errorDetect.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_errorDetect.Image = global::RcdTablet.Properties.Resources.bt_alert_off;
            this.btn_errorDetect.Location = new System.Drawing.Point(39, 142);
            this.btn_errorDetect.Name = "btn_errorDetect";
            this.btn_errorDetect.Size = new System.Drawing.Size(122, 92);
            this.btn_errorDetect.TabIndex = 45;
            this.btn_errorDetect.UseVisualStyleBackColor = false;
            this.btn_errorDetect.Click += new System.EventHandler(this.btn_errorDetect_Click);
            // 
            // btn_warning
            // 
            this.btn_warning.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_warning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_warning.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.btn_warning.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_warning.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_warning.Image = global::RcdTablet.Properties.Resources.bt_warning_off;
            this.btn_warning.Location = new System.Drawing.Point(179, 142);
            this.btn_warning.Name = "btn_warning";
            this.btn_warning.Size = new System.Drawing.Size(122, 92);
            this.btn_warning.TabIndex = 46;
            this.btn_warning.UseVisualStyleBackColor = false;
            this.btn_warning.Click += new System.EventHandler(this.btn_warning_Click);
            // 
            // clsTablet
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::RcdTablet.Properties.Resources.back_1920x1080_ver2;
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.ControlBox = false;
            this.Controls.Add(this.btn_warning);
            this.Controls.Add(this.btn_errorDetect);
            this.Controls.Add(this.btn_continue);
            this.Controls.Add(this.btn_preparetion);
            this.Controls.Add(this.btn_andonStop);
            this.Controls.Add(this.btn_Conveyor);
            this.Controls.Add(this.btn_CarResolveSndMsg);
            this.Controls.Add(this.btn_tabnoope);
            this.Controls.Add(this.btn_tb_ope);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.rB_Stop);
            this.Controls.Add(this.rB_onesec);
            this.Controls.Add(this.dgvfacility_allow);
            this.Controls.Add(this.tb_allowControl);
            this.Controls.Add(this.cb_stationlist);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.btn_OriPosi);
            this.Controls.Add(this.elementHost1);
            this.Controls.Add(this.btn_sysstatus);
            this.Controls.Add(this.btn_EventHistory);
            this.Controls.Add(this.btn_DrivingHistory);
            this.Controls.Add(this.dgvCarStatus);
            this.Controls.Add(this.manageViewer2);
            this.Controls.Add(this.lbl_black);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.MinimumSize = new System.Drawing.Size(960, 540);
            this.Name = "clsTablet";
            this.Text = "clsTablet";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.clsTablet_FormClosed);
            this.Load += new System.EventHandler(this.clsTablet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvfacility_allow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        //private System.Windows.Forms.Button btn_preparetion;
        //private System.Windows.Forms.Button btn_errorDetect;
        private System.Windows.Forms.Button btn_sysstatus;
        private System.Windows.Forms.Button btn_EventHistory;
        private System.Windows.Forms.Button btn_DrivingHistory;
        //private System.Windows.Forms.Button btn_warning;
        private MapViewer.ManageViewer manageViewer1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private MapViewer.ManageViewer manageViewer2;
        private System.Windows.Forms.Label lbl_black;
        private System.Windows.Forms.DataGridView dgvCarStatus;
        private System.Windows.Forms.Button btn_OriPosi;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        //private UserControl1 userControl11;
        //private System.Windows.Forms.Button btn_CarResolveSndMsg;
        //private System.Windows.Forms.Button btn_andonStop;
        //private System.Windows.Forms.Button btn_continue;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.ComboBox cb_stationlist;
        private System.Windows.Forms.TabControl tb_allowControl;
        private System.Windows.Forms.DataGridView dgvfacility_allow;
        //private System.Windows.Forms.Button btn_Conveyor;
        private RoundButton rB_onesec;
        private RoundButton rB_Stop;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btn_tb_ope;
        private System.Windows.Forms.Button btn_tabnoope;
        private System.Windows.Forms.Button btn_CarResolveSndMsg;
        private System.Windows.Forms.Button btn_Conveyor;
        private System.Windows.Forms.Button btn_andonStop;
        private System.Windows.Forms.Button btn_preparetion;
        private System.Windows.Forms.Button btn_continue;
        private System.Windows.Forms.Button btn_errorDetect;
        private System.Windows.Forms.Button btn_warning;
    }
}

