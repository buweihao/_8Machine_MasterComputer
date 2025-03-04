using _8Machine_MasterComputer.Instance;
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
using System.Windows.Shapes;

namespace _8Machine_MasterComputer.View
{
    /// <summary>
    /// InferComputer2.xaml 的交互逻辑
    /// </summary>
    public partial class InferComputer2 : Window
    {
        public InferComputer2()
        {
            InitializeComponent();
            SingleInstance.Instance.InitializeInferComputer2ViewModel();
            DataContext = SingleInstance.Instance.inferComputer2VM;
        }
        // 更新 UI 界面的 Image
        private void UpdateImage(BitmapSource bitmap)
        {
            CameraImage.Source = bitmap;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }



}
