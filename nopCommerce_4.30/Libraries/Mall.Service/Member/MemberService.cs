using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Entities;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Mall.Application;
using Mall.Core.Plugins.Message;
using Mall.CommonModel;
using System.Text;

namespace Mall.Service
{
    public class MemberService : ServiceBase, IMemberService
    {
        #region 方法

        public MemberInfo GetMemberByName(string userName)
        {

            var results = DbFactory.Default.Get<MemberInfo>().ToList();

            MemberInfo result = DbFactory.Default.Get<MemberInfo>().Where(d => d.UserName == userName).FirstOrDefault();
            return result;
        }



        /// <summary>
        /// 新方法修改用户信息
        /// </summary>
        /// <param name="model"></param>
        public void UpdateMemberInfo(MemberInfo model)
        {
            var m = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
            m.Nick = model.Nick;
            m.Email = model.Email;
            m.CreateDate = model.CreateDate;
            m.TopRegionId = model.TopRegionId;
            m.RegionId = model.RegionId;
            m.RealName = model.RealName;
            m.CellPhone = model.CellPhone;
            m.QQ = model.QQ;
            m.Address = model.Address;
            m.Photo = model.Photo;
            m.Remark = model.Remark;
            m.Sex = model.Sex;
            m.BirthDay = model.BirthDay;
            m.Occupation = model.Occupation;
            DbFactory.Default.Update(m);
            string CACHE_USER_KEY = CacheKeyCollection.Member(model.Id);
            Cache.Remove(CACHE_USER_KEY);
        }

        public void UpdateMember(MemberInfo model)
        {
            var m = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
            m.Nick = model.Nick;
            m.RealName = model.RealName;
            m.Email = model.Email;
            m.QQ = model.QQ;
            m.CellPhone = model.CellPhone;
            DbFactory.Default.Update(m);
            string CACHE_USER_KEY = CacheKeyCollection.Member(model.Id);
            Cache.Remove(CACHE_USER_KEY);
        }

        public void DeleteMember(long id)
        {
            DbFactory.Default.Del<MemberInfo>(n => n.Id == id);
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(id);
            Cache.Remove(CACHE_MEMBER_KEY);
        }

        public QueryPageModel<MemberInfo> GetMembers(MemberQuery query)
        {
            var db = WhereBuilder(query);
            switch (query.Sort.ToLower())
            {
                case "availableintegral":
                    db.LeftJoin<MemberIntegralInfo>((m, i) => m.Id == i.MemberId);
                    if (query.IsAsc) db.OrderBy<MemberIntegralInfo>(p => p.AvailableIntegrals);
                    else db.OrderByDescending<MemberIntegralInfo>(p => p.AvailableIntegrals);
                    break;
                case "netamount":
                    if (query.IsAsc) db.OrderBy(p => p.NetAmount);
                    else db.OrderByDescending(p => p.NetAmount);
                    break;
                case "createdate":
                    if (query.IsAsc) db.OrderBy(p => p.CreateDate);
                    else db.OrderByDescending(p => p.CreateDate);
                    break;
                default:
                    db.OrderByDescending(p => p.CreateDate);
                    break;

            }
            var models = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<MemberInfo>() { Models = models, Total = models.TotalRecordCount };
        }

        public int GetMemberCount(MemberQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        private GetBuilder<MemberInfo> WhereBuilder(MemberQuery query)
        {
            var db = DbFactory.Default.Get<MemberInfo>();
            if (query.IsHaveEmail.HasValue)
                db.Where(u => u.Email.ExIfNull("") != "");

            if (query.Platform.HasValue)//终端来源搜索
                db.Where(d => d.Platform == query.Platform.Value);

            if (!string.IsNullOrEmpty(query.Mobile))
                db.Where(d => d.CellPhone.Contains(query.Mobile));

            if (!string.IsNullOrWhiteSpace(query.keyWords))
                db.Where(d => d.UserName.Contains(query.keyWords));
            if (!string.IsNullOrWhiteSpace(query.weChatNick))
                db.Where(d => d.Nick.Contains(query.weChatNick));

            if (query.Status.HasValue)
                db.Where(d => d.Disabled == query.Status.Value);

            if (query.Labels != null && query.Labels.Length > 0)
            {
                var uids = DbFactory.Default.Get<MemberLabelInfo>().Where(p => p.LabelId.ExIn(query.Labels)).Select(p => p.MemId).Distinct().ToList<long>();
                db.Where(d => d.Id.ExIn(uids));
            }
            if (query.LoginTimeStart.HasValue)
                db.Where(e => e.LastLoginDate >= query.LoginTimeStart.Value);

            if (query.LoginTimeEnd.HasValue)
            {
                var end = query.LoginTimeEnd.Value.Date.AddDays(1);
                db.Where(e => e.LastLoginDate < end);
            }

            if (query.RegistTimeStart.HasValue)
                db.Where(e => e.CreateDate >= query.RegistTimeStart.Value);

            if (query.RegistTimeEnd.HasValue)
            {
                var end = query.RegistTimeEnd.Value.Date.AddDays(1);
                db.Where(e => e.CreateDate < end);
            }

            if (query.IsSeller.HasValue)
            {
                var uids = DbFactory.Default.Get<ManagerInfo>().LeftJoin<ShopInfo>((mi, si) => mi.ShopId == si.Id).Where<ShopInfo>(p => p.ShopStatus == ShopInfo.ShopAuditStatus.Open).Select(p => p.UserName).ToList<string>();
                if (query.IsSeller.Value)
                    db.Where(e => e.UserName.ExIn(uids));
                else
                    db.Where(e => e.UserName.ExNotIn(uids));
            }

            if (query.IsFocusWeiXin.HasValue)
            {
                var fusers = DbFactory.Default.Get<MemberOpenIdInfo>().LeftJoin<OpenIdInfo>((moi, oi) => moi.OpenId == oi.OpenId).Where<OpenIdInfo>(p => p.IsSubscribe == true).Select(p => p.UserId).ToList<long>();
                if (query.IsFocusWeiXin.Value)
                    db.Where(e => e.Id.ExIn(fusers));
                else
                    db.Where(e => e.Id.ExNotIn(fusers));
            }

            if (query.MinIntegral.HasValue || query.MaxIntegral.HasValue)
            {
                db.LeftJoin<MemberIntegralInfo>((m, mi) => m.Id == mi.MemberId);
                if (query.MinIntegral.HasValue)
                    db.Where<MemberIntegralInfo>(p => p.HistoryIntegrals >= query.MinIntegral.Value);
                if (query.MaxIntegral.HasValue)
                    db.Where<MemberIntegralInfo>(p => p.HistoryIntegrals < query.MaxIntegral.Value);
            }




            if (query.MemberStatisticsType.HasValue)
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;
                switch (query.MemberStatisticsType.Value)
                {
                    case MemberStatisticsType.ActiveOne:
                        var oneIds = DbFactory.Default.Get<MemberActivityDegreeInfo>().Where(p => p.OneMonth == true).Select(p => p.UserId).ToList<long>();
                        db.Where(p => p.Id.ExIn(oneIds));
                        break;
                    case MemberStatisticsType.ActiveThree:
                        var threeIds = DbFactory.Default.Get<MemberActivityDegreeInfo>().Where(p => p.OneMonth == false && p.ThreeMonth == true).Select(p => p.UserId).ToList<long>();
                        db.Where(p => p.Id.ExIn(threeIds));
                        break;
                    case MemberStatisticsType.ActiveSix:
                        var sixIds = DbFactory.Default.Get<MemberActivityDegreeInfo>().Where(p => p.OneMonth == false && p.ThreeMonth == false && p.SixMonth == true).Select(p => p.UserId).ToList<long>();
                        db.Where(p => p.Id.ExIn(sixIds));
                        break;
                    case MemberStatisticsType.SleepingThree:
                        startDate = DateTime.Now.AddMonths(-6);
                        endDate = DateTime.Now.AddMonths(-3);
                        db.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingSix:
                        startDate = DateTime.Now.AddMonths(-9);
                        endDate = DateTime.Now.AddMonths(-6);
                        db.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingNine:
                        startDate = DateTime.Now.AddMonths(-12);
                        endDate = DateTime.Now.AddMonths(-9);
                        db.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingTwelve:
                        startDate = DateTime.Now.AddMonths(-24);
                        endDate = DateTime.Now.AddMonths(-12);
                        db.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingTwentyFour:
                        endDate = DateTime.Now.AddMonths(-24);
                        db.Where(p => p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.BirthdayToday:
                        db.Where(p => p.BirthDay.Value.Month == DateTime.Now.Month && p.BirthDay.Value.Day == DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayToMonth:
                        db.Where(p => p.BirthDay.Value.Month == DateTime.Now.Month && p.BirthDay.Value.Day != DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayNextMonth:
                        startDate = DateTime.Now.AddMonths(1);
                        db.Where(p => p.BirthDay.Value.Month == startDate.Month);
                        break;
                    case MemberStatisticsType.RegisteredMember:
                        db.Where(p => p.OrderNumber == 0);
                        break;
                }
            }
            return db;
        }

        /// <summary>
        /// 根据用户id获取用户信息
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public List<MemberInfo> GetMembers(List<long> users)
        {
            return DbFactory.Default.Get<MemberInfo>().Where(p => p.Id.ExIn(users)).ToList();
        }

        public MemberInfo GetMember(long id)
        {
            MemberInfo result = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == id).FirstOrDefault();
            if (result != null)
            {
                int memberIntergral = GetHistoryIntegral(result.Id);
                result.MemberDiscount = GetMemberDiscount(memberIntergral);
            }
            return result;
        }

        /// <summary>
        /// 根据用户id和类型获取会员openid信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appIdType"></param>
        /// <returns></returns>
        public MemberOpenIdInfo GetMemberOpenIdInfoByuserId(long userId, MemberOpenIdInfo.AppIdTypeEnum appIdType, string serviceProvider = "")
        {
            var sql = DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.UserId == userId && p.AppIdType == appIdType);
            if (!string.IsNullOrWhiteSpace(serviceProvider))
            {
                sql = sql.Where(d => d.ServiceProvider == serviceProvider);
            }
            return sql.FirstOrDefault();
        }

        public MemberOpenIdInfo GetMemberOpenIdInfoByOpenIdOrUnionId(string openId = "", string unionId = "")
        {
            MemberOpenIdInfo info = null;
            if (!string.IsNullOrEmpty(openId))
                info = DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.OpenId.ToLower() == openId.ToLower()).FirstOrDefault();
            if (info == null && !string.IsNullOrEmpty(unionId))
                info = DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.UnionId.ToLower() == unionId.ToLower()).FirstOrDefault();
            return info;
        }

