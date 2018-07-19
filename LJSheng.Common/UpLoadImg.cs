//-----------------------------------------------------------
// 描    述: 上传类（图片）功能：上传文件操作(主要用于图片上传);
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System;
using System.IO;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Drawing;

namespace LJSheng.Common
{
    public class UploadImg
    {
        private int _Error = 0;//返回上传状态。 
        private int _MaxSize = 5120000;//最大单个上传文件 (默认)
        private string _FileType = "jpg$gif$png";//所支持的上传类型用"/"隔开 
        private string _SavePath = HttpContext.Current.Server.MapPath(".") + "\\";//保存文件的实际路径 
        private int _SaveType = 0;//上传文件的类型，0代表自动生成文件名 
        private HtmlInputFile _FormFile;//上传控件。 
        private string _InFileName = "";//非自动生成文件名设置。 
        private string _OutFileName = "";//输出文件名。 
        private bool _IsCreateImg = false;//是否生成缩略图。 
        private bool _Iss = false;//是否有缩略图生成.
        private int _Height = 0;//获取上传图片的高度 
        private int _Width = 0;//获取上传图片的宽度 
        private int _sHeight = 120;//设置生成缩略图的高度 
        private int _sWidth = 120;//设置生成缩略图的宽度
        private bool _IsDraw = false;//设置是否加水印
        private int _DrawStyle = 0;//设置加水印的方式０：文字水印模式，１：图片水印模式,2:不加
        private int _DrawString_x = 10;//绘制文本的Ｘ坐标（左上角）
        private int _DrawString_y = 10;//绘制文本的Ｙ坐标（左上角）
        private string _AddText = "爱学习";//设置水印内容
        private string _Font = "宋体";//设置水印字体
        private int _FontSize = 12;//设置水印字大小
        private int _FileSize = 0;//获取已经上传文件的大小
        private string _CopyIamgePath = "/uploadfiles/logo.png";//图片水印模式下的覆盖图片的实际地址

        /// <summary>
        /// Error返回值，1、没有上传的文件。2、类型不允许。3、大小超限。4、未知错误。0、上传成功。 
        /// </summary>
        public int Error
        {
            get { return _Error; }
        }
        /// <summary>
        /// 最大单个上传文件
        /// </summary>
        public int MaxSize
        {
            set { _MaxSize = value; }
        }
        /// <summary>
        /// 所支持的上传类型用"/"隔开 
        /// </summary>
        public string FileType
        {
            set { _FileType = value; }
        }
        /// <summary>
        /// //保存文件的实际路径 
        /// </summary>
        public string SavePath
        {
            set { _SavePath = HttpContext.Current.Request.PhysicalApplicationPath + (value); }
            get { return _SavePath; }
        }
        /// <summary>
        /// 上传文件的类型，0代表自动生成文件名
        /// </summary>
        public int SaveType
        {
            set { _SaveType = value; }
        }
        /// <summary>
        /// 上传控件
        /// </summary>
        public HtmlInputFile FormFile
        {
            set { _FormFile = value; }
        }
        /// <summary>
        /// //非自动生成文件名设置。
        /// </summary>
        public string InFileName
        {
            set { _InFileName = value; }
        }
        /// <summary>
        /// 输出文件名
        /// </summary>
        public string OutFileName
        {
            get { return _OutFileName; }
            set { _OutFileName = value; }
        }
        /// <summary>
        /// 是否有缩略图生成.
        /// </summary>
        public bool Iss
        {
            get { return _Iss; }
        }
        /// <summary>
        /// //获取上传图片的宽度
        /// </summary>
        public int Width
        {
            get { return _Width; }
        }
        /// <summary>
        /// //获取上传图片的高度
        /// </summary>
        public int Height
        {
            get { return _Height; }
        }
        /// <summary>
        /// 设置缩略图的宽度
        /// </summary>
        public int sWidth
        {
            get { return _sWidth; }
            set { _sWidth = value; }
        }
        /// <summary>
        /// 设置缩略图的高度
        /// </summary>
        public int sHeight
        {
            get { return _sHeight; }
            set { _sHeight = value; }
        }
        /// <summary>
        /// 是否生成缩略图
        /// </summary>
        public bool IsCreateImg
        {
            get { return _IsCreateImg; }
            set { _IsCreateImg = value; }
        }
        /// <summary>
        /// 是否加水印
        /// </summary>
        public bool IsDraw
        {
            get { return _IsDraw; }
            set { _IsDraw = value; }
        }
        /// <summary>
        /// 设置加水印的方式０：文字水印模式，１：图片水印模式,2:不加
        /// </summary>
        public int DrawStyle
        {
            get { return _DrawStyle; }
            set { _DrawStyle = value; }
        }
        /// <summary>
        /// 绘制文本的Ｘ坐标（左上角）
        /// </summary>
        public int DrawString_x
        {
            get { return _DrawString_x; }
            set { _DrawString_x = value; }
        }
        /// <summary>
        /// 绘制文本的Ｙ坐标（左上角）
        /// </summary>
        public int DrawString_y
        {
            get { return _DrawString_y; }
            set { _DrawString_y = value; }
        }
        /// <summary>
        /// 设置文字水印内容
        /// </summary>
        public string AddText
        {
            get { return _AddText; }
            set { _AddText = value; }
        }
        /// <summary>
        /// 设置文字水印字体
        /// </summary>
        public string Font
        {
            get { return _Font; }
            set { _Font = value; }
        }
        /// <summary>
        /// 设置文字水印字的大小
        /// </summary>
        public int FontSize
        {
            get { return _FontSize; }
            set { _FontSize = value; }
        }
        public int FileSize
        {
            get { return _FileSize; }
            set { _FileSize = value; }
        }
        /// <summary>
        /// 图片水印模式下的覆盖图片的实际地址
        /// </summary>
        public string CopyIamgePath
        {
            set { _CopyIamgePath = HttpContext.Current.Server.MapPath(value); }
        }

