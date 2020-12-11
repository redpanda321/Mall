using Mall.DTO;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IShopCategoryService : IService
    {
        List<Entities.CategoryInfo> GetBusinessCategory(long shopId);
        List<Entities.CategoryInfo> GetBusinessCategory(long shopId, bool isSelf);
        List<Entities.ShopCategoryInfo> GetShopCategory(long shopId);

        /// <summary>
        /// 获取所有主分类
        /// </summary>
        /// <returns></returns>
        List<Entities.ShopCategoryInfo> GetMainCategory(long shop);

        /// <summary>
        /// 获取指定分类下面的子级分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<Entities.ShopCategoryInfo> GetCategoryByParentId(long id);

        /// <summary>
        /// 添加一个分类
        /// </summary>
        /// <param name="model"></param>
        void AddCategory(Entities.ShopCategoryInfo model);

        /// <summary>
        /// 获取一个分类信息
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <returns></returns>
        Entities.ShopCategoryInfo GetCategory(long id);
        /// <summary>
        /// 通过商品编号取得店铺所属分类
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        ShopCategoryInfo GetCategoryByProductId(long id);
        /// <summary>
        /// 通过商品编号取得店铺所属分类和分类名称集合
        /// </summary>
        /// <param name="productIds">商品id集合</param>
        /// <returns></returns>
        List<Entities.ShopInfo.ShopCategoryAndProductIdModel> GetCategoryNameAndProductIdByProductId(IEnumerable<long> productIds);

        /// <summary>
        /// 通过商品编号取得店铺所属分类列表
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        List<ShopCategoryInfo> GetCategorysByProductId(long id);

        List<ProductShopCategory> GetProductShopCategorys(List<long> products);

        /// <summary>
        /// 更新指定分类的名称
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <param name="name">分类的名称</param>
        void UpdateCategoryName(long id, string name);

        /// <summary>
        /// 更新指定分类的显示顺序
        /// </summary>
        /// <param name="id">分类Id</param>
        /// <param name="displaySequence">分类的顺序</param>
        void UpdateCategoryDisplaySequence(long id, long displaySequence);
       
        void UpdateCategorysShow(bool isShow,List<long>ids);
        IEnumerable<Entities.ShopCategoryInfo> GetSecondAndThirdLevelCategories(params long[] ids);


      


        /// <summary>
        /// 根据ID删除分类（递归删除子分类）
        /// </summary>
        /// <param name="id"></param>
        void DeleteCategory(long id, long shopId);

        /// <summary>
        /// 根据父级Id获取商品分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        IEnumerable<Entities.ShopCategoryInfo> GetCategoryByParentId(long id, long shopId);

        IEnumerable<Entities.ShopCategoryInfo> GetParentCategoryById(long id, bool isshow);
    }
}
