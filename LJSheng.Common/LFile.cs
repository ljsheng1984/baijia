using System.IO;

namespace LJSheng.Common
{
    public class LFile
    {
        //创建文件
        public static void Create(string FileName)
        {
            FileStream fs;
            fs = File.Create(System.Web.HttpContext.Current.Server.MapPath(FileName));
            fs.Close();  
        }

        //复制文件
        public static void Copy(string FileName)
        {
            File.Copy(System.Web.HttpContext.Current.Server.MapPath(FileName), System.Web.HttpContext.Current.Server.MapPath("a\\a.txt"));
        }

        //移动文件
        public static void Move(string FileName)
        {
            File.Move(System.Web.HttpContext.Current.Server.MapPath("b.txt"), System.Web.HttpContext.Current.Server.MapPath("a\\b.txt"));
        }

        //删除文件
        public static void Delete(string FileName)
        {
            File.Delete(System.Web.HttpContext.Current.Server.MapPath(FileName));
        }
    }
}
