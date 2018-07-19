using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 分润详情报表
    /// </summary>
    public partial class ReportList
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
        /// 报表主键
        /// </summary>
        public Guid ReportGid { get; set; }

        /// <summary>
        /// 会员主键
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 分红类型[6=项目合伙人 7=主任 8=经理 9=总裁 10=生态圈 11=股东 26=彩链项目 50=彩链团队分红]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 分红积分
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 分红购物积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 分红数量
        /// </summary>
        public decimal Number { get; set; }

        /// <summary>
        /// 是否成功[1=冻结中 2=已分红]
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
