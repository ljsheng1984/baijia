namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Api����
    /// </summary>
    public partial class AppApi
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
        /// ������Դ
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// �汾��
        /// </summary>
        [StringLength(20)]
        public string VersionNumber { get; set; }

        /// <summary>
        /// �ͺ�
        /// </summary>
        [StringLength(100)]
        public string Model { get; set; }

        /// <summary>
        /// imei
        /// </summary>
        [StringLength(50)]
        public string Imei { get; set; }

        /// <summary>
        /// ����IP
        /// </summary>
        [StringLength(200)]
        public string IP { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [StringLength(20)]
        public string Latitude { get; set; }

        /// <summary>
        /// γ��
        /// </summary>
        [StringLength(20)]
        public string Longitude { get; set; }

        /// <summary>
        /// ���ʺ�ʱ
        /// </summary>
        public int? TimeConsuming { get; set; }
    }
}
