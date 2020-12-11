using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FlashSaleInfo
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public long ShopId { get; set; }
        public long ProductId { get; set; }
        public int Status { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int LimitCountOfThePeople { get; set; }
        public int SaleCount { get; set; }
        public string CategoryName { get; set; }
        public string ImagePath { get; set; }
        public decimal MinPrice { get; set; }
    }
}
