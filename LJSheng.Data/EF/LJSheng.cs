using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 管理员
    /// </summary>
    public partial class LJSheng
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
        /// 帐号
        /// </summary>
        [Required, MaxLength(50)]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required, MaxLength(50)]
        public string PWD { get; set; }

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
        /// 性别
        /// </summary>
        [StringLength(2)]
        public string Gender { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [StringLength(50)]
        public string Picture { get; set; }

        /// <summary>
        /// 权限[管理员 冻结]
        /// </summary>
        [StringLength(50)]
        public string Jurisdiction { get; set; }

        /// <summary>
        /// 最后登录标识
        /// </summary>
        [StringLength(50)]
        public string LoginIdentifier { get; set; }
    }
}
