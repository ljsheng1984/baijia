using System.IO;
using System.Net;
using System.Text;

namespace LJSheng.Web
{
    public static class KDAPI
    {
        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        /// <returns>请求结果</returns>
        public static string request()
        {
            string strURL = "http://apis.baidu.com/netpopo/express/express2";
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "GET";
            // 添加header
            request.Headers.Add("apikey", "39cae6fae5030d67b97d01feb32ea4d2");
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;
        }

        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        /// <param name="param">请求的参数</param>
        /// <returns>请求结果</returns>
        public static string requestKD(string param)
        {
            string strURL = "http://apis.baidu.com/netpopo/express/express1" + '?' + param;
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "GET";
            // 添加header
            request.Headers.Add("apikey", "39cae6fae5030d67b97d01feb32ea4d2");
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;
        }
    }
}