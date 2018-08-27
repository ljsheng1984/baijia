//-----------------------------------------------------------
// 描    述: 生成随机操作
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System;
using System.Text;

namespace LJSheng.Common
{
    public class RandStr
    {
        //  生成随机数字和字母混合字符串
        public static string GetRandomNumberString(int int_NumberLength)
        {
            string str_Number = string.Empty;
            Random theRandomNumber = new Random();

            for (int int_index = 0; int_index < int_NumberLength; int_index++)
            {
                if (int_index % 2 == 0)
                {
                    //str_Number += ((Char)(theRandomNumber.Next(26) + 65)).ToString();//随机字母
                    str_Number += returnChar(theRandomNumber);
                }
                else
                {
                    str_Number += theRandomNumber.Next(2, 9).ToString();//随机数字
                }
            }
            return str_Number;
        }

        private static string returnChar(Random theRandomNumber)
        {
            string test = ((Char)(theRandomNumber.Next(26) + 65)).ToString();
            if (test.Equals("I") || test.Equals("O"))
                return returnChar(theRandomNumber);
            else
                return test;
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string CreateValidateNumber(int length)
        {
            int[] randMembers = new int[length];
            int[] validateNums = new int[length];
            string validateNumberStr = "";
            //生成起始序列值
            int seekSeek = unchecked((int)DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            int beginSeek = (int)seekRand.Next(0, Int32.MaxValue - length * 10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }
            //生成随机数字
            for (int i = 0; i < length; i++)
            {
                Random rand = new Random(seeks[i]);
                int pownum = 1 * (int)Math.Pow(10, length);
                randMembers[i] = rand.Next(pownum, Int32.MaxValue);
            }
            //抽取随机数字
            for (int i = 0; i < length; i++)
            {
                string numStr = randMembers[i].ToString();
                int numLength = numStr.Length;
                Random rand = new Random();
                int numPosition = rand.Next(0, numLength - 1);
                validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
            }
            //生成验证码
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i].ToString();
            }
            return validateNumberStr;
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="length">随机数长度</param>
        /// <param name="mix">开始</param>
        /// <param name="max">结束</param>
        /// <returns></returns>
        public static string CreateNumber(int length, int mix, int max)
        {
            string str_Number = string.Empty;
            Random theRandomNumber = new Random();
            for (int int_index = 0; int_index < length; int_index++)
            {
                str_Number += theRandomNumber.Next(mix, max).ToString();//随机数字
            }
            return str_Number;
        }
        /// <summary>
        ///生成制定位数的随机码（数字）
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomCode(int length)
        {
            var result = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                result.Append(r.Next(0, 10));
            }
            return result.ToString();
        }
        /// <summary>
        /// 生成订单
        /// </summary>
        /// <returns></returns>
        public static string CreateOrderNO()
        {
            return DateTime.Now.ToString("yyMMddHHmmss") + CreateValidateNumber(4);
        }
    }
}
