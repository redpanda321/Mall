using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.Entities;
using Mall.IServices;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{
    public class TypeApplication:BaseApplicaion<ITypeService>
	{
		/// <summary>
		/// 获取所有的商品类型列表，包括分页信息
		/// search是搜索条件，如果search为空即显示全部
		/// </summary>
		/// <param name="search">搜索条件</param>
		/// <param name="page">页码</param>
		/// <param name="rows">每页行数</param>
		/// <param name="count">总行数</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.ProductType> GetTypes(string search, int pageNo, int pageSize)
		{
			var result= Service.GetTypes(search, pageNo, pageSize);

			return new QueryPageModel<DTO.ProductType>()
			{
				Models = result.Models.Map<List<DTO.ProductType>>(),
				Total = result.Total
			};
		}

		/// <summary>
		/// 获取所有的商品类型列表
		/// </summary>
		/// <returns></returns>
		public static List<Entities.TypeInfo> GetTypes()
		{
			return Service.GetTypes().ToList();
		}

		/// <summary>
		/// 根据Id获取商品类型实体
		/// </summary>
		/// <param name="id">类型Id</param>
		/// <returns></returns>
		public static DTO.ProductType GetProductType(long id)
		{
			return Service.GetType(id).Map<DTO.ProductType>();
		}

        public static TypeInfo GetType(long id)
        {
            return Service.GetType(id);
        }

        public static List<TypeInfo> GetTypes(List<long> types)
        {
            return Service.GetTypes(types);
        }
		

		/// <summary>
		/// 删除商品类型
		/// </summary>
		/// <param name="id"></param>
		public static void DeleteType(long id)
		{
			Service.DeleteType(id);
		}

        public static void SaveType(ProductType type)
        {
            //保存基本信息
            var info = type.Map<TypeInfo>();
            Service.SaveType(info);

            //保存管理品牌
            var brands = type.Brands.Select(p => p.Id).ToList();
            Service.SaveBrands(info.Id, brands);

            //保存属性


        }
    }
}
 