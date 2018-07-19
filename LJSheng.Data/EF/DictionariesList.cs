namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// 字典列表
    /// </summary>
    public partial class DictionariesList
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
        /// 字典Gid
        /// </summary>
        public Guid DGid { get; set; }

        /// <summary>
        /// 键
        /// </summary>
        [StringLength(50)]
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [StringLength(50)]
        public string Value { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
