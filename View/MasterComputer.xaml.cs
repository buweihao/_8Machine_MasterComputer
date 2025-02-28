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
using MachDBTcp.Services;
using System.Net.Sockets;
using System.Reflection.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime;
using Microsoft.Extensions.Configuration;
using _8Machine_MachDB.Interfaces;
using _8Machine_MachDB.Models;
namespace _8Machine_MasterComputer.View
{
    /// <summary>
    /// MasterComputer.xaml 的交互逻辑
    /// </summary>
    public partial class MasterComputer : Window
    {

        ITcpService ITcpService;
        public MasterComputer()
        {

            InitializeComponent();

            //加载数据库名字的下拉框
            LoadDataBaseNames();

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
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel, SingleInstance.Instance.tcpSerModel.BoardCard_DataBaseTcpClient);
        }

        private void Stark_Click(object sender, RoutedEventArgs e)
        {
            // 发送开始按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Start\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel, SingleInstance.Instance.tcpSerModel.BoardCard_DataBaseTcpClient); ;
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            // 发送暂停按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Pause\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel, SingleInstance.Instance.tcpSerModel.BoardCard_DataBaseTcpClient); ;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            // 发送停止按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Stop\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel, SingleInstance.Instance.tcpSerModel.BoardCard_DataBaseTcpClient); ;
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            // 发送重新开始按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Restart\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel, SingleInstance.Instance.tcpSerModel.BoardCard_DataBaseTcpClient); ;
        }

