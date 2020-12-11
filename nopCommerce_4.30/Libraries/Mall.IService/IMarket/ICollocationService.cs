using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.IServices
{
    public interface ICollocationService : IService
    {

        /// <summary>
        /// 商家添加一个组合购
        /// </summary>
        /// <param name="info"></param>
        void AddCollocation(Collocation info);


        /// <summary>
        /// 修改组合购
        /// </summary>
        /// <param name="info"></param>
        void EditCollocation(Collocation info);



        //使组合购失效
        void CancelCollocation(long CollocationId, long shopId);
        /// <summary>

          /// <summary>
        /// 获取商家添加的组合购列表
        /// </summary>
        /// <returns></returns>
        QueryPageModel<CollocationInfo> GetCollocationList(CollocationQuery query);


        /// <summary>
        /// 根据商品ID获取组合购信息
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        CollocationInfo GetCollocationByProductId(long productId);
        List<CollocationPoruductInfo> GetCollocationListByProductId(long productId);
        List<CollocationInfo> GetAvailableCollocationByProduct(long product);
        List<CollocationPoruductInfo> GetProducts(List<long> collocation);
        List<CollocationSkuInfo> GetSKUs(List<long> collProducts);
        /// <summary>
        /// 根据组合购ID获取组合购信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        CollocationInfo GetCollocation(long Id);

        /// <summary>
        /// 根据组合商品获取组合SKU信息
        /// </summary>
        /// <param name="colloPid"></param>
        /// <param name="skuid"></param>
        /// <returns></returns>
         CollocationSkuInfo GetColloSku(long colloPid, string skuid);

        //获取一个商品的组合购SKU信息
         List<CollocationSkuInfo> GetProductColloSKU(long productid, long colloPid);
    }
}
