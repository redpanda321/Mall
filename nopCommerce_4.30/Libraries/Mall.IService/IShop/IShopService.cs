using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Linq;
using System;
using System.Collections.Generic;
using Mall.CommonModel;
using Mall.DTO;

namespace Mall.IServices
{
    public interface IShopService : IService
    {
        #region  店铺服务接口
        /// <summary>
        /// 获取店铺信息（以分页的形式展示）
        /// </summary>
        /// <param name="shopQueryModel">ShopQuery对象</param>
        /// <returns></returns>
        QueryPageModel<ShopInfo> GetShops(ShopQuery shopQueryModel);
        /// <summary>
        /// 获取店铺数量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetShopCount(ShopQuery query);
        List<ShopInfo> GetAllShops();

        /// <summary>
        /// 获取一个店铺信息
        /// </summary>
        /// <param name="id">店铺ID</param>
        /// <returns></returns>
        ShopInfo GetShop(long id, bool businessCategoryOn = false);

        /// <summary>
        /// 根据id获取门店
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<ShopInfo> GetShops(IEnumerable<long> ids);

       

        /// <summary>
        /// 获取店铺基本信息
        /// </summary>
        /// <param name="id">店铺ID</param>
        /// <returns></returns>
        ShopInfo GetShopBasicInfo(long id);

        /// <summary>
        /// 删除一个店铺
        /// </summary>
        /// <param name="id">店铺Id</param>
        void DeleteShop(long id);

        /// <summary>
        /// 更新店铺
        /// </summary>
        /// <param name="shop">店铺Id</param>
        void UpdateShop(ShopInfo shop);
        /// <summary>
        /// 清除店铺缓存
        /// </summary>
        /// <param name="id"></param>
        void ClearShopCache(long id);

        void SetProvideInvoice(ShopInvoiceConfigInfo info);
        /// <summary>
        /// 设置门店自动分配订单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enable"></param>
        void SetAutoAllotOrder(long id, bool enable);
        /// <summary>
        /// 设置商家入驻类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        void SetBusinessType(long id, ShopBusinessType type);
        /// <summary>
        /// 设置银行账户
        /// </summary>
        /// <param name="account"></param>
        void SetBankAccount(BankAccount account);

        /// <summary>
        /// 修改银行账户
        /// </summary>
        /// <param name="account"></param>
        void UpdateBankAccount(BankAccount account);
        /// <summary>
        /// 设置微信账户
        /// </summary>
        /// <param name="model"></param>
        void SetWeChatAccount(WeChatAccount model);
        void SetLicenseCert(ShopLicenseCert model);

        /// <summary>
        /// 更新店铺
        /// </summary>
        /// <param name="shop">店铺Id</param>
        /// <param name="categoryIds">经营类目</param>
        void UpdateShop(ShopInfo shop, List<long> categoryIds);


        /// <summary>
        /// 申请新的经营类目
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="categoryIds"></param>
        void ApplyShopBusinessCate(long shopId, List<long> categoryIds);
        List<BusinessCategoryApplyDetailInfo> GetBusinessCategoriesApplyDetails(long applyid);


        /// <summary>
        /// 审核申请的经营类目
        /// </summary>
        /// <param name="applyId"></param>
        void AuditShopBusinessCate(long applyId, BusinessCategoryApplyInfo.BusinessCateApplyStatus status);


        /// <summary>
        /// 分頁獲取申請的經營類目列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<BusinessCategoryApplyInfo> GetBusinessCateApplyList(BussinessCateApplyQuery query);


        /// <summary>
        /// 獲取申請的某個經營列表
        /// </summary>
        /// <param name="applyId"></param>
        /// <returns></returns>
        BusinessCategoryApplyInfo GetBusinessCategoriesApplyInfo(long applyId);


        /// <summary>
        /// 获取店铺的经营类目
        /// </summary>
        /// <param name="id">店铺Id</param>
        /// <returns></returns>
        List<BusinessCategoryInfo> GetBusinessCategory(long id);
        QueryPageModel<BusinessCategoryInfo> GetBusinessCategory(long shop, int pageNo, int pageSize);

