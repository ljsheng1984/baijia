using LJSheng.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace LJSheng.Web.Controllers
{
    public class LJSController : BaseController
    {
        protected override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            string ck = Common.LCookie.GetCookie("ljsheng");
            if (string.IsNullOrEmpty(ck))
            {
                if (Request.RawUrl != "/LJSheng/Login" && Request.RawUrl.IndexOf("/LJSheng/Page") == -1)
                {
                    filterContext.HttpContext.Response.Redirect("/LJSheng/Login");
                }
            }
            else
            {
                using (EFDB db = new EFDB())
                {
                    JObject json = JsonConvert.DeserializeObject(Common.DESRSA.DESDeljsheng(ck)) as JObject;
                    Guid Gid = Guid.Parse(json["Gid"].ToString());
                    var b = db.LJSheng.Where(l => l.Gid == Gid).FirstOrDefault();
                    if (b == null || b.LoginIdentifier != json["LoginIdentifier"].ToString() || b.Jurisdiction == "冻结")
                    {
                        filterContext.HttpContext.Response.Redirect("/LJSheng/Login");
                    }
                }
            }
        }
    }
}