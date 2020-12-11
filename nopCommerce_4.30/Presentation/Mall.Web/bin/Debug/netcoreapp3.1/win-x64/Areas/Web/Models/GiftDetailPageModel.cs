using Mall.DTO;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.Web.Areas.Web.Models
{
    public class GiftDetailPageModel
    {
        /// <summary>
        /// 礼品信息
        /// </summary>
        public GiftModel GiftData { get; set; }
        /// <summary>
        /// 热门礼品
        /// </summary>
        public List<GiftModel> HotGifts { get; set; }
        /// <summary>
        /// 是否可兑
        /// </summary>
        public bool GiftCanBuy { get; set; }
        public string CanNotBuyDes { get; internal set; }
    }
}