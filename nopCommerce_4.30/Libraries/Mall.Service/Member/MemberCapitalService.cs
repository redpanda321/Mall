using System;
using System.Collections.Generic;
using System.Text;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.CommonModel;
using Mall.DTO;
using Mall.Entities;
using NetRube.Data;
using Mall.Core.Plugins.Message;
using Mall.Application;
using System.Threading.Tasks;
using Mall.Core;

namespace Mall.Service
{
    public class MemberCapitalService : ServiceBase, IMemberCapitalService
    {

        public QueryPageModel<MemberCapital> GetCapitals(CapitalQuery query)
        {
            var db = DbFactory.Default.Get<MemberInfo>().LeftJoin<CapitalInfo>((mi, ci) => ci.MemId == mi.Id);
            if (query.MemberId > 0)
                db.Where(e => e.Id == query.MemberId);
            if (!string.IsNullOrEmpty(query.UserName))
            {
                //var members = DbFactory.Default.Get<MemberInfo>().Where(p => p.UserName.Contains(query.UserName)).Select(p => p.Id).ToList<long>();
                //db.Where(p => p.MemId.ExIn(members));
                db.Where(p => p.UserName.Contains(query.UserName));
            }
            switch (query.Sort.ToLower())
            {
                case "balance":
                    if (query.IsAsc) db.OrderBy<CapitalInfo>(p => p.Balance);
                    else db.OrderByDescending<CapitalInfo>(p => p.Balance);
                    break;
                case "freezeamount":
                    if (query.IsAsc) db.OrderBy<CapitalInfo>(p => p.FreezeAmount);
                    else db.OrderByDescending<CapitalInfo>(p => p.FreezeAmount);
                    break;
                case "chargeamount":
                    if (query.IsAsc) db.OrderBy<CapitalInfo>(p => p.ChargeAmount);
                    else db.OrderByDescending<CapitalInfo>(p => p.ChargeAmount);
                    break;
                case "presentamount":
                    if (query.IsAsc) db.OrderBy<CapitalInfo>(p => p.PresentAmount);
                    else db.OrderByDescending<CapitalInfo>(p => p.PresentAmount);
                    break;
                default:
                    db.OrderByDescending<CapitalInfo>(e => e.Balance);
                    break;
            }
            db.Select<CapitalInfo>(p => new { Id = p.Id, Balance = p.Balance.ExIfNull(0), ChargeAmount = p.ChargeAmount.ExIfNull(0), FreezeAmount = p.FreezeAmount.ExIfNull(0), PresentAmount = p.PresentAmount.ExIfNull(0) });
            db.Select(p => new { UserName = p.UserName, RealName = p.RealName, UserId = p.Id });
            var page = db.ToPagedList<MemberCapital>(query.PageNo, query.PageSize);
            return new QueryPageModel<MemberCapital>
            {
                Models = page,
                Total = page.TotalRecordCount
            };
        }

        private void InitCapital(long memberId)
        {
            CapitalInfo newCapital = new CapitalInfo()
            {
                Balance = 0,
                ChargeAmount = 0,
                MemId = memberId,
                FreezeAmount = 0
            };
            DbFactory.Default.Add(newCapital);
        }

        public QueryPageModel<CapitalDetailInfo> GetCapitalDetails(CapitalDetailQuery query)
        {
            //var capitalDetail = Context.CapitalDetailInfo.Where(item => item.Mall_Capital.MemId == query.memberId);

            //var capital = DbFactory.Default.Get<CapitalInfo>().Where(p => p.MemId == query.memberId).Select(p => p.Id).ToList<long>();
            var capitalDetail = DbFactory.Default
                .Get<CapitalDetailInfo>()
                .InnerJoin<CapitalInfo>((cdi, ci) => cdi.CapitalID == ci.Id && ci.MemId == query.memberId);
            if (query.capitalType.HasValue && query.capitalType.Value != 0)
            {
                capitalDetail.Where(e => e.SourceType == query.capitalType.Value);
            }
            if (query.startTime.HasValue)
            {
                capitalDetail.Where(e => e.CreateTime >= query.startTime);
            }
            if (query.endTime.HasValue)
            {
                capitalDetail.Where(e => e.CreateTime < query.endTime);
            }
            //int total = 0;
            //var model = capitalDetail.GetPage(p => p.OrderByDescending(e => e.CreateTime), out total, query.PageNo, query.PageSize);
            var model = capitalDetail.OrderByDescending(e => e.CreateTime).Select().ToPagedList(query.PageNo, query.PageSize);

            QueryPageModel<CapitalDetailInfo> result = new QueryPageModel<CapitalDetailInfo> { Models = model, Total = model.TotalRecordCount };
            return result;
        }

