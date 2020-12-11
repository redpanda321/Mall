using Mall.CommonModel;
using Mall.Core.Plugins.Message;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IMessageService : IService
    {
        //更新信息=用户表
        void UpdateMemberContacts(MemberContactInfo info);
        /// <summary>
        /// 获取发送目标
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pluginId">插件ID</param>
        /// <param name="type">用户类型</param>
        /// <returns></returns>
        string GetDestination(long userId, string pluginId, MemberContactInfo.UserTypes type);

        /// <summary>
        /// 根据插件类型和ID和目标获取信息
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="contact"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        MemberContactInfo GetMemberContactsInfo(string pluginId, string contact, MemberContactInfo.UserTypes type);

        /// <summary>
        /// 根据用户ID获取目标信息
        /// </summary>
        /// <param name="UserId">用户ID</param>
        /// <returns></returns>
        List<MemberContactInfo> GetMemberContactsInfo(long UserId);

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageCode(string destination, string pluginId, MessageUserInfo info);

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnFindPassWord(long userId, MessageUserInfo info);
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnOrderCreate(long userId, MessageOrderInfo info);

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnOrderPay(long userId, MessageOrderInfo info);

        #region 分销
        /// <summary>
        /// 分销：申请成为销售员
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        void SendMessageOnDistributorApply(long userId, string userName, string siteName);
        /// <summary>
        /// 分销：申请审核通过
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        void SendMessageOnDistributorAuditSuccess(long userId, string userName, string siteName);
        /// <summary>
        /// 分销：申请审核拒绝
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="remark"></param>
        /// <param name="applyTime"></param>
        void SendMessageOnDistributorAuditFail(long userId, string userName, string remark, DateTime applyTime, string siteName);
        /// <summary>
        /// 分销：会员发展成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="subUserName"></param>
        /// <param name="subUserRegTime"></param>
        void SendMessageOnDistributorNewJoin(long userId, string subUserName, DateTime subUserRegTime, string siteName);
        /// <summary>
        /// 分销：有已结算佣金时
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <param name="settlementDate"></param>
        void SendMessageOnDistributorCommissionSettled(long userId, decimal amount, DateTime settlementDate, string siteName);
        #endregion
        /// <summary>
        /// 店铺有新订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnShopHasNewOrder(long shopId, MessageOrderInfo info);

        /// <summary>
        /// 订单退货退款申请通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        void SendMessageOnRefundApply(long userId, MessageOrderInfo info, int refundMode, long refundid = 0);
        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnOrderRefund(long userId, MessageOrderInfo info, long refundid = 0, string refundTypeName = "");
        /// <summary>
        /// 退货成功通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnRefundDeliver(long userId, MessageOrderInfo info, long refundid = 0);

        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnOrderShipping(long userId, MessageOrderInfo info);
        /// <summary>
        /// 店铺审核
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnShopAudited(long userId, MessageShopInfo info);

        /// <summary>
        /// 发送优惠券成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnCouponSuccess(long userId, MessageCouponInfo info);

        ///// <summary>
        ///// 店铺成功2.4去除
        ///// </summary>
        ///// <param name="destination"></param>
        ///// <param name="info"></param>
        //void SendMessageOnShopSuccess(long userId, MessageShopInfo info);
        /// <summary>
        /// 添加群发消息记录
        /// </summary>
        /// <param name="model"></param>
        void AddSendMessageRecord(dynamic model);
        /// <summary>
        /// 查询群发消息记录
        /// </summary>
        /// <param name="querymodel"></param>
        /// <returns></returns>
        QueryPageModel<object> GetSendMessageRecords(object querymodel);

        /// <summary>
        /// 退款/退货失败通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        void SendMessageOnOrderRefundFail(long userId, MessageOrderInfo info, int refundMode, long refundid = 0);

        /// <summary>
        /// 自提订单支付成功通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnSelfTakeOrderPay(long userId, MessageOrderInfo info);

        /// <summary>
        /// 自提订单核销成功通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnAlreadyVerification(long userId, MessageOrderInfo info);

        /// <summary>
        /// 会员提现申请
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnMemberWithDrawApply(long userId, MessageWithDrawInfo info);

        /// <summary>
        /// 会员提现成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnMemberWithDrawSuccess(long userId, MessageWithDrawInfo info);

        /// <summary>
        /// 会员提现失败
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnMemberWithDrawFail(long userId, MessageWithDrawInfo info);

        #region 分销提现
        /// <summary>
        /// 分销会员提现申请
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnDistributionMemberWithDrawApply(long userId, MessageWithDrawInfo info);

        /// <summary>
        /// 分销会员提现成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnDistributionMemberWithDrawSuccess(long userId, MessageWithDrawInfo info);
        #endregion

        /// <summary>
        /// 商家售后处理提醒
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnShopDelivering(MessageOrderInfo info);

        /// <summary>
        /// 商家发货提醒
        /// </summary>
        /// <param name="info"></param>
        void SendMessageOnShopOrderShipping(MessageOrderInfo info);

        /// <summary>
        /// 修改支付密码提醒
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnEditPayPassWord(long userId, MessageUserInfo info);

        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnEditLoginPassWord(long userId, MessageUserInfo info);

        /// <summary>
        /// 虚拟商品购买成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnVirtualOrderPay(long userId, MessageVirtualOrderInfo info);

        /// <summary>
        /// 虚拟订单核销成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnVirtualOrderVerificationSuccess(long userId, MessageVirtualOrderVerificationInfo info);
    }
}
