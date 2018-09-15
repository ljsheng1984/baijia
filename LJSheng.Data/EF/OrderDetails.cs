using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 订单详情
    /// </summary>
    public partial class OrderDetails
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
        /// 订单号的GID
        /// </summary>
        public Guid OrderGid { get; set; }

        /// <summary>
        /// 产品GID
        /// </summary>
        public Guid ProductGid { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 获得积分
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 获得购物积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 库存扣除情况[1=已扣除 2=未扣除 3=已退回]
        /// </summary>
        public int State { get; set; }
    }
}