        private void SysReset_Click(object sender, RoutedEventArgs e)
        {
            // 发送系统复位按钮的 JSON 数据
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"sysReset\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel, SingleInstance.Instance.tcpSerModel.BoardCard_DataBaseTcpClient); ;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string data = "{\"Cmd\":\"Msg_ctl\",\"Subcmd\":\"Test\"}";
            ITcpService.SendToBoardCard(ref data, SingleInstance.Instance.tcpSerModel, SingleInstance.Instance.tcpSerModel.BoardCard_DataBaseTcpClient);
        }


        // 获取文件列表按钮点击事件（上标刻和下标刻共用）
        private void btnGetFileList_Click(object sender, RoutedEventArgs e)
        {
            TcpSerModel serModel = SingleInstance.Instance.tcpSerModel;
            string data = string.Empty;

            // 构建12034请求
            SingleInstance.Instance.IJsonServices.BuildJson_12034(ref data);

            // 发送12034请求，获取文件列表
            string responseUp = SingleInstance.Instance.ITcpService.SendToMarkingSoftwareAndWaitForReturn(ref data, serModel.MarkingClientUp, serModel);
            string responseDn = SingleInstance.Instance.ITcpService.SendToMarkingSoftwareAndWaitForReturn(ref data, serModel.MarkingClientDn, serModel);


            // 解析上标刻文件列表
            if (!string.IsNullOrEmpty(responseUp))
            {
                var jsonResponse = JObject.Parse(responseUp);
                var files = jsonResponse["files"]?.ToObject<List<string>>();

                if (files != null)
                {
                    // 将文件列表加载到上标刻和下标刻的下拉选项框
                    var fileList = files.Select(f => new { FileName = f }).ToList();
                    cmbFileListUpper.ItemsSource = fileList;
                }
            }
            // 解析下标刻文件列表
            if (!string.IsNullOrEmpty(responseDn))
            {
                var jsonResponse = JObject.Parse(responseDn);
                var files = jsonResponse["files"]?.ToObject<List<string>>();

                if (files != null)
                {
                    // 将文件列表加载到上标刻和下标刻的下拉选项框
                    var fileList = files.Select(f => new { FileName = f }).ToList();
                    cmbFileListLower.ItemsSource = fileList;
                }
            }






        }

        // 上标刻 - 设为活动文件按钮点击事件
        private void btnSetActiveFileUpper_Click(object sender, RoutedEventArgs e)
        {
            SetActiveFile(cmbFileListUpper, itemsControlUnitInfoUpper, SingleInstance.Instance.tcpSerModel.MarkingClientUp);
        }

        // 下标刻 - 设为活动文件按钮点击事件
        private void btnSetActiveFileLower_Click(object sender, RoutedEventArgs e)
        {
            SetActiveFile(cmbFileListLower, itemsControlUnitInfoLower, SingleInstance.Instance.tcpSerModel.MarkingClientDn);
        }

        // 设为活动文件的通用逻辑
        private void SetActiveFile(ComboBox cmbFileList, ItemsControl itemsControlUnitInfo, TcpClient tcpClient)
        {
            TcpSerModel serModel = SingleInstance.Instance.tcpSerModel;

            var selectedFile = cmbFileList.SelectedItem as dynamic;
            if (selectedFile == null)
            {
                MessageBox.Show("请先选择一个文件！");
                return;
            }

            string data = string.Empty;
            // 构建8449请求
            SingleInstance.Instance.IJsonServices.BuildJson_8449(ref data, selectedFile.FileName);

            // 发送8449请求，打开文件并获取ID
            string response8449 = SingleInstance.Instance.ITcpService.SendToMarkingSoftwareAndWaitForReturn(ref data, tcpClient, serModel);

            // 从 response8449 中获取文件ID
            string fileID = string.Empty;
            if (!string.IsNullOrEmpty(response8449))
            {
                var jsonResponse = JObject.Parse(response8449);
                fileID = jsonResponse["fileID"]?.ToString(); // 提取 fileID
            }

            if (string.IsNullOrEmpty(fileID))
            {
                MessageBox.Show("获取文件ID失败！");
                return;
            }

            // 构建13057请求
            SingleInstance.Instance.IJsonServices.BuildJson_13057(ref data, fileID);

            // 发送13057请求，获取图元信息
            string response13057 = SingleInstance.Instance.ITcpService.SendToMarkingSoftwareAndWaitForReturn(ref data, tcpClient, serModel);

            // 解析图元信息
            if (!string.IsNullOrEmpty(response13057))
            {
                var jsonResponse = JObject.Parse(response13057);
                var unitInfos = jsonResponse["unitInfos"]?.ToObject<List<UnitInfo>>();

                if (unitInfos != null)
                {
                    // 将图元信息加载到对应的区域（最多六行）
                    itemsControlUnitInfo.ItemsSource = unitInfos.Take(6).Select(u => new UnitInfoViewModel
                    {
                        UnitName = u.name,
                        IsEnabled = u.selected,
                        DatabaseColumn = 0 // 默认值
                    }).ToList();
                }
            }
        }

        // 上标刻 - 确定按钮点击事件
        private void btnConfirmUpper_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSettings(itemsControlUnitInfoUpper, cmbLineType, cmbFileListUpper, "上标刻");
        }

