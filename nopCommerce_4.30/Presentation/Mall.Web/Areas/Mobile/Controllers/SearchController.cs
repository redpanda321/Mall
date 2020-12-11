using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class SearchController : BaseMobileTemplatesController
    {
        private IVShopService _iVShopService;
        private IProductService _iProductService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ISearchProductService _iSearchProductService;
        private ICategoryService _iCategoryService;
        public SearchController(
            IVShopService iVShopService,
            IProductService iProductService,
            ILimitTimeBuyService iLimitTimeBuyService,
            ISearchProductService iSearchProductService,
            ICategoryService iCategoryService
            )
        {
            _iVShopService = iVShopService;
            _iProductService = iProductService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iSearchProductService = iSearchProductService;
            _iCategoryService = iCategoryService;
        }

        public ActionResult Index(
             string keywords = "", /* 搜索关键字 */
             long cid = 0,  /* 分类ID */
             long b_id = 0, /* 品牌ID */
             string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
             int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
             int orderType = 1, /* 排序方式（1：升序，2：降序） */
             int pageNo = 1, /*页码*/
             int pageSize = 10, /*每页显示数据量*/
             long vshopId = 0//店铺ID
         )
        {
            if (string.IsNullOrEmpty(keywords) && cid <= 0 && b_id <= 0 && a_id == "")
                keywords = Application.SiteSettingApplication.SiteSettings.Keyword;
            else
                keywords = keywords.Trim();
            var result = DoSearch(keywords, cid, b_id, a_id, orderKey, orderType, pageNo, pageSize, vshopId);
            return View(result.Data);
        }

        private SearchProductResult DoSearch(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 10, /*每页显示数据量*/
            long vshopId = 0//店铺ID
            )
        {
            #region 初始化查询Model
            SearchProductQuery model = new SearchProductQuery();
            model.ShopId = 0;
            model.BrandId = b_id;
            if (cid != 0)
            {
                var catelist = _iCategoryService.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate != null)
                {
                    if (cate.Depth == 1)
                        model.FirstCateId = cid;
                    else if (cate.Depth == 2)
                        model.SecondCateId = cid;
                    else if (cate.Depth == 3)
                        model.ThirdCateId = cid;
                }
            }

            orderType = orderKey == 3 ? orderType : 2;
            model.AttrValIds = a_id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            model.Keyword = keywords;
            model.OrderKey = orderKey;
            model.OrderType = orderType == 1;
            model.PageNumber = pageNo;
            model.PageSize = pageSize;
            model.VShopId = vshopId;
            if (!string.IsNullOrWhiteSpace(model.Keyword) && model.Keyword.Length == 1)
            {
                model.IsLikeSearch = true;
            }
            #endregion
            SearchProductResult result = _iSearchProductService.SearchProduct(model);
            int total = result.Total;
            var siteSetingInfo = SiteSettingApplication.SiteSettings;
            bool isShow = siteSetingInfo != null && siteSetingInfo.ProductSaleCountOnOff == 1;
            //当查询的结果少于一页时用like进行补偿（与PC端同步）
            if (result.Total < pageSize)
            {
                model.IsLikeSearch = true;
                SearchProductResult result2 = _iSearchProductService.SearchProduct(model);
                var idList1 = result.Data.Select(a => a.ProductId).ToList();
                var nresult = result2.Data.Where(a => !idList1.Contains(a.ProductId)).ToList();
                if (nresult.Count > 0)
                {
                    result.Total += nresult.Count;
                    result.Data.AddRange(nresult);
                }
                //补充数据后，重新排序
                Func<IEnumerable<ProductView>, IOrderedEnumerable<ProductView>> orderby = null;
                Func<IEnumerable<ProductView>, IOrderedEnumerable<ProductView>> orderByDesc = null;
                switch (model.OrderKey)
                {
                    case 2:
                        //order.Append(" ORDER BY SaleCount ");
                        orderby = e => e.OrderBy(p => p.SaleCount + Core.Helper.TypeHelper.ObjectToInt(p.VirtualSaleCounts));
                        orderByDesc = e => e.OrderByDescending(p => p.SaleCount + Core.Helper.TypeHelper.ObjectToInt(p.VirtualSaleCounts));
                        break;
                    case 3:
                        //order.Append(" ORDER BY SalePrice ");
                        orderby = e => e.OrderBy(p => p.SalePrice);
                        orderByDesc = e => e.OrderByDescending(p => p.SalePrice);
                        break;
                    case 4:
                        //order.Append(" ORDER BY Comments ");
                        orderby = e => e.OrderBy(p => p.Comments);
                        orderByDesc = e => e.OrderByDescending(p => p.Comments);
                        break;
                    default:
                        //order.Append(" ORDER BY Id ");
                        //orderby = e => e.OrderBy(p => p.ProductId);
                        //orderByDesc = e => e.OrderByDescending(p => p.ProductId);
                        //break;
                        //按最新的排序规则作为默认排序【序号越大，在前台展示的商品越靠前，序号一致时，优先销量排前，销量一致时，优先上架时间排前】
                        if (isShow)
                        {
                            orderByDesc = e => e.OrderByDescending(p => p.DisplaySequence).ThenByDescending(p => p.SaleCount + Core.Helper.TypeHelper.ObjectToInt(p.VirtualSaleCounts)).ThenByDescending(p => p.ProductId);
                        }
                        else
                        {
                            orderByDesc = e => e.OrderByDescending(p => p.DisplaySequence).ThenByDescending(p => p.ProductId);
                        }
                        break;
                }
                if (model.OrderKey > 1)
                {
                    if (model.OrderType)
                    {
                        result.Data = orderby(result.Data).ToList();
                    }
                    else
                    {
                        result.Data = orderByDesc(result.Data).ToList();
                    }
                }
                else
                {
                    result.Data = orderByDesc(result.Data).ToList();
                }
            }
            total = result.Total;
            //补商品状态
            foreach (var item in result.Data)
            {
                var _pro = _iProductService.GetProduct(item.ProductId);
                var skus = _iProductService.GetSKUs(item.ProductId);
                if (_pro == null || skus == null)
                    continue;
                if (_pro.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale && _pro.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited)
                {
                    item.ShowStatus = 0;
                    if (skus.Sum(d => d.Stock) < 1)
                    {
                        item.ShowStatus = 2;
                    }
                }
                else
                {
                    if (_pro.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited && _pro.SaleStatus == Entities.ProductInfo.ProductSaleStatus.InStock)
                    {
                        item.ShowStatus = 3;
                    }
                    else
                    {
                        item.ShowStatus = 1;
                    }
                }
            }
            if (Core.MallIO.GetMallIO().GetType().FullName.Equals("Mall.Strategy.OSS"))
                ViewBag.IsOss = true;
            else
                ViewBag.IsOss = false;

            ViewBag.keywords = model.Keyword;
            ViewBag.Total = total;
            ViewBag.cid = cid;
            ViewBag.b_id = b_id;
            ViewBag.a_id = a_id;
            ViewBag.orderKey = orderKey;
            ViewBag.orderType = orderType;
            ViewBag.isSaleCountOnOff = isShow;
            return result;

        }
        [HttpPost]
        public JsonResult Index(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 10, /*每页显示数据量*/
            long vshopId = 0,//店铺ID
            bool t = false
            )
        {
            var result = DoSearch(keywords, cid, b_id, a_id, orderKey, orderType, pageNo, pageSize, vshopId);
            if (Core.MallIO.GetMallIO().GetType().FullName.Equals("Mall.Strategy.OSS"))
            {
                foreach (var item in result.Data)
                {
                    item.ImagePath = MallIO.GetProductSizeImage(item.ImagePath, 1, (int)Mall.CommonModel.ImageSize.Size_220);
                }
            }
            else
            {
                result.Data.ForEach(p =>
                {
                    p.ImagePath = System.IO.Path.Combine(p.ImagePath, "1_220.png");
                });
            }
            return SuccessResult(data: result.Data);
        }

        public JsonResult GetSalePrice()
        {
            //会员折扣
            decimal discount = 1M;
            long shopId = 0;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
                var shopInfo = ShopApplication.GetSelfShop();
                shopId = shopInfo.Id;
            }

            var limit = LimitTimeApplication.GetLimitProducts();
            var fight = FightGroupApplication.GetFightGroupPrice();

            return Json<dynamic>(success: true, data: new { SelfShopId = shopId, Discount = discount, LimitProducts = limit, FightProducts = fight });
        }

        public JsonResult GetSearchFilter(string keyword, long cid = 0, long b_id = 0, string a_id = "")
        {
            if (string.IsNullOrEmpty(keyword) && cid <= 0 && b_id <= 0 && a_id == "")
                keyword = Application.SiteSettingApplication.SiteSettings.Keyword;

            string cacheKey = CacheKeyCollection.CACHE_SEARCHFILTER(keyword, cid, b_id, a_id);
            //if (Core.Cache.Exists(cacheKey))
            //    return Core.Cache.Get(cacheKey) as JsonResult;

            SearchProductQuery query = new SearchProductQuery()
            {
                Keyword = keyword,
                AttrValIds = a_id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                BrandId = b_id,
                FilterVirtualProduct = true
            };
            if (cid != 0)
            {
                var catelist = _iCategoryService.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate.Depth == 1)
                    query.FirstCateId = cid;
                else if (cate.Depth == 2)
                    query.SecondCateId = cid;
                else if (cate.Depth == 3)
                    query.ThirdCateId = cid;
            }
            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Length == 1)
            {
                query.IsLikeSearch = true;
            }
            var result = _iSearchProductService.SearchProductFilter(query);
            foreach (BrandView brand in result.Brand)
                brand.Logo = Mall.Core.MallIO.GetImagePath(brand.Logo);

            JsonResult json = Json<dynamic>(success: true, data: new { Attr = result.Attribute, Brand = result.Brand, Category = result.Category }); //json = Json(new { success = true, Attr = result.Attribute, Brand = result.Brand, Category = result.Category });
            json = Json<dynamic>(success: true, data: new { Attr = result.Attribute, Brand = result.Brand, Category = result.Category });
            Core.Cache.Insert(cacheKey, json, 300);
            return json;
        }
    }
}