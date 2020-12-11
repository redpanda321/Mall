using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.Core.Plugins.Message;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class UserInfoController : BaseMemberController
    {
        private IMessageService _iMessageService;
        private IMemberService _iMemberService;

        public UserInfoController(IMessageService iMessageService, IMemberService iMemberService)
        {
            _iMemberService = iMemberService;
            _iMessageService = iMessageService;
        }
        // GET: Web/UserInfo
        public ActionResult Index()
        {
            var model = MemberApplication.GetMembers(CurrentUser.Id);
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var sms = PluginsManagement.GetPlugins<ISMSPlugin>();
            var smsInfo = sms.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(CurrentUser.Id, item.PluginInfo.PluginId, Entities.MemberContactInfo.UserTypes.General))
            }).FirstOrDefault();
            var email = PluginsManagement.GetPlugins<IEmailPlugin>();
            var emailInfo = email.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(CurrentUser.Id, item.PluginInfo.PluginId, Entities.MemberContactInfo.UserTypes.General))
            }).FirstOrDefault();


            ViewBag.BindSMS = smsInfo;
            ViewBag.BindEmail = emailInfo;
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(model);
        }

        [HttpPost]
        public JsonResult GetCurrentUserInfo()
        {

            if (CurrentUser != null)
            {
                var memberInfo = CurrentUser;
                string name = string.IsNullOrWhiteSpace(memberInfo.Nick) ? memberInfo.UserName : memberInfo.Nick;
                return Json(new { success = true, name = name });

            }
            else
            {

                return Json(new { success = false, name = "" });
            }
        }

        public JsonResult UpdateUserInfo(MemberUpdate model)
        {
            if (!model.BirthDay.HasValue && !CurrentUser.BirthDay.HasValue)
            {
                return Json(new Result() { success = false, msg = "生日必须填写" });
            }
            //if (string.IsNullOrWhiteSpace(model.CellPhone) || string.IsNullOrWhiteSpace(CurrentUser.CellPhone))
            //{
            //    return Json(new Result() { success = false, msg = "请先绑定手机号码" });
            //}
            if (string.IsNullOrWhiteSpace(model.RealName))
            {
                return Json(new Result() { success = false, msg = "用户姓名必须填写" });
            }
            model.Id = CurrentUser.Id;
            MemberApplication.UpdateMemberInfo(model);
            return Json(new Result() { success = true, msg = "修改成功" });
        }

        public ActionResult ReBind(string pluginId)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            ViewBag.ShortName = messagePlugin.Biz.ShortName;
            ViewBag.id = pluginId;
            ViewBag.ContactInfo = _iMessageService.GetDestination(CurrentUser.Id, pluginId, Entities.MemberContactInfo.UserTypes.General);
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View();
        }

        [HttpPost]
        public ActionResult CheckCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination); //带上发送目标防止修改
            var cacheCode = Core.Cache.Get<string>(cache);
            if (string.IsNullOrEmpty(cacheCode))
                return Json(new Result { success = false, msg = "验证码已经超时" });

            var member = CurrentUser;
            if (cacheCode == code)
            {
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination));
                Core.Cache.Insert("Rebind" + member.Id, "step2", DateTime.Now.AddMinutes(30));
                return Json(new { success = true, msg = "验证正确", key = member.Id });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确" });
            }
        }


        [HttpPost]  //验证第二步需要修改信息了
        public ActionResult CheckCodeStep2(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get<string>(cache);
            var member = CurrentUser;

            if (cacheCode != null && cacheCode == code)
            {
                var service = _iMessageService;
                if (service.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
                {
                    return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
                }
                if (pluginId.ToLower().Contains("email"))
                {
                    member.Email = destination;
                }
                else if (pluginId.ToLower().Contains("sms"))
                {
                    member.CellPhone = destination;
                }
                _iMemberService.UpdateMember(member);
                service.UpdateMemberContacts(new Entities.MemberContactInfo()
                {
                    Contact = destination,
                    ServiceProvider = pluginId,
                    UserId = CurrentUser.Id,
                    UserType = Entities.MemberContactInfo.UserTypes.General
                });
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination));
                Core.Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
                Core.Cache.Remove("Rebind" + CurrentUser.Id);
                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }

        [HttpPost]
        public ActionResult SendCode(string pluginId, string destination, bool checkBind = false)
        {
            if (checkBind && _iMessageService.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
            {
                return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
            }
            var timeout = CacheKeyCollection.MemberPluginReBindTime(CurrentUser.UserName, pluginId); //验证码超时时间
            if (Core.Cache.Exists(timeout))
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试！" });
            }
            var checkCode = new Random().Next(10000, 99999);
            //TODO yx 短信验证码超时时间需改成可配置，并且短信模板需添加超时时间变量
            var cacheTimeout = DateTime.Now.AddMinutes(2);
#if DEBUG
            //Log.Debug(destination + "[SendCode]" + checkCode);
#endif
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination), checkCode.ToString(), cacheTimeout);
            var user = new MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = SiteSettings.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginReBindTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));//验证码超时时间
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public ActionResult SendCodeStep2(string pluginId, string destination, bool checkBind = false)
        {
            if (checkBind && _iMessageService.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
            {
                return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
            }
            var timeout = CacheKeyCollection.MemberPluginReBindStepTime(CurrentUser.UserName, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
#if DEBUG
            //Log.Debug(destination + "[SendCodeStep2]" + checkCode);
#endif
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination), checkCode.ToString(), cacheTimeout);
            var user = new MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = SiteSettings.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginReBindStepTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        public ActionResult ReBindStep2(string pluginId, string key)
        {
            if (Core.Cache.Get<string>("Rebind" + key) != "step2")
            {
                RedirectToAction("ReBind", new { pluginId = pluginId });
            }
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            ViewBag.ShortName = messagePlugin.Biz.ShortName;
            ViewBag.id = pluginId;
            ViewBag.ContactInfo = _iMessageService.GetDestination(CurrentUser.Id, pluginId, Entities.MemberContactInfo.UserTypes.General);
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View();
        }

        public ActionResult ReBindStep3(string name)
        {
            ViewBag.ShortName = name;
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View();
        }

        public ActionResult ChangePassword()
        {
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View();
        }

        [HttpPost]
        public JsonResult ChangePassword(string oldpassword, string password)
        {
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            var model = CurrentUser;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldpassword) + model.PasswordSalt);
            if (pwd == model.Password)
            {
                _iMemberService.ChangePassword(model.Id, password);
                return Json(new Result() { success = true, msg = "修改成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "旧密码错误" });
            }
        }

        public JsonResult CheckOldPassWord(string password)
        {
            var model = CurrentUser;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(password) + model.PasswordSalt);
            if (model.Password == pwd)
            {
                return Json(new Result() { success = true });
            }
            return Json(new Result() { success = false });
        }

        /// <summary>
        /// 获取用户标识
        /// </summary>
        /// <returns></returns>
        public JsonResult UserIdentity()
        {
            if (CurrentUser == null)
                return Json(0);

            var identity = (CurrentUser.Id + CurrentUser.CreateDate.ToString("yyyyMMddHHmmss")).GetHashCode();
            return Json(identity);
        }

        [HttpPost]
        public JsonResult BindSms(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(destination, pluginId);
            var cacheCode = Core.Cache.Get<string>(cache);
            var member = CurrentUser;
            if (cacheCode != null && cacheCode == code)
            {
                var service = _iMessageService;
                if (service.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
                {
                    return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
                }
                if (pluginId.ToLower().Contains("email"))
                {
                    member.Email = destination;
                }
                else if (pluginId.ToLower().Contains("sms"))
                {
                    member.CellPhone = destination;
                }
                _iMemberService.UpdateMember(member);
                service.UpdateMemberContacts(new Entities.MemberContactInfo()
                {
                    Contact = destination,
                    ServiceProvider = pluginId,
                    UserId = CurrentUser.Id,
                    UserType = Entities.MemberContactInfo.UserTypes.General
                });
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination));
                Core.Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
                Core.Cache.Remove("Rebind" + CurrentUser.Id);
                return Json(new Result() { success = true, msg = "绑定成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }

        [HttpPost]
        public JsonResult IsConBindSms()
        {
            return Json<dynamic>(success: MessageApplication.IsOpenBindSms(CurrentUser.Id));
        }
    }
}