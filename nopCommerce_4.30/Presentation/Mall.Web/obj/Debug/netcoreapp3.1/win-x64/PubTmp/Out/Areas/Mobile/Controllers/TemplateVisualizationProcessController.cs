using Mall.Application;
using Mall.CommonModel;
using Mall.IServices;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class TemplateVisualizationProcessController : BaseMobileTemplatesController
    {
        private IProductService _iProductService;
        private IShopService _iShopService;
        public TemplateVisualizationProcessController(IProductService iProductService, IShopService iShopService)
        {
            _iProductService = iProductService;
            _iShopService = iShopService;
        }
        // GET: Admin/TemplateVisualizationProcess
        public ActionResult GoodsListAction()
        {
            var data = this.ControllerContext.RouteData.Values;
            var layout = data["Layout"];
            string ids = "";
            bool showIco = false, showPrice = false, showName = false;
            bool showWarp = true;
            string warpId = "";
            if (layout != null)
            {
                ids = data["IDs"].ToString();
                showIco = bool.Parse(data["ShowIco"].ToString());
                showPrice = bool.Parse(data["showPrice"].ToString());
                showName = data["showName"].ToString() == "1";
                warpId = data["ID"].ToString();
                showWarp = true;
            }
            else
            {
                layout = Request.Query["Layout"];
                ids = Request.Query["IDs"];
                showIco = bool.Parse(Request.Query["ShowIco"]);
                showPrice = bool.Parse(Request.Query["showPrice"]);
                showName = Request.Query["showName"] == "1";
                if (!string.IsNullOrWhiteSpace(Request.Query["showWarp"]))
                {
                    showWarp = bool.Parse(Request.Query["showWarp"]);
                }
            }
            var name = "~/Views/Shared/GoodGroup" + layout + ".cshtml";
            ProductAjaxModel model = new ProductAjaxModel() { list = new List<ProductContent>() };
            model.showIco = showIco;
            model.showPrice = showPrice;
            model.showName = showName;
            model.showWarp = showWarp;
            model.warpId = warpId;
            var productSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;
            List<long> idArray = new List<long>();
            idArray = ids.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => long.Parse(d)).ToList();
            if (idArray != null && idArray.Count > 0)
            {
                var products = ProductManagerApplication.GetProductByIds(idArray);
                model.list = new List<ProductContent>();
                var selfshop = _iShopService.GetSelfShop();
                decimal discount = 1m;
                if (CurrentUser != null)
                {
                    discount = CurrentUser.MemberDiscount;
                }
                foreach (var id in idArray)
                {
                    Entities.ProductInfo d = products.FirstOrDefault(p => p.Id == id);
                    if (null == d) continue;
                    decimal minprice = d.MinSalePrice;
                    if (selfshop != null && d.ShopId == selfshop.Id)
                    {
                        minprice = d.MinSalePrice * discount;
                    }
                    var _tmp = new ProductContent
                    {
                        product_id = d.Id,
                        link = "/m-wap/Product/Detail/" + d.Id.ToString(),
                        price = minprice.ToString("f2"),
                        original_price = d.MarketPrice.ToString("f2"),
                        pic = Core.MallIO.GetProductSizeImage(d.RelativePath, 1, (int)ImageSize.Size_350),
                        title = d.ProductName,
                        is_limitbuy = _iProductService.IsLimitBuy(d.Id),
                        SaleCounts = d.SaleCounts + (int)d.VirtualSaleCounts,
                        ProductSaleCountOnOff = productSaleCountOnOff,
                        productType=d.ProductType
                    };
                    model.list.Add(_tmp);
                }
            }
            return PartialView(name, model);
        }
    }
}