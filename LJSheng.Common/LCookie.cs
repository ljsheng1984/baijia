//-----------------------------------------------------------
// 描    述: Cookie 操作
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Web;

namespace LJSheng.Common
{
    public class LCookie
    {
        /// <summary>
        /// 添加Cookie
        /// </summary>
        /// <param name="CookieName">Cookie 名称</param>
        /// <param name="CookieValuer">Cookie 值</param>
        /// <param name="Day">Cookie 保存时间</param>
        public static void AddCookie(string CookieName, string CookieValuer, int Day)
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies[CookieName];
            if (ck != null)
            {
                //存在删除重新增加
                HttpContext.Current.Response.Cookies.Remove(CookieName);
            }
            ck = new HttpCookie(CookieName);
            ck.Value = StringTranscoding.Escape(CookieValuer);
            //ck.Domain = System.Web.Configuration.WebConfigurationManager.AppSettings["CookieDomain"].ToString();
            ck.Path = "/";
            if (Day != 0)
            {
                ck.Expires = DateTime.Now.AddDays(Day);
            }
            HttpContext.Current.Response.Cookies.Add(ck);
        }

        /// <summary>
        /// 读取Cookie
        /// </summary>
        /// <param name="CookieName">Cookie 名称</param>
        /// <returns></returns>
        public static string GetCookie(string CookieName)
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies[CookieName];
            if (ck == null)
            {
                return null;
            }
            else
            {
                return StringTranscoding.UnEscape(ck.Value.ToString());
            }
        }

        /// <summary>
        /// 清除Cookie
        /// </summary>
        /// <param name="CookieName">Cookie 名称</param>
        public static void DelCookie(string CookieName)
        {
            HttpCookie ck;
            ck = HttpContext.Current.Request.Cookies[CookieName];
            string Domain = System.Web.Configuration.WebConfigurationManager.AppSettings["CookieDomain"].ToString();
            if (HttpContext.Current.Request.ServerVariables.ToString().IndexOf(Domain) >= 0 && ck != null)
            {
                //ck.Domain = Domain;
                ck.Path = "/";
                ck.Expires = DateTime.Now.AddHours(-24); //关键是这一句
                HttpContext.Current.Response.Cookies.Add(ck);
            }
        }

        /// <summary>
        /// 删除整站Cookie
        /// </summary>
        /// <returns></returns>
        public static void DelALLCookie()
        {
            HttpCookie ck;
            string cookieName;
            int limit = HttpContext.Current.Request.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                cookieName = HttpContext.Current.Request.Cookies[i].Name;
                ck = new HttpCookie(cookieName);
                ck.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(ck);
            }
        }

        /// <summary>
        /// 读取城市Cookie
        /// </summary>
        /// <param name="CookieName">Cookie 名称</param>
        /// <returns></returns>
        public static string GetCity()
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies["city"];
            if (ck == null)
            {
                return "福州市";
            }
            else
            {
                return StringTranscoding.UnEscape(ck.Value.ToString());
            }
        }

        /// <summary>
        /// 读取抢单的城市Cookie
        /// </summary>
        /// <param name="CookieName">Cookie 名称</param>
        /// <returns></returns>
        public static string GetOrderCity()
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies["ordercity"];
            if (ck == null)
            {
                return null;
            }
            else
            {
                return StringTranscoding.UnEscape(ck.Value.ToString());
            }
        }

        /// <summary>
        /// 读取后台城市Cookie
        /// </summary>
        /// <param name="CookieName">Cookie 名称</param>
        /// <returns></returns>
        public static string GetAdminCity()
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies["AdminCity"];
            if (ck == null)
            {
                return "福州市";
            }
            else
            {
                return StringTranscoding.UnEscape(ck.Value.ToString());
            }
        }

        /// <summary>
        /// 读取用户访问的项目类型
        /// </summary>
        public static int Project()
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies["project"];
            if (ck == null)
            {
                return 0;
            }
            else
            {
                return int.Parse(StringTranscoding.UnEscape(ck.Value.ToString()));
            }
        }

        /// <summary>
        /// 获取登录用户的数据
        /// </summary>
        /// <returns></returns>
        public static string GetCK(string cookie, string field)
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies[cookie];
            JObject json = JsonConvert.DeserializeObject(DESRSA.DESDeljsheng(StringTranscoding.UnEscape(ck.Value.ToString()))) as JObject;
            return json[field].ToString();
        }

        /// <summary>
        /// 获取用户登录Cookie的GID
        /// </summary>
        /// <returns></returns>
        public static Guid GetMemberGid()
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies["linjiansheng"];
            if (ck != null)
            {
                JObject json = JsonConvert.DeserializeObject(DESRSA.DESDeljsheng(StringTranscoding.UnEscape(ck.Value.ToString()))) as JObject;
                return Guid.Parse(json["Gid"].ToString());
            }
            else
            {
                return Guid.Parse("00000000-0000-0000-0000-000000000000");
            }
        }

        /// <summary>
        /// 获取用户登录Cookie的商家GID
        /// </summary>
        /// <returns></returns>
        public static Guid GetShopGid()
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies["shop"];
            if (ck != null)
            {
                JObject json = JsonConvert.DeserializeObject(DESRSA.DESDeljsheng(StringTranscoding.UnEscape(ck.Value.ToString()))) as JObject;
                return Guid.Parse(json["Gid"].ToString());
            }
            else
            {
                return Guid.Parse("00000000-0000-0000-0000-000000000000");
            }
        }

        /// <summary>
        /// 获取管理员的信息
        /// </summary>
        /// <returns></returns>
        public static string Getljsheng(string field)
        {
            HttpCookie ck = HttpContext.Current.Request.Cookies["LJSheng"];
            JObject json = JsonConvert.DeserializeObject(DESRSA.DESDeljsheng(StringTranscoding.UnEscape(ck.Value.ToString()))) as JObject;
            return json[field].ToString();
        }
    }
}
