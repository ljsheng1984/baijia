//-----------------------------------------------------------
// 创建描述: 微信接口页面
// 创建信息: 2014-07-21 林建生
//-----------------------------------------------------------
using System;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace LJSheng.WX
{
    public class Access_token
    {
        public Access_token()
        {
            //  
            //TODO: 在此处添加构造函数逻辑  
            //  
        }
        string _access_token;
        string _expires_in;

        /// <summary>  
        /// 获取到的凭证   
        /// </summary>  
        public string access_token
        {
            get { return _access_token; }
            set { _access_token = value; }
        }

        /// <summary>  
        /// 凭证有效时间，单位：秒  
        /// </summary>  
        public string expires_in
        {
            get { return _expires_in; }
            set { _expires_in = value; }
        }

        /// <summary> 
        /// 得到微信生成的Access_token
        /// </summary> 
        /// <param name="return">无返回值</param> 
        /// <param name="逻辑说明"></param> 
        /// <param>修改备注</param> 
        /// 2014-5-20 林建生
        /// 
        public static Access_token GetAccess_token()
        {
            string appid = System.Web.Configuration.WebConfigurationManager.AppSettings["AppId"].ToString();
            string secret = System.Web.Configuration.WebConfigurationManager.AppSettings["AppSecret"].ToString();
            string strUrl = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appid + "&secret=" + secret;
            Access_token mode = new Access_token();

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(strUrl);

            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();

                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);

                string content = reader.ReadToEnd();
                //Response.Write(content);  
                //在这里对Access_token 赋值  
                JObject jb = JObject.Parse(content);
                //string name = o["n"].ToString();
                //Response.Write(name);
                //Access_token token = new Access_token();
                //token = JsonHelper.ParseFromJson<Access_token>(content);
                mode.access_token = jb["access_token"].ToString();
                mode.expires_in = jb["expires_in"].ToString();
            }
            return mode;
        }

        /// <summary>  
        /// 根据当前日期 判断Access_Token 是否超期  如果超期返回新的Access_Token   否则返回之前的Access_Token  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>  
        public static string IsExistAccess_Token()
        {

            string Token = string.Empty;
            DateTime YouXRQ;
            // 读取XML文件中的数据，并显示出来 ，注意文件路径  
            string filepath = System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/wxxml.xml");

            StreamReader str = new StreamReader(filepath, System.Text.Encoding.UTF8);
            XmlDocument xml = new XmlDocument();
            xml.Load(str);
            str.Close();
            str.Dispose();
            Token = xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText;
            YouXRQ = Convert.ToDateTime(xml.SelectSingleNode("xml").SelectSingleNode("Access_YouXRQ").InnerText);

            if (DateTime.Now > YouXRQ)
            {
                DateTime _youxrq = DateTime.Now;
                Access_token mode = GetAccess_token();
                xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText = mode.access_token;
                _youxrq = _youxrq.AddSeconds(int.Parse(mode.expires_in));
                xml.SelectSingleNode("xml").SelectSingleNode("Access_YouXRQ").InnerText = _youxrq.ToString();
                xml.Save(filepath);
                Token = mode.access_token;
            }
            return Token;
        }
    }
}