        /// <summary>
        /// 获取选择的类目下的所有三级类目（不包含已申请的类目)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<CategoryRateModel> GetThirdBusinessCategory(long id, long shopId);
        /// <summary>
        /// 是否可以删除经营类目
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="bCategoryId"></param>
        /// <returns></returns>
        bool CanDeleteBusinessCategory(long shopId, long bCategoryId);

        /// <summary>
        /// 保存指定店铺的经营类目
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="bCategoryList"></param>
        void SaveBusinessCategory(long shopId, Dictionary<long, decimal> bCategoryList);

        /// <summary>
        /// 修改指定经营类目分佣比例
        /// </summary>
        /// <param name="id">经营类目Id</param>
        /// <param name="commisRate">分佣比例</param>
        void SaveBusinessCategory(long id, decimal commisRate);

        /// <summary>
        /// 更新店铺的状态
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="status">状态</param>
        /// <param name="comments">注释</param>
        void UpdateShopStatus(long shopId, ShopInfo.ShopAuditStatus status, string comments = "", int TrialDays = 0);

        /// <summary>
        /// 判断店铺名称是否存在
        /// </summary>
        /// <param name="shopName"></param>
        /// <returns></returns>
        bool ExistShop(string shopName, long shopId = 0);

        /// <summary>
        /// 是否已过期
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        bool IsExpiredShop(long shopId);
        /// <summary>
        /// 是否冻结
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        bool IsFreezeShop(long shopId);
        /// <summary>
        /// 是否官方自营店
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        bool IsSelfShop(long shopId);
        /// <summary>
        /// 判断公司名称是否存在
        /// </summary>
        /// <param name="shopName"></param>
        /// <returns></returns>
        bool ExistCompanyName(string companyName, long shopId = 0);


        /// <summary>
        /// 检测营业执照号是否重复
        /// </summary>
        /// <param name="BusinessLicenceNumber">营业执照号</param>
        /// <param name="shopId"></param>
        bool ExistBusinessLicenceNumber(string BusinessLicenceNumber, long shopId = 0);

        QueryPageModel<ShopInfo> GetSellers(SellerQuery sellerQueryModel);

        #endregion

        #region 更新店铺运费

        /// <summary>
        /// 更新店铺运费
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="freight">运费</param>
        /// <param name="freeFreight">满额免运费</param>
        void SetShopFreight(long shopId, decimal freight, decimal freeFreight);

        void SetCompnayInfo(ShopCompanyInfo model);

        #endregion

        #region 店铺等级服务接口

        /// <summary>
        /// 获取所有店铺等级列表
        /// </summary>
        /// <returns></returns>
        List<ShopGradeInfo> GetShopGrades();

        /// <summary>
        /// 获取指定店铺等级信息
        /// </summary>
        /// <param name="id">店铺等级Id</param>
        /// <returns></returns>
        ShopGradeInfo GetShopGrade(long id);

        /// <summary>
        /// 新建一个店铺等级
        /// </summary>
        /// <param name="shopGrade"></param>
        void AddShopGrade(ShopGradeInfo shopGrade);

        /// <summary>
        /// 删除一个指定的店铺等级
        /// </summary>
        /// <param name="id">店铺等级Id</param>
        void DeleteShopGrade(long id);


        /// <summary>
        /// 更新店铺等级
        /// </summary>
        /// <param name="shopGrade"></param>
        void UpdateShopGrade(ShopGradeInfo shopGrade);

        #endregion

        /// <summary>
        /// 获取店铺已使用空间大小
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        long GetShopUsageSpace(long shopId);
        /// <summary>
        /// 获取门店剩余空间大小
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        long GetShopSurplusSpace(long shop);


        /// <summary>
        /// 获取用户关注的店铺
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        QueryPageModel<FavoriteShopInfo> GetUserConcernShops(long userId, int pageNo, int pageSize);

        /// <summary>
        /// 取消用户关注的店铺
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="userId"></param>
        void CancelConcernShops(List<long> ids, long userId);
        void CancelConcernShops(long shopId, long userId);


      

        #region 创建店铺

        /// <summary>
        /// 创建一个空店铺
        /// </summary>
        /// <returns></returns>
        ShopInfo CreateEmptyShop();



        #endregion


