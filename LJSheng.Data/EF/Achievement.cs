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
        /// 直推有人达到100W(团队业绩要减掉BCD的团队业绩)
        /// </summary>
        public decimal BCDMoney { get; set; }

        /// <summary>
        /// 团队积分分红
        /// </summary>
        public decimal TeamMoney { get; set; }

        /// <summary>
        /// 团队购物积分分红
        /// </summary>
        public decimal TeamIntegral { get; set; }

        /// <summary>
        /// 一级积分扣减
        /// </summary>
        public decimal DM1Money { get; set; }

        /// <summary>
        /// 二级积分扣减
        /// </summary>
        public decimal DM2Money { get; set; }

        /// <summary>
        /// 三级积分扣减
        /// </summary>
        public decimal DM3Money { get; set; }

        /// <summary>
        /// 一级购物积分扣减
        /// </summary>
        public decimal DM1Integral { get; set; }

        /// <summary>
        /// 二级购物积分扣减
        /// </summary>
        public decimal DM2Integral { get; set; }

        /// <summary>
        /// 三级购物积分扣减
        /// </summary>
        public decimal DM3Integral { get; set; }

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
        /// 资金记录Gid
        /// </summary>
        public Guid? MRGid { get; set; }
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
        /// 下级被冻结的积分收入
        /// </summary>
        public decimal FrozenMoney { get; set; }

        /// <summary>
        /// 下级被冻结的购物积分收入
        /// </summary>
        public decimal FrozenIntegral { get; set; }

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

        /// <summary>
        /// 结算时底下团队级别[0=没有A 1=B 2=C 3=D]
        /// </summary>
        public int Team { get; set; }
    }
}