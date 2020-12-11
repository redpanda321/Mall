using System;
using System.Collections.Generic;

namespace Mall.Core.Plugins.Message
{
    public interface IMessagePlugin : IPlugin
    {
        /// <summary>
        /// 获取表单数据
        /// </summary>
        FormData GetFormData();

        /// <summary>
        /// 设置表单数据
        /// </summary>
        /// <param name="values">表单数据键值对集合，键为表单项的name,值为用户填写的值</param>
        void SetFormValues(IEnumerable<KeyValuePair<string, string>> values);


        /// <summary>
        /// Logo图片路径
        /// </summary>
        string Logo { get; }

        /// <summary>
        /// 简称
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// 是否开启记录
        /// </summary>
        bool EnableLog { get; }

        /// <summary>
        /// 发送验证码
        /// </summary>
        string SendMessageCode(string destination, MessageUserInfo info);

        /// <summary>
        /// 发送测试信息
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="content"></param>
        /// <param name="title">邮件标题，短信可不填写</param>
        string SendTestMessage(string destination, string content, string title = "");

        /// <summary>
        /// 批量发送消息
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="content"></param>
        /// <param name="title">邮件标题，短信可不填写</param>
        void SendMessages(string[] destination, string content, string title = "");

        /// <summary>
        /// 修改交易密码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnEditPayPassWord(string destination, MessageUserInfo info);

        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnEditLoginPassWord(string destination, MessageUserInfo info);

        /// <summary>
        /// 店铺审核通过
        /// </summary>
        /// <param name="sellerName"></param>
        /// <param name="shopName"></param>
        //string SendMessageOnShopAudited(string destination, MessageShopInfo info);

        /// <summary>
        /// 店铺开店成功
        /// </summary>
        /// <param name="sellerName"></param>
        /// <param name="shopName"></param>
        //string SendMessageOnShopSuccess(string destination, MessageShopInfo info);

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="code"></param>
        //string SendMessageOnFindPassWord(string destination, MessageUserInfo info);

        /// <summary>
        /// 订单创建
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnOrderCreate(string destination, MessageOrderInfo info);

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnOrderPay(string destination, MessageOrderInfo info);

        /// <summary>
        /// 获得佣金
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        //string SendMessageGetBrokerage(string destination, GetBrokerageInfo info);

        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnOrderShipping(string destination, MessageOrderInfo info);

        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnOrderRefundApply(string destination, MessageOrderInfo info);

        /// <summary>
        /// 订单退款成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnOrderRefund(string destination, MessageOrderInfo info);

        /// <summary>
        /// 订单退款失败
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnOrderRefundFail(string destination, MessageOrderInfo info);

        /// <summary>
        /// 申请退货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnOrderReturnApply(string destination, MessageOrderInfo info);

        /// <summary>
        /// 订单退货成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnOrderReturn(string destination, MessageOrderInfo info);

        /// <summary>
        /// 订单退货失败
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnOrderReturnFail(string destination, MessageOrderInfo info);

        /// <summary>
        /// 拼团：开团成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnFightGroupOpenSuccess(string destination, MessageOrderInfo info);

        /// <summary>
        /// 拼团：参团成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnFightGroupJoinSuccess(string destination, MessageOrderInfo info);

        /// <summary>
        /// 拼团：拼团失败
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnFightGroupFailed(string destination, MessageOrderInfo info);

        /// <summary>
        /// 拼团：拼团成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnFightGroupSuccess(string destination, MessageOrderInfo info);

        /// <summary>
        /// 自提：自提订单支付成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnSelfTakeOrderPay(string destination, MessageOrderInfo info);

        /// <summary>
        /// 自提：核销成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnAlreadyVerification(string destination, MessageOrderInfo info);

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnMemberWithDrawApply(string destination, MessageWithDrawInfo info);

        /// <summary>
        /// 提现成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnMemberWithDrawSuccess(string destination, MessageWithDrawInfo info);

        /// <summary>
        /// 提现失败
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnMemberWithDrawFail(string destination, MessageWithDrawInfo info);

        /// <summary>
        /// 虚拟商品购买成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageOnVirtualOrderPay(string destination, MessageVirtualOrderInfo info);

        /// <summary>
        /// 虚拟订单核销成功
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        string SendMessageOnVirtualOrderVerificationSuccess(string destination, MessageVirtualOrderVerificationInfo info);

        /// <summary>
        /// 售后发货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        //string SendMessageOnRefundDeliver(string destination, MessageOrderInfo info);


        /// <summary>
        /// 优惠券发送成功提醒
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        //string SendMessageOnCouponSuccess(string destination, MessageCouponInfo info);

        #region 分销
        /// <summary>
        /// 分销：申请成为销售员
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="userName"></param>
        /// <param name="siteName"></param>
        string SendMessageOnDistributorApply(string destination, string userName, string siteName);
        /// <summary>
        /// 分销：申请审核通过
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="userName"></param>
        /// <param name="siteName"></param>
        string SendMessageOnDistributorAuditSuccess(string destination, string userName, string siteName);
        /// <summary>
        /// 分销：申请审核拒绝
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="userName"></param>
        /// <param name="siteName"></param>
        string SendMessageOnDistributorAuditFail(string destination, string userName, string siteName);
        /// <summary>
        /// 分销：有已结算佣金时
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="userName"></param>
        /// <param name="siteName"></param>
        string SendMessageOnDistributorCommissionSettled(string destination, string userName,string siteName);
        #endregion

        /// <summary>
        /// 关闭某项信息的发送
        /// </summary>
        /// <param name="e"></param>
        void Disable(MessageTypeEnum e);
        /// <summary>
        /// 开启某项信息的发送
        /// </summary>
        /// <param name="e"></param>
        void Enable(MessageTypeEnum e);

        /// <summary>
        /// 获取某项信息的状态
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        StatusEnum GetStatus(MessageTypeEnum e);
        /// <summary>
        /// 获取所有项的状态
        /// </summary>
        /// <returns></returns>
        Dictionary<MessageTypeEnum, StatusEnum> GetAllStatus();

        ///批量设置所有项
        ///
        void SetAllStatus(Dictionary<MessageTypeEnum, StatusEnum> dic);

        /// <summary>
        /// 检查发送格式
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        bool CheckDestination(string destination);

        /// <summary>
        /// 是否已设置完成
        /// </summary>
        bool IsSettingsValid { get; }
    }
}
