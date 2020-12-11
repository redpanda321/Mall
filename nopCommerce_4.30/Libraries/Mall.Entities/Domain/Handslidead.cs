using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class HandSlideAdInfo
    {
        public long Id { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public long DisplaySequence { get; set; }
    }
}
