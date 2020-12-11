using Mall.Application;
using Mall.CommonModel;
using Mall.Core.Helper;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    [Area("Web")]
    public class SearchController : Controller
    {
        private IBrandService _iBrandService;
        private ICategoryService _iCategoryService;
        private IProductService _iProductService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ISearchProductService _iSearchProductService;
        public SearchController(
            IBrandService iBrandService,
            ICategoryService iCategoryService,
            IProductService iProductService,
            ILimitTimeBuyService iLimitTimeBuyService,
            ISearchProductService iSearchProductService)
        {
            _iBrandService = iBrandService;
            _iCategoryService = iCategoryService;
            _iProductService = iProductService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iSearchProductService = iSearchProductService;
        }
        public Entities.MemberInfo CurrentUser
        {
            get
            {
                var cookieValue = WebHelper.GetCookie(CookieKeysCollection.Mall_USER);
                var userId = UserCookieEncryptHelper.Decrypt(cookieValue, CookieKeysCollection.USERROLE_USER);
                if (userId != 0)
                {
                    var userInfo = MemberApplication.GetMember(userId);
                    var siteInfo = SiteSettingApplication.SiteSettings;
                    if (siteInfo != null)
                    {
                        if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenMallSmallProg || siteInfo.IsOpenApp))//授权模块影响会员折扣功能
                        {
                            userInfo.MemberDiscount = 1M;
                        }

                    }
                    return userInfo;
                }

                return null;
            }
        }

        // GET: Web/Search/SearchAd
        /// <summary>
        ///  商品搜索页面
        /// </summary>
        /// <param name="keywords">搜索关键字</param>
        /// <param name="cid">分类ID</param>
        /// <param name="b_id">品牌ID</param>
        /// <param name="a_id">属性ID, 表现形式：attrId_attrValueId</param>
        /// <param name="orderKey">序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间）</param>
        /// <param name="orderType">排序方式（1：升序，2：降序）</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页显示数据量</param>
        /// <returns></returns>
        public ActionResult SearchAd(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性值ID, 表现形式：valueid,valueid */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 60 /*每页显示数据量*/
            )
        {
            try
            {
                var siteSetingInfo = SiteSettingApplication.SiteSettings;
                bool isShow = siteSetingInfo.ProductSaleCountOnOff == 1;
                if (string.IsNullOrEmpty(keywords) && cid <= 0 && b_id <= 0 && a_id == "")
                    keywords = siteSetingInfo.Keyword;

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
                        ViewBag.pageTitle = cate.Name;
                    }
                }
                model.AttrValIds = a_id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                model.Keyword = keywords;
                model.OrderKey = orderKey;
                model.OrderType = orderType == 1;
                model.PageNumber = pageNo;
                model.PageSize = pageSize;
                #endregion
                SearchProductResult result = _iSearchProductService.SearchProduct(model);
                int total = result.Total;

                //当查询的结果少于一页时用like进行补偿
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
                            orderby = e => e.OrderBy(p => p.SaleCount + TypeHelper.ObjectToInt(p.VirtualSaleCounts));
                            orderByDesc = e => e.OrderByDescending(p => p.SaleCount + TypeHelper.ObjectToInt(p.VirtualSaleCounts));
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
                            //按最新的排序规则作为默认排序【序号越大，在前台展示的商品越靠前，序号一致时，优先销量排前，销量一致时，优先上架时间排前】
                            //orderby = e => e.OrderBy(p => p.ProductId);
                            if (isShow)
                            {//底层已经将虚拟销量累加到销量中
                                orderByDesc = e => e.OrderByDescending(p => p.DisplaySequence).ThenByDescending(p => p.SaleCount).ThenByDescending(p => p.ProductId);
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
                if (result.Total == 0)
                {
                    ViewBag.BrowsedHistory = BrowseHistrory.GetBrowsingProducts(13, CurrentUser == null ? 0 : CurrentUser.Id);
                    var category = _iCategoryService.GetCategory(model.ThirdCateId);
                    string categoryName = category == null ? string.Empty : category.Name;
                    var brand = _iBrandService.GetBrand(b_id) ?? new Entities.BrandInfo();
                    string bname = brand == null ? "" : brand.Name;
                    ViewBag.categoryName = categoryName;
                    ViewBag.bName = bname;
                }
                total = result.Total;

                //补商品状态
                foreach (var item in result.Data)
                {
                    var _pro = _iProductService.GetProduct(item.ProductId);
                    var skus = ProductManagerApplication.GetSKUs(item.ProductId);
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
                ViewBag.cid = cid;
                ViewBag.b_id = b_id;
                ViewBag.a_id = a_id;
                ViewBag.orderKey = orderKey;
                ViewBag.orderType = orderType;

                #region 分页控制
                PagingInfo info = new PagingInfo
                {
                    CurrentPage = model.PageNumber,
                    ItemsPerPage = pageSize,
                    TotalItems = total
                };
                ViewBag.pageInfo = info;
                #endregion
                ViewBag.isSaleCountOnOff = isShow;

                return View(result.Data);
            }
            catch (Exception e)
            {
                //throw e;
                return null;
            }
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

            var list = LimitTimeApplication.GetLimitProducts();

            return Json(new { success = true, SelfShopId = shopId, Discount = discount, LimitProducts = list });
        }

        public JsonResult GetSearchFilter(string keyword, long cid, long b_id, string a_id)
        {
            string cacheKey = CacheKeyCollection.CACHE_SEARCHFILTER(keyword, cid, b_id, a_id);
            //if (Core.Cache.Exists(cacheKey))
            //return Core.Cache.Get<JsonResult>(cacheKey);

            SearchProductQuery query = new SearchProductQuery()
            {
                Keyword = keyword,
                AttrValIds = a_id.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                BrandId = b_id
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

            var result = _iSearchProductService.SearchProductFilter(query);
            foreach (BrandView brand in result.Brand)
                brand.Logo = Mall.Core.MallIO.GetImagePath(brand.Logo);

            JsonResult json = Json(new { success = true, Attr = result.Attribute, Brand = result.Brand, Category = result.Category });
            Core.Cache.Insert(cacheKey, json, 300);
            return json;
        }
    }
}