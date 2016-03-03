using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClS
{
    /// <summary>
    /// 2015-09-07
    /// 个人归还行为
    /// </summary>
    public class HuanAction
    {
        //归还的时间
        public DateTime ActionStartTime;

        public HuanAction()
        {
            ActionStartTime = DateTime.Now;
        }

        public int firstHuodao = -1;//每次第一个货道
        //在规定时间段内的失败次数
        public bool isEx
        {
            get
            {

                if (ActionStartTime.AddMinutes(2) < DateTime.Now)
                {
                    firstHuodao = -1;
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }

        //失败的
        public Queue<int> HasLostHuodao = new Queue<int>();
    }

    public class eachHuan
    {

        //本次失败的时间
        public DateTime ActionTime;
        //最后一次失败时的货道
        public int LastLostHuodao = -1;
    }
}
