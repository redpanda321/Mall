using Mall.CommonModel;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using System.Linq;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    [MarketingAuthorization]
    public class ShopBonusController : BaseAdminController
    {
        private IMarketService _iMarketService;

        public ShopBonusController(IMarketService iMarketService)
        {
            _iMarketService = iMarketService;
        }
      
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult ServiceSetting()
        {
            Entities.MarketSettingInfo model = _iMarketService.GetServiceSetting( MarketType.RandomlyBonus );
            return View( model );
        }

        [HttpPost]
        [UnAuthorize]
        public ActionResult SaveServiceSetting( decimal Price )
        {
            Result result = new Result();
            var model = new Entities.MarketSettingInfo { Price = Price , TypeId = MarketType.RandomlyBonus };
            _iMarketService.AddOrUpdateServiceSetting( model );
            result.success = true;
            result.msg = "保存成功！";
            return Json( result );
        }

        [UnAuthorize]
        public ActionResult List(MarketBoughtQuery query)
        {
            query.MarketType = MarketType.RandomlyBonus;
            var marketEntities = _iMarketService.GetBoughtShopList(query);
            var market = marketEntities.Models.Select(item => {
                var obj = MarketApplication.GetMarketService(item.MarketServiceId);
                return new
                {
                    Id = item.Id,
                    StartDate = item.StartTime.ToString("yyyy-MM-dd"),
                    EndDate = item.EndTime.ToString("yyyy-MM-dd"),
                    ShopName = obj.ShopName
                };
            }).ToList();
            return Json(new { rows = market, total = marketEntities.Total });
        }
	}
}