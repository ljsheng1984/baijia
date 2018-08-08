using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 资金记录
    /// </summary>
    public partial class MoneyRecord
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
        /// 订单Gid
        /// </summary>
        public Guid? OrderGid { get; set; }

        /// <summary>
        /// 会员Gid
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 购物积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 原余额
        /// </summary>
        public decimal OldMoney { get; set; }

        /// <summary>
        /// 原购物积分
        /// </summary>
        public decimal OldIntegral { get; set; }

        /// <summary>
        /// 类型[1=消费 2=提现 3=合伙人进货 4=购物分成 5=推荐奖 6=级差 7=平级 8=合伙人分红 9=股东分红 20=分享奖 21=发货人积分扣除 22=彩链团队分红 23=彩链项目分红]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
