namespace RcdTablet
{
    partial class EventHistory
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
            this.btn_close = new System.Windows.Forms.Button();
            this.dgv_EventHistry = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_EventHistry)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_close
            // 
            this.btn_close.Location = new System.Drawing.Point(1507, 12);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(246, 39);
            this.btn_close.TabIndex = 0;
            this.btn_close.Text = "閉じる";
            this.btn_close.UseVisualStyleBackColor = true;
            // 
            // dgv_EventHistry
            // 
            this.dgv_EventHistry.AllowUserToAddRows = false;
            this.dgv_EventHistry.AllowUserToDeleteRows = false;
            this.dgv_EventHistry.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_EventHistry.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.dgv_EventHistry.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgv_EventHistry.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_EventHistry.Location = new System.Drawing.Point(33, 60);
            this.dgv_EventHistry.Name = "dgv_EventHistry";
            this.dgv_EventHistry.RowHeadersVisible = false;
            this.dgv_EventHistry.RowTemplate.Height = 21;
            this.dgv_EventHistry.Size = new System.Drawing.Size(1720, 546);
            this.dgv_EventHistry.TabIndex = 1;
            // 
            // EventHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(61)))));
            this.ClientSize = new System.Drawing.Size(1789, 629);
            this.Controls.Add(this.dgv_EventHistry);
            this.Controls.Add(this.btn_close);
            this.Name = "EventHistory";
            this.Text = "EventHistory";
            this.Load += new System.EventHandler(this.EventHistory_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_EventHistry)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.DataGridView dgv_EventHistry;
    }
}