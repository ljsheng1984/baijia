using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 短信
    /// </summary>
    public partial class SMS
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
        /// 手机号码
        /// </summary>
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 短信内容
        /// </summary>
        [StringLength(500)]
        public string Content { get; set; }

        /// <summary>
        /// 回执内容
        /// </summary>
        [StringLength(200)]
        public string ReceiptContent { get; set; }

        /// <summary>
        /// 短信类型[1=验证码 2=系统短信]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 计费条数
        /// </summary>
        public int Number { get; set; }
    }
}
