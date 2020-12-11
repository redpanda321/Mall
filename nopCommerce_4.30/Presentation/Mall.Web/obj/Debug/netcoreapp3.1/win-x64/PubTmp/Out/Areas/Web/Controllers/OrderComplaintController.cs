using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Html;
using System;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class OrderComplaintController : BaseMemberController
    {
        private IOrderService _iOrderService;
        private IShopService _iShopService;
        private IComplaintService _iComplaintService;
        private ITypeService _iTypeService;

        public OrderComplaintController(IOrderService iOrderService, IShopService iShopService, IComplaintService iComplaintService, ITypeService iTypeService)
        {
            _iOrderService = iOrderService;
            _iShopService = iShopService;
            _iComplaintService = iComplaintService;
            _iTypeService = iTypeService;
        }

        public ActionResult Index(int pageSize = 10, int pageNo = 1)
        {
            OrderQuery query = new OrderQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.UserId = CurrentUser.Id;
            query.Status = OrderInfo.OrderOperateStatus.Finish;
            var orders = _iOrderService.GetOrders(query);
            var complaints = OrderApplication.GetOrderComplaintByOrders(orders.Models.Select(p => p.Id).ToList());
            var model = orders.Models.Where(o => !complaints.Any(p => p.OrderId == o.Id));
            ViewBag.Complaints = complaints;
            var orderItems = _iOrderService.GetOrderItemsByOrderId(orders.Models.Select(p => p.Id));
            if (orderItems != null)
            {
                foreach (var item in orderItems)
                {
                    Entities.TypeInfo typeInfo = _iTypeService.GetTypeByProductId(item.ProductId);
                    var productInfo = Mall.Application.ProductManagerApplication.GetProduct(item.ProductId);
                    item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                }
            }
            ViewBag.OrderItems = orderItems;
            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = orders.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.UserPhone = CurrentUser.CellPhone;
            ViewBag.UserId = CurrentUser.Id;
            #endregion
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(model);
        }

        [HttpPost]
        public JsonResult AddOrderComplaint(Mall.Entities.OrderComplaintInfo model)
        {
            model.UserId = CurrentUser.Id;
            model.UserName = CurrentUser.UserName;
            model.ComplaintDate = DateTime.Now;
            model.Status = Mall.Entities.OrderComplaintInfo.ComplaintStatus.WaitDeal;
            var shop = _iShopService.GetShop(model.ShopId);
            var order = _iOrderService.GetOrder(model.OrderId, CurrentUser.Id);
            if (model.ComplaintReason.Length < 5)
            {
                throw new MallException("投诉内容不能小于5个字符！");
            }
            if (model.ComplaintReason.Length > 500)
            {
                throw new MallException("字数过长，限制500个字！");
            }
            if (string.IsNullOrWhiteSpace(model.UserPhone))
            {
                throw new MallException("投诉电话不能为空！");
            }
            if (order == null || order.ShopId != model.ShopId)
            {
                throw new MallException("该订单不属于当前用户！");
            }
            model.ShopName = shop == null ? "" : shop.ShopName;
            model.ShopPhone = shop == null ? "" : shop.CompanyPhone;
            if (model.ShopPhone == null)
            {
                //管理员信息
                long uid = Mall.Application.ShopApplication.GetShopManagers(shop.Id);
                Mall.DTO.MemberAccountSafety mMemberAccountSafety = Mall.Application.MemberApplication.GetMemberAccountSafety(uid);
                model.ShopPhone = mMemberAccountSafety.Phone;
            }
            model.ShopName = model.ShopName == null ? "" : model.ShopName;

            _iComplaintService.AddComplaint(model);
            return Json(new { success = true, msg = "提交成功" });
        }

        public ActionResult Record(int pageSize = 10, int pageNo = 1)
        {
            ComplaintQuery query = new ComplaintQuery();
            query.UserId = CurrentUser.Id;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            var model = _iComplaintService.GetOrderComplaints(query);
            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            #endregion
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            if (model.Models != null)
            {
                foreach (var item in model.Models)
                {
                    item.ComplaintReason = ReplaceHtmlTag(HtmlHelper.ConvertHtmlToPlainText(item.ComplaintReason));
                    item.SellerReply = ReplaceHtmlTag(HtmlHelper.ConvertHtmlToPlainText(item.SellerReply));
                }
            }
            return View(model.Models.ToList());
        }
        [HttpPost]
        public JsonResult ApplyArbitration(long id)
        {
            _iComplaintService.UserApplyArbitration(id, CurrentUser.Id);
            return Json(new { success = true, msg = "处理成功" });
        }
        [HttpPost]
        public JsonResult DealComplaint(long id)
        {
            _iComplaintService.UserDealComplaint(id, CurrentUser.Id);
            return Json(new { success = true, msg = "处理成功" });
        }

        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }

    }
}