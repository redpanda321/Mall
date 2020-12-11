using AutoMapper;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;



namespace Mall.SmallProgAPI
{
    public class HomeController : BaseApiController
    {
        /// <summary>
        /// 获取首页数据
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetIndexData")]
        public object GetIndexData(string openId = "")
        {
            //CheckUserLogin();
            Entities.MemberInfo member = CurrentUser;
            var sitesetting = SiteSettingApplication.SiteSettings;
            string homejson = Request.Scheme + "://" + Request.Path + "/AppletHome/data/default.json";

            long vidnumber = sitesetting.XcxHomeVersionCode;
            return Json(new
            {
                HomeTopicPath = homejson,
                Vid = vidnumber,
                QQMapKey = CommonConst.QQMapKey
            });
        }
        /// <summary>
        /// 首页商品信息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetIndexProductData")]
        public  object GetIndexProductData(string openId = "", int pageIndex = 1, int pageSize = 10)
        {
            var homeProducts = ServiceProvider.Instance<IWXSmallProgramService>.Create.GetWXSmallHomeProducts(pageIndex, pageSize);
            decimal discount = 1M;
            long SelfShopId = 0;
            var CartInfo = new Mall.Entities.ShoppingCartInfo();
            var ids = homeProducts.Models.Select(p => p.Id).ToList();
            var productList = new List<dynamic>();
            var cartitems = new List<Mall.Entities.ShoppingCartItem>();
            long userId = 0;
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
                discount = CurrentUser.MemberDiscount;
                var shopInfo = ShopApplication.GetSelfShop();
                SelfShopId = shopInfo.Id;
                CartInfo = ServiceProvider.Instance<ICartService>.Create.GetCart(CurrentUser.Id);
                cartitems = CartApplication.GetCartQuantityByIds(CurrentUser.Id, ids);
            }

            foreach (var item in homeProducts.Models)
            {
                long activeId = 0;
                int activetype = 0;
                item.ImagePath = Core.MallIO.GetRomoteProductSizeImage(Core.MallIO.GetImagePath(item.ImagePath), 1, (int)Mall.CommonModel.ImageSize.Size_350);
                if (item.ShopId == SelfShopId)
                    item.MinSalePrice = item.MinSalePrice * discount;
                var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(item.Id);
                if (limitBuy != null)
                {
                    item.MinSalePrice = limitBuy.MinPrice;
                    activeId = limitBuy.Id;
                    activetype = 1;
                }
                int quantity = 0;
                quantity = cartitems.Where(d => d.ProductId == item.Id).Sum(d => d.Quantity);

                long stock = 0;

                var productInfo = ServiceProvider.Instance<IProductService>.Create.GetProduct(item.Id);
                if (productInfo != null)
                {
                    var skus = ProductManagerApplication.GetSKUs(productInfo.Id);
                    stock = skus.Sum(x => x.Stock);
                    if (productInfo.MaxBuyCount > 0)
                    {
                        stock = productInfo.MaxBuyCount;
                    }
                }
                if (productInfo.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited)
                {
                    var ChoiceProducts = new
                    {
                        ProductId = item.Id,
                        ProductName = item.ProductName,
                        SalePrice = item.MinSalePrice.ToString("0.##"),
                        ThumbnailUrl160 = item.ImagePath,
                        MarketPrice = item.MarketPrice.ToString("0.##"),
                        CartQuantity = quantity,
                        HasSKU = item.HasSKU,
                        SkuId = GetSkuIdByProductId(item.Id),
                        ActiveId = activeId,
                        ActiveType = activetype,//获取该商品是否参与活动
                        Stock = stock,
                        IsVirtual = item.ProductType == 1
                    };
                    productList.Add(ChoiceProducts);
                }
            }
            return Json(productList);
        }

        private string GetSkuIdByProductId(long productId = 0)
        {
            string skuId = "";
            if (productId > 0)
            {
                var Skus = ServiceProvider.Instance<IProductService>.Create.GetSKUs(productId);
                foreach (var item in Skus)
                {
                    skuId = item.Id;//取最后或默认
                }
            }
            return skuId;
        }

        /// <summary>
        /// 多门店首页
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetShopsIndexData")]
        public object GetShopsIndexData()
        {
            CheckOpenStore();

