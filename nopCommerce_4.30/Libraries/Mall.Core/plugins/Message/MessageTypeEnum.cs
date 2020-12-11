using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core.Plugins.Message
{
    public enum MessageTypeEnum
    {
        /// <summary>
        /// 订单创建时
        /// </summary>
        [Description("待付款提醒")]
        OrderCreated = 1,        
        
        /// <summary>
        /// 订单付款时
        /// </summary>
        [Description("订单支付成功通知")]
        OrderPay,
        /// <summary>
        /// 订单发货时
        /// </summary>
        [Description("订单发货提醒")]
        OrderShipping,
        /// <summary>
        /// 申请退款
        /// </summary>
        [Description("申请退款")]
        OrderRefundApply,

        /// <summary>
        /// 退款成功
        /// </summary>
        [Description("退款成功")]
        OrderRefundSuccess,

        /// <summary>
        /// 商家/门店拒绝退款
        /// </summary>
        [Description("商家/门店拒绝退款")]
        OrderRefundFail,

        /// <summary>
        /// 申请退货
        /// </summary>
        [Description("申请退货")]
        OrderReturnApply,

        /// <summary>
        /// 商家/门店同意退货
        /// </summary>
        [Description("商家/门店同意退货")]
        OrderReturnSuccess,

        /// <summary>
        /// 商家/门店拒绝退货
        /// </summary>
        [Description("商家/门店拒绝退货")]
        OrderReturnFail,

        #region 拼团
        /// <summary>
        /// 拼团：开团成功
        /// </summary>
        [Description("拼团：开团成功")]
        FightGroupOpenSuccess,
        /// <summary>
        /// 拼团：参团成功
        /// </summary>
        [Description("拼团：参团成功")]
        FightGroupJoinSuccess,

        //[Description("拼团：有新团员提示")]
        //FightGroupNewJoin,
        /// <summary>
        /// 拼团：拼团失败
        /// </summary>
        [Description("拼团：拼团失败")]
        FightGroupFailed,
        /// <summary>
        /// 拼团：拼团成功
        /// </summary>
        [Description("拼团：拼团成功")]
        FightGroupSuccess,
        #endregion

        #region 门店
        /// <summary>
        /// 自提：自提订单支付成功
        /// </summary>
        [Description("自提：自提订单支付成功")]
        SelfTakeOrderPay,

        /// <summary>
        /// 自提：核销成功
        /// </summary>
        [Description("自提：核销成功")]
        AlreadyVerification,
        #endregion

        #region 虚礼订单
        /// <summary>
        /// 虚拟订单：虚拟订单支付成功
        /// </summary>
        [Description("虚拟订单：虚拟订单支付成功")]
        VirtualOrderPay,

        /// <summary>
        /// 虚拟订单：核销成功
        /// </summary>
        [Description("虚拟订单：核销成功")]
        VirtualAlreadyVerification,
        #endregion

        #region 分销
        /// <summary>
        /// 分销：申请成功销售员
        /// </summary>
        [Description("分销：申请成功销售员")]
        DistributorApply,

        /// <summary>
        /// 分销：申请审核通过
        /// </summary>
        [Description("分销：申请审核通过")]
        DistributorAuditSuccess,

        /// <summary>
        /// 分销：申请审核拒绝
        /// </summary>
        [Description("分销：申请审核拒绝")]
        DistributorAuditFail,

        /// <summary>
        /// 分销：会员发展成功
        /// </summary>
        [Description("分销：会员发展成功")]
        DistributorNewJoin,

        /// <summary>
        /// 分销：有已结算佣金时
        /// </summary>
        [Description("分销：有已结算佣金时")]
        DistributorCommissionSettled,
        #endregion

        #region 会员
        /// <summary>
        /// 申请提现
        /// </summary>
        [Description("申请提现")]
        MemberWithDrawApply,

        /// <summary>
        /// 提现成功
        /// </summary>
        [Description("提现成功")]
        MemberWithDrawSuccess,

        /// <summary>
        /// 提现失败
        /// </summary>
        [Description("提现失败")]
        MemberWithDrawFail,
        #endregion

        #region 商家
        /// <summary>
        /// 发货提醒
        /// </summary>
        [Description("发货提醒")]
        ShopOrderShipping,

        /// <summary>
        /// 售后处理提醒
        /// </summary>
        [Description("售后处理提醒")]
        ShopDelivering,
        #endregion

        /// <summary>
        /// 会员修改交易密码成功
        /// </summary>
        [Description("会员修改交易密码成功")]
        EditPayPassWord,

        /// <summary>
        /// 会员修改登录密码成功
        /// </summary>
        [Description("会员修改登录密码成功")]
        EditLoginPassWord,

        #region TDO:ZYF 3.2去除
        ///// <summary>
        ///// 售后发货
        ///// </summary>
        //[Description("售后发货")]
        //RefundDeliver,

        //[Description("找回密码")]
        //FindPassWord,

        //[Description("店铺审核")]
        //ShopAudited,

        ///// <summary>
        ///// 发送优惠券
        ///// </summary>
        //[Description("发送优惠券通知")]
        //SendCouponSuccess,

        ////[Description("开店成功")] //2.4去除状态
        ////ShopSuccess,
        ///// <summary>
        ///// 店铺有新订单
        ///// </summary>
        //[Description("店铺有新订单")]
        //ShopHaveNewOrder,
        ///// <summary>
        ///// 领取红包通知
        ///// </summary>
        //[Description("领取红包通知")]
        //ReceiveBonus,
        ///// <summary>
        ///// 限时购开始通知
        ///// </summary>
        //[Description("限时购开始通知")]
        //LimitTimeBuy,
        ///// <summary>
        ///// 订阅限时购
        ///// </summary>
        //[Description("订阅限时购")]
        //SubscribeLimitTimeBuy,        

        ///// <summary>
        ///// 获得佣金
        ///// </summary>
        //[Description("获得佣金通知")]
        //GetBrokerage,
        #endregion
    }
}
