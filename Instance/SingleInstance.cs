using _8Machine_MasterComputer.ViewModel;
using MachDBTcp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _8Machine_Algorithm;
using MachDBTcp.Interfaces;
using MachDBTcp.Services;
using _8Machine_Camera.Models;
using MachDBTcp.Test;
using System.Windows.Controls;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Enrichers.CallerInfo;
using _8Machine_MasterComputer.View;

namespace _8Machine_MasterComputer.Instance
{
    public sealed class SingleInstance
    {
        // 使用 Lazy<T> 实现线程安全的单例
        private static readonly Lazy<SingleInstance> instance = new(() => new SingleInstance());

        // 全局访问点
        public static SingleInstance Instance => instance.Value;

        // 单例对象
        public MasterComputerVM masterComputerVM { get; private set; }
        public InferComputer1VM inferComputer1VM { get; private set; }
        public TcpSerModel tcpSerModel { get; set; }
        public TcpCliModel tcpCliModel { get; set; }
        public _8Machine_Algorithm.Models.AlgorithmModel algorithmModel { get; }
        public _8Machine_Camera.Models.CameraModel cameraModel { get; }
        public _8Machine_Json.Models.JsonModel jsonModel { get; }
        public _8Machine_MachDB.Models.MachDBModel machDBModel { get; }

        public ITcpService ITcpService { get; }
        public _8Machine_Algorithm.Interfaces.IAlgorithmService IAlgorithmService { get; }
        public _8Machine_Camera.Interfaces.ICameraService ICameraService { get; }
        public _8Machine_Json.Interfaces.IJsonServices IJsonServices { get; }
        public _8Machine_MachDB.Interfaces.IMachDBServices IMachDBServices { get; }
        public IConfigurationRoot config { get; }

        public ILogger MasterComputer2BoardCardLog { get; private set; }
        public ILogger MasterComputer2InferComputer1 { get; private set; }
        public ILogger InferComputer12MasterComputer { get; private set; }



        // 私有构造函数，防止外部实例化
        private SingleInstance()
        {
            // 初始化单例对象
            //tcpSerModel = new TcpSerModel();
            //cliModel = new TcpCliModel();

            algorithmModel = new _8Machine_Algorithm.Models.AlgorithmModel();
            cameraModel = new _8Machine_Camera.Models.CameraModel();
            jsonModel = new _8Machine_Json.Models.JsonModel();
            machDBModel = new _8Machine_MachDB.Models.MachDBModel();

            ITcpService = new TcpService();
            IAlgorithmService = new _8Machine_Algorithm.Services.AlgorithmService();
            ICameraService = new _8Machine_Camera.Services.CameraService();
            IJsonServices = new _8Machine_Json.Services.JsonServices();
            IMachDBServices = new _8Machine_MachDB.Services.MachDBServices();
            
            config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // 设置基路径
            .AddJsonFile("Config/Config000.json", optional: false, reloadOnChange: true) // 加载 JSON 文件
            .Build();

            

        }
        public void Initialize()
        {
            // 确保在应用程序启动时初始化
            Console.WriteLine("SingleInstance initialized.");
        }

        //特殊情况，需要在xmal启动时创建单例，因为需要注入一个控件
        public void InitializeMasterComputerViewModel(TextWriter textBoxWriter)
        {
            masterComputerVM = new MasterComputerVM(textBoxWriter);
            logConfig("MasterComputer");
        }

        public void InitializeInferComputerViewModel()
        {
            inferComputer1VM = new InferComputer1VM();
            logConfig("InferComputer1");
        }


        private  void logConfig(string s)
        {
            string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            // 配置 Serilog

            if (s == "MasterComputer")
            {
                MasterComputer2BoardCardLog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(LogEventLevel.Debug, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (Source:{SourceFile}:{LineNumber}){NewLine}")
                .WriteTo.File("MasterComputer2BoardCardLog/app.log", restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (Source:{SourceFile}:{LineNumber}){NewLine}") // 输出到文件
               .WriteTo.TextWriter(Instance.masterComputerVM._textBoxWriter, restrictedToMinimumLevel: LogEventLevel.Information, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (Source:{SourceFile}:{LineNumber}){NewLine}") // 界面从 Warning 开始                                                                                                                                     //.WriteTo.Async(a => a.Console()) // 将 Console Sink 配置为异步
                .Enrich.WithCallerInfo(
                    includeFileInfo: true, // 启用文件信息
                    allowedAssemblies: new[] { assemblyName }, // 修改为新程序集名称
                    prefix: "",
                    filePathDepth: 1
                )
                .CreateLogger();

                MasterComputer2InferComputer1 = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console(LogEventLevel.Debug, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (Source:{SourceFile}:{LineNumber}){NewLine}")
                    .WriteTo.File("MasterComputer2InferComputer1/app.log", restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (Source:{SourceFile}:{LineNumber}){NewLine}") // 输出到文件
                    .Enrich.WithCallerInfo(
                        includeFileInfo: true, // 启用文件信息
                        allowedAssemblies: new[] { assemblyName }, // 修改为新程序集名称
                        prefix: "",
                        filePathDepth: 1
                    )
                    .CreateLogger();

            }

            else if(s == "InferComputer1")
            {
                InferComputer12MasterComputer = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console(LogEventLevel.Debug, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (Source:{SourceFile}:{LineNumber}){NewLine}")
                    .WriteTo.File("MasterComputer2InferComputer1/app.log", restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (Source:{SourceFile}:{LineNumber}){NewLine}") // 输出到文件
                    .Enrich.WithCallerInfo(
                        includeFileInfo: true, // 启用文件信息
                        allowedAssemblies: new[] { assemblyName }, // 修改为新程序集名称
                        prefix: "",
                        filePathDepth: 1
                    )
                    .CreateLogger();
            }

            else if(s == "InferComputer2")
            {

            }






            //Log.Verbose("Verbose 日志");
            //Log.Debug("Debug 日志");
            //Log.Information("Information 日志");
            //Log.Warning("警告");
            //Log.Error("错误");
            //Log.Fatal("致命");
            // 捕获未处理的非 UI 异常
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception occurred.");
            };
        }
    }
}
