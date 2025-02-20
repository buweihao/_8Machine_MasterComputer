using _8Machine_MasterComputer.Instance;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// DataBase.xaml 的交互逻辑
    /// </summary>
    /// 
    public class ColumnMapping
    {
        public string ColumnName { get; set; }
        public List<string> ColumnOptions { get; set; }
        public string SelectedOption { get; set; }
    }

    public partial class DataBase : Window
    {
        private List<string> fileContent;
        private List<ColumnMapping> columnsMapping;
        private string filePath;
        private _8Machine_MachDB.Interfaces.IMachDBServices IMachDBServices = SingleInstance.Instance.IMachDBServices;
        private _8Machine_MachDB.Models.MachDBModel machDBModel = SingleInstance.Instance.machDBModel;

        // 预定义的六个列名
        private readonly List<string> availableColumns = new List<string>
        {
            "TopClearCode",
            "TopHiddenCode",
            "TopAuxiliaryCode",
            "BottomClearCode",
            "BottomHiddenCode",
            "BottomAuxiliaryCode"
        };


        public DataBase()
        {
            InitializeComponent();

            fileContent = new List<string>();
            columnsMapping = new List<ColumnMapping>();

            SingleInstance.Instance.InitializeDataBaseViewModel();
            DataContext = SingleInstance.Instance.dataBaseVM;
        }

        // 选择文件按钮事件
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                LoadFileContent(filePath);
            }
        }

        // 加载文件内容（只取第一行）
        private void LoadFileContent(string path)
        {
            try
            {
                string delimiter = DelimiterTextBox.Text; // 获取用户输入的分隔符
                var fileLines = File.ReadAllLines(path);

                if (fileLines.Length > 0)
                {
                    // 只取第一行
                    fileContent = fileLines[0].Split(new string[] { delimiter }, StringSplitOptions.None).ToList();

                    // 清空之前的列映射
                    columnsMapping.Clear();

                    // 为每列提供可选的六个列名
                    foreach (var data in fileContent)
                    {
                        var mapping = new ColumnMapping
                        {
                            ColumnName = data,
                            ColumnOptions = new List<string>(availableColumns), // 给每列提供所有六个可选项
                            SelectedOption = availableColumns[0] // 默认选择第一个选项
                        };
                        columnsMapping.Add(mapping);
                    }

                    // 更新 ListView 数据绑定
                    ColumnsListView.ItemsSource = columnsMapping;
                }
                else
                {
                    MessageBox.Show("文件为空，无法加载数据");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"文件加载失败: {ex.Message}");
            }
        }

        // 导入数据按钮事件
        private void ImportDataButton_Click(object sender, RoutedEventArgs e)
        {
            // 构建列名与选择的数据库字段映射
            var columnMappings = new Dictionary<string, string>();
            foreach (var mapping in columnsMapping)
            {
                columnMappings[mapping.ColumnName] = mapping.SelectedOption;
            }

            //2、连接数据库并初始化
            //IMachDBServices.InitMySQL("test", "root", "zkkx1413",1, machDBModel);


            //清空新表
            IMachDBServices.ClearNewBlank(machDBModel);

            // 调用后端函数导入数据
            //IMachDBServices.LoadNewBag(filePath, columnMappings, DelimiterTextBox.Text, machDBModel);
        }

        
        


    }
}
