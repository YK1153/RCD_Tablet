
namespace CircleButton
{
    partial class Form1
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

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.roundButton1 = new CircleButton.RoundButton();
            this.SuspendLayout();
            // 
            // roundButton1
            // 
            this.roundButton1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("roundButton1.BackgroundImage")));
            this.roundButton1.FlatAppearance.BorderSize = 0;
            this.roundButton1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.roundButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton1.Location = new System.Drawing.Point(35, 308);
            this.roundButton1.Name = "roundButton1";
            this.roundButton1.Round = 90;
            this.roundButton1.Size = new System.Drawing.Size(130, 130);
            this.roundButton1.TabIndex = 0;
            this.roundButton1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.roundButton1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private RoundButton roundButton1;
    }
}

