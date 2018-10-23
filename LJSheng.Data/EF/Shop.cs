using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 商家列表
    /// </summary>
    public partial class Shop
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
        /// 关联用户GID
        /// </summary>
        public Guid MemberGid { get; set; }

        /// <summary>
        /// 积分
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 项目类型[对应字典里的Shop列表的值]
        /// </summary>
        public int Project { get; set; }

        /// <summary>
        /// 商家名称
        /// </summary>
        [StringLength(200)]
        public string Name { get; set; }

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
        /// 图文详情
        /// </summary>
        [StringLength(50)]
        public string GraphicDetails { get; set; }

        /// <summary>
        /// 商家图片
        /// </summary>
        [StringLength(50)]
        public string Picture { get; set; }

        /// <summary>
        /// 统一社会信用代码证图片
        /// </summary>
        [StringLength(50)]
        public string USCI { get; set; }

        /// <summary>
        /// 法人
        /// </summary>
        [StringLength(50)]
        public string LegalPerson { get; set; }

        /// <summary>
        /// 许可证
        /// </summary>
        [StringLength(50)]
        public string Licence { get; set; }

        /// <summary>
        /// 浏览次数
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 状态[1=待审核 2=已审核]
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [StringLength(50)]
        public string ContactNumber { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        [StringLength(50)]
        public string RealName { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        [StringLength(20)]
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [StringLength(20)]
        public string City { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        [StringLength(20)]
        public string Area { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [StringLength(100)]
        public string Address { get; set; }

        /// <summary>
        /// 公交线路
        /// </summary>
        [StringLength(100)]
        public string Bus { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }

        /// <summary>
        /// 借用权限[0=默认 1=申请中 2=已通过]
        /// </summary>
        public int Borrow { get; set; }

        /// <summary>
        /// 代发货权限[0=默认 1=申请中 2=已通过]
        /// </summary>
        public int Consignment { get; set; }
    }
}