using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.MessagePlugin
{
    public class MessageStatus
    {
        /// <summary>
        /// 会员修改交易密码成功
        /// </summary>
        public int EditPayPassWord { get; set; }

        /// <summary>
        /// 会员修改登录密码成功
        /// </summary>
        public int EditLoginPassWord { get; set; }

        /// <summary>
        /// 待付款提醒
        /// </summary>
        public int OrderCreated { set; get; }

        /// <summary>
        /// 订单支付成功通知
        /// </summary>
        public int OrderPay { set; get; }

        /// <summary>
        /// 订单发货提醒
        /// </summary>
        public int OrderShipping { set; get; }

        /// <summary>
        /// 申请退款
        /// </summary>
        public int OrderRefundApply { set; get; }

        /// <summary>
        /// 退款成功
        /// </summary>
        public int OrderRefundSuccess { set; get; }

        /// <summary>
        /// 商家/门店拒绝退款
        /// </summary>
        public int OrderRefundFail { set; get; }

        /// <summary>
        /// 申请退货
        /// </summary>
        public int OrderReturnApply { set; get; }

        /// <summary>
        /// 商家/门店同意退货
        /// </summary>
        public int OrderReturnSuccess { set; get; }

        /// <summary>
        /// 商家/门店拒绝退货
        /// </summary>
        public int OrderReturnFail { set; get; }

        /// <summary>
        /// 拼团：开团成功
        /// </summary>
        public int FightGroupOpenSuccess { set; get; }
        /// <summary>
        /// 拼团：参团成功
        /// </summary>
        public int FightGroupJoinSuccess { set; get; }
        /// <summary>
        /// 拼团：拼团失败
        /// </summary>
        public int FightGroupFailed { set; get; }
        /// <summary>
        /// 拼团：拼团成功
        /// </summary>
        public int FightGroupSuccess { set; get; }
        /// <summary>
        /// 自提：自提订单支付成功
        /// </summary>
        public int SelfTakeOrderPay { set; get; }
        /// <summary>
        /// 自提：核销成功
        /// </summary>
        public int AlreadyVerification { set; get; }
        /// <summary>
        /// 虚拟订单：虚拟订单支付成功
        /// </summary>
        public int VirtualOrderPay { set; get; }
        /// <summary>
        /// 虚拟订单：核销成功
        /// </summary>
        public int VirtualAlreadyVerification { set; get; }
        /// <summary>
        /// 分销：申请成功销售员
        /// </summary>
        public int DistributorApply { get; set; }
        /// <summary>
        /// 分销：申请审核通过
        /// </summary>
        public int DistributorAuditSuccess { get; set; }
        /// <summary>
        /// 分销：申请审核拒绝
        /// </summary>
        public int DistributorAuditFail { get; set; }
        /// <summary>
        /// 分销：会员发展成功
        /// </summary>
        public int DistributorNewJoin { get; set; }
        /// <summary>
        /// 分销：有已结算佣金时
        /// </summary>
        public int DistributorCommissionSettled { get; set; }
        /// <summary>
        /// 申请提现
        /// </summary>
        public int MemberWithDrawApply { get; set; }
        /// <summary>
        /// 提现成功
        /// </summary>
        public int MemberWithDrawSuccess { get; set; }
        /// <summary>
        /// 提现失败
        /// </summary>
        public int MemberWithDrawFail { get; set; }
        /// <summary>
        /// 发货提醒
        /// </summary>
        public int ShopOrderShipping { get; set; }
        /// <summary>
        /// 售后处理提醒
        /// </summary>
        public int ShopDelivering { get; set; }
    }
}
