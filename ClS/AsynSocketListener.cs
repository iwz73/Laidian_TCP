using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace ClS
{

    public class AsynSocketListener
    {

        /// <summary>
        /// 检测点Socket监听器
        /// </summary>
       
            public Queue<string> logs = new Queue<string>(10000);
            //创建一个Thread类  
            private Thread thread1;
            //创建一个UdpClient对象，来接收消息  
            private Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
ProtocolType.Udp);

            public void WriteLogs()
            {
                try
                {
                    tools.CreatText();




                    tools.WriteText(logs);

                }
                catch
                {
                }
                finally
                {
                    //logs.Clear();
                }
            }

            /// <summary>
            /// 开启Socket监听
            /// </summary>
            public void StartListening()
            {
              
            
                //设置重传次数   

                while (true)
                {
                    try
                    {
                        if (DateTime.Now.Hour == 12 && DateTime.Now.Minute ==30)
                        {
                            tools.DelLog();
                            tools.DelFile();//删除10天前的日志
                        }
                        if (logs.Count > 0)
                        {
                            try
                            {
                                WriteLogs();
                            }
                            catch
                            { }
                           

                  
                        }
                        Thread.Sleep(3000);

                    }
                    catch
                    {

                    }
                }
            }


        
    }
}
