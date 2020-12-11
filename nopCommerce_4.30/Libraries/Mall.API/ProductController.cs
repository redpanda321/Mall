using Mall.API.Model;
using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mall.Entities.ProductInfo;

namespace Mall.API
{
    public class ProductController : BaseApiController
    {

        [HttpGet("GetProductDetail")]
        public object GetProductDetail(long id)
        {
            ProductDetailModelForMobie model = new ProductDetailModelForMobie()
            {
                Product = new ProductInfoModel(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };

            Entities.ProductInfo product = null;
            Entities.ShopInfo shop = null;
            string loadShowPrice = string.Empty;//app详细页加载显示的区间价

            product = ServiceProvider.Instance<IProductService>.Create.GetProduct(id);

            if (product == null || product.IsDeleted)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "该商品已被删除或者转移");
            }
            var cashDepositModel = ServiceProvider.Instance<ICashDepositsService>.Create.GetCashDepositsObligation(product.Id);//提供服务（消费者保障、七天无理由、及时发货）
            model.CashDepositsServer = cashDepositModel;
            #region 根据运费模板获取发货地址
            var freightTemplateService = ServiceApplication.Create<IFreightTemplateService>();
            FreightTemplateInfo template = null;
            if (product.ProductType == 0)
            {
                template = freightTemplateService.GetFreightTemplate(product.FreightTemplateId);
            }
            string productAddress = string.Empty;
            if (template != null)
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
            shop = ServiceProvider.Instance<IShopService>.Create.GetShopBasicInfo(product.ShopId);
            var vshopinfo = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
            {
                vShopId = -1;
                model.Shop.VShopId = vShopId;
                model.VShopLog = string.Empty;
            }
            else
            {
                vShopId = vshopinfo.Id;
                model.Shop.VShopId = vShopId;
                model.VShopLog = vshopinfo.WXLogo;
            }
            #endregion

            #region 商品基础参数
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
                discount = CurrentUser.MemberDiscount;
            }
            #endregion

            model.Shop.FavoriteShopCount = ServiceProvider.Instance<IShopService>.Create.GetShopFavoritesCount(product.ShopId);//关注人数

