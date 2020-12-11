using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ArticleInfo
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string Title { get; set; }
        public string IconUrl { get; set; }
        public string Content { get; set; }
        public DateTime AddDate { get; set; }
        public long DisplaySequence { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public byte IsRelease { get; set; }
    }
}
