using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberGroupInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public int StatisticsType { get; set; }
        public int Total { get; set; }
    }
}
