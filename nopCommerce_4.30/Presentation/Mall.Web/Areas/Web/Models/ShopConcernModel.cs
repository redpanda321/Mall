using System;
using System.Collections.Generic;
using static Mall.Entities.ShopInfo;

namespace Mall.Web.Areas.Web.Models
{
    public class ShopConcernModel
    {
        public ShopConcernModel()
        {
            FavoriteShopInfo = new ConcernShopInfo();
            HotSaleProducts = new List<HotProductInfo>();
            NewSaleProducts = new List<HotProductInfo>();

        }
        /// <summary>
        /// 关注的店铺
        /// </summary>
        public ConcernShopInfo FavoriteShopInfo { set; get; }
        /// <summary>
        /// 热销商品
        /// </summary>
        public List<HotProductInfo> HotSaleProducts { get; set; }

        /// <summary>
        /// 新上架的商品
        /// </summary>
        public List<HotProductInfo> NewSaleProducts { get; set; }

    }

    public class ConcernShopInfo
    {
        public string ShopName { set; get; }
        public string Logo { set; get; }
        public int ConcernCount { set; get; }

        public DateTime ConcernTime { set; get; }
        public string ConcernTimeStr { set; get; }
        public long ShopId { set; get; }
        public long Id { set; get; }
        public decimal SericeMark { set; get; }

        public ShopAuditStatus ShopStatus { get; set; }
    }
}