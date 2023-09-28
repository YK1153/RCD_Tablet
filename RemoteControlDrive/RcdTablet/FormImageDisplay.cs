using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RcdTablet
{
    public partial class FormImageDisplay : Form
    {
        private clsTablet m_parent;
        public FormImageDisplay(clsTablet _parent)
        {
            InitializeComponent();
            m_parent = _parent;
            pb_image.Image = Image.FromFile("picture/" + m_parent.ImageNo + ".png");
            pb_image.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Size = new Size(pb_image.Size.Width + 30, pb_image.Size.Height + 30);
        }
    }
}
