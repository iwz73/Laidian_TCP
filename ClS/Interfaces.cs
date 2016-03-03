using System;
using System.Collections.Generic;
using System.Text;

namespace ClS
{
    public delegate void FireServerEventDelegate(BasisEventCommand Event);
    public delegate BasisCommandResult ExecServerCommandDelegate(BasisEventCommand Command);
    /// <summary>
    /// ���������⹫���Ľӿ�
    /// </summary>
    public abstract class CentralService : MarshalByRefObject
    {
        /// <summary>
        /// ���������������޳�
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }


        /// <summary>
        /// Web������ģ���Socket�������������������һ���¼�
        /// </summary>
        /// <param name="Event">���͵����������¼�</param>
        public abstract void FireClientEvent(BasisEventCommand Event);
        /// <summary>
        /// Web������ģ���Socket�����������������ִ��һ��ָ��
        /// </summary>
        /// <param name="EventCommand">��Ҫִ�е�����</param>
        /// <returns></returns>
        public abstract BasisCommandResult ExecClientCommand(BasisEventCommand Command);
    }
   
}
