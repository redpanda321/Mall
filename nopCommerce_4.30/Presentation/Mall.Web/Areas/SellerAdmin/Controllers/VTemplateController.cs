using Mall.Core;
using Mall.IServices;
using Mall.Web.Framework;
using System.Linq;


using Mall.Web.Areas.SellerAdmin.Models;
using Mall.CommonModel;
using Mall.Application;
using System.Net;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Mall.Service.Weixin;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [H5AuthorizationAttribute]
    public class VTemplateController : BaseSellerController
    {
        private IVShopService _iVShopService;
        private ISlideAdsService _iSlideAdsService;
        private INavigationService _iNavigationService;
        private ICouponService _iCouponService;
        private ITemplateSettingsService _iTemplateSettingsService;

        public VTemplateController(IVShopService iVShopService,
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

        /// <summary>
        /// 模板编辑
        /// </summary>
        /// <param name="tName"></param>
        /// <returns></returns>
        public ActionResult EditTemplate(int client = 2, string tName = "t1")
        {
            VTemplateEditModel model = new VTemplateEditModel();
            model.Name = tName;
            model.ClientType = (VTemplateClientTypes)client;
            model.IsShowPrvPage = true;
            switch (model.ClientType)
            {
                case VTemplateClientTypes.SellerWapSpecial:
                    model.IsShowTitle = true;
                    model.IsShowTags = true;
                    model.IsShowPrvPage = false;
                    model.IsShowIcon = true;
                    break;
            }
            long shopid = CurrentSellerManager.ShopId;
            model.ShopId = shopid;
            var tmpobj = _iVShopService.GetVShopByShopId(shopid);
            //if (tmpobj == null)
            //{
            //    throw new Mall.Core.MallException("未开通微店");
            //}
            long vshopid = tmpobj == null ? 0 : tmpobj.Id;
            model.VShopId = vshopid;
            return View(model);
        }

        /// <summary>
        /// 微商城微信首页模板
        /// </summary>
        /// <returns></returns>
        public ActionResult VHomepage()
        {
            //Models.VshopHomeSiteViewModel model = new Models.VshopHomeSiteViewModel();
            //未开通微店就进不去首页设置 
            //VShopInfo vshop = 
            //model.VShop = vshop;
            //model.ShopId = CurrentSellerManager.ShopId;
            //model.SlideImage = _iSlideAdsService.GetSlidAds(CurrentSellerManager.ShopId, SlideAdInfo.SlideAdType.VShopHome).ToList();
            //model.Banner = _iNavigationService.GetSellerNavigations(CurrentSellerManager.ShopId, PlatformType.WeiXin).ToList();
            var vshop = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId);
            if (vshop == null)
            {
                //throw new Mall.Core.MallException("未开通微店");
            }
            ViewBag.IsOpenVShop = vshop != null;
            ViewBag.VShopId = vshop == null ? 0 : vshop.Id;
            ViewBag.ShopId = CurrentSellerManager.ShopId;
            string crrentTemplateName = "t1";
            var curr = _iTemplateSettingsService.GetCurrentTemplate(CurrentSellerManager.ShopId);
            if (null != curr)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }

            var helper = new GalleryHelper();
            var themes = helper.LoadThemes(CurrentSellerManager.ShopId);
            var CurTemplateObj = themes.FirstOrDefault(t => t.ThemeName.Equals(crrentTemplateName.ToLower()));
            if (CurTemplateObj == null)
            {
                CurTemplateObj = themes.FirstOrDefault(t => t.ThemeName.Equals("t1"));
            }
            if (CurTemplateObj == null)
            {
                throw new MallException("错误的模板：" + crrentTemplateName);
            }
            ViewBag.CurrentTemplate = CurTemplateObj;
            ViewBag.CurUrl = Request.Scheme + "://" + Request.Host.ToString();
            return View(themes.Where(t => t.ThemeName != crrentTemplateName.ToLower()).ToList());
        }


        /// <summary>
        /// 设置微商城首页模板
        /// </summary>
        /// <param name="tName"></param>
        /// <returns></returns>
        public JsonResult UpdateCurrentTemplate(string tName)
        {
            if (string.IsNullOrWhiteSpace(tName))
                return Json(new { success = false, msg = "模板名称不能为空" });
            _iTemplateSettingsService.SetCurrentTemplate(tName, CurrentSellerManager.ShopId);
            return Json(new { success = true, msg = "模板启用成功" });
        }

        public JsonResult UpdateTemplateName(string tName, string newName)
        {
            if (string.IsNullOrWhiteSpace(tName))
                return Json(new { success = false, msg = "模板名称不能为空" });
            new GalleryHelper().UpdateTemplateName(tName, newName, CurrentSellerManager.ShopId);
            return Json(new { success = true, msg = "模板名称修改成功" });
        }

        /// <summary>
        /// 小程序微店首页模板
        /// </summary>
        /// <returns></returns>
        public ActionResult SmallProgVHomepage()
        {
            //Models.VshopHomeSiteViewModel model = new Models.VshopHomeSiteViewModel();
            //未开通微店就进不去首页设置 
            //VShopInfo vshop = 
            //model.VShop = vshop;
            //model.ShopId = CurrentSellerManager.ShopId;
            //model.SlideImage = _iSlideAdsService.GetSlidAds(CurrentSellerManager.ShopId, SlideAdInfo.SlideAdType.VShopHome).ToList();
            //model.Banner = _iNavigationService.GetSellerNavigations(CurrentSellerManager.ShopId, PlatformType.WeiXin).ToList();
            var vshop = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId);
            if (vshop == null)
            {
                //throw new Mall.Core.MallException("未开通微店");
            }
            ViewBag.IsOpenVShop = vshop != null;
            ViewBag.VShopId = vshop == null ? 0 : vshop.Id;
            ViewBag.ShopId = CurrentSellerManager.ShopId;
            string crrentTemplateName = "t1";
            //var curr = _iTemplateSettingsService.GetCurrentTemplate(CurrentSellerManager.ShopId);
            //if (null != curr)
            //{
            //    crrentTemplateName = curr.CurrentTemplateName;
            //}

            var helper = new GalleryHelper();
            var themes = helper.LoadThemes(CurrentSellerManager.ShopId);
            var CurTemplateObj = themes.FirstOrDefault(t => t.ThemeName.Equals(crrentTemplateName.ToLower()));
            if (CurTemplateObj == null)
            {
                CurTemplateObj = themes.FirstOrDefault(t => t.ThemeName.Equals("t1"));
            }
            if (CurTemplateObj == null)
            {
                throw new MallException("错误的模板：" + crrentTemplateName);
            }
            ViewBag.CurrentTemplate = CurTemplateObj;
            ViewBag.CurUrl = Request.Scheme + "://" + Request.Host.ToString();
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (!string.IsNullOrWhiteSpace(siteSetting.WeixinAppletId) && !string.IsNullOrWhiteSpace(siteSetting.WeixinAppletSecret))
            {
                ViewBag.IsHaveApplet = true;
            }

            //获取指定页面小程序码
            try
            {
                var wxhelper = new WXHelper();
                var accessToken = wxhelper.GetAccessToken(SiteSettings.WeixinAppletId, SiteSettings.WeixinAppletSecret);
                var data = "{\"path\":\"pages/vShopHome/vShopHome?id=" + vshop.Id + "\",\"width\":600}";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/wxa/getwxacode?access_token=" + accessToken);  //创建url
                request.Method = "POST";
                request.ContentType = "application/json;charset=UTF-8";
                byte[] load = Encoding.UTF8.GetBytes(data);
                request.ContentLength = load.Length;
                Stream writer = request.GetRequestStream();
                writer.Write(load, 0, load.Length);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream s = response.GetResponseStream();
                byte[] mg = StreamToBytes(s);
                MemoryStream ms = new MemoryStream(mg);
                string qrCodeImagePath = string.Empty;
                qrCodeImagePath = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
                ms.Dispose();
                ViewBag.QrCodeImagePath = qrCodeImagePath;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return View(themes.Where(t => t.ThemeName != crrentTemplateName.ToLower()).ToList());
        }

        /// <summary>
        /// 小程序模板编辑
        /// </summary>
        /// <param name="tName"></param>
        /// <returns></returns>
        public ActionResult EditSmallProgTemplate(int client = 17, string tName = "t1")
        {
            VTemplateEditModel model = new VTemplateEditModel();
            model.Name = tName;
            model.ClientType = (VTemplateClientTypes)client;
            model.IsShowPrvPage = true;
            switch (model.ClientType)
            {
                case VTemplateClientTypes.SellerWapSpecial:
                    model.IsShowTitle = true;
                    model.IsShowTags = true;
                    model.IsShowPrvPage = false;
                    model.IsShowIcon = true;
                    break;
            }
            long shopid = CurrentSellerManager.ShopId;
            model.ShopId = shopid;
            var tmpobj = _iVShopService.GetVShopByShopId(shopid);
            //if (tmpobj == null)
            //{
            //    throw new Mall.Core.MallException("未开通微店");
            //}
            long vshopid = tmpobj == null ? 0 : tmpobj.Id;
            model.VShopId = vshopid;
            return View(model);
        }

        public static byte[] StreamToBytes(Stream stream)
        {

            List<byte> bytes = new List<byte>();
            int temp = stream.ReadByte();
            while (temp != -1)
            {
                bytes.Add((byte)temp);
                temp = stream.ReadByte();
            }
            return bytes.ToArray();

        }

    }
}