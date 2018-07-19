using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 积分冻结
    /// </summary>
    public partial class FrozenIntegral
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
        /// 冻结类型[1=会员支取基数 2=团队累计满足冻结]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 会员Gid
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 冻结积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 扣除个人积分
        /// </summary>
        public decimal MIntegral { get; set; }

        /// <summary>
        /// 倍数
        /// </summary>
        public int Multiple { get; set; }

        /// <summary>
        /// 解冻时间
        /// </summary>
        public DateTime? ThawTime { get; set; }

        /// <summary>
        /// 状态[1=成功 2=冻结 3=取消]
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
