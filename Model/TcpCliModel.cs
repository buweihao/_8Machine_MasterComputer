using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using _8Machine_MasterComputer.Instance;
using static _8Machine_MasterComputer.Instance.SingleInstance;

namespace MachDBTcp.Models
{
    public class TcpCliModel
    {
        //连接的IP和端口
        public string SerIpAddressForInferComputer1 = Instance.config["InferComputer1:IP:MasterComputerIP"];
        public int SerPortForInferComputer1 = int.Parse(SingleInstance.Instance.config["InferComputer1:Port:MasterComputerPort"]);

        //似乎推理机2不需要连接到上位机
        //public string SerIpAddressForInferComputer2 = Instance.config["InferComputer2:IP:MasterComputerIP"];
        //public int SerPortForInferComputer2 = int.Parse(SingleInstance.Instance.config["InferComputer1:Port:MasterComputerPort"]);
        public string PLCSerIpAddressForInferComputer2 = Instance.config["InferComputer2:IP:PLCIP"];
        public int PLCSerPortForInferComputer2 = int.Parse(SingleInstance.Instance.config["InferComputer2:Port:PLCPort"]);




        //套接字
        public TcpClient? cli;

        //约定图片名字的次数
        public ulong times;

        //正则表达式
        public Regex regex_Cmd = new Regex(@"""Cmd"":\s*""([^""]+)""", RegexOptions.Compiled);

        //计时器
        public Stopwatch stopwatch = new Stopwatch();

        //耦合部分
        //耦合所需要的所有库
        public MyAlgorithm.Models.AlgorithmModel algorithmModel = Instance.algorithmModel;
        public MyCameraClass.Models.CameraModel cameraModel = Instance.cameraModel;
        public MyJson.Models.JsonModel jsonModel = Instance.jsonModel;

        public MyAlgorithm.Interfaces.IAlgorithmService IAlgorithmService = Instance.IAlgorithmService;
        public MyCameraClass.Interfaces.ICameraService ICameraService = Instance.ICameraService;
        public MyJson.Interfaces.IJsonServices IJsonServices = Instance.IJsonServices;




    }
}
