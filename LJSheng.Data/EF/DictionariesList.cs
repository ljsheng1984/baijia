namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// �ֵ��б�
    /// </summary>
    public partial class DictionariesList
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
        /// �ֵ�Gid
        /// </summary>
        public Guid DGid { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        [StringLength(50)]
        public string Key { get; set; }

        /// <summary>
        /// ֵ
        /// </summary>
        [StringLength(50)]
        public string Value { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
