//-----------------------------------------------------------
// 描    述: MD5加密
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System;
using System.Security.Cryptography;
using System.Text;

namespace LJSheng.Common
{
    public class MD5
    {
        /// <summary>
        /// 多次使用MD5值以及内部拼hashKey法，增加穷举破解的难度
        /// </summary>
        /// <param name="argInput">输入的字符串</param>
        /// <returns>输出特殊处理过的MD5值</returns>
        public static string GetMd5Hash2(string argInput)
        {
            string hashKey = "ljsheng@#$@!,.Js+{f>1984oE";
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                string hashCode = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(argInput)))
                    .Replace("-", "")
                      + BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(hashKey)))
                    .Replace("-", "");

                return BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(hashCode))).Replace("-", "");
            }
        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="strSource">需要加密的明文</param>
        /// <returns>返回32位加密结果</returns>
        public static string GetMD5(string strSource)
        {
            //new 
            System.Security.Cryptography.MD5 md5 = new MD5CryptoServiceProvider();

            //获取密文字节数组
            byte[] bytResult = md5.ComputeHash(System.Text.Encoding.GetEncoding("UTF-8").GetBytes(strSource));

            //转换成字符串，并取9到25位 
            //string strResult = BitConverter.ToString(bytResult, 4, 8);  
            //转换成字符串，32位 

            string strResult = BitConverter.ToString(bytResult);

            //BitConverter转换出来的字符串会在每个字符中间产生一个分隔符，需要去除掉 
            strResult = strResult.Replace("-", "");

            return strResult;
        }

        public static string GetMD5ljsheng(string strSource)
        {
            //new 
            System.Security.Cryptography.MD5 md5 = new MD5CryptoServiceProvider();

            //获取密文字节数组
            byte[] bytResult = md5.ComputeHash(System.Text.Encoding.GetEncoding("UTF-8").GetBytes(strSource + "LJSheng"));

            //转换成字符串，并取9到25位 
            //string strResult = BitConverter.ToString(bytResult, 4, 8);  
            //转换成字符串，32位 

            string strResult = BitConverter.ToString(bytResult);

            //BitConverter转换出来的字符串会在每个字符中间产生一个分隔符，需要去除掉 
            strResult = strResult.Replace("-", "");

            return strResult;
        }

        //public static string StrMD5LJSheng(string Str)
        //{
        //    if (Str != "")
        //    {
        //        return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(Str + "LJSheng", "MD5");
        //    }
        //    else
        //    {
        //        return "123456";
        //    }
        //}
    }
}
