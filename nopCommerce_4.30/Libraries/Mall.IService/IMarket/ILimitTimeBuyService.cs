using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.IServices
{
    /// <summary>
    /// 限时购服务
    /// </summary>
    public interface ILimitTimeBuyService : IService
    {
        #region 平台

        FlashSaleConfigModel GetConfig();

        void UpdateConfig(FlashSaleConfigModel data);


        /// <summary>
        /// 更新限时购服务设置
        /// </summary>
        void UpdateServiceSetting(LimitTimeBuySettingModel model);

        /// <summary>
        /// 获取限时购服务设置
        /// </summary>
        /// <returns></returns>
        LimitTimeBuySettingModel GetServiceSetting();



        /// <summary>
        /// 更新限时购服务分类
        /// </summary>
        /// <param name="categoryId"></param>
        void AddServiceCategory(string categoryName);

        /// <summary>
        /// 删除一个限时购服务分类
        /// </summary>
        /// <param name="categoryId"></param>
        void DeleteServiceCategory(string categoryName);

        /// <summary>
        /// 获取限时购服务分类
        /// </summary>
        /// <returns></returns>
        string[] GetServiceCategories();

     

        #endregion


        #region 商家



        /// <summary>
        /// 根据店铺Id获取该店铺购买的限时购营销服务信息
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        ActiveMarketServiceInfo GetMarketService(long shopId);


        #endregion


        #region 前台

        void AddFlashSaleDetails(List<FlashSaleDetailInfo> details);


        /// <summary>
        ///  根据商品Id获取一个限时购的详细信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        FlashSaleInfo GetLimitTimeMarketItemByProductId(long pid);

        /// <summary>
        /// 判断商品是否正在做限时购
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsLimitTimeMarketItem(long id);

        #endregion

        #region 获取指定限时购活动商品Id的销售量(单个用户)

        int GetMarketSaleCountForUserId(long pId, long userId);

        #endregion


        #region 新版本限时购

        FlashSaleDetailInfo GetDetail(string skuid);

        /// <summary>
        /// 后台分页取数据
        /// </summary>
        QueryPageModel<FlashSaleInfo> GetFlashSaleInfos(LimitTimeQuery query);

        /// <summary>
        /// 获取限时购数量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetFlashSaleCount(LimitTimeQuery query);

      
        /// <summary>
        /// 前端获取参加限时购的所有活动商品列表
        /// </summary>
        /// <returns></returns>
        QueryPageModel<FlashSaleInfo> GetAll(FlashSaleQuery query);

     
        FlashSaleModel GetFlaseSaleByProductId(long pid);
        FlashSaleModel GetDetailInfo(long productId);

        /// <summary>
        /// 获取单条记录
        /// </summary>
        FlashSaleModel Get(long id);

        /// <summary>
        /// 增加限时购
        /// </summary>
        void AddFlashSale(FlashSaleModel model);

        /// <summary>
        /// 更新限时购
        /// </summary>
        void UpdateFlashSale(FlashSaleModel model);

        /// <summary>
        /// 通过
        /// </summary>
        void Pass(long id);

        /// <summary>
        /// 拒绝
        /// </summary>
        void Refuse(long id);

        /// <summary>
        /// 取消
        /// </summary>
        void Cancel(long id);


        /// <summary>
        /// 删除
        /// </summary>
        void Delete(long id, long shopId);

        /// <summary>
        /// 判断某产品是否可以作为限时购添加
        /// </summary>
        bool IsAdd(long productid);
        /// <summary>
        /// 判断某产品是否可以作为限时购
        /// </summary>
        bool IsEdit(long productid, long id);

        /// <summary>
        /// 判断某个产品是否是未开始的限时购
        /// </summary>
        FlashSaleModel IsFlashSaleDoesNotStarted(long productid);

        /// <summary>
        /// 根据商品id获取限时购价格
        /// </summary>
        /// <param name="ids">商品id集合</param> 
        /// <returns>key = productid , value = price</returns>
        List<FlashSalePrice> GetPriceByProducrIds(List<long> ids);

        /// <summary>
        /// 增加一条需要发送的开团提醒记录
        /// </summary>
        /// <param name="flashSaleId"></param>
        /// <param name="openId"></param>
        void AddRemind(long flashSaleId, string openId);

        /// <summary>
        /// 增加销量
        /// </summary>
        void IncreaseSaleCount(List<long> orderids);

        /// <summary>
        /// 订单退款减少销量
        /// </summary>
        /// <param name="orderids"></param>
        void ReduceSaleCount(OrderRefundInfo refund);
    
        /// <summary>
        /// 提前结束
        /// </summary>
        void EndActive(long id);
        #endregion

        FlashSaleInfo GetFlashSaleInfoByProductIdAndShopId(long productId, long shopId);

        FlashSaleInfo GetFlashSaleInfo(long id);

        /// <summary>
        /// 根据限时购id集合获取限时购详细列表
        /// </summary>
        /// <param name="flashSaleIds">限时购ids</param>
        /// <returns></returns>
        List<FlashSaleDetailInfo> GetFlashSaleDetailByFlashSaleIds(IEnumerable<long> flashSaleIds);

    }
}
