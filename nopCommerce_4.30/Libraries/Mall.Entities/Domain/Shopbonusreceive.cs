using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopBonusReceiveInfo
    {
        public long Id { get; set; }
        public long BonusGrantId { get; set; }
        public string OpenId { get; set; }
        public decimal Price { get; set; }
        public int State { get; set; }
        public DateTime ReceiveTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public long UserId { get; set; }
        public long? UsedOrderId { get; set; }
        public string Wxname { get; set; }
        public string Wxhead { get; set; }
    }
}
