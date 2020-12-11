using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 分销商品状态
    /// </summary>
    public enum DistributionProductStatus
    {
        /// <summary>
        /// 推广中
        /// </summary>
        [Description("推广中")]
        Normal = 0,

        /// <summary>
        /// 取消推广
        /// </summary>
        [Description("取消推广")]
        Removed = 1,
    }
}
