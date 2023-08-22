using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapViewer
{
    public class DoubleBufferingListView : ListView
    {
        public DoubleBufferingListView()
        {
            //ダブルバッファリングを有効にする
            this.SetStyle(
               ControlStyles.DoubleBuffer |
               ControlStyles.UserPaint |
               ControlStyles.AllPaintingInWmPaint,
               true);
        }
    }

    public class DoubleBufferingDataGridView : DataGridView
    {
        public DoubleBufferingDataGridView()
        {
            //ダブルバッファリングを有効にする
            this.SetStyle(
               ControlStyles.DoubleBuffer |
               ControlStyles.UserPaint |
               ControlStyles.AllPaintingInWmPaint,
               true);
        }
    }

}
