using Mall.Web.Framework;

using Mall.IServices;
using Mall.Core.Helper;
using System;
using Mall.Application;
using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class HomeController : BaseAdminController
    {
        IShopService _iShopService;
        IStatisticsService _iStatisticsService;
        IManagerService _iManagerService;
        public HomeController(IShopService iShopService, IStatisticsService iStatisticsService, IManagerService iManagerService)
        {
            _iShopService = iShopService;
            _iStatisticsService = iStatisticsService;
            _iManagerService = iManagerService;
        }

        [UnAuthorize]
        public ActionResult Index()
        {
            
            return RedirectToAction("Console");
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult ChangePassword(string oldpassword, string password)
        {
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            var model = CurrentManager;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldpassword) + model.PasswordSalt);
            if (pwd == model.Password)
            {
                _iManagerService.ChangePlatformManagerPassword(model.Id, password, 0);
                return Json(new Result() { success = true, msg = "修改成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "旧密码错误" });
            }
        }


        [UnAuthorize]
        public JsonResult CheckOldPassword(string password)
        {
            var model = CurrentManager;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(password) + model.PasswordSalt);
            if (model.Password == pwd)
            {
                return Json(new Result() { success = true });
            }
            return Json(new Result() { success = false });
        }

        [UnAuthorize]
        public ActionResult Copyright()
        {
            return View();
        }

        [UnAuthorize]
        public ActionResult About()
        {
            return View();
        }
        [UnAuthorize]
        public ActionResult Console()
        {
            var model = StatisticApplication.GetPlatformConsole();
            return View(model);
        }

        [HttpGet]
        [UnAuthorize]
        public ActionResult ProductRecentMonthSaleRank()
        {
            var end = DateTime.Now.Date;
            var begin = end.AddMonths(-1);
            var model = StatisticApplication.GetProductSaleRankingChart(0, begin, end, SaleDimension.Count, 15); 
            return Json(new { success = true, chart = model });
        }

        [HttpGet]
        [UnAuthorize]
        public ActionResult GetRecentMonthShopSaleRankChart()
        {
            var end = DateTime.Now.Date;
            var begin = end.AddMonths(-1);
            var model = StatisticApplication.GetShopRankingChart(begin, end, SaleDimension.Count, 15);
            return Json(new { success = true, chart = model });
        }
    }
}