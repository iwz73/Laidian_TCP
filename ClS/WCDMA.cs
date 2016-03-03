
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
//using System.DirectoryServices.Protocols;
//using System.ServiceModel.Security;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Collections;
using System.Threading;  
namespace ClS
{
    //串口接收事件,用于通知界面
    public class newGprsEventStr : EventArgs
    {
        public string OrderId;
        public int huodao;
        public string CDBNO;
        public RequestType EventType;
        public byte[] tys;
        public string userNikeName;
        public string userHeadPic;
        
    }
    public class WCDMA
    {
        public DateTime CommTime = DateTime.Now;
        private CookieContainer CC = new CookieContainer();
        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        private void BugFix_CookieDomain(CookieContainer cookieContainer)
        {
            System.Type _ContainerType = typeof(CookieContainer);
            Hashtable table = (Hashtable)_ContainerType.InvokeMember("m_domainTable",
                                       System.Reflection.BindingFlags.NonPublic |
                                       System.Reflection.BindingFlags.GetField |
                                       System.Reflection.BindingFlags.Instance,
                                       null,
                                       cookieContainer,
                                       new object[] { });
            ArrayList keys = new ArrayList(table.Keys);
            foreach (string keyObj in keys)
            {
                string key = (keyObj as string);
                if (key[0] == '.')
                {
                    string newKey = key.Remove(0, 1);
                    table[newKey] = table[keyObj];
                }
            }
        }

        private String DoGet(String url)
        {

            String data = "";
            HttpWebRequest webReqst = (HttpWebRequest)WebRequest.Create(url);
            webReqst.Method = "GET";
            webReqst.UserAgent = DefaultUserAgent;
            webReqst.KeepAlive = true;
            //如果在接收后想保存CookieContainer并再次提交（比如登录后操作）  
            //必须保证url的域名前面有www，比如http://www.a.com，  
            //而http://a.com是不行的，感觉是微软的bug  
            //webReqst.CookieContainer = CC;
            //if (_proxy != "")
            //{
            //    //webReqst.Proxy =;  
            //}
            webReqst.Timeout = 30000;

            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)webReqst.GetResponse();

                if (webResponse.StatusCode == HttpStatusCode.OK && webResponse.ContentLength < 1024 * 1024)
                {
                    StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8);
                 
