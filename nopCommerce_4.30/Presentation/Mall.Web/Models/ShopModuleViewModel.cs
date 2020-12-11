using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Models
{
    public class ShopModuleViewModel
    {
        public ShopHomeModuleInfo Module { get; set; } = new ShopHomeModuleInfo();
        public List<ShopHomeModuleProductInfo> ModuleProducts { get; set; } = new List<ShopHomeModuleProductInfo>();
        public List<ProductInfo> Products { get; set; } = new List<ProductInfo>();

        public List<ShopHomeModuleTopImgInfo> Images { get; set; }
    }
}