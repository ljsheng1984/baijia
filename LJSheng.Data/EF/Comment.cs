using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 评论
    /// </summary>
    public partial class Comment
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
        /// 是否显示
        /// </summary>
        public int Show { get; set; }

        /// <summary>
        /// 会员GID
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 产品GID
        /// </summary>
        public Guid ProductGid { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        [StringLength(800)]
        public string Content { get; set; }

        /// <summary>
        /// 回复
        /// </summary>
        [StringLength(800)]
        public string Reply { get; set; }

        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? ReplyTime { get; set; }

        /// <summary>
        /// 图片列表
        /// </summary>
        [StringLength(200)]
        public string Picture { get; set; }
    }
}
