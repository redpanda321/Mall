using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System.Collections.Generic;

namespace Mall.Application
{
    /// <summary>
    /// 限时购
    /// </summary>
    public class LimitTimeApplication : BaseApplicaion<ILimitTimeBuyService>
    {
        /// <summary>
        /// 是否正在限时购
        /// </summary>
        /// <param name="pid">商品ID</param>
        /// <returns></returns>
        public static bool IsLimitTimeMarketItem(long pid)
        {
            return Service.IsLimitTimeMarketItem(pid);
        }
        /// <summary>
        /// 取限时购价格
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<FlashSalePrice> GetPriceByProducrIds(List<long> ids)
        {
            return Service.GetPriceByProducrIds(ids);
        }

        public static List<FlashSalePrice> GetLimitProducts(List<long> ids = null)
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_LIMITPRODUCTS))
                return Cache.Get<List<FlashSalePrice>>(CacheKeyCollection.CACHE_LIMITPRODUCTS);
            if (ids == null)
            {
                ids = new List<long>();
            }
            var result = Service.GetPriceByProducrIds(ids);
            Cache.Insert(CacheKeyCollection.CACHE_LIMITPRODUCTS, result, 120);
            return result;
        }

        public static void AddFlashSaleDetails(List<FlashSaleDetailInfo> details)
        {
            Service.AddFlashSaleDetails(details);
        }
        /// <summary>
        /// 商家删除限时购
        /// </summary>
        /// <param name="id"></param>
        public void DeleteLimitBuy(long id, long shopId)
        {
            Service.Delete(id, shopId);
        }

        /// <summary>
        ///  根据商品Id获取一个限时购的详细信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static FlashSaleInfo GetLimitTimeMarketItemByProductId(long pid)
        {
            return Service.GetLimitTimeMarketItemByProductId(pid);
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="skuid"></param>
        /// <returns></returns>
        public static FlashSaleDetailInfo GetDetail(string skuid)
        {
            return Service.GetDetail(skuid);
        }
        public static FlashSaleModel IsFlashSaleDoesNotStarted(long productid)
        {
            return Service.IsFlashSaleDoesNotStarted(productid);
        }
        public static FlashSaleConfigModel GetConfig()
        {
            return Service.GetConfig();
        }
        public static bool IsAdd(long productid)
        {
            return Service.IsAdd(productid);
        }

        public static int GetFlashSaleCount(LimitTimeQuery query)
        {
            return Service.GetFlashSaleCount(query);
        }

        /// <summary>
        /// 根据限时购id集合获取限时购详细列表
        /// </summary>
        /// <param name="flashSaleIds">限时购ids</param>
        /// <returns></returns>
        public static List<FlashSaleDetailInfo> GetFlashSaleDetailByFlashSaleIds(IEnumerable<long> flashSaleIds)
        {
            return Service.GetFlashSaleDetailByFlashSaleIds(flashSaleIds);
        }
    }
}
