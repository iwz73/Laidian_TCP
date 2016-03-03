using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClS
{
    /// <summary>
    /// 参数
    /// </summary>
    public class TerminalParams
    {
        /// <summary>
        /// 烟雾传感器1阈值
        /// </summary>
        public int YW1;
        /// <summary>
        /// 烟雾传感器2阈值
        /// </summary>
        public int YW2;
        /// <summary>
        /// 充电时长阈值
        /// </summary>
        public int CDSC;
        /// <summary>
        /// 其他参数
        /// </summary>
        public byte[] others=new byte[248];
        
    }
}
