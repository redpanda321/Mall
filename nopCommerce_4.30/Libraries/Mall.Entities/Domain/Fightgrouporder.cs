using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FightGroupOrderInfo
    {
        public long Id { get; set; }
        public long ActiveId { get; set; }
        public long ProductId { get; set; }
        public string SkuId { get; set; }
        public long GroupId { get; set; }
        public long OrderId { get; set; }
        public long OrderUserId { get; set; }
        public short IsFirstOrder { get; set; }
        public DateTime JoinTime { get; set; }
        public int JoinStatus { get; set; }
        public DateTime? OverTime { get; set; }
        public long Quantity { get; set; }
        public decimal SalePrice { get; set; }
    }
}
