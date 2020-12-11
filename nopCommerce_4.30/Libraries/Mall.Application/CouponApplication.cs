using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{
    /// <summary>
    /// 优惠券业务实现
    /// </summary>
    public class CouponApplication:BaseApplicaion<ICouponService>
    {
        /// <summary>
        /// 优惠券设置
        /// </summary>
        /// <param name="mCouponRegister"></param>
        public static void SetCouponSendByRegister(CouponSendByRegisterModel model)
        {
            var detail = new List<CouponSendByRegisterDetailedInfo>();
            foreach (var item in model.CouponIds)
            {
                detail.Add(new CouponSendByRegisterDetailedInfo() { CouponId = item.Id });
            }
            var info = new CouponSendByRegisterInfo
            {
                Id = model.Id,
                Link = model.Link,
                Status = model.Status,
            };
            var service = GetService<ICouponSendByRegisterService>();
            if (model.Id <= 0)
                service.AddCouponSendByRegister(info, detail);
            else
                service.UpdateCouponSendByRegister(info, detail);
        }

        /// <summary>
        /// 获取优惠券设置
        /// </summary>
        /// <returns></returns>
        public static CouponSendByRegisterModel GetCouponSendByRegister()
        {
            var vModel = new CouponSendByRegisterModel();
            var model = GetService<ICouponSendByRegisterService>().GetCouponSendByRegister();
            if (model != null)
            {
                vModel.Id = model.Id;
                vModel.Link = model.Link;
                vModel.Status = model.Status;

                int total = 0;
                decimal price = 0;
                var lmCoupon = new List<CouponModel>();
                var details = Service.GetCouponSendByRegisterDetailedInfo(model.Id);
                var coupons = Service.GetCoupons(details.Select(p => p.CouponId).ToList());
                var records = Service.GetRecordCountByCoupon(coupons.Select(p => p.Id).ToList());
                foreach (var item in details)
                {
                    var coupon = coupons.FirstOrDefault(p => p.Id == item.CouponId);
                    var record = records.ContainsKey(coupon.Id) ? records[coupon.Id] : 0;
                    int inventory = coupon.Num - record;//优惠券剩余量
                    if (inventory > 0 && coupon.EndTime > DateTime.Now)
                    {
                        total += inventory;
                        price += coupon.Price;
                        lmCoupon.Add(new CouponModel
                        {
                            Id = item.CouponId,
                            CouponName = coupon.CouponName,
                            inventory = inventory,
                            Num = coupon.Num,
                            useNum = record,
                            Price = coupon.Price,
                            ShopId = coupon.ShopId,
                            ShopName = coupon.ShopName,
                            EndTime = coupon.EndTime,
                            StartTime = coupon.StartTime,
                            OrderAmount = coupon.OrderAmount == 0 ? "不限制" : "满" + coupon.OrderAmount
                        });
                    }
                }
                vModel.CouponIds = lmCoupon;
                vModel.total = total;
                vModel.price = price;
                if (vModel.CouponIds.Count.Equals(0))
                    vModel.Status = CouponSendByRegisterStatus.Shut;
            }
            return vModel;
        }

        /// <summary>
        /// 注册成功赠送优惠券
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <param name="userName">会员登录名</param>
        /// <returns>返回赠送张数</returns>
        public static int RegisterSendCoupon(long id, string userName)
        {
            int result = 0;
            var model = GetCouponSendByRegister();
            if (model != null && model.Status.Equals(Mall.CommonModel.CouponSendByRegisterStatus.Open) && model.total > 0)//如果活动开启，且优惠券有剩余
            {
                foreach (var item in model.CouponIds)
                {
                    if (item.inventory > 0)
                    {
                        CouponRecordInfo info = new CouponRecordInfo();
                        info.UserId = id;
                        info.UserName = userName;
                        info.ShopId = item.ShopId;
                        info.CouponId = item.Id;
                        Service.AddCouponRecord(info);
                        result++;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 发送优惠券
        /// </summary>
        /// <param name="labelids">发送标签</param>
        /// <param name="labelinfos">标签名称</param>
        /// <param name="couponIds">优惠券名称</param>
        /// <returns>-1:优惠券不足;-2:请选择发送的优惠券;-3:标签中用户数为0</returns>
        public static string SendCouponMsg(string labelids, string labelinfos, string couponIds, string url)
        {

            var messageEmali = PluginsManagement.GetPlugin<IMessagePlugin>("Mall.Plugin.Message.Email");
            var messageSMS = PluginsManagement.GetPlugin<IMessagePlugin>("Mall.Plugin.Message.SMS");
            string result = "";
            if (!couponIds.TrimEnd(',').Equals(""))
            {
                //取出标签对应的会员信息
                long[] lids = string.IsNullOrWhiteSpace(labelids) ? null : labelids.Split(',').Select(s => long.Parse(s)).ToArray();
                int pageNo = 1, pageSize = 100;
                var pageMode = MemberApplication.GetMembers(new MemberQuery
                {
                    Labels = lids,
                    PageNo = pageNo,
                    PageSize = pageSize
                });
                if (pageMode.Total > 0)
                {
                    List<Mall.Entities.MemberInfo> mUserMember = new List<Mall.Entities.MemberInfo>();
                    while (pageMode.Models.Count() > 0)//循环批量获取用户信息
                    {
                        string[] dests = pageMode.Models.Select(e => e.Email).ToArray();
                        foreach (var item in pageMode.Models)
                        {
                            mUserMember.Add(item);
                        }
                        pageNo += 1;
                        pageMode = MemberApplication.GetMembers(new MemberQuery
                        {
                            Labels = lids,
                            PageNo = pageNo,
                            PageSize = pageSize
                        });
                    }

                    string[] arrStr = couponIds.TrimEnd(',').Split(',');
                    long[] arrcouponIds = arrStr.Select(a => long.Parse(a)).ToArray();

                    var model = Service.GetCouponInfo(arrcouponIds);//获取所选优惠券集合

                    //查询优惠券领取状况
                    var mCouponRecord = Service.GetCouponRecordTotal(arrcouponIds);

                    decimal price = 0;
                    List<SendmessagerecordCouponInfo> lsendInfo = new List<SendmessagerecordCouponInfo>();
                    List<SendmessagerecordCouponSNInfo> lsendSN = new List<SendmessagerecordCouponSNInfo>();
                    var records = Service.GetRecordCountByCoupon(model.Select(p => p.Id).ToList());
                    //验证优惠券是否充足
                    foreach (var item in model)
                    {
                        price += item.Price;
                        lsendInfo.Add(new SendmessagerecordCouponInfo() { CouponId = item.Id });
                        var record = records.ContainsKey(item.Id) ? records[item.Id] : 0;
                        if (item.Num - record < mUserMember.Count)
                        {
                            result = item.CouponName + "优惠券的数量不足，无法赠送";
                            break;
                        }
                    }
                    var siteName = SiteSettingApplication.SiteSettings.SiteName;
                    if (result == "")
                    {
                        //发送优惠券
                        bool alTotal = false;
                        for (int i = 0; i < mUserMember.Count; i++)
                        {
                            bool suTotal = false;//会员发送优惠券成功数
                            foreach (var item in model)
                            {
                                //判断会员领取限制，是否可领取此优惠券
                                bool isf = true;
                                if (item.PerMax > 0)
                                {
                                    int total = mCouponRecord.Where(p => p.UserId == mUserMember[i].Id && p.CouponId == item.Id).ToList().Count;
                                    if (item.PerMax <= total)
                                    {
                                        isf = false;
                                    }
                                }

                                if (isf)
                                {
                                    suTotal = true;
                                    alTotal = true;

                                    CouponRecordInfo info = new CouponRecordInfo();
                                    info.UserId = mUserMember[i].Id;
                                    info.UserName = mUserMember[i].UserName;
                                    info.ShopId = item.ShopId;
                                    info.CouponId = item.Id;
                                    var couponRecord = Service.AddCouponRecord(info);
                                    lsendSN.Add(new SendmessagerecordCouponSNInfo() { CouponSN = couponRecord.CounponSN });
                                }
                            }

                            if (suTotal)
                            {
                                MessageCouponInfo info = new MessageCouponInfo();
                                info.Money = price;
                                info.SiteName = siteName;
                                info.UserName = mUserMember[i].UserName;
                                MessageApplication.SendMessageOnCouponSuccess(mUserMember[i].Id, info);
                            }
                        }

                        Log.Debug("sendCoupon:" + alTotal);
                        //查看成功发送会员数
                        if (alTotal)
                        {
                            //记录发送历史
                            var sendRecord = new SendMessageRecordInfo
                            {
                                ContentType = WXMsgType.wxcard,
                                MessageType = MsgType.Coupon,
                                SendContent = "",
                                SendState = 1,
                                SendTime = DateTime.Now,
                                ToUserLabel = labelinfos ?? "",
                            };
                            WXMsgTemplateApplication.AddSendRecord(sendRecord, lsendInfo);
                            foreach (var item in lsendSN)
                            {
                                item.MessageId = sendRecord.Id;
                            }
                            Service.AddSendmessagerecordCouponSN(lsendSN);
                        }
                        else
                        {
                            result = "此标签下无符合领取此优惠券的会员";
                        }
                    }
                }
                else
                    result = "该标签下无任何会员";
            }
            else
                result = "请选择发送的优惠券";
            return result;
        }

        /// <summary>
        /// 发送优惠券，根据会员ID
        /// </summary>
        /// <param name="userIds">发送对象</param>
        /// <param name="couponIds">优惠券名称</param>
        public static void SendCouponByUserIds(List<long> userIds, IEnumerable<long> couponIds)
        {
            var model = Service.GetCouponInfo(couponIds.ToArray());
            var siteName = SiteSettingApplication.SiteSettings.SiteName;
            var mCouponRecord = Service.GetCouponRecordTotal(couponIds.ToArray());
            var mUserMember = MemberApplication.GetMembers(userIds);
            decimal price = 0;
            string result = "";
            List<SendmessagerecordCouponInfo> lsendInfo = new List<SendmessagerecordCouponInfo>();
            var records = Service.GetRecordCountByCoupon(model.Select(p => p.Id).ToList());
            //验证优惠券是否充足
            foreach (var item in model)
            {
                price += item.Price;
                lsendInfo.Add(new SendmessagerecordCouponInfo() { CouponId = item.Id });
                var record = records.ContainsKey(item.Id) ? records[item.Id] : 0;
                if (item.Num - record < mUserMember.Count)
                {
                    result = item.CouponName + "优惠券的数量不足，无法赠送";
                    break;
                }
            }
            if (result == "")
            {
                //发送优惠券
                bool alTotal = false;
                for (int i = 0; i < mUserMember.Count; i++)
                {
                    bool suTotal = false;//会员发送优惠券成功数
                    foreach (var item in model)
                    {
                        //判断会员领取限制，是否可领取此优惠券
                        bool isf = true;
                        if (item.PerMax > 0)
                        {
                            int total = mCouponRecord.Where(p => p.UserId == mUserMember[i].Id && p.CouponId == item.Id).ToList().Count;
                            if (item.PerMax <= total)
                            {
                                isf = false;
                            }
                        }

                        if (isf)
                        {
                            suTotal = true;
                            alTotal = true;

                            CouponRecordInfo info = new CouponRecordInfo();
                            info.UserId = mUserMember[i].Id;
                            info.UserName = mUserMember[i].UserName;
                            info.ShopId = item.ShopId;
                            info.CouponId = item.Id;
                            Service.AddCouponRecord(info);
                        }
                    }

                    if (suTotal)
                    {
                        MessageCouponInfo info = new MessageCouponInfo();
                        info.Money = price;
                        info.SiteName = siteName;
                        info.UserName = mUserMember[i].UserName;
                        MessageApplication.SendMessageOnCouponSuccess(mUserMember[i].Id, info);
                    }
                }

                Log.Debug("sendCoupon:" + alTotal);
                //查看成功发送会员数
                if (alTotal)
                {
                    //记录发送历史
                    var sendRecord = new SendMessageRecordInfo
                    {
                        ContentType = WXMsgType.wxcard,
                        MessageType = MsgType.Coupon,
                        SendContent = "",
                        SendState = 1,
                        SendTime = DateTime.Now,
                        ToUserLabel = "",
                    };
                    WXMsgTemplateApplication.AddSendRecord(sendRecord, lsendInfo);
                }
                else
                {
                    result = "无符合领取此优惠券的会员";
                }
            }
            else
                result = "该标签下无任何会员";
            if (!string.IsNullOrWhiteSpace(result))
            {
                throw new MallException(result);
            }
        }

        /// <summary>
        /// 发送优惠券，根据搜索条件
        /// </summary>
        /// <param name="query"></param>
        /// <param name="couponIds"></param>
        public static void SendCoupon(MemberPowerQuery query, IEnumerable<long> couponIds, MemberQuery memberQuery = null, string labelinfos = "")
        {
            var siteName = SiteSettingApplication.SiteSettings.SiteName;
            decimal price = 0;
            string result = "";
            //会员领取优惠券记录ID
            //   List<long> memberCouponIds = new List<long>();
            // dictResult = new Dictionary<string, int>();  

            var isMember = memberQuery != null;//是否为会员管理发送
            var mUserMember = new List<MemberPurchasingPower>();
            var mUserMemberList = new List<Members>();
            if (isMember)
            {
                memberQuery.PageSize = 500;
                memberQuery.PageNo = 1;
                var pageMode = MemberApplication.GetMemberList(memberQuery);
                if (pageMode.Total > 0)
                {
                    while (pageMode.Models.Count() > 0)//循环批量获取用户信息
                    {
                        //   string[] dests = pageMode.Models.Select(e => e.).ToArray();
                        foreach (var item in pageMode.Models)
                        {
                            mUserMemberList.Add(item);
                        }
                        memberQuery.PageNo += 1;
                        pageMode = MemberApplication.GetMemberList(memberQuery);
                    }
                }
            }
            else
            {
                query.PageSize = 500;
                query.PageNo = 1;
                var pageMode = MemberApplication.GetPurchasingPowerMember(query);
                if (pageMode.Total > 0)
                {
                    while (pageMode.Models.Count() > 0)//循环批量获取用户信息
                    {
                        //   string[] dests = pageMode.Models.Select(e => e.).ToArray();
                        foreach (var item in pageMode.Models)
                        {
                            mUserMember.Add(item);
                        }
                        query.PageNo += 1;
                        pageMode = MemberApplication.GetPurchasingPowerMember(query);
                    }
                }
            }

            var isTrue = isMember ? mUserMemberList.Any() : mUserMember.Any();
            if (isTrue)
            {
                var model = Service.GetCouponInfo(couponIds.ToArray());//获取所选优惠券集合

                //查询优惠券领取状况
                var mCouponRecord = Service.GetCouponRecordTotal(couponIds.ToArray());

                List<SendmessagerecordCouponInfo> lsendInfo = new List<SendmessagerecordCouponInfo>();
                var records = Service.GetRecordCountByCoupon(model.Select(p => p.Id).ToList());
                //验证优惠券是否充足
                foreach (var item in model)
                {
                    price += item.Price;
                    lsendInfo.Add(new SendmessagerecordCouponInfo() { CouponId = item.Id });
                    var record = records.ContainsKey(item.Id) ? records[item.Id] : 0;
                    var count = isMember ? mUserMemberList.Count : mUserMember.Count;
                    if (item.Num - record < count)
                    {
                        result = item.CouponName + "优惠券的数量不足，无法赠送";
                        break;
                    }
                }
                if (result == "")
                {
                    //发送优惠券
                    bool alTotal = false;
                    var count = isMember ? mUserMemberList.Count : mUserMember.Count;
                    for (int i = 0; i < count; i++)
                    {
                        bool suTotal = false;//会员发送优惠券成功数
                        foreach (var item in model)
                        {
                            //判断会员领取限制，是否可领取此优惠券
                            bool isf = true;
                            if (item.PerMax > 0)
                            {
                                int total = mCouponRecord.Where(p => p.UserId == (isMember ? mUserMemberList[i].Id : mUserMember[i].Id) && p.CouponId == item.Id).ToList().Count;
                                if (item.PerMax <= total)
                                {
                                    isf = false;
                                }
                            }

                            if (isf)
                            {
                                suTotal = true;
                                alTotal = true;

                                CouponRecordInfo info = new CouponRecordInfo();
                                info.UserId = isMember ? mUserMemberList[i].Id : mUserMember[i].Id;
                                info.UserName = isMember ? mUserMemberList[i].UserName : mUserMember[i].UserName;
                                info.ShopId = item.ShopId;
                                info.CouponId = item.Id;
                                Service.AddCouponRecord(info);
                            }
                        }

                        if (suTotal)
                        {
                            MessageCouponInfo info = new MessageCouponInfo();
                            info.Money = price;
                            info.SiteName = siteName;
                            info.UserName = isMember ? mUserMemberList[i].UserName : mUserMember[i].UserName;
                            MessageApplication.SendMessageOnCouponSuccess((isMember ? mUserMemberList[i].Id : mUserMember[i].Id), info);
                        }
                    }

                    Log.Debug("sendCoupon:" + alTotal);
                    //查看成功发送会员数
                    if (alTotal)
                    {
                        //记录发送历史
                        var sendRecord = new SendMessageRecordInfo
                        {
                            ContentType = WXMsgType.wxcard,
                            MessageType = MsgType.Coupon,
                            SendContent = "",
                            SendState = 1,
                            SendTime = DateTime.Now,
                            ToUserLabel = labelinfos ?? "",
                        };
                        WXMsgTemplateApplication.AddSendRecord(sendRecord, lsendInfo);
                    }
                    else
                    {
                        result = "此标签下无符合领取此优惠券的会员";
                    }
                }
            }
            else
                result = "该标签下无任何会员";            

            if (!string.IsNullOrWhiteSpace(result))
            {
                throw new MallException(result);
            }
        }

        public static QueryPageModel<CouponModel> GetCouponByName(string text, DateTime endDate, int ReceiveType, int page, int pageSize)
        {
            var couponList = Service.GetCouponByName(text, endDate, ReceiveType, page, pageSize);
            var pageModel = new QueryPageModel<CouponModel>();

            var lmCoupon = new List<CouponModel>();
            var records = Service.GetRecordCountByCoupon(couponList.Models.Select(p => p.Id).ToList());
            foreach (var item in couponList.Models)
            {
                if (item.IsSyncWeiXin == 0 || (item.IsSyncWeiXin == 1 && item.WXAuditStatus == (int)WXCardLogInfo.AuditStatusEnum.Audited))
                {
                    var record = records.ContainsKey(item.Id) ? records[item.Id] : 0;
                    CouponModel couponModel = new CouponModel();
                    couponModel.CouponName = item.CouponName;
                    couponModel.Id = item.Id;
                    couponModel.Num = item.Num;
                    couponModel.useNum = record;
                    couponModel.inventory = item.Num - record;
                    couponModel.OrderAmount = item.OrderAmount == 0 ? "不限制" : "满" + item.OrderAmount;
                    couponModel.Price = item.Price;
                    couponModel.ShopName = item.ShopName;
                    couponModel.EndTime = item.EndTime;
                    couponModel.StartTime = item.StartTime;
                    couponModel.perMax = item.PerMax;
                    couponModel.UseArea = item.UseArea;
                    couponModel.Remark = item.Remark;
                    lmCoupon.Add(couponModel);
                }
            }
            pageModel.Models = lmCoupon;
            pageModel.Total = couponList.Total;
            return pageModel;
        }
        public static CouponModel Get(long id)
        {
            var couponList = Service.GetCouponInfo(id);
            if (couponList == null)
                return null;
            var record = Service.GetRecordCountByCoupon(couponList.Id);
            var lmCoupon = new Mall.DTO.CouponModel();
            lmCoupon.CouponName = couponList.CouponName;
            lmCoupon.Id = couponList.Id;
            lmCoupon.Num = couponList.Num;
            lmCoupon.useNum = record;
            lmCoupon.inventory = couponList.Num - record;
            lmCoupon.OrderAmount = couponList.OrderAmount == 0 ? "不限制" : "满" + couponList.OrderAmount;
            lmCoupon.Price = couponList.Id;
            lmCoupon.ShopName = couponList.ShopName;
            lmCoupon.EndTime = couponList.EndTime;
            lmCoupon.StartTime = couponList.StartTime;
            lmCoupon.perMax = couponList.PerMax;
            lmCoupon.UseArea = couponList.UseArea;
            lmCoupon.Remark = couponList.Remark;
            return lmCoupon;
        }
        /// <summary>
        /// 商家添加一个优惠券
        /// </summary>
        /// <param name="info"></param>
        public static void AddCoupon(CouponInfo info)
        {
            Service.AddCoupon(info);
        }

        //使优惠券失效
        public static void CancelCoupon(long couponId, long shopId)
        {
            Service.CancelCoupon(couponId, shopId);
        }
        /// <summary>
        /// 商家修改一个优惠券
        /// </summary>
        /// <param name="info"></param>
        public static void EditCoupon(CouponInfo info)
        {
            Service.EditCoupon(info);
        }

        /// <summary>
        /// 领取一个优惠券
        /// </summary>
        /// <param name="info"></param>
        public static void AddCouponRecord(CouponRecordInfo info)
        {
            Service.AddCouponRecord(info);
        }

        //使用优惠券
        public static void UseCoupon(long userId, IEnumerable<long> Ids, IEnumerable<OrderInfo> orders)
        {
            Service.UseCoupon(userId, Ids, orders);
        }

        /// <summary>
        /// 获取店铺订购的优惠券信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static ActiveMarketServiceInfo GetCouponService(long shopId)
        {
            return Service.GetCouponService(shopId);
        }

        /// <summary>
        /// 获取商家添加的优惠券列表
        /// </summary>
        /// <returns></returns>
        public static QueryPageModel<CouponInfo> GetCouponList(CouponQuery query)
        {
            return Service.GetCouponList(query);
        }

        /// <summary>
        /// 获取商家添加的优惠券列表(全部)
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static List<CouponInfo> GetCouponList(long shopid)
        {
            return Service.GetCouponList(shopid);
        }

        /// <summary>
        /// 获取商家添加的优惠券列表(排除失效)
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static List<CouponInfo> GetCouponLists(long shopid)
        {
            return Service.GetCouponLists(shopid);
        }
        /// <summary>
        /// 获取领取的优惠券列表
        /// </summary>
        /// <returns></returns>
        public static QueryPageModel<CouponRecordInfo> GetCouponRecordList(CouponRecordQuery query)
        {
            return Service.GetCouponRecordList(query);
        }
        /// <summary>
        /// 获取已邻取优惠券信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CouponRecordInfo GetCouponRecordById(long id)
        {
            return Service.GetCouponRecordById(id);
        }

        /// <summary>
        /// 获取优惠券信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public static CouponInfo GetCouponInfo(long shopId, long couponId)
        {
            return Service.GetCouponInfo(shopId, couponId);
        }
        /// <summary>
        /// 获取优惠券信息（couponid）
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public static CouponInfo GetCouponInfo(long couponId)
        {
            return Service.GetCouponInfo(couponId);
        }

        /// <summary>
        /// 批量获取优惠券信息（couponIds）
        /// </summary>
        /// <param name="couponIds">优惠券数组</param>
        /// <returns></returns>
        public static List<CouponInfo> GetCouponInfo(IEnumerable<long> couponIds)
        {
            return Service.GetCouponInfo(couponIds.ToArray());
        }
        /// <summary>
        /// 获取已使用的的某一个优惠券的详细
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static CouponRecordInfo GetCouponRecordInfo(long userId, long orderId)
        {
            return Service.GetCouponRecordInfo(userId, orderId);
        }

        /// <summary>
        /// 获取可用优惠券
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static IEnumerable<CouponInfo> GetTopCoupon(long shopId, int top = 5, PlatformType type = Core.PlatformType.PC)
        {
            return Service.GetTopCoupon(shopId, top, type);
        }

        /// <summary>
        /// 获取用户领取的某个优惠券的数量
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static int GetUserReceiveCoupon(long couponId, long userId)
        {
            return Service.GetUserReceiveCoupon(couponId, userId);
        }

        /// <summary>
        /// 获取一个用户在某个店铺的可用优惠券
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="userId"></param>
        /// <param name="totalPrice">总金额</param>
        /// <returns></returns>
        public static List<CouponRecordInfo> GetUserCoupon(long shopId, long userId, decimal totalPrice)
        {
            return Service.GetUserCoupon(shopId, userId, totalPrice);
        }

        ///获取用户将要使用的优惠券列表
        ///
        public static IEnumerable<CouponRecordInfo> GetOrderCoupons(long userId, IEnumerable<long> Ids)
        {
            return Service.GetOrderCoupons(userId, Ids);
        }
        /// <summary>
        /// 取用户领取的所有优惠卷信息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static IEnumerable<UserCouponInfo> GetUserCouponList(long userid)
        {
            return Service.GetUserCouponList(userid);
        }

        public static List<UserCouponInfo> GetAllUserCoupon(long userid)
        {
            return Service.GetAllUserCoupon(userid);
        }
        /// <summary>
        /// 是否可以添加积分兑换红包
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static bool CanAddIntegralCoupon(long shopid, long id = 0)
        {
            return Service.CanAddIntegralCoupon(shopid, id);

        }
        /// <summary>
        /// 取积分优惠券列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static QueryPageModel<CouponInfo> GetIntegralCoupons(int page, int pageSize)
        {
            return Service.GetIntegralCoupons(page, pageSize);
        }
        /// <summary>
        /// 同步微信卡券审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cardid"></param>
        /// <param name="auditstatus">审核状态</param>
        public static void SyncWeixinCardAudit(long id, string cardid, WXCardLogInfo.AuditStatusEnum auditstatus)
        {
            Service.SyncWeixinCardAudit(id, cardid, auditstatus);
        }
        /// <summary>
        /// 处理错误的卡券同步信息
        /// </summary>
        public static void ClearErrorWeiXinCardSync()
        {

            Service.ClearErrorWeiXinCardSync();
        }

        /// <summary>
        /// 获取指定优惠券会员领取情况统计
        /// </summary>
        /// <param name="couponIds">优惠券ID数组</param>
        /// <returns></returns>
        public static List<CouponRecordInfo> GetCouponRecordTotal(long[] couponIds)
        {
            return Service.GetCouponRecordTotal(couponIds);
        }
        public static List<CouponProductInfo> GetCouponProductsByCouponId(long couponId)
        {
            return Service.GetCouponProductsByCouponId(couponId);
        }

        /// <summary>
        /// 会员领取优惠券的前置状态判断
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="vshopId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public static int GetReceiveStatus(long userid, long vshopId, long couponId)
        {
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var couponInfo = couponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效
            if (userid > 0)
            {
                CouponRecordQuery crQuery = new CouponRecordQuery();
                crQuery.CouponId = couponId;
                crQuery.UserId = userid;
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
                    var integral = MemberIntegralApplication.GetAvailableIntegral(userid);
                    if (integral < couponInfo.NeedIntegral) return 5;//积分不足
                }
            }
            return 1;//可正常领取
        }

        /// <summary>
        /// 将前端传入参数转换成适合操作的格式
        /// </summary>
        public static IEnumerable<string[]> ConvertUsedCoupon(string couponIds)
        {
            //couponIds格式  "id_type,id_type,id_type"
            IEnumerable<string> couponArr = null;
            if (!string.IsNullOrEmpty(couponIds))
            {
                couponArr = couponIds.Split(',');
            }

            //返回格式  string[0] = id , string[1] = type
            return couponArr == null ? null : couponArr.Select(p => p.Split('_'));
        }
    }
}