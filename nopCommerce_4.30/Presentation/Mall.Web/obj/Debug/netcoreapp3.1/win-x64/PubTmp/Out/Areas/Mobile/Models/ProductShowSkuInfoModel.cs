using Mall.Web.Areas.Web.Models;
using System.Collections.Generic;

namespace Mall.Web.Areas.Mobile.Models
{
    public class ProductShowSkuInfoModel
    {
        public ProductShowSkuInfoModel()
        {
            Color = new CollectionSKU();
            Size = new CollectionSKU();
            Version = new CollectionSKU();
        }

        public string ProductImagePath { get; set; }
        public decimal MinSalePrice { get; set; }
        public string MeasureUnit { get; set; }
        public int MaxBuyCount { get; set; }
        public CollectionSKU Color { get; set; }
        public CollectionSKU Size { get; set; }
        public CollectionSKU Version { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }

        public bool IsOpenLadder { get; set; }
        public List<Entities.VirtualProductItemInfo> VirtualProductItem { get; set; }
        /// <summary>
        /// 规格总库存
        /// </summary>
        public long StockAll { get; set; }
    }
}