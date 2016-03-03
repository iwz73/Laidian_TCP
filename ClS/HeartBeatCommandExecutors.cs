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
    public class HeartBeatCommandExecutor : SvCommandExecutor
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
            LDMachine terminal=(LDMachine)Command.Session;
            LaidianCommandModel cmd = (LaidianCommandModel)Command.Content;
            if (cmd.MessageType == MessageType.HEARTBEAT)
            {
                /*----------------------
                 1、更新时间
                -----------------------*/
                terminal.ComLinkLastTime = DateTime.Now;
                
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
