using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 会员表
    /// </summary>
    public partial class Member
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public Guid Gid { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 微信Openid
        /// </summary>
        [StringLength(50)]
        public string Openid { get; set; }

        /// <summary>
        /// 推荐人GID
        /// </summary>
        public Guid? MemberGid { get; set; }

        /// <summary>
        /// 会员ID
        /// </summary>
        public int MID { get; set; }

        /// <summary>
        /// 帐号
        /// </summary>
        [Required, MaxLength(50)]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required, MaxLength(50)]
        public string PWD { get; set; }

        /// <summary>
        /// 支付密码
        /// </summary>
        [MaxLength(50)]
        public string PayPWD { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [StringLength(20)]
        public string RealName { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [StringLength(50)]
        public string ContactNumber { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [StringLength(2)]
        public string Gender { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        [StringLength(20)]
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [StringLength(20)]
        public string City { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        [StringLength(20)]
        public string Area { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [StringLength(100)]
        public string Address { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? DateBirth { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        [StringLength(200)]
        public string Picture { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [StringLength(20)]
        public string NickName { get; set; }

        /// <summary>
        /// 获得积分
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 获得购物积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 货款
        /// </summary>
        public decimal ProductMoney { get; set; }

        /// <summary>
        /// 商城个人积分
        /// </summary>
        public decimal MIntegral { get; set; }

        /// <summary>
        /// 商城团队积分
        /// </summary>
        public decimal TIntegral { get; set; }

        /// <summary>
        /// 商城可转换积分
        /// </summary>
        public decimal ShopIntegral { get; set; }
        /// <summary>
        /// 商城货款
        /// </summary>
        public decimal ShopMoney { get; set; }

        /// <summary>
        ///  发布彩链包金额
        /// </summary>
        public decimal CLMoney { get; set; }

        /// <summary>
        /// 股权
        /// </summary>
        public decimal StockRight { get; set; }

        /// <summary>
        /// 项目1级别
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 项目2级别
        /// </summary>
        public int CLLevel { get; set; }

        /// <summary>
        /// 权限[正常 审核中 冻结]
        /// </summary>
        [StringLength(50)]
        public string Jurisdiction { get; set; }

        /// <summary>
        /// 注册IP
        /// </summary>
        [StringLength(50)]
        public string IP { get; set; }

        /// <summary>
        /// 最后登录标识
        /// </summary>
        [StringLength(50)]
        public string LoginIdentifier { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>
        [StringLength(50)]
        public string Bank { get; set; }

        /// <summary>
        /// 开户名
        /// </summary>
        [StringLength(50)]
        public string BankName { get; set; }

        /// <summary>
        /// 卡号
        /// </summary>
        [StringLength(50)]
        public string BankNumber { get; set; }

        /// <summary>
        /// 项目合伙人
        /// </summary>
        public int Level6 { get; set; }
        /// <summary>
        /// 县级代理
        /// </summary>
        public int Level7 { get; set; }
        /// <summary>
        /// 市级代理
        /// </summary>
        public int Level8 { get; set; }
        /// <summary>
        /// 省级代理
        /// </summary>
        public int Level9 { get; set; }
        /// <summary>
        /// 总业绩
        /// </summary>
        public int TMoney { get; set; }
        /// <summary>
        /// 总人数
        /// </summary>
        public int TNumber { get; set; }

        /// <summary>
        /// 会员单价
        /// </summary>
        public decimal BuyPrice { get; set; }
        /// <summary>
        /// VIP会员
        /// </summary>
        public int Level22 { get; set; }
        /// <summary>
        /// 总代
        /// </summary>
        public int Level23 { get; set; }
        /// <summary>
        /// 联创
        /// </summary>
        public int Level24 { get; set; }
        /// <summary>
        /// 合伙人
        /// </summary>
        public int Level25 { get; set; }
        /// <summary>
        /// 总业绩
        /// </summary>
        public int CLTMoney { get; set; }
        /// <summary>
        /// 总人数
        /// </summary>
        public int CLTNumber { get; set; }
        /// <summary>
        /// 升级时间
        /// </summary>
        public DateTime? UPTime { get; set; }

        /// <summary>
        /// 冻结时间
        /// </summary>
        public DateTime? LockTime { get; set; }
        /// <summary>
        /// 同步状态[1=未同步 2=已同步 3=APP用户]
        /// </summary>
        public int APP { get; set; }
    }
}