using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.DTO
{
    public class OrderCreateModel
    {
        public OrderCreateModel()
        {
            PlatformType = PlatformType.PC;
            this.ProductList = new List<ProductInfo>();
            this.SKUList = new List<SKUInfo>();
        }
        public PlatformType PlatformType { set; get; }

        public Mall.Entities.MemberInfo CurrentUser { set; get; }
        /// <summary>
        /// 收货地址坐标
        /// </summary>
        public float ReceiveLatitude { get; set; }
        /// <summary>
        /// 收货地址坐标
        /// </summary>
        public float ReceiveLongitude { get; set; }


        public long ReceiveAddressId { get; set; }

        public IEnumerable<string[]> CouponIdsStr { set; get; }

        /// <summary>
        /// 组合购的商品编号
        /// </summary>
        public IEnumerable<long> CollPids { set; get; }

        public int Integral { set; get; }
        /// <summary>
        /// 预付款支付金额
        /// </summary>
        public decimal Capital { get; set; }

        public InvoiceType Invoice { set; get; }

        public string InvoiceTitle { set; get; }
        public string InvoiceCode { set; get; }
        /// <summary>
        /// 买家留言
        /// </summary>
        public IEnumerable<string> OrderRemarks { get; set; }
        public string InvoiceContext
        {
            set;
            get;
        }
        public long[] CartItemIds { set; get; }
        public IEnumerable<string> SkuIds { set; get; }
        public IEnumerable<int> Counts { set; get; }

        /// <summary>
        /// 是否货到付款
        /// </summary>
        public bool IsCashOnDelivery { get; set; }

        public bool IslimitBuy { set; get; }

        /// <summary>
        /// 限购活动ID
        /// </summary>
        public long FlashSaleId { get; set; }

        public List<Entities.ProductInfo> ProductList { set; get; }
        public List<Entities.SKUInfo> SKUList { set; get; }

      
        public long ActiveId { get; set; }
        /// <summary>
        /// 拼团活动
        /// </summary>
        public long GroupId { get; set; }

        public CommonModel.OrderShop[] OrderShops { get; set; }

        public string formId { get; set; }
        /// <summary>
        /// 是否为门店订单
        /// </summary>
        public bool IsShopbranchOrder { get; set; }
        /// <summary>
        /// 是否为虚拟订单
        /// </summary>
        public bool IsVirtual { get; set; }
        /// <summary>
        /// 使用预付款金额
        /// </summary>
        //public decimal CapitalAmount { get; set; }
    }

    /// <summary>
    /// 订单的额外对象，其中有创建日期、收货地址、使用的优惠券
    /// </summary>
    public class OrderCreateAdditional
    {
        public DateTime CreateDate { set; get; }
        public Entities.ShippingAddressInfo Address { set; get; }
        public IEnumerable<CouponRecordInfo> Coupons { set; get; }

        public IEnumerable<BaseAdditionalCoupon> BaseCoupons { set; get; }
        //public decimal OrdersTotal { set; get; } 
        public decimal IntegralTotal { set; get; }
        /// <summary>
        /// 预付款金额
        /// </summary>
        public decimal CapitalTotal { get; set; }
    }

    public class BaseAdditionalCoupon
    {
        public object Coupon { get; set; }

        public int Type { get; set; }

        public long ShopId { get; set; }
    }
}
