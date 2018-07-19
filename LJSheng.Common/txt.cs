using System;
using System.IO;

namespace LJSheng.Common
{
    public static class TXT
    {
        #region 写日志(用于跟踪)
        /// <summary>
        /// 写日志(用于跟踪)
        /// </summary> 
        /// <param name="log">日志内容</param> 
        /// <param name="logname">日志文件区分名</param> 
        /// <param name="return">无返回值</param> 
        /// <param name="逻辑说明"></param> 
        /// <param>修改备注</param> 
        /// 2014-5-20 林建生
        /// 
        public static void WriteLog(string log, string logname = "LJSheng")
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("/logs/" + DateTime.Now.ToString("yyyy年MM月dd日") + "/");
            string filename = path + logname + ".txt";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            StreamWriter sr = null;
            try
            {
                if (!File.Exists(filename))
                {
                    sr = File.CreateText(filename);
                }
                else
                {
                    sr = File.AppendText(filename);
                }
                sr.WriteLine(DateTime.Now.ToString() + "\r\n--------------------------------------------------------------------------------------\r\n" + log + "\r\n--------------------------------------------------------------------------------------\r\n");
            }
            catch
            {
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }
        #endregion
    }
}
