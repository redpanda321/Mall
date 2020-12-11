using AutoMapper;
using Mall.Application;
using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.SmallProgAPI
{
    public class CouponController : BaseApiController
    {
        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetUserCoupon")]
        public  object GetUserCoupon(string openId, long couponId)
        {
            CheckUserLogin();
            bool status = true;
            string message = "";
            //long vshopId = vspId;// value.vshopId; 店铺Id
            //long couponId = couponId;// value.couponId; 优惠劵Id
            var couponInfo = CouponApplication.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now)
            {//已经失效
                status = false;
                message = "优惠券已经过期";
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponId;
            crQuery.UserId = CurrentUser.Id;
            QueryPageModel<CouponRecordInfo> pageModel = CouponApplication.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
            {//达到个人领取最大张数
                status = false;
                message = "达到领取最大张数";
            }
            crQuery = new CouponRecordQuery()
            {
                CouponId = couponId
            };
            pageModel = CouponApplication.GetCouponRecordList(crQuery);
            if (pageModel.Total >= couponInfo.Num)
            {//达到领取最大张数
                status = false;
                message = "此优惠券已经领完了";
            }
            if (couponInfo.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                {
                    //积分不足
                    status = false;
                    message = "积分不足 ";
                }
            }
            if (status)
            {
                CouponRecordInfo couponRecordInfo = new CouponRecordInfo()
                {
                    CouponId = couponId,
                    UserId = CurrentUser.Id,
                    UserName = CurrentUser.UserName,
                    ShopId = couponInfo.ShopId
                };
                CouponApplication.AddCouponRecord(couponRecordInfo);
                return Json(new { Status = "OK", Message = "领取成功" });//执行成功
            }
            else
            {
                return Json(new { Status = "NO", Message = message });
            }
        }
        /// <summary>
        /// 获取用户优惠券列表
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet("GetLoadCoupon")]
        public object GetLoadCoupon()
        {
            CheckUserLogin();
            var userCouponList = CouponApplication.GetUserCouponList(CurrentUser.Id);
            var shopBonus = GetBonusList();
            if (userCouponList != null || shopBonus != null)
            {
                //优惠券
                var couponlist = new Object();
                if (userCouponList != null)
                {
                    couponlist = userCouponList.ToArray().Select(a => new
                    {
                        UserId = a.UserId,
                        ShopId = a.ShopId,
                        CouponId = a.CouponId,
                        Price = a.Price,
                        PerMax = a.PerMax,
                        OrderAmount = a.OrderAmount,
                        ShowOrderAmount=a.OrderAmount,
                        Num = a.Num,
                        StartTime = a.StartTime.ToString(),
                        EndTime = a.EndTime.ToString(),
                        CreateTime = a.CreateTime.ToString(),
                        CouponName = a.CouponName,
                        UseStatus = a.UseStatus,
                        UseTime = a.UseTime.HasValue ? a.UseTime.ToString() : null,
                        VShop = GetVShop(a.ShopId),
                        ShopName = a.ShopName,
                        Remark = a.Remark,
                        UseArea = a.UseArea
                    });
                }
                else
                    couponlist = null;
                //代金红包
                var userBonus = new List<dynamic>();
                if (shopBonus != null)
                {
                    userBonus = shopBonus.Select(item =>
                    {
                        var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
                        var grant = bonusService.GetGrant(item.BonusGrantId);
                        var bonus = bonusService.GetShopBonus(grant.ShopBonusId);
                        var shop = ShopApplication.GetShop(bonus.ShopId);
                        var vShop = VshopApplication.GetVShopByShopId(shop.Id);
                        var Price = item.Price;
                        var showOrderAmount = bonus.UsrStatePrice > 0 ? bonus.UsrStatePrice : item.Price;
                        if (bonus.UseState != Entities.ShopBonusInfo.UseStateType.FilledSend)
                            showOrderAmount = item.Price;
                        var Logo = string.Empty;
                        long VShopId = 0;
                        if (vShop != null)
                        {
                            Logo = Core.MallIO.GetRomoteImagePath(vShop.StrLogo);
                            VShopId = vShop.Id;
                        }

                        var State = (int)item.State;
                        if (item.State != ShopBonusReceiveInfo.ReceiveState.Use && bonus.DateEnd < DateTime.Now)
                            State = (int)ShopBonusReceiveInfo.ReceiveState.Expired;
                        var BonusDateEnd = bonus.BonusDateEnd.ToString("yyyy-MM-dd");
                        dynamic obj = new System.Dynamic.ExpandoObject();
                        obj.Price = Price;
                        obj.ShowOrderAmount = showOrderAmount;
                        obj.Logo = Logo;
                        obj.VShopId = VShopId;
                        obj.State = State;
                        obj.BonusDateEnd = BonusDateEnd;
                        obj.ShopName = shop.ShopName;
                        obj.DateEnd = bonus.DateEnd;
                        return obj;
                    }).ToList();
                }
                else
                    shopBonus = null;
                //优惠券
                int NoUseCouponCount = 0;
                int UseCouponCount = 0;
                if (userCouponList != null)
                {
                    NoUseCouponCount = userCouponList.Count(item => (item.EndTime > DateTime.Now && item.UseStatus == CouponRecordInfo.CounponStatuses.Unuse));
                    UseCouponCount = userCouponList.Count() - NoUseCouponCount;
                }
                //红包
                int NoUseBonusCount = 0;
                int UseBonusCount = 0;
                if (shopBonus != null)
                {
                    var nusv = ShopBonusReceiveInfo.ReceiveState.NotUse.GetHashCode();
                    NoUseBonusCount = userBonus.Count(r => r.State == nusv && r.DateEnd > DateTime.Now);
                    UseBonusCount = userBonus.Count() - NoUseBonusCount;
                }

                int UseCount = UseCouponCount + UseBonusCount;
                int NotUseCount = NoUseCouponCount + NoUseBonusCount;

                var result = new
                {
                    success = true,
                    NoUseCount = NotUseCount,
                    UserCount = UseCount,
                    Coupon = couponlist,
                    Bonus = userBonus
                };
                return Json(result);
            }
            else
            {
                throw new Mall.Core.MallException("没有领取记录!");
            }
        }

        /// <summary>
        /// 获取系统可领取优惠券列表
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetLoadSiteCoupon")]
        public object  GetLoadSiteCoupon(string openId = "", int pageSize = 10, int pageIndex = 1, int obtainWay = 1)
        {
            CheckUserLogin();
            CouponRecordQuery query = new CouponRecordQuery();
            query.UserId = CurrentUser.Id;
            query.PageNo = pageIndex;
            query.PageSize = pageSize;
            if (obtainWay == 1) //（1=未使用 2=已使用 3=已过期）
            {
                query.Status = 0;
            }
            else if (obtainWay == 2)
            {
                query.Status = 1;
            }
            else
            {
                query.Status = obtainWay;
            }
            var record = CouponApplication.GetCouponRecordList(query);
            var coupons = CouponApplication.GetCouponInfo(record.Models.Select(p => p.Id));
            var list = record.Models.Select(
               item => {
                   var coupon = coupons.FirstOrDefault(p => p.Id == item.CouponId);
                   return new
                   {
                       CouponId = coupon.Id,
                       CouponName = coupon.CouponName,
                       Price = Math.Round(coupon.Price, 2),
                       SendCount = coupon.Num,
                       UserLimitCount = coupon.PerMax,
                       OrderUseLimit = Math.Round(coupon.OrderAmount, 2),
                       StartTime = coupon.StartTime.ToString("yyyy.MM.dd"),
                       ClosingTime = coupon.EndTime.ToString("yyyy.MM.dd"),
                       ObtainWay = coupon.ReceiveType,
                       NeedPoint = coupon.NeedIntegral,
                       UseArea = coupon.UseArea,
                       Remark = coupon.Remark
                   };
               });
            return Json(new { total = record.Total, Data = list });
        }
        /// <summary>
        /// 获取优惠券信息
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet("GetCouponDetail")]
        public object GetCouponDetail(string openId, int couponId = 0)
        {
            if (couponId <= 0)
            {
                return Json(ErrorResult<dynamic>("参数错误"));
            }
            CouponInfo coupon = CouponApplication.GetCouponInfo(couponId);
            if (coupon == null)
            {
                return Json(ErrorResult<dynamic>("错误的优惠券编号"));
            }
            else
            {
                return Json(new
                {
                    CouponId = coupon.Id,
                    CouponName = coupon.CouponName,
                    Price = coupon.Price,
                    SendCount = coupon.Num,
                    UserLimitCount = coupon.PerMax,
                    OrderUseLimit = Math.Round(coupon.OrderAmount, 2),
                    StartTime = coupon.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ClosingTime = coupon.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    CanUseProducts = "",
                    ObtainWay = coupon.ReceiveType,
                    NeedPoint = coupon.NeedIntegral,
                    UseWithGroup = false,
                    UseWithPanicBuying = false,
                    UseWithFireGroup = false,
                    Remark = coupon.Remark,
                    UseArea = coupon.UseArea
                });
            }
        }

        [HttpGet("GetBonusList")]
        private List<ShopBonusReceiveInfo> GetBonusList()
        {
            return ShopBonusApplication.GetDetailByUserId(CurrentUser.Id);
        }

        private object GetVShop(long shopId)
        {
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shopId);
            if (vshop == null)
                return null;
            else
                return new { VShopId = vshop.Id, VShopLogo = Core.MallIO.GetRomoteImagePath(vshop.StrLogo) };

        }

        /// <summary>
        /// 取积分优惠券
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetIntegralCoupon")]
        public object GetIntegralCoupon(int pageNo = 1, int pageSize = 10)
        {
            QueryPageModel<CouponInfo> coupons = CouponApplication.GetIntegralCoupons(pageNo, pageSize);
          //  Mapper.CreateMap<CouponInfo, CouponGetIntegralCouponModel>(); 
            QueryPageModel<CouponGetIntegralCouponModel> result = new QueryPageModel<CouponGetIntegralCouponModel>();
            result.Total = coupons.Total;
            if (result.Total > 0)
            {
                var datalist = coupons.Models.ToList();
                var objlist = new List<CouponGetIntegralCouponModel>();
                foreach (var item in datalist)
                {
                    var tmp = item.Map<CouponGetIntegralCouponModel>();
                    tmp.ShowIntegralCover = Core.MallIO.GetRomoteImagePath(item.IntegralCover);
                    var vshopobj = VshopApplication.GetVShopByShopId(tmp.ShopId);
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
                        tmp.ShowIntegralCover = Core.MallIO.GetRomoteImagePath(siteset.WXLogo);
                    }
                    objlist.Add(tmp);
                }
                result.Models = objlist.ToList();
            }
            return Json(new { total = result.Total, Data = result.Models });
        }
    }
}
