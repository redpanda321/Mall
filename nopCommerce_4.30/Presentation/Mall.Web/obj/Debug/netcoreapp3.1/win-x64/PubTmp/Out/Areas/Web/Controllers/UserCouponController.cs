using Mall.Application;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Linq;



namespace Mall.Web.Areas.Web.Controllers
{
    public class UserCouponController : BaseMemberController
    {
        private IShopBonusService _iShopBonusService;
        private ICouponService _iCouponService;
        public UserCouponController(ICouponService iCouponService, IShopBonusService iShopBonusService)
        {
            _iCouponService = iCouponService;
            _iShopBonusService = iShopBonusService;
        }
        public ActionResult Index(int? status, int pageSize = 10, int pageNo = 1)
        {
            if (!status.HasValue)
            {
                status = 0;
            }

            CouponRecordQuery query = new CouponRecordQuery();
            query.UserId = CurrentUser.Id;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = status;
            var model = _iCouponService.GetCouponRecordList(query);
            var coupons = _iCouponService.GetCouponInfo(model.Models.Select(p => p.CouponId).ToArray());
            var shopBonus = ShopBonusApplication.GetShopBouns(query);


            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total > shopBonus.Total ? model.Total : shopBonus.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.Bonus = shopBonus.Models;
            ViewBag.State = query.Status;
            ViewBag.Coupons = coupons;
            #endregion
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(model.Models.ToList());
        }
    }
}