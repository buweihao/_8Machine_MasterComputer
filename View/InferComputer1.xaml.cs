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
using static _8Machine_MasterComputer.Instance.SingleInstance;

namespace _8Machine_MasterComputer.View
{
    /// <summary>
    /// InferComputer1.xaml 的交互逻辑
    /// </summary>
    public partial class InferComputer1 : Window
    {
        public InferComputer1()
        {
            InitializeComponent();
            SingleInstance.Instance.InitializeInferComputerViewModel();
            DataContext = SingleInstance.Instance.inferComputer1VM;
        }
    }
}
