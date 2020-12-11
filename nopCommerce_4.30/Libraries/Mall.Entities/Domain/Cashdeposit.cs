using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CashDepositInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalBalance { get; set; }
        public DateTime Date { get; set; }
        public byte EnableLabels { get; set; }
    }
}
