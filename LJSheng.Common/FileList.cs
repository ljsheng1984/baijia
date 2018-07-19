//-----------------------------------------------------------
// 描    述:获取路径下的所有文件
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Collections;

namespace LJSheng.Common
{
    public class FileList
    {
        /// <summary>
        /// 获取指定文件夹下所有的文件名称
        /// </summary>
        /// <param name="folderName">指定文件夹名称,绝对路径</param>
        /// <param name="fileFilter">文件类型过滤,根据文件后缀名,如:*,*.txt,*.xls</param>
        /// <param name="isContainSubFolder">是否包含子文件夹</param>
        /// <returns>ArrayList数组,为所有需要的文件路径名称</returns>
        public static ArrayList GetAllFilesByFolder(string folderName, string fileFilter, bool isContainSubFolder)
        {
            string serverpath = System.Web.HttpContext.Current.Server.MapPath(folderName);
            ArrayList resArray = new ArrayList();
            string[] files = Directory.GetFiles(serverpath, fileFilter);
            for (int i = 0; i < files.Length; i++)
            {
                resArray.Add(files[i]);
            }
            if (isContainSubFolder)
            {
                string[] folders = Directory.GetDirectories(serverpath);
                for (int j = 0; j < folders.Length; j++)
                {
                    //遍历所有文件夹
                    ArrayList temp = GetAllFilesByFolder(folders[j], fileFilter, isContainSubFolder);
                    resArray.AddRange(temp);
                }
            }
            return resArray;
        }

        /// <summary>
        /// 获取指定文件夹下所有的文件名称,不过滤文件类型
        /// </summary>
        /// <param name="folderName">指定文件夹名称,绝对路径</param>
        /// <param name="isContainSubFolder">是否包含子文件夹</param>
        /// <returns>ArrayList数组,为所有需要的文件路径名称</returns>
        public static ArrayList GetAllFilesByFolder(string folderName, bool isContainSubFolder)
        {
            return GetAllFilesByFolder(folderName, "*", isContainSubFolder);
        }

        /// <summary>
        /// 根据文件夹路径返回所有文件
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <returns>$号分割</returns>
        public static string Files(string path)
        {
            string serverpath = System.Web.HttpContext.Current.Server.MapPath(path);
            StringBuilder allfile = new StringBuilder("");
            if (Directory.Exists(serverpath))
            {
                DirectoryInfo aDir = new DirectoryInfo(serverpath);
                DirectoryInfo[] dirs = aDir.GetDirectories();
                foreach (DirectoryInfo s in dirs)
                {
                    allfile.Append(s.FullName.Replace(serverpath, "") + "$");
                }
            }
            return allfile.ToString().TrimEnd('$');
        }

