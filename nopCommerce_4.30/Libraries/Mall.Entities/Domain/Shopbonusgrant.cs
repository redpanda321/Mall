using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopBonusGrantInfo
    {
        public long Id { get; set; }
        public long ShopBonusId { get; set; }
        public long UserId { get; set; }
        public long OrderId { get; set; }
        public string BonusQr { get; set; }
    }
}
