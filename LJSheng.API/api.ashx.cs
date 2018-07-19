using System;
using System.IO;
using System.Text;
using System.Web;

namespace LJSheng.API
{
    /// <summary>
    /// api 的摘要说明
    /// </summary>
    public class api : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        /// <summary>
        /// 进入接口
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            if (!string.IsNullOrEmpty(context.Request.QueryString["Method"]))
            {
                //POST的参数
                StreamReader reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                string param = HttpUtility.UrlDecode(reader.ReadToEnd());
                string ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (String.IsNullOrEmpty(ip))
                {
                    ip = context.Request.ServerVariables["REMOTE_ADDR"];
                }
                context.Response.Write(toapi.ProcessRequest(context.Request.QueryString["Method"].ToLower(), ip, param));
            }
            #region 异常
            //try
            //{
               
            //}
            //catch (Exception err)
            //{
            //    Data.EF.EFDB db = new EFDB();
            //    string bugmsg = err.ToString();
            //    if (err.InnerException != null)
            //    {
            //        bugmsg = err.InnerException.Message;
            //    }
            //    apibug bug = new apibug()
            //    {
            //        gid = Guid.NewGuid(),
            //        rukusj = DateTime.Now,
            //        ffm = context.Request.QueryString["functionName"],
            //        mcheng = err.GetType().Name + "<hr />" + err.Message,
            //        xiaoxi = bugmsg,
            //        duizhai = err.Source + "<hr />" + err.StackTrace,
            //        canshu = "请求异常",
            //        deskey = null
            //    };
            //    db.apibug.Add(bug);
            //    db.SaveChanges();
            //    context.Response.Write("非法请求,你的IP已被记录:" + err);
            //}
            #endregion
        }
        /// <summary>
        /// 系统生成
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}