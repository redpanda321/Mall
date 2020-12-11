using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class OrderComplaintController : BaseAdminController
    {
        IComplaintService _iComplaintService;
        public OrderComplaintController(IComplaintService iComplaintService)
        {
            _iComplaintService = iComplaintService;
        }
        public ActionResult Management()
        {
            return View();
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult List(DateTime? startDate, DateTime? endDate, long? orderId, int? complaintStatus, string shopName, string userName, int page, int rows)
        {
            var queryModel = new ComplaintQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderId = orderId,
                Status = (Mall.Entities.OrderComplaintInfo.ComplaintStatus?)complaintStatus,
                ShopName = shopName,
                UserName = userName,
                PageSize = rows,
                PageNo = page
            };

            QueryPageModel<Mall.Entities.OrderComplaintInfo> orderComplaints = _iComplaintService.GetOrderComplaints(queryModel);

            var orderComplaintMode = orderComplaints.Models.ToArray().Select(item => new
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ComplaintStatus = item.Status.ToDescription(),
                ShopName = item.ShopName,
                ShopPhone = item.ShopPhone,
                UserName = item.UserName,
                UserPhone = item.UserPhone,
                ComplaintDate = item.ComplaintDate.ToShortDateString(),
                ComplaintReason = System.Text.RegularExpressions.Regex.Replace(item.ComplaintReason, @"(<|(&lt;))br[^>]*?(>|(&gt;))", "").Replace("<", "&lt;").Replace(">", "&gt;"),
                SellerReply = item.SellerReply,
                PlatRemark = string.IsNullOrWhiteSpace(item.PlatRemark) ? "" : item.PlatRemark
            }).ToList();
            return Json(new { rows = orderComplaintMode, total = orderComplaints.Total });
        }

        [OperationLog(Message = "处理交易投诉")]
        [UnAuthorize]
        [HttpPost]
        public JsonResult DealComplaint(long id, string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                return Json(new Result() { success = false, msg = "回复内容不能为空！" });
            }
            Result result = new Result();
            try
            {
                _iComplaintService.DealComplaint(id, reply);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }
    }
}