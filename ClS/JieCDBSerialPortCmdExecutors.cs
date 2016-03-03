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
    ///����˷��͵�ָ���ִ����
    /// </summary>
    public class JieCDBSerialPortCmdExecutors: SvCommandExecutor
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
            if (btype == 'D')//ȡ���籦���豸�ظ�����ָ���ظ��ĵ�һ�����ݣ���ĸ'O'
            {


                //udplog.logs.Enqueue(DateTime.Now.ToString() + "�ظ�" + strcmds);

                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];
                cuwugui.HasLostCnt = 0;
                if (data[4] == 0x21)
                {

                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "������������D");
                    tools.insertLog(strStation + "�Ų� ׼��������" + strcmds, Logtype.J);
                    terminal.JEvent.Set();
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ChuHuoING;
                    nui.huodao = strStation;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoING;
                    if (terminal.ServerWork != null)
                        nui.Msg = terminal.ServerWork.userNikeName + "," + terminal.ServerWork.userHeadPic;
                    //�¼�֪ͨ�������
                    terminal.fireEvent(nui);

                }
                else
                {

                    terminal.BCEvent.Set();
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "������������ʧ��");
                    //tools.insertLog(strStation + "�Ų� ������ʧ��" + strcmds, Logtype.J);
                    terminal.oneorder.status = false;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoL;
                    terminal.oneorder.iTryCnt = 3;
                    if (terminal.oneorder.iTryCnt == 3)
                    {


                        EventUI nui = new EventUI();
                        nui.huodao = strStation;
                        nui.UIType = UIUpdateType.ChuHuoL;
                        nui.Msg = "����ʧ��";
                        nui.Flag = false;
                        //CUWUGUI cuwugui = (CUWUGUI)Cuwuguis[strStation];//���´����   
                        cuwugui.JieLost++;
                        cuwugui.JieLostTime = DateTime.Now;
                        cuwugui.CommTime = DateTime.Now;

                        cuwugui.CWGCommStatus = CWGCOMMSTATUS.OK;

                        //cuwugui.JieLost++;
                        //cuwugui.LastLostTime = DateTime.Now;
                        terminal.Cuwuguis[strStation] = cuwugui;//������񱣴��ȥ
                        //�ظ��ƶ�����ִ��״̬
                        ReponseWorkEvent evn = new ReponseWorkEvent();

                        evn.workNo = terminal.oneorder.OrderNo;
                        evn.CDB = cuwugui.CDB.CDBNO;
                        evn.huodao = int.Parse(strStation);
                        evn.workstatus = false;
                        terminal.SendNetEvents.Enqueue(evn);
                        //�¼�֪ͨ�������
                        terminal.fireEvent(nui);
                        terminal.StartEvent.Set();

                    }
                }
                //���ڴ����ƶ˵����񣬸��½����״̬�������Ĳ�ѯ������ͣ��

            }
            else if (btype == 'E')//��ĸS,����λ��ִ��������󷵻�ִ�е�״̬����λ����Ҫ����λ������֪ͨ��λ���Ѿ��յ���
            {

               // udplog.logs.Enqueue(DateTime.Now.ToString() + "�ظ�" + strcmds);
                int flag = data[4];
                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//���´����

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
                    nui.Msg = "�������,�ȴ�ȡ��";
                    //�¼�֪ͨ�������
                    terminal.fireEvent(nui);

                    //�ظ��ƶ�����ִ��״̬
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
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "���������ȴ�����E1");
                    //tools.insertLog(strStation + "�Ų������ֵȴ�ȡ��" + strcmds, Logtype.J);
                }
                else
                {
                    //udplog.logs.Enqueue(DateTime.Now.ToString() + "������������ʧ��E0");
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
                            terminal.Cuwuguis[strStation] = cuwugui;//������񱣴��ȥ
                        }
                        EventUI nui = new EventUI();
                        nui.UIType = UIUpdateType.ChuHuoL;
                        nui.huodao = strStation;
                        nui.Msg = "����ʧ��";
                        nui.Flag = false;
                        //�ظ��ƶ�����ִ��״̬
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
                        //�¼�֪ͨ�������
                        terminal.fireEvent(nui);
                        tools.insertLog(strStation + "�Ų�������ʧ��" + strcmds, Logtype.J);
                        //Thread.Sleep(3000);
                        //StartEvent.Set();
                    }



                }


            }
            else if (data[3]== 'F')//��ĸS,����λ��ִ��������󷵻�ִ�е�״̬����λ����Ҫ����λ������֪ͨ��λ���Ѿ��յ���
            {

                //terminal.jieMonitor.Stop();
                terminal.BCEvent.Set();
                terminal.udplog.logs.Enqueue(DateTime.Now.ToString() + "�ظ�" + strcmds);
                int flag = data[4];
                CUWUGUI cuwugui = (CUWUGUI)terminal.Cuwuguis[strStation];//���´����   
                cuwugui.LastLostTime = DateTime.Now;
                cuwugui.HasLostCnt = 0;
                if (flag == 1 || flag == 0x21)
                {

                    terminal.udplog.logs.Enqueue(strStation + "������");
                    terminal.jieResult = 1;
                    EventUI nui = new EventUI();
                    nui.huodao = strStation;
                    nui.UIType = UIUpdateType.ChuHuoF;
                    nui.Msg = "�������";
                    //�ظ��ƶ�����ִ��״̬


                    //�¼�֪ͨ�������
                    terminal.fireEvent(nui);


                    //��ɳ��������´����״̬�� 
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
                    //Cuwuguis[strStation] = cuwugui;//������񱣴��ȥ
                    ////}
                    terminal.oneorder.status = true;
                    terminal.oneorder.nowstatus = UIUpdateType.ChuHuoF;
                    tools.insertLog(DateTime.Now.ToString() + " " + strStation + "�Ųֽ�����" + strcmds, Logtype.J);
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
                    terminal.udplog.logs.Enqueue(DateTime.Now.ToString() + "���������������F0,δȡ��");
                    EventUI nui = new EventUI();
                    nui.UIType = UIUpdateType.ChuHuoL;
                    nui.Msg = "����ʧ��";
                    nui.Flag = false;
                    nui.huodao = strStation;
                    //�¼�֪ͨ�������
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
                    //if(Test_Mode==0)//����ǲ���ģʽ ���豣��
                    terminal.Cuwuguis[strStation] = cuwugui;//������񱣴��ȥ

                    tools.insertLog(strStation + "�Ųֽ��δȡ��" + strcmds, Logtype.J);
                    Thread.Sleep(3000);
                    terminal.StartEvent.Set();


                }


            }
            else {
                if (next == null)
                {

                    // log.Info("�û�" + client.ApplicationID + "ִ�е�IDΪ" + Command.EventCommandID + "��ָ��û�ж�Ӧִ����.");
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