        /// <summary>
        /// 根据文件夹路径返回图片格式的文件
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="pagesize">返回个数</param>
        /// <returns>$号分割</returns>
        //自定义一个结构
        public struct sFileInfo
        {
            public string FileName;
            public DateTime FileCreateTime;
        }
        public static string ImagesList(string path, int pagesize)
        {
            string serverpath = System.Web.HttpContext.Current.Server.MapPath(path);
            StringBuilder returnvalue = new StringBuilder("");
            if (Directory.Exists(serverpath))
            {
                DirectoryInfo thisOne = new DirectoryInfo(serverpath);
                FileInfo[] fileInfo = thisOne.GetFiles();
                int n = 0;
                //根椐指定文件夹下的图片文件数目，获取数组的长度 n
                foreach (FileInfo fInfo in fileInfo)
                {
                    if (string.Compare(fInfo.Extension.ToLower(), ".jpg") == 0 || string.Compare(fInfo.Extension.ToLower(), ".gif") == 0 || string.Compare(fInfo.Extension.ToLower(), ".png") == 0)
                    {
                        n++;
                    }
                }

                //定义数组 并对数组进行赋值
                sFileInfo[] ArrFiles = new sFileInfo[n];
                int i = 0;
                foreach (FileInfo _f in fileInfo)
                {
                    if (string.Compare(_f.Extension.ToLower(), ".jpg") == 0 || string.Compare(_f.Extension.ToLower(), ".gif") == 0 || string.Compare(_f.Extension.ToLower(), ".png") == 0)
                    {
                        ArrFiles[i].FileName = _f.Name;
                        ArrFiles[i].FileCreateTime = _f.CreationTime;
                        i++;
                    }
                }

                //对数组根椐文件创建时间进行冒泡排序
                sFileInfo FileInfoTemp;
                for (int k = 0; k < n; k++)
                {
                    for (int j = k + 1; j < n; j++)
                    {
                        if (DateTime.Compare(ArrFiles[k].FileCreateTime, ArrFiles[j].FileCreateTime) < 0)
                        {
                            FileInfoTemp = ArrFiles[k];
                            ArrFiles[k] = ArrFiles[j];
                            ArrFiles[j] = FileInfoTemp;
                        }
                    }
                }
                for (int j = 0; j < n; j++)
                {
                    returnvalue.Append(ArrFiles[j].FileName + "$");
                }
            }
            try
            {
                return returnvalue.ToString().Substring(0, returnvalue.Length - 1);
            }
            catch
            {
                return "";
            }
        }
        public static string IList(string path, int pagesize)
        {
            string serverpath = System.Web.HttpContext.Current.Server.MapPath(path);
            StringBuilder returnvalue = new StringBuilder("");
            if (Directory.Exists(serverpath))
            {
                DirectoryInfo thisOne = new DirectoryInfo(serverpath);
                FileInfo[] fileInfo = thisOne.GetFiles();
                int i = 1;
                if (fileInfo.Length < pagesize)
                {
                    pagesize = fileInfo.Length;
                }
                foreach (FileInfo fInfo in fileInfo)
                {
                    if (i <= pagesize)
                    {
                        if (string.Compare(fInfo.Extension.ToLower(), ".jpg") == 0 || string.Compare(fInfo.Extension.ToLower(), ".gif") == 0)
                        {
                            returnvalue.Append(fInfo.Name + "$");
                            i++;
                        }
                    }
                }
            }
            try
            {
                return returnvalue.ToString().Substring(0, returnvalue.Length - 1);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 根据文件夹路径返回所有图片
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <returns>$号分割</returns>
         public static string AllImagesList(string path, int pagesize)
        {
            string serverpath = System.Web.HttpContext.Current.Server.MapPath(path);
            StringBuilder returnvalue = new StringBuilder("");
            if (Directory.Exists(serverpath))
            {
                DirectoryInfo thisOne = new DirectoryInfo(serverpath);
                FileInfo[] fileInfo = thisOne.GetFiles();
                int n = 0;
                //根椐指定文件夹下的图片文件数目，获取数组的长度 n
                foreach (FileInfo fInfo in fileInfo)
                {
                    if (string.Compare(fInfo.Extension.ToLower(), ".jpg") == 0 || string.Compare(fInfo.Extension.ToLower(), ".gif") == 0 || string.Compare(fInfo.Extension.ToLower(), ".png") == 0)
                    {
                        n++;
                    }
                }

                //定义数组 并对数组进行赋值
                sFileInfo[] ArrFiles = new sFileInfo[n];
                int i = 0;
                foreach (FileInfo _f in fileInfo)
                {
                    if (string.Compare(_f.Extension.ToLower(), ".jpg") == 0 || string.Compare(_f.Extension.ToLower(), ".gif") == 0 || string.Compare(_f.Extension.ToLower(), ".png") == 0)
                    {
                        ArrFiles[i].FileName = path + _f.Name;
                        ArrFiles[i].FileCreateTime = _f.CreationTime;
                        i++;
                    }
                }

                //对数组根椐文件创建时间进行冒泡排序
                sFileInfo FileInfoTemp;
                for (int k = 0; k < n; k++)
                {
                    for (int j = k + 1; j < n; j++)
                    {
                        if (DateTime.Compare(ArrFiles[k].FileCreateTime, ArrFiles[j].FileCreateTime) < 0)
                        {
                            FileInfoTemp = ArrFiles[k];
                            ArrFiles[k] = ArrFiles[j];
                            ArrFiles[j] = FileInfoTemp;
                        }
                    }
                }
                for (int j = 0; j < n; j++)
                {
                    returnvalue.Append(ArrFiles[j].FileName + "$");
                }
            }
            try
            {
                return returnvalue.ToString().Substring(0, returnvalue.Length - 1);
            }
            catch
            {
                return "";
            }
        }

        public static string AllFiles(string path)
        {
            string hxstr = LJSheng.Common.FileList.Files(path);
            if (hxstr == "")
            { return ""; }
            StringBuilder sb = new StringBuilder("");
            try
            {
                string[] s = hxstr.Split('$');
                for (int k = 0; k < s.Length; k++)
                {
                    sb.Append(AllImagesList(path + s[k].ToString() + "/", 999) + "$");
                }
            }
            catch
            {
                sb.Append(ImagesList(path + hxstr + "/", 999));
            }
            return sb.ToString().TrimEnd('$');
        }
    }
}
