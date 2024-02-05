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
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
            DataContext = new ButtonImageViewModel();
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
public class ButtonImageViewModel : INotifyPropertyChanged
{
    private ImageSource _imageSource;

    public ImageSource ImageSource
    {
        get { return _imageSource; }
        set
        {
            _imageSource = value;
            OnPropertyChanged("ImageSource");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

//public class OneSecPropatyClass : INotifyPropertyChanged
//{
//    private int _onesecpropaty;

//    public int onesecpropaty
//    {
//        get { return _onesecpropaty; }
//        set
//        {
//            _onesecpropaty = value;
//            OnPropertyChanged();
//        }
//    }

//    public event PropertyChangedEventHandler PropertyChanged;

//    protected virtual void OnPropertyChanged()
//    {
//        if(onesecpropaty == 0)
//        {
//            var viewModel = new ButtonImageViewModel();
//            viewModel.ImageSource = new BitmapImage(new Uri("your_image_path_here"));
//        }
//        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//    }
//}
