﻿using _8Machine_MasterComputer;
using _8Machine_MasterComputer.Instance;
using _8Machine_MasterComputer.View;
using Docker.DotNet.Models;
using MachDBTcp.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static _8Machine_MasterComputer.Instance.SingleInstance;
namespace MachDBTcp.Test
{
    public class MachDBTcpTestMain
    {



        public static void MasterComputer()
        {
            //获取所需实例
            var tcpModel = new TcpSerModel();
            var iTcpServices = Instance.ITcpService;
            Instance.tcpSerModel = tcpModel;

            //基础接口测试
            Console.WriteLine("开始测试MachDBTcp上位机软件库函数....");

            //0、测试Log
            Instance.MasterComputer2BoardCardLog.Verbose("日常");
            Instance.MasterComputer2BoardCardLog.Debug("调试");
            Instance.MasterComputer2BoardCardLog.Information("消息");
            Instance.MasterComputer2BoardCardLog.Warning("警告");
            Instance.MasterComputer2BoardCardLog.Error("错误");
            Instance.MasterComputer2BoardCardLog.Fatal("致命");

            //1、测试Tcp的启动 Start
            Console.WriteLine("开始测试函数：Start");
            iTcpServices.Start(tcpModel);

        }


        public static void InferComputer1()
        {
            //获取所需实例
            var tcpCliModel = new TcpCliModel();
            var ITcpServices = Instance.ITcpService;
            Instance.tcpCliModel = tcpCliModel;

            //基础接口测试
            Console.WriteLine("开始测试MachDBTcp推理机1库函数....");

            //1、连接服务器
            ITcpServices.ConnectToMasterComputer(tcpCliModel);
        }



    }

}
