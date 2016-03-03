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
    ///服务器发送的指令的执行器
    /// </summary>
    public class HuanCDBSerialPortCmdExecutor : SvCommandExecutor
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

            LDMachine terminal = (LDMachine)Command.Session;


            byte[] data = (byte[])Command.Content;
            string strcmds = COM_Cmd.byteToString(data);
            string strStation = "";
            try
            {
                strStation = ((int)data[1]).ToString("D2");
            }
            catch
            {
            }
            byte btype = data[3];

            if (btype == 'H' && data[2] == 0x04)//归还来电宝回复
            {


                //udplog.logs.Enqueue(DateTime.Now.ToString() + "请求归还出仓，回复" + strcmds);

                terminal.HuanEvent.Set();

                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//更新储物柜
                cuwugui.JieLost = 0;
                terminal.Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                if (data[4] == 0x21)
                {

                    terminal.HuanEvent.Set();//
                                             //StartEvent.Reset();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutInting;
                    nui.huodao = strStation;
                    nui.Msg = "设备已经准备好，请放入充电宝……";
                    terminal.fireEvent(nui);
                    terminal.oneorder.status = true;
                    tools.insertLog(strStation + "号仓已经准备好，请放入充电宝" + " " + strcmds, Logtype.H);

                }
                else
                {

                    terminal.BCEvent.Set();//归还出仓失败即可跳转下一个
                    terminal.oneorder.status = false;
                    terminal.oneorder.iTryCnt = 3;
                    cuwugui.LastLostTime = DateTime.Now;//记录
                    cuwugui.HasLostCnt += 1;
                    if (terminal.oneorder.iTryCnt == 3)
                    {
                        terminal.StartEvent.Set();
                        // CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//更新储物柜
                        cuwugui.JieLost = 0;
                        cuwugui.LastLostTime = DateTime.Now;//记录
                        cuwugui.HasLostCnt += 1;
                        terminal.Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                        EventUI nui = new EventUI();
                        nui.UIType = UIUpdateType.PutIntFalse;
                        nui.Msg = "归还充电宝失败";
                        nui.Flag = false;
                        nui.huodao = strStation;
                        terminal.fireEvent(nui);
                        terminal.oneorder.status = true;
                    }
                    tools.insertLog(strStation + "号仓未能打开仓门" + " " + strcmds, Logtype.H);
                }
            }
            else if (btype == 'i')
            {

                if (data[4] == 0x21)
                {
                    // StartEvent.Reset();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutInting_check;
                    nui.Msg = "充电宝检测通过，请稍候";
                    terminal.fireEvent(nui);
                    terminal.oneorder.status = true;
                }
                tools.insertLog("正在检测充电宝" + " " + strcmds, Logtype.H);
            }
            if (btype == 'I')
            {
                //udplog.logs.Enqueue(DateTime.Now.ToString() + "回复,归还充电宝失败");


                if (data[4] == 0x20)
                {
                    if (terminal.onehuanaction.HasLostHuodao.Contains((int)data[1]))
                    {
                        terminal.onehuanaction.HasLostHuodao.Dequeue();
                    }
                }
                else
                {

                    terminal.BCEvent.Set();
                    terminal.oneorder.iTryCnt++;
                    CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//更新储物柜
                    cuwugui.JieLost = 0;
                    cuwugui.LastLostTime = DateTime.Now;//记录
                    cuwugui.HasLostCnt += 1;
                    terminal.Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                                                            //归还失败  则Lost次数加1
                                                            //onehuanaction.ActionTime = DateTime.Now;
                                                            //onehuanaction.CntPin();

                    //onehuanaction.HasLostHuodao.Enqueue(int.Parse(strStation));
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutIntFalse;
                    nui.huodao = strStation;
                    nui.Msg = "充电宝放反或者充电宝故障，请电话联系客服";
                    terminal.fireEvent(nui);

                }
                terminal.StartEvent.Set();
                if (data[4] == 0x20)
                    tools.insertLog(strStation + "号仓归还超时没放入充电宝" + " " + strcmds, Logtype.H);
                else
                    tools.insertLog(strStation + "号仓归还时充电宝放反或者充电宝故障" + " " + strcmds, Logtype.H);



            }
            else if (btype == 'J')
            {
                terminal.BCEvent.Set();

                //udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                terminal.StartEvent.Set();
                //归还成功  则清除失败次数
                terminal.onehuanaction.ActionStartTime = DateTime.Now;
                terminal.onehuanaction.HasLostHuodao.Clear();
                terminal.onehuanaction.firstHuodao = -1;
                //更新充电宝柜
                string cdbno = "";// s.Substring(14, 10);
                double temp = double.Parse(((int)data[25]).ToString() + ((int)data[26]).ToString());
                double idl = double.Parse(((int)data[27]).ToString() + ((int)data[28]).ToString());
                int usecnt = 0;// int.Parse(((char)data[19]).ToString() + ((char)data[20]).ToString());
                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//更新储物柜
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
                    string StrMonth = data[9].ToString("X2");
                    string StrDay = data[10].ToString("X2");

                    cdbno = "00000000" + StrDay + StrMonth + StrYear + StrNo;
                    //StrNo + StrYear + StrMonth + StrDay + "000000";
                    if (cdbno != "00000000000000000000")
                    {
                        tools.insertLog(strStation + "号仓归还时充电宝放成功" + cdbno + " " + strcmds, Logtype.H);
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
                        terminal.oneorder.status = true;
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
                                terminal.NeedSendReBoot.Enqueue(lcd);
                            }
                        }
                        catch
                        { }
                        terminal.oneorder.iTryCnt++;//= true;
                        cuwugui.CWGStatus = CUWUGUISTATUS.Error;
                        tools.insertLog(strStation + "号仓归还充电宝编号错误" + " " + strcmds, Logtype.H);
                    }

                }
                if (data[4] == 1 || data[4] == 0x21)//1-表示检测坏
                {
                    terminal.oneorder.iTryCnt++;
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
                    terminal.oneorder.iTryCnt++;
                    cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;

                    cuwugui.CWGStatus = CUWUGUISTATUS.None;
                    cuwugui.CDB = null;
                    cuwugui.Cmd = null;

                }

                terminal.Cuwuguis[strStation] = cuwugui;//将储物柜保存回去

                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.PutIntSuccess;
                nui.huodao = strStation;
                nui.Msg = "充电宝归还成功";
                nui.Flag = true;

                terminal.fireEvent(nui);

            }else {
                if (next == null)
                {


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
