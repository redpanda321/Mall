using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberConsumeStatisticInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ShopId { get; set; }
        public decimal NetAmount { get; set; }
        public long OrderNumber { get; set; }
    }
}
