using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Web;

using Mall.Web.Areas.Admin.Models.Member;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class MemberController : BaseAdminController
    {
        IMemberService _iMemberService;
        IRegionService _iRegionService;
        IMemberLabelService _iMemberLabelService;
        public MemberController(IMemberService iMemberService, IRegionService iRegionService, IMemberLabelService iMemberLabelService)
        {
            _iMemberService = iMemberService;
            _iRegionService = iRegionService;
            _iMemberLabelService = iMemberLabelService;
        }

        [Description("会员管理页面")]
        public ActionResult Management(long? labelId = null)
        {
            var pageModel = _iMemberLabelService.GetMemberLabelList(new LabelQuery() { PageSize = 100 });
            ViewBag.LabelInfos = pageModel.Models.ToList();
            ViewBag.labelId = labelId;
            var grades = MemberGradeApplication.GetMemberGrades();
            return View(grades);
        }

        [Description("分页获取会员管理JSON数据")]
        public JsonResult List(MemberQuery query)
        {
            if (query.Sort == "CreateDateStr") query.Sort = "CreateDate";

            var result = MemberApplication.GetMemberList(query);
            var model = new DataGridModel<DTO.Members>()
            {
                rows = result.Models,
                total = result.Total
            };
            return Json(model);
        }

        [Description("导出会员数据")]
        /// <summary>
        /// 导出会员数据
        /// </summary>
        /// <param name="keywords">会员名</param>
        /// <param name="weChatNick">微信昵称</param>
        /// <param name="mobile">手机号码</param>
        /// <param name="gradeId">会员等级</param>
        /// <param name="status">会员状态</param>
        /// <param name="platform">会员来源</param>
        /// <param name="labels">会员标签</param>
        /// <param name="registTimeStart">注册时间开始时间</param>
        /// <param name="registTimeEnd">注册时间结束时间</param>
        /// <param name="isFocusWeiXin">是否关注微信</param>
        /// <param name="isSeller">是否为入驻商家</param>
        /// <param name="logintimeStart">登录时间开始</param>
        /// <param name="logintimeEnd">登录时间结束</param>
        /// <returns></returns>
        public ActionResult ExportToExcel(string keywords = "", string weChatNick = null,string mobile = null,
            long? gradeId = null,bool? status = null,int? platform=null, long? labels = null, 
            string registTimeStart = null, string registTimeEnd = null, bool? isFocusWeiXin = null, bool? isSeller = null,
            string logintimeStart = null, string logintimeEnd = null)
        {
            #region 获取查询结果
            DateTime? rtstart = null;
            DateTime? rtend = null;
            DateTime? ltstart = null;
            DateTime? ltend = null;
            if (!string.IsNullOrWhiteSpace(registTimeStart))
            {
                rtstart = DateTime.Parse(registTimeStart);
            }
            if (!string.IsNullOrWhiteSpace(registTimeEnd))
            {
                rtend = DateTime.Parse(registTimeEnd);
            }
            if (!string.IsNullOrWhiteSpace(logintimeStart))
            {
                ltstart = DateTime.Parse(logintimeStart);
            }
            if (!string.IsNullOrWhiteSpace(logintimeEnd))
            {
                ltend = DateTime.Parse(logintimeEnd);
            }
            var query = new MemberQuery()
            {
                PageNo = 1,
                PageSize = int.MaxValue,
                keyWords = keywords,
                weChatNick = weChatNick,
                Mobile = mobile,
                GradeId = gradeId,
                Status = status,
                Platform=platform,
                Labels = labels.HasValue ? new long[] { labels.Value } : null,
                RegistTimeStart = rtstart,
                RegistTimeEnd = rtend,
                IsFocusWeiXin = isFocusWeiXin,
                IsSeller = isSeller,
                LoginTimeStart = ltstart,
                LoginTimeEnd = ltend,
            };
            if (gradeId.HasValue)
            {
                var expenditureRange = MemberApplication.GetMemberGradeRange(gradeId.Value);
                query.MinIntegral = expenditureRange.MinIntegral;
                query.MaxIntegral = expenditureRange.MaxIntegral;
            }
            var result = MemberApplication.GetMemberList(query);
            #endregion
            return ExcelView("ExportMemberinfo", "会员信息", result.Models.ToList());
            #region 构建Excel文档
            //  ViewData.Model = result.Models;
            // string viewHtml = RenderPartialViewToString(this, "ExportMemberinfo");
            // return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("会员信息_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));
            #endregion
        }

        [NonAction]
        protected string RenderPartialViewToString(Controller controller, string partialViewName)
        {

            //get Razor view engine
            var razorViewEngine = EngineContext.Current.Resolve<IRazorViewEngine>();


       //     IView view = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName).View;

            IView view = razorViewEngine.FindView(controller.ControllerContext, partialViewName, true).View;



            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData, controller.TempData, writer,null);
                viewContext.View.RenderAsync(viewContext).Wait();
                return writer.GetStringBuilder().ToString();
            }
        }


        /// <summary>
        /// 会员详情页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MemberDetail(long id)
        {
            var member = MemberApplication.GetMembers(id);
            var model = new MemberViewModels.MemberDetail();
            member.Map(model);

            return View(model);
        }


        /// <summary>
        /// 取会员购买记录
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public JsonResult GetMemberBuyList(long id, string timeStart, string timeEnd, int page, int rows)
        {
            DateTime startDate, endDate;
            var query = new OrderQuery()
            {
                PageNo = page,
                PageSize = rows
            };
            if (DateTime.TryParse(timeStart, out startDate))
            {
                query.StartDate = startDate;
            }
            if (DateTime.TryParse(timeEnd, out endDate))
            {
                query.EndDate = endDate;
            }
            var orderPageModel = OrderApplication.GetUserBuyRecord(id, query);
            var model = new { rows = orderPageModel.Models, total = orderPageModel.Total };
            return Json(model, true);
        }
        /// <summary>
        /// 取会员三个月的消费统计
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetMemberBuyStatistics(long userId)
        {
            var orderStatistics = OrderAndSaleStatisticsApplication.GetLastThreeMonthOrderStatisticsByUser(userId);
            return Json(new { data = orderStatistics, success = true }, true);
        }


        public ActionResult Detail(long id)
        {
            var item = _iMemberService.GetMember(id);
            var Region = _iRegionService.GetFullName(item.RegionId);
            var member = new DTO.MemberModel()
            {
                Id = item.Id,
                UserName = item.UserName,
                LastLoginDate = item.LastLoginDate,
                QQ = item.QQ,
                Points = item.Points,
                RealName = item.RealName,
                Email = item.Email,
                Disabled = item.Disabled,
                Expenditure = item.Expenditure,
                OrderNumber = item.OrderNumber,
                CellPhone = item.CellPhone,
                CreateDate = item.CreateDate,
                Address = item.Address
            };
            ViewBag.Region = Region;
            return PartialView("Detail", member);
        }
        public JsonResult Lock(long id)
        {
            _iMemberService.LockMember(id);
            return Json(new Result() { success = true, msg = "冻结成功！" });
        }

        public JsonResult UnLock(long id)
        {
            _iMemberService.UnLockMember(id);
            return Json(new Result() { success = true, msg = "解冻成功！" });
        }

        [OperationLog(Message = "删除会员", ParameterNameList = "id")]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            _iMemberService.DeleteMember(id);
            return Json(new Result() { success = true, msg = "删除成功！" });
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
            _iMemberService.BatchDeleteMember(listid.ToArray());
            return Json(new Result() { success = true, msg = "批量删除成功！" });
        }

        [HttpPost]
        public JsonResult BatchLock(string ids)
        {
            var strArr = ids.Split(',');
            List<long> listid = new List<long>();
            foreach (var arr in strArr)
            {
                listid.Add(Convert.ToInt64(arr));
            }

            _iMemberService.BatchLock(listid.ToArray());
            return Json(new Result() { success = true, msg = "批量锁定成功！" });
        }

        public JsonResult GetMembers(bool? status, string keyWords)
        {
            var after = _iMemberService.GetMembers(status, keyWords);

            var values = after.Select(item => new { key = item.Id, value = item.UserName });
            return Json(values);
        }

        public JsonResult ChangePassWord(long id, string password)
        {
            if (DemoAuthorityHelper.IsDemo())
            {
                var manager = _iMemberService.GetMember(id);
                if (manager.UserName.ToLower() == "selleradmin")
                {
                    return Json(new Result() { success = false, msg = "演示数据禁止修改！" });
                }
            }
            if (password.Trim().Length < 6 || password.Trim().Length > 20)
            {
                return Json(new Result() { success = false, msg = "密码在6-20位之间，不能包含空格！" });
            }
            password = password.Trim();
            _iMemberService.ChangePassword(id, password);
            return Json(new Result() { success = true, msg = "修改成功！" });
        }

        public JsonResult GetMemberLabel(long id)
        {
            var memberLabels = _iMemberService.GetMemberLabels(id);
            return Json(new { Success = true, Data = memberLabels });
        }

        public JsonResult SetMemberLabel(long id, string labelids)
        {
            List<long> ids = new List<long>();
            if (!string.IsNullOrWhiteSpace(labelids))
            {
                ids = labelids.Split(',').Select(s => long.Parse(s)).ToList();
            }
            _iMemberService.SetMemberLabel(id, ids);
            return Json(new { Success = true });
        }

        public JsonResult SetMembersLabel(string ids, string labelids)
        {
            var lids = labelids.Split(',').Select(s => long.Parse(s));
            var uids = ids.Split(',').Select(s => long.Parse(s));
            _iMemberService.SetMembersLabel(uids.ToArray(), lids);
            return Json(new { Success = true });
        }


        /// <summary>
        /// 购买力度搜索结果设置标签
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="labelids"></param>
        /// <returns></returns>
        public JsonResult SetAllMembersLabel(MemberPowerQuery query, string labels)
        {
            var lids = labels.Split(',').Select(s => long.Parse(s));
            //  MemberApplication.SetMembersLabel(query, lids);
            return Json(new { Success = true });
        }

        public JsonResult SetResultMembersLabel(MemberQuery query, string labels)
        {
            var lids = labels.Split(',').Select(s => long.Parse(s));
            //   MemberApplication.SetResultMembersLabel(query, lids);
            return Json(new { Success = true });
        }



        #region 购买力度

        ///购买力度
        public ActionResult ManagementPower(MemberStatisticsType? StatisticsType)
        {
            var pageModel = _iMemberLabelService.GetMemberLabelList(new LabelQuery() { PageSize = int.MaxValue });
            var LabelInfos = pageModel.Models.ToList();

            var categorys = CategoryApplication.GetCategoryByParentId(0);
            ViewBag.categorys = categorys;
            ViewBag.StatisticsType = StatisticsType.HasValue ? (int)StatisticsType.Value : -1;
            return View(LabelInfos);
        }

        /// <summary>
        /// 购买力列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult MemberPowerList(MemberPowerQuery query)
        {
            var result = MemberApplication.GetPurchasingPowerMember(query);

            var models = result.Models.Select(item =>
            new
            {
                Id = item.Id,
                UserName = item.UserName,
                CellPhone = item.CellPhone,
                GradeName = item.GradeName,
                NetAmount = item.NetAmount,
                OrderNumber = item.OrderNumber,
                LastConsumptionTime = item.LastConsumptionTime.HasValue ? item.LastConsumptionTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                CategoryNames = item.CategoryNames,
            }
            );

            var model = new { rows = models, total = result.Total, ids = string.Join(",", models.Select(p => p.Id)) };
            return Json(model, true);
        }
        #endregion



        /// <summary>
        /// 获取优惠券
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CouponList(string name, int page, int rows)
        {
            var result = CouponApplication.GetCouponByName(name, DateTime.Now, 2, page, rows);//主动发放
            var datas = result.Models.ToList();
            DataGridModel<Mall.DTO.CouponModel> couponModel = new DataGridModel<Mall.DTO.CouponModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(couponModel, true);
        }






        /// <summary>
        /// 发送优惠券
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="couponIds"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendCoupon(string ids, string couponIds)
        {
            var cIds = couponIds.Split(',').Select(s => long.Parse(s));
            var uids = ids.Split(',').Select(s => long.Parse(s)).ToList();
            if (cIds.Count() == 0)
            {
                throw new MallException("请选择优惠券！");
            }
            if (uids.Count() == 0)
            {
                throw new MallException("请选择发送会员！");
            }

            Dictionary<string, int> dictResult = new Dictionary<string, int>();
            CouponApplication.SendCouponByUserIds(uids, cIds.ToList());
            return Json(new { Success = true }, true);
        }

        /// <summary>
        /// 发送优惠券
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="couponIds"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendAllCoupon(MemberPowerQuery query, string couponIds)
        {
            var cIds = couponIds.Split(',').Select(s => long.Parse(s));
            if (cIds.Count() == 0)
            {
                throw new MallException("请选择优惠券！");
            }
            CouponApplication.SendCoupon(query, cIds);
            return Json(new Result { success = true, msg = "发送成功！" });
        }

        public JsonResult SendAllCouponBymember(MemberQuery query, string couponIds)
        {
            var cIds = couponIds.Split(',').Select(s => long.Parse(s));
            if (cIds.Count() == 0)
            {
                throw new MallException("请选择优惠券！");
            }
            CouponApplication.SendCoupon(null, cIds, query);
            return Json(new Result { success = true, msg = "发送成功！" });
        }

        /// <summary>
        /// 选中发送短信
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="sendCon"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendSms(string ids, string sendCon)
        {
            var userIds = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(s => long.Parse(s)).ToArray();
            if (userIds == null || userIds.Length == 0)
            {
                throw new MallException("请选择发送对象！");
            }
            if (sendCon.Equals(""))
            {
                throw new MallException("请输入发送内容！");
            }
            MemberApplication.SendMsgByUserIds(userIds, HttpUtility.HtmlEncode(sendCon));

            return Json(new { Success = true }, true);
        }

        /// <summary>
        /// 结果发送短信
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sendCon"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendAllSms(MemberPowerQuery query, string sendCon)
        {
            MemberApplication.SendMsg(query, sendCon);
            return Json(new { Success = true }, true);
        }

        /// <summary>
        /// 会员管理结果群发短信
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendAllSmsBymember(MemberQuery query, string sendCon)
        {
            MemberApplication.SendMsg(null, sendCon, "会员管理", query);
            return Json(new { Success = true }, true);
        }

        /// <summary>
        /// 微信模板类型
        /// </summary>
        /// <param name="pageIdx"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetWXMaterialList(int pageIdx, int pageSize)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
            {
                throw new MallException("未配置微信公众号");
            }
            var offset = (pageIdx - 1) * pageSize;
            var list = WXMsgTemplateApplication.GetMediaMsgTemplateList(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, offset, pageSize);
            return Json(list);
        }

        /// <summary>
        /// 微信图片
        /// </summary>
        /// <param name="mediaid"></param>
        /// <returns></returns>
        public ActionResult GetMedia(string mediaid)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
            {
                throw new MallException("未配置微信公众号");
            }
            MemoryStream stream = new MemoryStream();
            WXMsgTemplateApplication.GetMedia(mediaid, siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, stream);
            return File(stream.ToArray(), "Image/png");
        }

        /// <summary>
        /// 结果发送微信消息
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="msgtype"></param>
        /// <param name="mediaid"></param>
        /// <param name="msgcontent"></param>
        /// <returns></returns>
        public JsonResult SendAllWXGroupMessage(MemberPowerQuery query, string msgtype, string mediaid = "", string msgcontent = "")
        {
            MemberApplication.SendWeiMessage(query, msgtype, mediaid, msgcontent);

            return Json(new { success = true, msg = "发送成功！" });
        }

        public JsonResult SendAllWXGroupMessageByMember(MemberQuery query, string msgtype, string mediaid = "", string msgcontent = "")
        {
            MemberApplication.SendWeiMessage(null, msgtype, mediaid, msgcontent, query);

            return Json(new { success = true, msg = "发送成功！" });
        }

        /// <summary>
        /// 选中发送微信消息
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="msgtype"></param>
        /// <param name="mediaid"></param>
        /// <param name="msgcontent"></param>
        /// <returns></returns>
        public JsonResult SendWXGroupMessage(string ids, string msgtype, string mediaid = "", string msgcontent = "")
        {
            var userIds = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(s => long.Parse(s)).ToArray();
            if (userIds == null || userIds.Length == 0)
            {
                throw new MallException("请选择发送对象！");
            }

            MemberApplication.SendWeiMessageByUserIds(userIds, msgtype, mediaid, msgcontent);

            return Json(new { success = true, msg = "发送成功！" });
        }




        #region 会员分组
        public ActionResult MemberGroup()
        {
            var memberGroup = MemberApplication.GetPlatformMemberGroup();
            return View(memberGroup);
        }
        #endregion

    }
}