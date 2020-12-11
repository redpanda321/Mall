using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class BranchProductController : BaseMobileTemplatesController
    {
        private IShopService _iShopService;
        private IVShopService _iVShopService;
        private IProductService _iProductService;
        private ICashDepositsService _iCashDepositsService;
        private IFreightTemplateService _iFreightTemplateService;
        private IRegionService _iRegionService;
        private IMessageService _iMessageService;
        private ITypeService _iTypeService;
        private IShopBranchService _iShopBranchService;
        private const string SMSPLUGIN = "Mall.Plugin.Message.SMS";
        public BranchProductController(IShopService iShopService, IVShopService iVShopService, IProductService iProductService,
            ICashDepositsService iCashDepositsService, IFreightTemplateService iFreightTemplateService, IRegionService iRegionService
            , ITypeService iTypeService, IMessageService iMessageService, IShopBranchService iShopBranchService
            )
        {
            _iShopService = iShopService;
            _iVShopService = iVShopService;
            _iProductService = iProductService;
            _iCashDepositsService = iCashDepositsService;
            _iFreightTemplateService = iFreightTemplateService;
            _iRegionService = iRegionService;
            _iTypeService = iTypeService;
            _iMessageService = iMessageService;
            _iShopBranchService = iShopBranchService;
        }

        public JsonResult GetNeedRefreshProductInfo(long id = 0, long shopId = 0, long bid = 0)
        {
            var productservice = _iProductService;
            var productId = id;
            if (productId == 0)
                return ErrorResult("参数异常");
            var productInfo = productservice.GetNeedRefreshProductInfo(productId);
            if (productInfo == null)
            {
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
            }
            var shopBranchInfo = _iShopBranchService.GetShopBranchById(bid);
            if (shopBranchInfo == null)
            {
                throw new MallException("很抱歉，您查看的门店不存在。");
            }

            var branchskuList = ShopBranchApplication.GetSkus(shopBranchInfo.ShopId, bid);
            if (branchskuList == null || branchskuList.Count <= 0)
            {
                throw new MallException("很抱歉，您查看的门店没有该商品，可能已下架。");
            }
            var shopInfo = ShopApplication.GetShop(productInfo.ShopId);


            decimal discount = 1M;
            if (CurrentUser != null && shopInfo.IsSelf)
            {
                discount = CurrentUser.MemberDiscount;
            }

            long vShopId;
            var vshopinfo = _iVShopService.GetVShopByShopId(productInfo.ShopId);
            if (vshopinfo == null)
                vShopId = -1;
            else
                vShopId = vshopinfo.Id;
            var skus = productservice.GetSKUs(productId);

            var strPrice = ProductWebApplication.GetProductPriceStr(productInfo, skus, discount);//最小价或区间价文本
            
            if (shopInfo.IsSelf)
            {
                strPrice = string.IsNullOrWhiteSpace(strPrice) ? (productInfo.MinSalePrice * discount).ToString("f2") : strPrice;
            }
            else
            {
                strPrice = string.IsNullOrWhiteSpace(strPrice) ? productInfo.MinSalePrice.ToString("f2") : strPrice;

            }
            bool isFavorite;
            bool isFavoriteShop = false;
            if (CurrentUser == null)
            {
                isFavorite = false;
            }
            else
            {
                isFavorite = _iProductService.IsFavorite(productId, CurrentUser.Id);
                var favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                isFavoriteShop = favoriteShopIds.Contains(shopId);
            }
            //购物车数量
            #region 门店购物车
            int cartcount = 0;
            long userId = 0;

            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
            }
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            var shopcartinfo = branchCartHelper.GetCart(userId, bid);
            var shopcartItemList = shopcartinfo.Items.Where(x => x.ProductId == productId);

            if (shopcartItemList != null)
            {
                foreach (var cartitem in shopcartItemList)
                {
                    var branchskuInfo = branchskuList.FirstOrDefault(x => x.SkuId == cartitem.SkuId);
                    if (branchskuInfo != null && branchskuInfo.Status == ShopBranchSkuStatus.Normal && branchskuInfo.Stock >= cartitem.Quantity)
                    {
                        cartcount += cartitem.Quantity;
                    }
                }
            }
            #endregion

            //var commentCount = CommentApplication.GetProductCommentCount(productId);
            var comment = CommentApplication.GetProductCommentStatistic(productId: productId,
                        shopBranchId: shopBranchInfo.Id);

            var dtNow = DateTime.Now;
            var productSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;
            var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(productId);
            var startDate = DateTime.Parse(dtNow.ToString("yyyy-MM-01 00:00:00"));//当月的
            var endDate = dtNow;
            var salecount = ShopBranchApplication.GetProductSaleCount(bid, productId, startDate, dtNow);
            var sbskus = ShopBranchApplication.GetSkusByProductId(bid, id);
            return SuccessResult<dynamic>(data: new
            {
                salecount = salecount,
                measureunit = productInfo.MeasureUnit,
                price = strPrice,
                auditStatus = (branchskuList.Sum(x => (UInt32)x.Status)) > 0 ? 1 : 0,
                isFavorite = isFavorite,
                isFavoriteShop = isFavoriteShop,
                freightTemplateId = productInfo.FreightTemplateId,
                vShopId = vShopId,
                stock = sbskus.Sum(a => a.Stock),
                cartCount = cartcount,
                allComment = comment.AllComment,
                productSaleCountOnOff = productSaleCountOnOff,
                saleStatus = (int)productInfo.SaleStatus,
                productType = productInfo.ProductType,
                isoverdue = virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value,
                saleCounts = salecount + Mall.Core.Helper.TypeHelper.ObjectToInt(productInfo.VirtualSaleCounts)
            });
        }

        /// <summary>
        /// 商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partnerid">合作者</param>
        /// <param name="nojumpfg">不跳转拼团</param>
        /// <returns></returns>
        public ActionResult Detail(long id, long shopBranchId, long partnerid = 0, int nojumpfg = 0)
        {
            var product = ProductManagerApplication.GetProduct(id);
            if (product == null)
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");

            var branch = _iShopBranchService.GetShopBranchById(shopBranchId);
            if (branch == null || branch.Status == ShopBranchStatus.Freeze)
                throw new MallException("很抱歉，您查看的门店不存在，可能已关闭。");

            if (!_iShopBranchService.CheckProductIsExist(shopBranchId, id))
                throw new MallException("很抱歉，该门店不存在您查看的商品，可能已转移。");

            var branchskuList = ShopBranchApplication.GetSkus(branch.ShopId, shopBranchId);
            if (branchskuList == null || branchskuList.Count <= 0)
            {
                throw new MallException("很抱歉，您查看的门店没有该商品，可能已下架。");
            }
            bool branchskustates = false;
            foreach (var bsku in branchskuList)
            {
                if (bsku.Status == ShopBranchSkuStatus.Normal)
                {
                    branchskustates = true;
                    break;
                }
            }
            if (!branchskustates)
            {
                throw new MallException("很抱歉，您查看的门店商品已下架。");
            }

            #region 拼团活动跳转
            //if (nojumpfg != 1)
            //{
            //    var fgactobj = FightGroupApplication.GetActiveByProductId(gid);
            //    if (fgactobj != null)
            //    {
            //        if (fgactobj.ActiveStatus == FightGroupActiveStatus.Ongoing)  //只跳转进行中的活动
            //        {
            //            return RedirectToAction("Detail", "FightGroup", new { id = fgactobj.Id });
            //        }
            //    }
            //}
            #endregion


            #region 限时购预热
            //var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();
            //var FlashSale = iLimitService.IsFlashSaleDoesNotStarted(gid);
            //var FlashSaleConfig = iLimitService.GetConfig();

            //if (FlashSale != null)
            //{
            //    TimeSpan flashSaleTime = DateTime.Parse(FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
            //    TimeSpan preheatTime = new TimeSpan(FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
            //    if (preheatTime >= flashSaleTime)  //预热大于开始
            //    {
            //        if (!FlashSaleConfig.IsNormalPurchase)
            //            return RedirectToAction("Detail", "LimitTimeBuy", new { Id = FlashSale.Id });
            //    }
            //}
            #endregion
            ProductManagerApplication.GetWAPBranchHtml(id, shopBranchId);
            string urlHtml = "/Storage/Products/Statics/" + id + "-" + shopBranchId + "-wap-branch.html";
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            //统计门店访问人数
            StatisticApplication.StatisticShopBranchVisitUserCount(branch.ShopId, branch.Id);
            return File(urlHtml, "text/html");
        }

        public JsonResult GetWeiXinShareArgs()
        {
            var _weiXinShareArgs = Application.WXApiApplication.GetWeiXinShareArgs(this.HttpContext.Request.GetDisplayUrl());
            return Json(_weiXinShareArgs);
        }


        /// <summary>
        /// 商品详情(用户生成html)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partnerid">合作者</param>
        /// <param name="nojumpfg">不跳转拼团</param>
        /// <returns></returns>
        public ActionResult Details(long id = 0, long partnerid = 0, int nojumpfg = 0, string shopBranchId = "")
        {
            var shopService = _iShopService;
            var customerService = ServiceApplication.Create<ICustomerService>();
            string price = "";

            ProductDetailModel detailModel = new ProductDetailModel();
            ProductDetailModelForWeb model = new ProductDetailModelForWeb()
            {
                Product = new Entities.ProductInfo(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };

            Entities.ProductInfo product = null;
            Entities.ShopInfo shop = null;
            long gid = id;

            #region 商品Id不合法
            if (gid == 0)
            {
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
                //跳转到出错页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
            }
            product = _iProductService.GetProduct(gid);

            if (product == null)
            {
                ////跳转到404页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
            }

            if (product.IsDeleted)
            {
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
                ////跳转到404页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
            }

            #endregion

            long branchId = 0;
            #region 门店Id不合法
            long.TryParse(shopBranchId, out branchId);
            if (branchId == 0)
            {
                throw new MallException("很抱歉，您查看的门店不存在。");
            }
            var shopBranch = _iShopBranchService.GetShopBranchById(branchId);
            if (shopBranch == null)
            {
                throw new MallException("很抱歉，您查看的门店不存在。");
            }
            if (shopBranch.Status == ShopBranchStatus.Freeze)
            {
                throw new MallException("很抱歉，您查看的门店已冻结。");
            }
            if (!_iShopBranchService.CheckProductIsExist(branchId, gid))
            {
                throw new MallException("很抱歉，该门店不存在您查看的商品，可能已转移。");
            }
            ViewBag.bid = branchId;
            #endregion

            #region 初始化商品和店铺
            //TODO:DZY[150729] 显示移动端描述
            var desc = ProductManagerApplication.GetProductDescription(product.Id);
            model.ProductDescription = desc.ShowMobileDescription;

            shop = _iShopService.GetShop(product.ShopId);
            var mark = Framework.ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = CommentApplication.GetProductAverageMark(gid);
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
            detailModel.ProductNum = _iProductService.GetShopOnsaleProducts(product.ShopId);
            bool isFavorite;
            detailModel.FavoriteShopCount = _iShopService.GetShopFavoritesCount(product.ShopId);
            if (CurrentUser == null)
            {
                isFavorite = false;
                detailModel.IsFavoriteShop = false;
            }
            else
            {
                isFavorite = _iProductService.IsFavorite(product.Id, CurrentUser.Id);
                var favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                detailModel.IsFavoriteShop = favoriteShopIds.Contains(product.ShopId);
            }
            detailModel.IsFavorite = isFavorite;
            #endregion

            #region 商品规格

            var typeservice = _iTypeService;
            Entities.TypeInfo typeInfo = typeservice.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            if (product != null)
            {
                colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
                sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
                versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
            }
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            if (skus.Count > 0)
            {
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
                                    Img = Core.MallIO.GetImagePath(sku.ShowPic)
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
                                    //Name = "选择规格",
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
                var min = skus.Where(s => s.Stock >= 0).Min(s => s.SalePrice);
                var max = skus.Where(s => s.Stock >= 0).Max(s => s.SalePrice);
                if (min == 0 && max == 0)
                {
                    price = product.MinSalePrice.ToString("f2");
                }
                else if (max > min)
                {
                    price = string.Format("{0}-{1}", min.ToString("f2"), max.ToString("f2"));
                }
                else
                {
                    price = string.Format("{0}", min.ToString("f2"));
                }

            }
            ViewBag.Price = string.IsNullOrWhiteSpace(price) ? product.MinSalePrice.ToString("f2") : price;
            #endregion

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(product.ShopId);

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
            if (productAndDescription != null && productAndDescriptionPeer != null && !shop.IsSelf)
            {
                detailModel.ProductAndDescription = productAndDescription.CommentValue;
                detailModel.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                detailModel.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                detailModel.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                detailModel.ProductAndDescription = defaultValue;
                detailModel.ProductAndDescriptionPeer = defaultValue;
                detailModel.ProductAndDescriptionMin = defaultValue;
                detailModel.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null && !shop.IsSelf)
            {
                detailModel.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                detailModel.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                detailModel.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                detailModel.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                detailModel.SellerServiceAttitude = defaultValue;
                detailModel.SellerServiceAttitudePeer = defaultValue;
                detailModel.SellerServiceAttitudeMax = defaultValue;
                detailModel.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null && !shop.IsSelf)
            {
                detailModel.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                detailModel.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                detailModel.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                detailModel.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                detailModel.SellerDeliverySpeed = defaultValue;
                detailModel.SellerDeliverySpeedPeer = defaultValue;
                detailModel.SellerDeliverySpeedMax = defaultValue;
                detailModel.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            #region 限时购预热
            var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();
            bool isPreheat = false;
            model.FlashSale = iLimitService.IsFlashSaleDoesNotStarted(gid);
            model.FlashSaleConfig = iLimitService.GetConfig();

            if (model.FlashSale != null)
            {
                TimeSpan flashSaleTime = DateTime.Parse(model.FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                TimeSpan preheatTime = new TimeSpan(model.FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                if (preheatTime >= flashSaleTime)  //预热大于开始
                {
                    isPreheat = true;
                    if (!model.FlashSaleConfig.IsNormalPurchase)
                        return RedirectToAction("Detail", "LimitTimeBuy", new { Id = model.FlashSale.Id });
                }
            }
            ViewBag.IsPreheat = isPreheat;
            #endregion

            var consultations = ServiceApplication.Create<IConsultationService>().GetConsultations(gid);
            model.Product = product;
            model.Favorites = FavoriteApplication.GetFavoriteCountByProduct(product.Id);

            var comments = CommentApplication.GetCommentsByProduct(product.Id);
            detailModel.CommentCount = comments.Count;

            var total = comments.Count;
            var niceTotal = comments.Count(item => item.ReviewMark >= 4);

            detailModel.NicePercent = (int)((niceTotal / (double)total) * 100);
            detailModel.Consultations = consultations.Count();
            long vShopId;
            var vshopinfo = _iVShopService.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
                vShopId = -1;
            else
                vShopId = vshopinfo.Id;
            detailModel.VShopId = vShopId;
            model.Shop.VShopId = vShopId;

            model.VShopLog = _iVShopService.GetVShopLog(model.Shop.VShopId);
            if (string.IsNullOrWhiteSpace(model.VShopLog))
            {
                //throw new Mall.Core.MallException("店铺未开通微店功能");
                model.VShopLog = SiteSettings.WXLogo;
            }

            var customerServices = CustomerServiceApplication.GetMobileCustomerServiceAndMQ(product.ShopId);
            ViewBag.CustomerServices = customerServices;

            ViewBag.DetailModel = detailModel;
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            //统计门店访问人数
            StatisticApplication.StatisticShopBranchVisitUserCount(shopBranch.ShopId, shopBranch.Id);
            var dtNow = DateTime.Now;
            model.Product.SaleCounts = ShopBranchApplication.GetProductSaleCount(branchId, product.Id, dtNow.AddDays(-30).Date, dtNow);
            model.Product.SaleCounts = model.Product.SaleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(product.VirtualSaleCounts);
            ViewBag.ProductSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;
            return View(model);
        }

        public ActionResult VDetails(long id = 0, long partnerid = 0, int nojumpfg = 0, string shopBranchId = "")
        {
            var shopService = _iShopService;
            var customerService = ServiceApplication.Create<ICustomerService>();
            string price = "";

            ProductDetailModel detailModel = new ProductDetailModel();
            ProductDetailModelForWeb model = new ProductDetailModelForWeb()
            {
                Product = new Entities.ProductInfo(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU(),
                VirtualProductInfo = new Entities.VirtualProductInfo()
            };

            Entities.ProductInfo product = null;
            Entities.ShopInfo shop = null;
            long gid = id;

            #region 商品Id不合法          
            if (gid == 0)
            {
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
                //跳转到出错页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
            }
            product = _iProductService.GetProduct(gid);

            if (product == null)
            {
                ////跳转到404页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
            }

            if (product.IsDeleted)
            {
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
                ////跳转到404页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
            }

            #endregion

            long branchId = 0;
            #region 门店Id不合法
            long.TryParse(shopBranchId, out branchId);
            if (branchId == 0)
            {
                throw new MallException("很抱歉，您查看的门店不存在。");
            }
            var shopBranch = _iShopBranchService.GetShopBranchById(branchId);
            if (shopBranch == null)
            {
                throw new MallException("很抱歉，您查看的门店不存在。");
            }
            if (shopBranch.Status == ShopBranchStatus.Freeze)
            {
                throw new MallException("很抱歉，您查看的门店已冻结。");
            }
            if (!_iShopBranchService.CheckProductIsExist(branchId, gid))
            {
                throw new MallException("很抱歉，该门店不存在您查看的商品，可能已转移。");
            }
            ViewBag.bid = branchId;
            #endregion

            #region 初始化商品和店铺
            //TODO:DZY[150729] 显示移动端描述
            var desc = ProductManagerApplication.GetProductDescription(product.Id);
            model.ProductDescription = desc.ShowMobileDescription;

            shop = _iShopService.GetShop(product.ShopId);
            var mark = Framework.ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;

            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = CommentApplication.GetProductAverageMark(gid);
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
            detailModel.ProductNum = _iProductService.GetShopOnsaleProducts(product.ShopId);
            bool isFavorite;
            detailModel.FavoriteShopCount = _iShopService.GetShopFavoritesCount(product.ShopId);
            decimal discount = 1M;
            if (CurrentUser == null)
            {
                isFavorite = false;
                detailModel.IsFavoriteShop = false;
            }
            else
            {
                isFavorite = _iProductService.IsFavorite(product.Id, CurrentUser.Id);
                var favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                detailModel.IsFavoriteShop = favoriteShopIds.Contains(product.ShopId);
                discount = CurrentUser.MemberDiscount;
            }
            detailModel.IsFavorite = isFavorite;
            #endregion

            #region 商品规格

            var typeservice = _iTypeService;
            Entities.TypeInfo typeInfo = typeservice.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            if (product != null)
            {
                colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
                sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
                versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
            }
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            if (skus.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in skus)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
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
                                    Img = Core.MallIO.GetImagePath(sku.ShowPic)
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1)
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

                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = skus.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择规格",
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

                var min = skus.Where(s => s.Stock >= 0).Min(s => s.SalePrice);
                var max = skus.Where(s => s.Stock >= 0).Max(s => s.SalePrice);
                if (min == 0 && max == 0)
                {
                    price = product.MinSalePrice.ToString("f2");
                }
                else if (max > min)
                {
                    price = string.Format("{0}-{1}", min.ToString("f2"), max.ToString("f2"));
                }
                else
                {
                    price = string.Format("{0}", min.ToString("f2"));
                }

            }
            ViewBag.Price = string.IsNullOrWhiteSpace(price) ? product.MinSalePrice.ToString("f2") : price;
            #endregion

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(product.ShopId);

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
            if (productAndDescription != null && productAndDescriptionPeer != null && !shop.IsSelf)
            {
                detailModel.ProductAndDescription = productAndDescription.CommentValue;
                detailModel.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                detailModel.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                detailModel.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                detailModel.ProductAndDescription = defaultValue;
                detailModel.ProductAndDescriptionPeer = defaultValue;
                detailModel.ProductAndDescriptionMin = defaultValue;
                detailModel.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null && !shop.IsSelf)
            {
                detailModel.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                detailModel.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                detailModel.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                detailModel.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                detailModel.SellerServiceAttitude = defaultValue;
                detailModel.SellerServiceAttitudePeer = defaultValue;
                detailModel.SellerServiceAttitudeMax = defaultValue;
                detailModel.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null && !shop.IsSelf)
            {
                detailModel.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                detailModel.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                detailModel.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                detailModel.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                detailModel.SellerDeliverySpeed = defaultValue;
                detailModel.SellerDeliverySpeedPeer = defaultValue;
                detailModel.SellerDeliverySpeedMax = defaultValue;
                detailModel.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            #region 限时购预热
            var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();
            bool isPreheat = false;
            model.FlashSale = iLimitService.IsFlashSaleDoesNotStarted(gid);
            model.FlashSaleConfig = iLimitService.GetConfig();

            if (model.FlashSale != null)
            {
                TimeSpan flashSaleTime = DateTime.Parse(model.FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                TimeSpan preheatTime = new TimeSpan(model.FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                if (preheatTime >= flashSaleTime)  //预热大于开始
                {
                    isPreheat = true;
                    if (!model.FlashSaleConfig.IsNormalPurchase)
                        return RedirectToAction("Detail", "LimitTimeBuy", new { Id = model.FlashSale.Id });
                }
            }
            ViewBag.IsPreheat = isPreheat;
            #endregion

            model.Product = product;
            model.Favorites = FavoriteApplication.GetFavoriteCountByProduct(product.Id);


            var comments = CommentApplication.GetCommentsByProduct(product.Id, shopBranch.Id);
            detailModel.CommentCount = comments.Count;

            var consultations = ServiceApplication.Create<IConsultationService>().GetConsultations(gid);

            var total = comments.Count;
            var niceTotal = comments.Count(item => item.ReviewMark >= 4);
            detailModel.NicePercent = (int)((niceTotal / (double)total) * 100);
            detailModel.Consultations = consultations.Count();
            long vShopId;
            var vshopinfo = _iVShopService.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
                vShopId = -1;
            else
                vShopId = vshopinfo.Id;
            detailModel.VShopId = vShopId;
            model.Shop.VShopId = vShopId;

            model.VShopLog = _iVShopService.GetVShopLog(model.Shop.VShopId);
            model.Product.VideoPath = model.Product.VideoPath ?? string.Empty;
            if (string.IsNullOrWhiteSpace(model.VShopLog))
            {
                //throw new Mall.Core.MallException("店铺未开通微店功能");
                model.VShopLog = SiteSettings.WXLogo;
            }

            model.IsSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;

            var customerServices = CustomerServiceApplication.GetMobileCustomerServiceAndMQ(product.ShopId);
            ViewBag.CustomerServices = customerServices;

            model.VirtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(product.Id);
            ViewBag.DetailModel = detailModel;
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            //统计门店访问人数
            StatisticApplication.StatisticShopBranchVisitUserCount(shopBranch.ShopId, shopBranch.Id);
            return View(model);
        }
        public JsonResult GetProductColloCation(long productId)
        {
            int count = CollocationApplication.GetCollocationCount(productId);
            return SuccessResult<dynamic>(data: count);
        }
        public ActionResult ProductColloCation(long productId)
        {
            var data = CollocationApplication.GetDisplayCollocation(productId);
            return View(data);
        }

        #region 累加浏览次数、 加入历史记录
        [HttpPost]
        public JsonResult LogProduct(long pid)
        {
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(pid, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(pid);
            }

            //_iProductService.LogProductVisti(pid);
            return SuccessResult();
        }

        #endregion
        //TODO:【2015-09-22】取红包数据

        [HttpPost]
        public JsonResult AddFavoriteProduct(long pid)
        {
            int status = 0;
            _iProductService.AddFavorite(pid, CurrentUser.Id, out status);
            return SuccessResult("成功关注");
        }

        [HttpPost]
        public JsonResult DeleteFavoriteProduct(long pid)
        {
            _iProductService.DeleteFavorite(pid, CurrentUser.Id);
            return SuccessResult("已取消关注");
        }

        public JsonResult GetSKUInfo(long pId, long bid)
        {
            var product = _iProductService.GetProduct(pId);
            var shopBranchInfo = _iShopBranchService.GetShopBranchById(bid);
            var branchskuList = ShopBranchApplication.GetSkus(shopBranchInfo.ShopId, bid);
            long userId = 0;
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
            }
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            var shopcartinfo = branchCartHelper.GetCart(userId, bid);


            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            var skuArray = new List<ProductSKUModel>();
            //foreach (var sku in product.SKUInfo.Where(s => s.Stock > 0))
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
                        Stock = branchskuList.FirstOrDefault(x => x.SkuId == sku.Id).Stock,
                        cartCount = (shopcartinfo == null || shopcartinfo.Items.Count() == 0) ? 0 : shopcartinfo.Items.FirstOrDefault(x => x.SkuId == sku.Id) == null ? 0 : shopcartinfo.Items.FirstOrDefault(x => x.SkuId == sku.Id).Quantity,
                        minMath = 0
                    });
                }
            }
            //foreach (var item in skuArray)
            //{
            //    var str = item.SkuId.Split('_');
            //    item.SkuId = string.Format("{0};{1};{2}", str[1], str[2], str[3]);
            //}
            //SuccessResult<dynamic>(data: skuArray);
            return Json(new { skuArray = skuArray });
        }

        public JsonResult GetSKUCartCount(string skuId, long bid)
        {
            int cartcount = 0;
            long userId = 0;

            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
            }
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            var shopcartinfo = branchCartHelper.GetCart(userId, bid);
            var shopcartItemList = shopcartinfo.Items.Where(x => x.SkuId == skuId);
            if (shopcartItemList != null && shopcartItemList.Count() > 0)
            {
                cartcount = shopcartItemList.Sum(x => x.Quantity);
            }
            return SuccessResult<dynamic>(data: cartcount);
        }
        public JsonResult GetProductCartCount(long pid, long bid)
        {
            int cartcount = 0;
            long userId = 0;

            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
            }
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            var shopcartinfo = branchCartHelper.GetCart(userId, bid);
            var shopBranchInfo = _iShopBranchService.GetShopBranchById(bid);
            var branchskuList = ShopBranchApplication.GetSkus(shopBranchInfo.ShopId, bid);

            var shopcartItemList = shopcartinfo.Items.Where(x => x.ProductId == pid);
            if (shopcartItemList != null && shopcartItemList.Count() > 0)
            {
                foreach (var cartitem in shopcartItemList)
                {
                    var branchskuInfo = branchskuList.FirstOrDefault(x => x.SkuId == cartitem.SkuId);
                    if (branchskuInfo.Status == ShopBranchSkuStatus.Normal && branchskuInfo.Stock >= cartitem.Quantity)
                    {
                        cartcount += cartitem.Quantity;
                    }
                }
            }
            return SuccessResult<dynamic>(data: cartcount);
        }


        public JsonResult GetCommentByProduct(long pId)
        {
            var statistic = CommentApplication.GetProductCommentStatistic(pId);
            var comments = CommentApplication.GetProductComments(new ProductCommentQuery
            {
                ProductId = pId,
                PageNo = 1,
                PageSize = 3
            });

            var data = comments.Models.Select(item => new
            {
                UserName = item.UserName,
                ReviewContent = item.ReviewContent,
                ReviewDate = item.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ReplyContent = string.IsNullOrWhiteSpace(item.ReplyContent) ? "暂无回复" : item.ReplyContent,
                ReplyDate = item.ReplyDate.HasValue ? item.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                ReviewMark = item.ReviewMark,
                BuyDate = ""
            });

            return SuccessResult<dynamic>(data: new
            {
                comments = data,
                goodComment = statistic.HighComment,
                badComment = statistic.LowComment,
                comment = statistic.MediumComment,
            });
        }

        public ActionResult ProductComment(long pId = 0, int commentType = 0, long shopBranchId = 0)
        {
            var statistic = CommentApplication.GetProductCommentStatistic(productId: pId, shopBranchId: shopBranchId);
            ViewBag.goodComment = statistic.HighComment;
            ViewBag.mediumComment = statistic.MediumComment;
            ViewBag.bedComment = statistic.LowComment;
            ViewBag.allComment = statistic.AllComment;
            ViewBag.hasAppend = statistic.AppendComment;
            ViewBag.hasImages = statistic.HasImageComment;
            ViewBag.shopBranchId = shopBranchId;
            return View();
        }

        public JsonResult GetProductComment(long pId, int pageNo, int commentType = 0, int pageSize = 10, long shopBranchId = 0)
        {
            var commentStatistic = CommentApplication.GetProductCommentStatistic(pId);
            var query = new ProductCommentQuery
            {
                ProductId = pId,
                PageNo = pageNo,
                PageSize = pageSize
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

            var productService = _iProductService;

            var comments = CommentApplication.GetComments(query).Models;
            var orderitems = OrderApplication.GetOrderItems(comments.Select(p => (long)p.SubOrderId));
            var orders = OrderApplication.GetOrders(orderitems.Select(p => p.OrderId));
            var commentImages = CommentApplication.GetProductCommentImagesByCommentIds(comments.Select(a => a.Id));

            var data = comments.Select(item =>
            {
                var orderitem = orderitems.FirstOrDefault(p => p.Id == item.SubOrderId);
                var order = orders.FirstOrDefault(p => p.Id == orderitem.OrderId);
                return new
                {
                    Sku = _iProductService.GetSkuString(orderitem.SkuId),
                    UserName = item.UserName,
                    ReviewContent = item.ReviewContent,
                    AppendContent = item.AppendContent,
                    AppendDate = item.AppendDate.HasValue ? item.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    ReplyAppendContent = item.ReplyAppendContent,
                    ReplyAppendDate = item.ReplyAppendDate.HasValue ? item.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    FinshDate = order.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Images = commentImages.Where(a => a.CommentId == item.Id && a.CommentType == 0).Select(a => new { CommentImage = Mall.Core.MallIO.GetImagePath(a.CommentImage) }).ToList(),
                    AppendImages = commentImages.Where(a => a.CommentId == item.Id && a.CommentType == 1).Select(a => new { CommentImage = Mall.Core.MallIO.GetImagePath(a.CommentImage) }).ToList(),
                    ReviewDate = item.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ReplyContent = string.IsNullOrWhiteSpace(item.ReplyContent) ? "暂无回复" : item.ReplyContent,
                    ReplyDate = item.ReplyDate.HasValue ? item.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                    ReviewMark = item.ReviewMark,
                    BuyDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")
                };
            });
            return SuccessResult<dynamic>(data: data);
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
        /// 获取店铺优惠券
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ActionResult GetShopCoupons(long shopId)
        {
            var coupons = GetCouponList(shopId);
            return null;
        }


        [UnAuthorize]
        [HttpPost]
        public ActionResult GetProductActives(long shopId, long productId, long branchId)
        {
            ProductActives actives = new ProductActives();
            //var freeFreight = ServiceApplication.Create<IShopService>().GetShopFreeFreight(shopId);
            var shopBranchs = ShopBranchApplication.GetShopBranchById(branchId);
            actives.freeFreight = shopBranchs.FreeMailFee;
            var bonus = ServiceApplication.Create<IShopBonusService>().GetByShopId(shopId);
            if (bonus != null)
            {
                ProductBonusLableModel model = new ProductBonusLableModel();
                model.Count = bonus.Count;
                model.GrantPrice = bonus.GrantPrice;
                model.RandomAmountStart = bonus.RandomAmountStart;
                model.RandomAmountEnd = bonus.RandomAmountEnd;
                actives.ProductBonus = model;
            }
            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(productId, shopId, branchId);
            if (fullDiscount != null)
            {
                actives.FullDiscount = fullDiscount;
            }
            return SuccessResult<dynamic>(data: actives);
        }

        public ActionResult HistoryVisite(long userId)
        {
            var product = BrowseHistrory.GetBrowsingProducts(10, userId);
            return View(product);
        }

        [HttpPost]
        public ActionResult GetStock(string skuId, long bid)
        {
            var sku = _iProductService.GetSku(skuId);
            var product = ProductManagerApplication.GetProduct(sku.ProductId);
            var shopBranchs = ShopBranchApplication.GetShopBranchById(bid);
            if (shopBranchs == null)
                throw new MallException("很抱歉，您查看的门店不存在。");
            //判断门店库存和状态
            var branchSkuInfo = ShopBranchApplication.GetSkusByIds(bid, new List<string> { skuId });
            if (branchSkuInfo == null)
                throw new MallException("很抱歉，您查看的商品不存在，或已下架。");
            var stock = branchSkuInfo.FirstOrDefault().Stock;
            //获取门店购物车中商品购买数量
            int cartcount = 0;
            long userId = 0;
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
            }
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            var shopcartinfo = branchCartHelper.GetCart(userId, bid);
            var shopcartItemList = shopcartinfo.Items.Where(x => x.SkuId == skuId);
            if (shopcartItemList != null && shopcartItemList.Count() > 0)
            {
                cartcount = shopcartItemList.Sum(x => x.Quantity);
            }
            var status = 0;
            if (product.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale && branchSkuInfo.FirstOrDefault().Status == ShopBranchSkuStatus.Normal)
            {
                status = 1;
            }
            return SuccessResult<dynamic>(data: new
            {
                Stock = stock,
                cartCount = cartcount,
                Status = status
            });
        }

        ////
        //private string GetSelected(long productId, long skuId, string skus,int skuType)
        //{
        //    string result = "";
        //    if (skus=="")
        //    {
        //        return result;
        //    }
        //    var groupSKU = skus.Split(',');
        //    for (int i = 0; i < groupSKU.Count(); i++)
        //    {
        //        var specs = groupSKU[i].Split('_');
        //        if (specs.Count() > 0)
        //        {
        //            long colorId = 0, sizeId = 0, versionId = 0,product=0;
        //            if (long.TryParse(specs[0], out product)) { }
        //            if (long.TryParse(specs[1], out colorId)) { }
        //            if (long.TryParse(specs[2], out sizeId)) { }
        //            if (long.TryParse(specs[3], out versionId)) { }
        //            if (colorId != 0 && skuType==1)
        //            {
        //                if (product == productId && colorId == skuId)
        //                {
        //                    result = "selected";
        //                }
        //            }
        //            else if (sizeId!= 0 && skuType==2)
        //            {
        //                if (product == productId && sizeId == skuId)
        //                {
        //                    result = "selected";
        //                }
        //            }
        //            else if (versionId != 0 && skuType == 3)
        //            {
        //                if (product == productId && versionId == skuId)
        //                {
        //                    result = "selected";
        //                }
        //            }
        //        }

        //    }
        //    return result;
        //}

        public ActionResult ShowSkuInfo(long id, long bid)
        {

            Entities.ProductInfo product = _iProductService.GetProduct(id);
            if (product == null)
            {
                throw new Mall.Core.MallException("产品编号错误");
            }

            if (product.IsDeleted)
            {
                throw new Mall.Core.MallException("产品编号错误");
            }
            var shopBranchInfo = _iShopBranchService.GetShopBranchById(bid);
            if (shopBranchInfo == null)
            {
                throw new Mall.Core.MallException("门店不存在");
            }
            var branchskuList = ShopBranchApplication.GetSkus(shopBranchInfo.ShopId, bid);
            if (branchskuList == null || branchskuList.Count <= 0)
            {
                throw new Mall.Core.MallException("门店商品不存在");
            }
            bool branchskustates = false;
            foreach (var bsku in branchskuList)
            {
                if (bsku.Status == ShopBranchSkuStatus.Normal)
                    branchskustates = true;
            }
            if (!branchskustates)
            {
                throw new Mall.Core.MallException("门店商品已下架");
            }

            ProductShowSkuInfoModel model = new ProductShowSkuInfoModel();
            model.MinSalePrice = product.MinSalePrice;
            model.ProductImagePath = product.RelativePath;
            model.MeasureUnit = product.MeasureUnit;
            model.MaxBuyCount = product.MaxBuyCount;

            #region 商品规格

            var typeservice = _iTypeService;
            Entities.TypeInfo typeInfo = typeservice.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            if (product != null)
            {
                colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
                sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
                versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
            }
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            if (skus.Count > 0 && branchskuList.Count > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in skus.OrderBy(s => s.AutoId).ToList())
                {
                    var brachone = branchskuList.Where(p => p.SkuId == sku.Id).FirstOrDefault();
                    if (brachone != null)
                    {
                        model.StockAll += brachone.Stock;//总库存
                    }

                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0 && branchskuList.Count(x => x.SkuId == sku.Id) > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }
                        if (colorId != 0 && !string.IsNullOrEmpty(sku.Color))
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = branchskuList.Where(p => skus.Where(s => s.Color.Equals(sku.Color)).Select(a => a.Id).Contains(p.SkuId)).Sum(p => p.Stock);
                                    //skus.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = Mall.Core.MallIO.GetImagePath(sku.ShowPic)
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1 && branchskuList.Count(x => x.SkuId == sku.Id) > 0)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0 && !string.IsNullOrEmpty(sku.Size))
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                //var ss = skus.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                var ss = branchskuList.Where(p => skus.Where(s => s.Size.Equals(sku.Size)).Select(a => a.Id).Contains(p.SkuId)).Sum(p => p.Stock);
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

                    if (specs.Count() > 2 && branchskuList.Count(x => x.SkuId == sku.Id) > 0)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0 && !string.IsNullOrEmpty(sku.Version))
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                //var v = skus.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                var v = branchskuList.Where(p => skus.Where(s => s.Version.Equals(sku.Version)).Select(a => a.Id).Contains(p.SkuId)).Sum(p => p.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择规格",
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
            model.VirtualProductItem = ProductManagerApplication.GetVirtualProductItemInfoByProductId(id);
            return View(model);
        }

        [HttpPost]
        public JsonResult GetHasSku(long id)
        {
            var result = ProductManagerApplication.HasSKU(id);
            return SuccessResult<dynamic>(data: result);
        }
        /// <summary>
        /// 获取商品描述
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetProductDescription(long pid)
        {
            var ProductDescription = ServiceApplication.Create<IProductService>().GetProductDescription(pid);
            if (ProductDescription == null)
            {
                throw new Mall.Core.MallException("错误的商品编号");
            }
            string DescriptionPrefix = "", DescriptiondSuffix = "";
            var product = ProductManagerApplication.GetProduct(pid);
            var iprodestempser = ServiceApplication.Create<IProductDescriptionTemplateService>();
            if (ProductDescription.DescriptionPrefixId != 0)
            {
                var desc = iprodestempser.GetTemplate(ProductDescription.DescriptionPrefixId, product.ShopId);
                DescriptionPrefix = desc == null ? "" : desc.MobileContent;
            }

            if (ProductDescription.DescriptiondSuffixId != 0)
            {
                var desc = iprodestempser.GetTemplate(ProductDescription.DescriptiondSuffixId, product.ShopId);
                DescriptiondSuffix = desc == null ? "" : desc.MobileContent;
            }
            return SuccessResult<dynamic>(data: new
            {
                prodes = ProductDescription.ShowMobileDescription,
                prodesPrefix = DescriptionPrefix,
                prodesSuffix = DescriptiondSuffix
            });

        }

        public JsonResult GetHotProduct(long productId, int categoryId)
        {
            var relationProduct = ProductManagerApplication.GetRelationProductByProductId(productId);
            List<DTO.Product.Product> products;
            if (relationProduct == null || ProductManagerApplication.GetProductsByIds(relationProduct.RelationProductIds).Count == 0)
                products = ProductManagerApplication.GetHotSaleProductByCategoryId(categoryId, 10);
            else
                products = ProductManagerApplication.GetProductsByIds(relationProduct.RelationProductIds);

            foreach (var item in products)
            {
                item.ImagePath = item.GetImage(ImageSize.Size_220);
                item.SaleCounts = item.SaleCounts + Core.Helper.TypeHelper.ObjectToInt(item.VirtualSaleCounts.Value);
            }
            return SuccessResult<dynamic>(data: products, camelCase: true);
        }

        //public JsonResult CanBuy(long productId, int count)
        //{
        //    if (CurrentUser == null)
        //        return SuccessResult<dynamic>(data: new { Result = true }, camelCase: true);

        //    if (CurrentUser.Disabled)
        //    {
        //        return ErrorResult<dynamic>(data: new
        //        {
        //            Result = false,
        //            ResultType = -10,
        //            Message = "用户被冻结"
        //        }, camelCase: true);
        //    }
        //    int reason;
        //    var msg = new Dictionary<int, string>() { { 0, "" }, { 1, "商品已下架" }, { 2, "很抱歉，您查看的商品不存在，可能被转移。" }, { 3, "超出商品最大限购数" }, { 9, "商品无货" } };
        //    var result = ProductManagerApplication.BranchCanBuy(CurrentUser.Id, productId, count, out reason);

        //    return Json<dynamic>(success: result, data: new
        //    {
        //        Result = result,
        //        ResultType = reason,
        //        Message = msg[reason]
        //    }, camelCase: true);
        //}
        public JsonResult CanBuy(long productId, int count, string skuId, long shopBranchId)
        {
            if (CurrentUser == null)
                return SuccessResult<dynamic>(data: new { Result = true }, camelCase: true);

            if (CurrentUser.Disabled)
            {
                return ErrorResult<dynamic>(data: new
                {
                    Result = false,
                    ResultType = -10,
                    Message = "用户被冻结"
                }, camelCase: true);
            }
            int reason;
            var msg = new Dictionary<int, string>() { { 0, "" }, { 1, "商品已下架" }, { 2, "很抱歉，您查看的商品不存在，可能被转移。" }, { 3, "超出商品最大限购数" }, { 4, "错误的门店" }, { 9, "商品无货" } };
            var result = ProductManagerApplication.BranchCanBuy(CurrentUser.Id, productId, count, skuId, shopBranchId, out reason);
            return Json<dynamic>(success: result, data: new
            {
                Result = result,
                ResultType = reason,
                Message = msg[reason]
            }, camelCase: true);
        }

        #region 页面调用块
        /// <summary>
        /// 显示发货地
        /// </summary>
        /// <param name="ftid">运费模板编号</param>
        /// <returns></returns>    
        public ActionResult ShowDepotAddress(long ftid)
        {
            var template = _iFreightTemplateService.GetFreightTemplate(ftid);
            string productAddress = string.Empty;
            if (template != null)
            {
                var fullName = _iRegionService.GetFullName(template.SourceAddress);
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

            template.DepotAddress = productAddress;
            return View(template);
        }
        /// <summary>
        /// 显示服务承诺
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        
        public ActionResult ShowServicePromise(long id, long shopId)
        {
            var model = _iCashDepositsService.GetCashDepositsObligation(id);
            int regionId = 0;
            if (CurrentUser != null)
            {
                var defaultShippingAddress = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                if (defaultShippingAddress != null)
                    regionId = defaultShippingAddress.RegionId;
            }

            if (regionId == 0)
            {
                string curip = Mall.Core.Helper.WebHelper.GetIP();
                regionId = (int)RegionApplication.GetRegionByIPInTaobao(curip);
            }

            var region = RegionApplication.GetRegion(regionId, Region.RegionLevel.City);

            if (region != null)
            {
                var shopBranchQuery = new ShopBranchQuery();
                shopBranchQuery.ShopId = shopId;
                shopBranchQuery.Status = ShopBranchStatus.Normal;
                shopBranchQuery.ProductIds = new[] { id };
                shopBranchQuery.AddressPath = region.GetIdPath();
                shopBranchQuery.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                model.CanSelfTake = ShopBranchApplication.Exists(shopBranchQuery);
            }

            ViewBag.ProductId = id;
            ViewBag.ShopId = shopId;
            return View(model);
        }
        /// <summary>
        /// 显示商品详情
        /// <para>显示模板，由js完成内容加载</para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
  
        public ActionResult ShowProductDescription(long id)
        {
            return View(id);
        }


        /// <summary>
        /// 显示商品评论前两条
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <param name="top">取多少条</param>
        /// <returns></returns>      
        public ActionResult ProductCommentShow(long id,long bid=0, int top = 1, bool isshowtit = false)
        {
            ProductCommentShowModel model = new ProductCommentShowModel();
            model.ProductId = id;
            var product = _iProductService.GetProduct(id);
            model.CommentList = new List<ProductDetailCommentModel>();
            model.IsShowColumnTitle = isshowtit;
            model.IsShowCommentList = true;
            if (top < 1)
            {
                model.IsShowCommentList = false;
            }

            if (product == null)
            {
                //跳转到404页面
                throw new Core.MallException("商品不存在");
            }
            if (product.IsDeleted)
            {
                //跳转到404页面
                throw new Core.MallException("商品不存在");
            }
            var comments = CommentApplication.GetCommentsByProduct(product.Id, bid);
            model.CommentCount = comments.Count;
            if (comments.Count > 0 && top > 0)
            {
                var productCommentInfoList = comments.OrderByDescending(a => a.ReviewDate).Take(top);

                var orderItems = OrderApplication.GetOrderItems(productCommentInfoList.Select(c => c.SubOrderId).ToList()).ToDictionary(item => item.Id, item => item);
                var orders = OrderApplication.GetOrders(orderItems.Values.Select(a => a.OrderId).ToList()).ToDictionary(o => o.Id, o => o);
                var commentImages = CommentApplication.GetProductCommentImagesByCommentIds(productCommentInfoList.Select(a => a.Id));
                model.CommentList = productCommentInfoList.Select(c => new ProductDetailCommentModel
                {
                    Sku = _iProductService.GetSkuString(orderItems[c.SubOrderId].SkuId),
                    UserName = c.UserName.Substring(0, 1) + "***" + c.UserName.Substring(c.UserName.Length - 1, 1),
                    ReviewContent = c.ReviewContent,
                    AppendContent = c.AppendContent,
                    AppendDate = c.AppendDate,
                    ReplyAppendContent = c.ReplyAppendContent,
                    ReplyAppendDate = c.ReplyAppendDate,
                    FinshDate = orders[orderItems[c.SubOrderId].OrderId].FinishDate,
                    Images = commentImages.Where(a => a.CommentId == c.Id && a.CommentType == 0).Select(a => a.CommentImage).ToList(),
                    AppendImages = commentImages.Where(a => a.CommentId == c.Id && a.CommentType == 1).Select(a => a.CommentImage).ToList(),
                    ReviewDate = c.ReviewDate,
                    ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                    ReplyDate = c.ReplyDate,
                    ReviewMark = c.ReviewMark,
                    BuyDate = orders[orderItems[c.SubOrderId].OrderId].OrderDate

                }).ToList();
                foreach (var citem in model.CommentList)
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
            }

            return View(model);
        }

        #endregion

        /// <summary>
        /// 异步请求是否门店授权
        /// </summary>
        /// <returns></returns>
        public JsonResult GetIsOpenStore()
        {
            var result = new
            {
                IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore
            };
            return SuccessResult(data: result.IsOpenStore, camelCase: true);
        }

        /// <summary>
        /// 获取该商品所在商家下距离用户最近的门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        public JsonResult GetStroreInfo(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0)
                return ErrorResult("请传入合法商家ID", camelCase: true);
            if (!(fromLatLng.Split(',').Length == 2))
                return ErrorResult("您当前定位信息异常", true);

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                FromLatLng = fromLatLng,
                Status = ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal,
                ProductIds = new long[] { productId }
            };
            var shopbranchs = ShopBranchApplication.GetShopBranchsAll(query).Models.Where(p => (p.Latitude > 0 && p.Longitude > 0)).ToList();
            int total = shopbranchs.Count; //商家下有该产品的门店总数(余勇确认,郭赞修改,原来是取商家下所有门店总数)

            var shopBranch = shopbranchs.OrderBy(p => p.Distance).Take(1).FirstOrDefault<Mall.DTO.ShopBranch>();  //商家下有该产品的且距离用户最近的门店
            var result = new
            {
                StoreInfo = shopBranch,
                Total = total
            };
            return SuccessResult<dynamic>(data: result, camelCase: true);
        }
        /// <summary>
        /// 是否可允许自提门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        public JsonResult GetIsSelfDelivery(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0)
                return ErrorResult<dynamic>(msg: "请传入合法商家ID", data: 0, camelCase: true);
            if (!(fromLatLng.Split(',').Length == 2))
                return ErrorResult<dynamic>(msg: "请传入合法经纬度", data: 0, camelCase: true);

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                Status = ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal
            };
            string address = "", province = "", city = "", district = "", street = "";
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (string.IsNullOrWhiteSpace(city))
                return ErrorResult<dynamic>(msg: "无法获取当前城市", data: 0, camelCase: true);

            Region cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
            if (cityInfo == null)
                return ErrorResult<dynamic>(msg: "获取当前城市异常", data: 0, camelCase: true);
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
            return SuccessResult<dynamic>(msg: "", data: shopBranch.Where(p => p.Enabled).Count() > 0 ? 1 : 0, camelCase: true);//至少有一个能够自提的门店，才可显示图标
        }
    }
}