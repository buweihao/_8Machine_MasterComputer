using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using _8Machine_MasterComputer.ViewModel;
using MachDBTcp.Test;
using System.Threading;
using static _8Machine_MasterComputer.Instance.SingleInstance;
using _8Machine_MasterComputer.Instance;
using MachDBTcp.Interfaces;
using MachDBTcp.Services;
using MachDBTcp.Models;
using Serilog.Events;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using System.IO;
using _8Machine_MasterComputer.View;
namespace _8Machine_MasterComputer

{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 点击按钮1（上位机），启动对应的窗口
        private void LaunchMasterComputerWindow_Click(object sender, RoutedEventArgs e)
        {
            LaunchMasterComputerWindow();
            this.Close(); // 关闭当前窗口
            MachDBTcpTestMain.MasterComputer();
        }

        // 点击按钮2（推理机1），启动对应的窗口
        private void LaunchInferComputer1Window_Click(object sender, RoutedEventArgs e)
        {
            LaunchInferComputer1Window();  // 启动推理机1窗口
            this.Close(); // 关闭当前窗口
            MachDBTcpTestMain.InferComputer1();
        }

        // 点击按钮3（推理机2），启动对应的窗口
        private void LaunchInferComputer2Window_Click(object sender, RoutedEventArgs e)
        {
            LaunchInferComputer2Window();  // 启动推理机2窗口
            this.Close(); // 关闭当前窗口
            MachDBTcpTestMain.InferComputer2();
        }

        // 点击按钮4（数据库），启动对应的窗口
        private void LaunchDataBaseWindow_Click(object sender, RoutedEventArgs e)
        {
            LaunchDatabaseWindow();  // 启动数据库窗口
            this.Close(); // 关闭当前窗口
        }

        // 启动 MasterComputer 窗口
        private static void LaunchMasterComputerWindow()
        {
            var masterWindow = new MasterComputer();
            masterWindow.Show();
        }

        // 启动 InferComputer1 窗口
        private static void LaunchInferComputer1Window()
        {
            var inferWindow1 = new InferComputer1();
            inferWindow1.Show();
        }
        // 启动 InferComputer2 窗口

        private static void LaunchInferComputer2Window()
        {
            var inferWindow2 = new InferComputer2();
            inferWindow2.Show();
        }

        private static void LaunchDatabaseWindow()
        {
            var databaseWindow = new DataBase();
            databaseWindow.Show();
        }


    }
}