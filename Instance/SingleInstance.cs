using _8Machine_MasterComputer.ViewModel;
using MachDBTcp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachDBTcp.Interfaces;
using MachDBTcp.Services;
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

        // ViewModel
        public MasterComputerVM masterComputerVM { get; private set; }
        public InferComputer1VM inferComputer1VM { get; private set; }
        public InferComputer2VM inferComputer2VM { get; private set; }
        public DataBaseVM dataBaseVM { get; private set; }

        // 本项目model，存储和分发其它model和service
        public TcpSerModel tcpSerModel { get; set; }
        public TcpCliModel tcpCliModel { get; set; }

        // 本项目model用于分发到各库中的model
        public MyAlgorithm.Models.AlgorithmModel algorithmModel { get; }
        public MyCamera.Models.CameraModel cameraModel { get; }
        public MyJson.Models.JsonModel jsonModel { get; }
        public MyDatabase.Models.DatabaseModel machDBModel { get; set; }

        // 本项目service用于分发到各库中的service
        public ITcpService ITcpService { get; }
        public MyAlgorithm.Interfaces.IAlgorithmService IAlgorithmService { get; }
        public MyCamera.Interfaces.ICameraService ICameraService { get; }
        public MyJson.Interfaces.IJsonServices IJsonServices { get; }
        public MyDatabase.Interfaces.IDatabaseServices IMachDBServices { get; }

        //json全局变量访问点
        public IConfigurationRoot config { get; }

        //日志系统访问点
        public ILogger MasterComputer2BoardCardLog { get; private set; }
        public ILogger MasterComputer2InferComputer1 { get; private set; }
        public ILogger InferComputer12MasterComputer { get; private set; }

        //用户选择的打标机图元信息
        public  string up_actfileName;
        public string dn_actfileName;

        //A线上
        public string[] up_picName_A;   //A线上图元名称
        public int[] up_picDb_A;        //A线上图元数据列

        //A线下
        public int[] dn_picDb_A;        //A线下图元名称
        public string[] dn_picName_A;   //A线下图元数据列

        //B线上
        public string[] dn_picName_B;   //B线上图元名称
        public int[] up_picDb_B;        //B线上图元数据列

        //B线下
        public string[] up_picName_B;   //A线下图元名称
        public int[] dn_picDb_B;        //A线下图元数据列

        //主键的数据列
        public int PrimarykeyColumn = 0;




        // 私有构造函数，防止外部实例化,这个函数将会在单例首次被使用时执行
        private SingleInstance()
        {
            // 初始化单例对象
            //tcpSerModel = new TcpSerModel();
            //tcpCliModel = new TcpCliModel();
            ITcpService = new TcpService();

            //用于分发的model和service
            algorithmModel = new MyAlgorithm.Models.AlgorithmModel();
            cameraModel = new MyCamera.Models.CameraModel();
            jsonModel = new MyJson.Models.JsonModel();

            IAlgorithmService = new MyAlgorithm.Services.AlgorithmService();
            ICameraService = new MyCamera.Services.CameraService();
            IJsonServices = new MyJson.Services.JsonServices();
            IMachDBServices = new MyDatabase.Services.DatabaseServices();

            //配置日志
            config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // 设置基路径
            .AddJsonFile("Config/Config000.json", optional: false, reloadOnChange: true) // 加载 JSON 文件
            .Build();

            

        }

        //特殊情况，需要在xmal启动时创建单例，因为需要注入一个控件
        public void InitializeMasterComputerViewModel(TextWriter textBoxWriter)
        {
            masterComputerVM = new MasterComputerVM(textBoxWriter);
            logConfig("MasterComputer");
        }

        public void InitializeInferComputer1ViewModel()
        {
            inferComputer1VM = new InferComputer1VM();
            logConfig("InferComputer1");
        }
         public void InitializeInferComputer2ViewModel()
        {
            inferComputer2VM = new InferComputer2VM();
            logConfig("InferComputer1");
        }


        public void InitializeDataBaseViewModel()
        {
            dataBaseVM = new DataBaseVM();
            //logConfig("InferComputer1");
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
