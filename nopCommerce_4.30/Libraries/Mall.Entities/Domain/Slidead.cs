using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SlideAdInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public long DisplaySequence { get; set; }
        public int TypeId { get; set; }
        public string Description { get; set; }
    }
}