        public void LockMember(long id)
        {
            DbFactory.Default.Set<MemberInfo>().Set(n => n.Disabled, true).Where(p => p.Id == id).Succeed();
            string CACHE_USER_KEY = CacheKeyCollection.Member(id);
            Core.Cache.Remove(CACHE_USER_KEY);
        }

        public void UnLockMember(long id)
        {
            DbFactory.Default.Set<MemberInfo>().Set(n => n.Disabled, false).Where(p => p.Id == id).Succeed();
            string CACHE_USER_KEY = CacheKeyCollection.Member(id);
            Core.Cache.Remove(CACHE_USER_KEY);
        }

        public List<MemberInfo> GetMembers(bool? status, string keyWords)
        {
            var members = DbFactory.Default.Get<MemberInfo>().Where(item => item.ParentSellerId == 0);
            if (status.HasValue)
            {
                members = members.Where(p => p.Disabled == status.Value);
            }
            if (keyWords != null && keyWords != "")
            {
                members = members.Where(p => p.UserName.Contains(keyWords));
            }
            return members.ToList();
        }


        public void ChangePassword(long id, string password)
        {
            if (password.Length < 6)
                throw new MallException("密码长度至少6位字符！");

            var model = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == id).FirstOrDefault();

            ChangePassword(model, password);
        }

        /// <summary>
        /// 根据用户名修改密码
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        public void ChangePassword(string name, string password)
        {

            var user = DbFactory.Default.Get<MemberInfo>().Where(p => p.UserName == name).FirstOrDefault();
            if (user == null)
                throw new MallException("未找到指定用户");

            ChangePassword(user, password);
        }

        /// <summary>
        /// 修改支付密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        public void ChangePayPassword(long id, string password)
        {
            if (password.Length < 6)
                throw new MallException("密码长度至少6位字符！");

            var model = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (model != null)
            {
                model.PayPwd = GetPasswrodWithTwiceEncode(password, model.PayPwdSalt);

                var result = DbFactory.Default.Update(model);
                if (result > 0)
                {
                    //消息通知
                    var userMessage = new MessageUserInfo();
                    userMessage.UserName = model.UserName;
                    userMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnEditPayPassWord(model.Id, userMessage));
                }
            }
        }


        public void BatchDeleteMember(long[] ids)
        {
            DbFactory.Default.Del<MemberInfo>(p => p.Id.ExIn(ids));
            foreach (var id in ids)
            {
                string CACHE_USER_KEY = CacheKeyCollection.Member(id);
                Core.Cache.Remove(CACHE_USER_KEY);
            }
        }


        public void BatchLock(long[] ids)
        {

            var models = DbFactory.Default.Get<MemberInfo>().Where(item => item.Id.ExIn(ids)).ToList();
            foreach (var model in models)
            {

                DbFactory.Default.Set<MemberInfo>().Set(n => n.Disabled, true).Where(p => p.Id == model.Id).Succeed();
            }

            foreach (var id in ids)
            {
                string CACHE_USER_KEY = CacheKeyCollection.Member(id);
                Core.Cache.Remove(CACHE_USER_KEY);
            }
        }


        public int GetHistoryIntegral(long userId)
        {
            var historyIntegral = DbFactory.Default.Get<MemberIntegralInfo>().Where(a => a.MemberId == userId).Select(a => a.HistoryIntegrals).FirstOrDefault<int>();
            return historyIntegral;
        }
        public decimal GetMemberDiscount(int historyIntegrals)
        {
            var settings = SiteSettingApplication.SiteSettings;
            //授权模块影响会员折扣功能
            if (!(settings.IsOpenPC || settings.IsOpenH5 || settings.IsOpenMallSmallProg || settings.IsOpenApp))
            {
                return 1;
            }

            var grade = DbFactory.Default.Get<MemberGradeInfo>().OrderByDescending(a => a.Integral).Where(a => a.Integral <= historyIntegrals).FirstOrDefault();
            if (grade != null)
                return grade.Discount / 10;
            return 1;
        }

        public MemberInfo Register(string username, string password, int platform, string mobile = "", string email = "", long introducer = 0, long? spreadId = null)
        {
            //检查输入合法性
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException("用户名不能为空");
            if (CheckMemberExist(username))
                throw new MallException("用户名 " + username + " 已经被其它会员注册");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("密码不能为空");

            if (!string.IsNullOrEmpty(mobile) && Core.Helper.ValidateHelper.IsMobile(mobile))
            {
                if (CheckMobileExist(mobile))
                {
                    throw new MallException("手机号已经被其它会员注册");
                }
            }
            if (!string.IsNullOrEmpty(email) && Core.Helper.ValidateHelper.IsEmail(email))
            {
                if (CheckEmailExist(email))
                {
                    throw new MallException("邮箱已经被其它会员注册");
                }
            }
            password = password.Trim();
            Mall.Entities.MemberInfo member = null;
            var salt = Guid.NewGuid().ToString("N").Substring(12);
            password = GetPasswrodWithTwiceEncode(password, salt);
            //using (TransactionScope scope = new TransactionScope())
            //{
            var flag = DbFactory.Default.InTransaction(() =>
            {
                //填充会员信息
                member = new MemberInfo()
                {
                    UserName = username,
                    PasswordSalt = salt,
                    CreateDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Nick = NickFilterEmoji(username),
                    RealName = username,
                    CellPhone = mobile,
                    Email = email,
                    Platform = platform
                };
                if (introducer != 0)
                    member.InviteUserId = introducer;
                //密码加密
                member.Password = password;
                //member = Context.UserMemberInfo.Add(member);
                //Context.SaveChanges();
                DbFactory.Default.Add(member);
                if(!string.IsNullOrEmpty(mobile) || !string.IsNullOrEmpty(email))
                {
                    Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                    info.UserName = username;
                    info.MemberId = member.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Reg;
                    info.ReMark = "";
                    if (!string.IsNullOrEmpty(mobile) && Core.Helper.ValidateHelper.IsMobile(mobile)) //绑定手机号
                    {
                        var service = ServiceProvider.Instance<IMessageService>.Create;
                        service.UpdateMemberContacts(new Entities.MemberContactInfo() { Contact = mobile, ServiceProvider = "Mall.Plugin.Message.SMS", UserId = member.Id, UserType = Entities.MemberContactInfo.UserTypes.General });
                        //Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                        //info.UserName = username;
                        //info.MemberId = member.Id;
                        //info.RecordDate = DateTime.Now;
                        //info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Reg;
                        info.ReMark = "绑定手机";
                        //var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
                        //ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                        //var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                        //if (introducer != 0)
                        //{
                        //    var inviteMember = GetMember(introducer);
                        //    if (inviteMember != null)
                        //    {
                        //        inviteService.AddInviteIntegel(member, inviteMember);
                        //    }
                        //}
                    }
                    //TODO:DZY[160301]起码有三处,优化时记得合并
                    if (!string.IsNullOrEmpty(email) && Core.Helper.ValidateHelper.IsEmail(email)) //绑定邮箱
                    {
                        var service = ServiceProvider.Instance<IMessageService>.Create;
                        service.UpdateMemberContacts(new Entities.MemberContactInfo()
                        {
                            Contact = email,
                            ServiceProvider = "Mall.Plugin.Message.Email",
                            UserId = member.Id,
                            UserType = Entities.MemberContactInfo.UserTypes.General
                        });
                        //Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                        //info.UserName = username;
                        //info.MemberId = member.Id;
                        //info.RecordDate = DateTime.Now;
                        //info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Reg;
                        info.ReMark = "绑定邮箱";
                        //var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
                        //ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                        //var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                        //if (introducer != 0)
                        //{
                        //    var inviteMember = GetMember(introducer);
                        //    if (inviteMember != null)
                        //    {
                        //        inviteService.AddInviteIntegel(member, inviteMember);
                        //    }
                        //}
                    }
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                }
                
                var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                if (introducer != 0)
                {
                    var inviteMember = GetMember(introducer);
                    if (inviteMember != null)
                    {
                        var hasEmailOrPhone = !string.IsNullOrEmpty(mobile) || !string.IsNullOrEmpty(email);
                        inviteService.AddInviteIntegel(member, inviteMember, hasEmailOrPhone);
                    }
                }
                SyncAddDistributor(member.Id, spreadId);
                //scope.Complete();
                return true;
            });
            return member;
        }

        public MemberInfo Register(string username, string password, string serviceProvider, string openId, int platform
            , string sex = null, string headImage = null, long introducer = 0, string nickname = null, string unionid = null
            , string city = null, string province = null, long? spreadId = null)
        {
            if (string.IsNullOrWhiteSpace(serviceProvider))
                throw new ArgumentNullException("信任登录提供商不能为空");
            if (string.IsNullOrWhiteSpace(openId))
                throw new ArgumentNullException("openId不能为空");

            //检查OpenId是否被使用
            //CheckOpenIdHasBeenUsed(serviceProvider, openId);

            //检查输入合法性
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException("用户名不能为空");
            if (CheckMemberExist(username))
                throw new MallException("用户名 " + username + " 已经被其它会员注册");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("密码不能为空");
            password = password.Trim();
            int? wxsex = null;
            if (!string.IsNullOrEmpty(sex))
                wxsex = int.Parse(sex);

            int topRegionID = 0;
            int regionID = 0;
            //省份信息绑定
            if (!string.IsNullOrEmpty(province))
            {
                var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                var region = iRegionService.GetRegionByName(province.Trim(), Region.RegionLevel.Province);
                if (region != null)
                    topRegionID = region.Id;
                if (!string.IsNullOrEmpty(city))
                {
                    region = iRegionService.GetRegionByName(city.Trim(), Region.RegionLevel.City);
                    if (region != null)
                    {
                        regionID = region.Id;
                    }
                }
            }

            //填充会员信息
            MemberInfo memeber = new MemberInfo()
            {
                UserName = username,
                PasswordSalt = Guid.NewGuid().ToString("N").Substring(12),
                CreateDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                Nick = string.IsNullOrWhiteSpace(nickname) ? username : NickFilterEmoji(nickname),
                Sex = (SexType)wxsex.Value,
                TopRegionId = topRegionID,
                RegionId = regionID,
                Platform = platform
            };
            if (Core.Helper.ValidateHelper.IsMobile(username))
            {
                memeber.CellPhone = username;
            }
            if (introducer != 0)
                memeber.InviteUserId = introducer;
            //using (TransactionScope scope = new TransactionScope())
            //{
            var flag = DbFactory.Default.InTransaction(() =>
            {
                //密码加密
                memeber.Password = GetPasswrodWithTwiceEncode(password, memeber.PasswordSalt);
                //memeber = Context.UserMemberInfo.Add(memeber);
                //Context.SaveChanges();
                DbFactory.Default.Add(memeber);

                //更新绑定
                MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
                {
                    UserId = memeber.Id,
                    OpenId = openId,
                    ServiceProvider = serviceProvider,
                    UnionId = string.IsNullOrWhiteSpace(unionid) ? string.Empty : unionid
                };
                ChangeOpenIdBindMember(memberOpenIdInfo);
                //Context.SaveChanges();

                if (!string.IsNullOrWhiteSpace(headImage))
                    memeber.Photo = TransferHeadImage(headImage, memeber.Id);
                //Context.SaveChanges();
                DbFactory.Default.Update(memeber);

                if (!string.IsNullOrEmpty(username) && Core.Helper.ValidateHelper.IsMobile(username)) //绑定手机号
                {
                    var service = ServiceProvider.Instance<IMessageService>.Create;
                    service.UpdateMemberContacts(new Entities.MemberContactInfo()
                    {
                        Contact = username,
                        ServiceProvider = "Mall.Plugin.Message.SMS",
                        UserId = memeber.Id,
                        UserType = Entities.MemberContactInfo.UserTypes.General
                    });
                    Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                    info.UserName = username;
                    info.MemberId = memeber.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Reg;
                    info.ReMark = "绑定手机";
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                }

                if (introducer != 0)
                {
                    var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                    var inviteMember = GetMember(introducer);
                    if (inviteMember != null)
                    {
                        var hasEmailOrPhone = !string.IsNullOrEmpty(memeber.CellPhone) || !string.IsNullOrEmpty(memeber.Email);
                        inviteService.AddInviteIntegel(memeber, inviteMember, hasEmailOrPhone);
                    }
                }
                SyncAddDistributor(memeber.Id, spreadId);
                //scope.Complete();
                return true;
            });
            return memeber;
        }

        public MemberInfo Register(OAuthUserModel model)
        {
            if (string.IsNullOrWhiteSpace(model.LoginProvider))
                throw new ArgumentNullException("信任登录提供商不能为空");
            if (string.IsNullOrWhiteSpace(model.OpenId))
                throw new ArgumentNullException("openId不能为空");

            //检查OpenId是否被使用
            //CheckOpenIdHasBeenUsed(model.LoginProvider, model.OpenId);

            //检查输入合法性
            if (string.IsNullOrWhiteSpace(model.UserName))
                throw new ArgumentNullException("用户名不能为空");
            if (CheckMemberExist(model.UserName))
                throw new MallException("用户名 " + model.UserName + " 已经被其它会员注册");

            if (string.IsNullOrWhiteSpace(model.Password))
                throw new ArgumentNullException("密码不能为空");
            var password = model.Password.Trim();

            var sex = 0;
            int.TryParse(model.Sex, out sex);

            int topRegionID = 0;
            int regionID = 0;
            //省份信息绑定
            if (!string.IsNullOrEmpty(model.Province))
            {
                var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                var regionProvince = iRegionService.GetRegionByName(model.Province.Trim(), Region.RegionLevel.Province);
                if (regionProvince != null)
                {
                    topRegionID = (int)regionProvince.Id;
                }
                if (!string.IsNullOrEmpty(model.City))
                {
                    var regionCity = iRegionService.GetRegionByName(model.City.Trim(), Region.RegionLevel.City);
                    if (regionCity != null)
                    {
                        regionID = (int)regionCity.Id;
                    }
                }
            }


            //填充会员信息
            MemberInfo memeber = new MemberInfo()
            {
                UserName = model.UserName,
                PasswordSalt = Guid.NewGuid().ToString("N").Substring(12),
                CreateDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                Email = model.Email,
                Nick = string.IsNullOrWhiteSpace(model.NickName) ? model.UserName : model.NickName,
                Sex = (Mall.CommonModel.SexType)sex,
                TopRegionId = topRegionID,
                RegionId = regionID,
                Platform = model.Platform
            };

            if (model.introducer.HasValue && model.introducer.Value != 0)
                memeber.InviteUserId = model.introducer.Value;
            if (Core.Helper.ValidateHelper.IsMobile(model.UserName))
            {
                memeber.CellPhone = model.UserName;
            }
            //using (TransactionScope scope = new TransactionScope())
            //{
            var flag = DbFactory.Default.InTransaction(() =>
            {
                //密码加密
                memeber.Password = GetPasswrodWithTwiceEncode(password, memeber.PasswordSalt);
                //memeber = Context.UserMemberInfo.Add(memeber);
                //Context.SaveChanges();
                DbFactory.Default.Add(memeber);
                //更新绑定
                MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
                {
                    UserId = memeber.Id,
                    OpenId = model.OpenId,
                    ServiceProvider = model.LoginProvider,
                    UnionId = string.IsNullOrWhiteSpace(model.UnionId) ? string.Empty : model.UnionId
                };
                ChangeOpenIdBindMember(memberOpenIdInfo);
                //Context.SaveChanges();

                if (!string.IsNullOrWhiteSpace(model.Headimgurl))
                    memeber.Photo = TransferHeadImage(model.Headimgurl, memeber.Id);
                //Context.SaveChanges();
                DbFactory.Default.Update(memeber);
                var username = model.UserName;
                if (!string.IsNullOrEmpty(username) && Core.Helper.ValidateHelper.IsMobile(username)) //绑定手机号
                {
                    var service = ServiceProvider.Instance<IMessageService>.Create;
                    service.UpdateMemberContacts(new Entities.MemberContactInfo()
                    {
                        Contact = username,
                        ServiceProvider = "Mall.Plugin.Message.SMS",
                        UserId = memeber.Id,
                        UserType = Entities.MemberContactInfo.UserTypes.General
                    });
                    Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                    info.UserName = username;
                    info.MemberId = memeber.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Reg;
                    info.ReMark = "绑定手机";
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                }
                var email = model.Email;
                if (!string.IsNullOrEmpty(email) && Core.Helper.ValidateHelper.IsEmail(email)) //绑定邮箱
                {
                    var service = ServiceProvider.Instance<IMessageService>.Create;
                    service.UpdateMemberContacts(new Entities.MemberContactInfo()
                    {
                        Contact = email,
                        ServiceProvider = "Mall.Plugin.Message.Email",
                        UserId = memeber.Id,
                        UserType = Entities.MemberContactInfo.UserTypes.General
                    });
                    Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                    info.UserName = username;
                    info.MemberId = memeber.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Reg;
                    info.ReMark = "绑定邮箱";
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                }

                var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                if (model.introducer.HasValue && model.introducer.Value != 0)
                {
                    var inviteMember = GetMember(model.introducer.Value);
                    if (inviteMember != null)
                    {
                        var hasEmailOrPhone = !string.IsNullOrEmpty(memeber.CellPhone) || !string.IsNullOrEmpty(memeber.Email);
                        inviteService.AddInviteIntegel(memeber, inviteMember, hasEmailOrPhone);
                    }
                }
                SyncAddDistributor(memeber.Id, model.SpreadId);

                if (model.LoginProvider.ToLower() == "Mall.Plugin.OAuth.WeiXin".ToLower())
                {
                    AddBindInergral(memeber);
                }
                //scope.Complete();
                return true;
            });
            return memeber;
        }
        public bool CheckMemberExist(string username)
        {
            //bool result = Context.UserMemberInfo.Any(item => item.UserName == username);
            //return result;
            var isExist = DbFactory.Default.Get<ShopBranchManagerInfo>().Where(item => item.UserName == username).Exist()
                        || DbFactory.Default.Get<MemberInfo>().Where(item => item.UserName == username).Exist();
            return isExist;
        }

        public bool CheckMobileExist(string mobile)
        {
            //bool result = Context.UserMemberInfo.Any(item => item.CellPhone == mobile);
            //return result;
            return DbFactory.Default.Get<MemberInfo>().Where(item => item.CellPhone == mobile).Exist();
        }
        public bool CheckEmailExist(string email)
        {
            //bool result = Context.UserMemberInfo.Any(item => item.Email == email);
            //return result;
            return DbFactory.Default.Get<MemberInfo>().Where(item => item.Email == email).Exist();
        }

        string GetPasswrodWithTwiceEncode(string password, string salt)
        {
            string encryptedPassword = SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }

        public MemberInfo Login(string username, string password)
        {
            MemberInfo memberInfo = null;

            var IsEmail = Core.Helper.ValidateHelper.IsEmail(username);
            var IsPhone = Core.Helper.ValidateHelper.IsPhone(username);
            if (IsEmail)
            {
                //var contact = Context.MemberContactsInfo.Where(a => a.ServiceProvider == "Mall.Plugin.Message.Email" && a.Contact == username && a.UserType == Model.MemberContactsInfo.UserTypes.General).FirstOrDefault();
                var contact = DbFactory.Default.Get<MemberContactInfo>().Where(a => a.ServiceProvider == "Mall.Plugin.Message.Email" && a.Contact == username && a.UserType == MemberContactInfo.UserTypes.General).FirstOrDefault();
                if (contact == null)
                {
                    memberInfo = GetMemberByName(username);
                    if (memberInfo == null)
                    {
                        return null;
                    }
                }
                else
                    //memberInfo = Context.UserMemberInfo.Where(a => a.Id == contact.UserId).FirstOrDefault();
                    memberInfo = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == contact.UserId).FirstOrDefault();
            }
            else if (IsPhone)
            {
                //var contact = Context.MemberContactsInfo.Where(a => a.ServiceProvider == "Mall.Plugin.Message.SMS" && a.Contact == username && a.UserType == MemberContactsInfo.UserTypes.General).FirstOrDefault();
                var contact = DbFactory.Default.Get<MemberContactInfo>().Where(a => a.ServiceProvider == "Mall.Plugin.Message.SMS" && a.Contact == username && a.UserType == MemberContactInfo.UserTypes.General).FirstOrDefault();
                if (contact == null)
                {
                    memberInfo = GetMemberByName(username);
                    if (memberInfo == null)
                    {
                        return null;
                    }
                }
                else
                    //memberInfo = Context.UserMemberInfo.Where(a => a.Id == contact.UserId).FirstOrDefault();
                    memberInfo = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == contact.UserId).FirstOrDefault();
            }
            else
            {
                memberInfo = GetMemberByName(username);
            }

            if (memberInfo != null)
            {
                string encryptedWithSaltPassword = GetPasswrodWithTwiceEncode(password, memberInfo.PasswordSalt);
                if (encryptedWithSaltPassword.ToLower() != memberInfo.Password)//比较密码是否一致
                    memberInfo = null;//不一致，则置空，表示未找到指定的会员
                else
                {
                    if (memberInfo.Disabled)
                        throw new MessageException(ExceptionMessages.MemberDisabled);

                    //一致，则更新最后登录时间

                    //memberInfo.LastLoginDate = DateTime.Now;
                    //Context.SaveChanges();
                    DbFactory.Default.Set<MemberInfo>().Set(n => n.LastLoginDate, DateTime.Now).Where(p => p.Id == memberInfo.Id).Succeed();
                    Task.Factory.StartNew(() => { AddIntegel(memberInfo); }); //给用户加积分//执行登录后初始化相关操作
                    string CACHE_MEMBER_KEY = CacheKeyCollection.Member(memberInfo.Id);
                    Core.Cache.Remove(CACHE_MEMBER_KEY);
                }
            }
            return memberInfo;
        }

        /// <summary>
        /// 修改最后登录时间
        /// </summary>
        /// <param name="id"></param>
        public void UpdateLastLoginDate(long id)
        {
            //var memberInfo = this.Context.UserMemberInfo.FirstOrDefault(p => p.Id == id);
            var memberInfo = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (memberInfo != null)
            {
                //memberInfo.LastLoginDate = DateTime.Now;
                //this.Context.SaveChanges();
                DbFactory.Default.Set<MemberInfo>().Set(n => n.LastLoginDate, DateTime.Now).Where(p => p.Id == id).Succeed();
                //DbFactory.Default.Update(memberInfo);
            }
        }

        public void AddIntegel(MemberInfo member)
        {
            if (!ServiceProvider.Instance<IMemberIntegralService>.Create.HasLoginIntegralRecord(member.Id)) //当天没有登录过，加积分
            {
                Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                info.UserName = member.UserName;
                info.MemberId = member.Id;
                info.RecordDate = DateTime.Now;
                info.ReMark = "每天登录";
                info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Login;
                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Login);
                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
            }
        }


        public MemberInfo GetMemberByOpenId(string serviceProvider, string openId)
        {
            MemberInfo memberInfo = null;
            //var memberOpenInfo = Context.MemberOpenIdInfo.FirstOrDefault(item => item.ServiceProvider == serviceProvider && item.OpenId == openId);
            var memberOpenInfo = DbFactory.Default.Get<MemberOpenIdInfo>().Where(item => item.ServiceProvider == serviceProvider && item.OpenId == openId).OrderByDescending(p=>p.Id).FirstOrDefault();
            if (memberOpenInfo != null)
            {
                //memberInfo = Context.UserMemberInfo.FirstOrDefault(p => p.Id == memberOpenInfo.UserId);
                memberInfo = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == memberOpenInfo.UserId).FirstOrDefault();
                if (memberInfo != null)
                {
                    int memberIntergral = GetHistoryIntegral(memberInfo.Id);
                    memberInfo.MemberDiscount = GetMemberDiscount(memberIntergral);
                }
            }
            return memberInfo;
        }


        public MemberInfo GetMemberByContactInfo(string contact)
        {
            MemberInfo memberInfo = null;
            //var memberContactInfo = Context.MemberContactsInfo.FirstOrDefault(item => item.Contact == contact && item.UserType == MemberContactsInfo.UserTypes.General);
            var memberContactInfo = DbFactory.Default.Get<MemberContactInfo>().Where(item => item.Contact == contact && item.UserType == MemberContactInfo.UserTypes.General).FirstOrDefault();
            if (memberContactInfo != null)
                //memberInfo = Context.UserMemberInfo.FirstOrDefault(p => p.Id == memberContactInfo.UserId);
                memberInfo = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == memberContactInfo.UserId).FirstOrDefault();
            else
            {
                //memberInfo = Context.UserMemberInfo.Where(a => a.UserName == contact && Context.MemberContactsInfo.Any(item => item.UserId == a.Id)).FirstOrDefault();
                //var userids = DbFactory.Default.Get<MemberContactInfo>().Where<MemberContactInfo, MemberInfo>((mc, mi) => mc.UserId == mi.Id);
                //memberInfo = DbFactory.Default.Get<MemberInfo>().Where(a => a.UserName == contact && a.ExExists(userids)).FirstOrDefault();
                memberInfo = DbFactory.Default.Get<MemberInfo>().Where(a => a.UserName == contact).FirstOrDefault();
            }
            return memberInfo;
        }

        public void CheckContactInfoHasBeenUsed(string serviceProvider, string contact, MemberContactInfo.UserTypes userType = MemberContactInfo.UserTypes.General)
        {
            //var memberOpenIdInfo = Context.MemberContactsInfo.FirstOrDefault(item => item.ServiceProvider == serviceProvider && item.Contact == contact && item.UserType == userType);
            var memberOpenIdInfo = DbFactory.Default.Get<MemberContactInfo>().Where(item => item.ServiceProvider == serviceProvider && item.Contact == contact && item.UserType == userType).FirstOrDefault();
            if (memberOpenIdInfo != null)
                throw new MallException(string.Format("{0}已经被其它用户绑定", contact));
        }
        /// <summary>
        /// 获取一个新的用户名
        /// </summary>
        /// <returns></returns>
        private string GetNewUserName()
        {
            string result = "";
            while (true)
            {
                result = "wx";
                Random rnd = new Random();
                string[] seeds = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                int seedlen = seeds.Length;
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                //if (!Context.UserMemberInfo.Any(d => d.UserName == result))
                if (!DbFactory.Default.Get<MemberInfo>().Where(d => d.UserName == result).Exist())
                {
                    break;
                }
            }
            return result;
        }

        public MemberInfo QuickRegister(string username, string realName, string nickName, string serviceProvider, string openId, int platform, string unionid
            , string sex = null, string headImage = null, MemberOpenIdInfo.AppIdTypeEnum appidtype = MemberOpenIdInfo.AppIdTypeEnum.Normal
            , string unionopenid = null, string city = null, string province = null, long? spreadId = null)
        {
            if (string.IsNullOrEmpty(unionid) && string.IsNullOrEmpty(openId))
                throw new ArgumentNullException("unionid and openId");

            MemberInfo userMember = null;
            if (!string.IsNullOrWhiteSpace(unionid))
            {
                userMember = GetMemberByUnionId(unionid);
            }
            if (userMember == null)
                userMember = GetMemberByOpenId(serviceProvider, openId);
            if (userMember == null)
            {
                username = GetNewUserName();   //重新生成用户名
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentNullException("用户名不能为空");
                if (string.IsNullOrWhiteSpace(serviceProvider))
                    throw new ArgumentNullException("服务提供商不能为空");

                //检查OpenId是否被使用
                //CheckOpenIdHasBeenUsed(serviceProvider, openId);

                if (string.IsNullOrWhiteSpace(nickName))
                    nickName = username;
                var salt = "o" + Guid.NewGuid().ToString("N").Substring(12);  //o开头表示一键注册用户
                string password = GetPasswrodWithTwiceEncode("", salt);
                int? wxsex = null;
                if (!string.IsNullOrEmpty(sex))
                    wxsex = int.Parse(sex);

                int topRegionID = 0;
                int regionID = 0;
                //省份信息绑定
                if (!string.IsNullOrEmpty(province))
                {
                    var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                    var topRegion = iRegionService.GetRegionByName(province.Trim(), Region.RegionLevel.Province);
                    if (topRegion != null)
                    {
                        topRegionID = topRegion.Id;
                    }
                    if (!string.IsNullOrEmpty(city))
                    {
                        var cityRegion = iRegionService.GetRegionByName(city.Trim(), Region.RegionLevel.City);
                        if (cityRegion != null)
                        {
                            regionID = cityRegion.Id;
                        }
                    }
                }
                //填充会员信息
                userMember = new MemberInfo()
                {
                    UserName = username,
                    PasswordSalt = salt,
                    CreateDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Nick = nickName,
                    //Nick = NickFilterEmoji(nickName),
                    RealName = realName,
                    TopRegionId = topRegionID,
                    RegionId = regionID,
                    Platform = platform
                };
                if (wxsex.HasValue)
                    userMember.Sex = (SexType)wxsex.Value;
                //密码加密
                userMember.Password = password;
                //userMember = Context.UserMemberInfo.Add(userMember);
                //Context.SaveChanges();
                DbFactory.Default.Add(userMember);

                if (!string.IsNullOrWhiteSpace(headImage))
                    userMember.Photo = TransferHeadImage(headImage, userMember.Id);
                //Context.SaveChanges();
                DbFactory.Default.Update(userMember);
                userMember.IsNewAccount = true;//是新注册用户
            }
            else
            {
                //Log.Debug("QuickRegister:" + headImage);
                //如果头像发生改变
                if (!string.IsNullOrWhiteSpace(headImage))
                {
                    userMember.Photo = TransferHeadImage(headImage, userMember.Id);
                    //Context.SaveChanges();
                    DbFactory.Default.Update(userMember);
                }
            }

            //if (!this.Context.MemberOpenIdInfo.Any(p => p.UserId == userMember.Id && p.OpenId == openId))//微信和app的OpenId不同
            if (!DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.UserId == userMember.Id && p.OpenId == openId).Exist())//微信和app的OpenId不同
            {
                var memberOpenIdInfo = new MemberOpenIdInfo()
                {
                    UserId = userMember.Id,
                    OpenId = openId,
                    ServiceProvider = serviceProvider,
                    AppIdType = appidtype,
                    UnionId = string.IsNullOrWhiteSpace(unionid) ? string.Empty : unionid,
                    UnionOpenId = string.IsNullOrWhiteSpace(unionopenid) ? string.Empty : unionopenid
                };
                //Context.MemberOpenIdInfo.Add(memberOpenIdInfo);
                //Context.SaveChanges();
                DbFactory.Default.Add(memberOpenIdInfo);
            }
            if (serviceProvider.ToLower() == "Mall.Plugin.OAuth.WeiXin".ToLower())
            {
                AddBindInergral(userMember);
                //Task.Factory.StartNew(()=>AddBindInergral(member));
            }
            Task.Factory.StartNew(() => { AddIntegel(userMember); }); //给用户加积分//执行登录后初始化相关操作
            SyncAddDistributor(userMember.Id, spreadId);
            return userMember;
        }

        public void BindMember(long userId, string serviceProvider, string openId, string sex = null, string headImage = null, string unionid = null, string unionopenid = null, string city = null, string province = null)
        {
            //检查是否已经存在同一服务商相同的openId
            //CheckOpenIdHasBeenUsed(serviceProvider, openId, userId);
            int? wxsex = null;
            if (!string.IsNullOrEmpty(sex))
                wxsex = int.Parse(sex);

            int topRegionID = 0;
            int regionID = 0;
            //省份信息绑定
            if (!string.IsNullOrEmpty(province))
            {
                var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                var topRegion = iRegionService.GetRegionByName(province.Trim(), Region.RegionLevel.Province);
                if (topRegion != null)
                {
                    topRegionID = (int)topRegion.Id;
                }
                if (!string.IsNullOrEmpty(city))
                {
                    var cityRegion = iRegionService.GetRegionByName(city.Trim(), Region.RegionLevel.City);
                    if (cityRegion != null)
                    {
                        regionID = (int)cityRegion.Id;
                    }
                }
            }

            //var member = Context.UserMemberInfo.FirstOrDefault(item => item.Id == userId);
            var member = DbFactory.Default.Get<MemberInfo>().Where(item => item.Id == userId).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headImage))
            {
                if (string.IsNullOrWhiteSpace(member.Photo))//优先使用原头像
                    member.Photo = TransferHeadImage(headImage, userId);
            }

            if (wxsex != null)
                member.Sex = (Mall.CommonModel.SexType)wxsex.Value;

            member.RegionId = regionID;
            member.TopRegionId = topRegionID;

            MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
            {
                UserId = userId,
                OpenId = openId,
                ServiceProvider = serviceProvider,
                UnionId = unionid == null ? string.Empty : unionid,
                UnionOpenId = string.IsNullOrWhiteSpace(unionopenid) ? string.Empty : unionopenid
            };
            ChangeOpenIdBindMember(memberOpenIdInfo);
            //Context.SaveChanges();
            DbFactory.Default.Update(member);
            if (serviceProvider.ToLower() == "Mall.Plugin.OAuth.WeiXin".ToLower())
            {
                AddBindInergral(member);
                //Task.Factory.StartNew(()=>AddBindInergral(member));
            }
            //TODO:ZJT  在绑定用户与OpenId的时候，检查此OpenId是否存在红包，存在则添加到用户预存款里
            //注：因绑定OpenId的代码入口不同，所有会有多处调用此方法
            Mall.ServiceProvider.Instance<IBonusService>.Create.DepositToRegister(member.Id);
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(userId);
            Core.Cache.Remove(CACHE_MEMBER_KEY);
        }


        string TransferHeadImage(string image, long memberId)
        {
            string localName = string.Empty;
            if (!string.IsNullOrWhiteSpace(image))
            {
                if ((image.StartsWith("http://") || image.StartsWith("https://")) && image.IndexOf("/Storage") < 0)//网络图片
                {
                    var webClient = new WebClient();
                    string shortName = image.Substring(image.LastIndexOf('/'));
                    string ext = string.Empty;//获取文件扩展名
                    if (shortName.LastIndexOf('.') <= 0)//如果扩展名不包含 '.'，那么说明该文件名不包含扩展名，因此扩展名赋空
                        ext = ".jpg";
                    else
                        ext = shortName.Substring(shortName.LastIndexOf('.'));//否则取扩展名

                    string localTempName = "/temp/" + DateTime.Now.ToString("yyMMddHHmmssff") + ext;
                    try
                    {
                        var bytes = webClient.DownloadData(image);
                        Stream stream = new MemoryStream(bytes);
                        Core.MallIO.CreateFile(localTempName, stream, FileCreateType.Create);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error(ex.Message);
                        //网络下载异常
                        localName = null;
                    }
                    image = localTempName;
                }
                string directory = string.Format("/Storage/Member/{0}", memberId);
                localName = directory + "/headImage.jpg";
                if (image.Contains("/temp/"))
                {
                    image = image.Substring(image.LastIndexOf("/temp"));
                    //转移图片
                    try
                    {
                        Core.MallIO.CopyFile(image, localName, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("复制图片异常（TransferHeadImage）：" + image + " " + ex.Message);
                    }
                }
            }
            return localName;
        }

        public void BindMember(long userId, string serviceProvider, string openId, MemberOpenIdInfo.AppIdTypeEnum AppidType, string sex = null, string headImage = null, string unionid = null)
        {
            //检查是否已经存在同一服务商相同的openId
            MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
            {
                UserId = userId,
                OpenId = openId,
                ServiceProvider = serviceProvider,
                AppIdType = AppidType,
                UnionId = string.IsNullOrWhiteSpace(unionid) ? string.Empty : unionid
            };

            var member = DbFactory.Default.Get<MemberInfo>().Where(item => item.Id == userId).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headImage))
            {
                if (string.IsNullOrWhiteSpace(member.Photo))//优先使用原头像
                    member.Photo = TransferHeadImage(headImage, userId);
                if (!string.IsNullOrEmpty(sex))
                    member.Sex = (Mall.CommonModel.SexType)int.Parse(sex);
            }
            ChangeOpenIdBindMember(memberOpenIdInfo);

            DbFactory.Default.Update(member);
            //TODO:ZJT  在绑定用户与OpenId的时候，检查此OpenId是否存在红包，存在则添加到用户预存款里
            Mall.ServiceProvider.Instance<IBonusService>.Create.DepositToRegister(member.Id);
            if (serviceProvider.ToLower() == "Mall.Plugin.OAuth.WeiXin".ToLower())
            {
                AddBindInergral(member);
            }
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(userId);
            Core.Cache.Remove(CACHE_MEMBER_KEY);
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="payPwd"></param>
        /// <returns></returns>
        public bool VerificationPayPwd(long memid, string payPwd)
        {
            payPwd = payPwd.Trim();
            var data = DbFactory.Default.Get<MemberInfo>().Where(e => e.Id == memid).Select(p => new { p.PayPwdSalt, p.PayPwd }).FirstOrDefault();
            if (data != null)
            {
                var pwdmd5 = Mall.Core.Helper.SecureHelper.MD5(Mall.Core.Helper.SecureHelper.MD5(payPwd) + data.PayPwdSalt);
                if (pwdmd5 == data.PayPwd)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否有支付密码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasPayPassword(long id)
        {
            return DbFactory.Default.Get<MemberInfo>(p => p.Id == id && p.PayPwd.ExIsNotNull()).Exist();
        }

        public void BindMember(OAuthUserModel model)
        {
            //检查是否已经存在同一服务商相同的openId
            int topRegionID = 0;
            int regionID = 0;
            //省份信息绑定
            if (!string.IsNullOrEmpty(model.Province))
            {
                var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                var topRegion = iRegionService.GetRegionByName(model.Province.Trim(), Region.RegionLevel.Province);
                if (topRegion != null)
                {
                    topRegionID = (int)topRegion.Id;
                }
                if (!string.IsNullOrEmpty(model.City))
                {
                    var cityRegion = iRegionService.GetRegionByName(model.City.Trim(), Region.RegionLevel.City);
                    if (cityRegion != null)
                    {
                        regionID = (int)cityRegion.Id;
                    }
                }
            }
            var member = DbFactory.Default.Get<MemberInfo>().Where(item => item.Id == model.UserId).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(model.Headimgurl))
            {
                Log.Error("BindMember(OAuthUserModel model.Headimgurl )" + model.Headimgurl);
                Log.Error("member.Photol )" + member.Photo);
                if (string.IsNullOrWhiteSpace(member.Photo))//优先使用原头像
                    member.Photo = TransferHeadImage(model.Headimgurl, model.UserId);
            }
            if (!string.IsNullOrWhiteSpace(model.NickName))
            {
                member.Nick = model.NickName;
            }
            if (!string.IsNullOrWhiteSpace(model.Sex))
            {
                int sex = 0;
                if (int.TryParse(model.Sex, out sex))
                {
                    member.Sex = (Mall.CommonModel.SexType)sex;
                }
            }

            member.TopRegionId = topRegionID;
            member.RegionId = regionID;

            //更新绑定
            MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
            {
                UserId = model.UserId,
                OpenId = model.OpenId,
                ServiceProvider = model.LoginProvider,
                AppIdType = model.AppIdType,
                UnionId = string.IsNullOrWhiteSpace(model.UnionId) ? string.Empty : model.UnionId
            };
            ChangeOpenIdBindMember(memberOpenIdInfo);
            DbFactory.Default.Update(member);
            //TODO:ZJT  在绑定用户与OpenId的时候，检查此OpenId是否存在红包，存在则添加到用户预存款里
            Mall.ServiceProvider.Instance<IBonusService>.Create.DepositToRegister(member.Id);
            if (model.LoginProvider == "Mall.Plugin.OAuth.WeiXin".ToLower())
            {
                AddBindInergral(member);
            }
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(model.UserId);
            Core.Cache.Remove(CACHE_MEMBER_KEY);
        }

        private void ChangeOpenIdBindMember(MemberOpenIdInfo model)
        {
            //更新绑定
            MemberOpenIdInfo memberOpenIdInfo = DbFactory.Default.Get<MemberOpenIdInfo>().Where(d => d.OpenId == model.OpenId && d.ServiceProvider == model.ServiceProvider).FirstOrDefault();
            //清理绑定
            if (memberOpenIdInfo != null)
            {
                DbFactory.Default.Del(memberOpenIdInfo);
            }
            DbFactory.Default.Add(model);
        }

        private void AddBindInergral(MemberInfo member)
        {
            bool x = DbFactory.Default.Get<MemberIntegralRecordInfo>().Where(a => a.MemberId == member.Id && a.TypeId == MemberIntegralInfo.IntegralType.BindWX).Exist();
            if (x)
                return;
            try
            {
                //绑定微信积分
                Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                info.UserName = member.UserName;
                info.MemberId = member.Id;
                info.RecordDate = DateTime.Now;
                info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.BindWX;
                info.ReMark = "绑定微信";
                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.BindWX);
                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex);
            }
        }


        public void DeleteMemberOpenId(long userid, string openid)
        {
            var memberopenid = DbFactory.Default.Get<MemberOpenIdInfo>().Where(e => e.UserId == userid);
            if (!string.IsNullOrEmpty(openid))
            {
                memberopenid = memberopenid.Where(p => p.OpenId == openid);
            }
            DbFactory.Default.Del<MemberOpenIdInfo>(memberopenid.ToList());
        }


        public MemberInfo GetMemberByUnionId(string serviceProvider, string UnionId)
        {
            MemberInfo memberInfo = null;
            if (!string.IsNullOrEmpty(UnionId))
            {
                var memberOpenInfo = DbFactory.Default.Get<MemberOpenIdInfo>().Where(item => item.ServiceProvider == serviceProvider && item.UnionId == UnionId).FirstOrDefault();
                if (memberOpenInfo != null)
                    memberInfo = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == memberOpenInfo.UserId).FirstOrDefault();
            }
            return memberInfo;
        }
        public MemberInfo GetMemberByUnionId(string UnionId)
        {
            MemberInfo memberInfo = null;
            if (!string.IsNullOrWhiteSpace(UnionId))
            {
                var memberOpenInfo = DbFactory.Default.Get<MemberOpenIdInfo>().Where(item => item.UnionId == UnionId).FirstOrDefault();
                if (memberOpenInfo != null)
                    memberInfo = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == memberOpenInfo.UserId).FirstOrDefault();
            }
            return memberInfo;
        }

        public MemberInfo GetMemberByUnionIdOpenId(string UnionId, string openId)
        {
            MemberInfo memberInfo = null;
            if (!string.IsNullOrWhiteSpace(UnionId) && !string.IsNullOrWhiteSpace(openId))
            {
                var memberOpenInfo = DbFactory.Default.Get<MemberOpenIdInfo>().Where(item => item.UnionId == UnionId && item.OpenId == openId).FirstOrDefault();
                if (memberOpenInfo != null)
                    memberInfo = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == memberOpenInfo.UserId).FirstOrDefault();
            }
            return memberInfo;
        }

        public List<MemberLabelInfo> GetMembersByLabel(long labelid)
        {
            return DbFactory.Default.Get<MemberLabelInfo>().Where(item => item.LabelId == labelid).ToList();
        }

        public IEnumerable<MemberLabelInfo> GetMemberLabels(long memid)
        {
            return DbFactory.Default.Get<MemberLabelInfo>().Where(e => e.MemId == memid).ToList();
        }

        public void SetMemberLabel(long userid, IEnumerable<long> labelids)
        {
            var memLabels = DbFactory.Default.Get<MemberLabelInfo>().Where(e => e.MemId == userid).ToList();
            DbFactory.Default.Del<MemberLabelInfo>(memLabels);
            if (labelids.Count() > 0)
            {
                var labelInfo = labelids.Select(e => new MemberLabelInfo { LabelId = e, MemId = userid });
                DbFactory.Default.Add(labelInfo);
            }
        }
        public void SetMembersLabel(long[] userid, IEnumerable<long> labelids)
        {
            var memLabels = DbFactory.Default.Get<MemberLabelInfo>().Where(e => e.MemId.ExIn(userid) && e.LabelId.ExIn(labelids)).ToList();
            var member = (from u in userid
                          from l in labelids
                          select new { uid = u, lid = l });
            var label = from m in member
                        where !memLabels.Any(l => m.lid == l.LabelId && m.uid == l.MemId)
                        select new MemberLabelInfo { LabelId = m.lid, MemId = m.uid };

            DbFactory.Default.Add(label);
        }

        public IEnumerable<int> GetAllTopRegion()
        {
            return DbFactory.Default.Get<MemberInfo>().Select(e => e.TopRegionId).Distinct().ToList<int>();
        }

        /// <summary>
        /// 通过会员等级ID获取会员消费范围
        /// </summary>
        /// <param name="gradeId"></param>
        /// <returns></returns>
        public GradeIntegralRange GetMemberGradeRange(long gradeId)
        {
            GradeIntegralRange range = new GradeIntegralRange();
            var min = DbFactory.Default.Get<MemberGradeInfo>().Where(a => a.Id == gradeId).Select(a => a.Integral).FirstOrDefault<int>();
            var max = int.MaxValue;
            var maxIntegral = DbFactory.Default.Get<MemberGradeInfo>().Where(a => a.Integral > min).OrderBy(a => a.Integral).FirstOrDefault();
            if (maxIntegral != null)
            {
                max = maxIntegral.Integral;
            }
            range.MinIntegral = min;
            range.MaxIntegral = max;
            return range;
        }


        /// <summary>
        /// 会员购买力列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<MemberInfo> GetPurchasingPowerMember(MemberPowerQuery query)
        {
            var result = DbFactory.Default.Get<MemberInfo>().Where(p => p.Disabled == false);

            #region 最近消费
            if (query.RecentlySpentTime.HasValue)
            {
                var date = DateTime.Now;
                switch (query.RecentlySpentTime.Value)
                {
                    case RecentlySpentTime.OneWeek:
                        date = DateTime.Now.AddDays(-7);
                        result = result.Where(p => p.LastConsumptionTime > date);
                        break;
                    case RecentlySpentTime.TwoWeek:
                        date = DateTime.Now.AddDays(-14);
                        result = result.Where(p => p.LastConsumptionTime > date);
                        break;
                    case RecentlySpentTime.OneMonthWithin:
                        date = DateTime.Now.AddMonths(-1);
                        result = result.Where(p => p.LastConsumptionTime > date);
                        break;
                    case RecentlySpentTime.OneMonth:
                        date = DateTime.Now.AddMonths(-1);
                        result = result.Where(p => p.LastConsumptionTime < date);
                        break;
                    case RecentlySpentTime.TwoMonth:
                        date = DateTime.Now.AddMonths(-2);
                        result = result.Where(p => p.LastConsumptionTime < date);
                        break;
                    case RecentlySpentTime.ThreeMonth:
                        date = DateTime.Now.AddMonths(-3);
                        result = result.Where(p => p.LastConsumptionTime < date);
                        break;
                    case RecentlySpentTime.SixMonth:
                        date = DateTime.Now.AddMonths(-6);
                        result = result.Where(p => p.LastConsumptionTime < date);
                        break;
                }
            }
            if (query.StartTime.HasValue)
            {
                var date = query.StartTime.Value;
                result = result.Where(p => p.LastConsumptionTime > date);
            }
            if (query.EndTime.HasValue)
            {
                var date = query.EndTime.Value.AddDays(1);
                result = result.Where(p => p.LastConsumptionTime < date);
            }
            #endregion

            #region 购买次数

            if (query.Purchases.HasValue)
            {
                switch (query.Purchases.Value)
                {
                    case Purchases.ZeroTimes:
                        result = result.Where(p => p.OrderNumber == 0);
                        break;
                    case Purchases.OneTimes:
                        result = result.Where(p => p.OrderNumber >= 1);
                        break;
                    case Purchases.TwoTimes:
                        result = result.Where(p => p.OrderNumber >= 2);
                        break;
                    case Purchases.ThreeTimes:
                        result = result.Where(p => p.OrderNumber >= 3);
                        break;
                    case Purchases.FourTimes:
                        result = result.Where(p => p.OrderNumber >= 4);
                        break;
                }
            }

            if (query.StartPurchases.HasValue)
            {
                result = result.Where(p => p.OrderNumber >= query.StartPurchases.Value);
            }

            if (query.EndPurchases.HasValue)
            {
                result = result.Where(p => p.OrderNumber <= query.EndPurchases.Value);
            }

            #endregion

            #region 类目

            if (query.CategoryId.HasValue)
            {
                //var userIds = Context.MemberBuyCategoryInfo.Where(p => p.CategoryId == query.CategoryId.Value).Select(p => p.UserId);
                var userIds = DbFactory.Default.Get<MemberBuyCategoryInfo>().Where(p => p.CategoryId == query.CategoryId.Value).Select(p => p.UserId).ToList<long>();
                result = result.Where(p => p.Id.ExIn(userIds));
            }

            #endregion

            #region 消费金额
            if (query.AmountOfConsumption.HasValue)
            {
                switch (query.AmountOfConsumption.Value)
                {
                    case AmountOfConsumption.AmountOne:
                        result = result.Where(p => p.NetAmount >= 0 && p.NetAmount < 500);
                        break;
                    case AmountOfConsumption.AmountTwo:
                        result = result.Where(p => p.NetAmount >= 500 && p.NetAmount < 1000);
                        break;
                    case AmountOfConsumption.AmountThree:
                        result = result.Where(p => p.NetAmount >= 1000 && p.NetAmount < 3000);
                        break;
                    case AmountOfConsumption.AmountFour:
                        result = result.Where(p => p.NetAmount >= 3000);
                        break;
                        //case AmountOfConsumption.AmountFive:
                        //    result = result.Where(p => p.NetAmount >= 200 && p.NetAmount < 300);
                        //    break;
                }
            }

            if (query.StartAmountOfConsumption.HasValue)
            {
                result = result.Where(p => p.NetAmount >= query.StartAmountOfConsumption.Value);
            }

            if (query.EndAmountOfConsumption.HasValue)
            {
                result = result.Where(p => p.NetAmount <= query.EndAmountOfConsumption.Value);
            }
            #endregion

            #region 会员标签
            if (query.LabelId.HasValue)
            {
                //var userIds = Context.MemberLabelInfo.Where(p => p.LabelId == query.LabelId.Value).Select(p => p.MemId);
                var userIds = DbFactory.Default.Get<MemberLabelInfo>().Where(p => p.LabelId == query.LabelId.Value).Select(p => p.MemId).ToList<long>();
                result = result.Where(p => p.Id.ExIn(userIds));
            }

            if (query.LabelIds != null && query.LabelIds.Count() > 0)
            {
                //var userIds = Context.MemberLabelInfo.Where(p => query.LabelIds.Contains(p.LabelId)).Select(p => p.MemId);
                var userIds = DbFactory.Default.Get<MemberLabelInfo>().Where(p => p.LabelId.ExIn(query.LabelIds)).Select(p => p.MemId).ToList<long>();
                result = result.Where(p => p.Id.ExIn(userIds));
            }
            #endregion

            #region 会员分组搜索

            if (query.MemberStatisticsType.HasValue)
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;
                switch (query.MemberStatisticsType.Value)
                {
                    case MemberStatisticsType.ActiveOne:
                        //var oneIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == true).Select(p => p.UserId);
                        var oneIds = DbFactory.Default.Get<MemberActivityDegreeInfo>().Where(p => p.OneMonth == true).Select(p => p.UserId).ToList<long>();
                        result = result.Where(p => p.Id.ExIn(oneIds));
                        break;
                    case MemberStatisticsType.ActiveThree:
                        //var threeIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == false && p.ThreeMonth == true).Select(p => p.UserId);
                        var threeIds = DbFactory.Default.Get<MemberActivityDegreeInfo>().Where(p => p.OneMonth == false && p.ThreeMonth == true).Select(p => p.UserId).ToList<long>();
                        result = result.Where(p => p.Id.ExIn(threeIds));
                        break;
                    case MemberStatisticsType.ActiveSix:
                        //var sixIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == false && p.ThreeMonth == false && p.SixMonth == true).Select(p => p.UserId);
                        var sixIds = DbFactory.Default.Get<MemberActivityDegreeInfo>().Where(p => p.OneMonth == false && p.ThreeMonth == false && p.SixMonth == true).Select(p => p.UserId).ToList<long>();
                        result = result.Where(p => p.Id.ExIn(sixIds));
                        break;
                    case MemberStatisticsType.SleepingThree:
                        startDate = DateTime.Now.AddMonths(-6);
                        endDate = DateTime.Now.AddMonths(-3);
                        result = result.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingSix:
                        startDate = DateTime.Now.AddMonths(-9);
                        endDate = DateTime.Now.AddMonths(-6);
                        result = result.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingNine:
                        startDate = DateTime.Now.AddMonths(-12);
                        endDate = DateTime.Now.AddMonths(-9);
                        result = result.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingTwelve:
                        startDate = DateTime.Now.AddMonths(-24);
                        endDate = DateTime.Now.AddMonths(-12);
                        result = result.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingTwentyFour:
                        endDate = DateTime.Now.AddMonths(-24);
                        result = result.Where(p => p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.BirthdayToday:
                        result = result.Where(p => p.BirthDay.Value.Month == DateTime.Now.Month && p.BirthDay.Value.Day == DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayToMonth:
                        result = result.Where(p => p.BirthDay.Value.Month == DateTime.Now.Month && p.BirthDay.Value.Day != DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayNextMonth:
                        startDate = DateTime.Now.AddMonths(1);
                        result = result.Where(p => p.BirthDay.Value.Month == startDate.Month);
                        break;
                    case MemberStatisticsType.RegisteredMember:
                        result = result.Where(p => p.OrderNumber == 0);
                        break;
                }
            }

            #endregion

            #region 排序

            switch (query.Sort.ToLower())
            {
                case "netamount":
                    if (query.IsAsc) result = result.OrderBy(o => o.NetAmount).OrderByDescending(o => o.Id);
                    else result = result.OrderByDescending(o => o.NetAmount).OrderByDescending(o => o.Id);
                    break;
                case "ordernumber":
                    if (query.IsAsc) result = result.OrderBy(o => o.OrderNumber).OrderByDescending(o => o.Id);
                    else result = result.OrderByDescending(o => o.OrderNumber).OrderByDescending(o => o.Id);
                    break;
                case "lastconsumptiontime":
                    if (query.IsAsc) result = result.OrderBy(o => o.LastConsumptionTime).OrderByDescending(o => o.Id);
                    else result = result.OrderByDescending(o => o.LastConsumptionTime).OrderByDescending(o => o.Id);
                    break;
                default:
                    result = result.OrderByDescending(o => o.NetAmount);
                    break;
            }

            #endregion
            var model = result.Select().ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<MemberInfo>()
            {
                Models = model,
                Total = model.TotalRecordCount
            };
        }


        /// <summary>
        /// 获取会员分组数据
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="franchiseeId"></param>
        /// <returns></returns>
        public List<MemberGroupInfo> GetMemberGroup()
        {
            return DbFactory.Default.Get<MemberGroupInfo>().ToList();
        }


        /// <summary>
        /// 批量获取会员购买类别
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public List<MemberBuyCategoryInfo> GetMemberBuyCategoryByUserIds(IEnumerable<long> userIds)
        {
            var sqlData = DbFactory.Default.Get<MemberBuyCategoryInfo>().LeftJoin<CategoryInfo>((mc, ci) => mc.CategoryId == ci.Id).Where(p => p.UserId.ExIn(userIds))
                .OrderByDescending(o => o.OrdersCount).OrderByDescending(o => o.Id)
                .Select(p => new { CategoryId = p.CategoryId, Id = p.Id, OrdersCount = p.OrdersCount, UserId = p.UserId }).Select<CategoryInfo>(c => new { Name = c.Name }).ToList<dynamic>();

            var models = sqlData.Select(p => new MemberBuyCategoryInfo
            {
                CategoryId = p.CategoryId,
                UserId = p.UserId,
                CategoryName = p.Name,
                Id = p.Id,
                OrdersCount = p.OrdersCount
            }).ToList();

            return models;
        }

        /// <summary>
        /// 批量获取用户OPENID
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public IEnumerable<string> GetOpenIdByUserIds(IEnumerable<long> userIds)
        {

            var memOpenid = DbFactory.Default.Get<MemberOpenIdInfo>().LeftJoin<OpenIdInfo>((moi, oi) => moi.OpenId == oi.OpenId)
                .LeftJoin<MemberInfo>((moi, mi) => moi.UserId == mi.Id).Where<OpenIdInfo>(p => p.IsSubscribe == true)
                .Where<MemberInfo>(p => p.Id.ExIn(userIds)).Select(p => p.OpenId).Distinct().ToList<string>();


            return memOpenid;
        }


        public List<MemberOpenIdInfo> GetOpenIdByUser(long user)
        {
            return DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.UserId == user).ToList();
        }
        /// <summary>
        /// 修改会员净消费金额
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="netAmount"></param>
        public void UpdateNetAmount(long userId, decimal netAmount)
        {
            DbFactory.Default.Set<MemberInfo>().Set(n => n.NetAmount, n => n.NetAmount + netAmount).Where(n => n.Id == userId).Succeed();
        }

        /// <summary>
        /// 增加会员下单量
        /// </summary>
        /// <param name="userId"></param>
        public void IncreaseMemberOrderNumber(long userId)
        {
            DbFactory.Default.Set<MemberInfo>().Set(n => n.OrderNumber, n => n.OrderNumber + 1).Where(n => n.Id == userId).Succeed();
        }

        /// <summary>
        /// 减少会员下单量
        /// </summary>
        /// <param name="userId"></param>
        public void DecreaseMemberOrderNumber(long userId)
        {

            DbFactory.Default.Set<MemberInfo>().Set(n => n.OrderNumber, n => n.OrderNumber - 1).Where(n => n.Id == userId).Succeed();
        }

        /// <summary>
        /// 修改最后消费时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastConsumptionTime">最后消费时间</param>
        public void UpdateLastConsumptionTime(long userId, DateTime lastConsumptionTime)
        {
            //this.Context.UserMemberInfo.Where(p => p.Id == userId).Update(p => new UserMemberInfo
            //{
            //	LastConsumptionTime = lastConsumptionTime
            //});
            //var model = Context.UserMemberInfo.Where(p => p.Id == userId).FirstOrDefault();
            //var model = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == userId).FirstOrDefault();
            //model.LastConsumptionTime = lastConsumptionTime;
            //Context.SaveChanges();
            Log.Info(string.Format("用户ID={0},最后消费时间={1}", userId, lastConsumptionTime));
            DbFactory.Default.Set<MemberInfo>().Set(n => n.LastConsumptionTime, lastConsumptionTime).Where(p => p.Id == userId).Succeed();
        }
        #endregion

        #region 私有方法
        private void ChangePassword(MemberInfo model, string password)
        {
            if (model.PasswordSalt.StartsWith("o"))
            {
                model.PasswordSalt = Guid.NewGuid().ToString("N").Substring(12);   //微信一键注册用户初次改密需要重新生成掩值
            }
            model.Password = GetPasswrodWithTwiceEncode(password, model.PasswordSalt);

            //var seller = Context.ManagerInfo.FirstOrDefault(a => a.UserName == model.UserName && a.ShopId != 0);
            var seller = DbFactory.Default.Get<ManagerInfo>().Where(a => a.UserName == model.UserName && a.ShopId != 0).FirstOrDefault();
            if (seller != null)
            {
                //seller.PasswordSalt = model.PasswordSalt;
                //seller.Password = model.Password;
                DbFactory.Default.Set<ManagerInfo>().Set(n => n.PasswordSalt, model.PasswordSalt).Set(n => n.Password, model.Password).Where(p => p.Id == seller.Id).Succeed();
            }
            //Context.SaveChanges();
            var result = DbFactory.Default.Update(model);
            string CACHE_USER_KEY = CacheKeyCollection.Member(model.Id);
            Core.Cache.Remove(CACHE_USER_KEY);
            string CACHE_Seller_KEY = CacheKeyCollection.Seller(model.Id);
            Core.Cache.Remove(CACHE_Seller_KEY);
            if (result > 0)
            {
                //消息通知
                var userMessage = new MessageUserInfo();
                userMessage.UserName = model.UserName;
                userMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnEditLoginPassWord(model.Id, userMessage));
            }
        }
        #endregion


        public void UpdateOpenIdBindMember(MemberOpenIdInfo model)
        {
            //更新绑定
            //var memberOpenIdInfos = Context.MemberOpenIdInfo.Where(d => d.OpenId == model.OpenId && d.ServiceProvider == model.ServiceProvider);
            var memberOpenIdInfos = DbFactory.Default.Get<MemberOpenIdInfo>().Where(d => d.OpenId == model.OpenId && d.ServiceProvider == model.ServiceProvider).ToList();
            //清理绑定
            //Context.MemberOpenIdInfo.Remove(d => d.UserId == model.UserId && d.OpenId == model.OpenId);
            if (memberOpenIdInfos.Count() > 0)
            {
                //Context.MemberOpenIdInfo.RemoveRange(memberOpenIdInfos);
                DbFactory.Default.Del<MemberOpenIdInfo>(memberOpenIdInfos);
            }
            //Context.MemberOpenIdInfo.Add(model);
            //Context.SaveChanges();
            DbFactory.Default.Add(model);
        }

        /// <summary>
        /// 根据用户id和平台获取会员openid信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appIdType"></param>
        /// <returns></returns>
        public MemberOpenIdInfo GetMemberOpenIdInfoByuserIdAndType(long userId, string serviceProvider)
        {
            //return this.Context.MemberOpenIdInfo.FirstOrDefault(p => p.UserId == userId && p.ServiceProvider == serviceProvider);
            return DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.UserId == userId && p.ServiceProvider == serviceProvider).FirstOrDefault();
        }
        /// <summary>
        /// 是否可以提现
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool CanWithdraw(long userId)
        {
            bool result = true;
            var c = DbFactory.Default.Get<CapitalInfo>().Where(d => d.MemId == userId).FirstOrDefault();
            if (c != null)
            {
                result = DbFactory.Default.Get<CapitalDetailInfo>().Where(d => d.CapitalID == c.Id && d.PresentAmount > 0).Count() == 0;
            }
            return result;
        }
        /// <summary>
        /// 同步添加分销会员信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="spreadId"></param>
        /// <returns></returns>
        private bool SyncAddDistributor(long memberId, long? spreadId)
        {
            bool result = false;
            spreadId = SiteSettingApplication.SiteSettings.DistributionIsEnable ? spreadId : 0;
            int dasv = DistributorStatus.Audited.GetHashCode();
            if (SiteSettingApplication.SiteSettings.DistributionIsEnable
                && !DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == memberId).Exist())
            {
                var super = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == spreadId && d.DistributionStatus == dasv).FirstOrDefault();
                if (super == null)
                {
                    spreadId = 0;
                }
                var uobj = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == memberId).FirstOrDefault();
                var duobj = new DistributorInfo()
                {
                    MemberId = memberId,
                    DistributionStatus = (int)DistributorStatus.UnApply,    //普通会员
                    SuperiorId = spreadId ?? 0,
                    RegDate = DateTime.Now
                    //ShopLogo = uobj.Photo,
                };
                DbFactory.Default.Add(duobj);
                if (spreadId.HasValue && spreadId > 0)
                {
                    //同步下级数量
                    int subnum = 0;
                    try
                    {
                        var _tmp = DbFactory.Default.Get<DistributorInfo>()
                            .Where(d => d.SuperiorId == spreadId.Value)
                            .Select(d => new { result = d.MemberId.ExCount(false) }).FirstOrDefault<int?>();
                        subnum = _tmp ?? 0;
                    }
                    catch
                    {
                        subnum = 0;
                    }
                    DbFactory.Default.Set<DistributorInfo>()
                            .Where(d => d.MemberId == spreadId.Value)
                            .Set(d => d.SubNumber, subnum)
                            .Succeed();

                    //发送短信通知
                    Task.Factory.StartNew(() =>
                    {
                        ServiceProvider.Instance<IMessageService>.Create.SendMessageOnDistributorNewJoin(spreadId.Value, uobj.UserName, duobj.RegDate.Value, SiteSettingApplication.SiteSettings.SiteName);
                    });
                }
            }
            return result;
        }
        public bool containsEmoji(string source)
        {
            char[] item = source.ToCharArray();
            for (int i = 0; i < source.Length; i++)
            {
                if (isEmojiCharacter(item[i]))
                    return true; //do nothing，判断到了这里表明，确认有表情字符
            }
            return false;
        }

        private bool isEmojiCharacter(char codePoint)
        {
            return (codePoint == 0x0) ||
                    (codePoint == 0x9) ||
                    (codePoint == 0xA) ||
                    (codePoint == 0xD) ||
                    ((codePoint >= 0x20) && (codePoint <= 0xD7FF)) ||
                    ((codePoint >= 0xE000) && (codePoint <= 0xFFFD)) ||
                    ((codePoint >= 0x10000) && (codePoint <= 0x10FFFF));
        }

        public string NickFilterEmoji(string source)
        {
            if (!containsEmoji(source))
                return source;//如果不包含，直接返回
            //到这里铁定包含
            StringBuilder buf = null;
            char[] item = source.ToCharArray();
            for (int i = 0; i < source.Length; i++)
            {
                char codePoint = item[i];
                if (!isEmojiCharacter(codePoint))
                {
                    if (buf == null)
                        buf = new StringBuilder(source.Length);
                    buf.Append(codePoint);
                }
            }
            if (buf == null)
                return source;//如果没有找到 emoji表情，则返回源字符串
            else
            {
                if (buf.Length == source.Length)
                {
                    buf = null;//这里的意义在于尽可能少的toString，因为会重新生成字符串
                    return source;
                }
                else
                    return buf.ToString();
            }

        }
    }
}


