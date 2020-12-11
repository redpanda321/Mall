using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopCategoryInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long ParentCategoryId { get; set; }
        public string Name { get; set; }
        public long DisplaySequence { get; set; }
        public byte IsShow { get; set; }
    }
}
