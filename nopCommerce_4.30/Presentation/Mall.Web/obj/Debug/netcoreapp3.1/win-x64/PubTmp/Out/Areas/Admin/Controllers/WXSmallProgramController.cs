using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Areas.Admin.Models.Product;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class WXSmallProgramController : BaseAdminController
    {
        private IWXSmallProgramService _iWXSmallProgramService;
        private IWXMsgTemplateService _iWXMsgTemplateService;

        public WXSmallProgramController(
            IWXSmallProgramService iWXSmallProgramService,  IWXMsgTemplateService iWXMsgTemplateService)
        {
            _iWXSmallProgramService = iWXSmallProgramService;
            _iWXMsgTemplateService = iWXMsgTemplateService;
        }
        public ActionResult HomePageSetting()
        {
            var model = new VTemplateEditModel();
            model.ClientType = VTemplateClientTypes.WXSmallProgram;
            model.Name = "smallprog";
            //门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            VTemplateHelper.DownloadTemplate("", model.ClientType, 0);
            return View(model);
        }

        public ActionResult ProductSetting()
        {
            return View();
        }

        /// <summary>
        /// 设置小程序商品
        /// </summary>
        /// <param name="productIds">商品ID，用','号隔开</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddWXSmallProducts(string productIds)
        {
            WXSmallProgramApplication.SetWXSmallProducts(productIds);
            return Json(new { success = true });
        }

        /// <summary>
        /// 查询已绑定的商品信息
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="rows">每页行数</param>
        /// <param name="keyWords">搜索关键字</param>
        /// <param name="categoryId">3级分类</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetWXSmallProducts(int page, int rows, string keyWords, string shopName, long? categoryId = null)
        {
            ProductQuery productQuery = new ProductQuery()
            {
                CategoryId = categoryId,
                KeyWords = keyWords,
                ShopName = shopName
            };
            var datasql = _iWXSmallProgramService.GetWXSmallProducts(page, rows,
                productQuery);

            var products = datasql.Models.ToArray().Select(item => new ProductModel()
            {
                name = item.ProductName,
                brandName = item.BrandName,
                id = item.Id,
                imgUrl = item.GetImage(ImageSize.Size_50),
                price = item.MinSalePrice,
                state = item.ShowProductState,
                productCode = item.ProductCode,
                shopName = item.ShopName
            });
            var dataGrid = new DataGridModel<ProductModel>() { rows = products, total = datasql.Total };
            return Json(dataGrid);
        }

        /// <summary>
        /// 删除对应商品
        /// </summary>
        /// <param name="Id">设置ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteWXSmallProductById(long Id)
        {
            _iWXSmallProgramService.DeleteWXSmallProductById(Id);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteList(string ids)
        {
            var list = ids.Split(',').Select(p => long.Parse(p)).ToList();
            _iWXSmallProgramService.DeleteWXSmallProductByIds(list);
            return Json(new Result() { success = true, msg = "批量删除成功！" });
        }
        #region 微信模版
        public ActionResult EditWXMessage()
        {
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item =>
            {
                dynamic model = new ExpandoObject();
                model.name = item.PluginInfo.DisplayName;
                model.pluginId = item.PluginInfo.PluginId;
                model.enable = item.PluginInfo.Enable;
                model.status = item.Biz.GetAllStatus();
                return model;
            }
                );

            var siteSetting = SiteSettingApplication.SiteSettings;
            ViewBag.WeixinAppletId = siteSetting.WeixinAppletId;
            ViewBag.WeixinAppletSecret = siteSetting.WeixinAppletSecret;

            ViewBag.messagePlugins = data;

            List<Entities.WeiXinMsgTemplateInfo> wxtempllist = new List<Entities.WeiXinMsgTemplateInfo>();
            wxtempllist = _iWXMsgTemplateService.GetWeiXinMsgTemplateListByApplet();
            return View(wxtempllist);
        }

        //public ActionResult EditWXO2OMessage()
        //{
        //    var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
        //    var data = messagePlugins.Select(item =>
        //    {
        //        dynamic model = new ExpandoObject();
        //        model.name = item.PluginInfo.DisplayName;
        //        model.pluginId = item.PluginInfo.PluginId;
        //        model.enable = item.PluginInfo.Enable;
        //        model.status = item.Biz.GetAllStatus();
        //        return model;
        //    });
        //    var siteSetting = SiteSettingApplication.SiteSettings;
        //    ViewBag.WeixinO2OAppletId = siteSetting.WeixinO2OAppletId;
        //    ViewBag.WeixinO2OAppletSecret = siteSetting.WeixinO2OAppletSecret;
        //    ViewBag.messagePlugins = data;
        //    List<Entities.WeiXinMsgTemplateInfo> wxtempllist = new List<Entities.WeiXinMsgTemplateInfo>();
        //    wxtempllist = _iWXMsgTemplateService.GetWeiXinMsgTemplateListByApplet(true);
        //    return View(wxtempllist);
        //}

        #endregion

        [HttpPost]
        [UnAuthorize]
    
        public JsonResult Save(string values, string weixinAppletId, string WeixinAppletSecret)
        {
            weixinAppletId = (!string.IsNullOrEmpty(weixinAppletId) ? weixinAppletId.Trim() : weixinAppletId);
            WeixinAppletSecret = (!string.IsNullOrEmpty(WeixinAppletSecret) ? WeixinAppletSecret.Trim() : WeixinAppletSecret);
            var settings = SiteSettingApplication.SiteSettings;
            settings.WeixinAppletId = weixinAppletId;
            settings.WeixinAppletSecret = WeixinAppletSecret;
            SiteSettingApplication.SaveChanges();
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(values);
            _iWXMsgTemplateService.UpdateWXsmallMessage(items);
            return Json(new { success = true });
        }
    }
}