            var model = SlideApplication.GetShopBranchListSlide();
            var defaultImage = new Mall.DTO.SlideAdModel { };
            var adimgs = model.Where(e => e.TypeId == Entities.SlideAdInfo.SlideAdType.NearShopBranchSpecial);
            var siteinfo = SiteSettingApplication.SiteSettings;
            dynamic result = new ExpandoObject();
            result.QQMapKey = CommonConst.QQMapKey;
            result.TopSlide = model.Where(e => e.TypeId == Entities.SlideAdInfo.SlideAdType.NearShopBranchHome).ToList(); //顶部轮播图
            result.Menu = model.Where(e => e.TypeId == Entities.SlideAdInfo.SlideAdType.NearShopBranchIcon).ToList(); //菜单图
            result.ADImg1 = adimgs.Count() > 0 ? adimgs.ElementAt(0) : defaultImage;//广告图1
            result.ADImg2 = adimgs.Count() > 1 ? adimgs.ElementAt(1) : defaultImage;//广告图2
            result.ADImg3 = adimgs.Count() > 2 ? adimgs.ElementAt(2) : defaultImage;//广告图3
            result.ADImg4 = adimgs.Count() > 3 ? adimgs.ElementAt(3) : defaultImage;//广告图4
            result.ADImg5 = adimgs.Count() > 4 ? adimgs.ElementAt(4) : defaultImage;//广告图5
            result.MiddleSlide = model.Where(e => e.TypeId == Entities.SlideAdInfo.SlideAdType.NearShopBranchHome2).ToList(); //中间轮播图
            
            return Json(result);
        }

