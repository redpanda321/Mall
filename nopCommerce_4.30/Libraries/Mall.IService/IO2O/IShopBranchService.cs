using System;
using System.Collections.Generic;
using Mall.Entities;
using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.DTO;

namespace Mall.IServices
{

    public interface IShopBranchService : IService
    {
        /// <summary>
        /// 添加分店
        /// </summary>
        /// <param name="shopBranchInfo"></param>
        void AddShopBranch(ShopBranchInfo shopBranchInfo);

        /// <summary>
        /// 添加分店管理员
        /// </summary>
        /// <param name="shopBranchManagersInfo"></param>
        void AddShopBranchManagers(ShopBranchManagerInfo shopBranchManagersInfo);
        /// <summary>
        /// 更新门店信息
        /// </summary>
        /// <param name="shopBranch"></param>
        void UpdateShopBranch(ShopBranchInfo shopBranch);


        /// <summary>
        /// 设置门店管理员密码
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="password"></param>
        /// <param name="passwordSalt"></param>
        void SetShopBranchManagerPassword(long managerId, string password, string passwordSalt);

        /// <summary>
        /// 删除门店
        /// </summary>
        /// <param name="id"></param>
        void DeleteShopBranch(long id);
        /// <summary>
        /// 判断门店名称是否重复
        /// </summary>
        /// <param name="shopId">商家店铺ID</param>
        /// <param name="shopBranchName">门店名字</param>
        /// <returns></returns>
        bool Exists(long shopId, long shopBranchId, string shopBranchName);

        /// <summary>
        /// 根据查询条件判断是否有门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        bool Exists(ShopBranchQuery query);
        /// <summary>
        /// 根据门店ID获取门店
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ShopBranchInfo GetShopBranchById(long id);

        /// <summary>
        /// 根据门店IDs获取门店
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        List<ShopBranchInfo> GetShopBranchByIds(List<long> Ids);

        /// <summary>
        /// 根据门店联系方式获取门店信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        ShopBranchInfo GetShopBranchByContact(string contact);

        /// <summary>
        /// 根据门店ID取门店管理员
        /// </summary>
        /// <param name="branchId"></param>
        /// <returns></returns>
        List<ShopBranchManagerInfo> GetShopBranchManagers(long branchId);
        /// <summary>
        /// 根据ID取门店管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ShopBranchManagerInfo GetShopBranchManagersById(long id);
        /// <summary>
        /// 根据用户名、密码、门店ID,取管理员信息
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        ShopBranchManagerInfo GetShopBranchManagersByName(string userName);
        /// <summary>
        /// 分页查询门店信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<ShopBranchInfo> GetShopBranchs(ShopBranchQuery query);
        /// <summary>
        /// 取商家所有门店
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        List<ShopBranchInfo> GetShopBranchByShopId(long shopId);
        /// <summary>
        /// 根据分店id获取分店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<ShopBranchInfo> GetShopBranchs(List<long> ids);
        /// <summary>
        /// 冻结门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        void FreezeShopBranch(long shopBranchId);
        /// <summary>
        /// 解冻门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        void UnFreezeShopBranch(long shopBranchId);

        /// <summary>
        /// 获取分店经营的商品SKU
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchIds"></param>
        /// <param name="status">null表示所有</param>
        /// <returns></returns>
        List<ShopBranchSkuInfo> GetSkus(long shopId, List<long> shopBranchIds, ShopBranchSkuStatus? status = ShopBranchSkuStatus.Normal, List<string> skuids = null);

        List<ShopBranchSkuInfo> GetSkus(long shopId, List<long> branchs);
        /// <summary>
        /// 根据门店id、skuid获取门店sku信息
        /// </summary>
        /// <param name="shopBranchIds"></param>
        /// <param name="skuids"></param>
        /// <returns></returns>
        List<ShopBranchSkuInfo> GetSkusByBranchIds(List<long> shopBranchIds, List<string> skuids = null);
        /// <summary>
        /// 根据skuid取门店SKU
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        List<ShopBranchSkuInfo> GetSkusByIds(long shopBranchId, List<string> skuIds);
        /// <summary>
        /// 添加门店sku
        /// </summary>
        /// <param name="infos"></param>
        void AddSkus(List<ShopBranchSkuInfo> infos);

        void SetStock(long branch, string sku, int stock, StockOptionType option);
        void SetStock(long branch, Dictionary<string, int> changes, StockOptionType option);

        void SetProductStock(long branch, long productId, int stock, StockOptionType option);

