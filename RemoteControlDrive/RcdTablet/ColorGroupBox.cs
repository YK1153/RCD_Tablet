using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RcdTablet
{
    public partial class ColorGroupBox : GroupBox
    {
        private AutoScaleMode AutoScaleMode; 
        public ColorGroupBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.DoubleBuffer, true);

            // グループボックスの描画をオーナードローにする
            SetStyle(ControlStyles.UserPaint, true);
        }
        /// <summary>
        /// 枠線。
        /// </summary>
        private Color _borderColor = Color.Black;

        /// <summary>
        /// 枠線
        /// </summary>
        public Color BorderColor
        {
            get { return this._borderColor; }
            set
            {
                this._borderColor = value;
            }
        }

        /// <summary>
        /// OnPaintイベント
        /// </summary>
        /// <param name="e">イベントデータ</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // テキストサイズを取得
            Size tTextSize = TextRenderer.MeasureText(this.Text, this.Font);

            // グループボックスの領域を取得
            Rectangle tBorderRect = e.ClipRectangle;

            // テキストを考慮（グループボックス枠線がテキスト（高さ）の真ん中に来るように）して枠を描画
            tBorderRect.Y += tTextSize.Height / 2;
            tBorderRect.Height -= tTextSize.Height / 2;
            ControlPaint.DrawBorder(e.Graphics, tBorderRect, this._borderColor, ButtonBorderStyle.Solid);

            // テキストを描画
            Rectangle tTextRect = e.ClipRectangle;
            tTextRect.X += 6;                           // テキストの描画開始位置(X)をグループボックスの領域から6ドットずらす
            tTextRect.Width = tTextSize.Width;
            tTextRect.Height = tTextSize.Height;
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), tTextRect);
            e.Graphics.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), tTextRect);
        }
    }
}
