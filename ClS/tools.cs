using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using System.Data;


namespace ClS
{
    public enum Logtype
    { 
        Query=1,//仓道查询
        J=2,//借出日志
        H=3,//归还
        Sale=4,//售线
        ReStart=5,//系统重启
        BX=6//补线日志

        
    }
    public class LoseCD
    {
        public DateTime ldate;
        public byte cdn;
      
    }
    public class XmlOperatorLog
    {


        public string ID { get; set; }

        public string datatime { get; set; }

        public string dianliang { get; set; }

        public string msg { get; set; }



        private static XDocument doc = new XDocument();

        public static string filePath = @".\data.xml";



        public XmlOperatorLog()
        {

            doc = XDocument.Load(filePath);

        }

        public XmlOperatorLog(string filepath)
            : this()
        {
            try
            {
                filePath = filepath;
                if (!File.Exists(filepath))
                {
                    File.Copy("data.xml", filepath, true);

                }
            }
            catch
            { }

        }



        /// <summary>

        /// 增

        /// </summary>

        /// <returns></returns>

        public bool Add()
        {

            XElement db = new XElement("DataBase",

            new XAttribute("id", ID),

            new XAttribute("datatime", datatime),

            new XAttribute("dianlian", dianliang),
            new XAttribute("msg", msg)



           );

            try
            {

                //用XElement的Add方法

                //XElement doc = XElement.Load(filePath);

                //doc.Add(db);



                //用XDocument的Add方法
                XElement xe = (from kb in doc.Element("DataBases").Elements("DataBase") where kb.Attribute("datatime").Value == datatime select kb).Single() as XElement;
                if (xe == null)
                {
                    doc.Element("DataBases").Add(db);

                    doc.Save(filePath);
                }
                return true;

            }

            catch
            {
                doc.Element("DataBases").Add(db);

                doc.Save(filePath);
                return false;

            }

        }

        /// <summary>

        /// 删

        /// </summary>

        /// <param name="id"></param>

        /// <returns></returns>

        public static bool Remove(string id)
        {

            XElement xe = (from db in doc.Element("DataBases").Elements("DataBase") where db.Attribute("id").Value == id select db).Single() as XElement;

            try
            {

                xe.Remove();

                doc.Save(filePath);

                return true;

            }

            catch
            {

                return false;



            }



        }

        /// <summary>

        /// 改

        /// </summary>

        /// <returns></returns>

        public bool Modify()
        {

            XElement xe = (from db in doc.Element("DataBases").Elements("DataBase") where db.Attribute("id").Value.ToString() == ID select db).Single();

            try
            {

                xe.Attribute("id").Value = ID;

                xe.Attribute("datatime").Value = datatime;

                xe.Attribute("dianliang").Value = dianliang;

                xe.Attribute("msg").Value = msg;



                doc.Save(filePath);

                return true;

            }

            catch
            {

                return false;

            }



        }

        /// <summary>

        /// 查

        /// </summary>

        /// <returns></returns>

        public List<XmlOperatorLog> GetAll()
        {



            List<XmlOperatorLog> dbs = (from db in doc.Element("DataBases").Elements("DataBase")

                                        select new XmlOperatorLog

                                        {

                                            ID = db.Attribute("id").Value.ToString(),

                                            datatime = db.Attribute("datatime").Value.ToString(),

                                            dianliang = db.Attribute("dianliang").Value.ToString(),

                                            msg = db.Attribute("msg").Value.ToString()

                                        }).ToList();

            return dbs;

        }
    }
    /// <summary>
    /// 线操作
    /// </summary>
    public class XmlOperatorLine
    {


        public string ID { get; set; }



        public int Num { get; set; }



        private static XDocument doc = new XDocument();

        public static string filePath = @".\line.xml";



        public XmlOperatorLine()
        {

            doc = XDocument.Load(filePath);

        }

        public XmlOperatorLine(string filepath)
            : this()
        {
            try
            {
                filePath = filepath;
                if (!File.Exists(filepath))
                {
                    File.Copy("line.xml", filepath, true);

                }
            }
            catch
            { }

        }



        /// <summary>

        /// 增

        /// </summary>

