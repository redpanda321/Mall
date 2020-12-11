using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.DTO.QueryModel;
using Mall.CommonModel;
using Mall.Entities;

namespace Mall.IServices
{
    public interface IShopBonusService : IService
    {
        /// <summary>
        ///  获取红包列表
        /// </summary>
        QueryPageModel<ShopBonusInfo> Get(long shopid, string name, int state, int pageIndex, int pageSize);

        List<ShopBonusReceiveInfo> GetCanUseDetailByUserId(long userid);
        List<Entities.ShopBonusReceiveInfo> GetDetailByUserId(long userid);
        ShopBonusReceiveInfo GetDetailById(long userid, long id);
        List<ShopBonusReceiveInfo> GetDetailToUse(long shopid, long userid, decimal sumprice);
        /// <summary>
        /// 获取用户可用红包数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        int GetAvailableBonusCountByUser(long userId);
        QueryPageModel<ShopBonusReceiveInfo> GetDetailByQuery(CouponRecordQuery query);


        ShopBonusGrantInfo GetByOrderId(long orderid);
        decimal GetUsedPrice(long orderid, long userid);

        Entities.ShopBonusGrantInfo GetGrantByUserOrder(long orderid, long userid);

        /// <summary>
        /// 获取红包
        /// </summary>
        ShopBonusInfo GetShopBonus(long id);
        List<ShopBonusInfo> GetShopBonus(List<long> bonus);

        /// <summary>
        /// 根据grantid获取
        /// </summary>
        ShopBonusInfo GetByGrantId(long grantid);

        long GetGrantIdByOrderId(long orderid);

        void SetBonusToUsed(long userid, List<Entities.OrderInfo> orders, long rid);

        /// <summary>
        /// 检查是否能添加
        /// </summary>
        bool IsAdd(long shopid);

        /// <summary>
        ///  获取红包详情
        /// </summary> 
        QueryPageModel<ShopBonusReceiveInfo> GetDetail(long bonusid, int pageIndex, int pageSize);

        List<ShopBonusReceiveInfo> GetDetailByGrantId(long grantid);

        /// <summary>
        ///  添加红包 
        /// </summary>
        void Add(ShopBonusInfo model, long shopid);

        /// <summary>
        ///  修改红包
        /// </summary>
        void Update(ShopBonusInfo model);

        /// <summary>
        ///  失效
        /// </summary>
        void Invalid(long id);

        /// <summary>
        /// 领取
        /// </summary>
        object Receive(long grantid, string openId, string wxhead, string wxname);

        /// <summary>
        /// 新增红包时判断是否超出服务费用结束日期
        /// </summary>
        /// <returns></returns>
        bool IsOverDate(DateTime bonusDateEnd, DateTime dateEnd, long shopid);

        ActiveMarketServiceInfo GetShopBonusService(long shopId);

        /// <summary>
        /// 订单支付完成时，生成红包详情
        /// </summary>
        long GenerateBonusDetail(Entities.ShopBonusInfo model, long orderid, string receiveurl);

        ShopBonusInfo GetByShopId(long shopid);
        ShopInfo GetShopByReceive(long id);
        ShopBonusGrantInfo GetGrant(long id);
        List<ShopBonusGrantInfo> GetGrants(List<long> grants);
        /// <summary>
        /// 同步微信卡券审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="auditstatus">审核状态</param>
        void SyncWeixinCardAudit(long id, Entities.WXCardLogInfo.AuditStatusEnum auditstatus);
    }
}
