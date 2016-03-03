using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClS
{
    public class UIEventDeal
    {
        /// <summary>
        /// 更新界面的事件发布者。
        /// </summary>
        public event EventHandler UpdateUIhandle;

        /// <summary>
        /// 初始化化界面的事件发布者。
        /// </summary>
        public event EventHandler UpdateInitUIhandle;

    }
}
