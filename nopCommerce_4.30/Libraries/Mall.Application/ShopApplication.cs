using System;
using System.Collections.Generic;
using Mall.IServices;
using AutoMapper;
using Mall.Core;
using Mall.DTO;
using Mall.Core.Plugins.Message;
using Mall.Entities;
using Mall.DTO.QueryModel;
using Mall.CommonModel;
using System.Linq;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class ShopApplication : BaseApplicaion<IShopService>
    {
        //     private static IAppMessageService _appMessageService =  EngineContext.Current.Resolve<IAppMessageService>();

        private static IAppMessageService _appMessageService =  EngineContext.Current.Resolve<IAppMessageService>();


        #region 商家入驻设置
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="mSettled"></param>
        public static void Settled(Settled mSettled)
        {
            //Mapper.CreateMap<Settled, Mall.Entities.SettledInfo>();
          //  var model = Mapper.Map<Settled, Mall.Entities.SettledInfo>(mSettled);
            var model = mSettled.Map< Mall.Entities.SettledInfo>();

            if (model.ID > 0)
            {
                SettledApplication.UpdateSettled(model);
            }
            else
            {
                SettledApplication.AddSettled(model);
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        public static Settled GetSettled()
        {
            Settled mSettled = new Settled();
            Mall.Entities.SettledInfo mSettledInfo = SettledApplication.GetSettled();
            if (mSettledInfo != null)
            {
                //Mapper.CreateMap<Mall.Entities.SettledInfo, Settled>();
                //  mSettled = Mapper.Map<Mall.Entities.SettledInfo, Settled>(mSettledInfo);
                mSettled = mSettledInfo.Map<Settled>();


            }
            return mSettled;
        }
        #endregion

        #region 商家入驻流程

        /// <summary>
        /// 添加商家管理员
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static Manager AddSellerManager(string username, string password, string salt)
        {
            var model = ManagerApplication.AddSellerManager(username, password, salt);
            Manager mManagerInfo = new Manager()
            {
                Id = model.Id
            };
            return mManagerInfo;
        }

        /// <summary>
        /// 获取店铺信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="businessCategoryOn"></param>
        /// <returns></returns>
        public static Shop GetShop(long id, bool businessCategoryOn = false)
        {
            var shop = Cache.Get<Shop>(CacheKeyCollection.CACHE_SHOPDTO(id, businessCategoryOn));
            if (shop != null) return shop;

            var model = Service.GetShop(id, businessCategoryOn);
            if (model == null) return new Shop();

            // shop = Mapper.Map<ShopInfo, Shop>(model);
            shop = model.Map< Shop>();


            Cache.Insert(CacheKeyCollection.CACHE_SHOPDTO(id, businessCategoryOn), shop, 600);
            return shop;
        }

        public static ShopInfo GetSelfShop()
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_SELFSHOP))
                return Cache.Get<ShopInfo>(CacheKeyCollection.CACHE_SELFSHOP);

            var model = Service.GetSelfShop();
            Cache.Insert<ShopInfo>(CacheKeyCollection.CACHE_SELFSHOP, model, 600);
            return model;
        }

        /// <summary>
        /// 根据id获取门店
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<Shop> GetShops(IEnumerable<long> ids)
        {
            var list = Service.GetShops(ids);
            //     return Mapper.Map<List<Shop>>(list);
            return list.Map<List<Shop>>();

        }

        /// <summary>
        /// 获取店铺信息（以分页的形式展示）
        /// </summary>
        /// <param name="shopQueryModel">ShopQuery对象</param>
        /// <returns></returns>
        public static QueryPageModel<Shop> GetShops(ShopQuery shopQueryModel)
        {
            var data = Service.GetShops(shopQueryModel);
            return new QueryPageModel<Shop>()
            {
                Models = data.Models.Map<List<Shop>>(),
                Total = data.Total
            };
        }

        public static int GetShopCount(ShopQuery query)
        {
            return BaseApplicaion<IShopService>.Service.GetShopCount(query);
        }

        /// <summary>
        /// 获取商家名称
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static Dictionary<long, string> GetShopNames(List<long> ids)
        {
            var shops = Service.GetShops(ids);
            return shops.ToDictionary(key => key.Id, value => value.ShopName);
        }

        public static Entities.ShopInfo GetShopInfo(long id, bool businessCategoryOn = false)
        {
            var model = Service.GetShop(id, businessCategoryOn);
            return model;
        }
        /// <summary>
        /// 商家入驻第二部
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ShopProfileStep1 GetShopProfileStep1(long id, out long CompanyRegionId, out long BusinessLicenceRegionId, out string RefuseReason)
        {
            var shop = Service.GetShop(id);

            var step1 = new ShopProfileStep1();
            step1.Address = shop.CompanyAddress;


            step1.BusinessLicenceArea = shop.BusinessLicenceRegionId;
            step1.BusinessLicenceNumber = shop.BusinessLicenceNumber;
            step1.BusinessLicenceNumberPhoto = shop.BusinessLicenceNumberPhoto;
            if (shop.BusinessLicenceEnd.HasValue)
                step1.BusinessLicenceValidEnd = shop.BusinessLicenceEnd.Value;

            if (shop.BusinessLicenceStart.HasValue)
                step1.BusinessLicenceValidStart = shop.BusinessLicenceStart.Value;
            string BusinessLicenseCert = string.Empty;
            for (int i = 1; i < 4; i++)
            {
                if (MallIO.ExistFile(shop.BusinessLicenseCert + string.Format("{0}.png", i)))
                {
                    BusinessLicenseCert += shop.BusinessLicenseCert + string.Format("{0}.png", i) + ",";
                }
            }
            step1.BusinessLicenseCert = BusinessLicenseCert.TrimEnd(',');
            step1.BusinessSphere = shop.BusinessSphere;
            step1.CityRegionId = shop.CompanyRegionId;
            if (shop.CompanyFoundingDate.HasValue)
                step1.CompanyFoundingDate = shop.CompanyFoundingDate.Value;
            step1.CompanyName = shop.CompanyName;
            step1.ContactName = shop.ContactsName;
            step1.ContactPhone = shop.ContactsPhone;
            step1.Email = shop.ContactsEmail;
            step1.EmployeeCount = shop.CompanyEmployeeCount;
            step1.GeneralTaxpayerPhoto = shop.GeneralTaxpayerPhot;
            step1.legalPerson = shop.legalPerson;
            step1.OrganizationCode = shop.OrganizationCode;
            step1.OrganizationCodePhoto = shop.OrganizationCodePhoto;
            step1.BusinessType = shop.BusinessType;

            string OtherCert = string.Empty;
            for (int i = 1; i < 4; i++)
            {
                if (MallIO.ExistFile(shop.OtherCert + string.Format("{0}.png", i)))
                {
                    OtherCert += shop.OtherCert + string.Format("{0}.png", i) + ",";
                }
            }
            step1.OtherCert = OtherCert.TrimEnd(',');
            step1.Phone = shop.CompanyPhone;

            string ProductCert = string.Empty;
            for (int i = 1; i < 4; i++)
            {
                if (MallIO.ExistFile(shop.ProductCert + string.Format("{0}.png", i)))
                {
                    ProductCert += shop.ProductCert + string.Format("{0}.png", i) + ",";
                }
            }
            step1.ProductCert = ProductCert.TrimEnd(',');
            step1.RegisterMoney = shop.CompanyRegisteredCapital;
            step1.taxRegistrationCert = shop.TaxRegistrationCertificate;
            step1.Settled = GetSettled();

            CompanyRegionId = shop.CompanyRegionId;
            BusinessLicenceRegionId = shop.BusinessLicenceRegionId;
            RefuseReason = null;
            if (shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Refuse) RefuseReason = shop.RefuseReason;

            return step1;
        }

        /// <summary>
        /// 个人入驻第二部信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="CompanyRegionId"></param>
        /// <param name="BusinessLicenceRegionId"></param>
        /// <param name="RefuseReason"></param>
        /// <returns></returns>
        public static ShopProfileSteps1 GetShopProfileSteps1(long id, out long CompanyRegionId, out long BusinessLicenceRegionId, out string RefuseReason)
        {
            var shop = Service.GetShop(id);

            var step1 = new ShopProfileSteps1();
            step1.Address = shop.CompanyAddress;

            step1.CityRegionId = shop.CompanyRegionId;
            step1.CompanyName = shop.CompanyName;

            step1.IDCard = shop.IDCard;
            step1.IDCardUrl = shop.IDCardUrl;
            step1.IDCardUrl2 = shop.IDCardUrl2;
            step1.BusinessType = shop.BusinessType;
            step1.Settled = GetSettled();

            CompanyRegionId = shop.CompanyRegionId;
            BusinessLicenceRegionId = shop.BusinessLicenceRegionId;
            RefuseReason = null;
            if (shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Refuse) RefuseReason = shop.RefuseReason;

            return step1;
        }
        /// <summary>
        /// 获取商家入驻第三部信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ShopProfileStep2 GetShopProfileStep2(long id, out Entities.ShopInfo.ShopStage Stage)
        {
            var shop = Service.GetShop(id);
            var model = new ShopProfileStep2()
            {
                BankAccountName = shop.BankAccountName == null ? "" : shop.BankAccountName,
                BankAccountNumber = shop.BankAccountNumber == null ? "" : shop.BankAccountNumber,
                BankCode = shop.BankCode == null ? "" : shop.BankCode,
                BankName = shop.BankName == null ? "" : shop.BankName,
                BankPhoto = shop.BankPhoto == null ? "" : shop.BankPhoto,
                BankRegionId = shop.BankRegionId,
                TaxpayerId = shop.TaxpayerId == null ? "" : shop.TaxpayerId,
                TaxRegistrationCertificate = shop.TaxRegistrationCertificate == null ? "" : shop.TaxRegistrationCertificate,
                TaxRegistrationCertificatePhoto = shop.TaxRegistrationCertificatePhoto == null ? "" : shop.TaxRegistrationCertificatePhoto,
                WeiXinAddress = shop.WeiXinAddress == null ? "" : shop.WeiXinAddress,
                WeiXinNickName = shop.WeiXinNickName == null ? "" : shop.WeiXinNickName,
                WeiXinOpenId = shop.WeiXinOpenId == null ? "" : shop.WeiXinOpenId,
                WeiXinSex = shop.WeiXinSex == null ? 0 : shop.WeiXinSex.Value,
                WeiXinTrueName = shop.WeiXinTrueName == null ? "" : shop.WeiXinTrueName,
                BusinessType = shop.BusinessType,
                Settled = GetSettled()
            };
            Stage = shop.Stage;
            return model;
        }

        /// <summary>
        /// 商家入驻协议
        /// </summary>
        /// <returns></returns>
        public static string GetSellerAgreement()
        {
            var model = SystemAgreementApplication.GetAgreement(Entities.AgreementInfo.AgreementTypes.Seller);
            if (model != null)
                return model.AgreementContent;
            else
                return "";
        }

        /// <summary>
        /// 注册协议
        /// </summary>
        /// <returns></returns>
        public static string GetBuyersAgreement()
        {
            var model = SystemAgreementApplication.GetAgreement(Entities.AgreementInfo.AgreementTypes.Buyers);
            if (model != null)
                return model.AgreementContent;
            else
                return "";
        }

        /// <summary>
        /// 商家入驻店铺信息更新
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ShopId"></param>
        /// <returns>0、失败；1、成功；-1、店铺名称已经存在</returns>
        public static int UpdateShop(ShopProfileStep3 model, long ShopId)
        {
            int result = 0;
            if (Service.ExistShop(model.ShopName, ShopId))
            {
                result = -1;
            }
            else
            {
                Entities.ShopInfo shopInfo = Service.GetShop(ShopId);
                shopInfo.Id = ShopId;
                shopInfo.ShopName = model.ShopName;
                shopInfo.GradeId = model.ShopGrade;
                shopInfo.Stage = Entities.ShopInfo.ShopStage.UploadPayOrder;
                var shopCategories = model.Categories;
                Service.UpdateShop(shopInfo, model.Categories.ToList());
                ClearCacheShop(ShopId);
                result = 1;
            }
            return result;
        }
        /// <summary>
        /// 清除门店缓存
        /// </summary>
        /// <param name="shop"></param>
        public static void ClearCacheShop(long shop)
        {
            //TODO:FG 缓存策略需要统一至 应用层
            Service.ClearShopCache(shop);
        }

        #endregion

        #region 店铺信息

        /// <summary>
        /// 商店信息更新
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateShop(Shop model)
        {
            //  var mShop = Mapper.Map<Shop, ShopInfo>(model);

            var mShop = model.Map< ShopInfo>();

            Service.UpdateShop(mShop);
            ClearCacheShop(model.Id);
        }





        /// <summary>
        /// 判断公司名称是否存在
        /// </summary>
        /// <param name="companyName">公司名字</param>
        /// <param name="shopId"></param>
        public static bool ExistCompanyName(string companyName, long shopId = 0)
        {
            return Service.ExistCompanyName(companyName, shopId);
        }

        /// <summary>
        /// 检测营业执照号是否重复
        /// </summary>
        /// <param name="BusinessLicenceNumber">营业执照号</param>
        /// <param name="shopId"></param>
        public static bool ExistBusinessLicenceNumber(string BusinessLicenceNumber, long shopId = 0)
        {
            return Service.ExistBusinessLicenceNumber(BusinessLicenceNumber, shopId);
        }

        /// <summary>
        /// 判断店铺名名称是否存在
        /// </summary>
        /// <param name="shopName">公司名字</param>
        /// <param name="shopId"></param>
        public static bool ExistShop(string shopName, long shopId = 0)
        {
            return Service.ExistShop(shopName, shopId);
        }

        /// <summary>
        /// 获取店铺等级列表
        /// </summary>
        /// <returns></returns>
        public static List<ShopGrade> GetShopGrades()
        {
            List<ShopGrade> lmShopGrade = new List<ShopGrade>();
            var model = Service.GetShopGrades();
            foreach (var item in model)
            {
                // Mapper.CreateMap<ShopGradeInfo, ShopGrade>();
                //   lmShopGrade.Add(Mapper.Map<ShopGradeInfo, ShopGrade>(item));

                lmShopGrade.Add(item.Map< ShopGrade>());

            }
            return lmShopGrade;
        }

        /// <summary>
        /// 获取店铺账户信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopAccount> GetShopAccounts(List<long> ids)
        {
            return Service.GetShopAccounts(ids).Map<List<ShopAccount>>();
        }

        /// <summary>
        /// 获取单个店铺账户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ShopAccount GetShopAccount(long id)
        {
            return Service.GetShopAccount(id).Map<ShopAccount>();
        }

        /// <summary>
        /// 获取店铺经营项目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<BusinessCategory> GetBusinessCategory(long id)
        {
            List<BusinessCategory> lvBusinessCategory = new List<BusinessCategory>();
            var model = Service.GetBusinessCategory(id);
            foreach (var item in model)
            {
                lvBusinessCategory.Add(new BusinessCategory()
                {
                    Id = item.Id,
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    ShopId = item.ShopId
                });
            }
            return lvBusinessCategory;
        }

        /// <summary>
        /// 获取店铺宝贝数
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static int GetShopProductCount(long shopId)
        {
            return Service.GetShopProductCount(shopId);
        }

        /// <summary>
        /// 获取单个入驻缴费记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SettledPayment GetSettledPaymentRecord(long id)
        {
            var item = Service.GetShopRenewRecord(id);
            var shopName = Service.GetShop(item.ShopId).ShopName;
            SettledPayment model = new SettledPayment();
            model.Id = item.Id;
            model.OperateType = item.OperateType.ToDescription();
            model.OperateDate = item.OperateDate.ToString("yyyy-MM-dd HH:mm");
            model.Operate = item.Operator;
            model.Content = item.OperateContent;
            model.Amount = item.Amount;
            model.ShopName = shopName;
            return model;
        }
        /// <summary>
        /// 获取关注店铺数量
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static int GetUserConcernShopsCount(long userid)
        {
            var result = BaseApplicaion<IShopService>.Service.GetUserConcernShops(userid, 1, int.MaxValue);
            return result.Total;
        }



        #endregion


        #region 管理员认证
        /// <summary>
        /// 获取店铺认证情况
        /// </summary>
        /// <param name="ShopId">店铺ID</param>
        /// <returns></returns>
        public static MemberAccountSafety GetShopAccountSafety(long ShopId)
        {
            MemberAccountSafety model = new MemberAccountSafety();
            long UserId = Service.GetShopManagers(ShopId);
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
        /// 获取店铺管理员ID
        /// </summary>
        /// <param name="ShopId">店铺ID</param>
        /// <returns></returns>
        public static long GetShopManagers(long ShopId)
        {
            long UserId = Service.GetShopManagers(ShopId);
            return UserId;
        }

        /// <summary>
        /// 发送验证码，认证管理员
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="UserName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static bool SendShopCode(string pluginId, string destination, string UserName, string SiteName)
        {
            var timeout = CacheKeyCollection.MemberPluginReBindTime(UserName, pluginId); //验证码超时时间
            if (Core.Cache.Exists(timeout))
            {
                return false;
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(UserName, pluginId + destination), checkCode.ToString(), cacheTimeout);
            var user = new MessageUserInfo() { UserName = UserName, SiteName = SiteName, CheckCode = checkCode.ToString() };
            MessageApplication.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginReBindTime(UserName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
            return true;
        }

        /// <summary>
        /// 验证码验证，认证管理员
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int CheckShopCode(string pluginId, string code, string destination, long userId)
        {
            var member = MemberApplication.GetMembers(userId);
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

                    MemberApplication.UpdateMember(member);

                    MessageApplication.UpdateMemberContacts(new Entities.MemberContactInfo()
                    {
                        Contact = destination,
                        ServiceProvider = pluginId,
                        UserId = userId,
                        UserType = Entities.MemberContactInfo.UserTypes.General
                    });

                    Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(member.UserName, pluginId));
                    Core.Cache.Remove(CacheKeyCollection.Member(userId));//移除用户缓存
                    Core.Cache.Remove("Rebind" + userId);
                    result = 1;
                }
            }
            return result;
        }

        /// <summary>
        /// <summary>
        /// 是否官方自营店
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool IsSelfShop(long shopId)
        {
            return Service.IsSelfShop(shopId);
        }



        #endregion
        /// <summary>
        /// 检查、初始化店铺模版
        /// </summary>
        /// <param name="shopid"></param>
        public static void CheckInitTemplate(long shopid)
        {
            Service.CheckInitTemplate(shopid);
        }


        #region 验证码

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
            var timeout = CacheKeyCollection.ShopPluginAuthenticateTime(UserName, pluginId); //验证码超时时间
            if (Core.Cache.Exists(timeout))
            {
                return false;
            }
            var checkCode = new Random().Next(10000, 99999);
            Log.Debug("Code:" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.ShopPluginAuthenticate(UserName, pluginId + destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = UserName, SiteName = SiteName, CheckCode = checkCode.ToString() };
            MessageApplication.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.ShopPluginAuthenticateTime(UserName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
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
        public static int CheckCode(string pluginId, string code, string destination, string userName)
        {
            int result = 0;
            var cache = CacheKeyCollection.ShopPluginAuthenticate(userName, pluginId + destination);
            var cacheCode = Core.Cache.Get<string>(cache);
            if (cacheCode != null && cacheCode == code)
            {
                Core.Cache.Remove(CacheKeyCollection.ShopPluginAuthenticate(userName, pluginId + destination));
                result = 1;
            }
            return result;
        }


        /// <summary>
        /// (绑定银行卡)发送验证码
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static bool BindBankSendCode(string pluginId, string destination, string userName, string SiteName)
        {
            var timeout = CacheKeyCollection.ShopPluginBindBankTime(userName, pluginId); //验证码超时时间
            if (Core.Cache.Exists(timeout))
            {
                return false;
            }
            var checkCode = new Random().Next(10000, 99999);
            // Log.Debug("Code:" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.ShopPluginBindBank(userName, pluginId + destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = userName, SiteName = SiteName, CheckCode = checkCode.ToString() };
            MessageApplication.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.ShopPluginBindBankTime(userName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
            return true;
        }

        /// <summary>
        /// （绑定银行卡）验证码验证
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int BindBankCheckCode(string pluginId, string code, string destination, string userName)
        {
            int result = 0;
            var cache = CacheKeyCollection.ShopPluginBindBank(userName, pluginId + destination);
            var cacheCode = Core.Cache.Get<string>(cache);
            if (cacheCode != null && cacheCode == code)
            {
                Core.Cache.Remove(CacheKeyCollection.ShopPluginBindBank(userName, pluginId + destination));
                result = 1;
            }
            return result;
        }

        #endregion
        /// <summary>
        /// 获取指定店铺等级信息
        /// </summary>
        /// <param name="id">店铺等级Id</param>
        /// <returns></returns>
        public static ShopGrade GetShopGrade(long id)
        {
            // Mapper.CreateMap<ShopGradeInfo, ShopGrade>();
           

            return Service.GetShopGrade(id).Map<DTO.ShopGrade>();
        }

        /// <summary>
        /// 是否已过期
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool IsExpiredShop(long shopId)
        {
            return Service.IsExpiredShop(shopId);
        }

        /// <summary>
        /// 是否冻结
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool IsFreezeShop(long shopId)
        {
            return Service.IsFreezeShop(shopId);
        }



        /// <summary>
        /// 根据网店管家uCode获取对应店铺
        /// </summary>
        /// <param name="uCode"></param>
        /// <returns></returns>
        public static ShopWdgjSettingInfo GetshopWdgjInfoByCode(string uCode)
        {
            return Service.GetshopInfoByCode(uCode);
        }
        /// <summary>
        /// 获取网店管家信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static ShopWdgjSettingInfo GetshopWdgjInfoById(long shopId)
        {
            return Service.GetshopWdgjInfoById(shopId);
        }
        /// <summary>
        /// 更新商家入驻类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public static void SetBusinessType(long id, ShopBusinessType type)
        {
            Service.SetBusinessType(id, type);
            ClearCacheShop(id);
        }

        #region 门店设置
        public static void SetProvideInvoice(ShopInvoiceConfigInfo info)
        {
            Service.SetProvideInvoice(info);
            ClearCacheShop(info.ShopId);
        }

        public static void SetShopFreight(long id, decimal freight, decimal freeFreight)
        {
            Service.SetShopFreight(id, freight, freeFreight);
            ClearCacheShop(id);
        }

        public static void SetAutoAllotOrder(long id, bool enable)
        {
            Service.SetAutoAllotOrder(id, enable);
            ClearCacheShop(id);
        }
        /// <summary>
        /// 设置店铺公司信息
        /// </summary>
        /// <param name="info"></param>
        public static void SetCompanyInfo(ShopCompanyInfo info)
        {
            Service.SetCompnayInfo(info);
            ClearCacheShop(info.ShopId);
        }
        /// <summary>
        /// 设置店铺银行帐户
        /// </summary>
        /// <param name="bankAccount"></param>
        public static void SetBankAccount(BankAccount bankAccount)
        {
            Service.SetBankAccount(bankAccount);
            ClearCacheShop(bankAccount.ShopId);
        }

        /// <summary>
        /// 设置店铺银行帐户
        /// </summary>
        /// <param name="bankAccount"></param>
        public static void UpdateBankAccount(BankAccount bankAccount)
        {
            Service.UpdateBankAccount(bankAccount);
            ClearCacheShop(bankAccount.ShopId);
        }

        /// <summary>
        /// 设置店铺微信帐户
        /// </summary>
        /// <param name="account"></param>
        public static void SetWeChatAccount(WeChatAccount account)
        {
            Service.SetWeChatAccount(account);
            ClearCacheShop(account.ShopId);
        }

        public static void SetLicenseCert(ShopLicenseCert model) {
            Service.SetLicenseCert(model);
            ClearCacheShop(model.ShopId);
        }

        public static void SetAutoPrint(long id, bool enable)
        {
            Service.SetAutoPrint(id, enable);
            ClearCacheShop(id);
        }
        public static void SetPrintCount(long id, int count)
        {
            Service.SetPrintCount(id, count);
            ClearCacheShop(id);
        }

        #endregion

        #region 商家缴费回调
        /// <summary>
        /// 商家缴费回调
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="userName"></param>
        /// <param name="balance"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public static void ShopReNewPayNotify(string tradeNo, long shopId, string userName, decimal balance, int type, int value, bool isShopAccount = false)
        {
            Entities.ShopRenewRecordInfo model = new Entities.ShopRenewRecordInfo();
            model.TradeNo = tradeNo;
            //添加店铺续费记录
            model.ShopId = shopId;
            model.OperateDate = DateTime.Now;
            model.Operator = userName;
            model.Amount = balance;
            //续费操作
            if (type == 1)
            {
                model.OperateType = Entities.ShopRenewRecordInfo.EnumOperateType.ReNew;
                var shopInfo = Service.GetShop(shopId);
                DateTime beginTime = shopInfo.EndDate;
                if (beginTime < DateTime.Now)
                    beginTime = DateTime.Now;
                string strNewEndTime = beginTime.AddYears(value).ToString("yyyy-MM-dd");
                model.OperateContent = "续费 " + value + " 年至 " + strNewEndTime;
                Service.AddShopRenewRecord(model, isShopAccount);
                //店铺续费
                Service.ShopReNew(shopId, value);
            }
            //升级操作
            else
            {
                model.ShopId = shopId;
                model.OperateType = Entities.ShopRenewRecordInfo.EnumOperateType.Upgrade;
                var shopInfo = Service.GetShop(shopId);
                var shopGrade = Service.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
                var newshopGrade = Service.GetShopGrades().Where(c => c.Id == (long)value).FirstOrDefault();
                model.OperateContent = "将套餐‘" + shopGrade.Name + "'升级为套餐‘" + newshopGrade.Name + "'";
                Service.AddShopRenewRecord(model, isShopAccount);
                //店铺升级
                Service.ShopUpGrade(shopId, (long)value);
            }
            ClearCacheShop(shopId);//清除当前商家缓存
        }
        #endregion
        public static bool UpdateOpenTopImageAd(long shopId, bool isOpenTopImageAd)
        {
            return Service.UpdateOpenTopImageAd(shopId, isOpenTopImageAd);
        }

        public static long GetShopDisplaySales(long shop)
        {
            var sale = Service.GetSales(shop);
            var pro = GetService<IProductService>();
            var virtualSale = pro.GetProductVirtualSale(shop);
            return sale + virtualSale;
        }
        public static ShopInfo GetShopBasicInfo(long id)
        {
            return Service.GetShopBasicInfo(id);
        }

        public static List<ShopBrand> GetShopBrands(List<long> shops)
        {
            return Service.GetShopBrands(shops);
        }

        public static bool HasProvideInvoice(List<long> shops)
        {
            return Service.HasProvideInvoice(shops);
        }

        /// <summary>
        /// 设置申请商家步骤
        /// </summary>
        /// <param name="shopStage">第几步</param>
        /// <param name="id">店铺Id</param>
        public static void SetShopStage(ShopInfo.ShopStage shopStage, long id)
        {
            Service.SetShopStage(shopStage, id);
            ClearCacheShop(id);
        }

        public static StatisticOrderComment GetStatisticOrderComment(long shop) {
            return GetStatisticOrderComment(new List<long> { shop }).FirstOrDefault();

        }
        public static List<StatisticOrderComment> GetStatisticOrderComment(List<long> shops)
        {
            var data = Service.GetStatisticOrderComments(shops);
            return shops.Select(item =>
            {
                var itemData = data.Where(p => p.ShopId == item).ToList();
                return new StatisticOrderComment
                {
                    ShopId = item,
                    ProductAndDescription = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescription),
                    ProductAndDescriptionMax = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionMax),
                    ProductAndDescriptionMin = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionMin),
                    ProductAndDescriptionPeer = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionPeer),
                    SellerDeliverySpeed = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeed),
                    SellerDeliverySpeedMax = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedMax),
                    SellerDeliverySpeedMin = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedMin),
                    SellerDeliverySpeedPeer = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedPeer),
                    SellerServiceAttitude = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitude),
                    SellerServiceAttitudeMax = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudeMax),
                    SellerServiceAttitudeMin = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudeMin),
                    SellerServiceAttitudePeer = GetStatisticOrderCommentValue(itemData, StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudePeer),
                };
            }).ToList();
        }
        private static decimal GetStatisticOrderCommentValue(List<StatisticOrderCommentInfo> data, StatisticOrderCommentInfo.EnumCommentKey key)
        {
            return data.FirstOrDefault(c => c.CommentKey == key)?.CommentValue ?? 5;
        }

 /// <summary>
        /// 店铺名称修改了同时修改其他表保存店铺的名称
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopName"></param>
        /// <returns></returns>
        public static bool UpdateOtherTableShopName(long shopId,string shopName)
        {
         //   IVShopService _iVShopService =  EngineContext.Current.Resolve<IVShopService>();
          //  ICouponService _iCouponService =  EngineContext.Current.Resolve<ICouponService>();

            IVShopService _iVShopService =  EngineContext.Current.Resolve<IVShopService>();
            ICouponService _iCouponService =  EngineContext.Current.Resolve<ICouponService>();


            var vshop = _iVShopService.GetVShopByShopId(shopId);
            if (vshop != null)
            {
                vshop.Name = shopName;
                _iVShopService.UpdateVShop(vshop);
            }

            _iCouponService.SetCouponShopName(shopId, shopName);
            return true;
        }

        #region TDO:ZYF Invoice
        /// <summary>
        /// 获取商家发票管理配置
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static ShopInvoiceConfigInfo GetShopInvoiceConfig(long shopId)
        {
            return Service.GetShopInvoiceConfig(shopId);
        }

        /// <summary>
        /// 获取商家发票类型列表
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static List<InvoiceTypes> GetInvoiceTypes(long shopid)
        {
            List<long> shops = new List<long>();
            shops.Add(shopid);
            var isInvoice = Service.HasProvideInvoice(shops);
            if (!isInvoice)
                return null;
            else
            {
                var config = GetShopInvoiceConfig(shopid);
                List<InvoiceTypes> types = new List<InvoiceTypes>();
                
                if (config.IsPlainInvoice)
                {
                    var type = new InvoiceTypes()
                    {
                        Id = InvoiceType.OrdinaryInvoices.GetHashCode(),
                        Name = InvoiceType.OrdinaryInvoices.ToDescription(),
                        Rate = config.PlainInvoiceRate
                    };
                    types.Add(type);
                }
                if (config.IsElectronicInvoice)
                {
                    var type = new InvoiceTypes()
                    {
                        Id = InvoiceType.ElectronicInvoice.GetHashCode(),
                        Name = InvoiceType.ElectronicInvoice.ToDescription(),
                        Rate = config.PlainInvoiceRate
                    };
                    types.Add(type);
                }
                if (config.IsVatInvoice)
                {
                    var type = new InvoiceTypes()
                    {
                        Id = InvoiceType.VATInvoice.GetHashCode(),
                        Name = InvoiceType.VATInvoice.ToDescription(),
                        Rate = config.VatInvoiceRate
                    };
                    types.Add(type);
                }
                return types;
            }
        }

        /// <summary>
        /// 获取用户默认的发票信息
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static InvoiceTitleInfo GetInvoiceTitleInfo(long userid,InvoiceType type)
        {
            return Service.GetInvoiceTitleInfo(userid, type);
        }
        #endregion
    }
}
