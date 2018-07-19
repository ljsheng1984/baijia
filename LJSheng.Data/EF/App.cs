namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// App更新
    /// </summary>
    public partial class App
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
        /// 访问来源
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [StringLength(20)]
        public string VersionNumber { get; set; }

        /// <summary>
        /// 内部版本号
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 是否强制更新[1=不 2=是]
        /// </summary>
        public int Update { get; set; }

        /// <summary>
        /// 更新内容
        /// </summary>
        [StringLength(200)]
        public string Content { get; set; }

        /// <summary>
        /// 更新地址
        /// </summary>
        [StringLength(100)]
        public string Url { get; set; }
    }
}
