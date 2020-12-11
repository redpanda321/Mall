using Mall.Application;
using Mall.CommonModel;
using System;
using System.Linq;

namespace Mall.Web.Framework
{
    public class ShopServiceMark
    {
        public static ShopServiceMarkModel GetShopComprehensiveMark(long shopId)
        {
            var result = new ShopServiceMarkModel();
            var orderComment = TradeCommentApplication.GetOrderComments(shopId);


            result.PackMark = orderComment.Models.Count() == 0 ? 0 :
                Math.Round(orderComment.Models.ToList().Average(o => ((decimal)o.PackMark + o.DeliveryMark) / 2), 2);
            result.ServiceMark = orderComment.Models.Count() == 0 ? 0 :
                Math.Round(orderComment.Models.ToList().Average(o => (decimal)o.ServiceMark), 2);
            result.ComprehensiveMark = Math.Round((result.PackMark + result.ServiceMark) / 2, 2);
            return result;
        }
    }
}
