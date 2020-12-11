using AutoMapper;
using Mall.API.Model;
using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.CommonModel;
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
    public class CouponController : BaseApiController
    {

        [HttpGet("GetShopCouponList")]
        public object GetShopCouponList(long shopId)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting(shopId).Select(item => item.CouponID);
            
            //取设置的优惠券
            var coupons = service.GetCouponList(shopId)
                        .Where(item => couponSetList.Contains(item.Id));
      
         
            if (coupons.Count()>0)
            {
               
                var vshop = ShopApplication.GetShop(shopId);
                var settings = service.GetSettingsByCoupon(coupons.Select(p => p.Id).ToList());
                var userCoupon = coupons.Where(d => settings.Any(c => c.CouponID==d.Id && c.PlatForm == Core.PlatformType.Wap)).Select(a => new
                {
                    ShopId = a.ShopId,
                    CouponId = a.Id,
                    Price = a.Price,
                    PerMax = a.PerMax,
                    OrderAmount = a.OrderAmount,
                    Num = a.Num,
                    StartTime = a.StartTime.ToString(),
                    EndTime = a.EndTime.ToString(),
                    CreateTime = a.CreateTime.ToString(),
                    CouponName = a.CouponName,
                    VShopLogo = Core.MallIO.GetRomoteImagePath(vshop.Logo),
                    VShopId = vshop?.Id,
                    ShopName = a.ShopName,
                    Receive = Receive(a.ShopId, a.Id),
                    Remark = a.Remark,
                    UseArea = a.UseArea
                });
                var data = userCoupon.Where(p => p.Receive != 2 && p.Receive != 4);//优惠券已经过期、优惠券已领完，则不显示在店铺优惠券列表中
                dynamic result = SuccessResult();
                result.Coupon = data;
                return result;
            }
            else

                return ErrorResult("该店铺没有可领优惠券");
        }

        [HttpGet("GetUserCounponList")]
        public object GetUserCounponList()
        {
            CheckUserLogin();
            var service = ServiceProvider.Instance<ICouponService>.Create;
            var vshop = ServiceProvider.Instance<IVShopService>.Create;
            var userCouponList = service.GetUserCouponList(CurrentUser.Id);
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
                       var Price = item.Price;

                       var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
                       var grant = bonusService.GetGrant(item.BonusGrantId);
                       var bonus = bonusService.GetShopBonus(grant.ShopBonusId);
                       var shop = ShopApplication.GetShop(bonus.ShopId);
                       var vShop = VshopApplication.GetVShopByShopId(shop.Id);

                       var showOrderAmount = bonus.UsrStatePrice > 0 ? bonus.UsrStatePrice : item.Price;
                       if (bonus.UseState != ShopBonusInfo.UseStateType.FilledSend)
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
                       obj.DateEnd = bonus.DateEnd;
                       obj.ShopName = shop.ShopName;
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
                    NoUseBonusCount = userBonus.Count(r => r.State == (int)ShopBonusReceiveInfo.ReceiveState.NotUse && r.DateEnd > DateTime.Now);
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
                return result;
            }
            else
            {
                throw new Mall.Core.MallException("没有领取记录!");
            }
        }

        private object GetVShop(long shopId)
        {
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shopId);
            if (vshop == null)
                return ErrorResult("没有开通微店");
            else
            {
                var result = new
                {
                    success = true,
                    VShopId = vshop.Id,
                    VShopLogo = Core.MallIO.GetRomoteImagePath(vshop.StrLogo)
                };
                return result;
            }

        }
        //领取优惠券

        [HttpPost("PostAcceptCoupon")]
        public object PostAcceptCoupon(CouponAcceptCouponModel value)
        {
            CheckUserLogin();
            long vshopId = value.vshopId;
            long couponId = value.couponId;
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var couponInfo = couponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now)
            {
                //已经失效
                return ErrorResult("优惠券已经过期.", 2);
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponId;
            crQuery.UserId = CurrentUser.Id;
            QueryPageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
            {
                //达到个人领取最大张数
                return ErrorResult("达到个人领取最大张数，不能再领取.", 3);
            }
            crQuery = new CouponRecordQuery()
            {
                CouponId = couponId
            };
            pageModel = couponService.GetCouponRecordList(crQuery);
            if (pageModel.Total >= couponInfo.Num)
            {
                //达到领取最大张数
                return ErrorResult("此优惠券已经领完了.", 4);
            }
            if (couponInfo.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var integral = MemberIntegralApplication.GetAvailableIntegral(CurrentUserId);
                if (integral < couponInfo.NeedIntegral)
                    return ErrorResult("积分不足 " + couponInfo.NeedIntegral.ToString(), 5);
            }
            CouponRecordInfo couponRecordInfo = new CouponRecordInfo()
            {
                CouponId = couponId,
                UserId = CurrentUser.Id,
                UserName = CurrentUser.UserName,
                ShopId = couponInfo.ShopId
            };
            couponService.AddCouponRecord(couponRecordInfo);
            return SuccessResult("", 1);
        }


        /// <summary>
        /// 取积分优惠券
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetIntegralCoupon")]
        public object GetIntegralCoupon(int page = 1, int pagesize = 10)
        {
            var _iCouponService = ServiceProvider.Instance<ICouponService>.Create;
            IVShopService _iVShopService = ServiceProvider.Instance<IVShopService>.Create;
            QueryPageModel<CouponInfo> coupons = _iCouponService.GetIntegralCoupons(page, pagesize);
           // Mapper.CreateMap<CouponInfo, CouponGetIntegralCouponModel>();
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
                        tmp.ShowIntegralCover = Core.MallIO.GetRomoteImagePath(siteset.WXLogo);
                    }
                    objlist.Add(tmp);
                }
                result.Models = objlist.ToList();
            }
            dynamic _result = SuccessResult();
            _result.Models = result.Models;
            _result.total = result.Total;
            return _result;
        }
      

        private List<ShopBonusReceiveInfo> GetBonusList()
        {
            var service = ServiceProvider.Instance<IShopBonusService>.Create;
            return service.GetDetailByUserId(CurrentUser.Id);
        }
        /// <summary>
        /// 是否可领取优惠券
        /// </summary>
        /// <param name="vshopId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        private int Receive(long vshopId, long couponId)
        {
            if (CurrentUser != null && CurrentUser.Id > 0)//未登录不可领取
            {
                var couponService = ServiceProvider.Instance<ICouponService>.Create;
                var couponInfo = couponService.GetCouponInfo(couponId);
                if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效

                CouponRecordQuery crQuery = new CouponRecordQuery();
                crQuery.CouponId = couponId;
                crQuery.UserId = CurrentUser.Id;
                QueryPageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
                if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax) return 3;//达到个人领取最大张数

                crQuery = new CouponRecordQuery()
                {
                    CouponId = couponId
                };
                pageModel = couponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponInfo.Num) return 4;//达到领取最大张数

                if (couponInfo.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                    if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
                }

                return 1;//可正常领取
            }
            return 0;
        }
    }
}
