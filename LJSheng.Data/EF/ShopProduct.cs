using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 商家产品列表
    /// </summary>
    public partial class ShopProduct
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
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否显示[1=显示 2=不显示 3=首页显示]
        /// </summary>
        public int Show { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// 售价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 宣传价格
        /// </summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [StringLength(800)]
        public string Profile { get; set; }

        /// <summary>
        /// 详情介绍
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [StringLength(50)]
        public string Picture { get; set; }

        /// <summary>
        /// 图文详情
        /// </summary>
        [StringLength(50)]
        public string GraphicDetails { get; set; }

        /// <summary>
        /// 商家GID
        /// </summary>
        public Guid ShopGid { get; set; }

        /// <summary>
        /// 分类外键
        /// </summary>
        public Guid ClassifyGid { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 浏览次数
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 快递费
        /// </summary>
        public int ExpressFee { get; set; }

        /// <summary>
        /// 商品单位
        /// </summary>
        [StringLength(20)]
        public string Company { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        [StringLength(20)]
        public string Brand { get; set; }

        /// <summary>
        /// 商品编号
        /// </summary>
        [StringLength(20)]
        public string Prefix { get; set; }

        /// <summary>
        /// 借用[0=不可借用 1=借用中 2=已借用]
        /// </summary>
        public int Borrow { get; set; }
    }
}
