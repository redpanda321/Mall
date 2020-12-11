using Mall.Entities;
using System.Collections.Generic;

namespace Mall.Web.Areas.Web.Models
{
    public class HomeBrands
    {
        public HomeBrands()
        {
            listBrands = new List<WebFloorBrand>();
        }

        public ImageAdInfo imageAdinroLeft { get; set; }
        public ImageAdInfo imageAdinroRight { get; set; }

        public List<WebFloorBrand> listBrands { get; set; }
    }
}