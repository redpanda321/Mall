using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core.Plugins.Message
{
   public class MessageContent
    {
        public string Bind { set; get; }

        /// <summary>
        /// 会员修改交易密码成功
        /// </summary>
        public string EditPayPassWord { get; set; }

        /// <summary>
        /// 会员修改登录密码成功
        /// </summary>
        public string EditLoginPassWord { get; set; }

        /// <summary>
        /// 待付款提醒
        /// </summary>
        public string OrderCreated { set; get; }

        /// <summary>
        /// 订单支付成功通知
        /// </summary>
        public string OrderPay { set; get; }

        /// <summary>
        /// 订单发货提醒
        /// </summary>
        public string OrderShipping { set; get; }

        /// <summary>
        /// 申请退款
        /// </summary>
        public string OrderRefundApply { set; get; }

        /// <summary>
        /// 退款成功
        /// </summary>
        public string OrderRefundSuccess { set; get; }

        /// <summary>
        /// 商家/门店拒绝退款
        /// </summary>
        public string OrderRefundFail { set; get; }

        /// <summary>
        /// 申请退货
        /// </summary>
        public string OrderReturnApply { set; get; }

        /// <summary>
        /// 商家/门店同意退货
        /// </summary>
        public string OrderReturnSuccess { set; get; }

        /// <summary>
        /// 商家/门店拒绝退货
        /// </summary>
        public string OrderReturnFail { set; get; }

        /// <summary>
        /// 拼团：开团成功
        /// </summary>
        public string FightGroupOpenSuccess { set; get; }
        /// <summary>
        /// 拼团：参团成功
        /// </summary>
        public string FightGroupJoinSuccess { set; get; }
        /// <summary>
        /// 拼团：拼团失败
        /// </summary>
        public string FightGroupFailed { set; get; }
        /// <summary>
        /// 拼团：拼团成功
        /// </summary>
        public string FightGroupSuccess { set; get; }
        /// <summary>
        /// 自提：自提订单支付成功
        /// </summary>
        public string SelfTakeOrderPay { set; get; }
        /// <summary>
        /// 自提：核销成功
        /// </summary>
        public string AlreadyVerification { set; get; }
        /// <summary>
        /// 虚拟订单：虚拟订单支付成功
        /// </summary>
        public string VirtualOrderPay { set; get; }
        /// <summary>
        /// 虚拟订单：核销成功
        /// </summary>
        public string VirtualAlreadyVerification { set; get; }
        /// <summary>
        /// 分销：申请成功销售员
        /// </summary>
        public string DistributorApply { get; set; }
        /// <summary>
        /// 分销：申请审核通过
        /// </summary>
        public string DistributorAuditSuccess { get; set; }
        /// <summary>
        /// 分销：申请审核拒绝
        /// </summary>
        public string DistributorAuditFail { get; set; }
        /// <summary>
        /// 分销：会员发展成功
        /// </summary>
        public string DistributorNewJoin { get; set; }
        /// <summary>
        /// 分销：有已结算佣金时
        /// </summary>
        public string DistributorCommissionSettled { get; set; }
        /// <summary>
        /// 申请提现
        /// </summary>
        public string MemberWithDrawApply { get; set; }
        /// <summary>
        /// 提现成功
        /// </summary>
        public string MemberWithDrawSuccess { get; set; }
        /// <summary>
        /// 提现失败
        /// </summary>
        public string MemberWithDrawFail { get; set; }
        /// <summary>
        /// 发货提醒
        /// </summary>
        public string ShopOrderShipping { get; set; }
        /// <summary>
        /// 售后处理提醒
        /// </summary>
        public string ShopDelivering { get; set; }

    }
}
