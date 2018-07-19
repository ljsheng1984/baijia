//-----------------------------------------------------------
// 描    述: 日志操作
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System;
using System.IO;

/// <summary>
///Depiction：林建生的通用类库
///Author：林建生
///Create Date：1984-02-04
///</summary>
namespace LJSheng.Common
{
    public class LogManager
    {
        private static string logPath = string.Empty;
        /// <summary>
        /// 保存日志的文件夹
        /// </summary>
        public static string LogPath
        {
            get
            {
                if (logPath == string.Empty)
                {
                    if (System.Web.HttpContext.Current == null)
                        // Windows Forms 应用
                        logPath = AppDomain.CurrentDomain.BaseDirectory;
                    else
                        // Web 应用
                        logPath = AppDomain.CurrentDomain.BaseDirectory + @"bin\";
                }
                return logPath;
            }
            set { logPath = value; }
        }

        private static string logFielPrefix = string.Empty;
        /// <summary>
        /// 日志文件前缀
        /// </summary>
        public static string LogFielPrefix
        {
            get { return logFielPrefix; }
            set { logFielPrefix = value; }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        public static void WriteLog(string logFile, string msg)
        {
            StreamWriter sr = null;
            try
            {
                string path = System.Web.HttpContext.Current.Server.MapPath("/logs/" + DateTime.Now.ToString("yyyy年MM月dd日") + "/");
                string filename = path + logFile + ".txt";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (!File.Exists(filename))
                {
                    sr = File.CreateText(filename);
                }
                else
                {
                    sr = File.AppendText(filename);
                }
                sr.WriteLine(DateTime.Now.ToString() + "\r\n--------------------------------------------------------------------------------------\r\n" + msg + "\r\n--------------------------------------------------------------------------------------\r\n");
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

        /// <summary>
        /// 写日志
        /// </summary>
        public static void WriteLog(LogFile logFile, string msg)
        {
            WriteLog(logFile.ToString(), msg);
        }
    }

    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogFile
    {
        Trace,
        Warning,
        Error,
        SQL
    }
}
