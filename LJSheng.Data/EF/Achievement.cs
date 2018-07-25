using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 团队业绩
    /// </summary>
    public partial class Achievement
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
        /// 统计年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 统计月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 团队业绩
        /// </summary>
        public decimal TMoney { get; set; }

        /// <summary>
        /// 个人业绩
        /// </summary>
        public decimal MMoney { get; set; }

        /// <summary>
        /// 项目分红积分
        /// </summary>
        public decimal ProjectMoney { get; set; }

        /// <summary>
        /// 项目分红购物积分
        /// </summary>
        public decimal ProjectIntegral { get; set; }

        /// <summary>
        /// 股东分红积分
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 股东分红购物积分
        /// </summary>
        public decimal Integral { get; set; }

        /// <summary>
        /// 项目分红资金记录Gid
        /// </summary>
        public Guid? ProjectMRGid { get; set; }
        /// <summary>
        /// 股东分红资金记录Gid
        /// </summary>
        public Guid? StockRightMRGid { get; set; }

        /// <summary>
        /// 结算时候彩链等级
        /// </summary>
        public int CLLevel { get; set; }

        /// <summary>
        /// 结算状态[1=未分红 2=待分红 3=已分红]
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 分红时间
        /// </summary>
        public DateTime? BonusTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 项目备注
        /// </summary>
        [StringLength(500)]
        public string ProjectRemarks { get; set; }

        /// <summary>
        /// 股权备注
        /// </summary>
        [StringLength(500)]
        public string StockRightRemarks { get; set; }
    }
}