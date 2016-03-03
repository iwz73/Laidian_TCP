using com.imlaidian.protobuf.model;
using Google.ProtocolBuffers;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClS
{

    public class TcpProto3
    {
        public AsyncTcpSession client;
        public ManualResetEvent StartEvent = new ManualResetEvent(false);
        public int count = 0;
        public DataEventArgs buffer = new DataEventArgs();
        public byte[] protobuffer = new byte[8192];
        public Thread PgoogleProtoLink;
        public bool bLoginFlag = false;//登陆是否成功
        private SvCommandExecutor SvCommandChainHeader = new EmptyCommandExecutor();
        //private SvEventProcessor EventProcessorChainHeader = new EmptyEventProcessor();
        //public Queue<LaidianStatusModel> actionMsg = new Queue<LaidianStatusModel>();
        public Queue<LaidianCommandModel> taskMsg = new Queue<LaidianCommandModel>();
        //public Queue<CDBStatusModel> status = new Queue<CDBStatusModel>();
        /**
        * 这里 C# 的实现和Protobuf 官方给的Java实现是一样的
        */
        public void sendMessage(AsyncTcpSession client, LaidianCommandModel request)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                CodedOutputStream os = CodedOutputStream.CreateInstance(stream);
                //一定要去看它的代码实现，
                os.WriteMessageNoTag(request);
                /**
                * WriteMessageNoTag 等价于 WriteVarint32, WriteByte(byte[])
                * 也就是：变长消息头 + 消息体
                */
                os.Flush();

                byte[] data = stream.ToArray();
                client.Send(new ArraySegment<byte>(data));
            }
        }
        /// <summary>
        /// 注册处理函数
        /// </summary>
        /// <param ></param>
        public void RegisteredModelHandler(SvCommandExecutor handler)
        {
            //加入到处理链中
            SvCommandChainHeader.AppendExecutor(handler);
        }
        /**
       * 协议解析，把这里搞明白了，就没白看
       */
        public void OnDataReceive(Object sender, DataEventArgs e)
        {
            //DataEventArgs 里面有 byte[] Data是从协议层接收上来的字节数组，需要程序端进行缓存
            Console.WriteLine("buff length: {0}, offset: {1}", e.Length, e.Offset);
            if (e.Length <= 0)
            {
                return;
            }

            //把收取上来的自己全部缓存到本地 buffer 中
            Array.Copy(e.Data, 0, buffer.Data, buffer.Length, e.Length);
            buffer.Length += e.Length;

            CodedInputStream stream = CodedInputStream.CreateInstance(buffer.Data);
            while (!stream.IsAtEnd)
            {
                //标记读取的Position, 在长度不够时进行数组拷贝，到下一次在进行解析
                int markReadIndex = (int)stream.Position;

                //Protobuf 变长头, 也就是消息长度
                int varint32 = (int)stream.ReadRawVarint32();
                if (varint32 <= (buffer.Length - (int)stream.Position))
                {
                    try
                    {
                        byte[] body = stream.ReadRawBytes(varint32);

                        LaidianCommandModel response = LaidianCommandModel.ParseFrom(body);
                        taskMsg.Enqueue(response);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
                else
                {
                    /**
                    * 本次数据不够长度,缓存进行下一次解析
                    */
                    byte[] dest = new byte[8192];
                    int remainSize = buffer.Length - markReadIndex;
                    Array.Copy(buffer.Data, markReadIndex, dest, 0, remainSize);

                    /**
                     * 缓存未处理完的字节 
                     */
                    buffer.Data = dest;
                    buffer.Offset = 0;
                    buffer.Length = remainSize;

                    break;
                }
            }
        }

        /// <summary>
        /// 定义一个client  每间隔10秒发送一次心跳，然后10分钟发送一次状态消息，借还购线等操作日志则马上传输
        /// </summary>
        public void protobuf_checklink()
        {

            while (true)
            {
                string str = "";
                try
                {
                    if (bLoginFlag)//首先检测是否在线
                    {
                        LaidianCommandModel.Builder laidianCommand = LaidianCommandModel.CreateBuilder();
                        laidianCommand.SetMessageType(MessageType.HEARTBEAT);
                        sendMessage(client, laidianCommand.Build());
                        Thread.Sleep(500);
                    }
                    else
                    {
                        //连接服务器
                        client.Connect();

                    }
                }
                catch (IOException ex)//远程连接已经关闭
                {

                }
                catch (Exception sd)
                {
                    str = sd.Message;
                    Console.WriteLine(str);

                }
                Thread.Sleep(3000);
            }
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            bLoginFlag = false;
            // throw new NotImplementedException();
        }
        /// <summary>
        /// 如果连接上去，则发送登陆指令。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_Connected(object sender, EventArgs e)
        {
            LaidianCommandModel.Builder loginbuilder = LaidianCommandModel.CreateBuilder();
            loginbuilder.SetMessageType(MessageType.LOGIN_REQ);
            LaidianCommandModel logincmd = loginbuilder.Build();
            sendMessage(client, logincmd);
        }

        public void StartServices()
        {
            #region 与监控后台通信
            //不在线则重连
            buffer.Data = new byte[8192]; //8 KB
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001);
            client = new AsyncTcpSession(endPoint);
            client.Connected += Client_Connected; ;
            client.Closed += Client_Closed;
            client.DataReceived += OnDataReceive; //重点解析在这里

            PgoogleProtoLink = new Thread(new ThreadStart(protobuf_checklink));
            PgoogleProtoLink.Start();
            #endregion
        }
    }
}
