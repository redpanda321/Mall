using Mall.Web.Framework;
using System.Linq;

using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.CommonModel;
using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    [MarketingAuthorization]
    public class CollocationController : BaseAdminController
    {
        IMarketService _iMarketService;
        public CollocationController(IMarketService iMarketService)
        {
            _iMarketService = iMarketService;
        }

        #region 活动列表
        public ActionResult Management()
        {
           var  model = _iMarketService.GetServiceSetting(MarketType.Collocation);
            return View();
        }
        #endregion


        #region 服务购买列表
        public JsonResult List(MarketBoughtQuery query)
        {
            query.MarketType = MarketType.Collocation;
            var data = _iMarketService.GetBoughtShopList(query);

            var list = data.Models.Select(item => {
                var market = MarketApplication.GetMarketService(item.MarketServiceId);
                return new
                {
                    Id = item.Id,
                    StartDate = item.StartTime.ToString("yyyy-MM-dd"),
                    EndDate = item.EndTime.ToString("yyyy-MM-dd"),
                    ShopName = market.ShopName
                };
            }).ToList();

            return Json(new { rows = list, total = data.Total });
        }
        #endregion

        #region 服务费用设置

        public ActionResult ServiceSetting()
        {
            Entities.MarketSettingInfo model = _iMarketService.GetServiceSetting(MarketType.Collocation);
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveServiceSetting(decimal Price)
        {
            Result result = new Result();
            var model = new Entities.MarketSettingInfo { Price = Price, TypeId = MarketType.Collocation };
            _iMarketService.AddOrUpdateServiceSetting(model);
            result.success = true;
            result.msg = "保存成功！";
            return Json(result);
        }
        #endregion


    }
}