using AutoMapper;
using Mall.API.Model;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.API
{
    public class ShopBranchWebController : BaseApiController
    {
        /// <summary>
        /// 多门店首页
        /// </summary>
        /// <returns></returns>

        [HttpGet("GetIndexData")]
        public object GetIndexData()
        {
            CheckOpenStore();

            var model = SlideApplication.GetShopBranchListSlide();
            var defaultImage = new Mall.DTO.SlideAdModel { };
            var adimgs = model.Where(e => e.TypeId == Entities.SlideAdInfo.SlideAdType.NearShopBranchSpecial);
            return new
            {
                success = true,
                QQMapKey = CommonConst.QQMapKey,
                TopSlide = model.Where(e => e.TypeId == Entities.SlideAdInfo.SlideAdType.NearShopBranchHome).ToList(), //顶部轮播图
                Menu = model.Where(e => e.TypeId == Entities.SlideAdInfo.SlideAdType.NearShopBranchIcon).ToList(), //菜单图
                ADImg1 = adimgs.Count() > 0 ? adimgs.ElementAt(0) : defaultImage,//广告图1
                ADImg2 = adimgs.Count() > 1 ? adimgs.ElementAt(1) : defaultImage,//广告图2
                ADImg3 = adimgs.Count() > 2 ? adimgs.ElementAt(2) : defaultImage,//广告图3
                ADImg4 = adimgs.Count() > 3 ? adimgs.ElementAt(3) : defaultImage,//广告图4
                ADImg5 = adimgs.Count() > 4 ? adimgs.ElementAt(4) : defaultImage,//广告图5
                MiddleSlide = model.Where(e => e.TypeId == Entities.SlideAdInfo.SlideAdType.NearShopBranchHome2).ToList(), //中间轮播图
            };
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
            var shopBranchs = ShopBranchApplication.SearchNearShopBranchs(query);
            //组装首页数据
            //补充门店活动数据
            var homepageBranchs = ProcessBranchHomePageData(shopBranchs.Models);
            //AutoMapper.Mapper.CreateMap<HomePageShopBranch, ShopBranchWebGetStoreListModel>();
            var homeStores = homepageBranchs.Map< List<ShopBranchWebGetStoreListModel>>();
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
                //TODO:FG 循环内查询 单请求查询100+
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
                var pageModel = ShopBranchApplication.GetShopBranchProducts(proquery);
                var dtNow = DateTime.Now;
                //var saleCountByMonth = OrderApplication.GetSaleCount(dtNow.AddDays(-30).Date, dtNow, shopBranchId: proquery.ShopBranchId.Value);
                item.SaleCount = OrderApplication.GetSaleCount(shopBranchId: proquery.ShopBranchId.Value);
                item.SaleCountByMonth = ShopBranchApplication.GetShopBranchSaleCount(item.ShopBranch.Id, dtNow.AddDays(-30).Date, dtNow);
                item.CommentScore = ShopBranchApplication.GetServiceMark(item.ShopBranch.Id).ComprehensiveMark;//综合评分
                item.ShowProducts = pageModel.Models.Select(p => new ShopBranchWebGetStoreListProductModel
                {
                    Id = p.Id,
                    DefaultImage = MallIO.GetRomoteProductSizeImage(p.ImagePath, 1, ImageSize.Size_150.GetHashCode()),
                    MinSalePrice = p.MinSalePrice,
                    ProductName = p.ProductName,
                    HasSKU = p.HasSKU,
                    MarketPrice = p.MarketPrice
                }).ToList();
                item.ProductCount = pageModel.Total;
                if (cartItemCount != null && cartItemCount.Count > 0)
                {
                    item.CartQuantity = cartItemCount.ContainsKey(item.ShopBranch.Id) ? cartItemCount[item.ShopBranch.Id] : 0;
                }
            }
            return new
            {
                success = true,
                total = shopBranchs.Total,
                Stores = homeStores,
                CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name },
                CurrentAddress = currentPosition,
                ProductSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1
            };
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
            //AutoMapper.Mapper.CreateMap<HomePageShopBranch, ShopBranchWebGetStoreListModel>();
            var homeStores = homepageBranchs.Map<List<ShopBranchWebGetStoreListModel>>();
      
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
                proquery.ShopBranchId = item.ShopBranch.Id;
                proquery.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
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
                item.CommentScore = ShopBranchApplication.GetServiceMark(item.ShopBranch.Id).ComprehensiveMark;//综合评分
                item.ShowProducts = pageModel.Models.Select(p => new ShopBranchWebGetStoreListProductModel
                {
                    Id = p.Id,
                    DefaultImage = MallIO.GetRomoteProductSizeImage(p.ImagePath, 1, ImageSize.Size_150.GetHashCode()),
                    MinSalePrice = p.MinSalePrice,
                    ProductName = p.ProductName,
                    HasSKU = p.HasSKU,
                    MarketPrice = p.MarketPrice
                }).ToList();
                item.ProductCount = pageModel.Total;
                if (cartItemCount != null && cartItemCount.Count > 0)
                {
                    item.CartQuantity = cartItemCount.ContainsKey(item.ShopBranch.Id) ? cartItemCount[item.ShopBranch.Id] : 0;
                }
            }
            return new
            {
                success = true,
                total = shopBranchs.Total,
                CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name },
                CurrentAddress = currentPosition,
                Stores = homeStores,
                ProductSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1
            };
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
                var couponList = coupons.Where(d => settings.Any(c => d.Id==c.CouponID && c.PlatForm == PlatformType.Wap)).ToList();
                var appCouponlist = new List<CouponModel>();
                foreach (CouponInfo couponinfo in couponList)
                {
                    var coupon = new CouponModel();
                    var status = 0;
                    if (CurrentUser != null)
                    {
                        status = ShopBranchApplication.CouponIsUse(couponinfo, CurrentUser.Id);
                    }
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
                actives.ShopCoupons = appCouponlist;
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
                        FreeFreightAmount = shop.ShopBranch.FreeMailFee,
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
            return new
            {
                success = true,
                Id = tag.Id,
                Title = tag.Title,
                ShopBranchCount = tag.ShopBranchCount
            };
        }

        #region 商品详情
        /// <summary>
        /// 获取商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="partnerid"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetProductDetail")]
        public object GetProductDetail(long id, long shopBranchId)
        {
            CheckOpenStore();
            var _iProductService = ServiceApplication.Create<IProductService>();
            var _iShopService = ServiceApplication.Create<IShopService>();
            var _iTypeService = ServiceApplication.Create<ITypeService>();
          
            var product = _iProductService.GetProduct(id);
            if (product == null || product.IsDeleted)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "该商品已被删除或者转移");
            }
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranch == null)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "shopBranchId");
            }
            if (shopBranch.Status== ShopBranchStatus.Freeze)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "门店已冻结");
            }
            if (!ShopBranchApplication.CheckProductIsExist(shopBranchId, id))
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "该商品已被删除或者转移");
            }
            Mall.Entities.ShoppingCartInfo memberCartInfo = null;
            if (CurrentUser != null)
            {
                //如果已登陆取购物车数据
                memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id);
            }

            ProductDetailModelForMobie model = new ProductDetailModelForMobie()
            {
                Product = new ProductInfoModel(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };
            Entities.ShopInfo shop = null;

            product = ServiceProvider.Instance<IProductService>.Create.GetProduct(id);

            var cashDepositModel = ServiceProvider.Instance<ICashDepositsService>.Create.GetCashDepositsObligation(product.Id);//提供服务（消费者保障、七天无理由、及时发货）
            model.CashDepositsServer = cashDepositModel;
            #region 根据运费模板获取发货地址
            var freightTemplateService = ServiceApplication.Create<IFreightTemplateService>();
            FreightTemplateInfo template = new FreightTemplateInfo ();
            if (product.ProductType == 0)
            {
                template=freightTemplateService.GetFreightTemplate(product.FreightTemplateId);
            }
            string productAddress = string.Empty;
            if (template != null )
            {
                var fullName = ServiceApplication.Create<IRegionService>().GetFullName(template.SourceAddress);
                if (fullName != null)
                {
                    var ass = fullName.Split(' ');
                    if (ass.Length >= 2)
                    {
                        productAddress = ass[0] + " " + ass[1];
                    }
                    else
                    {
                        productAddress = ass[0];
                    }
                }
            }

            model.ProductAddress = productAddress;
            model.FreightTemplate = template;
            #endregion
            #region 店铺Logo
            long vShopId;
            shop = ServiceProvider.Instance<IShopService>.Create.GetShop(product.ShopId);
            var vshopinfo = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
                vShopId = -1;
            else
                vShopId = vshopinfo.Id;
            model.Shop.VShopId = vShopId;
            model.VShopLog = ServiceProvider.Instance<IVShopService>.Create.GetVShopLog(model.Shop.VShopId);
            #endregion

            model.Shop.FavoriteShopCount = ServiceProvider.Instance<IShopService>.Create.GetShopFavoritesCount(product.ShopId);//关注人数

            var comment = CommentApplication.GetProductCommentStatistic(productId: id,
                        shopBranchId: shopBranchId);

            var branchskuList = ShopBranchApplication.GetSkus(shopBranch.ShopId, shopBranch.Id, null);
            if (branchskuList == null || branchskuList.Count <= 0)
            {
                throw new Mall.Core.MallException("门店商品不存在");
            }
          

            #region 商品SKU

            Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            if (product != null)
            {
                colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
                sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
                versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
            }
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            if (skus.Count > 0 && branchskuList.Count > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in skus)
                {
                    var specs = sku.Id.Split('_');
                    if (!branchskuList.Any(x => x.SkuId == sku.Id))
                    {
                        continue;
                    }
                    if (specs.Count() > 0 && !string.IsNullOrEmpty(sku.Color))
                    {
                        if (long.TryParse(specs[1], out colorId)) { }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = skus.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = Mall.Core.MallIO.GetRomoteImagePath(sku.ShowPic)
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1 && !string.IsNullOrEmpty(sku.Size))
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = skus.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size
                                });
                            }
                        }
                    }

                    if (specs.Count() > 2 && !string.IsNullOrEmpty(sku.Version))
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = skus.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }

            }
            #endregion

            #region 店铺
            shop = ServiceProvider.Instance<IShopService>.Create.GetShop(product.ShopId);
            var mark = Web.Framework.ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;

            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = CommentApplication.GetProductAverageMark(id);
            model.Shop.Id = product.ShopId;
            //model.Shop.FreeFreight = shop.FreeFreight;
            //TODO:lly 如果门店不包邮，则默认满邮金额为0
            model.Shop.FreeFreight = shopBranch.IsFreeMail ? shopBranch.FreeMailFee : 0;//这里应该取门店的满邮金额
            model.Shop.ProductNum = ServiceProvider.Instance<IProductService>.Create.GetShopOnsaleProducts(product.ShopId);

            var shopStatisticOrderComments = ServiceProvider.Instance<IShopService>.Create.GetShopStatisticOrderComments(product.ShopId);

            var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey ==
                Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionPeer).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudePeer).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where(c => c.CommentKey ==
                Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedPeer).FirstOrDefault();

            var productAndDescriptionMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionMax).FirstOrDefault();
            var productAndDescriptionMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionMin).FirstOrDefault();

            var sellerServiceAttitudeMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudeMax).FirstOrDefault();
            var sellerServiceAttitudeMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudeMin).FirstOrDefault();

            var sellerDeliverySpeedMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedMax).FirstOrDefault();
            var sellerDeliverySpeedMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedMin).FirstOrDefault();

            decimal defaultValue = 5;
            //宝贝与描述
            if (productAndDescription != null && productAndDescriptionPeer != null)
            {
                model.Shop.ProductAndDescription = productAndDescription.CommentValue;
            }
            else
            {
                model.Shop.ProductAndDescription = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                model.Shop.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
            }
            else
            {
                model.Shop.SellerServiceAttitude = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                model.Shop.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
            }
            else
            {
                model.Shop.SellerDeliverySpeed = defaultValue;
            }
            if (ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id) == null)
                model.Shop.VShopId = -1;
            else
                model.Shop.VShopId = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id).Id;

            //优惠券
            var couponCount = ServiceProvider.Instance<ICouponService>.Create.GetUserCouponCount(shop.Id);//取设置的优惠券
            if (couponCount > 0)
            {
                model.Shop.CouponCount = couponCount;
            }

            // 客服
            var customerServices = CustomerServiceApplication.GetMobileCustomerServiceAndMQ(shop.Id);
            #endregion

            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations(id);
            var comments = CommentApplication.GetCommentsByProduct(product.Id);
            var total = comments.Count;
            var niceTotal = comments.Count(item => item.ReviewMark >= 4);
            bool isFavorite = false;
            bool IsFavoriteShop = false;
            decimal discount = 1M;
            if (CurrentUser == null)
            {
                isFavorite = false;
                IsFavoriteShop = false;
            }
            else
            {
                isFavorite = ServiceProvider.Instance<IProductService>.Create.IsFavorite(product.Id, CurrentUser.Id);
                var favoriteShopIds = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                IsFavoriteShop = favoriteShopIds.Contains(product.ShopId);
                if (shop.IsSelf)
                {
                    discount = CurrentUser.MemberDiscount;
                }
            }

            var productImage = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                if (Core.MallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    var path = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, i, (int)Mall.CommonModel.ImageSize.Size_350);
                    productImage.Add(path);
                }
            }
            //File.Exists(HttpContext.Current.Server.MapPath(product.ImagePath + string.Format("/{0}.png", 1)));
            decimal minSalePrice = shop.IsSelf ? product.MinSalePrice * discount : product.MinSalePrice;
            bool isValidLimitBuy = false;
            bool isFightGroupActive = false;
            var activeInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(product.Id);
            if (activeInfo != null && activeInfo.ActiveStatus > FightGroupActiveStatus.Ending)
            {
                isFightGroupActive = true;
            }
            var desc = ProductManagerApplication.GetProductDescription(product.Id);
            model.Product = new ProductInfoModel()
            {
                ProductId = product.Id,
                CommentCount = comment.AllComment,
                Consultations = consultations.Count(),
                ImagePath = productImage,
                IsFavorite = isFavorite,
                MarketPrice = product.MarketPrice,
                MinSalePrice = minSalePrice,
                NicePercent = model.Shop.ProductMark == 0 ? 100 : (int)((niceTotal / total) * 100),
                ProductName = product.ProductName,
                ProductSaleStatus = product.SaleStatus,
                AuditStatus = product.AuditStatus,
                ShortDescription = product.ShortDescription,
                ProductDescription = GetProductDescription(desc),
                IsOnLimitBuy = false,
                SaleCounts = product.SaleCounts,
                MeasureUnit = product.MeasureUnit
            };
            #endregion
            

            #region  代金红包

            var bonus = ServiceProvider.Instance<IShopBonusService>.Create.GetByShopId(shop.Id);
            int BonusCount = 0;
            decimal BonusGrantPrice = 0;
            decimal BonusRandomAmountStart = 0;
            decimal BonusRandomAmountEnd = 0;

            if (bonus != null)
            {
                BonusCount = bonus.Count;
                BonusGrantPrice = bonus.GrantPrice;
                BonusRandomAmountStart = bonus.RandomAmountStart;
                BonusRandomAmountEnd = bonus.RandomAmountEnd;
            }

            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(id, shop.Id, shopBranchId);

            #endregion

            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            //统计门店访问人数
            StatisticApplication.StatisticShopBranchVisitUserCount(shopBranch.ShopId, shopBranch.Id);
            #region 购物车总量
            int cartcount = 0;
            if (memberCartInfo != null)
            {
                var shopcartinfo = memberCartInfo.Items.Where(d => d.ShopBranchId == shopBranchId && d.ProductId == id);
                var _iShopBranchService = ServiceApplication.Create<IShopBranchService>();
                foreach (var cartitem in shopcartinfo)
                {
                    var branchskuInfo = branchskuList.FirstOrDefault(x => x.SkuId == cartitem.SkuId);
                    if (branchskuInfo.Status == ShopBranchSkuStatus.Normal && branchskuInfo.Stock >= cartitem.Quantity)
                    {
                        cartcount += cartitem.Quantity;
                    }
                }
            }
            #endregion

            //获取商品月销量（不包含当天销量）
            var dtNow = DateTime.Now;
            model.Product.SaleCounts = ShopBranchApplication.GetProductSaleCount(shopBranchId, product.Id, dtNow.AddDays(-30).Date, dtNow);
            model.Product.SaleCounts = model.Product.SaleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(product.VirtualSaleCounts);

            #region 虚拟商品
            var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(product.Id);
            VirtualProductModel virtualPInfo = null;
            if (virtualProductInfo != null)
            {
                virtualPInfo = new Model.VirtualProductModel()
                {
                    EndDate = virtualProductInfo.EndDate.HasValue ? virtualProductInfo.EndDate.Value.ToString("yyyy-MM-dd") : "",
                    StartDate = virtualProductInfo.StartDate.HasValue ? virtualProductInfo.StartDate.Value.ToString("yyyy-MM-dd") : "",
                    SupportRefundType = virtualProductInfo.SupportRefundType,
                    EffectiveType = virtualProductInfo.EffectiveType,
                    Hour = virtualProductInfo.Hour,
                    UseNotice = virtualProductInfo.UseNotice,
                    ValidityType = virtualProductInfo.ValidityType ? 1 : 0,
                    IsOverdue = virtualProductInfo.ValidityType && DateTime.Now >virtualProductInfo.EndDate.Value
                };
            }

            var virtualProductItem = ProductManagerApplication.GetVirtualProductItemInfoByProductId(product.Id);
            List<VirtualProductItemModel> virtualProductItemModels = null;
            if (virtualProductItem != null)
            {
                virtualProductItemModels = new List<VirtualProductItemModel>();
                virtualProductItem.ForEach(a =>
                {
                    virtualProductItemModels.Add(new VirtualProductItemModel()
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Required = a.Required,
                        Type = a.Type
                    });
                });
            }
            #endregion
            return new
            {
                success = true,
                IsOnLimitBuy = isValidLimitBuy,
                IsFightGroupActive = isFightGroupActive,
                ActiveId = isFightGroupActive ? activeInfo.Id : 0,
                ActiveStatus = activeInfo != null ? activeInfo.ActiveStatus.GetHashCode() : 0,
                MaxSaleCount = 0,
                Title = string.Empty,
                Second = 0,
                Product = model.Product,
                CashDepositsServer = model.CashDepositsServer,//提供服务（消费者保障、七天无理由、及时发货）
                ProductAddress = model.ProductAddress,//发货地址
                Free = model.FreightTemplate.IsFree == FreightTemplateType.Free ? "免运费" : "",//是否免运费
                VShopLogo = Mall.Core.MallIO.GetRomoteImagePath(model.VShopLog),
                Shop = model.Shop,
                ShopBranch = new
                {
                    shopBranch.Id,
                    shopBranch.ShopBranchName,
                    shopBranch.StoreOpenStartTime,
                    shopBranch.StoreOpenEndTime,
                    shopBranch.AddressFullName,
                    shopBranch.IsFreeMail,
                },
                IsFavoriteShop = IsFavoriteShop,
                Color = model.Color.OrderBy(p => p.SkuId),
                Size = model.Size.OrderBy(p => p.SkuId),
                Version = model.Version.OrderBy(p => p.SkuId),
                BonusCount = BonusCount,
                BonusGrantPrice = BonusGrantPrice,
                BonusRandomAmountStart = BonusRandomAmountStart,
                BonusRandomAmountEnd = BonusRandomAmountEnd,
                fullDiscount = fullDiscount,
                ColorAlias = colorAlias,
                SizeAlias = sizeAlias,
                VersionAlias = versionAlias,
                userId = CurrentUser == null ? 0 : CurrentUser.Id,
                IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore,
                CustomerServices = customerServices,
                CartCount = cartcount,
                ProductSaleCountOnOff = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1,
                Status = (branchskuList.Any(d => d.Status == ShopBranchSkuStatus.Normal) ? ((branchskuList.Sum(d => d.Stock) > 0 ? 0 : 2)) : 3),
                SaleCounts = model.Product.SaleCounts,    //销量
                Price = ProductWebApplication.GetProductPriceStr2(product, skus, discount),//最小价或区间价文本
                ProductType = product.ProductType,
                VirtualProductInfo = virtualPInfo,
                VirtualProductItemModels = virtualProductItemModels,
            };
        }

        /// <summary>
        /// 将商品关联版式组合商品描述
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetProductDescription")]
        private string GetProductDescription(ProductDescriptionInfo productDescription)
        {
            if (productDescription == null)
            {
                throw new Mall.Core.MallException("错误的商品信息");
            }
            string descriptionPrefix = "", descriptiondSuffix = "";//顶部底部版式
            string description = productDescription.ShowMobileDescription.Replace("src=\"/Storage/", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/") + "/");//商品描述
            var product = ProductManagerApplication.GetProduct(productDescription.ProductId);
            var iprodestempser = ServiceApplication.Create<IProductDescriptionTemplateService>();
            if (productDescription.DescriptionPrefixId != 0)
            {
                var desc = iprodestempser.GetTemplate(productDescription.DescriptionPrefixId, product.ShopId);
                descriptionPrefix = desc == null ? "" : desc.MobileContent.Replace("src=\"/Storage/", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/") + "/");
            }

            if (productDescription.DescriptiondSuffixId != 0)
            {
                var desc = iprodestempser.GetTemplate(productDescription.DescriptiondSuffixId, product.ShopId);
                descriptiondSuffix = desc == null ? "" : desc.MobileContent.Replace("src=\"/Storage/", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/") + "/");
            }

            return string.Format("{0}{1}{2}", descriptionPrefix, description, descriptiondSuffix);
        }
        /// <summary>
        /// 获取门店商品规格
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="bid"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetSKUInfo")]
        public object GetSKUInfo(long pId, long bid)
        {
            var _iProductService = ServiceApplication.Create<IProductService>();
            var _iShopBranchService = ServiceApplication.Create<IShopBranchService>();
            var _iBranchCartService = ServiceApplication.Create<IBranchCartService>();
            var product = _iProductService.GetProduct(pId);
            var shopBranchInfo = _iShopBranchService.GetShopBranchById(bid);
            var branchskuList = ShopBranchApplication.GetSkus(shopBranchInfo.ShopId, bid);
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            Mall.Entities.ShoppingCartInfo memberCartInfo = null;
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                //如果已登陆取购物车数据
                memberCartInfo = _iBranchCartService.GetCart(CurrentUser.Id, bid);
                if (shopInfo.IsSelf)
                {
                    discount = CurrentUser.MemberDiscount;
                }
            }

            var skuArray = new List<ProductSKUModel>();
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            foreach (var sku in skus)
            {
                decimal price = 1M;
                if (shopInfo.IsSelf)
                    price = sku.SalePrice * discount;
                else
                    price = sku.SalePrice;

                if (branchskuList.Count(x => x.SkuId == sku.Id && x.Stock > 0) > 0)
                {
                    skuArray.Add(new ProductSKUModel
                    {
                        Price = price,
                        SkuId = sku.Id,
                        Stock = branchskuList.FirstOrDefault(x => x.SkuId == sku.Id).Stock,
                        cartCount = (memberCartInfo == null || memberCartInfo.Items.Count() == 0) ? 0 : memberCartInfo.Items.FirstOrDefault(x => x.SkuId == sku.Id) == null ? 0 : memberCartInfo.Items.FirstOrDefault(x => x.SkuId == sku.Id).Quantity
                    });
                }
            }
            return new { success = true, SkuArray = skuArray };
        }
        #endregion

        /// <summary>
        /// 获取门店信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetShopBranchInfo")]
        public object GetShopBranchInfo(long id, string fromLatLng = "")
        {
            CheckOpenStore();
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);
            if (shopBranch == null)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "id");
            }
            var shop = ShopApplication.GetShop(shopBranch.ShopId);
            if (null != shop && shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired)
                throw new MallApiException("此店铺已过期");
            if (null != shop && shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze)
                throw new MallApiException("此店铺已冻结");
            if (!string.IsNullOrWhiteSpace(fromLatLng))
            {
                shopBranch.Distance = ShopBranchApplication.GetLatLngDistances(fromLatLng, string.Format("{0},{1}", shopBranch.Latitude, shopBranch.Longitude));
            }
            shopBranch.AddressDetail = ShopBranchApplication.RenderAddress(shopBranch.AddressPath, shopBranch.AddressDetail, 2);
            shopBranch.ShopImages = MallIO.GetRomoteImagePath(shopBranch.ShopImages);
            shopBranch.CommentScore = ShopBranchApplication.GetServiceMark(id).ComprehensiveMark;
           // Mapper.CreateMap<ShopBranch, ShopBranchWebGetShopBranchInfoModel>();
            var store = shopBranch.Map<ShopBranchWebGetShopBranchInfoModel>();
            var homepageBranch = ProcessBranchHomePageData(new List<ShopBranch>() { shopBranch }, true).FirstOrDefault();
            //统计门店访问人数
            StatisticApplication.StatisticShopBranchVisitUserCount(shopBranch.ShopId, shopBranch.Id);
            return new
            {
                success = true,
                Store = store,
                homepageBranch.ShopAllActives,
            };
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
            return new
            {
                success = true,
                Categories = cate
            };
        }


        #region 搜索商品


        [HttpGet("GetProductList")]
        public object GetProductList(long shopId, long shopBranchId, string keyWords = "", long? productId = null, long? shopCategoryId = null, long? categoryId = null, int pageNo = 1, int pageSize = 10, int type = 0)
        {
            CheckOpenStore();
            if (shopId <= 0)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "shopId");
            }
            if (shopBranchId <= 0)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "shopBranchId");
            }
            ShopBranchProductQuery query = new ShopBranchProductQuery
            {
                PageSize = pageSize,
                PageNo = pageNo,
                KeyWords = keyWords,
                ShopId = shopId,
                ShopBranchId = shopBranchId,
                RproductId = productId,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal,
                OrderKey = 5
            };
            if (shopCategoryId.HasValue && shopCategoryId > 0)
            {
                query.ShopCategoryId = shopCategoryId;
            }
            if (categoryId.HasValue && categoryId > 0)
            {
                query.CategoryId = categoryId;
            }
            var _iBranchCartService = ServiceApplication.Create<IBranchCartService>();
            var _iLimitTimeBuyService = ServiceApplication.Create<ILimitTimeBuyService>();

            var now = DateTime.Now;
            query.StartDate = now.AddYears(-10).Date;
            query.EndDate = now;
            var pageModel = ShopBranchApplication.GetShopBranchProductsMonth(query, now.AddYears(-10).Date, now);
            Mall.Entities.ShoppingCartInfo cartInfo = new Mall.Entities.ShoppingCartInfo();
            if (CurrentUser != null)
            {
                cartInfo = new BranchCartHelper().GetCart(CurrentUser.Id, shopBranchId);//获取购物车数据
            }
            #region 置顶商品
            if (productId.HasValue && productId > 0 && pageNo == 1)
            {
                if (type == 1)
                    query.ShopCategoryId = null;
                query.RproductId = null;
                query.ProductId = productId;
                var topModel = ShopBranchApplication.GetShopBranchProductsMonth(query, now.AddDays(-30).Date, now);
                if (topModel.Models.Count() > 0)
                {
                    pageModel.Models.Insert(0, topModel.Models.FirstOrDefault());
                }
            }
            #endregion

            //获取门店活动
            var shopBranchs = ShopBranchApplication.GetShopBranchById(shopBranchId);

            if (pageModel.Models != null && pageModel.Models.Count > 0)
            {
                #region 处理商品 官方自营店会员折扣价。
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
                    item.Quantity = cartInfo != null ? cartInfo.Items.Where(d => d.ProductId == item.Id && d.ShopBranchId == shopBranchId).Sum(d => d.Quantity) : 0;
                }
                #endregion
            }
            var product = pageModel.Models.ToList().Select(item =>
            {
                var comment = CommentApplication.GetProductCommentStatistic(productId: item.Id,
                        shopBranchId: shopBranchId);
                var sbskus = ShopBranchApplication.GetSkusByProductId(shopBranchId, item.Id);
                return new
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    CategoryName = ShopCategoryApplication.GetCategoryByProductId(item.Id).Name,
                    MeasureUnit = item.MeasureUnit,
                    MinSalePrice = item.MinSalePrice.ToString("f2"),
                    SaleCounts = item.ShopBranchSaleCounts,//销量统计没有考虑订单支付完成。
                    MarketPrice = item.MarketPrice,
                    HasSku = item.HasSKU,
                    Quantity = item.Quantity,
                    IsTop = item.Id == productId,
                    DefaultImage = Core.MallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_350),
                    HighCommentCount = comment.HighComment,
                    Stock = sbskus.Sum(d => d.Stock),
                    IsVirtual = item.ProductType == 1
                };
            }).OrderByDescending(d => d.IsTop).ToList();

            return new
            {
                success = true,
                Products = product,
                total = pageModel.Total,
                isSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1
            };
        }
        #endregion


        /// <summary>
        /// 根据商品Id获取商品规格
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetProductSkuInfo")]
        public object GetProductSkuInfo(long id, long shopBranchId)
        {
            var _iProductService = ServiceApplication.Create<IProductService>();
            var _iBranchCartService = ServiceApplication.Create<IBranchCartService>();
            var _iTypeService = ServiceApplication.Create<ITypeService>();
            if (id <= 0)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "id");
            }
            if (shopBranchId <= 0)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "shopBranchId");
            }
            var product = _iProductService.GetProduct(id);
            var shop = ShopApplication.GetShop(product.ShopId);
            decimal discount = 1M;
            if (CurrentUser != null &&shop.IsSelf)
            {
                discount = CurrentUser.MemberDiscount;
            }

            var skuArray = new List<ProductSKUModel>();
            object defaultsku = new object();

            Mall.Entities.ShoppingCartInfo cartInfo = null;
            if (CurrentUser != null)
            {
                cartInfo = _iBranchCartService.GetCart(CurrentUser.Id, shopBranchId);//获取购物车数据
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
                                    UseAttributeImage = true,
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
                        CartQuantity = cartInfo != null ? cartInfo.Items.Where(d => d.SkuId == sku.Id && d.ShopBranchId == shopBranchId).Sum(d => d.Quantity) : 0,
                        ImageUrl = Core.MallIO.GetRomoteProductSizeImage(sku.ShowPic, 1, (int)ImageSize.Size_350)
                    };
                    Skus.Add(prosku);
                }
                defaultsku = Skus[0];
                #endregion
            }
            return new
            {
                success = true,
                data = new
                {
                    ProductId = id,
                    ProductName = product.ProductName,
                    ImageUrl = Core.MallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350), //GetImageFullPath(model.SubmitOrderImg),
                    Stock = skuArray.Sum(s => s.Stock),// skus.Sum(s => s.Stock),
                                                       //ActivityUrl = activetype,
                    SkuItems = SkuItemList,
                    Skus = Skus,
                    DefaultSku = defaultsku
                }
            };
        }

        /// <summary>
        /// 检测是否已开启门店功能
        /// </summary>

        private void CheckOpenStore()
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
        }
    }
}
