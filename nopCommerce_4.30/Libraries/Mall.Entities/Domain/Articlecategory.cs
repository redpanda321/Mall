using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ArticleCategoryInfo
    {
        public long Id { get; set; }
        public long ParentCategoryId { get; set; }
        public string Name { get; set; }
        public long DisplaySequence { get; set; }
        public byte IsDefault { get; set; }
    }
}
