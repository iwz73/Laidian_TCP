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
     ///�����¼��Ĵ���
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
                    log.Info("��ϢIDΪ" + Event.MessageType.ToString() + "��ָ��û�ж�Ӧ������.");
                }
                else
                {
                    this.next.Process(Event);
                }
            }

        }
    }
}
