namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// �����쳣��¼
    /// </summary>
    public partial class ApiBug
    {
        /// <summary>
        /// ����
        /// </summary>
        [Key]
        public Guid Gid { get; set; }

        /// <summary>
        /// ���ʱ��
        /// </summary>
        public DateTime AddTime { get; set; }

        /// <summary>
        /// ���ʷ���
        /// </summary>
        [StringLength(50)]
        public string Method { get; set; }

        /// <summary>
        /// �쳣����
        /// </summary>
        [StringLength(200)]
        public string ExceptionName { get; set; }

        /// <summary>
        /// �쳣��Ϣ
        /// </summary>
        [StringLength(800)]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// ��ջ
        /// </summary>
        [StringLength(2000)]
        public string Stack { get; set; }

        /// <summary>
        /// ���ʲ���
        /// </summary>
        [StringLength(5000)]
        public string Parameter { get; set; }

        /// <summary>
        /// ��Կ
        /// </summary>
        [StringLength(20)]
        public string SecretKey { get; set; }
    }
}
