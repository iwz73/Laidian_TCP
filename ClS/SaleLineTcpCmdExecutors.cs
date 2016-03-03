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
    public class SaleLineTcpCmdExecutor : SvCommandExecutor
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

            if (cmd.MessageType == MessageType.LAIDIAN_BUYLINE_REQ)
            {
                //cmd.Result = 0;
                int lineType = 0;
               
                /*----------------------
                 1����������ָ��
                -----------------------*/
                //DN.cdport.SaleLine();

            }
            else {
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
