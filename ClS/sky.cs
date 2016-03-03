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
        /// �ַ����봦��.
        /// </summary>
        private static readonly Encoding ASCII;


        /// <summary>
        /// ���� ����/���յĶ˿�.
        /// </summary>
        private const int PORT = 8088;


        private const String SEND_MESSAGE = "Hello Socket Server!";


        public static Byte[] SendMessage(Byte[] sendBytes, string IP, int port)
        {
            // �������ڷ��͵� �ֽڻ���.
            // Byte[] sendBytes = ASCII.GetBytes(SEND_MESSAGE);

            // �������ڽ��յ� �ֽڻ���.
            Byte[] recvBytes = new Byte[256];

            // IP��ַ.
            IPAddress localAddr = IPAddress.Parse(IP);

            // �����.
            IPEndPoint ephost = new IPEndPoint(localAddr, port);


            // ��һ��������AddressFamily = ָ�� Socket ���ʵ������ʹ�õ�Ѱַ������
            //     Unspecified δָ����ַ�塣
            //     InterNetwork IP �汾 4 �ĵ�ַ��
            //     InterNetworkV6 IP �汾 6 �ĵ�ַ��
            //
            // �ڶ���������SocketType = ָ�� Socket ���ʵ����ʾ���׽������͡�
            //     Stream һ���׽������ͣ�֧�ֿɿ���˫�򡢻������ӵ��ֽ����������ظ����ݣ�Ҳ�������߽硣
            //            �����͵� Socket �뵥���Է�����ͨ�ţ�������ͨ�ſ�ʼ֮ǰ��Ҫ����Զ���������ӡ�
            //            ���׽�������ʹ�ô������Э�� (Tcp)��AddressFamily ������ InterNetwork��Ҳ������ InterNetworkV6��
            //
            // ������������ProtocolType = ָ�� Socket ��֧�ֵ�Э�顣
            //     Tcp �������Э�� (TCP)�� 
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                s.SendTimeout = 5000;//�ܿس�ʱ
                try
                {



                    // ������������.
                    s.Connect(ephost);


                    //   Console.WriteLine("����������͵���:{0}", SEND_MESSAGE);

                    // ��������������.
                    s.Send(sendBytes, sendBytes.Length, SocketFlags.None);

                    // ���շ�������Ӧ��.
                    //Int32 bytes = s.Receive(recvBytes, recvBytes.Length, SocketFlags.None);


                    //StringBuilder buff = new StringBuilder();

                    //while (bytes > 0)
                    //{
                    //    // ��������ֽ����飬װ��Ϊ�ַ���.
                    //    String str = ASCII.GetString(recvBytes, 0, bytes);
                    //    // �����ַ�������
                    //    buff.Append(str);
                    //    // �ٴν��ܣ��������滹��û������.
                    //    bytes = s.Receive(recvBytes, recvBytes.Length, SocketFlags.None);
                    //}




                }
                catch (Exception ex)
                {
                    Console.WriteLine("����/����/���չ����У������˴���");
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
