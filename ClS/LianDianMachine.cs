using Com;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using com.imlaidian.protobuf.model;
namespace ClS
{
    public class LDMachine
    {
      
        /********属性********/
        public string MonitorStatus = "";//主控机的状态
        public string MonitorVersion = "";//主控机版本
        public string MonitorGraStatus = "";//回收仓状态

        public string ShopPay = "";//支付的说明
        public string ShopPayImg = "";//支付的图片
        public bool HasGetShopInfoFlag = true;//是否已经获取信息
        public int HasGetShopInfoHour = 0;//获取店铺信息时间
        public string ShopLogo = "";

        private string terminalno;
        /// <summary>
        /// 机器身份编号
        /// </summary>

        public string TerminalNO
        {
            get { return terminalno; }
        }
        /// <summary>
        /// 2015-12-28
        /// 需要重启的仓道编号
        /// </summary>
        public Queue<LoseCD> NeedSendReBoot = new Queue<LoseCD>();
        /// <summary>
        /// 线数量
        /// </summary>
        public int[] Lines = new int[4];
        /// <summary>
        /// 仓道数量，默认30
        /// </summary>
        private int CDCnt = 30;
        /*****对象*****/
        /// <summary>
        /// 仓道
        /// </summary>
        public Dictionary<string, CUWUGUI> Cuwuguis;
        /// <summary>
        /// 连接仓道串口，默认com1
        /// </summary>
        public Com.MyCOMPort cdport;
        /// <summary>
        /// 连接售卖模块串口,默认com2
        /// </summary>
        public Com.MyCOMPort saleport;
        /// <summary>
        /// 与服务器端通信模块
        /// </summary>
        public TcpProto3 tcpchannel;

        /// <summary>
        /// 命令执行管道头，所有命令经管道流转过滤处理
        /// </summary>
        public SvCommandExecutor CommandExecutorChainHeader = new EmptyCommandExecutor();

        /// <summary>
        /// 归还动作
        /// </summary>
        public HuanAction onehuanaction = new HuanAction();

        /// <summary>
        /// 出货标志，如果是0则是用户借充电宝，需上传服务器，如果是1则是管理员拿出充电宝，不需要上传服务器。
        /// </summary>
        public int cflag = 0;//出货标志，如果是0则是用户借充电宝，需上传服务器，如果是1则是管理员拿出充电宝，不需要上传服务器。
        /// <summary>
        /// 与云端通信的最后时间
        /// </summary>
        public DateTime ComLinkLastTime;
        /// <summary>
        /// 0为正常模式  1为测试模式
        /// </summary>
        public int Test_Mode = 0;
        /// <summary>
        /// 借出结果
        /// </summary>
        public int jieResult = 0;
        /// <summary>
        /// 售线串口的线程控制信号
        /// </summary>
        public ManualResetEvent SaleEvent = new ManualResetEvent(false);
        /// <summary>
        /// 仓道串口的线程控制信号
        /// </summary>
        public ManualResetEvent StartEvent = new ManualResetEvent(false);

        /// <summary>
        /// 循环归还开关 ，如果归还时出现某个仓位没有回应，则跑到另一个舱口
        /// </summary>
        public ManualResetEvent HuanEvent = new ManualResetEvent(true);
        /// <summary>
        /// 循环借出开关 ，如果归还时出现某个仓位没有回应，则跑到另一个舱口
        /// </summary>
        public ManualResetEvent JEvent = new ManualResetEvent(true);

        /// <summary>
        /// 循环补货需要的信号灯
        /// </summary>
        public ManualResetEvent BCEvent = new ManualResetEvent(false);
        public bool stop = false;
        /// <summary>
        /// 日志
        /// </summary>
        public AsynSocketListener udplog = new AsynSocketListener();


        /// <summary>
        /// 回收仓已存数量
        /// </summary>
        public int Huishou_Num = 0;

        /// <summary>
        /// 回收仓允许的最大存放数量
        /// </summary>
        private int HuiShouMaxNum = 30;

