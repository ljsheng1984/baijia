using System;
using System.IO;
using System.Text;
using System.Web;
using LJSheng.WX;

namespace LJSheng.Web.weixin
{
    /// <summary>
    /// wxurl 的摘要说明
    /// </summary>
    public class wxurl : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string Token = System.Web.Configuration.WebConfigurationManager.AppSettings["Token"].ToString();
            string signature = context.Request["signature"];
            string timestamp = context.Request["timestamp"];
            string nonce = context.Request["nonce"];
            string echostr = context.Request["echostr"];

            if (context.Request.HttpMethod == "GET")
            {
                //get method - 仅在微信后台填写URL验证时触发
                if (CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    //Common.LogManager.WriteLog("开发者验证",echostr); //返回随机字符串则表示验证通过
                    context.Response.Output.Write(echostr);
                }
                else
                {
                    context.Response.Output.Write("failed:" + signature + "," + CheckSignature.GetSignature(timestamp, nonce, Token) + "。" +
                                "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。", "yzok");
                }
                context.Response.End();
            }
            else
            {
                string postString = string.Empty;
                using (Stream stream = HttpContext.Current.Request.InputStream)
                {
                    Byte[] postBytes = new Byte[stream.Length];
                    stream.Read(postBytes, 0, (Int32)stream.Length);
                    postString = Encoding.UTF8.GetString(postBytes);
                    Handle(postString);
                    //Common.LogManager.WriteLog("微信请求", postString);
                }
            }
        }

        #region 处理微信消息类型并应答
        /// <summary>
        /// 处理微信消息类型并应答
        /// <param name="postStr">微信的XML字符串</param> 
        /// </summary>
        public static void Handle(string postStr)
        {
            messageHelp help = new messageHelp();
            string responseContent = help.ReturnMessage(postStr);
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            //Common.LogManager.WriteLog("回复微信", responseContent);
            HttpContext.Current.Response.Write(responseContent);
        }
        #endregion

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}