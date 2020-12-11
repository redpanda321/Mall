using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class FavoriteApplication:BaseApplicaion<IProductService>
    {
        public static int GetFavoriteCountByUser(long userid)
        {
            return Service.GetFavoriteCountByUser(userid);
        }

        public static int GetFavoriteCountByProduct(long product) {
            return Service.GetFavoriteCountByProduct(product);
        }
        public static int GetFavoriteShopCount(long userid)
        {
            return Service.GetFavoriteShopCount(userid);
        }

        public static int GetFavoriteShopCountByShop(long shop) {
            return Service.GetFavoriteShopCountByShop(shop);
        }
        public static List<FavoriteShopInfo> GetFavoriteShop(long user, int top)
        {
            return Service.GetFavoriteShop(user, top);
        }
        public static List<FavoriteInfo> GetFavoriteByUser(long user, int top)
        {
            return Service.GetUserAllConcern(user, top);
        }
    }
}
