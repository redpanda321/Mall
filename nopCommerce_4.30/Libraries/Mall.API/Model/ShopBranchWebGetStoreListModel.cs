using Mall.DTO;
using System.Collections.Generic;

namespace Mall.API.Model
{
    public class ShopBranchWebGetStoreListModel : HomePageShopBranch
    {
        public long SaleCount { get; set; }
        public long SaleCountByMonth { get; set; }
        public long CartQuantity { get; set; }
        public decimal CommentScore { get; set; } 
        public List<ShopBranchWebGetStoreListProductModel> ShowProducts { get; set; }
        public int ProductCount { get; set; }
    }

    public class ShopBranchWebGetStoreListProductModel
    {
        public long Id { get; set; }
        public string ProductName { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal MinSalePrice { get; set; }
        public bool HasSKU { get; set; }
        
        /// <summary>
        /// 默认图片
        /// </summary>
        public string DefaultImage
        {
            get;
            set;
        }
        
    }
}