        #region 门店列表
        /// <summary>
        /// 门店列表
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetStoreList")]
        public object GetStoreList(string fromLatLng, string keyWords = "", long? tagsId = null, long? shopId = null, int pageNo = 1, int pageSize = 10)
        {
            //TODO:FG 异常查询 MysqlExecuted:226,耗时:1567.4137毫秒
            CheckOpenStore();
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal;
            query.ShopBranchName = keyWords.Trim();
            query.ShopBranchTagId = tagsId;
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.OrderKey = 2;
            query.OrderType = true;
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            if (query.FromLatLng.Split(',').Length != 2)
            {
                throw new MallException("无法获取您的当前位置，请确认是否开启定位服务！");
            }
            if (shopId.HasValue)
            {
                //var shop = ShopApplication.GetShopInfo(shopId.Value);
                var isFreeze = ShopApplication.IsFreezeShop(shopId.Value);
                if (isFreeze)
                {
                    return Json(ErrorResult<dynamic>(msg: "此店铺已冻结"));
                }
                else
                {
                    var isExpired = ShopApplication.IsExpiredShop(shopId.Value);
                    if (isExpired)
                    {
                        return Json(ErrorResult<dynamic>(msg: "此店铺已过期"));
                    }
                }
            }
            string address = "", province = "", city = "", district = "", street = "";
            string currentPosition = string.Empty;//当前详情地址，优先顺序：建筑、社区、街道
            Region cityInfo = new Region();
            if (shopId.HasValue)//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = shopId.Value;
                if (query.ShopId <= 0)
                {
                    throw new MallException("无法定位到商家！");
                }
            }
            else//否则取用户同城门店
            {
                var addressObj = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                {
                    city = province;
                }
                if (string.IsNullOrWhiteSpace(city))
                {
                    throw new MallException("无法定位到城市！");
                }
                cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo == null)
                {
                    throw new MallException("无法定位到城市！");
                }
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
            //AutoMapper.Mapper.CreateMap<HomePageShopBranch, HomeGetStoreListModel>();
            var homeStores = homepageBranchs.Map<List<HomeGetStoreListModel>>();
            long userId = 0;
            if (CurrentUser != null)
            {//如果已登陆取购物车数据
                //memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id);
                userId = CurrentUser.Id;
            }
            //统一处理门店购物车数量
            var cartItemCount = ShopBranchApplication.GetShopBranchCartItemCount(userId, homeStores.Select(e => e.ShopBranch.Id).ToList());
            foreach (var item in homeStores)
            {
                //商品
                ShopBranchProductQuery proquery = new ShopBranchProductQuery();
                proquery.PageSize = 4;
                proquery.PageNo = 1;
                proquery.OrderKey = 3;
                if (!string.IsNullOrWhiteSpace(keyWords))
                {
                    proquery.KeyWords = keyWords;
                }
                proquery.ShopBranchId = item.ShopBranch.Id;
                proquery.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                //proquery.FilterVirtualProduct = true;
                var pageModel = ShopBranchApplication.GetShopBranchProducts(proquery);
                var dtNow = DateTime.Now;
                //var saleCountByMonth = OrderApplication.GetSaleCount(dtNow.AddDays(-30).Date, dtNow, shopBranchId: proquery.ShopBranchId.Value);
                item.SaleCount = OrderApplication.GetSaleCount(shopBranchId: proquery.ShopBranchId.Value);
                item.SaleCountByMonth = ShopBranchApplication.GetShopBranchSaleCount(item.ShopBranch.Id, dtNow.AddDays(-30).Date, dtNow);
                item.ShowProducts = pageModel.Models.Select(p =>
                {
                    var comcount = CommentApplication.GetProductHighCommentCount(productId: p.Id, shopBranchId: proquery.ShopBranchId.Value);
                    return new HomeGetStoreListProductModel
                    {
                        Id = p.Id,
                        DefaultImage = MallIO.GetRomoteProductSizeImage(p.ImagePath, 1, ImageSize.Size_150.GetHashCode()),
                        MinSalePrice = p.MinSalePrice,
                        ProductName = p.ProductName,
                        HasSKU = p.HasSKU,
                        MarketPrice = p.MarketPrice,
                        SaleCount = Mall.Core.Helper.TypeHelper.ObjectToInt(p.VirtualSaleCounts) + OrderApplication.GetSaleCount(dtNow.AddDays(-30).Date, dtNow, shopBranchId: proquery.ShopBranchId.Value, productId: p.Id),
                        HighCommentCount = comcount,
                    };
                }).ToList();
                item.ProductCount = pageModel.Total;
                if (cartItemCount != null)
                {
                    item.CartQuantity = cartItemCount.ContainsKey(item.ShopBranch.Id) ? cartItemCount[item.ShopBranch.Id] : 0;
                }
                //评分
                item.CommentScore = ShopBranchApplication.GetServiceMark(item.ShopBranch.Id).ComprehensiveMark;
            }
            return Json(new
            {
                Total = shopBranchs.Total,
                CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name },
                CurrentAddress = currentPosition,
                Stores = homeStores,
                ProductSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1
            });
        }

        /// <summary>
        /// 根据商品查找门店
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetStoresByProduct")]
        public object GetStoresByProduct(string fromLatLng, long productId, long? shopId = null, int pageNo = 1, int pageSize = 10)
        {
            CheckOpenStore();
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
            //query.FilterVirtualProduct = true;
            if (query.FromLatLng.Split(',').Length != 2)
            {
                throw new MallException("无法获取您的当前位置，请确认是否开启定位服务！");
            }

            string address = "", province = "", city = "", district = "", street = "";
            string currentPosition = string.Empty;//当前详情地址，优先顺序：建筑、社区、街道
            Region cityInfo = new Region();
            if (shopId.HasValue)//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = shopId.Value;
                if (query.ShopId <= 0)
                {
                    throw new MallException("无法定位到商家！");
                }
            }
            else//否则取用户同城门店
            {
                var addressObj = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                {
                    throw new MallException("无法定位到城市！");
                }
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
            //AutoMapper.Mapper.CreateMap<HomePageShopBranch, HomeGetStoreListModel>();
            var homeStores = homepageBranchs.Map<List<HomeGetStoreListModel>>();
            long userId = 0;
            if (CurrentUser != null)
            {
                //如果已登陆取购物车数据
                //memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id);
                userId = CurrentUser.Id;
            }

            var cartItemCount = ShopBranchApplication.GetShopBranchCartItemCount(userId, homeStores.Select(e => e.ShopBranch.Id).ToList());
            foreach (var item in homeStores)
            {
                //商品
                ShopBranchProductQuery proquery = new ShopBranchProductQuery();
                proquery.PageSize = 4;
                proquery.PageNo = 1;
                proquery.OrderKey = 3;
                proquery.ShopBranchId = item.ShopBranch.Id;
                proquery.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                //proquery.FilterVirtualProduct = true;
                var pageModel = ShopBranchApplication.GetShopBranchProducts(proquery);
                if (productId > 0)
                {
                    var product = pageModel.Models.FirstOrDefault(n => n.Id == productId);
                    pageModel.Models.Remove(product);
                    var models = pageModel.Models.OrderByDescending(p => p.SaleCounts).ThenByDescending(p => p.Id).Take(3).ToList();
                    if (null != product)
                    {
                        models.Insert(0, product);
                    }
                    pageModel.Models = models;
                }
                var dtNow = DateTime.Now;
                //var saleCountByMonth = OrderApplication.GetSaleCount(dtNow.AddDays(-30).Date, dtNow, shopBranchId: proquery.ShopBranchId.Value);
                item.SaleCount = OrderApplication.GetSaleCount(shopBranchId: proquery.ShopBranchId.Value);
                item.SaleCountByMonth = ShopBranchApplication.GetShopBranchSaleCount(item.ShopBranch.Id, dtNow.AddDays(-30).Date, dtNow);
                item.ShowProducts = pageModel.Models.Select(p => new HomeGetStoreListProductModel
                {
                    Id = p.Id,
                    DefaultImage = MallIO.GetRomoteProductSizeImage(p.ImagePath, 1, ImageSize.Size_150.GetHashCode()),
                    MinSalePrice = p.MinSalePrice,
                    ProductName = p.ProductName,
                    HasSKU = p.HasSKU,
                    MarketPrice = p.MarketPrice
                }).ToList();
                item.ProductCount = pageModel.Total;
                if (cartItemCount != null)
                {
                    item.CartQuantity = cartItemCount.ContainsKey(item.ShopBranch.Id) ? cartItemCount[item.ShopBranch.Id] : 0;
                }

                //评分
                item.CommentScore = ShopBranchApplication.GetServiceMark(item.ShopBranch.Id).ComprehensiveMark;
            }
            return Json(new
            {
                Total = shopBranchs.Total,
                CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name },
                CurrentAddress = currentPosition,
                Stores = homeStores,
                ProductSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1
            });
        }

        private List<HomePageShopBranch> ProcessBranchHomePageData(List<ShopBranch> list, bool isAllCoupon = false)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
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
                var settings = service.GetSettingsByCoupon(coupons.Select(p => p.Id).ToList());
                var couponList = coupons.Where(d => settings.Any(c => c.CouponID==d.Id &&  c.PlatForm == PlatformType.Wap));
                var appCouponlist = new List<CouponModel>();
                foreach (var couponinfo in couponList)
                {
                    var coupon = new CouponModel();
                    var status = 0;
                    long userid = 0;
                    if (CurrentUser != null)
                    {
                        userid = CurrentUser.Id;
                    }
                    //当前优惠券的可领状态
                    status = ShopBranchApplication.CouponIsUse(couponinfo, userid);

                    coupon.Id = couponinfo.Id;
                    coupon.CouponName = couponinfo.CouponName;
                    coupon.ShopId = couponinfo.ShopId;
                    coupon.OrderAmount = couponinfo.OrderAmount.ToString("F2");
                    coupon.Price = Math.Round(couponinfo.Price, 2);
                    coupon.StartTime = couponinfo.StartTime;
                    coupon.EndTime = couponinfo.EndTime;
                    coupon.IsUse = status;
                    coupon.UseArea = couponinfo.UseArea;
                    coupon.Remark = couponinfo.Remark;
                    appCouponlist.Add(coupon);
                }
                actives.ShopCoupons = appCouponlist.OrderBy(d => d.Price).ToList();
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
                    shop.ShopAllActives = new ShopActiveList
                    {
                        ShopActives = actives.ShopActives,
                        ShopCoupons = actives.ShopCoupons,
                        FreeFreightAmount = shop.ShopBranch.IsFreeMail ? shop.ShopBranch.FreeMailFee : 0,
                        IsFreeMail = shop.ShopBranch.IsFreeMail
                    };
                }
            }
            return homepageBranchs;
        }
        #endregion


        /// <summary>
        /// 获取门店标签信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetTagsInfo")]
        public object GetTagsInfo(long id)
        {
            var tag = ShopBranchApplication.GetShopBranchTagInfo(id);
            if (null == tag)
            {
                throw new MallException("非法参数！");
            }
            return Json(new
            {
                Id = tag.Id,
                Title = tag.Title,
                ShopBranchCount = tag.ShopBranchCount
            });
        }

        #region 门店

        /// <summary>
        /// 获取门店信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetStoreInfo")]
        public object GetStoreInfo(long id, string fromLatLng = "")
        {
            CheckOpenStore();
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);
            if (shopBranch == null)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "id");
            }
            var shop = ShopApplication.GetShop(shopBranch.ShopId);
            if (null != shop && shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired)
                return Json(ErrorResult<dynamic>("此店铺已过期"));
            if (null != shop && shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze)
                return Json(ErrorResult<dynamic>("此店铺已冻结"));
            if (!string.IsNullOrWhiteSpace(fromLatLng))
            {
                shopBranch.Distance = ShopBranchApplication.GetLatLngDistances(fromLatLng, string.Format("{0},{1}", shopBranch.Latitude, shopBranch.Longitude));
            }
            shopBranch.AddressDetail = ShopBranchApplication.RenderAddress(shopBranch.AddressPath, shopBranch.AddressDetail, 2);
            shopBranch.ShopImages = MallIO.GetRomoteImagePath(shopBranch.ShopImages);
            //Mapper.CreateMap<ShopBranch, HomeGetShopBranchInfoModel>();
            var store = shopBranch.Map< HomeGetShopBranchInfoModel>();
            var homepageBranch = ProcessBranchHomePageData(new List<ShopBranch>() { shopBranch }, true).FirstOrDefault();
            //过滤不能领取的优惠券
            homepageBranch.ShopAllActives.ShopCoupons = homepageBranch.ShopAllActives.ShopCoupons.ToList();
            //统计门店访问人数
            StatisticApplication.StatisticShopBranchVisitUserCount(shopBranch.ShopId, shopBranch.Id);
            return Json(new
            {
                Store = store,
                homepageBranch.ShopAllActives,
                CommentScore = ShopBranchApplication.GetServiceMark(store.Id).ComprehensiveMark,   //评分
            });
        }
        /// <summary>
        /// 获取商铺分类
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetShopCategory")]
        public object GetShopCategory(long shopId, long pid = 0, long shopBranchId = 0)
        {
            var cate = ShopCategoryApplication.GetCategoryByParentId(pid, shopId);
            if (shopBranchId > 0)
            {
                //屏蔽没有商品的分类
                List<long> noshowcid = new List<long>();
                foreach (var item in cate)
                {
                    ShopBranchProductQuery query = new ShopBranchProductQuery();
                    query.PageSize = 1;
                    query.PageNo = 1;
                    query.ShopId = shopId;
                    query.ShopBranchId = shopBranchId;
                    query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                    query.ShopCategoryId = item.Id;
                    //query.FilterVirtualProduct = true;//过滤虚拟商品
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
            }
            return Json(cate);
        }

        #endregion

        #region 评价
        /// <summary>
        /// 评价聚合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetCommentCountAggregate")]
        public  object  GetCommentCountAggregate(long id)
        {
            var data = CommentApplication.GetProductCommentStatistic(shopBranchId: id);
            return Json(data);
        }
        /// <summary>
        /// 获取评价
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetComments")]
        public object GetComments([FromQuery]ProductCommentQuery query)
        {
            if (query.PageNo == 0) query.PageNo = 1;
            if (query.PageSize == 0) query.PageSize = 5;
            var data = CommentApplication.GetProductComments(query);
           // AutoMapper.Mapper.CreateMap<ProductComment, HomeGetCommentListModel>();
            var datalist = data.Models.Map<List<HomeGetCommentListModel>>();
            var users = MemberApplication.GetMembers(datalist.Select(d => d.UserId).ToList());
            var products = ProductManagerApplication.GetAllProductByIds(datalist.Select(d => d.ProductId).ToList());
            //补充数据信息
            foreach (var item in datalist)
            {
                var u = users.FirstOrDefault(d => d.Id == item.UserId);
                var product = products.FirstOrDefault(d => d.Id == item.ProductId);
                if (u != null)
                {
                    item.UserPhoto = Mall.Core.MallIO.GetRomoteImagePath(u.Photo);
                }
                if (product != null)
                {
                    item.ProductName = product.ProductName;
                }
                //规格
                var sku = ProductManagerApplication.GetSKU(item.SkuId);
                if (sku != null)
                {
                    List<string> skucs = new List<string>();
                    if (!string.IsNullOrWhiteSpace(sku.Color))
                    {
                        skucs.Add(sku.Color);
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Size))
                    {
                        skucs.Add(sku.Size);
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Version))
                    {
                        skucs.Add(sku.Version);
                    }
                    item.SKU = string.Join("+", skucs);
                }
                foreach (var pitem in item.Images)
                {
                    pitem.CommentImage = MallIO.GetRomoteImagePath(pitem.CommentImage);
                }
            }
            return Json(new { total = data.Total, rows = datalist });
        }
        #endregion

        /// <summary>
        /// 获取分类
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetAllCategories")]
        public object GetAllCategories()
        {
            var categories = CategoryApplication.GetMainCategory();
            var model = categories.Where(p => p.IsShow).OrderBy(c => c.DisplaySequence).Select(c => new
            {
                cid = c.Id,
                name = c.Name,
                subs = CategoryApplication.GetCategoryByParentId(c.Id).Select(a => new
                {
                    cid = a.Id,
                    name = a.Name
                })
            }).ToList();
            var result = SuccessResult<dynamic>(data: model);
            return Json(result);
        }

        /// <summary>
        /// 是否授权门店
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("IsOpenStore")]
        public  object IsOpenStore()
        {
            #region 是否开启门店授权
            var isopenstore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            var IsOpenStore = isopenstore ? 1 : 0;
            #endregion
            return Json(IsOpenStore);
        }
    }
}
