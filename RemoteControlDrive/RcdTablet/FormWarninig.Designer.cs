namespace RcdTablet
{
    partial class FormWarninig
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
            this.lbl_WarningDetectMsg = new System.Windows.Forms.Label();
            this.dgv_warningList = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_warningList)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_WarningDetectMsg
            // 
            this.lbl_WarningDetectMsg.AutoSize = true;
            this.lbl_WarningDetectMsg.Font = new System.Drawing.Font("MS UI Gothic", 32F);
            this.lbl_WarningDetectMsg.ForeColor = System.Drawing.Color.White;
            this.lbl_WarningDetectMsg.Location = new System.Drawing.Point(285, 32);
            this.lbl_WarningDetectMsg.Name = "lbl_WarningDetectMsg";
            this.lbl_WarningDetectMsg.Size = new System.Drawing.Size(191, 43);
            this.lbl_WarningDetectMsg.TabIndex = 0;
            this.lbl_WarningDetectMsg.Text = "警告一覧";
            // 
            // dgv_warningList
            // 
            this.dgv_warningList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_warningList.Location = new System.Drawing.Point(46, 100);
            this.dgv_warningList.Name = "dgv_warningList";
            this.dgv_warningList.RowTemplate.Height = 21;
            this.dgv_warningList.Size = new System.Drawing.Size(702, 243);
            this.dgv_warningList.TabIndex = 1;
            // 
            // FormWarninig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.ClientSize = new System.Drawing.Size(800, 355);
            this.Controls.Add(this.dgv_warningList);
            this.Controls.Add(this.lbl_WarningDetectMsg);
            this.Name = "FormWarninig";
            this.Text = "FormWarninig";
            this.Load += new System.EventHandler(this.DrivingHistory_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_warningList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_WarningDetectMsg;
        private System.Windows.Forms.DataGridView dgv_warningList;
    }
}