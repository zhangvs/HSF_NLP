using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hsf.Framework
{
    public class NlpSocketManager
    {
        public static Socket clientSocket = null;
        public static IPAddress ipAddress = null;
        public static byte[] result = new byte[1024];

        /// <summary>  
        /// 判断是否已连接  
        /// </summary>  
        public static bool Connected
        {
            get { return clientSocket != null && clientSocket.Connected; }
        }

        /// <summary>  
        /// 连接到服务器  
        /// </summary>  
        /// <returns></returns>  
        public static void Connect()
        {
            try
            {
                if (Connected) return;
                //我这里是读取配置
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string ip = config.AppSettings.Settings["server"].Value;
                int port = Convert.ToInt32(config.AppSettings.Settings["port"].Value);
                ipAddress = IPAddress.Parse(ip);

                //创建连接对象, 连接到服务器  
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//实例化WebSocketServer
                                                                                                           //添加事件侦听
                clientSocket.Connect(new IPEndPoint(ipAddress, port)); //配置服务器IP与端口
                int receiveLength = clientSocket.Receive(result);//通过clientSocket接收数据
                string returnMsg = Encoding.UTF8.GetString(result, 0, receiveLength);
                Console.WriteLine($"连接到智能家居nlp服务器: {returnMsg}");
            }
            catch (SocketException e)
            {
                Console.WriteLine("智能家居nlp连接失败");
                Console.WriteLine(e.ToString());
                return;
            }

        }
    }
}
