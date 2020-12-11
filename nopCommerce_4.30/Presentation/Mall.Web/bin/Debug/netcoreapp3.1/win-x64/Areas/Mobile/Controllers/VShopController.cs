using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class VShopController : BaseMobileTemplatesController
    {
        private string wxlogo = "/images/defaultwxlogo.png";
        private Entities.WXCardLogInfo.CouponTypeEnum ThisCouponType = Entities.WXCardLogInfo.CouponTypeEnum.Coupon;
        private IWXCardService _iWXCardService;
        private IVShopService _iVShopService;
        private IShopService _iShopService;
        private ITemplateSettingsService _iTemplateSettingsService;
        private IProductService _iProductService;
        private ICustomerService _iCustomerService;
        private IShopBonusService _iShopBonusService;

        public VShopController(IWXCardService iWXCardService,
            IVShopService iVShopService,
             IShopService iShopService,
             ITemplateSettingsService iTemplateSettingsService,
            IProductService iProductService,
            ICustomerService iCustomerService
            , IShopBonusService iShopBonusService
            )
        {
            this._iWXCardService = iWXCardService;
            _iVShopService = iVShopService;
            _iShopService = iShopService;
            _iTemplateSettingsService = iTemplateSettingsService;
            _iProductService = iProductService;
            _iCustomerService = iCustomerService;
            _iShopBonusService = iShopBonusService;
        }

        [HttpGet]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost]
        public JsonResult List(int page, int pageSize)
        {
            int total;
            var vshops = _iVShopService.GetVShops(page, pageSize, out total, Entities.VShopInfo.VShopStates.Normal, true).ToArray();
            long[] favoriteShopIds = new long[] { };
            if (CurrentUser != null)
                favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();
            var model = vshops.Select(item =>
            {
                int productCount = _iShopService.GetShopProductCount(item.ShopId);
                int FavoritesCount = _iShopService.GetShopFavoritesCount(item.ShopId);
                return new
                {
                    id = item.Id,
                    //image = MallIO.GetImagePath(item.StrBackgroundImage),
                    image = item.Logo,
                    tags = item.Tags,
                    name = item.Name,
                    shopId = item.ShopId,
                    favorite = favoriteShopIds.Contains(item.ShopId),
                    productCount = productCount,
                    FavoritesCount = FavoritesCount
                };
            });
            return SuccessResult<dynamic>(data: model);
        }

        [ActionName("Index")]
        public ActionResult Main()
        {
            var service = _iVShopService;
            var topShop = service.GetTopShop();
            bool isFavorite = false;
            if (topShop != null)
            {
                var query = new ProductQuery()
                {
                    PageSize = 3,
                    PageNo = 1,
                    ShopId = topShop.ShopId,
                    AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.Audited },
                    SaleStatus = Entities.ProductInfo.ProductSaleStatus.OnSale
                };
                var products = ProductManagerApplication.GetProducts(query).Models;
                var topShopProducts = products.Select(item => new ProductItem()
                {
                    Id = item.Id,
                    ImageUrl = item.GetImage(ImageSize.Size_350),
                    MarketPrice = item.MarketPrice,
                    Name = item.ProductName,
                    SalePrice = item.MinSalePrice
                });
                ViewBag.TopShopProducts = topShopProducts;//主推店铺的商品
                if (CurrentUser != null)
                {
                    var favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                    isFavorite = favoriteShopIds.Contains(topShop.ShopId);
                }
                int productCount = _iShopService.GetShopProductCount(topShop.ShopId);
                int FavoritesCount = _iShopService.GetShopFavoritesCount(topShop.ShopId);
                ViewBag.ProductCount = productCount;
                ViewBag.FavoritesCount = FavoritesCount;
                if (!string.IsNullOrEmpty(topShop.Tags))
                {
                    var array = topShop.Tags.Split(new string[] { ";", "；" }, StringSplitOptions.RemoveEmptyEntries);
                    string wxTag = string.Empty;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i < 2)
                            wxTag += " " + array[i] + " ·";
                    }
                    wxTag = wxTag.TrimStart().Trim('·');
                    ViewBag.Tags = wxTag;
                    ViewBag.TagsArray = array;
                }
            }


            ViewBag.IsFavorite = isFavorite;
            return View(topShop);
        }

        [HttpPost]
        public JsonResult GetHotShops(int page, int pageSize)
        {
            int total;
            var hotShops = _iVShopService.GetHotShops(page, pageSize, out total).ToArray();//获取热门微店
            var homeProductService = ServiceApplication.Create<IMobileHomeProductsService>();
            long[] favoriteShopIds = new long[] { };
            if (CurrentUser != null)
                favoriteShopIds = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();
            var model = hotShops.Select(item =>
                {
                    //TODO:FG 循环内查询数据(小数据)
                    int productCount = _iShopService.GetShopProductCount(item.ShopId);
                    int FavoritesCount = _iShopService.GetShopFavoritesCount(item.ShopId);
                    var queryModel = new ProductQuery()
                    {
                        PageSize = 3,
                        PageNo = 1,
                        ShopId = item.ShopId,
                        OrderKey = 4//微店推荐3个商品按商家商品序号排
                    };
                    queryModel.AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.Audited };
                    queryModel.SaleStatus = Entities.ProductInfo.ProductSaleStatus.OnSale;
                    var products = ProductManagerApplication.GetProducts(queryModel).Models;
                    var tags = string.Empty;
                    if (!string.IsNullOrEmpty(item.Tags))
                    {
                        var array = item.Tags.Split(new string[] { ";", "；" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (i < 2)
                                tags += " " + array[i] + " ·";
                        }
                        tags = tags.TrimStart().Trim('·');
                    }


                    return new
                    {
                        id = item.Id,
                        name = item.Name,
                        logo = MallIO.GetImagePath(item.StrLogo),
                        products = products.Select(t => new
                        {
                            id = t.Id,
                            name = t.ProductName,
                            image = t.GetImage(ImageSize.Size_220),
                            salePrice = t.MinSalePrice,
                        }),
                        favorite = favoriteShopIds.Contains(item.ShopId),
                        shopId = item.ShopId,
                        productCount = productCount,
                        FavoritesCount = FavoritesCount,
                        Tags = tags,
                        sourcetags= item.Tags
                    };
                }
            );

            return SuccessResult<dynamic>(data: model);
        }

        public ActionResult Detail(long id, int? couponid, int? shop, bool sv = false, int ispv = 0, string tn = "")
        {
            var vshop = _iVShopService.GetVShop(id);
            var s = ShopApplication.GetShop(vshop.ShopId);
            if (null != s && s.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired)
                throw new MallException("此店铺已过期");
            if (null != s && s.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze)
                throw new MallException("此店铺已冻结");
            if (vshop.State == Entities.VShopInfo.VShopStates.Close)
            {
                throw new MallException("商家暂未开通微店");
            }
            if (!vshop.IsOpen)
            {
                throw new MallException("此微店已关闭");
            }
            string crrentTemplateName = "t1";
            _iShopService.CheckInitTemplate(vshop.ShopId);
            var curr = _iTemplateSettingsService.GetCurrentTemplate(vshop.ShopId);
            if (null != curr)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }
            if (ispv == 1)
            {
                if (!string.IsNullOrWhiteSpace(tn))
                {
                    crrentTemplateName = tn;
                }
            }
            ViewBag.VshopId = id;
            ViewBag.ShopId = vshop.ShopId;
            ViewBag.Title = vshop.HomePageTitle;

            ViewBag.CustomerServices = CustomerServiceApplication.GetMobileCustomerServiceAndMQ(vshop.ShopId);//客服

            ViewBag.ShowAside = 1;
            //统计店铺访问人数
            VshopApplication.LogVisit(vshop.Id);//保持和APP端一致
            StatisticApplication.StatisticShopVisitUserCount(vshop.ShopId);
            VTemplateHelper.DownloadTemplate(crrentTemplateName, VTemplateClientTypes.SellerWapIndex, vshop.ShopId);
            return View(string.Format("~/Areas/SellerAdmin/Templates/vshop/{0}/{1}/Skin-HomePage.cshtml", vshop.ShopId, crrentTemplateName));
        }
        //TODO:DZY[171122]前台调用被注释，暂不修改
        public JsonResult LoadProductsFromCache(long shopid, long page)
        {
            var html = TemplateSettingsApplication.GetShopGoodTagFromCache(shopid, page);
            return Json(new { htmlTag = html });
        }
        /// <summary>
        /// 未开通微店提醒
        /// </summary>
        /// <returns></returns>
        public ActionResult NoOpenVShopTips()
        {
            return View();
        }

        #region 优惠券
        private IEnumerable<Entities.CouponInfo> GetCouponList(long shopid)
        {
            var service = ServiceApplication.Create<ICouponService>();
            var result = service.GetCouponList(shopid);
            var couponSetList = _iVShopService.GetVShopCouponSetting(shopid).Select(item => item.CouponID);
            if (result.Count() > 0 && couponSetList.Count() > 0)
            {
                var couponList = result.Where(item => couponSetList.Contains(item.Id));//取设置的优惠券
                return couponList;
            }
            return null;
        }

        public ActionResult CouponInfo(long id, int? accept)
        {
            VshopCouponInfoModel result = new VshopCouponInfoModel();
            var couponService = ServiceApplication.Create<ICouponService>();
            var couponInfo = couponService.GetCouponInfo(id) ?? new Entities.CouponInfo() { };
            if (couponInfo.EndTime < DateTime.Now)
            {
                //已经失效
                result.CouponStatus = Entities.CouponInfo.CouponReceiveStatus.HasExpired;
            }

            if (CurrentUser != null)
            {
                CouponRecordQuery crQuery = new CouponRecordQuery();
                crQuery.CouponId = id;
                crQuery.UserId = CurrentUser.Id;
                var pageModel = couponService.GetCouponRecordList(crQuery);
                if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
                {
                    //达到个人领取最大张数
                    result.CouponStatus = Entities.CouponInfo.CouponReceiveStatus.HasLimitOver;
                }
                crQuery = new CouponRecordQuery()
                {
                    CouponId = id,
                    PageNo = 1,
                    PageSize = 9999
                };
                pageModel = couponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponInfo.Num)
                {
                    //达到领取最大张数
                    result.CouponStatus = Entities.CouponInfo.CouponReceiveStatus.HasReceiveOver;
                }
                if (couponInfo.ReceiveType == Entities.CouponInfo.CouponReceiveType.IntegralExchange)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                    if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                    {
                        result.CouponStatus = Entities.CouponInfo.CouponReceiveStatus.IntegralLess;
                    }
                }
                var isFav = _iShopService.IsFavoriteShop(CurrentUser.Id, couponInfo.ShopId);
                if (isFav)
                {
                    result.IsFavoriteShop = true;
                }
            }
            result.CouponId = id;
            if (accept.HasValue)
                result.AcceptId = accept.Value;

            var vshop = _iVShopService.GetVShopByShopId(couponInfo.ShopId);
            var settings = SiteSettingApplication.SiteSettings;
            string curwxlogo = wxlogo;
            if (vshop != null)
            {
                result.VShopid = vshop.Id;
                if (!string.IsNullOrWhiteSpace(vshop.WXLogo))
                {
                    curwxlogo = vshop.WXLogo;
                }
                if (string.IsNullOrWhiteSpace(wxlogo))
                {
                    if (!string.IsNullOrWhiteSpace(settings.WXLogo))
                    {
                        curwxlogo = settings.WXLogo;
                    }
                }
            }
            ViewBag.ShopLogo = curwxlogo;
            var vshopSetting = _iVShopService.GetVShopSetting(couponInfo.ShopId);
            if (vshopSetting != null)
            {
                result.FollowUrl = vshopSetting.FollowUrl;
            }
            result.ShopId = couponInfo.ShopId;
            result.CouponData = couponInfo;
            //补充ViewBag
            ViewBag.ShopId = result.ShopId;
            ViewBag.FollowUrl = result.FollowUrl;
            ViewBag.FavText = result.IsFavoriteShop ? "已收藏" : "收藏店铺";
            ViewBag.VShopid = result.VShopid;
            return View(result);
        }
        [HttpPost]
        public JsonResult AcceptCoupon(long vshopid, long couponid)
        {
            if (CurrentUser == null)
            {
                return ErrorResult("未登录.", 1, true);
            }
            var couponService = ServiceApplication.Create<ICouponService>();
            var couponInfo = couponService.GetCouponInfo(couponid);
            if (couponInfo.EndTime < DateTime.Now)
            {//已经失效
                return ErrorResult("优惠券已经过期.", 2, true);
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponid;
            crQuery.UserId = CurrentUser.Id;
            var pageModel = couponService.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
            {
                //达到个人领取最大张数
                return ErrorResult("达到个人领取最大张数，不能再领取.", 3, true);
            }
            crQuery = new CouponRecordQuery()
            {
                CouponId = couponid
            };
            pageModel = couponService.GetCouponRecordList(crQuery);
            if (pageModel.Total >= couponInfo.Num)
            {//达到领取最大张数
                return ErrorResult("此优惠券已经领完了.", 4, true);
            }
            if (couponInfo.ReceiveType == Entities.CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                {
                    //积分不足
                    return ErrorResult("积分不足 " + couponInfo.NeedIntegral.ToString(), 5, true);
                }
            }
            Entities.CouponRecordInfo couponRecordInfo = new Entities.CouponRecordInfo()
            {
                CouponId = couponid,
                UserId = CurrentUser.Id,
                UserName = CurrentUser.UserName,
                ShopId = couponInfo.ShopId
            };
            couponService.AddCouponRecord(couponRecordInfo);
            return SuccessResult("领取成功", data: new { crid = couponRecordInfo.Id }, code: 0);
        }
        public ActionResult GetCouponSuccess(long id)
        {
            VshopCouponInfoModel result = new VshopCouponInfoModel();
            var couponser = ServiceApplication.Create<ICouponService>();
            var couponRecordInfo = couponser.GetCouponRecordById(id);
            if (couponRecordInfo == null) throw new MallException("错误的优惠券编号");
            var couponInfo = couponser.GetCouponInfo(couponRecordInfo.ShopId, couponRecordInfo.CouponId);
            if (couponInfo == null) throw new MallException("错误的优惠券编号");
            result.CouponData = couponInfo;
            result.CouponId = couponInfo.Id;
            result.CouponRecordId = couponRecordInfo.Id;
            result.ShopId = couponInfo.ShopId;
            result.IsShowSyncWeiXin = false;

            if (CurrentUser != null)
            {
                var isFav = _iShopService.IsFavoriteShop(CurrentUser.Id, couponInfo.ShopId);
                if (isFav)
                {
                    result.IsFavoriteShop = true;
                }
            }
            result.CouponId = id;

            #region 同步微信前信息准备
            if (couponInfo.IsSyncWeiXin == 1 && this.PlatformType == PlatformType.WeiXin)
            {
                result.WXJSInfo = _iWXCardService.GetSyncWeiXin(couponInfo.Id, couponRecordInfo.Id, ThisCouponType, Request.GetDisplayUrl());
                if (result.WXJSInfo != null)
                {
                    result.IsShowSyncWeiXin = true;
                    //result.WXJSCardInfo = ser_wxcard.GetJSWeiXinCard(couponRecordInfo.CouponId, couponRecordInfo.Id, ThisCouponType);    //同步方式有重复留的Bug
                }
            }
            #endregion

            var settings = SiteSettingApplication.SiteSettings;
            string curwxlogo = wxlogo;
            var vshop = _iVShopService.GetVShopByShopId(couponInfo.ShopId);
            if (vshop != null)
            {
                result.VShopid = vshop.Id;
                if (!string.IsNullOrWhiteSpace(vshop.WXLogo))
                {
                    curwxlogo = vshop.WXLogo;
                }
                if (string.IsNullOrWhiteSpace(wxlogo))
                {
                    if (!string.IsNullOrWhiteSpace(settings.WXLogo))
                    {
                        curwxlogo = settings.WXLogo;
                    }
                }
            }
            ViewBag.ShopLogo = curwxlogo;
            //补充ViewBag
            ViewBag.ShopId = result.ShopId;
            ViewBag.FollowUrl = result.FollowUrl;
            ViewBag.FavText = result.IsFavoriteShop ? "已收藏" : "收藏店铺";
            ViewBag.VShopid = result.VShopid;
            return View(result);
        }
        [HttpPost]
        public JsonResult GetWXCardData(long id)
        {
            Entities.WXJSCardModel result = new Entities.WXJSCardModel();
            bool isdataok = true;
            var couponser = ServiceApplication.Create<ICouponService>();
            Entities.CouponRecordInfo couponRecordInfo = null;
            if (isdataok)
            {
                couponRecordInfo = couponser.GetCouponRecordById(id);
                if (couponRecordInfo == null)
                {
                    isdataok = false;
                }
            }
            Entities.CouponInfo couponInfo = null;
            if (isdataok)
            {
                couponInfo = couponser.GetCouponInfo(couponRecordInfo.ShopId, couponRecordInfo.CouponId);
                if (couponInfo == null)
                {
                    isdataok = false;
                }
            }
            #region 同步微信前信息准备
            if (isdataok)
            {
                if (couponInfo.IsSyncWeiXin == 1 && this.PlatformType == PlatformType.WeiXin)
                {
                    result = _iWXCardService.GetJSWeiXinCard(couponRecordInfo.CouponId, couponRecordInfo.Id, ThisCouponType);
                }
            }
            #endregion
            return SuccessResult<dynamic>(data: result);
        }
        #endregion

        public JsonResult AddFavorite(long shopId)
        {
            if (CurrentUser == null)
                return ErrorResult("请先登录.");
            _iShopService.AddFavoriteShop(CurrentUser.Id, shopId);
            return SuccessResult("成功关注该微店.");
        }

        public JsonResult DeleteFavorite(long shopId)
        {
            _iShopService.CancelConcernShops(shopId, CurrentUser.Id);
            return SuccessResult("成功取消关注该微店.");
        }

        public ActionResult Introduce(long id)
        {
            var vshop = _iVShopService.GetVShop(id);
            string qrCodeImagePath = string.Empty;
            long shopid = -1;
            if (vshop != null)
            {
                string vshopUrl = CurrentUrlHelper.CurrentUrlNoPort() + "/m-" + PlatformType.WeiXin.ToString() + "/vshop/detail/" + id;

                Image map;
                if (!string.IsNullOrWhiteSpace(vshop.StrLogo) && MallIO.ExistFile(vshop.StrLogo))
                    map = Core.Helper.QRCodeHelper.Create(vshopUrl, MallIO.GetImagePath(vshop.StrLogo));
                else
                    map = Core.Helper.QRCodeHelper.Create(vshopUrl);

                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                //  将图片内存流转成base64,图片以DataURI形式显示  
                string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
                ms.Dispose();
                qrCodeImagePath = strUrl;
                shopid = vshop.ShopId;
            }
            ViewBag.QRCode = qrCodeImagePath;
            bool isFavorite;
            if (CurrentUser == null)
                isFavorite = false;
            else
                isFavorite = _iShopService.IsFavoriteShop(CurrentUser.Id, shopid);
            ViewBag.IsFavorite = isFavorite;
            var mark = Framework.ShopServiceMark.GetShopComprehensiveMark(shopid);
            ViewBag.shopMark = mark.ComprehensiveMark.ToString();

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(shopid);

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
                ViewBag.ProductAndDescription = productAndDescription.CommentValue;
                ViewBag.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                ViewBag.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                ViewBag.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                ViewBag.ProductAndDescription = defaultValue;
                ViewBag.ProductAndDescriptionPeer = defaultValue;
                ViewBag.ProductAndDescriptionMin = defaultValue;
                ViewBag.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                ViewBag.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                ViewBag.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                ViewBag.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                ViewBag.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                ViewBag.SellerServiceAttitude = defaultValue;
                ViewBag.SellerServiceAttitudePeer = defaultValue;
                ViewBag.SellerServiceAttitudeMax = defaultValue;
                ViewBag.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                ViewBag.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                ViewBag.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                ViewBag.SellerDeliverySpeedMax = sellerDeliverySpeedMax.CommentValue;
                ViewBag.sellerDeliverySpeedMin = sellerDeliverySpeedMin.CommentValue;
            }
            else
            {
                ViewBag.SellerDeliverySpeed = defaultValue;
                ViewBag.SellerDeliverySpeedPeer = defaultValue;
                ViewBag.SellerDeliverySpeedMax = defaultValue;
                ViewBag.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion
            ViewBag.shop = ShopApplication.GetShop(shopid);
            return View(vshop);
        }

        [HttpPost]
        public JsonResult ProductList(long shopId, int pageNo, int pageSize)
        {
            var homeProduct = ServiceApplication.Create<IMobileHomeProductsService>().GetMobileHomeProducts(shopId, PlatformType.WeiXin, pageNo, pageSize);
            var products = ProductManagerApplication.GetProducts(homeProduct.Models.Select(p => p.ProductId));
            var result = products.Select(item => new
            {
                Id = item.Id,
                ImageUrl = item.GetImage(ImageSize.Size_350),
                Name = item.ProductName,
                MarketPrice = item.MarketPrice,
                SalePrice = item.MinSalePrice.ToString("F2")
            });
            return SuccessResult<dynamic>(data: result);
        }

        public ActionResult Search(string keywords = "", /* 搜索关键字 */
        string exp_keywords = "", /* 渐进搜索关键字 */
        long cid = 0,  /* 分类ID */
        long b_id = 0, /* 品牌ID */
        string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
        int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
        int orderType = 1, /* 排序方式（1：升序，2：降序） */
        int pageNo = 1, /*页码*/
        int pageSize = 6, /*每页显示数据量*/
        long vshopId = 0,//店铺ID
        long shopCid = 0//店铺分类 
        )
        {
            int total;
            long shopId = -1;
            if (vshopId > 0)
            {
                var vshop = _iVShopService.GetVShop(vshopId);
                if (vshop != null)
                    shopId = vshop.ShopId;
            }
            if (!string.IsNullOrWhiteSpace(keywords))
                keywords = keywords.Trim();

            SearchProductQuery model = new SearchProductQuery()
            {
                ShopId = shopId,
                BrandId = b_id,
                //CategoryId = cid,
                //Ex_Keyword = exp_keywords,
                Keyword = keywords,
                OrderKey = orderKey,
                ShopCategoryId = shopCid,
                OrderType = orderType == 1,
                //AttrIds = new System.Collections.Generic.List<string>(),
                PageNumber = pageNo,
                PageSize = pageSize,
            };

            var productsResult = ServiceApplication.Create<ISearchProductService>().SearchProduct(model);
            total = productsResult.Total;
            var products = productsResult.Data;


            //decimal discount = 1M;
            //long selfShopId = 0;
            //var selfshop = _iShopService.GetSelfShop();
            //if (selfshop != null) selfShopId = selfshop.Id;
            //if (CurrentUser != null) discount = CurrentUser.MemberDiscount;

            //var limit = LimitTimeApplication.GetLimitProducts();
            //var fight = FightGroupApplication.GetFightGroupPrice();
            //var commentService = ServiceApplication.Create<ICommentService>();
            var productsModel = products.Select(item =>
                new ProductItem()
                {
                    Id = item.ProductId,
                    ImageUrl = Core.MallIO.GetProductSizeImage(item.ImagePath, 1, (int)ImageSize.Size_350),
                    //SalePrice = (item.ShopId == selfshop.Id ? item.MinSalePrice * discount : item.MinSalePrice),
                    SalePrice = item.SalePrice,
                    Name = item.ProductName,
                    CommentsCount = item.Comments
                }
            );



            var bizCategories = ServiceApplication.Create<IShopCategoryService>().GetShopCategory(shopId);

            var shopCategories = GetSubCategories(bizCategories, 0, 0);

            ViewBag.ShopCategories = shopCategories;
            ViewBag.Total = total;
            ViewBag.Keywords = keywords;
            ViewBag.VShopId = vshopId;
            if (shopId > 0)
            {
                //统计店铺访问人数
                StatisticApplication.StatisticShopVisitUserCount(shopId);
            }
            ViewBag.ProductSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;
            return View(productsModel);
        }

        /// <summary>
        /// 商品价格
        /// </summary>
        /// <returns></returns>
        private decimal GetProductPrice(Entities.ProductInfo item, List<FlashSalePrice> limit, List<FightGroupPrice> fight, decimal discount, long selfShopId)
        {
            decimal price = item.MinSalePrice;//原价

            if (item.ShopId == selfShopId) price = price * discount;//自营店，会员价

            var isLimit = limit.Where(r => r.ProductId == item.Id).FirstOrDefault();
            var isFight = fight.Where(r => r.ProductId == item.Id).FirstOrDefault();

            if (isLimit != null) price = isLimit.MinPrice;//限时购价
            if (isFight != null) price = isFight.ActivePrice;//团购价

            return price;
        }

        /// <summary>
        ///  商品搜索页面
        /// </summary>
        /// <param name="keywords">搜索关键字</param>
        /// <param name="exp_keywords">渐进搜索关键字</param>
        /// <param name="cid">分类ID</param>
        /// <param name="b_id">品牌ID</param>
        /// <param name="a_id">属性ID, 表现形式：attrId_attrValueId</param>
        /// <param name="orderKey">序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间）</param>
        /// <param name="orderType">排序方式（1：升序，2：降序）</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页显示数据量</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Search(
            string keywords = "", /* 搜索关键字 */
            string exp_keywords = "", /* 渐进搜索关键字 */
            long cid = 0,  /* 分类ID */
            long b_id = 0, /* 品牌ID */
            string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            int orderKey = 1, /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            int orderType = 1, /* 排序方式（1：升序，2：降序） */
            int pageNo = 1, /*页码*/
            int pageSize = 6,/*每页显示数据量*/
           long vshopId = 0,//微店ID
           long shopCid = 0,
           string t = ""/*无意义参数，为了重载*/
            )
        {
            int total;
            long shopId = -1;
            if (vshopId > 0)
            {
                var vshop = _iVShopService.GetVShop(vshopId);
                if (vshop != null)
                    shopId = vshop.ShopId;
            }
            if (!string.IsNullOrWhiteSpace(keywords))
                keywords = keywords.Trim();
            SearchProductQuery model = new SearchProductQuery()
            {
                ShopId = shopId,
                BrandId = b_id,
                //CategoryId = cid,
                ShopCategoryId = shopCid,
                //Ex_Keyword = exp_keywords,
                Keyword = keywords,
                OrderKey = orderKey,
                OrderType = orderType == 1,
                //AttrIds = new System.Collections.Generic.List<string>(),
                PageNumber = pageNo,
                PageSize = pageSize
            };

            var productsResult = ServiceApplication.Create<ISearchProductService>().SearchProduct(model);
            total = productsResult.Total;
            var products = productsResult.Data;
            //var selfshop = _iShopService.GetSelfShop();
            //decimal discount = 1m;
            //if (CurrentUser != null)
            //{
            //    discount = CurrentUser.MemberDiscount;
            //}
            var resultModel = products.Select(item => new
            {
                id = item.ProductId,
                name = item.ProductName,
                price = item.SalePrice,
                commentsCount = item.Comments,
                img = Core.MallIO.GetProductSizeImage(item.ImagePath, 1, (int)ImageSize.Size_350)
            });
            return SuccessResult<dynamic>(data: resultModel);
        }

        public ActionResult Category(long vShopId)
        {
            var vshopInfo = _iVShopService.GetVShop(vShopId);
            var bizCategories = ServiceApplication.Create<IShopCategoryService>().GetShopCategory(vshopInfo.ShopId).Where(a => a.IsShow).ToList();
            var shopCategories = GetSubCategories(bizCategories, 0, 0);
            ViewBag.VShopId = vShopId;
            return View(shopCategories);
        }


        IEnumerable<CategoryModel> GetSubCategories(IEnumerable<Entities.ShopCategoryInfo> allCategoies, long categoryId, int depth)
        {
            var categories = allCategoies
                .Where(item => item.ParentCategoryId == categoryId && item.IsShow)
                .Select(item =>
                {
                    string image = string.Empty;
                    return new CategoryModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        SubCategories = GetSubCategories(allCategoies, item.Id, depth + 1),
                        Depth = 1
                    };
                });
            return categories;
        }
        [HttpPost]
        public JsonResult GetVShopIdByShopId(long shopId)
        {
            var vshop = _iVShopService.GetVShopByShopId(shopId);
            return Json(new Result { success = true, msg = vshop.Id.ToString() });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="couponid"></param>
        /// <param name="shop"></param>
        /// <param name="sv"></param>
        /// <param name="ispv"></param>
        /// <param name="tn"></param>
        /// <returns></returns>
        public ActionResult VShopHeader(long id)
        {
            var vshopService = ServiceApplication.Create<IVShopService>();
            var vshop = vshopService.GetVShop(id);
            if (vshop == null)
            {
                throw new MallException("错误的微店Id");
            }
            //轮播图
            var slideImgs = ServiceApplication.Create<ISlideAdsService>().GetSlidAds(vshop.ShopId, Entities.SlideAdInfo.SlideAdType.VShopHome).ToList();


            var homeProducts = ServiceApplication.Create<IMobileHomeProductsService>().GetMobileHomeProducts(vshop.ShopId, PlatformType.WeiXin, 1, 8);
            var productData = ProductManagerApplication.GetProducts(homeProducts.Models.Select(p => p.ProductId));
            var products = productData.Select(item => new ProductItem()
            {
                Id = item.Id,
                ImageUrl = item.GetImage(ImageSize.Size_350),
                Name = item.ProductName,
                MarketPrice = item.MarketPrice,
                SalePrice = item.MinSalePrice
            });
            var banner = ServiceApplication.Create<INavigationService>().GetSellerNavigations(vshop.ShopId, Core.PlatformType.WeiXin).ToList();

            ViewBag.SlideAds = slideImgs.ToArray().Select(item => new HomeSlideAdsModel() { ImageUrl = item.ImageUrl, Url = item.Url });

            ViewBag.Banner = banner;
            ViewBag.Products = products;
            if (CurrentUser == null)
                ViewBag.IsFavorite = false;
            else
                ViewBag.IsFavorite = ServiceApplication.Create<IShopService>().IsFavoriteShop(CurrentUser.Id, vshop.ShopId);
            //快速关注
            var vshopSetting = ServiceApplication.Create<IVShopService>().GetVShopSetting(vshop.ShopId);
            if (vshopSetting != null)
                ViewBag.FollowUrl = vshopSetting.FollowUrl;

            ViewBag.VshopId = id;
            ViewBag.ShopId = vshop.ShopId;
            return View("~/Areas/Mobile/Templates/Default/Views/Shared/_VShopHeader.cshtml", vshop);
        }
        /// <summary>
        /// 获取模板节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTemplateItem(string id, long shopid, string tn = "")
        {
            string result = "";
            if (string.IsNullOrWhiteSpace(tn))
            {
                tn = "t1";
                var curr = _iTemplateSettingsService.GetCurrentTemplate(shopid);
                if (null != curr)
                {
                    tn = curr.CurrentTemplateName;
                }
            }
            result = VTemplateHelper.GetTemplateItemById(id, tn, VTemplateClientTypes.SellerWapIndex, shopid);
            return result;
        }

        #region 页面调用块
        /// <summary>
        /// 显示营销信息
        /// <para>优惠券，满额免</para>
        /// </summary>
        /// <param name="id">店铺编号</param>
        /// <param name="showcoupon">是否显示优惠券</param>
        /// <param name="showfreefreight">是否显示满额免</param>
        /// <param name="showfullsend">是否显示满就送</param>
        /// <returns></returns>
     
        public ActionResult ShowPromotion(long id, bool showcoupon = true, bool showfreefreight = true, bool showfullsend = true)
        {
            VShopShowPromotionModel model = new VShopShowPromotionModel();
            model.ShopId = id;
            var shop = _iShopService.GetShop(id);
            if (shop == null)
            {
                throw new MallException("错误的店铺编号");
            }
            if (showcoupon)
            {
                model.CouponCount = ServiceApplication.Create<ICouponService>().GetTopCoupon(id, 10, PlatformType.Wap).Count();
            }

            if (showfreefreight)
            {
                model.FreeFreight = shop.FreeFreight;
            }
            model.BonusCount = 0;
            if (showfullsend)
            {
                var bonus = ServiceApplication.Create<IShopBonusService>().GetByShopId(id);
                if (bonus != null)
                {
                    model.BonusCount = bonus.Count;
                    model.BonusGrantPrice = bonus.GrantPrice;
                    model.BonusRandomAmountStart = bonus.RandomAmountStart;
                    model.BonusRandomAmountEnd = bonus.RandomAmountEnd;
                }
            }
            return View(model);
        }

        /// <summary>
        /// 移动端优惠券静态化
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetPromotions(long id)
        {
            VShopShowPromotionModel model = new VShopShowPromotionModel();
            model.ShopId = id;
            //model.CouponCount = ServiceApplication.Create<ICouponService>().GetTopCoupon(id, 10, PlatformType.Wap).Count();
            var shop = ShopApplication.GetShopBasicInfo(id);
            if (shop != null && (shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze || shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired))
                model.CouponCount = 0;
            else
                model.CouponCount = GetCouponCount(id);

            return SuccessResult<dynamic>(data: model);
        }

        /// <summary>
        /// 店铺评分
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        
        public ActionResult ShowShopScore(long id)
        {
            VShopShowShopScoreModel model = new VShopShowShopScoreModel();
            model.ShopId = id;
            var shop = _iShopService.GetShop(id);
            if (shop == null)
            {
                throw new MallException("错误的店铺信息");
            }

            model.ShopName = shop.ShopName;

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = ServiceApplication.Create<IShopService>().GetShopStatisticOrderComments(id);

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
                model.ProductAndDescription = productAndDescription.CommentValue;
                model.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                model.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                model.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                model.ProductAndDescription = defaultValue;
                model.ProductAndDescriptionPeer = defaultValue;
                model.ProductAndDescriptionMin = defaultValue;
                model.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                model.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                model.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                model.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                model.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                model.SellerServiceAttitude = defaultValue;
                model.SellerServiceAttitudePeer = defaultValue;
                model.SellerServiceAttitudeMax = defaultValue;
                model.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                model.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                model.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                model.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                model.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                model.SellerDeliverySpeed = defaultValue;
                model.SellerDeliverySpeedPeer = defaultValue;
                model.SellerDeliverySpeedMax = defaultValue;
                model.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            model.ProductNum = _iProductService.GetShopOnsaleProducts(id);
            model.IsFavoriteShop = false;
            model.FavoriteShopCount = _iShopService.GetShopFavoritesCount(id);
            if (CurrentUser != null)
            {
                model.IsFavoriteShop = _iShopService.GetFavoriteShopInfos(CurrentUser.Id).Any(d => d.ShopId == id);
            }

            long vShopId;
            var vshopinfo = _iVShopService.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
            {
                vShopId = -1;
            }
            else
            {
                vShopId = vshopinfo.Id;
                model.VShopLog = vshopinfo.WXLogo; // _iVShopService.GetVShopLog(vShopId);
            }
            model.VShopId = vShopId;
            if (string.IsNullOrWhiteSpace(model.VShopLog))
            {
                model.VShopLog = "/Areas/Mobile/Templates/Default/Images/noimage200.png";//没图片默认图片
            }
            if (!string.IsNullOrWhiteSpace(model.VShopLog))
            {
                model.VShopLog = Mall.Core.MallIO.GetImagePath(model.VShopLog);
            }

            return View(model);
        }
        #endregion
        #region 获取优惠券数
        internal int GetCouponCount(long shopId)
        {
            var service = ServiceApplication.Create<ICouponService>();
            //var result = service.GetCouponList(shopid);
            //var couponSetList = _iVShopService.GetVShopCouponSetting(shopid).Where(a => a.PlatForm == Core.PlatformType.Wap).Select(item => item.CouponID);
            //if (result.Count() > 0 && couponSetList.Count() > 0)
            //{
            //    var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id)).Select(p => new
            //    {
            //        Receive = Receive(p.Id)
            //    });
            //    return couponList.Where(p => p.Receive != 2 && p.Receive != 4).Count();//排除过期和已领完的
            //}
            //return 0;
            return service.GetUserCouponCount(shopId);
        }

        //private int Receive(long couponId)
        //{
        //    if (CurrentUser != null && CurrentUser.Id > 0)//未登录不可领取
        //    {
        //        var couponService = ServiceApplication.Create<ICouponService>();
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
        //            var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
        //            if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
        //        }

        //        return 1;//可正常领取
        //    }
        //    return 0;
        //} 
        #endregion
    }
}