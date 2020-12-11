using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Mall.CommonModel;

namespace Mall.DTO
{
    /// <summary>
    /// 订单提交页面ViewModel 
    /// </summary>
    public class OrderSubmitModel
    {
        /// <summary>
        /// 多少积分可以换一元钱 
        /// </summary>
        public int IntegralPerMoney { get; set; }
        /// <summary>
        /// 订单使用的积分
        /// </summary>
        public int Integral { get; set; }

        /// <summary>
        /// 订单使用的预付款金额
        /// </summary>
        public decimal Capital { get; set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public Mall.Entities.MemberInfo Member { get; set; }

        public string cartItemIds { get; set; }
        /// <summary>
        /// 完成订单将可以获得的积分
        /// </summary>
        public decimal TotalIntegral { get; set; }
        /// <summary>
        /// 消费多少钱可以获处一积分
        /// </summary>
        public int MoneyPerIntegral { get; set; }

        public List<InvoiceTitleInfo> InvoiceTitle { get; set; }

        public List<InvoiceContextInfo> InvoiceContext { get; set; }

        public List<ShopCartItemModel> products { get; set; }

        public List<OrderSubmitItemModel> freightProductGroup { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal totalAmount { get; set; }

        public decimal Freight { get; set; }

        public decimal orderAmount
        {
            get { return Freight + totalAmount; }
        }

        public Entities.ShippingAddressInfo address { get; set; }

        public string collIds { get; set; }
        public string skuIds { get; set; }

        public string counts { get; set; }

        public bool IsCashOnDelivery { get; set; }

        public bool IsLimitBuy { get; set; }
        public List<VirtualProductItemInfo> VirtualProductItemInfos { get; set; }
        public sbyte ProductType { get; set; }

        /// <summary>
        /// 默认发票抬头
        /// </summary>
        public string invoiceName { get; set; }

        /// <summary>
        /// 默认发票税号
        /// </summary>
        public string invoiceCode { get; set; }

        /// <summary>
        /// 收票人手机
        /// </summary>
        public string cellPhone { get; set; }

        /// <summary>
        /// 收票人邮箱
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// 增值税发票信息
        /// </summary>
        public InvoiceTitleInfo vatInvoice { get; set; }
    }

    public class OrderSubmitItemModel
    {
        public long id { get; set; }
        public long ProductId { get; set; }

        public long FreightTemplateId { get; set; }

        public decimal price { get; set; }

        public int count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string skuId { get; set; }

        public string skuColor { get; set; }

        public string skuSize { get; set; }

        public string skuVersion { get; set; }
        public string skuDetails { get; set; }

        public string name { get; set; }

        public string productCode { get; set; }

        public string imgUrl { get; set; }
        /// <summary>
        /// 七天退货标记
        /// </summary>
        public bool sevenDayNoReasonReturn { get; set; }
        /// <summary>
        /// 急速发货
        /// </summary>
        public bool timelyShip { get; set; }

        /// <summary>
        /// 消费者保障
        /// </summary>
        public bool customerSecurity { get; set; }

        public string colorAlias { get; set; }
        public string sizeAlias { get; set; }
        public string versionAlias { get; set; }
        public long collpid { get; set; }

        /// <summary>
        /// 是否开启阶梯价
        /// </summary>
        public bool isOpenLadder { get; set; }

    }

    public class ShopCartItemModel
    {
        public ShopCartItemModel()
        {
            CartItemModels = new List<CartItemModel>();
            UserCoupons = new List<CouponRecordInfo>();
        }
        public long shopId { set; get; }

        public string ShopName { set; get; }

        public decimal Freight { set; get; }
        public decimal FreeFreight { set; get; }
        /// <summary>
        /// 满减优惠
        /// </summary>
        public decimal FullDiscount { set; get; }

        /// <summary>
        /// 店铺合计（含运费，不含优惠券）
        /// </summary>
        public decimal ShopTotal { get { return CartItemModels.Sum(item => decimal.Round(decimal.Round(item.price, 2, MidpointRounding.AwayFromZero) * item.count, 2, MidpointRounding.AwayFromZero)) - FullDiscount + Freight; } }
        /// <summary>
        /// 店铺合计（不含运费，不含优惠券）
        /// </summary>
        public decimal ShopTotalWithoutFreight { get { return CartItemModels.Sum(item => decimal.Round(decimal.Round(item.price, 2, MidpointRounding.AwayFromZero) * item.count, 2, MidpointRounding.AwayFromZero)) - FullDiscount; } }

        public bool isFreeFreight { get; set; }
        public decimal shopFreeFreight { get; set; }

        public IBaseCoupon Coupon { get; set; }

        public List<CartItemModel> CartItemModels { set; get; }

        public List<CouponRecordInfo> UserCoupons { set; get; }

        public List<ShopBonusReceiveInfo> UserBonus { set; get; }

        public List<BaseCoupon> BaseCoupons { get; set; }

        public BaseCoupon OneCoupons { get; set; }

        public List<OrderSubmitItemModel> freightProductGroup { get; set; }

        public bool ExistShopBranch { get; set; }

        /// <summary>
        /// 是否启用发票
        /// </summary>
        public bool IsInvoice { get; set; }

        public List<InvoiceTypes> invoiceTpyes { get; set; }

        /// <summary>
        /// 发票寄出天数
        /// </summary>
        public string InvoiceDay { get; set; }
    }

    public class MobileShopCartItemModel
    {
        public MobileShopCartItemModel()
        {
            CartItemModels = new List<CartItemModel>();
            UserCoupons = new List<CouponRecordInfo>();
        }
        public long shopId { set; get; }

        public string ShopName { set; get; }

        public decimal Freight { set; get; }
        public decimal FreeFreight { set; get; }
        public List<CartItemModel> CartItemModels { set; get; }

        public List<CouponRecordInfo> UserCoupons { set; get; }

        /// <summary>
        /// 满额减优惠
        /// </summary>
        public decimal FullDiscount { set; get; }

        /// <summary>
        /// 店铺合计（含运费，不含优惠券）
        /// </summary>
        public decimal ShopTotal { get { return CartItemModels.Sum(item => decimal.Round(item.price, 2, MidpointRounding.AwayFromZero) * item.count) - FullDiscount + Freight; } }
        /// <summary>
        /// 店铺合计（不含运费，不含优惠券）
        /// </summary>
        public decimal ShopTotalWithoutFreight { get { return CartItemModels.Sum(item => decimal.Round(item.price, 2, MidpointRounding.AwayFromZero) * item.count) - FullDiscount; } }

        public List<ShopBonusReceiveInfo> UserBonus { set; get; }

        public List<BaseCoupon> BaseCoupons { get; set; }

        public BaseCoupon OneCoupons { get; set; }

        public List<OrderSubmitItemModel> freightProductGroup { get; set; }

        public bool IsFreeFreight { get; set; }
        public long VshopId { get; set; }
        /// <summary>
        /// 是否自营店
        /// </summary>
        public bool IsSelf { get; set; }

        public bool ExistShopBranch { get; set; }
        /// <summary>
        /// 门店ID
        /// </summary>
        public long ShopBranchId { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopBranchName { get; set; }

        /// <summary>
        /// 是否存在阶梯价商品
        /// </summary>
        public bool IsOpenLadder { get; set; }

        /// <summary>
        /// 是否启用发票
        /// </summary>
        public bool IsInvoice { get; set; }

        public List<InvoiceTypes> invoiceTpyes { get; set; }

        /// <summary>
        /// 发票寄出天数
        /// </summary>
        public string InvoiceDay { get; set; }
    }

    public class MobileOrderDetailConfirmModel
    {
        public List<MobileShopCartItemModel> products { get; set; }

        public decimal totalAmount { get; set; }

        public decimal Freight { get; set; }

        public dynamic orderAmount { get; set; }

        public decimal integralPerMoney { get; set; }
        public decimal integralPerMoneyRate { get; set; }
        public decimal userIntegralMaxDeductible { get; set; }
        /// <summary>
        /// 积分最高可扣抵比例
        /// </summary>
        public int IntegralDeductibleRate { get; set; }

        public decimal userIntegrals { get; set; }
        /// <summary>
        /// 预付款
        /// </summary>
        public decimal capitalAmount { get; set; }

        public Entities.MemberIntegralInfo memberIntegralInfo { get; set; }

        public List<InvoiceContextInfo> InvoiceContext { get; set; }

        public List<InvoiceTitleInfo> InvoiceTitle { get; set; }

        public bool IsCashOnDelivery { get; set; }

        public Entities.ShippingAddressInfo Address { get; set; }

        public string Sku { get; set; }

        public string Count { get; set; }
        public bool IsOpenStore { get; set; }
        public bool ProvideInvoice { get; set; }
        public Entities.ShopBranchInfo shopBranchInfo { get; set; }
        public sbyte ProductType { get; set; }
        public long ProductId { get; set; }

        /// <summary>
        /// 默认发票抬头
        /// </summary>
        public string invoiceName { get; set; }

        /// <summary>
        /// 默认发票税号
        /// </summary>
        public string invoiceCode { get; set; }
        /// <summary>
        /// 收票人手机
        /// </summary>
        public string cellPhone { get; set; }

        /// <summary>
        /// 收票人邮箱
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// 默认增值税发票信息
        /// </summary>
        public InvoiceTitleInfo vatInvoice { get; set; }
    }


    public class ShipAddressInfo
    {
        public long id { get; set; }
        public string fullRegionName { get; set; }
        public string address { get; set; }
        public string addressDetail { get; set; }
        public string phone { get; set; }
        public string shipTo { get; set; }
        public string fullRegionIdPath { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }

        public int regionId { get; set; }
        public bool NeedUpdate { get; set; }
    }
}
