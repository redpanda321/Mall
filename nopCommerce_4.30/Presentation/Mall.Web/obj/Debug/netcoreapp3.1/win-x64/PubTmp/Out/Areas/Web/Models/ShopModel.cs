using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;



namespace Mall.Web.Areas.Web
{
    public class ShopSearchViewModel
    {
        public string Keywords { get; set; }

        public long BrandId { get; set; }

        public long CategoryId { get; set; }

        public int OrderBy { get; set; }

        public bool IsEmpty { get { return Total == 0;  } }

        public bool IsSaleCountOnOff { get; set; }

        public Dictionary<long, string> Brands { get; set; }

        public Dictionary<long, string> Categorys { get; set; }

        public long Total { get; set; }

        public List<Shop> Shops { get; set; }

        public PagingInfo Pager { get; set; }
    }

    public class Shop
    {
        public long Id { get; set; }
        public string ShopName { get; set; }
        public string ProductAndDescription { get; set; }
        public string SellerDeliverySpeed { get; set; }
        public string SellerServiceAttitude { get; set; }

        public string Logo { get; set; }

        public int SaleCount { get; set; }

        public int ProductCount { get; set; }

        public int FreeFreight { get; set; }

        public bool HasCoupon { get; set; }

        public List<Product> Products { get; set; }
    }

    public class Product
    {
        public long Id { get; set; }
        public decimal Price { get; set; }

        public string Image { get; set; }
        public long CategoryId { get; set; }

    }
}