using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{
    public class BrandApplication:BaseApplicaion<IBrandService>
	{
		/// <summary>
		/// 添加一个品牌
		/// </summary>
		/// <param name="model"></param>
		public static void AddBrand(DTO.Brand model)
		{
			Service.AddBrand(model.Map<Entities.BrandInfo>());
		}

		/// <summary>
		/// 申请一个品牌
		/// </summary>
		/// <param name="model"></param>
		public static void ApplyBrand(DTO.ShopBrandApply model)
		{
			Service.ApplyBrand(model.Map<Entities.ShopBrandApplyInfo>());
		}

		/// <summary>
		/// 编辑一个品牌
		/// </summary>
		/// <param name="model"></param>
		public static void UpdateBrand(DTO.Brand model)
		{
			Service.UpdateBrand(model.Map<Entities.BrandInfo>());
		}

		/// 商家编辑品牌
		/// </summary>
		/// <param name="model"></param>
		public static void UpdateSellerBrand(DTO.ShopBrandApply model)
		{
			Service.UpdateSellerBrand(model.Map<Entities.ShopBrandApplyInfo>());
		}

		/// <summary>
		/// 删除一个品牌
		/// </summary>
		/// <param name="id"></param>
		public static void DeleteBrand(long id)
		{
			Service.DeleteBrand(id);
		}

		/// <summary>
		/// 分页获取品牌列表
		/// </summary>
		/// <param name="keyWords">查询关键字</param>
		/// <param name="pageNo">当前第几页</param>
		/// <param name="pageSize">每页显示多少条数据</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.Brand> GetBrands(string keyWords, int pageNo, int pageSize)
		{
			var list = Service.GetBrands(keyWords, pageNo, pageSize);

			return new QueryPageModel<DTO.Brand>
			{
				Models = list.Models.Map<List<DTO.Brand>>(),
				Total = list.Total
			};
		}

        public static List<BrandInfo> GetBrands(IEnumerable<long> brands)
        {
            return Service.GetBrandsByIds(brands.ToList());
        }

		/// <summary>
		/// 审核品牌
		/// </summary>
		/// <param name="id">品牌ID</param>
		/// <param name="status">审核结果</param>
		public static void AuditBrand(long id, Mall.Entities.ShopBrandApplyInfo.BrandAuditStatus status,string remark)
		{
			Service.AuditBrand(id, status, remark);
		}

		/// <summary>
		/// 获取查询的品牌名称列表用于下拉显示
		/// </summary>
		/// <param name="keyWords">关键字</param>
		/// <returns></returns>
		public static List<DTO.Brand> GetBrands(string keyWords)
		{
			return Service.GetBrands(keyWords).Map<List<DTO.Brand>>();
		}
		
		/// <summary>
		/// 根据品牌id获取品牌
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static List<DTO.Brand> GetBrandsByIds(IEnumerable<long> ids)
		{
			return Service.GetBrandsByIds(ids).Map<List<DTO.Brand>>();
		}

		/// <summary>
		/// 获取一个品牌信息
		/// </summary>
		/// <param name="id">品牌ID</param>
		/// <returns></returns>
		public static BrandInfo GetBrand(long id)
		{
            return Service.GetBrand(id);
		}

        
		/// <summary>
		/// 获取指定分类下的所有商品
		/// </summary>
		/// <param name="categoryIds">分类id</param>
		/// <returns></returns>
		public static List<DTO.Brand> GetBrandsByCategoryIds(params long[] categoryIds)
		{
			return Service.GetBrandsByCategoryIds(categoryIds).Map<List<DTO.Brand>>();
		}

		/// <summary>
		/// 获取指定分类下的所有商品
		/// </summary>
		/// <param name="shopId">店铺id</param>
		/// <param name="categoryIds">分类id</param>
		/// <returns></returns>
		public static List<DTO.Brand> GetBrandsByCategoryIds(long shopId, params long[] categoryIds)
		{
			return Service.GetBrandsByCategoryIds(shopId, categoryIds).Map<List<DTO.Brand>>();
		}

		/// <summary>
		/// 分页获取商家品牌列表
		/// </summary>
		/// <param name="shopId">商家Id</param>
		/// <param name="pageNo">当前第几页</param>
		/// <param name="pageSize">每页显示多少条数据</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.Brand> GetShopBrands(long shopId, int pageNo, int pageSize)
		{
			var list = Service.GetShopBrands(shopId, pageNo, pageSize);
			return new QueryPageModel<DTO.Brand>
			{
				Models = list.Models.Map<List<DTO.Brand>>(),
				Total = list.Total
			};
		}

		/// <summary>
		/// 获取查询的商家品牌名称列表用于下拉显示
		/// </summary>
		/// <param name="shopId">商家Id</param>
		/// <returns></returns>
		public static List<DTO.Brand> GetShopBrands(long shopId)
		{
			return Service.GetShopBrands(shopId).Map<List<DTO.Brand>>();
		}

		/// <summary>
		/// 分页获取商家品牌申请列表
		/// </summary>
		/// <param name="shopId">商家Id</param>
		/// <param name="auditStatus">审核状态</param>
		/// <param name="pageNo">当前第几页</param>
		/// <param name="pageSize">每页显示多少条数据</param>
		/// <param name="keyWords">店铺名称</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.ShopBrandApply> GetShopBrandApplys(long? shopId, int? auditStatus, int pageNo, int pageSize, string keyWords)
		{
			var list = Service.GetShopBrandApplys(shopId, auditStatus, pageNo, pageSize, keyWords);

			return new QueryPageModel<DTO.ShopBrandApply>
			{
				Models = list.Models.Map<List<DTO.ShopBrandApply>>(),
				Total = list.Total
			};
		}

        public static  int GetShopBrandApplyCount(BrandApplyQuery query)
        {
            return Service.GetShopBrandApplyCount(query);
        }

        /// <summary>
        /// 获取查询的商家申请品牌名称列表用于下拉显示
        /// </summary>
        /// <param name="shopId">商家Id</param>
        /// <returns></returns>
        public static List<DTO.ShopBrandApply> GetShopBrandApplys(long shopId)
		{
			return Service.GetShopBrandApplys(shopId).Map<List<DTO.ShopBrandApply>>();
		}

        /// <summary>
        /// 根据品牌名称获取商家新品牌申请信息
        /// </summary>
        /// <param name="keyWords"></param>
        /// <returns></returns>
        public static DTO.ShopBrandApply GetShopBrandApply(string keyWords)
        {
            return Service.GetShopBrandApply(keyWords).Map<DTO.ShopBrandApply>();
        }

        /// <summary>
        /// 获取一个品牌申请信息
        /// </summary>
        /// <param name="id">品牌ID</param>
        /// <returns></returns>
        public static DTO.ShopBrandApply GetBrandApply(long id)
		{
			return Service.GetBrandApply(id).Map<DTO.ShopBrandApply>();
		}

		/// <summary>
		/// 删除商家品牌申请
		/// </summary>
		/// <param name="id"></param>
		public static void DeleteApply(int id)
		{
			Service.DeleteApply(id);
		}

		/// <summary>
		/// 是否已申请(审核中和审核通过)
		/// </summary>
		/// <param name="shopId">店铺Id</param>
		/// <param name="brandName">要申请的品牌名称</param>
		/// <returns></returns>
		public static bool IsExistApply(long shopId, string brandName)
		{
			return Service.IsExistApply(shopId, brandName);
		}

		/// <summary>
		/// 是否已存在指定品牌
		/// </summary>
		/// <param name="brandName">要添加的品牌名称</param>
		/// <returns></returns>
		public static bool IsExistBrand(string brandName)
		{
			return Service.IsExistBrand(brandName);
		}

		/// <summary>
		/// 判断品牌是否使用中
		/// </summary>
		/// <param name="id">品牌Id</param>
		/// <returns></returns>
		public static bool BrandInUse(long id)
		{
			return Service.BrandInUse(id);
		}

        public static Entities.ShopBrandApplyInfo GetExistApply(long shopId, string brandName)
        {
            return Service.GetExistApply(shopId, brandName);
        }
    }
}
