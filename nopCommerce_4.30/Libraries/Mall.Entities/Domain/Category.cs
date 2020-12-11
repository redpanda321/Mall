using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CategoryInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public long DisplaySequence { get; set; }
        public short SupportVirtualProduct { get; set; }
        public long ParentCategoryId { get; set; }
        public int Depth { get; set; }
        public string Path { get; set; }
        public string RewriteName { get; set; }
        public byte HasChildren { get; set; }
        public long TypeId { get; set; }
        public decimal CommisRate { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public short IsDeleted { get; set; }
        public byte IsShow { get; set; }
    }
}
