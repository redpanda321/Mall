
using System.ComponentModel;
using System.Configuration;

namespace Mall.Entities
{
    public partial class ApplyWithdrawInfo
    {
       
        public decimal PayAmount
        {
            get
            {
                return ApplyAmount - Poundage;
            }
        }

        /// <summary>
        /// 申请退款状态
        /// </summary>
        public enum ApplyWithdrawStatus
        {

            /// <summary>
            /// 待处理
            /// </summary>
            [Description("待处理")]
            WaitConfirm = 1,

            /// <summary>
            /// 付款失败
            /// </summary>
            [Description("付款失败")]
            PayFail = 2,

            /// <summary>
            /// 提现成功
            /// </summary>
            [Description("提现成功")]
            WithDrawSuccess = 3,

            /// <summary>
            /// 已拒绝
            /// </summary>
            [Description("已拒绝")]
            Refuse = 4,
            /// <summary>
            /// 付款中
            /// <para>第三方付款中,取消后回到待处理状态</para>
            /// </summary>
            [Description("付款中")]
            PayPending = 5,
        }
    }
}