        /// <summary>
        /// 添加店铺关注
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="shopId">店铺Id</param>
        void AddFavoriteShop(long memberId, long shopId);

        bool IsFavoriteShop(long memberId, long shopId);


        /// <summary>
        /// 获取指定会员所有店铺关注
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <returns></returns>
        List<FavoriteShopInfo> GetFavoriteShopInfos(long memberId);

        /// <summary>
        /// 获取店铺的关注度
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetShopFavoritesCount(long shopId);

        /// <summary>
        /// 获取店铺宝贝数
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetShopProductCount(long shopId);

        ShopInfo GetSelfShop();

        /// <summary>
        /// 获取商铺免邮活动的邮费
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        decimal GetShopFreeFreight(long id);

        /// <summary>
        /// 获取店铺账户信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<ShopAccountInfo> GetShopAccounts(List<long> ids);

        /// <summary>
        /// 获取单个店铺账户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ShopAccountInfo GetShopAccount(long id);

        void UpdateLogo(long shopId, string img);
        bool UpdateOpenTopImageAd(long shopId,bool isOpenTopImageAd);

        /// <summary>
        /// 店铺的评分统计
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        List<StatisticOrderCommentInfo> GetShopStatisticOrderComments(long shopId);
        List<StatisticOrderCommentInfo> GetStatisticOrderComments(List<long> shops);

            /// <summary>
            /// 检测并初始店铺模板
            /// </summary>
            /// <param name="shopId"></param>
            void CheckInitTemplate(long shopId);

        ShopInfo.ShopVistis GetShopVistiInfo(DateTime startDate, DateTime endDate, long shopId);

        /// <summary>
        /// 获取店铺的订单销量
        /// </summary>
        long GetSales(long id);

        List<ShopBrand> GetShopBrands(List<long> shops);

        /// <summary>
        /// 添加店铺续费记录
        /// </summary>
        /// <param name="record"></param>
        void AddShopRenewRecord(ShopRenewRecordInfo record, bool isShopAccount = false);

        /// <summary>
        /// 店铺续费
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="year"></param>
        void ShopReNew(long shopid, int year);

        /// <summary>
        /// 店铺升级
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="gradeid"></param>
        void ShopUpGrade(long shopid, long gradeid);

        /// <summary>
        ///  获取店铺续费记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<ShopRenewRecordInfo> GetShopRenewRecords(ShopQuery query);
        /// <summary>
        /// 冻结/解冻店铺
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state">true冻结 false解冻</param>
        void FreezeShop(long id, bool state);
        /// <summary>
        /// 将所有在售的商品下架
        /// </summary>
        /// <param name="id"></param>
        void SaleOffAllProduct(long id);


        /// <summary>
        /// 获取单条入驻缴费记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ShopRenewRecordInfo GetShopRenewRecord(long id);

        /// <summary>
        /// 获取商铺管理员会员ID
        /// </summary>
        /// <param name="ShopId"></param>
        /// <returns></returns>
        long GetShopManagers(long ShopId);

        /// <summary>
        /// 根据网店管家uCode获取对应店铺
        /// </summary>
        /// <param name="uCode"></param>
        /// <returns></returns>
        ShopWdgjSettingInfo GetshopInfoByCode(string uCode);

        ShopWdgjSettingInfo GetshopWdgjInfoById(long shopId);
        void UpdateShopWdgj(ShopWdgjSettingInfo wdgj);
        void AddShopWdgj(ShopWdgjSettingInfo wdgj);

        void SetAutoPrint(long id, bool enable);
        void SetPrintCount(long id, int count);
        bool HasProvideInvoice(List<long> shops);

        /// <summary>
        /// 设置申请商家步骤
        /// </summary>
        /// <param name="shopStage">第几步</param>
        /// <param name="id">店铺Id</param>
        void SetShopStage(ShopInfo.ShopStage shopStage, long id);

        #region TDO:ZYF Invoice
        /// <summary>
        /// 获取商家发票管理配置
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ShopInvoiceConfigInfo GetShopInvoiceConfig(long shopId);

        /// <summary>
        /// 获取用户默认的发票信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        InvoiceTitleInfo GetInvoiceTitleInfo(long userId, InvoiceType typeId);
        #endregion

    }
}
