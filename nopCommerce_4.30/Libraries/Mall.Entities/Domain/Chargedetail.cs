using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ChargeDetailInfo
    {
        public long Id { get; set; }
        public long MemId { get; set; }
        public DateTime? ChargeTime { get; set; }
        public decimal ChargeAmount { get; set; }
        public string ChargeWay { get; set; }
        public int ChargeStatus { get; set; }
        public DateTime CreateTime { get; set; }
        public decimal PresentAmount { get; set; }
    }
}
