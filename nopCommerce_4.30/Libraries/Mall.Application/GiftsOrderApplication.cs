using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class GiftsOrderApplication : BaseApplicaion<IGiftsOrderService>
    {
        public static int GetOrderCount(GiftsOrderQuery query)
        {
            return Service.GetOrderCount(query);
        }

        public static int GetOwnBuyQuantity(long userid, long giftid)
        {
            return Service.GetOwnBuyQuantity(userid, giftid);
        }
        public static GiftOrderInfo CreateOrder(GiftOrderModel model)
        {
            return Service.CreateOrder(model);
        }
        public static QueryPageModel<GiftOrderInfo> GetOrders(GiftsOrderQuery query)
        {
            return Service.GetOrders(query);
        }
        public static List<GiftOrderItemInfo> GetOrderItemByOrder(long id)
        {
            return Service.GetOrderItemByOrder(id);
        }

        public static int GetOrderCount(GiftOrderInfo.GiftOrderStatus? status, long userId = 0)
        {
            return Service.GetOrderCount(status, userId);
        }
        public static GiftOrderInfo GetOrder(long orderId, long userId)
        {
            return Service.GetOrder(orderId, userId);
        }
        public static void ConfirmOrder(long id, long userId)
        {
            Service.ConfirmOrder(id, userId);
        }
    }
}
