using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductDescriptionTemplateInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public string Content { get; set; }
        public string MobileContent { get; set; }
    }
}
