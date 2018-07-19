using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 提现记录
    /// </summary>
    public partial class Withdrawals
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
        /// 资金记录GID
        /// </summary>
        public Guid? MRGid { get; set; }

        /// <summary>
        /// 会员Gid
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>
        [StringLength(50)]
        public string Bank { get; set; }

        /// <summary>
        /// 开户名
        /// </summary>
        [StringLength(50)]
        public string BankName { get; set; }

        /// <summary>
        /// 卡号
        /// </summary>
        [StringLength(50)]
        public string BankNumber { get; set; }

        /// <summary>
        /// 状态[1=待付款 2=已付款 3=失败]
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 付款时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}