using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopBranchInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ShopBranchName { get; set; }
        public int AddressId { get; set; }
        public string AddressPath { get; set; }
        public string AddressDetail { get; set; }
        public string ContactUser { get; set; }
        public string ContactPhone { get; set; }
        public int Status { get; set; }
        public DateTime CreateDate { get; set; }
        public int ServeRadius { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public string ShopImages { get; set; }
        public short IsStoreDelive { get; set; }
        public short IsAboveSelf { get; set; }
        public TimeSpan StoreOpenStartTime { get; set; }
        public TimeSpan StoreOpenEndTime { get; set; }
        public short IsRecommend { get; set; }
        public long RecommendSequence { get; set; }
        public int DeliveFee { get; set; }
        public int DeliveTotalFee { get; set; }
        public int FreeMailFee { get; set; }
        public string DaDaShopId { get; set; }
        public short IsAutoPrint { get; set; }
        public int PrintCount { get; set; }
        public short IsFreeMail { get; set; }
        public short EnableSellerManager { get; set; }
        public short IsShelvesProduct { get; set; }
    }
}
