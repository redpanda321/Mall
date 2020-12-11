
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class VshopHomeSiteViewModel
    {
        public Entities.VShopInfo VShop { get; set; }
        public long ShopId { get; set; }
        public List<Entities.SlideAdInfo> SlideImage { get; set; }
        public List<Entities.BannerInfo> Banner { get; set; }
    }
}