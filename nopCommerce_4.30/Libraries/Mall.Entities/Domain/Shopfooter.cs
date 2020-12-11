using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopFooterInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string Footer { get; set; }
    }
}
