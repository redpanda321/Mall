using System.Collections.Generic;
using System.Linq;
using Mall.DTO;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;

namespace Mall.Application
{
    public class ShopCategoryApplication:BaseApplicaion<IShopCategoryService>
    {
        public static List<ShopCategory> GetShopCategory(long shopId)
        {
            return Service.GetShopCategory(shopId).ToList().Map<List<ShopCategory>>();
        }
        /// <summary>
        /// 根据父级Id获取商品分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<ShopCategory> GetCategoryByParentId(long id, long shopId)
        {
            return Service.GetCategoryByParentId(id, shopId).ToList().Map<List<ShopCategory>>();
        }

        /// <summary>
        /// 根据ID获取一个商家分类信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<ProductShopCategory> GetCategorysByProductId(long id)
        {
            return Service.GetProductShopCategorys(new List<long> { id });
        }


        public static List<ProductShopCategory> GetCategorysByProduct(List<long> products) {
            return Service.GetProductShopCategorys(products);
        }


        public static ShopCategoryInfo GetCategoryByProductId(long id)
        {
            return Service.GetCategoryByProductId(id);
        }
        public static List<ShopCategoryInfo> GetMainCategory(long shopId)
        {
            return Service.GetMainCategory(shopId);
        }
    }
}
