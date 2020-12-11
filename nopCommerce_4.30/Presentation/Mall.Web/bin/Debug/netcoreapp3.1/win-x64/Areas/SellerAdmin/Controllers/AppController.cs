using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;

using Mall.Web.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [APPAuthorizationAttribute]
    public class AppController : BaseSellerController
    {
        private IVShopService _iVShopService;
        private ISlideAdsService _iSlideAdsService;
        private INavigationService _iNavigationService;
        private ICouponService _iCouponService;
        private ITemplateSettingsService _iTemplateSettingsService;
        MobileHomeProducts mobileHomeproduct;

        public AppController(IVShopService iVShopService,
             ISlideAdsService iSlideAdsService,
            INavigationService iNavigationService,
            ICouponService iCouponService,
            ITemplateSettingsService iTemplateSettingsService
       )
        {
            _iVShopService = iVShopService;
            _iSlideAdsService = iSlideAdsService;
            _iNavigationService = iNavigationService;
            _iCouponService = iCouponService;
            _iTemplateSettingsService = iTemplateSettingsService;
            mobileHomeproduct = new MobileHomeProducts();
        }

        public ActionResult VshopHomeSiteApp()
        {
            Models.VshopHomeSiteViewModel model = new Models.VshopHomeSiteViewModel();
            //未开通微店就进不去首页设置 
            var vshop = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId);
            model.VShop = vshop;
            model.ShopId = CurrentSellerManager.ShopId;
            model.SlideImage = _iSlideAdsService.GetSlidAds(CurrentSellerManager.ShopId, Entities.SlideAdInfo.SlideAdType.VShopHome).ToList();
            model.Banner = _iNavigationService.GetSellerNavigations(CurrentSellerManager.ShopId, PlatformType.WeiXin).ToList();
            return View(model);
        }

        [HttpPost]
        public JsonResult GetMobileHomeProducts(PlatformType platformType, int page, int rows, string brandName, string productName, long? categoryId = null)
        {
            object model = mobileHomeproduct.GetSellerMobileHomePageProducts(CurrentSellerManager.ShopId, platformType, page, rows, brandName, categoryId);
            return Json(model);
        }

        [HttpPost]
        public JsonResult AddHomeProducts(string productIds, PlatformType platformType)
        {
            if (string.IsNullOrEmpty(productIds))
            {
                mobileHomeproduct.DeleteAll(CurrentSellerManager.ShopId);
                return Json(new { success = true });
            }
            mobileHomeproduct.AddHomeProducts(CurrentSellerManager.ShopId, productIds, platformType);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult UpdateSequence(long id, short sequence)
        {
            mobileHomeproduct.UpdateSequence(CurrentSellerManager.ShopId, id, sequence);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult Delete(long id)
        {
            mobileHomeproduct.Delete(CurrentSellerManager.ShopId, id);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult GetAllHomeProductIds(PlatformType platformType)
        {
            var homeProductIds = mobileHomeproduct.GetAllHomeProductIds(CurrentSellerManager.ShopId, platformType);
            return Json(homeProductIds);
        }

        public JsonResult GetSlideImage()
        {
            var slideAds = _iSlideAdsService.GetImageAds(CurrentSellerManager.ShopId);
            var slideModel = slideAds.ToArray().Select(item => new
            {
                id = item.Id,
                image = item.ImageUrl,
                url = item.Url
            });
            return Json(new { rows = slideModel, total = 100 });
        }

        [HttpPost]
        public JsonResult SlideImageChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(CurrentSellerManager.ShopId, oriRowNumber, newRowNumber, Entities.SlideAdInfo.SlideAdType.VShopHome);
            return Json(new { success = true });
        }

        public JsonResult DeleteSlideImage(string id)
        {
            _iSlideAdsService.DeleteSlidAd(CurrentSellerManager.ShopId, Convert.ToInt64(id));
            return Json(new { success = true });
        }

        /// <summary>
        /// 店铺优惠卷列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="couponName"></param>
        /// <returns></returns>
        public JsonResult GetCouponList(int page, int rows, string couponName)
        {
            var service = _iCouponService;
            var result = service.GetCouponList(new CouponQuery { CouponName = couponName, ShopId = CurrentSellerManager.ShopId, PageSize = rows, PageNo = page });
            var couponIdList = _iVShopService.GetVShopCouponSetting(CurrentSellerManager.ShopId).Select(item => item.CouponID);
            var list = result.Models.ToArray().Select(
                item => new
                {
                    Id = item.Id,
                    StartTime = item.StartTime.ToString("yyyy-MM-dd"),
                    EndTime = item.EndTime.ToString("yyyy-MM-dd"),
                    Price = Math.Round(item.Price, 2),
                    CouponName = item.CouponName,
                    PerMax = item.PerMax == 0 ? "不限张" : item.PerMax.ToString() + "张/人",
                    OrderAmount = item.OrderAmount == 0 ? "不限制" : "满" + item.OrderAmount,
                    IsSelect = couponIdList.Contains(item.Id)//是否已选择
                }
                );
            var model = new { rows = list, total = result.Total };
            return Json(model);
        }

        public JsonResult SaveGouponSetting(string ids, string values)
        {
            var valueArray = values.Split(',');
            var idArray = ids.Split(',');
            var list = new List<Entities.CouponSettingInfo>();

            for (int i = 0; i < valueArray.Length; i++)
            {
                list.Add(new Entities.CouponSettingInfo()
                {
                    Display = string.IsNullOrEmpty(valueArray[i]) ? 0 : int.Parse(valueArray[i]),
                    CouponID = long.Parse(idArray[i]),
                    PlatForm = PlatformType.Mobile
                });
            }
            _iVShopService.SaveVShopCouponSetting(list);
            return Json(new { success = true });
        }

        public JsonResult GetSlideImages()
        {
            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(CurrentSellerManager.ShopId, Entities.SlideAdInfo.SlideAdType.VShopHome).ToArray();
            var slideImageService = _iSlideAdsService;
            var slideModel = slideImageSettings.Select(item =>
            {
                var slideImage = slideImageService.GetSlidAd(CurrentSellerManager.ShopId, item.Id);
                return new
                {
                    id = item.Id,
                    image = MallIO.GetImagePath(item.ImageUrl),
                    displaySequence = (item.DisplaySequence == 0) ? "0" : item.DisplaySequence.ToString(),
                    url = item.Url
                };
            });
            return Json(new { rows = slideModel, total = 100 });
        }


        [HttpPost]
        public JsonResult GetSlideImage(long? id)
        {
            Entities.SlideAdInfo slideImageIfo;
            if (id.HasValue)
            {
                slideImageIfo = _iSlideAdsService.GetSlidAd(CurrentSellerManager.ShopId, id.Value);
                slideImageIfo.ImageUrl = MallIO.GetImagePath(slideImageIfo.ImageUrl);
            }
            else
                slideImageIfo = new Entities.SlideAdInfo();
            return Json(new { success = true, item = slideImageIfo });
        }


        public JsonResult SaveSlideImage(long? id, string imageUrl, string url)
        {
            Entities.SlideAdInfo slideImageIfo = new Entities.SlideAdInfo();
            slideImageIfo.ImageUrl = imageUrl;
            slideImageIfo.Url = url;
            slideImageIfo.ShopId = CurrentSellerManager.ShopId;
            slideImageIfo.TypeId = Entities.SlideAdInfo.SlideAdType.VShopHome;
            if (id.HasValue)
            {
                slideImageIfo.Id = id.Value;
                _iSlideAdsService.UpdateSlidAd(slideImageIfo);
            }
            else
            {
                _iSlideAdsService.AddSlidAd(slideImageIfo);
            }
            return Json(new { success = true });
        }

        public JsonResult SaveVShopHomePageTitle(string homePageTitle)
        {
            var vshop = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId);
            vshop.HomePageTitle = homePageTitle;
            _iVShopService.UpdateVShop(vshop);
            return Json(new { success = true });
        }

        public JsonResult GetVShopBanners()
        {
            var vshopBanner = _iNavigationService.GetSellerNavigations(CurrentSellerManager.ShopId, PlatformType.WeiXin).ToArray();
            var vshopBannerModel = vshopBanner.Select(item =>
            {
                return new
                {
                    id = item.Id,
                    name = item.Name,
                    url = item.UrlType.ToDescription() + ' ' + item.Url,
                    displaySequence = item.DisplaySequence
                };
            });
            return Json(new { rows = vshopBannerModel, tota = 100 });
        }

        public JsonResult GetVShopBanner(long? id)
        {
            Entities.BannerInfo banner = new Entities.BannerInfo();
            if (id.HasValue)
                banner = _iNavigationService.GetSellerNavigation(id.Value);
            return Json(new { success = true, item = banner });
        }

        public JsonResult SaveVShopBanner(long? id, string bannerName, string url, int urlType)
        {
            var vshop = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId);
            switch (urlType)
            {
                case 1:
                    url = "/m-" + PlatformType.WeiXin.ToString() + "/vshop/Search?vshopid=" + vshop.Id;
                    break;
                case 2:
                    url = "/m-" + PlatformType.WeiXin.ToString() + "/vshop/Category?vshopid=" + vshop.Id;
                    break;
                case 3:
                    url = "/m-" + PlatformType.WeiXin.ToString() + "/vshop/introduce/" + vshop.Id;
                    break;
                default:
                    break;
            }

            Entities.BannerInfo banner = new Entities.BannerInfo();
            banner.Name = bannerName;
            banner.Platform = PlatformType.WeiXin;
            banner.ShopId = CurrentSellerManager.ShopId;
            banner.Url = url;
            banner.Position = 0;
            banner.UrlType = (Entities.BannerInfo.BannerUrltypes)urlType;
            if (id.HasValue)
            {
                banner.Id = id.Value;
                _iNavigationService.UpdateSellerNavigation(banner);
            }
            else
                _iNavigationService.AddSellerNavigation(banner);
            return Json(new { success = true, item = banner });
        }

        public JsonResult BannerChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iNavigationService.SwapSellerDisplaySequence(CurrentSellerManager.ShopId, oriRowNumber, newRowNumber);
            return Json(new { success = true });
        }

        public JsonResult DeleteVShopBanner(long id)
        {
            _iNavigationService.DeleteSellerformNavigation(CurrentSellerManager.ShopId, id);
            return Json(new { success = true });
        }
    }
}