        // 下标刻 - 确定按钮点击事件
        private void btnConfirmLower_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSettings(itemsControlUnitInfoLower, cmbLineType, cmbFileListLower, "下标刻");
        }

        // 确定按钮的通用逻辑
        // 确定按钮的通用逻辑
        private void ConfirmSettings(ItemsControl itemsControlUnitInfo, ComboBox cmbLineType, ComboBox cmbFileList, string regionName)
        {


            var unitInfos = itemsControlUnitInfo.ItemsSource as List<UnitInfoViewModel>;
            if (unitInfos == null || unitInfos.Count == 0)
            {
                MessageBox.Show($"{regionName}区域没有可提交的图元信息！");
                return;
            }

            // 获取用户选择的线类型
            string lineType = (cmbLineType.SelectedItem as ComboBoxItem)?.Content.ToString();

            // 获取当前活动文件名
            var selectedFile = cmbFileList.SelectedItem as dynamic;
            string fileName = selectedFile?.FileName;

            // 根据regionName来决定是设置上下标刻的活动文件名
            if (regionName == "上标刻")
            {
                // 设置 up_actfileName
                SingleInstance.Instance.up_actfileName = fileName;
            }
            else if (regionName == "下标刻")
            {
                // 设置 dn_actfileName
                SingleInstance.Instance.dn_actfileName = fileName;
            }

            // 打包为两个数组（仅包含标刻使能打勾的行）
            List<string> enableList = new List<string>();
            List<int> databaseColumnList = new List<int>();

            foreach (var unitInfo in unitInfos)
            {
                if (unitInfo.IsEnabled) // 仅当标刻使能打勾时，才将该行数据放入数组
                {
                    enableList.Add(unitInfo.UnitName);
                    databaseColumnList.Add(unitInfo.DatabaseColumn);
                }
            }

            // 转换为数组
            string[] enableArray = enableList.ToArray();
            int[] databaseColumnArray = databaseColumnList.ToArray();

            // 根据线类型赋值到 SingleInstance
            if (regionName == "上标刻")
            {
                if (lineType == "A线")
                {
                    SingleInstance.Instance.up_picName_A = enableArray;
                    SingleInstance.Instance.up_picDb_A = databaseColumnArray;
                }
                else if (lineType == "B线")
                {
                    SingleInstance.Instance.up_picName_B = enableArray;
                    SingleInstance.Instance.up_picDb_B = databaseColumnArray;
                }
                else if (lineType == "AB线")
                {
                    SingleInstance.Instance.up_picName_A = enableArray;
                    SingleInstance.Instance.up_picDb_A = databaseColumnArray;
                    SingleInstance.Instance.up_picName_B = enableArray;
                    SingleInstance.Instance.up_picDb_B = databaseColumnArray;
                }
            }
            else if (regionName == "下标刻")
            {
                if (lineType == "A线")
                {
                    SingleInstance.Instance.dn_picName_A = enableArray;
                    SingleInstance.Instance.dn_picDb_A = databaseColumnArray;
                }
                else if (lineType == "B线")
                {
                    SingleInstance.Instance.dn_picName_B = enableArray;
                    SingleInstance.Instance.dn_picDb_B = databaseColumnArray;
                }
                else if (lineType == "AB线")
                {
                    SingleInstance.Instance.dn_picName_A = enableArray;
                    SingleInstance.Instance.dn_picDb_A = databaseColumnArray;
                    SingleInstance.Instance.dn_picName_B = enableArray;
                    SingleInstance.Instance.dn_picDb_B = databaseColumnArray;
                }
            }
            // 打印数组
            Console.WriteLine($"{regionName} - {lineType} - Enable Array: " + string.Join(", ", enableArray));
            Console.WriteLine($"{regionName} - {lineType} - Database Column Array: " + string.Join(", ", databaseColumnArray));
            // 打印活动文件名
            //if (regionName == "上标刻")
            //{
            //    Console.WriteLine($"上标刻活动文件名: {SingleInstance.Instance.up_actfileName}");
            //}
            //else if (regionName == "下标刻")
            //{
            //    Console.WriteLine($"下标刻活动文件名: {SingleInstance.Instance.dn_actfileName}");
            //}
            //// 打印A线的相关数据（如果有值）
            //if (SingleInstance.Instance.up_picName_A != null && SingleInstance.Instance.up_picName_A.Length > 0)
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - A线图元名称 (up_picName_A): " + string.Join(", ", SingleInstance.Instance.up_picName_A));
            //}
            //else
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - A线图元名称 (up_picName_A) 没有数据");
            //}

            //if (SingleInstance.Instance.up_picDb_A != null && SingleInstance.Instance.up_picDb_A.Length > 0)
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - A线图元数据列 (up_picDb_A): " + string.Join(", ", SingleInstance.Instance.up_picDb_A));
            //}
            //else
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - A线图元数据列 (up_picDb_A) 没有数据");
            //}

            //// 打印A线下的相关数据（如果有值）
            //if (SingleInstance.Instance.dn_picDb_A != null && SingleInstance.Instance.dn_picDb_A.Length > 0)
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - A线下图元数据列 (dn_picDb_A): " + string.Join(", ", SingleInstance.Instance.dn_picDb_A));
            //}
            //else
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - A线下图元数据列 (dn_picDb_A) 没有数据");
            //}

            //if (SingleInstance.Instance.dn_picName_A != null && SingleInstance.Instance.dn_picName_A.Length > 0)
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - A线下图元名称 (dn_picName_A): " + string.Join(", ", SingleInstance.Instance.dn_picName_A));
            //}
            //else
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - A线下图元名称 (dn_picName_A) 没有数据");
            //}

            //// 打印B线的相关数据（如果有值）
            //if (SingleInstance.Instance.dn_picName_B != null && SingleInstance.Instance.dn_picName_B.Length > 0)
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - B线图元名称 (dn_picName_B): " + string.Join(", ", SingleInstance.Instance.dn_picName_B));
            //}
            //else
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - B线图元名称 (dn_picName_B) 没有数据");
            //}

            //if (SingleInstance.Instance.up_picDb_B != null && SingleInstance.Instance.up_picDb_B.Length > 0)
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - B线图元数据列 (up_picDb_B): " + string.Join(", ", SingleInstance.Instance.up_picDb_B));
            //}
            //else
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - B线图元数据列 (up_picDb_B) 没有数据");
            //}

            //// 打印B线下的相关数据（如果有值）
            //if (SingleInstance.Instance.up_picName_B != null && SingleInstance.Instance.up_picName_B.Length > 0)
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - B线下图元名称 (up_picName_B): " + string.Join(", ", SingleInstance.Instance.up_picName_B));
            //}
            //else
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - B线下图元名称 (up_picName_B) 没有数据");
            //}

            //if (SingleInstance.Instance.dn_picDb_B != null && SingleInstance.Instance.dn_picDb_B.Length > 0)
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - B线下图元数据列 (dn_picDb_B): " + string.Join(", ", SingleInstance.Instance.dn_picDb_B));
            //}
            //else
            //{
            //    Console.WriteLine($"{regionName} - {lineType} - B线下图元数据列 (dn_picDb_B) 没有数据");
            //}





        }

        private void cmbLineType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // 添加主键数据列的下拉选项
        private void LoadPrimaryKeyColumnOptions()
        {
            // 创建一个 HashSet 用于去重
            HashSet<int> combinedColumns = new HashSet<int>();

            // 合并四个数组的数据并去掉0，同时检查空值
            if (SingleInstance.Instance.up_picDb_A != null)
            {
                combinedColumns.UnionWith(SingleInstance.Instance.up_picDb_A.Where(val => val != 0));
            }
            if (SingleInstance.Instance.dn_picDb_A != null)
            {
                combinedColumns.UnionWith(SingleInstance.Instance.dn_picDb_A.Where(val => val != 0));
            }
            if (SingleInstance.Instance.up_picDb_B != null)
            {
                combinedColumns.UnionWith(SingleInstance.Instance.up_picDb_B.Where(val => val != 0));
            }
            if (SingleInstance.Instance.dn_picDb_B != null)
            {
                combinedColumns.UnionWith(SingleInstance.Instance.dn_picDb_B.Where(val => val != 0));
            }

            // 将去重后的数据绑定到 ComboBox
            cmbPrimarykeyColumn.ItemsSource = combinedColumns.ToList();
        }

        private void ConfirmPrimaryKeyColumnSelection()
        {
            // 获取 ComboBox 中选中的值
            int selectedPrimaryKeyColumn = (int)cmbPrimarykeyColumn.SelectedItem;

            // 将选择的值赋给 PrimarykeyColumn
            SingleInstance.Instance.PrimarykeyColumn = selectedPrimaryKeyColumn;

            // 打印选中的值
            Console.WriteLine($"选中的主键数据列是: {SingleInstance.Instance.PrimarykeyColumn}");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LoadPrimaryKeyColumnOptions();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ConfirmPrimaryKeyColumnSelection();
        }


        #region 拖拽文件框

        // 拖放文件时触发
        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            // 检查拖入的是否是文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        // 文件拖放完成后触发
        private void TextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            // 获取拖入的文件路径
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                string filePath = files[0];
                FilePathTextBox.Text = filePath;

                // 解析文件并显示数据
                ParseAndDisplayData(filePath);
            }
        }

        // 解析文件并显示数据
        private void ParseAndDisplayData(string filePath)
        {
            try
            {
                // 读取文件的第一行
                string firstLine = File.ReadLines(filePath).FirstOrDefault();
                if (firstLine != null)
                {
                    // 按逗号分隔数据
                    string[] columns = firstLine.Split(',');

                    // 清空 DataGrid
                    DataGrid.Items.Clear();
                    DataGrid.Columns.Clear();

                    // 动态生成列并标明列号
                    for (int i = 0; i < columns.Length; i++)
                    {
                        DataGridTextColumn column = new DataGridTextColumn
                        {
                            Header = $"第 {i + 1} 列", // 列号从 1 开始
                            Binding = new System.Windows.Data.Binding($"[{i}]")
                        };
                        DataGrid.Columns.Add(column);
                    }

                    // 添加数据到 DataGrid
                    DataGrid.Items.Add(columns);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            // 打印 A 线上内容
            sb.AppendLine("A 线上图元名称:");
            sb.AppendLine(ArrayToString(SingleInstance.Instance.up_picName_A));

            sb.AppendLine("A 线上图元数据列:");
            sb.AppendLine(ArrayToString(SingleInstance.Instance.up_picDb_A));

            // 打印 A 线下内容
            sb.AppendLine("A 线下图元名称:");
            sb.AppendLine(ArrayToString(SingleInstance.Instance.dn_picName_A));

            sb.AppendLine("A 线下图元数据列:");
            sb.AppendLine(ArrayToString(SingleInstance.Instance.dn_picDb_A));

            // 打印 B 线上内容
            sb.AppendLine("B 线上图元名称:");
            sb.AppendLine(ArrayToString(SingleInstance.Instance.up_picName_B));

            sb.AppendLine("B 线上图元数据列:");
            sb.AppendLine(ArrayToString(SingleInstance.Instance.up_picDb_B));

            // 打印 B 线下内容
            sb.AppendLine("B 线下图元名称:");
            sb.AppendLine(ArrayToString(SingleInstance.Instance.dn_picName_B));

            sb.AppendLine("B 线下图元数据列:");
            sb.AppendLine(ArrayToString(SingleInstance.Instance.dn_picDb_B));

            // 将内容显示到 TextBox 或 TextBlock
            OutputTextBox.Text = sb.ToString();
        }

        // 将字符串数组转换为字符串
        private string ArrayToString(string[] array)
        {
            if (array != null && array.Length > 0)
            {
                return string.Join(Environment.NewLine, array);
            }
            return "数组为空或未初始化";
        }

        // 将整数数组转换为字符串
        private string ArrayToString(int[] array)
        {
            if (array != null && array.Length > 0)
            {
                return string.Join(Environment.NewLine, array);
            }
            return "数组为空或未初始化";
        }

        //根据数据库名获取同级的参数
        public object GetDatabaseConfigValue(string databaseName, string fieldName)
        {
            try
            {
                // 从配置中获取所有数据库配置
                var customers = SingleInstance.Instance.config.GetSection("MasterComputer:Customer").Get<DatabaseConfig[]>();

                // 查找匹配的数据库配置
                var databaseConfig = customers.FirstOrDefault(db => db.DataBaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase));

                if (databaseConfig == null)
                {
                    throw new Exception($"数据库 '{databaseName}' 未找到。");
                }

                // 根据字段名返回相应的值
                var property = typeof(DatabaseConfig).GetProperty(fieldName);

                if (property == null)
                {
                    throw new Exception($"字段 '{fieldName}' 在数据库配置中未找到。");
                }

                return property.GetValue(databaseConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取配置时发生错误: {ex.Message}");
                return null; // 出错时返回 null
            }
        }

        private void LoadDataBaseNames()
        {
            try
            {
                // 获取所有数据库配置
                var customers = SingleInstance.Instance.config.GetSection("MasterComputer:Customer").Get<DatabaseConfig[]>();

                // 确保加载到的配置不为空
                if (customers != null)
                {
                    // 获取所有数据库的 DataBaseName，并添加到 ComboBox
                    foreach (var customer in customers)
                    {
                        DataBaseComboBox.Items.Add(customer.DataBaseName);
                    }
                }
                else
                {
                    MessageBox.Show("没有找到任何数据库配置。");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据库名称时发生错误: {ex.Message}");
            }
        }


        private void LoadmMachDBModelClick(object sender, RoutedEventArgs e)
        {
            //加载变量
            // 获取 ComboBox 中选中的 DataBaseName
            string DataBaseName = DataBaseComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(DataBaseName))
            {
                MessageBox.Show("请选择数据库！");
                return;
            }

            // 获取 'test' 数据库的 PrimaryKeyColumn
            var primaryKeyColumn = Convert.ToInt32(GetDatabaseConfigValue(DataBaseName, "PrimaryKeyColumn"));
            Console.WriteLine($"PrimaryKeyColumn for {DataBaseName} database: {primaryKeyColumn}");

            // 获取 'test' 数据库的 ColumnCount
            var columnCount = Convert.ToInt32(GetDatabaseConfigValue(DataBaseName, "ColumnCount"));
            Console.WriteLine($"ColumnCount for {DataBaseName} database: {columnCount}");

            // 获取 'test' 数据库的 DataBaseUserName
            var dbUserName = Convert.ToString(GetDatabaseConfigValue(DataBaseName, "DataBaseUserName"));
            Console.WriteLine($"DataBaseUserName for {DataBaseName} database: {dbUserName}");

            // 获取 'test' 数据库的 DataBasePassword
            var dbPassword = Convert.ToString(GetDatabaseConfigValue(DataBaseName, "DataBasePassword"));
            Console.WriteLine($"DataBasePassword for {DataBaseName} database: {dbPassword}");

            //创建MachDBModel，包含所有确定的SQL语句
            SingleInstance.Instance.machDBModel = new _8Machine_MachDB.Models.MachDBModel(DataBaseName, primaryKeyColumn, columnCount, dbUserName, dbPassword, 8);

            //由于下发model时机已经错过，应该在这里手动下发到TcpSerModel
            SingleInstance.Instance.tcpSerModel.machDBModel = SingleInstance.Instance.machDBModel;

            //执行SQL语句，初始化数据库(表和存储过程）
            SingleInstance.Instance.IMachDBServices.InitMySQL(SingleInstance.Instance.machDBModel);
            //设置pool大小，可以从配置文件中引入
            SingleInstance.Instance.IMachDBServices.SetPoolSize(SingleInstance.Instance.machDBModel);

        }

        private void Test_DataBase(object sender, RoutedEventArgs e)
        {
            MachDBModel machDBModel = SingleInstance.Instance.machDBModel;
            IMachDBServices IMachDBServices = SingleInstance.Instance.IMachDBServices;
            TcpSerModel tcpSerModel = SingleInstance.Instance.tcpSerModel;


            //清空新表
            IMachDBServices.ClearNewBlank(machDBModel);

            //导入新码包到新表
            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            IMachDBServices.LoadNewBag(@"C:\\ProgramData\\MySQL\\MySQL Server 8.0\\Uploads\\100W.txt", machDBModel);
            Console.WriteLine($"LoadNewBag导入码包并自我去重 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");

            //新表历史去重
            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            IMachDBServices.NewBlankInpect(machDBModel);
            Console.WriteLine($"NewBlankInpect 历史去重 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");

            //向新表申请一条未使用的数据
            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            string[] strings;
            IMachDBServices.GetLeagleData(out strings, machDBModel, 0);
            Console.WriteLine(strings[0]);
            Console.WriteLine(strings[1]);
            Console.WriteLine(strings[2]);
            Console.WriteLine($"GetLeagleData 申请一条数据 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");
        }

        private void IsNG_Click(object sender, RoutedEventArgs e)
        {
            MachDBModel machDBModel = SingleInstance.Instance.machDBModel;
            IMachDBServices IMachDBServices = SingleInstance.Instance.IMachDBServices;
            TcpSerModel tcpSerModel = SingleInstance.Instance.tcpSerModel;
            //使用存储过程对一条数据进行重码检测

            //码不存在
            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            IMachDBServices.IsOKData("https://000BUeKCz5M1,https://1Nh2DSY5Osbv,LhGimCTw,位置超差", machDBModel, 0);
            Console.WriteLine($"IsOKData 数据库判断是否NG之 码不存在 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");


            //码存在，推理机直接NG
            string[] DataArray1 = new string[2];
            IMachDBServices.GetLeagleData(out DataArray1, machDBModel, 0); //先申请一条数据

            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            Console.WriteLine(IMachDBServices.IsOKData($"{DataArray1[0]},{DataArray1[1]},{DataArray1[2]},位置超差", machDBModel, 0));
            Console.WriteLine($"IsOKData 数据库判断是否NG之 推理机NG 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");

            //完全合法
            string[] DataArray2 = new string[2];
            IMachDBServices.GetLeagleData(out DataArray2, machDBModel, 0); //先申请一条数据

            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            Console.WriteLine(IMachDBServices.IsOKData($"{DataArray2[0]},{DataArray2[1]},{DataArray2[2]},OK", machDBModel, 0));
            Console.WriteLine($"IsOKData 数据库判断是否NG之 完全合法 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");

            //已使用
            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            Console.WriteLine(IMachDBServices.IsOKData($"{DataArray2[0]},{DataArray2[1]},{DataArray2[2]},OK", machDBModel, 0));
            Console.WriteLine($"IsOKData 数据库判断是否NG之 码已使用 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");

            //关联性错误
            string[] DataArray3 = new string[2];
            IMachDBServices.GetLeagleData(out DataArray3, machDBModel, 0); //先申请一条数据

            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            Console.WriteLine(IMachDBServices.IsOKData($"{DataArray3[0]},{DataArray3[1]},{DataArray1[1]},OK", machDBModel, 0));
            Console.WriteLine($"IsOKData 数据库判断是否NG之 关联性错误 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");




        }

        private void ApplayForAData_Click(object sender, RoutedEventArgs e)
        {
            string[] strings = null;
            SingleInstance.Instance.IMachDBServices.GetLeagleData(out strings, SingleInstance.Instance.machDBModel, 0);
            Console.WriteLine(strings[2]);
        }
    }




}

