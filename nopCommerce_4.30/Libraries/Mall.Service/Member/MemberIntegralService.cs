using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using NetRube.Data;

namespace Mall.Service
{
    public class MemberIntegralService : ServiceBase, IMemberIntegralService
    {
        public void AddMemberIntegral(MemberIntegralRecordInfo model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null)
        {
            if (null == model) { throw new NullReferenceException("添加会员积分记录时，会员积分Model为空."); }
            if (0 == model.MemberId) { throw new NullReferenceException("添加会员积分记录时，会员Id为空."); }
            if (!DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == model.MemberId && a.UserName == model.UserName).Exist())
            {
                throw new Mall.Core.MallException("不存在此会员");
            }
            if (null != conversionMemberIntegralEntity)
            {
                model.Integral = conversionMemberIntegralEntity.ConversionIntegral();
            }
            if (model.Integral == 0)
            {
                return;
            }
            var userIntegral = DbFactory.Default.Get<MemberIntegralInfo>().Where(a => a.MemberId == model.MemberId).FirstOrDefault();
            if (userIntegral == null)
            {
                userIntegral = new MemberIntegralInfo();
                userIntegral.MemberId = model.MemberId;
                userIntegral.UserName = model.UserName;
                if (model.Integral > 0)
                {
                    userIntegral.HistoryIntegrals += model.Integral;
                }
                else
                {
                    throw new Mall.Core.MallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
                DbFactory.Default.Add(userIntegral);
            }
            else
            {
                if (model.Integral > 0)
                {
                    userIntegral.HistoryIntegrals += model.Integral;
                }
                else
                {
                    if (userIntegral.AvailableIntegrals < Math.Abs(model.Integral))
                        throw new Mall.Core.MallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
                DbFactory.Default.Update(userIntegral);
            }
          
            DbFactory.Default.Add(model);

            if (model.MemberIntegralRecordActionInfo != null)
            {
                model.MemberIntegralRecordActionInfo.ForEach(p => p.IntegralRecordId = model.Id);
                DbFactory.Default.AddRange(model.MemberIntegralRecordActionInfo);
            }
        }

        public void AddMemberIntegralNotAddHistoryIntegrals(MemberIntegralRecordInfo model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null)
        {
            if (null == model) { throw new NullReferenceException("添加会员积分记录时，会员积分Model为空."); }
            if (0 == model.MemberId) { throw new NullReferenceException("添加会员积分记录时，会员Id为空."); }
            //if (!Context.UserMemberInfo.Any(a => a.Id == model.MemberId && a.UserName == model.UserName))
            if (!DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == model.MemberId && a.UserName == model.UserName).Exist())
            {
                throw new Mall.Core.MallException("不存在此会员");
            }
            if (null != conversionMemberIntegralEntity)
            {
                model.Integral = conversionMemberIntegralEntity.ConversionIntegral();
            }
            if (model.Integral == 0)
            {
                return;
            }
            //var userIntegral = Context.MemberIntegral.FirstOrDefault(a => a.MemberId == model.MemberId);
            var userIntegral = DbFactory.Default.Get<MemberIntegralInfo>().Where(a => a.MemberId == model.MemberId).FirstOrDefault();
            if (userIntegral == null)
            {
                userIntegral = new MemberIntegralInfo();
                userIntegral.MemberId = model.MemberId;
                userIntegral.UserName = model.UserName;
                if (model.Integral <= 0)
                {
                    throw new Mall.Core.MallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
                //Context.MemberIntegral.Add(userIntegral);
                DbFactory.Default.Add(userIntegral);
            }
            else
            {
                if (model.Integral <= 0)
                {
                    if (userIntegral.AvailableIntegrals < Math.Abs(model.Integral))
                        throw new Mall.Core.MallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
                DbFactory.Default.Update(userIntegral);
            }
          
            DbFactory.Default.Add(model);
            if (model.MemberIntegralRecordActionInfo != null)
            {
                model.MemberIntegralRecordActionInfo.ForEach(p => p.IntegralRecordId = model.Id);
                DbFactory.Default.AddRange(model.MemberIntegralRecordActionInfo);
            }
        }
        public void AddMemberIntegralByRecordAction(MemberIntegralRecordInfo model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null)
        {
            if (null == model) { throw new NullReferenceException("添加会员积分记录时，会员积分Model为空."); }
            if (0 == model.MemberId) { throw new NullReferenceException("添加会员积分记录时，会员Id为空."); }
            //if (!Context.UserMemberInfo.Any(a => a.Id == model.MemberId && a.UserName == model.UserName))
            if (!DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == model.MemberId && a.UserName == model.UserName).Exist())
            {
                throw new Mall.Core.MallException("不存在此会员");
            }
            if (null != conversionMemberIntegralEntity)
            {
                var conversionIntegral = conversionMemberIntegralEntity.ConversionIntegral();
                var actions = DbFactory.Default.Get<MemberIntegralRecordActionInfo>(p => p.IntegralRecordId == model.Id).ToList();
                if (actions.Count > 0)//多个明细记录时，每个记录都需计算
                    model.Integral = conversionIntegral * actions.Count;
                else
                    model.Integral = conversionIntegral;
            }
            if (model.Integral == 0)
                return;

            var userIntegral = DbFactory.Default.Get<MemberIntegralInfo>().Where(a => a.MemberId == model.MemberId).FirstOrDefault();
            if (userIntegral == null)
            {
                userIntegral = new MemberIntegralInfo();
                userIntegral.MemberId = model.MemberId;
                userIntegral.UserName = model.UserName;
                if (model.Integral > 0)
                {
                    userIntegral.HistoryIntegrals += model.Integral;
                }
                else
                {
                    throw new Mall.Core.MallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
                DbFactory.Default.Add(userIntegral);
            }
            else
            {
                if (model.Integral > 0)
                {
                    userIntegral.HistoryIntegrals += model.Integral;
                }
                else
                {
                    if (userIntegral.AvailableIntegrals < Math.Abs(model.Integral))
                        throw new Mall.Core.MallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
                DbFactory.Default.Update(userIntegral);
            }
           
            DbFactory.Default.Add(model);

            //加会员积分记录明细详情
            if (model.MemberIntegralRecordActionInfo != null)
            {
                model.MemberIntegralRecordActionInfo.ForEach(p => p.IntegralRecordId = model.Id);
                DbFactory.Default.AddRange(model.MemberIntegralRecordActionInfo);
            }
        }

        public QueryPageModel<MemberIntegralInfo> GetMemberIntegralList(IntegralQuery query)
        {
            var db = DbFactory.Default.Get<MemberIntegralInfo>()
                .InnerJoin<MemberInfo>((mii, mi) => mii.MemberId == mi.Id);
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                db.Where(item => item.UserName.Contains(query.UserName));
            }
            if (query.StartDate.HasValue)
            {
                var members = DbFactory.Default.Get<MemberInfo>().Where(p => p.CreateDate >= query.StartDate).Select(p => p.Id).ToList<long>();
                db.Where(item => item.MemberId.ExIn(members));
            }
            if (query.EndDate.HasValue)
            {
                var end = query.EndDate.Value.Date.AddDays(1);
                var members = DbFactory.Default.Get<MemberInfo>().Where(p => p.CreateDate < end).Select(p => p.Id).ToList<long>();
                db.Where(item => item.MemberId.ExIn(members));
            }
            switch (query.Sort.ToLower())
            {
                case "availableintegrals":
                    if (query.IsAsc) db.OrderBy(p => p.AvailableIntegrals);
                    else db.OrderByDescending(p => p.AvailableIntegrals);
                    break;
                case "historyintegrals":
                    if (query.IsAsc) db.OrderBy(p => p.HistoryIntegrals);
                    else db.OrderByDescending(p => p.HistoryIntegrals);
                    break;
                case "createdate":
                    if (query.IsAsc) db.OrderBy<MemberInfo>(p => p.CreateDate);
                    else db.OrderByDescending<MemberInfo>(p => p.CreateDate);
                    break;
                default:
                    db.OrderByDescending(item => item.HistoryIntegrals);
                    break;

            }

            var model = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<MemberIntegralInfo>
            {
                Models = model,
                Total = model.TotalRecordCount
            };
        }

        public QueryPageModel<MemberIntegralRecordInfo> GetIntegralRecordList(IntegralRecordQuery query)
        {
            //int total = 0;
            //IQueryable<MemberIntegralRecord> list = Context.MemberIntegralRecord.AsQueryable();
            var list = DbFactory.Default.Get<MemberIntegralRecordInfo>();
            if (query.UserId.HasValue)
            {
                list.Where(item => item.MemberId == query.UserId.Value);
            }
            if (query.StartDate.HasValue)
            {
                list.Where(item => query.StartDate <= item.RecordDate);
            }
            if (query.IntegralType.HasValue)
            {
                list.Where(item => item.TypeId == query.IntegralType.Value);
            }
            if (query.EndDate.HasValue)
            {
                list.Where(item => query.EndDate >= item.RecordDate);
            }
            //list = list.GetPage(d => d.OrderByDescending(o => o.Id), out total, query.PageNo, query.PageSize);
            var model = list.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<MemberIntegralRecordInfo> pageModel = new QueryPageModel<MemberIntegralRecordInfo>() { Models = model, Total = model.TotalRecordCount };
            return pageModel;
        }

        public QueryPageModel<MemberIntegralRecordInfo> GetIntegralRecordListForWeb(IntegralRecordQuery query)
        {
            //int total = 0;
            //IQueryable<MemberIntegralRecord> list = Context.MemberIntegralRecord.AsQueryable();
            var list = DbFactory.Default.Get<MemberIntegralRecordInfo>();
            if (query.UserId.HasValue)
            {
                list.Where(item => item.MemberId == query.UserId.Value);
            }
            if (query.StartDate.HasValue)
            {
                list.Where(item => query.StartDate <= item.RecordDate);
            }
            if (query.IntegralType.HasValue)
            {
                if ((int)query.IntegralType.Value == 0)
                {
                    //list.Where(item => true);
                }
                else if ((int)query.IntegralType.Value == 1)
                {
                    list.Where(item => item.Integral >= 0);
                }
                else if ((int)query.IntegralType.Value == 2)
                {
                    list.Where(item => item.Integral < 0);
                }
            }
            if (query.EndDate.HasValue)
            {
                list.Where(item => query.EndDate >= item.RecordDate);
            }
            //list = list.GetPage(d => d.OrderByDescending(o => o.Id), out total, query.PageNo, query.PageSize);
            var model = list.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<MemberIntegralRecordInfo> pageModel = new QueryPageModel<MemberIntegralRecordInfo>() { Models = model, Total = model.TotalRecordCount };
            return pageModel;
        }

        public void SetIntegralRule(IEnumerable<MemberIntegralRuleInfo> info)
        {
            var list = DbFactory.Default.Get<MemberIntegralRuleInfo>().ToList();
            foreach (var s in info)
            {
                var t = list.FirstOrDefault(a => a.TypeId == s.TypeId);
                if (t != null)
                    DbFactory.Default.Set<MemberIntegralRuleInfo>().Set(n => n.Integral, s.Integral).Where(a => a.TypeId == s.TypeId).Succeed();
                else
                    DbFactory.Default.Add(s);
            }
        }

        public void SetIntegralChangeRule(MemberIntegralExchangeRuleInfo info)
        {
        
            var model = DbFactory.Default.Get<MemberIntegralExchangeRuleInfo>().FirstOrDefault();
            if (model != null)
            {
                DbFactory.Default.Set<MemberIntegralExchangeRuleInfo>().Set(n => n.IntegralPerMoney, info.IntegralPerMoney)
                    .Set(n => n.MoneyPerIntegral, info.MoneyPerIntegral).Where(a => a.Id == model.Id).Succeed();
            }
            else
            {
                model = new MemberIntegralExchangeRuleInfo();
                model.IntegralPerMoney = info.IntegralPerMoney;
                model.MoneyPerIntegral = info.MoneyPerIntegral;
                DbFactory.Default.Add(model);
            }
        }

        public MemberIntegralExchangeRuleInfo GetIntegralChangeRule()
        {
            var model = DbFactory.Default.Get<MemberIntegralExchangeRuleInfo>().FirstOrDefault();
            #region 当新的项目没数据时初始数据
            if (model == null)
            {
                model = new MemberIntegralExchangeRuleInfo();
                model.IntegralPerMoney = 0;
                model.MoneyPerIntegral = 0;
                DbFactory.Default.Add(model);
            }
            #endregion
            return model;
        }

        public List<MemberIntegralRuleInfo> GetIntegralRule()
        {
            return DbFactory.Default.Get<MemberIntegralRuleInfo>().ToList();
        }

        public bool HasLoginIntegralRecord(long userId)
        {
            var Date = DateTime.Now.Date;
            var Date2 = Date.AddDays(1);
            return DbFactory.Default.Get<MemberIntegralRecordInfo>().Where(a => a.MemberId == userId && a.RecordDate >= Date && a.RecordDate < Date2 && a.TypeId == MemberIntegralInfo.IntegralType.Login).Exist();
        }

        public UserIntegralGroupModel GetUserHistroyIntegralGroup(long userId)
        {
            var data = DbFactory.Default.Get<MemberIntegralRecordInfo>()
                .Where(a => a.MemberId == userId)
                .GroupBy(p => p.TypeId)
                .Select(p => new { Item1 = p.TypeId, Item2 = p.Integral.ExSum() })
                .ToList<SimpItem<MemberIntegralInfo.IntegralType, int>>();
            return new UserIntegralGroupModel
            {
                BindWxIntegral = data.FirstOrDefault(a => a.Item1 == MemberIntegralInfo.IntegralType.BindWX)?.Item2 ?? 0,
                CommentIntegral = data.FirstOrDefault(a => a.Item1 == MemberIntegralInfo.IntegralType.Comment)?.Item2 ?? 0,
                ConsumptionIntegral = data.FirstOrDefault(a => a.Item1 == MemberIntegralInfo.IntegralType.Consumption)?.Item2 ?? 0,
                LoginIntegral = data.FirstOrDefault(a => a.Item1 == MemberIntegralInfo.IntegralType.Login)?.Item2 ?? 0,
                RegIntegral = data.FirstOrDefault(a => a.Item1 == MemberIntegralInfo.IntegralType.Reg)?.Item2 ?? 0,
                SignIn = data.FirstOrDefault(a => a.Item1 == MemberIntegralInfo.IntegralType.SignIn)?.Item2 ?? 0,
                InviteIntegral = data.FirstOrDefault(a => a.Item1 == MemberIntegralInfo.IntegralType.InvitationMemberRegiste)?.Item2 ?? 0
            };
        }

        public MemberIntegralInfo GetMemberIntegral(long userId)
        {
            return DbFactory.Default.Get<MemberIntegralInfo>().Where(a => a.MemberId == userId).FirstOrDefault();
        }

        public List<MemberIntegralInfo> GetMemberIntegrals(IEnumerable<long> userIds)
        {
            return DbFactory.Default.Get<MemberIntegralInfo>().Where(a => a.MemberId.ExIn(userIds)).ToList();
        }

        public List<MemberIntegralRecordActionInfo> GetIntegralRecordAction(IEnumerable<long> virtualItemIds, MemberIntegralInfo.VirtualItemType type)
        {
            return DbFactory.Default.Get<MemberIntegralRecordActionInfo>().Where(e => e.VirtualItemId.ExIn(virtualItemIds) && e.VirtualItemTypeId == type).ToList();
        }

        public List<MemberIntegralRecordActionInfo> GetIntegralRecordAction(long record)
        {
            return DbFactory.Default.Get<MemberIntegralRecordActionInfo>().Where(p => p.IntegralRecordId == record).ToList();
        }


    }
}