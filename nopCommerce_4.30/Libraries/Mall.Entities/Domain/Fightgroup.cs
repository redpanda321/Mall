using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FightGroupInfo
    {
        public long Id { get; set; }
        public long HeadUserId { get; set; }
        public long ActiveId { get; set; }
        public int LimitedNumber { get; set; }
        public decimal LimitedHour { get; set; }
        public int JoinedNumber { get; set; }
        public short IsException { get; set; }
        public int GroupStatus { get; set; }
        public DateTime AddGroupTime { get; set; }
        public DateTime? OverTime { get; set; }
        public long ProductId { get; set; }
        public long ShopId { get; set; }
    }
}
