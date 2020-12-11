using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class TypeBrandInfo
    {
        public long Id { get; set; }
        public long TypeId { get; set; }
        public long BrandId { get; set; }
    }
}
