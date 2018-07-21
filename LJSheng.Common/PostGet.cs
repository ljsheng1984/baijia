using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LJSheng.Common
{
    public static class PostGet
    {
        /// <summary>
        /// POST
        /// </summary>
        /// <param name="posturl">请求URL</param>
        /// <param name="postData">请求请求的数据</param>
        /// <returns>返回查询的数据</returns>
        public static string Post(string posturl, SortedDictionary<string, string> sParaTemp)
        {
            using (var client = new HttpClient())
            {
                //var values = new List<KeyValuePair<string, string>>();
                //foreach (KeyValuePair<string, string> temp in sParaTemp)
                //{
                //    if (temp.Key != "api_key")
                //    {
                //        values.Add(new KeyValuePair<string, string>(temp.Key, temp.Value));
                //    }
                //}
                //var content = new FormUrlEncodedContent(values);
                //var response = await client.PostAsync(posturl, content);
                //var responseString = await response.Content.ReadAsStringAsync();
                //return responseString;

                //client.BaseAddress = new Uri(posturl);
                var values = new List<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> temp in sParaTemp)
                {
                    if (temp.Key != "api_key")
                    {
                        values.Add(new KeyValuePair<string, string>(temp.Key, temp.Value));
                    }
                }
                var content = new FormUrlEncodedContent(values);
                var result = client.PostAsync(posturl, content).Result;
                return result.Content.ReadAsStringAsync().Result;
            }
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="geturl">请求URL</param>
        /// <returns>返回查询的数据</returns>
        public static string Get(string geturl)
        {
            using (var client = new HttpClient())
            {
                var responseString = client.GetStringAsync(geturl).Result;
                return responseString;
            }
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="posturl">请求URL</param>
        /// <param name="postData">请求请求的数据</param>
        /// <returns>返回查询的数据</returns>
        public static string GetPage(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            byte[] data = encoding.GetBytes(HttpUtility.UrlEncode(postData));
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                //request.Headers.Add("Cookie", "Cookie 字符串值");
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                //Response.Write(content);
                return content;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
    }
}
