namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Api访问
    /// </summary>
    public partial class AppApi
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
        /// 访问方法
        /// </summary>
        [StringLength(50)]
        public string Method { get; set; }

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
        /// 型号
        /// </summary>
        [StringLength(100)]
        public string Model { get; set; }

        /// <summary>
        /// imei
        /// </summary>
        [StringLength(50)]
        public string Imei { get; set; }

        /// <summary>
        /// 访问IP
        /// </summary>
        [StringLength(200)]
        public string IP { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        [StringLength(20)]
        public string Latitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        [StringLength(20)]
        public string Longitude { get; set; }

        /// <summary>
        /// 访问耗时
        /// </summary>
        public int? TimeConsuming { get; set; }
    }
}
