namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// 数据字典
    /// </summary>
    public partial class Dictionaries
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
        /// 字典类型
        /// </summary>
        [StringLength(50)]
        public string DictionaryType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(200)]
        public string Remarks { get; set; }
    }
}
