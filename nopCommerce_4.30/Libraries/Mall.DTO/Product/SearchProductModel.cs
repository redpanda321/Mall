using Mall.Entities;
using System.Collections.Generic;

namespace Mall.DTO
{

    public class SearchProductModel
    {
        public List<BrandInfo> Brands { get; set; }

        public List<TypeAttributesModel> ProductAttrs { get; set; }
    }
}
