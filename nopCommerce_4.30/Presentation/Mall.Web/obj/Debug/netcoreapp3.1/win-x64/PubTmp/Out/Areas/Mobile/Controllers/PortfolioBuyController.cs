using Mall.Application;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class PortfolioBuyController : BaseMobileMemberController
    {
        private IShopService _iShopService;
        private IVShopService _iVShopService;
        private IProductService _iProductService;
        private ICashDepositsService _iCashDepositsService;
        private IFreightTemplateService _iFreightTemplateService;
        private IRegionService _iRegionService;
        public PortfolioBuyController(IShopService iShopService, IVShopService iVShopService, IProductService iProductService,
            ICashDepositsService iCashDepositsService, IFreightTemplateService iFreightTemplateService, IRegionService iRegionService
            )
        {
            _iShopService = iShopService;
            _iVShopService = iVShopService;
            _iProductService = iProductService;
            _iCashDepositsService = iCashDepositsService;
            _iFreightTemplateService = iFreightTemplateService;
            _iRegionService = iRegionService;
        }

        // GET: Mobile/PortfolioBuy
        public ActionResult ProductDetail(long productId)
        {
            var serivce = ServiceApplication.Create<ICollocationService>();
            var collocation = serivce.GetCollocationByProductId(productId);
            if (collocation == null) return View();
            var cProducts = serivce.GetProducts(new List<long> { collocation.Id });
            var allCollSKUs = serivce.GetSKUs(cProducts.Select(p => p.Id).ToList());
            var products = ProductManagerApplication.GetOnSaleProducts(cProducts.Select(p => p.ProductId).ToList());
            var allSKUs = ProductManagerApplication.GetSKUByProducts(products.Select(p => p.Id).ToList());
            //移除下架商品
            cProducts = cProducts.Where(p => products.Select(o => o.Id).Contains(p.ProductId)).ToList();

            var result = cProducts.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                var cSKUs = allCollSKUs.Where(p => p.ProductId == item.ProductId);
                var skus = allSKUs.Where(p => p.ProductId == item.ProductId);
                var collocationProduct = new CollocationProducts()
                {
                    DisplaySequence = item.DisplaySequence,
                    IsMain = item.IsMain,
                    Stock = skus.Sum(t => t.Stock),
                    ProductName = product.ProductName,
                    ProductId = item.ProductId,
                    ColloPid = item.Id,
                    Image = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_100),
                    IsShowSku = isShowSku(item.ProductId)
                };
                if (cSKUs != null && cSKUs.Count() > 0)
                {
                    collocationProduct.MaxCollPrice = cSKUs.Max(x => x.Price);
                    collocationProduct.MaxSalePrice = cSKUs.Max(x => x.SkuPirce);
                    collocationProduct.MinCollPrice = cSKUs.Min(x => x.Price);
                    collocationProduct.MinSalePrice = cSKUs.Min(x => x.SkuPirce);
                }
                return collocationProduct;
            }).Where(p => p.Stock > 0).OrderBy(a => a.DisplaySequence).ToList();

            return View(result);

        }

        public bool isShowSku(long id)
        {
            return ProductManagerApplication.HasSKU(id);
        }

        public JsonResult GetSKUInfo(long pId, long colloPid = 0)
        {
            var product = ServiceApplication.Create<IProductService>().GetProduct(pId);
            List<Mall.Entities.CollocationSkuInfo> collProduct = null;
            if (colloPid != 0)
            {
                collProduct = ServiceApplication.Create<ICollocationService>().GetProductColloSKU(pId, colloPid);
            }
            var skuArray = new List<ProductSKUModel>();
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            foreach (var sku in skus.Where(s => s.Stock > 0))
            {

                var price = sku.SalePrice;
                if (collProduct != null && collProduct.Count > 0)
                {
                    var collsku = collProduct.FirstOrDefault(a => a.SkuID == sku.Id);
                    if (collsku != null)
                        price = collsku.Price;
                }
                skuArray.Add(new ProductSKUModel
                {
                    Price = price,
                    SkuId = sku.Id,
                    Stock = sku.Stock
                });
            }
            //foreach (var item in skuArray)
            //{
            //    var str = item.SKUId.Split('_');
            //    item.SKUId = string.Format("{0};{1};{2}", str[1], str[2], str[3]);
            //}
            return Json(new
            {
                skuArray = skuArray
            });
        }


    }
}