        public string huishouTaskId = "";

        public byte iSetAddr = 0;//待写入地址
        /// <summary>
        /// 发送云端事件的队列
        /// </summary>
        public Queue<ReponseWorkEvent> SendNetEvents = new Queue<ReponseWorkEvent>();

        /// <summary>
        /// 发送售线机器的命令队列
        /// </summary>
        public Queue<ReponseWorkEvent> SaleNetEvents = new Queue<ReponseWorkEvent>();
        /// <summary>
        /// 借出订单状态
        /// </summary>
        public JSLdbOrder oneorder;

        /// <summary>
        /// 借出任务
        /// </summary>
        public newGprsEventStr ServerWork;
        /// <summary>
        /// 更新界面的事件发布者。
        /// </summary>
        public event EventHandler UpdateUIhandle;

        /// <summary>
        /// 初始化化界面的事件发布者。
        /// </summary>
        public event EventHandler UpdateInitUIhandle;


        public SvCommandExecutor ServerTaskExecutorchainheader = new EmptyCommandExecutor();
        /// <summary>
        /// 仓道模块串口指令执行链
        /// </summary>
        public SvCommandExecutor CdSerialportCmdExecutorchainheader = new EmptyCommandExecutor();
        /// <summary>
        /// 售线模块串口指令执行链
        /// </summary>
        public SvCommandExecutor SaleLineSerialportCmdExecutorchainheader = new EmptyCommandExecutor();
        public Queue<BasisEventCommand> cmdpool = new Queue<BasisEventCommand>();
        /// <summary>
        /// 机器初始化
        /// 1、初始化加密狗
        /// 2、初始化仓道
        /// 3、初始化串口
        /// 4、初始化网络通信模块
        /// 5、初始化处理链表
        /// </summary>
        public int InitMachine()
        {
            /*******************初始化加密狗********************************************/
            SetWindow.ShowWindow(SetWindow.FindWindow("Shell_TrayWnd", null), 0);
            SetWindow.ShowWindow(SetWindow.FindWindow("Button", null), 0);
           
            Operator.HasLock();//判断是否有加密狗
            terminalno = Operator.GetTerminalNO();//从加密狗获取编号
            /****************************初始化线数量**********************************/
            try
            {
                //加载基本数据

                XmlOperatorLine lines = new XmlOperatorLine();
                List<XmlOperatorLine> xmls = lines.GetAll();
                //int line1 = 0, line2 = 0, line3 = 0, line4 = 0;
                foreach (XmlOperatorLine ls in xmls)
                {
                    if (ls.ID == "1")
                    {
                        Lines[0] = ls.Num;
                    }
                    if (ls.ID == "2")
                    {
                        Lines[1] = ls.Num;
                    }
                    if (ls.ID == "3")
                    {
                        Lines[2] = ls.Num;
                    }
                    if (ls.ID == "4")
                    {
                        Lines[3] = ls.Num;
                    }
                }
                

            }
            catch
            {

            }
            /****************************初始化仓口***********************************/
            Cuwuguis = new Dictionary<string, CUWUGUI>();
            for (int i = 1; i <= CDCnt; i++)
            {
                CUWUGUI cwg = new CUWUGUI();
                cwg.CDB = null;
                cwg.CWGStatus = CUWUGUISTATUS.None;
                cwg.CWGCommStatus = CWGCOMMSTATUS.Error;
                cwg.CWGID = i.ToString("D2");
                cwg.ResetTime = DateTime.Now;
                Cuwuguis[i.ToString("D2")] = cwg;

            }
            /****************************初始化串口***********************************/
            XElement root = XElement.Load("configdata.xml");//读取配置文件
            var baud = (from customer1 in root.Descendants("baud")
                        select customer1.Value).FirstOrDefault();
            //30个仓口通信串口
            var com1 = (from customer1 in root.Descendants("com")
                        select customer1.Value).FirstOrDefault();
            //主控板的通信串口
            var com2 = (from customer1 in root.Descendants("com1")
                        select customer1.Value).FirstOrDefault();


            try
            {
                cdport = new Com.MyCOMPort(com1.ToString(), baud);
                cdport.DongEvent += new EventHandler(Cdport_DongEvent);

                if (!cdport.Online)
                {
                    cdport.Open();
                }

                saleport = new Com.MyCOMPort(com2.ToString(), baud);
                saleport.iTimeout = 10000;
                saleport.DongEvent +=new EventHandler(Saleport_DongEvent);
                if (!saleport.Online)
                    saleport.Open();


            }
            catch
            {
                //发布异常处理
                //EventUIInit initUi = new EventUIInit();
                //initUi.UIType = TermialStatus.ComError;
                //initUi.datatime = ComLinkLastTime;
                //initUi.Msg = "网络连接超时，最后联网时间" + ComLinkLastTime.ToString();
                //UpdateInitUIhandle(this, initUi);
            }

            return 0;

        }

