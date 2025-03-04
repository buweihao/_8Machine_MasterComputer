using System.Net.Sockets;
using System.Net;
using System.Text;
using MachDBTcp.Interfaces;
using Serilog;
using MachDBTcp.Models;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Docker.DotNet.Models;
using System.Diagnostics;
using System.Net.Http;
using _8Machine_MasterComputer.ViewModel;
using static _8Machine_MasterComputer.Instance.SingleInstance;
using _8Machine_MasterComputer.Instance;
using System.Threading.Tasks;
using K4os.Compression.LZ4.Internal;
using System.Runtime.Intrinsics.X86;
using System.Windows.Shapes;
using ZstdSharp.Unsafe;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Tls;

namespace MachDBTcp.Services
{



    public class TcpService : ITcpService
    {
        #region  上位机软件调用的函数
        //需要根据连接者的不同，板卡连接需要开新线程处理，推理机连接添加套接字即可，因为推理机不会主动给上位机软件发送消息
        public void Start(TcpSerModel tcp)
        {
            //获取Json配置文件的变量
            int ListenDelayTime = int.Parse(Instance.config["MasterComputer:Delay:ListenDlayTime"]);
            int ReciveDelayTime = int.Parse(Instance.config["MasterComputer:Delay:ReciveDelayTime"]);
            string? BoardCardIP = Instance.config["MasterComputer:IP:BoardCardIP"];
            string? InferComputer1IP = Instance.config["MasterComputer:IP:InferComputer1IP"];
            string? InferComputer2IP = Instance.config["MasterComputer:IP:InferComputer2IP"];

            string? UpMarkingSoftWareIP = Instance.config["MasterComputer:IP:UpMarkingSoftWareIP"];
            string? DownMarkingSoftWareIP = Instance.config["MasterComputer:IP:DownMarkingSoftWareIP"];
            int upMarkingSoftWarePort = int.Parse(Instance.config["MasterComputer:Port:UpMarkingSoftWarePort"]);
            int downMarkingSoftWarePort = int.Parse(Instance.config["MasterComputer:Port:DownMarkingSoftWarePort"]);

            // 绑定 IP 和端口
            var _tcpListener0 = new TcpListener(IPAddress.Parse(tcp.ipAddress1), tcp.port0);
            var _tcpListener1 = new TcpListener(IPAddress.Parse(tcp.ipAddress1), tcp.port1);
            var _tcpListener2 = new TcpListener(IPAddress.Parse(tcp.ipAddress2), tcp.port2);

            // 启动监听
            _tcpListener0.Start();
            //_tcpListener1.Start();
            _tcpListener2.Start();

            Instance.MasterComputer2BoardCardLog.Debug("服务器已启动，等待客户端连接...");
            Instance.MasterComputer2BoardCardLog.Debug($"监听地址1：{tcp.ipAddress1}:{tcp.port0}");
            //Instance.MasterComputer2BoardCardLog.Debug($"监听地址1：{tcp.ipAddress1}:{tcp.port1}");//8003为测试端口，不需要
            Instance.MasterComputer2BoardCardLog.Debug($"监听地址2：{tcp.ipAddress2}:{tcp.port2}");

            // 无限等待客户端连接
            Thread listenerThread = new Thread(() =>
            {
                while (true) // 无限等待客户端连接
                {
                    try
                    {
                        if (
                        false
                        //_tcpListener1.Pending()
                        )
                        {
                            //获取新客户端套接字
                            var tcpClient = _tcpListener1.AcceptTcpClient();

                            // 获取客户端的 IP 和端口
                            var clientEndPoint = tcpClient.Client.RemoteEndPoint as System.Net.IPEndPoint;
                            if (clientEndPoint != null)
                            {
                                Instance.MasterComputer2BoardCardLog.Debug($"客户端已连接 - IP: {clientEndPoint.Address}, 端口: {clientEndPoint.Port}");

                                // 检查客户端IP 地址是否合法
                                string clientIP = clientEndPoint.Address.ToString();
                                if (clientIP.StartsWith(BoardCardIP))
                                {
                                    Instance.MasterComputer2BoardCardLog.Debug("板卡已和上位机软件连接");

                                    //保存套接字为板卡套接字
                                    tcp.BoardCardTcpClient = tcpClient;

                                    //使用HandleBoardCard处理套接字消息
                                    Thread clientThread = new Thread(() => HandleBoardCardThread(tcpClient, ReciveDelayTime, tcp));
                                    clientThread.Start();
                                }
                                else
                                {
                                    Instance.MasterComputer2BoardCardLog.Debug($"未知网段的客户端连接 - IP: {clientIP}");
                                    tcpClient.Close(); // 关闭连接，拒绝未知网段
                                }

                            }
                        }
                        else if (_tcpListener0.Pending())
                        {
                            //获取新客户端套接字
                            var tcpClient = _tcpListener0.AcceptTcpClient();

                            // 获取客户端的 IP 和端口
                            var clientEndPoint = tcpClient.Client.RemoteEndPoint as System.Net.IPEndPoint;
                            if (clientEndPoint != null)
                            {
                                Instance.MasterComputer2BoardCardLog.Debug($"客户端已连接 - IP: {clientEndPoint.Address}, 端口: {clientEndPoint.Port}");

                                // 检查客户端IP 地址是否合法
                                string clientIP = clientEndPoint.Address.ToString();
                                if (clientIP.StartsWith(BoardCardIP))
                                {
                                    Instance.MasterComputer2BoardCardLog.Debug("板卡已和数据库连接");

                                    //保存套接字为板卡-数据库套接字
                                    tcp.BoardCard_DataBaseTcpClient = tcpClient;

                                    //使用HandleBoardCard处理套接字消息
                                    Thread clientThread = new Thread(() => HandleBoardCardThread(tcpClient, ReciveDelayTime, tcp));
                                    clientThread.Start();
                                }
                                else
                                {
                                    Instance.MasterComputer2BoardCardLog.Debug($"未知网段的客户端连接 - IP: {clientIP}");
                                    tcpClient.Close(); // 关闭连接，拒绝未知网段
                                }

                            }
                        }

                        else if (_tcpListener2.Pending())
                        {
                            //获取新客户端套接字
                            var tcpClient = _tcpListener2.AcceptTcpClient();

                            // 获取客户端的 IP 和端口
                            var clientEndPoint = tcpClient.Client.RemoteEndPoint as System.Net.IPEndPoint;
                            if (clientEndPoint != null)
                            {
                                Instance.MasterComputer2BoardCardLog.Debug($"客户端已连接 - IP: {clientEndPoint.Address}, 端口: {clientEndPoint.Port}");

                                // 检查客户端IP 地址是否合法
                                string clientIP = clientEndPoint.Address.ToString();
                                if (clientIP.StartsWith(InferComputer1IP))
                                {
                                    //保存为推理机A的套接字
                                    tcp.InferTcpClient1 = tcpClient;
                                }

                                else if (clientIP.StartsWith(InferComputer2IP))
                                {
                                    //保存为推理机B的套接字
                                    tcp.InferTcpClient2 = tcpClient;
                                    //使用HandleInferComputerB处理套接字消息,这里需要修改
                                    Thread clientThread = new Thread(() => HandleBoardCardThread(tcpClient, ReciveDelayTime, tcp));
                                }
                                else
                                {
                                    Instance.MasterComputer2BoardCardLog.Debug($"未知网段的客户端连接 - IP: {clientIP}");
                                    tcpClient.Close(); // 关闭连接，拒绝未知网段
                                }
                            }
                            else
                            {
                                // ListenDelayTime 毫秒检查一次
                                Thread.Sleep(ListenDelayTime);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Instance.MasterComputer2BoardCardLog.Error($"监听线程异常: {ex.Message}");
                        break; // 处理异常后退出循环（或根据需要重新启动监听逻辑）
                    }
                }
            });
            // 启动监听线程
            listenerThread.Start();

            Thread ConnectThread = new Thread(() => {
                //作为客户端连接上下打标软件，并将套接字保存到tcp.MarkingClientUp和tcp.MarkingClientDn
                ConnectToMarkingSoftWare(tcp, DownMarkingSoftWareIP, downMarkingSoftWarePort, UpMarkingSoftWareIP, upMarkingSoftWarePort, 5);
            });
            ConnectThread.Start();

        }



        public void SendToBoardCard(ref string JsonMessage, TcpSerModel tcp,TcpClient tcpClient)
        {
            if (!CheckTcpClient(tcpClient))
            {
                Instance.MasterComputer2BoardCardLog.Fatal("板卡套接字不可用...");
                return;
            }
            try
            {
                //获取流
                NetworkStream networkStream = tcp.BoardCard_DataBaseTcpClient.GetStream();

                // 将 JSON 数据转为字节流
                byte[] responseBytes = Encoding.UTF8.GetBytes(JsonMessage);

                if (networkStream.CanWrite)
                {
                    //发送请求
                    try
                    {
                        // 发送数据到客户端
                        networkStream.Write(responseBytes, 0, responseBytes.Length);

                        // 使用正则表达式提取 Cmd 字段
                        try
                        {
                            // 格式化日志
                            Instance.MasterComputer2BoardCardLog.Debug($"向板卡发送了 {TcpSerModel.GetCmd(JsonMessage, tcp)}");
                        }
                        catch (Exception ex)
                        {
                            Instance.MasterComputer2BoardCardLog.Error($"正则解析 Cmd 时发生错误: {ex.Message}");
                        }


                        //清除缓存
                        JsonMessage = string.Empty;

                    }
                    catch (Exception ex)
                    {
                        Instance.MasterComputer2BoardCardLog.Error($"发送数据时发生错误: {ex.Message}");
                    }
                }

            }
            catch (IOException ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"连接已关闭，异常: {ex.Message}");
            }
            catch (SocketException ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"Socket 异常: {ex.Message}");
            }

        }

