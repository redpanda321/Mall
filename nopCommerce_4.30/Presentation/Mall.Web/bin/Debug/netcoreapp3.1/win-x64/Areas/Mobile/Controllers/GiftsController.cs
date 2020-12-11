using AutoMapper;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    /// <summary>
    /// 拼团
    /// </summary>
    public class GiftsController : BaseMobileTemplatesController
    {
        private IProductService _iProductService;
        private ITypeService _iTypeService;
        private IGiftService _iGiftService;
        private IGiftsOrderService _iGiftsOrderService;
        private ICouponService _iCouponService;
        private IVShopService _iVShopService;
        private IMemberGradeService _iMemberGradeService;

        public GiftsController(IProductService iProductService, ITypeService iTypeService
            , IGiftService iGiftService, ICouponService iCouponService, IVShopService iVShopService
            , IGiftsOrderService iGiftsOrderService, IMemberGradeService iMemberGradeService)
        {
            _iProductService = iProductService;
            _iTypeService = iTypeService;
            _iGiftService = iGiftService;
            _iCouponService = iCouponService;
            _iVShopService = iVShopService;
            _iGiftsOrderService = iGiftsOrderService;
            _iMemberGradeService = iMemberGradeService;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //轮播图
            GiftsIndexModel result = new GiftsIndexModel();
            var slidads = SlideApplication.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.AppGifts).ToList();
            foreach (var item in slidads)
            {
                item.ImageUrl = MallIO.GetRomoteImagePath(item.ImageUrl);
            }
            result.SlideAds = slidads;

            //大转盘刮刮卡
            var robj = _iGiftService.GetAdInfo(IntegralMallAdInfo.AdActivityType.Roulette, IntegralMallAdInfo.AdShowPlatform.APP);
            if (robj != null)
            {
                robj.LinkUrl = "/m-wap/BigWheel/index/" + robj.ActivityId;
                result.WeiActives.Add(robj);
            }
            var cobj = _iGiftService.GetAdInfo(IntegralMallAdInfo.AdActivityType.ScratchCard, IntegralMallAdInfo.AdShowPlatform.APP);
            if (cobj != null)
            {
                cobj.LinkUrl = "/m-wap/ScratchCard/index/" + cobj.ActivityId;
                result.WeiActives.Add(cobj);
            }

            //首页礼品
            GiftQuery query = new GiftQuery();
            query.skey = "";
            query.status = GiftInfo.GiftSalesStatus.Normal;
            query.PageSize = 4;
            query.PageNo = 1;
            QueryPageModel<GiftModel> gifts = _iGiftService.GetGifts(query);
            result.HomeGiftses = gifts.Models.ToList();
            result.HasMoreGifts = gifts.Total > 4;
            foreach (var item in result.HomeGiftses)
            {
                item.DefaultShowImage = MallIO.GetRomoteImagePath(item.GetImage(ImageSize.Size_350));
            }

            //积分优惠券
            var coupons = _iCouponService.GetIntegralCoupons(1, 3);
            //Mapper.CreateMap<CouponInfo, CouponGetIntegralCouponModel>();
            if (coupons.Models.Count > 0)
            {
                var datalist = coupons.Models.ToList();
                var objlist = new List<CouponGetIntegralCouponModel>();
                foreach (var item in datalist)
                {
                    var tmp = item.Map<CouponGetIntegralCouponModel>();
                    tmp.ShowIntegralCover = MallIO.GetRomoteImagePath(item.IntegralCover);
                    var vshopobj = _iVShopService.GetVShopByShopId(tmp.ShopId);
                    if (vshopobj != null)
                    {
                        tmp.VShopId = vshopobj.Id;
                        //优惠价封面为空时，取微店Logo，微店Logo为空时，取商城微信Logo
                        if (string.IsNullOrWhiteSpace(tmp.ShowIntegralCover))
                        {
                            if (!string.IsNullOrWhiteSpace(vshopobj.WXLogo))
                            {
                                tmp.ShowIntegralCover = MallIO.GetRomoteImagePath(vshopobj.WXLogo);
                            }
                        }
                    }
                    if (string.IsNullOrWhiteSpace(tmp.ShowIntegralCover))
                    {
                        var siteset = SiteSettingApplication.SiteSettings;
                        tmp.ShowIntegralCover = MallIO.GetRomoteImagePath(siteset.WXLogo);
                    }
                    objlist.Add(tmp);
                }
                result.IntegralCoupons = objlist.ToList();
                result.HasMoreIntegralCoupons = coupons.Total > 3;
            }

            result.HasLogined = false;

            //用户积分与等级
            if (CurrentUser != null)
            {
                //登录后处理会员积分
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                var userGrade = MemberGradeApplication.GetMemberGradeByUserIntegral(userInte.HistoryIntegrals);
                result.MemberAvailableIntegrals = userInte.AvailableIntegrals;
                result.MemberGradeName = userGrade.GradeName;
                result.HasLogined = true;
            }

            return View(result);
        }
        /// <summary>
        /// 礼品列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            return View();
        }
        /// <summary>
        /// 获取礼品列表
        /// </summary>
        /// <param name="pageno"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetList(int pageno = 1, int pagesize = 10)
        {
            GiftQuery query = new GiftQuery();
            query.skey = "";
            query.status = GiftInfo.GiftSalesStatus.Normal;
            query.PageSize = pagesize;
            query.PageNo = pageno;
            QueryPageModel<GiftModel> gifts = _iGiftService.GetGifts(query);
            var data = gifts.Models.ToList();
            foreach (var item in data)
            {
                item.DefaultShowImage = MallIO.GetRomoteImagePath(item.GetImage(ImageSize.Size_350));
            }
            var result = SuccessResult<List<GiftModel>>(data: data);
            return Json(result);
        }
        /// <summary>
        /// 礼品详情
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail(long id)
        {
            GiftsDetailModel result = new GiftsDetailModel();
            var data = _iGiftService.GetById(id);
         //   Mapper.CreateMap<GiftInfo, GiftsDetailModel>();
            result = data.Map<GiftsDetailModel>();
            if (result == null)
            {
                throw new Exception("礼品信息无效！");
            }
            string tmpdefaultimg = result.GetImage(ImageSize.Size_100);
            result.DefaultShowImage = MallIO.GetRomoteImagePath(tmpdefaultimg);
            result.Images = new List<string>();
            result.Description = result.Description.Replace("src=\"/Storage/", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/") + "/");

            //补充图片信息
            for (var _n = 1; _n < 6; _n++)
            {
                string _imgpath = data.ImagePath + "/" + _n.ToString() + ".png";
                if (MallIO.ExistFile(_imgpath))
                {
                    var tmp = MallIO.GetRomoteImagePath(result.GetImage(ImageSize.Size_500, _n));
                    result.Images.Add(tmp);
                }
            }

            #region 礼品是否可兑
            result.CanBuy = true;
            //礼品信息
            if (result.CanBuy)
            {
                if (result.GetSalesStatus != GiftInfo.GiftSalesStatus.Normal)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "礼品" + result.ShowSalesStatus;
                    if (result.GetSalesStatus == GiftInfo.GiftSalesStatus.HasExpired)
                        result.CanNotBuyDes = "活动已结束";//统一app名称而加
                }
            }

            if (result.CanBuy)
            {
                //库存判断
                if (result.StockQuantity < 1)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "已兑完";
                }
            }

            if (result.CanBuy)
            {
                //积分数
                if (result.NeedIntegral < 1)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "礼品信息错误";
                }
            }
            #endregion

            #region 用户信息判断

            if (result.CanBuy && CurrentUser != null)
            {
                //限购数量
                if (result.LimtQuantity > 0)
                {
                    int ownbuynumber = _iGiftsOrderService.GetOwnBuyQuantity(CurrentUser.Id, id);
                    if (ownbuynumber >= result.LimtQuantity)
                    {
                        result.CanBuy = false;
                        result.CanNotBuyDes = "限兑数量已满";
                    }
                }
                if (result.CanBuy)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                    if (userInte.AvailableIntegrals < result.NeedIntegral)
                    {
                        result.CanBuy = false;
                        result.CanNotBuyDes = "积分不足";
                    }
                }
            }
            #endregion
            return View(result);
        }
        /// <summary>
        /// 优惠券列表
        /// </summary>
        /// <returns></returns>
        public ActionResult CouponList()
        {
            return View();
        }
        /// <summary>
        /// 取积分优惠券
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetCouponList(int page = 1, int pagesize = 10)
        {
            QueryPageModel<CouponInfo> coupons = _iCouponService.GetIntegralCoupons(page, pagesize);
            //Mapper.CreateMap<CouponInfo, CouponGetIntegralCouponModel>();
            QueryPageModel<CouponGetIntegralCouponModel> result = new QueryPageModel<CouponGetIntegralCouponModel>();
            result.Total = coupons.Total;
            if (result.Total > 0)
            {
                var datalist = coupons.Models.ToList();
                var objlist = new List<CouponGetIntegralCouponModel>();
                foreach (var item in datalist)
                {
                    var tmp = item.Map<CouponGetIntegralCouponModel>();
                    tmp.ShowIntegralCover = MallIO.GetRomoteImagePath(item.IntegralCover);
                    var vshopobj = _iVShopService.GetVShopByShopId(tmp.ShopId);
                    if (vshopobj != null)
                    {
                        tmp.VShopId = vshopobj.Id;
                        //优惠价封面为空时，取微店Logo，微店Logo为空时，取商城微信Logo
                        if (string.IsNullOrWhiteSpace(tmp.ShowIntegralCover))
                        {
                            if (!string.IsNullOrWhiteSpace(vshopobj.WXLogo))
                            {
                                tmp.ShowIntegralCover = Core.MallIO.GetRomoteImagePath(vshopobj.WXLogo);
                            }
                        }
                    }
                    if (string.IsNullOrWhiteSpace(tmp.ShowIntegralCover))
                    {
                        var siteset = SiteSettingApplication.SiteSettings;
                        tmp.ShowIntegralCover = MallIO.GetRomoteImagePath(siteset.WXLogo);
                    }
                    objlist.Add(tmp);
                }
                result.Models = objlist.ToList();
            }

            int MemberAvailableIntegrals = 0;
            //用户积分与等级
            if (CurrentUser != null)
            {
                //登录后处理会员积分
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                MemberAvailableIntegrals = userInte.AvailableIntegrals;
            }

            var _result = new
            {
                success = true,
                MemberAvailableIntegrals = MemberAvailableIntegrals,
                data = result.Models
            };
            return Json(_result);
        }
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <returns></returns>
        public ActionResult SubmitOrder(long id, long? regionId, int count = 1)
        {
            GiftOrderConfirmPageModel data = new Models.GiftOrderConfirmPageModel();
            List<GiftOrderItemInfo> gorditemlist = new List<GiftOrderItemInfo>();
            GiftOrderItemInfo gorditem;     //订单项

            #region 礼品信息判断
            //礼品信息
            GiftInfo giftdata = _iGiftService.GetById(id);
            if (giftdata == null)
            {
                throw new MallException("错误的礼品编号！");
            }
            #endregion

            gorditem = new GiftOrderItemInfo(); //补充订单项
            gorditem.GiftId = giftdata.Id;
            gorditem.GiftName = giftdata.GiftName;
            gorditem.GiftValue = giftdata.GiftValue;
            gorditem.ImagePath = giftdata.ImagePath;
            gorditem.OrderId = 0;
            gorditem.Quantity = count;
            gorditem.SaleIntegral = giftdata.NeedIntegral;
            gorditemlist.Add(gorditem);

            data.GiftList = gorditemlist;

            data.GiftValueTotal = (decimal)data.GiftList.Sum(d => d.Quantity * d.GiftValue);
            data.TotalAmount = (int)data.GiftList.Sum(d => d.SaleIntegral * d.Quantity);

            //用户地址
            data.ShipAddress = GetShippingAddress(regionId);
            return View(data);
        }

        /// <summary>
        /// 下单前判断
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CanBuy(long id, int count)
        {
            Result result = new Result();
            bool isdataok = true;

            if (CurrentUser == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "您还未登录！";
                result.status = -1;
                return Json(result);
            }


            #region 礼品信息判断
            //礼品信息
            GiftInfo giftdata = _iGiftService.GetById(id);
            if (isdataok)
            {
                if (giftdata == null)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品不存在！";
                    result.status = -2;
                }
            }

            if (isdataok)
            {
                if (giftdata.GetSalesStatus != GiftInfo.GiftSalesStatus.Normal)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品已失效！";
                    result.status = -2;
                }
            }

            if (isdataok)
            {
                //库存判断
                if (count > giftdata.StockQuantity)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品库存不足,仅剩 " + giftdata.StockQuantity.ToString() + " 件！";
                    result.status = -3;
                }
            }

            if (isdataok)
            {
                //积分数
                if (giftdata.NeedIntegral < 1)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品关联等级信息有误或礼品积分数据有误！";
                    result.status = -5;
                    return Json(result);
                }
            }

            #endregion

            #region 用户信息判断

            if (isdataok)
            {
                //限购数量
                if (giftdata.LimtQuantity > 0)
                {
                    int ownbuynumber = _iGiftsOrderService.GetOwnBuyQuantity(CurrentUser.Id, id);
                    if (ownbuynumber + count > giftdata.LimtQuantity)
                    {
                        isdataok = false;
                        result.success = false;
                        result.msg = "超过礼品限兑数量！";
                        result.status = -4;
                    }
                }
            }

            if (isdataok)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                if (giftdata.NeedIntegral * count > userInte.AvailableIntegrals)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "积分不足！";
                    result.status = -6;
                }
            }

            if (isdataok && giftdata.NeedGrade > 0)
            {
                //等级判定
                if (!MemberGradeApplication.IsAllowGrade(CurrentUser.Id,giftdata.NeedGrade))
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "用户等级不足！";
                    result.status = -6;
                }
            }
            #endregion

            if (isdataok)
            {
                result.success = true;
                result.msg = "可以购买！";
                result.status = 1;
            }

            return Json(result);
        }

        /// <summary>
        /// 提交并处理订单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="regionId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OrderSubmit(long id, long regionId, int count)
        {
            Result result = new Result() { success = false, msg = "未知错误", status = 0 };
            bool isdataok = true;

            if (count < 1)
            {
                isdataok = false;
                result.success = false;
                result.msg = "错误的兑换数量！";
                result.status = -8;

                return Json(result);
            }
            //Checkout
            List<GiftOrderItemModel> gorditemlist = new List<GiftOrderItemModel>();

            #region 礼品信息判断
            //礼品信息
            GiftInfo giftdata = _iGiftService.GetById(id);
            if (giftdata == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "礼品不存在！";
                result.status = -2;

                return Json(result);
            }

            if (giftdata.GetSalesStatus != GiftInfo.GiftSalesStatus.Normal)
            {
                isdataok = false;
                result.success = false;
                result.msg = "礼品已失效！";
                result.status = -2;

                return Json(result);
            }

            //库存判断
            if (count > giftdata.StockQuantity)
            {
                isdataok = false;
                result.success = false;
                result.msg = "礼品库存不足,仅剩 " + giftdata.StockQuantity.ToString() + " 件！";
                result.status = -3;

                return Json(result);
            }

            //积分数
            if (giftdata.NeedIntegral < 1)
            {
                isdataok = false;
                result.success = false;
                result.msg = "礼品关联等级信息有误或礼品积分数据有误！";
                result.status = -5;

                return Json(result);
            }
            #endregion

            #region 用户信息判断
            //限购数量
            if (giftdata.LimtQuantity > 0)
            {
                int ownbuynumber = _iGiftsOrderService.GetOwnBuyQuantity(CurrentUser.Id, id);
                if (ownbuynumber + count > giftdata.LimtQuantity)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "超过礼品限兑数量！";
                    result.status = -4;

                    return Json(result);
                }
            }
            var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
            if (giftdata.NeedIntegral * count > userInte.AvailableIntegrals)
            {
                isdataok = false;
                result.success = false;
                result.msg = "积分不足！";
                result.status = -6;

                return Json(result);
            }
            if (giftdata.NeedGrade > 0)
            {
                //等级判定
                if (!MemberGradeApplication.IsAllowGrade(CurrentUser.Id, giftdata.NeedGrade))
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "用户等级不足！";
                    result.status = -6;
                    return Json(result);
                }
            }
            #endregion

            Entities.ShippingAddressInfo shipdata = GetShippingAddress(regionId);
            if (shipdata == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "错误的收货人地址信息！";
                result.status = -6;

                return Json(result);
            }

            if (isdataok)
            {
                gorditemlist.Add(new GiftOrderItemModel { GiftId = giftdata.Id, Counts = count });
                GiftOrderModel createorderinfo = new GiftOrderModel();
                createorderinfo.Gifts = gorditemlist;
                createorderinfo.CurrentUser = CurrentUser;
                createorderinfo.ReceiveAddress = shipdata;
                Mall.Entities.GiftOrderInfo orderdata = _iGiftsOrderService.CreateOrder(createorderinfo);
                result.success = true;
                result.msg = orderdata.Id.ToString();
                result.status = 1;
            }

            return Json(result);
        }
        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        private Entities.ShippingAddressInfo GetShippingAddress(long? regionId)
        {
            Entities.ShippingAddressInfo result = null;
            if (regionId != null)
            {
                result = ShippingAddressApplication.GetUserShippingAddress((long)regionId);
            }
            else
            {
                result = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
            }

            return result;
        }
    }
}