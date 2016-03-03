using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClS
{
    public class COM_Cmd
    {
        /// <summary>
        /// byte[]转换成16进制字符串
        /// </summary>
        /// <param name="cmdbyte"></param>
        /// <returns></returns>
        public static string byteToString(byte[] cmdbyte)
        {
            string str = "";
            foreach (byte bt in cmdbyte)
            {
                str += bt.ToString("X2") + " ";
            }
            return str;
        }
        /// <summary>
        /// 查询仓道状态
        /// </summary>
        /// <param name="cd"></param>
        /// <returns></returns>
        public static Byte[] QueryCDStatusCmd(int cd)
        {
            byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)cd;
            cmdbyte[2] = (byte)(4);

            cmdbyte[3] = (byte)'S';//字母T

            cmdbyte[4] = (byte)2;
            // output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
            cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


            cmdbyte[6] = 0x23;
            return cmdbyte;
        }
        /// <summary>
        /// 重启路由指令C
        /// </summary>
        /// <returns></returns>
        public static byte[] ResetRouteCmd()
        {
            byte[] salecmd = new byte[7];
            salecmd[0] = 0x25;
            salecmd[1] = 200;
            salecmd[2] = 4;
            salecmd[3] = (byte)'M';
            salecmd[4] = 4;
            salecmd[5] = tools.Crc(salecmd, 0, 5);
            salecmd[6] = 0x23;
            return salecmd;
        }
        /// <summary>
        /// 烧写仓道地址
        /// </summary>
        /// <param name="iaddr"></param>
        /// <returns></returns>
        public static byte[] WriteAddrCom(int iaddr)
        {
            byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)0xC8;
            cmdbyte[2] = (byte)(4);

            cmdbyte[3] = (byte)'A';//字母Q

            cmdbyte[4] = (byte)iaddr;
            // output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
            cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


            cmdbyte[6] = 0x23;
            return cmdbyte;
        }
        /// <summary>
        /// 打开回收仓指令
        /// </summary>
        /// <returns></returns>
        public static byte[] OpenGraDoorCmd()
        {
            byte[] salecmd = new byte[7];
            salecmd[0] = 0x25;
            salecmd[1] = 200;
            salecmd[2] = 4;
            salecmd[3] = (byte)'K';
            salecmd[4] = 3;
            salecmd[5] = tools.Crc(salecmd, 0, 5);
            salecmd[6] = 0x23;
            return salecmd;
        }
        /// <summary>
        /// 写参数到控制板P
        /// </summary>
        /// <param name="terminalparams"></param>
        /// <returns></returns>
        public static byte[] WriteParamCmd(byte[] terminalparams)
        {
            byte[] cmdbyte = new byte[255];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)0xC8;
            cmdbyte[2] = (byte)(252);

            cmdbyte[3] = (byte)'P';//字母Q

            cmdbyte[4] = (byte)0;
            for (int i = 0; i < 248; i++)
            {
                cmdbyte[i + 5] = terminalparams[i];
            }
            // output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
            cmdbyte[253] = tools.Crc(cmdbyte, 0, 253);//校验


            cmdbyte[254] = 0x23;
            return cmdbyte;
        }

        /// <summary>
        /// 读取参数指令L
        /// </summary>
        /// <returns></returns>
        public static byte[] ReadparamCmd()
        {
            byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)0xC8;
            cmdbyte[2] = (byte)(4);

            cmdbyte[3] = (byte)'L';//字母Q

            cmdbyte[4] = (byte)0;
            // output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
            cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


            cmdbyte[6] = 0x23;
            return cmdbyte;
        }
        
        /// <summary>
        /// 关机指令W
        /// </summary>
        /// <returns></returns>
        public static byte[] ClosePCCmd()
        {
            byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)0xC8;
            cmdbyte[2] = (byte)(4);

            cmdbyte[3] = (byte)'W';//字母Q

            cmdbyte[4] = (byte)1;
            // output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
            cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


            cmdbyte[6] = 0x23;
            return cmdbyte;
        }
      
        /// <summary>
        /// 重启仓道指令
        /// </summary>
        /// <param name="cd"></param>
        /// <returns></returns>
        public static byte[] ResetCDCmd(int cd)
        {
            byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)cd;
            cmdbyte[2] = (byte)(4);

            cmdbyte[3] = (byte)'T';//字母Q

            cmdbyte[4] = (byte)1;
            // output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
            cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


            cmdbyte[6] = 0x23;
            return cmdbyte;
        }
      
        /// <summary>
        /// 接充电宝指令D
        /// </summary>
        /// <param name="cd"></param>
        /// <returns></returns>
        public static byte[] GCDBCmd(int cd)
        {
            byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)cd;
            cmdbyte[2] = (byte)(4);

            cmdbyte[3] = (byte)'D';//字母Q

            cmdbyte[4] = (byte)2;
     
            cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


            cmdbyte[6] = 0x23;
            return cmdbyte;
        }
        /// <summary>
        /// 归还充电宝指令D
        /// </summary>
        /// <param name="cd"></param>
        /// <returns></returns>
        public static byte[] ReturnCDBCmd(int cd)
        {
            byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)cd;
            cmdbyte[2] = (byte)(4);

            cmdbyte[3] = (byte)'H';//字母Q

            cmdbyte[4] = (byte)3;
            // output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
            cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


            cmdbyte[6] = 0x23;
            return cmdbyte;
        }
      
        /// <summary>
        /// 查询售线模块指令C
        /// </summary>
        /// <returns></returns>
        public static byte[] QuerySaleCmd()
        {
            byte[] salecmd = new byte[7];
            salecmd[0] = 0x25;
            salecmd[1] = 200;
            salecmd[2] = 4;
            salecmd[3] = (byte)'C';
            salecmd[4] = 0x01;
            salecmd[5] = tools.Crc(salecmd, 0, 5);
            salecmd[6] = 0x23;
            return salecmd;
        }
     
        /// <summary>
        /// 查询仓道Q
        /// </summary>
        /// <param name="cd"></param>
        /// <returns></returns>
        public static byte[] QueryCDCmd(int cd)
        {
            byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
            cmdbyte[0] = 0x25;
            cmdbyte[1] = (byte)cd;
            cmdbyte[2] = (byte)(4);

            cmdbyte[3] = 0x51;//字母Q

            cmdbyte[4] = (byte)3;
            // output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
            cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


            cmdbyte[6] = 0x23;
            return cmdbyte;
        }

        /// <summary>
        /// 售线指令B
        /// </summary>
        /// <param name="type"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public static byte[] SaleLineCmd(int type, int taskId)
        {
            byte[] salecmd = new byte[11];
            salecmd[0] = 0x25;
            salecmd[1] = 0xC8;
            salecmd[2] = 8;
            salecmd[3] = (byte)'B';
            salecmd[4] = (byte)type;
      
            /*将售线任务的ID下发给下位机*/

            string strtaskid = taskId.ToString("X8");

            salecmd[5] = Convert.ToByte(strtaskid.Substring(0, 2), 16);
            salecmd[6] = Convert.ToByte(strtaskid.Substring(2, 2), 16);
            salecmd[7] = Convert.ToByte(strtaskid.Substring(4, 2), 16);
            salecmd[8] = Convert.ToByte(strtaskid.Substring(6, 2), 16);

            salecmd[9] = tools.Crc(salecmd, 0, 9);
            salecmd[10] = 0x23;
            return salecmd;
        }
    }
}
