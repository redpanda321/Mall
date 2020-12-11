using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mall.Service
{
    public class CouponService : ServiceBase, ICouponService
    {
        /// <summary>
        /// 优惠券类型
        /// </summary>
        private WXCardLogInfo.CouponTypeEnum ThisCouponType = WXCardLogInfo.CouponTypeEnum.Coupon;
        /// <summary>
        /// 微信卡券服务
        /// </summary>
        private IWXCardService ser_wxcard;

        public CouponService()
        {
            ser_wxcard = Mall.ServiceProvider.Instance<IWXCardService>.Create;
        }
        public CouponRecordInfo AddCouponRecord(CouponRecordInfo info)
        {
            var shop = DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == info.ShopId).FirstOrDefault();
            if (shop == null)
            {
                throw new MallException("错误的店铺信息");
            }
            if (shop.ShopStatus != ShopInfo.ShopAuditStatus.Open)
            {
                throw new MallException("兑换失败，当前店铺已过期/冻结");
            }
            if (shop.EndDate < DateTime.Now)
            {
                throw new MallException("兑换失败，当前店铺已过期/冻结");
            }
            var shopName = shop.ShopName;
            var coupondata = DbFactory.Default.Get<CouponInfo>().Where(d => d.Id == info.CouponId).FirstOrDefault();

            if (coupondata.IsSyncWeiXin == 1 && coupondata.WXAuditStatus != (int)WXCardLogInfo.AuditStatusEnum.Audited)
            {
                throw new MallException("优惠券状态错误，不可领取");
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    //扣积分
                    if (coupondata.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange)
                    {
                        Mall.Entities.MemberIntegralRecordInfo mirinfo = new Mall.Entities.MemberIntegralRecordInfo();
                        mirinfo.UserName = info.UserName;
                        mirinfo.MemberId = info.UserId;
                        mirinfo.RecordDate = DateTime.Now;
                        mirinfo.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Exchange;
                        mirinfo.Integral = coupondata.NeedIntegral;
                        mirinfo.ReMark = "兑换优惠券:面值" + coupondata.Price.ToString("f2");
                        var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Exchange, mirinfo.Integral);
                        ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(mirinfo, memberIntegral);
                    }

                    info.CounponStatus = CouponRecordInfo.CounponStatuses.Unuse;
                    info.CounponSN = Guid.NewGuid().ToString().Replace("-", "");
                    info.UsedTime = null;
                    info.CounponTime = DateTime.Now;
                    info.ShopName = shopName;
                    info.OrderId = null;
                    var flag = DbFactory.Default.Add(info);
                });
            return info;
        }
        public void AddSendmessagerecordCouponSN(List<SendmessagerecordCouponSNInfo> items)
        {
            if (items != null && items.Count > 0)
            {
                DbFactory.Default.Add<SendmessagerecordCouponSNInfo>(items);
            }
        }
        public void UseCoupon(long userId, IEnumerable<long> Ids, IEnumerable<OrderInfo> orders)
        {
            var date = DateTime.Now.Date;
            var records = DbFactory.Default.Get<CouponRecordInfo>()
                .LeftJoin<CouponInfo>((cri, ci) => cri.CouponId == ci.Id)
                .Where(p => p.UserId == userId && p.Id.ExIn(Ids) && p.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse)
                .Where<CouponInfo>(a => a.EndTime > date)
                .ToList();
            var ids = records.Select(p => p.CouponId).ToList();
            var coupons = DbFactory.Default.Get<CouponInfo>(p => p.Id.ExIn(ids)).ToList();
            DbFactory.Default.InTransaction(() =>
            {
                //微信卡券操作
                foreach (var info in records)
                {
                    var coupon = coupons.FirstOrDefault(p => p.Id == info.CouponId);
                    info.CounponStatus = CouponRecordInfo.CounponStatuses.Used;
                    info.UsedTime = DateTime.Now;
                    info.OrderId = orders.FirstOrDefault(a => a.ShopId == info.ShopId && a.ProductTotalAmount >= coupon.OrderAmount).Id;
                    ser_wxcard.Consume(info.Id, ThisCouponType);
                    DbFactory.Default.Update(info);
                }
            });
        }

        public ActiveMarketServiceInfo GetCouponService(long shopId)
        {
            if (shopId <= 0)
            {
                throw new MallException("ShopId不能识别");
            }
            var market = DbFactory.Default.Get<ActiveMarketServiceInfo>().Where(m => m.ShopId == shopId && m.TypeId == MarketType.Coupon).FirstOrDefault();
            return market;
        }

        public List<CouponSettingInfo> GetSettingsByCoupon(List<long> coupons)
        {
            return DbFactory.Default.Get<CouponSettingInfo>().Where(p => p.CouponID.ExIn(coupons)).ToList();
        }


        public QueryPageModel<CouponInfo> GetCouponList(CouponQuery query)
        {
            if (query.ShopId.HasValue)
            {
                if (query.ShopId <= 0)
                {
                    throw new MallException("ShopId不能识别");
                }
            }
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            var coupon = DbFactory.Default.Get<CouponInfo>();
            if (query.ShopId.HasValue)
            {
                coupon = coupon.Where(d => d.ShopId == query.ShopId);
            }
            if (query.IsShowAll != true)
            {
                coupon = coupon.Where(d => d.WXAuditStatus == auditsuccess);
            }
            if (query.ShowPlatform.HasValue)
            {
                var sett = DbFactory.Default
                    .Get<CouponSettingInfo>()
                    .Where<CouponInfo>((csi, ci) => ci.Id == csi.CouponID && csi.PlatForm == query.ShowPlatform.Value);
                coupon = coupon.Where(d => d.ExExists(sett));
            }
            if (query.IsOnlyShowNormal == true)
            {
                DateTime curMindate = DateTime.Now.Date;
                DateTime curMaxdate = curMindate.AddDays(1).Date;
                coupon = coupon.Where(d => d.EndTime >= curMaxdate && d.StartTime <= curMindate);
            }
            if (!string.IsNullOrWhiteSpace(query.CouponName))
            {
                coupon = coupon.Where(d => d.CouponName.Contains(query.CouponName));
            }
            var ret = coupon.OrderByDescending(o => o.EndTime).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<CouponInfo> pageModel = new QueryPageModel<CouponInfo>() { Models = ret, Total = ret.TotalRecordCount };
            return pageModel;
        }
        public List<CouponInfo> GetCouponList(long shopid)
        {
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            return DbFactory.Default
                .Get<CouponInfo>()
                .Where(item => item.ShopId == shopid && item.WXAuditStatus == auditsuccess)
                .ToList();
        }

        public List<CouponInfo> GetCoupons(List<long> coupons) {
            return DbFactory.Default.Get<CouponInfo>()
                .Where(p => p.Id.ExIn(coupons)).ToList();
        }
        public List<CouponInfo> GetCouponLists(long shopId)
        {
            var Date = DateTime.Now;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            return DbFactory.Default
                .Get<CouponInfo>()
                .Where(a => a.ShopId == shopId && a.EndTime >= Date && a.WXAuditStatus == auditsuccess)
                .OrderByDescending(a => a.Price)
                .ToList();
        }
        /// <summary>
        /// 取用户领取的所有优惠卷信息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<UserCouponInfo> GetUserCouponList(long userid)
        {
            var list = DbFactory.Default
                .Get<CouponInfo>()
                .InnerJoin<CouponRecordInfo>((ci, cri) => ci.Id == cri.CouponId && cri.UserId == userid)
                .Select(a => new { CouponId = a.Id })
                .Select()
                .Select<CouponRecordInfo>(n => new { n.UserId, UseStatus = n.CounponStatus, UseTime = n.UsedTime })
                .ToList<UserCouponInfo>();
            return list;
        }
        public List<CouponSendByRegisterDetailedInfo> GetCouponSendByRegisterDetailedInfo(long registerId)
        {
            return DbFactory.Default.Get<CouponSendByRegisterDetailedInfo>().Where(p => p.CouponRegisterId == registerId).ToList();
        }

        public List<UserCouponInfo> GetAllUserCoupon(long userid)
        {
            var date = DateTime.Now;

            var list = DbFactory.Default
                .Get<CouponRecordInfo>()
                .LeftJoin<CouponInfo>((cri, ci) => cri.CouponId == ci.Id)
                .Where(item => item.UserId == userid && item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse)
                .Where<CouponInfo>(item => item.EndTime > date)
                .Select(n => new { n.UserId, n.ShopId, n.CouponId, UseStatus = n.CounponStatus, UseTime = n.UsedTime })
                .Select<CouponInfo>()
                .ToList<UserCouponInfo>();
            

            return list;

           
        }
     
        public Dictionary<long, int> GetRecordCountByCoupon(List<long> coupons)
        {
            return DbFactory.Default.Get<CouponRecordInfo>(p => p.CouponId.ExIn(coupons))
                .GroupBy(p => p.CouponId)
                .Select(p => new { CouponId = p.CouponId, Count = p.Id.ExCount(false) })
                .ToList<dynamic>()
                .ToDictionary(k => (long)k.CouponId, v => (int)v.Count);
        }

        public List<CouponRecordInfo> GetRecordByCoupon(long coupon)
        {
            return DbFactory.Default.Get<CouponRecordInfo>().Where(p => p.CouponId == coupon).ToList();
        }

        public int GetRecordCountByCoupon(long coupon)
        {
            return DbFactory.Default.Get<CouponRecordInfo>().Where(p => p.CouponId == coupon).Count();
        }
        public QueryPageModel<CouponRecordInfo> GetCouponRecordList(CouponRecordQuery query)
        {
            var date = DateTime.Now;
            var coupons = DbFactory.Default
                .Get<CouponRecordInfo>()
                .LeftJoin<CouponInfo>((cri, ci) => cri.CouponId == ci.Id);
            if (query.CouponId.HasValue)
            {
                coupons.Where(d => d.CouponId == query.CouponId);
            }
            if (query.UserId.HasValue)
            {
                coupons.Where(d => d.UserId == query.UserId.Value);
            }
            if (query.ShopId.HasValue)
            {
                coupons.Where(d => d.ShopId == query.ShopId.Value);
            }
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                coupons.Where(d => d.UserName.Contains(query.UserName));
            }

            switch (query.Status)
            {
                case 0:
                    coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse);
                    coupons.Where<CouponInfo>(item => item.EndTime > date);
                    break;
                case 1:
                    coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Used);
                    break;
                case 2:
                    coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse);
                    coupons.Where<CouponInfo>(item => item.EndTime <= date);
                    break;
                case 3:
                    coupons.Where<CouponRecordInfo, CouponInfo>((cri, ci) => cri.CounponStatus == CouponRecordInfo.CounponStatuses.Used ||
                        (cri.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse && ci.EndTime <= date));
                    break;
            }
            var ret = coupons.OrderByDescending(o => o.UsedTime).OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<CouponRecordInfo> pageModel = new QueryPageModel<CouponRecordInfo>() { Models = ret, Total = ret.TotalRecordCount };
            return pageModel;
        }

        public CouponRecordInfo GetCouponRecordById(long id)
        {
            CouponRecordInfo result = DbFactory.Default.Get<CouponRecordInfo>().Where(d => d.Id == id).FirstOrDefault();
            //数据补充
            if (result != null && result.WXCodeId.HasValue)
            {
                result.WXCardCodeInfo = DbFactory.Default.Get<WXCardCodeLogInfo>().Where(d => d.Id == result.WXCodeId.Value).FirstOrDefault();
            }
            return result;
        }


        public CouponInfo GetCouponInfo(long shopId, long couponId)
        {
            CouponInfo result = DbFactory.Default.Get<CouponInfo>().Where(a => a.ShopId == shopId && a.Id == couponId).FirstOrDefault();
            if (result != null)
            {
                if (result.IsSyncWeiXin == 1)
                {
                    result.WXCardInfo = DbFactory.Default.Get<WXCardLogInfo>().Where(a => a.Id == result.CardLogId).FirstOrDefault();
                }
            }
            return result;
        }

        public CouponInfo GetCouponInfo(long couponId)
        {
            CouponInfo result = DbFactory.Default.Get<CouponInfo>().Where(a => a.Id == couponId).FirstOrDefault();
            if (result != null)
            {
                if (result.IsSyncWeiXin == 1)
                {
                    result.WXCardInfo = DbFactory.Default.Get<WXCardLogInfo>().Where(a => a.Id == result.CardLogId).FirstOrDefault();
                }
            }
            return result;
        }

        public List<CouponInfo> GetCouponInfo(long[] couponId)
        {
            List<CouponInfo> result = DbFactory.Default.Get<CouponInfo>().Where(a => a.Id.ExIn(couponId)).ToList();
            return result;
        }

        /// <summary>
        /// 添加或修改条件判断
        /// </summary>
        /// <param name="info"></param>
        private void CanAddOrEditCoupon(CouponInfo info)
        {
            var ids = DbFactory.Default
                .Get<CouponInfo>()
                .Where(a => a.EndTime > DateTime.Now && a.ShopId == info.ShopId && a.ReceiveType != CouponInfo.CouponReceiveType.IntegralExchange
                    && a.ReceiveType != CouponInfo.CouponReceiveType.DirectHair)
                .Select(a => a.Id)
                .ToList<long>();

            var setting = DbFactory.Default.Get<CouponSettingInfo>().Where(a => a.CouponID.ExIn(ids)).ToList();
            var infoSetting = DbFactory.Default.Get<CouponSettingInfo>().Where(p => p.CouponID == info.Id).ToList();
            if (infoSetting.Count > 0)
            {
                int maxnum = 5;
                if (setting.Count(a => a.PlatForm == Core.PlatformType.Wap) >= maxnum && !ids.Any(d => d == info.Id) && infoSetting.Any(d => d.PlatForm == PlatformType.Wap))
                    throw new MallException("最多添加5个移动端优惠券");
                if (setting.Count(a => a.PlatForm == Core.PlatformType.PC) >= maxnum && !ids.Any(d => d == info.Id) && infoSetting.Any(d => d.PlatForm == PlatformType.PC))
                    throw new MallException("最多添加5个PC端个优惠券");
            }

        }

        public void AddCoupon(CouponInfo info)
        {
            CanAddOrEditCoupon(info);
            var coupon = DbFactory.Default.Get<ActiveMarketServiceInfo>().Where(a => a.TypeId == MarketType.Coupon && a.ShopId == info.ShopId).FirstOrDefault();
            if (coupon == null)
            {
                throw new MallException("您没有订购此服务");
            }
            var serviceEndTime = MarketApplication.GetServiceEndTime(coupon.Id);
            var newEnd = DateTime.Parse(serviceEndTime.ToString("yyyy-MM-dd") + " 23:59:59");
            if (newEnd < info.EndTime)
            {
                throw new MallException(string.Format("活动结束时间不得超过服务到期时间，<br/>您的服务到期时间为{0}", newEnd.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            info.WXAuditStatus = 1;
            if (info.IsSyncWeiXin == 1)
            {
                //微信卡券同步改为自动过审后有BUG,暂时默认优惠券添加就是同步状态成功的
                //info.WXAuditStatus = 0;
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    DbFactory.Default.Add(info);

                    info.CouponSettingInfo.ForEach(p => p.CouponID = info.Id);
                    DbFactory.Default.AddRange(info.CouponSettingInfo);

                    if (info.CouponProductInfo != null)
                    {
                        info.CouponProductInfo.ForEach(p => p.CouponId = info.Id);
                        DbFactory.Default.Add<CouponProductInfo>(info.CouponProductInfo);
                    }

                    #region 同步微信
                    if (info.IsSyncWeiXin == 1)
                    {
                        WXCardLogInfo wxdata = new WXCardLogInfo();
                        wxdata.CardColor = info.FormWXColor;
                        wxdata.CardTitle = info.FormWXCTit;
                        wxdata.CardSubTitle = info.FormWXCSubTit;
                        wxdata.CouponType = ThisCouponType;
                        wxdata.CouponId = info.Id;
                        wxdata.ShopId = info.ShopId;
                        wxdata.ShopName = info.ShopName;
                        wxdata.ReduceCost = (int)(info.Price * 100);
                        wxdata.LeastCost = (int)(info.OrderAmount * 100);
                        wxdata.Quantity = info.Num;
                        wxdata.GetLimit = info.PerMax;
                        wxdata.DefaultDetail = info.Price.ToString("F2") + "元优惠券1张";
                        wxdata.BeginTime = info.StartTime.Date;
                        wxdata.EndTime = info.EndTime.AddDays(1).AddMinutes(-1);
                        if (ser_wxcard.Add(wxdata))
                        {
                            DbFactory.Default
                                .Set<CouponInfo>()
                                .Set(n => n.CardLogId, wxdata.Id)
                                .Where(n => n.Id == info.Id)
                                .Succeed();
                        }
                        else
                        {
                            DbFactory.Default.Del(info);
                            throw new MallException("同步微信卡券失败，请检查参数是否有错！");
                        }
                    }
                    #endregion
                });
     

            SaveCover(info);
        }

        public void EditCoupon(CouponInfo info)
        {

            CanAddOrEditCoupon(info);
            var model = DbFactory.Default.Get<CouponInfo>().Where(a => a.ShopId == info.ShopId && a.Id == info.Id).FirstOrDefault();
            if (model == null)
            {
                throw new MallException("错误的优惠券信息");
            }
            //计算库存量
            int nqnum = DbFactory.Default.Get<CouponRecordInfo>().Where(d => d.CouponId == model.Id).Count();
            if (info.Num < nqnum)
            {
                throw new MallException("错误的发放总量，已领取数已超过发放总量");
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    DbFactory.Default.Del<CouponSettingInfo>(p => p.CouponID == model.Id);
                    DbFactory.Default.Del<CouponProductInfo>(p => p.CouponId == model.Id);
                    model.CouponName = info.CouponName;
                    model.PerMax = info.PerMax;
                    model.Num = info.Num;
                    model.ReceiveType = info.ReceiveType;
                    model.EndIntegralExchange = info.EndIntegralExchange;
                    model.NeedIntegral = info.NeedIntegral;
                    model.IntegralCover = info.IntegralCover;
                    model.UseArea = info.UseArea;
                    model.Remark = info.Remark;

                    if (model.IsSyncWeiXin == 1 && model.CardLogId.HasValue)
                    {
                        var carddata = DbFactory.Default.Get<WXCardLogInfo>().Where(d => d.Id == model.CardLogId.Value).FirstOrDefault();
                        if (carddata != null)
                        {
                            int wxstock = model.Num - nqnum;
                            //同步微信限领
                            ser_wxcard.EditGetLimit(model.PerMax, carddata.CardId);
                            //同步微信库存
                            ser_wxcard.EditStock(wxstock, carddata.CardId);
                        }
                    }
                    SaveCover(model);

                    DbFactory.Default.Update(model);
                    if ((info.CouponSettingInfo?.Count ?? 0) > 0)
                    {
                        info.CouponSettingInfo.ForEach(p => p.CouponID = model.Id);
                        DbFactory.Default.Add<CouponSettingInfo>(info.CouponSettingInfo);
                    }
                    DbFactory.Default.AddRange(info.CouponProductInfo);
                });
        }

        public List<CouponInfo> GetTopCoupon(long shopId, int top = 8, PlatformType type = Core.PlatformType.PC)
        {
            var Date = DateTime.Now;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            var sub = DbFactory.Default
                .Get<CouponSettingInfo>()
                .Where<CouponInfo>((csi, ci) => csi.CouponID == ci.Id && csi.PlatForm == type)
                .Select(n => n.Id);

            return DbFactory.Default
                .Get<CouponInfo>()
                .Where(a => a.ShopId == shopId && a.EndTime >= Date && a.WXAuditStatus == auditsuccess && a.ExExists(sub))
                .OrderByDescending(a => a.Price)
                .Take(top)
                .ToList();
        }

        //使某个优惠券失效
        public void CancelCoupon(long couponId, long shopId)
        {
            var coupon = DbFactory.Default.Get<CouponInfo>().Where(a => a.ShopId == shopId && a.Id == couponId).FirstOrDefault();
            if (coupon == null)
            {
                throw new MallException("找不到相对应的优惠券！");
            }
            coupon.EndTime = DateTime.Now.Date.AddDays(-1);
            DbFactory.Default
                .InTransaction(() =>
                {
                    DbFactory.Default.Update(coupon);
                    //同步微信
                    if (coupon.IsSyncWeiXin == 1 && coupon.CardLogId != null)
                    {
                        ser_wxcard.Delete(coupon.CardLogId.Value);
                        //TODO:DZY 处理过期投放??
                    }
                });
        }

        public int GetUserReceiveCoupon(long couponId, long userId)
        {
            return DbFactory.Default.Get<CouponRecordInfo>().Where(a => a.CouponId == couponId && a.UserId == userId).Count();
        }
        public int GetAvailableCouponCountByUser(long userId)
        {
            return DbFactory.Default.Get<CouponRecordInfo>()
                 .LeftJoin<CouponInfo>((cri, ci) => cri.CouponId == ci.Id)
                 .Where(a => a.UserId == userId && a.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse)
                 .Where<CouponInfo>(a => a.EndTime > DateTime.Now)
                 .Count();
        }

        public List<CouponRecordInfo> GetUserCoupon(long shopId, long userId, decimal totalPrice)
        {
            var date = DateTime.Now;
            return DbFactory.Default
                .Get<CouponRecordInfo>()
                .LeftJoin<CouponInfo>((cri, ci) => cri.CouponId == ci.Id)
                .Where(item => item.ShopId == shopId && item.UserId == userId && item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse)
                .Where<CouponInfo>(item => item.StartTime <= date && item.EndTime > date && item.OrderAmount <= totalPrice)
                .OrderByDescending<CouponInfo>(item => item.Price).Select<CouponRecordInfo>()
                .ToList();
            // item.Mall_Coupon.Price < totalPrice
            //不需要大于优惠券的价格
        }

        public List<CouponRecordInfo> GetOrderCoupons(long userId, IEnumerable<long> Ids)
        {
            var date = DateTime.Now;
            return DbFactory.Default
                .Get<CouponRecordInfo>()
                .LeftJoin<CouponInfo>((cri, ci) => cri.CouponId == ci.Id)
                .Where(a => a.UserId == userId && a.Id.ExIn(Ids) && a.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse)
                .Where<CouponInfo>(a => a.StartTime <= date && a.EndTime > date).Select<CouponRecordInfo>()
                .ToList();
            //.DistinctBy(a => a.ShopId);
        }


        public CouponRecordInfo GetCouponRecordInfo(long userId, long orderId)
        {
            CouponRecordInfo result = DbFactory.Default.Get<CouponRecordInfo>().Where(a => a.UserId == userId && a.OrderId == orderId).FirstOrDefault();
            if (result != null)
            {
                //数据补充
                if (result.WXCodeId.HasValue)
                {
                    result.WXCardCodeInfo = DbFactory.Default.Get<WXCardCodeLogInfo>().Where(d => d.Id == result.WXCodeId.Value).FirstOrDefault();
                }
            }
            return result;
        }
        /// <summary>
        /// 是否可以添加积分兑换红包
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public bool CanAddIntegralCoupon(long shopid, long id = 0)
        {
            bool result = false;
            DateTime CurDay = DateTime.Now.Date;
            DateTime CurTime = DateTime.Now;
            var sql = DbFactory.Default
                .Get<CouponInfo>()
                .Where(d => d.ShopId == shopid && d.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange
                    && d.EndIntegralExchange >= CurTime && d.EndTime >= CurDay);
            if (id > 0)
            {
                sql = sql.Where(d => d.Id != id);
            }
            result = sql.Count() < 1;
            return result;
        }
        /// <summary>
        /// 取积分优惠券列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public QueryPageModel<CouponInfo> GetIntegralCoupons(int page, int pageSize)
        {
            QueryPageModel<CouponInfo> result = new QueryPageModel<CouponInfo>();
            DateTime CurDay = DateTime.Now;
            DateTime CurTime = DateTime.Now;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            var sql = DbFactory.Default.Get<CouponInfo>().Where(d => d.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange
                && d.EndIntegralExchange >= CurTime && d.EndTime >= CurDay && d.StartTime <= CurDay
                && d.WXAuditStatus == auditsuccess && d.Num > 0);
            sql.InnerJoin<ShopInfo>((jc, js) => jc.ShopId == js.Id
            && js.ShopStatus == ShopInfo.ShopAuditStatus.Open
            && js.EndDate >= DateTime.Now
            );
            var ret = sql.OrderByDescending(o => o.CreateTime).ToPagedList(page, pageSize);
            result.Models = ret;
            result.Total = ret.TotalRecordCount;
            return result;
        }

        /// <summary>
        /// 同步微信卡券审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cardid"></param>
        /// <param name="auditstatus">审核状态</param>
        public void SyncWeixinCardAudit(long id, string cardid, WXCardLogInfo.AuditStatusEnum auditstatus)
        {
            var coupon = DbFactory.Default.Get<CouponInfo>().Where(d => d.Id == id).FirstOrDefault();
            if (coupon != null)
            {
                coupon.WXAuditStatus = (int)auditstatus;
                DbFactory.Default.Update(coupon);
            }
        }

        /// <summary>
        /// 处理错误的卡券同步信息
        /// </summary>
        public void ClearErrorWeiXinCardSync()
        {
            DateTime overtime = DateTime.Now.AddDays(-2).Date;
            int wxaudstate = (int)WXCardLogInfo.AuditStatusEnum.Auditin;
            var datalist = DbFactory.Default.Get<CouponInfo>().Where(d => d.CreateTime < overtime && d.IsSyncWeiXin == 1 && d.WXAuditStatus == wxaudstate).ToList();
            if (datalist.Count > 0)
            {
                DbFactory.Default
                    .InTransaction(() =>
                    {
                        var cardids = datalist.Select(d => d.CardLogId.Value).ToList();
                        var cardlist = DbFactory.Default.Get<WXCardLogInfo>().Where(d => d.Id.ExIn(cardids)).ToList();
                        if (cardlist.Count > 0)
                        {
                            DbFactory.Default.Del<WXCardLogInfo>(cardlist);
                        }
                        foreach (var item in datalist)
                        {
                            item.WXAuditStatus = (int)WXCardLogInfo.AuditStatusEnum.AuditNot;
                            item.EndTime = DateTime.Now.AddDays(-1);
                            DbFactory.Default.Update(item);
                        }
                    });
            }
        }

        /// <summary>
        /// 转移封面图片
        /// </summary>
        /// <param name="model"></param>
        public void SaveCover(CouponInfo model)
        {
            string image = model.IntegralCover;
            string path = string.Format(@"/Storage/Shop/{0}/Coupon/", model.ShopId);
            var ext = ".png";
            string filename = model.Id.ToString() + ext;
            var savepath = Path.Combine(path, filename);
            if (image != null && image.Contains("/temp/"))
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(temp, savepath, true);
                model.IntegralCover = savepath;
                DbFactory.Default.Update(model);
            }
        }



        public QueryPageModel<CouponInfo> GetCouponByName(string text, DateTime endDate, int ReceiveType, int page, int pageSize)
        {
            var coupon = DbFactory.Default
                .Get<CouponInfo>();

            if (!string.IsNullOrEmpty(text))
                coupon.Where(d => d.CouponName.Contains(text));

            if (endDate > DateTime.Parse("2000-01-01"))
                coupon.Where(d => d.EndTime > endDate);
            else
                coupon.Where(d => d.EndTime > DateTime.Now);//查询未结束

            if (ReceiveType >= 0)
                coupon.Where(d => d.ReceiveType == (CouponInfo.CouponReceiveType)ReceiveType);

            coupon.LeftJoin<CouponRecordInfo>((ci, cri) => ci.Id == cri.CouponId)
            .GroupBy(n => n.Id)
            .Having<CouponRecordInfo>((ci, cri) => ci.Num > cri.Id.ExCount(true));

            //coupon.Where(d => d.Num > cr.ExResolve<int>()); //d.Mall_CouponRecord.Count());//查询库存剩余


            var ret = coupon.OrderByDescending(o => o.EndTime).ToPagedList(page, pageSize);
            QueryPageModel<CouponInfo> pageModel = new QueryPageModel<CouponInfo>() { Models = ret, Total = ret.TotalRecordCount };
            return pageModel;
        }


        /// <summary>
        /// 获取指定优惠券会员领取情况统计
        /// </summary>
        /// <param name="couponIds">优惠券ID数组</param>
        /// <returns></returns>
        public List<CouponRecordInfo> GetCouponRecordTotal(long[] couponIds)
        {
            var model = DbFactory.Default.Get<CouponRecordInfo>().Where(d => d.CouponId.ExIn(couponIds)).ToList();
            return model;
        }
        /// <summary>
        /// 获取能够使用的优惠券数量[排除过期、已领完]
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public int GetUserCouponCount(long shopId)
        {
            //parms.Add("@ShopId", shopId);
            //parms.Add("@PlatForm", Core.PlatformType.Wap);
            //var countsql = new Sql();
            //countsql.Append(" select count(1) from ( ");
            //countsql.Append(" select A.Id,A.ShopId,A.CouponName,A.Num, ");
            //countsql.Append(" (select count(1) from Mall_CouponRecord where CouponID=A.Id)AS hasNum from Mall_Coupon A ");
            //countsql.Append(" left join Mall_CouponSetting S on A.Id=S.CouponID  ");
            //countsql.Append(" where A.ShopId=@0 and A.EndTime>CURDATE() and S.PlatForm=@1 and (S.Display=1 OR S.Display is NULL) ", shopId, PlatformType.Wap.GetHashCode());
            //countsql.Append(" order by A.Id desc )T where T.hasNum<T.Num ");/*此SQL还需优化*/
            //return DbFactory.Default.ExecuteScalar<int>(countsql);

            var crisql = DbFactory.Default
                .Get<CouponRecordInfo>()
                .Where<CouponRecordInfo, CouponInfo>((cri, ci) => ci.Id == cri.CouponId)
                .Select(n => n.Id.ExCount(false));

            return DbFactory.Default
                .Get<CouponInfo>()
                .LeftJoin<CouponSettingInfo>((ci, csi) => ci.Id == csi.CouponID)
                .Select(n => n.Id)
                .Where(n => n.ShopId == shopId && n.EndTime > DateTime.Now.Date)
                .Where<CouponSettingInfo>(n => n.PlatForm == PlatformType.Wap && n.Display.ExIfNull(1) == 1)
                .Where(n => crisql.ExResolve<int>() < n.Num)
                .OrderByDescending(n => n.Id)
                .Count();
        }

        public List<CouponProductInfo> GetCouponProductsByCouponId(long couponId)
        {
            return DbFactory.Default.Get<CouponProductInfo>().Where(p => p.CouponId == couponId).ToList();
        }

        /// <summary>
        /// 批量修改优惠券商铺名称
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopName"></param>
        public void SetCouponShopName(long shopId, string shopName)
        {
            DbFactory.Default.Set<CouponInfo>().Where(p => p.ShopId == shopId).Set(p => p.ShopName, shopName).Succeed();
        }

        /// <summary>
        /// 根据发送消息Id获取优惠券列表
        /// </summary>
        /// <param name="messageId">发送消息记录Id</param>
        /// <returns></returns>
        public List<CouponInfo> GetCouponBySendmessagerecordId(long messageId)
        {
            //优惠券编码
            var couponSNlist = DbFactory.Default.Get<SendmessagerecordCouponSNInfo>().Where(p => p.MessageId == messageId).Select(p => p.CouponSN);
            if (couponSNlist == null || couponSNlist.Count() <= 0)
                return null;

            //优惠券编码
            var couponIdList = DbFactory.Default.Get<CouponRecordInfo>().Where(p => p.CounponSN.ExIn(couponSNlist)).Select(p => p.CouponId);
            
            List<CouponInfo> result = DbFactory.Default.Get<CouponInfo>().Where(a => a.Id.ExIn(couponIdList)).ToList();
            return result;
        }
    }
}
