using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberBuyCategoryInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long CategoryId { get; set; }
        public int OrdersCount { get; set; }
    }
}
