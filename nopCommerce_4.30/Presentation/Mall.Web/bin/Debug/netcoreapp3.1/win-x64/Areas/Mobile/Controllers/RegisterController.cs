using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http.Extensions;
using System;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class RegisterController : BaseMobileTemplatesController
    {
        const string CHECK_CODE_KEY = "checkCode";

        private IMemberService _iMemberService;
        private IMemberInviteService _iMemberInviteService;
        private IMemberIntegralService _iMemberIntegralService;
        private IBonusService _iBonusService;
        private IMessageService _iMessageService;
        private IMemberIntegralConversionFactoryService _iMemberIntegralConversionFactoryService;
        public RegisterController(
            IMemberService iMemberService,
            IMemberInviteService iMemberInviteService,
            IMemberIntegralService iMemberIntegralService,
            IMemberIntegralConversionFactoryService iMemberIntegralConversionFactoryService,
            IBonusService iBonusService,
            IMessageService iMessageService
            )
        {
            _iMessageService = iMessageService;
            _iMemberService = iMemberService;
            _iMemberInviteService = iMemberInviteService;
            _iMemberIntegralService = iMemberIntegralService;
            _iMemberIntegralConversionFactoryService = iMemberIntegralConversionFactoryService;
            _iBonusService = iBonusService;
        }
        // GET: Mobile/Register
        public ActionResult Index(long id = 0, string openid = "")
        {
            ViewBag.Introducer = id;
            if (id > 0)
            {
                if (string.IsNullOrWhiteSpace(openid))
                {
                    string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
                    string url = webRoot + "/m-" + PlatformType + "/Register/InviteRegist?id=" + id;
                    if (PlatformType == PlatformType.WeiXin)
                        return Redirect("/m-" + PlatformType.ToString() + "/WXApi/WXAuthorize?returnUrl=" + url);
                    else
                        return Redirect(url);
                }
            }
            var setting = SiteSettingApplication.SiteSettings;
            var type = setting.RegisterType;
            ViewBag.EmailVerifOpen = setting.EmailVerifOpen;
            if (type == (int)RegisterTypes.Mobile)
            {
                return View("MobileReg");
            }
            return View();
        }
        public ActionResult InviteRegist(long id = 0, string openId = "", string unionid = "", string serviceProvider = "")
        {
            ViewBag.Introducer = id;
            var memberInfo = _iMemberService.GetMemberByUnionId(unionid);
            var settings = SiteSettingApplication.SiteSettings;
            var inviteRule = _iMemberInviteService.GetInviteRule();
            var model = _iMemberIntegralService.GetIntegralChangeRule();
            var perMoney = model == null ? 0 : model.IntegralPerMoney;
            ViewBag.WXLogo = settings.WXLogo;
            string money;
            if (perMoney > 0)
            {
                money = (Convert.ToDouble(inviteRule.RegIntegral) / perMoney).ToString("f1");
            }
            else
            {
                money = "0.0";
            }


            int isRegist = 0;
            if (memberInfo != null)
            {
                isRegist = 1;
            }
            ViewBag.Money = money;
            ViewBag.IsRegist = isRegist;
            ViewBag.RegisterType = settings.RegisterType;
            return View(inviteRule);
        }
        [HttpPost]
        public JsonResult Index(string serviceProvider, string openId, string username, string password, string checkCode, string mobilecheckCode,
            string headimgurl, long introducer = 0, string unionid = null, string sex = null,
            string city = null, string province = null, string country = null, string nickName = null, string email = "", string emailcheckCode = "")
        {
            var mobilepluginId = "Mall.Plugin.Message.SMS";
            var emailpluginId = "Mall.Plugin.Message.Email";
            string systemCheckCode = HttpContext.Session.Get<string>(CHECK_CODE_KEY) as string;
            if (systemCheckCode.ToLower() != checkCode.ToLower())
                throw new Core.MallException("验证码错误");

            if (Core.Helper.ValidateHelper.IsMobile(username))
            {
                var cache = CacheKeyCollection.MemberPluginCheck(username, mobilepluginId);
                var cacheCode = Core.Cache.Get<string>(cache);

                if (string.IsNullOrEmpty(mobilecheckCode) || mobilecheckCode.ToLower() != cacheCode.ToLower())
                {
                    throw new Core.MallException("手机验证码错误");
                }
            }

            if (!string.IsNullOrEmpty(email) && Core.Helper.ValidateHelper.IsMobile(email))
            {
                var cache = CacheKeyCollection.MemberPluginCheck(username, emailpluginId);
                var cacheCode = Core.Cache.Get<string>(cache);

                if (string.IsNullOrEmpty(emailcheckCode) || emailcheckCode.ToLower() != cacheCode.ToLower())
                {
                    throw new Core.MallException("手机验证码错误");
                }
            }

            headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
            nickName = System.Web.HttpUtility.UrlDecode(nickName);
            province = System.Web.HttpUtility.UrlDecode(province);
            city = System.Web.HttpUtility.UrlDecode(city);
            Entities.MemberInfo member;
            var mobile = "";
            if (Core.Helper.ValidateHelper.IsMobile(username))
                mobile = username;
            var platform = PlatformType.GetHashCode();//注册终端来源
            if (!string.IsNullOrWhiteSpace(serviceProvider) && !string.IsNullOrWhiteSpace(openId))
            {
                OAuthUserModel userModel = new OAuthUserModel
                {
                    UserName = username,
                    Password = password,
                    LoginProvider = serviceProvider,
                    OpenId = openId,
                    Headimgurl = headimgurl,
                    Sex = sex,
                    NickName = nickName,
                    Email = email,
                    UnionId = unionid,
                    introducer = introducer,
                    Province = province,
                    City = city,
                    Platform = platform,
                    SpreadId = CurrentSpreadId
                };
                member = _iMemberService.Register(userModel);
            }
            else
                member = _iMemberService.Register(username, password, platform, mobile, email, introducer, spreadId: CurrentSpreadId);
            if (member != null)
            {
                HttpContext.Session.Remove(CHECK_CODE_KEY);
                MessageHelper helper = new MessageHelper();
                helper.ClearErrorTimes(member.UserName);
                if (!string.IsNullOrEmpty(email))
                {
                    helper.ClearErrorTimes(member.Email);
                }
                ClearDistributionSpreadCookie();
            }
            //TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
            _iBonusService.DepositToRegister(member.Id);
            //用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
            int num = CouponApplication.RegisterSendCoupon(member.Id, member.UserName);

            base.SetUserLoginCookie(member.Id);
            Application.MemberApplication.UpdateLastLoginDate(member.Id);
            _iMemberService.AddIntegel(member); //给用户加积分//执行登录后初始化相关操作
            return Json<dynamic>(success: true, data: new { memberId = member.Id, num = num });
        }

        [HttpPost]
        public JsonResult InviteRegist(string serviceProvider, string openId, string username, string password, string nickName, string headimgurl, long introducer, string sex, string city = null, string province = null, string unionid = null, string mobile = null)
        {

            headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
            nickName = System.Web.HttpUtility.UrlDecode(nickName);
            username = System.Web.HttpUtility.UrlDecode(username);
            province = System.Web.HttpUtility.UrlDecode(province);
            city = System.Web.HttpUtility.UrlDecode(city);
            var platform = PlatformType.GetHashCode();//注册终端来源
            Entities.MemberInfo member;
            if (string.IsNullOrWhiteSpace(username))
                username = mobile;
            if (!string.IsNullOrWhiteSpace(serviceProvider) && !string.IsNullOrWhiteSpace(openId))
                member = _iMemberService.Register(username, password, serviceProvider, openId, platform, sex, headimgurl, introducer, nickName
                    , city, province, unionid, spreadId: CurrentSpreadId);
            else
                member = _iMemberService.Register(username, password, platform, mobile, "", introducer, spreadId: CurrentSpreadId);

            //TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
            _iBonusService.DepositToRegister(member.Id);
            //用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
            int num = CouponApplication.RegisterSendCoupon(member.Id, member.UserName);

            ClearDistributionSpreadCookie();
            base.SetUserLoginCookie(member.Id);
            Application.MemberApplication.UpdateLastLoginDate(member.Id);
            _iMemberService.AddIntegel(member); //给用户加积分//执行登录后初始化相关操作
            return Json<dynamic>(success: true, data: new { memberId = member.Id, num = num });
        }


        [HttpPost]
        public JsonResult Skip(string serviceProvider, string openId, string nickName, string realName, string headimgurl, Entities.MemberOpenIdInfo.AppIdTypeEnum appidtype = Entities.MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionid = null, string sex = null, string city = null, string province = null)
        {
            int num = 0;
            string username = DateTime.Now.ToString("yyMMddHHmmssffffff");   //TODO:DZY[150916]未使用，在方法内会重新生成
            nickName = System.Web.HttpUtility.UrlDecode(nickName);
            realName = System.Web.HttpUtility.UrlDecode(realName);
            headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
            province = System.Web.HttpUtility.UrlDecode(province);
            city = System.Web.HttpUtility.UrlDecode(city);
            Entities.MemberInfo memberInfo = _iMemberService.GetMemberByUnionIdOpenId(unionid, openId);
            if (memberInfo == null)
            {
                memberInfo = _iMemberService.QuickRegister(username, realName, nickName, serviceProvider, openId, PlatformType.GetHashCode(),
                    unionid, sex, headimgurl, appidtype, null, city, province, spreadId: CurrentSpreadId);
                //TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
                _iBonusService.DepositToRegister(memberInfo.Id);
                //用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
                if (memberInfo.IsNewAccount)
                    num = CouponApplication.RegisterSendCoupon(memberInfo.Id, memberInfo.UserName);
                ClearDistributionSpreadCookie();
                _iMemberService.AddIntegel(memberInfo); //给用户加积分//执行登录后初始化相关操作
            }

            base.SetUserLoginCookie(memberInfo.Id);
            Application.MemberApplication.UpdateLastLoginDate(memberInfo.Id);
            WebHelper.SetCookie(CookieKeysCollection.Mall_ACTIVELOGOUT, "0", DateTime.MaxValue);

            #region 判断是否强制绑定手机号
            MemberApplication.UpdateLastLoginDate(memberInfo.Id);
            var isBind = MessageApplication.IsOpenBindSms(memberInfo.Id);
            if (!isBind)
            {
                return Json<dynamic>(success: false, data: new { num = num }, code: 99);
            }
            #endregion

            return Json<dynamic>(success: true, data: new { num = num });
        }

        [HttpPost]
        public JsonResult CheckCode(string checkCode)
        {
            try
            {
                string systemCheckCode = HttpContext.Session.Get<string>(CHECK_CODE_KEY) as string;
                bool result = systemCheckCode.ToLower() == checkCode.ToLower();
                return Json<dynamic>(success: result);
            }
            catch (Mall.Core.MallException ex)
            {
                return ErrorResult<dynamic>(msg: ex.Message);
            }
            catch (Exception ex)
            {
                Core.Log.Error("检验验证码时发生异常", ex);
                return ErrorResult<dynamic>(msg: "未知错误");
            }
        }

        public ActionResult GetCheckCode()
        {
            string code;
            var image = Core.Helper.ImageHelper.GenerateCheckCode(out code);
            HttpContext.Session.Set<string>(CHECK_CODE_KEY, code);
            return File(image.ToArray(), "image/png");
        }


        [HttpPost]
        public JsonResult SendMobileCode(string pluginId, string destination, string imagecheckCode, string action)
        {
            //验证图形验证码
            var cacheCheckCode = HttpContext.Session.Get<string>(CHECK_CODE_KEY) as string;
            //Session.Remove(CHECK_CODE_KEY);
            if (string.IsNullOrEmpty(action))
            {
                if (cacheCheckCode == null || string.IsNullOrEmpty(imagecheckCode) || imagecheckCode.ToLower() != cacheCheckCode.ToLower())
                {
                    return Json(new Result { success = false, msg = "图形验证码错误" });
                }
            }
            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            MessageHelper helper = new MessageHelper();
            var timeout = CacheKeyCollection.MemberPluginCheckTime(destination, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return ErrorResult<dynamic>(msg: "120秒内只允许请求一次，请稍后重试!");
            }
            var num = helper.GetErrorTimes(destination);
            if (num > 5)
            {
                return ErrorResult<dynamic>(msg: "你发送的次数超过限制，请15分钟后再试！");
            }
            var checkCode = new Random().Next(10000, 99999);
            Log.Info(destination + "：" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(destination, pluginId), checkCode.ToString(), cacheTimeout);
            var user = new Mall.Core.Plugins.Message.MessageUserInfo() { UserName = destination, SiteName = SiteSettings.SiteName, CheckCode = checkCode.ToString() };
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(destination, pluginId), "0", DateTime.Now.AddSeconds(120));
            _iMessageService.SendMessageCode(destination, pluginId, user);
            helper.SetErrorTimes(destination);
            return SuccessResult<dynamic>(msg: "发送成功");
        }

        [HttpPost]
        public JsonResult SendEmailCode(string pluginId, string destination)
        {
            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            MessageHelper helper = new MessageHelper();
            var timeout = CacheKeyCollection.MemberPluginCheckTime(destination, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return ErrorResult<dynamic>(msg: "120秒内只允许请求一次，请稍后重试!");
            }
            var num = helper.GetErrorTimes(destination);
            if (num > 5)
            {
                return ErrorResult<dynamic>(msg: "你发送的次数超过限制，请15分钟后再试！");
            }
            var checkCode = new Random().Next(10000, 99999);
            Log.Info(destination + "：" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(destination, pluginId), checkCode.ToString(), cacheTimeout);
            var user = new Mall.Core.Plugins.Message.MessageUserInfo() { UserName = destination, SiteName = SiteSettings.SiteName, CheckCode = checkCode.ToString() };
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(destination, pluginId), "0", DateTime.Now.AddSeconds(120));
            _iMessageService.SendMessageCode(destination, pluginId, user);
            helper.SetErrorTimes(destination);
            return SuccessResult<dynamic>(msg: "发送成功");
        }


        [HttpPost]
        public JsonResult CheckEmailCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(destination, pluginId);
            var cacheCode = Core.Cache.Get<string>(cache);
            if (cacheCode != null && cacheCode == code)
            {
                return SuccessResult<dynamic>(msg: "验证正确");
            }
            else
            {
                return ErrorResult<dynamic>(msg: "邮箱验证码不正确或者已经超时");
            }
        }


        [HttpPost]
        public JsonResult CheckMobileCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(destination, pluginId);
            var cacheCode = Core.Cache.Get<string>(cache);
            if (cacheCode != null && cacheCode == code)
            {
                return SuccessResult<dynamic>(msg: "验证正确");
            }
            else
            {
                return ErrorResult<dynamic>(msg: "手机验证码不正确或者已经超时");
            }
        }

    }
}