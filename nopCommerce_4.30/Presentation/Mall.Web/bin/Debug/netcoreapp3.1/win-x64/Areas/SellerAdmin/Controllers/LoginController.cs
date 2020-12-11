using Mall.CommonModel;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http.Extensions;
using System;
using System.Configuration;


namespace Mall.Web.Areas.SellerAdmin.Controllers
{

    [Area("SellerAdmin")]
    public class LoginController : BaseController
    {
        private IShopService _iShopService;
        private IManagerService _iManagerService;
        public LoginController(IShopService iShopService, IManagerService iManagerService)
        {
            _iShopService = iShopService;
            _iManagerService = iManagerService;
        }
        /// <summary>
        /// 同一用户名无需验证的的尝试登录次数
        /// </summary>
        const int TIMES_WITHOUT_CHECKCODE = 3;

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Login", new { area = "Web" });
        }   


        [HttpPost]
        public JsonResult Login(string username, string password, string checkCode)
        {
            try
            {
                //检查输入合法性
                CheckInput(username, password);

                //检查验证码
                CheckCheckCode(username, checkCode);

                var manager = _iManagerService.Login(username, password);
                if (manager == null)
                {
                    throw new Mall.Core.MallException("用户名和密码不匹配");
                }

                var shopinfo = _iShopService.GetShop(manager.ShopId);

                ClearErrorTimes(username);//清除输入错误记录次数
                return Json(new { success = true, userId = UserCookieEncryptHelper.Encrypt(manager.Id, CookieKeysCollection.USERROLE_SELLERADMIN), stage = shopinfo.Stage });
            }
            catch (Mall.Core.MallException ex)
            {
                int errorTimes = SetErrorTimes(username);
                return Json(new { success = false, msg = ex.Message, errorTimes = errorTimes, minTimesWithoutCheckCode = TIMES_WITHOUT_CHECKCODE });
            }
            catch (Exception ex)
            {
                int errorTimes = SetErrorTimes(username);
                Core.Log.Error("用户" + username + "登录时发生异常", ex);
                return Json(new { success = false, msg = "未知错误", errorTimes = errorTimes, minTimesWithoutCheckCode = TIMES_WITHOUT_CHECKCODE });
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
                string systemCheckCode = HttpContext.Session.Get<string>("checkCode") as string;
                bool result = systemCheckCode.ToLower() == checkCode.ToLower();
                return Json(new { success = result });
            }
            catch (Mall.Core.MallException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                Core.Log.Error("检验验证码时发生异常", ex);
                return Json(new { success = false, msg = "未知错误" });
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

                string systemCheckCode = HttpContext.Session.Get<string>("checkCode") as string;
                if (systemCheckCode.ToLower() != checkCode.ToLower())
                    throw new Mall.Core.MallException("验证码错误");

                //生成随机验证码，强制使验证码过期（一次提交必须更改验证码）
                HttpContext.Session.Set<string>("checkCode" , Guid.NewGuid().ToString());
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
            //var times = timesObject == null ? 0 : (int)timesObject;
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

    }
}