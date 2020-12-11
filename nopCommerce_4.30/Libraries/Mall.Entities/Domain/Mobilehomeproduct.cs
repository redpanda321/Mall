using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MobileHomeProductInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public int PlatFormType { get; set; }
        public short Sequence { get; set; }
        public long ProductId { get; set; }
    }
}
