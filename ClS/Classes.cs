using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ClS
{
    /// <summary>
    /// ��������Web������ģ���Socket��������֮�佻�����¼�������������
    /// </summary>

    public class BasisEventCommand:ICloneable
    {
        /// <summary>
        /// serialportģ���Socket���������SessionID
        /// </summary>
        public object Session;
        /// <summary>
        /// �¼��������ID�����ڱ����������¼�������
        /// </summary>
        public string EventCommandID;
        /// <summary>
        /// �¼�������ľ������ݣ��������͸��ݲ�ͬ���¼����������
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
    /// ����ִ�н������
    /// </summary>
    
    public class BasisCommandResult
    {
        public object Session;
        public enum CommandResultTypeEnum { Succeeded, NoRight, ExceptionOcurred, NoRespnoseExecutor };
        /// <summary>
        /// ִ�н������
        /// </summary>
        public CommandResultTypeEnum ResultType;
        /// <summary>
        /// ����ִ�н�����󣬾������͸�������Ĳ�ͬ����ͬ
        /// </summary>
        public object Content;
    }
   
}
