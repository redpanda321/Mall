using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using System;
using System.Linq;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class OrderComplaintController : BaseSellerController
    {
        private IComplaintService _iComplaintService;
        public OrderComplaintController(IComplaintService iComplaintService)
        {
            _iComplaintService = iComplaintService;
        }
        public ActionResult Management()
        {
            return View();
        }


        [HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, long? orderId, int? complaintStatus, string userName, int page, int rows)
        {
            var queryModel = new ComplaintQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderId = orderId,
                Status = (Mall.Entities.OrderComplaintInfo.ComplaintStatus?)complaintStatus,
                ShopId = CurrentSellerManager.ShopId,
                UserName = userName,
                PageSize = rows,
                PageNo = page
            };

            var orderComplaints = _iComplaintService.GetOrderComplaints(queryModel);
            var orders = OrderApplication.GetOrders(orderComplaints.Models.Select(p => p.OrderId));

            var orderComplaintMode = orderComplaints.Models.Select(item =>
            {
                var order = orders.FirstOrDefault(p => p.Id == item.OrderId);
                return new
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    OrderTotalAmount = order.OrderTotalAmount.ToString("F2"),
                    PaymentTypeName = (order.OrderTotalAmount == 0 && order.CapitalAmount == 0) ? "积分支付" : order.PaymentTypeName,
                    ComplaintStatus = item.Status.ToDescription(),
                    ShopName = item.ShopName,
                    ShopPhone = item.ShopPhone,
                    UserName = item.UserName,
                    UserPhone = item.UserPhone,
                    ComplaintDate = item.ComplaintDate.ToShortDateString(),
                    ComplaintReason = System.Text.RegularExpressions.Regex.Replace(item.ComplaintReason, @"(<|(&lt;))br[^>]*?(>|(&gt;))", "").Replace("<", "&lt;").Replace(">", "&gt;"),
                    SellerReply = item.SellerReply,
                    PlatRemark = item.PlatRemark
                };
            });
            return Json(new { rows = orderComplaintMode, total = orderComplaints.Total });
        }


        [HttpPost]
        public JsonResult DealComplaint(long id, string reply)
        {
            if(string.IsNullOrWhiteSpace(reply))
            {
                return Json(new Result() { success=false,msg="回复内容不能为空！"});
            }

            Result result = new Result();
            try
            {
                _iComplaintService.SellerDealComplaint(id, reply);
                result.success = true;
            }
            catch(Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }
    }
}