using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ClS
{
    /// <summary>
    /// 服务器与Web服务器模块或Socket监听程序之间交互的事件对象或命令对象
    /// </summary>

    public class BasisEventCommand:ICloneable
    {
        /// <summary>
        /// serialport模块或Socket监听程序的SessionID
        /// </summary>
        public object Session;
        /// <summary>
        /// 事件或命令的ID，用于表明是哪种事件或命令
        /// </summary>
        public string EventCommandID;
        /// <summary>
        /// 事件或命令的具体内容，具体类型根据不同的事件或命令而异
        /// </summary>
        public object Content;

 
        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
    /// <summary>
    /// 命令执行结果对象
    /// </summary>
    
    public class BasisCommandResult
    {
        public object Session;
        public enum CommandResultTypeEnum { Succeeded, NoRight, ExceptionOcurred, NoRespnoseExecutor };
        /// <summary>
        /// 执行结果类型
        /// </summary>
        public CommandResultTypeEnum ResultType;
        /// <summary>
        /// 命令执行结果对象，具体类型根据命令的不同而不同
        /// </summary>
        public object Content;
    }
   
}
