using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ChargeDetailShopInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public DateTime? ChargeTime { get; set; }
        public decimal ChargeAmount { get; set; }
        public string ChargeWay { get; set; }
        public int ChargeStatus { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
