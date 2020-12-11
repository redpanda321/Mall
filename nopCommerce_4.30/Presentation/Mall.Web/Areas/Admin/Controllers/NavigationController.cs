using System;

using Mall.Web.Framework;
using System.ComponentModel;
using Mall.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class NavigationController : BaseAdminController
    {
        INavigationService _iNavigationService;

        public NavigationController(INavigationService iNavigationService)
        {
            _iNavigationService = iNavigationService;  
        }

		public ActionResult Get()
		{
			var result = _iNavigationService.GetPlatNavigations().ToArray();
			return Json(result,true);
		}

        // GET: Admin/Navigation
        [Description("获取导航列表数据")]
        public ActionResult Management()
        {
            var result = _iNavigationService.GetPlatNavigations();
            return View(result);
        }

        [UnAuthorize]
        [Description("删除导航")]
        [OperationLog(Message = "删除导航")]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            _iNavigationService.DeletePlatformNavigation(id);
            return Json(new Result() { success = true, msg = "删除成功！" });
        }

        [UnAuthorize]
        [Description("新增导航")]
        public JsonResult Add(Entities.BannerInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.Name) && !string.IsNullOrWhiteSpace(info.Url))
            {
                _iNavigationService.AddPlatformNavigation(info);
                return Json(new Result() { success = true, msg = "添加导航成功！" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "导航名称和跳转地址不能为空！" });
            }
        }

        [UnAuthorize]
        [Description("开关导航")]
        [OperationLog(Message = "开关导航")]
        public JsonResult OpenOrClose(long Id, bool status)
        {
            try
            {
                _iNavigationService.OpenOrClose(Id, status);
                return Json(new Result() { success = true, msg = "开关导航成功！" });
            }
            catch(Exception ex)
            {
                return Json(new Result() { success = false, msg = "开关导航失败！" + ex.Message });
            }
        }

        [UnAuthorize]
        [Description("编辑导航")]
        [OperationLog(Message = "编辑导航")]
        public JsonResult Edit(Entities.BannerInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.Name) && !string.IsNullOrWhiteSpace(info.Url))
            {
                _iNavigationService.UpdatePlatformNavigation(info);
                return Json(new Result() { success = true, msg = "编辑导航成功！" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "导航名称和跳转地址不能为空！" });
            }
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult SwapDisplaySequence(long id, long id2)
        {
            _iNavigationService.SwapPlatformDisplaySequence(id, id2);
            return Json(new Result() { success = true, msg = "排序成功！" });
        }
    }
}