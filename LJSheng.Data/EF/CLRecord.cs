using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 彩链包兑换记录
    /// </summary>
    public partial class CLRecord
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
        /// 会员Gid
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 兑换额度
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 彩链个数
        /// </summary>
        public decimal Number { get; set; }

        /// <summary>
        /// 原兑换额度
        /// </summary>
        public decimal OldMoney { get; set; }

        /// <summary>
        /// 原彩链个数
        /// </summary>
        public decimal OldNumber { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
