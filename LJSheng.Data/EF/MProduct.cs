using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 发货人库存不足
    /// </summary>
    public partial class MProduct
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
        /// 会员GID
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 发货人GID
        /// </summary>
        public Guid ShopGid { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 购买库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 获得积分
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 获得购物积分
        /// </summary>
        public decimal Integral { get; set; }
    }
}
