using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopBrandInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long BrandId { get; set; }
    }
}
