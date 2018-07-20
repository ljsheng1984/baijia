using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 积分转Token记录
    /// </summary>
    public partial class Token
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
        /// 转换积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 类型[1=积分 2=商城个人积分 3=商城团队积分]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 兑换币种[1=BCCB, 2=FBCC]
        /// </summary>
        public int BB { get; set; }

        /// <summary>
        /// 兑换Token
        /// </summary>
        public decimal AToken { get; set; }
    }
}