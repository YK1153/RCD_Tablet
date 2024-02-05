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
            this.btn_CarOffAndon = new System.Windows.Forms.Button();
            this.btn_carResolve = new System.Windows.Forms.Button();
            this.btn_close = new System.Windows.Forms.Button();
            this.dgvCarError = new System.Windows.Forms.DataGridView();
            this.dgvConnError = new System.Windows.Forms.DataGridView();
            this.lbl_carError = new System.Windows.Forms.Label();
            this.lbl_ConnectError = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.colorGroupBox1 = new RcdTablet.ColorGroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvConnError)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.colorGroupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_errorDetectMsg
            // 
            this.lbl_errorDetectMsg.AutoSize = true;
            this.lbl_errorDetectMsg.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 32.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lbl_errorDetectMsg.ForeColor = System.Drawing.Color.Red;
            this.lbl_errorDetectMsg.Location = new System.Drawing.Point(318, 42);
            this.lbl_errorDetectMsg.Name = "lbl_errorDetectMsg";
            this.lbl_errorDetectMsg.Size = new System.Drawing.Size(381, 43);
            this.lbl_errorDetectMsg.TabIndex = 0;
            this.lbl_errorDetectMsg.Text = "異常を検知しました";
            // 
            // btn_CarOffAndon
            // 
            this.btn_CarOffAndon.Location = new System.Drawing.Point(139, 281);
            this.btn_CarOffAndon.Name = "btn_CarOffAndon";
            this.btn_CarOffAndon.Size = new System.Drawing.Size(75, 23);
            this.btn_CarOffAndon.TabIndex = 3;
            this.btn_CarOffAndon.Text = "ブザー停止";
            this.btn_CarOffAndon.UseVisualStyleBackColor = true;
            this.btn_CarOffAndon.Click += new System.EventHandler(this.btnCarOffAndon_Click);
            // 
            // btn_carResolve
            // 
            this.btn_carResolve.Location = new System.Drawing.Point(762, 281);
            this.btn_carResolve.Name = "btn_carResolve";
            this.btn_carResolve.Size = new System.Drawing.Size(75, 23);
            this.btn_carResolve.TabIndex = 4;
            this.btn_carResolve.Text = "異常リセット";
            this.btn_carResolve.UseVisualStyleBackColor = true;
            this.btn_carResolve.Click += new System.EventHandler(this.btn_carResolve_Click);
            // 
            // btn_close
            // 
            this.btn_close.Location = new System.Drawing.Point(972, 12);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(75, 23);
            this.btn_close.TabIndex = 5;
            this.btn_close.Text = "閉じる";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dgvCarError
            // 
            this.dgvCarError.AllowUserToAddRows = false;
            this.dgvCarError.AllowUserToDeleteRows = false;
            this.dgvCarError.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCarError.Location = new System.Drawing.Point(18, 30);
            this.dgvCarError.Name = "dgvCarError";
            this.dgvCarError.RowHeadersVisible = false;
            this.dgvCarError.RowTemplate.Height = 21;
            this.dgvCarError.Size = new System.Drawing.Size(963, 234);
            this.dgvCarError.TabIndex = 6;
            // 
            // dgvConnError
            // 
            this.dgvConnError.AllowUserToAddRows = false;
            this.dgvConnError.AllowUserToDeleteRows = false;
            this.dgvConnError.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvConnError.Location = new System.Drawing.Point(21, 41);
            this.dgvConnError.Name = "dgvConnError";
            this.dgvConnError.RowHeadersVisible = false;
            this.dgvConnError.RowTemplate.Height = 21;
            this.dgvConnError.Size = new System.Drawing.Size(960, 204);
            this.dgvConnError.TabIndex = 7;
            // 
            // lbl_carError
            // 
            this.lbl_carError.AutoSize = true;
            this.lbl_carError.Font = new System.Drawing.Font("MS UI Gothic", 20F);
            this.lbl_carError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(117)))), ((int)(((byte)(161)))));
            this.lbl_carError.Location = new System.Drawing.Point(225, 108);
            this.lbl_carError.Name = "lbl_carError";
            this.lbl_carError.Size = new System.Drawing.Size(303, 27);
            this.lbl_carError.TabIndex = 8;
            this.lbl_carError.Text = "待機車両・走行車両エラー";
            // 
            // lbl_ConnectError
            // 
            this.lbl_ConnectError.AutoSize = true;
            this.lbl_ConnectError.Font = new System.Drawing.Font("MS UI Gothic", 16F);
            this.lbl_ConnectError.ForeColor = System.Drawing.Color.White;
            this.lbl_ConnectError.Location = new System.Drawing.Point(427, 1);
            this.lbl_ConnectError.Name = "lbl_ConnectError";
            this.lbl_ConnectError.Size = new System.Drawing.Size(104, 22);
            this.lbl_ConnectError.TabIndex = 9;
            this.lbl_ConnectError.Text = "接続エラー";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.dgvCarError);
            this.groupBox1.Controls.Add(this.btn_CarOffAndon);
            this.groupBox1.Controls.Add(this.btn_carResolve);
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(43, 108);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1004, 317);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(117)))), ((int)(((byte)(161)))));
            this.panel1.Controls.Add(this.label1);
            this.panel1.ForeColor = System.Drawing.Color.Black;
            this.panel1.Location = new System.Drawing.Point(0, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1004, 26);
            this.panel1.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 16F);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(356, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(247, 22);
            this.label1.TabIndex = 10;
            this.label1.Text = "待機車両・走行車両エラー";
            // 
            // colorGroupBox1
            // 
            this.colorGroupBox1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(117)))), ((int)(((byte)(161)))));
            this.colorGroupBox1.Controls.Add(this.dgvConnError);
            this.colorGroupBox1.Location = new System.Drawing.Point(43, 451);
            this.colorGroupBox1.Name = "colorGroupBox1";
            this.colorGroupBox1.Size = new System.Drawing.Size(1004, 254);
            this.colorGroupBox1.TabIndex = 11;
            this.colorGroupBox1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(117)))), ((int)(((byte)(161)))));
            this.panel2.Controls.Add(this.lbl_ConnectError);
            this.panel2.ForeColor = System.Drawing.Color.Black;
            this.panel2.Location = new System.Drawing.Point(43, 451);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1004, 26);
            this.panel2.TabIndex = 12;
            // 
            // ErrorDetect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.ClientSize = new System.Drawing.Size(1097, 737);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.colorGroupBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lbl_carError);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.lbl_errorDetectMsg);
            this.Name = "ErrorDetect";
            this.Text = "ErrorDetect";
            this.Load += new System.EventHandler(this.FormErrorDetect_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvConnError)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.colorGroupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_errorDetectMsg;
        private System.Windows.Forms.Button btn_CarOffAndon;
        private System.Windows.Forms.Button btn_carResolve;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.DataGridView dgvCarError;
        private System.Windows.Forms.DataGridView dgvConnError;
        private System.Windows.Forms.Label lbl_carError;
        private System.Windows.Forms.Label lbl_ConnectError;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private ColorGroupBox colorGroupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
    }
}