using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CollocationPoruductInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long ColloId { get; set; }
        public byte IsMain { get; set; }
        public int DisplaySequence { get; set; }
    }
}
