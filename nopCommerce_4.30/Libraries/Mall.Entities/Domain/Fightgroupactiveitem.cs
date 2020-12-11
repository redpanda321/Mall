using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FightGroupActiveItemInfo
    {
        public long Id { get; set; }
        public long ActiveId { get; set; }
        public long ProductId { get; set; }
        public string SkuId { get; set; }
        public decimal ActivePrice { get; set; }
        public int ActiveStock { get; set; }
        public int BuyCount { get; set; }
    }
}
