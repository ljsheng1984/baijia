using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 订单
    /// </summary>
    public partial class Order
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
        /// 会员商家GID
        /// </summary>
        public Guid? ShopGid { get; set; }

        /// <summary>
        /// 会员GID
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [StringLength(200)]
        public string Product { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [StringLength(50)]
        public string OrderNo { get; set; }

        /// <summary>
        /// 网银订单号
        /// </summary>
        [StringLength(50)]
        public string TradeNo { get; set; }

        /// <summary>
        /// 支付对账时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 支付状态[1=支付成功 2=未支付 3=已退款 4=交易关闭 5=支付成功但金额不对]
        /// </summary>
        public int PayStatus { get; set; }

        /// <summary>
        /// 支付类型[1=支付宝 2=微信 3=线下汇款 4=余额 5=积分]
        /// </summary>
        public int PayType { get; set; }

        /// <summary>
        /// 订单支付原总金额
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// 下单时在线支付金额
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 网上支付实际金额
        /// </summary>
        public decimal PayPrice { get; set; }

        /// <summary>
        /// 优惠券抵扣金额
        /// </summary>
        public decimal CouponPrice { get; set; }

        /// <summary>
        /// 使用的优惠券Gid
        /// </summary>
        public Guid? CouponNo { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 支付参数
        /// </summary>
        [StringLength(2000)]
        public string Pay { get; set; }

        /// <summary>
        /// 快递状态[1=待出货 2=快递中 3=已签收 4=退回]
        /// </summary>
        public int ExpressStatus { get; set; }

        /// <summary>
        /// 快递公司
        /// </summary>
        [StringLength(50)]
        public string Express { get; set; }

        /// <summary>
        /// 快递号
        /// </summary>
        [StringLength(50)]
        public string ExpressNumber { get; set; }

        /// <summary>
        /// 快递地址
        /// </summary>
        [StringLength(100)]
        public string Address { get; set; }

        /// <summary>
        /// 收货人
        /// </summary>
        [StringLength(50)]
        public string RealName { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [StringLength(50)]
        public string ContactNumber { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// 抢单GID
        /// </summary>
        public Guid? RobGid { get; set; }

        /// <summary>
        /// 抢单时间
        /// </summary>
        public DateTime? RobTime { get; set; }

        /// <summary>
        /// 货款状态[1=冻结中 2=已支付]
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 获得积分
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 获得购物积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 购买后可得股权
        /// </summary>
        public decimal StockRight { get; set; }

        /// <summary>
        /// 订单所属的项目
        /// </summary>
        public int Project { get; set; }

        /// <summary>
        /// 订单类型[项目1类型>1=普通商品 2=合伙人商品][项目2类型>3=公司发货 4=会员发货 5=会员转让]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 下单时候的级别
        /// </summary>
        public int CLLevel { get; set; }

        /// <summary>
        /// 付款凭证
        /// </summary>
        public string Voucher { get; set; }
    }
}