            var limitbuyser = ServiceProvider.Instance<ILimitTimeBuyService>.Create;
            var limitBuy = limitbuyser.GetLimitTimeMarketItemByProductId(id);

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
            if (limitBuy != null)
            {
                loadShowPrice = limitBuy.MinPrice.ToString("f2");//限时购不打折,初始时限时购销售价
                var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                var limitSkuItem = limitSku.Details.OrderBy(d => d.Price).FirstOrDefault();
                if (limitSkuItem != null)
                {
                    product.MinSalePrice = limitSkuItem.Price;

                    #region 限时购计算出它规格区间价
                    decimal min = limitSkuItem.Price;
                    decimal max = limitSku.Details.Max(s => s.Price);
                    loadShowPrice = (min < max) ? (min.ToString("f2") + " - " + max.ToString("f2")) : min.ToString("f2");//限时购不打折，普通商品自营店打折
                    #endregion
                }
            }
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            if (skus != null && skus.Count() > 0)
            {
                #region//不是限时购计算出它规格区间价
                if (limitBuy == null)
                {
                    decimal min = skus.Min(s => s.SalePrice);
                    decimal max = skus.Max(s => s.SalePrice);
                    if (shop.IsSelf)//自营店会员有级别比例
                    {
                        min = min * discount;
                        max = max * discount;
                    }
                    loadShowPrice = (min < max) ? (min.ToString("f2") + " - " + max.ToString("f2")) : min.ToString("f2");
                }
                #endregion

                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in skus)
                {
                    var specs = sku.Id.Split('_');
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
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
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
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
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
                                    //Name = "选择版本",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
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
            //shop = ServiceProvider.Instance<IShopService>.Create.GetShop(product.ShopId);
            var mark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;

            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = CommentApplication.GetProductAverageMark(id);
            model.Shop.Id = product.ShopId;
            if (product.ProductType == 0)
            {
                model.Shop.FreeFreight = shop.FreeFreight;
            }
            else
            {
                model.Shop.FreeFreight = 0;
            }
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
            if (shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze || shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired)
            {
                model.Shop.CouponCount = 0;
            }
            else
            {
                var couponCount = GetCouponList(shop.Id);//取设置的优惠券
                if (couponCount > 0)
                {
                    model.Shop.CouponCount = couponCount;
                }
            }

            // 客服
            var customerServices = CustomerServiceApplication.GetMobileCustomerServiceAndMQ(shop.Id);
            #endregion

            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations(id);
            var comments = CommentApplication.GetCommentsByProduct(product.Id);
            var total = comments.Count();
            var niceTotal = comments.Count(item => item.ReviewMark >= 4);

            var productImage = new List<string>();
            var thumbImage = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                if (Core.MallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    var path = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, i, (int)Mall.CommonModel.ImageSize.Size_500);
                    var thumb = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, i, (int)Mall.CommonModel.ImageSize.Size_50);
                    thumbImage.Add(thumb);
                    productImage.Add(path);
                }
            }
            decimal minSalePrice = shop.IsSelf ? product.MinSalePrice * discount : product.MinSalePrice;
            decimal limitPrice = 0;
            string limitPriceInterval = string.Empty;//限时购区间价
            var isValidLimitBuy = false;
            long countDownId = 0;
            var StartDate = string.Empty;
            if (limitBuy != null)
            {
                minSalePrice = limitBuy.MinPrice; //限时购不打折
                limitPrice = limitBuy.MinPrice;
                isValidLimitBuy = true;
                countDownId = limitBuy.Id;
                limitPriceInterval = loadShowPrice;//如是限时购，区间价已在上面取出则复制给它
            }
            else
            {
                #region 限时购预热
                var FlashSale = limitbuyser.IsFlashSaleDoesNotStarted(id);
                var FlashSaleConfig = limitbuyser.GetConfig();

                if (FlashSale != null)
                {
                    StartDate = DateTime.Parse(FlashSale.BeginDate).ToString("yyyy/MM/dd HH:mm:ss");
                    countDownId = FlashSale.Id;
                    limitPrice = FlashSale.MinPrice;
                    TimeSpan flashSaleTime = DateTime.Parse(FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                    TimeSpan preheatTime = new TimeSpan(FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                    if (preheatTime >= flashSaleTime)  //预热大于开始
                    {
                        if (!FlashSaleConfig.IsNormalPurchase)
                        {
                            isValidLimitBuy = true;
                            minSalePrice = FlashSale.MinPrice;
                        }
                    }

                    #region 限时购预热计算区间价
                    limitPriceInterval = limitPrice.ToString("f2");//初始时限时购销售价
                    var limitSku2 = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(FlashSale.Id);
                    var limitSkuItem2 = limitSku2.Details.OrderBy(d => d.Price).FirstOrDefault();
                    if (limitSkuItem2 != null)
                    {
                        decimal min = limitSkuItem2.Price;
                        decimal max = limitSku2.Details.Max(s => s.Price);
                        limitPriceInterval = (min < max) ? (min.ToString("f2") + " - " + max.ToString("f2")) : min.ToString("f2");//限时购不打折
                    }
                    #endregion
                }
                #endregion
            }
            var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(product.Id, shop.IsSelf, discount);
            if (product.IsOpenLadder && !isValidLimitBuy)
            {
                var ladder = ladderPrices.OrderBy(l => l.MinBath).FirstOrDefault();
                if (ladder != null)
                    minSalePrice = ladder.Price;
            }
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
                CommentCount = CommentApplication.GetCommentCountByProduct(product.Id),
                Consultations = consultations.Count(),
                ImagePath = productImage,
                ThumbnailPath = thumbImage,
                IsFavorite = isFavorite,
                MarketPrice = product.MarketPrice,
                MinSalePrice = minSalePrice,
                LimitBuyPrice = limitPrice,
                LimitBuyPriceInterval = limitPriceInterval,//限时购区间价
                NicePercent = model.Shop.ProductMark == 0 ? 100 : (int)((niceTotal / total) * 100),
                ProductName = product.ProductName,
                ProductSaleStatus = product.SaleStatus,
                AuditStatus = product.AuditStatus,
                ShortDescription = product.ShortDescription,
                ProductDescription = GetProductDescription(desc),
                IsOnLimitBuy = isValidLimitBuy,
                IsOpenLadder = product.IsOpenLadder,
                MinMath = ProductManagerApplication.GetProductLadderMinMath(product.Id),
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

            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(id, shop.Id);

            #endregion

            LogProduct(id);
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            var countDownStatus = 0;
            var EndDate = string.Empty;
            var NowTime = string.Empty;
            if (isValidLimitBuy)
            {
                var market = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(countDownId);
                if (market.Status == FlashSaleInfo.FlashSaleStatus.Ended)
                {
                    countDownStatus = 4;//"PullOff";  //已下架
                }
                else if (market.Status == FlashSaleInfo.FlashSaleStatus.Cancelled || market.Status == FlashSaleInfo.FlashSaleStatus.AuditFailed || market.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
                {
                    countDownStatus = 4;//"NoJoin";  //未参与
                }
                else if (DateTime.Parse(market.BeginDate) > DateTime.Now)
                {
                    countDownStatus = 6; // "AboutToBegin";  //即将开始   6
                }
                else if (DateTime.Parse(market.EndDate) < DateTime.Now)
                {
                    countDownStatus = 4;// "ActivityEnd";   //已结束  4
                }
                else if (market.Status == FlashSaleInfo.FlashSaleStatus.Ended)
                {
                    countDownStatus = 6;// "SoldOut";  //已抢完
                }
                else
                {
                    countDownStatus = 2;//"Normal";  //正常  2
                }
                StartDate = DateTime.Parse(market.BeginDate).ToString("yyyy/MM/dd HH:mm:ss");
                EndDate = DateTime.Parse(market.EndDate).ToString("yyyy/MM/dd HH:mm:ss");
            }
            NowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);

            long saleCounts = 0;
            if (isValidLimitBuy)
            {
                if (limitBuy != null)
                    saleCounts = limitBuy.SaleCount;
            }
            else
            {
                saleCounts = product.SaleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(product.VirtualSaleCounts);
            }
            if (string.IsNullOrEmpty(loadShowPrice))
            {
                loadShowPrice = product.MinSalePrice.ToString("f2");//前面都没有规格区间价格，则默认商品原区间价格
                if (isValidLimitBuy)
                    loadShowPrice = model.Product.LimitBuyPrice.ToString();//是限时购，则是限时购价
            }

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
                    IsOverdue = virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value
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

            var result = new
            {
                success = true,
                IsOnLimitBuy = isValidLimitBuy,
                CountDownStatus = countDownStatus,
                StartDate = StartDate,
                EndDate = EndDate,
                NowTime = NowTime,
                IsFightGroupActive = isFightGroupActive,
                ActiveId = isFightGroupActive ? activeInfo.Id : 0,
                ActiveStatus = activeInfo != null ? activeInfo.ActiveStatus.GetHashCode() : 0,
                MaxSaleCount = limitBuy == null ? product.MaxBuyCount : limitBuy.LimitCountOfThePeople,
                Title = limitBuy == null ? string.Empty : limitBuy.Title,
                Second = limitBuy == null ? 0 : (limitBuy.EndDate - DateTime.Now).TotalSeconds,
                Product = model.Product,
                CashDepositsServer = model.CashDepositsServer,//提供服务（消费者保障、七天无理由、及时发货）
                ProductAddress = model.ProductAddress,//发货地址
                //Free = model.FreightTemplate.IsFree == FreightTemplateType.Free ? "免运费" : "",//是否免运费
                VShopLog = Mall.Core.MallIO.GetRomoteImagePath(model.VShopLog),
                Shop = model.Shop,
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
                LadderPrices = ladderPrices,//阶梯价
                VideoPath = string.IsNullOrWhiteSpace(product.VideoPath) ? string.Empty : Mall.Core.MallIO.GetRomoteImagePath(product.VideoPath),
                LoadShowPrice = loadShowPrice,    //限时购或普通商品时区间价(没规格则只一个价)
                ProductSaleCountOnOff = (SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1),//是否显示销量
                SaleCounts = saleCounts,    //销量
                FreightStr = FreightTemplateApplication.GetFreightStr(product.Id, model.FreightTemplate, CurrentUser, product),//运费多少或免运费
                SendTime = (model.FreightTemplate != null && !string.IsNullOrEmpty(model.FreightTemplate.SendTime) ? (model.FreightTemplate.SendTime + "h内发货") : ""), //运费模板发货时间
                ProductType = product.ProductType,
                VirtualProductInfo = virtualPInfo,
                VirtualProductItemModels = virtualProductItemModels
            };
            return result;
        }
        /// <summary>
        /// 获取商品评论
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="top">top1</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetProductCommentShow")]
        public object GetProductCommentShow(long id,long bid=0, int top = 1)
        {
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(id);
            var model = new List<ProductDetailCommentModel>();
            if (top < 1)
            {
                top = 1;
            }

            if (product == null || product.IsDeleted)
            {
                return ErrorResult("不存在该商品！");
            }
            var comments = CommentApplication.GetCommentsByProduct(product.Id, bid);
            if (comments.Count > 0)
            {
                var productCommentInfoList = comments.OrderByDescending(a => a.ReviewDate).Take(top);
                var orderItems = OrderApplication.GetOrderItems(productCommentInfoList.Select(c => c.SubOrderId).ToList()).ToDictionary(item => item.Id, item => item);
                var orders = OrderApplication.GetOrders(orderItems.Values.Select(a => a.OrderId).ToList()).ToDictionary(o => o.Id, o => o);
                var images = CommentApplication.GetProductCommentImagesByCommentIds(productCommentInfoList.Select(p => p.Id));
                model = productCommentInfoList.Select(c => new ProductDetailCommentModel
                {
                    Sku = ServiceProvider.Instance<IProductService>.Create.GetSkuString(orderItems[c.SubOrderId].SkuId),
                    UserName = c.UserName.Substring(0, 1) + "***" + c.UserName.Substring(c.UserName.Length - 1, 1),
                    ReviewContent = c.ReviewContent,
                    AppendContent = c.AppendContent,
                    AppendDate = c.AppendDate,
                    ReplyAppendContent = c.ReplyAppendContent,
                    ReplyAppendDate = c.ReplyAppendDate,
                    FinshDate = orders[orderItems[c.SubOrderId].OrderId].FinishDate,
                    Images = images.Where(a => a.CommentId == c.Id && a.CommentType == 0).Select(a => a.CommentImage).ToList(),
                    AppendImages = images.Where(a => a.CommentId == c.Id && a.CommentType == 1).Select(a => a.CommentImage).ToList(),
                    ReviewDate = c.ReviewDate,
                    ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                    ReplyDate = c.ReplyDate,
                    ReviewMark = c.ReviewMark,
                    BuyDate = orders[orderItems[c.SubOrderId].OrderId].OrderDate

                }).ToList();
                foreach (var citem in model)
                {
                    if (citem.Images.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.Images.Count; _imgn++)
                        {
                            citem.Images[_imgn] = Mall.Core.MallIO.GetRomoteImagePath(citem.Images[_imgn]);
                        }
                    }
                    if (citem.AppendImages.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.AppendImages.Count; _imgn++)
                        {
                            citem.AppendImages[_imgn] = Mall.Core.MallIO.GetRomoteImagePath(citem.AppendImages[_imgn]);
                        }
                    }
                }
                return new { success = true, data = model };
            }
            else
            {
                return new { success = false, msg = "该宝贝暂无评论！" };
            }

        }
        internal int GetCouponList(long shopId)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            //var result = service.GetCouponList(shopid);
            //var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting(shopid).Where(a => a.PlatForm == Core.PlatformType.Wap).Select(item => item.CouponID);
            //if (result.Count() > 0 && couponSetList.Count() > 0)
            //{
            //    var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id)).Select(p => new
            //    {
            //        Receive = Receive(p.ShopId, p.Id)
            //    });
            //    return couponList.Where(p => p.Receive != 2 && p.Receive != 4).Count();//排除过期和已领完的
            //}
            //return 0;
            return service.GetUserCouponCount(shopId);//商铺可用优惠券，排除过期和已领完的
        }
        internal void LogProduct(long pid)
        {
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(pid, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(pid);
            }

            //ServiceProvider.Instance<IProductService>.Create.LogProductVisti(pid);
        }
        //新增或取消商品收藏

        [HttpPost("PostAddFavoriteProduct")]
        public object PostAddFavoriteProduct(ProductAddFavoriteProductModel value)
        {
            CheckUserLogin();
            long productId = value.productId;
            int status = 0;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            bool isFavorite = productService.IsFavorite(productId, CurrentUser.Id);
            if (isFavorite)
            {
                productService.DeleteFavorite(productId, CurrentUser.Id);
                return SuccessResult("取消成功");
            }
            else
            {
                productService.AddFavorite(productId, CurrentUser.Id, out status);
                return SuccessResult("收藏成功");
            }
        }


        [HttpGet("GetHistoryVisite")]
        public object GetHistoryVisite()
        {
            var products = BrowseHistrory.GetBrowsingProducts(10, CurrentUserId);
            foreach (var product in products)
            {
                //product.ImagePath = "http://" + Url.Request.RequestUri.Host + product.ImagePath;
                product.ImagePath = Core.MallIO.GetRomoteImagePath(product.ImagePath);
            }
            dynamic result = SuccessResult();
            result.Product = products;
            return result;
        }



        [HttpGet("GetSKUInfo")]
        public object GetSKUInfo(long productId, long colloPid = 0)
        {
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(productId);
            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(productId);
            List<Mall.Entities.CollocationSkuInfo> collProduct = null;
            if (colloPid != 0)
            {
                collProduct = ServiceProvider.Instance<ICollocationService>.Create.GetProductColloSKU(productId, colloPid);
            }
            var shopInfo = ShopApplication.GetShop(product.ShopId);
            decimal discount = 1M;
            if (CurrentUser != null && shopInfo.IsSelf)
            {
                discount = CurrentUser.MemberDiscount;
            }

            var skuArray = new List<ProductSKUModel>();
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            foreach (var sku in skus.Where(s => s.Stock > 0))
            {
                var price = sku.SalePrice;
                if (product.IsOpenLadder)
                {
                    var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(product.Id);
                    var ladder = ladderPrices.OrderBy(l => l.MinBath).FirstOrDefault();
                    if (ladder != null)
                        price = ladder.Price;
                }

                ProductSKUModel skuMode = new ProductSKUModel
                {
                    Price = shopInfo.IsSelf ? price * discount : price,
                    SkuId = sku.Id,
                    Stock = (int)sku.Stock
                };

                if (limitBuy != null)
                {
                    var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                    var limitSkuItem = limitSku.Details.Where(r => r.SkuId.Equals(sku.Id)).FirstOrDefault();
                    if (limitSkuItem != null)
                    {
                        skuMode.Price = limitSkuItem.Price;
                        skuMode.Stock = Math.Min(sku.Stock, limitSkuItem.TotalCount);
                    }
                }
                skuArray.Add(skuMode);
            }

            #region 这里是app端在已下架刚好库存0情况它弹窗多余提示，app端在下架商品时规格会隐藏，则在没改app端情况改接口端默认加一个库存为0的一条数据
            if (skuArray.Count == 0 && (product.SaleStatus == ProductSaleStatus.InStock || product.AuditStatus != ProductAuditStatus.Audited))
            {

                var sku = skus.First();
                ProductSKUModel skuMode = new ProductSKUModel
                {
                    Price = sku.SalePrice,
                    SkuId = sku.Id,
                    Stock = 0
                };
                skuArray.Add(skuMode);
            }
            #endregion

            //foreach (var item in skuArray)
            //{
            //    var str = item.SkuId.Split('_');
            //    item.SkuId = string.Format("{0};{1};{2}", str[1], str[2], str[3]);
            //}
            dynamic result = SuccessResult();
            result.SkuArray = skuArray.OrderBy(p => p.SkuId);
            return result;
        }


        [HttpGet("GetProductComment")]
        public object GetProductComment(long pId, int pageNo, int commentType = 0, int pageSize = 10, long shopBranchId = 0)
        {
            var query = new ProductCommentQuery
            {
                ProductId = pId,
                PageNo = pageNo,
                PageSize = pageSize,
            };
            if (shopBranchId > 0)
            {
                query.ShopBranchId = shopBranchId;
            }
            switch (commentType)
            {
                case 1: query.CommentType = ProductCommentMarkType.High; break;
                case 2: query.CommentType = ProductCommentMarkType.Medium; break;
                case 3: query.CommentType = ProductCommentMarkType.Low; break;
                case 4: query.CommentType = ProductCommentMarkType.HasImage; break;
                case 5: query.CommentType = ProductCommentMarkType.Append; break;
            }

            var productService = ServiceProvider.Instance<IProductService>.Create;
            var comments = CommentApplication.GetComments(query).Models;
            var orderitems = OrderApplication.GetOrderItems(comments.Select(p => p.SubOrderId));
            var orders = OrderApplication.GetOrders(orderitems.Select(p => p.OrderId));
            var images = CommentApplication.GetProductCommentImagesByCommentIds(comments.Select(p => p.Id));
            var data = comments.Select(item =>
            {
                var orderitem = orderitems.FirstOrDefault(p => p.Id == item.SubOrderId);
                var order = orders.FirstOrDefault(p => p.Id == orderitem.OrderId);

                return new
                {
                    Sku = productService.GetSkuString(orderitem.SkuId),
                    UserName = item.UserName.Substring(0, 1) + "***" + item.UserName.Substring(item.UserName.Length - 1, 1),
                    ReviewContent = item.ReviewContent,
                    AppendContent = item.AppendContent,
                    AppendDate = item.AppendDate.HasValue ? item.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    ReplyAppendContent = item.ReplyAppendContent,
                    ReplyAppendDate = item.ReplyAppendDate.HasValue ? item.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    FinshDate = order.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Images = images.Where(a => a.CommentId == item.Id && a.CommentType == 0).Select(a => new { CommentImage = Core.MallIO.GetRomoteImagePath(a.CommentImage) }).ToList(),
                    AppendImages = images.Where(a => a.CommentId == item.Id && a.CommentType == 1).Select(a => new { CommentImage = Core.MallIO.GetRomoteImagePath(a.CommentImage) }).ToList(),
                    ReviewDate = item.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ReplyContent = string.IsNullOrWhiteSpace(item.ReplyContent) ? "暂无回复" : item.ReplyContent,
                    ReplyDate = item.ReplyDate.HasValue ? item.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                    ReviewMark = item.ReviewMark,
                    BuyDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    AppendDays = item.AppendDate.HasValue ? GetTimeSpan(order.FinishDate.Value, item.AppendDate.Value) : ""
                };
            });
            var statistic = CommentApplication.GetProductCommentStatistic(productId: pId, shopBranchId: shopBranchId);
            return new
            {
                success = true,
                AllCommentCount = statistic.AllComment,
                GoodCount = statistic.HighComment,
                MediumCount = statistic.MediumComment,
                BadCount = statistic.LowComment,
                AppendCount = statistic.AppendComment,
                ImageCount = statistic.HasImageComment,
                List = data
            };
        }

        public static string GetTimeSpan(DateTime startTime, DateTime lasttime)
        {
            TimeSpan time = lasttime - startTime;
            if (time.TotalHours > 24)
                return Math.Floor(time.TotalHours / 24) + "天";
            else if (time.TotalMinutes > 60)
                return Math.Floor(time.TotalMinutes / 60) + "小时";
            else
            {
                if (Math.Floor(time.TotalSeconds / 60) <= 0)//不足1分钟，以1分钟显示
                    return "1分钟";

                return Math.Floor(time.TotalSeconds / 60) + "分钟";
            }
        }

        /// <summary>
        /// 获取该商品所在商家下距离用户最近的门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        /// 

        [HttpGet("GetStroreInfo")]
        public object GetStroreInfo(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0) return ErrorResult("请传入合法商家ID");
            if (!(fromLatLng.Split(',').Length == 2)) return ErrorResult("您当前定位信息异常");

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                FromLatLng = fromLatLng,
                Status = CommonModel.ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal,
                ProductIds = new long[] { productId }
            };
            //商家下门店总数
            var shopbranchs = ShopBranchApplication.GetShopBranchsAll(query).Models.Where(p => (p.Latitude > 0 && p.Longitude > 0)).ToList();
            int total = shopbranchs.Count;
            //商家下有该产品的且距离用户最近的门店
            var shopBranch = shopbranchs.Where(p => (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).Take(1).FirstOrDefault<Mall.DTO.ShopBranch>();
            dynamic result = SuccessResult();
            result.StoreInfo = shopBranch;
            result.total = total;
            return result;
        }

        /// <summary>
        /// 是否可领取优惠券
        /// </summary>
        /// <param name="vshopId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        //private int Receive(long vshopId, long couponId)
        //{
        //    if (CurrentUser != null && CurrentUser.Id > 0)//未登录不可领取
        //    {
        //        var couponService = ServiceProvider.Instance<ICouponService>.Create;
        //        var couponInfo = couponService.GetCouponInfo(couponId);
        //        if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效

        //        CouponRecordQuery crQuery = new CouponRecordQuery();
        //        crQuery.CouponId = couponId;
        //        crQuery.UserId = CurrentUser.Id;
        //        QueryPageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
        //        if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax) return 3;//达到个人领取最大张数

        //        crQuery = new CouponRecordQuery()
        //        {
        //            CouponId = couponId
        //        };
        //        pageModel = couponService.GetCouponRecordList(crQuery);
        //        if (pageModel.Total >= couponInfo.Num) return 4;//达到领取最大张数

        //        if (couponInfo.ReceiveType == Mall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
        //        {
        //            var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
        //            if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
        //        }

        //        return 1;//可正常领取
        //    }
        //    return 0;

        //}

        /// <summary>
        /// 将商品关联版式组合商品描述
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        /// 

        private string GetProductDescription(ProductDescriptionInfo productDescription)
        {
            if (productDescription == null)
            {
                throw new MallException("错误的商品信息");
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
        /// 是否可允许自提门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetIsSelfDelivery")]
        public object GetIsSelfDelivery(long shopId, long productId, string fromLatLng = "")
        {
            dynamic result = SuccessResult();
            if (shopId <= 0)
            {
                result = SuccessResult("请传入合法商家ID");
                result.IsSelfDelivery = false;
                return result;
            }
            if (!(fromLatLng.Split(',').Length == 2))
            {
                result = SuccessResult("请传入合法经纬度");
                result.IsSelfDelivery = false;
                return result;
            }

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                Status = ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal
            };
            string address = "", province = "", city = "", district = "", street = "";
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (string.IsNullOrWhiteSpace(city))
            {
                result = SuccessResult("无法获取当前城市");
                result.IsSelfDelivery = false;
                return result;
            }

            Region cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
            if (cityInfo == null)
            {
                result = SuccessResult("获取当前城市异常");
                result.IsSelfDelivery = false;
                return result;
            }
            query.CityId = cityInfo.Id;
            query.ProductIds = new long[] { productId };

            var shopBranch = ShopBranchApplication.GetShopBranchsAll(query).Models;//获取该商品所在商家下，且与用户同城内门店，且门店有该商品
            var skuInfos = ProductManagerApplication.GetSKUs(productId);//获取该商品的sku
            //门店SKU内会有默认的SKU
            if (!skuInfos.Exists(p => p.Id == string.Format("{0}_0_0_0", productId))) skuInfos.Add(new DTO.SKU()
            {
                Id = string.Format("{0}_0_0_0", productId)
            });
            var shopBranchSkus = ShopBranchApplication.GetSkus(query.ShopId, shopBranch.Select(p => p.Id).ToList());//门店商品SKU

            //门店商品SKU，只要有一个sku有库存即可
            shopBranch.ForEach(p =>
                p.Enabled = skuInfos.Where(skuInfo => shopBranchSkus.Where(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock > 0 && sbSku.SkuId == skuInfo.Id).Count() > 0).Count() > 0
            );

            result = SuccessResult("");
            result.IsSelfDelivery = shopBranch.Where(p => p.Enabled).Count() > 0 ? 1 : 0;
            return result;//至少有一个能够自提的门店，才可显示图标
        }

        /// <summary>
        /// 获取商品批发价
        /// </summary>
        /// <param name="pid">商品ID</param>
        /// <param name="buyNum">数量</param>
        /// <returns></returns>
        /// 

        [HttpGet("GetChangeNum")]
        public object GetChangeNum(long pid, int buyNum)
        {
            var _price = 0m;
            var product = ProductManagerApplication.GetProduct(pid);
            if (product.IsOpenLadder)
            {
                _price = ProductManagerApplication.GetProductLadderPrice(pid, buyNum);
                var discount = 1m;
                if (CurrentUser != null)
                    discount = CurrentUser.MemberDiscount;

                var shop = ShopApplication.GetShop(product.ShopId);
                if (shop.IsSelf)
                    _price = _price * discount;
            }

            dynamic result = SuccessResult();
            result.price = _price;
            return result;
        }
    }
}
