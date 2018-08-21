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
                return @"MIIEowIBAAKCAQEA1W3fViaVz7qTJ1U5RWZIEndMocRRSksuKeFPZFvH2+REnBNgGiXj0rzsY5Giriv4hszydUzzw7y/bA3LxBvcK3LAjKSbQm9GfmwugbNcVFri3DrwZEOTY0s+iUAnuuq2coLOd58JnVQqjP03OJa8sB3bdCh5G/hbSA5NQSZPfkavnY/ATkuJUFkN8QxTg+zGPe+Myhdo7WFJ2I2MglJkxmjRMgDNYXhl6X/Om4U+WbT1byFDLeXC48Mb03yjCP3ZWoH5oUify1cS6b3fOHBpT9JBzDrxARb4PtHvee3cCx9w6aGY7Jid3vFtRbKFvxIk5A4EoEpzW2nf4ohDdBwaywIDAQABAoIBACkQ5QIslnLL0Xil+0kRLxjuf344yfNjl9RTF/WVe1UiDNmDGOCvHbD0zP8zjHu61i0JzjUnw1eMP8DIvsZTHf/2KnplScJ0qrm0kxY9rqxEeJYreQmxvvBCEBxUjdB3TPenl5aIsU7mXPGX30IyLsgsVod5KiRpmxa4OcEpX+5JzXBGb0ZB4Kqf6sqS6xqTD/Ntf92akucaRy/Upyhp9605NI0hJ7qMfIWyUxC3vXBab1HqpvPyFq2v9gEJEvVjllJTWk/vlBTBeeqpvlIAmQ5Z/zN7PCmfWMHPMI0NSuZCaYu5vgoCxsjkfRdzKlAOIU66Ja4JGKxV37e+vPb8WoECgYEA+ArXmdpzpvIZ3wgYwzP5SNuJ6s4zUJc2+NxVeSkI1L7KgdWXQ0ZY99/6amZIvNjgHoD80vQrhrqp2gSMMb3vtjP7oX6tq14IVp8i+wOUiKhky36hGwTBI/mfjnDKYSPpYMlE9ttx1eIA4+hXMaXqyH8+legOX/nO8NM5SLs1iWsCgYEA3EbBF4mK15kmQ9naZYcBSLZ0q4s1p9VEs8T2R3YC3xmBm8S5/yEBe+EF4i1Nbxu7tpARAzxYc3dJVkB7q1h7z6HsSOIMmfUXivTRhc90kBQ4sAz+QOpwf1i6Tz8vF+xCiC+PzBiGTZ5E0/5hv6FJHVxqpfnqANZeEb4DPa6FLCECgYAbIBxp3+Dr7ef0Yw9KMaHeXNvdSMWF17OUTYwvXtRvZ5n/ztrm6YeLO/xHrH3h4RnJXDPJ4hfsePdgN0UZn3ragx7Oj2rwpazlxCtwJfkVZMz5EBUQPlnc1EylqCIJs4KvKfOXx3HQgRG12s/GRy3A1WGDxUflKQ1/eqn2ETsOlwKBgGp1n7j6dqoAkKfWUGElQI+d7wFBhKsavduMY+LhvD2LHELmb2ZI02jFtow7jOMYKj7vnelMbwtPKZiQDbUgTKZrAcGvzptSAxDbhbAANbu0qjkb7n39UGWbwl+uj9omC3m6Uus3JyG4TleO6DsvfiC8m6agBGJnOIumo/ZZtG9BAoGBAKcp4+EFf3/cwC2dP0VIs6E/DLszoay4+MQQHsyPvMPBwcAluMwJ6v9QGCaGXUQwegiLj94NO1gEVtMO2Zz8LkNh/+FOAmGp1iXRyXhO384aonQLvcSRvnMTwzlwZv4qJElvTXomfiWHq6siusVYtNodwfUQ0iQGKjzjfk0us9ve";
            }
        }

        // 支付宝公钥,查看地址：https://openhome.alipay.com/platform/keyManage.htm 对应APPID下的支付宝公钥。
        public static string alipay_public_key
        {
            get
            {
                return @"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuT7sX9l7FwmFRCtTJRXvDl70Ltsyzv+bfy4aRFOSuhl/7pvwQ+tzpZyAN+SgphqYB7t+THmvygwHHEjRJBYUk8/hc0RJs1K+7z3qZ1UxJ7Ijpwcnin0YtTQmd76Itp7jbmtB7Fsy+27f/CHqjmq9b82iXc1RGQU0dCyEfsNjIsIgcZ0h09pwcVb8tY7ok4OOv3qTTSKZsBZLis6dniXsVitGo+7L6Kp0JfFLZdYM3qBuBLNcbcrtWl9j005AIMry5yzBKWx4+uGFCzXQs5JHKfZu4BSi3jpbVh/EER/ZCBG5aZmTp8SpwQqr2Haxj3HQLSQl4UKRDfzkpH2jSNXRQwIDAQAB";
            }
        }
        // 应用ID,您的APPID
        public static string app_id
        {
            get
            {
                return "2018081661012662";
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
                return Url+"/Member/Index";
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
