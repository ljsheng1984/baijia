namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// 访问异常记录
    /// </summary>
    public partial class ApiBug
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
        /// 异常名称
        /// </summary>
        [StringLength(200)]
        public string ExceptionName { get; set; }

        /// <summary>
        /// 异常消息
        /// </summary>
        [StringLength(800)]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// 堆栈
        /// </summary>
        [StringLength(2000)]
        public string Stack { get; set; }

        /// <summary>
        /// 访问参数
        /// </summary>
        [StringLength(5000)]
        public string Parameter { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        [StringLength(20)]
        public string SecretKey { get; set; }
    }
}
