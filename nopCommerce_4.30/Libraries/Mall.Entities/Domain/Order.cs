using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderInfo
    {
        public long Id { get; set; }
        public int OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public string CloseReason { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string SellerPhone { get; set; }
        public string SellerAddress { get; set; }
        public string SellerRemark { get; set; }
        public int? SellerRemarkFlag { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserRemark { get; set; }
        public string ShipTo { get; set; }
        public string CellPhone { get; set; }
        public int TopRegionId { get; set; }
        public int RegionId { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public float ReceiveLongitude { get; set; }
        public float ReceiveLatitude { get; set; }
        public string ExpressCompanyName { get; set; }
        public decimal Freight { get; set; }
        public string ShipOrderNumber { get; set; }
        public DateTime? ShippingDate { get; set; }
        public byte IsPrinted { get; set; }
        public string PaymentTypeName { get; set; }
        public string PaymentTypeGateway { get; set; }
        public int PaymentType { get; set; }
        public string GatewayOrderId { get; set; }
        public string PayRemark { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal Tax { get; set; }
        public DateTime? FinishDate { get; set; }
        public decimal ProductTotalAmount { get; set; }
        public decimal RefundTotalAmount { get; set; }
        public decimal CommisTotalAmount { get; set; }
        public decimal RefundCommisAmount { get; set; }
        public int ActiveType { get; set; }
        public int Platform { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal IntegralDiscount { get; set; }
        public int OrderType { get; set; }
        public string OrderRemarks { get; set; }
        public DateTime LastModifyTime { get; set; }
        public int DeliveryType { get; set; }
        public long ShopBranchId { get; set; }
        public string PickupCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ActualPayAmount { get; set; }
        public decimal FullDiscount { get; set; }
        public decimal CapitalAmount { get; set; }
        public long CouponId { get; set; }
        public string CancelReason { get; set; }
        public int DadaStatus { get; set; }
    }
}
