namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// 微信关键字
    /// </summary>
    public partial class WXKeyword
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
        /// 是否显示
        /// </summary>
        public int Show { get; set; }

        /// <summary>
        /// 类型[1=文字 2=图文]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        [StringLength(50)]
        public string Keyword { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        [StringLength(500)]
        public string Content { get; set; }
    }
}
