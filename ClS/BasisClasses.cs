using System;
using System.Collections.Generic;
using System.Text;
using com.imlaidian.protobuf.model;
using System.Data;

namespace ClS
{
    /// <summary>
    /// 服务器端事件处理器
    /// </summary>
    public abstract class SvEventProcessor
    {
        /// <summary>
        /// 下一个处理器对象，使用该变量将所有处理器串接成链
        /// </summary>
        protected SvEventProcessor next = null;
        /// <summary>
        /// 抽象方法，该方法体中实现对特定事件的处理
        /// </summary>
        /// <param name="Event"></param>
        public abstract void Process(LaidianCommandModel Event);
        /// <summary>
        /// 添加新的事件处理器到处理器链尾。
        /// </summary>
        /// <param name="Processor">新加的事件处理器，该处理器的next必须是null，否则可能导致程序抛出异常或死循环。递归调用构造链</param>
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
    /// 不做任何操作的事件处理器，该类的实例用于建立处理器链的链首。
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
    /// 服务器端命令执行器
    /// </summary>
    public abstract class SvCommandExecutor
    {
        /// <summary>
        /// 下一个执行器对象，使用该变量将所有执行器串接到链
        /// </summary>
        protected SvCommandExecutor next = null;
        /// <summary>
        /// 抽象方法，在该方法体中实现对特定命令的处理。同时该方法内部还应该判断是否是执行器链尾，如果是链尾且命令不属于自己执行，
        /// 应该返回BasisCommandResult.CommandResultTypeEnum.NoRespnoseExecutor类型的结果.
        /// </summary>
        /// <param name="Command">需要执行的命令</param>
        /// <returns>处理结果对象</returns>
        public abstract BasisCommandResult Execute(BasisEventCommand Command);
        /// <summary>
        /// 添加新的执行器到执行器链的链尾
        /// </summary>
        /// <param name="Executor">新加的执行器，该执行器的next必须是null，否则可能导致抛出异常或死循环。</param>
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
    /// 不执行任何指令的执行器，用于建立执行器链的链首。
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
