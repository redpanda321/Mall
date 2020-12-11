using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class BrandInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long DisplaySequence { get; set; }
        public string Logo { get; set; }
        public string RewriteName { get; set; }
        public string Description { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public byte IsRecommend { get; set; }
        public short IsDeleted { get; set; }
    }
}
