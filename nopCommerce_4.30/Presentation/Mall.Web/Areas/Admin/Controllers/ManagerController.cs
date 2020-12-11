using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class ManagerController : BaseAdminController
    {
        IManagerService _iManagerService;
        IPrivilegesService _iPrivilegesService;
        public ManagerController(IManagerService iManagerService, IPrivilegesService iPrivilegesService)
        {
            _iManagerService = iManagerService;
            _iPrivilegesService = iPrivilegesService;
            
        }
        // GET: Admin/Member
        public ActionResult Management()
        {
            return View();
        }
        public JsonResult Add(ManagerInfoModel model)
        {
            var manager = new Entities.ManagerInfo() { UserName = model.UserName, Password = model.Password, RoleId = model.RoleId };
            _iManagerService.AddPlatformManager(manager);
            return Json(new Result() { success = true, msg = "添加成功！" });
        }

        [UnAuthorize]
        public JsonResult List(int page, string keywords, int rows, bool? status = null)
        {
            var result = _iManagerService.GetPlatformManagers(new ManagerQuery { PageNo = page, PageSize = rows });
            var role = _iPrivilegesService.GetPlatformRoles().ToList();
            var managers = result.Models.ToList().Select(item => {
                string strRoleName = "系统管理员";
                if (item.RoleId != 0)
                {
                    var roledetail = role.Where(a => a.Id == item.RoleId).FirstOrDefault();
                    strRoleName = (roledetail != null ? roledetail.RoleName : "");
                }
                return new
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    CreateDate = item.CreateDate.ToString("yyyy-MM-dd HH:mm"),
                    RoleName = strRoleName,
                    RoleId = item.RoleId
                };
            });
            var model = new { rows = managers, total = result.Total };
            return Json(model);
        }

        [HttpPost]
        public JsonResult Delete(long id)
        {
            _iManagerService.DeletePlatformManager(id);
            return Json(new Result() { success = true, msg = "删除成功！" });
        }

        [HttpPost]
        public JsonResult RoleList()
        {
            var roles = _iPrivilegesService.GetPlatformRoles().Select(item => new { Id = item.Id, RoleName = item.RoleName });
            return Json(roles);
        }

        [HttpPost]
        public JsonResult BatchDelete(string ids)
        {
            var strArr = ids.Split(',');
            List<long> listid = new List<long>();
            foreach (var arr in strArr)
            {
                listid.Add(Convert.ToInt64(arr));
            }
            _iManagerService.BatchDeletePlatformManager(listid.ToArray());
            return Json(new Result() { success = true, msg = "批量删除成功！" });
        }

        public JsonResult ChangePassWord(long id, string password, long roleId)
        {
            if (DemoAuthorityHelper.IsDemo())
            {
                var manager = _iManagerService.GetPlatformManager(id);
                if (manager.UserName.ToLower()=="admin")
                {
                    return Json(new Result() { success = false, msg = "演示数据禁止修改！" });
                }
            }
            _iManagerService.ChangePlatformManagerPassword(id, password, roleId);
            return Json(new Result() { success = true, msg = "修改成功！" });
        }


        public JsonResult IsExistsUserName(string userName)
        {
            return Json(new { Exists = _iManagerService.CheckUserNameExist(userName, true) });
        }
    }
}