        public QueryPageModel<ApplyWithdrawInfo> GetApplyWithDraw(ApplyWithDrawQuery query)
        {
            var db = WhereBuilder(query);
            if (string.IsNullOrWhiteSpace(query.Sort))
                db.OrderBy(e => e.ApplyStatus).OrderByDescending(e => e.ApplyTime);
            else
                db.OrderByDescending(e => e.ApplyTime);

            var model = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<ApplyWithdrawInfo> { Models = model, Total = model.TotalRecordCount };
        }

        public int GetApplyWithDrawCount(ApplyWithDrawQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        private GetBuilder<ApplyWithdrawInfo> WhereBuilder(ApplyWithDrawQuery query)
        {
            var db = DbFactory.Default.Get<ApplyWithdrawInfo>();

            if (query.MemberId.HasValue)
                db.Where(e => e.MemId == query.MemberId);

            if (query.WithDrawNo.HasValue)
                db.Where(e => e.Id == query.WithDrawNo);

            if (query.withDrawStatus.HasValue && query.withDrawStatus.Value > 0)
                db.Where(e => e.ApplyStatus == query.withDrawStatus.Value);

            if (query.ApplyType.HasValue)
                db.Where(e => e.ApplyType == query.ApplyType.Value);

            return db;
        }
        /// <summary>
        /// 获取提现记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ApplyWithdrawInfo GetApplyWithDrawInfo(long Id)
        {
            return DbFactory.Default.Get<ApplyWithdrawInfo>().Where(r => r.Id == Id).FirstOrDefault();
        }
        public List<ApplyWithdrawInfo> GetApplyWithDrawInfoByIds(IEnumerable<long> Ids)
        {
            return DbFactory.Default.Get<ApplyWithdrawInfo>().Where(e => e.Id.ExIn(Ids)).ToList();
        }
        public void ConfirmApplyWithDraw(ApplyWithdrawInfo info)
        {
            var model = DbFactory.Default.Get<ApplyWithdrawInfo>().Where(e => e.Id == info.Id).FirstOrDefault();

            var flag = DbFactory.Default.InTransaction(() =>
            {
                model.ApplyStatus = info.ApplyStatus;
                model.OpUser = info.OpUser;
                model.Remark = info.Remark;
                model.ConfirmTime = info.ConfirmTime.HasValue ? info.ConfirmTime.Value : DateTime.Now;
                //Context.SaveChanges();
                DbFactory.Default.Update(model);
                if (info.ApplyStatus == ApplyWithdrawInfo.ApplyWithdrawStatus.WithDrawSuccess)
                {
                    //model.PayNo = info.PayNo;
                    //model.PayTime = info.PayTime.HasValue ? info.PayTime.Value : DateTime.Now;
                    //DbFactory.Default.Update(model);
                    DbFactory.Default.Set<ApplyWithdrawInfo>().Set(n => n.PayNo, info.PayNo).Set(n => n.PayTime, info.PayTime.HasValue ? info.PayTime.Value : DateTime.Now).Where(p => p.Id == model.Id).Succeed();
                    CapitalDetailModel capitalDetail = new CapitalDetailModel
                    {
                        Amount = -info.ApplyAmount,
                        UserId = model.MemId,
                        PayWay = info.Remark,
                        SourceType = CapitalDetailInfo.CapitalDetailType.WithDraw,
                        SourceData = info.Id.ToString()
                    };
                    AddCapital(capitalDetail, false);
                }
                //scope.Complete();
                return true;
            });
            if(flag)
            {
                //发送消息
                var member = DbFactory.Default.Get<MemberInfo>(m => m.Id == model.MemId).FirstOrDefault();
                var message = new MessageWithDrawInfo();
                message.UserName = model.NickName;
                message.UserName = member != null ? member.UserName : "";
                message.Amount = model.ApplyAmount;
                message.ApplyType = model.ApplyType.GetHashCode();
                message.ApplyTime = model.ApplyTime;
                message.Remark = model.Remark;
                message.SiteName = SiteSettingApplication.SiteSettings.SiteName;

                if (info.ApplyStatus == ApplyWithdrawInfo.ApplyWithdrawStatus.WithDrawSuccess)
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnMemberWithDrawSuccess(model.MemId, message));
                
                if(info.ApplyStatus == ApplyWithdrawInfo.ApplyWithdrawStatus.Refuse)
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnMemberWithDrawFail(model.MemId, message));
            }
        }

