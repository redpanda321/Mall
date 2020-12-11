using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductLadderPriceInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public int MinBath { get; set; }
        public int MaxBath { get; set; }
        public decimal Price { get; set; }
    }
}
