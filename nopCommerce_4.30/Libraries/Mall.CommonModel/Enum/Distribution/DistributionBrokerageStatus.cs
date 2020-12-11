using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 分销结算状态
    /// </summary>
    public enum DistributionBrokerageStatus
    {
        /// <summary>
        /// 不可结算(下单未付款,货到付款未收货)
        /// </summary>

        [Description("不可结算")]
        NotAvailable = -1,
        /// <summary>
        /// 未结算(未到结算日期,正在售后中)
        /// </summary>
        [Description("未结算")]
        NotSettled = 0,

        /// <summary>
        /// 已结算
        /// </summary>
        [Description("已结算")]
        Settled = 1
    }
}
