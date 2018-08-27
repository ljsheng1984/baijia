using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// ����
    /// </summary>
    public partial class Order
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
        /// ��Ա�̼�GID
        /// </summary>
        public Guid? ShopGid { get; set; }

        /// <summary>
        /// ��ԱGID
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [StringLength(200)]
        public string Product { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        [StringLength(50)]
        public string OrderNo { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        [StringLength(50)]
        public string TradeNo { get; set; }

        /// <summary>
        /// ֧������ʱ��
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// ֧��״̬[1=֧���ɹ� 2=δ֧�� 3=���˿� 4=���׹ر� 5=֧���ɹ�������]
        /// </summary>
        public int PayStatus { get; set; }

        /// <summary>
        /// ֧������[1=֧���� 2=΢�� 3=���»�� 4=��� 5=����]
        /// </summary>
        public int PayType { get; set; }

        /// <summary>
        /// ����֧��ԭ�ܽ��
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// �µ�ʱ����֧�����
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// ����֧��ʵ�ʽ��
        /// </summary>
        public decimal PayPrice { get; set; }

        /// <summary>
        /// �Ż�ȯ�ֿ۽��
        /// </summary>
        public decimal CouponPrice { get; set; }

        /// <summary>
        /// ʹ�õ��Ż�ȯGid
        /// </summary>
        public Guid? CouponNo { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// ֧������
        /// </summary>
        [StringLength(2000)]
        public string Pay { get; set; }

        /// <summary>
        /// ���״̬[1=������ 2=����� 3=��ǩ�� 4=�˻�]
        /// </summary>
        public int ExpressStatus { get; set; }

        /// <summary>
        /// ��ݹ�˾
        /// </summary>
        [StringLength(50)]
        public string Express { get; set; }

        /// <summary>
        /// ��ݺ�
        /// </summary>
        [StringLength(50)]
        public string ExpressNumber { get; set; }

        /// <summary>
        /// ��ݵ�ַ
        /// </summary>
        [StringLength(100)]
        public string Address { get; set; }

        /// <summary>
        /// �ջ���
        /// </summary>
        [StringLength(50)]
        public string RealName { get; set; }

        /// <summary>
        /// ��ϵ�绰
        /// </summary>
        [StringLength(50)]
        public string ContactNumber { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// ����GID
        /// </summary>
        public Guid? RobGid { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime? RobTime { get; set; }

        /// <summary>
        /// ����״̬[1=������ 2=��֧��]
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// ��û���
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// ��ù������
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// �����ɵù�Ȩ
        /// </summary>
        public decimal StockRight { get; set; }

        /// <summary>
        /// ������������Ŀ
        /// </summary>
        public int Project { get; set; }

        /// <summary>
        /// ��������[��Ŀ1����>1=��ͨ��Ʒ 2=�ϻ�����Ʒ][��Ŀ2����>3=��˾���� 4=��Ա���� 5=��Աת��]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// �µ�ʱ��ļ���
        /// </summary>
        public int CLLevel { get; set; }

        /// <summary>
        /// ����ƾ֤
        /// </summary>
        public string Voucher { get; set; }
    }
}