        /// <summary>
        /// 设置门店SKU状态
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pIds"></param>
        /// <param name="status"></param>
        void SetBranchProductStatus(long shopBranchId, List<long> pIds, ShopBranchSkuStatus status);
        /// <summary>
        /// 设置门店SKU状态
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="status"></param>
        void SetBranchProductStatus(long productId, ShopBranchSkuStatus status);
        /// <summary>
        /// 获取门店商品销量
        /// </summary>
        /// <param name="branch">门店ID</param>
        /// <param name="products">商品ID</param>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>{ProductID:SaleCount}</returns>
        Dictionary<long, long> GetProductSaleCount(long branch, List<long> products, DateTime begin, DateTime end);
        /// <summary>
        /// 搜索门店商品
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        QueryPageModel<ProductInfo> SearchProduct(ShopBranchProductQuery search);
        /// <summary>
        /// 搜索门店是否存在该商品
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        bool CheckProductIsExist(long shopBranchId, long productId);
        /// <summary>
        /// 获取门店虚拟销量总和
        /// </summary>
        /// <param name="branchId"></param>
        /// <returns></returns>
        long GetVirtualSaleCounts(long branchId);
        /// <summary>
        /// 查询门店的sku
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        List<ShopBranchSkuInfo> SearchShopBranchSkus(long shopBranchId, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// 获取周边门店-分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<ShopBranchInfo> GetNearShopBranchs(ShopBranchQuery query);
        /// <summary>
        /// 搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>关键字搜索</returns>
        QueryPageModel<ShopBranchInfo> SearchNearShopBranchs(ShopBranchQuery search);
        /// <summary>
        /// 搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>标签搜索</returns>
        QueryPageModel<ShopBranchInfo> TagsSearchNearShopBranchs(ShopBranchQuery search);
        /// <summary>
        /// 根据商品搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>标签搜索</returns>
        QueryPageModel<ShopBranchInfo> StoreByProductNearShopBranchs(ShopBranchQuery search);

        QueryPageModel<ShopBranchInfo> GetShopBranchsAll(ShopBranchQuery query);
        double GetLatLngDistancesFromAPI(string fromLatLng, string latlng);
        /// <summary>
        /// 搜索门店-不分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<ShopBranchInfo> GetAllShopBranchs(ShopBranchQuery query);
        /// <summary>
        /// 获取门店配送范围在同一区域的门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        QueryPageModel<ShopBranchInfo> GetArealShopBranchsAll(int areaId, int shopId, float latitude, float longitude);
        /// <summary>
        /// 自动分配订单到门店
        /// </summary>
        /// <param name="query"></param>
        /// <param name="skuIds"></param>
        /// <param name="counts"></param>
        /// <returns></returns>
        ShopBranchInfo GetAutoMatchShopBranch(ShopBranchQuery query, string[] skuIds, int[] counts);
        /// <summary>
        /// 获取代理商品的门店编号集
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<long> GetAgentShopBranchIds(long productId);

        /// <summary>
        /// 推荐门店
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        bool RecommendShopBranch(long[] ids);

        /// <summary>
        /// 推荐门店排序
        /// </summary>
        /// <param name="oriShopBranchId"></param>
        /// <param name="newShopBranchId"></param>
        /// <returns></returns>
        bool RecommendChangeSequence(long oriShopBranchId, long newShopBranchId);

        /// <summary>
        /// 取消推荐门店
        /// </summary>
        /// <param name="shopBranchId">门店ID</param>
        /// <returns></returns>
        bool ResetShopBranchRecommend(long shopBranchId);

        #region 门店标签
        /// <summary>
        /// 检查标签名是否存在
        /// </summary>
        /// <param name="title">标签名</param>
        /// <param name="excludeId">排除ID</param>
        /// <returns></returns>
        bool ExistTag(string title, long excludeId);
        /// <summary>
        /// 获取所有标签
        /// </summary>
        /// <returns></returns>
        List<ShopBranchTagInfo> GetAllShopBranchTagInfo();
        List<ShopBranchInTag> GetShopBranchInTagByBranchs(List<long> branchs);
        Dictionary<long, int> GetShopBranchInTagCount(List<long> ids);

        ShopBranchTagInfo GetShopBranchTagInfo(long id);
        int GetShopBranchInTagCount(long id);
        /// <summary>
        /// 新增标签
        /// </summary>
        /// <param name="title">标签名</param>
        void AddShopBranchTagInfo(string title);
        /// <summary>
        /// 修改标签
        /// </summary>
        /// <param name="shopBranchTagInfo"></param>
        /// <returns></returns>
        void UpdateShopBranchTagInfo(long id,string title);
        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="ShopBranchTagInfoId"></param>
        /// <returns></returns>
        void DeleteShopBranchTagInfo(long id);
       
        /// <summary>
        /// 批量设置门店标签
        /// </summary>
        /// <param name="branchs"></param>
        /// <param name="tags"></param>
        void SetShopBranchTags(List<long> branchs, List<long> tags);
        /// <summary>
        /// 获取评分
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ShopStoreServiceMark GetServiceMark(long id);

        #endregion

        /// <summary>
        /// 获取允许管理的门店
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        List<ShopBranchInfo> GetManagerShops(long shop);

        /// <summary>
        /// 设置管理门店权限
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="enable"></param>
        void SetManagerShop(long branch, bool enable);
    }
}
