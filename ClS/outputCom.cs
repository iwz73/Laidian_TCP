 using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Xml.Linq;
using ClS;

namespace Com
{
    //���ڽ����¼�,����֪ͨ����
    public class newEventStr : EventArgs
    {
        public int OrderId;
        public int EventType;//���0���黹1������2
        public byte[] tys;
    }
    //

    /// <summary>
    /// ������Ҫ��ͨ����������Ƶ�Ƭ��ͨ�Ű������������������� ���Ž�ͨ��
    /// </summary>
    public class MyCOMPort : CommBase
    {
        // Fields
        public event EventHandler DongEvent;//�¼�����
        public int iFailedCounter = 0;
        public int iTimeout =100;//��������������û�лظ����㳬ʱ
        public bool IsWorkFlag=false;
        public CommBase.CommBaseSettings settings = new CommBase.CommBaseSettings();
        private List<byte>strRecive = new List<byte>();
        private List<byte> ExpectReceive = new List<byte>();
        public bool returnFlag = false;
        public ManualResetEvent TransFlag = new ManualResetEvent(false);
        public bool recflag=false;//����ָ���־λ
        public bool flag = false;//�ظ��Ƿ���ȷ
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
        /// ��Ϊһ��ί�з���������һ���̵߳��á��Դﵽ�첽���յ�Ŀ��
        /// ���յĹ�����Ҫ��2����
        /// 1�������յ������Ƿ�����������������ͼ����ȴ���
        /// 2������������Ļ� �Ƿ��Ӧ������Ԥ�ڵ�����,�������Ӧ�Ļ� ���ط�
        /// 3���ط������Ƿ����趨��������������
        /// ManualResetEvent��Set()��Reset()����������Ĺ��ܣ���״̬�ֱ�����̵ƺͺ�ơ�
        ///��Ƶ�״̬�� WaitOne���߳̽�ֹͣ
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

                            //if (tools.Crc(strRecive.ToArray(), 3, startIndex) == strRecive[startIndex + 1])//У����
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






                        this.flag = true;//�ظ�����ȷ




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
            this.TransFlag.Set();//�����߳����е�WaitOne����ͣ��
            this.strRecive.Clear();
            //�ǳ��ؼ����ȴ���ʱ���ÿ��Է�ֹ�������ϵ�����
            if (this.TransFlag.WaitOne(this.iTimeout, false))//���ն˷�����ManualResetEvent���߽��ճ�ʱ��ʱ�����ʹ�� ��ManualResetEvent����True,����Ͳ��õȴ�
            {
                iFailedCounter++;//ÿ�η������ݶ�����1
                //SendCommand command = MyData.DeleteCommand(SendCommandStatus.COMFinish);
                //MyData.WriteLogFile("���ͳɹ�:" + command.strAddress + "  " + MyData.myProtocol.Get_Send_Command_Info(command.sendCommand.Trim()));
                //this.iFailedCounter = 0;
                
                return this.returnFlag;
            }
            //SendCommand command2 = MyData.DeleteCommand(SendCommandStatus.COMDoning);
            //MyData.WriteLogFile("�����ݴ� :" + command2.strAddress + "  " + MyData.myProtocol.Get_Send_Command_Info(command2.sendCommand.Trim()));
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
                this.TransFlag.Reset();//�����߳����е�WaitOne����ͣ��
                ////this.strRecive.Clear() ;
                ////�ǳ��ؼ����ȴ���ʱ���ÿ��Է�ֹ�������ϵ�����
                if (!this.TransFlag.WaitOne(this.iTimeout, false))//���ն˷�����ManualResetEvent���߽��ճ�ʱ��ʱ�����ʹ�� ��ManualResetEvent����True,����Ͳ��õȴ�
                {
                    //    // iFailedCounter++;//ÿ�η������ݶ�����1
                    //    //SendCommand command = MyData.DeleteCommand(SendCommandStatus.COMFinish);
                    //    //MyData.WriteLogFile("���ͳɹ�:" + command.strAddress + "  " + MyData.myProtocol.Get_Send_Command_Info(command.sendCommand.Trim()));
                    this.iFailedCounter = 0;
                    //    //��ʱ
                    IsWorkFlag = false;
                    return false;
                }
                IsWorkFlag = false;
                //SendCommand command2 = MyData.DeleteCommand(SendCommandStatus.COMDoning);
                //MyData.WriteLogFile("�����ݴ� :" + command2.strAddress + "  " + MyData.myProtocol.Get_Send_Command_Info(command2.sendCommand.Trim()));
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
