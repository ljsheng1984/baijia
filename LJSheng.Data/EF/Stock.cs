using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 库存详情
    /// </summary>
    public partial class Stock
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
        /// 产品GID
        /// </summary>
        public Guid ProductGid { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}