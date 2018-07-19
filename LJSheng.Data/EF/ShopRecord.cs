using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 商城积分记录
    /// </summary>
    public partial class ShopRecord
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
        /// 个人积分
        /// </summary>
        public decimal MIntegral { get; set; }

        /// <summary>
        /// 团队积分
        /// </summary>
        public decimal TIntegral { get; set; }

        /// <summary>
        /// 原个人积分
        /// </summary>
        public decimal OldMIntegral { get; set; }

        /// <summary>
        /// 原团队积分
        /// </summary>
        public decimal OldTIntegral { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
