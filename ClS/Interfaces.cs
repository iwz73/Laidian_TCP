using System;
using System.Collections.Generic;
using System.Text;

namespace ClS
{
    public delegate void FireServerEventDelegate(BasisEventCommand Event);
    public delegate BasisCommandResult ExecServerCommandDelegate(BasisEventCommand Command);
    /// <summary>
    /// 服务器对外公布的接口
    /// </summary>
    public abstract class CentralService : MarshalByRefObject
    {
        /// <summary>
        /// 设置生命周期无限长
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }


        /// <summary>
        /// Web服务器模块或Socket监听程序向服务器发送一个事件
        /// </summary>
        /// <param name="Event">发送到服务器的事件</param>
        public abstract void FireClientEvent(BasisEventCommand Event);
        /// <summary>
        /// Web服务器模块或Socket监听程序请求服务器执行一个指令
        /// </summary>
        /// <param name="EventCommand">需要执行的命令</param>
        /// <returns></returns>
        public abstract BasisCommandResult ExecClientCommand(BasisEventCommand Command);
    }
   
}
