using System;
using System.ComponentModel.DataAnnotations;

namespace LJSheng.Data
{
    /// <summary>
    /// 管理员操作日记
    /// </summary>
    public partial class AdminLog
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
        /// 操作表的类型[1=订单 2=会员 3=提现 4=报表]
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 操作数据的Gid
        /// </summary>
        public Guid TGid { get; set; }

        /// <summary>
        /// 管理员名称
        /// </summary>
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 操作时候的IP
        /// </summary>
        [StringLength(20)]
        public string IP { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}
