using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopBranchIntagInfo
    {
        public long Id { get; set; }
        public long ShopBranchId { get; set; }
        public long ShopBranchTagId { get; set; }
    }
}
