namespace RcdTablet
{
    partial class DrivingHistory
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgv_drivingHistory = new System.Windows.Forms.DataGridView();
            this.cmbAllOrError = new System.Windows.Forms.ComboBox();
            this.btn_close = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_drivingHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_drivingHistory
            // 
            this.dgv_drivingHistory.AllowUserToAddRows = false;
            this.dgv_drivingHistory.AllowUserToDeleteRows = false;
            this.dgv_drivingHistory.AllowUserToResizeRows = false;
            this.dgv_drivingHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_drivingHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_drivingHistory.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_drivingHistory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_drivingHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_drivingHistory.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgv_drivingHistory.Location = new System.Drawing.Point(29, 68);
            this.dgv_drivingHistory.Name = "dgv_drivingHistory";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_drivingHistory.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgv_drivingHistory.RowHeadersVisible = false;
            this.dgv_drivingHistory.RowTemplate.Height = 21;
            this.dgv_drivingHistory.Size = new System.Drawing.Size(1247, 583);
            this.dgv_drivingHistory.TabIndex = 0;
            this.dgv_drivingHistory.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.formResultList_ColumnHeaderMouseClick);
            // 
            // cmbAllOrError
            // 
            this.cmbAllOrError.FormattingEnabled = true;
            this.cmbAllOrError.Items.AddRange(new object[] {
            "結果一覧",
            "成功一覧",
            "失敗一覧"});
            this.cmbAllOrError.Location = new System.Drawing.Point(29, 42);
            this.cmbAllOrError.Name = "cmbAllOrError";
            this.cmbAllOrError.Size = new System.Drawing.Size(121, 20);
            this.cmbAllOrError.TabIndex = 1;
            this.cmbAllOrError.SelectionChangeCommitted += new System.EventHandler(this.cmbALLOrError_SelectionChangeCommitted);
            // 
            // btn_close
            // 
            this.btn_close.Location = new System.Drawing.Point(1074, 12);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(202, 50);
            this.btn_close.TabIndex = 2;
            this.btn_close.Text = "閉じる";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // DrivingHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.ClientSize = new System.Drawing.Size(1302, 674);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.cmbAllOrError);
            this.Controls.Add(this.dgv_drivingHistory);
            this.Name = "DrivingHistory";
            this.Text = "DrivingHistory";
            this.Load += new System.EventHandler(this.DrivingHistory_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_drivingHistory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_drivingHistory;
        private System.Windows.Forms.ComboBox cmbAllOrError;
        private System.Windows.Forms.Button btn_close;
    }
}