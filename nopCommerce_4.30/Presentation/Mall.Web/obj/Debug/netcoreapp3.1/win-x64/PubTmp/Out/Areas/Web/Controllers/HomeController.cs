using Mall.Application;
using Mall.Core;
using Mall.Core.Helper;
using Mall.Core.Plugins.OAuth;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;


namespace Mall.Web.Areas.Web.Controllers
{
    public class HomeController : BaseWebController
    {
        private IMemberService _iMemberService;
        private ISlideAdsService _iSlideAdsService;
        private IFloorService _iFloorService;
        private IArticleCategoryService _iArticleCategoryService;
        private IArticleService _iArticleService;
        private IBrandService _iBrandService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IShopBonusService _iShopBonusService;
        const string _themesettings = "Areas/Admin/Views/PageSettings/themesetting.json";
        const string _templatesettings = "Areas/Admin/Views/PageSettings/templatesettings.json";

        private IConfiguration _configuration;

        public HomeController(
            IMemberService iMemberService,
            ISlideAdsService iSlideAdsService,
            IFloorService iFloorService,
            IArticleCategoryService iArticleCategoryService,
            IArticleService iArticleService,
            IBrandService iBrandService,
            ILimitTimeBuyService iLimitTimeBuyService,
            IShopBonusService iShopBonusService,
            IConfiguration configuration)
        {
            _iMemberService = iMemberService;
            _iSlideAdsService = iSlideAdsService;
            _iFloorService = iFloorService;
            _iArticleCategoryService = iArticleCategoryService;
            _iArticleService = iArticleService;
            _iBrandService = iBrandService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iShopBonusService = iShopBonusService;

            _configuration = configuration;


        }
        private bool IsInstalled()
        {
            // var t = ConfigurationManager.AppSettings["IsInstalled"];
            // return null == t || bool.Parse(t);

            bool t = _configuration.GetValue<bool>("Mall:IsInstalled");

            return t;
        
        
        }

        //#if !DEBUG
        //               [OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
        //#endif
        //[OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
        [HttpGet]
        public ActionResult Index()
        {
            if (!IsInstalled())
            {
                 return RedirectToAction("Agreement", "Installer");

             

            }
            else
            {

                  return PhysicalFile(IOHelper.GetMapPath("/Areas/Web/Views/Home/index1.html"), "text/html");

             //   return PhysicalFile(IOHelper.GetMapPath("/Areas/Web/Views/Home/index.cshtml"), "text/html");


            }


        }

        public ActionResult Index1()
        {
            return PhysicalFile(IOHelper.GetMapPath("/Areas/Web/Views/Home/index1.html"), "text/html");
        }

        public ActionResult Test()
        {
            return View();
        }

        /// <summary>n
        /// 用于响应SLB，直接返回
        /// </summary>
        /// <returns></returns>
        [HttpHead]
        public ContentResult Index(string s)
        {
            return Content("");
        }


        IEnumerable<string> GetOAuthValidateContents()
        {
            var oauthPlugins = Core.PluginsManagement.GetPlugins<IOAuthPlugin>(true);
            return oauthPlugins.Select(item => item.Biz.GetValidateContent());
        }

        [HttpPost]
        public JsonResult GetProducts(long[] ids)
        {
            var products = ProductManagerApplication.GetProductByIds(ids).ToList().Select(item => new
            {
                item.Id,
                item.ProductName,
                item.MarketPrice,
                item.MinSalePrice,
                item.SaleStatus,
                item.ImagePath
            });

            return Json(products, true);
        }

        /// <summary>
        /// 获取限时购商品
        /// </summary>
        /// <param name="ids">商品ID集合</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetLimitedProducts(List<long> ids)
        {
            var result = LimitTimeApplication.GetPriceByProducrIds(ids).ToList();
            var productIds = result.Select(p => p.ProductId);
            var skuInfos = ProductManagerApplication.GetSKUByProducts(productIds);//商品规格
            var flashSaleIds = result.Select(p => p.Id);
            var flashSaleDetails = LimitTimeApplication.GetFlashSaleDetailByFlashSaleIds(ids).ToList();//限时购明细
            var products = result.Select(item => new
            {
                ProductName = item.ProductName,
                MinPrice = item.MinPrice,
                ProductId = item.ProductId,
                Count = (skuInfos == null || flashSaleDetails == null) ? 0 :
                (Math.Min(skuInfos.Where(a => a.ProductId == item.ProductId).Sum(b => b.Stock)
                , flashSaleDetails.Where(t => t.ProductId == item.ProductId).Sum(t => t.TotalCount)))//取活动=限时购活动库存和规格库存最少的一个
            });

            return Json(products, true);
        }

