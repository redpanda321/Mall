using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using System.Linq;

using AutoMapper;
using Mall.DTO;
using Microsoft.AspNetCore.Mvc;
using Mall.Application;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class MemberInviteController : BaseAdminController
    {
        private IMemberService  _iMemberService;
        private IMemberInviteService _iMemberInviteService;
        public MemberInviteController(IMemberInviteService iMemberInviteService, IMemberService iMemberService)
        {
            _iMemberInviteService = iMemberInviteService;
            _iMemberService = iMemberService;
        }
        public ActionResult Setting()
        {
            var model = _iMemberInviteService.GetInviteRule();
        //    Mapper.CreateMap<Mall.Entities.InviteRuleInfo, InviteRuleModel>();
            var mapModel = model.Map< InviteRuleModel>();
            return View(mapModel);
        }

        [HttpPost]
        public ActionResult SaveSetting(InviteRuleModel model)
        {
            if (ModelState.IsValid)
            {
               // Mapper.CreateMap<InviteRuleModel, Mall.Entities.InviteRuleInfo>();
                var mapModel = model.Map<Mall.Entities.InviteRuleInfo>();
                _iMemberInviteService.SetInviteRule(mapModel);
                return Json(new Result() { success = true, msg = "保存成功！" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "数据验证错误！" });
            }
        }


        public JsonResult GetMembers(bool? status, string keyWords)
        {
            var after = _iMemberService.GetMembers(status, keyWords);
            var values = after.Select(item => new { key = item.Id, value = item.UserName });
            return Json(values);
        }

        public ActionResult Management()
        {
            return View();
        }

        public ActionResult List(int page, string keywords, int rows)
        {
            InviteRecordQuery query = new InviteRecordQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.userName = keywords;
            var pageModel= _iMemberInviteService.GetInviteList(query);

            var jsonModel=pageModel.Models.ToList().Select(a=> new{a.Id,a.InviteIntegral,RegTime=a.RegTime.ToString("yyyy-MM-dd"),a.RegIntegral,a.RegName,a.UserName});
            var model = new{ rows = jsonModel, total = pageModel.Total };
            return Json(model);
        }
    }
}