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
    ///���������͵�ָ���ִ����
    /// </summary>
    public class HuanCDBSerialPortCmdExecutor : SvCommandExecutor
    {
        public delegate string GetNewPic(string path, DateTime datatime);
        /// <summary>
        /// ����ִ�к���
        /// </summary>
        /// <param name="Command">����</param>
        /// <returns>ִ�н��</returns>
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

            if (btype == 'H' && data[2] == 0x04)//�黹���籦�ظ�
            {


                //udplog.logs.Enqueue(DateTime.Now.ToString() + "����黹���֣��ظ�" + strcmds);

                terminal.HuanEvent.Set();

                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//���´����
                cuwugui.JieLost = 0;
                terminal.Cuwuguis[strStation] = cuwugui;//������񱣴��ȥ
                if (data[4] == 0x21)
                {

                    terminal.HuanEvent.Set();//
                                             //StartEvent.Reset();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutInting;
                    nui.huodao = strStation;
                    nui.Msg = "�豸�Ѿ�׼���ã�������籦����";
                    terminal.fireEvent(nui);
                    terminal.oneorder.status = true;
                    tools.insertLog(strStation + "�Ų��Ѿ�׼���ã�������籦" + " " + strcmds, Logtype.H);

                }
                else
                {

                    terminal.BCEvent.Set();//�黹����ʧ�ܼ�����ת��һ��
                    terminal.oneorder.status = false;
                    terminal.oneorder.iTryCnt = 3;
                    cuwugui.LastLostTime = DateTime.Now;//��¼
                    cuwugui.HasLostCnt += 1;
                    if (terminal.oneorder.iTryCnt == 3)
                    {
                        terminal.StartEvent.Set();
                        // CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//���´����
                        cuwugui.JieLost = 0;
                        cuwugui.LastLostTime = DateTime.Now;//��¼
                        cuwugui.HasLostCnt += 1;
                        terminal.Cuwuguis[strStation] = cuwugui;//������񱣴��ȥ
                        EventUI nui = new EventUI();
                        nui.UIType = UIUpdateType.PutIntFalse;
                        nui.Msg = "�黹��籦ʧ��";
                        nui.Flag = false;
                        nui.huodao = strStation;
                        terminal.fireEvent(nui);
                        terminal.oneorder.status = true;
                    }
                    tools.insertLog(strStation + "�Ų�δ�ܴ򿪲���" + " " + strcmds, Logtype.H);
                }
            }
            else if (btype == 'i')
            {

                if (data[4] == 0x21)
                {
                    // StartEvent.Reset();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutInting_check;
                    nui.Msg = "��籦���ͨ�������Ժ�";
                    terminal.fireEvent(nui);
                    terminal.oneorder.status = true;
                }
                tools.insertLog("���ڼ���籦" + " " + strcmds, Logtype.H);
            }
            if (btype == 'I')
            {
                //udplog.logs.Enqueue(DateTime.Now.ToString() + "�ظ�,�黹��籦ʧ��");


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
                    CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//���´����
                    cuwugui.JieLost = 0;
                    cuwugui.LastLostTime = DateTime.Now;//��¼
                    cuwugui.HasLostCnt += 1;
                    terminal.Cuwuguis[strStation] = cuwugui;//������񱣴��ȥ
                                                            //�黹ʧ��  ��Lost������1
                                                            //onehuanaction.ActionTime = DateTime.Now;
                                                            //onehuanaction.CntPin();

                    //onehuanaction.HasLostHuodao.Enqueue(int.Parse(strStation));
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.PutIntFalse;
                    nui.huodao = strStation;
                    nui.Msg = "��籦�ŷ����߳�籦���ϣ���绰��ϵ�ͷ�";
                    terminal.fireEvent(nui);

                }
                terminal.StartEvent.Set();
                if (data[4] == 0x20)
                    tools.insertLog(strStation + "�Ųֹ黹��ʱû�����籦" + " " + strcmds, Logtype.H);
                else
                    tools.insertLog(strStation + "�Ųֹ黹ʱ��籦�ŷ����߳�籦����" + " " + strcmds, Logtype.H);



            }
            else if (btype == 'J')
            {
                terminal.BCEvent.Set();

                //udplog.logs.Enqueue(DateTime.Now.ToString() + "�ظ�" + strcmds);
                terminal.StartEvent.Set();
                //�黹�ɹ�  �����ʧ�ܴ���
                terminal.onehuanaction.ActionStartTime = DateTime.Now;
                terminal.onehuanaction.HasLostHuodao.Clear();
                terminal.onehuanaction.firstHuodao = -1;
                //���³�籦��
                string cdbno = "";// s.Substring(14, 10);
                double temp = double.Parse(((int)data[25]).ToString() + ((int)data[26]).ToString());
                double idl = double.Parse(((int)data[27]).ToString() + ((int)data[28]).ToString());
                int usecnt = 0;// int.Parse(((char)data[19]).ToString() + ((char)data[20]).ToString());
                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//���´����
                cuwugui.TestCnt = 0;
                cuwugui.CommTime = DateTime.Now;
                cuwugui.CWGStatus = CUWUGUISTATUS.SetingPower;


                if (data[4] == 2 || data[4] == 0x82)//2����籦��
                {
                    //�����
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
                        tools.insertLog(strStation + "�Ųֹ黹ʱ��籦�ųɹ�" + cdbno + " " + strcmds, Logtype.H);
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
                        //�����ȡ��籦�ı�Ŵ���,��������ָ��
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
                        tools.insertLog(strStation + "�Ųֹ黹��籦��Ŵ���" + " " + strcmds, Logtype.H);
                    }

                }
                if (data[4] == 1 || data[4] == 0x21)//1-��ʾ��⻵
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
                if (data[4] == 0)//0����ʾ��
                {
                    terminal.oneorder.iTryCnt++;
                    cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;

                    cuwugui.CWGStatus = CUWUGUISTATUS.None;
                    cuwugui.CDB = null;
                    cuwugui.Cmd = null;

                }

                terminal.Cuwuguis[strStation] = cuwugui;//������񱣴��ȥ

                EventUI nui = new EventUI();
                nui.UIType = UIUpdateType.PutIntSuccess;
                nui.huodao = strStation;
                nui.Msg = "��籦�黹�ɹ�";
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
