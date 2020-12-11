using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using Mall.Core;
using Mall.Entities;
using Mall.Application;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class MemberIntegralController : BaseAdminController
    {

        private IMemberService _iMemberService;
        private IMemberIntegralService _iMemberIntegralService;
        private IMemberGradeService _iMemberGradeService;
        private IMemberIntegralConversionFactoryService _iMemberIntegralConversionFactoryService;
        public MemberIntegralController(IMemberService iMemberService, 
            IMemberIntegralService iMemberIntegralService, 
            IMemberGradeService iMemberGradeService,
            IMemberIntegralConversionFactoryService iMemberIntegralConversionFactoryService)
        {
            _iMemberService = iMemberService;
            _iMemberIntegralService = iMemberIntegralService;
            _iMemberGradeService = iMemberGradeService;
            _iMemberIntegralConversionFactoryService = iMemberIntegralConversionFactoryService;
        }

        // GET: Admin/MemberIntegral
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Save(string Operation, int Integral, string userName, int? userId, string reMark)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new Core.MallException("该用户不存在");
            }
            var memeber = _iMemberService.GetMemberByName(userName);
            if (memeber == null)
            {
                throw new Core.MallException("该用户不存在");
            }
            if (Integral <= 0||Integral>100000)
            {
                throw new Core.MallException("积分必须为大于0且小于十万的整数");
            }
            var info = new MemberIntegralRecordInfo();
            info.UserName = userName;
            info.MemberId = memeber.Id;
            info.RecordDate = DateTime.Now;
            info.TypeId = MemberIntegralInfo.IntegralType.SystemOper;
            info.ReMark = reMark;
            if (Operation == "sub")
            {
                Integral = -Integral;
            }
            var memberIntegral = _iMemberIntegralConversionFactoryService.Create(MemberIntegralInfo.IntegralType.SystemOper, Integral);

            _iMemberIntegralService.AddMemberIntegral(info, memberIntegral);
            string CACHE_USER_KEY = CacheKeyCollection.Member(memeber.Id);
            Core.Cache.Remove(CACHE_USER_KEY);
            return Json(new Result() { success = true, msg = "操作成功" });
        }

        [Description("分页获取会员积分JSON数据")]
        public JsonResult List(IntegralQuery query)
        {
            var result = MemberIntegralApplication.GetMemberIntegrals(query);
            return Json(result, true);
        }

        private string GetMemberGrade(List<MemberGradeInfo> memberGrade, int historyIntegrals)
        {
            var grade = memberGrade.Where(a => a.Integral <= historyIntegrals).OrderByDescending(a => a.Integral).FirstOrDefault();
            if (grade == null)
                return "Vip0";
            return grade.GradeName;
        }



        [HttpPost]
        public JsonResult GetMembers(bool? status, string keyWords)
        {
            var after = _iMemberService.GetMembers(status, keyWords);
            var values = after.Select(item => new { key = item.Id, value = item.UserName });
            return Json(values);
        }

        public ActionResult Detail(int id)
        {
            ViewBag.UserId = id;
            return View();
        }

        [HttpPost]
        public JsonResult GetMemberIntegralDetail(int page, int? userId, Mall.Entities.MemberIntegralInfo.IntegralType? type, DateTime? startDate, DateTime? endDate, int rows)
        {
            var query = new IntegralRecordQuery() { StartDate = startDate, EndDate = endDate, IntegralType = type, UserId = userId, PageNo = page, PageSize = rows };
            var result = _iMemberIntegralService.GetIntegralRecordList(query);
            var list = result.Models.Select(item =>
            {
                var actions = _iMemberIntegralService.GetIntegralRecordAction(item.Id);
                return new
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    RecordDate = item.RecordDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Integral = item.Integral,
                    Type = item.TypeId.ToDescription(),
                    Remark = GetRemarkFromIntegralType(item.TypeId, actions, item.ReMark)
                };
            });

            var model = new { rows = list, total = result.Total };
            return Json(model);

        }

        private string GetRemarkFromIntegralType(Mall.Entities.MemberIntegralInfo.IntegralType type, ICollection<Mall.Entities.MemberIntegralRecordActionInfo> recordAction, string remark = "")
        {
            if (recordAction == null || recordAction.Count == 0)
                return remark;
            switch (type)
            {
                //case MemberIntegral.IntegralType.InvitationMemberRegiste:
                //    remark = "邀请用户(用户ID：" + recordAction.FirstOrDefault().VirtualItemId+")";
                //    break;
                case Mall.Entities.MemberIntegralInfo.IntegralType.Consumption:
                    var orderIds = "";
                    foreach (var item in recordAction)
                    {
                        orderIds += item.VirtualItemId + ",";
                    }
                    remark = "使用订单号(" + orderIds.TrimEnd(',') + ")";
                    break;
                case Mall.Entities.MemberIntegralInfo.IntegralType.Comment:
                    remark = "商品评价（订单号：" + recordAction.FirstOrDefault().VirtualItemId + ")";
                    break;
                //case MemberIntegral.IntegralType.ProportionRebate:
                //    remark = "使用订单号(" +recordAction.FirstOrDefault().VirtualItemId + ")";
                //    break;
                default:
                    return remark;
            }
            return remark;
        }
    }
}