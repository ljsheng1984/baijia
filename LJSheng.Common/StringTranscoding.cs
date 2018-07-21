//-----------------------------------------------------------
// 描    述: 字符串编码
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System;
using System.Text;

namespace LJSheng.Common
{
    public class StringTranscoding
    {
        #region Escape编码
        /// <summary>
        /// 进行Javascript的Escape编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Escape(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                StringBuilder sb = new StringBuilder();
                byte[] ba = System.Text.Encoding.Unicode.GetBytes(str);
                for (int i = 0; i < ba.Length; i += 2)
                {
                    sb.Append("%u");
                    sb.Append(ba[i + 1].ToString("X2"));
                    sb.Append(ba[i].ToString("X2"));
                }
                return sb.ToString();
            }
            return "";
        }
        /// <summary>
        /// 进行JavsScript的Escape解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UnEscape(string str)
        {
            return System.Web.HttpUtility.UrlDecode(str);
        }
        #endregion

        #region Unicode编码
        /// <summary>
        /// 将单个Unicode字符串转换为普通文字,如\u65e0转换为普通文字时，请这样调用ConvertStr("65e0")
        /// </summary>
        /// <param name="unicodeStr"></param>
        /// <returns></returns>
        public static string ConvertStr(string unicodeStr)
        {
            if (unicodeStr.Length != 4)
            {
                return String.Empty;
            }

            byte byteAfter = Convert.ToByte(unicodeStr.Substring(0, 2), 16);
            byte byteBefore = Convert.ToByte(unicodeStr.Substring(2), 16);

            return System.Text.Encoding.Unicode.GetString(new byte[] { byteBefore, byteAfter });
        }
        /// <summary>
        /// 将单个字符转换为16进制的Unicode字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertUnicode(string str)
        {
            Byte[] arrByte = System.Text.Encoding.Unicode.GetBytes(str);

            string strAfter = Convert.ToString(arrByte[0], 16);
            string strBefore = Convert.ToString(arrByte[1], 16);

            return strBefore + strAfter;
        }


        /// <summary>
        /// Unicode字符串转为正常字符串
        /// </summary>
        /// <param name="srcText"></param>
        /// <returns></returns>
        public static string UnicodeToString(string str)
        {
            string outStr = "";
            if (!string.IsNullOrEmpty(str))
            {
                string[] strlist = str.Replace("/", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        //将unicode字符转为10进制整数，然后转为char中文字符  
                        outStr += (char)int.Parse(strlist[i], System.Globalization.NumberStyles.HexNumber);
                    }
                }
                catch (FormatException ex)
                {
                    outStr = ex.Message;
                }
            }
            return outStr;
        }
        #endregion

        #region ASCII码含中文字符的编解码处理
        /// <summary>
        /// 含中文字符串转ASCII
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Str2ASCII(String str)
        {
            try
            {
                //这里我们将采用2字节一个汉字的方法来取出汉字的16进制码
                byte[] textbuf = Encoding.Default.GetBytes(str);
                //用来存储转换过后的ASCII码
                string textAscii = string.Empty;

                for (int i = 0; i < textbuf.Length; i++)
                {
                    textAscii += textbuf[i].ToString("X");
                }
                return textAscii;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// ASCII转含中文字符串
        /// </summary>
        /// <param name="textAscii">ASCII字符串</param>
        /// <returns></returns>
        public static string ASCII2Str(string textAscii)
        {
            try
            {
                int k = 0;//字节移动偏移量

                byte[] buffer = new byte[textAscii.Length / 2];//存储变量的字节

                for (int i = 0; i < textAscii.Length / 2; i++)
                {
                    //每两位合并成为一个字节
                    buffer[i] = byte.Parse(textAscii.Substring(k, 2), System.Globalization.NumberStyles.HexNumber);
                    k = k + 2;
                }
                //将字节转化成汉字
                return Encoding.Default.GetString(buffer);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion
    }
}