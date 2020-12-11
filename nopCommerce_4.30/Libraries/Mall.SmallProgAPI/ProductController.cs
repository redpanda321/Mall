using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.Product;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Mall.SmallProgAPI.Model.ParamsModel;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;


namespace Mall.SmallProgAPI
{
    public class ProductController : BaseApiController
    {
        protected const string DISTRIBUTION_SPREAD_ID_PARAMETER_NAME = "SpreadId";
        /// <summary>
        /// 搜索商品
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet("GetProducts")]
        public object GetProducts(
             string keyword = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
                           //long b_id = 0, /* 品牌ID */
            string openId = "",
            //string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            string sortBy = "", /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            string sortOrder = "", /* 排序方式（1：升序，2：降序） */
            int pageIndex = 1, /*页码*/
            int pageSize = 10,/*每页显示数据量*/
            long vshopId = 0,
            long sid = 0/*商家ID*/
            )
        {

            #region 初始化查询Model
            SearchProductQuery model = new SearchProductQuery();
            model.VShopId = vshopId;
            model.ShopId = sid;
            //model.BrandId = b_id;
            if (vshopId == 0 && cid != 0)
            {
                var catelist = ServiceProvider.Instance<ICategoryService>.Create.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate.Depth == 1)
                    model.FirstCateId = cid;
                else if (cate.Depth == 2)
                    model.SecondCateId = cid;
                else if (cate.Depth == 3)
                    model.ThirdCateId = cid;
            }
            else if (vshopId != 0 && cid != 0)
            {
                model.ShopCategoryId = cid;
            }

            model.Keyword = keyword;
            if (sortBy == "SalePrice")
            {
                model.OrderKey = 3;//默认
            }
            else if (sortBy == "SaleCounts")
            {
                model.OrderKey = 2;
            }
            else if (sortBy == "VistiCounts")
            {
                model.OrderKey = 4;
            }
            else
            {
                model.OrderKey = 1;
            }

            if (sortOrder == "desc")
            {
                model.OrderType = false;
            }
            else
            {
                model.OrderType = true;
            }


