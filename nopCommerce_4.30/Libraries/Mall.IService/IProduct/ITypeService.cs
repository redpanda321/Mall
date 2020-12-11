
namespace Mall.IServices
{
    using Mall.CommonModel;
    using Mall.Entities;
    using System.Collections.Generic;

    public interface ITypeService : IService
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
        QueryPageModel<TypeInfo> GetTypes(string search, int pageNo, int pageSize);
        List<SpecificationValueInfo> GetValuesByType(long type);
        List<AttributeInfo> GetAttributesByType(long type);
        AttributeInfo GetAttribute(long id);
        List<AttributeValueInfo> GetAttributeValues(long attribute);
        List<AttributeValueInfo> GetAttributeValues(List<long> attributes);
        List<TypeInfo> GetTypes(List<long> ids);
        void SaveType(TypeInfo type);
        void SaveBrands(long type, List<long> brands);
        List<long> GetBrandsByType(long type);

        /// <summary>
        /// 获取所有的商品类型列表
        /// </summary>
        /// <returns></returns>
        List<Entities.TypeInfo> GetTypes();

        /// <summary>
        /// 根据Id获取商品类型实体
        /// </summary>
        /// <param name="id">类型Id</param>
        /// <returns></returns>
        Entities.TypeInfo GetType(long id);

        /// <summary>
        /// 根据ProductId获取商品类型实体
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <returns></returns>
        Entities.TypeInfo GetTypeByProductId(long productId);

        /// <summary>
        /// 更新商品类型
        /// </summary>
        /// <param name="model"></param>
        void UpdateType(Entities.TypeInfo model);

        /// <summary>
        /// 删除商品类型
        /// </summary>
        /// <param name="id"></param>
        void DeleteType(long id);

        /// <summary>
        /// 创建商品类型
        /// </summary>
        /// <param name="model"></param>
        void AddType(Entities.TypeInfo model);

    }
}
