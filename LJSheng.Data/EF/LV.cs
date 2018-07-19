using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 奖励比例
    /// </summary>
    public partial class LV
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
        /// 对应等级
        /// </summary>
        public int LVID { get; set; }

        /// <summary>
        /// 级差比例
        /// </summary>
        public decimal Differential { get; set; }

        /// <summary>
        /// 平级比例
        /// </summary>
        public decimal SameLevel { get; set; }

        /// <summary>
        /// 奖励购物比例
        /// </summary>
        public decimal ShopProfit { get; set; }

        /// <summary>
        /// 推荐等级
        /// </summary>
        public int Number { get; set; }
    }
}
