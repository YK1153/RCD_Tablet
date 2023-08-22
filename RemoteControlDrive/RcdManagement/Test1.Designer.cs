
namespace RcdManagement
{
    partial class Test1
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
            this.btnTest = new System.Windows.Forms.Button();
            this.txtBodyNo = new System.Windows.Forms.TextBox();
            this.tboxLog = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbDebug = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(9, 7);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(87, 33);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "TEST";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // txtBodyNo
            // 
            this.txtBodyNo.Font = new System.Drawing.Font("MS UI Gothic", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtBodyNo.Location = new System.Drawing.Point(176, 14);
            this.txtBodyNo.Name = "txtBodyNo";
            this.txtBodyNo.Size = new System.Drawing.Size(139, 22);
            this.txtBodyNo.TabIndex = 1;
            // 
            // tboxLog
            // 
            this.tboxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tboxLog.Font = new System.Drawing.Font("MS UI Gothic", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tboxLog.Location = new System.Drawing.Point(9, 46);
            this.tboxLog.Multiline = true;
            this.tboxLog.Name = "tboxLog";
            this.tboxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tboxLog.Size = new System.Drawing.Size(779, 350);
            this.tboxLog.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(107, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "対象bodyNo";
            // 
            // txtURL
            // 
            this.txtURL.Font = new System.Drawing.Font("MS UI Gothic", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtURL.Location = new System.Drawing.Point(450, 14);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(338, 22);
            this.txtURL.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(417, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "URL";
            // 
            // cbDebug
            // 
            this.cbDebug.AutoSize = true;
            this.cbDebug.Location = new System.Drawing.Point(11, 406);
            this.cbDebug.Name = "cbDebug";
            this.cbDebug.Size = new System.Drawing.Size(15, 14);
            this.cbDebug.TabIndex = 5;
            this.cbDebug.UseVisualStyleBackColor = true;
            // 
            // Test1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 425);
            this.Controls.Add(this.cbDebug);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tboxLog);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.txtBodyNo);
            this.Controls.Add(this.btnTest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Test1";
            this.Text = "Test1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Test1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.TextBox txtBodyNo;
        private System.Windows.Forms.TextBox tboxLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbDebug;
    }
}