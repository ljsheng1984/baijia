using System;
using System.Text;
using System.IO;
using System.Net;
using LJSheng.Data;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace LJSheng.WX
{
    public static class media
    {
        /// <SUMMARY> 
        /// 上传多媒体文件,返回 MediaId 
        /// </SUMMARY> 
        /// <PARAM name="ACCESS_TOKEN"></PARAM> 
        /// <PARAM name="Type"></PARAM> 
        /// <RETURNS></RETURNS> 
        public static string UploadMultimedia(string Openid)
        {
            string result = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    var query = db.Member.Where(l => l.Openid == Openid).FirstOrDefault();
                    if (query != null)
                    {
                        string gid = query.Account;
                        string ACCESS_TOKEN = Access_token.IsExistAccess_Token();
                        //生成二维码
                        //if (!File.Exists(System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/ewmmp/" + gid + ".jpg")))
                        //{
                        //    string strUrl = "https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token=" + ACCESS_TOKEN;
                        //    string postData = "{\"action_name\": \"QR_LIMIT_STR_SCENE\", \"action_info\": {\"scene\": {\"scene_str\": \"" + gid + "\"}}}";
                        //    JObject jb = JObject.Parse(GetPage(strUrl, postData));
                        //    Common.imgaddimg.Cewm(Common.QRCode.Create_ImgCode(jb["url"].ToString(), 11), gid);
                        //}
                        //获取图片上传服务器
                        //System.Drawing.Image ewm = System.Drawing.Image.FromFile(System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/ewmmp/" + gid + ".jpg"));
                        //string path = System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/ewmmp/");
                        //System.Drawing.Image img = Common.imgaddimg.Ctjr(null, gid);
                        //img.Save(path + gid.Replace("-", "") + ".jpg");
                        //img.Dispose();
                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/ewmmp/") + gid + ".jpg"))
                        {
                            //Common.imgaddimg.Ctjr(gid);
                        }
                        //开始上传
                        string wxurl = "https://api.weixin.qq.com/cgi-bin/media/upload?access_token=" + ACCESS_TOKEN + "&type=image";
                        string filepath = System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/ewmmp/") + gid + ".jpg";
                        WebClient myWebClient = new WebClient();
                        myWebClient.Credentials = CredentialCache.DefaultCredentials;
                        byte[] responseArray = myWebClient.UploadFile(wxurl, "POST", filepath);
                        result = System.Text.Encoding.Default.GetString(responseArray, 0, responseArray.Length);
                        JObject wxjb = JObject.Parse(result);
                        result = wxjb["media_id"].ToString();
                    }
                    else
                    {
                        Common.LogManager.WriteLog("二维码日志", result + ",没有查询到openid用户的GID,错误:" + Openid);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogManager.WriteLog("二维码异常", result + ",错误:" + ex);
            }
            return result;
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
            byte[] data = encoding.GetBytes(postData);
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
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }
    }
}
