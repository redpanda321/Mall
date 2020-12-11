using AutoMapper;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: OnMallStartMethod(typeof(Mall.Application.MemberApplication), "InitMessageQueue")]
namespace Mall.Application
{
    public class MemberApplication:BaseApplicaion<IMemberService>
    {
        #region 静态字段
      //  private static IOrderService _orderService =  EngineContext.Current.Resolve<IOrderService>();
      //  private static IRefundService _refundService =  EngineContext.Current.Resolve<IRefundService>();
      //  private static IMemberGradeService _memberGradeService =  EngineContext.Current.Resolve<IMemberGradeService>();



        private static IOrderService _orderService =  EngineContext.Current.Resolve<IOrderService>();
        private static IRefundService _refundService =  EngineContext.Current.Resolve<IRefundService>();
        private static IMemberGradeService _memberGradeService =  EngineContext.Current.Resolve<IMemberGradeService>();
        #endregion

        #region 插件相关
        /// <summary>
        /// 获取会员认证情况
        /// </summary>
        /// <param name="UserId">会员ID</param>
        /// <returns></returns>
        public static MemberAccountSafety GetMemberAccountSafety(long UserId)
        {
            MemberAccountSafety model = new MemberAccountSafety();
            model.UserId = UserId;
            List<Entities.MemberContactInfo> lmMemberContactsInfo = MessageApplication.GetMemberContactsInfo(UserId);

            foreach (var item in lmMemberContactsInfo)
            {
                if (item.ServiceProvider.Contains("SMS"))
                {
                    model.Phone = item.Contact;
                    model.BindPhone = true;
                }
                else if (item.ServiceProvider.Contains("Email"))
                {
                    model.Email = item.Contact;
                    model.BindEmail = true;
                }
            }

            return model;
        }

        /// <summary>
        /// 更新插件信息
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateMemberContacts(MemberContacts model)
        {
           // Mapper.CreateMap<MemberContacts, Entities.MemberContactInfo>();
          //  var mm = Mapper.Map<MemberContacts, Entities.MemberContactInfo>(model);
            var mm = model.Map< Entities.MemberContactInfo>();


            MessageApplication.UpdateMemberContacts(mm);
        }

        /// <summary>
        /// 根据插件类型和ID和目标获取信息
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="contact"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MemberContacts GetMemberContactsInfo(string pluginId, string contact, Entities.MemberContactInfo.UserTypes type)
        {
            var model = MessageApplication.GetMemberContactsInfo(pluginId, contact, type);
       //     Mapper.CreateMap<Entities.MemberContactInfo, MemberContacts>();
        //    return Mapper.Map<Entities.MemberContactInfo, MemberContacts>(model);
            return model.Map<MemberContacts>();

        }

