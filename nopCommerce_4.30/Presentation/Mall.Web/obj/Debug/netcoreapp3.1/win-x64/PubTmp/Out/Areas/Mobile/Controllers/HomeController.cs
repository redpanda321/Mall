using Mall.Application;
using Mall.CommonModel;
using Mall.CommonModel;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class HomeController : BaseMobileTemplatesController
    {
        private ITemplateSettingsService _iTemplateSettingsService;
        private ICustomerService _iCustomerService;

        public HomeController(ITemplateSettingsService iTemplateSettingsService, ICustomerService iCustomerService)
        {
            _iTemplateSettingsService = iTemplateSettingsService;
            _iCustomerService = iCustomerService;
        }

        // GET: Mobile/Home
        public ActionResult Index(int ispv = 0, string tn = "")
        {
            string crrentTemplateName = "t1";
            var curr = _iTemplateSettingsService.GetCurrentTemplate(0);
            if (null != curr)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }
            if (ispv == 1)
            {
                if (!string.IsNullOrWhiteSpace(tn))
                {
                    crrentTemplateName = tn;
                }
            }
            ViewBag.Title = SiteSettings.SiteName + "首页";
            ViewBag.FootIndex = 0;

            var services = CustomerServiceApplication.GetPlatformCustomerService(true, true);

            ViewBag.CustomerServices = services;
            VTemplateHelper.DownloadTemplate(crrentTemplateName, VTemplateClientTypes.WapIndex, 0);
            return View(string.Format("~/Areas/Admin/Templates/vshop/{0}/Skin-HomePage.cshtml", crrentTemplateName));
        }
        //TODO:DZY[171121]前台没看到调用代码
        public JsonResult LoadProducts(int page, int pageSize)
        {
            var homeProducts = ServiceApplication.Create<IMobileHomeProductsService>().GetMobileHomeProducts(0, Core.PlatformType.WeiXin, page, pageSize);
            var products = ProductManagerApplication.GetProducts(homeProducts.Models.Select(p => p.ProductId));
            var model = products.Select(item => new
            {
                name = item.ProductName,
                id = item.Id,
                image = item.GetImage(ImageSize.Size_350),
                price = item.MinSalePrice,
                marketPrice = item.MarketPrice
            });
            return Json(model);
        }
        //TODO:DZY[171122]前台调用被注释，暂不修改
        public JsonResult LoadProductsFromCache(int page)
        {
            var html = TemplateSettingsApplication.GetGoodTagFromCache(page);
            return Json(new { htmlTag = html });
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult DownLoadApp()
        {
            if (PlatformType == Core.PlatformType.WeiXin)
                return RedirectToAction("WeiXinDownLoad");
            if (visitorTerminalInfo.OperaSystem == EnumVisitorOperaSystem.Android)
                return RedirectToAction("AndriodDownLoad");
            if (visitorTerminalInfo.OperaSystem == EnumVisitorOperaSystem.IOS)
                return RedirectToAction("IOSDownLoad");
            return View();
        }


        public ActionResult WeiXinDownLoad()
        {
            if (PlatformType == Core.PlatformType.WeiXin)
                return View();
            if (visitorTerminalInfo.OperaSystem == EnumVisitorOperaSystem.Android)
                return RedirectToAction("AndriodDownLoad");
            if (visitorTerminalInfo.OperaSystem == EnumVisitorOperaSystem.IOS)
                return RedirectToAction("IOSDownLoad");
            return View();
        }


        public ActionResult AndriodDownLoad()
        {
            var DownLoadApk = SiteSettingApplication.SiteSettings.AndriodDownLoad;
            if (!string.IsNullOrEmpty(DownLoadApk))
            {
                ViewBag.DownLoadApk = DownLoadApk;
                return View();
            }
            return RedirectToAction("DownLoadError");
        }

        public ActionResult IOSDownLoad()
        {
            var DownLoadApk = SiteSettingApplication.SiteSettings.IOSDownLoad;
            if (!string.IsNullOrEmpty(DownLoadApk))
            {
                return Redirect(DownLoadApk);
            }
            return RedirectToAction("DownLoadError");
        }

        public ActionResult DownLoadError()
        {
            return View();
        }

        /// <summary>
        /// 获取分享内容
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetShare(string url)
        {
            var shareArgs = Mall.Application.WXApiApplication.GetWeiXinShareArgs(url);
            var siteSetting = SiteSettingApplication.SiteSettings;
            var shareTitle = string.Empty;
            if (siteSetting != null && !string.IsNullOrEmpty(siteSetting.SiteName))
            {
                shareTitle = siteSetting.SiteName;
            }
            var result = new
            {
                AppId = shareArgs.AppId,
                Timestamp = shareArgs.Timestamp,
                NonceStr = shareArgs.NonceStr,
                Signature = shareArgs.Signature,
                ShareIcon = Core.MallIO.GetRomoteImagePath(SiteSettingApplication.SiteSettings.WXLogo),
                ShareTitle = shareTitle
            };

            return Json(new { success = true, data = result }, true);
        }
        /// <summary>
        /// 获取模板节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTemplateItem(string id, string tn = "")
        {
            string result = "";
            if (string.IsNullOrWhiteSpace(tn))
            {
                tn = "t1";
                var curr = _iTemplateSettingsService.GetCurrentTemplate(0);
                if (null != curr)
                {
                    tn = curr.CurrentTemplateName;
                }
            }
            result = VTemplateHelper.GetTemplateItemById(id, tn, VTemplateClientTypes.WapIndex);
            return result;
        }
        /// <summary>
        /// 分销头部
        /// </summary>
        /// <returns></returns>
        public ActionResult DistributionHeader()
        {
            Mall.Entities.DistributorInfo result = null;
            if (CurrentSpreadId.HasValue &&CurrentSpreadId > 0)
            {
                result = DistributionApplication.GetDistributor(CurrentSpreadId.Value);
                if (result != null && result.IsNormalDistributor)
                {
                    result.ShopLogo = Mall.Core.MallIO.GetRomoteImagePath(result.ShopLogo);
                }
                else
                {
                    result = null;
                }
            }
            ViewBag.NeedDistributionWeiXinShare = NeedDistributionWeiXinShare;
            return View(result);
        }

        /// <summary>
        /// 获取是否开启了分销和当前是否为分销
        /// </summary>
        /// <returns></returns>
        public JsonResult GetIsShowDistributionHead()
        {
            var isShowDistributionHead = (SiteSettingApplication.SiteSettings.DistributionIsEnable && CurrentSpreadId > 0);
            var result = DistributionApplication.GetDistributor(CurrentSpreadId.Value);
            isShowDistributionHead = isShowDistributionHead && result.IsShowShopLogo;
            return Json(new { success = true, isShowDistributionHead }, true);
        }
    }
}