public class DatabaseConfig
{
    public string DataBaseName { get; set; }
    public int ColumnCount { get; set; }
    public int PrimaryKeyColumn { get; set; }
    public string DataBaseUserName { get; set; }
    public string DataBasePassword { get; set; }
}
// 图元信息视图模型
public class UnitInfoViewModel
{
    public string UnitName { get; set; }
    public bool IsEnabled { get; set; }
    public int DatabaseColumn { get; set; }
}

// 图元信息模型
public class UnitInfo
{
    public string name { get; set; }
    public bool selected { get; set; }
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
            if (_textBox.LineCount >= 20)
            {
                _textBox.Clear();
            }
            _textBox.AppendText(value.ToString());
            _textBox.ScrollToEnd();
        });
    }

    public override void Write(string value)
    {
        _textBox.Dispatcher.Invoke(() =>
        {
            if (_textBox.LineCount >= 20)
            {
                _textBox.Clear();
            }
            _textBox.AppendText(value);
            _textBox.ScrollToEnd();
        });
    }

    public override void WriteLine(string value)
    {
        _textBox.Dispatcher.Invoke(() =>
        {
            if (_textBox.LineCount >= 20)
            {
                _textBox.Clear();
            }
            _textBox.AppendText(value + Environment.NewLine);
            _textBox.ScrollToEnd();
        });
    }


}




