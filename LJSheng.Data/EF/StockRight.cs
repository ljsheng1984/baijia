using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 项目股权分配
    /// </summary>
    public partial class StockRight
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
        public Guid OrderGid { get; set; }

        /// <summary>
        /// 会员GID
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 所属项目
        /// </summary>
        public int Project { get; set; }

        /// <summary>
        /// 股权数量
        /// </summary>
        public decimal Number { get; set; }
    }
}