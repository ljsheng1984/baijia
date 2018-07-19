//-----------------------------------------------------------
// 描    述: 字符串操作
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace LJSheng.Common
{
    /// <summary>
    /// 提供常用的功能函数。
    /// </summary>
    public class LCommon
    {
        #region 提取HTML代码中文字的C#函数
        ///   <summary>
        ///   提取HTML代码中文字的C#函数
        ///   </summary>
        ///   <param   name="strHtml">包括HTML的源码   </param>
        ///   <returns>已经去除后的文字</returns>
        public static string StripHTML(string strHtml)
        {
            string[] aryReg =
            {
              @"<script[^>]*?>.*?</script>",

              @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>", @"([\r\n])[\s]+", @"&(quot|#34);", @"&(amp|#38);", @"&(lt|#60);", @"&(gt|#62);", @"&(nbsp|#160);", @"&(iexcl|#161);", @"&(cent|#162);", @"&(pound|#163);",@"&(copy|#169);", @"&#(\d+);", @"-->", @"<!--.*\n"
            };

            string[] aryRep =
            {
              "", "", "", "\"", "&", "<", ">", "   ", "\xa1", //chr(161),
              "\xa2", //chr(162),
              "\xa3", //chr(163),
              "\xa9", //chr(169),
              "", "\r\n", ""
            };

            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }
            strOutput.Replace("<", "");
            strOutput.Replace(">", "");
            strOutput.Replace("\r\n", "");
            return strOutput;
        }
        #endregion

        #region 使用静态方法移除HTML标签
        ///   <summary>
        ///   使用静态方法移除HTML标签
        ///   </summary>
        ///   <param   name="HTMLStr">HTMLStr</param>
        public static string ParseTags(string HTMLStr)
        {
            return System.Text.RegularExpressions.Regex.Replace(HTMLStr, "<[^>]*>", "");
        }

        #endregion

        #region 转换int类型

        public static int StringToInt(string IntStr)
        {
            int GetInt = 0;
            Int32.TryParse(IntStr, out GetInt);
            return GetInt;
        }

        #endregion

        #region 时间处理
        /// <summary>
        /// 格式化DateTime类型为字符串类型，精确到天，如：2008/01/01
        /// </summary>
        /// <param name="dateTime">要格式化的时间变量</param>
        /// <returns></returns>
        public static string ConvertToDayString(DateTime dateTime)
        {
            if (string.IsNullOrEmpty(dateTime.ToString()))
            {
                return "";
            }
            return dateTime.ToString(@"yyyy\/MM\/dd");
        }
        /// <summary>
        /// 格式化object类型为字符串类型，精确到天，如：2008/01/01
        /// </summary>
        /// <param name="dateTime">要格式化的object</param>
        /// <returns></returns>
        public static string ConvertToDayString(object dateTime)
        {
            if (string.IsNullOrEmpty(dateTime.ToString()))
            {
                return "";
            }
            return ConvertToDayString((DateTime)dateTime);
        }
        /// <summary>
        /// 格式化DateTime类型为字符串类型，精确到小时，如：2008/01/01 18
        /// </summary>
        /// <param name="dateTime">要格式化的时间变量</param>
        /// <returns></returns>
        public static string ConvertToHourString(DateTime dateTime)
        {
            if (string.IsNullOrEmpty(dateTime.ToString()))
            {
                return "";
            }
            return dateTime.ToString(@"yyyy\/MM\/dd HH");
        }
        /// <summary>
        /// 格式化object类型为字符串类型，精确到小时，如：2008/01/01 18
        /// </summary>
        /// <param name="dateTime">要格式化的object</param>
        /// <returns></returns>
        public static string ConvertToHourString(object dateTime)
        {
            if (string.IsNullOrEmpty(dateTime.ToString()))
            {
                return "";
            }
            return ConvertToHourString((DateTime)dateTime);
        }
        /// <summary>
        /// 格式化DateTime类型为字符串类型，精确到分钟，如：2008/01/01 18:09
        /// </summary>
        /// <param name="dateTime">要格式化的时间变量</param>
        /// <returns></returns>
        public static string ConvertToMiniuteString(DateTime dateTime)
        {
            if (string.IsNullOrEmpty(dateTime.ToString()))
            {
                return "";
            }
            return dateTime.ToString(@"yyyy\/MM\/dd HH:mm");
        }
        /// <summary>
        /// 格式化object类型为字符串类型，精确到分钟，如：2008/01/01 18:09
        /// </summary>
        /// <param name="dateTime">要格式化的object</param>
        /// <returns></returns>
        public static string ConvertToMiniuteString(object dateTime)
        {
            if (string.IsNullOrEmpty(dateTime.ToString()))
            {
                return "";
            }
            return ConvertToMiniuteString((DateTime)dateTime);
        }
        /// <summary>
        /// 格式化DateTime类型为字符串类型，精确到秒，如：2008/01/01 18:09:20
        /// </summary>
        /// <param name="dateTime">要格式化的时间变量</param>
        /// <returns></returns>
        public static string ConvertToSecondString(DateTime dateTime)
        {
            if (string.IsNullOrEmpty(dateTime.ToString()))
            {
                return "";
            }
            return dateTime.ToString(@"yyyy\/MM\/dd HH:mm:ss");
        }
        /// <summary>
        /// 格式化object类型为字符串类型，精确到秒，如：2008/01/01 18:09:20
        /// </summary>
        /// <param name="dateTime">要格式化的object</param>
        /// <returns></returns>
        public static string ConvertToSecondString(object dateTime)
        {
            if (string.IsNullOrEmpty(dateTime.ToString()))
            {
                return "";
            }
            return ConvertToSecondString((DateTime)dateTime);
        }

        /// <summary>
        /// 把秒转换成分钟
        /// </summary>
        /// <param name="Second">多少秒</param>
        /// <returns></returns>
        public static int SecondToMinute(int Second)
        {
            decimal mm = (decimal)((decimal)Second / (decimal)60);
            return Convert.ToInt32(Math.Ceiling(mm));
        }

        /// <summary>
        /// 将系统时间转换成UNIX时间戳
        /// </summary>
        /// <param name="Second">多少秒</param>
        /// <returns></returns>
        public static string TimeToUNIX(DateTime time)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dtNow = time;
            TimeSpan toNow = dtNow.Subtract(dtStart);
            string timeStamp = toNow.Ticks.ToString();
            timeStamp = timeStamp.Substring(0, timeStamp.Length - 7);
            return timeStamp;
        }

        /// <summary>
        /// 将UNIX时间戳转换成系统时间
        /// </summary>
        /// <param name="Second">多少秒</param>
        /// <returns></returns>
        public static DateTime UNIXToTime(string time)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(time + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }

        #endregion

        #region 文本处理函数
        /// <summary>
        /// 判断文本是否为空
        /// </summary>
        /// <param name="text">要判断的文本</param>
        /// <returns>true 为空  false 不为空</returns>
        public static bool IsNullorEmpty(string text)
        {
            return text == null || text.Trim() == string.Empty;
        }

        /// <summary>
        /// 截取文本的长度，过长则用“...”替换symbol之前的部分文本
        /// </summary>
        /// <param name="length">要调整到的长度</param>
        /// <param name="symbol">确保不被替换的文本的起始字符串，如“(”“[”等</param>
        /// <param name="content">要处理的内容</param>
        /// <returns>要处理的内容</returns>
        public static string FixLength(int length, string symbol, string content)
        {
            content = content.Trim();
            if (content.Length <= length)
                return content;
            else
            {
                int pos = content.LastIndexOf(symbol);
                int distance = content.Length - pos;
                if (length < distance + 3)
                    content = content.Substring(0, length - 2) + "...";
                else
                    content = content.Substring(0, length - distance - 2) + "..." + content.Substring(pos);
                return content;
            }
        }


        /// <summary>
        /// 按字节，取左边的Ｎ位
        /// </summary>
        /// <param name="content">要处理的内容</param>
        /// <param name="fixLength">截取的位数</param>
        /// <returns>要处理的内容</returns>
        public static string LeftB(string content, int fixLength)
        {
            if (fixLength > content.Length * 2)
                return content;

            int length = 0;
            char[] chars = content.ToCharArray();
            int i = 0;
            for (; i < chars.Length; i++)
            {
                if ((int)chars[i] < 256)
                    length++;
                else
                    length = length + 2;
                if (length > fixLength)
                    break;
            }
            if (length <= fixLength)
                return content;

            return content.Substring(0, i - 3);
        }

        /// <summary>
        /// 按字节，截取文本的长度，过长则用“...”结束
        /// </summary>
        /// <param name="fixLength">截取的位数</param>
        /// <param name="content">要处理的内容</param>
        /// <returns>要处理的内容</returns>
        public static string FixLengthB(int fixLength, string content)
        {
            if (fixLength >= content.Length * 2)
                return content;

            if (fixLength < 3)
                return LeftB(content, fixLength);

            int length = 0;
            char[] chars = content.ToCharArray();
            int i = 0;
            for (; i < chars.Length; i++)
            {
                if ((int)chars[i] < 256)
                    length++;
                else
                    length = length + 2;
                if (length > fixLength)
                    break;
            }
            if (length <= fixLength)
                return content;

            for (; i >= 0; i--)
            {
                if ((int)chars[i] < 256)
                    length--;
                else
                    length = length - 2;
                if (length <= fixLength - 3)
                    break;
            }
            return content.Substring(0, i) + "...";
        }

        /// <summary>
        /// 截取文本的长度，过长则用“...”替换symbol之前的部分文本
        /// </summary>
        /// <param name="length">要截取到的长度</param>
        /// <param name="symbol">确保不被替换的文本的起始字符串，如“(”“[”等</param>
        /// <param name="content">要处理的内容</param>
        /// <returns>要处理的内容</returns>
        public static string FixLengthB(int length, string symbol, string content)
        {
            if (length >= content.Length * 2)
                return content;

            int pos = content.LastIndexOf(symbol);
            string leftPart = content.Substring(0, pos);
            string rightPart = content.Substring(pos);
            int leftLength = LengthB(leftPart);
            int rightLength = LengthB(rightPart);

            if (length >= leftLength + rightLength)
                return content;

            if (length > rightLength + 3)
                return FixLengthB(length - rightLength, leftPart) + rightPart;

            if (length == leftLength + 3)
                return content + "...";

            if (length < leftLength + 3)
                return FixLengthB(length, leftPart);

            return leftPart + FixLengthB(length - leftLength, content);
        }

        private static int LengthB(string content)
        {
            int length = 0;
            char[] chars = content.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if ((int)chars[i] < 256)
                    length++;
                else
                    length = length + 2;
            }
            return length;
        }

        #endregion

        #region 网站路径及URL获取
        /// <summary>
        /// 获得发布网站的根目录路径，如:/Web
        /// </summary>
        /// <returns>网站虚拟目录</returns>
        public static string GetRootPath()
        {
            return HttpContext.Current.Request.ApplicationPath;
        }

        /// <summary>
        /// 返回上一个页面的地址
        /// </summary>
        /// <returns>上一个页面的地址</returns>
        public static string GetUrlReferrer()
        {
            string retVal = null;

            try
            {
                retVal = HttpContext.Current.Request.UrlReferrer.ToString();
            }
            catch { }

            if (retVal == null)
                return "";

            return retVal;

        }

        /// <summary>
        /// 获取当前请求的原始 URL(URL 中域信息之后的部分,包括查询字符串(如果存在))
        /// </summary>
        /// <returns>原始 URL</returns>
        public static string GetRawUrl()
        {
            return HttpContext.Current.Request.RawUrl;
        }

        /// <summary>
        /// 获得当前完整Url地址
        /// </summary>
        /// <returns>当前完整Url地址</returns>
        public static string GetUrl()
        {
            return HttpContext.Current.Request.Url.ToString();
        }

        /// <summary>
        /// 根据虚拟路径获取物理路径
        /// </summary>
        /// <returns>物理路径</returns>
        public static string GetMapPath()
        {
            return System.Web.HttpRuntime.AppDomainAppPath;
        }

        /// <summary>
        /// 根据虚拟路径获取物理路径
        /// </summary>
        /// <param name="url">虚拟路径</param>
        /// <returns>物理路径</returns>
        public static string GetMapPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }

        /// <summary>
        /// URL参数检测
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static bool IsURLName(string Name)
        {
            Boolean bl = true;
            if (Name == null || Name == "")
            {
                bl = false;
            }
            return bl;
        }

        #endregion

        #region 字符串过滤处理

        /// <summary>
        /// 过滤危险字符串
        /// </summary>
        /// <param name="chr">要过滤的字符串</param>
        /// <returns>返回过滤后的字符串</returns>
        public static string ReplaceStr(string chr)
        {
            if (chr == null)
                return "";
            chr = chr.Replace("<", "");
            chr = chr.Replace(">", "");
            chr = chr.Replace("\n", "");
            chr = chr.Replace("\"", "");
            chr = chr.Replace("'", "");
            chr = chr.Replace(" ", "");
            chr = chr.Replace("\r", "");
            chr = chr.Replace("--", "");
            return (chr);

        }

        /// <summary>
        /// 替换html中的特殊字符
        /// </summary>
        /// <param name="theString">需要进行替换的文本。</param>
        /// <returns>替换完的文本。</returns>
        public static string HtmlEncode(string theString)
        {
            theString = theString.Replace(">", "&gt;");
            theString = theString.Replace("<", "&lt;");
            theString = theString.Replace("  ", " &nbsp;");
            theString = theString.Replace("  ", " &nbsp;");
            theString = theString.Replace("\"", "&quot;");
            theString = theString.Replace("\'", "&#39;");
            theString = theString.Replace("\n", "<br/> ");
            return theString;
        }

        /// <summary>
        /// 恢复html中的特殊字符
        /// </summary>
        /// <param name="theString">需要恢复的文本。</param>
        /// <returns>恢复好的文本。</returns>
        public static string HtmlDiscode(string theString)
        {
            theString = theString.Replace("&gt;", ">");
            theString = theString.Replace("&lt;", "<");
            theString = theString.Replace("&nbsp;", " ");
            theString = theString.Replace(" &nbsp;", "  ");
            theString = theString.Replace("&quot;", "\"");
            theString = theString.Replace("&#39;", "\'");
            theString = theString.Replace("<br/> ", "\n");
            return theString;
        }

        /// <summary> 
        /// 过滤HTML 
        /// </summary> 
        /// <param name="Htmlstring">等待处理的字符串</param> 
        /// <returns></returns> 
        public static string ClearHtml(string Htmlstring)
        {   //删除脚本       
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            //删除HTML           
            Htmlstring = Regex.Replace(Htmlstring, @"([rn])[s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(d+);", "", RegexOptions.IgnoreCase);
            Htmlstring = HttpContext.Current.Server.HtmlEncode(Htmlstring).Trim();
            return Htmlstring;
        }

        /// <summary> 
        /// 过滤SQL关键字 
        /// </summary> 
        /// <param name="inputString">等待处理的字符串</param> 
        /// <returns></returns> 
        public static string NoSql(object inputString)
        {
            if (inputString == null) return "";
            StringBuilder retVal = new StringBuilder();
            retVal.Append(inputString.ToString());
            if (retVal.ToString() != String.Empty)
            {
                retVal = retVal.Replace("'", "’");
                retVal = retVal.Replace("$", "");
                retVal = retVal.Replace("\\", "");
                retVal = retVal.Replace("\"", "“");
                retVal = retVal.Replace("insert", "");
                retVal = retVal.Replace("INSERT", "");
                retVal = retVal.Replace("select", "");
                retVal = retVal.Replace("SELECT", "");
                retVal = retVal.Replace("delete", "");
                retVal = retVal.Replace("DELETE", "");
                retVal = retVal.Replace("create", "");
                retVal = retVal.Replace("CREATE", "");
                retVal = retVal.Replace("drop", "");
                retVal = retVal.Replace("DROP", "");
                retVal = retVal.Replace("alter", "");
                retVal = retVal.Replace("ALTER", "");
                retVal = retVal.Replace("or", "");
                retVal = retVal.Replace("OR", "");
                retVal = retVal.Replace("and", "");
                retVal = retVal.Replace("AND", "");

            }
            return retVal.ToString();
        }

        /// <summary> 
        /// 过滤非法字符 
        /// </summary> 
        /// <param name="StringSql">等待处理的字符串</param> 
        /// <returns></returns> 
        public static string ClearSql(string StringSql)
        {
            string inputString = Regex.Replace(StringSql, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            if (inputString == null) return "";
            StringBuilder retVal = new StringBuilder();
            retVal.Append(inputString.ToString());
            if (retVal.ToString() != String.Empty)
            {
                retVal = retVal.Replace("\\", "");
                retVal = retVal.Replace("\"", "“");
                retVal = retVal.Replace("insert", "");
                retVal = retVal.Replace("INSERT", "");
                retVal = retVal.Replace("delete", "");
                retVal = retVal.Replace("DELETE", "");
                retVal = retVal.Replace("create", "");
                retVal = retVal.Replace("CREATE", "");
                retVal = retVal.Replace("drop", "");
                retVal = retVal.Replace("DROP", "");
                retVal = retVal.Replace("alter", "");
                retVal = retVal.Replace("ALTER", "");
                retVal = retVal.Replace("update", "");
                retVal = retVal.Replace("UPDATE", "");
            }
            return retVal.ToString();
        }

        #endregion

        #region 字符串类型检测

        /// <summary>
        /// 检查一个字符串是否可以转化为日期，一般用于验证用户输入日期的合法性。
        /// </summary>
        /// <param name="_value">需验证的字符串。</param>
        /// <returns>是否可以转化为日期的bool值。</returns>
        public static bool IsStringDate(string _value)
        {
            DateTime dt;
            try
            {
                dt = DateTime.Parse(_value);
            }
            catch (FormatException e)
            {
                //日期格式不正确时
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查一个字符串是否是纯数字构成的，一般用于查询字符串参数的有效性验证。
        /// </summary>
        /// <param name="_value">需验证的字符串。。</param>
        /// <returns>是否合法的bool值。</returns>
        public static bool IsNumberId(string _value)
        {
            return LCommon.QuickValidate("^[1-9]*[0-9]*$", _value);
        }

        /// <summary>
        /// 检查一个字符串是否是纯字母和数字构成的，一般用于查询字符串参数的有效性验证。
        /// </summary>
        /// <param name="_value">需验证的字符串。</param>
        /// <returns>是否合法的bool值。</returns>
        public static bool IsLetterOrNumber(string _value)
        {
            return LCommon.QuickValidate("^[a-zA-Z0-9_]*$", _value);
        }

        /// <summary>
        /// 判断是否是数字，包括小数和整数。
        /// </summary>
        /// <param name="_value">需验证的字符串。</param>
        /// <returns>是否合法的bool值。</returns>
        public static bool IsNumber(string _value)
        {
            return LCommon.QuickValidate("^(0|([1-9]+[0-9]*))(.[0-9]+)?$", _value);
        }

        /// <summary>
        /// 快速验证一个字符串是否符合指定的正则表达式。
        /// </summary>
        /// <param name="_express">正则表达式的内容。</param>
        /// <param name="_value">需验证的字符串。</param>
        /// <returns>是否合法的bool值。</returns>
        public static bool QuickValidate(string _express, string _value)
        {
            System.Text.RegularExpressions.Regex myRegex = new System.Text.RegularExpressions.Regex(_express);
            if (_value.Length == 0)
            {
                return false;
            }
            return myRegex.IsMatch(_value);
        }

        #endregion

        #region 字符串截取

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="value">截取的字符串</param>
        /// <param name="length">截取的长度</param>
        /// <returns></returns>
        public static string strValue(string strHtml, int Length)
        {
            if (strHtml.Length > Length)
            {
                return strHtml.Substring(0, Length);

            }
            else
            {
                return strHtml;
            }
        }

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="value">截取的字符串</param>
        /// <param name="length">截取的长度</param>
        /// <returns></returns>
        public static string strValuePoint(string strHtml, int Length)
        {
            if (strHtml.Length > Length)
            {
                return strHtml.Substring(0, Length) + "...";

            }
            else
            {
                return strHtml;
            }
        }

        #endregion 字符串截取

        #region 获取页面参数

        /// <summary>
        /// 获取页面参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <returns></returns>
        public static string GetRequestString(string paramName)
        {
            if (HttpContext.Current.Request.QueryString[paramName] == null)
            {
                return "";
            }
            else
            {
                return HttpContext.Current.Request.QueryString[paramName].ToString();
            }
        }

        #endregion 获取页面参数

        #region 删除最后一个字符之后的字符

        /// <summary>
        /// 删除最后结尾的指定字符后的字符
        /// </summary>
        public static string DelLastChar(string str, string strchar)
        {
            return str.Substring(0, str.LastIndexOf(strchar));
        }

        #endregion

        #region 取单个字符的拼音声母

        /// <summary> 
        /// 取单个字符的拼音声母 
        /// </summary> 
        /// <param name="c">要转换的单个汉字</param> 
        /// <returns>拼音声母</returns> 
        private static string GetPYChar(string c)
        {
            byte[] array = new byte[2];
            array = System.Text.Encoding.Default.GetBytes(c);
            int i = (short)(array[0] - '\0') * 256 + ((short)(array[1] - '\0'));
            if (i < 0xB0A1) return "*";
            if (i < 0xB0C5) return "A";
            if (i < 0xB2C1) return "B";
            if (i < 0xB4EE) return "C";
            if (i < 0xB6EA) return "D";
            if (i < 0xB7A2) return "E";
            if (i < 0xB8C1) return "F";
            if (i < 0xB9FE) return "G";
            if (i < 0xBBF7) return "H";
            if (i < 0xBFA6) return "G";
            if (i < 0xC0AC) return "K";
            if (i < 0xC2E8) return "L";
            if (i < 0xC4C3) return "M";
            if (i < 0xC5B6) return "N";
            if (i < 0xC5BE) return "O";
            if (i < 0xC6DA) return "P";
            if (i < 0xC8BB) return "Q";
            if (i < 0xC8F6) return "R";
            if (i < 0xCBFA) return "S";
            if (i < 0xCDDA) return "T";
            if (i < 0xCEF4) return "W";
            if (i < 0xD1B9) return "X";
            if (i < 0xD4D1) return "Y";
            if (i < 0xD7FA) return "Z";
            return "*";
        }

        #endregion 取单个字符的拼音声母

        #region 其他功能函数

        /// <summary>
        /// 繁简互转
        /// </summary>
        /// <param name="content">要处理的内容</param>
        /// <param name="isToSimple">true:转为简体 false:转为繁体</param>
        /// <returns>要处理的内容</returns>
        public static string Trans(string content, bool isToSimple)
        {
            //注意：简体有些字没有，仍用繁体字。如下：“琺”
            //新加的字都放在前面
            string complex = "槓穌靨嘆繽軼隻製嚀幺皚藹礙愛翺襖奧壩罷擺敗頒辦絆幫綁鎊謗剝飽寶報鮑輩貝鋇狽備憊繃筆畢斃幣閉邊編貶變辯辮標鼈別癟瀕濱賓擯餅並撥缽鉑駁蔔補財參蠶殘慚慘燦蒼艙倉滄廁側冊測層詫攙摻蟬饞讒纏鏟産闡顫場嘗長償腸廠暢鈔車徹塵沈陳襯撐稱懲誠騁癡遲馳恥齒熾沖蟲寵疇躊籌綢醜櫥廚鋤雛礎儲觸處傳瘡闖創錘純綽辭詞賜聰蔥囪從叢湊躥竄錯達帶貸擔單鄲撣膽憚誕彈當擋黨蕩檔搗島禱導盜燈鄧敵滌遞締顛點墊電澱釣調叠諜疊釘頂錠訂丟東動棟凍鬥犢獨讀賭鍍鍛斷緞兌隊對噸頓鈍奪墮鵝額訛惡餓兒爾餌貳發罰閥琺礬釩煩範販飯訪紡飛誹廢費紛墳奮憤糞豐楓鋒風瘋馮縫諷鳳膚輻撫輔賦複負訃婦縛該鈣蓋幹趕稈贛岡剛鋼綱崗臯鎬擱鴿閣鉻個給龔宮鞏貢鈎溝構購夠蠱顧剮關觀館慣貫廣規矽歸龜閨軌詭櫃貴劊輥滾鍋國過駭韓漢號閡鶴賀橫轟鴻紅後壺護滬戶嘩華畫劃話懷壞歡環還緩換喚瘓煥渙黃謊揮輝毀賄穢會燴彙諱誨繪葷渾夥獲貨禍擊機積饑譏雞績緝極輯級擠幾薊劑濟計記際繼紀夾莢頰賈鉀價駕殲監堅箋間艱緘繭檢堿鹼揀撿簡儉減薦檻鑒踐賤見鍵艦劍餞漸濺澗將漿蔣槳獎講醬膠澆驕嬌攪鉸矯僥腳餃繳絞轎較稭階節莖鯨驚經頸靜鏡徑痙競淨糾廄舊駒舉據鋸懼劇鵑絹傑潔結誡屆緊錦僅謹進晉燼盡勁荊覺決訣絕鈞軍駿開凱顆殼課墾懇摳庫褲誇塊儈寬礦曠況虧巋窺饋潰擴闊蠟臘萊來賴藍欄攔籃闌蘭瀾讕攬覽懶纜爛濫撈勞澇樂鐳壘類淚籬離裏鯉禮麗厲勵礫曆瀝隸倆聯蓮連鐮憐漣簾斂臉鏈戀煉練糧涼兩輛諒療遼鐐獵臨鄰鱗凜賃齡鈴淩靈嶺領餾劉龍聾嚨籠壟攏隴樓婁摟簍蘆盧顱廬爐擄鹵虜魯賂祿錄陸驢呂鋁侶屢縷慮濾綠巒攣孿灤亂掄輪倫侖淪綸論蘿羅邏鑼籮騾駱絡媽瑪碼螞馬罵嗎買麥賣邁脈瞞饅蠻滿謾貓錨鉚貿麽黴沒鎂門悶們錳夢謎彌覓冪綿緬廟滅憫閩鳴銘謬謀畝鈉納難撓腦惱鬧餒內擬膩攆撚釀鳥聶齧鑷鎳檸獰甯擰濘鈕紐膿濃農瘧諾歐鷗毆嘔漚盤龐賠噴鵬騙飄頻貧蘋憑評潑頗撲鋪樸譜棲淒臍齊騎豈啓氣棄訖牽扡釺鉛遷簽謙錢鉗潛淺譴塹槍嗆牆薔強搶鍬橋喬僑翹竅竊欽親寢輕氫傾頃請慶瓊窮趨區軀驅齲顴權勸卻鵲確讓饒擾繞熱韌認紉榮絨軟銳閏潤灑薩鰓賽叁傘喪騷掃澀殺紗篩曬刪閃陝贍繕傷賞燒紹賒攝懾設紳審嬸腎滲聲繩勝聖師獅濕詩屍時蝕實識駛勢適釋飾視試壽獸樞輸書贖屬術樹豎數帥雙誰稅順說碩爍絲飼聳慫頌訟誦擻蘇訴肅雖隨綏歲孫損筍縮瑣鎖獺撻擡態攤貪癱灘壇譚談歎湯燙濤縧討騰謄銻題體屜條貼鐵廳聽烴銅統頭禿圖塗團頹蛻脫鴕馱駝橢窪襪彎灣頑萬網韋違圍爲濰維葦偉僞緯謂衛溫聞紋穩問甕撾蝸渦窩臥嗚鎢烏汙誣無蕪吳塢霧務誤錫犧襲習銑戲細蝦轄峽俠狹廈嚇鍁鮮纖鹹賢銜閑顯險現獻縣餡羨憲線廂鑲鄉詳響項蕭囂銷曉嘯蠍協挾攜脅諧寫瀉謝鋅釁興洶鏽繡虛噓須許敘緒續軒懸選癬絢學勳詢尋馴訓訊遜壓鴉鴨啞亞訝閹煙鹽嚴顔閻豔厭硯彥諺驗鴦楊揚瘍陽癢養樣瑤搖堯遙窯謠藥爺頁業葉醫銥頤遺儀彜蟻藝億憶義詣議誼譯異繹蔭陰銀飲隱櫻嬰鷹應纓瑩螢營熒蠅贏穎喲擁傭癰踴詠湧優憂郵鈾猶遊誘輿魚漁娛與嶼語籲禦獄譽預馭鴛淵轅園員圓緣遠願約躍鑰嶽粵悅閱雲鄖勻隕運蘊醞暈韻雜災載攢暫贊贓髒鑿棗竈責擇則澤賊贈紮劄軋鍘閘柵詐齋債氈盞斬輾嶄棧戰綻張漲帳賬脹趙蟄轍鍺這貞針偵診鎮陣掙睜猙爭幀鄭證織職執紙摯擲幟質滯鍾終種腫衆謅軸皺晝驟豬諸誅燭矚囑貯鑄築駐專磚轉賺樁莊裝妝壯狀錐贅墜綴諄著濁茲資漬蹤綜總縱鄒詛組鑽為麼於產崙眾餘衝準兇佔歷釐髮臺嚮啟週譁薑寧傢尷鉅乾倖徵逕誌愴恆託摺掛闆樺慾洩瀏薰箏籤蹧係紓燿骼臟捨甦盪穫讚輒蹟跡採裡鐘鏢閒闕僱靂獃騃佈牀脣閧鬨崑崐綑蔴阩昇牠蓆巖灾剳紥註";
            string simple = "杠稣厣叹缤轶只制咛么皑蔼碍爱翱袄奥坝罢摆败颁办绊帮绑镑谤剥饱宝报鲍辈贝钡狈备惫绷笔毕毙币闭边编贬变辩辫标鳖别瘪濒滨宾摈饼并拨钵铂驳卜补财参蚕残惭惨灿苍舱仓沧厕侧册测层诧搀掺蝉馋谗缠铲产阐颤场尝长偿肠厂畅钞车彻尘沉陈衬撑称惩诚骋痴迟驰耻齿炽冲虫宠畴踌筹绸丑橱厨锄雏础储触处传疮闯创锤纯绰辞词赐聪葱囱从丛凑蹿窜错达带贷担单郸掸胆惮诞弹当挡党荡档捣岛祷导盗灯邓敌涤递缔颠点垫电淀钓调迭谍叠钉顶锭订丢东动栋冻斗犊独读赌镀锻断缎兑队对吨顿钝夺堕鹅额讹恶饿儿尔饵贰发罚阀琺矾钒烦范贩饭访纺飞诽废费纷坟奋愤粪丰枫锋风疯冯缝讽凤肤辐抚辅赋复负讣妇缚该钙盖干赶秆赣冈刚钢纲岗皋镐搁鸽阁铬个给龚宫巩贡钩沟构购够蛊顾剐关观馆惯贯广规硅归龟闺轨诡柜贵刽辊滚锅国过骇韩汉号阂鹤贺横轰鸿红后壶护沪户哗华画划话怀坏欢环还缓换唤痪焕涣黄谎挥辉毁贿秽会烩汇讳诲绘荤浑伙获货祸击机积饥讥鸡绩缉极辑级挤几蓟剂济计记际继纪夹荚颊贾钾价驾歼监坚笺间艰缄茧检碱硷拣捡简俭减荐槛鉴践贱见键舰剑饯渐溅涧将浆蒋桨奖讲酱胶浇骄娇搅铰矫侥脚饺缴绞轿较秸阶节茎鲸惊经颈静镜径痉竞净纠厩旧驹举据锯惧剧鹃绢杰洁结诫届紧锦仅谨进晋烬尽劲荆觉决诀绝钧军骏开凯颗壳课垦恳抠库裤夸块侩宽矿旷况亏岿窥馈溃扩阔蜡腊莱来赖蓝栏拦篮阑兰澜谰揽览懒缆烂滥捞劳涝乐镭垒类泪篱离里鲤礼丽厉励砾历沥隶俩联莲连镰怜涟帘敛脸链恋炼练粮凉两辆谅疗辽镣猎临邻鳞凛赁龄铃凌灵岭领馏刘龙聋咙笼垄拢陇楼娄搂篓芦卢颅庐炉掳卤虏鲁赂禄录陆驴吕铝侣屡缕虑滤绿峦挛孪滦乱抡轮伦仑沦纶论萝罗逻锣箩骡骆络妈玛码蚂马骂吗买麦卖迈脉瞒馒蛮满谩猫锚铆贸么霉没镁门闷们锰梦谜弥觅幂绵缅庙灭悯闽鸣铭谬谋亩钠纳难挠脑恼闹馁内拟腻撵捻酿鸟聂啮镊镍柠狞宁拧泞钮纽脓浓农疟诺欧鸥殴呕沤盘庞赔喷鹏骗飘频贫苹凭评泼颇扑铺朴谱栖凄脐齐骑岂启气弃讫牵扦钎铅迁签谦钱钳潜浅谴堑枪呛墙蔷强抢锹桥乔侨翘窍窃钦亲寝轻氢倾顷请庆琼穷趋区躯驱龋颧权劝却鹊确让饶扰绕热韧认纫荣绒软锐闰润洒萨鳃赛三伞丧骚扫涩杀纱筛晒删闪陕赡缮伤赏烧绍赊摄慑设绅审婶肾渗声绳胜圣师狮湿诗尸时蚀实识驶势适释饰视试寿兽枢输书赎属术树竖数帅双谁税顺说硕烁丝饲耸怂颂讼诵擞苏诉肃虽随绥岁孙损笋缩琐锁獭挞抬态摊贪瘫滩坛谭谈叹汤烫涛绦讨腾誊锑题体屉条贴铁厅听烃铜统头秃图涂团颓蜕脱鸵驮驼椭洼袜弯湾顽万网韦违围为潍维苇伟伪纬谓卫温闻纹稳问瓮挝蜗涡窝卧呜钨乌污诬无芜吴坞雾务误锡牺袭习铣戏细虾辖峡侠狭厦吓锨鲜纤咸贤衔闲显险现献县馅羡宪线厢镶乡详响项萧嚣销晓啸蝎协挟携胁谐写泻谢锌衅兴汹锈绣虚嘘须许叙绪续轩悬选癣绚学勋询寻驯训讯逊压鸦鸭哑亚讶阉烟盐严颜阎艳厌砚彦谚验鸯杨扬疡阳痒养样瑶摇尧遥窑谣药爷页业叶医铱颐遗仪彝蚁艺亿忆义诣议谊译异绎荫阴银饮隐樱婴鹰应缨莹萤营荧蝇赢颖哟拥佣痈踊咏涌优忧邮铀犹游诱舆鱼渔娱与屿语吁御狱誉预驭鸳渊辕园员圆缘远愿约跃钥岳粤悦阅云郧匀陨运蕴酝晕韵杂灾载攒暂赞赃脏凿枣灶责择则泽贼赠扎札轧铡闸栅诈斋债毡盏斩辗崭栈战绽张涨帐账胀赵蛰辙锗这贞针侦诊镇阵挣睁狰争帧郑证织职执纸挚掷帜质滞钟终种肿众诌轴皱昼骤猪诸诛烛瞩嘱贮铸筑驻专砖转赚桩庄装妆壮状锥赘坠缀谆着浊兹资渍踪综总纵邹诅组钻为么于产仑众余冲准凶占历厘发台向启周哗姜宁家尴巨干幸征径志怆恒托折挂板桦欲泄浏熏筝签糟系纾耀胳脏舍苏荡获赞辄迹迹采里钟镖闲阙雇雳呆呆布床唇哄哄昆昆捆麻升升它席岩灾札扎注";
            string str = "";
            if (isToSimple)
            {
                for (int i = 0; i < content.Length; i++)
                {
                    string word = content.Substring(i, 1);
                    //忽略字母
                    if (string.CompareOrdinal(word, "~") <= 0)
                    {
                        str += word;
                        continue;
                    }
                    int pos = complex.IndexOf(word);
                    if (pos != -1)
                        str += simple.Substring(pos, 1);
                    else
                        str += word;
                }
            }
            else
            {
                for (int i = 0; i < content.Length; i++)
                {
                    string word = content.Substring(i, 1);
                    //忽略字母
                    if (string.CompareOrdinal(word, "~") <= 0)
                    {
                        str += word;
                        continue;
                    }
                    int pos = simple.IndexOf(word);
                    if (pos != -1)
                        str += complex.Substring(pos, 1);
                    else
                        str += word;
                }
            }
            return str;
        }

        /// <summary>
        /// 将二进制文件读入byte[]（如图片等）
        /// </summary>
        /// <param name="fileName">文件名与路径</param>
        /// <returns>byte[]类型数据</returns>
        public static byte[] ReadFile(string fileName)
        {
            FileStream pFileStream = null;
            byte[] pReadByte = new byte[0];

            try
            {
                pFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader r = new BinaryReader(pFileStream);
                r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开
                pReadByte = r.ReadBytes((int)r.BaseStream.Length);
                return pReadByte;
            }
            catch
            {
                return pReadByte;
            }
            finally
            {
                if (pFileStream != null)
                    pFileStream.Close();
            }
        }

        /// <summary>
        /// 写byte[]数据到文件（如图片等二进制数据）
        /// </summary>
        /// <param name="pReadByte">二进制数</param>
        /// <param name="fileName">文件名</param>
        /// <returns>成功返回True 否返回False</returns>
        public static bool WriteFile(byte[] pReadByte, string fileName)
        {
            FileStream pFileStream = null;

            try
            {
                pFileStream = new FileStream(fileName, FileMode.OpenOrCreate);
                pFileStream.Write(pReadByte, 0, pReadByte.Length);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (pFileStream != null)
                    pFileStream.Close();
            }
            return true;
        }

        /// <summary>
        /// 获取IP地址
        /// </summary>
        public static string GetIPAddress()
        {
            string userIP;
            HttpRequest Request = HttpContext.Current.Request;
            // 如果使用代理，获取真实IP
            if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != "")
                userIP = Request.ServerVariables["REMOTE_ADDR"];
            else
                userIP = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userIP == null || userIP == "")
                userIP = Request.UserHostAddress;
            return userIP;
        }

        #endregion
    }
}
