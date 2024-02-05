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
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        //private AppLog LOGGER = AppLog.GetInstance();

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(UserControl1));
        public delegate void ChangeImageEventHandler(string path);
        public event ChangeImageEventHandler ChangeImage;

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }


        public UserControl1()
        {
            InitializeComponent();
            this.Background = System.Windows.Media.Brushes.Transparent;

            this.PreviewMouseDown += UserControl1_PreviewMouseDown;
            //DataContext = new ButtonImageViewModel();

        }

        public event EventHandler WpfButtonClick;

        private void UserControl1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as UserControl1;
            var mousePosition = e.GetPosition(button);

            // 中心からの距離を計算
            double dx = button.ActualWidth / 2 - mousePosition.X;
            double dy = button.ActualHeight / 2 - mousePosition.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            // 距離が半径以下であれば、Clickイベントを発生させる
            if (distance <= button.ActualWidth / 2)
            {
                WpfButtonClick?.Invoke(sender, e);
                //circleButton.RaiseEvent(new RoutedEventArgs(UserControl1.ClickEvent));
            }
        }

        public class ButtonImageViewModel2 : INotifyPropertyChanged
        {
            private ImageSource _ImageSource;

            public ImageSource ImageSource
            {
                get { return _ImageSource; }
                set
                {
                    _ImageSource = value;
                    OnPropertyChanged("ImageSource");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        //private void OnClick()
        //{
        //    RaiseEvent(new RoutedEventArgs(ClickEvent));
        //}

        //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    base.OnMouseLeftButtonDown(e);
        //    OnClick();
        //}

        //private void UserControl1_Click(object sender, RoutedEventArgs e)
        //{            
        //    // イベントを発火
        //    WpfButtonClick?.Invoke(sender, e);
        //    //CarStopSndMsg msg = new CarStopSndMsg();
        //    //LOGGER.Info($"走行停止ボタン押下");
        //    //SendTcp(msg);
        //}

    }
}
