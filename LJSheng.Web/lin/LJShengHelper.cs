using System.Collections.Generic;
using System.Text;

namespace LJSheng.Web
{
    public static class LJShengHelper
    {
        #region 性别
        public enum Sex
        {
            未设置 = 0,
            男 = 1,
            女 = 2
        };
        #endregion

        #region 是否显示
        public enum Show
        {
            显示 = 1,
            不显示 = 2,
            推荐 = 3
        };
        #endregion

        #region 图片路径
        /// <summary>
        /// 编辑器
        /// </summary>
        public static string BJQ
        {
            get
            {
                return "/uploadfiles/bjq/";
            }
        }
        /// <summary>
        /// 产品
        /// </summary>
        public static string Product
        {
            get
            {
                return "/uploadfiles/product/";
            }
        }
        /// <summary>
        /// 用户头像
        /// </summary>
        public static string Member
        {
            get
            {
                return "/uploadfiles/member/";
            }
        }
        /// <summary>
        /// 新闻
        /// </summary>
        public static string News
        {
            get
            {
                return "/uploadfiles/news/";
            }
        }
        /// <summary>
        /// 银行凭证
        /// </summary>
        public static string Voucher
        {
            get
            {
                return "/uploadfiles/voucher/";
            }
        }
        #endregion

        #region 域名设置
        /// <summary>
        /// API接口用到的所有图片URL
        /// </summary>
        public static string ApiUrl
        {
            get
            {
                return "http://www.huizhongxin.cc";
            }
        }

        /// <summary>
        /// 后台URL
        /// </summary>
        public static string Url
        {
            get
            {
                return "http://www.huizhongxin.cc"; 
            }
        }

        /// <summary>
        /// 支付宝对账URL
        /// </summary>
        public static string alipay
        {
            get
            {
                return Url + "/pay/zfbpay.aspx";
            }
        }
        /// <summary>
        /// 微信对账URL
        /// </summary>
        public static string wxpay
        {
            get
            {
                return Url + "/pay/wxpay.aspx";
            }
        }
        #endregion

        #region 微信支付算法
        public static string BuildRequest(SortedDictionary<string, string> sParaTemp, string key)
        {
            //获取过滤后的数组
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            dicPara = FilterPara(sParaTemp);

            //组合参数数组
            string prestr = CreateLinkString(dicPara);
            //拼接支付密钥
            string stringSignTemp = prestr + "&key=" + key;

            //获得加密结果转换为大写的加密串
            return Common.MD5.GetMD5(stringSignTemp).ToUpper();
        }

        /// <summary>
        /// 除去数组中的空值和签名参数并以字母a到z的顺序排序
        /// </summary>
        /// <param name="dicArrayPre">过滤前的参数组</param>
        /// <returns>过滤后的参数组</returns>
        public static Dictionary<string, string> FilterPara(SortedDictionary<string, string> dicArrayPre)
        {
            Dictionary<string, string> dicArray = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> temp in dicArrayPre)
            {
                if (temp.Key != "sign" && !string.IsNullOrEmpty(temp.Value))
                {
                    dicArray.Add(temp.Key, temp.Value);
                }
            }

            return dicArray;
        }

        //组合参数数组
        public static string CreateLinkString(Dictionary<string, string> dicArray)
        {
            StringBuilder prestr = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in dicArray)
            {
                prestr.Append(temp.Key + "=" + temp.Value + "&");
            }

            int nLen = prestr.Length;
            prestr.Remove(nLen - 1, 1);