        /// <returns></returns>

        public bool Add()
        {

            XElement db = new XElement("DataBase",

            new XAttribute("id", ID),

            new XAttribute("num", Num)

       



           );

            try
            {

                //用XElement的Add方法

                //XElement doc = XElement.Load(filePath);

                //doc.Add(db);



                //用XDocument的Add方法
                XElement xe = (from kb in doc.Element("DataBases").Elements("DataBase") where kb.Attribute("id").Value == ID select kb).Single() as XElement;
                if (xe == null)
                {
                    doc.Element("DataBases").Add(db);

                    doc.Save(filePath);
                }
                return true;

            }

            catch
            {
                doc.Element("DataBases").Add(db);

                doc.Save(filePath);
                return false;

            }

        }

        /// <summary>

        /// 删

        /// </summary>

        /// <param name="id"></param>

        /// <returns></returns>

        public static bool Remove(string id)
        {

            XElement xe = (from db in doc.Element("DataBases").Elements("DataBase") where db.Attribute("id").Value == id select db).Single() as XElement;

            try
            {

                xe.Remove();

                doc.Save(filePath);

                return true;

            }

            catch
            {

                return false;



            }



        }

        /// <summary>

        /// 改

        /// </summary>

        /// <returns></returns>

        public bool Modify()
        {

            XElement xe = (from db in doc.Element("DataBases").Elements("DataBase") where db.Attribute("id").Value.ToString() == ID select db).Single();

            try
            {

                xe.Attribute("id").Value = ID;

                xe.Attribute("num").Value = Num.ToString();

     



                doc.Save(filePath);

                return true;

            }

            catch
            {

                return false;

            }



        }

        /// <summary>

        /// 查

        /// </summary>

        /// <returns></returns>

        public List<XmlOperatorLine> GetAll()
        {



            List<XmlOperatorLine> dbs = (from db in doc.Element("DataBases").Elements("DataBase")

                                        select new XmlOperatorLine

                                        {

                                            ID = db.Attribute("id").Value.ToString(),

                                            Num =int.Parse( db.Attribute("num").Value.ToString())

                                       

                                        }).ToList();

            return dbs;

        }
    }
    /// <summary>
    /// 订单操作
    /// </summary>
    public class XmlOperatorOrder
    {

        //订单状态0开始, 1失败，2成功
        public string status { get; set; }

        //时间
        public string datatime { get; set; }

        //充电宝编号
        public string cdbno { get; set; }

        //任务编号
        public string orderno { get; set; }
        
        //涉及的仓门号
        public string cmno { get; set; }

        //状态上传成功与否
        public string upserver { get; set; }
        //状态上传次数
        public string cs { get; set; }
        //订单ID，0借，1售线，2回收仓
        public string ordertype { get; set;}

        private static XDocument doc = new XDocument();


        public static string filePath = @"orderdata.xml";



        public XmlOperatorOrder()
        {

            doc = XDocument.Load(filePath);

        }

        public XmlOperatorOrder(string filepath)
            : this()
        {
            try
            {
                filePath = filepath;
                if (!File.Exists(filepath))
                {
                    File.Copy("order.xml", filepath, true);

                }
            }
            catch
            { }

        }



        /// <summary>

        /// 增

        /// </summary>

        /// <returns></returns>

        public bool Add()
        {
            try
            {
                XElement db = new XElement("order",
                new XAttribute("orderno", orderno),
                new XAttribute("datatime", datatime),
                new XAttribute("cdbno", cdbno),
                new XAttribute("cmno", cmno),
                new XAttribute("status", status),
                new XAttribute("upserver", upserver),
                new XAttribute("ordertype", ordertype),
                new XAttribute("cs", cs)

               );



                //用XElement的Add方法

                //XElement doc = XElement.Load(filePath);

                //doc.Add(db);



                //用XDocument的Add方法

                doc.Element("list").Add(db);

                doc.Save(filePath);

                return true;

            }

            catch
            {

                return false;

            }

        }

        /// <summary>

        /// 删

        /// </summary>

        /// <param name="id"></param>

        /// <returns></returns>

