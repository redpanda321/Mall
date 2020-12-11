using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SellerSpecificationValueInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long ValueId { get; set; }
        public int Specification { get; set; }
        public long TypeId { get; set; }
        public string Value { get; set; }
    }
}
