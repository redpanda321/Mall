using Mall.Entities;
using System.Collections.Generic;
using System.Linq;
using Mall.DTO;

namespace Mall.IServices
{
    /// <summary>
    /// 店铺首页商品模块服务
    /// </summary>
    public interface IShopHomeModuleService : IService
    {
        #region 老版本
        /// <summary>
        /// 添加商品模块信息
        /// </summary>
        /// <param name="shopProductModuleInfo">商品模块</param>
        void AddShopProductModule(ShopHomeModuleInfo shopProductModuleInfo);

        /// <summary>
        /// 更新商品模块名称
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id">商品模块id</param>
        /// <param name="name">待更新的商品模块名称</param>
        void UpdateShopProductModuleName(long shopId, long id, string name);

        /// <summary>
        /// 更新商品模块所包含的商品模块
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id">待更新的商品模块id</param>
        /// <param name="productIds">商品id</param>
        void UpdateShopProductModuleProducts(long shopId,long id, IEnumerable<long> productIds);

        /// <summary>
        /// 删除商品模块
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id">待删除的商品模块id</param>
        void Delete(long shopId, long id);


        /// <summary>
        /// 获取指定店铺所有商品模块
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <returns></returns>
        List<ShopHomeModuleInfo> GetAllShopHomeModuleInfos(long shopId);
        List<ShopHomeModuleTopImgInfo> GetImages(List<long> modules);
        List<ShopHomeModuleProductInfo> GetProducts(List<long> modules);

            /// <summary>
            /// 获取指定商品模块
            /// </summary>
            /// <param name="shopId">店铺id</param>
            /// <param name="id">商品模块id</param>
            /// <returns></returns>
            ShopHomeModuleInfo GetShopHomeModuleInfo(long shopId, long id);
        #endregion

        string GetFooter( long shopid );

        void EditFooter( long shopid , string footer );

        void SaveFloor( AddShopHomeModuleModel model );

        void Enable( long id , bool enable );

        void UpdateFloorSequence( long shopId , int oriRowNumber , int newRowNumber , string direction );

        void DelFloor( long id );
    }
}
