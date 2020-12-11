using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FloorCategoryInfo
    {
        public long Id { get; set; }
        public long FloorId { get; set; }
        public long CategoryId { get; set; }
        public int Depth { get; set; }
    }
}
