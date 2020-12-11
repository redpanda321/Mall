using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class PendingSettlementOrderInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long OrderId { get; set; }
        public int? OrderType { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal ProductsAmount { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal IntegralDiscount { get; set; }
        public decimal PlatCommission { get; set; }
        public decimal DistributorCommission { get; set; }
        public decimal RefundAmount { get; set; }
        public DateTime? RefundDate { get; set; }
        public decimal PlatCommissionReturn { get; set; }
        public decimal DistributorCommissionReturn { get; set; }
        public decimal SettlementAmount { get; set; }
        public DateTime? OrderFinshTime { get; set; }
        public string PaymentTypeName { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
