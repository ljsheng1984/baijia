using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 会员关系
    /// </summary>
    public partial class MRelation
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
        /// 注册用户的GID
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M1 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M1Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M2 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M2Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M3 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M3Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M4 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M4Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M5 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M5Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M6 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M6Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M7 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M7Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M8 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M8Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M9 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M9Time { get; set; }

        /// <summary>
        /// 上级GID
        /// </summary>
        public Guid? M10 { get; set; }

        /// <summary>
        /// 推荐时间
        /// </summary>
        public DateTime? M10Time { get; set; }
    }
}