using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.imlaidian.protobuf.model;
using System.Threading;
using Com;

namespace ClS
{
    public class LaidianBBL
    {

        public bool bProcess = true;
        public TcpProto3 protobuf;
        public SvCommandExecutor ServerTaskExecutorchainheader = new EmptyCommandExecutor();
        /// <summary>
        /// 串口指令执行
        /// </summary>
        public SvCommandExecutor SerialportCmdExecutorchainheader = new EmptyCommandExecutor();
        public Queue<BasisEventCommand> cmdpool = new Queue<BasisEventCommand>();
        public LDMachine terminal = new LDMachine();
      


        //public void InitSerialPort()
        //{
        //    terminal.cdport.DongEvent += Cdport_DongEvent;
        //    terminal.saleport.DongEvent += Saleport_DongEvent;
        //}
   

        //private void Saleport_DongEvent(object sender, EventArgs e)
        //{
        //    //throw new NotImplementedException();
        //    Com.newEventStr eventFromCom = (newEventStr)e;
        //    byte[] data = (byte[])eventFromCom.tys;
        //    BasisEventCommand serialportevent = new BasisEventCommand();
        //    serialportevent.Session = terminal;
        //    serialportevent.EventCommandID = "serial";
        //    serialportevent.Content = data;
        //    SerialportCmdExecutorchainheader.Execute(serialportevent);
        //}

        //private void Cdport_DongEvent(object sender, EventArgs e)
        //{
        //    Com.newEventStr eventFromCom = (newEventStr)e;
        //    byte[] data = (byte[])eventFromCom.tys;
        //    BasisEventCommand serialportevent = new BasisEventCommand();
        //    serialportevent.Session = terminal;
        //    serialportevent.EventCommandID = "serial";
        //    serialportevent.Content = data;
        //    SerialportCmdExecutorchainheader.Execute(serialportevent);
        //}

        public void IninChain()
        {
            HeartBeatCommandExecutor heartbeatexecutor = new HeartBeatCommandExecutor();
          
        }
        /// <summary>
        /// 烧写地址
        /// </summary>
        /// <param name="iaddr"></param>
        public void WriteParam(byte[] terminalparams)
        {
            terminal.udplog.logs.Enqueue(DateTime.Now.ToString() + "写开始写入参数没有响应");

            terminal.SaleEvent.Reset();//等待10秒
            Thread.Sleep(2000);
            byte[] cmdbyte = COM_Cmd.WriteParamCmd(terminalparams);
            string str = COM_Cmd.byteToString(cmdbyte);
            terminal.udplog.logs.Enqueue(DateTime.Now.ToString() + "写入参数" + str);

            if (!terminal.cdport.SendByte(cmdbyte))
            {
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.ParamF;
                //nui.huodao = strStation;
                nui.Msg = "烧写参数指令超时";


                terminal.udplog.logs.Enqueue(DateTime.Now.ToString() + "烧写参数指令超时");
                //事件通知界面更新
                terminal.fireEvent(nui);



            }
        }
        /// <summary>
        /// 云端命令处理
        /// </summary>
        public void ExecuteServerCmd()
        {
            while (bProcess)
            {
                try
                {
                    if (terminal.tcpchannel.taskMsg.Count > 0)
                    {
                        LaidianCommandModel model = (LaidianCommandModel)terminal.tcpchannel.taskMsg.Dequeue();
                        BasisEventCommand cmd = new BasisEventCommand();
                        cmd.Content = model;
                        cmd.EventCommandID = "tcp";
                        cmd.Session = terminal;
                        ServerTaskExecutorchainheader.Execute(cmd);
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
                catch
                { }
            }
        }
        public void RespServerCmd()
        {
            while (true)
            {
                if (terminal.SendNetEvents.Count > 0)
                {
                    ResponseWorkEvent 
                }

            }
        }



        
    }
}
