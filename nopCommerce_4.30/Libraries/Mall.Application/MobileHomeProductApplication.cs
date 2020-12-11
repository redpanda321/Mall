using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class MobileHomeProductApplication
    {
       // static IMobileHomeProductsService _iMobileHomeProductsService =  EngineContext.Current.Resolve<IMobileHomeProductsService>();

        static IMobileHomeProductsService _iMobileHomeProductsService =  EngineContext.Current.Resolve<IMobileHomeProductsService>();


        /// <summary>
        /// 获取指定店铺移动端首页商品
        /// </summary>
        /// <param name="shopId">待获取移动端首页商品的店铺Id（平台为0）</param>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        public static QueryPageModel<MobileHomeProductInfo> GetMobileHomePageProducts(long shopId, PlatformType platformType, int page, int rows, string keyWords, string shopName, long? categoryId = null)
        {
            return _iMobileHomeProductsService.GetMobileHomePageProducts(shopId, platformType, new ProductQuery()
            {
                CategoryId = categoryId,
                KeyWords = keyWords,
                ShopName = shopName,
                PageSize = rows,
                PageNo = page
            });
        }

        /// <summary>
        /// 获取指定店铺移动端首页商品
        /// </summary>
        /// <param name="shopId">待获取移动端首页商品的店铺Id（平台为0）</param>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        public static List<MobileHomeProductInfo> GetMobileHomePageProducts(long shopId, PlatformType platformType)
        {
            return _iMobileHomeProductsService.GetMobileHomeProducts(shopId, platformType, 1, int.MaxValue).Models;
        }

        /// <summary>
        /// 获取指定商家店铺移动端首页商品
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="platformType"></param>
        /// <param name="productQuery"></param>
        /// <returns></returns>
        public static QueryPageModel<MobileHomeProductInfo> GetSellerMobileHomePageProducts(long shopId, PlatformType platformType, int page, int rows, string brandName, long? categoryId = null)
        {
            return _iMobileHomeProductsService.GetSellerMobileHomePageProducts(shopId, platformType, new ProductQuery()
            {
                ShopCategoryId = categoryId,
                KeyWords = brandName,
                PageSize = rows,
                PageNo = page
            });
        }

        /// <summary>
        /// 添加商品至首页
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="productIds">待添加的首页商品Id</param>
        /// <param name="platformType">平台类型</param>
        public static void AddProductsToHomePage(long shopId, PlatformType platformType, IEnumerable<long> productIds)
        {
            _iMobileHomeProductsService.AddProductsToHomePage(shopId, platformType, productIds);
        }

        /// <summary>
        /// 更新顺序
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="id"></param>
        /// <param name="sequenc">顺序号</param>
        public static void UpdateSequence(long shopId, long id, short sequence)
        {
            _iMobileHomeProductsService.UpdateSequence(shopId, id, sequence);
        }

        /// <summary>
        /// 从首页删除商品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId">店铺Id</param>
        public static void Delete(long shopId, long id)
        {
            _iMobileHomeProductsService.Delete(shopId, id);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        public static void DeleteList(long[] ids)
        {
            _iMobileHomeProductsService.DeleteList(ids);
        }

        /// <summary>
        /// 删除该店铺所有首页商品
        /// </summary>
        /// <param name="shopId"></param>
        public static void DeleteAll(long shopId)
        {
            _iMobileHomeProductsService.DeleteAll(shopId);
        }
    }
}
