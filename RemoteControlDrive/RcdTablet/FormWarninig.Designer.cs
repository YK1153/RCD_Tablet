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
            this.lbl_DetectContents = new System.Windows.Forms.Label();
            this.btn_CarOffAndon = new System.Windows.Forms.Button();
            this.btn_Detail = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbl_WarningDetectMsg
            // 
            this.lbl_WarningDetectMsg.AutoSize = true;
            this.lbl_WarningDetectMsg.Font = new System.Drawing.Font("MS UI Gothic", 32F);
            this.lbl_WarningDetectMsg.ForeColor = System.Drawing.Color.Red;
            this.lbl_WarningDetectMsg.Location = new System.Drawing.Point(230, 85);
            this.lbl_WarningDetectMsg.Name = "lbl_WarningDetectMsg";
            this.lbl_WarningDetectMsg.Size = new System.Drawing.Size(351, 43);
            this.lbl_WarningDetectMsg.TabIndex = 0;
            this.lbl_WarningDetectMsg.Text = "警告を検知しました";
            // 
            // lbl_DetectContents
            // 
            this.lbl_DetectContents.AutoSize = true;
            this.lbl_DetectContents.Location = new System.Drawing.Point(96, 184);
            this.lbl_DetectContents.Name = "lbl_DetectContents";
            this.lbl_DetectContents.Size = new System.Drawing.Size(83, 12);
            this.lbl_DetectContents.TabIndex = 1;
            this.lbl_DetectContents.Text = "検知内容      ：";
            // 
            // btn_CarOffAndon
            // 
            this.btn_CarOffAndon.Location = new System.Drawing.Point(224, 265);
            this.btn_CarOffAndon.Name = "btn_CarOffAndon";
            this.btn_CarOffAndon.Size = new System.Drawing.Size(75, 23);
            this.btn_CarOffAndon.TabIndex = 2;
            this.btn_CarOffAndon.Text = "ブザー停止";
            this.btn_CarOffAndon.UseVisualStyleBackColor = true;
            // 
            // btn_Detail
            // 
            this.btn_Detail.Location = new System.Drawing.Point(461, 265);
            this.btn_Detail.Name = "btn_Detail";
            this.btn_Detail.Size = new System.Drawing.Size(75, 23);
            this.btn_Detail.TabIndex = 3;
            this.btn_Detail.Text = "詳細";
            this.btn_Detail.UseVisualStyleBackColor = true;
            // 
            // FormWarninig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 355);
            this.Controls.Add(this.btn_Detail);
            this.Controls.Add(this.btn_CarOffAndon);
            this.Controls.Add(this.lbl_DetectContents);
            this.Controls.Add(this.lbl_WarningDetectMsg);
            this.Name = "FormWarninig";
            this.Text = "FormWarninig";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_WarningDetectMsg;
        private System.Windows.Forms.Label lbl_DetectContents;
        private System.Windows.Forms.Button btn_CarOffAndon;
        private System.Windows.Forms.Button btn_Detail;
    }
}