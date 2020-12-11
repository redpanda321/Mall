using Mall.Application;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;



namespace Mall.Web.Areas.Web.Controllers
{
    public class UserIntegralController : BaseMemberController
    {
        private IGiftsOrderService _iGiftsOrderService;
        private IMemberIntegralService _iMemberIntegralService;
        public UserIntegralController(IGiftsOrderService iGiftsOrderService, IMemberIntegralService iMemberIntegralService)
        {
            _iGiftsOrderService = iGiftsOrderService;
            _iMemberIntegralService = iMemberIntegralService;
        }

        // GET: Web/UserIntegral
        public ActionResult Index(int? type, int pageSize = 10, int pageNo = 1)
        {
            var model = _iMemberIntegralService.GetIntegralChangeRule();
            ViewBag.IntegralPerMoney = model == null ? 0 : model.IntegralPerMoney;
            
            ViewBag.Integral = MemberIntegralApplication.GetAvailableIntegral(CurrentUser.Id);
            Mall.Entities.MemberIntegralInfo.IntegralType? integralType = null;
            if (type.HasValue)
            {
                integralType = (Mall.Entities.MemberIntegralInfo.IntegralType)type.Value;
            }
            var query = new IntegralRecordQuery() { IntegralType = integralType, UserId = CurrentUser.Id, PageNo = pageNo, PageSize = pageSize };
            var result = _iMemberIntegralService.GetIntegralRecordListForWeb(query);
            var list = result.Models.Select(item => {
                var actions = _iMemberIntegralService.GetIntegralRecordAction(item.Id);
                return new MemberIntegralRecordInfo
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    RecordDate = item.RecordDate,
                    Integral = item.Integral,
                    TypeId = item.TypeId,
                    ReMark = GetRemarkFromIntegralType(item.TypeId, actions, item.ReMark)
                };
            });
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = result.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(list);
        }

        public ActionResult IntegralRule()
        {
            var model = _iMemberIntegralService.GetUserHistroyIntegralGroup(CurrentUser.Id);
            var rule = _iMemberIntegralService.GetIntegralRule();
            model.LoginIntegralRule = rule.FirstOrDefault(p => p.TypeId == (int)MemberIntegralInfo.IntegralType.Login)?.Integral ?? 0;
            model.CommentIntegralRule = rule.FirstOrDefault(p => p.TypeId == (int)MemberIntegralInfo.IntegralType.Comment)?.Integral ?? 0;
            model.ConsumptionIntegralRule = rule.FirstOrDefault(p => p.TypeId == (int)MemberIntegralInfo.IntegralType.Consumption)?.Integral ?? 0;
            model.BindWxIntegralRule = rule.FirstOrDefault(p => p.TypeId == (int)MemberIntegralInfo.IntegralType.BindWX)?.Integral ?? 0;
            model.RegIntegralRule = rule.FirstOrDefault(p => p.TypeId == (int)MemberIntegralInfo.IntegralType.Reg)?.Integral ?? 0;
            model.InviteIntegralRule = rule.FirstOrDefault(p => p.TypeId == (int)MemberIntegralInfo.IntegralType.InvitationMemberRegiste)?.Integral ?? 0;
            model.SignInRule = rule.FirstOrDefault(p => p.TypeId == (int)MemberIntegralInfo.IntegralType.SignIn)?.Integral ?? 0;
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(model);
        }

        private string GetRemarkFromIntegralType(MemberIntegralInfo.IntegralType type, ICollection<MemberIntegralRecordActionInfo> recordAction, string remark = "")
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
                //case MemberIntegral.IntegralType.Comment:
                //    remark = "商品评价（商品ID：" + recordAction.FirstOrDefault().VirtualItemId + ")";
                //    break;
                //case MemberIntegral.IntegralType.ProportionRebate:
                //    remark = "使用订单号(" +recordAction.FirstOrDefault().VirtualItemId + ")";
                //    break;
                default:
                    return remark;
            }
            return remark;
        }

        #region 礼品订单
        public ActionResult OrderList(string skey, Mall.Entities.GiftOrderInfo.GiftOrderStatus? status, int page = 1)
        {
            int rows = 12;
            GiftsOrderQuery query = new GiftsOrderQuery();
            query.Skey = skey;
            if (status != null)
            {
                if ((int)status != 0)
                {
                    query.Status = status;
                }
            }
            query.UserId = CurrentUser.Id;
            query.PageSize = rows;
            query.PageNo = page;
            var orderdata = _iGiftsOrderService.GetOrders(query);
            var info = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = rows,
                TotalItems = orderdata.Total
            };
            var orderlist = orderdata.Models;
            ViewBag.pageInfo = info;
            _iGiftsOrderService.OrderAddUserInfo(orderlist);
            var result = orderlist;
            foreach (var item in result)
            {
                item.Address = ClearHtmlString(item.Address);
                item.CloseReason = ClearHtmlString(item.CloseReason);
                item.UserRemark = ClearHtmlString(item.UserRemark);
            }
            ViewBag.OrderItems = _iGiftsOrderService.GetOrderItemByOrder(orderlist.Select(p => p.Id).ToList());
            return View(result);
        }
        /// <summary>
        /// 确认到货
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmOrder(long id)
        {
            Result result = new Result();
            _iGiftsOrderService.ConfirmOrder(id, CurrentUser.Id);
            result.success = true;
            result.status = 1;
            result.msg = "订单完成";
            return Json(result);
        }
        #endregion

        //TODO：YZY 应该提交
        /// <summary>
        /// 清理引号类字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ClearHtmlString(string str)
        {
            string result = str;
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Replace("'", "&#39;");
                result = result.Replace("\"", "&#34;");
                result = result.Replace(">", "&gt;");
                result = result.Replace("<", "&lt;");
            }
            return result;
        }
    }
}