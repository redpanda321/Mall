using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ImageAdInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public byte IsTransverseAd { get; set; }
        public int TypeId { get; set; }
    }
}
