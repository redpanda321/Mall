using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Linq;


namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class SellerPublicController : BaseSellerController
    {
        
        public ActionResult Top()
        {
            //  var t = ConfigurationManager.AppSettings["IsInstalled"];
            //  if (!(null == t || bool.Parse(t)))

            bool t = configuration.GetValue<bool>("Mall:IsInstalled");
            if (!t)
            {
                return RedirectToAction("Agreement", "Installer", new { area = "Web" });
            }
            var setting = SiteSettingApplication.SiteSettings;
            if (CurrentSellerManager != null)
            {
                var shopInfo =ShopApplication.GetShopInfo(CurrentSellerManager.ShopId);
                ViewBag.IsSellerAdmin = shopInfo.IsSelf;
                ViewBag.ShopId = CurrentSellerManager.ShopId;
                ViewBag.Name = CurrentSellerManager.UserName;
                ViewBag.SiteName = setting.SiteName;
                ViewBag.IsOpenPC = setting.IsOpenPC;
                ViewBag.Logo = MallIO.GetImagePath(SiteSettingApplication.SiteSettings.MemberLogo);
                ViewBag.EndDate = shopInfo.EndDate.ToString("yyyy-MM-dd");
                var cache = CacheKeyCollection.isPromptKey(CurrentSellerManager.ShopId);
                var cacheCode = Core.Cache.Get<string>(cache);
                if (string.IsNullOrEmpty(cacheCode))
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

        
        public ActionResult Bottom()
        {

            ViewBag.Rights = string.Join(",", CurrentSellerManager.SellerPrivileges.Select(a => (int)a).OrderBy(a => a));

            return View();
        }
    }
}