using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LJSheng.Common
{
    public class imgaddimg
    {
        /// <summary>    
        /// 二维码打上LOGO  
        /// </summary>     
        /// <param name="ewm">推荐人的二维码</param>    
        public static void Cewm(Image ewm,string gid)
        {
            Image imgBack = ewm;
            string path = System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/ewmmp/");
            imgBack.Save(path + gid + ".jpg");
            imgBack.Dispose();
        }

        /// <summary>    
        /// 二维码打上背景图片  
        /// </summary>     
        /// <param name="Account">会员帐号</param>
        /// <param name="Gid">会员Gid</param>
        public static Image Ctjr(string Account,Guid Gid)
        {
            //图片里打帐号
            string path = System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/member/");
            Bitmap img = QRCode.Create_ImgCode(Help.Url + "/Home/Register?m=" + Gid.ToString(), 8);
            try
            {
                img = KiResizeImage(img, img.Width, img.Height, 0);
                Graphics g = Graphics.FromImage(img);
                g.DrawImage(img, 0, 0, img.Width, img.Height);
                Font font = new Font("宋体", 20f, FontStyle.Bold); //字体
                Brush brush = Brushes.Red; //字体颜色
                g.DrawString(Account, font, brush, 50, 230);
                img.Save(path + Account + ".jpg");
            }
            catch (Exception err)
            {
                LogManager.WriteLog("生成二维码失败", Account + " --- " + err.Message);
            }
            finally
            {
                GC.Collect();
                img.Dispose();
            }
            return img;
            //图片里打图片
            //Image imgBack = Image.FromFile(System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/ewmmp/ewm.png"));
            //Image img = Image.FromFile(System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/ewmmp/" + gid + ".jpg"));
            //img = KiResizeImage(img, 250, 250, 0);
            //Graphics g = Graphics.FromImage(imgBack);

            //g.DrawImage(imgBack, 0, 0, imgBack.Width, imgBack.Height);      //g.DrawImage(imgBack, 0, 0, 相框宽, 相框高);     
            //g.DrawImage(img, 120, 100, 120, 150);
            //GC.Collect();
            //return imgBack;
        }

        /// <summary>    
        /// Resize图片    
        /// </summary>    
        /// <param name="bmp">原始Bitmap</param>    
        /// <param name="newW">新的宽度</param>    
        /// <param name="newH">新的高度</param>    
        /// <param name="Mode">保留着，暂时未用</param>    
        /// <returns>处理以后的图片</returns>    
        public static Bitmap KiResizeImage(Bitmap bmp, int newW, int newH, int Mode)
        {
            try
            {
                Bitmap b = new Bitmap(newW, newH);
                Graphics g = Graphics.FromImage(b);
                // 插值算法的质量    
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                g.Dispose();
                return b;
            }
            catch
            {
                return null;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }  
    }
}
