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
    class RoundButton : Button
    {
        protected override void OnPaint(PaintEventArgs pevent)
        {
            float r = pr;
            float x = 0.0f;
            float y = 0.0f;
            float w = ClientSize.Width;
            float h = ClientSize.Height;
            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddArc(x, y, r, r, 180.0f, 90.0f);
            gp.AddArc(w - r, y, r, r, 270.0f, 90.0f);
            gp.AddArc(w - r, h - r, r, r, 0.0f, 90.0f);
            gp.AddArc(x, h - r, r, r, 90.0f, 90.0f);
            gp.CloseFigure();
            this.Region = new System.Drawing.Region(gp);
            base.OnPaint(pevent);
        }
        private int pr = 50;
        public int Round
        {
            set
            {
                pr = value;
            }
            get
            {
                return pr;
            }
        }
    }
}