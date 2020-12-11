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
using System.Linq;


namespace Mall.SmallProgAPI
{

    public class LimitTimeBuyController : BaseApiController
    {
        /// <summary>
        /// 获取限时购列表接口
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetLimitBuyList")]
        public object GetLimitBuyList(int pageIndex, int pageSize)
        {
            #region 初始化查询Model
            FlashSaleQuery query = new FlashSaleQuery()
            {
                PageNo = pageIndex,
                PageSize = pageSize,
                IsPreheat = true,
                CheckProductStatus = true,
                OrderKey = 5, /* 排序项（1：默认，2：销量，3：价格，4 : 结束时间,5:状态 开始排前面） */
                AuditStatus = FlashSaleInfo.FlashSaleStatus.Ongoing
            };

            #endregion
            var data = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetAll(query);
            var products = ProductManagerApplication.GetProducts(data.Models.Select(p => p.ProductId));
            var list = data.Models.ToList().Select(item => {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                return new
                {
                    CountDownId = item.Id,
                    ProductId = item.ProductId,
                    ProductName = product.ProductName,
                    SalePrice = product.MarketPrice.ToString("0.##"),//各端统一取商品市场价
                    CountDownPrice = item.MinPrice,
                    CountDownType = DateTime.Now < item.BeginDate ? 1 : 2,   //1=即将开始，2=立即抢购
                    ThumbnailUrl160 = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_220)
                };
            });
            return Json(list);
        }

        ///// <summary>
        ///// 获取限时抢购商品详情
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>

