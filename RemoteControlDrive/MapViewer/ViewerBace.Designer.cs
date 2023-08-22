namespace MapViewer
{
    partial class ViewerBace
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
            this.pnl_view = new MapViewer.MyPanel();
            this.SuspendLayout();
            // 
            // pnl_view
            // 
            this.pnl_view.Location = new System.Drawing.Point(0, 0);
            this.pnl_view.Name = "pnl_view";
            this.pnl_view.Size = new System.Drawing.Size(124, 100);
            this.pnl_view.TabIndex = 0;
            this.pnl_view.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnl_view_MouseDown);
            this.pnl_view.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnl_view_MouseMove);
            this.pnl_view.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnl_view_MouseUp);
            // 
            // ViewerBace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.pnl_view);
            this.Name = "ViewerBace";
            this.ResumeLayout(false);

        }

        #endregion

        public MyPanel pnl_view;
    }
}
