namespace RcdTablet
{
    partial class ErrorDetect
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
            this.lbl_errorDetectMsg = new System.Windows.Forms.Label();
            this.lbl_boddyNo = new System.Windows.Forms.Label();
            this.lbl_Errorcontents = new System.Windows.Forms.Label();
            this.btn_CarOffAndon = new System.Windows.Forms.Button();
            this.btn_carResolve = new System.Windows.Forms.Button();
            this.btn_close = new System.Windows.Forms.Button();
            this.lbl_bodyNo_Value = new System.Windows.Forms.Label();
            this.lbl_emergency_value = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_errorDetectMsg
            // 
            this.lbl_errorDetectMsg.AutoSize = true;
            this.lbl_errorDetectMsg.Font = new System.Drawing.Font("MS UI Gothic", 32F);
            this.lbl_errorDetectMsg.ForeColor = System.Drawing.Color.Red;
            this.lbl_errorDetectMsg.Location = new System.Drawing.Point(227, 92);
            this.lbl_errorDetectMsg.Name = "lbl_errorDetectMsg";
            this.lbl_errorDetectMsg.Size = new System.Drawing.Size(351, 43);
            this.lbl_errorDetectMsg.TabIndex = 0;
            this.lbl_errorDetectMsg.Text = "異常を検知しました";
            // 
            // lbl_boddyNo
            // 
            this.lbl_boddyNo.AutoSize = true;
            this.lbl_boddyNo.Location = new System.Drawing.Point(110, 195);
            this.lbl_boddyNo.Name = "lbl_boddyNo";
            this.lbl_boddyNo.Size = new System.Drawing.Size(67, 12);
            this.lbl_boddyNo.TabIndex = 1;
            this.lbl_boddyNo.Text = "ボデーNo    :";
            // 
            // lbl_Errorcontents
            // 
            this.lbl_Errorcontents.AutoSize = true;
            this.lbl_Errorcontents.Location = new System.Drawing.Point(110, 252);
            this.lbl_Errorcontents.Name = "lbl_Errorcontents";
            this.lbl_Errorcontents.Size = new System.Drawing.Size(67, 12);
            this.lbl_Errorcontents.TabIndex = 2;
            this.lbl_Errorcontents.Text = "異常内容   :";
            // 
            // btn_CarOffAndon
            // 
            this.btn_CarOffAndon.Location = new System.Drawing.Point(144, 349);
            this.btn_CarOffAndon.Name = "btn_CarOffAndon";
            this.btn_CarOffAndon.Size = new System.Drawing.Size(75, 23);
            this.btn_CarOffAndon.TabIndex = 3;
            this.btn_CarOffAndon.Text = "ブザー停止";
            this.btn_CarOffAndon.UseVisualStyleBackColor = true;
            this.btn_CarOffAndon.Click += new System.EventHandler(this.btnCarOffAndon_Click);
            // 
            // btn_carResolve
            // 
            this.btn_carResolve.Location = new System.Drawing.Point(503, 349);
            this.btn_carResolve.Name = "btn_carResolve";
            this.btn_carResolve.Size = new System.Drawing.Size(75, 23);
            this.btn_carResolve.TabIndex = 4;
            this.btn_carResolve.Text = "異常リセット";
            this.btn_carResolve.UseVisualStyleBackColor = true;
            this.btn_carResolve.Click += new System.EventHandler(this.btn_carResolve_Click);
            // 
            // btn_close
            // 
            this.btn_close.Location = new System.Drawing.Point(713, 12);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(75, 23);
            this.btn_close.TabIndex = 5;
            this.btn_close.Text = "閉じる";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lbl_bodyNo_Value
            // 
            this.lbl_bodyNo_Value.AutoSize = true;
            this.lbl_bodyNo_Value.Location = new System.Drawing.Point(184, 195);
            this.lbl_bodyNo_Value.Name = "lbl_bodyNo_Value";
            this.lbl_bodyNo_Value.Size = new System.Drawing.Size(0, 12);
            this.lbl_bodyNo_Value.TabIndex = 6;
            // 
            // lbl_emergency_value
            // 
            this.lbl_emergency_value.AutoSize = true;
            this.lbl_emergency_value.Location = new System.Drawing.Point(184, 252);
            this.lbl_emergency_value.Name = "lbl_emergency_value";
            this.lbl_emergency_value.Size = new System.Drawing.Size(0, 12);
            this.lbl_emergency_value.TabIndex = 7;
            // 
            // ErrorDetect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lbl_emergency_value);
            this.Controls.Add(this.lbl_bodyNo_Value);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.btn_carResolve);
            this.Controls.Add(this.btn_CarOffAndon);
            this.Controls.Add(this.lbl_Errorcontents);
            this.Controls.Add(this.lbl_boddyNo);
            this.Controls.Add(this.lbl_errorDetectMsg);
            this.Name = "ErrorDetect";
            this.Text = "ErrorDetect";
            this.Load += new System.EventHandler(this.FormErrorDetect_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_errorDetectMsg;
        private System.Windows.Forms.Label lbl_boddyNo;
        private System.Windows.Forms.Label lbl_Errorcontents;
        private System.Windows.Forms.Button btn_CarOffAndon;
        private System.Windows.Forms.Button btn_carResolve;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.Label lbl_bodyNo_Value;
        private System.Windows.Forms.Label lbl_emergency_value;
    }
}