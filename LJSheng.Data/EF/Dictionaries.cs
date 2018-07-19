namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// �����ֵ�
    /// </summary>
    public partial class Dictionaries
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
        /// ����
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// �ֵ�����
        /// </summary>
        [StringLength(50)]
        public string DictionaryType { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        [StringLength(200)]
        public string Remarks { get; set; }
    }
}
