using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Globalization;
using com.imlaidian.protobuf.model;

namespace ClS
{
     ///<summary>
     ///心跳事件的处理
     ///</summary>
    public class heartbeatRespEventProcessor : SvEventProcessor
    {
        LogWrite log = new LogWrite();

        public override void Process(LaidianCommandModel Event)
        {

            if (Event.MessageType == MessageType.HEARTBEAT)
            {
            }
            else
            {
                if (next == null)
                {
                    log.Info("消息ID为" + Event.MessageType.ToString() + "的指令没有对应处理器.");
                }
                else
                {
                    this.next.Process(Event);
                }
            }

        }
    }
}
