using MachDBTcp.Test;
using System.Configuration;
using System.Data;
using System.Windows;
using MachDBTcp.Test;
using _8Machine_MasterComputer.Instance;
using _8Machine_MasterComputer.View;

namespace _8Machine_MasterComputer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        App()
        {
            //启动主窗口让用户选择
            var mainWindow = new MainWindow();
            mainWindow.Show();

        }

    }

}
