namespace RcdOperation.Control
{
    partial class DrawConfigForm
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
            this.dgv_otherarea = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.gbox_drawconfig = new System.Windows.Forms.GroupBox();
            this.rbtn_drawgetarea = new System.Windows.Forms.RadioButton();
            this.rbtn_drawangle = new System.Windows.Forms.RadioButton();
            this.cbox_drawauto = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_otherarea)).BeginInit();
            this.gbox_drawconfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv_otherarea
            // 
            this.dgv_otherarea.AllowUserToAddRows = false;
            this.dgv_otherarea.AllowUserToDeleteRows = false;
            this.dgv_otherarea.AllowUserToResizeColumns = false;
            this.dgv_otherarea.AllowUserToResizeRows = false;
            this.dgv_otherarea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_otherarea.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_otherarea.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_otherarea.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgv_otherarea.Location = new System.Drawing.Point(2, 128);
            this.dgv_otherarea.MultiSelect = false;
            this.dgv_otherarea.Name = "dgv_otherarea";
            this.dgv_otherarea.ReadOnly = true;
            this.dgv_otherarea.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgv_otherarea.RowHeadersVisible = false;
            this.dgv_otherarea.RowTemplate.Height = 21;
            this.dgv_otherarea.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_otherarea.Size = new System.Drawing.Size(194, 341);
            this.dgv_otherarea.TabIndex = 17;
            this.dgv_otherarea.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_otherarea_CellClick);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 12);
            this.label1.TabIndex = 18;
            this.label1.Text = "制御エリア描画設定";
            // 
            // gbox_drawconfig
            // 
            this.gbox_drawconfig.Controls.Add(this.rbtn_drawgetarea);
            this.gbox_drawconfig.Controls.Add(this.rbtn_drawangle);
            this.gbox_drawconfig.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.gbox_drawconfig.Location = new System.Drawing.Point(8, 38);
            this.gbox_drawconfig.Name = "gbox_drawconfig";
            this.gbox_drawconfig.Size = new System.Drawing.Size(157, 66);
            this.gbox_drawconfig.TabIndex = 19;
            this.gbox_drawconfig.TabStop = false;
            this.gbox_drawconfig.Text = "描画対象";
            this.gbox_drawconfig.Visible = false;
            // 
            // rbtn_drawgetarea
            // 
            this.rbtn_drawgetarea.AutoSize = true;
            this.rbtn_drawgetarea.Location = new System.Drawing.Point(22, 41);
            this.rbtn_drawgetarea.Name = "rbtn_drawgetarea";
            this.rbtn_drawgetarea.Size = new System.Drawing.Size(119, 16);
            this.rbtn_drawgetarea.TabIndex = 0;
            this.rbtn_drawgetarea.Text = "測位取得範囲表示";
            this.rbtn_drawgetarea.UseVisualStyleBackColor = true;
            //this.rbtn_drawgetarea.CheckedChanged += new System.EventHandler(this.rbtn_drawgetarea_CheckedChanged);
            // 
            // rbtn_drawangle
            // 
            this.rbtn_drawangle.AutoSize = true;
            this.rbtn_drawangle.Checked = true;
            this.rbtn_drawangle.Location = new System.Drawing.Point(22, 21);
            this.rbtn_drawangle.Name = "rbtn_drawangle";
            this.rbtn_drawangle.Size = new System.Drawing.Size(71, 16);
            this.rbtn_drawangle.TabIndex = 0;
            this.rbtn_drawangle.TabStop = true;
            this.rbtn_drawangle.Text = "舵角表示";
            this.rbtn_drawangle.UseVisualStyleBackColor = true;
            //this.rbtn_drawangle.CheckedChanged += new System.EventHandler(this.rbtn_drawangle_CheckedChanged);
            // 
            // cbox_drawauto
            // 
            this.cbox_drawauto.AutoSize = true;
            this.cbox_drawauto.Location = new System.Drawing.Point(8, 12);
            this.cbox_drawauto.Name = "cbox_drawauto";
            this.cbox_drawauto.Size = new System.Drawing.Size(108, 16);
            this.cbox_drawauto.TabIndex = 20;
            this.cbox_drawauto.Text = "車両画面外追尾";
            this.cbox_drawauto.UseVisualStyleBackColor = true;
            this.cbox_drawauto.CheckedChanged += new System.EventHandler(this.cbox_drawauto_CheckedChanged);
            // 
            // DrawConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(199, 471);
            this.Controls.Add(this.cbox_drawauto);
            this.Controls.Add(this.gbox_drawconfig);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgv_otherarea);
            this.Name = "DrawConfigForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DrawConfigForm_FormClosing);
            this.Load += new System.EventHandler(this.DrawConfigFormcs_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_otherarea)).EndInit();
            this.gbox_drawconfig.ResumeLayout(false);
            this.gbox_drawconfig.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbox_drawconfig;
        private System.Windows.Forms.RadioButton rbtn_drawgetarea;
        private System.Windows.Forms.RadioButton rbtn_drawangle;
        private System.Windows.Forms.CheckBox cbox_drawauto;
        public System.Windows.Forms.DataGridView dgv_otherarea;
    }
}