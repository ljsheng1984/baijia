using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 发货人
    /// </summary>
    public partial class Consignor
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
        /// 注册用户的GID
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 发货人Gid
        /// </summary>
        public Guid MGid { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime MTime { get; set; }
    }
}