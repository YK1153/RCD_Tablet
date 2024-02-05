using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CircleButton
{
    class SideRoundButton : Button
    {
        protected override void OnPaint(PaintEventArgs pevent)
        {
            float x = 0.0f;
            float y = 0.0f;
            float w = ClientSize.Width;
            float h = ClientSize.Height;
            GraphicsPath graphics = new GraphicsPath();
            graphics.StartFigure();
            graphics.AddArc(x, y, h, h, 90.0f, 180.0f);
            graphics.AddArc(w - h, y, h, h, 270.0f, 180.0f);
            graphics.CloseFigure();
            this.Region = new System.Drawing.Region(graphics);
            base.OnPaint(pevent);
        }
    }
}