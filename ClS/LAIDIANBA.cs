using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ThoughtWorks.QRCode.Codec;
using System.Drawing;
using Com;
using System.Xml.Linq;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using com.imlaidian.protobuf.model;
namespace ClS
{
    /// <summary>
    /// 来电吧终端类
    /// 主要任务
    /// 1、循环检测所有下位机的状态
    /// 2、不断向云端请求，发送下位机状态，循环检测是否有租借和出售任务
    /// 3、如果有任意一个租借和出售任务，则暂停检测，优先执行租借和出售任务
    /// 4、将日常运行日志写入队列，
    /// 5、写日志到txt
    /// 6、
    /// 2015-10-26修改，加入一个重启指令，每天11时重启各仓道
    /// 2015-10-28修改，将售线、回收仓、仓道状态、日志记录、订单记录等数据储存到sqlite数据库中
    /// 2016-01-10修改 售线桶逻辑修改。
    /// </summary>
    public class LAIDIANBA
    {
        public string MonitorStatus = "";//主控机的状态
        public string MonitorVersion = "";//主控机版本
        public string MonitorGraStatus = "";//回收仓状态

        public string ShopPay = "";//支付的说明
        public string ShopPayImg = "";//支付的图片
        public bool HasGetShopInfoFlag = true;//是否已经获取信息
        public int HasGetShopInfoHour = 0;//获取店铺信息时间
        public string ShopLogo= "";

        //归还失败的
        public HuanAction onehuanaction=new HuanAction();

        //正在归还的仓口
        public int Hing;
        //正在租借的仓口
        public int Jing;

        AsynSocketListener udplog = new AsynSocketListener();
        public string CurrentUserName;
        public string CurrentUserPic;
        /// <summary>
        /// 租借的结果
        /// </summary>
        int prtJie = 0;
        /// <summary>
        /// 租借任务监控器
        /// </summary>
        System.Timers.Timer jieMonitor = new System.Timers.Timer(1000);
        public string code_token;
        private string GKurl = "119.29.63.197";//管控服务器地址
        private int jieResult = 0;
        /// <summary>
        /// 日志
        /// </summary>
        public string HttpUrl =@"http://mobile-api.imlaidian.com:8088";

        public Queue<string> logs = new Queue<string>();
        public JSLdbOrder oneorder;
        public newGprsEventStr ServerWork;
        public DateTime MonitorUpdateTime;

        /// <summary>
        /// 与云端的通信类
        /// </summary>
        public WCDMA wcdma;
        /// <summary>
        /// 读取下位机命令队列的停顿开关，只有上一个指令已经发送或者超时后才能开始下一个
        /// </summary>
        public ManualResetEvent StartEvent = new ManualResetEvent(true);
        /// <summary>
        /// 补报 回复状态不成功的订单队列
        /// </summary>
        public Queue<XmlOperatorOrder> bubaolist = new Queue<XmlOperatorOrder>(1000);
        /// <summary>
        /// 读取售线的停顿开关，只有上一个指令已经发送或者超时后才能开始下一个
        /// </summary>
        public ManualResetEvent SaleEvent = new ManualResetEvent(true);
        /// <summary>
        /// 循环归还开关 ，如果归还时出现某个仓位没有回应，则跑到另一个舱口
        /// </summary>
        public ManualResetEvent HuanEvent = new ManualResetEvent(true);
        /// <summary>
        /// 循环借出开关 ，如果归还时出现某个仓位没有回应，则跑到另一个舱口
        /// </summary>
        public ManualResetEvent JEvent = new ManualResetEvent(true);
        /// <summary>
        /// 更新界面的事件发布者。
        /// </summary>
        public event EventHandler UpdateUIhandle;

        /// <summary>
        /// 初始化化界面的事件发布者。
        /// </summary>
        public event EventHandler UpdateInitUIhandle;

        /// <summary>
        /// 包含的串口
        /// </summary>
        public Com.MyCOMPort output;// = new Com.MyCOMPort("com1");

        /// <summary>
        /// 主控串口
        /// </summary>
        public Com.MyCOMPort outputsale;// = new Com.MyCOMPort("com2");

       

        /// <summary>
        /// 0为正常模式  1为测试模式
        /// </summary>
        public int Test_Mode = 0;
        /// <summary>
        /// 储物柜的个数
        /// </summary>
        //演示机
        //public int icnt = 2;
        public int icnt = 30;
        /// <summary>
        /// 终端编号
        /// </summary>
        public string TerminalNO = "000200000001";
        public string Power="000";//电压
        public string smk="1";//烟雾
        public string temp="000";//温度
        public int s1 = 0, s2 = 0, s3 = 0, s4 = 0;//现存线数量
        public int ls1 = 0, ls2 = 0, ls3 = 0, ls4 = 0;//上次补线数量
        public int InPutOutHuodao = 0;//正在出货货道
        /// <summary>
        /// 发送下位机的队列
        /// </summary>
        public Queue<Com.newEventStr> SendComCmds = new Queue<Com.newEventStr>();


        /// <summary>
        /// 发送云端事件的队列
        /// </summary>
        public Queue<ReponseWorkEvent> SendNetEvents = new Queue<ReponseWorkEvent>();

        /// <summary>
        /// 发送售线机器的命令队列
        /// </summary>
        public Queue<ReponseWorkEvent> SaleNetEvents = new Queue<ReponseWorkEvent>();
        /// <summary>
        ///运行日志队列
        /// </summary>
        public Queue<string> Logs = new Queue<string>();
        /// <summary>
        ///各个仓状态日志队列
        /// </summary>
        public Queue<string> statuslogs = new Queue<string>();
        /// <summary>
        /// 终端状态,如果在执行云端任务的话，就设置为忙
        /// </summary>
        public LDBStatus TerminalStatus;

        /// <summary>
        /// 终端所含的储物柜
        /// </summary>
        public Dictionary<string,CUWUGUI> Cuwuguis;
        /// <summary>
        /// 出货标志，如果是0则是用户借充电宝，需上传服务器，如果是1则是管理员拿出充电宝，不需要上传服务器。
        /// </summary>
        public int cflag = 0;//出货标志，如果是0则是用户借充电宝，需上传服务器，如果是1则是管理员拿出充电宝，不需要上传服务器。
        /// <summary>
        /// 与云端通信的最后时间
        /// </summary>
        public DateTime ComLinkLastTime;
        /// <summary>
        /// 与云端握手线程
        /// </summary>
        public Thread PLinkServer;
        /// <summary>
        /// 如果有需要回复的事件 则此线程读取回复事件，连接云端。
        /// </summary>
        public Thread PResponseEvent;
        /// <summary>
        /// 查询云端任务线程
        /// </summary>
        public Thread PQueryServer;
        /// <summary>
        /// 与下位机通信线程
        /// </summary>
        public Thread PComm;
        /// <summary>
        /// 发送云端通信线程
        /// </summary>
        public Thread PSComm;
        /// <summary>
        /// 写日志线程
        /// </summary>
        public Thread PLogWrite;

        /// <summary>
        /// 上传日志线程
        /// </summary>
        public Thread PUploadLog;
        /// <summary>
        /// 根据储物柜，和云端通信状态更新来电吧的状态
        /// </summary>
        public Thread PUpdateTerminalStatus;
        /// <summary>
        /// 下位机通信所激发的事件，用来更新界面
        /// </summary>
        /// <summary>
        /// 查询是否有销售任务
        /// </summary>
        public Thread PQuerySale;
        //存放不能上报的售线任务
        public System.Collections.Hashtable SaleLinesNoReports = new System.Collections.Hashtable(1000);
        /// <summary>
        /// 线数量更新
        /// </summary>
        public Thread PLinesUpdate;

        /// <summary>
        /// 循环补货功能
        /// </summary>
        public Thread PBH;

        /// <summary>
        /// 重启仓道标志，每天重启一次仓道
        /// </summary>
        public bool RsetCDFlag = false;
        /// <summary>
        /// 查询售线模块状态的线程线程
        /// </summary>
        public Thread PQuerySaleStatus;
        /// <summary>
        /// 售线线程结果回馈线程
        /// </summary>
        public Thread PclientLineCheckOutFeedBack;

        /// <summary>
        /// 查找订单不成功线程
        /// </summary>
        public Thread FindLostOrderThread;

        /// <summary>
        /// 将回复不成功订单加入订单反馈队列
        /// </summary>
        public Thread BuBaoThread;
        /// <summary>
        /// 回收仓已存数量
        /// </summary>
        public int Huishou_Num = 0;

        //AsynSocketListener udplog = new AsynSocketListener();
        /// <summary>
        /// 回收仓允许的最大存放数量
        /// </summary>
        private int HuiShouMaxNum = 30;
        /// <summary>
        /// 存放回复仓执行情况消息
        /// </summary>
        public Queue<ResponseHuiShouData> huishouEventDatas = new Queue<ResponseHuiShouData>();
        /// <summary>
        /// 回收回复云端线程
        /// </summary>
        public Thread sendHuishouThread;
        ///
        public string huishouTaskId = "";

        public byte iSetAddr = 0;//待写入地址
        /// <summary>
        /// 参数
        /// </summary>
        public TerminalParams tparams=new TerminalParams();
        /// <summary>
        /// 2015-12-28
        /// 需要重启的仓道编号
        /// </summary>
        public Queue<LoseCD> NeedSendReBoot = new Queue<LoseCD>();


