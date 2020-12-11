using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Mall.IServices
{
    public interface IMobileHomeProductsService : IService
    {

        /// <summary>
        /// 获取指定店铺移动端首页商品
        /// </summary>
        /// <param name="shopId">待获取移动端首页商品的店铺Id（平台为0）</param>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        QueryPageModel<MobileHomeProductInfo> GetMobileHomePageProducts(long shopId, PlatformType platformType, ProductQuery productQuery);


        /// <summary>
        /// 分页获取指定店铺移动端首页商品
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="type"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        QueryPageModel<MobileHomeProductInfo> GetMobileHomeProducts(long shopId, PlatformType type, int pageNo, int pageSize);

        QueryPageModel<MobileHomeProductInfo> GetSellerMobileHomePageProducts(long shopId, PlatformType platformType, ProductQuery productQuery);

        /// <summary>
        /// 添加商品至首页
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="productIds">待添加的首页商品Id</param>
        /// <param name="platformType">平台类型</param>
        void AddProductsToHomePage(long shopId, PlatformType platformType, IEnumerable<long> productIds);

        /// <summary>
        /// 更新顺序
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="id"></param>
        /// <param name="sequenc">顺序号</param>
        void UpdateSequence(long shopId, long id, short sequenc);

        /// <summary>
        /// 从首页删除商品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId">店铺Id</param>
        void Delete(long shopId,long id);

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="id"></param>
        void DeleteList(long[] id);

        /// <summary>
        /// 删除该店铺所有首页商品
        /// </summary>
        /// <param name="shopId"></param>
        void DeleteAll(long shopId);
    }
}
