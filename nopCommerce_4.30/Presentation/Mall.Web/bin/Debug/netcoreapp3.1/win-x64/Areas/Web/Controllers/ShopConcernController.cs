using Mall.Application;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class ShopConcernController : BaseMemberController
    {
        private IShopService _iShopService;
        private ICustomerService _iCustomerService;
        private IProductService _iProductService;
        public ShopConcernController(IShopService iShopService, ICustomerService iCustomerService, IProductService iProductService)
        {
            _iShopService = iShopService;
            _iCustomerService = iCustomerService;
            _iProductService = iProductService;
        }
        public ActionResult Index(int pageSize = 10, int pageNo = 1)
        {
            var model = _iShopService.GetUserConcernShops(CurrentUser.Id, pageNo, pageSize);
            var list = new List<ShopConcernModel>();
            foreach (var m in model.Models)
            {
                var shop = ShopApplication.GetShop(m.ShopId);
                if (shop == null) continue;

                ShopConcernModel concern = new ShopConcernModel();
                concern.FavoriteShopInfo.Id = m.Id;
                concern.FavoriteShopInfo.Logo = shop.Logo;
                concern.FavoriteShopInfo.ConcernTime = m.Date;
                concern.FavoriteShopInfo.ShopId = m.ShopId;
                concern.FavoriteShopInfo.ShopName = shop.ShopName;
                concern.FavoriteShopInfo.ConcernCount = FavoriteApplication.GetFavoriteShopCountByShop(m.ShopId);
                concern.FavoriteShopInfo.ShopStatus = shop.ShopStatus;
                #region 热门销售
                var sale = _iProductService.GetHotSaleProduct(m.ShopId, 10);
                if (sale != null)
                {
                    foreach (var item in sale)
                    {
                        concern.HotSaleProducts.Add(new HotProductInfo
                        {
                            ImgPath = item.ImagePath,
                            Name = item.ProductName,
                            Price = item.MinSalePrice,
                            Id = item.Id,
                            SaleCount = (int)(item.SaleCounts + item.VirtualSaleCounts)
                        });
                    }
                }
                #endregion

                #region 最新上架
                var newsale = _iProductService.GetNewSaleProduct(m.ShopId, 10);
                if (newsale != null && newsale.Count > 0)
                {
                    foreach (var item in newsale)
                    {
                        concern.NewSaleProducts.Add(new HotProductInfo
                        {
                            ImgPath = item.ImagePath,
                            Name = item.ProductName,
                            Price = item.MinSalePrice,
                            Id = item.Id,
                            SaleCount = (int)item.ConcernedCount
                        });
                    }
                }
                list.Add(concern);
                #endregion
            }
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(list);
        }

        public JsonResult CancelConcernShops(string ids)
        {
            var list = ids.Split(',').Select(p => long.Parse(p)).ToList();
            _iShopService.CancelConcernShops(list, CurrentUser.Id);
            return Json(new Result() { success = true, msg = "取消成功！" });
        }

        
        public ActionResult CustmerServices(long shopId)
        {
            var model = CustomerServiceApplication.GetAfterSaleByShopId(shopId).OrderBy(m => m.Tool).ToList();
            ViewBag.Keyword = SiteSettings.Keyword;
            return PartialView(model);
        }
    }
}


