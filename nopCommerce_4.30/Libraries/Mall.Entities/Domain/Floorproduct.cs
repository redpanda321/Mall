using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FloorProductInfo
    {
        public long Id { get; set; }
        public long FloorId { get; set; }
        public int Tab { get; set; }
        public long ProductId { get; set; }
    }
}
