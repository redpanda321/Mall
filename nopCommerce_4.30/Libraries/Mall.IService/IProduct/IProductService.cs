using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Mall.IServices
{
    /// <summary>
    /// 商品服务接口
    /// </summary>
    public interface IProductService : IService
    {
        #region 平台服务

        /// <summary>
        /// 查询商品
        /// </summary>
        /// <returns></returns>
        QueryPageModel<ProductInfo> GetProducts(ProductQuery query);
        /// <summary>
        /// 查询商品数量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetProductCount(ProductQuery query);

        /// <summary>
        /// 审核商品
        /// </summary>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="productId">商品编号</param>
        /// <param name="failedMessage">说明</param>
        void AuditProduct(long productId, ProductInfo.ProductAuditStatus auditStatus, string message);


        /// <summary>
        /// 批量审核商品
        /// </summary>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="productIds">商品编号</param>
        /// <param name="failedMessage">说明</param>
        void AuditProducts(IEnumerable<long> productIds, ProductInfo.ProductAuditStatus auditStatus, string message);

        #endregion

        #region 商家服务

        long GetNextProductId();



        List<ProductShopCategoryInfo> GetProductShopCategories(long productId);

        /// <summary>
        /// 根据商品Id获取属性
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <returns></returns>
        List<ProductAttributeInfo> GetProductAttribute(long productId);

        /// <summary>
        /// 根据商品Id获取规格
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <returns></returns>
        List<SellerSpecificationValueInfo> GetSellerSpecifications(long shopId, long typeId);

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="model"></param>
        [System.Obsolete]
        void AddProduct(ProductInfo model,ProductDescriptionInfo description, List<SKUInfo> skus);

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="product">商品信息</param>
        /// <param name="pics">需要转移的商品图片地址</param>
        /// <param name="skus">skus，至少要有一项</param>
        /// <param name="description">描述</param>
        /// <param name="attributes">商品属性</param>
        /// <param name="goodsCategory">商家分类</param>
        /// <param name="sellerSpecifications">商家自定义规格</param>
        void AddProduct(long shopId, ProductInfo product, string[] pics, SKUInfo[] skus, ProductDescriptionInfo description, ProductAttributeInfo[] attributes, long[] goodsCategory, SellerSpecificationValueInfo[] sellerSpecifications, ProductLadderPriceInfo[] prices);

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="model"></param>
        void UpdateProduct(ProductInfo model);

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="product">修改后的商品</param>
        /// <param name="pics">商品图片地址</param>
        /// <param name="skus">skus，至少要有一项</param>
        /// <param name="description">描述</param>
        /// <param name="attributes">商品属性</param>
        /// <param name="goodsCategory">商家分类</param>
        /// <param name="sellerSpecifications">商家自定义规格</param>
        /// <param name="prices">阶梯价</param>
        void UpdateProduct(ProductInfo product, string[] pics, SKUInfo[] skus, ProductDescriptionInfo description, ProductAttributeInfo[] attributes, long[] goodsCategory, SellerSpecificationValueInfo[] sellerSpecifications, ProductLadderPriceInfo[] prices);

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="ids">商品id</param>
        /// <param name="shopId">商铺id</param>
        void DeleteProduct(IEnumerable<long> ids, long shopId);

        /// <summary>
        /// 获取一个商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProductInfo GetProduct(long id);
        List<ProductInfo> GetProducts(List<long> products);

        Dictionary<string, int> GetCommentsNumber(long product);


        /// <summary>
        /// 获取商品详情页需要及时刷新的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProductInfo GetNeedRefreshProductInfo(long id);
        /// <summary>
        /// 获取商品描述
        /// </summary>
        /// <param name="productId">商品编号</param>
        /// <returns></returns>
        ProductDescriptionInfo GetProductDescription(long productId);

        /// <summary>
        /// 获取商品描述
        /// </summary>
        /// <param name="productIds">商品编号</param>
        /// <returns></returns>
        List<ProductDescriptionInfo> GetProductDescriptions(IEnumerable<long> productIds);

        /// <summary>
        /// 获取商品的评论数
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        int GetProductCommentCount(long productId);


        /// <summary>
        /// 获取某件商品的所有sku
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<SKUInfo> GetSKUs(long productId);
        List<SKUInfo> GetSkuList(long productId);
        /// <summary>
        /// 是否有规格
        /// </summary>
        /// <param name="id">产品编号</param>
        /// <returns></returns>
        bool HasSKU(long id);


        /// <summary>
        /// 获取某些商品的所有sku 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        ///  /// Add:zesion[150906]
        List<SKUInfo> GetSKUs(IEnumerable<long> productIds);

        /// <summary>
        /// 根据sku id 获取sku信息
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        List<Entities.SKUInfo> GetSKUs(IEnumerable<string> skuIds);

        /// <summary>
        /// 获取一个sku
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        Entities.SKUInfo GetSku(string skuId);

        string GetSkuString(string skuId);

        /// <summary>
        /// 获取某店铺下所有商品数量
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <returns></returns>
        int GetShopAllProducts(long shopId);

        /// <summary>
        /// 获取店铺销售中的所有商品
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetShopOnsaleProducts(long shopId);

        void SaveSellerSpecifications(List<SellerSpecificationValueInfo> info);

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="id">待下架的商品id</param>
        /// <param name="shopId">店铺Id</param>
        void SaleOff(long id, long shopId);

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="ids">待下架的商品id</param>
        /// <param name="shopId">店铺Id</param>
        void SaleOff(IEnumerable<long> ids, long shopId);
        /// <summary>
        /// 设置商品的安全库存
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        void SetProductOverSafeStock(IEnumerable<long> pids, long stock);
        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="id">商品id</param>
        /// <param name="shopId">店铺id</param>
        void OnSale(long id, long shopId);

        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="ids">商品id</param>
        /// <param name="shopId">店铺id</param>
        void OnSale(IEnumerable<long> ids, long shopId);



        /// <summary>
        /// 获取门店虚拟销量
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        long GetProductVirtualSale(long shop);

        /// <summary>
        /// 批量获取商品信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<ProductInfo> GetProductByIds(IEnumerable<long> ids);
        List<ProductInfo> GetAllProductByIds(IEnumerable<long> ids);
        /// <summary>
        /// 根据ID，取商品信息（所有状态）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<ProductInfo> GetAllStatusProductByIds(IEnumerable<long> ids);
        /// <summary>
        /// 绑定商品描述版式
        /// </summary>
        /// <param name="topTemplateId">顶部版式id</param>
        /// <param name="bottomTemplateId">底部版式id</param>
        /// <param name="productIds">商品id</param>
        void BindTemplate(long? topTemplateId, long? bottomTemplateId, IEnumerable<long> productIds);

        #endregion

        #region  前台页面(搜索)

        QueryPageModel<ProductInfo> SearchProduct(ProductSearch search);

        #endregion

        #region 获取店铺热销的前N件商品
        List<ProductInfo> GetHotSaleProduct(long shopId, int count = 5);
        #endregion

        #region 获取店铺最新上架的前N件商品
        List<ProductInfo> GetNewSaleProduct(long shopId, int count = 5);
        #endregion

        #region 获取店铺最受关注的前N件商品
        List<ProductInfo> GetHotConcernedProduct(long shopId, int count = 5);
        #endregion

        #region 获取用户关注的商品
        QueryPageModel<FavoriteInfo> GetUserConcernProducts(long userId, int pageNo, int pageSize);

        /// <summary>
        /// 添加商品关注
        /// </summary>
        /// <param name="productId"></param>
        void AddFavorite(long productId, long userId, out int status);
        /// <summary>
        /// 获取用户关注商品数
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        int GetFavoriteCountByUser(long userid);
        int GetFavoriteCountByProduct(long product);
        /// <summary>
        /// 获取用户关注门店数
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        int GetFavoriteShopCount(long userid);
        /// <summary>
        /// 获取用户关注门店
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        List<FavoriteShopInfo> GetFavoriteShop(long userid, int top);

        int GetFavoriteShopCountByShop(long shop);

        /// <summary>
        /// 判断是否关注过此商品
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        bool IsFavorite(long productId, long userId);
        #endregion

        #region 取消用户关注的商品
        void CancelConcernProducts(IEnumerable<long> ids, long userId);

        void DeleteFavorite(long productId, long userId);
        #endregion

        #region 更新商品库存

        void SetSkuStock(string skuId, long stock, StockOptionType option);
        void SetProductStock(List<long> products, long stock, StockOptionType option);
        void SetSkuStock(StockOptionType option, Dictionary<string, long> changes);

        #endregion

        #region 添加商品浏览记录
        void AddBrowsingProduct(BrowsingHistoryInfo info);
        #endregion

        #region 获取用户浏览记录
        List<BrowsingHistoryInfo> GetBrowsingProducts(long userId);
        #endregion



        #region 获取商品的销售情况

        ProductVistiInfo GetProductVistInfo(long pId, List<ProductVistiInfo> pInfo = null);

        #endregion



        #region 获取运费

        /// <summary>
        /// 根据产品获取运费
        /// </summary>
        /// <param name="productIds">产品Id集合</param>
        /// <param name="counts">购买总数（数量/重量/体积）集合</param>
        /// <param name="cityId">收货地址城市id</param>
        /// <returns></returns>
        decimal GetFreight(IEnumerable<long> productIds, IEnumerable<int> counts, int cityId, bool isShow = false);


        #endregion

        #region 获取最近一次交易的卖家的推荐商品
        List<ProductInfo> GetPlatHotSaleProductByNearShop(int count, long userId, bool isRecommend = false);
        #endregion

        List<FavoriteInfo> GetUserAllConcern(long user, int top);


        /// <summary>
        /// 申请商品上架
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        /// Add:DZY[150715]
        bool ApplyForSale(long id);
        void ApplyForSale(ProductInfo product);
        /// <summary>
        /// 是否为限时购商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsLimitBuy(long id);

        /// <summary>
        /// 修改推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="relationProductIds"></param>
        void UpdateRelationProduct(long productId, string relationProductIds);

        /// <summary>
        /// 获取商品的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        ProductRelationProductInfo GetRelationProductByProductId(long productId);

        /// <summary>
        /// 获取商品的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<ProductRelationProductInfo> GetRelationProductByProductIds(IEnumerable<long> productIds);

        /// <summary>
        /// 获取指定类型下面热销的前N件商品
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<ProductInfo> GetHotSaleProductByCategoryId(int categoryId, int count);

        /// <summary>
        /// 获取商品销量
        /// </summary>
        /// <param name="products">商品集合</param>
        /// <returns>Key:ProductID,Value:SaleCounts</returns>
        Dictionary<long, long> GetProductSaleCounts(List<long> products);

        /// <summary>
        /// 是否指定地区包邮
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="discount"></param>
        /// <param name="streetId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        bool IsFreeRegion(long productId, decimal discount, int streetId, int count, string skuId);
        /// <summary>
        /// 批量设置商品的运费模板
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="freightTemplateId"></param>
        bool BatchSettingFreightTemplate(IEnumerable<long> pids, long freightTemplateId);
        /// <summary>
        /// 批量设置sku的库存
        /// </summary>
        /// <param name="dics"></param>
        /// <returns></returns>
        bool BatchSettingStock(Dictionary<long, long> dics);
        /// <summary>
        /// 批量设置市场价/商城价
        /// </summary>
        /// <param name="productDics"></param>
        /// <param name="priceDics"></param>
        /// <returns></returns>
        bool BatchSettingPrice(Dictionary<long, decimal> productDics, Dictionary<long, string> priceDics);
        /// <summary>
        /// 当前参加的活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        string CurrentJoinActive(long productId);
        /// <summary>
        /// 更新商品商家序号
        /// </summary>
        /// <param name="id"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        bool UpdateShopDisplaySequence(long id, int order);
        /// <summary>
        /// 更新商品平台序号
        /// </summary>
        /// <param name="id"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        bool UpdateDisplaySequence(long id, int order);
        /// <summary>
        /// 批量更新虚拟销量
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="virtualSaleCounts"></param>
        /// <returns></returns>
        bool BtachUpdateSaleCount(List<long> productIds, long virtualSaleCounts, int minSaleCount = 0, int maxSaleCount = 0);
        void DeleteImportProduct(List<long> productIds);

        #region 虚拟商品
        VirtualProductInfo GetVirtualProductInfoByProductId(long productId);
        List<VirtualProductInfo> GetVirtualProductInfoByProductIds(List<long> productIds);

        List<VirtualProductItemInfo> GetVirtualProductItemInfoByProductId(long productId);
        #endregion
    }
}
