namespace MapViewer
{
    partial class UserStatusPanel
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

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.pnl_draw = new MapViewer.MyPanel();
            this.SuspendLayout();
            // 
            // pnl_draw
            // 
            this.pnl_draw.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_draw.Location = new System.Drawing.Point(0, 0);
            this.pnl_draw.Name = "pnl_draw";
            this.pnl_draw.Size = new System.Drawing.Size(62, 36);
            this.pnl_draw.TabIndex = 0;
            this.pnl_draw.Paint += new System.Windows.Forms.PaintEventHandler(this.UserStatusPanel_Paint);
            // 
            // UserStatusPanel
            // 
            this.Controls.Add(this.pnl_draw);
            this.Name = "UserStatusPanel";
            this.Size = new System.Drawing.Size(137, 50);
            this.Load += new System.EventHandler(this.UserStatusPanel_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private MapViewer.MyPanel pnl_draw;
    }
}
