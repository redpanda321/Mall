using System;

using Microsoft.AspNetCore.Mvc;
using Mall.Application;
using System.Threading.Tasks;
using Mall.CommonModel;

namespace Mall.Web.Areas.SellerAdmin.Components
{
    public class TopViewComponent : ViewComponent
    {

        
        public async Task<IViewComponentResult> InvokeAsync(ISellerManager CurrentSellerManager)
        {

            var CurrentSiteSetting = SiteSettingApplication.SiteSettings;

           
            if (CurrentSellerManager != null)
            {
                var shopInfo = Mall.Application.ShopApplication.GetShopInfo(CurrentSellerManager.ShopId);
                ViewBag.IsSellerAdmin = shopInfo.IsSelf;
                ViewBag.ShopId = CurrentSellerManager.ShopId;
                ViewBag.Name = CurrentSellerManager.UserName;
                ViewBag.SiteName = CurrentSiteSetting.SiteName;
                ViewBag.Logo = Mall.Core.MallIO.GetImagePath(CurrentSiteSetting.MemberLogo);
                ViewBag.EndDate = shopInfo.EndDate.ToString("yyyy-MM-dd");
                var cache = CacheKeyCollection.isPromptKey(CurrentSellerManager.ShopId);
                var cacheCode = Core.Cache.Get<string>(cache);
                if (cacheCode == null)
                {
                    Core.Cache.Insert(cache, "0", DateTime.Parse(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")));//一天只提醒一次
                    ViewBag.isPrompt = shopInfo.EndDate < DateTime.Now.AddDays(15) ? 1 : 0;//到期前15天提示
                }
                else
                {
                    ViewBag.isPrompt = 0;
                }
            }
            return View(CurrentSellerManager);
        }

        
        
    }
}