        //获取文件的后缀名 
        private string GetExt(string path)
        {
            return Path.GetExtension(path);
        }
        //获取输出文件的文件名。 
        private string FileName(string Ext)
        {
            if (String.IsNullOrEmpty(_OutFileName))
            {
                if (_SaveType == 0 || _InFileName.Trim() == "")
                    return DateTime.Now.ToString("yyyyMMddHHmmssfff") + Ext;
                else
                    return _InFileName;
            }
            return _OutFileName;
        }
        //检查上传的文件的类型，是否允许上传。 
        private bool IsUpload(string Ext)
        {
            Ext = Ext.Replace(".", "");
            bool b = false;
            string[] arrFileType = _FileType.Split('$');
            foreach (string str in arrFileType)
            {
                if (str.ToLower() == Ext.ToLower())
                {
                    b = true;
                    break;
                }
            }
            return b;
        }
        //上传主要部分。 
        public void Open()
        {
            HttpPostedFile hpFile = _FormFile.PostedFile;
            if (hpFile == null || hpFile.FileName.Trim() == "")
            {
                _Error = 1;
                return;
            }

            string Ext = GetExt(hpFile.FileName);
            if (!IsUpload(Ext))
            {
                _Error = 2;
                return;
            }

            int iLen = hpFile.ContentLength;
            if (iLen > _MaxSize)
            {
                _Error = 3;
                return;
            }

            try
            {
                //判断文件夹是否存在.不存在创建!
                if (!Directory.Exists(_SavePath))
                    Directory.CreateDirectory(_SavePath);
                byte[] bData = new byte[iLen];
                hpFile.InputStream.Read(bData, 0, iLen);
                string FName;
                FName = FileName(Ext);
                string TempFile = "";
                if (_IsDraw)
                {
                    TempFile = FName.Split('.').GetValue(0).ToString() + "_temp." + FName.Split('.').GetValue(1).ToString();
                }
                else
                {
                    TempFile = FName;
                }
                FileStream newFile = new FileStream(_SavePath + TempFile, FileMode.Create);
                newFile.Write(bData, 0, bData.Length);
                newFile.Flush();
                int _FileSizeTemp = hpFile.ContentLength;

                if (_IsDraw)
                {
                    if (_DrawStyle == 0)
                    {
                        System.Drawing.Image Img1 = System.Drawing.Image.FromStream(newFile);
                        Graphics g = Graphics.FromImage(Img1);
                        g.DrawImage(Img1, 100, 100, Img1.Width, Img1.Height);
                        Font f = new Font(_Font, _FontSize);
                        Brush b = new SolidBrush(Color.Red);
                        string addtext = _AddText;
                        g.DrawString(addtext, f, b, _DrawString_x, _DrawString_y);
                        g.Dispose();
                        Img1.Save(_SavePath + FName);
                        Img1.Dispose();
                    }
                    else
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromStream(newFile);
                        System.Drawing.Image copyImage = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath(_CopyIamgePath));
                        Graphics g = Graphics.FromImage(image);
                        g.DrawImage(copyImage, new Rectangle(image.Width - copyImage.Width - 5, image.Height - copyImage.Height - 5, copyImage.Width, copyImage.Height), 0, 0, copyImage.Width, copyImage.Height, GraphicsUnit.Pixel);
                        g.Dispose();
                        image.Save(_SavePath + FName);
                        image.Dispose();
                    }
                }

                try
                {
                    //获取图片的高度和宽度
                    System.Drawing.Image Img = System.Drawing.Image.FromStream(newFile);
                    _Width = Img.Width;
                    _Height = Img.Height;
                    //生成缩略图部分 
                    if (_IsCreateImg)
                    {
                        //如果上传文件小于15k，则不生成缩略图。 
                        if (iLen > 15360)
                        {
                            System.Drawing.Image newImg = Img.GetThumbnailImage(_sWidth, _sHeight, null, System.IntPtr.Zero);
                            newImg.Save(_SavePath + FName.Split('.').GetValue(0).ToString() + "_s." + FName.Split('.').GetValue(1).ToString());
                            newImg.Dispose();
                            _Iss = true;
                        }
                    }
                    if (_IsDraw)
                    {
                        if (File.Exists(_SavePath + FName.Split('.').GetValue(0).ToString() + "_temp." + FName.Split('.').GetValue(1).ToString()))
                        {
                            newFile.Dispose();
                            File.Delete(_SavePath + FName.Split('.').GetValue(0).ToString() + "_temp." + FName.Split('.').GetValue(1).ToString());
                        }
                    }
                }
                catch { }
                newFile.Close();
                newFile.Dispose();
                _OutFileName = FName;
                _FileSize = _FileSizeTemp;
                _Error = 0;
                return;
            }
            catch
            {
                _Error = 4;
                return;
            }
        }

        //上传图片
        public static string UPimg(string fname, HtmlInputFile fn, string path)
        {
            if (fn == null)
            {
                return null;
            }
            UploadImg upload = new UploadImg();
            upload.FormFile = fn;
            upload.SavePath = path;
            //upload.IsCreateImg = true;
            //upload.IsDraw = true;
            //upload.DrawStyle = 0;
            //if (!String.IsNullOrEmpty(fname))
            //{
            //    upload.OutFileName = fname;
            //}
            upload.Open();
            if (upload.Error.ToString() == "0")
            {
                return upload.OutFileName;
            }
            else
            {
                return "";
            }
        }
    }
}

