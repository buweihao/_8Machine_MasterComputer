using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using System.Configuration;


using MachDBTcp.Services;
using MachDBTcp.Interfaces;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using static _8Machine_MasterComputer.Instance.SingleInstance;



namespace MachDBTcp.Models
{
    /// <summary>
    /// Tcp类依赖
    /// </summary>
    public class TcpSerModel
    {
        //设置本机服务器的IP地址和端口号
        public string ipAddress1 = Instance.config["MasterComputer:IP:BoardCardSeverIP"];
        public int port1 = int.Parse(Instance.config["MasterComputer:Port:BoardCardSeverPort"]);
        public string ipAddress2 = Instance.config["MasterComputer:IP:InferComputerServeIP"];
        public int port2 = int.Parse(Instance.config["MasterComputer:Port:InferComputerServerPort"]);

        //根据项目，需要有以下套接字
        public TcpClient? BoardCardTcpClient;
        public TcpClient? InferTcpClient1;
        public TcpClient? InferTcpClient2;
        public TcpClient? MarkingClientUp;
        public TcpClient? MarkingClientDn;

        //正则表达式
        public Regex regex_Cmd = new Regex(@"""Cmd"":\s*""([^""]+)""", RegexOptions.Compiled);
        public Regex regex_act = new Regex(@"""act""\s*:\s*(\d+)", RegexOptions.Compiled);

        //计时器
        public Stopwatch stopwatch = new Stopwatch();

        //耦合部分
        //注入所有库model
        public _8Machine_Json.Models.JsonModel jsonModel = Instance.jsonModel;
        public _8Machine_MachDB.Models.MachDBModel machDBModel = Instance.machDBModel;

        //注入所有IService
        public _8Machine_Json.Interfaces.IJsonServices IJsonServices = Instance.IJsonServices;
        public _8Machine_MachDB.Interfaces.IMachDBServices IMachDBServices = Instance.IMachDBServices;

        public static string GetCmd(string json, TcpSerModel tcpSerModel)
        {
            // 使用 regex_Cmd 正则表达式来匹配 JSON 字符串中的 "Cmd" 字段
            var match = tcpSerModel.regex_Cmd.Match(json);

            // 如果匹配成功，返回 "Cmd" 字段的值，否则返回 "UnknownCmd"
            return match.Success ? match.Groups[1].Value : "UnknownCmd";
        }
        public static string GetAct(string inferComputerReturnJson, TcpSerModel tcpSerModel)
        {
            var match = tcpSerModel.regex_act.Match(inferComputerReturnJson);
            return match.Success ? match.Groups[1].Value : "UnknownAct";
        }
    }
}
