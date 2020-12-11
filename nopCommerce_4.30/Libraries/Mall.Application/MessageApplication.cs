using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;

namespace Mall.Application
{
    public class MessageApplication
    {
        //private static IMessageService _iMessageService =  EngineContext.Current.Resolve<IMessageService>();

        private static IMessageService _iMessageService =  EngineContext.Current.Resolve<IMessageService>();


        //更新信息=用户表
        public static void UpdateMemberContacts(Entities.MemberContactInfo info)
        {
            _iMessageService.UpdateMemberContacts(info);
        }
        /// <summary>
        /// 获取发送目标
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pluginId">插件ID</param>
        /// <param name="type">用户类型</param>
        /// <returns></returns>
        public static string GetDestination(long userId, string pluginId, Entities.MemberContactInfo.UserTypes type)
        {
            return _iMessageService.GetDestination(userId, pluginId, type);
        }

        /// <summary>
        /// 根据插件类型和ID和目标获取信息
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="contact"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Entities.MemberContactInfo GetMemberContactsInfo(string pluginId, string contact, Entities.MemberContactInfo.UserTypes type)
        {
            return _iMessageService.GetMemberContactsInfo(pluginId, contact, type);
        }

        /// <summary>
        /// 根据用户ID获取目标信息
        /// </summary>
        /// <param name="UserId">用户ID</param>
        /// <returns></returns>
        public static List<Entities.MemberContactInfo> GetMemberContactsInfo(long UserId)
        {
            return _iMessageService.GetMemberContactsInfo(UserId);
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageCode(string destination, string pluginId, MessageUserInfo info)
        {
            _iMessageService.SendMessageCode(destination, pluginId, info);
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnFindPassWord(long userId, MessageUserInfo info)
        {
            _iMessageService.SendMessageOnFindPassWord(userId, info);
        }
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnOrderCreate(long userId, MessageOrderInfo info)
        {
            _iMessageService.SendMessageOnOrderCreate(userId, info);
        }

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnOrderPay(long userId, MessageOrderInfo info)
        {
            _iMessageService.SendMessageOnOrderPay(userId, info);
        }
        /// <summary>
        /// 店铺有新订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnShopHasNewOrder(long shopId, MessageOrderInfo info)
        {
            _iMessageService.SendMessageOnShopHasNewOrder(shopId, info);
        }
        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnOrderRefund(long userId, MessageOrderInfo info, long refundid = 0)
        {
            _iMessageService.SendMessageOnOrderRefund(userId, info, refundid);
        }

        public static void SendMessageOnRefundApply(long userId, MessageOrderInfo info, int refundMode, long refundid = 0) {
            _iMessageService.SendMessageOnRefundApply(userId, info, refundMode, refundid);
        }
        /// <summary>
        /// 售后发货信息提醒
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnRefundDeliver(long userId, MessageOrderInfo info, long refundid = 0)
        {
            _iMessageService.SendMessageOnRefundDeliver(userId, info, refundid);
        }

        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnOrderShipping(long userId, MessageOrderInfo info)
        {
            _iMessageService.SendMessageOnOrderShipping(userId, info);
        }
        /// <summary>
        /// 店铺审核
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public static void SendMessageOnShopAudited(long userId, MessageShopInfo info)
        {
            _iMessageService.SendMessageOnShopAudited(userId, info);
        }

        /// <summary>
        /// 发送优惠券成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public static void SendMessageOnCouponSuccess(long userId, MessageCouponInfo info)
        {
            _iMessageService.SendMessageOnCouponSuccess(userId, info);
        }
        /// <summary>
        /// 会员提现失败
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public static void SendMessageOnMemberWithDrawFail(long userId, MessageWithDrawInfo info)
        {
            _iMessageService.SendMessageOnMemberWithDrawFail(userId, info);
        }

        #region 分销提现
        /// <summary>
        /// 分销会员提现申请
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public static void SendMessageOnDistributionMemberWithDrawApply(long userId, MessageWithDrawInfo info)
        {
            _iMessageService.SendMessageOnDistributionMemberWithDrawApply(userId, info);
        }

        /// <summary>
        /// 分销会员提现成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public static void SendMessageOnDistributionMemberWithDrawSuccess(long userId, MessageWithDrawInfo info)
        {
            _iMessageService.SendMessageOnDistributionMemberWithDrawSuccess(userId, info);
        }
        #endregion
        #region 分销
        /// <summary>
        /// 分销：申请成为销售员
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        public static void SendMessageOnDistributorApply(long userId, string userName)
        {
            _iMessageService.SendMessageOnDistributorApply(userId, userName, SiteSettingApplication.SiteSettings.SiteName);
        }
        /// <summary>
        /// 分销：申请审核通过
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        public static void SendMessageOnDistributorAuditSuccess(long userId, string userName)
        {
            _iMessageService.SendMessageOnDistributorAuditSuccess(userId, userName, SiteSettingApplication.SiteSettings.SiteName);
        }
        /// <summary>
        /// 分销：申请审核拒绝
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="remark"></param>
        /// <param name="applyTime"></param>
        public static void SendMessageOnDistributorAuditFail(long userId, string userName, string remark, DateTime applyTime)
        {
            _iMessageService.SendMessageOnDistributorAuditFail(userId, userName, remark, applyTime, SiteSettingApplication.SiteSettings.SiteName);
        }
        /// <summary>
        /// 分销：会员发展成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="subUserName"></param>
        /// <param name="subUserRegTime"></param>
        public static void SendMessageOnDistributorNewJoin(long userId, string subUserName, DateTime subUserRegTime)
        {
            _iMessageService.SendMessageOnDistributorNewJoin(userId, subUserName, subUserRegTime, SiteSettingApplication.SiteSettings.SiteName);
        }
        /// <summary>
        /// 分销：有已结算佣金时
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <param name="settlementDate"></param>
        public static void SendMessageOnDistributorCommissionSettled(long userId, decimal amount, DateTime settlementDate)
        {
            _iMessageService.SendMessageOnDistributorCommissionSettled(userId, amount, settlementDate, SiteSettingApplication.SiteSettings.SiteName);
        }
        #endregion
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
        public static void AddSendMessageRecord(dynamic model)
        {
            _iMessageService.AddSendMessageRecord(model);
        }
        /// <summary>
        /// 查询群发消息记录
        /// </summary>
        /// <param name="querymodel"></param>
        /// <returns></returns>
        public static QueryPageModel<object> GetSendMessageRecords(object querymodel)
        {
            return _iMessageService.GetSendMessageRecords(querymodel);
        }

        /// <summary>
        /// 是否强制绑定手机号
        /// </summary>
        /// <returns></returns>
        public static bool IsOpenBindSms(long userId)
        {
            var setting = SiteSettingApplication.SiteSettings;
            var IsBind = true;
            if (setting.IsConBindCellPhone)
            {
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(userId, "Mall.Plugin.Message.SMS", Entities.MemberContactInfo.UserTypes.General));
            }

            return IsBind;
        }

        public static void SendMessageOnVirtualOrderVerificationSuccess(long userId, MessageVirtualOrderVerificationInfo info)
        {
            _iMessageService.SendMessageOnVirtualOrderVerificationSuccess(userId, info);
        }
    }
}
