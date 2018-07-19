using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 报表
    /// </summary>
    public partial class Report
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
        /// 分红项目[0=全部]
        /// </summary>
        public int Project { get; set; }

        /// <summary>
        /// 报表时间
        /// </summary>
        public DateTime RTime { get; set; }

        /// <summary>
        /// 订单收入
        /// </summary>
        public decimal OrderPrice { get; set; }

        /// <summary>
        /// 订单利润
        /// </summary>
        public decimal ProfitPrice { get; set; }

        /// <summary>
        /// 分享奖
        /// </summary>
        public decimal Recommendation { get; set; }

        /// <summary>
        /// 级差奖
        /// </summary>
        public decimal Differential { get; set; }

        /// <summary>
        /// 平级奖
        /// </summary>
        public decimal SameLevel { get; set; }

        /// <summary>
        /// 推荐购物积分
        /// </summary>
        public decimal SRecommendation { get; set; }

        /// <summary>
        /// 级差购物积分
        /// </summary>
        public decimal SDifferential { get; set; }

        /// <summary>
        /// 平级购物积分
        /// </summary>
        public decimal SSameLevel { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
