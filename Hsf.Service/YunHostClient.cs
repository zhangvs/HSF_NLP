using Hsf.Framework;
using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hsf.Service
{
    public class YunHostClient
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("YunHostClient");

        //客户端对象
        static AsyncTcpSession tcpClient = null;
        static System.Timers.Timer xt_timer = null;
        static System.Timers.Timer cl_timer = null;

        public static void ConnectServer()
        {
            tcpClient = new AsyncTcpSession(new IPEndPoint(IPAddress.Parse("192.168.88.58"), 9004));
            tcpClient.DataReceived += TcpClient_DataReceived;
            tcpClient.Error += TcpClient_Error;
            tcpClient.Closed += TcpClient_Closed;
            tcpClient.Connect();
        }

        public static void ReConnect()
        {
            if (cl_timer != null)
            {
                //先停止之前的重连
                cl_timer.Stop();
                cl_timer.Enabled = false;
                cl_timer.Dispose();
            }

            //每5秒发一次心跳
            cl_timer = new System.Timers.Timer(5000);
            //cl_timer.Elapsed += new System.Timers.ElapsedEventHandler((s, x) =>{});
            cl_timer.Elapsed += OnTimedEvent;
            cl_timer.Enabled = true;
            cl_timer.Start();
        }
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            //Task.Run(() =>
            //{
            if (tcpClient != null)
            {
                if (tcpClient.IsConnected)
                {
                    cl_timer.Stop();
                    cl_timer.Enabled = false;
                    cl_timer.Dispose();
                    Heartbeat();
                    log.Info("连接YunHost断线重连成功！！开始心跳！！");
                }
                else
                {
                    tcpClient = null;
                    ConnectServer();
                }
            }
            else
            {
                ConnectServer();
                log.Error("连接YunHost断线重连失败！！");
            }
            //});
        }


        public static void Heartbeat()
        {
            Task.Run(() =>
            {
                if (tcpClient != null)
                {
                    if (tcpClient.IsConnected)
                    {
                        if (xt_timer != null)
                        {
                            //先停止之前的心跳
                            xt_timer.Stop();
                            xt_timer.Enabled = false;
                            xt_timer.Dispose();
                        }

                        //每5秒发一次心跳
                        xt_timer = new System.Timers.Timer(5000);
                        xt_timer.Elapsed += new System.Timers.ElapsedEventHandler((s, x) =>
                        {
                            if (tcpClient.IsConnected)
                            {
                                SendMsg("{\"code\":010}");
                            }
                        });
                        xt_timer.Enabled = true;
                        xt_timer.Start();
                    }
                }
            });
        }

        public static void SendMsg(string message)
        {
            if (tcpClient != null && tcpClient.IsConnected)
            {
                try
                {
                    #region 发送字符串
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    tcpClient.Send(data, 0, data.Length);
                    #endregion
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                log.Error("YunHost网关客户端连接未启动,发送失败！" + message);
                Console.WriteLine("YunHost网关客户端连接未启动,发送失败！" + message);
            }
        }
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TcpClient_Closed(object sender, EventArgs e)
        {
            log.Error("YunHost网关客户端断开连接！！");
            Console.WriteLine("YunHost网关客户端断开连接！！");
            ReConnect();
        }

        /// <summary>
        /// 连接发生异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TcpClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            log.Error("连接YunHost网关服务器异常！！");
            Console.WriteLine("连接YunHost网关服务器异常！！");
        }

        /// <summary>
        /// 接到的服务器消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TcpClient_DataReceived(object sender, DataEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Data, 0, e.Length);
            log.Info("收到YunHost网关服务器消息：" + message);
        }

    }
}
