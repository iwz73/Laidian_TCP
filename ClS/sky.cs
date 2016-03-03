using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace ClS
{
    public class sky
    {
        /// <summary>
        /// 字符编码处理.
        /// </summary>
        private static readonly Encoding ASCII;


        /// <summary>
        /// 用于 发送/接收的端口.
        /// </summary>
        private const int PORT = 8088;


        private const String SEND_MESSAGE = "Hello Socket Server!";


        public static Byte[] SendMessage(Byte[] sendBytes, string IP, int port)
        {
            // 构造用于发送的 字节缓冲.
            // Byte[] sendBytes = ASCII.GetBytes(SEND_MESSAGE);

            // 构造用于接收的 字节缓冲.
            Byte[] recvBytes = new Byte[256];

            // IP地址.
            IPAddress localAddr = IPAddress.Parse(IP);

            // 接入点.
            IPEndPoint ephost = new IPEndPoint(localAddr, port);


            // 第一个参数：AddressFamily = 指定 Socket 类的实例可以使用的寻址方案。
            //     Unspecified 未指定地址族。
            //     InterNetwork IP 版本 4 的地址。
            //     InterNetworkV6 IP 版本 6 的地址。
            //
            // 第二个参数：SocketType = 指定 Socket 类的实例表示的套接字类型。
            //     Stream 一个套接字类型，支持可靠、双向、基于连接的字节流，而不重复数据，也不保留边界。
            //            此类型的 Socket 与单个对方主机通信，并且在通信开始之前需要建立远程主机连接。
            //            此套接字类型使用传输控制协议 (Tcp)，AddressFamily 可以是 InterNetwork，也可以是 InterNetworkV6。
            //
            // 第三个参数：ProtocolType = 指定 Socket 类支持的协议。
            //     Tcp 传输控制协议 (TCP)。 
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                s.SendTimeout = 5000;//管控超时
                try
                {



                    // 尝试连接主机.
                    s.Connect(ephost);


                    //   Console.WriteLine("向服务器发送到了:{0}", SEND_MESSAGE);

                    // 向主机发送数据.
                    s.Send(sendBytes, sendBytes.Length, SocketFlags.None);

                    // 接收服务器的应答.
                    //Int32 bytes = s.Receive(recvBytes, recvBytes.Length, SocketFlags.None);


                    //StringBuilder buff = new StringBuilder();

                    //while (bytes > 0)
                    //{
                    //    // 将缓冲的字节数组，装换为字符串.
                    //    String str = ASCII.GetString(recvBytes, 0, bytes);
                    //    // 加入字符串缓存
                    //    buff.Append(str);
                    //    // 再次接受，看看后面还有没有数据.
                    //    bytes = s.Receive(recvBytes, recvBytes.Length, SocketFlags.None);
                    //}




                }
                catch (Exception ex)
                {
                    Console.WriteLine("连接/发送/接收过程中，发生了错误！");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                finally
                {
                    s.Close();

                }
            }
            catch
            {
            }
            return recvBytes;
        }
    }
}
