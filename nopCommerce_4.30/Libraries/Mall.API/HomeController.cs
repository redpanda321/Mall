using Mall.Application;
using Mall.Core;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mall.API
{
    public class HomeController : BaseApiController
    {
        //APP首页配置共用于安卓和IOS，这里的平台类型写的为IOS，安卓调用首页接口数据时平台类型也选IOS


        [HttpGet("Get")]
        public APPHome Get(int pageNo, int pageSize)
        {
            var slideImageSettings = ServiceProvider.Instance<ISlideAdsService>.Create.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.IOSShopHome);

            var images = ServiceProvider.Instance<ISlideAdsService>.Create.GetImageAds(0, Mall.CommonModel.ImageAdsType.APPSpecial).ToList();

            var mhproser = ServiceProvider.Instance<IMobileHomeProductsService>.Create;
            var data = mhproser.GetMobileHomeProducts(0, PlatformType.IOS, pageNo, pageSize);
            var totalProducts = data.Total;
            var homeProducts = data.Models;
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var products = new List<HomeProduct>();

            var limitService = ServiceProvider.Instance<ILimitTimeBuyService>.Create;
            var fight = FightGroupApplication.GetFightGroupPrice();
            var allProducts = ProductManagerApplication.GetProducts(homeProducts.Select(p => p.ProductId));
            var allShops = ShopApplication.GetShops(allProducts.Select(p => p.ShopId));
            foreach (var item in homeProducts)
            {
                var limitBuy = limitService.GetLimitTimeMarketItemByProductId(item.ProductId);
                var product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                var shop = allShops.FirstOrDefault(p => p.Id == product.ShopId);
                decimal minSalePrice = shop.IsSelf ? product.MinSalePrice * discount : product.MinSalePrice;
                if (limitBuy != null)
                {
                    minSalePrice = limitBuy.MinPrice; //限时购不打折
                }
                var isFight = fight.Where(r => r.ProductId == item.ProductId).FirstOrDefault();
                long activeId = 0;
                if (isFight != null)
                {
                    minSalePrice = isFight.ActivePrice;
                    activeId = isFight.ActiveId;
                }
                products.Add(new HomeProduct()
                {
                    Id = item.ProductId.ToString(),
                    ImageUrl = MallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)CommonModel.ImageSize.Size_220),
                    Name = product.ProductName,
                    MarketPrice = product.MarketPrice.ToString(),
                    SalePrice = minSalePrice.ToString("f2"),
                    Discount = product.MarketPrice <= 0 ? "0" : (minSalePrice / product.MarketPrice).ToString("0.0"),
                    Url = Core.MallIO.GetRomoteImagePath("/m-ios/product/detail/" + item.ProductId),
                    FightGroupId = activeId
                });
            }

            var iconSettings = ServiceProvider.Instance<ISlideAdsService>.Create.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.APPIcon);
            var icon = iconSettings.ToArray().Select(item => new HomeSlides { Desc = item.Description, ImageUrl = Core.MallIO.GetRomoteImagePath(item.ImageUrl), Url = item.Url });

            var services = CustomerServiceApplication.GetPlatformCustomerService(true, true);
            //var meiqia = CustomerServiceApplication.GetPlatformCustomerService(true, false).FirstOrDefault(p => p.Tool == Entities.CustomerServiceInfo.ServiceTool.MeiQia);
            //if (meiqia != null)
            //    services.Insert(0, meiqia);

            APPHome appHome = new APPHome();
            appHome.success = true;
            //2017年9月1号 商城首页接口修改（把原广告图片的去掉，只保留商品）
            appHome.TotalProduct = totalProducts;
            appHome.Icon = icon;
            //appHome.Slide = slides;//轮播图数组
            //appHome.Topic = homeImage;//专题数组
            appHome.Product = products;
            appHome.CustomerServices = services;
            return appHome;
        }
        [HttpGet("GetUpdateApp")]
        public object GetUpdateApp(string appVersion, int type)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;

            if (string.IsNullOrWhiteSpace(appVersion) || (3 < type && type < 2))
            {
                return ErrorResult("版本号不能为空或者平台类型错误", 10006);
            }
            Version ver = null;
            try
            {
                ver = new Version(appVersion);
            }
            catch (Exception)
            {
                return ErrorResult("错误的版本号", 10005);
            }
            if (string.IsNullOrWhiteSpace(siteSetting.AppVersion))
            {
                siteSetting.AppVersion = "0.0.0";
            }
            var downLoadUrl = "";
            Version v1 = new Version(siteSetting.AppVersion), v2 = new Version(appVersion);
            if (v1 > v2)
            {
                if (type == (int)PlatformType.IOS)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.IOSDownLoad))
                    {
                        return ErrorResult("站点未设置IOS下载地址", 10004);
                    }
                    downLoadUrl = siteSetting.IOSDownLoad;
                }
                if (type == (int)PlatformType.Android)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.AndriodDownLoad))
                    {
                        return ErrorResult("站点未设置Andriod下载地址", 10003);
                    }
                    string str = siteSetting.AndriodDownLoad.Substring(siteSetting.AndriodDownLoad.LastIndexOf("/"), siteSetting.AndriodDownLoad.Length - siteSetting.AndriodDownLoad.LastIndexOf("/"));
                    // var curProjRootPath = System.Web.Hosting.HostingEnvironment.MapPath("~/app") + str;
                    var env = EngineContext.Current.Resolve<IWebHostEnvironment>();
                    var curProjRootPath = env.ContentRootPath + ("/app") + str;

                    if (!System.IO.File.Exists(curProjRootPath))
                    {
                        return ErrorResult("站点未上传app安装包", 10002);
                    }
                    downLoadUrl = siteSetting.AndriodDownLoad;
                }
            }
            else
            {
                return ErrorResult("当前为最新版本", 10001);
            }
            dynamic result = SuccessResult();
            result.code = 10000;
            result.DownLoadUrl = downLoadUrl;
            result.Description = siteSetting.AppUpdateDescription;

            return result;
        }

        /// <summary>
        /// 获取App引导页图片
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetAppGuidePages")]
        public List<Mall.DTO.SlideAdModel> GetAppGuidePages()
        {
            var result = SlideApplication.GetGuidePages();
            foreach (var item in result)
            {
                item.ImageUrl = MallIO.GetRomoteImagePath(item.ImageUrl);
            }
            if (result == null)
            {
                result = new List<DTO.SlideAdModel>();
            }
            return result;
        }

        [HttpGet("GetAboutUs")]
        public object GetAboutUs()
        {
            var appModel = SystemAgreementApplication.GetAgreement(Entities.AgreementInfo.AgreementTypes.APP);
            var content = string.Empty;
            if (appModel != null)
                content = appModel.AgreementContent.Replace("src=\"/Storage/", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/") + "/");
            return SuccessResult(content);
        }
    }
}
