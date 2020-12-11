using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AccountDetailInfo
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public DateTime Date { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime OrderFinshDate { get; set; }
        public int OrderType { get; set; }
        public long OrderId { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal ProductActualPaidAmount { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal IntegralDiscount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal RefundTotalAmount { get; set; }
        public decimal RefundCommisAmount { get; set; }
        public string OrderRefundsDates { get; set; }
        public decimal BrokerageAmount { get; set; }
        public decimal ReturnBrokerageAmount { get; set; }
        public decimal SettlementAmount { get; set; }
        public string PaymentTypeName { get; set; }
    }
}
