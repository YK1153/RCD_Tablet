namespace MapViewer
{
    partial class ManageBace
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
            this.pnl_view = new MapViewer.MyPanel();
            this.SuspendLayout();
            // 
            // pnl_view
            // 
            this.pnl_view.Location = new System.Drawing.Point(3, 3);
            this.pnl_view.Name = "pnl_view";
            this.pnl_view.Size = new System.Drawing.Size(200, 100);
            this.pnl_view.TabIndex = 0;
            // 
            // ManageBace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnl_view);
            this.Name = "ManageBace";
            this.Size = new System.Drawing.Size(800, 450);
            this.ResumeLayout(false);

        }

        #endregion

        public MyPanel pnl_view;
    }
}