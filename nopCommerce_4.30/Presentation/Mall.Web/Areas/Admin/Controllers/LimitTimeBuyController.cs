using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    [MarketingAuthorization]
    public class LimitTimeBuyController : BaseAdminController
    {
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IMarketService _iMarketService;
        private ISlideAdsService _iSlideAdsService;

        public LimitTimeBuyController(ILimitTimeBuyService iLimitTimeBuyService, IMarketService iMarketService, ISlideAdsService iSlideAdsService)
        {
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iMarketService = iMarketService;
            _iSlideAdsService = iSlideAdsService;
        }

        #region 活动列表

        // GET: Admin/LimitTimeBuy
        public ActionResult Management(int? status)
        {
            ViewBag.status = status;
            return View();
        }

        public ActionResult Audit(long id)
        {
            var result = _iLimitTimeBuyService.Get(id);
            ViewBag.IsAudit = true;
            return View(result);
        }

        public ActionResult Detail(long id)
        {
            var result = _iLimitTimeBuyService.Get(id);
            ViewBag.IsAudit = false;
            return View(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(LimitTimeQuery query)
        {
            var result = _iLimitTimeBuyService.GetFlashSaleInfos(query);
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
               m.ProductImg = Mall.Core.MallIO.GetProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350);
               return m;
           });
            var dataGrid = new DataGridModel<FlashSaleModel>() { rows = market, total = result.Total };
            return Json(dataGrid);
        }


        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="auditState">审核状态</param>
        /// <param name="message">理由</param>
        /// <returns></returns>
        [UnAuthorize]
        [OperationLog(Message = "审核商品状态")]
        [HttpPost]
        public JsonResult AuditItem(long id)
        {

            Result result = new Result();
            try
            {
                _iLimitTimeBuyService.Pass(id);
                Cache.Remove(CacheKeyCollection.CACHE_LIMITPRODUCTS);
                var info = _iLimitTimeBuyService.GetFlashSaleInfo(id);
                ProductManagerApplication.SaveCaculateMinPrice(info.ProductId, info.ShopId);
                result.success = true;
                result.msg = "审核成功！";
            }
            catch (MallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("审核出错", ex);
                result.msg = "审核出错！";
            }
            return Json(result);
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        [UnAuthorize]
        [OperationLog(Message = "拒绝商品状态")]
        [HttpPost]
        public JsonResult RefuseItem(long id)
        {

            Result result = new Result();
            try
            {
                _iLimitTimeBuyService.Refuse(id);
                result.success = true;
                result.msg = "成功拒绝！";
            }
            catch (MallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("拒绝出错", ex);
                result.msg = "拒绝出错！";
            }
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult CancelItem(long id)
        {
            Result result = new Result();
            try
            {
                _iLimitTimeBuyService.Cancel(id);

                var info = _iLimitTimeBuyService.GetFlashSaleInfo(id);
                ProductManagerApplication.SaveCaculateMinPrice(info.ProductId, info.ShopId);
                result.success = true;
                result.msg = "取消成功！";
            }
            catch (MallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("取消出错", ex);
                result.msg = "取消出错！";
            }
            return Json(result);
        }



        public ActionResult SetSlide()
        {
            return View();
        }

        #endregion

        #region 轮播图

        public JsonResult GetSlideJson()
        {
            var data = _iSlideAdsService.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.PlatformLimitTime);
            IEnumerable<HandSlideModel> slide = data.ToArray().Select(item => new HandSlideModel()
            {
                Id = item.Id,
                Pic = Core.MallIO.GetImagePath(item.ImageUrl),
                URL = item.Url,
                Index = item.DisplaySequence
            });

            DataGridModel<HandSlideModel> dataGrid = new DataGridModel<HandSlideModel>() { rows = slide, total = slide.Count() };
            return Json(dataGrid);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult AddSlideAd(string pic, string url)
        {
            var slide = new Entities.SlideAdInfo()
            {
                ImageUrl = pic,
                Url = url,
                ShopId = 0,
                DisplaySequence = 0,
                TypeId = Entities.SlideAdInfo.SlideAdType.PlatformLimitTime
            };
            if (!string.IsNullOrWhiteSpace(pic))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp/"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = dest + Path.GetFileName(pic);
                    Core.MallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }

                slide.ImageUrl = pic;
            }
            _iSlideAdsService.AddSlidAd(slide);
            return Json(new Result { success = true });
        }

        [UnAuthorize]
        public JsonResult DeleteSlide(long Id)
        {
            _iSlideAdsService.DeleteSlidAd(0, Id);
            return Json(new Result { success = true });
        }

        [UnAuthorize]
        public JsonResult EditSlideAd(long id, string pic, string url)
        {
            var slide = _iSlideAdsService.GetSlidAd(0, id);

            if (!string.IsNullOrWhiteSpace(pic) && (!slide.ImageUrl.Equals(pic)))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp/"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = dest + Path.GetFileName(pic);
                    Core.MallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }
            }

            _iSlideAdsService.UpdateSlidAd(new Entities.SlideAdInfo
            {
                Id = id,
                ImageUrl = pic,
                Url = url
            });
            return Json(new Result { success = true });
        }

        [UnAuthorize]
        [HttpPost]
        public ActionResult AdjustSlideIndex(long id, int direction)
        {
            _iSlideAdsService.AdjustSlidAdIndex(0, id, direction == 1, Entities.SlideAdInfo.SlideAdType.PlatformLimitTime);
            return Json(new Result { success = true });
        }

        #endregion

        #region 服务购买列表

        public ActionResult BoughtList()
        {
            return View();
        }

        [UnAuthorize]
        public JsonResult GetBoughtJson(string shopName, int page, int rows)
        {
            var queryModel = new MarketBoughtQuery()
            {
                PageSize = rows,
                PageNo = page,
                ShopName = shopName,
                MarketType = MarketType.LimitTimeBuy
            };

            QueryPageModel<Entities.MarketServiceRecordInfo> marketEntities = _iMarketService.GetBoughtShopList(queryModel);

            var market = marketEntities.Models.OrderByDescending(m => m.MarketServiceId).ThenByDescending(m => m.EndTime)
                .Select(item => {
                    var obj = MarketApplication.GetMarketService(item.MarketServiceId);
                    return new
                    {
                        Id = item.Id,
                        StartDate = item.StartTime.ToString("yyyy-MM-dd"),
                        EndDate = item.EndTime.ToString("yyyy-MM-dd"),
                        ShopName = obj.ShopName
                    };
                });

            return Json(new { rows = market, total = marketEntities.Total });
        }
        #endregion

        #region 活动商品分类

        public ActionResult MarketCategory()
        {
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetMarketCategoryJson()
        {
            var service = _iLimitTimeBuyService;
            var cate = service.GetServiceCategories();
            var list = from i in cate
                       select new { Name = i, Id = 0 };
            return Json(new { rows = list, total = list.Count() });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult AddMarketCategory(string name)
        {
            Result result = new Result();
            try
            {
                var service = _iLimitTimeBuyService;
                service.AddServiceCategory(name.Replace(",", "").Replace("，", ""));
                result.success = true;
                result.msg = "添加分类成功！";
            }
            catch (MallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("添加分类出错", ex);
                result.msg = "添加分类出错！";
            }
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DeleteMarketCategory(string name)
        {
            Result result = new Result();
            try
            {
                var service = _iLimitTimeBuyService;
                service.DeleteServiceCategory(name);
                result.success = true;
                result.msg = "删除分类成功！";
            }
            catch (MallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("删除分类出错", ex);
                result.msg = "删除分类出错！";
            }
            return Json(result);
        }
        #endregion

        #region 服务费用设置

        public ActionResult ServiceSetting()
        {
            LimitTimeBuySettingModel model = _iLimitTimeBuyService.GetServiceSetting();
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveServiceSetting(decimal Price, int ReviceDays = 0)
        {
            Result result = new Result();
            try
            {
                var model = new LimitTimeBuySettingModel { Price = Price, ReviceDays = ReviceDays };
                _iLimitTimeBuyService.UpdateServiceSetting(model);
                result.success = true;
                result.msg = "保存成功！";
            }
            catch (MallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("保存出错", ex);
                result.msg = "保存出错！";
            }
            return Json(result);
        }
        #endregion

        #region  活动参数

        public ActionResult ConfigSetting()
        {
            ViewBag.LimitTimeBuyNeedAuditing = SiteSettings.LimitTimeBuyNeedAuditing;
            return View(_iLimitTimeBuyService.GetConfig());
        }

        public ActionResult SetConfig(FlashSaleConfigModel data)
        {
            _iLimitTimeBuyService.UpdateConfig(data);
            var isneedaudit = bool.Parse(Request.Form["isneedaudit"]);
            var setting = SiteSettingApplication.SiteSettings;
            setting.LimitTimeBuyNeedAuditing = isneedaudit;
            SiteSettingApplication.SaveChanges();
            Result result = new Result { success = true };
            return Json(result);
        }

        #endregion
    }
}