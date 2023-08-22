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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(clsTablet));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btn_continue = new System.Windows.Forms.Button();
            this.dgvfacility = new System.Windows.Forms.DataGridView();
            this.btn_sysstatus = new System.Windows.Forms.Button();
            this.btn_EventHistory = new System.Windows.Forms.Button();
            this.btn_DrivingHistory = new System.Windows.Forms.Button();
            this.btn_OriPosi = new System.Windows.Forms.Button();
            this.btn_andonStop = new System.Windows.Forms.Button();
            this.btn_CarResolveSndMsg = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.manageViewer2 = new MapViewer.ManageViewer();
            this.lbl_black = new System.Windows.Forms.Label();
            this.btn_warning = new System.Windows.Forms.Button();
            this.btn_errorDetect = new System.Windows.Forms.Button();
            this.btn_preparetion = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cb_stationlist = new System.Windows.Forms.ComboBox();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.userControl11 = new RcdTablet.UserControl1();
            this.elementHost2 = new System.Windows.Forms.Integration.ElementHost();
            this.onesec_btnctrl1 = new RcdTablet.onesec_btnctrl();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.dgvCarStatus = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.splitter3 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            ((System.ComponentModel.ISupportInitialize)(this.dgvfacility)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarStatus)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_continue
            // 
            this.btn_continue.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_continue.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_continue.Location = new System.Drawing.Point(35, 563);
            this.btn_continue.Name = "btn_continue";
            this.btn_continue.Size = new System.Drawing.Size(299, 61);
            this.btn_continue.TabIndex = 4;
            this.btn_continue.Text = "連続";
            this.btn_continue.UseVisualStyleBackColor = true;
            this.btn_continue.Click += new System.EventHandler(this.btn_Continue_Click);
            // 
            // dgvfacility
            // 
            this.dgvfacility.AllowUserToAddRows = false;
            this.dgvfacility.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvfacility.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvfacility.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel2.SetColumnSpan(this.dgvfacility, 4);
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvfacility.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvfacility.Location = new System.Drawing.Point(3, 85);
            this.dgvfacility.Name = "dgvfacility";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvfacility.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvfacility.RowHeadersVisible = false;
            this.dgvfacility.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvfacility.Size = new System.Drawing.Size(674, 953);
            this.dgvfacility.TabIndex = 6;
            // 
            // btn_sysstatus
            // 
            this.btn_sysstatus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_sysstatus.Location = new System.Drawing.Point(345, 33);
            this.btn_sysstatus.Name = "btn_sysstatus";
            this.btn_sysstatus.Size = new System.Drawing.Size(156, 46);
            this.btn_sysstatus.TabIndex = 9;
            this.btn_sysstatus.Text = "システムステータス";
            this.btn_sysstatus.UseVisualStyleBackColor = true;
            // 
            // btn_EventHistory
            // 
            this.btn_EventHistory.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_EventHistory.Location = new System.Drawing.Point(179, 33);
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
            this.btn_DrivingHistory.Location = new System.Drawing.Point(9, 33);
            this.btn_DrivingHistory.Name = "btn_DrivingHistory";
            this.btn_DrivingHistory.Size = new System.Drawing.Size(156, 46);
            this.btn_DrivingHistory.TabIndex = 11;
            this.btn_DrivingHistory.Text = "走行履歴";
            this.btn_DrivingHistory.UseVisualStyleBackColor = true;
            this.btn_DrivingHistory.Click += new System.EventHandler(this.btnDrivingHistory_Click);
            // 
            // btn_OriPosi
            // 
            this.btn_OriPosi.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_OriPosi.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_OriPosi.Location = new System.Drawing.Point(35, 653);
            this.btn_OriPosi.Name = "btn_OriPosi";
            this.btn_OriPosi.Size = new System.Drawing.Size(300, 52);
            this.btn_OriPosi.TabIndex = 13;
            this.btn_OriPosi.Text = "原位置復帰";
            this.btn_OriPosi.UseVisualStyleBackColor = true;
            this.btn_OriPosi.Click += new System.EventHandler(this.btn_OriPosi_Click);
            // 
            // btn_andonStop
            // 
            this.btn_andonStop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_andonStop.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_andonStop.Location = new System.Drawing.Point(35, 723);
            this.btn_andonStop.Name = "btn_andonStop";
            this.btn_andonStop.Size = new System.Drawing.Size(299, 53);
            this.btn_andonStop.TabIndex = 16;
            this.btn_andonStop.Text = "ブザー停止";
            this.btn_andonStop.UseVisualStyleBackColor = true;
            this.btn_andonStop.Click += new System.EventHandler(this.btn_andonStop_Click);
            // 
            // btn_CarResolveSndMsg
            // 
            this.btn_CarResolveSndMsg.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_CarResolveSndMsg.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_CarResolveSndMsg.Location = new System.Drawing.Point(35, 793);
            this.btn_CarResolveSndMsg.Name = "btn_CarResolveSndMsg";
            this.btn_CarResolveSndMsg.Size = new System.Drawing.Size(299, 54);
            this.btn_CarResolveSndMsg.TabIndex = 17;
            this.btn_CarResolveSndMsg.Text = "異常リセット";
            this.btn_CarResolveSndMsg.UseVisualStyleBackColor = true;
            this.btn_CarResolveSndMsg.Click += new System.EventHandler(this.btnCarResolveSndMsg_Click);
            // 
            // manageViewer2
            // 
            this.manageViewer2.Dock = System.Windows.Forms.DockStyle.Top;
            this.manageViewer2.Location = new System.Drawing.Point(380, 0);
            this.manageViewer2.Name = "manageViewer2";
            this.manageViewer2.Size = new System.Drawing.Size(835, 826);
            this.manageViewer2.TabIndex = 20;
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
            // btn_warning
            // 
            this.btn_warning.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_warning.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_warning.BackgroundImage")));
            this.btn_warning.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_warning.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_warning.Location = new System.Drawing.Point(35, 143);
            this.btn_warning.Name = "btn_warning";
            this.btn_warning.Size = new System.Drawing.Size(299, 72);
            this.btn_warning.TabIndex = 12;
            this.btn_warning.Text = "警告";
            this.btn_warning.UseVisualStyleBackColor = true;
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
            this.btn_errorDetect.Location = new System.Drawing.Point(35, 70);
            this.btn_errorDetect.Margin = new System.Windows.Forms.Padding(0);
            this.btn_errorDetect.Name = "btn_errorDetect";
            this.btn_errorDetect.Size = new System.Drawing.Size(299, 70);
            this.btn_errorDetect.TabIndex = 5;
            this.btn_errorDetect.Text = "!異常検知";
            this.btn_errorDetect.UseVisualStyleBackColor = false;
            this.btn_errorDetect.Click += new System.EventHandler(this.btn_errorDetect_Click);
            // 
            // btn_preparetion
            // 
            this.btn_preparetion.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_preparetion.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_preparetion.BackgroundImage")));
            this.btn_preparetion.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_preparetion.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.btn_preparetion.Location = new System.Drawing.Point(35, 233);
            this.btn_preparetion.Name = "btn_preparetion";
            this.btn_preparetion.Size = new System.Drawing.Size(299, 72);
            this.btn_preparetion.TabIndex = 2;
            this.btn_preparetion.Text = "運転準備";
            this.btn_preparetion.UseVisualStyleBackColor = true;
            this.btn_preparetion.Click += new System.EventHandler(this.btn_preparetion_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.cb_stationlist, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn_warning, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btn_preparetion, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.btn_OriPosi, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.btn_errorDetect, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.elementHost1, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.btn_CarResolveSndMsg, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.btn_andonStop, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.btn_continue, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.elementHost2, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(370, 1041);
            this.tableLayoutPanel1.TabIndex = 26;
            // 
            // cb_stationlist
            // 
            this.cb_stationlist.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cb_stationlist.Font = new System.Drawing.Font("MS UI Gothic", 22F);
            this.cb_stationlist.FormattingEnabled = true;
            this.cb_stationlist.Location = new System.Drawing.Point(35, 16);
            this.cb_stationlist.Name = "cb_stationlist";
            this.cb_stationlist.Size = new System.Drawing.Size(299, 37);
            this.cb_stationlist.TabIndex = 15;
            // 
            // elementHost1
            // 
            this.elementHost1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.elementHost1.BackColor = System.Drawing.Color.Transparent;
            this.elementHost1.Location = new System.Drawing.Point(35, 863);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(299, 228);
            this.elementHost1.TabIndex = 18;
            this.elementHost1.ChildChanged += new System.EventHandler<System.Windows.Forms.Integration.ChildChangedEventArgs>(this.elementHost1_ChildChanged);
            this.elementHost1.Child = this.userControl11;
            // 
            // elementHost2
            // 
            this.elementHost2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.elementHost2.Location = new System.Drawing.Point(35, 353);
            this.elementHost2.Name = "elementHost2";
            this.elementHost2.Size = new System.Drawing.Size(299, 194);
            this.elementHost2.TabIndex = 23;
            this.elementHost2.Text = "elementHost2";
            this.elementHost2.Child = this.onesec_btnctrl1;
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.splitter1.Location = new System.Drawing.Point(370, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(10, 1041);
            this.splitter1.TabIndex = 27;
            this.splitter1.TabStop = false;
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
            this.dgvCarStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvCarStatus.Location = new System.Drawing.Point(380, 895);
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
            this.dgvCarStatus.Size = new System.Drawing.Size(835, 146);
            this.dgvCarStatus.TabIndex = 7;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 164F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 168F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 182F));
            this.tableLayoutPanel2.Controls.Add(this.btn_DrivingHistory, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btn_EventHistory, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.btn_sysstatus, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.dgvfacility, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(1215, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 959F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(689, 1041);
            this.tableLayoutPanel2.TabIndex = 30;
            // 
            // splitter3
            // 
            this.splitter3.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.splitter3.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter3.Location = new System.Drawing.Point(1205, 826);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(10, 69);
            this.splitter3.TabIndex = 31;
            this.splitter3.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(380, 892);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(825, 3);
            this.splitter2.TabIndex = 32;
            this.splitter2.TabStop = false;
            // 
            // clsTablet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.splitter3);
            this.Controls.Add(this.dgvCarStatus);
            this.Controls.Add(this.manageViewer2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.lbl_black);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.ForeColor = System.Drawing.Color.Black;
            this.MaximumSize = new System.Drawing.Size(1920, 1280);
            this.MinimumSize = new System.Drawing.Size(960, 540);
            this.Name = "clsTablet";
            this.Text = "clsTablet";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.clsTablet_FormClosed);
            this.Load += new System.EventHandler(this.clsTablet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvfacility)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarStatus)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_preparetion;
        private System.Windows.Forms.Button btn_continue;
        private System.Windows.Forms.Button btn_errorDetect;
        private System.Windows.Forms.DataGridView dgvfacility;
        private System.Windows.Forms.Button btn_sysstatus;
        private System.Windows.Forms.Button btn_EventHistory;
        private System.Windows.Forms.Button btn_DrivingHistory;
        private System.Windows.Forms.Button btn_warning;
        private System.Windows.Forms.Button btn_OriPosi;
        private System.Windows.Forms.Button btn_andonStop;
        private System.Windows.Forms.Button btn_CarResolveSndMsg;
        private MapViewer.ManageViewer manageViewer1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private MapViewer.ManageViewer manageViewer2;
        private System.Windows.Forms.Label lbl_black;
        private System.Windows.Forms.Integration.ElementHost elementHost2;
        private onesec_btnctrl onesec_btnctrl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cb_stationlist;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private UserControl1 userControl11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataGridView dgvCarStatus;
        private System.Windows.Forms.Splitter splitter3;
        private System.Windows.Forms.Splitter splitter2;
    }
}

