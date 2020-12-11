using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.Core.Plugins;
using Mall.SmallProgAPI.Model.ParamsModel;
using Mall.Web.Framework;
using Mall.Application;
using System;
using System.Linq;


using Microsoft.AspNetCore.Mvc;

namespace Mall.SmallProgAPI
{
    [ApiExceptionFilter]
    public class BaseApiController<TUser> : HiAPIController<TUser>
    {
        /// <summary>
        /// 检测用户登录信息是否有效
        /// </summary>
        public virtual void CheckUserLogin()
        {
            if (CurrentUser == null)
            {
                throw new MallApiException(ApiErrorCode.Invalid_User_Key_Info, "NOUser");
            }
        }


        /// <summary>
        /// 获取图形验证码
        /// </summary>
        /// <returns></returns>

        /*
        public object GetImageCheckCode()
        {
            string code;
            var image = Core.Helper.ImageHelper.GenerateCheckCode(out code);
            var id = Guid.NewGuid().ToString("N");
            Cache.Insert("ImageCheckCode:" + id, code, 600);
            var file = new System.Web.Mvc.FileContentResult(image.ToArray(), "image/png");
            return Json(new
            {
                Id = id,
                file.ContentType,
                file.FileContents,
                file.FileDownloadName
            });
        }
        */
        /// <summary>
        /// 发送手机或邮箱验证码
        /// </summary>
        /// <param name="imageCheckCode"></param>
        /// <param name="contact"></param>
        /// <returns></returns>

        [HttpGet("GetPhoneOrEmailCheckCode")]
        public object GetPhoneOrEmailCheckCode(string contact, string id = null, string imageCheckCode = null, bool checkBind = false)
        {
            if (CurrentUser == null)
            {
                if (string.IsNullOrEmpty(imageCheckCode))
                    return Json(ErrorResult<int>("请输入验证码"));

                var key = "ImageCheckCode:" + id;
                var systemCheckCode = Cache.Get<string>(key);
                if (systemCheckCode == null)
                    return Json(ErrorResult<int>("验证码已过期"));

                if (systemCheckCode.ToLower() != imageCheckCode.ToLower())
                    return Json(ErrorResult<int>("验证码错误"));
                else
                    Cache.Remove(key);
            }

            string msg;
            var checkResult = this.CheckContact(contact, out msg);
            if (!checkResult)
                return Json(ErrorResult<int>(string.IsNullOrEmpty(msg) ? "手机或邮箱号码不存在" : msg));

            PluginInfo pluginInfo;
            var isMobile = Core.Helper.ValidateHelper.IsMobile(contact);
            if (isMobile)
                pluginInfo = PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.SMS).First();
            else
                pluginInfo = PluginsManagement.GetInstalledPluginInfos(PluginType.Email).First();

            if (pluginInfo == null)
            {
                Log.Error(string.Format("未找到{0}发送插件", isMobile ? "短信" : "邮件"));
                return Json(ErrorResult<int>("验证码发送失败"));
            }

            if (checkBind && Application.MessageApplication.GetMemberContactsInfo(pluginInfo.PluginId, contact, Entities.MemberContactInfo.UserTypes.General) != null)
            {
                return Json(ErrorResult<int>(contact + "已经绑定过了！"));
            }

            var timeoutKey = CacheKeyCollection.MemberPluginCheckTime(contact, pluginInfo.PluginId);
            if (Core.Cache.Exists(timeoutKey))
                return Json(ErrorResult<int>("请求过于频繁，请稍后再试!"));
            int cacheTime = 60;
            Core.Cache.Insert(timeoutKey, cacheTime, DateTime.Now.AddSeconds(cacheTime));

            var checkCode = new Random().Next(10000, 99999);
            var siteName = Application.SiteSettingApplication.SiteSettings.SiteName;
            var message = new Mall.Core.Plugins.Message.MessageUserInfo() { UserName = contact, SiteName = siteName, CheckCode = checkCode.ToString() };

            Application.MessageApplication.SendMessageCode(contact, pluginInfo.PluginId, message);
            //缓存验证码
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(contact, pluginInfo.PluginId), checkCode.ToString(), DateTime.Now.AddMinutes(10));

