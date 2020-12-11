using System;
using System.Collections.Generic;

namespace Mall.Web.Areas.Mobile
{
    public class CartProductModel
    {
        public long cartItemId { get; set; }
        public string skuId { get; set; }
        public long id { get; set; }
        public string imgUrl { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public int count { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Version { get; set; }
        public string ShopId { get; set; }
        public string VShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopLogo { get; set; }
        public string Url { get; set; }
        public int status { get; set; }
        public int ProductStatus { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
        public DateTime? AddTime { get; set; }

        public long shopBranchId { get; set; }

        public string shopBranchName { get; set; }

        public string ShopBranchLogo { get; set; }

        public bool IsShopBranch { get { return shopBranchId > 0; } set { } }

    }
}