using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;

using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.IO;
using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [H5AuthorizationAttribute]
    public class VShopController : BaseSellerController
    {
        private IVShopService _iVShopService;
        private ISlideAdsService _iSlideAdsService;
        private INavigationService _iNavigationService;
        private ICouponService _iCouponService;
        private ITemplateSettingsService _iTemplateSettingsService;

        public VShopController(IVShopService iVShopService,
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
        }

        // GET: SellerAdmin/Weixin
        public ActionResult Management()
        {
            var vshop = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId);
            string qrCodeImagePath = string.Empty;
            if (vshop != null)
            {
                string vshopUrl = CurrentUrlHelper.CurrentUrlNoPort() + "/m-" + PlatformType.WeiXin.ToString() + "/vshop/detail/" + vshop.Id;

                qrCodeImagePath = CurrentUrlHelper.CurrentUrlNoPort() + GenerateQR(vshopUrl, vshop.Id, vshop.StrLogo);
            }
            ViewBag.QRCode = qrCodeImagePath;
            ViewBag.ShopId = CurrentSellerManager.ShopId;
            ViewBag.Shop = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            return View(vshop);
        }

        public JsonResult SetVshopOpen()
        {
            var vshop = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId);
            bool isOpen = false;
            if (vshop != null)
            {
                vshop.IsOpen = isOpen = !vshop.IsOpen;
                _iVShopService.SetVShopIsOpen(vshop.Id, vshop.IsOpen);
            }
            return Json(new { success = true, isOpen = isOpen });
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        private string GenerateQR(string path, long shopId, string logo)
        {
            string fileName = shopId + "_QRCode.jpg";
            try
            {
                Bitmap map;
                if (!string.IsNullOrWhiteSpace(logo) && MallIO.ExistFile(logo))
                    map = Core.Helper.QRCodeHelper.Create(path, MallIO.GetImagePath(logo));
                else
                    map = Core.Helper.QRCodeHelper.Create(path);
                string fileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Shop", "VShop");
                string fileFullPath = Path.Combine(fileFolderPath, fileName);
                if (!Directory.Exists(fileFolderPath))
                {
                    Directory.CreateDirectory(fileFolderPath);
                }
                map.Save(fileFullPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return "/Storage/Shop/VShop/" + fileName;
        }

        public ActionResult EditVShop(long shopId)
        {
            if (shopId != CurrentSellerManager.ShopId)
            {
                throw new MallException("获取店铺信息错误");
            }
            var vshop = _iVShopService.GetVShopByShopId(shopId);
            if (vshop == null)
                vshop = new Entities.VShopInfo();
            else
            {
                vshop.StrLogo = Core.MallIO.GetRomoteImagePath(vshop.StrLogo);
                vshop.StrBackgroundImage = Core.MallIO.GetRomoteImagePath(vshop.StrBackgroundImage);
                vshop.WXLogo = Core.MallIO.GetRomoteImagePath(vshop.WXLogo);
            }
            ViewBag.ShopId = CurrentSellerManager.ShopId;
            return View(vshop);
        }

        [HttpPost]
        public JsonResult EditVShop(Entities.VShopInfo vshopInfo)
        {
            if (vshopInfo.Id > 0)
            {
                var vshop = _iVShopService.GetVShop(vshopInfo.Id);
                vshop.StrLogo = vshopInfo.StrLogo;
                vshop.WXLogo = vshopInfo.WXLogo;
                vshop.StrBackgroundImage = vshopInfo.StrBackgroundImage;
                vshop.Tags = vshopInfo.Tags;

                _iVShopService.UpdateVShop(vshop);
            }
            else
                _iVShopService.CreateVshop(vshopInfo);

            return Json(new { success = true });
        }

        public JsonResult EditSlideImage(Entities.SlideAdInfo slideAdInfo)
        {
            slideAdInfo.TypeId = Entities.SlideAdInfo.SlideAdType.VShopHome;
            slideAdInfo.ShopId = CurrentSellerManager.ShopId;
            if (slideAdInfo.Id > 0)
                _iSlideAdsService.UpdateSlidAd(slideAdInfo);
            else
                _iSlideAdsService.AddSlidAd(slideAdInfo);
            return Json(new { success = true });
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