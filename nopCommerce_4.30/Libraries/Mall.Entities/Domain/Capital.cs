using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CapitalInfo
    {
        public long Id { get; set; }
        public long MemId { get; set; }
        public decimal Balance { get; set; }
        public decimal FreezeAmount { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal PresentAmount { get; set; }
    }
}
