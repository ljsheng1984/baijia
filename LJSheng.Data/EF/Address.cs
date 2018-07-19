namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// 地址
    /// </summary>
    public partial class Address
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
        /// 默认[1=不是 2=是]
        /// </summary>
        public int Default { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [StringLength(100)]
        public string Addr { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [StringLength(20)]
        public string RealName { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [StringLength(50)]
        public string ContactNumber { get; set; }

        /// <summary>
        /// 邮编
        /// </summary>
        [StringLength(10)]
        public string PostCode{ get; set; }
    }
}
