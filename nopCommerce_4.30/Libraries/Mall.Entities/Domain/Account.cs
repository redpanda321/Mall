using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AccountInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public DateTime AccountDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public decimal ProductActualPaidAmount { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal RefundCommissionAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal AdvancePaymentAmount { get; set; }
        public decimal PeriodSettlement { get; set; }
        public string Remark { get; set; }
        public decimal Brokerage { get; set; }
        public decimal ReturnBrokerage { get; set; }
    }
}
