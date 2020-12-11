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
    public class CouponController : BaseAdminController
    {
        IMarketService _iMarketService;

        public CouponController(IMarketService iMarketService)
        {
            _iMarketService = iMarketService;
        }

        #region 活动列表
        public ActionResult Management()
        {
            return View();
        }
        #endregion


        #region 服务购买列表

        [UnAuthorize]
        public JsonResult List(MarketBoughtQuery query)
        {
            query.MarketType = MarketType.Coupon;
            var result = _iMarketService.GetBoughtShopList(query);
            var list = result.Models.Select(item =>
            {
                var market = MarketApplication.GetMarketService(item.MarketServiceId);
                return new
                {
                    Id = item.Id,
                    StartDate = item.StartTime.ToString("yyyy-MM-dd"),
                    EndDate = item.EndTime.ToString("yyyy-MM-dd"),
                    ShopName = market.ShopName
                };
            }).ToList();
            return Json(new { rows = list, total = result.Total });
        }
        #endregion

        #region 服务费用设置

        public ActionResult ServiceSetting()
        {
            Entities.MarketSettingInfo model =  _iMarketService.GetServiceSetting(MarketType.Coupon);
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveServiceSetting(decimal Price)
        {
            Result result = new Result();
            var model = new Entities.MarketSettingInfo { Price = Price, TypeId = MarketType.Coupon };
             _iMarketService.AddOrUpdateServiceSetting(model);
            result.success = true;
            result.msg = "保存成功！";
            return Json(result);
        }
        #endregion


    }
}