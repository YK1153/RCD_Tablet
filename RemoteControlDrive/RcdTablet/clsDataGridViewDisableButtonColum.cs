using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RcdTablet
{
    public class DataGridViewDisableButtonColumn : DataGridViewButtonColumn
    {
        public DataGridViewDisableButtonColumn()
        {
            this.CellTemplate = new DataGridViewDisableButtonCell();
        }
    }
}
