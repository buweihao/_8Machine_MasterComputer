using _8Machine_MasterComputer.Instance;
using MachDBTcp.Interfaces;
using MachDBTcp.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// MasterComputer.xaml 的交互逻辑
    /// </summary>
    public partial class MasterComputer : Window
    {

        ITcpService ITcpService ;
        public MasterComputer()
        {

            InitializeComponent();

            //获取所需实例
            ITcpService = SingleInstance.Instance.ITcpService;

            // 使用自定义控件 TextBoxWriter 封装 前端控件OutputTextBox
            TextBoxWriter _textBoxWriter = new TextBoxWriter(OutputTextBox);

            // 初始化单例 ViewModel，并传递 TextBoxWriter
            SingleInstance.Instance.InitializeMasterComputerViewModel(_textBoxWriter);

            //设置刚刚初始化好的单例作为上下文
            this.DataContext = SingleInstance.Instance.masterComputerVM;
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            // 发送测试按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Test\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel);
        }

        private void Stark_Click(object sender, RoutedEventArgs e)
        {
            // 发送开始按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Start\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel); ;
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            // 发送暂停按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Pause\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel); ;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            // 发送停止按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Stop\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel); ;
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            // 发送重新开始按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Restart\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel); ;
        }

        private void SysReset_Click(object sender, RoutedEventArgs e)
        {
            // 发送系统复位按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"sysReset\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel); ;
        }

    }

    }
    public class TextBoxWriter : TextWriter
    {
        private readonly TextBox _textBox;

        public TextBoxWriter(TextBox textBox)
        {
            _textBox = textBox;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value.ToString());
                _textBox.ScrollToEnd();
            });
        }

        public override void Write(string value)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value);
                _textBox.ScrollToEnd();
            });
        }

        public override void WriteLine(string value)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value + Environment.NewLine);
                _textBox.ScrollToEnd();
            });
        }


    }

