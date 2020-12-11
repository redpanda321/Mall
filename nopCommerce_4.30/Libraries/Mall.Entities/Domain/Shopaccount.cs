using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopAccountInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal Balance { get; set; }
        public decimal PendingSettlement { get; set; }
        public decimal Settled { get; set; }
        public string ReMark { get; set; }
    }
}
