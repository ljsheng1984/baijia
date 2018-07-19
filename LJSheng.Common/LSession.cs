//-----------------------------------------------------------
// 描    述: Session操作
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LJSheng.Common
{
    public class LSession
    {
        /// <summary>
        /// 判断Session是否存在,存在返回访问的URL
        /// </summary>
        /// <returns></returns>
        public static string UrlSession(string SessionName)
        {
            if (HttpContext.Current.Session[SessionName] == null || HttpContext.Current.Session[SessionName].ToString() == "")
            {
                return null; 
            }
            else
            {
                return HttpContext.Current.Request.RawUrl;
            }
        }

        /// <summary>
        /// 判断Session是否存在,存在返回访问的Sessio的值
        /// </summary>
        /// <returns></returns>
        public static string GetSession(string SessionName)
        {
            if (HttpContext.Current.Session[SessionName] == null || HttpContext.Current.Session[SessionName].ToString() == "")
            {
                return null;
            }
            else
            {
                return HttpContext.Current.Session[SessionName].ToString();
            }
        }

        /// <summary>
        /// 判断Session是否存在,存在返回访问的Sessio的值
        /// </summary>
        /// <returns></returns>
        public static string GetJsonSession(string SessionName)
        {
            if (!string.IsNullOrEmpty(LCookie.GetCookie("ljsheng")) || HttpContext.Current.Session[SessionName]!= null ||!string.IsNullOrEmpty(HttpContext.Current.Session[SessionName].ToString()))
            {
                JObject paramJson = null;
                if (!string.IsNullOrEmpty(LCookie.GetCookie("ljsheng")))
                {
                    paramJson = JsonConvert.DeserializeObject(LJSheng.Common.DESRSA.DESDeljsheng(LCookie.GetCookie("ljsheng"))) as JObject;
                }
                else
                {
                    paramJson = JsonConvert.DeserializeObject(LJSheng.Common.DESRSA.DESDeljsheng(HttpContext.Current.Session[SessionName].ToString())) as JObject;
                }
                return paramJson["account"].ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
