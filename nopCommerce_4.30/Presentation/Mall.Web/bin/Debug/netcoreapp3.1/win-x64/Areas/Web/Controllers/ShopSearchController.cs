using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class ShopSearchController : BaseWebController
    {
        private IShopService _iShopService;
        private IBrandService _iBrandService;
        public ShopSearchController(IShopService iShopService, IBrandService iBrandService)
        {
            _iShopService = iShopService;
            _iBrandService = iBrandService;
        }
        public ActionResult Index(string keywords = "", long categoryId = 0, long brandId = 0, int orderBy = 0, int pageNo = 1, int pageSize = 40)
        {
            var query = new ShopQuery
            {
                ShopName = keywords,
                CategoryId = categoryId,
                BrandId = brandId,
                PageNo = pageNo,
                PageSize = pageSize,
                Status = Entities.ShopInfo.ShopAuditStatus.Open
            };
            if (orderBy == 1)
                query.Sort = "OrderCount";
            var result = new ShopSearchViewModel
            {
                Keywords = keywords,
                CategoryId = categoryId,
                BrandId = brandId,
                Shops=new List<Shop>(),
                OrderBy = orderBy,
                IsSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1
            };

            var shopData = _iShopService.GetShops(query);
            var shops = shopData.Models;
            if (shopData.Total == 0) {
                return View(result);
            }

            result.Pager = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = shopData.Total
            };
            result.Total = shopData.Total;


            var statisticList = ShopApplication.GetStatisticOrderComment(shops.Select(p => p.Id).ToList());

            var productQuery = new ProductQuery
            {
                PageNo = 1,
                PageSize = 4,
                OnSale = true,
                IsAsc = false,
                Sort = "displaysale",
            };
            foreach (var item in shops)
            {
                var shop = new Shop();
                shop.Id = item.Id;
                shop.ShopName = item.ShopName;
                //销量
                shop.SaleCount = (int)ShopApplication.GetShopDisplaySales(item.Id);
                var statistic = statisticList.FirstOrDefault(p => p.ShopId == item.Id);
                //宝贝与描述
                shop.ProductAndDescription = string.Format("{00:F}", statistic.ProductAndDescription);
                //卖家服务态度
                shop.SellerServiceAttitude = string.Format("{00:F}", statistic.SellerServiceAttitude);
                //卖家发货速度
                shop.SellerDeliverySpeed = string.Format("{00:F}", statistic.SellerDeliverySpeed);

                //查询商品
                productQuery.ShopId = item.Id;
                var products = ProductManagerApplication.GetProducts(productQuery);
                shop.ProductCount = products.Total;
                shop.Products = products.Models.Select(p => new Product {
                    Id = p.Id,
                    Image = MallIO.GetProductSizeImage(p.ImagePath, 1, (int)ImageSize.Size_150),
                    Price = p.MinSalePrice,
                    CategoryId =p.CategoryId,
                }).ToList();
                shop.Logo = item.Logo;
                result.Shops.Add(shop);
            }

            //经营品牌
            var shopBrands = ShopApplication.GetShopBrands(shops.Select(p => p.Id).ToList());
            result.Brands = shopBrands.GroupBy(p => p.BrandId).ToDictionary(k => k.Key, v => v.FirstOrDefault().BrandName);
            
            //经营类型
            var CategoryIds = result.Shops.SelectMany(s => s.Products.Select(p => p.CategoryId));
            result.Categorys = CategoryApplication.GetCategories()
                .Where(p => CategoryIds.Contains(p.Id))
                .ToDictionary(k => k.Id, v => v.Name);

            return View(result);
        }
    }
}