        public static bool Remove(string id)
        {

            XElement xe = (from db in doc.Element("list").Elements("order") where db.Attribute("orderno").Value ==id select db).Single() as XElement;

            try
            {

                xe.Remove();

                doc.Save(filePath);

                return true;

            }

            catch
            {

                return false;



            }



        }

        /// <summary>

        /// 改

        /// </summary>

        /// <returns></returns>

        public bool Modify()
        {

            List< XElement> ls = (from db in doc.Element("list").Elements("order") where db.Attribute("orderno").Value.ToString() == orderno select db).ToList();

            try
            {
                foreach (XElement xe in ls)
                {
                    // xe.Attribute("orderno").Value = orderno;
                    if (datatime != null && datatime != "")
                        xe.Attribute("datatime").Value = datatime;
                    if (status != null && status != "")
                        xe.Attribute("status").Value = status;
                    if (cmno != null && cmno != "")
                        xe.Attribute("cmno").Value = cmno;
                    if (cdbno != null && cdbno != "")
                        xe.Attribute("cdbno").Value = cdbno;
                    if (ordertype != null && ordertype != "")
                        xe.Attribute("ordertype").Value = ordertype;
                    if (upserver != null && upserver != "")
                        xe.Attribute("upserver").Value = upserver;
                    if (cs != null && cs != "")
                        xe.Attribute("cs").Value = upserver;
                  
                   
                }
                doc.Save(filePath);
                return true; 

            }
            catch
            {

                return false;

            }



        }

        /// <summary>

        /// 查

        /// </summary>

