using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductAttributeInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long AttributeId { get; set; }
        public long ValueId { get; set; }
    }
}
