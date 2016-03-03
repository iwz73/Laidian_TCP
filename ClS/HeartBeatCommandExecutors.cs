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
    public class HeartBeatCommandExecutor : SvCommandExecutor
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
            LDMachine terminal=(LDMachine)Command.Session;
            LaidianCommandModel cmd = (LaidianCommandModel)Command.Content;
            if (cmd.MessageType == MessageType.HEARTBEAT)
            {
                /*----------------------
                 1������ʱ��
                -----------------------*/
                terminal.ComLinkLastTime = DateTime.Now;
                
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