        public void AddWithDrawApply(ApplyWithdrawInfo model)
        {
            DbFactory.Default.Add(model);
            var capital = DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == model.MemId).FirstOrDefault();
            capital.Balance -= model.ApplyAmount;
            capital.FreezeAmount = capital.FreezeAmount + model.ApplyAmount;
            var result = DbFactory.Default.Update(capital);
            //发送消息
            if(result > 0)
            {
                var member = DbFactory.Default.Get<MemberInfo>(m => m.Id == model.MemId).FirstOrDefault();
                var message = new MessageWithDrawInfo();
                message.UserName = model.NickName;
                message.UserName = member != null ? member.UserName : "";
                message.Amount = model.ApplyAmount;
                message.ApplyType = model.ApplyType.GetHashCode();
                message.ApplyTime = model.ApplyTime;
                message.Remark = model.Remark;
                message.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnMemberWithDrawApply(model.MemId, message));
            }
        }


        public void AddCapital(CapitalDetailModel model, bool isAccrualRecharge = true)
        {
            //var capital = Context.CapitalInfo.FirstOrDefault(e => e.MemId == model.UserId);
            var capital = DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == model.UserId).FirstOrDefault();
            decimal balance = 0;
            decimal presentAmount = 0;
            //充值赠送
            if (model.PresentAmount.HasValue && model.PresentAmount > 0)
            {
                presentAmount = model.PresentAmount.Value;
            }
            decimal chargeAmount = model.Amount;
            decimal freezeAmount = 0;
            StringBuilder strBuilder = new StringBuilder();
            if (presentAmount > 0)
            {
                strBuilder.Append("充" + model.Amount + "元赠送" + presentAmount + "元");
            }
            else
            {
                //备注、支付方式保存到remark
                //strBuilder.Append(model.PayWay);//BUG 40384
                //strBuilder.Append(" ");
                strBuilder.Append(model.Remark);
            }
            switch (model.SourceType)
            {
                case CapitalDetailInfo.CapitalDetailType.ChargeAmount:
                    balance = chargeAmount + presentAmount;
                    break;
                case CapitalDetailInfo.CapitalDetailType.WithDraw:
                    freezeAmount = model.Amount;
                    break;
                default:
                    balance = chargeAmount + presentAmount;
                    break;
            }
            if (capital == null)
            {
                capital = new CapitalInfo
                {
                    MemId = model.UserId,
                    Balance = balance,
                    ChargeAmount = chargeAmount,
                    FreezeAmount = freezeAmount,
                    PresentAmount = presentAmount
                };
                if (!isAccrualRecharge)
                {
                    capital.ChargeAmount = 0;
                }
                DbFactory.Default.Add(capital);
                var capitalDetail = new CapitalDetailInfo
                {
                    Id = CreateCode(model.SourceType),
                    CapitalID = capital.Id,
                    Amount = balance,
                    PresentAmount = presentAmount,
                    CreateTime = DateTime.Parse(model.CreateTime),
                    SourceType = model.SourceType,
                    SourceData = model.SourceData,
                    Remark = strBuilder.ToString()
                };
                DbFactory.Default.Add(capitalDetail);
                if (string.IsNullOrWhiteSpace(capitalDetail.SourceData)
                    && capitalDetail.SourceType == CapitalDetailInfo.CapitalDetailType.ChargeAmount
                    && model.PayWay == "管理员操作")
                {
                    capitalDetail.SourceData = capitalDetail.Id.ToString();
                    DbFactory.Default.Update(capitalDetail);
                }
            }
            else
            {
                var capitalDetail = DbFactory.Default.Get<CapitalDetailInfo>().Where(e => e.Id == model.Id && e.Id != 0).FirstOrDefault();
                if (capitalDetail == null)
                {
                    capitalDetail = new CapitalDetailInfo()
                    {
                        Id = CreateCode(model.SourceType),
                        Amount = model.Amount,
                        PresentAmount = model.PresentAmount.HasValue ? model.PresentAmount.Value : 0,
                        CreateTime = DateTime.Parse(model.CreateTime),
                        CapitalID = capital.Id,
                        SourceType = model.SourceType,
                        SourceData = model.SourceData,
                        Remark = strBuilder.ToString()
                    };
                    DbFactory.Default.Add(capitalDetail);
                    if (string.IsNullOrWhiteSpace(capitalDetail.SourceData)
                        && capitalDetail.SourceType == CapitalDetailInfo.CapitalDetailType.ChargeAmount
                        && model.PayWay == "管理员操作")
                    {
                        capitalDetail.SourceData = capitalDetail.Id.ToString();
                        DbFactory.Default.Update(capitalDetail);
                    }
                    capital.Balance += balance;
                    capital.PresentAmount += presentAmount;

                    if (isAccrualRecharge)
                    {
                        capital.ChargeAmount += chargeAmount;
                    }
                    capital.FreezeAmount += freezeAmount;
                    DbFactory.Default.Update(capital);
                }
            }
        }

        /// <summary>
        /// 充值成功
        /// </summary>
        /// <param name="chargeDetailId"></param>
        public void ChargeSuccess(long chargeDetailId, string remark = "")
        {
            var chargeDetail = DbFactory.Default.Get<ChargeDetailInfo>().Where(p => p.Id == chargeDetailId).FirstOrDefault();
            if (chargeDetail == null)
                return;

            chargeDetail.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
            var capitalDetail = DbFactory.Default.Get<CapitalDetailInfo>().Where(e => e.SourceData == chargeDetailId.ToString() && e.SourceType == CapitalDetailInfo.CapitalDetailType.ChargeAmount).FirstOrDefault();
            if (capitalDetail != null)//已经处理过直接返回
                return;
            DbFactory.Default.Update(chargeDetail);
            CapitalDetailModel detail = new CapitalDetailModel
            {
                Id = CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount),
                UserId = chargeDetail.MemId,
                Amount = chargeDetail.ChargeAmount,
                CreateTime = DateTime.Now.ToString(),
                SourceType = CapitalDetailInfo.CapitalDetailType.ChargeAmount,
                SourceData = chargeDetailId.ToString(),
                Remark = remark,
                PresentAmount = chargeDetail.PresentAmount
            };
            AddCapital(detail);
        }

        public void UpdateCapitalAmount(long memid, decimal amount, decimal freezeAmount, decimal chargeAmount)
        {
            throw new NotImplementedException();
        }


        private static object obj = new object();
        public long CreateCode(CapitalDetailInfo.CapitalDetailType type)
        {
            lock (obj)
            {
                int rand;
                char code;
                string orderId = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 4; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    orderId += code.ToString();
                }
                //17位
                return long.Parse(DateTime.Now.ToString("yyMMddHHmmss") + (int)type + orderId);
            }
        }


        public CapitalDetailInfo GetCapitalDetailInfo(long id)
        {
            return DbFactory.Default.Get<CapitalDetailInfo>().Where(e => e.Id == id).FirstOrDefault();
        }

        public List<CapitalDetailInfo> GetTopCapitalDetailList(long capitalId, int num)
        {
            return DbFactory.Default.Get<CapitalDetailInfo>().Where(e => e.CapitalID == capitalId).OrderByDescending(e => e.CreateTime).Take(num).ToList();
        }


        public CapitalInfo GetCapitalInfo(long userid)
        {
            var result= DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == userid).FirstOrDefault();
            if (result == null)
            {
                InitCapital(userid);
                result = DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == userid).FirstOrDefault();
            }
            return result;
        }


        public void SetPayPwd(long memid, string pwd)
        {
            pwd = pwd.Trim();
            var salt = Guid.NewGuid().ToString("N");
            var pwdmd5 = Mall.Core.Helper.SecureHelper.MD5(Mall.Core.Helper.SecureHelper.MD5(pwd) + salt);
            var member = DbFactory.Default.Get<MemberInfo>().Where(e => e.Id == memid).FirstOrDefault();
            if (member != null)
            {
              
                DbFactory.Default.Set<MemberInfo>().Set(n => n.PayPwd, pwdmd5).Set(n => n.PayPwdSalt, salt).Where(p => p.Id == member.Id).Succeed();
                string CACHE_USER_KEY = CacheKeyCollection.Member(memid);
                Core.Cache.Remove(CACHE_USER_KEY);

                //消息通知
                var userMessage = new MessageUserInfo();
                userMessage.UserName = member.UserName;
                userMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnEditPayPassWord(member.Id, userMessage));
            }
        }

        public void RefuseApplyWithDraw(long id, ApplyWithdrawInfo.ApplyWithdrawStatus status, string opuser, string remark)
        {
            //var model = Context.ApplyWithDrawInfo.FirstOrDefault(e => e.Id == id);
            var model = DbFactory.Default.Get<ApplyWithdrawInfo>().Where(e => e.Id == id).FirstOrDefault();
            model.ApplyStatus = status;
            model.OpUser = opuser;
            model.Remark = remark;
            model.ConfirmTime = DateTime.Now;
            DbFactory.Default.Update(model);
            //var capital = Context.CapitalInfo.FirstOrDefault(e => e.MemId == model.MemId);
            var capital = DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == model.MemId).FirstOrDefault();
            //capital.Balance = capital.Balance.Value + model.ApplyAmount;
            //capital.FreezeAmount = capital.FreezeAmount - model.ApplyAmount;
            DbFactory.Default.Set<CapitalInfo>().Set(n => n.Balance, capital.Balance + model.ApplyAmount)
                .Set(n => n.FreezeAmount, capital.FreezeAmount - model.ApplyAmount).Where(p => p.Id == capital.Id).Succeed();
            //发送消息
            var member = DbFactory.Default.Get<MemberInfo>(m => m.Id == model.MemId).FirstOrDefault();
            var message = new MessageWithDrawInfo();
            message.UserName = model.NickName;
            message.UserName = member != null ? member.UserName : "";
            message.Amount = model.ApplyAmount;
            message.ApplyType = model.ApplyType.GetHashCode();
            message.ApplyTime = model.ApplyTime;
            message.Remark = model.Remark;
            message.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnMemberWithDrawFail(model.MemId, message));
            //Context.SaveChanges();
        }

        public long AddChargeApply(ChargeDetailInfo model)
        {
            if (model.Id == 0)
            {
                model.Id = CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount);
            }
            //Context.ChargeDetailInfo.Add(model);
            //Context.SaveChanges();
            DbFactory.Default.Add(model);
            return model.Id;
        }

        public ChargeDetailInfo GetChargeDetail(long id)
        {
            //return Context.ChargeDetailInfo.FirstOrDefault(e => e.Id == id);
            return DbFactory.Default.Get<ChargeDetailInfo>().Where(e => e.Id == id).FirstOrDefault();
        }

        public void UpdateChargeDetail(ChargeDetailInfo model)
        {
            //var oldmodel = Context.ChargeDetailInfo.FirstOrDefault(e => e.Id == model.Id);
            var oldmodel = DbFactory.Default.Get<ChargeDetailInfo>().Where(e => e.Id == model.Id).FirstOrDefault();
            //using (TransactionScope scope = new TransactionScope())
            //{
            var flag = DbFactory.Default.InTransaction(() =>
            {
                //oldmodel.ChargeStatus = model.ChargeStatus;
                //oldmodel.ChargeTime = model.ChargeTime.Value;
                //oldmodel.ChargeWay = model.ChargeWay;
                //Context.SaveChanges();
                DbFactory.Default.Set<ChargeDetailInfo>().Set(n => n.ChargeStatus, model.ChargeStatus)
                    .Set(n => n.ChargeTime, model.ChargeTime.Value).Set(n => n.ChargeWay, model.ChargeWay).Where(e => e.Id == oldmodel.Id).Succeed();
                CapitalDetailModel capitalDetail = new CapitalDetailModel
                {
                    Amount = oldmodel.ChargeAmount,
                    UserId = oldmodel.MemId,
                    PayWay = model.ChargeWay,
                    SourceType = CapitalDetailInfo.CapitalDetailType.ChargeAmount,
                    SourceData = oldmodel.Id.ToString(),
                    PresentAmount = oldmodel.PresentAmount
                };
                AddCapital(capitalDetail);
                //scope.Complete();
                return true;
            });

        }

        public QueryPageModel<ChargeDetailInfo> GetChargeLists(ChargeQuery query)
        {
            //var charges = Context.ChargeDetailInfo.AsQueryable();
            var charges = DbFactory.Default.Get<ChargeDetailInfo>();
            if (query.ChargeStatus.HasValue)
            {
                charges = charges.Where(e => e.ChargeStatus == query.ChargeStatus.Value);
            }
            if (query.memberId.HasValue)
            {
                charges = charges.Where(e => e.MemId == query.memberId.Value);
            }
            if (query.ChargeNo.HasValue)
            {
                charges = charges.Where(e => e.Id == query.ChargeNo.Value);
            }
            //int total = 0;
            //var model = charges.GetPage(p => p.OrderByDescending(o => o.CreateTime), out total, query.PageNo, query.PageSize);
            var model = charges.OrderByDescending(o => o.CreateTime).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<ChargeDetailInfo> result = new QueryPageModel<ChargeDetailInfo> { Models = model, Total = model.TotalRecordCount };
            return result;
        }



        /// <summary>
        /// 添加店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long AddChargeDetailShop(ChargeDetailShopInfo model)
        {
            if (model.Id == 0)
            {
                model.Id = CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount);
            }
            DbFactory.Default.Add(model);
            return model.Id;
        }

        /// <summary>
        /// 修改店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        public void UpdateChargeDetailShop(ChargeDetailShopInfo model)
        {
            DbFactory.Default.Set<ChargeDetailShopInfo>().Set(n => n.ChargeStatus, model.ChargeStatus)
                .Set(n => n.ChargeTime, model.ChargeTime).Set(n => n.ChargeWay, model.ChargeWay).Where(e => e.Id == model.Id).Succeed();
        }

        /// <summary>
        /// 获取店铺充值流水信息
        /// </summary>
        /// <param name="Id">流水ID</param>
        /// <returns></returns>
        public ChargeDetailShopInfo GetChargeDetailShop(long Id)
        {
            return DbFactory.Default.Get<ChargeDetailShopInfo>().Where(e => e.Id == Id).FirstOrDefault();
        }
        /// <summary>
        /// 更新提现支付号
        /// </summary>
        /// <param name="id"></param>
        /// <param name="PayNo"></param>
        public void UpdateApplyWithDrawInfoPayNo(long Id, string PayNo)
        {
            DbFactory.Default.Set<ApplyWithdrawInfo>().Set(n => n.PayNo, PayNo).Where(r => r.Id == Id).Succeed();
        }
        /// <summary>
        /// 取消第三方付款
        /// </summary>
        /// <param name="Id"></param>
        public bool CancelPay(long Id)
        {
            bool result = false;
            var Info = DbFactory.Default.Get<ApplyWithdrawInfo>().Where(r => r.Id == Id).FirstOrDefault();
            if (Info != null && Info.ApplyStatus == ApplyWithdrawInfo.ApplyWithdrawStatus.PayPending)
            {
                result = DbFactory.Default.Set<ApplyWithdrawInfo>().Set(n => n.ApplyStatus, ApplyWithdrawInfo.ApplyWithdrawStatus.WaitConfirm).Where(r => r.Id == Info.Id).Succeed();
            }
            return result;
        }
        /// <summary>
        /// 获取累计收到红包
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public decimal GetSumRedPacket(long Id)
        {
            return DbFactory.Default.Get<CapitalDetailInfo>().Where(d => d.SourceType == CapitalDetailInfo.CapitalDetailType.RedPacket && d.CapitalID == Id).Sum<decimal>(d => d.Amount);
        }

        public void AddCapital(CapitalDetailInfo detail)
        {
            detail.Id = CreateCode(detail.SourceType);
            detail.CreateTime = DateTime.Now;
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Add(detail);
                //根据 detail.SourceType 进行处理
                switch (detail.SourceType) {
                    case CapitalDetailInfo.CapitalDetailType.ChargeAmount:
                    case CapitalDetailInfo.CapitalDetailType.Brokerage:
                        var change = detail.Amount;
                        DbFactory.Default.Set<CapitalInfo>()
                        .Where(p => p.Id == detail.CapitalID)
                        .Set(p => p.Balance, n => n.Balance + change)//变更数值已包含正负符号
                        .Succeed();
                        break;
                    default:
                        throw new MallException("未能处理当前业务类型");
                }
            });
        }
    }
}
