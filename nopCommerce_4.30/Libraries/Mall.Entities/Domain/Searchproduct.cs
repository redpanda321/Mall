using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SearchProductInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string BrandLogo { get; set; }
        public long FirstCateId { get; set; }
        public string FirstCateName { get; set; }
        public long SecondCateId { get; set; }
        public string SecondCateName { get; set; }
        public long ThirdCateId { get; set; }
        public string ThirdCateName { get; set; }
        public string AttrValues { get; set; }
        public int Comments { get; set; }
        public int SaleCount { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime OnSaleTime { get; set; }
        public string ImagePath { get; set; }
        public short CanSearch { get; set; }
    }
}
