using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class PlatAccountInfo
    {
        public long Id { get; set; }
        public decimal Balance { get; set; }
        public decimal PendingSettlement { get; set; }
        public decimal Settled { get; set; }
    }
}
