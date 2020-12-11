using Mall.Application;
using Mall.IServices;
using Mall.Web.Framework;
using System.Collections.Generic;

namespace Mall.Web.App_Code.Common
{
    public class PaymentHelper
    {
        /// <summary>
        /// 支付完生成红包
        /// </summary>
        public static Dictionary<long, Entities.ShopBonusInfo> GenerateBonus(IEnumerable<long> orderIds, string urlHost)
        {
            var bonusGrantIds = new Dictionary<long, Entities.ShopBonusInfo>();
            string url = CurrentUrlHelper.GetScheme() + "://" + urlHost + "/m-weixin/shopbonus/index/";
            var bonusService = ServiceApplication.Create<IShopBonusService>();
            var buyOrders = ServiceApplication.Create<IOrderService>().GetOrders(orderIds);
            foreach (var o in buyOrders)
            {
                var shopBonus = bonusService.GetByShopId(o.ShopId);
                if (shopBonus == null)
                {
                    continue;
                }
                if (shopBonus.GrantPrice <= o.OrderTotalAmount)
                {
                    long grantid = bonusService.GenerateBonusDetail(shopBonus, o.Id, url);
                    bonusGrantIds.Add(grantid, shopBonus);
                }
            }

            return bonusGrantIds;
        }

        /// <summary>
        /// 更改限时购销售量
        /// </summary>
        public static void IncreaseSaleCount(List<long> orderid)
        {
            if (orderid.Count == 1)
            {
                var service = ServiceApplication.Create<ILimitTimeBuyService>();
                service.IncreaseSaleCount(orderid);
            }
        }
    }
}