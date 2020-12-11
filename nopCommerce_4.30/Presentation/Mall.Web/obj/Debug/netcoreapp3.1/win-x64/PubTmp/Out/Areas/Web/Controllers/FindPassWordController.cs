using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

using Nop.Core.Http.Extensions;


namespace Mall.Web.Areas.Web.Controllers
{
    public class FindPassWordController : BaseWebController
    {
        private IMemberService _iMemberService;
        private IMemberCapitalService _iMemberCapitalService;
        private IMessageService _iMessageService;
        public FindPassWordController(IMessageService iMessageService, IMemberService iMemberService, IMemberCapitalService iMemberCapitalService)
        {
            _iMessageService = iMessageService;
            _iMemberService = iMemberService;
            _iMemberCapitalService = iMemberCapitalService;
        }
        // GET: Web/FindPassWord
        public ActionResult Index(int id)
        {
            SetTitle(id);

            string FindPayContact = "";
            if (id == 2 && CurrentUser != null)
            {
                FindPayContact = string.IsNullOrEmpty(CurrentUser.CellPhone) ? CurrentUser.Email : CurrentUser.CellPhone;
            }
            ViewBag.Contact = FindPayContact;
            return View();
        }

        //找回密码第二步
        public ActionResult Step2(int id, string key)
        {
            SetTitle(id);
            var s = Core.Cache.Get<MemberInfo>(key);
            if (s == null)
            {
                return RedirectToAction("Error", "FindPassWord");
            }
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(s.Id, item.PluginInfo.PluginId, Entities.MemberContactInfo.UserTypes.General))
            });
            ViewBag.BindContactInfo = data;
            ViewBag.Key = key;
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(s);
        }

        public ActionResult Error()
        {
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }

        public ActionResult Step3(int id, string key)
        {
            SetTitle(id);
            var s = Core.Cache.Get<MemberInfo>(key + "3");
            if (s == null)
            {
                return RedirectToAction("Error", "FindPassWord");
            }
            ViewBag.Key = key;
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }

        public ActionResult ChangePassWord(int id, string passWord, string key)
        {
            var member = Core.Cache.Get<MemberInfo>(key + "3");
            if (member == null)
            {
                return Json(new { success = false, flag = -1, msg = "验证超时" });
            }
            var userId = member.Id;
            if (id == 1)
            {
                _iMemberService.ChangePassword(userId, passWord);
            }
            else
            {
                _iMemberCapitalService.SetPayPwd(UserId, passWord);
            }
            MessageUserInfo info = new MessageUserInfo();
            info.SiteName = SiteSettings.SiteName;
            info.UserName = member.UserName;
            Task.Factory.StartNew(() => _iMessageService.SendMessageOnFindPassWord(userId, info));
            return Json(new { success = true, flag = 1, msg = "成功找回密码" });
        }

        public ActionResult Step4(int id)
        {
            SetTitle(id);
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }

        public ActionResult GetCheckCode()
        {
            string code;
            var image = Core.Helper.ImageHelper.GenerateCheckCode(out code);
            HttpContext.Session.Set<string>("FindPassWordcheckCode", code);
            return File(image.ToArray(), "image/png");
        }
        ///短信或者邮件验证码对比
        [HttpPost]
        public ActionResult CheckPluginCode(string pluginId, int code, string key)
        {
            var member = Core.Cache.Get<MemberInfo>(key);
            var cache = CacheKeyCollection.MemberFindPasswordCheck(member.UserName, pluginId);
            var cacheCode = Core.Cache.Get<int>(cache);
            if (cacheCode != 0 && cacheCode == code)
            {
                Core.Cache.Remove(CacheKeyCollection.MemberFindPasswordCheck(member.UserName, pluginId));
                Core.Cache.Insert(key + "3", member, DateTime.Now.AddMinutes(15));
                return Json(new { success = true, msg = "验证正确", key = key });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }
        void VaildCode(string checkCode)
        {
            if (string.IsNullOrWhiteSpace(checkCode))
            {
                throw new MallException("验证码不能为空");
            }
            else
            {
                string systemCheckCode = HttpContext.Session.Get<string>("FindPassWordcheckCode") as string;
                if (string.IsNullOrEmpty(systemCheckCode))
                {
                    throw new MallException("验证码超时，请刷新");
                }
                if (systemCheckCode.ToLower() != checkCode.ToLower())
                {
                    throw new MallException("验证码不正确");
                }
            }
            HttpContext.Session.Set<string>("FindPassWordcheckCode", Guid.NewGuid().ToString());
        }


        //发送短信邮件验证码
        [HttpPost]
        public ActionResult SendCode(string pluginId, string key)
        {
            var s = Core.Cache.Get<MemberInfo>(key);
            if (s == null)
                return Json(new { success = false, flag = -1, msg = "验证已超时！" });
            string destination = _iMessageService.GetDestination(s.Id, pluginId, Entities.MemberContactInfo.UserTypes.General);
            var timeout = CacheKeyCollection.MemberPluginFindPassWordTime(s.UserName, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return Json(new { success = false, flag = 0, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberFindPasswordCheck(s.UserName, pluginId), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = s.UserName, SiteName = SiteSettings.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginFindPassWordTime(s.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new { success = true, flag = 1, msg = "发送成功" });
        }

        ///第一步，检查用户邮箱手机是否存在对应的用户
        [HttpPost]
        public ActionResult CheckUser(string userName, string checkCode)
        {
            VaildCode(checkCode);//检测验证码
            return CheckUserCommon(userName);
        }

        /// <summary>
        /// 第二步中，用户重置key
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckUserNoCode(string userName)
        {
            return CheckUserCommon(userName);
        }

        /// <summary>
        /// 检查用户邮箱手机是否存在对应的用户
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private ActionResult CheckUserCommon(string userName)
        {
            var key = Guid.NewGuid().ToString().Replace("-", "");
            var member = _iMemberService.GetMemberByContactInfo(userName);

            if (member == null)
                return Json(new { success = false, tag = "username", msg = "您输入的账户名不存在或者没有绑定邮箱和手机，请核对后重新输入" });
            else
            {
                Core.Cache.Insert<Entities.MemberInfo>(key, member, DateTime.Now.AddMinutes(15));
                return Json(new { success = true, key, memberID = member.Id });
            }
        }

        private void SetTitle(int id)
        {
            ViewBag.OpType = id;
            if (id == 1)
            {
                ViewBag.Title = "找回密码";
            }
            else
            {
                ViewBag.Title = "找回支付密码";
            }
        }
    }
}