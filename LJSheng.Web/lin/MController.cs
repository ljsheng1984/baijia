using LJSheng.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace LJSheng.Web.Controllers
{
    public class MController : BaseController
    {
        protected override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            string ck = Common.LCookie.GetCookie("linjiansheng");
            bool Login = true;
            if (!string.IsNullOrEmpty(ck))
            {
                JObject json = JsonConvert.DeserializeObject(Common.DESRSA.DESDeljsheng(ck)) as JObject;
                Guid Gid = Guid.Parse(json["Gid"].ToString());
                using (EFDB db = new EFDB())
                {
                    var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                    if (b != null && b.LoginIdentifier == json["LoginIdentifier"].ToString())
                    {
                        Login = false;
                    }
                }
            }
            if(Login)
            { filterContext.HttpContext.Response.Redirect("/Home/Login"); }
        }
    }
}