        [HttpGet("GetLimitBuyProduct")]
        public object GetLimitBuyProduct(string openId, long countDownId)
        {
            //CheckUserLogin();
            ProductDetailModelForMobie model = new ProductDetailModelForMobie()
            {
                Product = new ProductInfoModel(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };
           
            Entities.ShopInfo shop = null;
            FlashSaleModel market = null;

            market = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(countDownId);


            if (market == null || market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing)
            {
                //可能参数是商品ID
                market = market == null ? ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetFlaseSaleByProductId(countDownId) : market;
                if (market == null || market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing)
                {
                    //跳转到404页面
                    return Json(ErrorResult<dynamic>("你所请求的限时购或者商品不存在！"));
                }
            }
           
            if (market != null && (market.Status != FlashSaleInfo.FlashSaleStatus.Ongoing || DateTime.Parse(market.EndDate) < DateTime.Now))
            {
                return Json(new { IsValidLimitBuy = false });
            }

            model.MaxSaleCount = market.LimitCountOfThePeople;
            model.Title = market.Title;

            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(market.ProductId);
            var description = ProductManagerApplication.GetProductDescription(product.Id);

          
            #region 根据运费模板获取发货地址
            var freightTemplateService = ServiceApplication.Create<IFreightTemplateService>();
            var template = freightTemplateService.GetFreightTemplate(product.FreightTemplateId);
            //string productAddress = string.Empty;
            //if (template != null)
            //{
            //    var fullName = ServiceApplication.Create<IRegionService>().GetFullName(template.SourceAddress);
            //    if (fullName != null)
            //    {
            //        var ass = fullName.Split(' ');
            //        if (ass.Length >= 2)
            //        {
            //            productAddress = ass[0] + " " + ass[1];
            //        }
            //        else
            //        {
            //            productAddress = ass[0];
            //        }
            //    }
            //}

            //model.ProductAddress = productAddress;
            model.FreightTemplate = template;
            #endregion
          
            #region 商品SKU
            Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

          
            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();
            var skus = ProductManagerApplication.GetSKUs(product.Id);
          
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
                                    UseAttributeImage = "False",
                                    Value = sku.Color,
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
                    AttributeName = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,//如果商品有自定义规格名称则用
                    AttributeId = product.TypeId,
                    AttributeValue = colorAttributeValue,
                    AttributeIndex = 0,
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion
              
                #region 容量
                List<object> sizeAttributeValue = new List<object>();
                List<string> listsize = new List<string>();
                foreach (var sku in skus)
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
                                    UseAttributeImage = false,
                                    Value = sku.Size,
                                    //ImageUrl = Mall.Core.MallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listsize.Add(sku.Size);
                                sizeAttributeValue.Add(sizeValue);
                            }
                        }
                    }
                }
           
                var size = new
                {
                    AttributeName = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = sizeAttributeValue,
                    AttributeIndex = 1,
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
                                    UseAttributeImage = false,
                                    Value = sku.Version,
                                    //ImageUrl = Mall.Core.MallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listversion.Add(sku.Version);
                                versionAttributeValue.Add(versionValue);
                            }
                        }
                    }
                }
               
                var version = new
                {
                    AttributeName = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = versionAttributeValue,
                    AttributeIndex = 2,
                };
                if (versionId > 0)
                {
                    SkuItemList.Add(version);
                }
                #endregion
              
                #region Sku值

                foreach (var sku in skus)
                {
                    FlashSaleDetailInfo detailInfo = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetDetail(sku.Id);
                  
                    var prosku = new
                    {
                        SkuItems = "",
                        MemberPrices = "",
                        SkuId = sku.Id,
                        ProductId = product.Id,
                        SKU = sku.Sku,
                        Weight = 0,
                        Stock = detailInfo == null? sku.Stock: Math.Min(detailInfo.TotalCount,sku.Stock),
                        WarningStock = sku.SafeStock,
                        CostPrice = sku.CostPrice,
                        SalePrice = sku.SalePrice,//限时抢购价格
                        StoreStock = 0,
                        StoreSalePrice = 0,
                        OldSalePrice = 0,
                        ImageUrl = "",
                        ThumbnailUrl40 = "",
                        ThumbnailUrl410 = "",
                        MaxStock = 15,
                        FreezeStock = 0,
                        ActivityStock = sku.Stock,//限时抢购库存
                        ActivityPrice = detailInfo == null ? sku.SalePrice : detailInfo.Price//限时抢购价格
                    };
                    Skus.Add(prosku);
                }
             
                #endregion
            }
            #endregion

            #region 店铺
            shop = ServiceProvider.Instance<IShopService>.Create.GetShop(product.ShopId);
            var vshopinfo = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id);
            if (vshopinfo != null)
            {
                model.VShopLog = vshopinfo.WXLogo;
                model.Shop.VShopId = vshopinfo.Id;
            }
            else
            {
                model.Shop.VShopId = -1;
                model.VShopLog = string.Empty;
            }
            var mark = Web.Framework.ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
         
            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = CommentApplication.GetProductAverageMark(product.Id);
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
            model.Shop.ProductNum = ServiceProvider.Instance<IProductService>.Create.GetShopOnsaleProducts(product.ShopId);
          
            var shopStatisticOrderComments = ShopApplication.GetStatisticOrderComment(product.ShopId);
            //宝贝与描述
            model.Shop.ProductAndDescription = shopStatisticOrderComments.ProductAndDescription;
            //卖家服务态度
            model.Shop.SellerServiceAttitude = shopStatisticOrderComments.SellerServiceAttitude;
            //卖家发货速度
            model.Shop.SellerDeliverySpeed = shopStatisticOrderComments.SellerDeliverySpeed;
           
            if (ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id) == null)
                model.Shop.VShopId = -1;
            else
                model.Shop.VShopId = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id).Id;

            List<object> coupons = new List<object>();
            //优惠券
            var result = GetCouponList(shop.Id);//取设置的优惠券
            if (result != null)
            {
                var couponCount = result.Count();
                model.Shop.CouponCount = couponCount;
                if (result.ToList().Count > 0)
                {
                    foreach (var item in result.ToList())
                    {
                        if (CurrentUser != null)
                        {//登录才处理已领
                            var Receive = CouponApplication.GetReceiveStatus(CurrentUserId, item.ShopId, item.Id);
                            if (Receive == 2 || Receive == 4)
                            {//不符合领取条件
                                continue;
                            }
                        }
                        var couponInfo = new
                        {
                            CouponId = item.Id,
                            CouponName = item.CouponName,
                            Price = item.Price,
                            SendCount = item.Num,
                            UserLimitCount = item.PerMax,
                            OrderUseLimit = item.OrderAmount,
                            StartTime = item.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            ClosingTime = item.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            CanUseProducts = "",
                            ObtainWay = item.ReceiveType,
                            NeedPoint = item.NeedIntegral,
                            UseWithGroup = false,
                            UseWithPanicBuying = false,
                            UseWithFireGroup = false,
                            LimitText = item.CouponName,
                            CanUseProduct = item.UseArea == 1 ? "部分商品可用" : "全店通用",
                            StartTimeText = item.StartTime.ToString("yyyy.MM.dd"),
                            ClosingTimeText = item.EndTime.ToString("yyyy.MM.dd")
                        };
                        coupons.Add(couponInfo);
                    }
                }
            }


            #endregion

            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations(product.Id);
            var comments = CommentApplication.GetCommentsByProduct(product.Id);
            var total = comments.Count();
            var niceTotal = comments.Count(item => item.ReviewMark >= 4);
            bool isFavorite = false;
            if (CurrentUser == null)
                isFavorite = false;
            else
                isFavorite = ServiceProvider.Instance<IProductService>.Create.IsFavorite(product.Id, CurrentUser.Id);
            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(product.Id);
            var productImage = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                if (i == 1 || Mall.Core.MallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    productImage.Add(Core.MallIO.GetRomoteImagePath(product.RelativePath + string.Format("/{0}.png", i)));
                }
            }
       
            model.Product = new ProductInfoModel()
            {
                ProductId = product.Id,
                CommentCount = CommentApplication.GetCommentCountByProduct(product.Id),
                Consultations = consultations.Count(),
                ImagePath = productImage,
                IsFavorite = isFavorite,
                MarketPrice = product.MarketPrice,
                MinSalePrice = product.MinSalePrice,
                NicePercent = model.Shop.ProductMark == 0 ? 100 : (int)((niceTotal / total) * 100),
                ProductName = product.ProductName,
                ProductSaleStatus = product.SaleStatus,
                AuditStatus = product.AuditStatus,
                ShortDescription = product.ShortDescription,
                ProductDescription = description.ShowMobileDescription,
                IsOnLimitBuy = limitBuy != null,
                MeasureUnit = product.MeasureUnit
            };

            #endregion
        
            //LogProduct(market.ProductId);
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);

            TimeSpan end = new TimeSpan(DateTime.Parse(market.EndDate).Ticks);
            TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts = end.Subtract(start);
            var second = ts.TotalSeconds < 0 ? 0 : ts.TotalSeconds;

            List<object> ProductImgs = new List<object>();
            for (int i = 1; i < 5; i++)
            {
                if (i == 1 || Mall.Core.MallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    ProductImgs.Add(Core.MallIO.GetRomoteProductSizeImage(product.ImagePath, i, (int)ImageSize.Size_350));
                }
            }
        
            var countDownStatus = 0;

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

            long saleCounts = 0;
            if (countDownStatus == 2)
            {
                saleCounts = market.SaleCount;
            }
            else
            {
                saleCounts = product.SaleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(product.VirtualSaleCounts);
            }
            //Normal：正常
            //PullOff：已下架
            //NoJoin：未参与
            //AboutToBegin：即将开始
            //ActivityEnd：已结束
            //SoldOut：已抢完

            decimal discount = 1M;//限时购不考虑会员折
            int addressId = 0;
            if (CurrentUser != null)
            {
                var addressInfo = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                if (addressInfo != null)
                {
                    addressId = addressInfo.RegionId;
                }
            }

            
            //商品关联版式
            string DescriptionPrefix = "", DescriptiondSuffix = "";
            var iprodestempser = ServiceApplication.Create<IProductDescriptionTemplateService>();            
            if (description.DescriptionPrefixId != 0)
            {
                var desc = iprodestempser.GetTemplate(description.DescriptionPrefixId, product.ShopId);
                DescriptionPrefix = desc == null ? "" : desc.MobileContent;
            }

            if (description.DescriptiondSuffixId != 0)
            {
                var desc = iprodestempser.GetTemplate(description.DescriptiondSuffixId, product.ShopId);
                DescriptiondSuffix = desc == null ? "" : desc.MobileContent;
            }
          
            var productDescription = DescriptionPrefix + model.Product.ProductDescription + DescriptiondSuffix;
            string skuId = skus.FirstOrDefault()?.Id ?? string.Empty;
            
            return Json(new
            {
                CountDownId = market.Id,//.CountDownId,
                MaxCount = market.LimitCountOfThePeople,
                CountDownStatus = countDownStatus,
                StartDate = DateTime.Parse(market.BeginDate).ToString("yyyy/MM/dd HH:mm:ss"),
                EndDate = DateTime.Parse(market.EndDate).ToString("yyyy/MM/dd HH:mm:ss"),
                NowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                ProductId = product.Id,
                ProductName = product.ProductName,
                MetaDescription = productDescription.Replace("\"/Storage/Shop", "\"" + Core.MallIO.GetRomoteImagePath("/Storage/Shop")),//替换链接  /Storage/Shop,
                ShortDescription = product.ShortDescription,
                ShowSaleCounts = saleCounts,
                IsSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1,
                Weight = product.Weight.ToString(),
                MinSalePrice = market.MinPrice.ToString("0.##"),//限时抢购价格
                MaxSalePrice = market.SkuMaxPrice,
                Stock = market.Quantity,//限时抢购库存
                MarketPrice = product.MarketPrice,
                IsfreeShipping = shop.FreeFreight,
                ThumbnailUrl60 = Core.MallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350),
                ProductImgs = ProductImgs,
                SkuItemList = SkuItemList,
                Skus = Skus,
                Shop = model.Shop,
                VShopLog = Mall.Core.MallIO.GetRomoteImagePath(model.VShopLog),
                Freight = GetFreightStr(product.Id, discount, skuId, addressId),
                Coupons = coupons,
                IsValidLimitBuy = true,
                CommentsNumber = CommentApplication.GetCommentCountByProduct(product.Id),
                VideoPath = string.IsNullOrWhiteSpace(product.VideoPath) ? string.Empty : Mall.Core.MallIO.GetRomoteImagePath(product.VideoPath),
                MeasureUnit = string.IsNullOrEmpty(product.MeasureUnit)?"":product.MeasureUnit, //单位
                SendTime = (model.FreightTemplate != null && !string.IsNullOrEmpty(model.FreightTemplate.SendTime) ? (model.FreightTemplate.SendTime + "h内发货") : ""), //运费模板发货时间
            });
        }

        internal IEnumerable<Entities.CouponInfo> GetCouponList(long shopid)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var result = service.GetCouponList(shopid);
            var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting(shopid).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id));//取设置的优惠卷
                return couponList;
            }
            return null;
        }

        private string GetFreightStr(long productId, decimal discount, string skuId, int addressId)
        {
            string freightStr = "免运费";
            //if (addressId <= 0)//如果用户的默认收货地址为空，则运费没法计算
            //    return freightStr;
            bool isFree = false;
            if (addressId > 0)
                isFree = ProductManagerApplication.IsFreeRegion(productId, discount, addressId, 1, skuId);//默认取第一个规格
            if (isFree)
            {
                freightStr = "免运费";//卖家承担运费
            }
            else
            {
                decimal freight = ServiceApplication.Create<IProductService>().GetFreight(new List<long>() { productId }, new List<int>() { 1 }, addressId);
                freightStr = freight <= 0 ? "免运费" : string.Format("运费 {0}元", freight.ToString("f2"));
            }
            return freightStr;
        }
    }
}
