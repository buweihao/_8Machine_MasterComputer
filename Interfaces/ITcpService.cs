using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MachDBTcp.Models;
using MachDBTcp.Services;

namespace MachDBTcp.Interfaces
{
    public interface ITcpService
    {
        #region  上位机软件需要调用的方法
        /// <summary>
        /// 开启服务器端Tcp通信
        /// </summary>
        /// <param name="tcp"></param>
        /// <param name="handleClientMessages">应对新客户端的服务方法，根据应对方调用</param>
        /// <param name="ListenDelayTime">（可选）轮询监听新客户端连接的间隔</param>
        /// <param name="ReciveDelayTime">（可选）轮询监听客户端消息的间隔</param>
        public void Start(TcpSerModel tcp);


        /// <summary>
        /// 上位机软件（服务器端）通过Tcp发送信息给板卡
        /// </summary>
        /// <param name="JsonMessage">需要发送的消息</param>
        /// <param name="tcp"></param>
        public void SendToBoardCard(ref string JsonMessage,  TcpSerModel tcp, TcpClient tcpClient);

        /// <summary>
        /// 上位机软件（服务器端）通过Tcp发送信息给推理机，并等待回复
        /// </summary>
        /// <param name="JsonMesssage">需要发送给推理机的消息</param>
        /// <param name="tcpClient">推理机的套接字（因为有两台所以需要精准传入）</param>
        /// <param name="tcp"></param>
        /// <param name="timeout">（可选）最长阻塞等待回复时间</param>
        public string SendToInferComputerAndWaitForReturn(ref string JsonMessage, TcpClient tcpClient, TcpSerModel tcp, int timeout = 500);

        /// <summary>
        /// 上位机软件（服务器端）通过Tcp发送信息给打标软件，并等待回复
        /// </summary>
        /// <param name="JsonMessage"></param>
        /// <param name="tcpClient"></param>
        /// <param name="tcp"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public string SendToMarkingSoftwareAndWaitForReturn(ref string JsonMessage, TcpClient tcpClient, TcpSerModel tcp, int timeout = 5000);




        #endregion

        #region  推理机需要调用的方法

        /// <summary>
        /// 推理机发起Tcp连接到上位机软件 
        /// </summary>
        /// <param name="tcpCliModel"></param>
        /// <param name="ReciveDelayTime">（可选）推理机监听上位机软件消息间隔，建议给小值，提高推理机相应速度</param>
        /// <param name="maxRetryCount">（可选）最大重连次</param>
        /// <param name="retryDelaySeconds">（可选）重连间隔</param>
        public void ConnectToMasterComputer(int num, TcpCliModel tcpCliModel);


        /// <summary>
        /// 推理服务器通过Tcp发送消息到上位机软件
        /// </summary>
        /// <param name="JsonMessage">需要发送的Json数据</param>
        /// <param name="tcpCliModel"></param>
        public void SendToMasterComputer(ref string JsonMessage, TcpCliModel tcpCliModel);
        #endregion


        public void PrintConnectedClients(TcpSerModel tcp);
        public  bool CheckTcpClient(TcpClient? tcpClient);













    }
}
