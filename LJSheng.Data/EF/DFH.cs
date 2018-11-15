using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 待发货申请
    /// </summary>
    public partial class DFH
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
        /// 商家GID
        /// </summary>
        public Guid ShopGid { get; set; }

        /// <summary>
        /// 申请的商品GID
        /// </summary>
        public Guid ProductGid { get; set; }

        /// <summary>
        /// 状态[1=申请中 2=已发布]
        /// </summary>
        public int State { get; set; }
    }
}