        /// <returns></returns>
        public List<XmlOperatorOrder> GetDate(string data,string type)
        {



            List<XmlOperatorOrder> dbs = (from db in doc.Element("list").Elements("order")
                                          where DateTime.Parse(db.Attribute("datatime").Value.ToString()) > DateTime.Now.AddDays(-30) && (db.Attribute("ordertype").Value.ToString() == type) && (db.Attribute("upserver").Value.ToString() == "0")// &&(db.Attribute("status").Value.ToString()=="1"||db.Attribute("status").Value.ToString()=="2")
                                          select new XmlOperatorOrder

                                          {

                                              orderno = db.Attribute("orderno").Value.ToString(),
                                              datatime = db.Attribute("datatime").Value.ToString(),
                                              cdbno = db.Attribute("cdbno").Value.ToString(),
                                              cmno = db.Attribute("cmno").Value.ToString(),
                                              status = db.Attribute("status").Value.ToString(),
                                              ordertype = db.Attribute("ordertype").Value.ToString(),
                                              upserver = db.Attribute("upserver").Value.ToString()




                                          }).ToList();

            return dbs;

        }
        public List<XmlOperatorOrder> GetAll()
        {



            List<XmlOperatorOrder> dbs = (from db in doc.Element("list").Elements("order")

                                          select new XmlOperatorOrder

                                          {

                                              orderno = db.Attribute("orderno").Value.ToString(),
                                              datatime = db.Attribute("datatime").Value.ToString(),
                                              cdbno = db.Attribute("cdbno").Value.ToString(),
                                              cmno=db.Attribute("cmno").Value.ToString(),
                                              status = db.Attribute("status").Value.ToString(),
                                              ordertype = db.Attribute("ordertype").Value.ToString(),
                                              upserver = db.Attribute("upserver").Value.ToString()




                                          }).ToList();

            return dbs;

        }
    }
    public class tools
    {
        public static void InsertCwgRecord(CUWUGUI cwg)
        {
            string insertsql = "INSERT INTO cd_cdb(data_time,cd_no,cdb_isNull,cdb_no,cdb_power,cdb_temp,cdb_is_in_power,cdb_use_cnt,cd_temp,cd_Adl,cd_5V,cd_12V,cd_status) VALUES "
            +"('{0}',{1},{2},'{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12})";
            string data_time=DateTime.Now.ToString();
            string cd_no="0";
            string cdb_isNull="0";
            string cdb_no="00000000000000000000";
            string cdb_power="0";
            string cdb_temp="0";
            string cdb_is_in_power="0";
            string cdb_use_cnt="0";
            string cd_temp="0";
            string cd_Adl="0";
            string cd_5V="0";
            string cd_12V="0";
            string cd_status = "0";
            cd_no = cwg.CWGID;
            cd_temp = cwg.Temp.ToString();
            cd_12V = cwg.V12.ToString();
            cd_5V = cwg.V5.ToString();
            cd_Adl = cwg.Adl.ToString();
            cd_status = cwg.errorflag.ToString();
         
            if(cwg.CWGStatus==CUWUGUISTATUS.None)
            {
                cdb_isNull="1";
            }
            if (cwg.CWGStatus == CUWUGUISTATUS.SetingPower)
            {
                cdb_is_in_power = "1";
            }
            if(cwg.CDB!=null)//
            { 
                Chongdianbao cdb=cwg.CDB;
                cdb_no = cdb.CDBNO;
                cdb_power = cdb.PowerDeep.ToString();
                cdb_temp = cdb.temp.ToString();
                cdb_use_cnt = cdb.UseCnt.ToString();
            }
            string sql = string.Format(insertsql, data_time, cd_no, cdb_isNull, cdb_no, cdb_power, cdb_temp, cdb_is_in_power, cdb_use_cnt, cd_temp, cd_Adl, cd_5V, cd_12V, cd_status);
            laidian.DbHelperSQL.ExecuteSql(sql);
            
        }
        public static DataSet SelectLog(DateTime st, DateTime et, Logtype logtype)
        {
            string strst = st.ToString("s");
            string stret = et.ToString("s");
            int f = (int)logtype;
            DataSet dt = laidian.DbHelperSQL.Query("select * from all_log where logtype="+f.ToString()+" and logdate<='" + stret + "' and logdate>='"+strst+"' order by logdate  desc");
            return dt;
        }
        public static void insertLog(string slog, Logtype flag)
        {
            try
            {
                int f = (int)flag;
                //laidian.DbHelperSQL.ExecuteSql("insert into all_log(logdate,logdesc,logtype)values('" + DateTime.Now.ToString("s") + "','" + slog + "'," + f.ToString() + ")");
            }
            catch
            { }
        }
        public static void DelLog()
        {
            try
            {
                laidian.DbHelperSQL.ExecuteSql("delete from all_log where logdate<'" + DateTime.Now.AddDays(-10).ToString("s") + "')");
                laidian.DbHelperSQL.ExecuteSql("delete from cd_cdb where data_time<'" + DateTime.Now.AddDays(-10).ToString("s") + "')");
            }
            catch
            { }
        }
        public static void DelFile()
        {
            try
            {
                if (Directory.Exists(System.Environment.CurrentDirectory + @"\record"))
                {
                    DirectoryInfo folder = new DirectoryInfo(System.Environment.CurrentDirectory + @"\record");

                    foreach (FileInfo file in folder.GetFiles("*.txt"))
                    {
                        DateTime tmp = DateTime.Now;
                        if (file.Name.Length == 12)
                        {
                            string st = file.Name.Substring(0, 4) + "-" + file.Name.Substring(4, 2) + "-" + file.Name.Substring(6, 2);
                            if (DateTime.TryParse(st, out tmp))
                            {
                                if (tmp.AddDays(30) < DateTime.Now)
                                {
                                    File.Delete(System.Environment.CurrentDirectory + @"\record\" + file.Name);
                                }
                            }
                        }
                    }
                }
            }
            catch
            { }
        
        }
        public static void CreatText()
        {
            if (!Directory.Exists(System.Environment.CurrentDirectory + @"\record"))
                Directory.CreateDirectory(System.Environment.CurrentDirectory + @"\record");
            string txtname = System.Environment.CurrentDirectory + @"\record\" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + ".txt";
            if (!File.Exists(txtname))
            {
                File.Create(txtname);
            }
        }
        public static void WriteText(Queue<string>strs)
        {
            string txtname = System.Environment.CurrentDirectory + @"\record\" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + ".txt";
            //string str = File.ReadAllText(txtname);
            StreamWriter sw = File.AppendText(txtname);
            
            try
             {
                 while (strs.Count > 0)
                 {
                     sw.Write("\r\n"+strs.Dequeue());
                     sw.Flush();
                     //Thread.Sleep(1000);
                     //File.AppendText((strs.Dequeue(),);
                 }
                 //File.AppendAllLines(txtname, strs);
                 //Thread.Sleep(2000);
                 
                 //string ss="";
                 //foreach (string st in strs)
                 //    ss+=st+"\r\n";
                 //    sw.WriteLine(str+ss);
             }
             catch
             {
             }
             finally
             { 
                
                sw.Close();
                 sw.Dispose();
             
                //File. sw.Close();
                 
             }
        }
        public static byte Crc(byte[] cmd,int start,int end)
        {
            byte bb = 0x00;
            for (int i = start; i < end; i++)
            {
                bb ^= cmd[i];
            }
            return bb;
        }
        [DllImport("Kernel32.dll")]
        public static extern bool SetSystemTime(ref SystemTime sysTime);

        [DllImport("Kernel32.dll")]
        public static extern void GetSystemTime(ref SystemTime sysTime);

        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);

        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime sysTime);
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }
        /// <summary> 
        /// 设置本机时间 
        /// </summary> 
        public static bool SyncTime(DateTime currentTime)
        {
            bool flag = false;
            SystemTime sysTime = new SystemTime();
            sysTime.wYear = Convert.ToUInt16(currentTime.Year);
            sysTime.wMonth = Convert.ToUInt16(currentTime.Month);
            sysTime.wDay = Convert.ToUInt16(currentTime.Day);
            sysTime.wHour = Convert.ToUInt16(currentTime.Hour);
            sysTime.wMinute = Convert.ToUInt16(currentTime.Minute);
            sysTime.wSecond = Convert.ToUInt16(currentTime.Second);
            try
            {
                flag = SetLocalTime(ref sysTime);
            }
            catch (Exception e)
            {
                Console.WriteLine("SetSystemDateTime函数执行异常" + e.Message);
            }
            return flag;
        }
        public static string GetNowTime()
        {
            string str = DateTime.Now.Year.ToString()
                + DateTime.Now.Month.ToString("D2")
                + DateTime.Now.Day.ToString("D2")
                + DateTime.Now.Hour.ToString("D2")
                + DateTime.Now.Minute.ToString("D2")
                + DateTime.Now.Second.ToString("D2");
            return str;

        }

        public static DateTime GetServerTime(string strtime)
        {
            DateTime dtime = DateTime.Now;
            try
            {
                string stryear = strtime.Substring(0, 4);
                string strmonth = strtime.Substring(4, 2);
                string strday = strtime.Substring(6, 2);
                string strhour = strtime.Substring(8, 2);
                string strminute = strtime.Substring(10, 2);
                string strsecond = strtime.Substring(12, 2);
                DateTime.TryParse(stryear + "-" + strmonth + "-" + strday + " " + strhour + ":" + strminute + ":" + strsecond, out dtime);
            }
            catch
            { }
            return dtime;

        }
    }
    /// <summary>
    /// 握手后服务器回复的数据包
    /// </summary>
    public class TestLinkServerData
    {
        public TestLinkServerData()
        { }
        #region Model
        private int _type;
        private string _terminal;
        private string _status;
        private string _time;
  
        /// <summary>
        /// 
        /// </summary>
        public int type
        {
            set { _type = value; }
            get { return _type; }
        }
        /// <summary>
        /// 平台编号
        /// </summary>
        public string terminal
        {
            set { _terminal = value; }
            get { return _terminal; }
        }
        /// <summary>
        /// 服务器编号
        /// </summary>
        public string status
        {
            set { _status = value; }
            get { return _status; }
        }
        /// <summary>
        /// 原始服务器编号
        /// </summary>
        public string time
        {
            set { _time = value; }
            get { return _time; }
        }
       #endregion
    }
    /// <summary>
    /// 成功：{"user":{"age":18,"lastLoginTime":"1970-01-01 00:00:00","lat":0,"lng":0,"mobile":"18680077777"
    /// ,"money":0,"nickName":"哈哈","phonecode":"","pushToken":"",
    /// "registerTime":"2014-11-02 16:50:53","resume":"","sex":0,"source":0,
    /// "tokenType":0,"updateTime":"2014-11-18 11:19:10","userType":1},
    /// "access_token":"tewdajbntawdajhxi3nr43dfl3w5typ6tfytB","msg":"成功","result":1}
	///	失败：{"msg":"登录失败","result":-1}
	///		{"msg":"请使用管理员账号登录","result":-1}
    /// </summary>
    public class User
    {
        public int age;
        public string lastLoginTime;
        public float lat;
        public float lng;
        public string mobile;
        public float money;
        public string nickName;
        public string phonecode;
        public string pushToken;
        public string registerTime;
        public string resume;
        public int sex;
        public int source;
        public int tokenType;
        public string updateTime;
        public int userType;
    }

    /// <summary>
    /// 管理员登陆服务器回复的数据包
    /// </summary>
    public class Admin
    {
        public Admin()
        { }
        #region Model
 
        public User user;
        public string  access_token;
        public string msg;
        public int result;
     
        #endregion
    }

    /// <summary>
    ///查询是否有需要租借任务回复的数据包
    /// </summary>
    public class QueryRecordServerData
    {
        #region json数据类型
        public QueryRecordServerData()
        { }
    
        private int _type;
        private string _terminal;
        private string _status;
        private string _time;

        /// <summary>
        /// 
        /// </summary>
        public int type
        {
            set { _type = value; }
            get { return _type; }
        }
        /// <summary>
        /// 平台编号
        /// </summary>
        public string terminal
        {
            set { _terminal = value; }
            get { return _terminal; }
        }
        /// <summary>
        /// 服务器编号
        /// </summary>
        public string status
        {
            set { _status = value; }
            get { return _status; }
        }
        /// <summary>
        /// 原始服务器编号
        /// </summary>
        public string time
        {
            set { _time = value; }
            get { return _time; }
        }
        #endregion
    }

    /// <summary>
    ///查询是否有需要租借任务回复的数据包
    ///2015-9-7修改：加入用户昵称和头像
    /// </summary>
    public class QueryWorkServerData
    {
        #region json数据类型
        public QueryWorkServerData()
        {
        }

        // {"status":true,"taskId":450,"time":"","CDB":"11075627100814000000","work":"121313"}
        
        private bool _status;
        private int _taskId;
        private string _time;
        private string _work; 
        private string _cdb;
        //
        private string _userNickName;

        private string _userHeadPic;
 
        /// <summary>
        /// 服务器编号
        /// </summary>
        public bool status
        {
            set { _status = value; }
            get { return _status; }
        }
        public int taskId
        {
            set { _taskId = value; }
            get { return _taskId; }
        }
        /// <summary>
        /// 原始服务器编号
        /// </summary>
        public string time
        {
            set { _time = value; }
            get { return _time; }
        }


        /// <summary>
        /// 平台编号
        /// </summary>
        public string CDB
        {
            set { _cdb = value; }
            get { return _cdb; }
        }    
        public string work
        {
              set{ _work = value; }
            get{ return _work; }
        }

        public string userNickName
        {
            set { _userNickName=value; }
            get { return _userNickName; }
        }

        public string userHeadPic
        {
            set { _userHeadPic = value; }
            get { return _userHeadPic; }
        }
        #endregion
    }
    //“addTime”:”2015-03-20 16:06:16”,”deviceTerminal”:”000110000008”,”id”:1,”status”:9,”updateTime”:”2015-03-20 16:06:16”,”userId”:1000000016
    public class garbage
    {
        public DateTime addTime;
        public string deviceTerminal;
        public int id;
        public int status;
        public DateTime updateTime;
        public  int userId;
    }
    public class GarbageTask
    {
        #region json数据类型
        public GarbageTask()
        {
        }

        // {"status":true,"taskId":450,"time":"","CDB":"11075627100814000000","work":"121313"}
       public garbage garbageTask;
       public  bool status;
       
       public  string time;
   

        #endregion
    }


    /// <summary>
    /// 状态回复服务器后得到的回复数据包
    /// </summary>
    public class TestNetResponseData
    {
        #region json数据类型
        public TestNetResponseData()
        { }

        //private bool _status;
        //private int _taskId;
        //private string _time;
        //private string _work;
        //private string _cdb;



        ///// <summary>
        ///// 服务器编号
        ///// </summary>
        //public bool status
        //{
        //    set { _status = value; }
        //    get { return _status; }
        //}
        //public int taskId
        //{
        //    set { _taskId = value; }
        //    get { return _taskId; }
        //}
        ///// <summary>
        ///// 原始服务器编号
        ///// </summary>
        //public string time
        //{
        //    set { _time = value; }
        //    get { return _time; }
        //}


        ///// <summary>
        ///// 平台编号
        ///// </summary>
        //public string CDB
        //{
        //    set { _cdb = value; }
        //    get { return _cdb; }
        //}
        //public string work
        //{
        //    set { _work = value; }
        //    get { return _work; }
        //}


        private bool _status;
        private Int64 _time;


        /// <summary>
        /// 服务器编号
        /// </summary>
        public bool status
        {
            set { _status = value; }
            get { return _status; }
        }
        /// <summary>
        /// 原始服务器编号
        /// </summary>
        public Int64 time
        {
            set { _time = value; }
            get { return _time; }
        }
        #endregion
    }
    /// <summary>
    /// 状态回复服务器后得到的回复数据包
    /// </summary>
    public class ResponseServerData
    {
        #region json数据类型
        public ResponseServerData()
        { }

        //private bool _status;
        //private int _taskId;
        //private string _time;
        //private string _work;
        //private string _cdb;



        ///// <summary>
        ///// 服务器编号
        ///// </summary>
        //public bool status
        //{
        //    set { _status = value; }
        //    get { return _status; }
        //}
        //public int taskId
        //{
        //    set { _taskId = value; }
        //    get { return _taskId; }
        //}
        ///// <summary>
        ///// 原始服务器编号
        ///// </summary>
        //public string time
        //{
        //    set { _time = value; }
        //    get { return _time; }
        //}


        ///// <summary>
        ///// 平台编号
        ///// </summary>
        //public string CDB
        //{
        //    set { _cdb = value; }
        //    get { return _cdb; }
        //}
        //public string work
        //{
        //    set { _work = value; }
        //    get { return _work; }
        //}

          public QueryWorkServerData task;
          private bool _status;
          private string _time;
          public string msg;

          /// <summary>
          /// 服务器编号
          /// </summary>
          public bool status
          {
              set { _status = value; }
             get { return _status; }
          }
          /// <summary>
          /// 原始服务器编号
          /// </summary>
          public string time
          {
              set { _time = value; }
              get { return _time; }
          }
        #endregion
    }

 
    /// <summary>
    /// 状态回复服务器后得到的回复数据包
    /// status":true,"lineType":1,"taskId":1432,"time":"20140526152345"
    /// </summary>
    public class ResponseLineData
    {
        #region json数据类型
        public ResponseLineData()
        { }
        private int _taskId;
        private bool _status;
        private string _time;

        private int _lineType;
        /// <summary>
        /// 服务器编号
        /// </summary>
        public bool status
        {
            set { _status = value; }
            get { return _status; }
        }
        /// <summary>
        /// 原始服务器编号
        /// </summary>
        public string time
        {
            set { _time = value; }
            get { return _time; }
        }
        public int lineType
        {
            set { _lineType = value; }
            get { return _lineType; }
        }
        public int taskId
        {
            set { _taskId = value; }
            get { return _taskId; }
        }
        #endregion
    }


    ///
    /// <summary>
    /// 回收状态回复服务器后得到的回复数据包
    /// status":true,"lineType":1,"taskId":1432,"time":"20140526152345"
    /// </summary>
    public class ResponseHuiShouData
    {
        #region json数据类型
        public ResponseHuiShouData()
        { }
        private string _taskId;
        private bool _status;
        private string _terminal;

   
        /// <summary>
        /// 服务器编号
        /// </summary>
        public bool status
        {
            set { _status = value; }
            get { return _status; }
        }

        public string terminal
        {
            set { _terminal = value; }
            get { return _terminal; }
        }
        public string taskId
        {
            set { _taskId = value; }
            get { return _taskId; }
        }
        #endregion
    }
}