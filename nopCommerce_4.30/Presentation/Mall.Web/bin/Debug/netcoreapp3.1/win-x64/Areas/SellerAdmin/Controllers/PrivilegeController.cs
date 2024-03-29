﻿using Mall.IServices;
using Mall.Web.Areas.Admin.Models;
using System.Linq;

using Mall.Web.Framework;
using Newtonsoft.Json;
using Mall.CommonModel;
using Mall.Application;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class PrivilegeController : BaseSellerController
    {
        private IPrivilegesService _iPrivilegesService;
        private IManagerService _iManagerService;
        private IShopService _iShopService;
        public PrivilegeController(IPrivilegesService iPrivilegesService, IManagerService iManagerService, IShopService iShopService)
        {
            _iPrivilegesService = iPrivilegesService;
            _iManagerService = iManagerService;
            _iShopService = iShopService;
        }

        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        [Description("角色列表显示")]
        [UnAuthorize]
        public JsonResult List()
        {
            var shopId = CurrentSellerManager.ShopId;
            var list = _iPrivilegesService.GetSellerRoles(shopId);
            var result = list.Select(item => new { Id = item.Id, Name = item.RoleName });
            var model = new { rows = result };
            return Json(model);
        }

        public ActionResult Edit(long id)
        {
            var shopId = CurrentSellerManager.ShopId;
            SetPrivileges();
            var model = _iPrivilegesService.GetSellerRole(id, shopId);
            RoleInfoModel result = new RoleInfoModel() { ID = model.Id, RoleName = model.RoleName };
            var s = new JsonSerializerSettings();
            s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            var privilege = RoleApplication.GetPrivileges(model.Id);
            ViewBag.RolePrivilegeInfo = JsonConvert.SerializeObject(privilege.Select(item => new { Privilege = item }), s);
            return View(result);
        }

        [UnAuthorize]
        [ShopOperationLog(Message = "编辑商家权限组")]
        [HttpPost]
        public JsonResult Edit(string roleJson, long id)
        {
            if (ModelState.IsValid)
            {
                var shopId = CurrentSellerManager.ShopId;
                var s = new JsonSerializerSettings();
                s.MissingMemberHandling = MissingMemberHandling.Ignore;
                s.NullValueHandling = NullValueHandling.Ignore;
                var role = JsonConvert.DeserializeObject<Entities.RoleInfo>(roleJson, s);
                role.Id = id;
                role.ShopId = CurrentSellerManager.ShopId;
                _iPrivilegesService.UpdateSellerRole(role);
                RoleApplication.ClearRoleCache(role.Id);
                var users = _iManagerService.GetSellerManagerByRoleId(id, shopId).ToList();
                foreach (var user in users)
                {
                    string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(user.Id);
                    Core.Cache.Remove(CACHE_MANAGER_KEY);
                }
            }
            else
            {
                return Json(new { success = true, msg = "验证失败" });
            }
            return Json(new { success = true });
        }

        private void SetPrivileges()
        {

            var shopId = CurrentSellerManager.ShopId;
            var shopInfo = _iShopService.GetShop(shopId);
            ViewBag.IsSelf = shopInfo.IsSelf;
            var privileges = PrivilegeHelper.SellerAdminPrivileges;
            ViewBag.Privileges = privileges;
        }

        public ActionResult Add()
        {
            SetPrivileges();
            return View();
        }
        [Description("权限组添加")]
        [HttpPost]
        [UnAuthorize]
        public JsonResult Add(string roleJson)
        {
            if (ModelState.IsValid)
            {
                var s = new JsonSerializerSettings();
                s.MissingMemberHandling = MissingMemberHandling.Ignore;
                s.NullValueHandling = NullValueHandling.Ignore;
                var role = JsonConvert.DeserializeObject<Entities.RoleInfo>(roleJson, s);
                role.ShopId = CurrentSellerManager.ShopId;
                _iPrivilegesService.AddSellerRole(role);
            }
            else
            {
                return Json(new { success = true, msg = "验证失败" });
            }
            return Json(new { success = true });
        }

        [ShopOperationLog(Message = "删除商家权限组")]
        [UnAuthorize]
        public JsonResult Delete(long id)
        {
            var shopId = CurrentSellerManager.ShopId;
            var service = _iPrivilegesService;
            var roles = service.GetPlatformRole(id);
            if (_iManagerService.GetSellerManagerByRoleId(id, shopId).Count() > 0)
            {
                return Json(new Result() { success = false, msg = "该权限组下还有管理员，不允许删除！" });
            }
            service.DeleteSellerRole(id, shopId);
            return Json(new Result() { success = true, msg = "删除成功！" });
        }
    }
}