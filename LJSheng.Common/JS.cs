//-----------------------------------------------------------
// 描    述: 一些常用的Js调用
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
// 采用ClientScript.RegisterStartupScript(string msg)的方式输出，不会改变xhtml的结构, 
// 不会影响执行效果。 
// 为了向下兼容，所以采用了重载的方式，要求一个System.Web.UI.Page类的实例。 
//-----------------------------------------------------------
using System.Web;
using System.Web.UI;

namespace LJSheng.Common
{
    public class JS
    {
        #region 一些常用的Js调用,要求一个System.Web.UI.Page类的实例
        /// <summary> 
        /// 弹出JavaScript小窗口 
        /// </summary> 
        /// <param name="js">窗口信息</param> 
        public static void Alert(string message, Page page)
        {
            #region
            string js = @"<Script language='JavaScript'> 
                    alert('" + message + "');</Script>";
            if (!page.ClientScript.IsStartupScriptRegistered(page.GetType(), "alert"))
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), "alert", js);
            }
            #endregion
        }
        /// <summary> 
        /// 弹出消息框并且转向到新的URL 
        /// </summary> 
        /// <param name="message">消息内容</param> 
        /// <param name="toURL">连接地址</param> 
        public static void AlertAndRedirect(string message, string toURL, Page page)
        {
            #region
            string js = "<script language=javascript>alert('{0}');window.location.replace('{1}')</script>";
            if (!page.ClientScript.IsStartupScriptRegistered(page.GetType(), "AlertAndRedirect"))
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), "AlertAndRedirect", string.Format(js, message, toURL));
            }
            #endregion
        }
        /// <summary> 
        /// 回到历史页面 
        /// </summary> 
        /// <param name="value">-1/1</param> 
        public static void GoHistory(int value, Page page)
        {
            #region
            string js = @"<Script language='JavaScript'> 
                    history.go({0});   
                  </Script>";
            if (!page.ClientScript.IsStartupScriptRegistered(page.GetType(), "GoHistory"))
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), "GoHistory", string.Format(js, value));
            }
            #endregion
        }
        /// <summary> 
        /// 关闭当前窗口 
        /// </summary> 
        public static void CloseWindow()
        {
            #region
            string js = @"<Script language='JavaScript'> 
                            parent.opener=null;window.close();   
                          </Script>";
            HttpContext.Current.Response.Write(js);
            HttpContext.Current.Response.End();
            #endregion
        }
        /// <summary> 
        /// 刷新父窗口 
        /// </summary> 
        public static void RefreshParent(string url, Page page)
        {
            #region
            string js = @"<Script language='JavaScript'> 
                    window.opener.location.href='" + url + "';window.close();</Script>";
            if (!page.ClientScript.IsStartupScriptRegistered(page.GetType(), "RefreshParent"))
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), "RefreshParent", js);
            }
            #endregion
        }

        /// <summary> 
        /// 刷新打开窗口 
        /// </summary> 
        public static void RefreshOpener(Page page)
        {
            #region
            string js = @"<Script language='JavaScript'> 
                    opener.location.reload(); 
                  </Script>";
            if (!page.ClientScript.IsStartupScriptRegistered(page.GetType(), "RefreshOpener"))
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), "RefreshOpener", js);
            }
            #endregion
        }

        /// <summary> 
        /// 打开指定大小的新窗体 
        /// </summary> 
        /// <param name="url">地址</param> 
        /// <param name="width">宽</param> 
        /// <param name="heigth">高</param> 
        /// <param name="top">头位置</param> 
        /// <param name="left">左位置</param> 
        public static void OpenWebFormSize(string url, int width, int heigth, int top, int left, Page page)
        {
            #region
            string js = @"<Script language='JavaScript'>window.open('" + url + @"','','height=" + heigth + ",width=" + width + ",top=" + top + ",left=" + left + ",location=no,menubar=no,resizable=yes,scrollbars=yes,status=yes,titlebar=no,toolbar=no,directories=no');</Script>";
            if (!page.ClientScript.IsStartupScriptRegistered(page.GetType(), "OpenWebFormSize"))
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), "OpenWebFormSize", js);
            }
            #endregion
        }

        /// <summary> 
        /// 转向Url制定的页面 
        /// </summary> 
        /// <param name="url">连接地址</param> 
        public static void JavaScriptLocationHref(string url, Page page)
        {
            #region
            string js = @"<Script language='JavaScript'> 
                    window.location.replace('{0}'); 
                  </Script>";
            js = string.Format(js, url);
            if (!page.ClientScript.IsStartupScriptRegistered(page.GetType(), "JavaScriptLocationHref"))
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), "JavaScriptLocationHref", js);
            }
            #endregion
        }
        /// <summary> 
        /// 打开指定大小位置的模式对话框 
        /// </summary> 
        /// <param name="webFormUrl">连接地址</param> 
        /// <param name="width">宽</param> 
        /// <param name="height">高</param> 
        /// <param name="top">距离上位置</param> 
        /// <param name="left">距离左位置</param> 
        public static void ShowModalDialogWindow(string webFormUrl, int width, int height, int top, int left, Page page)
        {
            #region
            string features = "dialogWidth:" + width.ToString() + "px"
                + ";dialogHeight:" + height.ToString() + "px"
                + ";dialogLeft:" + left.ToString() + "px"
                + ";dialogTop:" + top.ToString() + "px"
                + ";center:yes;help=no;resizable:no;status:no;scroll=yes";
            ShowModalDialogWindow(webFormUrl, features, page);
            #endregion
        }
        /// <summary> 
        /// 弹出模态窗口 
        /// </summary> 
        /// <param name="webFormUrl"></param> 
        /// <param name="features"></param> 
        public static void ShowModalDialogWindow(string webFormUrl, string features, Page page)
        {
            string js = ShowModalDialogJavascript(webFormUrl, features);
            if (!page.ClientScript.IsStartupScriptRegistered(page.GetType(), "ShowModalDialogWindow"))
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), "ShowModalDialogWindow", js);
            }
        }
        /// <summary> 
        /// 弹出模态窗口 
        /// </summary> 
        /// <param name="webFormUrl"></param> 
        /// <param name="features"></param> 
        /// <returns></returns> 
        public static string ShowModalDialogJavascript(string webFormUrl, string features)
        {
            #region
            string js = @"<script language=javascript>showModalDialog('" + webFormUrl + "','','" + features + "');</script>";
            return js;
            #endregion
        }
        #endregion 一些常用的Js调用,要求一个System.Web.UI.Page类的实例

        # region Javascript常用方法，不要求一个System.Web.UI.Page类的实例

        private static string ScriptStart = "<script type=\"text/javascript\">";
        private static string ScriptEnd = "</script>";


        /// <summary> 
        /// 写入JS脚本内容 
        /// </summary> 
        /// <param name="ScriptString">脚本内容</param> 
        /// <param name="IsResponseEnd">是否中断服务端脚本执行</param> 
        public static void WriteScript(string ScriptString, bool IsResponseEnd)
        {
            HttpContext.Current.Response.Write(ScriptStart);
            HttpContext.Current.Response.Write(ScriptString);
            HttpContext.Current.Response.Write(ScriptEnd);
            if (IsResponseEnd)
            {
                HttpContext.Current.Response.End();
            }
        }

        /// <summary> 
        /// 弹出警告框 
        /// </summary> 
        /// <param name="AlertMessage">提示信息</param> 
        /// <param name="IsResponseEnd">是否中断服务端脚本执行</param> 
        public static void Alert(string AlertMessage, bool IsResponseEnd)
        {
            HttpContext.Current.Response.Write(ScriptStart);
            HttpContext.Current.Response.Write("alert('" + AlertMessage + "');history.back();");
            HttpContext.Current.Response.Write(ScriptEnd);
            if (IsResponseEnd)
            {
                HttpContext.Current.Response.End();
            }
        }

        /// <summary> 
        /// 弹出警告框并跳转 
        /// </summary> 
        /// <param name="AlertMessage">提示信息</param> 
        /// <param name="RedirectPath">跳转路径</param> 
        /// <param name="IsTopWindow">是否为全屏跳转</param> 
        public static void Alert(string AlertMessage, string RedirectPath, bool IsTopWindow)
        {
            HttpContext.Current.Response.Write(ScriptStart);
            HttpContext.Current.Response.Write("alert('" + AlertMessage + "');");
            if (IsTopWindow)
            {
                HttpContext.Current.Response.Write("parent.top.location.href='" + RedirectPath + "';");
            }
            else
            {
                HttpContext.Current.Response.Write("location.href='" + RedirectPath + "';");
            }
            HttpContext.Current.Response.Write(ScriptEnd);
            HttpContext.Current.Response.End();
        }

        /// <summary> 
        /// 弹出警告框并关闭窗口 
        /// </summary> 
        /// <param name="AlertMessage">提示信息</param> 
        public static void AlertAndClose(string AlertMessage)
        {
            HttpContext.Current.Response.Write(ScriptStart);
            HttpContext.Current.Response.Write("alert('" + AlertMessage + "');window.close();");
            HttpContext.Current.Response.Write(ScriptEnd);
            HttpContext.Current.Response.End();
        }

        /// <summary> 
        /// 全屏跳转 
        /// </summary> 
        /// <param name="RedirectpPath">跳转路径</param> 
        public static void TopRedirect(string RedirectpPath)
        {
            HttpContext.Current.Response.Write(ScriptStart);
            HttpContext.Current.Response.Write("parent.top.location.href='" + RedirectpPath + "';");
            HttpContext.Current.Response.Write(ScriptEnd);
            HttpContext.Current.Response.End();
        }

        /// <summary> 
        /// 判断并跳转 
        /// </summary> 
        /// <param name="confirmMessage">提示信息</param> 
        /// <param name="YesRedirectPath">选择“是”后跳转的路径</param> 
        /// <param name="NoRedirectPath">选择“否”后跳转的路径</param> 
        /// <param name="IsResponseEnd">是否中断服务端脚本执行</param> 
        public static void ConfirmRedirect(string confirmMessage, string YesRedirectPath, string NoRedirectPath, bool IsResponseEnd)
        {
            HttpContext.Current.Response.Write(ScriptStart);
            HttpContext.Current.Response.Write("if(confirm('" + confirmMessage + "')){location.href='" + YesRedirectPath + "';}else{location.href='" + NoRedirectPath + "';}");
            HttpContext.Current.Response.Write(ScriptEnd);
            if (IsResponseEnd)
            {
                HttpContext.Current.Response.End();
            }
        }

        /// <summary> 
        /// 判断并写入脚本代码 
        /// </summary> 
        /// <param name="confirmMessage">提示信息</param> 
        /// <param name="YesScript">选择“是”后写入的脚本内容</param> 
        /// <param name="NoScript">选择“否”后写入的脚本内容</param> 
        /// <param name="IsResponseEnd">是否中断服务端脚本执行</param> 
        public static void ConfirmScript(string confirmMessage, string YesScript, string NoScript, bool IsResponseEnd)
        {
            HttpContext.Current.Response.Write(ScriptStart);
            HttpContext.Current.Response.Write("if(confirm('" + confirmMessage + "')){" + YesScript + "}else{" + NoScript + "}");
            HttpContext.Current.Response.Write(ScriptEnd);
            if (IsResponseEnd)
            {
                HttpContext.Current.Response.End();
            }
        }

        #endregion Javascript常用方法，不要求一个System.Web.UI.Page类的实例
    }
}