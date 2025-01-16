using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using _8Machine_Algorithm;
using _8Machine_Camera;
using _8Machine_RTSPStreamer;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using _8Machine_MasterComputer.Instance;
using static _8Machine_MasterComputer.Instance.SingleInstance;

namespace MachDBTcp.Models
{
    public class TcpCliModel
    {
        //连接的IP和端口
        public string SerIpAddress = Instance.config["InferComputer1:IP:MasterComputerIP"];
        public int SerPort = int.Parse(SingleInstance.Instance.config["InferComputer1:Port:MasterComputerPort"]);

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
        public _8Machine_Algorithm.Models.AlgorithmModel algorithmModel = Instance.algorithmModel;
        public _8Machine_Camera.Models.CameraModel cameraModel = Instance.cameraModel;
        public _8Machine_Json.Models.JsonModel jsonModel = Instance.jsonModel;
        //public _8Machine_RTSPStreamer.Models.FFmpegRTSPStreamerModel fFmpegRTSPStreamerModel = null;

        public _8Machine_Algorithm.Interfaces.IAlgorithmService IAlgorithmService = Instance.IAlgorithmService;
        public _8Machine_Camera.Interfaces.ICameraService ICameraService = Instance.ICameraService;
        public _8Machine_Json.Interfaces.IJsonServices IJsonServices = Instance.IJsonServices;
        //public _8Machine_RTSPStreamer.Interfaces.IFFmpegRTSPStreamerServices IFFmpegRTSPStreamerServices = null;




    }
}
