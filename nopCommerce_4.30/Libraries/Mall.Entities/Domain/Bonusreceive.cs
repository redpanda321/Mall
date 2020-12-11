using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class BonusReceiveInfo
    {
        public long Id { get; set; }
        public long BonusId { get; set; }
        public string OpenId { get; set; }
        public DateTime? ReceiveTime { get; set; }
        public decimal Price { get; set; }
        public byte IsShare { get; set; }
        public byte IsTransformedDeposit { get; set; }
        public long? UserId { get; set; }
    }
}
