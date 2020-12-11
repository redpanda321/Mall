using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 销售员状态
    /// </summary>
    public enum DistributorStatus
    {
        /// <summary>
        /// 普通会员
        /// </summary>
        [Description("普通会员")]
        UnApply = 0,
        /// <summary>
        /// 待审核
        /// </summary>
        [Description("待审核")]
        UnAudit = 1,
        /// <summary>
        /// 已审核
        /// </summary>
        [Description("已审核")]
        Audited = 2,
        /// <summary>
        /// 已拒绝
        /// </summary>
        [Description("已拒绝")]
        Refused = 3,
        /// <summary>
        /// 已清退
        /// </summary>
        [Description("已清退")]
        NotAvailable = 4,
    }
}
