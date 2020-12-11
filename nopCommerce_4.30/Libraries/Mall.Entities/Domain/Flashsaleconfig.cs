using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FlashSaleConfigInfo
    {
        public long Id { get; set; }
        public int Preheat { get; set; }
        public byte IsNormalPurchase { get; set; }
    }
}
