using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderPayInfo
    {
        public long Id { get; set; }
        public long PayId { get; set; }
        public long OrderId { get; set; }
        public byte PayState { get; set; }
        public DateTime? PayTime { get; set; }
    }
}
