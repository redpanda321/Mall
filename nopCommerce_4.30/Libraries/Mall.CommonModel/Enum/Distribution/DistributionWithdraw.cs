using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 佣金提现类型
    /// </summary>
    public enum DistributionWithdrawType
    {
        /// <summary>
        /// 预存款
        /// </summary>
        [Description("预存款")]
        Capital = 1,
        /// <summary>
        /// 微信
        /// </summary>
        [Description("微信")]
        WeChat = 2,
        /// <summary>
        /// 支付宝
        /// </summary>
        [Description("支付宝")]
        Alipay = 3
    }
    /// <summary>
    /// 佣金提现状态
    /// </summary>
    public enum DistributionWithdrawStatus
    {
        /// <summary>
        /// 待审核
        /// </summary>
        [Description("待审核")]
        UnAudit = 1,
        /// <summary>
        /// 通过
        /// </summary>
        [Description("付款中")]
        WaitPayment = 2,
        /// <summary>
        /// 拒绝
        /// </summary>
        [Description("已拒绝")]
        Refused = 3,
        /// <summary>
        /// 提现失败
        /// </summary>
        [Description("提现失败")]
        Fail = 4,
        /// <summary>
        /// 付款成功
        /// </summary>
        [Description("提现成功")]
        Completed = 5
    }
}
