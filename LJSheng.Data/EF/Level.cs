using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 会员等级
    /// </summary>
    public partial class Level
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
        /// 等级名称
        /// </summary>
        [Required, MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 等级标识
        /// </summary>
        public int LV { get; set; }

        /// <summary>
        /// 全站分红比例
        /// </summary>
        public decimal Bonus { get; set; }

        /// <summary>
        /// 分享奖比例
        /// </summary>
        public decimal Recommendation { get; set; }

        /// <summary>
        /// 级差奖比例
        /// </summary>
        public decimal Differential { get; set; }

        /// <summary>
        /// 平级奖比例
        /// </summary>
        public decimal SameLevel { get; set; }

        /// <summary>
        /// 项目分红比例
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// 股权收益
        /// </summary>
        public decimal EquityProfit { get; set; }

        /// <summary>
        /// 奖励购物比例
        /// </summary>
        public decimal ShopProfit { get; set; }

        /// <summary>
        /// 需要几个下级
        /// </summary>
        public int LNumber { get; set; }

        /// <summary>
        /// 购买多少金额达到
        /// </summary>
        public int BuyAmount { get; set; }

        /// <summary>
        /// 分销数量
        /// </summary>
        public int Distribution { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [StringLength(20)]
        public string Label { get; set; }

        /// <summary>
        /// 所属项目
        /// </summary>
        public int Project { get; set; }

        /// <summary>
        /// 升级条件[1=一次性 2=累计]
        /// </summary>
        public int UP { get; set; }

        /// <summary>
        /// 允许出售库存[1=不允许 2=允许]
        /// </summary>
        public int SellStock { get; set; }

        /// <summary>
        /// 商城消费积分倍数
        /// </summary>
        public int Multiple { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