        /// <summary>
        /// 首页动态获取我的积分、优惠券、已领取的优惠券
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Coupon()
        {
            int memberIntegral = 0; List<Coupon> baseCoupons = new List<Coupon>();
            long shopId = CurrentSellerManager != null ? CurrentSellerManager.ShopId : 0;
            if (CurrentUser != null)
            {
                memberIntegral = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id).AvailableIntegrals;

                //优惠卷
                var coupons = CouponApplication.GetAllUserCoupon(CurrentUser.Id).ToList();
                coupons = coupons == null ? new List<UserCouponInfo>() : coupons;
                if (coupons != null)
                {
                    baseCoupons.AddRange(coupons.Select(p => new Coupon()
                    {
                        BasePrice = p.Price,
                        BaseShopId = p.ShopId,
                        BaseShopName = p.ShopName,
                        BaseType = p.BaseType,
                        OrderAmount = p.OrderAmount
                    }));
                }

                //红包
                var shopBonus = ShopBonusApplication.GetCanUseDetailByUserId(CurrentUser.Id);
                shopBonus = shopBonus == null ? new List<ShopBonusReceiveInfo>() : shopBonus;
                if (shopBonus != null)
                {
                    baseCoupons.AddRange(shopBonus.Select(p => {
                        var grant = _iShopBonusService.GetGrant(p.BonusGrantId);
                        var bonus = _iShopBonusService.GetShopBonus(grant.ShopBonusId);
                        var shop = ShopApplication.GetShop(bonus.ShopId);

                        return new Coupon()
                        {
                            BasePrice = p.Price,
                            BaseShopId = shop.Id,
                            BaseShopName = shop.ShopName,
                            BaseType = p.BaseType,
                            UseState = bonus.UseState,
                            UsrStatePrice = bonus.UsrStatePrice
                        };
                    }));
                }
            }
            return Json(new
            {
                memberIntegral = memberIntegral,
                baseCoupons = baseCoupons,
                shopId = shopId
            }, true);
        }

        // GET: Web/Home
        public ActionResult Index2()
        {
            BranchShopDayFeatsQuery query = new BranchShopDayFeatsQuery();
            query.StartDate = DateTime.Now.Date.AddDays(-10);
            query.EndDate = DateTime.Now.Date;
            query.ShopId = 288;
            query.BranchShopId = 21;
            var model = Mall.Application.OrderAndSaleStatisticsApplication.GetDayAmountSale(query);
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }

        [HttpGet]
        public JsonResult GetFoot()
        {
            var articleCategoryService = _iArticleCategoryService;
            var articleService = _iArticleService;
            //服务文章
            var pageFootServiceCategory = articleCategoryService.GetSpecialArticleCategory(SpecialCategory.PageFootService);
            if (pageFootServiceCategory == null)
            {
                return Json(new List<PageFootServiceModel>());
            }
            var pageFootServiceSubCategies = articleCategoryService.GetArticleCategoriesByParentId(pageFootServiceCategory.Id);
            var pageFootService = pageFootServiceSubCategies.ToArray().Select(item =>
                 new PageFootServiceModel()
                 {
                     CateogryName = item.Name,
                     Articles = articleService.GetArticleByArticleCategoryId(item.Id).Where(t => t.IsRelease)
                 }
                );
            var PageFootService = pageFootService;
            return Json(PageFootService);
        }

        public ActionResult TestLogin()
        {
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }


