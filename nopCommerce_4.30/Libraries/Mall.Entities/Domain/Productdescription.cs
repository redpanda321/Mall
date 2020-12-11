using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductDescriptionInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string AuditReason { get; set; }
        public string Description { get; set; }
        public long DescriptionPrefixId { get; set; }
        public long DescriptiondSuffixId { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MobileDescription { get; set; }
    }
}
