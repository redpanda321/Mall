using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class ShopBranchController : BaseMobileTemplatesController
    {
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ICartService _iCartService;
        private IProductService _iProductService;
        private ITypeService _iTypeService;
        private ICouponService _iCouponService;
        private IShopBranchService _iShopBranchService;

        public ShopBranchController(ILimitTimeBuyService iLimitTimeBuyService, ICartService iCartService, IProductService iProductService, ITypeService iTypeService, ICouponService iCouponService, IShopBranchService iShopBranchService)
        {
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iCartService = iCartService;
            _iProductService = iProductService;
            _iTypeService = iTypeService;
            _iCouponService = iCouponService;
            _iShopBranchService = iShopBranchService;
        }
        /// <summary>
        /// 门店首页
        /// </summary>
        /// <param name="id">门店ID</param>
        /// <returns></returns>
        public ActionResult Index(long id = 0)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);
            if (shopBranch == null)
                return RedirectToAction("Error404", "Error", new { area = "Web" });
            var shop = ShopApplication.GetShop(shopBranch.ShopId);
            if (null != shop && shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired)
                throw new MallException("此店铺已过期");
            if (null != shop && shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze)
                throw new MallException("此店铺已冻结");
            if (shopBranch.Status == ShopBranchStatus.Freeze)
                throw new MallException("此门店已冻结");
            shopBranch.AddressDetail = ShopBranchApplication.RenderAddress(shopBranch.AddressPath, shopBranch.AddressDetail, 2);
            shopBranch.CommentScore = ShopBranchApplication.GetServiceMark(id).ComprehensiveMark;//综合评分
            ViewBag.ShopBranch = shopBranch;
            var cate = ShopCategoryApplication.GetCategoryByParentId(0, shopBranch.ShopId);
            //屏蔽没有商品的分类
            List<long> noshowcid = new List<long>();
            foreach (var item in cate)
            {
                ShopBranchProductQuery query = new ShopBranchProductQuery();
                query.PageSize = 1;
                query.PageNo = 1;
                query.ShopId = shopBranch.ShopId;
                query.ShopBranchId = id;
                query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                query.ShopCategoryId = item.Id;
                var _pros = ShopBranchApplication.GetShopBranchProducts(query);
                if (_pros.Total <= 0)
                {
                    noshowcid.Add(item.Id);
                }
            }
            if (noshowcid.Count > 0)
            {
                cate = cate.Where(d => !noshowcid.Contains(d.Id)).ToList();
            }
            ViewBag.ShopCategory = cate;
            ViewBag.QQMapKey = CommonConst.QQMapKey;
            //统计门店访问人数
            StatisticApplication.StatisticShopBranchVisitUserCount(shopBranch.ShopId, shopBranch.Id);
            return View();
        }

        /// <summary>
        /// 周边门店列表
        /// </summary>
        /// <returns></returns>
        public ActionResult StoreList()
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            ViewBag.QQMapKey = CommonConst.QQMapKey;
            var model = new ShopBranchListHomePage();
            model.SlideAdModels = SlideApplication.GetShopBranchListSlide();
            return View(model);
        }

        /// <summary>
        /// 周边门店列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <param name="fromLatLng"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult List(int pageSize, int pageNo, string fromLatLng, string shopId)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal;
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.OrderKey = 2;
            query.OrderType = true;
            if (query.FromLatLng.Split(',').Length != 2)
                return Json(new Result { success = false, msg = "无法获取您的当前位置，请确认是否开启定位服务！" });

            string address = "", province = "", city = "", district = "", street = "";
            string currentPosition = string.Empty;//当前详情地址，优先顺序：建筑、社区、街道
            Region cityInfo = new Region();
            if (!string.IsNullOrWhiteSpace(shopId))//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (query.ShopId <= 0)
                    return Json(new Result { success = false, msg = "无法定位到商家！" });
            }
            else//否则取用户同城门店
            {
                var addressObj = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                    return Json(new Result { success = false, msg = "无法定位到城市！" });

                cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo != null)
                {
                    query.CityId = cityInfo.Id;
                }
                //处理当前地址

                currentPosition = addressObj.result.formatted_addresses.recommend; //street;
                if (string.IsNullOrEmpty(currentPosition))
                    currentPosition = addressObj.result.formatted_addresses.rough;
            }
            var shopBranchs = ShopBranchApplication.GetNearShopBranchs(query);
            //组装首页数据
            //补充门店活动数据
            var homepageBranchs = ProcessBranchHomePageData(shopBranchs.Models);

            return SuccessResult<dynamic>(data: new { Models = homepageBranchs, Total = shopBranchs.Total, CityInfo = new { Id = (cityInfo != null ? cityInfo.Id : 0), Name = (cityInfo != null ? cityInfo.Name : "") }, CurrentAddress = currentPosition });
        }
        /// <summary>
        /// 获取门店活动
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<HomePageShopBranch> ProcessBranchHomePageData(List<ShopBranch> list)
        {
            var shopIds = list.Select(e => e.ShopId).Distinct();
            var homepageBranchs = list.Select(e => new HomePageShopBranch
            {
                ShopBranch = e
            }).ToList();
            foreach (var sid in shopIds)
            {
                ShopActiveList actives = new ShopActiveList();
                //优惠券
                var coupons = CouponApplication.GetCouponLists(sid);
                var settings = _iCouponService.GetSettingsByCoupon(coupons.Select(p => p.Id).ToList());
                var couponList = coupons.Where(d => settings.Any(c => c.CouponID == d.Id && c.PlatForm == PlatformType.Wap)).ToList();
                actives.ShopCoupons = couponList.Select(e => new CouponModel
                {
                    Id = e.Id,
                    CouponName = e.CouponName,
                    ShopId = e.ShopId,
                    OrderAmount = e.OrderAmount.ToString(),
                    Price = e.Price,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    UseArea = e.UseArea,
                    Remark = e.Remark,
                }).ToList();
                //满额减活动
                var fullDiscount = FullDiscountApplication.GetOngoingActiveByShopId(sid);
                if (fullDiscount != null)
                {
                    actives.ShopActives = fullDiscount.Select(e => new ActiveInfo
                    {
                        ActiveName = e.ActiveName,
                        ShopId = e.ShopId
                    }).ToList();
                }
                //商家所有门店显示活动相同
                var shopBranchs = homepageBranchs.Where(e => e.ShopBranch.ShopId == sid);
                foreach (var shop in shopBranchs)
                {

                    //满额免运费（门店的）
                    actives.FreeFreightAmount = shop.ShopBranch.FreeMailFee;
                    actives.IsFreeMail = shop.ShopBranch.IsFreeMail;
                    shop.ShopAllActives = actives;
                }
            }
            return homepageBranchs;
        }
        /// <summary>
        /// 取门店销量排名前的商品、门店月销量、门店加入购物车的商品数量
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetShopBranchProductAndSaleCount(string ids)
        {
            //TODO:FG MysqlExecuted:173,耗时:538.0472毫秒
            if (string.IsNullOrEmpty(ids))
                throw new MallException("非法参数");

            var memberCartInfo = new ShoppingCartInfo();
            if (CurrentUser != null)//如果已登陆取购物车数据
                memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id);

            var branchs = ids.Split(',').Select(p => long.Parse(p));
            var result = branchs.Select(branch =>
            {
                var products = ShopBranchApplication.GetShopBranchProducts(new ShopBranchProductQuery
                {
                    PageSize = 4,
                    PageNo = 1,
                    Sort = "SaleCount",
                    ShopBranchId = branch,
                    ShopBranchProductStatus = ShopBranchSkuStatus.Normal
                });
                var now = DateTime.Now;
                return new
                {
                    saleCount = ShopBranchApplication.GetShopBranchSaleCount(branch),
                    saleCountByMonth = ShopBranchApplication.GetShopBranchSaleCount(branch, now.AddDays(-30).Date, now),
                    cartQuantity = memberCartInfo.Items.Where(c => c.ShopBranchId == branch && c.Status == 0).Sum(c => c.Quantity),
                    products = products.Models.Select(p => new ProductInfo
                    {
                        Id = p.Id,
                        ImagePath = p.ImagePath,
                        MinSalePrice = p.MinSalePrice,
                        ProductName = p.ProductName
                    }),//取4个商品
                    branchId = branch,
                    productSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1,
                    CommentScore = ShopBranchApplication.GetServiceMark(branch).ComprehensiveMark//综合评分
                };
            });
            return SuccessResult<dynamic>(data: result);
        }


        /// <summary>
        /// 门店首页商品列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <param name="shopCategoryId"></param>
        /// <param name="shopId"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult ProductList(int pageSize, int pageNo, string productId, string shopCategoryId, string shopId, string shopBranchId, string keyWords, int type)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            long shopBrandId = TypeHelper.ObjectToInt(shopBranchId, 0);
            long sId = TypeHelper.ObjectToInt(shopId, 0);
            long pId = TypeHelper.ObjectToInt(productId, 0);
            ShopBranchProductQuery query = new ShopBranchProductQuery();
            query.PageSize = pageSize;
            query.PageNo = pageNo;
            query.KeyWords = keyWords;
            query.RproductId = pId;//移除
            query.ShopId = sId;
            query.ShopBranchId = shopBrandId;
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;

            //if (!string.IsNullOrEmpty(keyWords))
            //{
            //    query.OrderKey = 6;//关键字按销量排序
            //}
            //else
            //{
            query.OrderKey = 5;
            //}
            if (query.ShopId <= 0)
                return ErrorResult<dynamic>(msg: "无法定位到商家！");

            if (TypeHelper.ObjectToInt(shopCategoryId, 0) > 0)
            {
                query.ShopCategoryId = TypeHelper.ObjectToInt(shopCategoryId);
            }

            if (query.ShopBranchId <= 0)
                return ErrorResult<dynamic>(msg: "无法定位到商家！");

            var dtNow = DateTime.Now;
            //query.StartDate = dtNow.AddYears(-10).Date;
            query.StartDate = DateTime.Parse(dtNow.ToString("yyyy-MM-01 00:00:00"));//当月的
            query.EndDate = dtNow;
            var pageModel = ShopBranchApplication.GetShopBranchProductsMonth(query, (DateTime)query.StartDate, dtNow);//普通分页           

            Mall.Entities.ShoppingCartInfo cartInfo = new Mall.Entities.ShoppingCartInfo();
            if (CurrentUser != null)
            {
                cartInfo = new BranchCartHelper().GetCart(CurrentUser.Id, shopBrandId);//获取购物车数据
            }

            if (pageModel.Models != null && pageModel.Models.Count > 0)
            {
                #region 处理商品 官方自营店会员折扣价，各活动价等。
                if (CurrentUser != null)
                {
                    var shopInfo = ShopApplication.GetShop(query.ShopId.Value);
                    if (shopInfo != null && shopInfo.IsSelf)//当前商家是否是官方自营店
                    {
                        decimal discount = 1M;
                        discount = CurrentUser.MemberDiscount;
                        foreach (var item in pageModel.Models)
                        {
                            item.MinSalePrice = Math.Round(item.MinSalePrice * discount, 2);
                        }
                    }
                }

                foreach (var item in pageModel.Models)
                {
                    item.Quantity = cartInfo != null ? cartInfo.Items.Where(d => d.ProductId == item.Id && d.ShopBranchId == shopBrandId).Sum(d => d.Quantity) : 0;

                }

                #endregion
            }
            //如果商品Id不为空，则表示此商品需要置顶
            var product = pageModel.Models.ToList().Select(item =>
            {
                var comment = CommentApplication.GetProductCommentStatistic(productId: item.Id,
                    shopBranchId: shopBrandId);
                var sbskus = ShopBranchApplication.GetSkusByProductId(shopBrandId, item.Id);
                var shopCat = ShopCategoryApplication.GetCategoryByProductId(item.Id);
                return new
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    ProductTypeId = shopCat.Id,
                    ProductType = shopCat.Name,
                    MeasureUnit = item.MeasureUnit,
                    MarketPrice = item.MarketPrice.ToString("F2"),
                    MinSalePrice = item.MinSalePrice.ToString("f2"),
                    SaleCounts = item.ShopBranchSaleCounts,//销量统计没有考虑订单支付完成。                   
                    HasSku = item.HasSKU,
                    Quantity = item.Quantity,
                    IsTop = item.Id == pId,
                    RelativePath = Core.MallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_350),
                    HighCommentCount = comment.HighComment,
                    Stock = sbskus.Sum(d => d.Stock),
                    isvirtual = item.ProductType==1
                };
            }).ToList();

            #region 置顶商品
            query.RproductId = null;
            var topproduct = new object();
            if (pId > 0)
            {
                query.ProductId = pId;
                if (type == 1)
                    query.ShopCategoryId = null;
                var topModel = ShopBranchApplication.GetShopBranchProductsMonth(query, dtNow.AddDays(-30).Date, dtNow);//置顶商品

                if (topModel.Models != null && topModel.Models.Count > 0)
                {
                    #region 处理商品 官方自营店会员折扣价，各活动价等。
                    if (CurrentUser != null)
                    {
                        var shopInfo = ShopApplication.GetShop(query.ShopId.Value);
                        if (shopInfo != null && shopInfo.IsSelf)//当前商家是否是官方自营店
                        {
                            decimal discount = 1M;
                            discount = CurrentUser.MemberDiscount;
                            foreach (var item in topModel.Models)
                            {
                                item.MinSalePrice = Math.Round(item.MinSalePrice * discount, 2);
                            }
                        }
                    }

                    foreach (var item in topModel.Models)
                    {
                        item.Quantity = cartInfo != null ? cartInfo.Items.Where(d => d.ProductId == item.Id && d.ShopBranchId == shopBrandId).Sum(d => d.Quantity) : 0;

                    }

                    #endregion
                }
                topproduct = topModel.Models.ToList().Select(item =>
                {
                    var comment = CommentApplication.GetProductCommentStatistic(productId: item.Id,
                        shopBranchId: shopBrandId);
                    var sbskus = ShopBranchApplication.GetSkusByProductId(shopBrandId, item.Id);
                    var shopCat = ShopCategoryApplication.GetCategoryByProductId(item.Id);
                    return new
                    {
                        Id = item.Id,
                        ProductName = item.ProductName,
                        ProductTypeId = shopCat.Id,
                        ProductType = shopCat.Name,
                        MeasureUnit = item.MeasureUnit,
                        MarketPrice = item.MarketPrice.ToString("F2"),
                        MinSalePrice = item.MinSalePrice.ToString("f2"),
                        SaleCounts = item.ShopBranchSaleCounts,//销量统计没有考虑订单支付完成。                   
                        HasSku = item.HasSKU,
                        IsTop = item.Id == pId,
                        Quantity = item.Quantity,
                        RelativePath = Core.MallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_350),
                        HighCommentCount = comment.HighComment,
                        Stock = sbskus.Sum(d => d.Stock),
                        isvirtual = item.ProductType == 1
                    };
                }).ToList();
            }
            #endregion

            var productSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;

            return SuccessResult<dynamic>(data: new { Models = product, TopModels = topproduct, Total = pageModel.Total, ProductSaleCountOnOff = productSaleCountOnOff });
            //return Json(new { Success = true, Models = product, TopModels = topproduct, Total = pageModel.Total });
        }

        /// <summary>
        /// 根据商品Id获取商品规格
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ProductSkuInfoById(string id, string shopBranchId)
        {
            if (!string.IsNullOrEmpty(id))
            {
                long shopBrandId = TypeHelper.ObjectToInt(shopBranchId, 0);
                long productId = Convert.ToInt32(id);
                var product = _iProductService.GetProduct(productId);
                var shop = ShopApplication.GetShop(product.ShopId);

                decimal discount = 1M;
                if (CurrentUser != null)
                {
                    discount = CurrentUser.MemberDiscount;
                }

                var skuArray = new List<ProductSKUModel>();
                object defaultsku = new object();

                var cartInfo = new Mall.Entities.ShoppingCartInfo();
                if (CurrentUser != null)
                {
                    cartInfo = new BranchCartHelper().GetCart(CurrentUser.Id, shopBrandId);//获取购物车数据
                }

                var skus = ProductManagerApplication.GetSKUs(product.Id);
                foreach (var sku in skus.Where(s => s.Stock > 0))
                {
                    var price = shop.IsSelf ? sku.SalePrice * discount : sku.SalePrice;
                    ProductSKUModel skuMode = new ProductSKUModel
                    {
                        Price = sku.SalePrice,
                        SkuId = sku.Id,
                        Stock = sku.Stock
                    };
                    //if (limitBuy != null)
                    //{
                    //    activetype = 1;
                    //    var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                    //    var limitSkuItem = limitSku.Details.Where(r => r.SkuId.Equals(sku.Id)).FirstOrDefault();
                    //    if (limitSkuItem != null)
                    //        skuMode.Price = limitSkuItem.Price;
                    //}
                    skuArray.Add(skuMode);
                }

                Entities.TypeInfo typeInfo = _iTypeService.GetType(product.TypeId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                if (product != null)
                {
                    colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
                    sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
                    versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
                }
                List<object> SkuItemList = new List<object>();
                List<object> Skus = new List<object>();
                
                //var skus = ProductManagerApplication.GetSKUs(product.Id);
                if (skus.Count > 0)
                {
                    #region 颜色
                    long colorId = 0, sizeId = 0, versionId = 0;
                    List<object> colorAttributeValue = new List<object>();
                    List<string> listcolor = new List<string>();
                    foreach (var sku in skus)
                    {
                        var specs = sku.Id.Split('_');
                        if (specs.Count() > 0 && !string.IsNullOrEmpty(sku.Color))
                        {
                            if (long.TryParse(specs[1], out colorId)) { }//相同颜色规格累加对应值
                            if (colorId != 0)
                            {
                                if (!listcolor.Contains(sku.Color))
                                {
                                    var c = skus.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                    var colorvalue = new
                                    {
                                        ValueId = colorId,
                                        Value = sku.Color,
                                        UseAttributeImage = "False",
                                        ImageUrl = Mall.Core.MallIO.GetRomoteImagePath(sku.ShowPic)
                                    };
                                    listcolor.Add(sku.Color);
                                    colorAttributeValue.Add(colorvalue);
                                }
                            }
                        }
                    }
                    var color = new
                    {
                        AttributeId = product.TypeId,
                        AttributeName = colorAlias,
                        AttributeValue = colorAttributeValue
                    };
                    if (colorId > 0)
                    {
                        SkuItemList.Add(color);
                    }
                    #endregion
                    #region 容量
                    List<object> sizeAttributeValue = new List<object>();
                    List<string> listsize = new List<string>();
                    foreach (var sku in skus.OrderBy(a => a.Size))
                    {
                        var specs = sku.Id.Split('_');
                        if (specs.Count() > 1)
                        {
                            if (long.TryParse(specs[2], out sizeId)) { }
                            if (sizeId != 0)
                            {
                                if (!listsize.Contains(sku.Size))
                                {
                                    var ss = skus.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                    var sizeValue = new
                                    {
                                        ValueId = sizeId,
                                        Value = sku.Size,
                                        UseAttributeImage = false,
                                        ImageUrl = Mall.Core.MallIO.GetRomoteImagePath(sku.ShowPic)
                                    };
                                    listsize.Add(sku.Size);
                                    sizeAttributeValue.Add(sizeValue);
                                }
                            }
                        }
                    }
                    var size = new
                    {
                        AttributeId = product.TypeId,
                        AttributeName = sizeAlias,
                        AttributeValue = sizeAttributeValue
                    };
                    if (sizeId > 0)
                    {
                        SkuItemList.Add(size);
                    }
                    #endregion
                    #region 规格
                    List<object> versionAttributeValue = new List<object>();
                    List<string> listversion = new List<string>();
                   
                    foreach (var sku in skus)
                    {
                        var specs = sku.Id.Split('_');
                        if (specs.Count() > 2)
                        {
                            if (long.TryParse(specs[3], out versionId)) { }
                            if (versionId != 0)
                            {
                                if (!listversion.Contains(sku.Version))
                                {
                                    var v = skus.Where(s => s.Version.Equals(sku.Version));
                                    var versionValue = new
                                    {
                                        ValueId = versionId,
                                        Value = sku.Version,
                                        UseAttributeImage = false,
                                        ImageUrl = Mall.Core.MallIO.GetRomoteImagePath(sku.ShowPic)
                                    };
                                    listversion.Add(sku.Version);
                                    versionAttributeValue.Add(versionValue);
                                }
                            }
                        }
                    }
                    var version = new
                    {
                        AttributeId = product.TypeId,
                        AttributeName = versionAlias,
                        AttributeValue = versionAttributeValue
                    };
                    if (versionId > 0)
                    {
                        SkuItemList.Add(version);
                    }
                    #endregion

                    #region Sku值

                    foreach (var sku in skus)
                    {
                        var prosku = new
                        {
                            SkuId = sku.Id,
                            SKU = sku.Sku,
                            Weight = product.Weight,
                            Stock = sku.Stock,
                            WarningStock = sku.SafeStock,
                            SalePrice = shop.IsSelf ? (sku.SalePrice * discount).ToString("0.##") : sku.SalePrice.ToString("0.##"),
                            CartQuantity = cartInfo != null ? cartInfo.Items.Where(d => d.SkuId == sku.Id && d.ShopBranchId == shopBrandId).Sum(d => d.Quantity) : 0,
                            ImageUrl = Core.MallIO.GetRomoteProductSizeImage(sku.ShowPic, 1, (int)ImageSize.Size_350)
                        };
                        Skus.Add(prosku);
                    }
                    defaultsku = Skus[0];
                    #endregion
                }
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        ProductId = productId,
                        ProductName = product.ProductName,
                        ImageUrl = Core.MallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350), //GetImageFullPath(model.SubmitOrderImg),
                        Stock = skuArray.Sum(s => s.Stock),// skus.Sum(s => s.Stock),
                        //ActivityUrl = activetype,
                        SkuItems = SkuItemList,
                        Skus = Skus,
                        DefaultSku = defaultsku
                    }
                });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// 购买商品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skuId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateCartItem(string skuId, string shopBranchId, string count)
        {
            BranchCartHelper branCart = new BranchCartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            int number = Convert.ToInt32(count);
            long shopbranchId = Convert.ToInt64(shopBranchId);
            List<string> skuids = new List<string>();
            skuids.Add(skuId);
            var branchSku = _iShopBranchService.GetSkusByIds(shopbranchId, skuids);
            long stock = branchSku.Sum(b => b.Stock);
            if (branchSku != null && stock < number)
                return Json(new { success = false, msg = "超出库存或限购量", stock = stock });
            branCart.UpdateCartItem(skuId, number, userId, shopbranchId);
            return Json(new { success = true });
        }
        private IEnumerable<Entities.CouponInfo> GetCouponList(long shopid)
        {
            var service = ServiceApplication.Create<ICouponService>();
            var result = service.GetCouponList(shopid);
            var couponSetList = ServiceApplication.Create<IVShopService>().GetVShopCouponSetting(shopid).Where(a => a.PlatForm == Core.PlatformType.Wap).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.Where(item => couponSetList.Contains(item.Id));//取设置的优惠卷
                return couponList;
            }
            return new List<Entities.CouponInfo>();
        }

        /// <summary>
        /// 获取店铺优惠券（过滤已被领取和过期的）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetLoadCoupon(long shopid)
        {
            var result = _iCouponService.GetCouponList(shopid);
            var couponSetList = ServiceApplication.Create<IVShopService>().GetVShopCouponSetting(shopid).Where(a => a.PlatForm == Core.PlatformType.Wap).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.Where(item => couponSetList.Contains(item.Id)).ToList();
                var list = new List<CouponRecord>();
                foreach (Entities.CouponInfo couponinfo in couponList)
                {
                    var status = 0;
                    if (CurrentUser != null)
                    {
                        status = ShopBranchApplication.CouponIsUse(couponinfo, CurrentUser.Id);
                    }
                    var coupon = new Entities.CouponRecord();
                    coupon.CouponId = couponinfo.Id;
                    coupon.CouponName = couponinfo.CouponName;
                    coupon.Price = Math.Round(couponinfo.Price, 2);
                    coupon.StartTime = couponinfo.StartTime.ToString("yyyy.MM.dd");
                    coupon.ClosingTime = couponinfo.EndTime.ToString("yyyy.MM.dd");
                    coupon.IsUse = status;
                    coupon.Remark = couponinfo.Remark;
                    coupon.UseArea = couponinfo.UseArea;
                    coupon.OrderAmount = couponinfo.OrderAmount.ToString("F2");
                    list.Add(coupon);
                }
                return Json(new
                {
                    success = true,
                    data = list
                });
            }
            else
            {
                return Json(new { success = false, msg = "没有可领取的优惠券" });
            }
        }

        /// <summary>
        /// 领取优惠劵
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetUserCoupon(long couponId)
        {
            bool status = true;
            string message = "";
            var couponInfo = _iCouponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now)
            {
                //已经失效
                return Json(new { success = false, msg = "优惠券已经过期" });
            }
            if (CurrentUser == null)
            {
                return Json(new { success = false, msg = "用户未登录" });
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponId;
            crQuery.UserId = CurrentUser.Id;
            var pageModel = _iCouponService.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
            {
                //达到个人领取最大张数
                return Json(new { success = false, msg = "达到领取最大张数" });
            }
            crQuery = new CouponRecordQuery()
            {
                CouponId = couponId
            };
            pageModel = _iCouponService.GetCouponRecordList(crQuery);
            if (pageModel.Total >= couponInfo.Num)
            {
                //达到领取最大张数
                return Json(new { success = false, msg = "此优惠券已经领完了" });
            }
            if (couponInfo.ReceiveType == Entities.CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                {
                    //积分不足
                    return Json(new { success = false, msg = "积分不足" });
                }
            }
            if (status)
            {
                Entities.CouponRecordInfo couponRecordInfo = new Entities.CouponRecordInfo()
                {
                    CouponId = couponId,
                    UserId = CurrentUser.Id,
                    UserName = CurrentUser.UserName,
                    ShopId = couponInfo.ShopId
                };
                _iCouponService.AddCouponRecord(couponRecordInfo);
                return Json(new { success = true, msg = "领取成功" });//执行成功
            }
            else
            {
                return Json(new { success = false, msg = message });
            }
        }
        public ActionResult StoreListAddress()
        {
            ViewBag.QQMapKey = CommonConst.QQMapKey;
            return View();
        }
        /// <summary>
        /// 获取门店活动和优惠劵
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetBranchList(long shopBranchId, string fromLatLng)
        {
            //获取门店活动
            var shopBranchs = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranchs != null && !string.IsNullOrWhiteSpace(fromLatLng))
            {
                shopBranchs.Distance = ShopBranchApplication.GetLatLngDistances(fromLatLng, string.Format("{0},{1}", shopBranchs.Latitude, shopBranchs.Longitude));
            }
            List<ShopBranch> shoplist = new List<ShopBranch>();
            shoplist.Add(shopBranchs);
            var homepageBranchs = ProcessBranchHomePageData(shoplist);
            //获取用户是否有领取优惠券
            var isCouponsReceived = GetUserCoupons(shopBranchs.ShopId);
            if (homepageBranchs.Count > 0)
            {
                return SuccessResult<dynamic>(data: new { data = homepageBranchs, isCouponsReceived = isCouponsReceived });
            }
            else
            {
                return ErrorResult<dynamic>();
            }
        }

        /// <summary>
        /// 获取用户是否有可领取的优惠券
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public bool GetUserCoupons(long shopid)
        {
            var resultStatus = false;
            try
            {
                var userId = CurrentUser.Id;
            }
            catch
            {
                return true; //没有获取到用户Id,用户没登录，默认可以领取优惠券
            }
            var result = _iCouponService.GetCouponList(shopid);
            var couponSetList = ServiceApplication.Create<IVShopService>().GetVShopCouponSetting(shopid).Where(a => a.PlatForm == Core.PlatformType.Wap).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.Where(item => couponSetList.Contains(item.Id)).ToList();
                foreach (Entities.CouponInfo couponinfo in couponList)
                {
                    var status = true;
                    CouponRecordQuery crQuery = new CouponRecordQuery();
                    crQuery.CouponId = couponinfo.Id;
                    crQuery.UserId = CurrentUser.Id;
                    var pageModel = _iCouponService.GetCouponRecordList(crQuery);
                    if (couponinfo.PerMax != 0 && pageModel.Total >= couponinfo.PerMax)
                    {//达到个人领取最大张数
                        status = false;
                    }
                    crQuery = new CouponRecordQuery()
                    {
                        CouponId = couponinfo.Id
                    };
                    pageModel = _iCouponService.GetCouponRecordList(crQuery);
                    if (pageModel.Total >= couponinfo.Num)
                    {//达到领取最大张数
                        status = false;
                    }
                    if (status)
                    {
                        resultStatus = true;
                    }
                }
            }
            return resultStatus;
        }

        /// <summary>
        /// 取用户收货地址
        /// </summary>
        /// <returns></returns>
        public JsonResult GetUserShippingAddressesList()
        {
            if (CurrentUser == null)
            {
                return Json(new Result { success = false, msg = "nologin" });
            }
            var addressList = OrderApplication.GetUserAddresses(CurrentUser.Id);
            var json = addressList.Select(addresses => new
            {
                id = addresses.Id,
                fullRegionName = addresses.RegionFullName,
                address = addresses.Address,
                addressDetail = addresses.AddressDetail,
                phone = addresses.Phone,
                shipTo = addresses.ShipTo,
                fullRegionIdPath = addresses.RegionIdPath,
                latitude = addresses.Latitude,
                longitude = addresses.Longitude
            });
            return Json(new Result() { success = true, data = json });
        }
        #region Search

        public ActionResult SearchBox()
        {
            return View();
        }

        /// <summary>
        /// 周边门店搜索
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchList(int? id, string keywords)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            ViewBag.QQMapKey = CommonConst.QQMapKey;
            return View(model: keywords);
        }

        /// <summary>
        /// 获取周边门店列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <param name="fromLatLng"></param>
        /// <param name="shopId"></param>
        /// <param name="keyWords">关键字</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult Search(int pageSize, int pageNo, string fromLatLng, string shopId, string keyWords)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal;
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.ShopBranchName = keyWords.Trim();
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            query.OrderKey = 2;
            query.OrderType = true;
            if (query.FromLatLng.Split(',').Length != 2)
                return Json(new Result { success = false, msg = "无法获取您的当前位置，请确认是否开启定位服务！" });

            string address = "", province = "", city = "", district = "", street = "";
            string currentPosition = string.Empty;//当前详情地址，优先顺序：建筑、社区、街道
            Region cityInfo = new Region();
            if (!string.IsNullOrWhiteSpace(shopId))//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (query.ShopId <= 0)
                    return Json(new Result { success = false, msg = "无法定位到商家！" });
            }
            else//否则取用户同城门店
            {
                var addressObj = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                    return Json(new Result { success = false, msg = "无法定位到城市！" });

                cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo != null)
                {
                    query.CityId = cityInfo.Id;
                }
                //处理当前地址

                currentPosition = street;
            }
            var shopBranchs = ShopBranchApplication.SearchNearShopBranchs(query);
            //组装首页数据
            //补充门店活动数据
            var homepageBranchs = ProcessBranchHomePageData(shopBranchs.Models);

            return SuccessResult<dynamic>(data: new { Models = homepageBranchs, Total = shopBranchs.Total, CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name }, CurrentAddress = currentPosition });
        }

        /// <summary>
        /// 获取门店下的商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="keyWords"></param>
        /// <returns></returns>
        public JsonResult SearchShopBranchProductAndSaleCount(string ids, string keyWords)
        {
            if (string.IsNullOrEmpty(ids))
                throw new MallException("非法参数");

            var memberCartInfo = new ShoppingCartInfo();
            long userId = 0;
            if (CurrentUser != null)
            {//如果已登陆取购物车数据
                memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id);
                userId = CurrentUser.Id;
            }

            var productSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff;
            var branchs = ids.Split(',').Select(p => long.Parse(p)).ToList();
            //购物车商品数
            var cartItemCount = ShopBranchApplication.GetShopBranchCartItemCount(userId, branchs);
            var result = branchs.Select(branch =>
            {
                var products = ShopBranchApplication.GetShopBranchProducts(new ShopBranchProductQuery
                {
                    PageSize = 4,
                    PageNo = 1,
                    ShopBranchId = branch,
                    KeyWords = keyWords,
                    ShopBranchProductStatus = ShopBranchSkuStatus.Normal
                });

                //过滤购物车 无效商品
                var cartQuantity = cartItemCount.ContainsKey(branch) ? cartItemCount[branch] : 0;
                var now = DateTime.Now;

                return new
                {
                    saleCount = ShopBranchApplication.GetShopBranchSaleCount(branch),
                    saleCountByMonth = ShopBranchApplication.GetShopBranchSaleCount(branch, now.AddDays(-30).Date, now),
                    cartQuantity = cartQuantity,
                    products = products.Models.Select(p => new ProductInfo
                    {
                        Id = p.Id,
                        ImagePath = p.ImagePath,
                        MinSalePrice = p.MinSalePrice,
                        ProductName = p.ProductName
                    }),
                    branchId = branch,
                    productSaleCountOnOff = productSaleCountOnOff == 1,
                    CommentScore = ShopBranchApplication.GetServiceMark(branch).ComprehensiveMark//综合评分
                };
            });

            return SuccessResult<dynamic>(data: result);
        }

        #endregion

        #region Tags

        public ActionResult Tags(int? id)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            ViewBag.QQMapKey = CommonConst.QQMapKey;
            if (!id.HasValue)
            {
                throw new Core.MallException("非法参数！");
            }
            var tag = ShopBranchApplication.GetShopBranchTagInfo(id.Value);
            if (null == tag)
            {
                throw new Core.MallException("门店标签已经被管理员删除 ！");
            }
            return View(tag);
        }

        /// <summary>
        /// 获取周边门店列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <param name="fromLatLng"></param>
        /// <param name="shopId"></param>
        /// <param name="tagsId">标签ID</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult TagsSearch(int pageSize, int pageNo, string fromLatLng, string shopId, long tagsId)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal;
            query.ShopBranchTagId = tagsId;
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.OrderKey = 2;
            query.OrderType = true;
            if (query.FromLatLng.Split(',').Length != 2)
                return Json(new Result { success = false, msg = "无法获取您的当前位置，请确认是否开启定位服务！" });

            string address = "", province = "", city = "", district = "", street = "";
            string currentPosition = string.Empty;//当前详情地址，优先顺序：建筑、社区、街道
            Region cityInfo = new Region();
            if (!string.IsNullOrWhiteSpace(shopId))//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (query.ShopId <= 0)
                    return Json(new Result { success = false, msg = "无法定位到商家！" });
            }
            else//否则取用户同城门店
            {
                var addressObj = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                    return Json(new Result { success = false, msg = "无法定位到城市！" });

                cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo != null)
                {
                    query.CityId = cityInfo.Id;
                }
                //处理当前地址

                currentPosition = street;
            }
            var shopBranchs = ShopBranchApplication.TagsSearchNearShopBranchs(query);
            //组装首页数据
            //补充门店活动数据
            var homepageBranchs = ProcessBranchHomePageData(shopBranchs.Models);

            return Json(new { success = true, data = new { Models = homepageBranchs, Total = shopBranchs.Total, CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name }, CurrentAddress = currentPosition } });
        }

        #endregion

        #region ProductStoreList

        public ActionResult ProductStoreList(int? id, long? shopId)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            ViewBag.QQMapKey = CommonConst.QQMapKey;
            if (!id.HasValue || !shopId.HasValue)
            {
                throw new Core.MallException("非法参数！");
            }
            ViewBag.ShopID = shopId.Value;
            ViewBag.ProductSaleCountOnOff = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;
            return View(id.Value);
        }

        /// <summary>
        /// 通过商品获取周边门店列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <param name="fromLatLng"></param>
        /// <param name="shopId"></param>
        /// <param name="productId">商品ID</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult StoresByProduct(int pageSize, int pageNo, string fromLatLng, string shopId, long productId)
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal;
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            query.ProductIds = new long[] { productId };
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.OrderKey = 2;
            query.OrderType = true;
            if (query.FromLatLng.Split(',').Length != 2)
                return Json(new Result { success = false, msg = "无法获取您的当前位置，请确认是否开启定位服务！" });

            string address = "", province = "", city = "", district = "", street = "";
            string currentPosition = string.Empty;//当前详情地址，优先顺序：建筑、社区、街道
            Region cityInfo = new Region();
            if (!string.IsNullOrWhiteSpace(shopId))//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (query.ShopId <= 0)
                    return Json(new Result { success = false, msg = "无法定位到商家！" });
            }
            else//否则取用户同城门店
            {
                var addressObj = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                    return Json(new Result { success = false, msg = "无法定位到城市！" });

                cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo != null)
                {
                    query.CityId = cityInfo.Id;
                }
                //处理当前地址

                currentPosition = street;
            }
            var shopBranchs = ShopBranchApplication.StoreByProductNearShopBranchs(query);
            //组装首页数据
            //补充门店活动数据
            var homepageBranchs = ProcessBranchHomePageData(shopBranchs.Models);

            return Json(new { success = true, data = new { Models = homepageBranchs, Total = shopBranchs.Total, CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name }, CurrentAddress = currentPosition } });
        }

        /// <summary>
        /// 取门店销量排名前的商品、门店月销量、门店加入购物车的商品数量
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetStoreByProductAndSaleCount(string ids, long? productId)
        {
            if (string.IsNullOrEmpty(ids))
            {
                throw new MallException("非法参数");
            }
            var memberCartInfo = new ShoppingCartInfo();
            var productSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;
            if (CurrentUser != null)
            {//如果已登陆取购物车数据
                memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id);
            }
            var branchs = ids.Split(',').Select(p => long.Parse(p));
            var result = branchs.Select(branch =>
            {
                var products = ShopBranchApplication.GetShopBranchProducts(new ShopBranchProductQuery
                {
                    PageSize = 4,
                    PageNo = 1,
                    OrderKey = 3,
                    ShopBranchId = branch,
                    ShopBranchProductStatus = ShopBranchSkuStatus.Normal
                }).Models;
                if (productId.HasValue)
                {
                    var product = products.FirstOrDefault(n => n.Id == productId.Value);
                    products.Remove(product);
                    var models = products.Take(3).ToList();
                    if (null != product)
                    {
                        models.Insert(0, product);
                    }
                    products = models;
                }
                var now = DateTime.Now;

                var cartQuantity = memberCartInfo.Items.Where(c => c.ShopBranchId == branch).Sum(c => c.Quantity);
                return new
                {
                    saleCount = ShopBranchApplication.GetShopBranchSaleCount(branch),
                    saleCountByMonth = ShopBranchApplication.GetShopBranchSaleCount(branch, now.AddDays(-30).Date, now.Date),
                    cartQuantity = cartQuantity,
                    products = products.Select(p => new ProductInfo
                    {
                        Id = p.Id,
                        ImagePath = p.ImagePath,
                        MinSalePrice = p.MinSalePrice,
                        ProductName = p.ProductName
                    }),
                    branchId = branch,
                    CommentScore = ShopBranchApplication.GetServiceMark(branch).ComprehensiveMark//综合评分
                };
            });
            return Json(new { success = true, data = result, productSaleCountOnOff = productSaleCountOnOff });
        }

        #endregion
    }
}