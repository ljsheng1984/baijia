namespace LJSheng.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// 广告
    /// </summary>
    public partial class AD
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
        /// 是否显示[1=显示 2=不显示]
        /// </summary>
        public int Show { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [StringLength(50)]
        public string Picture { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(50)]
        public string Title { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [StringLength(200)]
        public string Profile { get; set; }

        /// <summary>
        /// 外链地址
        /// </summary>
        [StringLength(200)]
        public string Url { get; set; }

        /// <summary>
        /// 访问量
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 到期时间
        /// </summary>
        public DateTime ExpireTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 项目类型[对应字典里的ClassifyType列表的值]
        /// </summary>
        public int Project { get; set; }
    }
}
