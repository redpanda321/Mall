using Mall.IServices;
using Mall.Web.Framework;
using System;
using System.Configuration;

using Mall.Core.Helper;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Nop.Core.Http.Extensions;
using Microsoft.Extensions.Configuration;

namespace Mall.Web.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class LoginController : BaseController
    {

        private IManagerService _iManagerService;

        public LoginController(IManagerService iManagerService)
        {
            _iManagerService = iManagerService; // 第三方容器.构造<T>();
        }

        /// <summary>
        /// 同一用户名无需验证的的尝试登录次数
        /// </summary>
        const int TIMES_WITHOUT_CHECKCODE = 3;

        // GET: SellerAdmin/Login
        public ActionResult Index()
        {
            //  var t = ConfigurationManager.AppSettings["IsInstalled"];
            //  if (!(null == t || bool.Parse(t)))
            bool t = configuration.GetValue<bool>("Mall:IsInstalled");
            if(!t)
            {
                return RedirectToAction("Agreement", "Installer", new { area = "Web" });
            }
            return View();
        }



        [HttpPost]
        public JsonResult Login(string username, string password, string checkCode, bool keeplogin)
        {
            string host = HttpContext.Request.Host.ToString();
            try
            {
                //检查输入合法性
                CheckInput(username, password);

                //检查验证码
                CheckCheckCode(username, checkCode);

                var manager = _iManagerService.Login(username, password, true);
                if (manager == null)
                {
                    throw new Mall.Core.MallException("用户名和密码不匹配");
                }
                ClearErrorTimes(username);//清除输入错误记录次数

                //日龙修改
                //去除将过期店铺变为不可用店铺的功能
                //ServiceApplication.Create<IManagerService>().UpdateShopStatus();
                if(keeplogin)
                    base.SetAdminLoginCookie(manager.Id, DateTime.Now.AddDays(3));
                else
                    base.SetAdminLoginCookie(manager.Id);
             //   System.Web.HttpResponse.RemoveOutputCacheItem("/m-wap");//更新授权要移除掉移动端首页缓存
              //  System.Web.HttpResponse.RemoveOutputCacheItem("/m-wap/");//更新授权要移除掉移动端首页缓存

                return Json(new Result { success = true });
            }
            catch (Mall.Core.MallException ex)
            {
                int errorTimes = SetErrorTimes(username);
                return Json(new { success = false, msg = ex.Message, errorTimes = errorTimes, minTimesWithoutCheckCode = TIMES_WITHOUT_CHECKCODE });
            }
            catch (Exception ex)
            {
                int errorTimes = SetErrorTimes(username);
                Exception innerEx = GerInnerException(ex);
                string showerrmsg = "未知错误";
                if (innerEx is Mall.Core.MallException)
                {
                    showerrmsg = innerEx.Message;
                }
                else
                {
                    Core.Log.Error("用户" + username + "登录时发生异常", ex);
                }
                return Json(new { success = false, msg = showerrmsg, errorTimes = errorTimes, minTimesWithoutCheckCode = TIMES_WITHOUT_CHECKCODE });
            }

        }

        [HttpPost]
        public JsonResult GetErrorLoginTimes(string username)
        {
            var errorTimes = GetErrorTimes(username);
            return Json(new { errorTimes = errorTimes });
        }


        [HttpPost]
        public JsonResult CheckCode(string checkCode)
        {
            try
            {
                string systemCheckCode = HttpContext.Session.Get<string>("checkCode");
                bool result = systemCheckCode.ToLower() == checkCode.ToLower();
                return Json(new Result { success = result });
            }
            catch (Mall.Core.MallException ex)
            {
                return Json(new Result { success = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                Core.Log.Error("检验验证码时发生异常", ex);
                return Json(new Result { success = false, msg = "未知错误" });
            }
        }

        void CheckInput(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Mall.Core.MallException("请填写用户名");

            if (string.IsNullOrWhiteSpace(password))
                throw new Mall.Core.MallException("请填写密码");

        }

        void CheckCheckCode(string username, string checkCode)
        {
            var errorTimes = GetErrorTimes(username);
            if (errorTimes >= TIMES_WITHOUT_CHECKCODE)
            {
                if (string.IsNullOrWhiteSpace(checkCode))
                    throw new Mall.Core.MallException("30分钟内登录错误3次以上需要提供验证码");

                string systemCheckCode = HttpContext.Session.Get<string>("checkCode");
                if (systemCheckCode.ToLower() != checkCode.ToLower())
                    throw new Mall.Core.MallException("验证码错误");

                //生成随机验证码，强制使验证码过期（一次提交必须更改验证码）
                HttpContext.Session.Set<string>("checkCode", Guid.NewGuid().ToString());
            }
        }


        public ActionResult GetCheckCode()
        {
            string code;
            var image = Core.Helper.ImageHelper.GenerateCheckCode(out code);
            HttpContext.Session.Set<string>("checkCode", code);
            return File(image.ToArray(), "image/png");
        }


        /// <summary>
        /// 获取指定用户名在30分钟内的错误登录次数
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        int GetErrorTimes(string username)
        {
            var timesObject = Core.Cache.Get<int>(CacheKeyCollection.ManagerLoginError(username));
            //var times = timesObject == null ? 0 : int.Parse(timesObject.ToString());
            return timesObject;
        }

        void ClearErrorTimes(string username)
        {
            Core.Cache.Remove(CacheKeyCollection.ManagerLoginError(username));
        }

        /// <summary>
        /// 设置错误登录次数
        /// </summary>
        /// <param name="username"></param>
        /// <returns>返回最新的错误登录次数</returns>
        int SetErrorTimes(string username)
        {
            var times = GetErrorTimes(username) + 1;
            Core.Cache.Insert(CacheKeyCollection.ManagerLoginError(username), times, DateTime.Now.AddMinutes(30.0));//写入缓存
            return times;
        }

        public ActionResult Logout()
        {
            WebHelper.DeleteCookie(CookieKeysCollection.PLATFORM_MANAGER);

            return RedirectToAction("index");
        }
    }
}