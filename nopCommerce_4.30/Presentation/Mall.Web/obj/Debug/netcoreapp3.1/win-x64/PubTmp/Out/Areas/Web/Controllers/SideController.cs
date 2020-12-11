using Mall.Application;
using Mall.CommonModel;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class SideController : BaseWebController
    {
        private IMemberIntegralService _iMemberIntegralService;
        private IProductService _iProductService;
        private IShopBonusService _iShopBonusService;
        private ICouponService _iCouponService;
        public SideController(IMemberIntegralService iMemberIntegralService,
            IProductService iProductService,
            IShopBonusService iShopBonusService,
            ICouponService iCouponService
            )
        {

            _iMemberIntegralService = iMemberIntegralService;
            _iProductService = iProductService;
            _iShopBonusService = iShopBonusService;
            _iCouponService = iCouponService;
        }

        /// <summary>
        /// 侧边我的资产
        /// </summary>
        /// <returns></returns>
        public ActionResult MyAsset()
        {
            MyAssetViewModel result = new Models.MyAssetViewModel();
            result.MyCouponCount = 0;
            result.isLogin = CurrentUser != null;
            ViewBag.isLogin = result.isLogin ? "true" : "false";
            //用户积分
            result.MyMemberIntegral = result.isLogin ? MemberIntegralApplication.GetAvailableIntegral(CurrentUser.Id) : 0;
            //关注商品
            result.MyConcernsProducts = result.isLogin ? _iProductService.GetUserAllConcern(CurrentUser.Id,10) : new List<Entities.FavoriteInfo>();
            //优惠卷
            var coupons = result.isLogin ? _iCouponService.GetAllUserCoupon(CurrentUser.Id).ToList() : new List<UserCouponInfo>();
            coupons = coupons == null ? new List<UserCouponInfo>() : coupons;
            result.MyCoupons = coupons;
            result.MyCouponCount += result.MyCoupons.Count();

            //红包
            result.MyShopBonus = ShopBonusApplication.GetShopBounsByUser(CurrentUser.Id); 
            result.MyCouponCount += result.MyShopBonus.Count();

            //浏览的商品
            var browsingPro = result.isLogin ? BrowseHistrory.GetBrowsingProducts(10, CurrentUser == null ? 0 : CurrentUser.Id) : new List<ProductBrowsedHistoryModel>();
            result.MyBrowsingProducts = browsingPro;
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(result);
        }
    }
}