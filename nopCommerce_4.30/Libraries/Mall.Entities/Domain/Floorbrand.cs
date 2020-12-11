using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FloorBrandInfo
    {
        public long Id { get; set; }
        public long FloorId { get; set; }
        public long BrandId { get; set; }
    }
}