                    data = reader.ReadToEnd();
                }
            }
            catch
            {

            }

            return data;
        }  
        public event EventHandler GprsDongEvent;//事件发布

        /// <summary>
        /// 由来电宝主机调用，用来向云端请求信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="strparams"></param>
        public string GetRequest(string loginUrl, IDictionary<string, string> parameters)
        {

            StringBuilder buffer = new StringBuilder();
            buffer.Append(loginUrl);
            int i = 0;
            foreach (string key in parameters.Keys)
            {
                if (i > 0)
                {
                    buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                }
                else
                {
                    buffer.AppendFormat("{0}={1}", key, parameters[key]);
                }
                i++;
            }

            string strhtml = "";
            
            strhtml = DoGet(buffer.ToString());
            //sLogs.Enqueue(DateTime.Now.ToShortTimeString() + ":向云端汇报状态失败");
            if (strhtml != "")
            {
                CommTime = DateTime.Now;
                
            }
            //重新拨号
            //if (CommTime.AddMinutes(5) < DateTime.Now)
            //{
            //    new Thread(() =>
            //    {
            //        int ic = 0;
            //        while (ic < 3)
            //        {
            //            try
            //            {
            //                NetWork("cdma", "连接");
            //            }
            //            catch
            //            {
            //            }
            //            Thread.Sleep(20000);
            //            ic++;
            //        }
            //    }).Start();
            //}
            return strhtml;

            //HttpWebResponse response = null;
            //try
            //{
            //    string stra = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)\r\n\r\n";
            //    response = HttpWebResponseUtility.CreatePostHttpResponse(loginUrl, parameters, 50, stra, Encoding.UTF8, null);
            //}
            //catch (WebException ex1)
            //{
            //    response = (HttpWebResponse)ex1.Response;
            //}
            //finally
            //{
            //    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            //    strhtml = sr.ReadToEnd();

            //}
            //return strhtml;

        }
    }
    /// <summary>  
    /// 有关HTTP请求的辅助类  
    /// </summary>  
    public class HttpWebResponseUtility
    {
        public static CookieContainer CC = new CookieContainer();
        //private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        public static void BugFix_CookieDomain(CookieContainer cookieContainer)
        {
            System.Type _ContainerType = typeof(CookieContainer);
            Hashtable table = (Hashtable)_ContainerType.InvokeMember("m_domainTable",
                                       System.Reflection.BindingFlags.NonPublic |
                                       System.Reflection.BindingFlags.GetField |
                                       System.Reflection.BindingFlags.Instance,
                                       null,
                                       cookieContainer,
                                       new object[] { });
            ArrayList keys = new ArrayList(table.Keys);
            foreach (string keyObj in keys)
            {
                string key = (keyObj as string);
                if (key[0] == '.')
                {
                    string newKey = key.Remove(0, 1);
                    table[newKey] = table[keyObj];
                }
            }
        }

        public static string DoGet(string url)
        {
            string data = "";
            HttpWebRequest webReqst = (HttpWebRequest)WebRequest.Create(url);
            webReqst.Method = "GET";
            webReqst.UserAgent = DefaultUserAgent;
            webReqst.KeepAlive = true;
            //如果在接收后想保存CookieContainer并再次提交（比如登录后操作）  
            //必须保证url的域名前面有www，比如http://www.a.com，  
            //而http://a.com是不行的，感觉是微软的bug  
            webReqst.CookieContainer = CC;
            //if (_proxy != "")
            //{
            //    //webReqst.Proxy =;  
            //}
            webReqst.Timeout = 30000;

            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)webReqst.GetResponse();

                if (webResponse.StatusCode == HttpStatusCode.OK && webResponse.ContentLength < 1024 * 1024)
                {
                    StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.Default);
                    data = reader.ReadToEnd();
                }
            }
            catch
            {

            }

            return data;
        }  
  
        public static string GetHTMLTCP(string URL)
        {
            string strHTML = "";//用来保存获得的HTML代码
            TcpClient clientSocket = new TcpClient();
            Uri URI = new Uri(URL);
            clientSocket.Connect(URI.Host, URI.Port);
            StringBuilder RequestHeaders = new StringBuilder();//用来保存HTML协议头部信息
            RequestHeaders.AppendFormat("{0} {1} HTTP/1.1\r\n", "GET", URI.PathAndQuery);
            RequestHeaders.AppendFormat("Connection:close\r\n");
            RequestHeaders.AppendFormat("Host:{0}\r\n", URI.Host);
            RequestHeaders.AppendFormat("Accept:*/*\r\n");
            RequestHeaders.AppendFormat("Accept-Language:zh-cn\r\n");
            RequestHeaders.AppendFormat("User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)\r\n\r\n");
            Encoding encoding = Encoding.Default;
            byte[] request = encoding.GetBytes(RequestHeaders.ToString());
            clientSocket.Client.Send(request);
            //获取要保存的网络流
            Stream readStream = clientSocket.GetStream();
            StreamReader sr = new StreamReader(readStream, Encoding.Default);
            strHTML = sr.ReadToEnd();


            readStream.Close();
            clientSocket.Close();

            return strHTML;
        }
        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public static HttpWebResponse CreateGetHttpResponse(string url, int? timeout, string userAgent, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request.GetResponse() as HttpWebResponse;
        }
        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int? timeout, string userAgent, Encoding requestEncoding, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (requestEncoding == null)
            {
                throw new ArgumentNullException("requestEncoding");
            }
            HttpWebRequest request = null;

            //System.Net.ServicePointManager.DefaultConnectionLimit = 50;

            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //如果需要POST数据  
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                    }
                    i++;
                }
                byte[] data = requestEncoding.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return request.GetResponse() as HttpWebResponse;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
    }  
}
