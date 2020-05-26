using Hsf.Redis.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Service
{
    /// <summary>
        /// 第一步、定义一个和协议合适的数据对象
        /// 该对象由开始标记、结束标记、消息主体等组成
        /// </summary>
    public class HansReceiveData
    {
        /// <summary>
        /// 开始标记 如："!Start"
        /// </summary>
        public string StartMark { get; set; }
        /// <summary>
        /// 结束标记 如："$End"
        /// </summary>
        public string EndMark { get; set; }
        /// <summary>
        /// 备用字段 暂时用GUID
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 消息主体 消息数据流【除去开始标记、结束标记】
        /// </summary>
        public byte[] BodyBuffer { get; set; }
        /// <summary>
        /// 消息主体 消息内容【除去开始标记、结束标记】
        /// </summary>
        public string BodyString { get; set; }
    }

    /// <summary>
    /// 第二步、请求(RequestInfo) RequestInfo 是表示来自客户端请求的实体类。 每个来自客户端的请求都能应该被实例化为 RequestInfo 类型。
    /// </summary>
    public class HansRequestInfo : SuperSocket.SocketBase.Protocol.RequestInfo<HansReceiveData>
    {
        public HansRequestInfo(string key, HansReceiveData myData)
        {
            //如果需要使用命令行协议的话，那么key与命令类名称myData相同
            Initialize(key, myData);
        }
    }

    /// <summary>
    /// 第三步、接收过滤器(ReceiveFilter) 接收过滤器(ReceiveFilter)用于将接收到的二进制数据转化成请求实例(RequestInfo)。
    /// 需要实现接口 IReceiveFilter
    /// </summary>
    public class HansReceiveFilter : SuperSocket.SocketBase.Protocol.IReceiveFilter<HansRequestInfo>
    {
        /// <summary>
        /// 该接收过滤器已缓存数据的长度:这里用过滤后的数据长度【BodyBuffer.Length】
        /// </summary>
        public int leftBufferSize;
        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding Encoder = Encoding.GetEncoding("gbk");
        /// <summary>
        /// 开始标记 如："!Start"
        /// </summary>
        public string StartMark = "!Start";
        /// <summary>
        /// 结束标记 如："$End"
        /// </summary>
        public string EndMark = "$End";
        public HansReceiveFilter(Encoding encoder, string startMark, string endMark)
        {
            Encoder = encoder;
            StartMark = startMark;
            EndMark = endMark;
        }
        /// <summary>
        /// 该方法将会在 SuperSocket 收到一块二进制数据时被执行，接收到的数据在 readBuffer 中从 offset 开始， 长度为 length 的部分。
        /// 接收收到的数据流，对数据流进行过滤处理
        /// </summary>
        /// <param name="readBuffer">接收缓冲区, 接收到的数据存放在此数组里【缓冲区默认大小：100KB】</param>
        /// <param name="offset">接收到的数据在接收缓冲区的起始位置</param>
        /// <param name="length">本轮接收到的数据的长度</param>
        /// <param name="toBeCopied">表示当你想缓存接收到的数据时，是否需要为接收到的数据重新创建一个备份而不是直接使用接收缓冲区</param>
        /// <param name="rest">这是一个输出参数, 它应该被设置为当解析到一个为政的请求后，接收缓冲区还剩余多少数据未被解析</param>
        /// <returns>当你在接收缓冲区中找到一条完整的请求时，你必须返回一个你的请求类型的实例.当你在接收缓冲区中没有找到一个完整的请求时, 你需要返回 NULL.当你在接收缓冲区中找到一条完整的请求, 但接收到的数据并不仅仅包含一个请求时，设置剩余数据的长度到输出变量 "rest". SuperSocket 将会检查这个输出参数 "rest", 如果它大于 0, 此 Filter 方法 将会被再次执行, 参数 "offset" 和 "length" 会被调整为合适的值.</returns>
        public HansRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            //因接收的数据如果不匹配内容，则舍弃之，因此都为0
            rest = 0;
            byte[] startMarkBuffer = Encoder.GetBytes(StartMark);
            byte[] endMarkBuffer = Encoder.GetBytes(EndMark);
            if (length < startMarkBuffer.Length + endMarkBuffer.Length)//没有数据
                return null;
            byte[] data = new byte[length];
            Buffer.BlockCopy(readBuffer, offset, data, 0, length);
            string receiveStartMark = Encoder.GetString(data, 0, startMarkBuffer.Length);
            string receiveEndMark = Encoder.GetString(data, length - endMarkBuffer.Length, endMarkBuffer.Length);
            HansReceiveData receiveData = new HansReceiveData();
            receiveData.StartMark = StartMark;
            receiveData.Key = Guid.NewGuid().ToString("B");
            receiveData.BodyBuffer = new byte[length - startMarkBuffer.Length - endMarkBuffer.Length];
            Buffer.BlockCopy(data, startMarkBuffer.Length, receiveData.BodyBuffer, 0, length - startMarkBuffer.Length - endMarkBuffer.Length);
            receiveData.EndMark = EndMark;
            receiveData.BodyString = Encoder.GetString(receiveData.BodyBuffer);
            leftBufferSize = length - startMarkBuffer.Length - endMarkBuffer.Length;
            //如果开始标记 或者 结束标记 不匹配设定值，则返回null
            if (!receiveStartMark.Equals(StartMark) || !receiveEndMark.Equals(EndMark))
                return null;
            return new HansRequestInfo(receiveData.Key, receiveData);
        }

        /// <summary>
        /// 该接收过滤器已缓存数据的长度
        /// </summary>
        public int LeftBufferSize
        {
            get { return leftBufferSize; }
        }

        /// <summary>
        /// 当下一块数据收到时，用于处理数据的接收过滤器实例;
        /// </summary>
        public SuperSocket.SocketBase.Protocol.IReceiveFilter<HansRequestInfo> NextReceiveFilter
        {
            get { return this; }
        }

        /// <summary>
        /// 重设接收过滤器实例到初始状态
        /// </summary>
        public void Reset()
        {

        }

        public SuperSocket.SocketBase.Protocol.FilterState State
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// 第四步、接收过滤器工厂(ReceiveFilterFactory)
    /// 接收过滤器工厂(ReceiveFilterFactory)用于为每个会话创建接收过滤器. 定义一个过滤器工厂(ReceiveFilterFactory)类型, 你必须实现接口 IReceiveFilterFactory. 类型参数 "TRequestInfo" 是你要在整个程序中使用的请求类型
    /// </summary>
    public class HansReceiveFilterFactory : SuperSocket.SocketBase.Protocol.IReceiveFilterFactory<HansRequestInfo>
    {
        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding Encoder = Encoding.GetEncoding("gbk");
        /// <summary>
        /// 开始标记 如："!Start"
        /// </summary>
        public string StartMark = "!Start";
        /// <summary>
        /// 结束标记 如："$End"
        /// </summary>
        public string EndMark = "$End";
        public HansReceiveFilterFactory(Encoding encoder, string startMark, string endMark)
        {
            Encoder = encoder;
            StartMark = startMark;
            EndMark = endMark;
        }
        public SuperSocket.SocketBase.Protocol.IReceiveFilter<HansRequestInfo> CreateFilter(SuperSocket.SocketBase.IAppServer appServer, SuperSocket.SocketBase.IAppSession appSession, System.Net.IPEndPoint remoteEndPoint)
        {
            return new HansReceiveFilter(Encoder, StartMark, EndMark);
        }
    }

    /// <summary>
    /// 第五步、定义一个用于通信的会话对象【类似于客户端】
    /// </summary>
    public class HansAppSession : SuperSocket.SocketBase.AppSession<HansAppSession, HansRequestInfo>
    {
        public uint DeviceUDID;
        protected override void HandleException(Exception e)
        {

        }
    }

    /// <summary>
    /// 第六步、定义一个服务对象【类似于服务端】
    /// </summary>
    public class HansAppServer : SuperSocket.SocketBase.AppServer<HansAppSession, HansRequestInfo>
    {
        /// <summary>
        /// 字符编码
        /// </summary>
        public static Encoding Encoder = Encoding.GetEncoding("gbk");
        /// <summary>
        /// 开始标记 如："!Start"
        /// 如果不需要开始标记，请将 开始标记设置为string.Empty。
        /// </summary>
        public static string StartMark = "!Start";
        /// <summary>
        /// 结束标记 如："$End"
        /// 如果不需要结束标记，请将 结束标记设置为string.Empty。
        /// </summary>
        public static string EndMark = "$End";
        /// <summary>
        /// 按照默认的字符编码、开始标记、结束标记实例化服务对象
        /// </summary>
        public HansAppServer() : base(new HansReceiveFilterFactory(Encoder, StartMark, EndMark))
        {
        }
        /// <summary>
        /// 按照设置的字符编码、开始标记、结束标记实例化服务对象
        /// 【如果不需要开始标记，请将 开始标记设置为string.Empty。如果不需要结束标记，请将 结束标记设置为string.Empty。】
        /// </summary>
        /// <param name="encoder"></param>
        /// <param name="startMark"></param>
        /// <param name="endMark"></param>
        public HansAppServer(Encoding encoder, string startMark, string endMark)
 : base(new HansReceiveFilterFactory(encoder, startMark, endMark))
        {
            HansAppServer.Encoder = encoder;
            HansAppServer.StartMark = startMark;
            HansAppServer.EndMark = endMark;
        }
    }

    /// <summary>
    /// 服务端代码辅助类
    /// </summary>
    public class CloudHostServer
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("CloudHostServer");
        static Encoding encoding = Encoding.GetEncoding("gbk");
        HansAppServer myServer = new HansAppServer(encoding, "{\"code\":", "}");
        public void Listen()
        {
            //Setup the appServer
            if (!myServer.Setup(2019)) //Setup with listening port
            {
                log.Debug("云主机 Failed to setup!");
                return;
            }
            //Try to start the appServer
            if (!myServer.Start())
            {
                log.Debug("云主机 Failed to start!");
                return;
            }
            myServer.NewSessionConnected += MyServer_NewSessionConnected;
            myServer.NewRequestReceived += MyServer_NewRequestReceived;
            myServer.SessionClosed += myServer_SessionClosed;
            Console.WriteLine($"云主机开启{DateTime.Now.ToString()}");
        }

        private void MyServer_NewRequestReceived(HansAppSession session, HansRequestInfo requestInfo)
        {
            string msg = requestInfo.Body.StartMark+ requestInfo.Body.BodyString+ requestInfo.Body.EndMark;//encoding.GetString(requestInfo.Body.BodyBuffer);
            if (msg.Contains("101,"))
            {
                //心跳
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                session.Send("{\"code\":1001,\"result\":0,\"timestamp\": "+ Convert.ToInt64(ts.TotalSeconds) + "}");
            }
            else if (msg.Contains("104,"))
            {
                //状态
                dynamic result = JsonConvert.DeserializeObject<dynamic>(msg);//反序列化
                //缓存设备状态
                using (RedisHashService service = new RedisHashService())
                {
                    service.SetEntryInHash("104", result.id.Value, result.st.on.Value.ToString());
                }
            }
            log.Debug(msg);
        }

        private void MyServer_NewSessionConnected(HansAppSession session)
        {
            //session.Send("Welcome to SuperSocket Telnet Server");
        }

        void myServer_SessionClosed(HansAppSession session, SuperSocket.SocketBase.CloseReason value)
        {
            throw new NotImplementedException();
        }
    }
}
