 using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Xml.Linq;
using ClS;

namespace Com
{
    //串口接收事件,用于通知界面
    public class newEventStr : EventArgs
    {
        public int OrderId;
        public int EventType;//租借0，归还1，销售2
        public byte[] tys;
    }
    //

    /// <summary>
    /// 本类主要是通过串口与控制单片机通信包括出货，传感器数据 ，门禁通信
    /// </summary>
    public class MyCOMPort : CommBase
    {
        // Fields
        public event EventHandler DongEvent;//事件发布
        public int iFailedCounter = 0;
        public int iTimeout =100;//发送命令五秒钟没有回复就算超时
        public bool IsWorkFlag=false;
        public CommBase.CommBaseSettings settings = new CommBase.CommBaseSettings();
        private List<byte>strRecive = new List<byte>();
        private List<byte> ExpectReceive = new List<byte>();
        public bool returnFlag = false;
        public ManualResetEvent TransFlag = new ManualResetEvent(false);
        public bool recflag=false;//接收指令标志位
        public bool flag = false;//回复是否正确
        public string strBaud;
        public string tempcom;
        //Com.ProtocolVersion2011 protocol = new ProtocolVersion2011();
        
        // Methods
        public MyCOMPort(string strCom,string baud)
        {
            try
            {

                tempcom = strCom;

               
                string s = "5";// ConfigurationSettings.AppSettings["SendSMS_Timeout"];
                //this.iTimeout = int.Parse(s) * 0x3e8; 
                this.settings.SetStandard(strCom, int.Parse(baud), CommBase.Handshake.none);
                //if (strCom == "")
                //{
                //    strCom = ConfigurationSettings.AppSettings["ComPortNo"];// MyData.Get_SMS_Com_Port();
                //}
            }
            catch
            {
                
            }
  
           
           
        }

        protected override CommBase.CommBaseSettings CommSettings()
        {
            return this.settings;
        }

        public  CommBase.QueueStatus GetQueueStatus()
        {
            return base.GetQueueStatus();
        }

        public void Init_Modem_To_DefaultStatus()
        {
            //string s = "ATZ\r AT+CNMI=1,2,0,1,0\r AT+CSMP=33,167,0,0\r AT&W\r ";
            //byte[] bytes = Encoding.ASCII.GetBytes(s);
            //base.Send(bytes);
            //base.Flush();
        }

        public bool IsCongested()
        {
            return base.IsCongested();
        }
        /// <summary>
        /// 作为一个委托方法被另外一个线程调用。以达到异步接收的目的
        /// 接收的过程中要做2件事
        /// 1、监测接收的数据是否完整，如果不完整就继续等待。
        /// 2、如果是完整的话 是否对应了命令预期的数据,如果不对应的话 ，重发
        /// 3、重发次数是否查过设定？超过了则丢弃。
        /// ManualResetEvent的Set()和Reset()方法即上面的功能，将状态分别设成绿灯和红灯。
        ///红灯的状态下 WaitOne的线程将停止
        /// </summary>
        /// <param name="c"></param>
        protected override void OnRxChar(byte c)
        {
            try
            {
                if (c == 0x25)
                    recflag = true;
                if (recflag)
                {
                    this.strRecive.Add(c);
                    //if (this.strRecive.Count > 60)
                    //{
                    //    strRecive.Clear();
                    //    recflag = false;
                    //}
                    //if (c == 0x23)
                    //{

                    if (strRecive.Count > 4)
                    {
                        int startIndex = (int)strRecive[2];
                        if ((strRecive.Count == startIndex + 3) && strRecive[startIndex + 2] == 0x23)
                        {

                            //if (tools.Crc(strRecive.ToArray(), 3, startIndex) == strRecive[startIndex + 1])//校验码
                            //{ }
                            newEventStr newsms = new newEventStr();
                            newsms.EventType = 0x00;
                            newsms.tys = strRecive.ToArray();
                            TransFlag.Set();
                            //TransFlag.WaitOne(1000, false);


                            strRecive.Clear();
                            recflag = false;
                            if (this.DongEvent != null)
                            {
                                DongEvent(this, newsms);

                            }

                            //TransFlag.Set();
                            ////TransFlag.WaitOne(1000, false);


                            //strRecive.Clear();
                            //recflag = false;
                            //}
                            //else
                            //{
                            //    strRecive.Clear();

                            //}



                        }
                        else if (strRecive.Count > 100)
                        {

                            strRecive.Clear();
                            recflag = false;

                        }

                        //  string tempS = Encoding.ASCII.GetString(strRecive.ToArray());






                        this.flag = true;//回复不正确




                        //}
                        //else
                        //{
                        //    this.strRecive.Clear();
                        //    recflag = false;
                        //}
                    }
                }

            }
            catch (Exception exception)
            {

            }





        }
       
        public bool SendString(string strSend)
        {
            
            byte[] bytes = Encoding.ASCII.GetBytes(strSend);
            base.Send(bytes);
            base.Flush();
            this.TransFlag.Set();//？让线程运行到WaitOne处暂停吗
            this.strRecive.Clear();
            //非常关键，等待超时设置可以防止不明故障的死锁
            if (this.TransFlag.WaitOne(this.iTimeout, false))//接收端发送了ManualResetEvent或者接收超时的时候才能使用 当ManualResetEvent处于True,这里就不用等待
            {
                iFailedCounter++;//每次发送数据都增加1
                //SendCommand command = MyData.DeleteCommand(SendCommandStatus.COMFinish);
                //MyData.WriteLogFile("发送成功:" + command.strAddress + "  " + MyData.myProtocol.Get_Send_Command_Info(command.sendCommand.Trim()));
                //this.iFailedCounter = 0;
                
                return this.returnFlag;
            }
            //SendCommand command2 = MyData.DeleteCommand(SendCommandStatus.COMDoning);
            //MyData.WriteLogFile("发送暂存 :" + command2.strAddress + "  " + MyData.myProtocol.Get_Send_Command_Info(command2.sendCommand.Trim()));
           // this.iFailedCounter++;
            return false;
        }

        public bool SendByte(byte[] bytes)
        {
            try
            {
                IsWorkFlag = true;


                base.Send(bytes);
                base.Flush();
                this.TransFlag.Reset();//？让线程运行到WaitOne处暂停吗
                ////this.strRecive.Clear() ;
                ////非常关键，等待超时设置可以防止不明故障的死锁
                if (!this.TransFlag.WaitOne(this.iTimeout, false))//接收端发送了ManualResetEvent或者接收超时的时候才能使用 当ManualResetEvent处于True,这里就不用等待
                {
                    //    // iFailedCounter++;//每次发送数据都增加1
                    //    //SendCommand command = MyData.DeleteCommand(SendCommandStatus.COMFinish);
                    //    //MyData.WriteLogFile("发送成功:" + command.strAddress + "  " + MyData.myProtocol.Get_Send_Command_Info(command.sendCommand.Trim()));
                    this.iFailedCounter = 0;
                    //    //超时
                    IsWorkFlag = false;
                    return false;
                }
                IsWorkFlag = false;
                //SendCommand command2 = MyData.DeleteCommand(SendCommandStatus.COMDoning);
                //MyData.WriteLogFile("发送暂存 :" + command2.strAddress + "  " + MyData.myProtocol.Get_Send_Command_Info(command2.sendCommand.Trim()));
                //this.iFailedCounter++;
                return true;
            }
            catch
            {
                this.settings.SetStandard(tempcom, 19200, CommBase.Handshake.none);
                this.Open();
                return false;
                
            }
        }
    }
}
