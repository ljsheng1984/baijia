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
                return "MIICXQIBAAKBgQDem9GE1OPH6by0sXBGR1DSAa6Z7dY7/JU2bkAWLkk0S7Q2azzaR38+3zbl21MQ94OAzUwNE/i5p2ULusN5geW3baCwEPNY7Dw+dJPCbbYUgZTbv+zXvpLxFDpJ5FswZEZnE1PTcaK1iur6AYSGZST0DHCf+VQJJhF1kr7hMhlbLwIDAQABAoGARUdhEWHf8duAawu90WFoebMkT6uBPPICzgnJ0B1fkvLshvMg4R3XMA3v2+FCHMmrF27M+FT612yNOfVJ2IrgnnyUML/t3JiHjm8iVenUbXg/1YkmJpC6iHriKLRNNh7GVcFM8lkK+bID60rYyZBtfPyh4FZnX06xG1J3acsrASECQQD99Tth0iuUvoWDMq4RYymC8RdzRhqdVyuUkI43guRVdifugIDMfg/UTbatltV/5TWLsuy7jlcBWXBsa2kQDmPdAkEA4GYN9haG4Q2tAqPw0BHCa3hEHIH92/xYomwvkdw7pAUOf7bpIzGQ6mgUGXzfhYSovLN/BgwdAYTvd1L2xSbgewJBAKFjJsmTdn6gVNh7bINAMTE1ZWGsO12h9+ABvV8pn1FwNRAsOa26rvSMLntCT4tnbg/JNQg7/K6u1/MOj0XU30ECQQDEGWtp+xWz5veirxHrQxncQSWEpDTs4gGzL0gC96tHEwQwp+/lSZOSxZGLFI1haSTsgxAmgL94bGu4o0/zyp2xAkAhvIe6p0Ik78sUlAdSgwz++dNIrCuPgFbKhjP0Z0aObA0iMOasD6XXUOGXuoWLHLCCZQzMSAkvMAfSmBaicVsz";
            }
        }

        // 支付宝公钥,查看地址：https://openhome.alipay.com/platform/keyManage.htm 对应APPID下的支付宝公钥。
        public static string alipay_public_key
        {
            get
            {
                return "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDem9GE1OPH6by0sXBGR1DSAa6Z7dY7/JU2bkAWLkk0S7Q2azzaR38+3zbl21MQ94OAzUwNE/i5p2ULusN5geW3baCwEPNY7Dw+dJPCbbYUgZTbv+zXvpLxFDpJ5FswZEZnE1PTcaK1iur6AYSGZST0DHCf+VQJJhF1kr7hMhlbLwIDAQAB";
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
