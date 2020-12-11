using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 提现方式
    /// </summary>
    public enum UserWithdrawType
    {
        [Description("微信")]
        WeiChat = 1,

        [Description("支付宝")]
        ALiPay = 3
    }
}
