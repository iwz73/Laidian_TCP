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
     ///售线事件的处理
     ///售线主要是调用SaleLine()函数，在这里则采用事件机制，在次激发一个事件
     ///
     ///</summary>
    public class SaleLineEventProcessor : SvEventProcessor
    {
        LogWrite log = new LogWrite();

        public override void Process(LaidianCommandModel Event)
        {

            if (Event.MessageType == MessageType.HEARTBEAT)
            {
                //call SaleLine;



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
