using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;


namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [MarketingAuthorization]
    public class CouponController : BaseSellerController
    {
        private ICouponService _iCouponService;
        private IMarketService _iMarketService;
        private IShopService _iShopService;
        public CouponController(ICouponService iCouponService, IMarketService iMarketService, IShopService iShopService)
        {
            _iCouponService = iCouponService;
            _iMarketService = iMarketService;
            _iShopService = iShopService;
        }
        // GET: SellerAdmin/Coupon
        public ActionResult Management()
        {
            //处理错误同步结果
            _iCouponService.ClearErrorWeiXinCardSync();
            
            var settings = MarketApplication.GetServiceSetting(MarketType.Coupon);
            if (settings == null)
                return View("Nosetting");

            var market = _iCouponService.GetCouponService(CurrentSellerManager.ShopId);
            //未购买服务且列表刚进来则让进入购买服务页
            if ((market == null || market.Id <= 0) && Request.Query["first"].ToString() == "1")
                return RedirectToAction("BuyService");
            
            ViewBag.Available = false;
            if (market != null && MarketApplication.GetServiceEndTime(market.Id) > DateTime.Now) 
                ViewBag.Available = true;
             
            return View();
        }

        public JsonResult Cancel(long couponId)
        {
            var shopId = CurrentSellerManager.ShopId;
            _iCouponService.CancelCoupon(couponId, shopId);
            return Json(new Result() { success = true, msg = "操作成功！" });
        }

        #region 添加修改优惠券
        public ActionResult Edit(long id)
        {
            var couponser = _iCouponService;
            var model = couponser.GetCouponInfo(CurrentSellerManager.ShopId, id);
            if (model == null)
                throw new MallException("错误的优惠券编号。");
            if (model.IsSyncWeiXin == 1 && model.WXAuditStatus != (int)WXCardLogInfo.AuditStatusEnum.Audited)
                throw new MallException("同步微信优惠券未审核通过时不可修改。");

            model.FormIsSyncWeiXin = model.IsSyncWeiXin == 1;

            var viewmodel = new CouponViewModel();
            var products = couponser.GetCouponProductsByCouponId(id);

            viewmodel.Coupon = model;
            viewmodel.CouponProducts = products;
            viewmodel.Products = ProductManagerApplication.GetProducts(products.Select(p => p.ProductId));
            viewmodel.Settings = couponser.GetSettingsByCoupon(new System.Collections.Generic.List<long> { id });
            viewmodel.CanVshopIndex = CurrentSellerManager.VShopId > 0;
            var market = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Coupon);
            viewmodel.EndTime = MarketApplication.GetServiceEndTime(market.Id);
            viewmodel.CanAddIntegralCoupon = couponser.CanAddIntegralCoupon(CurrentSellerManager.ShopId, id);
            return View(viewmodel);
        }
        public ActionResult Add()
        {
            CouponInfo model = new CouponInfo();
            long shopId = CurrentSellerManager.ShopId;
            var couponser = _iCouponService;
            model = new CouponInfo();
            model.StartTime = DateTime.Now;
            model.EndTime = model.StartTime.AddMonths(1);
            model.ReceiveType = CouponInfo.CouponReceiveType.ShopIndex;

            model.CanVshopIndex = CurrentSellerManager.VShopId > 0;
            var settings = new System.Collections.Generic.List<CouponSettingInfo>();
        
            if (model.CanVshopIndex)
                settings.Add(new CouponSettingInfo() { Display = 1, PlatForm = PlatformType.Wap });
            settings.Add(new CouponSettingInfo() { Display = 1, PlatForm = PlatformType.PC });
            ViewBag.Settings = settings;
            model.FormIsSyncWeiXin = false;
            model.ShopId = shopId;
            var market = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Coupon);
            var maxEndTime = MarketApplication.GetServiceEndTime(market?.Id??0);
            if (model.EndTime > maxEndTime.Date)
            {
                model.EndTime = maxEndTime.Date;
            }
            ViewBag.EndTime = maxEndTime.ToString("yyyy-MM-dd");
            ViewBag.CanAddIntegralCoupon = couponser.CanAddIntegralCoupon(shopId);
            return View(model);
        }

        [HttpPost]
        public JsonResult Edit(CouponInfo info)
        {
            bool isAdd = false;
            if (info.Id == 0) isAdd = true;
            var couponser = _iCouponService;
            var shopId = CurrentSellerManager.ShopId;
            info.ShopId = shopId;
            var shopName = _iShopService.GetShop(shopId).ShopName;
            info.ShopName = shopName;
            if (info.UseArea == 0)
            {
                info.CouponProductInfo = null;
                info.Remark = "";
            }
            if (isAdd)
            {
                info.CreateTime = DateTime.Now;
                if (info.StartTime >= info.EndTime)
                {
                    return Json(new Result() { success = false, msg = "开始时间必须小于结束时间" });
                }
                info.IsSyncWeiXin = 0;
                if (info.FormIsSyncWeiXin)
                {
                    info.IsSyncWeiXin = 1;

                    if (string.IsNullOrWhiteSpace(info.FormWXColor))
                    {
                        return Json(new Result() { success = false, msg = "错误的卡券颜色" });
                    }
                    if (string.IsNullOrWhiteSpace(info.FormWXCTit))
                    {
                        return Json(new Result() { success = false, msg = "请填写卡券标题" });
                    }

                    if (!WXCardLogInfo.WXCardColors.Contains(info.FormWXColor))
                    {
                        return Json(new Result() { success = false, msg = "错误的卡券颜色" });
                    }
                    //判断字符长度
                    var enc = System.Text.Encoding.Default;
                    if (enc.GetBytes(info.FormWXCTit).Count() > 18)
                    {
                        return Json(new Result() { success = false, msg = "卡券标题不得超过9个汉字" });
                    }
                    if (!string.IsNullOrWhiteSpace(info.FormWXCSubTit))
                    {
                        if (enc.GetBytes(info.FormWXCSubTit).Count() > 36)
                        {
                            return Json(new Result() { success = false, msg = "卡券副标题不得超过18个汉字" });
                        }
                    }
                }
            }
            if (info.CouponSettingInfo == null)
                info.CouponSettingInfo = new System.Collections.Generic.List<CouponSettingInfo>();
            if (info.UseArea == 1 && (info.CouponProductInfo == null || info.CouponProductInfo.Count <= 0))
            {
                return Json(new Result() { success = false, msg = "请选择指定商品" });
            }
            if (info.UseArea == 1 && string.IsNullOrEmpty(info.Remark))
            {
                return Json(new Result() { success = false, msg = "请输入指定商品的备注信息" });
            }
            var couponsetting = Request.Form["chkShow"];

            info.CanVshopIndex = CurrentSellerManager.VShopId > 0;

            switch (info.ReceiveType)
            {
                case CouponInfo.CouponReceiveType.IntegralExchange:
                    if (!couponser.CanAddIntegralCoupon(shopId, info.Id))
                    {
                        return Json(new Result() { success = false, msg = "当前已有积分优惠券，每商家只可以推广一张积分优惠券！" });
                    }
                    info.CouponSettingInfo.Clear();
                    if (info.EndIntegralExchange == null)
                    {
                        return Json(new Result() { success = false, msg = "错误的兑换截止时间" });
                    }
                    if (info.EndIntegralExchange > info.EndTime.AddDays(1).Date)
                    {
                        return Json(new Result() { success = false, msg = "错误的兑换截止时间" });
                    }
                    if (info.NeedIntegral < 10)
                    {
                        return Json(new Result() { success = false, msg = "积分最少10分起兑" });
                    }
                    break;
                case CouponInfo.CouponReceiveType.DirectHair:
                    info.CouponSettingInfo.Clear();
                    break;
                default:
                    if (!string.IsNullOrEmpty(couponsetting))
                    {
                        info.CouponSettingInfo.Clear();
                        var t = couponsetting.ToString().Split(',');
                        if (t.Contains("WAP"))
                            info.CouponSettingInfo.Add(new CouponSettingInfo() { Display = 1, PlatForm = Mall.Core.PlatformType.Wap });
                        if (t.Contains("PC"))
                            info.CouponSettingInfo.Add(new CouponSettingInfo() { Display = 1, PlatForm = Mall.Core.PlatformType.PC });
                    }
                    else
                    {
                        return Json(new Result() { success = false, msg = "必须选择一个推广类型" });
                    }
                    break;
            }

            #region 转移图片
            string path = IOHelper.GetMapPath(string.Format(@"/Storage/Shop/{0}/Coupon/{1}", shopId, info.Id));
            #endregion

            try
            {
                if (isAdd)
                {
                    couponser.AddCoupon(info);
                }
                else
                {
                    couponser.EditCoupon(info);
                }
                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                return Json(new Result { msg = ex.Message, success = false });
            }
        }
        #endregion

        public ActionResult Receivers(long Id)
        {
            ViewBag.Id = Id;
            return View();
        }

        public ActionResult Detail(long Id)
        {
            var model = _iCouponService.GetCouponInfo(CurrentSellerManager.ShopId, Id);
            if (model != null)
            {
                if (model.IsSyncWeiXin == 1 && model.WXAuditStatus != (int)WXCardLogInfo.AuditStatusEnum.Audited)
                {
                    throw new MallException("同步微信优惠券未审核通过时不可修改。");
                }
            }
            string host = CurrentUrlHelper.CurrentUrlNoPort();
            ViewBag.Url = String.Format("{0}/m-wap/vshop/CouponInfo/{1}", host, model.Id);
            var map = Core.Helper.QRCodeHelper.Create(ViewBag.Url);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            //  显示  
            ViewBag.Image = strUrl;
            var market = _iMarketService.GetMarketService(CurrentSellerManager.ShopId, MarketType.Coupon);
            ViewBag.EndTime = MarketApplication.GetServiceEndTime(market.Id).ToString("yyyy-MM-dd");
            return View(model);
        }


        [HttpPost]
        public ActionResult GetReceivers(long Id, int page, int rows)
        {
            CouponRecordQuery query = new CouponRecordQuery();
            query.CouponId = Id;
            query.ShopId = CurrentSellerManager.ShopId;
            query.PageNo = page;
            query.PageSize = rows;
            var record = _iCouponService.GetCouponRecordList(query);
            var coupons = CouponApplication.GetCouponInfo(record.Models.Select(p => p.CouponId));
            var list = record.Models.Select( item =>
               {
                   var coupon = coupons.FirstOrDefault(p => p.Id == item.CouponId);
                   return new
                   {
                       Id = item.Id,
                       Price = Math.Round(coupon.Price, 2),
                       CreateTime = coupon.CreateTime.ToString("yyyy-MM-dd"),
                       CouponSN = item.CounponSN,
                       UsedTime = item.UsedTime.HasValue ? item.UsedTime.Value.ToString("yyyy-MM-dd") : "",
                       ReceviceTime = item.CounponTime.ToString("yyyy-MM-dd"),
                       Recever = item.UserName,
                       OrderId = item.OrderId,
                       Status = item.CounponStatus == Entities.CouponRecordInfo.CounponStatuses.Unuse ? (coupon.EndTime < DateTime.Now.Date ? "已过期" : item.CounponStatus.ToDescription()) : item.CounponStatus.ToDescription(),
                   };
               });
            var model = new { rows = list, total = record.Total };
            return Json(model);
        }

        public ActionResult BuyService()
        {
            var market = _iCouponService.GetCouponService(CurrentSellerManager.ShopId);
            ViewBag.Market = market;
            string endDate = null;
            ViewBag.Available = false;
            ViewBag.LastBuyPrice = -1;
            if (market != null)
            {
                var now = DateTime.Now.Date;
                var endtime = MarketApplication.GetServiceEndTime(market.Id);
                ViewBag.Available = false;
                if (endtime < now)
                    endDate = string.Format("<font class=\"red\">{0} 年 {1} 月 {2} 日</font> (您的优惠券服务已经过期)", endtime.Year, endtime.Month, endtime.Day);
                else if (endtime >= now)
                {
                    ViewBag.Available = true;
                    endDate = string.Format("{0} 年 {1} 月 {2} 日", endtime.Year, endtime.Month, endtime.Day);
                }
                ViewBag.LastBuyPrice = MarketApplication.GetLastBuyPrice(market.Id);
            }
            ViewBag.EndDate = endDate;
            ViewBag.Price = _iMarketService.GetServiceSetting(MarketType.Coupon).Price;
            return View();
        }
        [HttpPost]
        [UnAuthorize]
        public JsonResult BuyService(int month)
        {
            Result result = new Result();
            var service = _iMarketService;
            service.OrderMarketService(month, CurrentSellerManager.ShopId, MarketType.Coupon);
            result.success = true;
            result.msg = "购买服务成功";
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetItemList(int page, int rows, string couponName)
        {
            var service = _iCouponService;
            var result = service.GetCouponList(new CouponQuery { CouponName = couponName, ShopId = CurrentSellerManager.ShopId, IsShowAll = true, PageSize = rows, PageNo = page });
            var list = result.Models.Select(
                item =>
                {
                    var records = service.GetRecordByCoupon(item.Id);
                    int Status = 0;
                    if (item.StartTime <= DateTime.Now && item.EndTime > DateTime.Now)
                        Status = 2;
                    else
                        if (item.StartTime > DateTime.Now)
                        Status = 1;
                    else
                        Status = 0;

                    return new
                    {
                        Id = item.Id,
                        StartTime = item.StartTime.ToString("yyyy/MM/dd"),
                        EndTime = item.EndTime.ToString("yyyy/MM/dd"),
                        Price = Math.Round(item.Price, 2),
                        CouponName = item.CouponName,
                        PerMax = item.PerMax == 0 ? "不限张" : item.PerMax.ToString() + "张/人",
                        OrderAmount = item.OrderAmount == 0 ? "不限制" : "满" + item.OrderAmount + "使用",
                        Num = item.Num,
                        ReceviceNum = records.Count(),
                        RecevicePeople = records.GroupBy(a => a.UserId).Count(),
                        Used = records.Count(a => a.CounponStatus == Entities.CouponRecordInfo.CounponStatuses.Used),
                        IsSyncWeiXin = item.IsSyncWeiXin,
                        WXAuditStatus = (item.IsSyncWeiXin != 1 ? (int)WXCardLogInfo.AuditStatusEnum.Audited : item.WXAuditStatus),
                        Status = Status,
                        CreateTime = item.CreateTime
                    };
                }
                ).OrderByDescending(r => r.Status).ThenByDescending(r => r.CreateTime);
            var model = new { rows = list, total = result.Total };
            return Json(model);
        }
    }
}