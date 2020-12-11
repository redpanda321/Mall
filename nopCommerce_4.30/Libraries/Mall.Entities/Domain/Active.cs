using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ActiveInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ActiveName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public byte IsAllProduct { get; set; }
        public byte IsAllStore { get; set; }
        public int ActiveType { get; set; }
        public int ActiveStatus { get; set; }
    }
}
