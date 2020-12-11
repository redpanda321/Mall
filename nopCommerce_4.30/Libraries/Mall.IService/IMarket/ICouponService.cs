using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.IServices
{
    public interface ICouponService : IService
    {

        /// <summary>
        /// 商家添加一个优惠券
        /// </summary>
        /// <param name="info"></param>
        void AddCoupon(CouponInfo info);

        //使优惠券失效
        void CancelCoupon(long couponId, long shopId);
        /// <summary>
        /// 商家修改一个优惠券
        /// </summary>
        /// <param name="info"></param>
        void EditCoupon(CouponInfo info);

        /// <summary>
        /// 领取一个优惠券
        /// </summary>
        /// <param name="info"></param>
        CouponRecordInfo AddCouponRecord(Entities.CouponRecordInfo info);
        void AddSendmessagerecordCouponSN(List<Entities.SendmessagerecordCouponSNInfo> items);
        //使用优惠券
        void UseCoupon(long userId, IEnumerable<long> Ids, IEnumerable<OrderInfo> orders);

        /// <summary>
        /// 获取店铺订购的优惠券信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ActiveMarketServiceInfo GetCouponService(long shopId);
        List<CouponSettingInfo> GetSettingsByCoupon(List<long> coupons);

        /// <summary>
        /// 获取商家添加的优惠券列表
        /// </summary>
        /// <returns></returns>
        QueryPageModel<CouponInfo> GetCouponList(CouponQuery query);

        /// <summary>
        /// 获取商家添加的优惠券列表(全部)
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        List<CouponInfo> GetCouponList(long shopid);

        List<CouponInfo> GetCoupons(List<long> coupons);
        /// <summary>
        /// 获取商家添加的优惠券列表(过滤过期失效的)
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        List<CouponInfo> GetCouponLists(long shopid);
        /// <summary>
        /// 获取领取的优惠券列表
        /// </summary>
        /// <returns></returns>
        QueryPageModel<CouponRecordInfo> GetCouponRecordList(CouponRecordQuery query);
        /// <summary>
        /// 获取已邻取优惠券信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CouponRecordInfo GetCouponRecordById(long id);

        /// <summary>
        /// 获取优惠券信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        CouponInfo GetCouponInfo(long shopId, long couponId);
        /// <summary>
        /// 获取优惠券信息（couponid）
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        CouponInfo GetCouponInfo(long couponId);

        /// <summary>
        /// 批量获取优惠券信息（couponIds）
        /// </summary>
        /// <param name="couponIds">优惠券数组</param>
        /// <returns></returns>
        List<CouponInfo> GetCouponInfo(long[] couponIds);
        /// <summary>
        /// 获取已使用的的某一个优惠券的详细
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        CouponRecordInfo GetCouponRecordInfo(long userId, long orderId);

        /// <summary>
        /// 获取可用优惠券
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        List<CouponInfo> GetTopCoupon(long shopId, int top = 5, PlatformType type = Core.PlatformType.PC);

        /// <summary>
        /// 获取用户领取的某个优惠券的数量
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        int GetUserReceiveCoupon(long couponId, long userId);
        /// <summary>
        /// 获取用户可用优惠券数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        int GetAvailableCouponCountByUser(long userId);

        /// <summary>
        /// 获取一个用户在某个店铺的可用优惠券
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="userId"></param>
        /// <param name="totalPrice">总金额</param>
        /// <returns></returns>
        List<CouponRecordInfo> GetUserCoupon(long shopId, long userId, decimal totalPrice);

        ///获取用户将要使用的优惠券列表
        ///
        List<CouponRecordInfo> GetOrderCoupons(long userId, IEnumerable<long> Ids);
        /// <summary>
        /// 取用户领取的所有优惠卷信息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        List<UserCouponInfo> GetUserCouponList(long userid);
        List<CouponSendByRegisterDetailedInfo> GetCouponSendByRegisterDetailedInfo(long registerId);

        List<UserCouponInfo> GetAllUserCoupon(long userid);
        Dictionary<long, int> GetRecordCountByCoupon(List<long> coupons);
        List<CouponRecordInfo> GetRecordByCoupon(long coupon);
        int GetRecordCountByCoupon(long coupon);
        /// <summary>
        /// 是否可以添加积分兑换红包
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        bool CanAddIntegralCoupon(long shopid, long id = 0);
        /// <summary>
        /// 取积分优惠券列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        QueryPageModel<CouponInfo> GetIntegralCoupons(int page, int pageSize);
        /// <summary>
        /// 同步微信卡券审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cardid"></param>
        /// <param name="auditstatus">审核状态</param>
        void SyncWeixinCardAudit(long id, string cardid, WXCardLogInfo.AuditStatusEnum auditstatus);
        /// <summary>
        /// 处理错误的卡券同步信息
        /// </summary>
        void ClearErrorWeiXinCardSync();

        QueryPageModel<CouponInfo> GetCouponByName(string text, DateTime EndDate, int ReceiveType, int page, int pageSize);

        /// <summary>
        /// 获取指定优惠券会员领取情况统计
        /// </summary>
        /// <param name="couponIds">优惠券ID数组</param>
        /// <returns></returns>
        List<CouponRecordInfo> GetCouponRecordTotal(long[] couponIds);
        /// <summary>
        /// 获取能够使用的优惠券数量[排除过期、已领完]
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetUserCouponCount(long shopId);

        List<CouponProductInfo> GetCouponProductsByCouponId(long couponId);

        /// <summary>
        /// 批量修改优惠券商铺名称
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopName"></param>
        void SetCouponShopName(long shopId, string shopName);

        /// <summary>
        /// 根据发送消息Id获取优惠券列表
        /// </summary>
        /// <param name="messageId">发送消息记录Id</param>
        /// <returns></returns>
        List<CouponInfo> GetCouponBySendmessagerecordId(long messageId);
    }
}
