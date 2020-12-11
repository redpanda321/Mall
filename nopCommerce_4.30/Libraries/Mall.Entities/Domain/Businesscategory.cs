using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class BusinessCategoryInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long CategoryId { get; set; }
        public decimal CommisRate { get; set; }
    }
}