            return prestr.ToString();
        }
        #endregion

        #region 支付参数
        /// <summary>
        /// 支付宝
        /// </summary>
        public static string partner
        {
            get
            {
                return "2088421220545364";
            }
        }
        public static string seller_email
        {
            get
            {
                return "17057216673@163.com";
            }
        }
        public static string alipay_key
        {
            get
            {
                return "jx8f4jwx47zq7zn8qv5m9z4fvx3pzl3f";
            }
        }
        public static string private_key
        {
            get
            {
                return "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMZy8cWH4yULvFjJt5osQe/TwIRCUW1X6Y6ONpLrY3hQKpsJ/q/Aa707ujoOGnc3r0aeEHP06KliM+bYcHrT6unBBHcxSi2U28mxFhqFsX9gKK1DC2wo/EPMCQrnCVsDthCMhBgfaQ7mw45BJzarLEVeUEqXQFXR20mjgTLRo8gpAgMBAAECgYEAhHJa4pcbBrKadjfLDl7TcxlEuAD7D5tJChfoXI41ySrYBLna/bnTLm0akXywNTk5BkygdoPSdJpSQZPbHl8pvMKYQPgSL9if4uJQo7W/Ht/Qi7/arTOgd16PApM+/bFipKJSvL7+0k1p5DGZuV7vkapwX77H8wIGTupTVzmmcDECQQD742HMX86yVhfSrHdB3N8WIEIsi4fyjiaHhIEtAYMpcDsu8wZCYgCmOEZhmXfMQgrJcFfeGQUeE/1UzznbWI7/AkEAybA+qn+F5QdfHwesi5GkuFlmVUYbVQrDfe5fbt84L9yWwjbrUZIEIx1/MlLxaVDQBqn/Ctwnvqgt5IlGZmxQ1wJBAKXQa2LkdubC8e/HhMIgqeKg3a8BMz8jAI4ZVgfQhQ1USkF/zdEJPrAtP3ekVU5q8zrj75PPGKVSN2QK/mU8iPUCQDp/7V1EydBpd/SnJCwDXZS/EYiQYiMjkRp4xqOBCWoQgIXqqgyp3ptU1e0B09XpQ717F2fN/ZU2cMFF8+6HYlcCQQD0gcOfWcOgCCUjVIr0KkRqW2LRyvV7EyZmfIPR4NMrOW/x08RDR4+v2EBJSRbKdCz7jQgXqA+i5dFV1v+sbfea";
            }
        }
        public static string public_key
        {
            get
            {
                return "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCnxj/9qwVfgoUh/y2W89L6BkRAFljhNhgPdyPuBV64bfQNN1PjbCzkIM6qRdKBoLPXmKKMiFYnkd6rAoprih3/PrQEB/VsW8OoM8fxn67UDYuyBTqA23MML9q1+ilIZwBC2AQ2UBVOrFXfFl75p6/B5KsiNG9zpgmLCUYuLkxpLQIDAQAB";
            }
        }
        /// <summary>
        /// 小程序唯一appid
        /// </summary>
        public static string appid
        {
            get
            {
                return "wxb95209e7af00dc95";
            }
        }
        public static string mch_id
        {
            get
            {
                return "1488757182";
            }
        }
        public static string api_key
        {
            get
            {
                return "linjiansheng1988888LIN1314520520";
            }
        }
        /// <summary>
        /// 小程序的app secret
        /// </summary>
        public static string appsecret
        {
            get
            {
                return "80bb93f16787e43ec4f62abf64bacf91";
            }
        }
        #endregion

        #region 管理员权限
        public enum QX
        {
            管理员 = 1,
            客服部 = 2,
            编辑部 = 3
        };
        #endregion

        #region 订单优惠券
        /// <summary>
        /// 来源
        /// </summary>
        public enum LSystem
        {
            苹果 = 1,
            苹果PAD = 2,
            安卓 = 3,
            安卓PAD = 4,
            网页 = 5
        };

        /// <summary>
        /// 提现类型
        /// </summary>
        public enum TX
        {
            成功 = 5,
            审核 = 1,
            失败 = 2
        };

        /// <summary>
        /// 支付类型
        /// </summary>
        public enum PayType
        {
            支付宝 = 1,
            微信 = 2,
            线下汇款 = 3,
            余额 = 4,
            积分 = 5
        };

        /// <summary>
        /// 支付状态
        /// </summary>
        public enum PayStatus
        {
            支付成功 = 1,
            未支付 = 2,
            已退款 = 3,
            交易关闭 = 4,
            支付成功金额不对 = 5
        };
        #endregion
    }
}
