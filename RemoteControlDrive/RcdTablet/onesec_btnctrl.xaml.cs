using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RcdTablet
{
    /// <summary>
    /// onesec_btnctrl.xaml の相互作用ロジック
    /// </summary>
    public partial class onesec_btnctrl : UserControl
    {

        public event EventHandler WpfButtonClick;

        public onesec_btnctrl()
        {
            InitializeComponent();
        }

        private void OneSecbtn_Click(object sender, RoutedEventArgs e)
        {
            // イベントを発火
            WpfButtonClick?.Invoke(sender, e);
            //CarStopSndMsg msg = new CarStopSndMsg();
            //LOGGER.Info($"走行停止ボタン押下");
            //SendTcp(msg);
        }
    }
}
