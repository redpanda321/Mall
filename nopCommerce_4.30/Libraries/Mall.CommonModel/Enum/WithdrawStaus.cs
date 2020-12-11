using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 提现状态
    /// </summary>
    public enum WithdrawStaus
    {
        [Description("待处理")]
        WatingAudit = 1,

        [Description("拒绝提现")]
        Refused = 2,

        [Description("提现完成")]
        Succeed = 3,

        [Description("提现失败")]
        Fail = 4,
        /// <summary>
        /// 付款中
        /// <para>第三方付款中,取消后回到待处理状态</para>
        /// </summary>
        [Description("付款中")]
        PayPending = 5,
    }
}
