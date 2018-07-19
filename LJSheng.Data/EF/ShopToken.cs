﻿using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 积分转Token记录
    /// </summary>
    public partial class ShopToken
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
        /// 会员Gid
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 转换积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 兑换Token
        /// </summary>
        public decimal Token { get; set; }
    }
}