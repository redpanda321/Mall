using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class VirtualProductItemInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Name { get; set; }
        public byte Type { get; set; }
        public short Required { get; set; }
    }
}
