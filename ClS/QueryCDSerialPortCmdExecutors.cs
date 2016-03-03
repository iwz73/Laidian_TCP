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
    public class QueryCDSerialPortCmdExecutors : SvCommandExecutor
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
            string strcmds = COM_Cmd.byteToString(data);
            if (data[3]== 0x51)//查询仓道
            {

                string strStation = "";
                try
                {
                    strStation = ((int)data[1]).ToString("D2");
                }
                catch
                {
                }
                /*----------------------
                 1、更新仓道
                -----------------------*/
                //udplog.logs.Enqueue(DateTime.Now.ToString() + "回复" + strcmds);
                string cdbno = "";// s.Substring(14, 10);


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
                usecnt = Convert.ToInt32(data[23].ToString("X2") + data[24].ToString("X2"), 16);
                //int.TryParse(((char)data[12]).ToString() + ((char)data[13]).ToString(),out errocode);//机器错误码
                double temp = 0;
                double.TryParse(((int)data[25]).ToString() + ((int)data[26]).ToString(), out temp);

                double idl = 0;
                double.TryParse(((int)data[27]).ToString() + ((int)data[28]).ToString(), out idl);

                double Adl = 0;
                double.TryParse(Convert.ToInt32((data[29].ToString("X2") + data[30].ToString("X2")), 16).ToString(), out Adl);

                int MachineStatus = data[31];//机器码


                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//更新储物柜
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
                        if (cuwugui.isReset == false && cuwugui.ResetCnt < 5 && cuwugui.ResetTime < DateTime.Now.AddMinutes(-2))
                        {
                            LoseCD lcd = new LoseCD();
                            lcd.ldate = DateTime.Now;
                            lcd.cdn = byte.Parse(strStation);
                            terminal.NeedSendReBoot.Enqueue(lcd);
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
                        if (cuwugui.LastLostTime < DateTime.Now.AddMinutes(-1))
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
                                if (cuwugui.isReset == false && cuwugui.ResetCnt < 5 && cuwugui.ResetTime < DateTime.Now.AddMinutes(-2))
                                {
                                    LoseCD lcd = new LoseCD();
                                    lcd.ldate = DateTime.Now;
                                    lcd.cdn = byte.Parse(strStation);
                                    terminal.NeedSendReBoot.Enqueue(lcd);
                                    cuwugui.ResetCnt++;
                                    cuwugui.isReset = true;
                                }
                            }
                            catch
                            {
                            }
                        }
                        terminal.udplog.logs.Enqueue(DateTime.Now.ToString() + "充电宝编号错误" + idl.ToString());

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
                terminal.Cuwuguis[strStation] = cuwugui;//将储物柜保存回去
                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.StatusUpdate;
                nui.huodao = strStation;
                nui.Msg = strcmds;
                //事件通知界面更新
                terminal.fireEvent(nui);

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
