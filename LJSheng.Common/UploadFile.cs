//-----------------------------------------------------------
// 描    述: 文件上传
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System;
using System.Web.UI.HtmlControls;
using System.IO;

namespace LJSheng.Common
{
    /// <summary>
    /// .Net通用大文件上传类(可上传大文件)
    /// </summary>
    public class UploadFile
    {
        //字段
        private string _UploadInfo;  // 文件上传的返回信息。
        private int _UploadState;  // 文件上传的返回状态 0 为成功。
        private string _FileType;  // 允许上传文件的类型。
        private int _FileSize;  // 上传文件的大小。
        private int _MaxFileSize;  // 上传文件大小的最大长度。
        private string _NewFileName;  // 上传后的文件名。

        //初始
        public UploadFile()
        {
            _UploadInfo = "NONE";
            _UploadState = -1;
            _FileType = "apk|ipa|rar|zip|jpg|png|gif|mp3";
            _MaxFileSize = 10240000;//10M
            _NewFileName = "";
        }

        #region 属性
        /**//// <summary>
        /// 文件上传的返回信息
        /// </summary>
        public string UploadInfo
        {
            set { _UploadInfo = value; }
            get { return _UploadInfo; }
        }
        /**//// <summary>
        /// 文件上传的返回状态 0 成功 1 没有选择文件或者0字节 2 文件太大 3 文件类型不允许
        /// </summary>
        public int UploadState
        {
            set { _UploadState = value; }
            get { return _UploadState; }
        }
        /**//// <summary>
        /// 允许上传文件的类型,* 或默认代表可任意类型,例 "jpg|gif|bmp"
        /// </summary>
        public string FileType
        {
            set { _FileType = value; }
            get { return _FileType; }
        }
        /**//// <summary>
        /// 上传文件的大小
        /// </summary>
        public int FileSize
        {
            get { return _FileSize / 1024; }
        }
        /**//// <summary>
        /// 上传文件大小的最大长度
        /// </summary>
        public int MaxFileSize
        {
            set { _MaxFileSize = value * 1024; }
            get { return _MaxFileSize / 1024; }
        }
        /**//// <summary>
        /// 上传后的文件名
        /// </summary>
        public string NewFileName
        {
            set { _NewFileName = value; }
            get { return _NewFileName; }
        }

        #endregion

        #region 上传主程序
        /**//// <summary>
        /// 上传本地文件到服务器。
        /// </summary>
        /// <param name="strSaveDir">在服务器端保存的物理路径。</param>
        /// <param name="HtmCtrlObjUploadFile">上传的文件对象 Input(File)。</param>
        /// <param name="name">上传文件名。</param>
        /// <returns></returns>
        public void ExecUploadFile(string strSaveDir, HtmlInputFile HtmCtrlObjUploadFile, string name="")
        {
            int intFileExtPoint = HtmCtrlObjUploadFile.PostedFile.FileName.LastIndexOf("."); //存储最后一个 . 号的位置
            string strFileExtName = HtmCtrlObjUploadFile.PostedFile.FileName.Substring(intFileExtPoint + 1).ToLower();  // 获取要上传的文件的后缀名。

            _FileSize = HtmCtrlObjUploadFile.PostedFile.ContentLength;//文件大小 byte

            if (_FileSize == 0)  // 判断是否有文件需要上传或所选文件是否为0字节。
            {
                _UploadInfo = "没有选择要上传的文件或所选文件大小为0字节";
                _UploadState = 1;
                return;  // 返回文件上传状态和信息。
            }
            if (_FileSize > _MaxFileSize)  // 限制要上传的文件大小(byte)。
            {
                _UploadInfo = "上传的文件("+ (_FileSize / 1024).ToString() + ")超过限制大小(" + (_MaxFileSize / 1024).ToString() + "K)";
                _UploadState = 2;
                return;  // 返回文件上传状态和信息。
            }

            if (_FileType != "*")
            {
                if (_FileType.ToLower().IndexOf(strFileExtName.ToLower().Trim()) == -1)  // 判断要上传的文件类型的是否在允许的范围内。
                {
                    _UploadInfo = "不允许上传的文件类型(允许的类型：" + _FileType + ")";
                    _UploadState = 3;
                    return;  // 返回文件上传状态和信息
                }
            }
            _NewFileName = name;
            if (_NewFileName == "")
            {
                DateTime dteNow = DateTime.Now;  // 定义日期对象，为上传后的文件命名。
                _NewFileName = dteNow.Year.ToString() + dteNow.Month.ToString() + dteNow.Day.ToString() + GetRandomStr(8);  // 随机地为上传后的文件命名。
                _NewFileName = _NewFileName + "." + strFileExtName; //包含扩展名的文件名
            }
            else
            {
                _NewFileName = _NewFileName + "." + strFileExtName; //包含扩展名的文件名
            }
            HtmCtrlObjUploadFile.PostedFile.SaveAs(this.GetSaveDirectory(System.Web.HttpContext.Current.Server.MapPath(strSaveDir)) + _NewFileName);  // 以新的文件名保存上传的文件到指定物理路径。            
            _UploadInfo = "文件上传成功";  // 返回上传后的服务器端文件物理路径。
            _UploadState = 0;

        }
        #endregion

        #region 获取指定位数的随机数
        /**//// <summary>
        /// 获取指定位数的随机数。
        /// </summary>
        /// <param name="RndNumCount">随机数位数。</param>
        /// <returns></returns>
        private string GetRandomStr(int RndNumCount)
        {
            string RandomStr;
            RandomStr = "";
            Random Rnd = new Random();
            for (int i = 1; i <= RndNumCount; i++)
            {
                RandomStr += Rnd.Next(0, 9).ToString();
            }
            return RandomStr;
        }
        #endregion

        #region 获取上传文件存放目录
        /**//// <summary>
        /// 获取上传文件存放目录。
        /// </summary>
        /// <param name="DirectoryPath">存放文件的物理路径。</param>
        /// <returns>返回存放文件的目录。</returns>
        public string GetSaveDirectory(string DirectoryPath)
        {
            if (!Directory.Exists(DirectoryPath))  // 判断当前目录是否存在。
            {
                Directory.CreateDirectory(DirectoryPath);  // 建立上传文件存放目录。
            }
            return DirectoryPath;
        }
        #endregion

        public static string upload(string path, HtmlInputFile file)
        {
            if (file != null)
            {
                UploadFile uf = new UploadFile();
                uf.ExecUploadFile(path, file);
                return uf._NewFileName;
            }
            else
            {
                return null;
            }
            
        }
    }

    #region 附   修改上传大小的配置
    /*
    需要修改的是
    在 C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\CONFIG目录里，
    找到文件maxRequestLength="4096"
    将值修改大一些，例如：102400
    这个参数的单位应该是KB的

    以上方法是修改全局的，如果公需要修改一个项目，那么是修改项目里的Web.config文件

    在<system.web></system.web>之间添加，
    <httpRuntime useFullyQualifiedRedirectUrl="true" maxRequestLength="21000" executionTimeout="300" />
    其中，
    maxRequestLength：设置上传文件的最大值，单位：KB。（默认是4096KB，即4M）
    executionTimeout：设置超时时间，单位：秒。（默认是90秒） 

    */
    #endregion
}
