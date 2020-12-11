using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class BannerInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public long DisplaySequence { get; set; }
        public string Url { get; set; }
        public int Platform { get; set; }
        public int UrlType { get; set; }
        public int Status { get; set; }
        public int EnableDelete { get; set; }
    }
}
