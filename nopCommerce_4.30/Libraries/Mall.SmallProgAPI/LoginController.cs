using Mall.Application;
using Mall.CommonModel;
using Mall.Core.Helper;
using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Mall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mall.Core;
using Mall.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Mall.SmallProgAPI
{
    public class LoginController : BaseApiController
    {
        private static string _encryptKey = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 根据OpenId判断是否有账号，根据OpenId进行登录
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetLoginByOpenId")]
        public object GetLoginByOpenId(string openId = "")
        {
            //string oauthType = "Mall.Plugin.OAuth.WeiXin";//默认小程序微信端登录
            string unionid = "";
            var wxuserinfo = ApiHelper.GetAppletUserInfo(HttpContext);
            if (wxuserinfo != null)
            {
                unionid = wxuserinfo.unionId;
            }
            if (!string.IsNullOrEmpty(openId))
            {
                MemberInfo member = new MemberInfo();
                if (!string.IsNullOrWhiteSpace(unionid))
                {
                    member = MemberApplication.GetMemberByUnionIdAndProvider(SmallProgServiceProvider, unionid) ?? new MemberInfo();
                }

                if (member.Id == 0)
                    member = MemberApplication.GetMemberByOpenId(SmallProgServiceProvider, openId) ?? new MemberInfo();
                if (member.Id > 0)
                {
                    //信任登录并且已绑定             
                    string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                    MemberApplication.AddIntegel(member);//给用户加积分//执行登录后初始化相关操作
                    return GetMember(member, openId);
                }
                else
                {
                    //信任登录未绑定
                    return Json(ErrorResult<dynamic>("未绑定商城帐号"));
                }
            }
            return Json(ErrorResult<dynamic>(string.Empty));
        }
        /// <summary>
        ///账号密码登录
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetLoginByUserName")]
        public object GetLoginByUserName(string openId = "", string userName = "", string password = "", string nickName = "")
        {
            if (!string.IsNullOrEmpty(openId) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                MemberInfo member = null;

                try
                {
                    member = ServiceProvider.Instance<IMemberService>.Create.Login(userName, password);
                }
                catch (Exception ex)
                {
                    return Json(ErrorResult<dynamic>(ex.Message));
                }
                if (member == null)
                {
                    return Json(ErrorResult<dynamic>("用户名或密码错误"));
                }
                else
                {
                    if (member != null)
                    {
                        //如果不是一键登录的 则绑定openId
                        if (!string.IsNullOrEmpty(openId))
                        {
                            MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
                            {
                                UserId = member.Id,
                                OpenId = openId,
                                ServiceProvider = SmallProgServiceProvider,
                                AppIdType = MemberOpenIdInfo.AppIdTypeEnum.Normal,
                                UnionId = string.Empty
                            };
                            MemberApplication.UpdateOpenIdBindMember(memberOpenIdInfo);
                        }

                        string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                        return GetMember(member, openId);
                    }
                }
            }
            return Json(ErrorResult<dynamic>(string.Empty));
        }
        /// <summary>
        ///一键登录
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetQuickLogin")]
        public object GetQuickLogin(string openId = "", string nickName = "", string headImage = "", long? spreadId = null)
        {
            var wxuserinfo = ApiHelper.GetAppletUserInfo(HttpContext);
            string unionid = string.Empty;
            if (wxuserinfo != null)
            {
                unionid = wxuserinfo.unionId;
            }
            string unionopenid = "";
            if (!string.IsNullOrEmpty(openId))
            {
                headImage = System.Web.HttpUtility.UrlDecode(headImage);
                nickName = System.Web.HttpUtility.UrlDecode(nickName);
                string username = DateTime.Now.ToString("yyMMddHHmmssffffff");
                var member = ServiceProvider.Instance<IMemberService>.Create.QuickRegister(username, string.Empty, nickName, SmallProgServiceProvider, openId, (int)PlatformType.WeiXinSmallProg, unionid, unionopenid: unionopenid, headImage: headImage, spreadId: spreadId);
                if (member != null)
                {
                    //TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
                    BonusApplication.DepositToRegister(member.Id);
                    //用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
                    if (member.IsNewAccount)
                        CouponApplication.RegisterSendCoupon(member.Id, member.UserName);
                }
                return GetMember(member, openId);

            }
            return Json(ErrorResult<dynamic>(string.Empty));
        }
        /// <summary>
        ///退出登录
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetProcessLogout")]
        public  object GetProcessLogout(string openId)
        {
            if (!string.IsNullOrEmpty(openId))
            {
                var member = MemberApplication.GetMemberByOpenId(SmallProgServiceProvider, openId);
                if (member == CurrentUser)
                {
                    var cacheKey = WebHelper.GetCookie(CookieKeysCollection.Mall_USER);
                    if (!string.IsNullOrWhiteSpace(cacheKey))
                    {
                        //_iMemberService.DeleteMemberOpenId(userid, string.Empty);
                        WebHelper.DeleteCookie(CookieKeysCollection.Mall_USER);
                        WebHelper.DeleteCookie(CookieKeysCollection.SELLER_MANAGER);
                        //记录主动退出符号
                        WebHelper.SetCookie(CookieKeysCollection.Mall_ACTIVELOGOUT, "1", DateTime.MaxValue);
                        return Json(0);
                    }
                }
            }
            return Json(0);
        }

        /// <summary>
        /// 获取客服电话
        /// </summary>
        /// <param name="context"></param>
        /// 

        //public object GetServicePhone()
        //{
        //    var siteSettings = SiteSettingApplication.SiteSettings;
        //    return Json(new
        //    {
        //        ServicePhone = siteSettings.SitePhone
        //    });
        //}
        /// <summary>
        /// 获取首页数据
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        //public object GetIndexData(string openId = "", int pageIndex = 10, int pageSize = 1)
        //{
        //    //CheckUserLogin();
        //    MemberInfo member = CurrentUser;
        //    var sitesetting = SiteSettingApplication.SiteSettings;
        //    string homejson = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/AppletHome/data/default.json";

        //    long vidnumber = sitesetting.XcxHomeVersionCode;
        //    return Json(new
        //    {
        //        HomeTopicPath = homejson,
        //        Vid = vidnumber,
        //        QQMapKey = CommonConst.QQMapKey
        //    });
        //}
        /// <summary>
        /// 检查版本号
        /// </summary>
        /// <param name="context"></param>
        //public JsonResult<Result<int>> GetInitVeCode(string vid)
        //{
        //    if (string.IsNullOrEmpty(vid))
        //    {
        //        return Json(ErrorResult<int>("版本号不允许为空", 100, 100));
        //    }

        //    var sitesetting = SiteSettingApplication.SiteSettings;
        //    long xcxvid = sitesetting.XcxHomeVersionCode;

        //    if (xcxvid > long.Parse(vid))
        //    {
        //        return Json(ErrorResult("版本需要更新", 101, 101));
        //    }
        //    else
        //    {
        //        return JsonResult(100, "版本不需要更新", 100);
        //    }
        //}

        //public JsonResult<Result<List<dynamic>>> GetIndexProductData(string openId = "", int pageIndex = 1, int pageSize = 10)
        //{
        //    var homeProducts = ServiceProvider.Instance<IWXSmallProgramService>.Create.GetWXSmallHomeProducts(pageIndex,pageSize);
        //    decimal discount = 1M;
        //    long SelfShopId = 0;
        //    var CartInfo = new ShoppingCartInfo();
        //    var ids = homeProducts.Models.Select(p => p.Id).ToList();
        //    var productList = new List<dynamic>();
        //    var cartitems = new List<Mall.Entities.ShoppingCartItem>();
        //    long userId = 0;
        //    if (CurrentUser != null)
        //    {
        //        userId = CurrentUser.Id;
        //        discount = CurrentUser.MemberDiscount;
        //        var shopInfo = ShopApplication.GetSelfShop();
        //        SelfShopId = shopInfo.Id;
        //        CartInfo = ServiceProvider.Instance<ICartService>.Create.GetCart(CurrentUser.Id);
        //        cartitems = CartApplication.GetCartQuantityByIds(CurrentUser.Id, ids);
        //    }

        //    foreach (var item in homeProducts.Models)
        //    {
        //        long activeId = 0;
        //        int activetype = 0;
        //        item.ImagePath = MallIO.GetRomoteProductSizeImage(Core.MallIO.GetImagePath(item.ImagePath), 1, (int)Mall.CommonModel.ImageSize.Size_350);
        //        if (item.ShopId == SelfShopId)
        //            item.MinSalePrice = item.MinSalePrice * discount;
        //        var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(item.Id);
        //        if (limitBuy != null)
        //        {
        //            item.MinSalePrice = limitBuy.MinPrice;
        //            activeId = limitBuy.Id;
        //            activetype = 1;
        //        }
        //        int quantity = 0;
        //        quantity = cartitems.Where(d => d.ProductId == item.Id).Sum(d => d.Quantity);
               
        //        long stock = 0;

        //        var productInfo = ServiceProvider.Instance<IProductService>.Create.GetProduct(item.Id);
        //        if (productInfo != null)
        //        {
        //            var skus = ProductManagerApplication.GetSKUs(productInfo.Id);
        //            stock = skus.Sum(x => x.Stock);
        //            if (productInfo.MaxBuyCount > 0)
        //            {
        //                stock = productInfo.MaxBuyCount;
        //            }
        //        }
        //        if (productInfo.AuditStatus == ProductInfo.ProductAuditStatus.Audited)
        //        {
        //            var ChoiceProducts = new
        //            {
        //                ProductId = item.Id,
        //                ProductName = item.ProductName,
        //                SalePrice = item.MinSalePrice.ToString("0.##"),
        //                ThumbnailUrl160 = item.ImagePath,
        //                MarketPrice = item.MarketPrice.ToString("0.##"),
        //                CartQuantity = quantity,
        //                HasSKU = item.HasSKU,
        //                SkuId = GetSkuIdByProductId(item.Id),
        //                ActiveId = activeId,
        //                ActiveType = activetype,//获取该商品是否参与活动
        //                Stock = stock
        //            };
        //            productList.Add(ChoiceProducts);
        //        }
        //    }
        //    return JsonResult(productList);
        //}

        private object GetMember(Entities.MemberInfo member, string openId)
        {
            var model = MemberApplication.GetUserCenterModel(member.Id, false);
            //获取会员未使用的优惠券数目
            int couponsCount = model.UserCoupon;
            return Json(new
            {
                couponsCount = couponsCount,
                picture = MallIO.GetRomoteImagePath(member.Photo),
                points = model.Intergral,
                waitPayCount = model.WaitPayOrders,
                waitSendCount = model.WaitDeliveryOrders,
                waitFinishCount = model.WaitReceivingOrders,
                waitReviewCount = model.WaitEvaluationOrders,
                afterSalesCount = model.RefundCount,
                realName = string.IsNullOrEmpty(member.ShowNick) ? (string.IsNullOrEmpty(member.RealName) ? member.UserName : member.RealName) : member.ShowNick,
                gradeId = model.GradeId,
                gradeName = model.GradeName,
                UserName = member.UserName,
                UserId = member.Id,
                OpenId = openId,
                ServicePhone = SiteSettingApplication.SiteSettings.SitePhone
            });
        }

        //private string GetSkuIdByProductId(long productId = 0)
        //{
        //    string skuId = "";
        //    if (productId > 0)
        //    {
        //        var Skus = ServiceProvider.Instance<IProductService>.Create.GetSKUs(productId);
        //        foreach (var item in Skus)
        //        {
        //            skuId = item.Id;//取最后或默认
        //        }
        //    }
        //    return skuId;
        //}


        [HttpGet("GetOpenId")]
        public object GetOpenId(string appid, string secret, string js_code)
        {
            string requestUrl = "https://api.weixin.qq.com/sns/jscode2session?appid=" + appid + "&secret=" + secret + "&js_code=" + js_code + "&grant_type=authorization_code";
            string result = "";
            var response = Mall.Core.Helper.WebHelper.GetURLResponse(requestUrl);
            using (Stream receiveStream = response.GetResponseStream())
            {

                using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                {
                    result = readerOfStream.ReadToEnd();
                }
            }
            var openModel = JsonConvert.DeserializeObject<WeiXinOpenIdModel>(result);
            return Json(openModel);
        }

        protected override bool CheckContact(string contact, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(contact))
            {
                var userMenberInfo = MemberApplication.GetMemberByContactInfo(contact);
                if (userMenberInfo != null)
                {
                    errorMessage = "手机或邮箱号码已经存在";
                    Cache.Insert(_encryptKey + contact, string.Format("{0}:{1:yyyyMMddHHmmss}", userMenberInfo.Id, userMenberInfo.CreateDate), DateTime.Now.AddHours(1));
                    return false;
                }
                else
                {//不存在，可以绑定
                    return true;
                }
            }
            return false;
        }

        protected override string CreateCertificate(string contact)
        {
            var identity = Cache.Get<string>(_encryptKey + contact);
            identity = SecureHelper.AESEncrypt(identity, _encryptKey);
            return identity;
        }

        protected override object ChangePassowrdByCertificate(string certificate, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Json(ErrorResult<int>("密码不能为空"));

            certificate = SecureHelper.AESDecrypt(certificate, _encryptKey);
            long userId = long.TryParse(certificate.Split(':')[0], out userId) ? userId : 0;

            if (userId == 0)
                throw new MallException("数据异常");

            MemberApplication.ChangePassword(userId, password);
            return Json(new { msg="密码修改成功" });
        }

        protected override object ChangePayPwdByCertificate(string certificate, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Json(ErrorResult<int>("密码不能为空"));

            certificate = SecureHelper.AESDecrypt(certificate, _encryptKey);
            long userId = long.TryParse(certificate.Split(':')[0], out userId) ? userId : 0;

            if (userId == 0)
                throw new MallException("数据异常");

            var _iMemberCapitalService = ServiceProvider.Instance<IMemberCapitalService>.Create;

            _iMemberCapitalService.SetPayPwd(userId, password);
            return Json(new { msg= "支付密码修改成功" });
        }

        protected override object OnCheckCheckCodeSuccess(string contact)
        {
            CheckUserLogin();

            string pluginId = PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.SMS).First().PluginId;

            var _iMemberIntegralConversionFactoryService = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
            var _iMemberIntegralService = ServiceProvider.Instance<IMemberIntegralService>.Create;
            var _iMemberInviteService = ServiceProvider.Instance<IMemberInviteService>.Create;

            var member = CurrentUser;
            if (Application.MessageApplication.GetMemberContactsInfo(pluginId, contact, MemberContactInfo.UserTypes.General) != null)
            {
                return Json(ErrorResult<string>(contact + "已经绑定过了！"));
            }
            member.CellPhone = contact;
            MemberApplication.UpdateMember(member.Map<DTO.Members>());
            MessageApplication.UpdateMemberContacts(new MemberContactInfo()
            {
                Contact = contact,
                ServiceProvider = pluginId,
                UserId = CurrentUser.Id,
                UserType = MemberContactInfo.UserTypes.General
            });
            Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId));
            Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
            Cache.Remove("Rebind" + CurrentUser.Id);

            MemberInfo inviteMember = MemberApplication.GetMember(member.InviteUserId);


            var info = new MemberIntegralRecordInfo();
            info.UserName = member.UserName;
            info.MemberId = member.Id;
            info.RecordDate = DateTime.Now;
            info.TypeId = MemberIntegralInfo.IntegralType.Reg;
            info.ReMark = "绑定手机";
            var memberIntegral = _iMemberIntegralConversionFactoryService.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
            _iMemberIntegralService.AddMemberIntegral(info, memberIntegral);
            if (inviteMember != null)
                _iMemberInviteService.AddInviteIntegel(member, inviteMember, true);

            return base.OnCheckCheckCodeSuccess(contact);
        }
    }
}
