using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class VShopInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long ShopId { get; set; }
        public DateTime CreateTime { get; set; }
        public int VisitNum { get; set; }
        public int BuyNum { get; set; }
        public int State { get; set; }
        public string Logo { get; set; }
        public string BackgroundImage { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public string HomePageTitle { get; set; }
        public string Wxlogo { get; set; }
        public byte IsOpen { get; set; }
    }
}