        /// <summary>
        /// 查询销售模块状态线程
        /// </summary>
        public void QuerySaleModelStatusFromSerial()
        {
            while (true)
            {

                SaleEvent.WaitOne(19000);//等待19秒
                byte[] salecmd = COM_Cmd.QuerySaleCmd();
                string strcmds = COM_Cmd.byteToString(salecmd);

                udplog.logs.Enqueue(DateTime.Now.ToString() + "发主控机查询命令" + strcmds);
                if (!saleport.SendByte(salecmd))
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "主控机对查询命令没有响应");
                }
                Thread.Sleep(5000);
            }
        }
        /// <summary>
        /// 自动查询仓道状态模块
        /// </summary>
        public void QueryCdStatusFromSerial()
        {
            while (true)
            {
                while(NeedSendReBoot.Count > 0)
                {
                    if (StartEvent.WaitOne(30000, false)) //查看其他线程是否有发送任务，有的话则停止
                    {
                        StartEvent.Set();
                    }
                    if (NeedSendReBoot.First().ldate.AddMinutes(-1) < DateTime.Now)
                    {
                        LoseCD lscd = NeedSendReBoot.Dequeue();
                        try
                        {
                            Cuwuguis[lscd.cdn.ToString("D2")].isReset = false;


                            Cuwuguis[lscd.cdn.ToString("D2")].ResetTime = DateTime.Now;
                            //Cuwuguis[lscd.cdn.ToString("D2")].ResetCnt = Cuwuguis[lscd.cdn.ToString("D2")].ResetCnt+1;
                        }
                        catch (Exception ews)
                        {
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "发送重启指令" + ews.Message);
                        }
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "发送重启指令" + lscd.cdn.ToString());
                        byte[] cmdbyte = COM_Cmd.ResetCDCmd(lscd.cdn);
                        if (!cdport.SendByte(cmdbyte))
                        {
                            if (!cdport.SendByte(cmdbyte))
                            {

                            }
                        };//发送后即output.TransFlag启动WaitOne()使整个
                        Thread.Sleep(100);
                    }
                }


                for (int i = 1; i <= CDCnt; i++)//循环插入查询指令
                {
                    if (StartEvent.WaitOne(30000, false)) //查看其他线程是否有发送任务，有的话则停止
                    {
                        StartEvent.Set();
                    }
                    Thread.Sleep(200);
                    byte[] cmdbyte = COM_Cmd.QueryCDCmd(i);
                    string strcmds = COM_Cmd.byteToString(cmdbyte);

                    udplog.logs.Enqueue(DateTime.Now.ToString() + "发送" + strcmds);

                    if (!cdport.SendByte(cmdbyte))
                    {
                        if (!cdport.SendByte(cmdbyte))
                        {

                            Cuwuguis[i.ToString("D2")].CWGCommStatus = CWGCOMMSTATUS.Error;
                            Cuwuguis[i.ToString("D2")].CWGStatus = CUWUGUISTATUS.Error;
                            Cuwuguis[i.ToString("D2")].HasLostCnt++;
                            Cuwuguis[i.ToString("D2")].TestCnt++;

                            //udplog.logs.Enqueue(DateTime.Now.ToString() + "通信超时" + Cuwuguis[i.ToString("D2")].TestCnt.ToString() + "次,总故障" + Cuwuguis[i.ToString("D2")].HasLostCnt.ToString());
                            if (Cuwuguis[i.ToString("D2")].TestCnt > 3)
                            {
                                EventUI nui = new EventUI();
                                nui.UIType = UIUpdateType.Comm1Error;
                                nui.huodao = i.ToString();
                                nui.Msg = "通信超时" + Cuwuguis[i.ToString("D2")].TestCnt.ToString() + "次,总故障" + Cuwuguis[i.ToString("D2")].HasLostCnt.ToString();
                                fireEvent(nui);
                            }
                        }
                    };//发送后即output.TransFlag启动WaitOne()使整个

                    Thread.Sleep(200);
                }


            }
            
        }
        /// <summary>
        /// 界面更新事件
        /// </summary>
        /// <param name="uievent"></param>
        public void fireEvent(EventUI uievent)
        {
            if (UpdateUIhandle != null)
            {
                UpdateUIhandle(this, uievent);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void PoolTcpChannel()
        {
            while (true)
            {
                if (tcpchannel.taskMsg.Count > 0)
                {
                    LaidianCommandModel data = tcpchannel.taskMsg.Dequeue();
                    BasisEventCommand cmd = new BasisEventCommand();
                    cmd.Content = data;
                    cmd.Session = this;
                    cmd.EventCommandID = "tcp";
                    ServerTaskExecutorchainheader.Execute(cmd);
                }
            }
        }
        /// <summary>
        /// 售卖串口的回复处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Saleport_DongEvent(object sender, EventArgs e)
        {
            Com.newEventStr eventFromCom = (newEventStr)e;
            byte[] data = (byte[])eventFromCom.tys;
            BasisEventCommand serialportevent = new BasisEventCommand();
            serialportevent.Session = this;
            serialportevent.EventCommandID = "serial";
            serialportevent.Content = data;
            SaleLineSerialportCmdExecutorchainheader.Execute(serialportevent);
        }

        /// <summary>
        /// 仓道串口的回复数据处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cdport_DongEvent(object sender, EventArgs e)
        {
            Com.newEventStr eventFromCom = (newEventStr)e;
            byte[] data = (byte[])eventFromCom.tys;
            BasisEventCommand serialportevent = new BasisEventCommand();
            serialportevent.Session = this;
            serialportevent.EventCommandID = "serial";
            serialportevent.Content = data;
            CdSerialportCmdExecutorchainheader.Execute(serialportevent);
        }

        //功能性函数
        /// <summary>
        /// 烧写地址
        /// </summary>
        /// <param name="iaddr"></param>
        public void WriteAddress(int iaddr)
        {

            SaleEvent.Reset();//等待10秒
            Thread.Sleep(2000);

            byte[] cmdbyte = COM_Cmd.WriteAddrCom(iaddr);
            string str = COM_Cmd.byteToString(cmdbyte);
            udplog.logs.Enqueue(DateTime.Now.ToString() + "读取参数" + str);
            Thread.Sleep(500);
            if (!saleport.SendByte(cmdbyte))
            {
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.SetAddrF;
                //nui.huodao = strStation;
                nui.Msg = "烧写地址指令超时";
                udplog.logs.Enqueue(DateTime.Now.ToString() + "烧写指令超时");
                fireEvent(nui);

            }
        }

        /// <summary>
        /// 关闭主控PC
        /// </summary>
        public void ClosePC()
        {
            SaleEvent.Reset();//等待10秒
            Thread.Sleep(1000);
            byte[] cmdbyte = COM_Cmd.ClosePCCmd();
            string str = COM_Cmd.byteToString(cmdbyte);
            udplog.logs.Enqueue(DateTime.Now.ToString() + "关闭电脑" + str);
            Thread.Sleep(2000);
            if (!saleport.SendByte(cmdbyte))
            {

                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.RParamF;
                //nui.huodao = strStation;
                nui.Msg = "关闭电脑超时";


                udplog.logs.Enqueue(DateTime.Now.ToString() + "关闭电脑指令超时");
                fireEvent(nui);
            }
        }
        /// <summary>
        /// 读取参数
        /// </summary>
        /// <param name="iaddr"></param>
        public void ReadParams()
        {

            SaleEvent.Reset();//等待10秒
            Thread.Sleep(1000);
            byte[] cmdbyte = COM_Cmd.ReadparamCmd();
            string str = COM_Cmd.byteToString(cmdbyte);
            udplog.logs.Enqueue(DateTime.Now.ToString() + "读取参数" + str);
            Thread.Sleep(2000);
            if (!saleport.SendByte(cmdbyte))
            {

                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.RParamF;
                //nui.huodao = strStation;
                nui.Msg = "读取参数指令超时";


                udplog.logs.Enqueue(DateTime.Now.ToString() + "读取参数指令超时");
                fireEvent(nui);
            }
        }
        /// <summary>
        /// 烧写地址
        /// </summary>
        /// <param name="iaddr"></param>
        public void WriteParam(byte[] terminalparams)
        {
            udplog.logs.Enqueue(DateTime.Now.ToString() + "写开始写入参数没有响应");

            SaleEvent.Reset();//等待10秒
            Thread.Sleep(2000);
            byte[] cmdbyte = COM_Cmd.WriteParamCmd(terminalparams);
            string str = COM_Cmd.byteToString(cmdbyte);
            udplog.logs.Enqueue(DateTime.Now.ToString() + "写入参数" + str);

            if (!saleport.SendByte(cmdbyte))
            {
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.ParamF;
                //nui.huodao = strStation;
                nui.Msg = "烧写参数指令超时";
                udplog.logs.Enqueue(DateTime.Now.ToString() + "烧写参数指令超时");
                fireEvent(nui);



            }
        }

        //启动路由器的时刻
        public int ResetRouteAtMin = -1;
        //启动路由器的次数
        public int HasRetSetRoute = 0;
        /// <summary>
        /// 重启路由器
        /// </summary>
        public void ReSetRoute()
        {

            SaleEvent.Reset();//等待10秒
            Thread.Sleep(400);
            byte[] salecmd = COM_Cmd.ResetRouteCmd();
            if (!saleport.SendByte(salecmd))
            {
                Thread.Sleep(500);
                if (!saleport.SendByte(salecmd))
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "下位机对重启路由命令没有响应");

                }
            }
            //SaleEvent.Set();//等待10秒

        }
        /// <summary>
        /// 开回收仓
        /// </summary>
        public void OpenDoor()
        {
            if (MonitorGraStatus.Length == 2)
            {
                try
                {
                    int sd = Convert.ToInt32(MonitorGraStatus, 16);

                    if ((sd & 0x08) == 0x01)
                    {
                        EventUI nuilog = new EventUI();
                        nuilog.UIType = UIUpdateType.KCF;// UIUpdateType.StatusUpdate;
                        nuilog.huodao = "0xC9";
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "回收仓故障 不能打开");
                        fireEvent(nuilog);

                        return;
                    }
                }
                catch
                {
                }
            }

            SaleEvent.Reset();//等待10秒
            Thread.Sleep(400);
            byte[] salecmd = COM_Cmd.OpenGraDoorCmd();
            if (Huishou_Num >= HuiShouMaxNum)
            {
                return;
            }
            if (!saleport.SendByte(salecmd))
            {
                Thread.Sleep(500);
                if (!saleport.SendByte(salecmd))
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "下位机对开仓命令没有响应");
                }
            }
            //SaleEvent.Set();//等待10秒

        }
    }
}
