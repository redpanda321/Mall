using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MarketSettingInfo
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public decimal Price { get; set; }
    }
}