            model.PageNumber = pageIndex;
            model.PageSize = pageSize;
            #endregion
            SearchProductResult result = ServiceProvider.Instance<ISearchProductService>.Create.SearchProduct(model);
            int total = result.Total;
            //当查询的结果少于一页时用like进行补偿（与PC端同步）
            if (result.Total < pageSize)
            {
                model.IsLikeSearch = true;
                SearchProductResult result2 = ServiceProvider.Instance<ISearchProductService>.Create.SearchProduct(model);
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
                        orderby = e => e.OrderBy(p => p.SaleCount);
                        orderByDesc = e => e.OrderByDescending(p => p.SaleCount);
                        break;
                    case 3:
                        orderby = e => e.OrderBy(p => p.SalePrice);
                        orderByDesc = e => e.OrderByDescending(p => p.SalePrice);
                        break;
                    case 4:

                        orderby = e => e.OrderBy(p => p.Comments);
                        orderByDesc = e => e.OrderByDescending(p => p.Comments);
                        break;
                    default:

                        //按最新的排序规则作为默认排序【序号越大，在前台展示的商品越靠前，序号一致时，优先销量排前，销量一致时，优先上架时间排前】
                        orderByDesc = e => e.OrderByDescending(p => p.DisplaySequence).ThenByDescending(p => p.SaleCount).ThenByDescending(p => p.ProductId);
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
                var ser = ServiceProvider.Instance<IProductService>.Create;
                var _pro = ser.GetProduct(item.ProductId);
                var _skus = ser.GetSKUs(item.ProductId);
                if (_pro.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale && _pro.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited)
                {
                    item.ShowStatus = 0;
                    if (_skus.Sum(d => d.Stock) < 1)
                    {
                        item.ShowStatus = 2;
                    }
                }
                else
                {
                    if (_pro.SaleStatus != Entities.ProductInfo.ProductSaleStatus.OnSale || _pro.AuditStatus == Entities.ProductInfo.ProductAuditStatus.InfractionSaleOff)
                    {
                        item.ShowStatus = 3;
                    }
                    else
                    {
                        item.ShowStatus = 1;
                    }
                }
            }
            #region 价格更新
            //会员折扣
            decimal discount = 1M;
            long SelfShopId = 0;
            long currentUserId = 0;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
                var shopInfo = ShopApplication.GetSelfShop();
                SelfShopId = shopInfo.Id;
                currentUserId = CurrentUser.Id;
            }
            //填充商品和购物车数据
            var ids = result.Data.Select(d => d.ProductId).ToArray();
            List<Product> products = ProductManagerApplication.GetProductsByIds(ids);
            List<SKU> skus = ProductManagerApplication.GetSKUByProducts(ids);
            List<Mall.Entities.ShoppingCartItem> cartitems = CartApplication.GetCartQuantityByIds(currentUserId, ids);
            List<dynamic> productList = new List<dynamic>();
            var productSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;
            foreach (var item in result.Data)
            {
                Product proInfo = products.Where(d => d.Id == item.ProductId).FirstOrDefault();
                if (proInfo == null)
                {
                    continue;
                }
                SKU skuInfo = skus.Where(d => d.ProductId == item.ProductId).FirstOrDefault();
                bool hasSku = proInfo.HasSKU;
                decimal marketPrice = Core.Helper.TypeHelper.ObjectToDecimal(proInfo.MarketPrice);
                string skuId = skuInfo.Id;
                int quantity = 0;
                quantity = cartitems.Where(d => d.ProductId == item.ProductId).Sum(d => d.Quantity);
                //decimal salePrice = item.MinSalePrice;
                item.ImagePath = Core.MallIO.GetRomoteProductSizeImage(Core.MallIO.GetImagePath(item.ImagePath), 1, (int)Mall.CommonModel.ImageSize.Size_350);
                //if (item.ShopId == SelfShopId)
                //    salePrice = item.MinSalePrice * discount;

                long activeId = 0;
                int activetype = 0;
                var limitbuyser = ServiceProvider.Instance<ILimitTimeBuyService>.Create;
                var limitBuy = limitbuyser.GetLimitTimeMarketItemByProductId(item.ProductId);
                if (limitBuy != null)
                {
                    //salePrice = limitBuy.MinPrice;
                    activeId = limitBuy.Id;
                    activetype = 1;
                }
                else
                {
                    #region 限时购预热
                    var FlashSale = limitbuyser.IsFlashSaleDoesNotStarted(item.ProductId);
                    var FlashSaleConfig = limitbuyser.GetConfig();

                    if (FlashSale != null)
                    {
                        TimeSpan flashSaleTime = DateTime.Parse(FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                        TimeSpan preheatTime = new TimeSpan(FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                        if (preheatTime >= flashSaleTime)  //预热大于开始
                        {
                            if (!FlashSaleConfig.IsNormalPurchase)
                            {
                                activeId = FlashSale.Id;
                                activetype = 1;
                            }
                        }
                    }
                    #endregion
                }
                //var activeInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(item.ProductId);
                //if (activeInfo != null)
                //{
                //    item.SalePrice = activeInfo.MiniGroupPrice;
                //    activeId = activeInfo.Id;
                //    activetype = 2;
                //}
                var pro = new
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Pic = item.ImagePath,// GetProductImageFullPath(d.Field<string>("ThumbnailUrl310")),
                    MarketPrice = marketPrice.ToString("0.##"),//市场价
                    SalePrice = item.SalePrice.ToString("0.##"),//当前价
                    SaleCounts = item.SaleCount + Mall.Core.Helper.TypeHelper.ObjectToInt(item.VirtualSaleCounts),
                    ProductSaleCountOnOff = productSaleCountOnOff,
                    CartQuantity = quantity,// item.cartquantity,
                    HasSKU = hasSku,// d.Field<bool>("HasSKU"),//是否有规格
                    //Stock = skuInfo.Stock,//默认规格库存
                    Stock = skus.Where(d => d.ProductId == item.ProductId).Sum(p => p.Stock),
                    SkuId = skuId,// d.Field<string>("SkuId"),//规格ID
                    ShowStatus = item.ShowStatus,
                    ActiveId = activeId,//活动Id
                    ActiveType = activetype,//活动类型（1代表限购，2代表团购，3代表商品预售，4代表限购预售，5代表团购预售）
                    IsVirtual = item.ProductType == 1
                };
                productList.Add(pro);
            }
            #endregion
            var json = Json(data: productList);
            return json;
        }

        /// <summary>
        /// 门店搜索商品
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="keyWords"></param>
        /// <param name="productId"></param>
        /// <param name="shopCategoryId"></param>
        /// <param name="categoryId"></param>
        /// <param name="type"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetProductList")]
        public object GetProductList(long shopId, long shopBranchId, string keyWords = "", long? productId = null, long? shopCategoryId = null, long? categoryId = null, int type = 0, int pageNo = 1, int pageSize = 10)
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
            ShopBranchProductQuery query = new ShopBranchProductQuery();
            query.PageSize = pageSize;
            query.PageNo = pageNo;
            query.KeyWords = keyWords;
            query.ShopId = shopId;
            query.ShopBranchId = shopBranchId;
            query.RproductId = productId;
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            query.OrderKey = 5;
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

            var dtNow = DateTime.Now;
            query.StartDate = dtNow.AddYears(-10).Date;
            query.EndDate = dtNow;
            var pageModel = ShopBranchApplication.GetShopBranchProductsMonth(query, dtNow.AddYears(-10).Date, dtNow);
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
                query.ShopCategoryId = null;
                query.CategoryId = null;
                var topModel = ShopBranchApplication.GetShopBranchProductsMonth(query, dtNow.AddDays(-30).Date, dtNow);
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
                var comcount = CommentApplication.GetProductCommentStatistic(productId: item.Id, shopBranchId: shopBranchId);
                var sbskus = ShopBranchApplication.GetSkusByProductId(shopBranchId, item.Id);
                return new
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    CategoryName = ShopCategoryApplication.GetCategoryByProductId(item.Id).Name,
                    MeasureUnit = item.MeasureUnit,
                    MinSalePrice = item.MinSalePrice.ToString("f2"),
                    SaleCounts = item.ShopBranchSaleCounts,//销量统计没有考虑订单支付完成。
                    isSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1,
                    MarketPrice = item.MarketPrice,
                    HasSku = item.HasSKU,
                    Quantity = item.Quantity,
                    IsTop = item.Id == (productId == null ? 0 : productId),
                    DefaultImage = MallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)ImageSize.Size_350),
                    HighCommentCount = comcount.HighComment,
                    Stock = sbskus.Sum(d => d.Stock),
                    IsVirtual = item.ProductType == 1
                };
            }).OrderByDescending(d => d.IsTop).ToList();

            return Json(new
            {
                Products = product,
                Total = pageModel.Total
            });
        }
        /// <summary>
        /// 获取商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetProductDetail")]
        public object GetProductDetail(string openId, long ProductID, long shopBranchId = 0)
        {
            //CheckUserLogin();
            if (shopBranchId > 0)
            {
                CheckOpenStore();
            }
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
            long activeId = 0;
            int activetype = 0;

            product = ServiceProvider.Instance<IProductService>.Create.GetProduct(ProductID);

            if (product == null || product.IsDeleted)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "该商品已被删除或者转移");
            }
            List<ShopBranchSkuInfo> branchskuList = null;
            if (shopBranchId > 0)
            {
                var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
                if (shopBranch == null)
                {
                    throw new MallApiException(ApiErrorCode.Parameter_Error, "shopBranchId");
                }
                if (shopBranch.Status == ShopBranchStatus.Freeze)
                {
                    throw new MallApiException(ApiErrorCode.Parameter_Error, "门店已冻结");
                }
                if (!ShopBranchApplication.CheckProductIsExist(shopBranchId, ProductID))
                {
                    throw new MallApiException(ApiErrorCode.Parameter_Error, "该商品已被删除或者转移");
                }

                var comment = CommentApplication.GetProductCommentStatistic(productId: ProductID,
                        shopBranchId: shopBranchId);

                branchskuList = ShopBranchApplication.GetSkus(shopBranch.ShopId, shopBranch.Id, null);
                if (branchskuList == null || branchskuList.Count <= 0)
                {
                    throw new Mall.Core.MallException("门店商品不存在");
                }
            }
            var cashDepositModel = ServiceProvider.Instance<ICashDepositsService>.Create.GetCashDepositsObligation(product.Id);//提供服务（消费者保障、七天无理由、及时发货）
            model.CashDepositsServer = cashDepositModel;

            #region 根据运费模板获取发货地址
            var freightTemplateService = ServiceApplication.Create<IFreightTemplateService>();
            FreightTemplateInfo template = new FreightTemplateInfo();
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
            var mark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;

            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = CommentApplication.GetProductAverageMark(ProductID);
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
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
            #endregion
            #region 商品SKU


            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(ProductID);
            Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            if (limitBuy != null && shopBranchId <= 0)
            {
                var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                var limitSkuItem = limitSku.Details.OrderBy(d => d.Price).FirstOrDefault();
                if (limitSkuItem != null)
                    product.MinSalePrice = limitSkuItem.Price;
            }
            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();


            long[] Ids = { ProductID };
            long userId = 0;
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
                if (shop.IsSelf)
                {
                    discount = CurrentUser.MemberDiscount;
                }
            }
            List<Mall.Entities.ShoppingCartItem> cartitems = CartApplication.GetCartQuantityByIds(userId, Ids);
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            if (skus.Count() > 0)
            {
                #region 颜色
                long colorId = 0, sizeId = 0, versionId = 0;
                List<ProductSKUItem> colorAttributeValue = new List<ProductSKUItem>();
                List<string> listcolor = new List<string>();
                foreach (var sku in skus)
                {
                    var specs = sku.Id.Split('_');
                    if (shopBranchId > 0 && !branchskuList.Any(x => x.SkuId == sku.Id))
                    {
                        continue;
                    }
                    if (specs.Count() > 0 && !string.IsNullOrEmpty(sku.Color))
                    {
                        if (long.TryParse(specs[1], out colorId)) { }//相同颜色规格累加对应值
                        if (colorId != 0)
                        {
                            if (!listcolor.Contains(sku.Color))
                            {
                                var c = skus.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                var colorvalue = new ProductSKUItem
                                {
                                    ValueId = colorId,
                                    UseAttributeImage = false,
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
                    AttributeName = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = colorAttributeValue.OrderBy(p => p.ValueId),
                    AttributeIndex = 0,
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion

                #region 容量
                List<ProductSKUItem> sizeAttributeValue = new List<ProductSKUItem>();
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
                                var sizeValue = new ProductSKUItem
                                {
                                    ValueId = sizeId,
                                    UseAttributeImage = false,
                                    Value = sku.Size,
                                    ImageUrl = ""// Mall.Core.MallIO.GetRomoteImagePath(sku.ShowPic)
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
                    AttributeValue = sizeAttributeValue.OrderBy(p => p.ValueId),
                    AttributeIndex = 1,
                };
                if (sizeId > 0)
                {
                    SkuItemList.Add(size);
                }
                #endregion

                #region 规格
                List<ProductSKUItem> versionAttributeValue = new List<ProductSKUItem>();
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
                                var versionValue = new ProductSKUItem
                                {
                                    ValueId = versionId,
                                    UseAttributeImage = false,
                                    Value = sku.Version,
                                    ImageUrl = ""// Mall.Core.MallIO.GetRomoteImagePath(sku.ShowPic)
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
                    AttributeValue = versionAttributeValue.OrderBy(p => p.ValueId),
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
                    int quantity = 0;
                    quantity = cartitems.Where(d => d.SkuId == sku.Id).Sum(d => d.Quantity);//购物车购买数量
                    ShopBranchSkuInfo shopSku = null;
                    if (shopBranchId > 0)
                    {
                        shopSku = branchskuList.Where(x => x.SkuId == sku.Id).FirstOrDefault();
                        if (shopSku != null) sku.Stock = shopSku.Stock;
                    }
                    var prosku = new
                    {
                        SkuItems = "",
                        MemberPrices = "",
                        SkuId = sku.Id,
                        ProductId = product.Id,
                        SKU = sku.Sku,
                        Weight = 0,
                        Stock = shopSku != null ? shopSku.Stock : sku.Stock,
                        WarningStock = sku.SafeStock,
                        CostPrice = sku.CostPrice,
                        SalePrice = shop.IsSelf ? decimal.Parse((sku.SalePrice * discount).ToString("F2")) : sku.SalePrice,
                        StoreStock = 0,
                        StoreSalePrice = 0,
                        OldSalePrice = 0,
                        ImageUrl = "",
                        ThumbnailUrl40 = "",
                        ThumbnailUrl410 = "",
                        MaxStock = 0,
                        FreezeStock = 0,
                        Quantity = quantity
                    };
                    Skus.Add(prosku);
                }
                #endregion
            }
            #endregion            
            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations(ProductID);
            var comments = CommentApplication.GetCommentsByProduct(product.Id, shopBranchId);
            var total = comments.Count(item => !item.IsHidden);
            var niceTotal = comments.Count(item => item.ReviewMark >= 4);
            bool isFavorite = false;
            bool IsFavoriteShop = false;
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
            }

            decimal maxprice = shop.IsSelf ? skus.Max(d => d.SalePrice) * discount : skus.Max(d => d.SalePrice);//最高SKU价格
            decimal minprice = shop.IsSelf ? skus.Min(d => d.SalePrice) * discount : skus.Min(d => d.SalePrice);//最低价

            var productImage = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                if (Core.MallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    var path = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, i, (int)Mall.CommonModel.ImageSize.Size_350);
                    productImage.Add(path);
                }
            }
            var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(product.Id, shop.IsSelf, discount);
            var isValidLimitBuy = false;
            if (limitBuy != null && shopBranchId <= 0)
            {
                maxprice = limitBuy.MinPrice;
                minprice = limitBuy.MinPrice;
                activeId = limitBuy.Id;
                activetype = 1;
                isValidLimitBuy = true;
            }
            else
            {
                #region 限时购预热
                var FlashSale = ServiceProvider.Instance<ILimitTimeBuyService>.Create.IsFlashSaleDoesNotStarted(product.Id);
                var FlashSaleConfig = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetConfig();

                if (FlashSale != null && shopBranchId <= 0)
                {
                    TimeSpan flashSaleTime = DateTime.Parse(FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                    TimeSpan preheatTime = new TimeSpan(FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                    if (preheatTime >= flashSaleTime)  //预热大于开始
                    {
                        if (!FlashSaleConfig.IsNormalPurchase)
                        {
                            isValidLimitBuy = true;
                        }
                    }
                }
                #endregion              

            }
            if (product.IsOpenLadder)
            {
                var ladder = ladderPrices.OrderBy(l => l.MinBath).FirstOrDefault();
                if (ladder != null)
                    minprice = ladder.Price;
            }
            var desc = ProductManagerApplication.GetProductDescription(product.Id);
            model.Product = new ProductInfoModel()
            {
                ProductId = product.Id,
                CommentCount = CommentApplication.GetCommentCountByProduct(product.Id),
                Consultations = consultations.Count(),
                ImagePath = productImage,
                IsFavorite = isFavorite,
                MarketPrice = product.MarketPrice,
                NicePercent = model.Shop.ProductMark == 0 || niceTotal == 0 ? 100 : (int)((niceTotal / total) * 100),
                ProductName = product.ProductName,
                ProductSaleStatus = product.SaleStatus,
                AuditStatus = product.AuditStatus,
                ShortDescription = product.ShortDescription,
                ProductDescription = GetProductDescription(desc),
                IsOnLimitBuy = limitBuy != null,
                IsOpenLadder = product.IsOpenLadder,
                MinMath = ProductManagerApplication.GetProductLadderMinMath(product.Id)
            };


            #endregion
            LogProduct(ProductID);
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);

            //图片集合
            List<object> ProductImgs = new List<object>();
            for (int i = 1; i < 5; i++)
            {
                if (i == 1 || Mall.Core.MallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    //productImage.Add(Core.MallIO.GetRomoteImagePath(product.RelativePath + string.Format("/{0}.png", i)));
                    ProductImgs.Add(Core.MallIO.GetRomoteProductSizeImage(product.ImagePath, i, (int)ImageSize.Size_350));
                }
            }
            //优惠劵
            var coupons = GetShopCouponList(shop.Id);


            dynamic Promotes = new System.Dynamic.ExpandoObject();
            //ProductActives actives = new ProductActives();
            var freeFreight = ServiceApplication.Create<IShopService>().GetShopFreeFreight(shop.Id);
            Promotes.freeFreight = freeFreight;
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
            #endregion

            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(product.Id, shop.Id);
            if (fullDiscount != null)
            {
                Promotes.FullDiscount = fullDiscount;
            }

            string skuId = skus.FirstOrDefault()?.Id ?? string.Empty;
            int addressId = 0;
            if (CurrentUser != null)
            {
                var addressInfo = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                if (addressInfo != null)
                {
                    addressId = addressInfo.RegionId;
                }
            }
            var dtpro = ProductManagerApplication.GetProduct(product.Id);
            bool hasFightGroup = false;
            decimal fightFroupPrice = 0;
            //string fightGroupUrl = "";
            var fgactobj = FightGroupApplication.GetActiveAndItemsByProductId(product.Id);
            if (fgactobj != null)
            {
                if (fgactobj.ActiveStatus == FightGroupActiveStatus.Ongoing)  //只跳转进行中的活动
                {
                    hasFightGroup = true;
                    activeId = fgactobj.Id;
                    fightFroupPrice = fgactobj.ActiveItems.Min(p => p.ActivePrice);
                    //fightGroupUrl = "/m-WeiXin/FightGroup/Detail/" + fgactobj.Id;
                }
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
            long saleCounts = 0;
            if (shopBranchId > 0)
            {
                var dtNow = DateTime.Now;
                saleCounts = OrderApplication.GetSaleCount(dtNow.AddDays(-30).Date, dtNow, shopBranchId: shopBranchId, productId: product.Id);
                //saleCounts = ShopBranchApplication.GetProductSaleCount(shopBranchId, product.Id, dtNow.AddDays(-30).Date, dtNow);
                saleCounts = saleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(product.VirtualSaleCounts);
            }
            else
            {
                saleCounts = product.SaleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(product.VirtualSaleCounts);
            }

            #region 购物车总量
            int cartcount = 0;
            if (shopBranchId > 0)
            {
                Mall.Entities.ShoppingCartInfo memberCartInfo = null;
                if (CurrentUser != null)
                {
                    //如果已登陆取购物车数据
                    memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id, shopBranchId);
                }
                if (memberCartInfo != null)
                {
                    var shopcartinfo = memberCartInfo.Items.Where(d => d.ProductId == ProductID);
                    //var _iShopBranchService = ServiceApplication.Create<IShopBranchService>();
                    foreach (var cartitem in shopcartinfo)
                    {
                        var branchskuInfo = branchskuList.FirstOrDefault(x => x.SkuId == cartitem.SkuId);
                        if (branchskuInfo.Status == ShopBranchSkuStatus.Normal && branchskuInfo.Stock >= cartitem.Quantity)
                        {
                            cartcount += cartitem.Quantity;
                        }
                    }
                }
            }
            #endregion
            
            return Json(new
            {
                IsFavorite = isFavorite,
                IsFavoriteShop = IsFavoriteShop,
                ProductId = product.Id,
                ProductName = product.ProductName,
                ShortDescription = product.ShortDescription,
                MetaDescription = model.Product.ProductDescription.Replace("\"/Storage/Shop", "\"" + Core.MallIO.GetImagePath("/Storage/Shop")),//替换链接  /Storage/Shop
                MarketPrice = product.MarketPrice.ToString("0.##"),//市场价
                IsfreeShipping = "False",//是否免费送货
                MaxSalePrice = maxprice.ToString("0.##"),
                MinSalePrice = minprice.ToString("0.##"),//限时抢购或商城价格
                ThumbnailUrl60 = Core.MallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350),
                ProductImgs = ProductImgs,
                ReviewCount = total,
                Stock = skus.Max(d => d.Stock),
                SkuItemList = SkuItemList,
                Skus = Skus,
                Coupons = coupons,//优惠劵
                Promotes = Promotes,//活动
                IsUnSale = product.SaleStatus == Entities.ProductInfo.ProductSaleStatus.InStock ? true : false,
                ProductSaleStatus = product.SaleStatus.GetHashCode(),
                AuditStatus = product.AuditStatus.GetHashCode(),
                ShowStatus = ProductManagerApplication.GetProductShowStatus(dtpro),
                ActiveId = activeId,
                ActiveType = activetype,
                IsOpenLadder = product.IsOpenLadder,//是否开启阶梯价
                LadderPrices = ladderPrices,//阶梯价
                MinBath = model.Product.MinMath,//最小批量
                Shop = model.Shop,
                VShopLog = Mall.Core.MallIO.GetRomoteImagePath(model.VShopLog),
                MeasureUnit = string.IsNullOrEmpty(product.MeasureUnit) ? "" : product.MeasureUnit, //单位
                MaxBuyCount = product.MaxBuyCount,//限购数
                IsOnLimitBuy = isValidLimitBuy,
                IsSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1,//是否显示销量
                ShowSaleCounts = saleCounts,
                //Freight = GetFreightStr(product.Id, discount, skuId, addressId),//运费
                Freight = product.ProductType == 1 ? "0" : FreightTemplateApplication.GetFreightStr(product.Id, model.FreightTemplate, CurrentUser, product),
                SendTime = (model.FreightTemplate != null && !string.IsNullOrEmpty(model.FreightTemplate.SendTime) ? (model.FreightTemplate.SendTime + "h内发货") : ""), //运费模板发货时间
                hasFightGroup = hasFightGroup,
                fightFroupPrice = fightFroupPrice,
                ProductType = product.ProductType,
                VirtualProductInfo = virtualPInfo,
                VirtualProductItemModels = virtualProductItemModels,
                BonusCount = BonusCount,
                BonusGrantPrice = BonusGrantPrice,
                BonusRandomAmountStart = BonusRandomAmountStart,
                BonusRandomAmountEnd = BonusRandomAmountEnd,
                CartCount = cartcount
                //fightGroupUrl = fightGroupUrl
            });
        }
        /// <summary>
        /// 判断商品是否参加限时购
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetIsOnLimitBuy")]
        public object GetIsOnLimitBuy(string openId, long ProductID)
        {
            var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();
            var flashSaleModel = iLimitService.GetFlaseSaleByProductId(ProductID);
            var flashSaleConfig = iLimitService.GetConfig();

            if (flashSaleModel != null)
            {
                if (DateTime.Parse(flashSaleModel.BeginDate) > DateTime.Now)
                {
                    TimeSpan flashSaleTime = DateTime.Parse(flashSaleModel.BeginDate) - DateTime.Now;  //开始时间还剩多久
                    TimeSpan preheatTime = new TimeSpan(flashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                    if (preheatTime >= flashSaleTime)  //预热大于开始
                    {

                        if (!flashSaleConfig.IsNormalPurchase)
                            return Json(data: new { Id = flashSaleModel.Id });
                    }
                }
                else
                {
                    return Json(data: new { Id = flashSaleModel.Id });
                }
            }
            return Json(ErrorResult<dynamic>(msg: "可以正常购买"));

        }
        /// <summary>
        /// 获取商品的规格信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetProductSkus")]
        public object GetProductSkus(long productId, string openId = "")
        {
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(productId);
            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(productId);
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            decimal discount = 1M;
            if (CurrentUser != null && shopInfo.IsSelf)
            {
                discount = CurrentUser.MemberDiscount;
            }
            Mall.Entities.ShoppingCartInfo cartInfo = CartApplication.GetCart(CurrentUser.Id);

            var skuArray = new List<ProductSKUModel>();
            object defaultsku = new object();
            int activetype = 0;

            decimal SalePrice = 0;
            var skus = ProductManagerApplication.GetSKUs(productId);
            foreach (var sku in skus.Where(s => s.Stock > 0))
            {
                var price = SalePrice = sku.SalePrice;
                if (product.IsOpenLadder)
                {
                    var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(product.Id);
                    var ladder = ladderPrices.OrderBy(l => l.MinBath).FirstOrDefault();
                    if (ladder != null)
                    {
                        price = SalePrice = ladder.Price;
                    }

                }
                SalePrice = shopInfo.IsSelf ? SalePrice * discount : SalePrice;
                price = shopInfo.IsSelf ? price * discount : price;
                ProductSKUModel skuMode = new ProductSKUModel
                {
                    Price = price,
                    SkuId = sku.Id,
                    Stock = sku.Stock
                };
                if (limitBuy != null)
                {
                    activetype = 1;
                    var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                    var limitSkuItem = limitSku.Details.Where(r => r.SkuId.Equals(sku.Id)).FirstOrDefault();
                    if (limitSkuItem != null)
                        skuMode.Price = limitSkuItem.Price;
                }
                skuArray.Add(skuMode);
            }

            Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();
            //var skus = ProductManagerApplication.GetSKUs(product.Id);
            if (skus.Count > 0)
            {
                #region 颜色
                long colorId = 0, sizeId = 0, versionId = 0;
                List<ProductSKUItem> colorAttributeValue = new List<ProductSKUItem>();
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
                                var colorvalue = new ProductSKUItem
                                {
                                    ValueId = colorId,
                                    Value = sku.Color,
                                    UseAttributeImage = false,
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
                    AttributeName = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,
                    AttributeValue = colorAttributeValue.OrderBy(p => p.ValueId),
                    AttributeIndex = 0,
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion

                #region 容量
                List<ProductSKUItem> sizeAttributeValue = new List<ProductSKUItem>();
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
                                var sizeValue = new ProductSKUItem
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
                    AttributeName = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias,
                    AttributeValue = sizeAttributeValue.OrderBy(p => p.ValueId),
                    AttributeIndex = 1,
                };
                if (sizeId > 0)
                {
                    SkuItemList.Add(size);
                }
                #endregion

                #region 规格
                List<ProductSKUItem> versionAttributeValue = new List<ProductSKUItem>();
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
                                var versionValue = new ProductSKUItem
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
                    AttributeName = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias,
                    AttributeValue = versionAttributeValue.OrderBy(p => p.ValueId),
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
                    var sku_price = skuArray.FirstOrDefault(e => e.SkuId == sku.Id);
                    var prosku = new
                    {
                        SkuId = sku.Id,
                        SKU = sku.Sku,
                        Weight = product.Weight,
                        Stock = sku.Stock,
                        WarningStock = sku.SafeStock,
                        SalePrice = sku_price != null ? sku_price.Price.ToString("0.##") : sku.SalePrice.ToString("0.##"),
                        CartQuantity = cartInfo.Items.Where(d => d.SkuId == sku.Id).Sum(d => d.Quantity),
                        ImageUrl = Core.MallIO.GetRomoteProductSizeImage(sku.ShowPic, 1, (int)ImageSize.Size_350)
                    };
                    Skus.Add(prosku);
                }
                defaultsku = Skus[0];
                #endregion
            }
            var json = Json(new
            {
                ProductId = productId,
                ProductName = product.ProductName,
                ImageUrl = Core.MallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350), //GetImageFullPath(model.SubmitOrderImg),
                Stock = skuArray.Sum(s => s.Stock),// skus.Sum(s => s.Stock),
                ActivityUrl = activetype,
                SkuItems = SkuItemList,
                Skus = Skus,
                DefaultSku = defaultsku
            });
            return json;
        }

        /// <summary>
        /// 根据商品Id获取商品规格【门店】
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
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            decimal discount = 1M;
            if (CurrentUser != null && shopInfo.IsSelf)
            {
                discount = CurrentUser.MemberDiscount;
            }

            var skuArray = new List<ProductSKUModel>();
            object defaultsku = new object();
            Mall.Entities.ShoppingCartInfo cartInfo = null;
            long userId = 0;
            if (CurrentUser != null)
            {
                cartInfo = _iBranchCartService.GetCart(CurrentUser.Id, shopBranchId);//获取购物车数据
                userId = CurrentUser.Id;
            }

            var shopBranchInfo = ShopBranchApplication.GetShopBranchById(shopBranchId);
            var branchskuList = ShopBranchApplication.GetSkus(shopBranchInfo.ShopId, shopBranchId);
            var shopcartinfo = new BranchCartHelper().GetCart(userId, shopBranchId);
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            foreach (var sku in skus)
            {
                decimal price = 1M;
                if (shopInfo.IsSelf)
                {
                    price = sku.SalePrice * discount;
                }
                else
                {
                    price = sku.SalePrice;
                }
                if (branchskuList.Count(x => x.SkuId == sku.Id && x.Stock > 0) > 0)
                {
                    var skuCartNumber = 0;
                    if (shopcartinfo != null && shopcartinfo.Items != null && shopcartinfo.Items.Count() > 0)
                    {
                        var _tmp = shopcartinfo.Items.FirstOrDefault(x => x.SkuId == sku.Id);
                        if (_tmp != null)
                        {
                            skuCartNumber = _tmp.Quantity;
                        }
                    }
                    skuArray.Add(new ProductSKUModel
                    {
                        Price = price,
                        SkuId = sku.Id,
                        Stock = branchskuList.FirstOrDefault(x => x.SkuId == sku.Id).Stock
                    });
                }
            }

            Entities.TypeInfo typeInfo = _iTypeService.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();
            if (skus.Count > 0)
            {
                #region 颜色
                long colorId = 0, sizeId = 0, versionId = 0;
                var colorAttributeValue = new List<ProductSKUItem>();
                List<string> listcolor = new List<string>();
                foreach (var sku in skus)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0 && !string.IsNullOrEmpty(sku.Color))
                    {
                        if (long.TryParse(specs[1], out colorId)) { }//相同颜色规格累加对应值
                        if (colorId != 0)
                        {
                            sku.ColorId = colorId;
                            if (!listcolor.Contains(sku.Color))
                            {
                                var colorvalue = new ProductSKUItem
                                {
                                    ValueId = colorId,
                                    Value = sku.Color,
                                    UseAttributeImage = true,
                                    ImageUrl = MallIO.GetRomoteImagePath(sku.ShowPic)
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
                    AttributeName = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,//如果商品有自定义规格则用
                    AttributeValue = colorAttributeValue.OrderBy(p => p.ValueId).ToList(),
                    AttributeIndex = 0,
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion

                #region 容量
                var sizeAttributeValue = new List<ProductSKUItem>();
                List<string> listsize = new List<string>();
                foreach (var sku in skus.OrderBy(a => a.Size))
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            sku.SizeId = sizeId;
                            if (!listsize.Contains(sku.Size))
                            {
                                var sizeValue = new ProductSKUItem
                                {
                                    ValueId = sizeId,
                                    Value = sku.Size,
                                    UseAttributeImage = false,
                                    ImageUrl = MallIO.GetRomoteImagePath(sku.ShowPic)
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
                    AttributeName = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias,
                    AttributeValue = sizeAttributeValue.OrderBy(p => p.ValueId).ToList(),
                    AttributeIndex = 1,
                };
                if (sizeId > 0)
                {
                    SkuItemList.Add(size);
                }
                #endregion

                #region 规格
                var versionAttributeValue = new List<ProductSKUItem>();
                List<string> listversion = new List<string>();
                foreach (var sku in skus)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            sku.VersionId = versionId;
                            if (!listversion.Contains(sku.Version))
                            {
                                var versionValue = new ProductSKUItem
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
                    AttributeName = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias,
                    AttributeValue = versionAttributeValue.OrderBy(p => p.ValueId).ToList(),
                    AttributeIndex = 2,
                };
                if (versionId > 0)
                {
                    SkuItemList.Add(version);
                }
                #endregion

                #region Sku值
                foreach (var sku in skus.OrderBy(p => p.ColorId).OrderBy(p => p.SizeId).OrderBy(p => p.VersionId))
                {
                    var prosku = new
                    {
                        SkuId = sku.Id,
                        SKU = sku.Sku,
                        Weight = product.Weight,
                        Stock = branchskuList.FirstOrDefault(x => x.SkuId == sku.Id).Stock,
                        WarningStock = sku.SafeStock,
                        SalePrice = shopInfo.IsSelf ? (sku.SalePrice * discount).ToString("0.##") : sku.SalePrice.ToString("0.##"),
                        CartQuantity = cartInfo != null ? cartInfo.Items.Where(d => d.SkuId == sku.Id && d.ShopBranchId == shopBranchId).Sum(d => d.Quantity) : 0,
                        ImageUrl = Core.MallIO.GetRomoteProductSizeImage(sku.ShowPic, 1, (int)ImageSize.Size_350)
                    };
                    Skus.Add(prosku);
                }
                defaultsku = Skus[0];
                #endregion
            }
            return Json(new
            {
                ProductId = id,
                ProductName = product.ProductName,
                ImageUrl = Core.MallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350), //GetImageFullPath(model.SubmitOrderImg),
                Stock = skuArray.Sum(s => s.Stock),// skus.Sum(s => s.Stock),
                                                   //ActivityUrl = activetype,
                SkuItems = SkuItemList,
                Skus = Skus,
                DefaultSku = defaultsku
            });
        }

        /// <summary>
        /// 获取门店商品规格【门店】
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

            Mall.Entities.ShoppingCartInfo memberCartInfo = null;
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                //如果已登陆取购物车数据
                memberCartInfo = _iBranchCartService.GetCart(CurrentUser.Id, bid);
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            var skuArray = new List<ProductSKUModel>();
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            foreach (var sku in skus.Where(s => s.Stock > 0))
            {
                decimal price = 1M;
                if (shopInfo.IsSelf)
                {
                    price = sku.SalePrice * discount;
                }
                else
                {
                    price = sku.SalePrice;
                }
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
            return Json(skuArray);
        }

        //public object GetSellOut(long pid)
        //{
        //    var skus = ProductManagerApplication.GetSKUs(pid);
        //    long stock = skus.Sum(a => a.Stock);
        //    return Json(new { issellout = stock });
        //}
        /// <summary>
        /// 商品评价数接口
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetStatisticsReview")]
        public object GetStatisticsReview(long productId, long shopBranchId = 0)
        {
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(productId);
            var statistic = CommentApplication.GetProductCommentStatistic(productId, shopBranchId: shopBranchId);
            var json = Json(new
            {
                productName = product.ProductName,
                reviewNum = statistic.AllComment,
                reviewNum1 = statistic.HighComment,
                reviewNum2 = statistic.MediumComment,
                reviewNum3 = statistic.LowComment,
                reviewNumImg = statistic.HasImageComment,
                appendReviewNum = statistic.AppendComment
            });
            return json;
        }
        /// <summary>
        /// 商品评价列表
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetLoadReview")]
        public object GetLoadReview(long productId, int pageIndex, int pageSize, int type, long shopBranchId = 0)
        {
            var query = new ProductCommentQuery
            {
                ProductId = productId,
                PageNo = pageIndex,
                PageSize = pageSize,
            };
            switch (type)
            {
                case 1: query.CommentType = ProductCommentMarkType.High; break;
                case 2: query.CommentType = ProductCommentMarkType.Medium; break;
                case 3: query.CommentType = ProductCommentMarkType.Low; break;
                case 4: query.CommentType = ProductCommentMarkType.HasImage; break;
                case 5: query.CommentType = ProductCommentMarkType.Append; break;
            }
            if (shopBranchId > 0)
                query.ShopBranchId = shopBranchId;
            var comments = CommentApplication.GetComments(query);
            var members = MemberApplication.GetMembers(comments.Models.Select(p => p.UserId).ToList());
            var orderItems = OrderApplication.GetOrderItems(comments.Models.Select(c => c.SubOrderId)).ToDictionary(item => item.Id, item => item);
            var orders = OrderApplication.GetOrders(orderItems.Values.Select(a => a.OrderId)).ToDictionary(o => o.Id, o => o);
            var commentImages = CommentApplication.GetProductCommentImagesByCommentIds(comments.Models.Select(a => a.Id));
            var data = comments.Models.Select(item =>
            {
                var product = ProductManagerApplication.GetProduct(item.ProductId);
                string picture = MallIO.GetRomoteImagePath(members.FirstOrDefault(p => p.Id == item.UserId)?.Photo ?? string.Empty);
                var typeInfo = ServiceApplication.Create<ITypeService>().GetTypeByProductId(item.ProductId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                List<string> Images = commentImages.Where(a => a.CommentId == item.Id && a.CommentType == 0).Select(a => a.CommentImage).ToList();//首评图片
                string images1 = "", images2 = "", images3 = "", images4 = "", images5 = "";
                for (int i = 0; i < Images.Count; i++)
                {
                    if (i == 0)
                    {
                        images1 = MallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                    if (i == 1)
                    {
                        images2 = Core.MallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                    if (i == 2)
                    {
                        images3 = Core.MallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                    if (i == 3)
                    {
                        images4 = Core.MallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                    if (i == 4)
                    {
                        images5 = Core.MallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                }

                return new
                {
                    UserName = item.UserName,
                    Picture = picture,
                    ProductId = item.ProductId,
                    ProductName = product.ProductName,
                    ThumbnailUrl100 = Core.MallIO.GetRomoteImagePath(product.ImagePath),
                    ReviewText = item.ReviewContent,
                    AppendContent = item.AppendContent,
                    SKUContent = "",
                    AppendDate = item.AppendDate.HasValue ? item.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    ReplyAppendContent = item.ReplyAppendContent,
                    ReplyAppendDate = item.ReplyAppendDate.HasValue ? item.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    FinshDate = orders[orderItems[item.SubOrderId].OrderId].FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Images1 = images1,//首评图片
                    Images2 = images2,
                    Images3 = images3,
                    Images4 = images4,
                    Images5 = images5,
                    AppendImages = commentImages.Where(a => a.CommentId == item.Id && a.CommentType == 1).Select(a => new { CommentImage = Core.MallIO.GetRomoteImagePath(MoveImages(a.CommentImage)) }),//追加图片
                    ReviewDate = item.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ReplyText = string.IsNullOrWhiteSpace(item.ReplyContent) ? null : item.ReplyContent,
                    ReplyDate = item.ReplyDate.HasValue ? item.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                    ReviewMark = item.ReviewMark,
                    BuyDate = orders[orderItems[item.SubOrderId].OrderId].OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Color = orderItems[item.SubOrderId].Color == null ? "" : orderItems[item.SubOrderId].Color,
                    Version = orderItems[item.SubOrderId].Version == null ? "" : orderItems[item.SubOrderId].Version,
                    Size = orderItems[item.SubOrderId].Size == null ? "" : orderItems[item.SubOrderId].Size,
                    ColorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,//如果该商品有自定义规格名称则用
                    SizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias,
                    VersionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias
                };
            }).ToArray();
            var json = Json(new
            {
                totalCount = comments.Total,
                Data = data.AsEnumerable().Select(d => new
                {
                    UserName = d.UserName,
                    Picture = d.Picture,
                    ProductId = d.ProductId,
                    ThumbnailUrl100 = d.ThumbnailUrl100,
                    ProductName = d.ProductName,
                    SKUContent = d.SKUContent,
                    ReviewText = d.ReviewText,
                    Score = d.ReviewMark,
                    ImageUrl1 = d.Images1,
                    ImageUrl2 = d.Images2,
                    ImageUrl3 = d.Images3,
                    ImageUrl4 = d.Images4,
                    ImageUrl5 = d.Images5,
                    AppendImages = d.AppendImages,
                    ReplyText = d.ReplyText,
                    ReviewDate = d.ReviewDate,
                    ReplyDate = d.ReplyDate,
                    AppendContent = d.AppendContent,
                    AppendDate = d.AppendDate,
                    ReplyAppendContent = d.ReplyAppendContent,
                    ReplyAppendDate = d.ReplyAppendDate
                })
            });
            return json;
        }
        /// <summary>
        /// 添加商品评论（评价送积分）
        /// </summary>
        /// 

        [HttpGet("GetAddProductReview")]
        public object GetAddProductReview(string openId, string DataJson)
        {
            CheckUserLogin();
            if (!string.IsNullOrEmpty(DataJson))
            {
                bool result = false;
                List<AddOrderCommentModel> orderComment = DataJson.FromJSON<List<AddOrderCommentModel>>();
                if (orderComment != null)
                {
                    List<ProductComment> list = new List<ProductComment>();
                    string orderIds = "";
                    foreach (var item in orderComment)
                    {
                        AddOrderCommentModel ordercom = new AddOrderCommentModel();
                        ordercom.ReviewDate = DateTime.Now;
                        ordercom.UserId = CurrentUser.Id;
                        ordercom.UserName = CurrentUser.UserName;
                        ordercom.UserEmail = CurrentUser.Email;
                        ordercom.OrderId = item.OrderId;
                        if (!orderIds.Contains(item.OrderId))
                        {
                            AddOrderComment(ordercom, orderComment.Where(a => a.OrderId == item.OrderId).Count());//添加订单评价（订单评价只一次）
                            orderIds += item.OrderId + ",";
                        }

                        var model = new ProductComment();

                        var OrderInfo = ServiceApplication.Create<IOrderService>().GetOrderItemsByOrderId(long.Parse(item.OrderId)).Where(d => d.ProductId == item.ProductId).FirstOrDefault();
                        if (OrderInfo != null)
                        {
                            model.ReviewDate = DateTime.Now;
                            model.ReviewContent = item.ReviewText;
                            model.UserId = CurrentUser.Id;
                            model.UserName = CurrentUser.UserName;
                            model.Email = CurrentUser.Email;
                            model.SubOrderId = OrderInfo.Id;//订单明细Id
                            model.ReviewMark = item.Score;
                            model.ProductId = item.ProductId;
                            model.Images = new List<ProductCommentImage>();
                            foreach (var img in item.ImageUrl1.Split(','))
                            {
                                var p = new ProductCommentImage();

                                p.CommentType = 0;//0代表默认的表示评论的图片
                                p.CommentImage = Core.MallIO.GetImagePath(img);
                                if (!string.IsNullOrEmpty(p.CommentImage))
                                {
                                    model.Images.Add(p);
                                }
                            }
                            list.Add(model);
                        }
                        result = true;
                    }
                    CommentApplication.Add(list);
                }
                if (result)
                {
                    return Json(SuccessResult("评价成功", "评价成功"));
                }
                else
                {
                    return Json(ErrorResult("评价失败", "评价失败"));
                }
            }
            return Json(ApiResult<string>(true));
        }

        /// <summary>
        /// 增加订单评论
        /// </summary>
        /// <param name="comment"></param>
        void AddOrderComment(AddOrderCommentModel comment, int productNum)
        {
            TradeCommentApplication.Add(new OrderComment()
            {
                OrderId = long.Parse(comment.OrderId),
                DeliveryMark = 5,//物流评价
                ServiceMark = 5,//服务评价
                PackMark = 5,//包装评价
                UserId = comment.UserId,
                CommentDate = comment.ReviewDate,
                UserName = comment.UserName
            }, productNum);
        }

        /// <summary>
        /// 获取商品批发价
        /// </summary>
        /// <param name="pid">商品ID</param>
        /// <param name="buyNum">数量</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetChangeNum")]
        public  object GetChangeNum(long pid, int buyNum)
        {
            var _price = 0m;
            var product = ProductManagerApplication.GetProduct(pid);
            if (product.IsOpenLadder)
            {
                _price = ProductManagerApplication.GetProductLadderPrice(pid, buyNum);
                var shop = ShopApplication.GetShop(product.ShopId);
                var discount = 1m;
                if (CurrentUser != null && shop.IsSelf)
                    discount = CurrentUser.MemberDiscount;

                if (shop.IsSelf)
                    _price = _price * discount;
            }

            return Json(_price.ToString("F2"));
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
        }

        internal List<Entities.CouponInfo> GetCouponList(long shopId)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting(shopId).Where(n => n.PlatForm == PlatformType.Wap).Select(item => item.CouponID);

            //取设置的优惠券
            var coupons = service.GetCouponList(shopId)
                        .Where(item => couponSetList.Contains(item.Id)).ToList();
            return coupons;//商铺可用优惠券，排除过期和已领完的
        }

        private dynamic GetShopCouponList(long shopId)
        {
            var shop = ShopApplication.GetShopBasicInfo(shopId);
            if (shop != null && (shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze || shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired))
                return null;


            var coupons = GetCouponList(shopId);
            if (coupons != null)
            {
                //VShopInfo vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shopId);
                //if (vshop == null)
                //{
                //    return null;
                //}
                var userCoupon = coupons.ToList().Where(p => p.ReceiveType == Entities.CouponInfo.CouponReceiveType.ShopIndex).Select(a => new
                {
                    CouponId = a.Id,
                    CouponName = a.CouponName,
                    Price = a.Price,
                    SendCount = a.Num,
                    UserLimitCount = a.PerMax,
                    OrderUseLimit = a.OrderAmount,
                    StartTime = a.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ClosingTime = a.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    CanUseProducts = "",
                    ObtainWay = a.ReceiveType,
                    NeedPoint = a.NeedIntegral,
                    UseWithGroup = false,
                    UseWithPanicBuying = false,
                    UseWithFireGroup = false,
                    LimitText = a.CouponName,
                    CanUseProduct = a.UseArea == 1 ? "部分商品可用" : "全店通用",
                    StartTimeText = a.StartTime.ToString("yyyy.MM.dd"),
                    ClosingTimeText = a.EndTime.ToString("yyyy.MM.dd"),
                    EndTime = a.EndTime,
                    Receive = Receive(a.ShopId, a.Id),
                    Remark = a.Remark,
                    UseArea = a.UseArea
                });
                var data = userCoupon.Where(p => p.Receive != 2 && p.Receive != 4).OrderByDescending(d => d.EndTime);//优惠券已经过期、优惠券已领完，则不显示在店铺优惠券列表中
                return data;
            }
            else
                return null;
        }

        private int Receive(long vshopId, long couponId)
        {
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var couponInfo = couponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效
            if (CurrentUser != null)
            {
                CouponRecordQuery crQuery = new CouponRecordQuery();
                crQuery.CouponId = couponId;
                crQuery.UserId = CurrentUser.Id;
                QueryPageModel<Entities.CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
                if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax) return 3;//达到个人领取最大张数
                crQuery = new CouponRecordQuery()
                {
                    CouponId = couponId
                };
                pageModel = couponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponInfo.Num) return 4;//达到领取最大张数
                if (couponInfo.ReceiveType == Mall.Entities.CouponInfo.CouponReceiveType.IntegralExchange)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                    if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
                }
            }
            return 1;//可正常领取
        }


        /// <summary>
        /// 获取门店商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="partnerid"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetShopProductDetail")]
        public object GetShopProductDetail(long id, long shopBranchId)
        {
            CheckOpenStore();
            //var _iProductService = ServiceApplication.Create<IProductService>();
            //var _iShopService = ServiceApplication.Create<IShopService>();
            //var _iTypeService = ServiceApplication.Create<ITypeService>();

            var product = ProductManagerApplication.GetProduct(id);
            if (product == null || product.IsDeleted)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "该商品已被删除或者转移");
            }
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranch == null)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "shopBranchId");
            }
            if (shopBranch.Status == ShopBranchStatus.Freeze)
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

            //product = ProductManagerApplication.GetProduct(id);

            var cashDepositModel = CashDepositsApplication.GetCashDepositsObligation(product.Id);//提供服务（消费者保障、七天无理由、及时发货）
            model.CashDepositsServer = cashDepositModel;
            #region 根据运费模板获取发货地址
            //var freightTemplateService = ServiceApplication.Create<IFreightTemplateService>();
            FreightTemplateInfo template = new FreightTemplateInfo();
            if (product.ProductType == 0)
            {
                template = FreightTemplateApplication.GetFreightTemplate(product.FreightTemplateId);
            }
            string productAddress = string.Empty;
            if (template != null)
            {
                var fullName = RegionApplication.GetFullName(template.SourceAddress);
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
            shop = ShopApplication.GetShop(product.ShopId);
            var vshopinfo = VshopApplication.GetVShopByShopId(shop.Id);
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

            Entities.TypeInfo typeInfo = TypeApplication.GetType(product.TypeId);
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
            shop = ShopApplication.GetShop(product.ShopId);
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
            if (VshopApplication.GetVShopByShopId(shop.Id) == null)
                model.Shop.VShopId = -1;
            else
                model.Shop.VShopId = VshopApplication.GetVShopByShopId(shop.Id).Id;

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
                isFavorite = ProductManagerApplication.IsFavorite(product.Id, CurrentUser.Id);
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

            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(id, shop.Id);

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
            return Json(new
            {
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
            });
        }


        /// <summary>
        /// 将商品关联版式组合商品描述
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private string GetProductDescription(Entities.ProductDescriptionInfo productDescription)
        {
            if (productDescription == null)
            {
                throw new Mall.Core.MallException("错误的商品信息");
            }
            string descriptionPrefix = "", descriptiondSuffix = "";//顶部底部版式
            string description = productDescription.ShowMobileDescription.Replace("src=\"/Storage/Shop", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/Shop"));//商品描述
            var product = ProductManagerApplication.GetProduct(productDescription.ProductId);
            var iprodestempser = ServiceApplication.Create<IProductDescriptionTemplateService>();
            if (productDescription.DescriptionPrefixId != 0)
            {
                var desc = iprodestempser.GetTemplate(productDescription.DescriptionPrefixId, product.ShopId);
                descriptionPrefix = desc == null ? "" : desc.MobileContent.Replace("src=\"/Storage/Shop", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/Shop"));
            }

            if (productDescription.DescriptiondSuffixId != 0)
            {
                var desc = iprodestempser.GetTemplate(productDescription.DescriptiondSuffixId, product.ShopId);
                descriptiondSuffix = desc == null ? "" : desc.MobileContent.Replace("src=\"/Storage/Shop", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/Shop"));
            }

            return string.Format("{0}{1}{2}", descriptionPrefix, description, descriptiondSuffix);
        }

        private string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var oldname = Path.GetFileName(image);
            string ImageDir = string.Empty;

            //转移图片
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = oldname;
            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var de = image.Substring(image.LastIndexOf("/temp/"));
                Core.MallIO.CopyFile(de, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else if (image.Contains("/Storage"))
            {
                return image.Substring(image.LastIndexOf("/Storage"));
            }
            return image;
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
                freightStr = "免运费";
            }
            else
            {
                decimal freight = ServiceApplication.Create<IProductService>().GetFreight(new List<long>() { productId }, new List<int>() { 1 }, addressId, true);
                freightStr = freight <= 0 ? "免运费" : string.Format("运费 {0}元", freight.ToString("f2"));
            }
            return freightStr;
        }

        /// <summary>
        /// 获取追加评论
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        //public object GetAppendComment(long orderId)
        //{
        //    CheckUserLogin();
        //    var model = CommentApplication.GetProductEvaluationByOrderIdNew(orderId, CurrentUser.Id);

        //    if (model.Count() > 0 && model.FirstOrDefault().AppendTime.HasValue)
        //        return Json(ErrorResult<dynamic>("追加评论时，获取数据异常", new int[0]));
        //    else
        //    {
        //        var listResult = model.Select(item => new
        //        {
        //            Id = item.Id,
        //            CommentId = item.CommentId,
        //            ProductId = item.ProductId,
        //            ProductName = item.ProductName,
        //            //ThumbnailsUrl = item.ThumbnailsUrl,
        //            ThumbnailsUrl = Core.MallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_220), //商城App追加评论时获取商品图片
        //            BuyTime = item.BuyTime,
        //            EvaluationStatus = item.EvaluationStatus,
        //            EvaluationContent = item.EvaluationContent,
        //            AppendContent = item.AppendContent,
        //            AppendTime = item.AppendTime,
        //            EvaluationTime = item.EvaluationTime,
        //            ReplyTime = item.ReplyTime,
        //            ReplyContent = item.ReplyContent,
        //            ReplyAppendTime = item.ReplyAppendTime,
        //            ReplyAppendContent = item.ReplyAppendContent,
        //            EvaluationRank = item.EvaluationRank,
        //            OrderId = item.OrderId,
        //            CommentImages = item.CommentImages.Select(r => new
        //            {
        //                CommentImage = r.CommentImage,
        //                CommentId = r.CommentId,
        //                CommentType = r.CommentType
        //            }).ToList(),
        //            Color = item.Color,
        //            Size = item.Size,
        //            Version = item.Version
        //        }).ToList();
        //        return Json(listResult);
        //    }
        //}
        /// <summary>
        /// 追加评价
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //public JsonResult<Result<int>> PostAppendComment(CommentAppendCommentModel value)
        //{
        //    CheckUserLogin();
        //    string productCommentsJSON = value.productCommentsJSON;
        //    //var commentService = ServiceProvider.Instance<ICommentService>.Create;
        //    var productComments = JsonConvert.DeserializeObject<List<AppendCommentModel>>(productCommentsJSON);

        //    foreach (var m in productComments)
        //    {
        //        m.UserId = CurrentUser.Id;
        //    }
        //    CommentApplication.Append(productComments);
        //    return JsonResult<int>();
        //}


        #region 分销 
        /// <summary>
        /// 获取商品的佣金信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetDistributionInfo")]
        public object GetDistributionInfo(long id)
        {
            dynamic model = new System.Dynamic.ExpandoObject();
            //ProductGetDistributionInfoModel model = new ProductGetDistributionInfoModel { };
            var user = CurrentUser;
            //model.ShareUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/m-wap/product/Detail/" + id.ToString();
            if (user != null && user.Id > 0 && SiteSettingApplication.SiteSettings.DistributionIsEnable)
            {
                if (SiteSettingApplication.SiteSettings.DistributionIsProductShowTips)
                {
                    model.IsShowBrokerage = true;
                }
                var prom = DistributionApplication.GetDistributor(user.Id);
                if (prom != null && prom.DistributionStatus == (int)DistributorStatus.Audited)
                {
                    model.IsShowBrokerage = true;
                    //model.ShareUrl += "?" + DISTRIBUTION_SPREAD_ID_PARAMETER_NAME + "=" + user.Id.ToString();
                }
            }

            var probroker = DistributionApplication.GetProduct(id);
            if (probroker != null && probroker.ProductStatus == DistributionProductStatus.Normal)
            {
                model.Brokerage = probroker.MaxBrokerage;
                model.SaleCount = probroker.SaleCount;
            }
            else
            {
                model.IsShowBrokerage = false;
            }

            //model.WeiXinShareArgs = Application.WXApiApplication.GetWeiXinShareArgs(Request.RequestUri.AbsoluteUri);
            return Json(model);
        }
        #endregion

        /// <summary>
        /// 获取该商品所在商家下距离用户最近的门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetStroreInfo")]
        public object GetStroreInfo(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0) return Json(ErrorResult<dynamic>("请传入合法商家ID"));
            if (!(fromLatLng.Split(',').Length == 2)) return Json(ErrorResult<dynamic>("您当前定位信息异常"));

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
            var shopBranch = shopbranchs.Where(p => (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).FirstOrDefault();
            return Json(new
            {
                StoreInfo = shopBranch,
                total = total
            });
        }


        /// <summary>
        /// 新增或取消商品收藏
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
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


    }
}
