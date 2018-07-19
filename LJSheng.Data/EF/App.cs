namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// App����
    /// </summary>
    public partial class App
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
        /// ������Դ
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// �汾��
        /// </summary>
        [StringLength(20)]
        public string VersionNumber { get; set; }

        /// <summary>
        /// �ڲ��汾��
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// �Ƿ�ǿ�Ƹ���[1=�� 2=��]
        /// </summary>
        public int Update { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [StringLength(200)]
        public string Content { get; set; }

        /// <summary>
        /// ���µ�ַ
        /// </summary>
        [StringLength(100)]
        public string Url { get; set; }
    }
}