        public string SendToInferComputerAndWaitForReturn(ref string JsonMessage, TcpClient tcpClient, TcpSerModel tcp, int timeout = 5000)
        {
            try
            {
                // 获取流
                NetworkStream networkStream = tcpClient.GetStream();

                // 将 JSON 数据转为字节流
                byte[] responseBytes = Encoding.UTF8.GetBytes(JsonMessage);

                if (networkStream.CanWrite)
                {
                    try
                    {
                        // 发送数据到客户端
                        networkStream.Write(responseBytes, 0, responseBytes.Length);

                        // 使用正则表达式提取 Cmd 字段并记录日志
                        try
                        {
                            // 格式化日志
                            Instance.MasterComputer2InferComputer1.Information($"向推理机发送了 {TcpSerModel.GetCmd(JsonMessage, tcp)}");
                        }
                        catch (Exception ex)
                        {
                            Instance.MasterComputer2BoardCardLog.Error($"正则解析 Cmd 时发生错误: {ex.Message}");
                        }

                        // 等待对方的回复
                        if (networkStream.CanRead)
                        {
                            // 设置读取超时时间
                            networkStream.ReadTimeout = timeout;

                            // 用于存储完整的消息
                            StringBuilder jsonMessageBuilder = new StringBuilder();
                            byte[] buffer = new byte[1024];
                            int bytesRead;

                            // 循环读取，直到收到完整的 JSON 数据或超时
                            while (true)
                            {
                                if ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    // 将本次读取的字节数据追加到 StringBuilder
                                    jsonMessageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                                    // 检查 StringBuilder 是否包含完整的 JSON 数据
                                    string currentData = jsonMessageBuilder.ToString();

                                    // 检查是否以 `{` 开头并以 `}` 结尾
                                    if (currentData.StartsWith("{") && currentData.EndsWith("}"))
                                    {
                                        try
                                        {
                                            // 转换为 JSON 对象（可选）
                                            var messageObject = JsonConvert.DeserializeObject<object>(currentData);

                                            // 返回完整的 JSON 数据，实际应该返回上面处理后的Json
                                            return currentData;
                                        }
                                        catch (JsonException ex)
                                        {
                                            Instance.MasterComputer2BoardCardLog.Error($"JSON 解析失败: {ex.Message}");
                                            jsonMessageBuilder.Clear(); // 清空缓存，防止后续解析错误
                                        }
                                    }
                                }
                                else
                                {
                                    // 如果没有数据可读，等待一段时间（防止空循环占用 CPU 资源）
                                    Thread.Sleep(10);
                                }
                            }
                        }

                        // 清除缓存
                        JsonMessage = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Instance.MasterComputer2BoardCardLog.Error($"发送数据或接收回复时发生错误: {ex.Message}");
                        return string.Empty;
                    }
                }
            }
            catch (IOException ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"连接已关闭，异常: {ex.Message}");
                return string.Empty;
            }
            catch (SocketException ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"Socket 异常: {ex.Message}");
                return string.Empty;
            }
            return string.Empty;
        }

        public string SendToMarkingSoftwareAndWaitForReturn(ref string JsonMessage, TcpClient tcpClient, TcpSerModel tcp, int timeout = 3000)
        {
            try
            {
                // 获取流
                NetworkStream networkStream = tcpClient.GetStream();

                // 将 JSON 数据转为字节流
                byte[] responseBytes = null;

                AddStartTail(ref responseBytes, JsonMessage);

                if (networkStream.CanWrite)
                {
                    try
                    {
                        // 发送数据到客户端
                        networkStream.Write(responseBytes, 0, responseBytes.Length);

                        // 使用正则表达式提取 Cmd 字段并记录日志
                        try
                        {
                            // 格式化日志
                            Instance.MasterComputer2InferComputer1.Information($"向打标软件发送了 {TcpSerModel.GetAct(JsonMessage, tcp)}");
                        }
                        catch (Exception ex)
                        {
                            Instance.MasterComputer2BoardCardLog.Error($"正则解析 act 时发生错误: {ex.Message}");
                        }

                        // 等待对方的回复
                        if (networkStream.CanRead)
                        {
                            // 设置读取超时时间
                            networkStream.ReadTimeout = timeout;

                            // 用于存储完整的消息
                            StringBuilder jsonMessageBuilder = new StringBuilder();
                            byte[] buffer = new byte[1024];
                            int bytesRead;

                            // 循环读取，直到收到完整的 JSON 数据或超时
                            while (true)
                            {
                                if ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    // 将本次读取的字节数据追加到 StringBuilder
                                    jsonMessageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                                    // 获取当前存储的所有数据
                                    string currentData = jsonMessageBuilder.ToString();

                                    // 查找第一个 '{' 的位置
                                    int startIndex = currentData.IndexOf('{');
                                    if (startIndex >= 0)
                                    {
                                        // 从 '{' 开始截取数据
                                        currentData = currentData.Substring(startIndex);

                                        // 检查是否以 `{` 开头并以 `}` 结尾
                                        if (currentData.StartsWith("{") && currentData.EndsWith("}"))
                                        {
                                            try
                                            {
                                                // 格式化日志
                                                Instance.MasterComputer2InferComputer1.Information($"打标软件回复了一条： {currentData}");

                                                // 返回完整的 JSON 数据，实际应该返回上面处理后的Json
                                                return currentData;
                                            }
                                            catch (JsonException ex)
                                            {
                                                Instance.MasterComputer2BoardCardLog.Error($"JSON 解析失败: {ex.Message}");
                                                jsonMessageBuilder.Clear(); // 清空缓存，防止后续解析错误
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // 如果没有数据可读，等待一段时间（防止空循环占用 CPU 资源）
                                    Thread.Sleep(10);
                                }
                            }
                        }

                        // 清除缓存
                        //JsonMessage = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Instance.MasterComputer2BoardCardLog.Error($"发送数据或接收回复时发生错误: {ex.Message}");
                        return string.Empty;
                    }
                }
            }
            catch (IOException ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"打标软件连接已关闭，异常: {ex.Message}");
                return string.Empty;
            }
            catch (SocketException ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"Socket 异常: {ex.Message}");
                return string.Empty;
            }
            return string.Empty;
        }
        private void HandleBoardCardThread(TcpClient tcpClient, int delayTime, TcpSerModel model)
        {
            if (!CheckTcpClient(model.BoardCard_DataBaseTcpClient))
            {
                Instance.MasterComputer2BoardCardLog.Fatal("板卡套接字不可用...");
                return;
            }
            NetworkStream networkStream = tcpClient.GetStream();
            byte[] buffer = new byte[1024]; // 缓冲区大小
            StringBuilder jsonMessageBuilder = new StringBuilder();
            int bytesRead;
            Instance.MasterComputer2BoardCardLog.Debug("开始轮询板卡发来的请求,并且处理请求和返回...");
            try
            {
                while (tcpClient.Connected) // 保持连接，只要客户端未断开
                {

                    // 等待客户端数据
                    if (networkStream.DataAvailable)
                    {
                        if ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // 将本次读取的数据转化为字符串，并累加到 StringBuilder 中
                            jsonMessageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        }

                        // 检查 StringBuilder 中是否有完整的 JSON 数据
                        while (jsonMessageBuilder.Length > 0)
                        {
                            string currentData = jsonMessageBuilder.ToString();

                            // 检查是否是完整的 JSON 数据（起始为 `{`，结束为 `}`）
                            if (currentData.StartsWith('{') && currentData.EndsWith('}'))
                            {
                                try
                                {
                                    //这里写HandleBoardCard库中的分发方法，参数是板卡发来的Json数据，HandleBoardCard库是一个耦合库,由委托传入，具体参数由耦合方法编写时再决定
                                    HandleBoardCard(currentData, model,tcpClient);
                                    // 清空 StringBuilder
                                    jsonMessageBuilder.Clear();
                                }
                                catch (System.Text.Json.JsonException ex)
                                {
                                    Instance.MasterComputer2BoardCardLog.Error($"JSON 解析失败: {ex.Message}");
                                    jsonMessageBuilder.Clear(); // 清空缓存，防止后续读取错误
                                }
                            }
                            else
                            {
                                // 如果不是完整的 JSON 数据，退出当前检查，继续读取数据
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 如果没有数据可用，主动等待（防止空循环占用 CPU 资源）
                        Thread.Sleep(delayTime); // 适当休眠，减少 CPU 消耗
                    }
                }
            }
            catch (Exception ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"客户端连接处理失败: {ex.Message}");
            }
            finally
            {
                // 确保连接关闭
                networkStream.Close();
                tcpClient.Close();
            }


        }

        // 主处理函数,每次有一条板卡消息调用一次这个函数分发任务
        private void HandleBoardCard(string JsonSource, MachDBTcp.Models.TcpSerModel tcpSerModel,TcpClient tcpClient)
        {
            try
            {
                // 正则表达式匹配多个 JSON 对象
                string pattern = @"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))*\}";

                var matches = Regex.Matches(JsonSource, pattern);

                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        string jsonObject = match.Value; // 获取单个 JSON 对象
                        var matchCmd = tcpSerModel.regex_Cmd.Match(jsonObject);

                        if (matchCmd.Success)
                        {
                            string cmd = matchCmd.Groups[1].Value;
                            switch (cmd)
                            {
                                case "Msg_filepic":
                                    _ = Handle_Msg_filepic(jsonObject, tcpSerModel, int.Parse(Instance.config["MasterComputer:TestTime:MarkingDelayTime"]));
                                    break;

                                case "Msg_data":
                                    _ = Handle_Msg_data(jsonObject, tcpSerModel);
                                    break;

                                case "Msg_text":
                                    tcpSerModel.stopwatch.Reset();
                                    tcpSerModel.stopwatch.Start();
                                    Handle_Msg_text(jsonObject, tcpSerModel);
                                    tcpSerModel.stopwatch.Stop();
                                    Instance.MasterComputer2BoardCardLog.Debug($"Handle_Msg_text 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");
                                    break;

                                case "Msg_ctl":
                                    tcpSerModel.stopwatch.Reset();
                                    tcpSerModel.stopwatch.Start();
                                    Handle_Msg_ctl(jsonObject, tcpSerModel);
                                    tcpSerModel.stopwatch.Stop();
                                    Instance.MasterComputer2BoardCardLog.Debug($"Handle_Msg_ctl 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");
                                    break;

                                case "Msg_camera":
                                    tcpSerModel.stopwatch.Reset();
                                    tcpSerModel.stopwatch.Start();
                                    Handle_Msg_camera(jsonObject, tcpSerModel);
                                    tcpSerModel.stopwatch.Stop();
                                    Instance.MasterComputer2BoardCardLog.Debug($"Handle_Msg_camera 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");
                                    break;
                                case "Msg_rstdata":
                                    Handle_Msg_rstdata(jsonObject, tcpSerModel);
                                    break;
                                default:
                                    Instance.MasterComputer2BoardCardLog.Debug($"未知的 Cmd 类型: {cmd}");
                                    Console.WriteLine(jsonObject);
                                    break;
                            }
                        }
                        else
                        {
                            Instance.MasterComputer2BoardCardLog.Error("无法解析 Cmd 字段。");
                        }
                    }
                }
                else
                {
                    Instance.MasterComputer2BoardCardLog.Error("无法解析连续的 JSON 对象。");
                    Console.WriteLine(JsonSource);
                }
            }
            catch (Exception ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"处理 JSON 数据时发生异常: {ex.Message}");
            }
        }

        private void AddStartTail(ref byte[] allByteData, string strData)
        {
            try
            {
                byte[] headArrar = new byte[] { 0xEF, 0xBB, 0xBF, 0x00 };
                byte[] endArrar = new byte[] { 0x00 };
                byte[] dataArrar = Encoding.UTF8.GetBytes(strData);
                int bLength = dataArrar.Length + 1;
                byte[] lengthArray = BitConverter.GetBytes(bLength);//数据加结束符长度
                bLength += headArrar.Length + lengthArray.Length;
                allByteData = new byte[bLength];
                Array.Copy(headArrar, 0, allByteData, 0, headArrar.Length);
                Array.Copy(lengthArray, 0, allByteData, headArrar.Length, lengthArray.Length);
                int toIndex = headArrar.Length + lengthArray.Length;
                Array.Copy(dataArrar, 0, allByteData, toIndex, dataArrar.Length);
                toIndex += dataArrar.Length;
                Array.Copy(endArrar, 0, allByteData, toIndex, endArrar.Length);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task Handle_Msg_filepic(string SourceJson, TcpSerModel tcpSerModel, int DelayTime)
        {
            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();

            // 如果有延迟时间不为0，表示是测试模式,给默认值，否则从软件获取
            if (DelayTime != 0)
            {
                //测试时需要延时
                Thread.Sleep(DelayTime);
            }
            try
            {
                // 解析 JSON 数据
                var jsonObject = JObject.Parse(SourceJson);

                // 提取 Cmd 和 cDev 信息（可根据需要使用）
                string cmd = jsonObject["Cmd"]?.ToString() ?? "UnknownCmd";
                string cDev = jsonObject["cDev"]?.ToString() ?? "UnknownDevice";

                // 提取 cUpmark 和 cDnmark 的值
                bool cUpmark = jsonObject["cUpmark"]?.ToObject<bool>() ?? false;
                bool cDnmark = jsonObject["cDnmark"]?.ToObject<bool>() ?? false;

                Instance.MasterComputer2BoardCardLog.Debug($"处理命令: {cmd}, 设备: {cDev}");

                // 初始化图元信息变量，默认赋值
                string up_actfileName = string.Empty;
                string dn_actfileName = string.Empty;
                string[] up_picName_A = Array.Empty<string>();
                int[] up_picDb_A = Array.Empty<int>();
                string[] up_picName_B = Array.Empty<string>();
                int[] up_picDb_B = Array.Empty<int>();
                string[] dn_picName_A = Array.Empty<string>();
                int[] dn_picDb_A = Array.Empty<int>();
                string[] dn_picName_B = Array.Empty<string>();
                int[] dn_picDb_B = Array.Empty<int>();
                var tasks = new List<Task>();
                // 根据 cUpmark 的值处理上标刻机图元信息
                if (cUpmark)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        if (DelayTime != 0)
                        {
                            up_actfileName = "001.bpd";
                            up_picName_A = new string[] { "MBar1", "TextT16" };
                            up_picDb_A = new int[] { 1, 2 };
                            up_picName_B = new string[] { "MBar1", "TextT16" };
                            up_picDb_B = new int[] { 1, 2 };
                        }
                        else
                        {
                            // 异步获取上标刻机图元信息
                            up_actfileName = Instance.up_actfileName ?? up_actfileName;
                            up_picName_A = Instance.up_picName_A ?? up_picName_A;
                            up_picDb_A = Instance.up_picDb_A ?? up_picDb_A;
                            up_picName_B = Instance.up_picName_B ?? up_picName_B;
                            up_picDb_B = Instance.up_picDb_B ?? up_picDb_B;
                        }
                    }));
                }

                // 根据 cDnmark 的值处理下标刻机图元信息
                if (cDnmark)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        if (DelayTime != 0)
                        {
                            // 设置默认图元数据（可以替换成从软件获取的实际数据）
                            dn_actfileName = "546.bpd";
                            dn_picName_A = new string[] { "TextT2", "MBar1" };
                            dn_picDb_A = new int[] { 3, 4 };
                            dn_picName_B = new string[] { "TextT2", "MBar1" };
                            dn_picDb_B = new int[] { 3, 4 };
                        }
                        else
                        {
                            // 异步获取下标刻机图元信息
                            dn_actfileName = Instance.dn_actfileName ?? dn_actfileName;
                            dn_picDb_A = Instance.dn_picDb_A ?? dn_picDb_A;
                            dn_picName_A = Instance.dn_picName_A ?? dn_picName_A;
                            dn_picDb_B = Instance.dn_picDb_B ?? dn_picDb_B;
                            dn_picName_B = Instance.dn_picName_B ?? dn_picName_B;
                        }
                    }));
                }

                // 等待所有异步任务完成
                await Task.WhenAll(tasks);

                // 组合信息
                string JsonContainer = string.Empty;
                tcpSerModel.IJsonServices.BuildJson_Msg_filepic(out JsonContainer, SourceJson, up_actfileName, dn_actfileName, up_picName_A, up_picName_B, dn_picName_A, dn_picName_B, up_picDb_A, up_picDb_B, dn_picDb_A, dn_picDb_B);
                Console.WriteLine($"本次运行，图元信息：{JsonContainer}");

                // 发图元信息给板卡
                SendToBoardCard(ref JsonContainer, tcpSerModel,tcpSerModel.BoardCard_DataBaseTcpClient);

            }
            catch (Exception ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"处理 Msg_filepic 时发生异常: {ex.Message}");
            }
            tcpSerModel.stopwatch.Stop();
            Instance.MasterComputer2BoardCardLog.Debug($"Handle_Msg_filepic 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");
        }

        private async Task Handle_Msg_data(string SourceJson, TcpSerModel tcpSerModel)
        {
            //获取全局变量
            int delay = int.Parse(Instance.config["MasterComputer:TestTime:GetLeagleDataDelayTime"]);
            int isOKdelay = int.Parse(Instance.config["MasterComputer:TestTime:IsOKDataDelayTime"]);
            tcpSerModel.stopwatch.Reset();
            tcpSerModel.stopwatch.Start();
            string[]? dbData = null;
            string InferComputerReturnJson = string.Empty;
            string JsonContainer = string.Empty;
            try
            {
                // 解析 JSON 数据
                var jsonObject = JObject.Parse(SourceJson);


                // 提取布尔值
                int cData = jsonObject["cData"]?.ToObject<int>() ?? 0;
                int cTest = jsonObject["cTest"]?.ToObject<int>() ?? 0;
                bool fcamEn = jsonObject["fcamEn"]?.ToObject<bool>() ?? false;
                bool bcamEn = jsonObject["bcamEn"]?.ToObject<bool>() ?? false;

                // 定义用于存储任务的列表
                var tasks = new List<Task>();

                // 如果需要处理合法数据，创建一个任务
                if (cData == 1)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        Instance.MasterComputer2BoardCardLog.Debug("向数据库请求合法数据...");
                        tcpSerModel.IMachDBServices.GetLeagleData(out dbData, tcpSerModel.machDBModel, delay);
                    }));
                }
                int cTresult = 0;

                // 如果需要检测相机拍照和需要约定相片名字，创建另一个任务
                if (cTest ==1 || fcamEn || bcamEn)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        if (CheckTcpClient(tcpSerModel.InferTcpClient1))
                        {

                            InferComputerReturnJson = SendToInferComputerAndWaitForReturn(
                            ref SourceJson,
                            tcpSerModel.InferTcpClient1,
                            tcpSerModel);

                            Instance.MasterComputer2InferComputer1.Information($"收到推理机返回: {TcpSerModel.GetCmd(InferComputerReturnJson, tcpSerModel)}");

                            string cTresult_Infer = string.Empty ;
                            try
                            {
                                // 使用 Newtonsoft.Json.Linq.JObject 进行解析
                                var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(InferComputerReturnJson);

                                // 尝试获取名为 "cTresult" 的字段
                                var cTresultToken = jsonObj["cTresult"];

                                if (cTresultToken != null)
                                {
                                    cTresult_Infer = cTresultToken.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                // 如果 JSON 格式不正确或其他错误，可以在这里捕获并处理
                                Console.WriteLine($"JSON 解析出错: {ex.Message}");
                            }

                            //将推理机返回的内容交由数据库处理, InferComputerReturnJson的 cTresult字段包含了推理机的明暗码结果和NG情况
                            if (tcpSerModel.IMachDBServices.IsOKData(cTresult_Infer, tcpSerModel.machDBModel, isOKdelay))
                            {
                                cTresult = 0;

                                //零时测试剔除功能，全部剔除
                                //cTresult = 1;
                            }
                            else
                            {
                                cTresult = 1;
                            }
                            }
                    }));
                }
                // 等待所有任务完成
                await Task.WhenAll(tasks);

                //组合信息，发回给板卡
                // 解析 JSON 数据
                string fpicAdd  = "fDefault.png";
                string bpicAdd = "bDefault.png";
                if (InferComputerReturnJson != string.Empty)
                {
                    var jsonObject2 = JObject.Parse(InferComputerReturnJson);

                    // 推理机发来的提取字段内容
                     fpicAdd = jsonObject2["Data"]?["fpicAdd"]?.ToString() ?? "N/A";
                     bpicAdd = jsonObject2["Data"]?["bpicAdd"]?.ToString() ?? "N/A";
                }

                //组装
                tcpSerModel.IJsonServices.BuildJson_Msg_data(out JsonContainer, SourceJson, dbData, cTresult, fpicAdd, bpicAdd);

                //测试时打印查看
                Console.WriteLine(JsonContainer);

                //回复板卡的请求
                SendToBoardCard(ref JsonContainer, tcpSerModel,tcpSerModel.BoardCard_DataBaseTcpClient);

            }
            catch (Exception ex)
            {
                Instance.MasterComputer2BoardCardLog.Error($"处理 Msg_data 时发生异常: {ex.Message}");
            }
            tcpSerModel.stopwatch.Stop(); ;
            Instance.MasterComputer2BoardCardLog.Debug($"Handle_Msg_data 执行时间：{tcpSerModel.stopwatch.ElapsedMilliseconds} ms");

        }

        private void Handle_Msg_text(string SourceJson, TcpSerModel tcpSerModel)
        {
            //需要将内容显示到主窗口，但是暂时没有上位机的控件库，先直接打印到控制台

            // 解析 JSON 数据
            var jsonObject = JObject.Parse(SourceJson);

            // 提取 Content
            string content = jsonObject["Content"]?.ToString();

            Instance.MasterComputer2BoardCardLog.Information("板卡：" + content);
        }

        private void Handle_Msg_ctl(string SourceJson, TcpSerModel tcpSerModel)
        {
            //需要将内容赋值到页面的按钮的可否按下的属性上
            // 获取全局 ViewModel 实例
            var viewModel = Instance.masterComputerVM;

            // 解析 JSON 数据
            var jsonObject = JObject.Parse(SourceJson);

            // 遍历 JSON 对象中的每个键值对
            foreach (var property in jsonObject)
            {
                if (property.Key != "Cmd" && property.Key != "cDev")
                {
                    bool value = bool.Parse(property.Value.ToString());
                    switch (property.Key)
                    {
                        case "Test": viewModel.Test = value; break;
                        case "Start": viewModel.Start = value; break;
                        case "Pause": viewModel.Pause = value; break;
                        case "Stop": viewModel.Stop = value; break;
                        case "Restart": viewModel.Restart = value; break;
                        case "sysReset": viewModel.SysReset = value; break;
                    }
                }
            }
        }

        //对上位机软件来说，此条数据直接转发给推理机，只作中间传输即可
        private void Handle_Msg_camera(string SourceJson, TcpSerModel tcpSerModel)
        {
            string InferComputerReturnJson = string.Empty;


            if (CheckTcpClient(tcpSerModel.InferTcpClient1))
            {
                //发送给推理机
                InferComputerReturnJson = SendToInferComputerAndWaitForReturn(ref SourceJson, tcpSerModel.InferTcpClient1, tcpSerModel);
                Instance.MasterComputer2InferComputer1.Information($"收到推理机返回: {TcpSerModel.GetCmd(InferComputerReturnJson, tcpSerModel)}");

                //将推理机返回的内容发送给回板卡
                SendToBoardCard(ref InferComputerReturnJson, tcpSerModel,tcpSerModel.BoardCard_DataBaseTcpClient);
            }

            else
            {
                Instance.MasterComputer2BoardCardLog.Error("推理机套接字不可用，无法获取结果返回给板卡");
            }

        }

        private void ConnectToMarkingSoftWare(TcpSerModel tcp, string DownMarkingSoftWareIP, int downMarkingSoftWarePort, string UpMarkingSoftWareIP, int upMarkingSoftWarePort, int timeoutInSeconds)
        {
            int timeoutInMilliseconds = timeoutInSeconds * 1000;

            // 连接上打标软件
            ConnectWithTimeout(tcp, UpMarkingSoftWareIP, upMarkingSoftWarePort, timeoutInMilliseconds, true);

            // 连接下打标软件
            ConnectWithTimeout(tcp, DownMarkingSoftWareIP, downMarkingSoftWarePort, timeoutInMilliseconds, false);
        }

        private void ConnectWithTimeout(TcpSerModel tcp, string ipAddress, int port, int timeoutInMilliseconds, bool isUpMarking)
        {
            TcpClient client = new TcpClient();
            IAsyncResult result = client.BeginConnect(IPAddress.Parse(ipAddress), port, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(timeoutInMilliseconds, false);

            if (success)
            {
                try
                {
                    client.EndConnect(result);
                    if (isUpMarking)
                    {
                        tcp.MarkingClientUp = client;
                        Instance.MasterComputer2BoardCardLog.Debug($"已连接到上打标软件: {ipAddress}:{port}");
                    }
                    else
                    {
                        tcp.MarkingClientDn = client;
                        Instance.MasterComputer2BoardCardLog.Debug($"已连接到下打标软件: {ipAddress}:{port}");
                    }
                }
                catch (Exception ex)
                {
                    if (isUpMarking)
                    {
                        Instance.MasterComputer2BoardCardLog.Error($"连接上打标软件时出错: {ex.Message}");
                    }
                    else
                    {
                        Instance.MasterComputer2BoardCardLog.Error($"连接下打标软件时出错: {ex.Message}");
                    }
                    client.Close();
                }
            }
            else
            {
                if (isUpMarking)
                {
                    Instance.MasterComputer2BoardCardLog.Error($"连接上打标软件超时: {ipAddress}:{port}");
                }
                else
                {
                    Instance.MasterComputer2BoardCardLog.Error($"连接下打标软件超时: {ipAddress}:{port}");
                }
                client.Close();
                return;
            }
        }

        private void Handle_Msg_rstdata(string SourceJson, TcpSerModel tcpSerModel)
        {
            string s = string.Empty;
            SingleInstance.Instance.IJsonServices.BuildJson_Msg_rstdata(out s, SourceJson);
            SendToBoardCard(ref s, tcpSerModel, tcpSerModel.BoardCard_DataBaseTcpClient);
        }


        #endregion



        #region  推理机调用的函数

        public void ConnectToMasterComputer(int num ,TcpCliModel tcpCliModel)
        {
            // 获取需要的变量
            int ReciveDelayTime = int.Parse(Instance.config["InferComputer1:Delay:ReciveDelayTime"]);
            int maxRetryCount = int.Parse(Instance.config["InferComputer1:Retry:RetryMaxTimes"]);
            int retryDelaySeconds = int.Parse(Instance.config["InferComputer1:Delay:RetryDelaySeconds"]);

            // 使用 Task.Run 异步执行连接逻辑
            Task.Run(() =>
            {
                int retryCount = 0;
                bool isConnected = false;

                while (retryCount < maxRetryCount && !isConnected)
                {
                    try
                    {
                        // 创建 TcpClient 并尝试连接到服务器
                        tcpCliModel.cli = new TcpClient();

                        if(num == 1)
                        {
                            tcpCliModel.cli.Connect(tcpCliModel.SerIpAddressForInferComputer1, tcpCliModel.SerPortForInferComputer1);

                            // 连接成功
                            isConnected = true;
                            Instance.InferComputer12MasterComputer.Debug($"成功连接到服务器 {tcpCliModel.SerIpAddressForInferComputer1}:{tcpCliModel.SerPortForInferComputer1}");
                        }
                        else if(num ==2)
                        {
                            tcpCliModel.cli.Connect(tcpCliModel.PLCSerIpAddressForInferComputer2, tcpCliModel.PLCSerPortForInferComputer2);

                            // 连接成功
                            isConnected = true;
                            Instance.InferComputer12MasterComputer.Debug($"成功连接到服务器 {tcpCliModel.PLCSerIpAddressForInferComputer2}:{tcpCliModel.PLCSerPortForInferComputer2}");
                        }


                        // 分配线程处理服务器消息
                        Thread clientThread = new Thread(() => HandleMasterComputerThread(ReciveDelayTime, tcpCliModel));
                        clientThread.Start();
                    }
                    catch (Exception ex)
                    {
                        // 记录连接失败的错误信息
                        retryCount++;
                        Instance.InferComputer12MasterComputer.Debug($"连接到服务器失败 (第 {retryCount} 次尝试): {ex.Message}");

                        if (retryCount < maxRetryCount)
                        {
                            Instance.InferComputer12MasterComputer.Debug($"将在 {retryDelaySeconds} 秒后重试...");
                            Thread.Sleep(retryDelaySeconds * 1000);
                        }
                        else
                        {
                            Instance.InferComputer12MasterComputer.Debug("已达到最大重试次数，无法连接到服务器。");
                        }
                    }
                }
            });
        }

        public void HandleMasterComputerThread(int delayTime, TcpCliModel tcpCliModel)
        {

            if (!CheckTcpClient(tcpCliModel.cli))
            {
                Instance.InferComputer12MasterComputer.Fatal("上位机套接字不可用...");
                return;
            }
            // 获取服务器的网络流
            NetworkStream stream = tcpCliModel.cli.GetStream();

            // 创建缓冲区、数据读得得量、json容器
            byte[] buffer = new byte[1024];
            int bytesRead;
            StringBuilder jsonMessageBuilder = new StringBuilder();

            //处理信息
            try
            {
                //如果和服务器有连接
                while (tcpCliModel.cli.Connected)
                {
                    // 检查是否有数据可读
                    if (stream.DataAvailable)
                    {
                        //读二进制数据到buffer
                        if ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // 将本次读取的二进制数据转化为字符串，并累加到 StringBuilder 中
                            jsonMessageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        }

                        // 检查 StringBuilder 中是否有完整的 JSON 数据
                        while (jsonMessageBuilder.Length > 0)
                        {
                            string currentData = jsonMessageBuilder.ToString();
                            // 检查是否是完整的 JSON 数据（起始为 `{`，结束为 `}`）
                            if (currentData.StartsWith('{') && currentData.EndsWith('}'))
                            {
                                try
                                {
                                    // 转换为 JSON 对象
                                    var messageObject = JsonConvert.DeserializeObject<JSONMessage>(currentData);

                                    // 处理消息（根据不同的逻辑进行分发）
                                    if (messageObject != null)
                                    {
                                        HandleMasterComputer(currentData, tcpCliModel);
                                    }
                                    // 清空 StringBuilder
                                    jsonMessageBuilder.Clear();
                                }
                                catch (System.Text.Json.JsonException ex)
                                {
                                    Log.Error($"JSON 解析失败: {ex.Message}");
                                    jsonMessageBuilder.Clear(); // 清空缓存，防止后续读取错误
                                }
                            }
                            else
                            {
                                // 如果不是完整的 JSON 数据，退出当前检查，继续读取数据
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 如果没有数据可用，主动等待（防止空循环占用 CPU 资源）
                        Thread.Sleep(delayTime);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"客户端连接处理失败: {ex.Message}");
            }
            finally
            {
                // 确保连接关闭
                stream.Close();
                tcpCliModel.cli.Close();
            }
        }

        public void SendToMasterComputer(ref string JsonMessage, TcpCliModel tcpCliModel)
        {
            if (!CheckTcpClient(tcpCliModel.cli))
            {
                Log.Fatal("上位机套接字不可用...");
                return;
            }

            try
            {
                //获取流
                NetworkStream networkStream = tcpCliModel.cli.GetStream();

                // 将 JSON 数据转为字节流
                byte[] responseBytes = Encoding.UTF8.GetBytes(JsonMessage);

                if (networkStream.CanWrite)
                {
                    //发送请求
                    try
                    {
                        // 发送数据到客户端
                        networkStream.Write(responseBytes, 0, responseBytes.Length);

                        // 使用正则表达式提取 Cmd 字段
                        try
                        {
                            // 正则表达式匹配 "Cmd": 后的内容
                            var match = tcpCliModel.regex_Cmd.Match(JsonMessage);
                            string cmd = match.Success ? match.Groups[1].Value : "UnknownCmd";

                            // 格式化日志
                            Log.Information($"向上位机软件发送了 {cmd}：{JsonMessage}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"正则解析 Cmd 时发生错误: {ex.Message}");
                        }


                        //清除缓存
                        JsonMessage = string.Empty;

                    }
                    catch (Exception ex)
                    {
                        Log.Error($"发送数据时发生错误: {ex.Message}");
                    }
                }

            }
            catch (IOException ex)
            {
                Instance.InferComputer12MasterComputer.Error($"连接已关闭，异常: {ex.Message}");
            }
            catch (SocketException ex)
            {
                Instance.InferComputer12MasterComputer.Error($"Socket 异常: {ex.Message}");
            }

        }




        // 主处理函数,每次有一条板卡消息调用一次这个函数分发任务
        private void HandleMasterComputer(string JsonSource, TcpCliModel tcpCliModel)
        {
            try
            {
                // 使用正则表达式解析 Cmd 字段
                var match = tcpCliModel.regex_Cmd.Match(JsonSource);
                if (match.Success)
                {
                    string cmd = match.Groups[1].Value;

                    // 根据 Cmd 调用对应的函数
                    switch (cmd)
                    {

                        case "Msg_data":

                            _Handle_Msg_data(JsonSource, tcpCliModel);

                            break;

                        case "Msg_camera":

                            _ = _Handle_Msg_camera(JsonSource, tcpCliModel);

                            break;

                        default:
                            Instance.InferComputer12MasterComputer.Error($"未知的 Cmd 类型: {cmd}");
                            break;
                    }
                }
                else
                {
                    Instance.InferComputer12MasterComputer.Error("无法解析 Cmd 字段。");
                }
            }
            catch (Exception ex)
            {
                Instance.InferComputer12MasterComputer.Error($"处理 JSON 数据时发生异常: {ex.Message}");
            }
        }

        private void _Handle_Msg_data(string SourceJson, TcpCliModel tcpCliModel)
        {
            tcpCliModel.stopwatch.Reset();
            tcpCliModel.stopwatch.Start();
            string cTresult = string.Empty;
            string fpicAdd = string.Empty;
            string bpicAdd = string.Empty;
            string JsonContainer = string.Empty;

            try
            {
                // 解析 JSON 数据
                var jsonObject = JObject.Parse(SourceJson);

                //提取cDev
                string cDev = jsonObject["cDev"]?.ToObject<string>();

                // 提取布尔值
                int cTest = jsonObject["cTest"]?.ToObject<int>() ?? 0;
                bool fcamEn = jsonObject["fcamEn"]?.ToObject<bool>() ?? false;
                bool bcamEn = jsonObject["bcamEn"]?.ToObject<bool>() ?? false;

                //检测相机需要立即工作得到NG情况
                if (cTest == 1)
                {
                    if (cDev == "A0")
                    {
                        //推理机请求相机拍照且将照片给算法计算，得到明暗码和NG情况，暂时放到cTresult中
                        cTresult = tcpCliModel.IAlgorithmService.InspectionAlgorithm(
                        tcpCliModel.ICameraService.ActivateInspectionCamera(3, tcpCliModel.cameraModel),
                        tcpCliModel.ICameraService.ActivateInspectionCamera(4, tcpCliModel.cameraModel),
                        tcpCliModel.algorithmModel,
                        int.Parse(Instance.config["InferComputer1:TestTime:InspectationAlgorithmTime"]));

                    }

                    else if (cDev == "B0")
                    {
                        //推理机请求相机拍照且将照片给算法计算，得到明暗码和NG情况，暂时放到cTresult中
                        cTresult = tcpCliModel.IAlgorithmService.InspectionAlgorithm(
                        tcpCliModel.ICameraService.ActivateInspectionCamera(7, tcpCliModel.cameraModel),
                        tcpCliModel.ICameraService.ActivateInspectionCamera(8, tcpCliModel.cameraModel),
                        tcpCliModel.algorithmModel,
                        int.Parse(Instance.config["InferComputer1:TestTime:InspectationAlgorithmTime"]));

                    }

                    else
                    {
                        Instance.InferComputer12MasterComputer.Fatal("出现了除了A0和B0之外的不合法的产线？？？请联系程序员修改代码，这里NG踢掉");
                        cTresult = "？？？？？？？";
                    }


                }
                //需要约定上定位照片名字,可能需要根据存的图的格式，修改，先写死bmp
                if (fcamEn)
                {
                    // 使用 Interlocked 对 times 进行原子操作
                    ulong currentTimes = (ulong)Interlocked.Increment(ref tcpCliModel.times);
                    fpicAdd = $"{currentTimes}f.bmp";
                }
                //需要约定下定位照片名字
                if (bcamEn)
                {
                    ulong currentTimes = (ulong)Interlocked.Increment(ref tcpCliModel.times);
                    bpicAdd = $"{currentTimes}b.bmp";
                }

                //构建Json数据
                tcpCliModel.IJsonServices._BuildJson_Msg_data(out JsonContainer, SourceJson, cTresult, fpicAdd, bpicAdd);

                //测试打印
                Console.WriteLine(JsonContainer);

                //发送回上位机软件
                SendToMasterComputer(ref JsonContainer, tcpCliModel);

            }
            catch (Exception ex)
            {
                Instance.InferComputer12MasterComputer.Error($"处理 Msg_data 时发生异常: {ex.Message}");
            }
            tcpCliModel.stopwatch.Start();
            Instance.InferComputer12MasterComputer.Debug($"_Handle_Msg_data 执行时间：{tcpCliModel.stopwatch.ElapsedMilliseconds} ms");
        }

        private async Task _Handle_Msg_camera(string SourceJson, TcpCliModel tcpCliModel)
        {
            tcpCliModel.stopwatch.Reset();
            tcpCliModel.stopwatch.Start();
            double[] aZData = null;
            double[] aFData = null;
            double[] bZData = null;
            double[] bFData = null;
            string JsonContainer = string.Empty;

            // 解析 JSON 数据
            var jsonObject = JObject.Parse(SourceJson);

            //提取aAddpic
            string? aAddpic = jsonObject["aAddpic"]?.ToObject<string>();
            //提取bAddpic
            string? bAddpic = jsonObject["bAddpic"]?.ToObject<string>();
            //提取cDev
            string? cDev = jsonObject["cDev"]?.ToObject<string>();

            var tasks = new List<Task>();

            // 异步操作 aAddpic
            if (aAddpic != null)
            {
                tasks.Add(Task.Run(async () =>
                {
                    byte[]? bytes = null;
                    if (cDev == "A0")
                    {
                        //请求A线上定位相机拍照，并且按名字存储
                        bytes = await Task.FromResult(tcpCliModel.ICameraService.ActivatePositioningCamera(1, tcpCliModel.cameraModel));
                    }
                    else if (cDev == "B0")
                    {
                        //请求B线上定位相机拍照，并且按名字存储
                        bytes = await Task.FromResult(tcpCliModel.ICameraService.ActivatePositioningCamera(5, tcpCliModel.cameraModel));
                    }
                    else
                    {
                        Instance.InferComputer12MasterComputer.Fatal("出现了除了A0和B0之外的不合法的产线？？？请联系程序员修改代码");
                    }
                    // 计算得到两个 double 数组
                    var result = await Task.FromResult(tcpCliModel.IAlgorithmService.PositionAlgorithm(bytes, tcpCliModel.algorithmModel, int.Parse(Instance.config["InferComputer1:TestTime:PositionAlgorithmTime"])));
                    aZData = result.ZData;
                    aFData = result.FData;
                }));
            }

            // 异步操作 bAddpic
            if (bAddpic != null)
            {
                tasks.Add(Task.Run(async () =>
                {
                    byte[] bytes = null;
                    if (cDev == "A0")
                    {
                        //请求A线下定位相机拍照，并且按名字存储
                        bytes = await Task.FromResult(tcpCliModel.ICameraService.ActivatePositioningCamera(2, tcpCliModel.cameraModel));
                    }
                    else if (cDev == "B0")
                    {
                        //请求B线下定位相机拍照，并且按名字存储
                        bytes = await Task.FromResult(tcpCliModel.ICameraService.ActivatePositioningCamera(6, tcpCliModel.cameraModel));
                    }
                    else
                    {
                        Instance.InferComputer12MasterComputer.Fatal("出现了除了A0和B0之外的不合法的产线？？？请联系程序员修改代码");
                    }
                    // 计算得到两个 double 数组
                    var result = await Task.FromResult(tcpCliModel.IAlgorithmService.PositionAlgorithm(bytes, tcpCliModel.algorithmModel, int.Parse(Instance.config["InferComputer1:TestTime:PositionAlgorithmTime"])));
                    bZData = result.ZData;
                    bFData = result.FData;
                }));
            }

            // 等待所有异步任务完成
            await Task.WhenAll(tasks);

            // 打包成 Json
            tcpCliModel.IJsonServices._BuildJson_Msg_camera(out JsonContainer, SourceJson, aZData, aFData, bZData, bFData);

            // 发送给上位机软件
            SendToMasterComputer(ref JsonContainer, tcpCliModel);
            tcpCliModel.stopwatch.Start();
            Instance.InferComputer12MasterComputer.Debug($"_Handle_Msg_camera 执行时间：{tcpCliModel.stopwatch.ElapsedMilliseconds} ms");
        }


        #endregion






        public bool CheckTcpClient(TcpClient? tcpClient)
        {
            // 检查 TcpClient 是否为 null
            if (tcpClient == null)
            {
                Log.Error("TcpClient 未初始化。");
                return false;
            }

            // 检查是否已连接
            if (!tcpClient.Connected)
            {
                Log.Error("TcpClient 未连接到服务器。");
                return false;
            }

            // 检查 NetworkStream 是否可用
            try
            {
                var stream = tcpClient.GetStream();
                if (!stream.CanRead || !stream.CanWrite)
                {
                    Log.Error("TcpClient 的网络流不可用。");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"检查 TcpClient 网络流时发生异常: {ex.Message}");
                return false;
            }

            return true;
        }

        public void PrintConnectedClients(TcpSerModel tcp)
        {
            // 定义一个列表存储所有的 TcpClient 和其名称
            var clients = new List<(TcpClient? Client, string Name)>
    {
        (tcp.BoardCard_DataBaseTcpClient, nameof(tcp.BoardCard_DataBaseTcpClient)),
        (tcp.InferTcpClient1, nameof(tcp.InferTcpClient1)),
        (tcp.InferTcpClient2, nameof(tcp.InferTcpClient2))
    };

            // 遍历列表并打印每个 TcpClient 的信息
            foreach (var (client, name) in clients)
            {
                if (client != null && client.Connected)
                {
                    try
                    {
                        // 获取客户端的远程终端点
                        var remoteEndPoint = client.Client.RemoteEndPoint as System.Net.IPEndPoint;
                        if (remoteEndPoint != null)
                        {
                            Console.WriteLine($"套接字名称: {name}, 客户端 IP: {remoteEndPoint.Address}, 端口: {remoteEndPoint.Port}");
                        }
                        else
                        {
                            Console.WriteLine($"套接字名称: {name}, 已连接但无法获取远程终端信息。");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"套接字名称: {name}, 获取客户端信息时发生错误: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"套接字名称: {name}, 未连接或为空。");
                }
            }
        }







    }


}




