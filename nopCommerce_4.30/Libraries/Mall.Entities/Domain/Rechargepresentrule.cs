using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class RechargePresentRuleInfo
    {
        public long Id { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal PresentAmount { get; set; }
    }
}
