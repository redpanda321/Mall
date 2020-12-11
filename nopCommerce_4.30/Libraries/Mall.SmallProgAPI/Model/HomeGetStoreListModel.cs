using Mall.DTO;
using System.Collections.Generic;

namespace Mall.SmallProgAPI.Model
{
    public class HomeGetStoreListModel : HomePageShopBranch
    {
        public decimal CommentScore { get; set; }
        public long SaleCount { get; set; }
        public long SaleCountByMonth { get; set; }
        public long CartQuantity { get; set; }
        public List<HomeGetStoreListProductModel> ShowProducts { get; set; }
        public int ProductCount { get; set; }
    }

    public class HomeGetStoreListProductModel
    {
        public long Id { get; set; }
        public string ProductName { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal MinSalePrice { get; set; }
        public bool HasSKU { get; set; }
        public long SaleCount { get; set; }
        public int HighCommentCount { get; set; }

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