        /// <summary>
        /// 发送验证码，认证管理员
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="UserName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static SendMemberCodeReturn SendMemberCode(string pluginId, string destination, string UserName, string SiteName)
        {
            //判断号码是否绑定
            if (MessageApplication.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
            {
                return SendMemberCodeReturn.repeat;
            }
            else
            {
                var timeout = CacheKeyCollection.MemberPluginReBindTime(UserName, pluginId); //验证码超时时间
                if (Core.Cache.Exists(timeout))
                {
                    return SendMemberCodeReturn.limit;
                }
                var checkCode = new Random().Next(10000, 99999);
                Log.Debug("Code:" + checkCode);
                var cacheTimeout = DateTime.Now.AddMinutes(15);
                Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(UserName, pluginId + destination), checkCode.ToString(), cacheTimeout);
                var user = new MessageUserInfo() { UserName = UserName, SiteName = SiteName, CheckCode = checkCode.ToString() };
                MessageApplication.SendMessageCode(destination, pluginId, user);
                Core.Cache.Insert(CacheKeyCollection.MemberPluginReBindTime(UserName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
                return SendMemberCodeReturn.success;
            }
        }

        /// <summary>
        /// 验证码验证，认证管理员
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int CheckMemberCode(string pluginId, string code, string destination, long userId)
        {
            var member = Service.GetMember(userId);
            int result = 0;
            var cache = CacheKeyCollection.MemberPluginCheck(member.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get<string>(cache);
            if (cacheCode != null && cacheCode == code)
            {
                if (MessageApplication.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
                {
                    result = -1;
                }
                else
                {
                    if (pluginId.ToLower().Contains("email"))
                    {
                        member.Email = destination;
                    }
                    else if (pluginId.ToLower().Contains("sms"))
                    {
                        member.CellPhone = destination;
                    }

                    Service.UpdateMember(member);

                    MessageApplication.UpdateMemberContacts(new Entities.MemberContactInfo() { Contact = destination, ServiceProvider = pluginId, UserId = userId, UserType = Entities.MemberContactInfo.UserTypes.General });

                    Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(member.UserName, pluginId + destination));
                    Core.Cache.Remove(CacheKeyCollection.Member(userId));//移除用户缓存
                    Core.Cache.Remove("Rebind" + userId);
                    result = 1;
                }
            }
            return result;
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="UserName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static bool SendCode(string pluginId, string destination, string UserName, string SiteName)
        {
            var timeout = CacheKeyCollection.MemberPluginAuthenticateTime(UserName, pluginId); //验证码超时时间
            if (Core.Cache.Exists(timeout))
            {
                return false;
            }
            var checkCode = new Random().Next(10000, 99999);
            Log.Debug("Code:" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginAuthenticate(UserName, pluginId + destination), checkCode.ToString(), cacheTimeout);
            var user = new MessageUserInfo() { UserName = UserName, SiteName = SiteName, CheckCode = checkCode.ToString() };
            MessageApplication.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginAuthenticateTime(UserName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
            return true;
        }

        /// <summary>
        /// 验证码验证
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int CheckCode(string pluginId, string code, string destination, long userId)
        {
            var member = Service.GetMember(userId);
            int result = 0;
            var cache = CacheKeyCollection.MemberPluginAuthenticate(member.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get<string>(cache);
            if (cacheCode != null && cacheCode == code)
            {
                Core.Cache.Remove(CacheKeyCollection.MemberPluginAuthenticate(member.UserName, pluginId + destination));
                Core.Cache.Remove(CacheKeyCollection.Member(userId));//移除用户缓存
                Core.Cache.Remove("Rebind" + userId);
                result = 1;
            }
            return result;
        }
        #endregion

        #region 会员相关
        /// <summary>
        /// 获取会员信息
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static Members GetMembers(long UserId)
        {
            var model = Service.GetMember(UserId);
            // var m = Mapper.Map<MemberInfo, Members>(model);
            var m = model.Map<Members>();


            m.Photo = model.PhotoUrl;

            var inviteUser = Service.GetMember(model.InviteUserId);
            if (inviteUser != null)
                m.InviteUserName = inviteUser.UserName;

            m.MemberLabels = MemberLabelApplication.GetMemberLabelList(UserId).Models;
            var userInte = MemberIntegralApplication.GetMemberIntegral(UserId);
            var userGrade = MemberGradeApplication.GetMemberGradeByUserIntegral(userInte.HistoryIntegrals);
            m.GradeName = userGrade.GradeName;//方法内部包含获取等级的方法
            return m;
        }

        /// <summary>
        /// 根据用户id获取用户信息
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static List<DTO.Members> GetMembers(List<long> userIds)
        {
            var result = Service.GetMembers(userIds).Map<List<DTO.Members>>();
            return result;
        }

        /// <summary>
        /// 更新会员信息
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateMember(Members model)
        {
            var mm = Service.GetMember(model.Id);
            mm.Email = model.Email;
            mm.CellPhone = model.CellPhone;
            mm.RealName = model.RealName;
            Service.UpdateMember(mm);
        }

        public static void UpdateOpenIdBindMember(Entities.MemberOpenIdInfo model)
        {
            Service.UpdateOpenIdBindMember(model);
        }

        public static string GetLogo()
        {
            return SiteSettingApplication.SiteSettings.MemberLogo;
        }

        /// <summary>
        /// 注册一个会员()
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="mobile">手机号码</param>
        public static Entities.MemberInfo Register(string username, string password,int platform, string mobile = "", string email = "", long introducer = 0)
        {
            return Service.Register(username, password, platform, mobile, email, introducer);
        }

        /// <summary>
        /// 注册并绑定一个会员
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="serviceProvider">服务提供商</param>
        /// <param name="openId">OpenId</param>
        /// <param name="headImage">头像地址</param>
        public static Entities.MemberInfo Register(string username, string password, string serviceProvider, string openId, int platform, string sex = null, string headImage = null, long introducer = 0, string nickname = null, string city = null, string province = null, string unionid = null)
        {
            return Service.Register(username, password, serviceProvider, openId, platform, sex = null, headImage = null, introducer = 0, nickname = null, city = null, province = null, unionid = null);
        }
        /// <summary>
        /// 注册并绑定一个会员(传model)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Entities.MemberInfo Register(OAuthUserModel model)
        {
            return Service.Register(model);
        }
        /// <summary>
        /// 快速注册
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="openId">OpenId</param>
        /// <param name="nickName">昵称</param>
        /// <param name="realName">真实姓名</param>
        /// <param name="serviceProvider">服务提供商</param>
        /// <returns></returns>
		public static Entities.MemberInfo QuickRegister(string username, string realName, string nickName, string serviceProvider, string openId, string unionid, int platform, string sex = null, string headImage = null, Entities.MemberOpenIdInfo.AppIdTypeEnum appidtype = Entities.MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionopenid = null, string city = null, string province = null)
        {
            return Service.QuickRegister(username, realName, nickName, serviceProvider, openId, platform, unionid, sex = null, headImage = null, appidtype, unionopenid, city, province);
        }

        /// <summary>
        /// 绑定会员
        /// </summary>
        /// <param name="userId">会员id</param>
        /// <param name="serviceProvider">信任登录服务提供商</param>
        /// <param name="openId">OpenId</param>
        /// <param name="headImage">头像</param>
        public static void BindMember(long userId, string serviceProvider, string openId, string sex = null, string headImage = null, string unionid = null, string unionopenid = null, string city = null, string province = null)
        {
            Service.BindMember(userId, serviceProvider, openId, sex, headImage, unionid, unionopenid, city, province);
        }
        /// <summary>
        /// 绑定会员（增加标记，是否是可以支付的openid）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="openId"></param>
        /// <param name="AppidType"></param>
        /// <param name="headImage"></param>
        public static void BindMember(long userId, string serviceProvider, string openId, Entities.MemberOpenIdInfo.AppIdTypeEnum AppidType, string sex = null, string headImage = null, string unionid = null)
        {
            Service.BindMember(userId, serviceProvider, openId, AppidType, sex, headImage, unionid);
        }

        public static void BindMember(OAuthUserModel model)
        {
            Service.BindMember(model);
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="payPwd"></param>
        public static bool VerificationPayPwd(long memid, string payPwd)
        {
            return Service.VerificationPayPwd(memid, payPwd);
        }

        /// <summary>
        /// 是否有支付密码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool HasPayPassword(long id)
        {
            return Service.HasPayPassword(id);
        }

        /// <summary>
        /// 检查用户名是否重复
        /// </summary>
        /// <param name="username">待检查的用户名</param>
        /// <returns></returns>
        public static bool CheckMemberExist(string username)
        {
            return Service.CheckMemberExist(username);
        }

        /// <summary>
        /// 检查手机号码是否重复
        /// </summary>
        /// <param name="mobile">待检查的手机号码</param>
        /// <returns></returns>
        public static bool CheckMobileExist(string mobile)
        {
            return Service.CheckMemberExist(mobile);
        }
        /// <summary>
        /// 检查邮箱是否重复
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool CheckEmailExist(string email)
        {
            return Service.CheckEmailExist(email);
        }

        /// <summary>
        /// 更新会员信息新方法必须
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateMemberInfo(MemberUpdate model)
        {
            var mm = Service.GetMember(model.Id);//先查询
                                                         // mm.Email = model.Email;
                                                         //  mm.CellPhone = model.CellPhone;
            mm.RealName = model.RealName;
            mm.Nick = model.Nick;
            mm.QQ = model.QQ;
            if (model.BirthDay.HasValue)
                mm.BirthDay = model.BirthDay;
            mm.Occupation = model.Occupation;
            mm.Sex = model.Sex;
            if (!string.IsNullOrWhiteSpace(model.Photo))
                mm.Photo = model.Photo;
            Service.UpdateMemberInfo(mm);
        }

        /// <summary>
        /// 更改会员密码
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <param name="password">会员密码</param>
        public static void ChangePassword(long id, string password)
        {
            Service.ChangePassword(id, password);
        }

        /// <summary>
        /// 根据用户名修改密码
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        public static void ChangePassword(string name, string password)
        {
            Service.ChangePassword(name, password);
        }

        /// <summary>
        /// 修改支付密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        public static void ChangePayPassword(long id, string password)
        {
            Service.ChangePayPassword(id, password);
        }

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <param name="id"></param>
        public static void LockMember(long id)
        {
            Service.LockMember(id);
        }

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <param name="id"></param>
        public static void UnLockMember(long id)
        {
            Service.UnLockMember(id);
        }

        /// <summary>
        /// 删除一个会员
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteMember(long id)
        {
            Service.DeleteMember(id);
        }


        /// <summary>
        /// 批量删除会员
        /// </summary>
        /// <param name="ids">批量ID数组</param>
        public static void BatchDeleteMember(long[] ids)
        {
            Service.BatchDeleteMember(ids);
        }

        /// <summary>
        /// 批量锁定
        /// </summary>
        /// <param name="ids"></param>
        public static void BatchLock(long[] ids)
        {
            Service.BatchLock(ids);
        }

        /// <summary>
        /// 根据查询条件分页获取会员信息(兼容老版本)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<MemberInfo> GetMembers(MemberQuery query)
        {
            return Service.GetMembers(query);
        }



        /// <summary>
        /// 根据查询条件分页获取会员信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<Members> GetMemberList(MemberQuery query)
        {
            if (query.GradeId.HasValue)
            {
                var expenditureRange = GetMemberGradeRange(query.GradeId.Value);
                query.MinIntegral = expenditureRange.MinIntegral;
                query.MaxIntegral = expenditureRange.MaxIntegral;
            }
            var list = Service.GetMembers(query);
            //    var members = Mapper.Map<QueryPageModel<Members>>(list);
            var members = list.Map<QueryPageModel<Members>>();

            var grades = MemberGradeApplication.GetMemberGrades();
            foreach (var m in members.Models)
            {
                var memberIntegral = MemberIntegralApplication.GetMemberIntegral(m.Id);
                m.GradeName = MemberGradeApplication.GetMemberGradeByIntegral(grades, memberIntegral.HistoryIntegrals).GradeName;
                if (memberIntegral != null)
                {
                    m.AvailableIntegral = memberIntegral.AvailableIntegrals;
                    m.HistoryIntegral = memberIntegral.HistoryIntegrals;
                }
                m.PlatformText = m.Platform.ToDescription();
                m.IconSrc = GetIconSrc(m.Platform);
            }
            return members;
        }
        public static int GetMemberCount(MemberQuery query)
        {
            if (query.GradeId.HasValue)
            {
                var expenditureRange = GetMemberGradeRange(query.GradeId.Value);
                query.MinIntegral = expenditureRange.MinIntegral;
                query.MaxIntegral = expenditureRange.MaxIntegral;
            }
            return BaseApplicaion<IMemberService>.Service.GetMemberCount(query);
        }

        /// <summary>
        /// 获取会员来源图标地址
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        static string GetIconSrc(PlatformType platform)
        {
            if (platform == PlatformType.IOS || platform == PlatformType.Android)
                return "/images/app.png";
            return string.Format("/images/{0}.png", platform.ToString());
        }

        /// <summary>
        /// 通过会员等级ID获取会员消费范围
        /// </summary>
        /// <param name="gradeId"></param>
        /// <returns></returns>
        public static GradeIntegralRange GetMemberGradeRange(long gradeId)
        {
            return Service.GetMemberGradeRange(gradeId);
        }



        /// <summary>
        /// 获取查询的会员信息列表
        /// </summary>
        /// <param name="keyWords">关键字</param>
        /// <returns></returns>
        public static List<MemberInfo> GetMembers(bool? status, string keyWords)
        {
            return Service.GetMembers(status, keyWords);
        }

        /// <summary>
        /// 获取一个会员信息
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <returns></returns>
        public static Entities.MemberInfo GetMember(long id)
        {
            return Service.GetMember(id);
        }
        /// <summary>
        /// 获取用户折扣
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static decimal GetMemberDiscount(long member)
        {
            if (member <= 0) return 1;
            var integral = MemberIntegralApplication.GetMemberIntegral(member);
            if (integral == null) return 1;
            return Service.GetMemberDiscount(integral.HistoryIntegrals);
        }
        /// <summary>
        /// 根据用户id和类型获取会员openid信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appIdType"></param>
        /// <returns></returns>
        public static DTO.MemberOpenId GetMemberOpenIdInfoByuserId(long userId, Entities.MemberOpenIdInfo.AppIdTypeEnum appIdType, string serviceProvider = "")
        {
            var model = Service.GetMemberOpenIdInfoByuserId(userId, appIdType, serviceProvider);
            //  return AutoMapper.Mapper.Map<DTO.MemberOpenId>(model);

            return model.Map<DTO.MemberOpenId>();

        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        public static Entities.MemberInfo Login(string username, string password)
        {
            return Service.Login(username, password);
        }

        /// <summary>
        /// 修改最后登录时间
        /// </summary>
        /// <param name="id"></param>
        public static void UpdateLastLoginDate(long id)
        {
            Service.UpdateLastLoginDate(id);
        }

        //根据用户名获取用户信息
        public static Entities.MemberInfo GetMemberByName(string userName)
        {
            return Service.GetMemberByName(userName);
        }

        /// <summary>
        /// 获取用户个人中心信息
        /// </summary>
        /// <returns></returns>
        /// <param name="id">用户id</param>
        /// <param name="isVirtual">是否区别虚拟订单，默认null不区分，false只查询实体订单</param>
        public static UserCenterModel GetUserCenterModel(long id, bool? isVirtual = null)
        {
            UserCenterModel model = new UserCenterModel();

            model.UserCoupon = GetAvailableCouponCount(id);
            model.Intergral = MemberIntegralApplication.GetAvailableIntegral(id);
            var grade = MemberGradeApplication.GetGradeByMember(id);
            model.GradeId = grade.Id;
            model.GradeName = grade.GradeName;

            //订单汇总
            var orderStatic = StatisticApplication.GetMemberOrderStatistic(id,true, isVirtual);
            model.RefundCount = orderStatic.RefundCount;
            model.WaitPayOrders = orderStatic.WaitingForPay;
            model.WaitReceivingOrders = orderStatic.WaitingForRecieve;
            model.WaitDeliveryOrders = orderStatic.WaitingForDelivery;
            model.WaitEvaluationOrders = orderStatic.WaitingForComments;

            //收藏商品
            model.FollowProductCount = FavoriteApplication.GetFavoriteCountByUser(id);
            if (model.FollowProductCount > 0)
            {
                var favorites = FavoriteApplication.GetFavoriteByUser(id, 4);
                var products = ProductManagerApplication.GetProducts(favorites.Select(p => p.ProductId));
                model.FollwProducts = favorites.Select(a =>
                {
                    var product = products.FirstOrDefault(p => p.Id == a.ProductId);
                    return new FollowProduct()
                    {
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        Price = product.MinSalePrice,
                        ImagePath = product.ImagePathUrl
                    };
                }).ToList();
            }

            //收藏门店
            model.FollowShopsCount = FavoriteApplication.GetFavoriteShopCount(id);
            if (model.FollowShopsCount > 0)
            {
                model.FollowShops = FavoriteApplication.GetFavoriteShop(id, 4).Select(a =>
                 {
                     var shop = ShopApplication.GetShop(a.ShopId);
                     var vShop = VshopApplication.GetVShopByShopId(shop.Id);
                     return new FollowShop
                     {
                         ShopName = shop.ShopName,
                         Logo = vShop != null ? vShop.StrLogo : shop.Logo,
                         ShopID = a.ShopId
                     };
                 }).ToList();
            }

            model.Orders = OrderApplication.GetUserOrders(id, 3);
            var cartService = GetService<ICartService>();
            ////购物车
            model.FollowShopCartsCount = CartApplication.GetCartCount(id);
            if (model.FollowShopCartsCount > 0)
            {
                var carts = CartApplication.GetCarts(id, 4);
                var products = ProductManagerApplication.GetProductsByIds(carts.Select(p => p.ProductId));
                model.FollowShopCarts = products.Select(p => new FollowShopCart
                {
                    ImagePath = p.ImagePath,
                    ProductName = p.ProductName,
                    ProductId = p.Id
                }).ToList();
            }
            return model;
        }

        /// <summary>
        /// 根据服务商返回的OpenId获取系统中对应的用户
        /// </summary>
        /// <param name="serviceProvider">服务商名称</param>
        /// <param name="openId">OpenId</param>
        /// <returns></returns>
        public static MemberInfo GetMemberByOpenId(string serviceProvider, string openId)
        {
            return Service.GetMemberByOpenId(serviceProvider, openId);
        }
        /// <summary>
        /// 根据UnionId、Provider取用户信息
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="UnionId"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberByUnionIdAndProvider(string serviceProvider, string UnionId)
        {
            return Service.GetMemberByUnionId(serviceProvider, UnionId);
        }
        /// <summary>
        /// 根据UnionId取用户信息
        /// </summary>
        /// <param name="UnionId"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberByUnionId(string UnionId)
        {
            return Service.GetMemberByUnionId(UnionId);
        }

        public static void DeleteMemberOpenId(long userid, string openid)
        {
            Service.DeleteMemberOpenId(userid, openid);
        }

        public static MemberOpenIdInfo GetMemberOpenIdInfoByOpenIdOrUnionId(string openId = "", string unionId = "")
        {
            return Service.GetMemberOpenIdInfoByOpenIdOrUnionId(openId, unionId);
        }

        /// <summary>
        /// 从手机号或者邮箱信息来获取用户信息
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static Entities.MemberInfo GetMemberByContactInfo(string contact)
        {
            return Service.GetMemberByContactInfo(contact);
        }

        /// <summary>
        /// 检查是邮箱或手机是否被占用
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="contact"></param>
        /// <param name="userType"></param>
        public static void CheckContactInfoHasBeenUsed(string serviceProvider, string contact, MemberContactInfo.UserTypes userType = MemberContactInfo.UserTypes.General)
        {
            Service.CheckContactInfoHasBeenUsed(serviceProvider, contact, userType);
        }

        public static List<MemberLabelInfo> GetMembersByLabel(long labelid)
        {
            return Service.GetMembersByLabel(labelid);
        }

        public static IEnumerable<MemberLabelInfo> GetMemberLabels(long memid)
        {
            return Service.GetMemberLabels(memid);
        }
        /// <summary>
        /// 设置会员标签(移除原标签)
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="labelids"></param>
        public static void SetMemberLabel(long userid, IEnumerable<long> labelids)
        {
            Service.SetMemberLabel(userid, labelids);
        }
        /// <summary>
        /// 设置多会员标签，只累加，不移除原标签
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="labelids"></param>
        public static void SetMembersLabel(long[] userid, IEnumerable<long> labelids)
        {
            Service.SetMembersLabel(userid, labelids);
        }

        public static IEnumerable<int> GetAllTopRegion()
        {
            return Service.GetAllTopRegion();
        }

        /// <summary>
        /// 给用户加积分
        /// </summary>
        /// <param name="member"></param>
        public static void AddIntegel(MemberInfo member)
        {
            Service.AddIntegel(member);
        }
        #endregion

            #region 方法
            /// <summary>
            /// 会员购买力列表
            /// </summary>
            /// <param name="query"></param>
            /// <returns></returns>
        public static QueryPageModel<DTO.MemberPurchasingPower> GetPurchasingPowerMember(MemberPowerQuery query)
        {
            var result = Service.GetPurchasingPowerMember(query);
            var model = new QueryPageModel<MemberPurchasingPower>
            {
                Total = result.Total,
                //     Models = AutoMapper.Mapper.Map<List<DTO.MemberPurchasingPower>>(result.Models)
                Models = result.Models.Map<List<DTO.MemberPurchasingPower>>()


            };

            var userIds = model.Models.Select(p => p.Id).ToList();

            var memberCategorys = Service.GetMemberBuyCategoryByUserIds(userIds);

            var grades = MemberGradeApplication.GetMemberGrades();

            var integrals = MemberIntegralApplication.GetMemberIntegrals(userIds);
            foreach (var item in model.Models)
            {
                var intergral = integrals.Where(a => a.MemberId == item.Id).Select(a => a.HistoryIntegrals).FirstOrDefault();
                //填充等级数据
                item.GradeName = MemberGradeApplication.GetMemberGradeByIntegral(grades, intergral).GradeName;

                //填充分类数据
                var categoryNames = memberCategorys.Where(p => p.UserId == item.Id).Select(p => p.CategoryName).Take(3).ToArray();
                if (categoryNames.Length == 0)
                    continue;
                item.CategoryNames = string.Join(",", categoryNames);
            }

            return model;
        }

        /// <summary>
        /// 平台获取会员分组信息
        /// </summary>
        /// <returns></returns>
        public static DTO.MemberGroup GetPlatformMemberGroup()
        {
            var memberGroupInfo = Service.GetMemberGroup();
            return MemberInfoToDto(memberGroupInfo);
        }


        /// <summary>
        /// 根据会员ID发送短信
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="sendCon"></param>
        public static void SendMsgByUserIds(long[] userIds, string sendCon)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>("Mall.Plugin.Message.SMS");

            var members = Service.GetMembers(userIds.ToList());

            string[] dests = members.Select(e => e.CellPhone).ToArray();
            List<string> phones = new List<string>();
            foreach (var dest in dests)
            {
                if (messagePlugin.Biz.CheckDestination(dest))
                {
                    phones.Add(dest);
                }
            }
            if (phones.Count == 0)
            {
                throw new MallException("可发送的对象未空！");
            }
            var siteName = SiteSettingApplication.SiteSettings.SiteName;
            sendCon = sendCon + "【" + siteName + "】";
            //【TODO使用队列】
            foreach (var phone in phones)
            {
                messagePlugin.Biz.SendTestMessage(phone, sendCon);
            }

            var sendRecord = new SendMessageRecordInfo
            {
                ContentType = WXMsgType.text,
                MessageType = MsgType.SMS,
                SendContent = sendCon == null ? "" : sendCon,
                SendState = 1,
                SendTime = DateTime.Now,
                ToUserLabel = "会员分组"
            };
            WXMsgTemplateApplication.AddSendRecord(sendRecord);
        }


        /// <summary>
        /// 发送短信，根据搜索条件
        /// </summary>
        /// <param name="query"></param>
        /// <param name="couponIds"></param>
        public static void SendMsg(MemberPowerQuery query, string sendCon, string labelinfos = "会员分组", MemberQuery memberQuery = null)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>("Mall.Plugin.Message.SMS");
            int result = 0;

            //循环执行发送
            for (int i = 0; i < int.MaxValue; i++)
            {
                string[] dests = new string[0];
                if (query != null)
                {
                    query.PageNo = i + 1;
                    query.PageSize = 1000;
                    var members = MemberApplication.GetPurchasingPowerMember(query);
                    dests = members.Models.Select(e => e.CellPhone).ToArray();
                }
                else if(memberQuery != null)
                {
                    memberQuery.PageNo = i + 1;
                    memberQuery.PageSize = 1000;
                    var memberlist = GetMemberList(memberQuery);
                    dests = memberlist.Models.Select(e => e.CellPhone).ToArray();
                }
                List<string> phones = new List<string>();
                foreach (var dest in dests)
                {
                    if (messagePlugin.Biz.CheckDestination(dest))
                    {
                        phones.Add(dest);
                        result++;
                    }
                }

                messagePlugin.Biz.SendMessages(phones.ToArray(), sendCon);

                if (!dests.Any())
                    break;
            }

            //记录发送历史
            if (result > 0)
            {
                var sendRecord = new SendMessageRecordInfo
                {
                    ContentType = WXMsgType.wxcard,
                    MessageType = MsgType.Coupon,
                    SendContent = "",
                    SendState = 1,
                    SendTime = DateTime.Now,
                    ToUserLabel = labelinfos //"会员分组"
                };
                WXMsgTemplateApplication.AddSendRecord(sendRecord);
            }
            else
            {
                throw new MallException("此标签下无符发送的会员;");
            }

        }

        /// <summary>
        /// 指定用户发送微信消息
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="msgtype"></param>
        /// <param name="mediaid"></param>
        /// <param name="msgcontent"></param>
        public static void SendWeiMessageByUserIds(long[] userIds, string msgtype, string mediaid = "", string msgcontent = "")
        {
            WXMsgType type;
            if (Enum.TryParse<WXMsgType>(msgtype, out type))
            {
                if (userIds.Length < 2)
                {
                    throw new MallException("群发消息，请至少选中两个以上用户！");
                }

                var openIds = Service.GetOpenIdByUserIds(userIds);
                if (openIds.Count() != userIds.Length && openIds.Count() < 2)
                {
                    throw new MallException("有用户未关注公众号，发送失败！");
                }
                var set = SiteSettingApplication.SiteSettings;
                var result = WXMsgTemplateApplication.SendWXMsg(openIds, type, msgcontent, mediaid, set.WeixinAppId, set.WeixinAppSecret);
                if (result.errCode == "0" || result.errMsg.Contains("success"))
                {
                    SendMessageRecordInfo sendRecord = new SendMessageRecordInfo()
                    {
                        ContentType = type,
                        MessageType = MsgType.WeiXin,
                        SendContent = msgcontent,
                        SendTime = DateTime.Now,
                        ToUserLabel = "会员分组",
                        SendState = 1
                    };
                    WXMsgTemplateApplication.AddSendRecord(sendRecord);
                }
                else
                {
                    throw new MallException(result.errCode);
                }
            }
        }


        /// <summary>
        /// 指定用户发送微信消息
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="msgtype"></param>
        /// <param name="mediaid"></param>
        /// <param name="msgcontent"></param>
        public static void SendWeiMessage(MemberPowerQuery query, string msgtype, string mediaid = "", string msgcontent = "", MemberQuery memberQuery = null)
        {
            WXMsgType type;
            if (Enum.TryParse<WXMsgType>(msgtype, out type))
            {
                List<string> allopenIds = new List<string>();
                var isMember = memberQuery != null;
                //循环执行发送
                for (int i = 0; i < int.MaxValue; i++)
                {
                    if (isMember)
                    {
                        memberQuery.PageNo = i + 1;
                        memberQuery.PageSize = 1000;
                        var members = MemberApplication.GetMemberList(memberQuery);
                        var userIds = members.Models.Select(p => p.Id).ToArray();
                        var openIds = Service.GetOpenIdByUserIds(userIds);
                        foreach (var item in openIds)
                        {
                            allopenIds.Add(item);
                        }
                        if (members.Models.Count == 0)
                            break;
                    }
                    else
                    {
                        query.PageNo = i + 1;
                        query.PageSize = 1000;
                        var members = MemberApplication.GetPurchasingPowerMember(query);
                        var userIds = members.Models.Select(p => p.Id).ToArray();
                        var openIds = Service.GetOpenIdByUserIds(userIds);
                        foreach (var item in openIds)
                        {
                            allopenIds.Add(item);
                        }
                        if (members.Models.Count == 0)
                            break;
                    }
                }

                var set = SiteSettingApplication.SiteSettings;
                var result = WXMsgTemplateApplication.SendWXMsg(allopenIds, type, msgcontent, mediaid, set.WeixinAppId, set.WeixinAppSecret);
                if (result.errCode == "0" || result.errMsg.Contains("success"))
                {
                    SendMessageRecordInfo sendRecord = new SendMessageRecordInfo()
                    {
                        ContentType = type,
                        MessageType = MsgType.WeiXin,
                        SendContent = msgcontent,
                        SendTime = DateTime.Now,
                        ToUserLabel = "会员分组",
                        SendState = 1
                    };
                    WXMsgTemplateApplication.AddSendRecord(sendRecord);
                }
                else
                {
                    throw new MallException(result.errCode);
                }
            }
        }
        /// <summary>
        /// 是否可以提现
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CanWithdraw(long userId)
        {
            bool result = SiteSettingApplication.SiteSettings.IsOpenWithdraw;
            result = result && Service.CanWithdraw(userId);
            return result;
        }
        #endregion


        public static int GetAvailableCouponCount(long userId)
        {
            var coupon = GetService<ICouponService>().GetAvailableCouponCountByUser(userId);
            var bouns = GetService<IShopBonusService>().GetAvailableBonusCountByUser(userId);
            return coupon + bouns;
        }
        
        /// <summary>
        /// 获取用户Id（因为有些会员冻结直接传的member是空，则根据用户名再获取下用户得到用户Id）
        /// </summary>
        /// <param name="member"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static long GetMemberUserIdOrShop(MemberInfo member, ISellerManager sellerManager)
        {
            long userId = 0;
            if (member != null && member.Id > 0)
                return member.Id;
            if (sellerManager != null)
            {
                var mem = Service.GetMemberByName(sellerManager.UserName);
                userId = (mem != null) ? mem.Id : 0;
            }
            
            return userId;
        }


        #region 静态方法
        /// <summary>
        /// 初始化队列消息
        /// </summary>
        public static void InitMessageQueue()
        {
            //MessageQueue.ListenTopic<long>(CommonConst.MESSAGEQUEUE_PAYSUCCESSED).OnMessageRecieved += PaySuccessed_OnMessageRecieved;
            //MessageQueue.ListenTopic<long>(CommonConst.MESSAGEQUEUE_REFUNDSUCCESSED).OnMessageRecieved += RefundSuccessed_OnMessageRecieved;
            Application.OrderApplication.OnOrderPaySuccessed += OrderService_OnOrderPaySuccessed;
            Application.RefundApplication.OnRefundSuccessed += RefundService_OnRefundSuccessed;
        }

        static void RefundService_OnRefundSuccessed(long refundId)
        {
            try
            {
                //var refundData = _refundService.GetOrderRefund(refundId);
                //var orderData = _orderService.GetOrder(refundData.OrderId);
                /////	_iMemberService.UpdateNetAmount(refundData.UserId, -refundData.Amount);//减少用户的净消费额

                //           //  OrderApplication.up
                //             if(refundData.RefundMode== OrderRefundInfo.OrderRefundMode.OrderRefund)
                //             {
                //                 _iMemberService.DecreaseMemberOrderNumber(refundData.UserId);//减少用户的下单量
                //             }
            }
            catch (Exception e)
            {
                Log.Error("处理退款成功消息出错", e);
            }
        }

        /// <summary>
        /// 付款成功事件会员信息处理
        /// </summary>
        /// <param name="orderId"></param>
        static void OrderService_OnOrderPaySuccessed(long orderId)
        {
            OrderService_ProcessingSuccessInformation(orderId);
        }

        /// <summary>
        /// 处理订单付款成功后会员信息处理
        /// </summary>
        /// <param name="orderId"></param>
        public static void OrderService_ProcessingSuccessInformation(long orderId, OrderInfo orderData = null)
        {
            try
            {
                //var orderData = _orderService.GetOrder(orderId);
                if (orderData == null || orderData.Id != orderId)
                    orderData = _orderService.GetOrder(orderId);
                Service.UpdateNetAmount(orderData.UserId, orderData.TotalAmount);
                Service.IncreaseMemberOrderNumber(orderData.UserId);
                Service.UpdateLastConsumptionTime(orderData.UserId, DateTime.Now);
                OrderApplication.DealDithOrderCategoryByUserId(orderId, orderData.UserId);
            }
            catch (Exception e)
            {
                Log.Error("处理付款成功消息出错", e);
            }
        }

        /// <summary>
        /// 增加会员表净消费金额
        /// </summary>
        public static void UpdateNetAmount(long UserId, decimal TotalAmount)
        {
            Service.UpdateNetAmount(UserId, TotalAmount);
        }
        /// <summary>
        /// 增加会员表下单数
        /// </summary>
        public static void IncreaseMemberOrderNumber(long UserId)
        {
            Service.IncreaseMemberOrderNumber(UserId);
        }
        ///// <summary>
        ///// 退款成功消息
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="args"></param>
        //static void RefundSuccessed_OnMessageRecieved(object sender, MQArgs<long> args)
        //{
        //	try
        //	{
        //		var refundId = args.Data;
        //		var refundData = _refundService.GetOrderRefund(refundId);
        //		var orderData = _orderService.GetOrder(refundData.OrderId);

        //		_iMemberService.UpdateNetAmount(orderData.UserId, -refundData.Amount);
        //	}
        //	catch(Exception e)
        //	{
        //		Log.Error("处理退款成功消息出错", e);
        //	}
        //}

        ///// <summary>
        ///// 付款成功消息
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="args"></param>
        //static void PaySuccessed_OnMessageRecieved(object sender, MQArgs<long> args)
        //{
        //	try
        //	{
        //		var orderId = args.Data;
        //		var orderData = _orderService.GetOrder(orderId);

        //		_iMemberService.UpdateNetAmount(orderData.UserId, orderData.TotalAmount);
        //		_iMemberService.UpdateLastConsumptionTime(orderData.UserId, DateTime.Now);
        //	}
        //	catch(Exception e)
        //	{
        //		Log.Error("处理付款成功消息出错", e);
        //	}
        //}
        #endregion

        #region 私有方法
        /// <summary>
        /// 会员分组数据实体到DTO转换
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static DTO.MemberGroup MemberInfoToDto(List<Entities.MemberGroupInfo> model)
        {
            DTO.MemberGroup memberGroup = new MemberGroup();

            #region 活跃会员
            //一个月活跃会员
            var memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.ActiveOne).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.ActiveOne = 0;
            else
                memberGroup.ActiveOne = memberGroupInfo.Total;

            //三个月活跃会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.ActiveThree).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.ActiveThree = 0;
            else
                memberGroup.ActiveThree = memberGroupInfo.Total;

            //六个月活跃会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.ActiveSix).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.ActiveSix = 0;
            else
                memberGroup.ActiveSix = memberGroupInfo.Total;
            #endregion

            #region 沉睡会员
            // 三个月沉睡会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingThree).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.SleepingThree = 0;
            else
                memberGroup.SleepingThree = memberGroupInfo.Total;

            // 六个月沉睡会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingSix).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.SleepingSix = 0;
            else
                memberGroup.SleepingSix = memberGroupInfo.Total;

            // 九个月沉睡会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingNine).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.SleepingNine = 0;
            else
                memberGroup.SleepingNine = memberGroupInfo.Total;

            // 十二个月沉睡会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingTwelve).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.SleepingTwelve = 0;
            else
                memberGroup.SleepingTwelve = memberGroupInfo.Total;

            // 二十四个月沉睡会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingTwentyFour).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.SleepingTwentyFour = 0;
            else
                memberGroup.SleepingTwentyFour = memberGroupInfo.Total;
            #endregion

            #region 生日会员

            // 今日生日会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.BirthdayToday).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.BirthdayToday = 0;
            else
                memberGroup.BirthdayToday = memberGroupInfo.Total;

            // 今月生日会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.BirthdayToMonth).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.BirthdayToMonth = 0;
            else
                memberGroup.BirthdayToMonth = memberGroupInfo.Total;

            // 下月生日会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.BirthdayNextMonth).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.BirthdayNextMonth = 0;
            else
                memberGroup.BirthdayNextMonth = memberGroupInfo.Total;
            #endregion

            #region 注册会员
            // 注册会员
            memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.RegisteredMember).FirstOrDefault();
            if (memberGroupInfo == null)
                memberGroup.RegisteredMember = 0;
            else
                memberGroup.RegisteredMember = memberGroupInfo.Total;
            #endregion

            return memberGroup;
        }
        #endregion
    }
}
