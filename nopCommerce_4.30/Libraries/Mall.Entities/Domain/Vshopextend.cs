using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class VShopExtendInfo
    {
        public long Id { get; set; }
        public long VshopId { get; set; }
        public int Sequence { get; set; }
        public int Type { get; set; }
        public DateTime AddTime { get; set; }
        public int State { get; set; }
    }
}
