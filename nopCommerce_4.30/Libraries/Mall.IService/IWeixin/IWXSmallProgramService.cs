using Mall.CommonModel;
using Mall.DTO.QueryModel;
using System.Collections.Generic;
using Mall.Entities;
namespace Mall.IServices
{
    public interface IWXSmallProgramService : IService
    {
        /// <summary>
        /// 获取所有商品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<ProductInfo> GetWXSmallProducts(int page, int rows, ProductQuery productQuery);

        /// <summary>
        /// 添加商品
        /// </summary>
        void AddWXSmallProducts(WXSmallChoiceProductInfo model);

        /// <summary>
        /// 获取所有商品
        /// </summary>
        /// <returns></returns>
        List<WXSmallChoiceProductInfo> GetWXSmallProducts();
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        void DeleteWXSmallProductById(long Id);
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        void DeleteWXSmallProductByIds(List<long> ids);

     
        QueryPageModel<ProductInfo> GetWXSmallHomeProducts(int pageNo, int pageSize);
    }
}
