using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductRelationProductInfo
    {
        public int Id { get; set; }
        public long ProductId { get; set; }
        public string Relation { get; set; }
    }
}
