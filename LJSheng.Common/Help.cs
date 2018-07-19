namespace LJSheng.Common
{
    public class Help
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
        /// 广告
        /// </summary>
        public static string AD
        {
            get
            {
                return "/uploadfiles/ad/";
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
        /// 产品
        /// </summary>
        public static string Market
        {
            get
            {
                return "/uploadfiles/market/";
            }
        }
        /// <summary>
        /// 管理员
        /// </summary>
        public static string LJSheng
        {
            get
            {
                return "/uploadfiles/ljsheng/";
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
        /// 分类
        /// </summary>
        public static string Classify
        {
            get
            {
                return "/uploadfiles/classify/";
            }
        }
        /// <summary>
        /// 商家
        /// </summary>
        public static string Shop
        {
            get
            {
                return "/uploadfiles/shop/";
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
                return "http://baijmc.com";
            }
        }

        /// <summary>
        /// 后台URL
        /// </summary>
        public static string Url
        {
            get
            {
                return "http://baijmc.com";
            }
        }

        /// <summary>
        /// 支付宝对账URL
        /// </summary>
        public static string alipay
        {
            get
            {
                return Url+"/pay/zfbpay.aspx";
            }
        }
        /// <summary>
        /// 微信对账URL
        /// </summary>
        public static string wxpay
        {
            get
            {
                return Url+"/pay/wxpay.aspx";
            }
        }
        #endregion

        #region 支付宝支付参数
        // 商户私钥，您的原始格式RSA私钥
        public static string private_key
        {
            get
            {
                return "MIIEowIBAAKCAQEAs74P4E7GX9ZEcoGXLaPOMNx9GBZvi4r8dH7xlTbDvYGeEogpyIwG+NL6bsMxVVm5OLLoYtKWiKPaT7XVbXoAeAFwaja8dhuYh07SfMl7eUWeKgcl9qdjclMBeFJ02MBXwo48gEMBqQAPX2e6yDHWFzdTCzq+vBHDpauNHydI0pyGKUrUUWVIbetNEcPDemlfT1mSbVEQiE6VAxx1FhdNlBfhhPFLNrnLeDgr+BzZrKrzcuEsxVn2iKoFm+nU/+fyihoB+Et7zZRb5rxaXTdBFOLRngVmqoW4gIyBFlukJpEKcfqP3TE2D62rDphR76MGGgoyZYFdCxOVwZAmuwkbPwIDAQABAoIBAEhd02KKFNBtIi3AAy/67X86pprWHZ7fHHmCyp066WAZGZ14eOonhn8T4oRJzkkA0NQFJA71nmnxHmpNWVq0bg/i9HGvC/25apW/pHtdW3seDmn5as1940oaJVNeT3EdL7hDMBSr5bU3MvlEeC01825xddvlkPmDZXQEN9M9K9KvSZB6jKUPEJzUF3s7Z7E4kPzwqfkdMfpTgoVpeaKzvV5Ut/fwkd6ZrzmivbrGEmetxuQBLIHAr9Nvn+EDenQczBnk2ScxC2u40sI8BgJjXTgd6frjlyPJb+y1h3afYfNuJzyR1KRXG/xsRzX/nZwDOCGrQbY51IutyGW0ZI6MHiECgYEA2CaJDcmMxrFy+kaHFwhu4oNYuaD0nQGWF7qDD/ROJ4X1WzT1ZyZjh5SDlp004ZOlYBec/p7et0D8vWqoSDgLX9YGtTbgyTixeg1U7UurvyPDDrHOyKy52fZ+Zp7uVhn9Y6WongmQ2Jh1acMjTsMSTufP/ZFrQ0NpJbt8NzkgytcCgYEA1OE0txurgvWvrkCprXezQKYIWPIrLGAmZ/gDcsp0D3fPOCpLs8tNEbxzcxeJXLESUZQK3+dKKJhiaGC5m+uDsxoOB/eXaxu+R/ubJ8WkRWzVne4yXgqFGtigzDUxL1z3rIPulU4+gaPE8HQ8m6zvhfWEeOxEuLmJS1/GpV/JzdkCgYEAvOXgfLYZW0OU0fh08sIh9b8H+SIa5GOQgigzhevhqVS+ufisRnscHE7EOQomPsVDj5jTUQoSZWBcAEWHz6V6+ 0QWFKRjCjeLkri38bFh5Och/mOR7XpV6ZArow9eBs0rzrvlgkel2ERUyBsLbje4dlDMSjOd4+izqZAR3EkGyX0CgYAIt+Bsz1lK170GKefJDunsb+BPpDtYaeeowmOBxADU0VdPOYw4JM9XNpyTzhb2ENprNODtIo/K8dK166AuXoraPursMJum60/zqr5D9rgvj7F/8k1lxJCG9PBA7LzC78/E5PJUcwvkS0y5fEkvJvn4RoHSYfq+hfq1d4qvIykkoQKBgHMgRmcNamMT5SKW3H4P22CFbKpC/uqKBJyx7RQHn7L8JEkL2FJAIYw9JBLK7mDg0RmtkFP2oX5ybjpYwy59cXiKtDZlRn/z/gdlpVVJzikW9i7DjJrEoLyey3u2oOn5tOc33D+B6vWZQGH0atb25ayDt8gVyhHLQO2vgS3x+zK5";
            }
        }

        // 支付宝公钥,查看地址：https://openhome.alipay.com/platform/keyManage.htm 对应APPID下的支付宝公钥。
        public static string alipay_public_key
        {
            get
            {
                return "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr1cSCbE9v2GUOu/IQLnrRnx6gb4iz/afIC7aZ/d+RCGwuL2M85HZXu3MLxlUxxBD1LRTfzXn5csIWDbJjLFSC1WNtFcAhlk6VA8ITrEwFODK9kc8z9NKLxlEP57VjXt50y/zR1OKFyU1/5oR6bP2aacsqtC7O+IpQ+w3Zzj00EfIj6oBniJOImOY/WA6snOU9G1Ti+d6AqvbvI1uD2YmKWZ4tBuWhVezE34+JGsOPCodM5OE+RlYhR4Vyc+ou6A5lmgrQQ1mbJ7pIanSoMpvEFdMDQ0/RCJGSw7MyKb+8dVxwZ3hOwHDKsQK1MJS0oJB4SfTRsLUpiY1vZKHkLVk8QIDAQAB";
            }
        }
        // 应用ID,您的APPID
        public static string app_id
        {
            get
            {
                return "2018013002112861";
            }
        }

        // 支付宝网关
        public static string gatewayUrl
        {
            get
            {
                return "https://openapi.alipay.com/gateway.do";
            }
        }
        // 回调地址
        public static string return_url
        {
            get
            {
                return Url+"/Member/Order";
            }
        }
        // 自动对账地址
        public static string notify_url
        {
            get
            {
                return Url+"/Pay/NotifyUrl";
            }
        }
        // 商城自动对账地址
        public static string shopnotify_url
        {
            get
            {
                return Url + "/Pay/ShopNotifyUrl";
            }
        }
        // 支付中途退出返回商户网站地址
        public static string quit_url
        {
            get
            {
                return Url+"/Member/Order";
            }
        }
        // 签名方式
        public static string sign_type
        {
            get
            {
                return "RSA2";
            }
        }

        // 编码格式
        public static string charset
        {
            get
            {
                return "UTF-8";
            }
        }
        #endregion

        #region 微信支付参数
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
        /// <summary>
        /// 商户号
        /// </summary>
        public static string mch_id
        {
            get
            {
                return "1488757182";
            }
        }
        /// <summary>
        /// 微信支付密钥
        /// </summary>
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

        #region 资金类型
        public enum MType
        {
            消费 =1,
            提现 =2,
            //合伙人进货=3,
            //购物分成=4,
            //推荐奖=5,
            //级差=6,
            //平级=7,
            //合伙人项目分红=8,
            股东分红=9,
            分享奖=20,
            彩链团队分红=22,
            彩链项目分红=23,
            发货人积分扣除 = 24,
            发货人积分解除 = 25
        };
        #endregion
    }
}
