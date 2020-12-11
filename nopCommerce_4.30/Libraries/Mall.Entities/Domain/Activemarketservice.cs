using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ActiveMarketServiceInfo
    {
        public long Id { get; set; }
        public int TypeId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
    }
}
