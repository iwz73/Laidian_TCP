using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClS
{
    /// <summary>
    /// 储物柜状态，包含故障、空缺、正在充电、完成充电
    /// </summary>
    public enum CUWUGUISTATUS
    {
        
        /// <summary>
        /// 故障
        /// </summary>
        Error,
        /// <summary>
        /// 串口通信错误
        /// </summary>
        CommError,
        /// <summary>
        /// 网络通信错误
        /// </summary>
        NetError,
        /// <summary>
        /// 空缺
        /// </summary>
        None,
        /// <summary>
        /// 正在充电
        /// </summary>
        SetingPower,
        /// <summary>
        /// 完成充电
        /// </summary>
        FullPower
 
    }
    public enum CWGCOMMSTATUS
    {
        /// <summary>
        /// 故障
        /// </summary>
        Error=0,
        /// <summary>
        /// 正常
        /// </summary>
        OK=1

    }
    /// <summary>
    /// 充电宝类
    /// </summary>
    public class Chongdianbao
    {
        /// <summary>
        /// 充电宝编号
        /// </summary>
        public string CDBNO;
        /// <summary>
        /// 充电宝电量
        /// </summary>
        public double PowerDeep;
        /// <summary>
        /// 充电宝使用次数
        /// </summary>
        public double temp;
        /// <summary>
        /// 充电宝使用次数
        /// </summary>
        public int UseCnt;
 
    }
    /// <summary>
    /// 
    /// </summary>
    public class CUWUGUI
    {
        /// <summary>
        /// 充电标志
        /// </summary>
        public bool cdstatus;
        /// <summary>
        /// 储物柜编号
        /// </summary>
        public string CWGID;
        /// <summary>
        /// 温度传感器状态
        /// </summary>
        public string Temp;
        /// <summary>
        /// 湿度传感器状态
        /// </summary>
        public string Sweet;
        /// <summary>
        /// 紫外线状态
        /// </summary>
        public string OutLight;
        /// <summary>
        /// 电机状态
        /// </summary>
        public string moda;
        /// <summary>
        /// 充电电路状态
        /// </summary>
        //public string SupperPowerStatus;
        ////储物柜状态：使用、空闲、故障
        /// <summary>
        /// 储物柜状态
        /// </summary>
        public CUWUGUISTATUS CWGStatus = CUWUGUISTATUS.None;
        /// <summary>
        /// 通信指令：
        /// </summary>
        public byte[] Cmd;

        /// <summary>
        /// 通信状态：
        /// </summary>

        public CWGCOMMSTATUS CWGCommStatus = CWGCOMMSTATUS.OK;
        /// <summary>
        /// 最后通信时间：
        /// </summary>
        public DateTime CommTime;
        /// <summary>
        /// 所含的充电宝
        /// </summary>
        public Chongdianbao CDB = null;
        /// <summary>
        /// 记录失败次数,超过三次则视为失败
        /// </summary>
        public int TestCnt = 0;
        /// <summary>
        /// 记录故障总次数
        /// </summary>
        public long HasLostCnt = 0;
        /// <summary>
        /// 最近一次失败的时间
        /// </summary>
        public DateTime LastLostTime;

        public int JieLost;//借失败次数
        public DateTime JieLostTime;//借的最后一次失败时间
        /// <summary>
        /// 使用次数
        /// </summary>
        public int UseCnt;
        /// <summary>
        /// 告警次数
        /// </summary>
        public int AlartCnt;
        /// <summary>
        /// 电流值
        /// </summary>
        public double Adl;
        /// <summary>
        /// 告警标志
        /// </summary>
        public int alertflag;
        /// <summary>
        /// 错误标志
        /// </summary>
        public int errorflag;
        /// <summary>
        /// 12v电压
        /// </summary>
        public double V12;

        /// <summary>
        /// 5V电压
        /// </summary>
        public double V5;

        /// <summary>
        /// 自动模式测试次数
        /// </summary>
        public int AutoTestCnt = 0;

        /// <summary>
        /// 自动测试成功次数
        /// </summary>
        public int AutoTestSuccessCnt = 0;

        /// <summary>
        /// 重启次数
        /// </summary>
        public int ResetCnt = 0;
        /// <summary>
        ///指示当前仓道是否已经处于要重启状态  如果是的话则不能再往重启之列队列中添加本仓道
        /// </summary>
        public bool isReset=false;
        /// <summary>
        /// 最近一次重启时间
        /// </summary>
        public DateTime ResetTime;
        /// <summary>
        /// 是否有充电宝
        /// </summary>
        public bool fHasCDB;
 
    }
}
