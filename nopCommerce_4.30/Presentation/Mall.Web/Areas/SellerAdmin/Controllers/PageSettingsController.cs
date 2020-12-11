using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [PCAuthorizationAttribute]
    public class PageSettingsController : BaseSellerController
    {
        private ISlideAdsService _iSlideAdsService;
        private IShopService _iShopService;
        public PageSettingsController(ISlideAdsService iSlideAdsService, IShopService iShopService)
        {
            _iSlideAdsService = iSlideAdsService;
            _iShopService = iShopService;
        }
        // GET: SellerAdmin/PageSettings
        public ActionResult Management()
        {
            var images = _iSlideAdsService.GetImageAds(CurrentSellerManager.ShopId).OrderBy(item => item.Id);
            var shop = _iShopService.GetShop(CurrentSellerManager.ShopId);
            ViewBag.Logo = shop.Logo;
            ViewBag.Logo = ViewBag.Logo == null ? "" : ViewBag.Logo;
            ViewBag.TransverseAD = images.FirstOrDefault(p => p.IsTransverseAD);
            ViewBag.SlidAds = SlideApplication.GetSlidAds(CurrentSellerManager.ShopId, Entities.SlideAdInfo.SlideAdType.ShopHome);
            ViewBag.IsOpenTopImageAd =  shop.IsOpenTopImageAd;
            return View(images.Where(p => !p.IsTransverseAD));
        }

        #region 普通广告图片

        [HttpPost]
        [UnAuthorize]
        public ActionResult UpdateImageAd(long id, string pic, string url, int? openTopImageAd)
        {
            var image = _iSlideAdsService.GetImageAd(CurrentSellerManager.ShopId, id);
            if (url.IndexOf("javascript:") > -1)
            {
                return Json(new { success = false, msg = "错误的跳转地址" });
            }

            if (!string.IsNullOrWhiteSpace(pic))
            {
                string dest = string.Format(@"/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);

                if (pic.Contains("/temp/"))
                {
                    var d = pic.Substring(pic.LastIndexOf("/temp/"));

                    var destimg = Path.Combine(dest, Path.GetFileName(pic));
                    Core.MallIO.CopyFile(d, destimg, true);
                    pic = destimg;
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }
                else
                {
                    pic = "";
                }

            }
            var imageAd = new Entities.ImageAdInfo { ShopId = CurrentSellerManager.ShopId, Url = url, ImageUrl = pic, Id = id };
            _iSlideAdsService.UpdateImageAd(imageAd);
            pic = Mall.Core.MallIO.GetImagePath(pic);
            if (openTopImageAd != null)
            {
                ShopApplication.UpdateOpenTopImageAd(CurrentSellerManager.ShopId, openTopImageAd.Value == 1);
            }
            ClearCache();
            return Json(new { success = true, imageUrl = pic });
        }

        #endregion

        public void ClearCache()
        {
          //  HttpResponse.RemoveOutputCacheItem(string.Format("/Shop/Home/{0}", this.CurrentShop.Id)); //移除页面缓存
           // HttpResponse.RemoveOutputCacheItem(string.Format("/shop/home/{0}", this.CurrentShop.Id));
        }

        public ActionResult SlideAds()
        {
            var slides = _iSlideAdsService.GetSlidAds(CurrentSellerManager.ShopId, Entities.SlideAdInfo.SlideAdType.ShopHome);
            return View(slides);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveSlideAd(long? id, string pic, string url)
        {
            var slide = new Entities.SlideAdInfo()
            {
                ImageUrl = pic,
                Url = url,
                ShopId = CurrentSellerManager.ShopId,
                DisplaySequence = 0,
                Id = id.GetValueOrDefault(),
                TypeId = Entities.SlideAdInfo.SlideAdType.ShopHome
            };
            if (!string.IsNullOrWhiteSpace(pic) && pic.Contains("/temp/"))
            {
                string dest = string.Format(@"/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);
                var destimg = slide.ImageUrl = Path.Combine(dest, Path.GetFileName(pic));
                var source = pic.Substring(pic.LastIndexOf("/temp/"));
                Core.MallIO.CopyFile(source, destimg, true);
                slide.ImageUrl = destimg;
            }
            else if (pic.Contains("/Storage/"))
            {
                slide.ImageUrl = pic.Substring(pic.LastIndexOf("/Storage/"));
            }
            else
            {
                slide.ImageUrl = "";
            }
            if (id.HasValue)
                _iSlideAdsService.UpdateSlidAd(slide);
            else
                _iSlideAdsService.AddSlidAd(slide);

            ClearCache();
            return Json(new { success = true, imageUrl = slide.ImageUrl });
        }


        [HttpPost]
        [UnAuthorize]
        public ActionResult AdjustSlideIndex(long id, int direction)
        {
            _iSlideAdsService.AdjustSlidAdIndex(CurrentSellerManager.ShopId, id, direction == 1, Entities.SlideAdInfo.SlideAdType.ShopHome);
            ClearCache();
            return Json(new { success = true });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult DeleteSlide(long Id)
        {
            _iSlideAdsService.DeleteSlidAd(CurrentSellerManager.ShopId, Id);
            ClearCache();
            return Json(new { success = true });
        }


        #region LOGO图片设置

        [HttpPost]
        [UnAuthorize]
        public JsonResult SetLogo(string logo)
        {
            var img = MoveImages(logo);
            _iShopService.UpdateLogo(CurrentSellerManager.ShopId, img);
            ClearCache();
            return Json(new { success = true, logo = Mall.Core.MallIO.GetImagePath(img) });
        }


        /// <summary>
        /// 转移LOGO图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            string ImageDir = string.Empty;
            var dir = string.Format("/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var source = image.Substring(image.LastIndexOf("/temp/"));
                Core.MallIO.CopyFile(source, dir + "logo.png", true);
            }  //目标地址
            return dir + "logo.png";
        }
        #endregion


        [HttpPost]
        [UnAuthorize]
        public JsonResult GetlimitTimeProducts(LimitTimeQuery query)
        {
            query.ShopId = this.CurrentShop.Id;//只取当前商家的限时购商品
           // var result =  EngineContext.Current.Resolve<ILimitTimeBuyService>().GetFlashSaleInfos(query);

            var result =  EngineContext.Current.Resolve<ILimitTimeBuyService>().GetFlashSaleInfos(query);

            var products = ProductManagerApplication.GetProducts(result.Models.Select(p => p.ProductId));
            var shops = ShopApplication.GetShops(result.Models.Select(p => p.ShopId));
            var market = result.Models.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                var shop = shops.FirstOrDefault(p => p.Id == item.ShopId);
                var m = new FlashSaleModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    BeginDate = item.BeginDate.ToString("yyyy-MM-dd"),
                    EndDate = item.EndDate.ToString("yyyy-MM-dd"),
                    ShopName = shop.ShopName,
                    ProductName = product.ProductName,
                    ProductId = item.ProductId,
                    StatusStr = item.Status.ToDescription()
                };
                if (item.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && item.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && item.BeginDate > DateTime.Now && item.EndDate < DateTime.Now)
                {
                    m.StatusStr = "进行中";
                }
                else if (item.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && item.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && item.BeginDate > DateTime.Now)
                {
                    m.StatusStr = "未开始";
                }
                m.SaleCount = item.SaleCount;
                m.MinPrice = item.MinPrice;
                m.MarketPrice = product.MarketPrice;
                m.ProductImg =MallIO.GetProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350);
                return m;
            });
            var dataGrid = new DataGridModel<FlashSaleModel>() { rows = market, total = result.Total };
            return Json(dataGrid);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetCoupons(int page = 1, string title = "")
        {
            long shopId = CurrentSellerManager.ShopId;
                var couponsList =  EngineContext.Current.Resolve<ICouponService>().GetCouponList(new


           Mall.DTO.QueryModel.CouponQuery
            {
                CouponName = title,
                ShopId = shopId,
                IsOnlyShowNormal = true,
                IsShowAll = false,
                ShowPlatform = Mall.Core.PlatformType.PC,
                PageNo = page,
                PageSize = 10
            });
            var resulst = couponsList.Models.ToArray().Select(a =>
            {
                var m = new CouponsContent
                {
                    create_time = a.CreateTime.ToString(),
                    game_id = a.Id,
                    link = "/m-wap/vshop/CouponInfo/" + a.Id,
                    type = 1,
                    title = a.CouponName,
                    condition = a.OrderAmount.ToString(),
                    endTime = a.EndTime.ToShortDateString(),
                    price = a.Price.ToString(),
                    shopName = a.ShopName
                };
                return m;
            });
            var dataGrid = new DataGridModel<CouponsContent>() { rows = resulst, total = couponsList.Total };
            return Json(dataGrid);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveSlideAds(List<SlideAdsModel> slideads)
        {
            //每次都重新保存序号
            if (slideads != null && slideads.Count > 0)
            {
                List<SlideAdInfo> list = new List<SlideAdInfo>();
                foreach (var item in slideads)
                {
                    var slide = new Entities.SlideAdInfo()
                    {
                        ImageUrl = item.Pic,
                        Url = item.Url,
                        ShopId = CurrentSellerManager.ShopId,
                        DisplaySequence = slideads.IndexOf(item) + 1,
                        Id = item.Id,
                        TypeId = Entities.SlideAdInfo.SlideAdType.ShopHome
                    };
                    if (!string.IsNullOrWhiteSpace(item.Pic) && item.Pic.Contains("/temp/"))
                    {
                        string dest = string.Format(@"/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);
                        var destimg = slide.ImageUrl = Path.Combine(dest, Path.GetFileName(item.Pic));
                        var source = item.Pic.Substring(item.Pic.LastIndexOf("/temp/"));
                        Core.MallIO.CopyFile(source, destimg, true);
                        slide.ImageUrl = destimg;
                    }
                    else if (item.Pic.Contains("/Storage/"))
                    {
                        slide.ImageUrl = item.Pic.Substring(item.Pic.LastIndexOf("/Storage/"));
                    }
                    else
                    {
                        slide.ImageUrl = "";
                    }
                    list.Add(slide);
                }
                SlideApplication.BatchAddShopHome(list, CurrentSellerManager.ShopId);
                ClearCache();
            }
            return Json(new { success = true });
        }

        public ActionResult Category()
        {
            var firstLevel = ShopCategoryApplication.GetMainCategory(CurrentSellerManager.ShopId);
            List<ShopCategoryModel> list = new List<ShopCategoryModel>();
            foreach (var item in firstLevel)
            {
                list.Add(new ShopCategoryModel(item));
            }
            return View(list);
        }
    }
}