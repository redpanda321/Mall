using Mall.API.Model;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Mall.API
{
    public class ShopBranchOrderRefundController : BaseShopBranchLoginedApiController
    {


        [HttpGet("GetRefund")]
        public List<OrderRefundApiModel> GetRefund(int? refundMode, long? orderId = null, int pageNo = 1, /*页码*/ int pageSize = 10/*每页显示数据量*/)
        {
            CheckUserLogin();

            var queryModel = new RefundQuery()
            {
                ShopId = CurrentShopBranch.ShopId,
                ShopBranchId = this.CurrentUser.ShopBranchId,
                PageSize = pageSize,
                PageNo = pageNo,
                ShowRefundType = refundMode,
                OrderId = orderId
            };

            var refunds = Application.RefundApplication.GetOrderRefunds(queryModel);

            return FullModel(refunds.Models);
        }
        [HttpPost("PostReply")]
        public object PostReply(OrderRefundReplyModel reply)
        {
            if (reply == null)
                return ErrorResult("参数不能为空");

            CheckUserLogin();

            switch (reply.AuditStatus)
            {
                case OrderRefundInfo.OrderRefundAuditStatus.UnAudit:
                    if (string.IsNullOrWhiteSpace(reply.SellerRemark))
                    {
                        throw new MallException("请填写拒绝理由");
                    }
                    break;
            }

            var refund = Application.RefundApplication.GetOrderRefund(reply.RefundId);

            if (refund == null || refund.ShopId != CurrentShopBranch.ShopId)
                return ErrorResult("无效的售后申请编号");

            var order = Application.OrderApplication.GetOrder(refund.OrderId);
            if (order == null || order.ShopBranchId != this.CurrentUser.ShopBranchId)
                return ErrorResult("无效的售后申请编号");

            if (order.DeliveryType != CommonModel.DeliveryType.SelfTake
                && reply.AuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving
                )
            {
                //如果不是自提订单，则状态还是为待买家寄货，不能直接到待商家收货
                reply.AuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery;
            }

            if (refund.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving && reply.AuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                RefundApplication.SellerConfirmRefundGood(reply.RefundId, this.CurrentUser.UserName, reply.SellerRemark);
            else
                RefundApplication.SellerDealRefund(reply.RefundId, reply.AuditStatus, reply.SellerRemark, this.CurrentUser.UserName);

            return SuccessResult("操作成功");
        }
        [HttpGet("GetRefundLogs")]
        public object GetRefundLogs(int refundId)
        {
            CheckUserLogin();

            var refund = RefundApplication.GetOrderRefund(refundId);

            if (refund == null || refund.ShopId != CurrentShopBranch.ShopId)
                return ErrorResult("无效的售后申请编号");

            var order = Application.OrderApplication.GetOrder(refund.OrderId);
            if (order == null || order.ShopBranchId != this.CurrentUser.ShopBranchId)
                return ErrorResult("无效的售后申请编号");

            var refundLogs = RefundApplication.GetRefundLogs(refundId);

            var logs = new List<object>();

            var roleMap = new Dictionary<OrderRefundStep, int>();//操作步骤 由谁完成的
            roleMap.Add(OrderRefundStep.Confirmed, 2);
            roleMap.Add(OrderRefundStep.UnAudit, 1);
            roleMap.Add(OrderRefundStep.UnConfirm, 1);
            roleMap.Add(OrderRefundStep.WaitAudit, 0);
            roleMap.Add(OrderRefundStep.WaitDelivery, 1);
            roleMap.Add(OrderRefundStep.WaitReceiving, 1);

            foreach (var log in refundLogs)
            {
                logs.Add(new
                {
                    Role = roleMap[log.Step],//操作者角色，0：买家，1：门店，2：平台
                    Step = log.Step,
                    log.OperateDate,
                    log.Remark
                });
            }

            var model = new OrderRefundApiModel();
            refund.Map(model);
            model.CertPics = new string[3];
            model.CertPics[0] = MallIO.GetRomoteImagePath(model.CertPic1);
            model.CertPics[1] = MallIO.GetRomoteImagePath(model.CertPic2);
            model.CertPics[2] = MallIO.GetRomoteImagePath(model.CertPic3);

            return new
            {
                success = true,
                Refund = model,
                Logs = logs
            };
        }
        [HttpGet("GetRefundInfo")]
        public OrderRefundApiModel GetRefundInfo(int refundId)
        {
            CheckUserLogin();

            var refund = Application.RefundApplication.GetOrderRefund(refundId, shopId: CurrentShopBranch.ShopId);

            return FullModel(new List<OrderRefund>() { refund })[0];
        }

        private List<OrderRefundApiModel> FullModel(List<OrderRefund> refunds)
        {
            var orders = Application.OrderApplication.GetOrders(refunds.Select(p => p.OrderId));
            var orderItems = Application.OrderApplication.GetOrderItemsByOrderId(refunds.Select(p => p.OrderId));

            var result = refunds.Select(item =>
            {
                var orditems = new List<DTO.OrderItem>();
                var order = orders.FirstOrDefault(o => o.Id == item.OrderId);
                if (item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    orditems = orderItems.Where(d => d.OrderId == item.OrderId).ToList();
                }
                else
                {
                    orditems.Add(orderItems.FirstOrDefault(oi => oi.Id == item.OrderItemId));
                }
                foreach (var orderItem in orditems)
                {
                    orderItem.ThumbnailsUrl = MallIO.GetRomoteProductSizeImage(orderItem.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_100);
                    Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(orderItem.ProductId);
                    orderItem.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    orderItem.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    orderItem.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    var productInfo = ProductManagerApplication.GetProduct(orderItem.ProductId);
                    if (productInfo != null)
                    {
                        orderItem.ColorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : orderItem.ColorAlias;
                        orderItem.SizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : orderItem.SizeAlias;
                        orderItem.VersionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : orderItem.VersionAlias;
                    }
                }

                var member = MemberApplication.GetMember(order.UserId);
                var model = new OrderRefundApiModel();
                item.Map(model);
                model.Status = item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited ? (int)item.ManagerConfirmStatus : (int)item.SellerAuditStatus;
                model.StatusDescription = item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited ? item.ManagerConfirmStatus.ToDescription() : (order.ShopBranchId > 0 ?
                                   ((CommonModel.Enum.OrderRefundShopAuditStatus)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription());
                model.StatusDescription = model.StatusDescription.Replace("商家", "门店");
                model.RefundStatus = model.RefundStatus.Replace("商家", "门店");
                //model.UserName = member.RealName;
                //model.UserCellPhone = member.CellPhone;
                model.UserName = item.ContactPerson;
                model.UserCellPhone = item.ContactCellPhone;
                model.OrderItem = orditems;
                model.CertPics = new string[3];
                model.CertPics[0] = MallIO.GetRomoteImagePath(model.CertPic1);
                model.CertPics[1] = MallIO.GetRomoteImagePath(model.CertPic2);
                model.CertPics[2] = MallIO.GetRomoteImagePath(model.CertPic3);
                string shopBranchName = order.ShopName;
                if (order.ShopBranchId > 0)
                {
                    var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                    if (shopBranchInfo != null)
                    {
                        shopBranchName = shopBranchInfo.ShopBranchName;
                    }
                }
                model.ShopName = shopBranchName;
                model.ReasonDetail = item.ReasonDetail;

                return model;
            });

            return result.ToList();
        }
    }
}
