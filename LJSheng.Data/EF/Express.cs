using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 视频购买记录
    /// </summary>
    public partial class Express
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
        /// 是否显示[1=显示 2=不显示]
        /// </summary>
        public int Show { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 查询编码
        /// </summary>
        [StringLength(50)]
        public string Type { get; set; }

        /// <summary>
        /// 首字母
        /// </summary>
        [StringLength(50)]
        public string Letter { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        [StringLength(50)]
        public string Tel { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        [StringLength(50)]
        public string Number { get; set; }
    }
}
