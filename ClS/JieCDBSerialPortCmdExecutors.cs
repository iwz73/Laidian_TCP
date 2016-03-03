using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Threading;
using com.imlaidian.protobuf.model;

namespace ClS
{

 
    /// <summary>
    ///服务端发送的指令的执行器
    /// </summary>
    public class JieCDBSerialPortCmdExecutors: SvCommandExecutor
    {
        public delegate string GetNewPic(string path, DateTime datatime);

       
        /// <summary>
        /// 命令执行函数
        /// </summary>
        /// <param name="Command">命令</param>
        /// <returns>执行结果</returns>
        public override BasisCommandResult Execute(BasisEventCommand Command)
        {
            BasisCommandResult result = new BasisCommandResult();
       
            //  Command.Session;
            LDMachine terminal = (LDMachine)Command.Session;
            byte[] data = (byte[])Command.Content;
            byte btype = data[3];
            string strcmds = COM_Cmd.byteToString(data);
            string strStation = "";
            try
            {
                strStation = ((int)data[1]).ToString("D2");
            }
            catch
            {
            }
            if (btype == 'D')//取来电宝后，设备回复发送指令后回复的第一个数据，字母'O'
            {


                //udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);

                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];
                cuwugui.HasLostCnt = 0;
                if (data[4] == 0x21)
                {

                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借出仓D");
                    tools.insertLog(strStation + "号仓 准备租借出仓" + strcmds, Logtype.J);
                    terminal.JEvent.Set();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ChuHuoING;
                    nui.huodao = strStation;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoING;
                    if (terminal.ServerWork != null)
                        nui.Msg = terminal.ServerWork.userNikeName + "," + terminal.ServerWork.userHeadPic;
                    //事件通知界面更新
                    terminal.fireEvent(nui);

                }
                else
                {

                    terminal.BCEvent.Set();
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借出仓失败");
                    //tools.insertLog(strStation + "号仓 租借出仓失败" + strcmds, Logtype.J);
                    terminal.oneorder.status = false;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoL;
                    terminal.oneorder.iTryCnt = 3;
                    if (terminal.oneorder.iTryCnt == 3)
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
                        terminal.Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                        //回复云端任务执行状态
                        ReponseWorkEvent evn = new ReponseWorkEvent();

                        evn.workNo = terminal.oneorder.OrderNo;
                        evn.CDB = cuwugui.CDB.CDBNO;
                        evn.huodao = int.Parse(strStation);
                        evn.workstatus = false;
                        terminal.SendNetEvents.Enqueue(evn);
                        //事件通知界面更新
                        terminal.fireEvent(nui);
                        terminal.StartEvent.Set();

                    }
                }
                //正在处理云端的任务，更新界面的状态，其他的查询任务暂停。

            }
            else if (btype == 'E')//字母S,当下位机执行完任务后返回执行的状态，上位机需要答复下位机，即通知下位机已经收到；
            {

               // udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                int flag = data[4];
                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//更新储物柜

                if (flag == 1 || flag == 0x21)
                {
                    try
                    {
                        if (terminal.Test_Mode == 1)
                            cuwugui.AutoTestSuccessCnt++;
                    }
                    catch
                    { }
                    // ResponseWorkStatus();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ChuHuoWait;
                    nui.huodao = strStation;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoWait;
                    nui.Msg = "出货完成,等待取走";
                    //事件通知界面更新
                    terminal.fireEvent(nui);

                    //回复云端任务执行状态
                    ReponseWorkEvent evn = new ReponseWorkEvent();
                    evn.CDB = cuwugui.CDB.CDBNO;
                    evn.huodao = int.Parse(strStation);
                    evn.workNo = terminal.oneorder.OrderNo;
                    evn.workstatus = true;

                    if (terminal.cflag == 0)
                    {
                        terminal.SendNetEvents.Enqueue(evn);
                    }
                    cuwugui.CDB = null;

                    cuwugui.HasLostCnt = 0;

                    terminal.Cuwuguis[strStation] = cuwugui;
                    //XmlOperatorOrder xmlorder = new XmlOperatorOrder();
                    //xmlorder.orderno = oneorder.OrderNo;
                    //xmlorder.datatime = DateTime.Now.ToString();
                    //xmlorder.cdbno = "00000000000000000000";
                    //xmlorder.cmno = strStation;
                    //xmlorder.status = "0E1";
                    //xmlorder.Add();
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借等待出货E1");
                    //tools.insertLog(strStation + "号仓租借出仓等待取走" + strcmds, Logtype.J);
                }
                else
                {
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借出货失败E0");
                    terminal.oneorder.status = false;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoL;
                    terminal.oneorder.iTryCnt = 3;

                    if (terminal.oneorder.iTryCnt == 3)
                    {
                        //BCEvent.Set();

                        // lock (Cuwuguis)
                        {

                            cuwugui.JieLost++;
                            cuwugui.JieLostTime = DateTime.Now;
                            cuwugui.CommTime = DateTime.Now;

                            cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;

                            //cuwugui.JieLost++;
                            //cuwugui.LastLostTime = DateTime.Now;
                            terminal.Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                        }
                        EventUI nui = new EventUI();
                        nui.UIType = UIUpdateType.ChuHuoL;
                        nui.huodao = strStation;
                        nui.Msg = "出货失败";
                        nui.Flag = false;
                        //回复云端任务执行状态
                        ReponseWorkEvent evn = new ReponseWorkEvent();
                        evn.workNo = terminal.oneorder.OrderNo;
                        evn.CDB = terminal.ServerWork.CDBNO;// cuwugui.CDB.CDBNO;
                        evn.huodao = int.Parse(strStation);
                        evn.workstatus = true;
                        terminal.jieResult = -5;
                        if (terminal.cflag == 0)
                        {
                            terminal.SendNetEvents.Enqueue(evn);
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
                        //事件通知界面更新
                        terminal.fireEvent(nui);
                        tools.insertLog(strStation + "号仓租借出仓失败" + strcmds, Logtype.J);
                        //Thread.Sleep(3000);
                        //StartEvent.Set();
                    }



                }


            }
            else if (data[3]== 'F')//字母S,当下位机执行完任务后返回执行的状态，上位机需要答复下位机，即通知下位机已经收到；
            {

                //terminal.jieMonitor.Stop();
                terminal.BCEvent.Set();
                terminal.udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                int flag = data[4];
                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//更新储物柜   
                cuwugui.LastLostTime = DateTime.Now;
                cuwugui.HasLostCnt = 0;
                if (flag == 1 || flag == 0x21)
                {

                    terminal.udplog.logs.Enqueue(strStation + "借出完成");
                    terminal.jieResult = 1;
                    EventUI nui = new EventUI();
                    nui.huodao = strStation;
                    nui.UIType = UIUpdateType.ChuHuoF;
                    nui.Msg = "出货完成";
                    //回复云端任务执行状态


                    //事件通知界面更新
                    terminal.fireEvent(nui);


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
                    terminal.oneorder.status = true;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoF;
                    tools.insertLog(DateTime.Now.ToString() + " " + strStation + "号仓借出完成" + strcmds, Logtype.J);
                    Thread.Sleep(1000);
                    terminal.StartEvent.Set();
                    //evn.workstatus = true;
                    //if (cflag == 0)
                    //{

                    //    //SendNetEvents.Enqueue(evn);
                    //}
                    // ResponseWorkStatus();



                }
                else 
                {
                    terminal.jieResult = -5;
                    terminal.udplog.logs.Enqueue(DateTime.Now.ToString() + "：请求租借出货完成F0,未取走");
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ChuHuoL;
                    nui.Msg = "出货失败";
                    nui.Flag = false;
                    nui.huodao = strStation;
                    //事件通知界面更新
                    terminal.fireEvent(nui);
                    terminal.oneorder.status = true;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoL;
                    terminal.oneorder.iTryCnt = 3;
                    cuwugui.CommTime = DateTime.Now;
                    cuwugui.CWGStatus = CUWUGUISTATUS.None;
                    cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;
                    cuwugui.JieLost = 0;

                    //cuwugui.CDB = null;
                    cuwugui.Cmd = null;
                    //if(Test_Mode==0)//如果是测试模式 则不需保存
                    terminal.Cuwuguis[strStation] = cuwugui;//将储物柜保存回去

                    tools.insertLog(strStation + "号仓借出未取走" + strcmds, Logtype.J);
                    Thread.Sleep(3000);
                    terminal.StartEvent.Set();


                }


            }
            else {
                if (next == null)
                {

                    // log.Info("用户" + client.ApplicationID + "执行的ID为" + Command.EventCommandID + "的指令没有对应执行器.");
                    result.ResultType = BasisCommandResult.CommandResultTypeEnum.NoRespnoseExecutor;
                    result.Content = null;
                    return result;
                }
                else
                {
                    return this.next.Execute(Command);
                }
            }
            return result;
                
   
        }
    }

}