        /// <summary>`  
        /// 初始化函数
        /// </summary>
        public void DefaultLoat()
        {
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
                            s1 = ls.Num;
                        }
                        if (ls.ID == "2")
                        {
                            s2 = ls.Num;
                        }
                        if (ls.ID == "3")
                        {
                            s3 = ls.Num;
                        }
                        if (ls.ID == "4")
                        {
                            s4 = ls.Num;
                        }
                    }
           
            }
            catch
            { 
            }

            jieMonitor.Elapsed += new System.Timers.ElapsedEventHandler(jieMonitor_Elapsed);

            SetWindow.ShowWindow(SetWindow.FindWindow("Shell_TrayWnd", null), 0);
            SetWindow.ShowWindow(SetWindow.FindWindow("Button", null), 0);
            //演示机
            //TerminalNO = "000775000003";
            Operator.HasLock();//判断是否有加密狗
            TerminalNO = Operator.GetTerminalNO();//"000775000002";//  "000775000001";//从加密狗获取编号
           // TerminalNO =  "000755010026";
            XElement root = XElement.Load("configdata.xml");

            //XElement xe = root.Element("config").Element("defaultPath");

            try
            {
                var baud = (from customer1 in root.Descendants("baud")
                            select customer1.Value).FirstOrDefault();
                //30个仓口通信串口
                var com1 = (from customer1 in root.Descendants("com")
                            select customer1.Value).FirstOrDefault();
                //主控板的通信串口
                var com2 = (from customer1 in root.Descendants("com1")
                            select customer1.Value).FirstOrDefault();

                //回收仓数量

                var huishounum= (from customer1 in root.Descendants("huishou")
                            select customer1.Value).FirstOrDefault();
                if (huishounum.ToString() != "")
                    int.TryParse(huishounum.ToString(),out Huishou_Num);
               // string[] ComPortName;
               // ComPortName = System.IO.Ports.SerialPort.GetPortNames();//检查当前都有哪些串口号、
                //if (ComPortName.Contains(com1.ToString()))
                {
                    try
                    {
                        output = new Com.MyCOMPort(com1.ToString(), baud);
                        output.DongEvent += new EventHandler(output_DongEvent);

                        if (!output.Online)
                        {
                            output.Open();
                        }

                        outputsale = new MyCOMPort(com2.ToString(), baud);
                        outputsale.iTimeout = 10000;
                        outputsale.DongEvent += new EventHandler(outputsale_DongEvent);
                        if (!outputsale.Online)
                            outputsale.Open();

                        
                    }
                    catch
                    { 
                        //发布异常处理
                        EventUIInit initUi = new EventUIInit();
                        initUi.UIType = TermialStatus.ComError;
                        initUi.datatime = ComLinkLastTime;
                        initUi.Msg = "网络连接超时，最后联网时间" + ComLinkLastTime.ToString();
                        UpdateInitUIhandle(this, initUi);
                    }


                    if (output.Online)
                    {
                        //初始化储物柜通信程序，并更新界面
                        Cuwuguis = new Dictionary<string, CUWUGUI>();
                        for (int i = 1; i <= icnt; i++)
                        {
                            CUWUGUI cwg = new CUWUGUI();
                            cwg.CDB = null;
                            cwg.CWGStatus = CUWUGUISTATUS.None;
                            cwg.CWGCommStatus = CWGCOMMSTATUS.Error;
                            cwg.CWGID = i.ToString("D2");
                            cwg.ResetTime = DateTime.Now;
                            Cuwuguis[i.ToString("D2")] = cwg;

                        }

                        //初始化跟云端的通信，
                        //订阅串口的事件处理
                        //网络拨号
                        try
                        {
                            wcdma = new WCDMA();
                        }
                        catch
                        { 
                        }

                        
                        //查询云端是否有需要执行的任务
                        PQueryServer = new Thread(new ThreadStart(QueryWork));
                        //PQuerySale.IsBackground = true;
                        PQueryServer.Start();
                        //回复云端任务执行情况线程
                        PResponseEvent = new Thread(new ThreadStart(ResponseWorkStatus));
                        //PResponseEvent.IsBackground = true;
                        PResponseEvent.Start();
                        //循环测试下位机
                        PComm = new Thread(new ThreadStart(ThreadOfInComCmd));
                        //PComm.IsBackground = true;
                        PComm.Start();
                        //巡测与云端通信
                        PLinkServer = new Thread(new ThreadStart(TestSLink));
                        //PLinkServer.IsBackground = true;
                        PLinkServer.Start();



                        //出线线程
                        PQuerySale = new Thread(new ThreadStart(ThreadofSale));
                         //PQuerySale.IsBackground = true;
                        PQuerySale.Start();
                        //查询状态主控板
                        PQuerySaleStatus = new Thread(new ThreadStart(QuerySale));
                        PQuerySaleStatus.Start();

                        //售线状态回复
                        PclientLineCheckOutFeedBack = new Thread(new ThreadStart(clientLineCheckOutFeedBack));
                        PclientLineCheckOutFeedBack.Start();


                        //执行租借与归还指令线程
                        PSComm = new Thread(new ThreadStart(ThreadOfSendComCmd));
                       // PSComm.IsBackground = true;
                        PSComm.Start();
                        //wcdma.GprsDongEven t += new EventHandler(gprs_GprsDongEvent);
                        //写日志线程
                        //PLogWrite = new Thread(new ThreadStart(udplog.StartListening));
                        ////PLogWrite.IsBackground = true;
                        //PLogWrite.Start();
                        //Thread PTEST = new Thread(new ThreadStart(Test));
                        //PTEST.Start();
                        PLinesUpdate = new Thread(new ThreadStart(UpdataLineNum));
                      //  PLinesUpdate.IsBackground = true;
                        PLinesUpdate.Start();

                        PUploadLog = new Thread(new ThreadStart(UpLoadLog));
                        PUploadLog.Start();

                        try
                        {
                            string strV = TerminalNO+" "+DateTime.Now.ToString()+"restart  Main Veson"+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 3);
                            byte[]bdata=Encoding.ASCII.GetBytes(strV);
                            ClS.sky.SendMessage(bdata, GKurl, 8234);
                        }
                        catch
                        { }

                        

                    }
                   
                    if (outputsale.Online)
                    {
                        
                    }
                }

            }
            catch (CommPortException ee)
            {
               // udplog.logs.Enqueue("端口初始化错误");
            }
            catch
            { }
            try
            {
                getLogo();
            }
            catch
            { }
            //加载线数量更新线程

           
        


        }

        void jieMonitor_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            prtJie++;
            if (prtJie > 12)
            {

                jieMonitor.Stop();
                //发送查询，并且将状态强制回复服务器
                try
                {

                    oneorder.iTryCnt = 1;
                    oneorder.status = false;
                    oneorder.nowstatus = UIUpdateType.StatusUpdate;
                    StartEvent.Set();

                }
                catch(Exception ejs)
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + ejs.Message);
                }

            }
        }
        /// <summary>
        /// 补报状态函数
        /// </summary>
        public void FunBubao()
        {
            try
            {
                while (true)
                {
                    if (bubaolist.Count > 0)
                    {
                        XmlOperatorOrder xmls = bubaolist.Dequeue();



                        if (xmls.ordertype == "0")//借出
                        {
                            ReponseWorkEvent report = new ReponseWorkEvent();
                            report.workNo = xmls.orderno;
                            if (xmls.status == "2")
                                report.workstatus = true;
                            else
                                report.workstatus = false;

                            report.CDB = xmls.cdbno;
                            SendNetEvents.Enqueue(report);
                        }
                        if (xmls.ordertype == "1")//售线
                        {
                            ReponseWorkEvent report = new ReponseWorkEvent();
                            report.workNo = xmls.orderno;
                            if (xmls.status == "2")
                                report.workstatus = true;
                            else
                                report.workstatus = false;
                            report.CDB = xmls.cdbno;
                            SaleSendNetEvents.Enqueue(report);

                        }
                        if (xmls.ordertype == "2")//回收
                        {

                            ResponseHuiShouData report = new ResponseHuiShouData();
                            report.taskId = xmls.orderno;
                            if (xmls.status == "2")
                                report.status = true;
                            else
                                report.status = false;
                            report.terminal = TerminalNO;
                            huishouEventDatas.Enqueue(report);

                        }

                    }
                    Thread.Sleep(2000);
                }
            }
            catch
            { }
        }
        /// <summary>
        /// 查询销售模块状态线程
        /// </summary>
        public void QuerySale()
        {
            while (true)
            {  
               
                SaleEvent.WaitOne(19000);//等待30秒

                byte[] salecmd = COM_Cmd.QuerySaleCmd();
                string strcmds = COM_Cmd.byteToString(salecmd);
                udplog.logs.Enqueue(DateTime.Now.ToString() + "发主控机查询命令"+strcmds);
                if (!outputsale.SendByte(salecmd))
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "主控机对查询命令没有响应");
                }
                Thread.Sleep(5000);
            }
        }
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
            if (!outputsale.SendByte(cmdbyte))
            {
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.SetAddrF;
                //nui.huodao = strStation;
                nui.Msg = "烧写地址指令超时";


                udplog.logs.Enqueue(DateTime.Now.ToString() + "烧写指令超时");
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nui);
                }

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
            if (!outputsale.SendByte(cmdbyte))
            {

                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.RParamF;
                //nui.huodao = strStation;
                nui.Msg = "关闭电脑超时";


                udplog.logs.Enqueue(DateTime.Now.ToString() + "关闭电脑指令超时");
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nui);
                }
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
            if (!outputsale.SendByte(cmdbyte))
            {

                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.RParamF;
                //nui.huodao = strStation;
                nui.Msg = "读取参数指令超时";


                udplog.logs.Enqueue(DateTime.Now.ToString() + "读取参数指令超时");
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nui);
                }
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
            udplog.logs.Enqueue(DateTime.Now.ToString() + "写入参数"+str);

            if (!outputsale.SendByte(cmdbyte))
            {
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.ParamF;
                //nui.huodao = strStation;
                nui.Msg = "烧写参数指令超时";

            
                udplog.logs.Enqueue(DateTime.Now.ToString() + "烧写参数指令超时");
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nui);
                }

                

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
            if (!outputsale.SendByte(salecmd))
            {
                Thread.Sleep(500);
                if (!outputsale.SendByte(salecmd))
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
                        if (UpdateUIhandle != null)
                        {
                            UpdateUIhandle(this, nuilog);
                        }

                        return;
                    }
                }
                catch { 
                }
            }

            SaleEvent.Reset();//等待10秒
            Thread.Sleep(400);
            byte[] salecmd = COM_Cmd.OpenGraDoorCmd();
            if (huishouTaskId != "" && Huishou_Num >= HuiShouMaxNum)
            {
                return;
            }
            if (!outputsale.SendByte(salecmd))
            {
                Thread.Sleep(500);
                if (!outputsale.SendByte(salecmd))
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "下位机对开仓命令没有响应");
                    if (huishouTaskId != "")
                    {
                        ResponseHuiShouData one = new ResponseHuiShouData();
                        one.status = false;
                        one.terminal = TerminalNO;
                        one.taskId = huishouTaskId;
                        huishouEventDatas.Enqueue(one);

                        huishouTaskId = "";
                    }
                }
            }
            //SaleEvent.Set();//等待10秒

        }
       // public string saleorderid = "";
        ReponseWorkEvent rsSale = null;


        public bool bAutoSaleLine = false;
        public int[] TestSLCnt=new int[3];
        public int[]TestSLSCnt =new int[3];
        private int TestID=0;

        /// <summary>
        /// 测试自动售线
        /// </summary>
        public void TestSaleLine(int id)
        {
           
            if (bAutoSaleLine)//如果已经处于测试模式
            { 
           

                bAutoSaleLine = false;//停止测试模式

            }
            else
            {
                TestSLCnt[0] = 0;
                TestSLCnt[1] = 0;
                TestSLCnt[2] = 0;

                TestSLSCnt[0] = 0;
                TestSLSCnt[1] = 0;
                TestSLSCnt[2] = 0;
                bAutoSaleLine =true;//开始测试模式
               
                // if (PComm.ThreadState == ThreadState.Running)
           
            }
            new Thread(() =>
            {
                while (bAutoSaleLine)
                {
                    Thread.Sleep(5000);


                    
                    //rsSale. = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + DateTime.Now.Hour.ToString("D2") + DateTime.Now.Minute.ToString("D2") + DateTime.Now.Second.ToString("D2");
                    for (int i = 0; i < 3; i++)
                    {
                        Thread.Sleep(8000);
                        TestID = i;
                        TestSLCnt[i]++;
                        SaleEvent.Reset();
                       
                        /*-----------------------测试代码-------------------*/
                        #region 测试代码
                        rsSale = new ReponseWorkEvent();
                        rsSale.workstatus = false;
                        rsSale.workNo = "123456566";
                        rsSale.CDB = string.Format("{0}",i+1);
                        rsSale.NeedToCheck = false;
                        rsSale.linetype = i;
                        #endregion
                        /*-----------------------测试代码-------------------*/

                        byte[] salecmd = COM_Cmd.SaleLineCmd(i, 0);
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "开始出线命令");
                        if (!this.outputsale.SendByte(salecmd))
                        {
                            Thread.Sleep(2000);
                            if (!this.outputsale.SendByte(salecmd))
                            {
                                //没有响应

                               // udplog.logs.Enqueue(DateTime.Now.ToString() + "下位机对出线命令没有响应");


                            }
                        }
                    }

                }
            }).Start();
        }

        /// <summary>
        /// 售线函数
        /// </summary>
        /// <param name="type">线种类</param>
        /// <param name="from">0为测试，1为正式订单</param>
        /// <returns></returns>
        public bool SaleLine(int type, int from)
        {
            udplog.logs.Enqueue(DateTime.Now.ToString() + "开始出线");
            //超过一分钟没有通信  则返回失败
            if (MonitorUpdateTime < DateTime.Now.AddMinutes(-2))
            {
                if (rsSale != null)
                {
                    rsSale.workstatus = false;

                    SaleSendNetEvents.Enqueue(rsSale);

                }
                EventUI nuilog = new EventUI();
                nuilog.UIType = UIUpdateType.SaleL;// UIUpdateType.StatusUpdate;
                nuilog.huodao = "0xC8";
                udplog.logs.Enqueue(DateTime.Now.ToString() + "售线机构通信故障，无法执行出线命令");
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nuilog);
                }

                return false;
            }
            /*---------------------------
             * 判断售线桶是否有问题
             * 高四位 是否有线 1为有线 ，低四位售线机构状态是否正常 0为正常
             * --------------------------*/
            bool canSale = true;


            for (int i = 0; i < 5; i++)
            {

                if (MonitorStatus.Length == 2)
                {


                    int sd = Convert.ToInt32(MonitorStatus, 16);
                    if (type == 0)
                    {
                        //售线机构故障
                        if ((sd & 0x01) == 0x01)
                        {
                  
                            udplog.logs.Enqueue(DateTime.Now.ToString() + " " + type.ToString() + "售线机构通信故障，无法执行出线命令");

                            canSale = false;
                        }
                        //没有线
                        if ((sd & 0x10) == 0x00)
                        {
                        
                            udplog.logs.Enqueue(DateTime.Now.ToString() + " " + type.ToString() + "线为空,不能出线");
         
                            //1号桶出现线为空，则转移到2号桶
                            if (canSale)
                            {
                                if (rsSale != null)
                                {
                                    udplog.logs.Enqueue(DateTime.Now.ToString() + " " + type.ToString() + "售线机为空，转移到1号桶");
                                    rsSale.CDB = "2";
                                    type = 1;
                                }
                            }
                            else
                            {
                                canSale = false;
                            }

                            
                        }
                    }
                    if (type == 1)
                    {
                        //售线机构故障
                        if ((sd & 0x02) == 0x02)
                        {

                            udplog.logs.Enqueue(DateTime.Now.ToString() + " " + type.ToString() + "售线机构故障，无法执行出线命令");
                    
                            canSale = false;
                        }
                        //没有线
                        if ((sd & 0x20) == 0x00)
                        {

                            udplog.logs.Enqueue(DateTime.Now.ToString() + " " + type.ToString() + "线为空，无法执行出线命令");
                     
                            canSale = false;
                        }
                    }
                    if (type == 2)
                    {
                        //售线机构故障
                        if ((sd & 0x04) == 0x04)
                        {
                 
                            udplog.logs.Enqueue(DateTime.Now.ToString() + " " + type.ToString() + "售线机构故障，无法执行出线命令");
                            canSale = false;
                        }

                        //没有线
                        if ((sd & 0x40) == 0x00)
                        {
                      
                            udplog.logs.Enqueue(DateTime.Now.ToString() + " " + type.ToString() + "线为空，无法执行出线命令");
                            canSale = false;
                        }
                    }
                }
                if (canSale)
                    break;
                udplog.logs.Enqueue(DateTime.Now.ToString() + " 第" +i.ToString() + "次检查,状态"+MonitorStatus);
                Thread.Sleep(6000);

            }

            //如果不能借出 则返回失败给服务器端
            if (canSale == false)//不能出线
            {
                if (rsSale != null)
                {
                    rsSale.workstatus = false;

                    SaleSendNetEvents.Enqueue(rsSale);

                }
                EventUI nuilog = new EventUI();
                nuilog.UIType = UIUpdateType.SaleL;// UIUpdateType.StatusUpdate;
                nuilog.huodao = "0xC8";
                udplog.logs.Enqueue(DateTime.Now.ToString() + " " + type.ToString() + "购线失败");
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nuilog);
                }
                return false;
            }
            //if (type == 1)
            {


                //rsSale. = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + DateTime.Now.Hour.ToString("D2") + DateTime.Now.Minute.ToString("D2") + DateTime.Now.Second.ToString("D2");
                bool fa = false;
                //for (int i = 0; i < 2; i++)
                //{
                SaleEvent.Reset();
                Thread.Sleep(600);
   
                /*将售线任务的ID下发给下位机*/
                int task_id = 0;
                if (rsSale != null)
                {
                    int.TryParse(rsSale.workNo, out task_id);
                }
                string strtaskid = task_id.ToString("X8");
                byte[] salecmd = COM_Cmd.SaleLineCmd(type,task_id);
                string strcmds = COM_Cmd.byteToString(salecmd);
                udplog.logs.Enqueue(DateTime.Now.ToString() + "开始出线命令" + strcmds);
                if (!this.outputsale.SendByte(salecmd))
                {
                    Thread.Sleep(1000);
                    //if (!this.outputsale.SendByte(salecmd))
                    //{
                    //没有响应
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "下位机对出线命令没有响应");


                    //}
                }

                byte[] bdata = Encoding.ASCII.GetBytes(TerminalNO + " saleline start");
                ClS.sky.SendMessage(bdata, GKurl, 8234);
                if (SaleEvent.WaitOne(20000) == false)//如果售线没有返回结果，则启动查询
                {
                    SaleEvent.Set();
                    if (rsSale != null)
                        rsSale.NeedToCheck = true;
                }
                else
                {
                    if (from == 1)
                    {
                        if (rsSale.workstatus)
                        {
                            fa = true;
                            //break;
                        }
                    }
                    if (rsSale != null)
                    {
                        //rsSale.workstatus = false;
                        if (fa == false)
                        {
                            SaleSendNetEvents.Enqueue(rsSale);
                        }
                    }
                }

            }
            return true;
        }
        //卖线模块接收事件处理
        void outputsale_DongEvent(object sender, EventArgs e)
        {
            Com.newEventStr eventFromCom = (newEventStr)e;
            byte[] data = (byte[])eventFromCom.tys;
            string strcmds = "";
            foreach (byte xt in data)
            {
                strcmds += " " + xt.ToString("X2");
            }

            byte btype = data[3];
       
            string strStation = ((int)data[1]).ToString("D2");
 
     
            if (btype == 'C')
            {
                MonitorUpdateTime = DateTime.Now;
                udplog.logs.Enqueue(DateTime.Now.ToString()+ "查询回复：" + strcmds );
                if (data.Length > 12)
                {
                    //data[4]低四位表示售线桶，为1则表示有线可售（限2代机方可使用），高四位第一至第四位分别代表1-4个售线桶，0表示正常，1表示故障，data[5]+data[6]为版本号 data[7]为回收仓状态
                    MonitorStatus = data[4].ToString("X2") ;
                    MonitorVersion = data[5].ToString("X2") + data[6].ToString("X2");
                    MonitorGraStatus=data[7].ToString("X2");// + data[7].ToString("X2") + data[8].ToString("X2");
                }
                else
                {
                    MonitorStatus = data[4].ToString("X2");// +data[5].ToString("X2") + data[6].ToString("X2");
                }
                #region 新版售线桶查询机制
                if (rsSale != null)
                {
                    if (rsSale.NeedToCheck)//需要检查
                    {
                        udplog.logs.Enqueue("由于售线未回复，正在查询售线结果");
                        /*将售线任务的ID下发给下位机*/
                        int task_id = 0;
                        if (rsSale != null)
                        {
                            int.TryParse(rsSale.workNo, out task_id);
                        }
                        string strtaskid = task_id.ToString("X8");
                        string checktaskid = data[8].ToString("X2") + data[9].ToString("X2") + data[10].ToString("X2") + data[11].ToString("X2");
                        udplog.logs.Enqueue("售线任务ID" + strtaskid + "，售线结果ID" + checktaskid);
                        if (checktaskid == strtaskid)
                        {
                            udplog.logs.Enqueue("查询到售线任务执行成功,ID" + strtaskid + "向服务器汇报结果");
                            try
                            {
                                XmlOperatorLine lines = new XmlOperatorLine();
                                List<XmlOperatorLine> xmls = lines.GetAll();

                                foreach (XmlOperatorLine ls in xmls)
                                {
                                    if (ls.ID == rsSale.CDB.Trim())
                                    {
                                        int line1 = ls.Num;
                                        if (line1 > 0)
                                        {

                                            ls.Num = line1 - 1;
                                            ls.Modify();
                                        }
                                    }

                                }
                            }
                            catch (Exception ss)
                            {
                                udplog.logs.Enqueue(DateTime.Now.ToString() + "#" + strStation + "出线成功时操作line文件错误" + ss.Message);
                            }
                            rsSale.workstatus = true;

                            SaleSendNetEvents.Enqueue(rsSale);

                        }
                        rsSale.NeedToCheck = false;

                    }
                }
                #endregion
            }
          

            /*
             *    售线命令
                 发送：头（%）+地址（1字节）+长度（字节）+类型是’B’+线道（1字节）+校验（1字节）+尾（%）；
                 1.返回：头（%）+地址（1字节）+长度（字节）+类型是’B’ +线道（1字节）+接收状态（1字节，0-成功，其他表示失败）+校验（1字节）+尾（%）；
                     等待时间：15秒；
                     重复次数：3；
                 2、返回：头（%）+地址（1字节）+长度（字节）+类型是’O’+线道（1字节）+执行状态（1字节，0-成功，其他表示失败）+校验（1字节）+尾（%）

             */
            if (btype == 'W')
            {
                SaleEvent.Set();
                if (data[4] == 0)
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "关机设置成功");
                else
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "关机设置失败");
            }
            //重启路由命令
            if (btype == 'M')
            {
                SaleEvent.Set();
                if (data[4] == 0x21)
                    udplog.logs.Enqueue(DateTime.Now.ToString() + strcmds + "重启路由成功");
                else
                    udplog.logs.Enqueue(DateTime.Now.ToString() + strcmds + "重启路由失败");
            }
            if (btype == 'L')//读取参数
            {
                SaleEvent.Set();
                byte ftype = data[4];
                if (ftype == 0x0)//开始参数下发成功
                {
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.RParamS;
                    //nui.huodao = strStation;
                    nui.Msg = "读取参数成功";

                    for (int i = 0; i < 248; i++)
                    {
                        tparams.others[i] = data[6 + i];
                    }
                    tparams.YW1 = data[6];
                    tparams.YW2 = data[7];
                    tparams.CDSC = data[8];
                    try
                    {
                        try
                        {
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "读取参数成功");
                        }
                        catch
                        {
                        }
                    }
                    catch
                    { }
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    //rsSale.status  oneorder.status = true;
                }
                else//开始参数下发失败
                {


                    //SaleEvent.Set();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.RParamF;
                    //nui.huodao = strStation;
                    nui.Msg = "读取参数失败";
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "读取参数失败");
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }

                }
            }


            if (btype == 'P')//已经接收出货指令开始开启
            {
                byte ftype = data[4];
                if (ftype == 0x0)//开始参数下发成功
                {
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ParamS;
                    //nui.huodao = strStation;
                    nui.Msg = "执行参数下发成功";
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "执行参数下发成功");
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    //rsSale.status  oneorder.status = true;
                }
                else//开始参数下发失败
                {


                    SaleEvent.Set();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ParamF;
                    //nui.huodao = strStation;
                    nui.Msg = "执行参数下发失败";
                    try
                    {
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "执行参数下发失败");
                    }
                    catch
                    {
                    }
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }

                }
            }
            if (btype == 'B')//已经接收出货指令开始开启
            {
               
                byte ftype = data[4];
                if (ftype == 0x0)//开始执行出货
                {
                    tools.insertLog("执行售线" + strcmds,Logtype.Sale);
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.SaleING;
                    nui.huodao = strStation;
                    nui.Msg = "设备准备出货……";
                    try
                    {
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "#" + strStation + "设备准备出货……" + strcmds);
                    }
                    catch
                    {
                    }
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    //rsSale.status  oneorder.status = true;
                }
                else//出货失败
                {
                    tools.insertLog("执行失败" + strcmds, Logtype.Sale);
                    if (rsSale != null)
                    {
                        rsSale.workstatus = false;

                        SaleSendNetEvents.Enqueue(rsSale);
                    }

                    SaleEvent.Set();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.SaleL;
                    nui.huodao = strStation;
                    nui.Msg = "出货失败";
                    try
                    {
                        if (rsSale != null)
                        {
                            try
                            {
                                XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                                xmlorder.orderno = rsSale.workNo;
                                xmlorder.datatime = DateTime.Now.ToString();
                                xmlorder.cdbno = "00";
                                xmlorder.cmno = "00";
                                xmlorder.status = "0";
                                xmlorder.ordertype = "0";
                                xmlorder.upserver = "0";
                                xmlorder.datatime = DateTime.Now.ToString();
                                xmlorder.Modify();
                            }
                            catch
                            {
                            }
                        }
                        byte[] bdata = Encoding.ASCII.GetBytes(TerminalNO + " saleline false");
                        ClS.sky.SendMessage(bdata, GKurl, 8234);
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "#" + strStation + "售线失败"+strcmds);
                    }
                    catch
                    { }
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }

                }
            }
            if (btype == 0x4F)//出货指令执行完成
            {
         
                byte ftype = data[4];
                if (ftype == 0x0)//出货成功
                {
                    try
                    {
                        TestSLSCnt[TestID]++;
                    }
                    catch
                    { 

                    }
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.SaleF;
                    nui.huodao = strStation;
                    nui.Msg = "出货成功";
                    byte[] bdata = Encoding.ASCII.GetBytes(TerminalNO + " saleline success");

                    ClS.sky.SendMessage(bdata, GKurl, 8234);

                    try
                    {
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "#" + strStation + "出货成功"+strcmds);
                    }
                    catch
                    {
                    }
                    if (rsSale != null)
                    {
                        //tools.insertLog("出线成功" + strcmds, Logtype.Sale);
                        //DataSet dt=laidian.DbHelperSQL.Query("select * from main_laidian");
                        
                        //if (dt.Tables[0].Rows.Count > 0)
                        //{
                        //    DataRow r = dt.Tables[0].Rows[0];
                        //if (File.Exists("laidian.sqlite"))
                        //{
                        //    try
                        //    {
                        //        //int sline = int.Parse(r["line" + rsSale.CDB.Trim()].ToString());

                        //        if (rsSale.CDB.ToString().Trim() == "1")
                        //        {
                        //            if (s1 > 0)
                        //                s1 = s1 - 1;
                        //            string updatelinenum = "update main_laidian set line1=" + s1.ToString();
                        //            laidian.DbHelperSQL.ExecuteSql(updatelinenum);
                        //        }
                        //        if (rsSale.CDB.ToString().Trim() == "2")
                        //        {
                        //            if (s2 > 0)
                        //                s2 = s2 - 1;
                        //            string updatelinenum = "update main_laidian set line2=" + s2.ToString();
                        //            laidian.DbHelperSQL.ExecuteSql(updatelinenum);
                        //        }
                        //        if (rsSale.CDB.ToString().Trim() == "3")
                        //        {
                        //            if (s3 > 0)
                        //                s3 = s3 - 1;
                        //            string updatelinenum = "update main_laidian set line3=" + s3.ToString();
                        //            laidian.DbHelperSQL.ExecuteSql(updatelinenum);
                        //        }
                        //        if (rsSale.CDB.ToString().Trim() == "4")
                        //        {
                        //            if (s4 > 0)
                        //                s4 = s4 - 1;
                        //            string updatelinenum = "update main_laidian set line4=" + s4.ToString();
                        //            laidian.DbHelperSQL.ExecuteSql(updatelinenum);
                        //        }
                        //    }
                        //    catch
                        //    {
                        //    }
                        //}
                        //else
                        //{
                        try
                        {
                            XmlOperatorLine lines = new XmlOperatorLine();
                            List<XmlOperatorLine> xmls = lines.GetAll();

                            foreach (XmlOperatorLine ls in xmls)
                            {
                                if (ls.ID == rsSale.CDB.Trim())
                                {
                                    int line1 = ls.Num;
                                    if (line1 > 0)
                                    {

                                        ls.Num = line1 - 1;
                                        ls.Modify();
                                    }
                                }

                            }
                        }
                        catch(Exception ss)
                        {
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "#" + strStation + "出线成功时操作line文件错误" + ss.Message);
                        }
                        //}
                        if (rsSale != null)
                        {
                            rsSale.workstatus = true;
                            try
                            {
                                //XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                                //xmlorder.orderno = rsSale.workNo;
                                //xmlorder.datatime = DateTime.Now.ToString();
                                //xmlorder.cdbno = "00";
                                //xmlorder.cmno = "00";
                                //xmlorder.status = "1";
                                //xmlorder.ordertype = "0";
                                //xmlorder.upserver = "0";
                                //xmlorder.datatime = DateTime.Now.ToString();
                                //xmlorder.Modify();
                            }
                            catch
                            { }
                            SaleSendNetEvents.Enqueue(rsSale);
                        }

                    }
                    SaleEvent.Set();
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                }
                else//出货失败
                {
                    if (rsSale != null)
                    {
                        rsSale.workstatus = false;
                        try
                        {
                            XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                            xmlorder.orderno = rsSale.workNo;
                            xmlorder.datatime = DateTime.Now.ToString();
                            xmlorder.cdbno = "00";
                            xmlorder.cmno = "00";
                            xmlorder.status = "0";
                            xmlorder.ordertype = "0";
                            xmlorder.upserver = "0";
                            xmlorder.datatime = DateTime.Now.ToString();
                            xmlorder.Modify();
                        }
                        catch
                        {
                        }
                        SaleSendNetEvents.Enqueue(rsSale);
                    }
                    SaleEvent.Set();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.SaleL;
                    nui.huodao = strStation;
                    nui.Msg = "出货失败";
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "#" + strStation + "出货失败" + strcmds);
                    tools.insertLog("出线失败" + strcmds, Logtype.Sale);
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    byte[] bdata = Encoding.ASCII.GetBytes(TerminalNO + " saleline false");
                    ClS.sky.SendMessage(bdata, GKurl, 8234);
                }

            }
            /*
                     设置货道地址指令：
                    发送：头（%）+地址（1字节）+长度（字节）+类型是’A’+货道地址（1字节）+校验（1字节）+尾（%）；
                    1.返回：头（%）+地址（1字节）+长度（字节）+类型是’A’+执行状态（1字节，0-成功，其他表示失败）+校验（1字节）+尾（%）；


                    回收充电宝指令：

                    发送：头（%）+地址（1字节）+长度（字节）+类型是’K’+校验（1字节）+尾（%）；
                    回收仓打开时返回。
                    1.	返回：头（%）+地址（1字节）+长度（字节）+类型是’K’+执行状态（1字节，0-成功，其他表示失败）+校验（1字节）+尾（%）；

                    回收仓闭合时返回。
                    2.	返回：头（%）+地址（1字节）+长度（字节）+类型是’G’+执行状态（1字节，0-成功，其他表示失败）+校验（1字节）+尾（%）；
             */
            if (btype == 'A')
            {
                SaleEvent.Set();
                byte ftype = data[4];
                if (ftype == iSetAddr)
                {
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.SetAddrS;
                    nui.huodao = strStation;
                    nui.Msg = "地址更新成功";
                    try
                    {
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "地址更新成功");
                    }
                    catch
                    {

                    }
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                }
                else
                {
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.SetAddrF;
                    nui.huodao = strStation;
                    nui.Msg = "地址更新失败";
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "地址更新失败");
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                }
            }
            if (btype == 'K')
            {

                udplog.logs.Enqueue(DateTime.Now.ToString() + "回收回复：" + strcmds);
                byte ftype = data[4];
                if (ftype == 0)
                {
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.KCS;
                    nui.huodao = strStation;
                    nui.Msg = "回收仓打开";
               
                    //onehuanaction.HasLostHuodao.Clear();
           
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "回收仓打开成功");
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                }
                else
                {

                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.KCF;
                    nui.huodao = strStation;
                    nui.Msg = "回收仓打开失败";
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "回收仓打开失败");
                    ///
                    if (huishouTaskId != "")
                    {
                        ResponseHuiShouData one = new ResponseHuiShouData();
                        one.status = false;
                        one.terminal = TerminalNO;
                        one.taskId = huishouTaskId;
                        huishouEventDatas.Enqueue(one);
                        huishouTaskId = "";
                    }
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    SaleEvent.Set();//回收仓打开失败
                }

            }
            if (btype == 'G')
            {
                udplog.logs.Enqueue(DateTime.Now.ToString() + "回收执行回复：" + strcmds);
                byte ftype = data[4];
                if (ftype == 0)
                {
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.GCS;
                    nui.huodao = strStation;
                    nui.Msg = "回收仓关闭";
                    try
                    {
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "回收仓关闭");
                        Huishou_Num++;

                        //string updatelinenum = "update main_laidian set hsnum=" + Huishou_Num.ToString();
                        //laidian.DbHelperSQL.ExecuteSql(updatelinenum);
                     
                        //XDocument root = new XDocument("configdata.xml");
                        //XElement huishounum = (from customer1 in root.Descendants("huishou")
                        //                       select customer1).FirstOrDefault();//FirstOrDefault();
                        //huishounum.Value = Huishou_Num.ToString();
                        //root.Save("configdata.xml");
                    }
                    catch
                    {
                    }


                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    //需要回复服务端
                    if (huishouTaskId != "")
                    {
                        ResponseHuiShouData one = new ResponseHuiShouData();
                        one.status = true;
                        one.terminal = TerminalNO;
                        one.taskId = huishouTaskId;

                        huishouEventDatas.Enqueue(one);
                        huishouTaskId = "";
                    }
                  
                }
                else
                {
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.GCF;
                    nui.huodao = strStation;
                    nui.Msg = "回收仓关闭失败";
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "回收仓关闭失败"); 
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    if (huishouTaskId != "")
                    {
                        ResponseHuiShouData one = new ResponseHuiShouData();
                        one.status = false;
                        one.terminal = TerminalNO;
                        one.taskId = huishouTaskId;
                        huishouEventDatas.Enqueue(one);
                        huishouTaskId = "";
                    }
                   
                }
                SaleEvent.Set();
            }

        }
        /// <summary>
        /// 退出系统 关闭线程
        /// </summary>
        public void Close()
        {
            try
            {
                SetWindow.ShowWindow(SetWindow.FindWindow("Shell_TrayWnd", null), 9);
                SetWindow.ShowWindow(SetWindow.FindWindow("Button", null), 9);
                try
                {

                }
                catch
                {
                    Process[] GetP = Process.GetProcesses();
                    foreach (Process p in GetP)
                    {
                        if (p.ProcessName == "TestUpdate")
                        {
                            p.Kill();
                        }
                    }
                }
                output.Close();
                outputsale.Close();
                HasGetShopInfoFlag = false;
                PLinkServer.Abort();
                //查询云端是否有需要执行的任务

                PQueryServer.Abort();
                //回复云端任务执行情况线程

                PResponseEvent.Abort();
                //循环测试下位机

                PComm.Abort();

                PQuerySale.Abort();

                PSComm.Abort();


                PUploadLog.Abort();
                //补报线程停止
                //BuBaoThread.Abort();
                //加载补报线程停止
                //FindLostOrderThread.Abort();
                //wcdma.GprsDongEven t += new EventHandler(gprs_GprsDongEvent);
                //写日志线程
                /// <summary>
                /// 根据储物柜，和云端通信状态更新来电吧的状态
                /// </summary>
                //PUpdateTerminalStatus.Abort();
                PLogWrite.Abort();
                PQuerySale.Abort();


                PLinesUpdate.Abort();
                sendHuishouThread.Abort();

                PclientLineCheckOutFeedBack.Abort();

       
           
               

            }
            catch
            { }
            
        }
        /// <summary>
        /// 添加线函数
        /// </summary>
        /// <param name="Line1"></param>
        /// <param name="Line2"></param>
        /// <param name="Line3"></param>
        /// <param name="Line4"></param>
        public void AddLine(int Line1, int Line2, int Line3, int Line4)
        {
            try
            {

                XmlOperatorLine line1 = new XmlOperatorLine();
                line1.ID = "1";
                line1.Num = Line1;
                line1.Modify();
                XmlOperatorLine line2 = new XmlOperatorLine();
                line2.ID = "2";
                line2.Num = Line2;
                line2.Modify();
                XmlOperatorLine line3 = new XmlOperatorLine();

                line3.ID = "3";
                line3.Num = Line3;
                line3.Modify();
                XmlOperatorLine line4 = new XmlOperatorLine();

                line4.ID = "4";
                line4.Num = Line4;
                line4.Modify();

                s1 = Line1;
                s2 = Line2;
                s3 = Line3;
                s4 = Line4;
                ls1 = Line1;
                ls2 = Line2;
                ls3 = Line3;
                ls4 = Line4;
                //string upline = "update main_laidian set line1=" + Line1.ToString() + ",line2=" + Line2.ToString() + ",line3=" + Line3.ToString() + ",last_line1=" + Line1.ToString() + ",last_line2=" + Line2.ToString() + ",last_line3=" + Line3.ToString();
                //laidian.DbHelperSQL.ExecuteSql(upline);
                clientLineUpdate(Line1.ToString(), Line2.ToString(), Line3.ToString(), Line4.ToString());
            }
            catch
            {
            }
           
    
        }
        /// <summary>
        /// 更新线数量线程
        /// </summary>
        public void UpdataLineNum()
        {
            while (true)
            {
                try
                {

                   
                        int line1 = 0, line2 = 0, line3 = 0, line4 = 0;
                        XmlOperatorLine lines = new XmlOperatorLine();
                        List<XmlOperatorLine> xmls = lines.GetAll();
                       
                        foreach (XmlOperatorLine ls in xmls)
                        {
                            if (ls.ID == "1")
                            {
                                line1 = ls.Num;
                            }
                            if (ls.ID == "2")
                            {
                                line2 = ls.Num;
                            }
                            if (ls.ID == "3")
                            {
                                line3 = ls.Num;
                            }
                            if (ls.ID == "4")
                            {
                                line4 = ls.Num;
                            }
                        }

                        s1 = line1;
                        s2 = line2;
                        s3 = line3;
                        s4 = line4;
                        //DataSet dt = laidian.DbHelperSQL.Query("select * from main_laidian");
                        //if (dt.Tables[0].Rows.Count > 0)
                        //{
                        //    s1 = int.Parse(dt.Tables[0].Rows[0]["line1"].ToString());
                        //    s2 = int.Parse(dt.Tables[0].Rows[0]["line2"].ToString());
                        //    s3 = int.Parse(dt.Tables[0].Rows[0]["line3"].ToString());
                        //    //s4 = int.Parse(dt.Tables[0].Rows[0]["line4"].ToString());
                        //}
                        clientLineUpdate((line1+line2).ToString(),"0", line3.ToString(), line4.ToString());
                        //if (!Directory.Exists("line"))
                        //{
                        //    Directory.CreateDirectory("line");

                        //}
                       // File.Copy("line.xml", @"line\line.xml",true);
                   
                   
                }
                catch(Exception ess)
                {
                    try
                    {
                        File.Copy(@"linebak.xml", "line.xml", true);
                        if (s1 == 0 && s2 == 0 && s3 == 0 && s4 == 0)
                        {
                        }
                        else
                        {
                            try
                            {
                                XmlOperatorLine lines = new XmlOperatorLine();
                                List<XmlOperatorLine> xmls = lines.GetAll();
                                foreach (XmlOperatorLine ls in xmls)
                                {
                                    if (ls.ID == "1")
                                    {


                                        ls.Num = s1;
                                        ls.Modify();

                                    }
                                    if (ls.ID == "2")
                                    {


                                        ls.Num = s2;
                                        ls.Modify();

                                    }
                                    if (ls.ID == "3")
                                    {


                                        ls.Num = s3;
                                        ls.Modify();

                                    }
                                    if (ls.ID == "4")
                                    {


                                        ls.Num = s4;
                                        ls.Modify();

                                    }

                                }
                            }
                            catch (Exception ss)
                            {
                                udplog.logs.Enqueue(DateTime.Now.ToString() + "试图恢复line文件错误出现异常" + ss.Message);
                            }
                        }
                        

                    }
                    catch
                    {
                        udplog.logs.Enqueue("复制文件错误");
                    }
                    udplog.logs.Enqueue("线数量更新有错"+ess.Message);
                }
                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// 做循环测试
        /// </summary>
        public void Test()
        {
            Thread.Sleep(10000);
            while (true)
            {
                if (FinCWGFromCDB("01000000000000000000")!=-1)
                {
                    ServerWork = new newGprsEventStr();
                    ServerWork.OrderId = "0123456";
                    ServerWork.CDBNO = "01000000000000000000";
                    ServerWork.EventType = RequestType.QueryWork;
                    ServerWork.tys = new byte[] { };
                    
                    InPutSCmd(ServerWork);
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "开始一个租借过程");
                    Thread.Sleep(80000);
                }
                else
                {
                    newGprsEventStr ServerWork = new newGprsEventStr();
                    ServerWork.OrderId = "1234567890";
                    ServerWork.CDBNO = "";
                    ServerWork.EventType = RequestType.QueryWork;
                    ServerWork.tys = new byte[] { };

                    HuanSCmd(ServerWork);
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "开始一个归还过程");
                    Thread.Sleep(80000);
                }

            }
        }

        /// <summary>
        /// 将补报订单加载到队列中
        /// </summary>
        public void tryNetRep()
        {
            while (true)
            {  
                
                try
                {
                    if (wcdma.CommTime.AddMinutes(2) > DateTime.Now)
                    {
                        XmlOperatorOrder xmls = new XmlOperatorOrder();
                        List<XmlOperatorOrder> sds = xmls.GetDate("", "0");
                        if (sds != null)
                        {
                            foreach (XmlOperatorOrder xl in sds)
                            {
                                //如果保存的借出订单是未处理的，则要查询购物仓是否都有
                                if (xl.status == "0")
                                {
                                    try
                                    {
                                        //2分钟内仍旧还没报送 //没有船里
                                        if (xl.ordertype == "0" && DateTime.Parse(xl.datatime).AddMinutes(2) < DateTime.Now)
                                        {
                                            //CUWUGUI std = Cuwuguis[xl.cmno];
                                            bool isExist = false;//充电宝是否有借出标志
                                            for (int i = 1; i <= 30; i++)
                                            {
                                                CUWUGUI std = Cuwuguis[i.ToString("D2")];
                                                if (std.CDB != null && std.CDB.CDBNO == xl.cdbno)
                                                {
                                                    isExist = true;
                                                    break;
                                                }
                                            }

                                            if (!isExist)//如果充电宝不在本地，则说明已经借出
                                            {
                                                xl.status = "2";

                                            }
                                            else
                                                xl.status = "1";
                                           // xl.Modify();
                                            bubaolist.Enqueue(xl);


                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                else
                                {
                                    bubaolist.Enqueue(xl);
                                }
                            }
                        }
                    }
                }
                catch
                { }
            
                Thread.Sleep(5000);
            }

        }
        bool sendGK = false;//发送管控服务器标志

        /// <summary>
        /// 测试云端的握手情况，向云端播报来电吧状态
        /// 把校时和网络状态判断放到心跳包去进行
        /// </summary>
        public void TestSLink()
        {
            while (true)
            {
                StartEvent.WaitOne(40000, false);//暂时先看看是否租借与归还都停止上报？
                LaidianCommandModel.Builder laidianStatusReqBuilder = LaidianCommandModel.CreateBuilder();
                laidianStatusReqBuilder.SetMessageType(MessageType.LAIDIAN_STATUS_REQ);
                laidianStatusReqBuilder.SetTerminal(TerminalNO);
                LaidianStatusModel.Builder laidianStatusBuilder = LaidianStatusModel.CreateBuilder();
                laidianStatusBuilder.SetSmk("0");
                laidianStatusBuilder.SetTime(DateTime.Now.ToString());
                laidianStatusBuilder.SetType("0");
                laidianStatusBuilder.SetVd("0");
                laidianStatusBuilder.SetTerminal(TerminalNO);
                laidianStatusBuilder.SetTemp("30");
                laidianStatusBuilder.SetSmk("0");
                laidianStatusBuilder.SetLines(0,s1);
                laidianStatusBuilder.SetLines(1,s2);
                laidianStatusBuilder.SetLines(2,s3);
                laidianStatusBuilder.SetLines(3,s4);

                for (int i = 1; i <= Cuwuguis.Count; i++)
                {
                    CDBStatusModel.Builder cdbbuilder = CDBStatusModel.CreateBuilder();
                    CUWUGUI cwg = Cuwuguis[i.ToString("D2")];
                    if (cwg.CDB != null)
                    {
                        cdbbuilder.SetV(cwg.CDB.CDBNO);
                        cdbbuilder.SetVp(cwg.CDB.PowerDeep.ToString());
                        cdbbuilder.SetPosition(i.ToString());
                        cdbbuilder.SetVstatus(cwg.errorflag.ToString());
                        cdbbuilder.SetVtmp(cwg.CDB.temp.ToString());
                        cdbbuilder.SetVuse(cwg.CDB.UseCnt.ToString());
                        

                    }
                    else
                    {
                        cdbbuilder.SetV("00000000000000000000");
                        cdbbuilder.SetVp("00");
                        cdbbuilder.SetPosition(i.ToString());
                        cdbbuilder.SetVstatus(cwg.errorflag.ToString());
                        cdbbuilder.SetVtmp("0");
                        cdbbuilder.SetVuse("0");
                       
                    }
                    laidianStatusBuilder.SetCdbs(i - 1, cdbbuilder.Build());

                }
                laidianStatusReqBuilder.SetLaidianData(google.protobuf.Any.ParseFrom(laidianStatusBuilder.Build().ToByteString()));
         
                udplog.logs.Enqueue(DateTime.Now.ToString() + ":向云端汇报状态");
                Thread.Sleep(30000);//30秒钟发起一次链接
            }
        }

        /// <summary>
        /// 做一个测试借
        /// </summary>
        /// <returns></returns>
        public int FineCeshiCDB(newGprsEventStr ServerWork)
        {
            jieResult = 0;
            bool hasFind = false;//是否能够查找到系统
            cflag = 0;
            JEvent.Reset();
            StartEvent.Reset();//使巡查线程停止
            int hd = -1;
            tools.insertLog(ServerWork.OrderId + "号租借任务寻找充电宝" + ServerWork.CDBNO, Logtype.J);
            udplog.logs.Enqueue(ServerWork.OrderId + "号任务开始找充电宝" + ServerWork.CDBNO);
            if (ServerWork.CDBNO != null && ServerWork.CDBNO !="")
            {
                for (int i = 1; i <= Cuwuguis.Count; i++)
                {  
                    udplog.logs.Enqueue(ServerWork.OrderId + "号任务开始找充电宝" + i.ToString()+"号仓");
                    if (Cuwuguis[i.ToString("D2")].CDB != null && Cuwuguis[i.ToString("D2")].CDB.CDBNO == ServerWork.CDBNO && Cuwuguis[i.ToString("D2")].CWGStatus != CUWUGUISTATUS.Error && (Cuwuguis[i.ToString("D2")].CWGStatus == CUWUGUISTATUS.FullPower || Cuwuguis[i.ToString("D2")].CWGStatus == CUWUGUISTATUS.SetingPower))
                    {
                        udplog.logs.Enqueue(ServerWork.OrderId + "号任务开始找到充电宝在"  +i.ToString() + "号仓");
                        ServerWork.huodao = i;
                        ServerWork.CDBNO = Cuwuguis[i.ToString("D2")].CDB.CDBNO;

                        hasFind = true;
                        break;
                 
                        //ServerWork.EventType = RequestType.ZujieRequest;
                        //ServerWork.tys = new byte[] { };
                       
                      
                    }
                }
            }

            if (hasFind)//如果已经找到充电宝
            {
                udplog.logs.Enqueue(ServerWork.OrderId+"号任务已经找到一个充电宝"+ServerWork.huodao.ToString());
                tools.insertLog(ServerWork.OrderId + "号任务找到充电宝" + ServerWork.CDBNO +"在仓道"+ ServerWork.huodao.ToString(), Logtype.J);
                Jing = ServerWork.huodao;
                Thread.Sleep(2000);
                InPutSCmd(ServerWork);
                if (JEvent.WaitOne(600))
                {
                    hd = ServerWork.huodao;
                   
                }
            }
            else//返回失败
            {
                udplog.logs.Enqueue(ServerWork.OrderId + "号任务找不到一个充电宝" );
                tools.insertLog(ServerWork.OrderId + "号租借任务失败，找不到充电宝" + ServerWork.CDBNO, Logtype.J);
                ReponseWorkEvent evn = new ReponseWorkEvent();
                evn.workNo = ServerWork.OrderId;
                jieResult = -3;
                evn.workstatus = false;
                evn.CDB = ServerWork.CDBNO;
                SendNetEvents.Enqueue(evn);
            }

            return hd;
        }

        /// <summary>
        /// 查找来电宝放在哪个格子
        /// </summary>
        /// <param name="cdbno"></param>
        /// <returns></returns>
        public int FinCWGFromCDB(string cdbno)
        {
            bool search = false;
            int hd = -1;
            for (int i = 1; i <= Cuwuguis.Count; i++)
            {
                if (Cuwuguis[i.ToString("D2")].CDB!=null&&Cuwuguis[i.ToString("D2")].CDB.CDBNO == cdbno)
                {
                    //如果连续租借次数超过3次没有成功
                    //if (Cuwuguis[i.ToString("D2")].JieLost > 2)//Cuwuguis[i.ToString("D2")].JieLostTime > DateTime.Now.AddSeconds(-300) &&
                    search = true;
                    return i;
                    

                }
            }
            //如果第一个寻找的错误的
            if (search == false)
            {
                newGprsEventStr k = new newGprsEventStr();
                k.OrderId = "12323";
               return FineCeshiCDB(k);
            }

            return hd;

        }
        #region 租借/售卖任务云端APP
        /*---------------------------------租借或者收买需要分两步--------------------------------------
         *第一步：手机APP通过云端请求，需要在某个终端获取充电宝，来电吧终端请求
         *云端查看是否具有APP请求，有的话根据充电宝的状况生成一个二维码
         *第二步：当手机APP扫描后，会在云端生成一个对来电吧请求租借的指令，来电吧通过查看云端请求后，则开始出货。
         * 
         * -----------------------------------------------------------------------------------------*/
        /// <summary>
        /// 查看APP对云端的请求任务,并生成二维码
        /// </summary>
        public void QueryWorkRcode()
        {
            //如果有任务
            string Url =HttpUrl+ @"/cdt/clientStatusUpdate?";

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("type", "2");
            param.Add("terminal", TerminalNO);
            param.Add("time", tools.GetNowTime());
            param.Add("status", this.TerminalStatus.ToString());
            string strJoin = wcdma.GetRequest(Url, param);
            if (strJoin != "")
            {
            }
        }

        /// <summary>
        /// 归还充电宝时向云端反馈
        /// </summary>
        public void clientGiveBackFail(string DevPst)
        {
            //如果有任务
            string Url = HttpUrl + @"/cdt/clientGiveBackFail?";

            Dictionary<string, string> param = new Dictionary<string, string>();
     
            param.Add("terminal", TerminalNO);

            param.Add("devicePosition", DevPst);
            string strJoin = wcdma.GetRequest(Url, param);
            if (strJoin != "")
            {
               
            }
        }
        /// <summary>
        /// 向云端汇报补线数量
        /// </summary>
        public void clientLineUpdate(string line1, string line2, string line3, string line4)
        {
            //如果有任务
            string Url = HttpUrl + @"/cdt/clientLineUpdate?";

            Dictionary<string, string> param = new Dictionary<string, string>();

            param.Add("terminal", TerminalNO);
            param.Add("lineNum1", line1);
            param.Add("lineNum2", line2);
            param.Add("lineNum3", line3);
            param.Add("lineNum4", line4);
            string strJoin = wcdma.GetRequest(Url, param);
            if (strJoin != "")
            {

            }
        }
        /// <summary>
        /// 获取店铺信息
        /// </summary>
        /// <returns></returns>
        public void getLogo()
        {
            new Thread(() =>
           {
               try
               {
                   while (HasGetShopInfoFlag)
                   {
                       udplog.logs.Enqueue(DateTime.Now.ToString() + "开始下载LOGO");
                       string Url = HttpUrl + @"/cdt/shopMessageGetByDevice?";
                       Dictionary<string, string> param = new Dictionary<string, string>();
                       param.Add("terminal", TerminalNO);
                       string strJoin = wcdma.GetRequest(Url, param);

                       //{"status":true,"taskId":461,"time":"20141211172031","CDB":"000003001F0A0E000000","work":"121313"}
                       //strJoin = "{\"status\":true,\"taskId\":461,\"time\":\"20141211172031\",\"CDB\":\"000003001F0A0E000000\",\"work\":\"121313\"}";
                       if (strJoin != "")
                       {
                           try
                           {

                               HasGetShopInfoHour = DateTime.Now.Hour;
                               JObject jo = (JObject)JsonConvert.DeserializeObject(strJoin);
                               string devicePayImg = jo["shop"]["devicePayImg"].ToString();
                               string devicePayTxt = jo["shop"]["payStr"].ToString();
                               string shop_logo = jo["shop"]["logo"].ToString();
                               udplog.logs.Enqueue(DateTime.Now.ToString() + "下载LOGO成功");
                               //if (devicePayTxt != "")
                               {
                                   ShopPay = devicePayTxt;
                                   ShopPayImg = devicePayImg;
                               }


                               if (shop_logo != "")
                               {

                                   HttpWebRequest req;
                                   HttpWebResponse res = null;
                                   try
                                   {

                                       Bitmap img = null;

                                       System.Uri httpUrl = new System.Uri(shop_logo);
                                       req = (HttpWebRequest)(WebRequest.Create(httpUrl));
                                       req.Timeout = 8000; //设置超时值10秒
                                       req.UserAgent = "XXXXX";
                                       req.Accept = "XXXXXX";
                                       req.Method = "GET";
                                       res = (HttpWebResponse)(req.GetResponse());
                                       img = new Bitmap(res.GetResponseStream());//获取图片流 

                                       img.Save(System.Environment.CurrentDirectory + "\\" + TerminalNO + ".png");//shop_logo.png
                                       ShopLogo = shop_logo;
                                   }
                                   catch (Exception sds)
                                   {
                                       udplog.logs.Enqueue(DateTime.Now.ToString() + "下载LOGO" + sds.Message);
                                   }
                                   finally
                                   {
                                       res.Close();
                                   }

                               }
                               if (devicePayImg != "")
                               {

                                   HttpWebRequest req;
                                   HttpWebResponse res = null;
                                   try
                                   {

                                       Bitmap img = null;

                                       System.Uri httpUrl = new System.Uri(devicePayImg);
                                       req = (HttpWebRequest)(WebRequest.Create(httpUrl));
                                       req.Timeout = 8000; //设置超时值10秒
                                       req.UserAgent = "XXXXX";
                                       req.Accept = "XXXXXX";
                                       req.Method = "GET";
                                       res = (HttpWebResponse)(req.GetResponse());
                                       img = new Bitmap(res.GetResponseStream());//获取图片流 

                                       img.Save(System.Environment.CurrentDirectory + "\\" + TerminalNO + "-pay.png");//shop_logo.png
                                       ShopLogo = shop_logo;
                                   }
                                   catch (Exception ssss)
                                   {
                                       udplog.logs.Enqueue(DateTime.Now.ToString() + "下载LOGO" + ssss.Message);
                                   }
                                   finally
                                   {
                                       res.Close();
                                   }

                               }

                           }
                           catch
                           {
                           }
                           break;
                       }
                       else
                       {
                           udplog.logs.Enqueue(DateTime.Now.ToString() + "下载LOGO失败");
                       }
                       Thread.Sleep(30000);
                   }
               }
               catch
               {
               }

           }).Start();
        }
        /// <summary>
        /// 查看APP对云端的租借/售卖任务
        /// </summary>
        public void QueryWork()
        {
            while (true)
            {
                //如果有任务
                StartEvent.WaitOne(20000, false);//如果已经在处理一个租借任务了则需要暂时等待
                //从tcpchannel中taskqueue获取任务。
                            //if (jo["status"].ToString() == "true")
                            //{
                            //    EventUI nui = new EventUI();
                            //    nui.UIType = UIUpdateType.StatusUpdate;
                            //    // nui.huodao = strStation;
                            //    // oneorder.nowstatus = UIUpdateType.ChuHuoWait;
                            //    nui.Msg = "下载一个任务下载";
                            //    //回复云端任务执行状态


                            //    if (UpdateUIhandle != null)
                            //    {
                            //        UpdateUIhandle(this, nui);
                            //    }
                            //    if (jo["CDB"].ToString().Length == 20)
                            //    {
                            //        //需要插入下位机指令，同时记录该任务编号

                            //        ServerWork = new newGprsEventStr();
                         
                            //        ServerWork.OrderId = jo["taskId"].ToString();
                            //        ServerWork.CDBNO = jo["CDB"].ToString();
                            //        ServerWork.userHeadPic = jo["userHeadPic"].ToString();
                            //        ServerWork.userNikeName = jo["userNickName"].ToString();
                            //        CurrentUserName = jo["userNickName"].ToString();
                            //        CurrentUserPic = jo["userHeadPic"].ToString();
                            //        ServerWork.EventType = RequestType.ZujieRequest;
                            //        ServerWork.tys = new byte[] { };

                            //        udplog.logs.Enqueue("下载一个租借任务" + jo["CDB"].ToString());

                            //        EventUI nuiready = new EventUI();
                            //        nuiready.UIType = UIUpdateType.ChuHuoReady;
                            
                            //        if (ServerWork != null)
                            //            nuiready.Msg = ServerWork.userNikeName + "," + ServerWork.userHeadPic;
                            //        if (UpdateUIhandle != null)
                            //        {
                            //            UpdateUIhandle(this, nuiready);
                            //        }
                         
                            //        //查找充电宝，如果查找到则进入系统出货队列
                            //        FineCeshiCDB(ServerWork);
                            //    }
                            //    else
                            //    {

                            //    }
                            //}
                Thread.Sleep(2000);
            }
        }

       
        /// <summary>
        /// 查询是否有销售任务
        /// </summary>
        
        public void ThreadofSale()
        {
            while (true)
            {
                //如果有任务
               // SaleEvent.WaitOne(20000, false);//如果已经在处理一个租借任务了则需要暂时等待
                string Url = HttpUrl + @"/cdt/clientLineCheckOut?";
                //clientLineCheckOut?type=4&terminal=000110000008&time=20140526144203&status=1&work=1
                //string Url = "http://61.235.80.199:9000/TestWork.aspx?";
                Dictionary<string, string> param = new Dictionary<string, string>();
               
                param.Add("type", "4");
                param.Add("terminal", TerminalNO);
                param.Add("time","20140526152345");
                param.Add("work","1");

                string strJoin = wcdma.GetRequest(Url, param);
                //strJoin = "{'status':true,'time':'20140526152345','work':1}";
                //"{"status":true,"lineType":1,"taskId":1432,"time":"20140526152345"}
                //{status:true,time:20140526152345,work:1}strJoin = "{\"status\":true,\"time\":\"20140619192600\",\"work\":\"0000000004\",\"cdb\":\"00000000001\"}";
                if (strJoin != "")
                {
                    try
                    {
                        //strJoin = "{\"status\":true,\"time\":\"20140619192600\",\"work\":\"0000000004\",\"cdb\":\"0000000001\"}";
                        JObject jo = (JObject)JsonConvert.DeserializeObject(strJoin);
                        //ClS.ResponseLineData respdata = JsonMapper.ToObject<ClS.ResponseLineData>(strJoin);
                        if (jo["status"].ToString()=="true")
                        {
                            XmlOperatorLine lines = new XmlOperatorLine();
                            List<XmlOperatorLine> xmls = lines.GetAll();
                            int line = 0;
                            bool iphone = false;
                            if (jo["lineType"].ToString() == "1")//iphone5
                            {
                                iphone = true;
                            }

                            foreach (XmlOperatorLine ls in xmls)
                            {
                                if (ls.ID == jo["lineType"].ToString())
                                {
                                    line = ls.Num;
                                }
                                if (ls.ID == jo["lineType"].ToString())
                                {
                                    line = ls.Num;
                                }
                                if (ls.ID == jo["lineType"].ToString())
                                {
                                    line = ls.Num;
                                }
                                if (ls.ID == jo["lineType"].ToString())
                                {
                                    line = ls.Num;
                                }
                            }

                            if (line > 5)
                            {
                                
                                /*因为原来iphone4的线改成了iphone5所以这里的逻辑需要改动*/
                                
                                rsSale = new ReponseWorkEvent();
                                rsSale.workstatus = false;
                                rsSale.workNo= jo["taskId"].ToString();
                                rsSale.CDB = jo["lineType"].ToString();
                                rsSale.NeedToCheck = false;
                                rsSale.linetype =int.Parse(jo["lineType"].ToString()) - 1;
                                SaleLine(rsSale.linetype,1);
                            }
                            else
                            {

                                if (iphone)//如果是IP5  第一个桶没有线的话 需查找第二个桶
                                {
                                    udplog.logs.Enqueue(DateTime.Now.ToString()+"ip5线的第一个售线桶数量不足" +line.ToString()+",查找售线桶二");
                                    
                                    foreach (XmlOperatorLine ls in xmls)
                                    {
                                        if (ls.ID == "2")
                                        {
                                            line = ls.Num;
                                        }
                                       
                                    }
                                    if (line > 5)
                                    {

                                        /*因为原来iphone4的线改成了iphone5所以这里的逻辑需要改动*/

                                        rsSale = new ReponseWorkEvent();
                                        rsSale.workstatus = false;
                                        rsSale.workNo = jo["taskId"].ToString();
                                        rsSale.CDB = "2";
                                        rsSale.NeedToCheck = false;
                                        rsSale.linetype = 1;
                                        SaleLine(1, 1);
                                    }
                                    else
                                    {
                                        udplog.logs.Enqueue(DateTime.Now.ToString()+"ip5线的第二个售线桶数量不足" +line.ToString());
                                    }
                                }
                            }

                        }
                    }
                    catch(Exception ek)
                    {
                        udplog.logs.Enqueue("售线任务解析异常"+ek.Message);
                    }

                }
                else
                {
                    //EventUI nui = new EventUI();
                    //nui.UIType = UIUpdateType.StatusUpdate;
                    //// nui.huodao = strStation;
                    //// oneorder.nowstatus = UIUpdateType.ChuHuoWait;
                    //nui.Msg = "下载任务失败";
                    ////回复云端任务执行状态

                    udplog.logs.Enqueue("任务下载无响应");
                    //if (UpdateUIhandle != null)
                    //{
                    //    UpdateUIhandle(this, nui);
                    //}
                }
                Thread.Sleep(2000);
            }
        }

        public Queue<ReponseWorkEvent> SaleSendNetEvents = new Queue<ReponseWorkEvent>();

        /// <summary>
        /// 上传日志
        /// </summary>
        public void UpLoadLog()
        {
            string Url = HttpUrl + @"/cdt/clientUploadLogTaskGet?";

            while (true)
            {


                Dictionary<string, string> param = new Dictionary<string, string>();

                param.Add("terminal", TerminalNO);


                string strJoin = wcdma.GetRequest(Url, param);


                //strJoin = "{\"status\":true,\"time\":\"20140619192600\",\"work\":\"0000000004\",\"cdb\":\"00000000001\"}";
                if (strJoin != "")
                {
                    try
                    {
                        //strJoin = "{\"status\":true,\"time\":\"20140619192600\",\"work\":\"0000000004\",\"cdb\":\"0000000001\"}";
                        //ClS.GetLog respdata = JsonMapper.ToObject<ClS.GetLog>(strJoin);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(strJoin);
                        if (jo["result"].ToString() == "1"&& jo["logNames"].ToString().Length>0)
                        {
                            string[] lognames = jo["logNames"].ToString().Split(',');
                            AutoGetLog uploadfile = new AutoGetLog();
                            foreach(string file in lognames)
                            { 
                                udplog.logs.Enqueue(DateTime.Now.ToString() + "开始将日志" + file + "上传服务器");

                                byte[] bdata = Encoding.ASCII.GetBytes(TerminalNO + " ready for upload file ");
                                ClS.sky.SendMessage(bdata, GKurl, 8234);
                                try
                                {
                                    uploadfile.uploadfile("logsdata", TerminalNO + "-" + file, System.Environment.CurrentDirectory + "\\record\\" + file + ".txt");
                                }
                                catch
                                {
                                    udplog.logs.Enqueue(DateTime.Now.ToString() + "上传日志" + file + "上传服务器出现异常");
                                }
                            }
                        }

                    }
                    catch
                    {

                    }

                }

                Thread.Sleep(300000);
            }
        }
        
        
        /// <summary>
        /// 对云端售卖任务的执行状态回复
        /// </summary>
        public void clientLineCheckOutFeedBack()
        {
            string Url = HttpUrl + @"/cdt/clientLineCheckOutFeedBack?";

            while (true)
            {
                if (SaleSendNetEvents.Count > 0)
                {
                    ReponseWorkEvent rse = (ReponseWorkEvent)SaleSendNetEvents.Dequeue();
                    udplog.logs.Enqueue(TerminalNO+"售线状态汇报taskid为" + rse.workNo + " 状态" + rse.workstatus.ToString() );
              
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 对云端任务的执行状态回复
        /// </summary>
        public void ResponseWorkStatus()
        {
            string Url = HttpUrl+@"/cdt/clientCheckOutFeedBack?";
   
            while (true)
            {
                if (SendNetEvents.Count > 0)
                {
                    ReponseWorkEvent rse = (ReponseWorkEvent)SendNetEvents.Dequeue();
                   //param.Add("terminal", TerminalNO);
                   //param.Add("CDB", rse.CDB);
                   //param.Add("taskId", rse.workNo);
                }
                Thread.Sleep(1000);
            }
        }
      #endregion
        /// <summary>
        /// 收到GPRS信息的更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void gprs_GprsDongEvent(object sender, EventArgs e)
        {
     
        }

        /// <summary>
        /// 生成二维码图
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        public Bitmap creatQRCodeImage(string[]temp)
        {
            if (temp==null)
            {
                //msg.Text = "Data must not be empty.";
                return null;
            }

            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            //String encoding = @"http://www.laidiantech.com/?qrcode=" + temp[0];// +"&time=" + temp[1];
            String encoding = @"http://weixin.qq.com/r/kEz95XLEZS8arTaG9xmC?qrcode=" + temp[0];// +"&time=" + temp[1];
            if (encoding == "Byte")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            }
            else if (encoding == "AlphaNumeric")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC;
            }
            else if (encoding == "Numeric")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.NUMERIC;
            }
            try
            {
                int scale = Convert.ToInt16(6);
                qrCodeEncoder.QRCodeScale = scale;
            }
            catch (Exception exs)
            {
               // msg.Text = "Invalid size!" + ex.Message;
                return null;
            }
            try
            {
                int version = Convert.ToInt16(6);
                qrCodeEncoder.QRCodeVersion = version;
            }
            catch (Exception ex)
            {
               // msg.Text = "Invalid version !" + ex.Message;
            }

            string errorCorrect = "L";
            if (errorCorrect == "L")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;
            else if (errorCorrect == "M")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            else if (errorCorrect == "Q")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.Q;
            else if (errorCorrect == "H")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;
            try
            {
               // String ls_fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
               // String ls_savePath = Server.MapPath(".") + "/QRCodeImages/" + ls_fileName;
                //msg.Text = ls_savePath;

               return qrCodeEncoder.Encode(encoding);//.Save(ls_savePath);
               // ImageButton2.ImageUrl = "QRCodeImages/" + ls_fileName;
            }
            catch (Exception ex)
            {
                return null;// msg.Text = "Invalid version !" + ex.Message;
            }
        }
        /// <summary>
        /// 将收到的来自云端的命令插入队列头
        /// </summary>
        public void InPutSCmd(newGprsEventStr Gprs)
        {
            ComLinkLastTime = DateTime.Now;
            //将租借、售卖指令插入队列
            if (Gprs.EventType == RequestType.ZujieRequest)
            {
                int huodao = Gprs.huodao;// FinCWGFromCDB(Gprs.CDBNO);
                if (huodao != -1)
                {
                    newEventStr cmd = new newEventStr();
                    cmd.EventType = 1;
                    cmd.OrderId = 1;

                    byte[] cmdbyte = COM_Cmd.GCDBCmd(huodao);
                    cmd.tys = cmdbyte;
                    InPutOutHuodao = huodao;
                    oneorder = new JSLdbOrder();
                    oneorder.OrderNo =Gprs.OrderId;// ServerWork.OrderId;
                    oneorder.huodao = huodao;
                    oneorder.iTryCnt = 1;
                    oneorder.CDB = Gprs.CDBNO;
                    oneorder.status = false;
                    oneorder.nowstatus = UIUpdateType.StatusUpdate;
                    oneorder.scmd = cmdbyte;
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "插入出货指令" + Gprs.CDBNO+"出货仓道"+huodao.ToString());
                    SendComCmds.Enqueue(cmd);
                   
                    prtJie = 0;

                }
            }
           

        }
        /// <summary>
        /// 重启指令
        /// </summary>
        public void InPutResetCmd(newGprsEventStr Gprs)
        {
            //ComLinkLastTime = DateTime.Now;
            //将租借、售卖指令插入队列
            if (Gprs.EventType == RequestType.Reset)
            {

                //if (Gprs.EventType == RequestType.QueryError)
                {


                    newEventStr cmd = new newEventStr();
                    cmd.EventType = 4;
                    cmd.OrderId = 4;
                    byte[] cmdbyte = COM_Cmd.ResetCDCmd(int.Parse(Gprs.CDBNO)); 
                    cmd.tys = cmdbyte;
                    oneorder = new JSLdbOrder();
                    oneorder.OrderNo = "";
                    oneorder.iTryCnt = 1;
                    oneorder.CDB = "";
                    oneorder.status = false;
                    oneorder.scmd = cmdbyte;
                    SendComCmds.Enqueue(cmd);

                }
            }


        }
        /// <summary>
        /// 查询错误指令
        /// </summary>
        public void InPutQueryErrorCmd(newGprsEventStr Gprs)
        {
            //ComLinkLastTime = DateTime.Now;
            //将租借、售卖指令插入队列
            if (Gprs.EventType == RequestType.QueryError)
            {
                newEventStr cmd = new newEventStr();
                cmd.EventType = 3;
                cmd.OrderId = 3;
                byte[] cmdbyte = COM_Cmd.QueryCDCmd(int.Parse(Gprs.CDBNO));
                cmd.tys = cmdbyte;
                oneorder = new JSLdbOrder();
                oneorder.OrderNo = "";
                oneorder.iTryCnt = 1;
                oneorder.CDB = "";
                oneorder.status = false;
                oneorder.scmd = cmdbyte;
                SendComCmds.Enqueue(cmd);


            }


        }
        
        
        /// <summary>
        /// 寻找一个不同于原来的存物柜
        /// </summary>
        /// <param name="OrigID"></param>
        /// <returns></returns>
        public int FindNullCUWUGUI(int OrigID)
        {
            int huodao = -1;
            for (int i = 1; i <= Cuwuguis.Count; i++)
            {
                if (Cuwuguis[i.ToString("D2")].CWGCommStatus == CWGCOMMSTATUS.OK && Cuwuguis[i.ToString("D2")].CWGStatus == CUWUGUISTATUS.None && i != OrigID)
                {
                    if (Cuwuguis[i.ToString("D2")].LastLostTime >DateTime.Now.AddMinutes(-1) && Cuwuguis[i.ToString("D2")].HasLostCnt > 2)
                        continue;//如果1分钟内连续3次归还失败，则选择下一个端口
                    else
                        return i;
                }
               

            }
            return huodao;
 
        }

        /// <summary>
        /// 寻找一个不同于原来的存物柜
        /// </summary>
        /// <param name="OrigID"></param>
        /// <returns></returns>
        public int FindNullCUWUGUI3(int OrigID)
        {
            udplog.logs.Enqueue(DateTime.Now.ToString()+"用户操作开仓按钮");

            List<int> khs = new List<int>();//可还货道
            //if (onehuanaction.ActionTime.AddSeconds(30) < DateTime.Now&&onehuanaction.LostCnt<3)
            //{ 
            //}

            bool isOther= false;//本次归还是否为另外一个人
            if (!onehuanaction.isEx)//如果本次租借的间隔距离上次已经超过了2分钟，则认为可能是第二个人
            {
                onehuanaction.ActionStartTime = DateTime.Now;
                onehuanaction.firstHuodao = -1;
                isOther = true;
                onehuanaction.HasLostHuodao.Clear();
            }
            else
            {
                isOther = false;
            }

            if (isOther == false)
            {
                if (onehuanaction.HasLostHuodao.Count > 2)
                { 
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "用户连续操作归还失败次数超过三次，将打开回收仓");
                    if (MonitorUpdateTime > DateTime.Now.AddMinutes(-2))
                    {
                       
                        //打开回收仓
                        //onehuanaction.ActionStartTime = DateTime.Now;
                        //isOther = true;
                        onehuanaction.HasLostHuodao.Clear();
                        onehuanaction.firstHuodao = -1;
                        OpenDoor();
                        return 0;

                    }
                    else
                    {

                        return -2;
                    }
                   
                }
            }

            int huodao = -1;
            HuanEvent.Reset();
            StartEvent.Reset();//使巡查线程停止

            //先将可用的通道全部拿出来
            for (int i = 1; i <= Cuwuguis.Count; i++)
            {
                if (Cuwuguis != null && Cuwuguis[i.ToString("D2")].CWGCommStatus == CWGCOMMSTATUS.OK && Cuwuguis[i.ToString("D2")].CWGStatus == CUWUGUISTATUS.None && i != OrigID)
                {
                    if (Cuwuguis[i.ToString("D2")].LastLostTime > DateTime.Now.AddSeconds(-60) && Cuwuguis[i.ToString("D2")].HasLostCnt > 2)
                    {
                        huodao = -2;
                        continue;//如果1分钟内连续3次归还失败，则选择下一个端口
                    }
                    else
                    {
                     
                        khs.Add(i);
                        
               
                    }
                }


            }
             
           


            int currentIndex = -1;
            if (khs.Count > 0)
            {

                //如果是第一次
                if (onehuanaction.firstHuodao == -1)
                {
                    Random sf = new Random();
                    currentIndex = sf.Next(0, khs.Count - 1);
                    if (currentIndex == khs.Count - 1)
                    {
                        currentIndex = 0;
                    }

                }
                else
                {
                    for (int idt = 0; idt < khs.Count; idt++)
                    {
                        if (khs[idt] == onehuanaction.firstHuodao)
                        {
                            if (idt == khs.Count - 1)
                            {
                                currentIndex = 0;
                            }
                            else
                            {
                                currentIndex = idt + 1;

                            }
                        }
                    }
                }
                //如果空闲
                int hasJ = currentIndex;

                //2015-10-12 构建一个环形数组，从随机位置开始轮询
                for (int idt = 0; idt < khs.Count; idt++)
                {
                    //hasJ++;
                    //清除掉

                    //Cuwuguis[i.ToString("D2")].LastLostTime = DateTime.Now;
                    //Cuwuguis[i.ToString("D2")].HasLostCnt = 0;
                    //如果在30秒内有同一次归还失败 则有可能是同一个人
                    int id = khs[hasJ];

                    if (hasJ >= currentIndex && hasJ < (khs.Count - 1))
                    {
                        hasJ++;
                    }
                    else
                    {
                        if (hasJ == (khs.Count - 1))
                        {
                            hasJ = 0;
                        }
                        else
                        {
                            hasJ++;
                        }
                    }


                    //if(onehuanaction.HasLostHuodao.Count>0&&onehuanaction.HasLostHuodao.Contains(id))
                    //{
                    //如果上一次失败的货道和轮询到的id是一样，并且还有其他货道可选，则跳过，否则就直接选这个
                    //if (onehuanaction.HasLostHuodao.Contains(id) && khs.Count > hasJ)
                    //{
                    //    continue;
                    //}
                    //else
                    {


                        ComLinkLastTime = DateTime.Now;
                        //将归还指令插入队列
                        Hing = id;
                        newEventStr cmd = new newEventStr();
                        cmd.EventType = 0;
                        cmd.OrderId = 1;
                        byte[] cmdbyte = COM_Cmd.ReturnCDBCmd(id);

                        oneorder = new JSLdbOrder();
                        oneorder.OrderNo = "1232643";
                        oneorder.iTryCnt = 1;
                        oneorder.status = false;
                        oneorder.scmd = cmdbyte;
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求归还仓号:" + id.ToString("D2"));
                        SendComCmds.Enqueue(cmd);
                    }
                    //}


                    onehuanaction.firstHuodao = id; ;
                    if (HuanEvent.WaitOne(2000))//如果已经收到了下位机对归还指令的响应
                    {
                        onehuanaction.HasLostHuodao.Enqueue(id);

                        huodao = id;
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "：执行归还H仓号:" + id.ToString("D2"));
                        break;
                    }
                    else
                    {
                        //break;
                        //Thread.Sleep(300);
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "：执行归还H仓号:" + id.ToString("D2") + "下位机无H响应");
                    }
                }
            }
            if (huodao == -1 || huodao == -2)
            {
                HuanEvent.Set();
                StartEvent.Set();
            }

            return huodao;

        }
        /// <summary>
        /// 寻找一个不同于原来的存物柜
        /// </summary>
        /// <param name="OrigID"></param>
        /// <returns></returns>
        public int FindNullCUWUGUI2(int OrigID)
        {
          
            int huodao = -1;
            HuanEvent.Reset(); 
            StartEvent.Reset();//使巡查线程停止
            for (int i = 1; i <= Cuwuguis.Count; i++)
            {
                if (Cuwuguis!=null&&Cuwuguis[i.ToString("D2")].CWGCommStatus == CWGCOMMSTATUS.OK && Cuwuguis[i.ToString("D2")].CWGStatus == CUWUGUISTATUS.None && i != OrigID)
                {
                    if (Cuwuguis[i.ToString("D2")].LastLostTime > DateTime.Now.AddSeconds(-60) && Cuwuguis[i.ToString("D2")].HasLostCnt > 1)
                    {
                        huodao = -2;
                        continue;//如果1分钟内连续3次归还失败，则选择下一个端口
                    }
                    else
                    {
                        //清除掉
                        //Cuwuguis[i.ToString("D2")].LastLostTime = DateTime.Now;
                        //Cuwuguis[i.ToString("D2")].HasLostCnt = 0;
                        //else
                        {
                            ComLinkLastTime = DateTime.Now;
                            //将归还指令插入队列

                            Hing = i;
                            newEventStr cmd = new newEventStr();
                            cmd.EventType = 0;
                            cmd.OrderId = 1;
                            byte[] cmdbyte = COM_Cmd.ReturnCDBCmd(i);
                            cmd.tys = cmdbyte;

                            oneorder = new JSLdbOrder();
                            oneorder.OrderNo = "1232643";
                            oneorder.iTryCnt = 1;
                            oneorder.status = false;
                            oneorder.scmd = cmdbyte;
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求归还仓号:" + i.ToString("D2"));
                            SendComCmds.Enqueue(cmd);
                        }
                        if (HuanEvent.WaitOne(1000))//如果已经收到了下位机对归还指令的响应
                        {
                            huodao = i;
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "：执行归还H仓号:" + i.ToString("D2"));
                            break;
                        }
                        else
                        {
                            //break;
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "：执行归还H仓号:" + i.ToString("D2")+"下位机无H响应");
                        }

                    }
                }


            }
            if (huodao == -1 || huodao == -2)
            {
                HuanEvent.Set();
                StartEvent.Set();
            }
 
            return huodao;

        }
        /// <summary>
        /// 请求归还充电宝命令插入队列头
        /// </summary>
        public void HuanSCmd(newGprsEventStr Gprs)
        {

            StartEvent.WaitOne(6000);
            ComLinkLastTime = DateTime.Now;
            //将租借、售卖指令插入队列
            if (Gprs.EventType == RequestType.GuiHuanRequest)
            {
                int huodao = FindNullCUWUGUI(-1);
                if (huodao != -1)
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求归还仓号:" + huodao.ToString("D2"));
                    newEventStr cmd = new newEventStr();
                    cmd.EventType = 0;
                    cmd.OrderId = 1;
                    byte[] cmdbyte = COM_Cmd.ReturnCDBCmd(huodao);
                    cmd.tys = cmdbyte;

                    oneorder = new JSLdbOrder();
                    oneorder.OrderNo = Gprs.OrderId;
                    oneorder.iTryCnt = 1;
                    oneorder.status = false;
                    oneorder.scmd = cmdbyte;
                    SendComCmds.Enqueue(cmd);
                }
                else
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求归还失败,找不到可以仓");
                }
            }


        }
        
        /// <summary>
        /// 循环发送命令归还与租借线程
        /// </summary>
        public void ThreadOfSendComCmd()
        {
            while (true)
            {
                if (SendComCmds.Count > 0 && output.IsWorkFlag == false)//如果云端有任务，则需要暂停8秒，等待任务执行
                {

                    //InPutCCmd();
                    StartEvent.Reset();//停止查询线程


                    Com.newEventStr cmdevent = (newEventStr)SendComCmds.Dequeue();

                    string strcmds = "";
                    foreach(byte bt in cmdevent.tys)
                    {
                        strcmds += bt.ToString("X2") + " ";
                    }
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "：租借或出仓:" + strcmds + "任务状态" + oneorder.status.ToString() + oneorder.iTryCnt.ToString());
                    while (oneorder.iTryCnt <= 1 && oneorder.status == false)
                    {
                        if ((oneorder.nowstatus == UIUpdateType.ChuHuoING || oneorder.nowstatus == UIUpdateType.ChuHuoWait) && cmdevent.EventType == 1)
                        {
                           // udplog.logs.Enqueue(DateTime.Now.ToString() + "：正在处理中无法完成此任务:" + strcmds);
                            Thread.Sleep(100);
                            continue;//等待

                        }

                        EventUI nui = new EventUI();

                        if (cmdevent.EventType == 0)
                        {
                            nui.UIType = UIUpdateType.StartGH;
                            nui.Msg = "请求归还:" + strcmds;
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求归还:" + strcmds);

                        }
                        else if (cmdevent.EventType == 1)
                        {
                            jieMonitor.Start();
                            jieResult = 0;
                            nui.UIType = UIUpdateType.StartZJ;
                            nui.Msg = "请求租借:" + strcmds;
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借:" + strcmds);
                        }
                        else if (cmdevent.EventType == 2)
                        {
                            nui.UIType = UIUpdateType.StartGM;
                            nui.Msg = "购买指令:" + strcmds;
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "：购买指令:" + strcmds);
                        }
                        if (UpdateUIhandle != null)
                        {
                            UpdateUIhandle(this, nui);
                        }
                        try
                        {
                            if (!output.Online)
                            {
                                output.Open();
                            }
                        }
                        catch
                        {
                        }


                        udplog.logs.Enqueue(DateTime.Now.ToString() + "：发送指令:" + strcmds);
                      
                        if (!output.SendByte(cmdevent.tys))//需要等待回复的
                        {
                            //2015-10-15修改，如果遇到发送成不成功时 则重发一次
                            Thread.Sleep(200);
                            //output.SendByte(cmdevent.tys);
                            if (cmdevent.EventType == 1)
                            {
                            }
                            else
                            {
                                udplog.logs.Enqueue(DateTime.Now.ToString() + "：成功发送指令:" + strcmds);
                            }

                            nui = new EventUI();
                            nui.UIType = UIUpdateType.NoResponse;
                            nui.Msg = cmdevent.tys[1].ToString("D2") + "无响应";
                            nui.huodao = cmdevent.tys[1].ToString("D2");
                            if (UpdateUIhandle != null)
                            {
                                UpdateUIhandle(this, nui);
                            }
                            oneorder.iTryCnt=5;
                        }
                        Thread.Sleep(1000);
                        // oneorder.iTryCnt++;

                    }

  
                    //如果是超时失败的
                    if (oneorder.status == false && oneorder.iTryCnt >= 5)
                    {
                        //如果租借失败
                        //oneorder.status = false;
                        //if ()
                        {
                            StartEvent.Set();
                            string STR = cmdevent.tys[1].ToString("D2");
                            CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[STR];//更新储物柜   

                            cuwugui.CommTime = DateTime.Now;

                            cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;
                            if (cmdevent.EventType == 1)//借指令
                            {
                                try
                                {
                                    if (ServerWork != null)
                                    {
                                        //XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                                        //xmlorder.orderno = ServerWork.OrderId;
                                        //xmlorder.status = "1";
                                        //xmlorder.Modify();
                                    }
                                }
                                catch
                                {
                                }

                                //ReponseWorkEvent evn1 = new ReponseWorkEvent();
                                //evn1.workNo = oneorder.OrderNo;

                                //evn1.workstatus = false;

                                //evn1.CDB = oneorder.CDB; 
                                //SendNetEvents.Enqueue(evn1);
                                udplog.logs.Enqueue(oneorder.OrderNo + "租借任务超时无返回");

                                cuwugui.JieLost++;
                                cuwugui.JieLostTime = DateTime.Now;
                            }
                            if (cmdevent.EventType == 0)
                            {
                                cuwugui.HasLostCnt++;
                                cuwugui.LastLostTime = DateTime.Now;
                            }
                            Cuwuguis[STR] = cuwugui;//将储物柜保存回去
                            //ResponseWorkStatus();
                            EventUI nui = new EventUI();
                            nui.UIType = UIUpdateType.ChuHuoL;
                            nui.Msg = "出货失败";
                            //回复云端任务执行状态
                            ReponseWorkEvent evn = new ReponseWorkEvent();

                            evn.workNo = oneorder.OrderNo;
                            evn.CDB = oneorder.CDB;
                            evn.workstatus = false;
                            SendNetEvents.Enqueue(evn);
                            if (UpdateUIhandle != null)
                            {
                                UpdateUIhandle(this, nui);
                            }
                        }
                    }

                }
                else
                {
                    Thread.Sleep(500);
                }
                //output.TransFlag.Reset();
               
            }
        }
        public bool bSCmd = true;
        /// <summary>
        /// 循测储物柜线程
        /// </summary>
        public void ThreadOfInComCmd()
        {
            while (true)
            {
                if (Test_Mode==0)//如果云端有任务，则需要暂停8秒，等待任务执行
                {
                    InPutCCmd();
                    //Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 测试模式
        /// </summary>
        public void TestModeFun()
        {
            if (Test_Mode == 1)//如果已经处于测试模式
            {
                Test_Mode = 0;//停止测试模式

            }
            else
            {
                Test_Mode = 1;//开始测试模式
                cflag = 1;

               // if (PComm.ThreadState == ThreadState.Running)
                    try
                    {
                        PComm.Suspend();//挂起查询进程
                    }
                    catch { }
                BCEvent.Set();
                Thread.Sleep(1000);
            }

            new Thread(() =>
            {
                try
                {
                    while (Test_Mode == 1)
                    {
                        for (int i = 1; i <= Cuwuguis.Count; i++)
                        {


                            if (Test_Mode==0||Cuwuguis[i.ToString("D2")].CDB == null || Cuwuguis[i.ToString("D2")].CWGStatus == CUWUGUISTATUS.Error || Cuwuguis[i.ToString("D2")].CWGCommStatus == CWGCOMMSTATUS.Error)
                            {
                                Thread.Sleep(500);
                                continue;
                            }
                            try
                            {
                                Cuwuguis[i.ToString("D2")].AutoTestCnt++;
                            }
                            catch
                            { }
                            //EventUI nui = new EventUI();
                            //nui.UIType = UIUpdateType.ChuHuoING;
                            //nui.huodao = i.ToString();
                            //nui.Msg = i.ToString() + "仓准备出货";
                            ////udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                            //UpdateUIhandle(this, nui);
                            byte[] cmdbyte = COM_Cmd.GCDBCmd(i);
                            BCEvent.Reset();
                            //Thread.Sleep(100);
                            if (!output.SendByte(cmdbyte))//需要等待回复的
                            {
                                Thread.Sleep(500);
                                //output.SendByte(cmdbyte);
                                //EventUI nui = new EventUI();
                                //nui.UIType = UIUpdateType.NoResponse;
                                //nui.huodao = i.ToString();
                                //nui.Msg = "出货" + i.ToString() + "仓无反应" ;
                                //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                                //UpdateUIhandle(this, nui);
                                //BCEvent.Set();
                            }
                            BCEvent.WaitOne(8000);
                        }

                        EventUI nui1 = new EventUI();
                        nui1.UIType = UIUpdateType.HCBtnEnable;

                        nui1.Msg = "本轮出货结束";
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                        UpdateUIhandle(this, nui1);
                        Thread.Sleep(2000);
                        InPutCCmd();
                    }
                }
                catch
                {
                    EventUI nui1 = new EventUI();
                    nui1.UIType = UIUpdateType.HCBtnEnable;

                    nui1.Msg = "本轮出货结束";
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                    UpdateUIhandle(this, nui1);
                }
            }).Start();

            if (PComm.ThreadState == System.Threading.ThreadState.Suspended)
                PComm.Resume();//挂起查询进程

        }
        /// <summary>
        /// 将本地巡检的命令插入队列,命令类型为Q
        /// 2015-10-26修改，加入一个重启指令，每天11时重启各仓道
        /// </summary>
        public void InPutCCmd()
        {
            if (TerminalStatus == 0)
            {
                //定义主控单片机的地址为01 货道柜单片机为02
                //if (DateTime.Now.Minute%5 == 0 && RsetCDFlag == false)
                //{
                //        if(StartEvent.WaitOne(30000, false)) ;//查看其他线程是否有发送任务，有的话则停止
                //        {
                //            StartEvent.Set();
                //        }
                if (NeedSendReBoot.Count > 0)
                {
                 
                    if (NeedSendReBoot.First().ldate.AddMinutes(-1) < DateTime.Now)
                    {
                        LoseCD lscd= NeedSendReBoot.Dequeue();
                        try
                        {
                            Cuwuguis[lscd.cdn.ToString("D2")].isReset = false;


                            Cuwuguis[lscd.cdn.ToString("D2")].ResetTime = DateTime.Now;
                            //Cuwuguis[lscd.cdn.ToString("D2")].ResetCnt = Cuwuguis[lscd.cdn.ToString("D2")].ResetCnt+1;
                        }
                        catch(Exception ews)
                        {
                            udplog.logs.Enqueue(DateTime.Now.ToString() + "发送重启指令" + ews.Message);
                        }
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "发送重启指令" +lscd.cdn.ToString());
                        byte[] cmdbyte = COM_Cmd.ResetCDCmd(lscd.cdn);
                        if (!output.SendByte(cmdbyte))
                        {
                            if (!output.SendByte(cmdbyte))
                            {

                            }
                        };//发送后即output.TransFlag启动WaitOne()使整个
                        Thread.Sleep(100);
                    }
                }


                for (int i = 1; i <= icnt; i++)//循环插入查询指令
                {
                    if (StartEvent.WaitOne(30000, false)) ;//查看其他线程是否有发送任务，有的话则停止
                    {
                        StartEvent.Set();
                    }
                    Thread.Sleep(200);
                    byte[] cmdbyte = COM_Cmd.QueryCDCmd(i);
                    string strcmds = COM_Cmd.byteToString(cmdbyte);
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "发送" + strcmds);

                    if (!output.SendByte(cmdbyte))
                    {
                        if (!output.SendByte(cmdbyte))
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
                                //udplog.logs.Enqueue(DateTime.Now.ToString() + "通信超时" + Cuwuguis[i.ToString("D2")].TestCnt.ToString() + "次,总故障" + Cuwuguis[i.ToString("D2")].HasLostCnt.ToString());
                                if (UpdateUIhandle != null)
                                {
                                    UpdateUIhandle(this, nui);
                                }
                            }
                        }
                    };//发送后即output.TransFlag启动WaitOne()使整个

                    Thread.Sleep(200);
                }



            }
        }
        /// <summary>
        /// 循环补货需要的信号灯
        /// </summary>
        public ManualResetEvent BCEvent = new ManualResetEvent(false);
        public bool stop = false; 
        /// <summary>
        /// 出货，弹出充电宝
        /// </summary>
        /// <param name="ni"></param>
        public void CH(int ni)
        {
            cflag = 1;
            stop = false; 
            PComm.Suspend();//挂起查询进程
            BCEvent.Set();
            Thread.Sleep(1000);
            if (ni == 0)
            {
                new Thread(()=>{
                    try
                    {
                        for (int i = 1; i <= Cuwuguis.Count; i++)
                        {

                            if (stop)
                            {
                                break;
                            }
                            if (Cuwuguis[i.ToString("D2")].CDB == null || Cuwuguis[i.ToString("D2")].CWGCommStatus == CWGCOMMSTATUS.Error)
                            {
                                continue;
                            }

                            EventUI nui = new EventUI();
                            nui.UIType = UIUpdateType.ChuHuoING;
                            nui.huodao = i.ToString();
                            nui.Msg = i.ToString() + "仓准备出货";
                            //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                            UpdateUIhandle(this, nui);
                            byte[] cmdbyte = COM_Cmd.GCDBCmd(i);
                            BCEvent.Reset();
                            //Thread.Sleep(100);
                            if (!output.SendByte(cmdbyte))//需要等待回复的
                            {
                                Thread.Sleep(100);
                                output.SendByte(cmdbyte);

                            }
                            BCEvent.WaitOne(6000);
                        }
                        PComm.Resume();
                        EventUI nui1 = new EventUI();
                        nui1.UIType = UIUpdateType.HCBtnEnable;

                        nui1.Msg = "本轮出货结束";
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                        UpdateUIhandle(this, nui1);
                    }
                    catch {
                        EventUI nui1 = new EventUI();
                        nui1.UIType = UIUpdateType.HCBtnEnable;

                        nui1.Msg = "本轮出货结束";
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                        UpdateUIhandle(this, nui1);
                    }
            }).Start();
            }
            else
            {
                new Thread(() =>
                {
                    if (ni >= 1 && ni <= icnt)
                    {
                        EventUI nui = new EventUI();
                        nui.UIType = UIUpdateType.ChuHuoING;
                        nui.huodao = ni.ToString();
                        nui.Msg =  ni.ToString() + "仓准备出货";
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                        UpdateUIhandle(this, nui);
                        byte[] cmdbyte = COM_Cmd.GCDBCmd(ni);
                        BCEvent.Reset();
                       // Thread.Sleep(100);
                        if (!output.SendByte(cmdbyte))//需要等待回复的
                        {
                            Thread.Sleep(100);
                            output.SendByte(cmdbyte);

                        }
                        BCEvent.WaitOne(6000);
                    }
                    PComm.Resume();
                    EventUI nui1 = new EventUI();
                    nui1.UIType = UIUpdateType.HCBtnEnable;

                    nui1.Msg = "本次出货结束";
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                    UpdateUIhandle(this, nui1);
                }).Start();
            }
           
        }
        /// <summary>
        /// 补货，自动循环出仓
        /// </summary>
        public void BU(int ni)
        {
            stop = false;
            PComm.Suspend();//挂起查询进程
            BCEvent.Set();
            Thread.Sleep(1000);
            if (ni == 0)
            {
                new Thread(() =>
                {
                    try
                    {
                        for (int i = 1; i <= Cuwuguis.Count; i++)
                        {
                            Thread.Sleep(100);
                            if (stop)
                            {
                                break;
                            }
                            if (Cuwuguis[i.ToString("D2")].CDB != null || Cuwuguis[i.ToString("D2")].CWGCommStatus == CWGCOMMSTATUS.Error)
                                continue;
                         
                            EventUI nui = new EventUI();
                            nui.UIType = UIUpdateType.PutInting;
                            nui.huodao = i.ToString();
                            nui.Msg = i.ToString() + "仓准备归还";
                            //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                            UpdateUIhandle(this, nui);
                            byte[] cmdbyte = COM_Cmd.ReturnCDBCmd(i);
                            
                           
                            BCEvent.Reset();
                           
                            if (!output.SendByte(cmdbyte))//需要等待回复的
                            {
                                Thread.Sleep(100);
                                output.SendByte(cmdbyte);
                    
                            }
                            BCEvent.WaitOne(9000);
                        }
                        PComm.Resume();
                        EventUI nui1 = new EventUI();
                        nui1.UIType = UIUpdateType.HCBtnEnable;

                        nui1.Msg = "本轮归还结束";
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                        UpdateUIhandle(this, nui1);
                    }
                    catch
                    {
                        EventUI nui1 = new EventUI();
                        nui1.UIType = UIUpdateType.HCBtnEnable;

                        nui1.Msg = "本轮归还结束";
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                        UpdateUIhandle(this, nui1);
                    }
                }).Start();
            }
            else
            {
                new Thread(() =>
                {
                    if (ni >= 1 && ni <= icnt)
                    {
                        EventUI nui = new EventUI();
                        nui.UIType = UIUpdateType.NoResponse;
                        nui.huodao = ni.ToString();
                        nui.Msg =  ni.ToString() + "仓准备开始";
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                        UpdateUIhandle(this, nui);
                        byte[] cmdbyte = COM_Cmd.ReturnCDBCmd(ni);
                        BCEvent.Reset();
                       
                        if (!output.SendByte(cmdbyte))//需要等待回复的
                        {
                            Thread.Sleep(100);
                            output.SendByte(cmdbyte);
                            //nui.UIType = UIUpdateType.NoResponse;
                            //nui.huodao = ni.ToString();
                            //nui.Msg =  ni.ToString() + "仓无反应";
                            ////udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                            //UpdateUIhandle(this, nui);
                            //BCEvent.Set();
                        }
                        BCEvent.WaitOne(9000);
                    }
                    PComm.Resume();
                    EventUI nui1 = new EventUI();
                    nui1.UIType = UIUpdateType.HCBtnEnable;

                    nui1.Msg = "本次归还结束";
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "检测" + i.ToString() + "储物柜" + strcmds);
                    UpdateUIhandle(this, nui1);
                }).Start();
            }
           
        }
        /// <summary>
        /// 端口的返回事件处理
        /// 1、更新储物柜的状态
        /// 2、如果是对执行任务的回复，则需要调用GPRS模块的状态回复函数。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void output_DongEvent(object sender, EventArgs e)
        {

            //EventUI nuiv = new EventUI();
            //nuiv.UIType = UIUpdateType.StatusUpdate;
           
           
          
            Com.newEventStr eventFromCom = (newEventStr)e;
            byte[] data = (byte[])eventFromCom.tys;

            string strcmds = COM_Cmd.byteToString(data);
            udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
           
            //if (UpdateUIhandle != null)
            //{

            //    UpdateUIhandle(this, nuiv);
               
            //}
            byte btype = data[3];
            //if(btype!='Q')
          
            //string s = Encoding.ASCII.GetString(data);
            //string str = s.Substring(6, 2);
            string strStation = "";
            try
            {
                strStation = ((int)data[1]).ToString("D2");
            }
            catch
            { 
            }

            /*
             *     头（%）+地址（1字节）+长度（2字节）+类型（1字节）+命令标识吗（4字节）
             *     +充电宝编号（10字节）+电量状态（1字节）+温度（1字节）+湿度（1字节）+紫外线（1字节）
             *     +尾（%）
             */

            if (btype == 'L')//下位机繁忙
            {
                udplog.logs.Enqueue(DateTime.Now.ToString() + "F回复" + strcmds);
            }
            if (btype == 'T')//重启完成
            {

                udplog.logs.Enqueue(DateTime.Now.ToString() + "F回复" + strcmds);
                StartEvent.Set();
                oneorder.status = true;
                oneorder.nowstatus = UIUpdateType.ProceStart;
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.Reset;
                nui.huodao = strStation;
                oneorder.nowstatus = UIUpdateType.Reset;
                nui.Msg = "重启完成";
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nui);
                }
            }
            if (btype == 'S')//错误代码查询
            {
                udplog.logs.Enqueue(DateTime.Now.ToString() + "F回复" + strcmds);
                StartEvent.Set();
                oneorder.status = true;
               oneorder.nowstatus = UIUpdateType.QueryError;
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.QueryError;
                nui.huodao = strStation;
                oneorder.nowstatus = UIUpdateType.QueryError;
                nui.Msg = "错误码" + data[4].ToString("X2") + data[5].ToString("X2");
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nui);
                }
            }

            /*查询：

            * 1．	发送：头（%）+地址（1字节）+长度（字节）+类型是’Q’+‘1’+校验（1字节）+尾（%）；
            * 2．	还回：头（%）+地址（1字节）+长度（字节）+类型是’Q’；+参数+校验（1字节）+尾（%）；
            * 3．	参数：来电宝状态字节（2,1,0）+ ID(10字节)+ 警告标志（1字节）+错误标志（1字节）
            * 4、   +警告次数（2字节）+12V电压（范围11-23，1字节）+5V电压（范围20~30，1字节）暂无意义（2字节）+使用次数（2字节）+温度（2字节）+电量（2字节）+充电电流（2字节）+编号状态（1和0）+；
            * 
            * 警告标志：0位表示来电宝温度  1位表示还时读取来电宝异常，2位表示借来电宝时候没拿走；
            * 3位来电超归还时超时没放入，4位读取来电宝失败  5位充电电压异常
            * 错误标志：0位是借出机械故障；1位是借出机械故障或者触点故障；2位是还机械故障或者触点故障,
            * 3位是红外1异常；4位是红外2异常；5位是触点异常；6位是读来电宝错误；
            */
            #region 查询储物柜
            
                
            if (btype == 'Q')//字母Q，代表查询的结果
            {
               
               // tools.insertLog("查询回复" + strcmds, Logtype.Query);
                //nuiv.Msg = strcmds;
                udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                string cdbno ="";// s.Substring(14, 10);
              
              
                int alertflag = (int)data[15];
                int errocode = data[16];//
                int alertcnt = 0;
                //int.TryParse(((int)data[15]).ToString() + ((int)data[16]).ToString(), out alertcnt);//充电宝使用次数
                alertcnt = Convert.ToInt32(data[17].ToString("X2") + data[18].ToString("X2"), 16);
                int usecnt = 0;//
                int cdtmp = data[22];//仓道温度
                if (data[21] == 1)
                {
                    cdtmp = -1 * cdtmp;
                }
              
                //int.TryParse(((int)data[23]).ToString() + ((int)data[24]).ToString(),out usecnt);//充电宝使用次数
                usecnt =  Convert.ToInt32(data[23].ToString("X2") + data[24].ToString("X2"), 16);
                //int.TryParse(((char)data[12]).ToString() + ((char)data[13]).ToString(),out errocode);//机器错误码
                double temp = 0;
                double.TryParse(((int)data[25]).ToString() + ((int)data[26]).ToString(),out temp);

                double idl = 0;
                double.TryParse(((int)data[27]).ToString() + ((int)data[28]).ToString(), out idl);

                double Adl = 0;
                double.TryParse(Convert.ToInt32((data[29].ToString("X2") + data[30].ToString("X2")),16).ToString(), out Adl);

                int MachineStatus =data[31];//机器码
                

                CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜
                cuwugui.TestCnt = 0;
                cuwugui.CommTime = DateTime.Now;
                cuwugui.Adl = Adl;
                cuwugui.AlartCnt = alertcnt;
                cuwugui.UseCnt = usecnt;
                cuwugui.alertflag = alertflag;
                cuwugui.errorflag = errocode;
                int V12 = data[19];
                int V5 = data[20];
                double DV12 = Math.Round(V12 * 11.5 / 15, 2);
                double DV5 = Math.Round((double)(V5 * 2 / 10), 2);
               
                cuwugui.V12 = DV12;
                cuwugui.V5 = DV5;
                cuwugui.Temp = cdtmp.ToString();
                if (errocode > 0)
                {
                    try
                    {
                        if (cuwugui.isReset==false&&cuwugui.ResetCnt < 5&&cuwugui.ResetTime<DateTime.Now.AddMinutes(-2))
                        {
                            LoseCD lcd = new LoseCD();
                            lcd.ldate = DateTime.Now;
                            lcd.cdn = byte.Parse(strStation);
                            NeedSendReBoot.Enqueue(lcd);
                            cuwugui.ResetCnt++;
                            cuwugui.isReset = true;
                        }
                    }
                    catch
                    { }
                }
                //if (data[data.Length - 3].ToString("D2") == "01")
                //{ 
                //    cuwugui.CWGStatus = CUWUGUISTATUS.SetingPower;
                   
                //}
                //else
                //{
                //    cuwugui.CWGStatus = CUWUGUISTATUS.Error;
                //}
                if (data[4] == 2 || data[4] == 0x82)//2—充电宝好
                {
                   // cuwugui.fHasCDB = true;
                   //if (cuwugui.LastLostTime<DateTime.Now.AddMinutes(-1))
                    cuwugui.HasLostCnt = 0;
                    if (data[4] == 0x82)
                    {
                        cuwugui.cdstatus = true;
                    }
                    else
                    {

                        cuwugui.cdstatus = false;
                    }
                    cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;
                    try
                    {
                        string StrNo = data[7].ToString("X2") + data[6].ToString("X2") + data[5].ToString("X2");
                        string StrYear = data[8].ToString("X2");
                        string StrMonth = data[9].ToString("X2");
                        string StrDay = data[10].ToString("X2");
                        cdbno = "00000000" + StrDay + StrMonth + StrYear + StrNo;

                    }
                    catch
                    {
                    }
                    if (cdbno != "00000000000000000000")
                    {
                        if (errocode == 0)
                        {
                            if (data[data.Length - 3].ToString("D2") == "01")
                            {
                                if (data[4] == 0x82)
                                    cuwugui.CWGStatus = CUWUGUISTATUS.SetingPower;

                                else
                                    cuwugui.CWGStatus = CUWUGUISTATUS.FullPower;
                            }
                            else
                            {
                                cuwugui.CWGStatus = CUWUGUISTATUS.None;
                            }
                            if (cuwugui.ResetTime < DateTime.Now.AddMinutes(-6))
                                cuwugui.ResetCnt = 0;
                        }
                        else
                        {
                            cuwugui.CWGStatus = CUWUGUISTATUS.Error;
                        }
                        cuwugui.HasLostCnt = 0;
                        cuwugui.LastLostTime = DateTime.Now;
                        Chongdianbao cdb = new Chongdianbao();
                        cdb.CDBNO = cdbno;
                        cdb.PowerDeep = idl;
                        cdb.temp = temp;
                        cdb.UseCnt = usecnt;
                        cuwugui.CDB = cdb;
                        //udplog.logs.Enqueue(DateTime.Now.ToString() + "电量" + idl.ToString());
                    }
                    else
                    {
                        cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;
                        if (data[data.Length - 3].ToString("D2") == "01")
                        {
                            cuwugui.CWGStatus = CUWUGUISTATUS.FullPower;
                        }
                        else
                        {
                            cuwugui.CWGStatus = CUWUGUISTATUS.None;
                        }
                        if(cuwugui.LastLostTime<DateTime.Now.AddMinutes(-1))
                            cuwugui.HasLostCnt = 0;
                  
                        Chongdianbao cdb = new Chongdianbao();
                        cdb.CDBNO = cdbno;

                        cdb.PowerDeep = idl;
                        cdb.temp = temp;
                        cdb.UseCnt = usecnt;
                        cuwugui.CDB = cdb;
                        if (errocode == 0)//如果errocode>0了  前面已经加入了一次重启 ，就不需要再在这里重启了
                        {
                            try
                            {
                                if (cuwugui.isReset == false&&cuwugui.ResetCnt < 5&&cuwugui.ResetTime<DateTime.Now.AddMinutes(-2))
                                {
                                    LoseCD lcd = new LoseCD();
                                    lcd.ldate = DateTime.Now;
                                    lcd.cdn = byte.Parse(strStation);
                                    NeedSendReBoot.Enqueue(lcd);
                                    cuwugui.ResetCnt++;
                                    cuwugui.isReset = true;
                                }
                            }
                            catch
                            {
                            }
                        }
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "充电宝编号错误" + idl.ToString());

                    }


                }
                else
                {
                    cuwugui.cdstatus = false;
                }

                if (data[4] == 1 || data[4] == 0x81 || data[4] == 4)//1-表示检测坏
                {
                    cuwugui.fHasCDB = true;
                    cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;



                    Chongdianbao cdb = new Chongdianbao();
                    try
                    {
                        string StrNo = data[7].ToString("X2") + data[6].ToString("X2") + data[5].ToString("X2");
                        string StrYear = data[8].ToString("X2");
                        string StrMonth = data[9].ToString("X2");
                        string StrDay = data[10].ToString("X2");
                        cdbno = "00000000" + StrDay + StrMonth + StrYear + StrNo;

                    }
                    catch
                    {
                    }
                    //if(cdb.CDBNO =="")
                    cdb.CDBNO = cdbno;

                    cdb.PowerDeep = 0;
                    cdb.UseCnt = usecnt;
                    cdb.temp = 0;
                    cuwugui.CDB = cdb;

                    cuwugui.CWGStatus = CUWUGUISTATUS.Error;


                }

                if (data[4] == 0 || data[4] == 0x80)//0—表示空
                {
                    cuwugui.fHasCDB = false;
                    cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;

                    cuwugui.CWGStatus = CUWUGUISTATUS.None;
                    if (cuwugui.LastLostTime < DateTime.Now.AddMinutes(-1))//如果归还时间失败时间已经过去两分钟，则清空失败，重新开放仓口
                    {
                        cuwugui.HasLostCnt = 0;
                    }
                    cuwugui.CDB = null;
                    cuwugui.Cmd = null;
                    if (errocode == 0)
                    {
                        if (cuwugui.ResetTime < DateTime.Now.AddMinutes(-6))
                            cuwugui.ResetCnt = 0;
                    }

                }
                if (errocode != 0)
                    cuwugui.CWGStatus = CUWUGUISTATUS.Error;
                Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.StatusUpdate;
                nui.huodao = strStation;
                nui.Msg = strcmds;
                if (UpdateUIhandle != null)
                {
                    UpdateUIhandle(this, nui);
                }
            }
            #endregion

            #region 借充电宝
            if (btype == 'D')//取来电宝后，设备回复发送指令后回复的第一个数据，字母'O'
            {

         
                udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                //SelectProductUpdateEvent es = new SelectProductUpdateEvent();
                //es.CurrentOrderFlow = OrderFlow.HasOneOrderDeal;
                //oneorder.CurrentStatus = Order.OrderStatus.Outputing;
                //es.Note = "正在出货中…………";
                //ManagerUpdateEvent.OnUpdate(es);
                CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];
                cuwugui.HasLostCnt = 0;
                if (data[4] == 0x21)
                {

                    //XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                    //xmlorder.orderno = "-1";
                    //xmlorder.datatime = DateTime.Now.ToString();
                    //xmlorder.cdbno = "00000000000000000000";
                    //xmlorder.cmno = strStation;
                    //xmlorder.status = "0D1";
                    //xmlorder.Add();
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借出仓D");
                    tools.insertLog(strStation + "号仓 准备租借出仓" + strcmds, Logtype.J);
                    JEvent.Set();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ChuHuoING;
                    nui.huodao = strStation;
                    oneorder.nowstatus = UIUpdateType.ChuHuoING;
                    if(ServerWork!=null)
                    nui.Msg = ServerWork.userNikeName+","+ServerWork.userHeadPic;
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                   
                }
                else
                {
                   
                    BCEvent.Set();
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借出仓失败");
                    tools.insertLog(strStation + "号仓 租借出仓失败" + strcmds, Logtype.J);
                    oneorder.status = false;
                    oneorder.nowstatus = UIUpdateType.ChuHuoL;
                    oneorder.iTryCnt=3;
                    if (oneorder.iTryCnt == 3)
                    {
                        
                      
                        EventUI nui = new EventUI();
                        nui.huodao = strStation;
                        nui.UIType = UIUpdateType.ChuHuoL;
                        nui.Msg = "出货失败";
                        nui.Flag = false;
                        //CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜   
                        cuwugui.JieLost++;
                        cuwugui.JieLostTime = DateTime.Now;
                        cuwugui.CommTime = DateTime.Now;
                      
                        cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;
                      
                        //cuwugui.JieLost++;
                        //cuwugui.LastLostTime = DateTime.Now;
                        Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                        //回复云端任务执行状态
                        ReponseWorkEvent evn = new ReponseWorkEvent();
                      
                        evn.workNo = oneorder.OrderNo;
                        evn.CDB = cuwugui.CDB.CDBNO;
                        evn.huodao = int.Parse(strStation);
                        evn.workstatus = false;
                        SendNetEvents.Enqueue(evn);
                        if (UpdateUIhandle != null)
                        {
                            UpdateUIhandle(this, nui);
                        }
                        StartEvent.Set();
                 
                    }    
                }
                //正在处理云端的任务，更新界面的状态，其他的查询任务暂停。

            }
            if (btype == 'E')//字母S,当下位机执行完任务后返回执行的状态，上位机需要答复下位机，即通知下位机已经收到；
            {
              
                udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                int flag = data[4];
                CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜

                if (flag == 1||flag==0x21)
                {
                    try
                    {
                        if (Test_Mode == 1)
                            cuwugui.AutoTestSuccessCnt++;
                    }
                    catch
                    { }
                    // ResponseWorkStatus();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ChuHuoWait;
                    nui.huodao = strStation;
                    oneorder.nowstatus = UIUpdateType.ChuHuoWait;
                    nui.Msg = "出货完成,等待取走";
                    //回复云端任务执行状态


                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    ReponseWorkEvent evn = new ReponseWorkEvent();
                    evn.CDB = cuwugui.CDB.CDBNO;
                    evn.huodao = int.Parse(strStation);
                    evn.workNo = oneorder.OrderNo;
                    evn.workstatus = true;
                  
                    if (cflag == 0)
                    {
                        SendNetEvents.Enqueue(evn);
                    }
                    cuwugui.CDB = null;
                   
                    cuwugui.HasLostCnt = 0;
                    
                    Cuwuguis[strStation] = cuwugui;
                    //XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                    //xmlorder.orderno = oneorder.OrderNo;
                    //xmlorder.datatime = DateTime.Now.ToString();
                    //xmlorder.cdbno = "00000000000000000000";
                    //xmlorder.cmno = strStation;
                    //xmlorder.status = "0E1";
                    //xmlorder.Add();
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借等待出货E1");
                    tools.insertLog(strStation+"号仓租借出仓等待取走"+strcmds, Logtype.J);
                }
                else
                {
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借出货失败E0");
                    oneorder.status = false;
                    oneorder.nowstatus = UIUpdateType.ChuHuoL;
                    oneorder.iTryCnt = 3;

                    if (oneorder.iTryCnt == 3)
                    {
                        BCEvent.Set();
                        
                        // lock (Cuwuguis)
                        {

                            cuwugui.JieLost++;
                            cuwugui.JieLostTime = DateTime.Now;
                            cuwugui.CommTime = DateTime.Now;

                            cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;

                            //cuwugui.JieLost++;
                            //cuwugui.LastLostTime = DateTime.Now;
                            Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                        }
                        EventUI nui = new EventUI();
                        nui.UIType = UIUpdateType.ChuHuoL;
                        nui.huodao = strStation;
                        nui.Msg = "出货失败";
                        nui.Flag = false;
                        //回复云端任务执行状态
                        ReponseWorkEvent evn = new ReponseWorkEvent();
                        evn.workNo = oneorder.OrderNo;
                        evn.CDB = ServerWork.CDBNO;// cuwugui.CDB.CDBNO;
                        evn.huodao = int.Parse(strStation);
                        evn.workstatus = true;
                        jieResult = -5;
                        if (cflag == 0)
                        {
                            SendNetEvents.Enqueue(evn);
                            try
                            {
                                //XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                                //xmlorder.orderno = oneorder.OrderNo;
                                //xmlorder.datatime = DateTime.Now.ToString();
                                //xmlorder.cdbno = cuwugui.CDB.CDBNO;
                                //xmlorder.cmno = strStation;
                                //xmlorder.status = "1";
                                //xmlorder.datatime = DateTime.Now.ToString();
                                //xmlorder.Modify();
                            }
                            catch
                            {
                            }
                        }
                        if (UpdateUIhandle != null)
                        {
                            UpdateUIhandle(this, nui);
                        }
                        tools.insertLog(strStation+"号仓租借出仓失败" + strcmds, Logtype.J);
                        //Thread.Sleep(3000);
                        //StartEvent.Set();
                    }



                }


            }
            if (btype == 'F')//字母S,当下位机执行完任务后返回执行的状态，上位机需要答复下位机，即通知下位机已经收到；
            {
             
                jieMonitor.Stop();
                BCEvent.Set();
                udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                int flag = data[4];
                CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜   
                cuwugui.LastLostTime = DateTime.Now;
                cuwugui.HasLostCnt = 0;
                if (flag == 1 || flag == 0x21)
                {      
                    
                    udplog.logs.Enqueue(strStation+"借出完成");
                    jieResult =1;
                    EventUI nui = new EventUI();
                    nui.huodao = strStation;
                    nui.UIType = UIUpdateType.ChuHuoF;
                    nui.Msg = "出货完成";
                    //回复云端任务执行状态
                  

                    if (UpdateUIhandle != null)
                    {
                       
                        UpdateUIhandle(this, nui);
                    }
                   
               
                    //完成出货，更新储物柜状态和 
                    //ReponseWorkEvent evn = new ReponseWorkEvent();
                    //evn.workNo = oneorder.OrderNo;

                    //evn.workstatus = true;
                    ////lock (Cuwuguis)
                    ////{

                    //evn.workNo = oneorder.OrderNo;
                    //evn.CDB = cuwugui.CDB.CDBNO;
                    //cuwugui.CommTime = DateTime.Now;
                    //cuwugui.CWGStatus = CUWUGUISTATUS.None;
                    //cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;
                    //cuwugui.JieLost = 0;

                    //cuwugui.CDB = null;
                    //cuwugui.Cmd = null;
                    //Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                    ////}
                    oneorder.status = true;
                    oneorder.nowstatus = UIUpdateType.ChuHuoF;
                    tools.insertLog(DateTime.Now.ToString()+" "+strStation + "号仓借出完成" + strcmds, Logtype.J);
                    Thread.Sleep(1000);
                    StartEvent.Set();
                    //evn.workstatus = true;
                    //if (cflag == 0)
                    //{
                  
                    //    //SendNetEvents.Enqueue(evn);
                    //}
                    // ResponseWorkStatus();



                }
                else
                {
                    jieResult = -5;
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借出货完成F0,未取走");
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ChuHuoL;
                    nui.Msg = "出货失败";
                    nui.Flag = false;
                    nui.huodao = strStation;
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    oneorder.status = true;
                    oneorder.nowstatus = UIUpdateType.ChuHuoL;
                    oneorder.iTryCnt = 3;
                    cuwugui.CommTime = DateTime.Now;
                    cuwugui.CWGStatus = CUWUGUISTATUS.None;
                    cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;
                    cuwugui.JieLost = 0;

                    //cuwugui.CDB = null;
                    cuwugui.Cmd = null;
                    //if(Test_Mode==0)//如果是测试模式 则不需保存
                    Cuwuguis[strStation] = cuwugui;//将储物柜保存回去


                    //byte[] cmdbyte = new byte[7];//Encoding.ASCII.GetBytes(strcmdSend);
                    //cmdbyte[0] = 0x25;
                    //cmdbyte[1] = byte.Parse(strStation);
                    //cmdbyte[2] = (byte)(4);

                    //cmdbyte[3] = 0x51;//字母Q

                    //cmdbyte[4] = (byte)1;
                    //// output.OrderId = cmdbyte[4].ToString("X2") + cmdbyte[5].ToString("X2") + cmdbyte[6].ToString("X2") + cmdbyte[7].ToString("X2");
                    //cmdbyte[5] = tools.Crc(cmdbyte, 0, 5);//校验


                    //cmdbyte[6] = 0x23;
                    //output.SendByte(cmdbyte);

                  

                   
                    //ResponseWorkStatus();

                    //回复云端任务执行状态
                    ReponseWorkEvent evn = new ReponseWorkEvent();
                   // CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜  
                    //evn.CDB = cuwugui.CDB.CDBNO;
                    evn.huodao = int.Parse(strStation);
                   
                    evn.workNo = oneorder.OrderNo;
                    evn.workstatus = false;
                    if (cflag == 0)
                    {  
                      // SendNetEvents.Enqueue(evn);

                    }
                    tools.insertLog(strStation + "号仓借出未取走"+strcmds, Logtype.J);
                    Thread.Sleep(3000);
                    StartEvent.Set();


                }


            }
            #endregion
            #region 归还充电宝
            if (btype == 'H'&&data[2]==0x04)//归还来电宝回复
            {

              
                udplog.logs.Enqueue(DateTime.Now.ToString() + "请求归还出仓，回复" + strcmds);
         
                HuanEvent.Set();
                
                CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜
                cuwugui.JieLost = 0;
                Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                if (data[4] == 0x21)
                {
                  
                    HuanEvent.Set();//
                    //StartEvent.Reset();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutInting;
                    nui.huodao = strStation;
                    nui.Msg = "设备已经准备好，请放入充电宝……";
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    oneorder.status=true;  
                    tools.insertLog(strStation+"号仓已经准备好，请放入充电宝" + " " + strcmds, Logtype.H);
                   
                }
                else
                {
                   
                    BCEvent.Set();//归还出仓失败即可跳转下一个
                    oneorder.status = false;
                    oneorder.iTryCnt=3;
                    cuwugui.LastLostTime = DateTime.Now;//记录
                    cuwugui.HasLostCnt += 1;
                    if(oneorder.iTryCnt==3)
                    {
                        StartEvent.Set();
                       // CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜
                        cuwugui.JieLost = 0;
                        cuwugui.LastLostTime = DateTime.Now;//记录
                        cuwugui.HasLostCnt += 1;
                        Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                        EventUI nui = new EventUI();
                        nui.UIType = UIUpdateType.PutIntFalse;
                        nui.Msg = "归还充电宝失败";
                        nui.Flag = false;
                        nui.huodao = strStation;
                        if (UpdateUIhandle != null)
                        {
                            UpdateUIhandle(this, nui);
                        }
                        oneorder.status = true;
                    }
                    tools.insertLog(strStation+"号仓未能打开仓门" + " " + strcmds, Logtype.H);
                }
            }
            if (btype == 'i')
            {
              
                if (data[4] == 0x21)
                {
                    // StartEvent.Reset();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutInting_check;
                    nui.Msg = "充电宝检测通过，请稍候";
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }
                    oneorder.status = true;
                }
                tools.insertLog("正在检测充电宝" + " " + strcmds, Logtype.H);
            }
            if (btype == 'I')
            {
                udplog.logs.Enqueue(DateTime.Now.ToString() + "回复,归还充电宝失败");
               
           
                if (data[4] == 0x20)
                {
                    if (onehuanaction.HasLostHuodao.Contains((int)data[1]))
                    {
                        onehuanaction.HasLostHuodao.Dequeue();
                    }
                }
                else
                {
                    // StartEvent.Reset();
                    //    EventUI nui = new EventUI();
                    //    nui.UIType = UIUpdateType.PutInting;
                    //    nui.Msg = "充电宝检测通过，请稍候";
                    //    if (UpdateUIhandle != null)
                    //    {
                    //        UpdateUIhandle(this, nui);
                    //    }
                    //    oneorder.status = true;
                    //}
                    //else
                    //{
                    BCEvent.Set();
                    oneorder.iTryCnt++;
                    CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜
                    cuwugui.JieLost = 0;
                    cuwugui.LastLostTime = DateTime.Now;//记录
                    cuwugui.HasLostCnt += 1;
                    Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                    //归还失败  则Lost次数加1
                    //onehuanaction.ActionTime = DateTime.Now;
                    //onehuanaction.CntPin();

                    //onehuanaction.HasLostHuodao.Enqueue(int.Parse(strStation));
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutIntFalse;
                    nui.huodao = strStation;
                    nui.Msg = "充电宝放反或者充电宝故障，请电话联系客服";
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    } 
                   
                }
                StartEvent.Set();
                if (data[4] == 0x20)
                    tools.insertLog(strStation + "号仓归还超时没放入充电宝" + " " + strcmds, Logtype.H);
                else
                    tools.insertLog(strStation + "号仓归还时充电宝放反或者充电宝故障" + " " + strcmds, Logtype.H);
                //XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                //xmlorder.orderno = "-1";
                //xmlorder.datatime = DateTime.Now.ToString();
                //xmlorder.cdbno = "00000000000000000000";
                //xmlorder.cmno = strStation;
                //xmlorder.status = "0I1";
                //xmlorder.Add();
                //Cuwuguis[strStation] = cuwugui;//将储物柜保存回去


            }
            if (btype == 'J')
            {
                    BCEvent.Set();
                   
                    udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                    StartEvent.Set();
                    //归还成功  则清除失败次数
                    onehuanaction.ActionStartTime = DateTime.Now;
                    onehuanaction.HasLostHuodao.Clear();
                    onehuanaction.firstHuodao = -1;
                //更新充电宝柜
                    string cdbno = "";// s.Substring(14, 10);
                    double temp = double.Parse(((int)data[25]).ToString() + ((int)data[26]).ToString());
                    double idl = double.Parse(((int)data[27]).ToString()  + ((int)data[28]).ToString());
                    int usecnt = 0;// int.Parse(((char)data[19]).ToString() + ((char)data[20]).ToString());
                    CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜
                    cuwugui.TestCnt = 0;
                    cuwugui.CommTime = DateTime.Now;
                    cuwugui.CWGStatus = CUWUGUISTATUS.SetingPower;


                    if (data[4] == 2 || data[4] == 0x82)//2—充电宝好
                    {
                        //清除掉
                        cuwugui.LastLostTime = DateTime.Now;
                        cuwugui.HasLostCnt = 0;//
                        cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;


                        string StrNo = data[7].ToString("X2") + data[6].ToString("X2") + data[5].ToString("X2");
                            //Convert.ToInt32(data[7].ToString("X2") + data[6].ToString("X2") + data[5].ToString("X2"), 16).ToString("D8");
                        string StrYear = data[8].ToString("X2");
                        string StrMonth =data[9].ToString("X2");
                        string StrDay = data[10].ToString("X2");

                        cdbno = "00000000" + StrDay + StrMonth + StrYear + StrNo;
                        //StrNo + StrYear + StrMonth + StrDay + "000000";
                        if (cdbno != "00000000000000000000")
                        {
                            tools.insertLog(strStation + "号仓归还时充电宝放成功"+cdbno + " " + strcmds, Logtype.H);
                            if (idl == 4)
                                cuwugui.CWGStatus = CUWUGUISTATUS.FullPower;
                            else
                                cuwugui.CWGStatus = CUWUGUISTATUS.SetingPower;
                            Chongdianbao cdb = new Chongdianbao();
                            cdb.CDBNO = cdbno;
                            cdb.PowerDeep = idl;
                            cdb.UseCnt = usecnt;
                            cuwugui.CDB = cdb;
                            cdb.temp = temp;
                            oneorder.status = true;
                            //XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                            //xmlorder.orderno = "-1";
                            //xmlorder.datatime = DateTime.Now.ToString();
                            //xmlorder.cdbno = cdbno;
                            //xmlorder.cmno = strStation;
                            //xmlorder.status = "0J1";
                            //xmlorder.Add();
                        }
                        else
                        {
                            //如果读取充电宝的编号错误,插入重启指令
                            try
                            {
                                if (cuwugui.isReset == false && cuwugui.ResetCnt < 5 && cuwugui.ResetTime < DateTime.Now.AddMinutes(-2))
                                {
                                    LoseCD lcd = new LoseCD();
                                    lcd.ldate = DateTime.Now;
                                    lcd.cdn = byte.Parse(strStation);
                                    cuwugui.ResetCnt++;
                                    cuwugui.isReset = true;
                                    NeedSendReBoot.Enqueue(lcd);
                                }
                            }
                            catch
                            { }
                            oneorder.iTryCnt++;//= true;
                            cuwugui.CWGStatus = CUWUGUISTATUS.Error;
                            tools.insertLog(strStation + "号仓归还充电宝编号错误"+ " " + strcmds, Logtype.H);
                        }
                  
                    }
                    if (data[4] == 1||data[4]==0x21)//1-表示检测坏
                    {
                        oneorder.iTryCnt++;
                        cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;
                        cuwugui.CWGStatus = CUWUGUISTATUS.SetingPower;
                        
                        cuwugui.LastLostTime = DateTime.Now;
                        cuwugui.HasLostCnt += 1;//
                        for (int i = 5; i < 25; i++)
                            cdbno += data[i].ToString("D2");
                        if (cdbno != "00000000000000000000")
                        {
                            if (idl == 4)
                                cuwugui.CWGStatus = CUWUGUISTATUS.FullPower;
                            else
                                cuwugui.CWGStatus = CUWUGUISTATUS.SetingPower;
                            Chongdianbao cdb = new Chongdianbao();
                            cdb.CDBNO = cdbno;
                            cdb.PowerDeep = idl;
                            cdb.UseCnt = usecnt;
                            cuwugui.CDB = cdb;
                            cdb.temp = temp;
                        }
                        else
                        {
                            cuwugui.CWGStatus = CUWUGUISTATUS.Error;
                        }
                     
                    }
                    if (data[4] == 0)//0—表示空
                    {
                        oneorder.iTryCnt++;
                        cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;

                        cuwugui.CWGStatus = CUWUGUISTATUS.None;
                        cuwugui.CDB = null;
                        cuwugui.Cmd = null;

                    }

                    Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutIntSuccess;
                    nui.huodao = strStation;
                    //tools.insertLog(strStation + "号仓归还成功充电宝" + cuwugui.CDB.CDBNO + " " + strcmds, Logtype.H);
                    //if (data[4] == 0x02 || data[4] == 0x82)
                    {
                        udplog.logs.Enqueue(DateTime.Now.ToString() + "：归还成功" + strStation +"充电宝"+cuwugui.CDB.CDBNO);
                        nui.Msg = "充电宝归还成功";
                        nui.Flag = true;
                    }
                    //else
                    //{

                    //    //onehuanaction.ActionTime = DateTime.Now;
                    //    //onehuanaction.CntPin();
                    //    //onehuanaction.LastLostHuodao = int.Parse(strStation);

                    //    udplog.logs.Enqueue(DateTime.Now.ToString() + "：充电宝归还失败");
                    //    nui.UIType = UIUpdateType.PutIntFalse;
                    //    nui.Msg = "充电宝归还失败，请联系客服";
                    //    nui.Flag = false;
                    //}
                    if (UpdateUIhandle != null)
                    {
                        UpdateUIhandle(this, nui);
                    }

            }
            #endregion
        }

     

        
       
    }
}
