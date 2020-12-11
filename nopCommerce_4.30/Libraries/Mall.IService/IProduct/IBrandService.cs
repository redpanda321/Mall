using Mall.CommonModel;
using Mall.DTO.QueryModel;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IBrandService : IService
    {
        /// <summary>
        /// 添加一个品牌
        /// </summary>
        /// <param name="model"></param>
        void AddBrand(Entities.BrandInfo model);

        /// <summary>
        /// 申请一个品牌
        /// </summary>
        /// <param name="model"></param>
        void ApplyBrand(Entities.ShopBrandApplyInfo model);
        /// <summary>
        /// 申请一个品牌
        /// </summary>
        /// <param name="model"></param>
        void UpdateApplyBrand(Entities.ShopBrandApplyInfo model);

        /// <summary>
        /// 编辑一个品牌
        /// </summary>
        /// <param name="model"></param>
        void UpdateBrand(Entities.BrandInfo model);

        /// 商家编辑品牌
        /// </summary>
        /// <param name="model"></param>
        void UpdateSellerBrand(Entities.ShopBrandApplyInfo model);

        /// <summary>
        /// 删除一个品牌
        /// </summary>
        /// <param name="id"></param>
        void DeleteBrand(long id);

        /// <summary>
        /// 分页获取品牌列表
        /// </summary>
        /// <param name="keyWords">查询关键字</param>
        /// <param name="pageNo">当前第几页</param>
        /// <param name="pageSize">每页显示多少条数据</param>
        /// <returns></returns>
        QueryPageModel<Entities.BrandInfo> GetBrands(string keyWords, int pageNo, int pageSize);

        /// <summary>
        /// 审核品牌
        /// </summary>
        /// <param name="id">品牌ID</param>
        /// <param name="status">审核结果</param>
        void AuditBrand(long id,Mall.Entities.ShopBrandApplyInfo.BrandAuditStatus status,string remark);

        /// <summary>
        /// 获取查询的品牌名称列表用于下拉显示
        /// </summary>
        /// <param name="keyWords">关键字</param>
        /// <returns></returns>
        List<Entities.BrandInfo> GetBrands(string keyWords, long shopId = 0, string action = "add");

		/// <summary>
		/// 根据品牌id获取品牌
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		List<Entities.BrandInfo> GetBrandsByIds(IEnumerable<long> ids);

        /// <summary>
        /// 获取一个品牌信息
        /// </summary>
        /// <param name="id">品牌ID</param>
        /// <returns></returns>
        Entities.BrandInfo GetBrand(long id);

        /// <summary>
        /// 获取指定分类下的所有商品
        /// </summary>
        /// <param name="categoryIds">分类id</param>
        /// <returns></returns>
        List<Entities.BrandInfo> GetBrandsByCategoryIds(params long[] categoryIds);

        /// <summary>
        /// 获取指定分类下的所有商品
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="categoryIds">分类id</param>
        /// <returns></returns>
        List<Entities.BrandInfo> GetBrandsByCategoryIds(long shopId, params long[] categoryIds);

        /// <summary>
        /// 分页获取商家品牌列表
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <param name="pageNo">当前第几页</param>
        /// <param name="pageSize">每页显示多少条数据</param>
        /// <returns></returns>
        QueryPageModel<Entities.BrandInfo> GetShopBrands(long shopId, int pageNo, int pageSize);

        /// <summary>
        /// 获取查询的商家品牌名称列表用于下拉显示
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <returns></returns>
        List<Entities.BrandInfo> GetShopBrands(long shopId);


        /// <summary>
        /// 分页获取商家品牌申请列表
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageNo">当前第几页</param>
        /// <param name="pageSize">每页显示多少条数据</param>
        /// <param name="keyWords">店铺名称</param>
        /// <returns></returns>
        QueryPageModel<Entities.ShopBrandApplyInfo> GetShopBrandApplys(long? shopId, int? auditStatus, int pageNo, int pageSize, string keyWords);

        /// <summary>
        /// 搜索商家品牌申请列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetShopBrandApplyCount(BrandApplyQuery query);

        /// <summary>
        /// 获取查询的商家申请品牌名称列表用于下拉显示
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <returns></returns>
        List<Entities.ShopBrandApplyInfo> GetShopBrandApplys(long shopId);

        /// <summary>
        /// 根据名称获取商家品牌申请信息
        /// </summary>
        /// <param name="keyWords"></param>
        /// <returns></returns>
        Entities.ShopBrandApplyInfo GetShopBrandApply(string keyWords);

        /// <summary>
        /// 获取一个品牌申请信息
        /// </summary>
        /// <param name="id">品牌ID</param>
        /// <returns></returns>
        Entities.ShopBrandApplyInfo GetBrandApply(long id);

        /// <summary>
        /// 删除商家品牌申请
        /// </summary>
        /// <param name="id"></param>
        void DeleteApply(int id);

        /// <summary>
        /// 是否已申请(审核中和审核通过)
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="brandName">要申请的品牌名称</param>
        /// <returns></returns>
        bool IsExistApply(long shopId, string brandName);

        /// <summary>
        /// 是否已存在指定品牌
        /// </summary>
        /// <param name="brandName">要添加的品牌名称</param>
        /// <returns></returns>
        bool IsExistBrand(string brandName);

        /// <summary>
        /// 判断品牌是否使用中
        /// </summary>
        /// <param name="id">品牌Id</param>
        /// <returns></returns>
        bool BrandInUse(long id);

        Entities.ShopBrandApplyInfo GetExistApply(long shopId, string brandName);
    }
}
