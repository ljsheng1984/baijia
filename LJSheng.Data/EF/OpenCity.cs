using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 已开通城市
    /// </summary>
    public partial class OpenCity
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
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary>
        public int Show { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        [StringLength(20)]
        public string Province { get; set; }

        /// <summary>
        /// 已开通城市
        /// </summary>
        [StringLength(20)]
        public string City { get; set; }
    }
}
