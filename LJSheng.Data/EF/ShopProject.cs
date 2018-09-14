using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 商家分类关联列表
    /// </summary>
    public partial class ShopProject
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
        /// 商家GID
        /// </summary>
        public Guid ShopGid { get; set; }

        /// <summary>
        /// 项目类型[对应字典里的Shop列表的值]
        /// </summary>
        public int Project { get; set; }
    }
}