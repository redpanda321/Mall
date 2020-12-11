using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FullDiscountRuleInfo
    {
        public long Id { get; set; }
        public long ActiveId { get; set; }
        public decimal Quota { get; set; }
        public decimal Discount { get; set; }
    }
}
