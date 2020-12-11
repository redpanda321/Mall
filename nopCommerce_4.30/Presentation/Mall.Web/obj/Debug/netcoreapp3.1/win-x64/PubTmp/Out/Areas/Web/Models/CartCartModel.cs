using System.Collections.Generic;

namespace Mall.Web.Areas.Web.Models
{
    public class CartCartModel
    {
        public CartCartModel()
        {
            Top3RecommendProducts = new List<Entities.ProductInfo>();
        }
        /// <summary>
        /// 最近一次交易的卖家的推荐商品
        /// </summary>
        public List<Entities.ProductInfo> Top3RecommendProducts { get; set; }
    }
}