            return SuccessResult( "验证码发送成功");
        }

        /// <summary>
        /// 验证手机或邮箱验证码
        /// </summary>
        /// <param name="checkCode">验证码</param>
        /// <param name="contact">手机号或邮箱</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetCheckPhoneOrEmailCheckCode")]
        public object GetCheckPhoneOrEmailCheckCode(string checkCode, string contact)
        {
            if (string.IsNullOrEmpty(checkCode))
                return Json(ErrorResult<string>("请输入验证码"));

            PluginInfo pluginInfo;
            var isMobile = Core.Helper.ValidateHelper.IsMobile(contact);
            if (isMobile)
                pluginInfo = PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.SMS).First();
            else
                pluginInfo = PluginsManagement.GetInstalledPluginInfos(PluginType.Email).First();

            var cache = CacheKeyCollection.MemberPluginCheck(contact, pluginInfo.PluginId);
            var cacheCode = Core.Cache.Get<string>(cache);

            if (cacheCode != null && cacheCode == checkCode)
                return OnCheckCheckCodeSuccess(contact);
            else
                return ErrorResult<string>("验证码输入错误");
        }

        /// <summary>
        /// 是否强制绑定手机号
        /// </summary>
        /// <returns></returns>
        public object IsConBindSms()
        {
            if (CurrentUser == null)
                return Json(ErrorResult<string>("未登录"));

            var isBind = MessageApplication.IsOpenBindSms(CurrentUserId);
            return Json(ApiResult(isBind, "", ""));
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="contact">手机号或邮箱</param>
        /// <param name="password">密码</param>
        /// <param name="repeatPassword">确认密码</param>
        /// <returns></returns>
        [HttpPost]
        public object PostChangePassword(LoginModPwdModel value)
        {
            if (!string.IsNullOrEmpty(value.Certificate))
                return ChangePassowrdByCertificate(value.Certificate, value.Password);

            return ChangePasswordByOldPassword(value.OldPassword, value.Password);
        }

        /// <summary>
        /// 修改支付密码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        public object PostChangePayPwd(LoginModPwdModel value)
        {
            if (!string.IsNullOrEmpty(value.Certificate))
            {
                return ChangePayPwdByCertificate(value.Certificate, value.Password);
            }
            return ChangePayPwdByOldPassword(value.OldPassword, value.Password);
        }

        /// <summary>
        /// 根据验证码验证成功后的凭证修改密码
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected virtual object ChangePayPwdByCertificate(string certificate, string password)
        {
            return Json(ErrorResult<int>(""));
        }

        /// <summary>
        /// 根据旧密码修改密码
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected virtual object ChangePayPwdByOldPassword(string oldPassword, string password)
        {
            return ErrorResult<int>("");
        }

        /// <summary>
        /// 根据验证码验证成功后的凭证修改密码
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected virtual object ChangePassowrdByCertificate(string certificate, string password)
        {
            return ErrorResult<int>("");
        }

        /// <summary>
        /// 根据旧密码修改密码
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected virtual object ChangePasswordByOldPassword(string oldPassword, string password)
        {
            return ErrorResult<int>("");
        }

        /// <summary>
        /// 发送验证码之前检查联系方式是否存在
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="errorMessage">手机号不存在时的提示</param>
        /// <returns></returns>
        protected virtual bool CheckContact(string contact, out string errorMessage)
        {
            errorMessage = string.Empty;
            return false;
        }

        protected virtual object OnCheckCheckCodeSuccess(string contact)
        {
            var certificate = this.CreateCertificate(contact);
            dynamic result = SuccessResult<string>();
            result.certificate = certificate;
            return result;
        }
        /// <summary>
        /// 短信或邮箱验证成功后创建用户凭证用于后续操作
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        protected virtual string CreateCertificate(string contact)
        {
            return string.Empty;
        }

        protected override TUser GetUser()
        {
            throw new NotImplementedException();
        }
    }

    public class BaseApiController : BaseApiController<Entities.MemberInfo>
    {
        /// <summary>
        /// 微信小程序信任登录服务标识
        /// </summary>
        public const string SmallProgServiceProvider = "WeiXinSmallProg";
        public override void CheckUserLogin()
        {
            base.CheckUserLogin();
            if (CurrentUser.Disabled)
            {
                throw new MallApiException(ApiErrorCode.Invalid_User_Key_Info, "openId");
            }
        }
        protected override Entities.MemberInfo GetUser()
        {
            var openId = this.CurrentUserOpenId;
            if (!string.IsNullOrWhiteSpace(openId))
            {
                var userInfo = Application.MemberApplication.GetMemberByOpenId(SmallProgServiceProvider, openId);
                return userInfo;
            }

            return null;
        }
        /// <summary>
        /// 小程序使用openId标识用户
        /// </summary>
        public string CurrentUserOpenId
        {
            get
            {
                string userkey = "";
                userkey = WebHelper.GetQueryString("openId");
                if (string.IsNullOrWhiteSpace(userkey))
                {
                    userkey = WebHelper.GetFormString("openId");
                }
                return userkey;
            }
        }
        public new long CurrentUserId
        {
            get
            {
                if (CurrentUser != null)
                {
                    return CurrentUser.Id;
                }
                throw new MallApiException(ApiErrorCode.Invalid_User_Key_Info, "openId");
            }
        }

        /// <summary>
        /// 检测是否已开启门店功能
        /// </summary>

        protected void CheckOpenStore()
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)
                throw new Core.MallException("门店未授权！");
        }
    }
}
