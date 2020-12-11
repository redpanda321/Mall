using Mall.CommonModel;
using Mall.CommonModel.Delegates;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mall.IServices
{
    /// <summary>
    /// 订单服务接口
    /// </summary>
    public interface IOrderService : IService
    {
        #region 属性
        /// <summary>
        /// 订单支付成功
        /// </summary>
        event OrderPaySuccessed OnOrderPaySuccessed;
        #endregion

        #region 方法
        SKUInfo GetSkuByID(string skuid);

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        List<OrderInfo> CreateOrder(OrderCreateModel model);

        void UpdateProductVistiOrderCount(long orderId);
        /// <summary>
        /// 确认0元订单（使用积分抵扣为0）
        /// </summary>
        /// <param name="orders"></param>
        void ConfirmZeroOrder(IEnumerable<long> Ids, long userId);

        /// <summary>
        /// 删除订单（使用积分抵扣会生成订单，生成后用户可能会点击取消使用积分抵扣）
        /// </summary>
        void CancelOrders(IEnumerable<long> Ids, long userId);

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        QueryPageModel<OrderInfo> GetOrders<Tout>(OrderQuery orderQuery, Expression<Func<OrderInfo, Tout>> sort = null);

        /// <summary>
        /// 分页获取订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<OrderInfo> GetOrders(OrderQuery query);

        /// <summary>
        /// 获取订单列表(忽略分页)
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        List<OrderInfo> GetAllOrders(OrderQuery orderQuery);

        /// <summary>
        /// 获取增量订单
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        QueryPageModel<OrderInfo> GetOrdersByLastModifyTime(OrderQuery orderQuery);

        /// <summary>
        /// 获取一批指定的订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<OrderInfo> GetOrders(IEnumerable<long> ids);

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        List<OrderItemInfo> GetOrderItemsByOrderId(long orderId);

        List<OrderOperationLogInfo> GetOrderLogs(long order);

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        List<OrderItemInfo> GetOrderItemsByOrderId(IEnumerable<long> orderIds);

        /// <summary>
        /// 获取订单的评论数
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        Dictionary<long, long> GetOrderCommentCount(IEnumerable<long> orderIds);

        /// <summary>
        /// 根据订单项id获取订单项
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        List<OrderItemInfo> GetOrderItemsByOrderItemId(IEnumerable<long> orderItemIds);

        /// <summary>
        /// 根据订单项id获取售后记录
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        List<OrderRefundInfo> GetOrderRefunds(IEnumerable<long> orderItemIds);

        decimal GetIntegralDiscountAmount(int integral, long userId);

        /// <summary>
        /// 获取某个用户的订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        OrderInfo GetOrder(long orderId, long userId);

        /// <summary>
        /// 获取某个用户的订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        OrderInfo GetOrder(long orderId);
        /// <summary>
        /// 根据提货码取订单
        /// </summary>
        /// <param name="pickCode"></param>
        /// <returns></returns>
        OrderInfo GetOrderByPickCode(string pickCode);

        /// <summary>
        /// 获取商品已购数（包括限时购、拼团）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        Dictionary<long, long> GetProductBuyCount(long userId, IEnumerable<long> productIds);
        /// <summary>
        /// 获取非限时购商品已购数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        Dictionary<long, long> GetProductBuyCountNotLimitBuy(long userId, IEnumerable<long> productIds);
        /// <summary>
        /// 是否存在订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopId">店铺Id,0表示不限店铺</param>
        /// <returns></returns>
        bool IsExistOrder(long orderId, long shopId = 0);

        /// <summary>
        /// 平台确认订单收款
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="payRemark"></param>
        /// <param name="managerName"></param>
        void PlatformConfirmOrderPay(long orderId, string payRemark, string managerName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderIds">订单Id</param>
        /// <param name="paymentId">支付方式Id</param>
        /// <param name="payNo">支付流水号</param>
        /// <param name="payTime">支付时间</param>
        void PaySucceed(IEnumerable<long> orderIds, string paymentId, DateTime payTime, string payNo = null
            , long payId = 0, OrderInfo.PaymentTypes paymentType = OrderInfo.PaymentTypes.Online
            , string payRemark = "");

        void PayCapital(IEnumerable<long> orderIds, string payNo = null, long payId = 0);

        bool PayByCapitalIsOk(long userid, IEnumerable<long> orderIds);
        /// <summary>
        /// 平台取消订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="managerName"></param>
        void PlatformCloseOrder(long orderId, string managerName, string CloseReason = "");

        /// <summary>
        /// 商家取消订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="sellerName"></param>
        void SellerCloseOrder(long orderId, string sellerName);

        /// <summary>
        /// 商家修改订单收货地址
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="sellerName"></param>
        /// <param name="shipTo"></param>
        /// <param name="cellPhone"></param>
        /// <param name="topRegionId"></param>
        /// <param name="regionId"></param>
        /// <param name="regionFullName"></param>
        /// <param name="address"></param>
        void SellerUpdateAddress(long orderId, string sellerName, string shipTo, string cellPhone, int topRegionId, int regionId, string regionFullName, string address);

        /// <summary>
        /// 商家修改订单商品的优惠金额
        /// </summary>
        /// <param name="orderItemId"></param>
        /// <param name="discountAmount"></param>
        /// <param name="sellerName"></param>
        void SellerUpdateItemDiscountAmount(long orderItemId, decimal discountAmount, string sellerName);


        /// <summary>
        /// 商家修改订单的运费
        /// </summary>
        /// <param name="roderId"></param>
        /// <param name="Freight"></param>
        void SellerUpdateOrderFreight(long orderId, decimal freight);

        /// <summary>
        /// 商家发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        OrderInfo SellerSendGood(long orderId, string sellerName, string companyName, string shipOrderNumber);

        /// <summary>
        /// 门店发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="deliveryType"></param>
        /// <param name="shopkeeperName"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        OrderInfo ShopSendGood(long orderId, int deliveryType, string shopkeeperName, string companyName, string shipOrderNumber);

        bool IsOrderAfterService(long orderId);
        /// <summary>
        /// 修改快递信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        OrderInfo UpdateExpress(long orderId, string companyName, string shipOrderNumber);

        /// <summary>
        /// 会员取消订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="memberName"></param>
        void MemberCloseOrder(long orderId, string memberName);

        /// <summary>
        /// 会员确认订单收货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="memberName"></param>
        void MembeConfirmOrder(long orderId, string memberName);
        /// <summary>
        /// 门店核销订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="managerName"></param>
        void ShopBranchConfirmOrder(long orderId, long shopBranchId, string managerName);
        /// <summary>
        /// 设置订单物流信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="expressName"></param>
        /// <param name="startCode"></param>
        /// <param name="orderIds"></param>
        void SetOrderExpressInfo(long shopId, string expressName, string startCode, IEnumerable<long> orderIds);
        /// <summary>
        /// 设置订单商家备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="mark"></param>
        void SetOrderSellerRemark(long orderId, string mark);

        /// <summary>
        /// 获取用户最新的Top N 订单
        /// </summary>
        /// <param name="top"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        List<OrderInfo> GetTopOrders(int top, long userId);
        List<OrderComplaintInfo> GetOrderComplaintByOrders(List<long> orders);

        int GetFightGroupOrderCountByUser(long userId);
        int GetOrderTotalProductCount(long order);

        /// <summary>
        /// 更新订单数
        /// </summary>
        /// <param name="userId">会员Id</param>
        /// <param name="addOrderCount">变更订单数(正数表示增加，负数表示减少）</param>
        /// <param name="addOrderAmount">变量订单金额(正数表示增加，负数表示减少）</param>
        void UpdateMemberOrderInfo(long userId, decimal addOrderAmount = 0, int addOrderCount = 1);

        /// <summary>
        /// 获取指定商品最近一个月的平均成交价格
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        decimal GetRecentMonthAveragePrice(long shopId, long productId);

        /// <summary>
        /// 计算订单条目可退款金额
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="isCompel">是否强制计算</param>
        void CalculateOrderItemRefund(long orderId, bool isCompel = false);
        /// <summary>
        /// 商家同意退款，关闭订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="managerName"></param>
        void AgreeToRefundBySeller(long orderId);
        /// <summary>
        /// 过期自动确认订单
        /// </summary>
        //void AutoConfirmOrder();

        /// <summary>
        /// 过期自动关闭订单
        /// </summary>
        //void AutoCloseOrder();
        /// <summary>
        /// 获取销量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        long GetSaleCount(DateTime? startDate = null, DateTime? endDate = null, long? shopBranchId = null, long? shopId = null, long? productId = null, bool hasReturnCount = false, bool hasWaitPay = false);
        /// <summary>
        /// 获取发票列表
        /// </summary>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        QueryPageModel<InvoiceContextInfo> GetInvoiceContexts(int PageNo, int PageSize = 20);

        /// <summary>
        /// 获取发票列表
        /// </summary>
        /// <returns></returns>
        List<InvoiceContextInfo> GetInvoiceContexts();

        void SaveInvoiceContext(InvoiceContextInfo info);

        void DeleteInvoiceContext(long id);

        List<InvoiceTitleInfo> GetInvoiceTitles(long userid, InvoiceType type);

        long SaveInvoiceTitle(InvoiceTitleInfo info);
        long EditInvoiceTitle(InvoiceTitleInfo info);

        /// <summary>
        /// 保存发票信息
        /// </summary>
        /// <param name="info"></param>
        void SaveInvoiceTitleNew(InvoiceTitleInfo info);

        void DeleteInvoiceTitle(long id, long userId = 0);

        /// <summary>
        /// 根据支付订单号的取订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<OrderPayInfo> GetOrderPay(long id);

        /// <summary>
        /// 保存支付订单信息，生成支付订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        long SaveOrderPayInfo(IEnumerable<OrderPayInfo> model, PlatformType platform);

        /// <summary>
        /// 根据订单id获取OrderPayInfo
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        List<OrderPayInfo> GetOrderPays(IEnumerable<long> orderIds);

        //TODO LRL 2015/08/06 添加获取子订单对象的方法
        /// <summary>
        /// 获取子订单对象
        /// </summary>
        /// <param name="orderItemId"></param>
        /// <returns></returns>
        OrderItemInfo GetOrderItem(long orderItemId);

        void MemberApplyCloseOrder(long orderId, string memberName, bool isBackStock = false);
        /// <summary>
        /// 是否超过售后期
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool IsRefundTimeOut(long orderId);

        /// <summary>
        /// 是否超过售后期
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool IsRefundTimeOut(OrderInfo order);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop">门店ID,为0表示不筛选</param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        OrderDayStatistics GetOrderDayStatistics(long shop, DateTime begin, DateTime end);
        /// <summary>
        /// 订单完成订单数据写入待结算表
        /// </summary>
        /// <param name="o"></param>
        void WritePendingSettlnment(OrderInfo o);

        /// <summary>
        /// 商家给订单备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="remark"></param>
        void UpdateSellerRemark(long orderId, string remark, int flag);
        /// <summary>
        /// 分配商家订单到门店，更新商家、门店库存
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="quantity"></param>
        void AllotStoreUpdateStock(List<string> skuIds, List<int> counts, long shopBranchId);
        /// <summary>
        /// 分配门店订单到新门店
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="newShopBranchId"></param>
        /// <param name="oldShopBranchId"></param>
        void AllotStoreUpdateStockToNewShopBranch(List<string> skuIds, List<int> counts, long newShopBranchId, long oldShopBranchId);
        /// <summary>
        /// 分配门店订单回到商家
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        void AllotStoreUpdateStockToShop(List<string> skuIds, List<int> counts, long shopBranchId);
        /// <summary>
        /// 更新订单所属门店
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        void UpdateOrderShopBranch(long orderId, long shopBranchId);
        /// <summary>
        /// 获取待评价订单数量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetWaitingForComments(OrderQuery query);

        /// <summary>
        /// 获取所有订单使用的优惠券列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="couponIdsStr"></param>
        /// <returns></returns>
        IEnumerable<BaseAdditionalCoupon> GetOrdersCoupons(long userId, IEnumerable<string[]> couponIdsStr);

        List<long> GetOrderIdsByLatestTime(int time, long shopBranchId, long shopId);

        OrderStatisticsItem GetOrderCountStatistics(OrderCountStatisticsQuery query);

        List<VirtualOrderItemInfo> GetVirtualOrderItemInfosByOrderId(long orderId);
        List<OrderVerificationCodeInfo> GetOrderVerificationCodeInfosByOrderIds(List<long> orderId);
        OrderVerificationCodeInfo GetOrderVerificationCodeInfoByCode(string verificationCode);
        List<OrderVerificationCodeInfo> GetOrderVerificationCodeInfoByCodes(List<string> verificationCodes);
        bool UpdateOrderVerificationCodeStatusByCodes(List<string> verficationCodes, long orderId, OrderInfo.VerificationCodeStatus status, DateTime? verificationTime, string verificationUser = "");
        bool AddVerificationRecord(VerificationRecordInfo info);
        VerificationRecordInfo GetVerificationRecordInfoById(long id);
        bool AddVirtualOrderItemInfo(List<VirtualOrderItemInfo> infos);
        bool AddOrderVerificationCodeInfo(List<OrderVerificationCodeInfo> infos);
        int GetWaitConsumptionOrderNumByUserId(long userId = 0, long shopId = 0, long shopBranchId = 0);
        QueryPageModel<VerificationRecordInfo> GetVerificationRecords(VerificationRecordQuery query);
        QueryPageModel<OrderVerificationCodeInfo> GetOrderVerificationCodeInfos(VerificationRecordQuery query);

        /// <summary>
        /// 虚拟订单信息项实体
        /// </summary>
        /// <param name="orderIds">订单号集合</param>
        /// <returns></returns>
        List<VirtualOrderItemInfo> GeVirtualOrderItemsByOrderId(IEnumerable<long> orderIds);
        List<SearchShopAndShopbranchModel> GetShopOrShopBranch(string keyword, sbyte? type);

        /// <summary>
        /// 待结算实体
        /// </summary>
        /// <param name="orderIds">订单号集合</param>
        /// <returns></returns>
        List<PendingSettlementOrderInfo> GetPendingSettlementOrdersByOrderId(IEnumerable<long> orderIds);
        
        /// <summary>
        /// 已结算订单集合
        /// </summary>
        /// <param name="orderIds">订单号集合</param>
        /// <returns></returns>
        List<AccountDetailInfo> GetAccountDetailByOrderId(IEnumerable<long> orderIds);

        /// <summary>
        /// 订单发票实体集合
        /// </summary>
        /// <param name="orderIds">订单号集合</param>
        /// <returns></returns>
        List<OrderInvoiceInfo> GetOrderInvoicesByOrderId(IEnumerable<long> orderIds);

        /// <summary>
        /// 订单发票实体
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        OrderInvoiceInfo GetOrderInvoiceInfo(long orderId);
        #endregion
    }
}
