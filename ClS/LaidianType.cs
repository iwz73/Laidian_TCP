using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClS
{
    /// <summary>
    /// 来电宝终端当前所处的动作
    /// </summary>
    public enum TerminalAction
    {
        Query,
        Huan,
        Jie

    }
    public class ReponseWorkEvent
    {
        public string workNo;
        public int huodao;
        public bool workstatus;
        public string CDB;
        public bool NeedToCheck;
        public int linetype;
    }
    /// <summary>
    /// 向云端请求连接的类型
    /// </summary>
    public enum RequestType
    {
        TestLink,  //与云端的握手请求回复
        QueryRcode,//查询是否有需要生成二维码的请求
        QueryWork, //查询是否有需要生成二维码的请求G
        GuiHuanRequest,
        SaleRequest,
        ZujieRequest,
        ResponseStatus,
        Reset,
        QueryError
    }
    /// <summary>
    /// 来电吧状态,三级状态，0为OK，1级状态为故障，但仍可运行，2级为比较严重需要检修，3级为紧急，应当立刻派人去检修
    /// </summary>
    public enum LDBStatus
    {
        OK = 1,
        OneLevel,
        TwoLevel,
        ThreeLevel

    }
    public enum TermialStatus
    {
        NetError = 0,
        NetOk = 3,
        ComError = 8,
        ComOK = 12
    }

    /// <summary>
    /// 界面更新类
    /// </summary>
    public class EventUIInit : EventArgs
    {
        public TermialStatus UIType;
        public DateTime datatime;
        public string Msg;
    }
    /// <summary>
    /// 界面更新类
    /// </summary>
    public class EventUI : EventArgs
    {
        public UIUpdateType UIType;
        public string huodao;
        public bool Flag;
        public object Msg;
    }
    /// <summary>
    /// 租借或购买订单，尝试三次失败就会取消
    /// </summary>
    public class JSLdbOrder
    {
        public string OrderNo;
        public int iTryCnt;
        public bool status;
        public int huodao;
        public byte[] scmd;
        public string CDB;
        public UIUpdateType nowstatus;
    }
    public enum UIUpdateType
    {
        ProceStart,//流程开始
        RecodeUpdate,//更新二维码
        CWGUpdate,//更新储物柜界面
        ChuHuoReady,//任务已经下载
        ChuHuoING,//出货中请等待
        ChuHuoF,//出货完成，流程结束
        ChuHuoL,//出货失败
        ChuHuoWait,//出货等待取走等待30秒

        SaleING,//售线出货中请等待
        SaleF,//售线出货完成，流程结束
        SaleL,//售线出货失败

        PutInting,//请放回充电宝等待30秒
        PutInting_check,//正在检测充电宝，请勿走开
        PutIntSuccess,//放入检测成功
        PutIntFalse,//放入检测失败
        StatusUpdate,//更新状态栏
        StartGH,//请求归还开始页面
        StartZJ,//请求租借
        StartGM,//请求购买
        SetAddrS,//设置地址成功
        SetAddrF,//设置地址失败
        KCS,//舱门打开成功
        KCF,//舱门打开失败
        GCS,//舱门关闭成果
        GCF,//舱门关闭失败
        NoResponse,//无回复
        QueryError,//错误代码
        Reset,//重启，
        BtnEnable,//借和还按钮可以使用
        HCBtnEnable,//管理的循环借和还按钮可以使用
        NetError,//网络失败
        Comm1Error,//仓门串口通信错误
        Comm2Error,//主控板通信错误

        ParamS,//参数下发成功
        ParamF,//参数下发失败

        RParamS,//读取参数成功
        RParamF//读取参数失败
    }
}
