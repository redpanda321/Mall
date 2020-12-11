using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderItemInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ShopId { get; set; }
        public long ProductId { get; set; }
        public string SkuId { get; set; }
        public string Sku { get; set; }
        public long Quantity { get; set; }
        public long ReturnQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal RealTotalPrice { get; set; }
        public decimal RefundPrice { get; set; }
        public string ProductName { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Version { get; set; }
        public string ThumbnailsUrl { get; set; }
        public decimal CommisRate { get; set; }
        public decimal? EnabledRefundAmount { get; set; }
        public byte IsLimitBuy { get; set; }
        public decimal? EnabledRefundIntegral { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal FullDiscount { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public long FlashSaleId { get; set; }
    }
}
