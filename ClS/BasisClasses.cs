using System;
using System.Collections.Generic;
using System.Text;
using com.imlaidian.protobuf.model;
using System.Data;

namespace ClS
{
    /// <summary>
    /// ���������¼�������
    /// </summary>
    public abstract class SvEventProcessor
    {
        /// <summary>
        /// ��һ������������ʹ�øñ��������д��������ӳ���
        /// </summary>
        protected SvEventProcessor next = null;
        /// <summary>
        /// ���󷽷����÷�������ʵ�ֶ��ض��¼��Ĵ���
        /// </summary>
        /// <param name="Event"></param>
        public abstract void Process(LaidianCommandModel Event);
        /// <summary>
        /// ����µ��¼�����������������β��
        /// </summary>
        /// <param name="Processor">�¼ӵ��¼����������ô�������next������null��������ܵ��³����׳��쳣����ѭ�����ݹ���ù�����</param>
        public void AppendProcessor(SvEventProcessor Processor)
        {
            if (Processor == null)
                return;
            else
            {
                if (this.next == null)
                    this.next = Processor;
                else
                    this.next.AppendProcessor(Processor);
            }
        }
    }


    /// <summary>
    /// �����κβ������¼��������������ʵ�����ڽ����������������ס�
    /// </summary>
    public class EmptyEventProcessor : SvEventProcessor
    {
        public override void Process(LaidianCommandModel Event)
        {
            if (next == null)
                return;
            else
                next.Process(Event);
        }
    }
    /// <summary>
    /// ������������ִ����
    /// </summary>
    public abstract class SvCommandExecutor
    {
        /// <summary>
        /// ��һ��ִ��������ʹ�øñ���������ִ�������ӵ���
        /// </summary>
        protected SvCommandExecutor next = null;
        /// <summary>
        /// ���󷽷����ڸ÷�������ʵ�ֶ��ض�����Ĵ���ͬʱ�÷����ڲ���Ӧ���ж��Ƿ���ִ������β���������β����������Լ�ִ�У�
        /// Ӧ�÷���BasisCommandResult.CommandResultTypeEnum.NoRespnoseExecutor���͵Ľ��.
        /// </summary>
        /// <param name="Command">��Ҫִ�е�����</param>
        /// <returns>����������</returns>
        public abstract BasisCommandResult Execute(BasisEventCommand Command);
        /// <summary>
        /// ����µ�ִ������ִ����������β
        /// </summary>
        /// <param name="Executor">�¼ӵ�ִ��������ִ������next������null��������ܵ����׳��쳣����ѭ����</param>
        public void AppendExecutor(SvCommandExecutor Executor)
        {
            if (Executor == null)
                return;
            else
            {
                if (this.next == null)
                    this.next = Executor;
                else
                    this.next.AppendExecutor(Executor);
            }
        }
    }
    /// <summary>
    /// ��ִ���κ�ָ���ִ���������ڽ���ִ�����������ס�
    /// </summary>
    public class EmptyCommandExecutor:SvCommandExecutor
    {
        public override BasisCommandResult Execute(BasisEventCommand Command)
        {
            if (next == null)
            {
                BasisCommandResult result = new BasisCommandResult();
                result.ResultType = BasisCommandResult.CommandResultTypeEnum.NoRespnoseExecutor;
                result.Content = null;
                return result;
            }
            else
            {
                return next.Execute(Command);
            }
        }
    }


}