        /// <summary>
        /// 获取主题配色json
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetThemeSettingJson()
        {
            string currentTempdate = System.IO.File.ReadAllText(IOHelper.GetMapPath(_templatesettings));//读取当前应用的模板
            TemplateSetting curTemplateObj = ParseFormJson<TemplateSetting>(currentTempdate);
            if (curTemplateObj != null)
            {
                if (System.IO.File.Exists(IOHelper.GetMapPath(_themesettings)))
                {
                    string currentTheme = System.IO.File.ReadAllText(IOHelper.GetMapPath(_themesettings));//读取当前模板应用的主题配色
                    List<ThemeSetting> curThemeObjs = ParseFormJson<List<ThemeSetting>>(currentTheme);
                    if (curThemeObjs != null && curThemeObjs.Count > 0)
                    {
                        var info = curThemeObjs.FirstOrDefault(a => a.templateId == curTemplateObj.Id);
                        if (null != info)
                        {
                            return Json(info, true);
                        }
                    }
                }
            }
            return Json(null, true);
        }


        [HttpPost]
        public JsonResult GetFootNew()
        {
            //页脚
            var articleCategoryService = _iArticleCategoryService;
            var articleService = _iArticleService;
            //服务文章
            var pageFootServiceCategory = articleCategoryService.GetSpecialArticleCategory(SpecialCategory.PageFootService);
            if (pageFootServiceCategory == null) { return Json(null); }
            var pageFootServiceSubCategies = articleCategoryService.GetArticleCategoriesByParentId(pageFootServiceCategory.Id).ToList();
            dynamic noticeInfo = new System.Dynamic.ExpandoObject();
            var allArticle = articleService.GetArticleByArticleCategoryIds(pageFootServiceSubCategies.Select(a => a.Id).ToList()).Where(p => p.IsRelease).ToList();
            FootNoticeModel info = null;
            List<FootNoticeModel> resultList = new List<FootNoticeModel>();
            pageFootServiceSubCategies.ForEach(p =>
            {
                info = new FootNoticeModel()
                {
                    CateogryName = p.Name,
                    List = allArticle.Where(x => x.CategoryId == p.Id).Select(y => new ArticleInfo
                    {
                        Id = y.Id,
                        Title = y.Title
                    }).ToList()
                };
                resultList.Add(info);
            });
            noticeInfo.PageFootService = resultList;
            //页脚
            noticeInfo.PageFoot = SiteSettings.PageFoot;
            noticeInfo.QRCode = SiteSettings.QRCode;
            noticeInfo.SiteName = SiteSettings.SiteName;
            noticeInfo.Logo = SiteSettings.Logo;
            noticeInfo.PCBottomPic = SiteSettings.PCBottomPic;
            noticeInfo.Site_SEOTitle = SiteSettings.Site_SEOTitle;
            noticeInfo.APPCanDownload = SiteSettings.CanDownload;
            if (SiteSettings.CanDownload)
            {
                string host = CurrentUrlHelper.CurrentUrlNoPort();
                var link = String.Format("{0}/m-wap/home/downloadApp", host);
                var map = Core.Helper.QRCodeHelper.Create(link);
                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                //  将图片内存流转成base64,图片以DataURI形式显示  
                string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
                ms.Dispose();
                noticeInfo.appqr = strUrl;
            }
            return Json(noticeInfo, true);
        }


        [HttpPost]
        public JsonResult GetNotice()
        {
             var specialArticleInfo =  EngineContext.Current.Resolve<IArticleCategoryService>().GetSpecialArticleCategory(SpecialCategory.InfoCenter);


            if (specialArticleInfo != null)
            {
                var result =  EngineContext.Current.Resolve<IArticleService>().GetTopNArticle<ArticleInfo>(7, specialArticleInfo.Id);


                var notice = result.Select(p => new
                {
                    url = "/Article/Details/" + p.Id,
                    title = p.Title,
                    cid = p.CategoryId
                });
                return Json(notice, true);
            }
            return Json(null);
        }

        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }


        [HttpPost]
        public JsonResult GetBrands()
        {
            var result = BrandApplication.GetBrands("", 1, int.MaxValue).Models;
            var brands = result.Select(item => new
            {
                BrandName = item.Name,
                BrandLogo = Core.MallIO.GetImagePath(item.Logo),
                Id = item.Id
            });
            return Json(brands, true);
        }

        [HttpPost]
        /// <summary>
        /// 获取平台客服信息
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPlatformCustomerService()
        {
            //List<CustomerService> result = CustomerServiceApplication.GetPlatformCustomerService(true, false).OrderBy(t => t.Tool);
            var result = CustomerServiceApplication.GetPlatformCustomerService(true, false).OrderByDescending(t => t.Tool);
            return Json(result, true);
        }
    }
}