using Mall.CommonModel;
using Mall.CommonModel.Delegates;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System.Collections.Generic;
using System.Linq;
namespace Mall.Application
{
    public class RefundApplication:BaseApplicaion<IRefundService>
    {
        public static event RefundSuccessed OnRefundSuccessed
        {
            add
            {
                Service.OnRefundSuccessed += value;
            }
            remove
            {
                Service.OnRefundSuccessed -= value;
            }
        }


        /// <summary>
        /// 添加一个退款申请
        /// </summary>
        /// <param name="info"></param>
        public static void AddOrderRefund(OrderRefundInfo info)
        {
            Service.AddOrderRefund(info);
        }

        /// <summary>
        /// 通过订单编号获取整笔退款
        /// </summary>
        /// <param name="id">订单编号</param>
        /// <returns></returns>
        public static OrderRefundInfo GetOrderRefundByOrderId(long id)
        {
            return Service.GetOrderRefundByOrderId(id);
        }

        public static List<OrderRefundInfo> GetOrderRefundsByOrder(long order) {
            return Service.GetOrderRefundsByOrder(order);
        }

        /// <summary>
        /// 通过订单编号获取整笔退款
        /// </summary>
        /// <param name="id">订单编号</param>
        /// <returns></returns>
        public static Entities.OrderRefundInfo GetOrderRefundById(long id)
        {
            return Service.GetOrderRefundById(id);
        }
        /// <summary>
        /// 获取一条退货记录
        /// </summary>
        /// <param name="id"></param>
        public static OrderRefund GetOrderRefund(long id, long? userId = null, long? shopId = null)
        {
            var data = Service.GetOrderRefund(id, userId);
            if (data == null) return null;
            var order = OrderApplication.GetOrder(data.OrderId);
            var result = data.Map<DTO.OrderRefund>();
            result.IsShopBranchOrder = order.ShopBranchId > 0;
            result.IsVirtual = order.OrderType == OrderInfo.OrderTypes.Virtual;
            return result;
        }
        /// <summary>
        /// 获取退款/退货列表
        /// </summary>
        /// <param name="refundQuery"></param>
        /// <returns></returns>
        public static QueryPageModel<DTO.OrderRefund> GetOrderRefunds(RefundQuery refundQuery)
        {
            var data = Service.GetOrderRefunds(refundQuery);
            var orders = OrderApplication.GetOrders(data.Models.Select(p => p.OrderId));
            var resultdata = new List<OrderRefund>();
            foreach (var item in data.Models)
            {
                var tmp = item.Map<OrderRefund>();
                var order = orders.FirstOrDefault(p => p.Id == item.OrderId);
                tmp.IsShopBranchOrder = order.ShopBranchId > 0;
                resultdata.Add(tmp);
            }

            return new QueryPageModel<DTO.OrderRefund>()
            {
                Models = resultdata,
                Total = data.Total
            };
        }

        public static int GetOrderRefundsCount(RefundQuery query) {
            return Service.GetOrderRefundCount(query);
        }

        /// <summary>
        /// 获取退款记录列表(忽略分页)
        /// </summary>
        /// <param name="refundQuery"></param>
        /// <returns></returns>
        public static List<OrderRefundExportModel> GetAllFullOrderReFunds(RefundQuery refundQuery)
        {
            var data = Service.GetAllOrderRefunds(refundQuery);
            if (data == null || data.Count() <= 0)
                return new List<OrderRefundExportModel>();

            var orderResults = new List<OrderRefundExportModel>();
            #region 购置OrderRefundExportModel实体
            var orders = Application.OrderApplication.GetOrders(data.Select(p => p.OrderId));
            var orderItems = Application.OrderApplication.GetOrderItems(data.Select(p => p.OrderItemId));

            foreach (var item in data)
            {
                string strProductName = "订单所有商品";//退款时商品名称
                #region 店名称
                var order = orders.Where(p => p.Id == item.OrderId).First();
                string strShopBranchName = order.ShopName;//门店名称
                if (order != null && order.ShopBranchId > 0)
                {
                    var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                    if (shopBranchInfo != null)
                    {
                        strShopBranchName = shopBranchInfo.ShopBranchName;
                    }
                }
                #endregion

                #region 商品名称
                if (item.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    var orderItem = orderItems.FirstOrDefault(p => p.Id == item.OrderItemId);
                    string spec = ((string.IsNullOrWhiteSpace(orderItem.Color) ? "" : orderItem.Color + "，")
                                + (string.IsNullOrWhiteSpace(orderItem.Size) ? "" : orderItem.Size + "，")
                                + (string.IsNullOrWhiteSpace(orderItem.Version) ? "" : orderItem.Version + "，")).TrimEnd('，');
                    if (!string.IsNullOrWhiteSpace(spec))
                    {
                        spec = "  【" + spec + " 】";
                    }
                    strProductName = orderItem.ProductName + spec;
                }
                #endregion

                var refundModel = new OrderRefundExportModel()
                {
                    RefundId = item.Id,
                    OrderId = item.OrderId,
                    ShopName=item.ShopName,
                    ProductName = strProductName,//商品名称
                    ShopBranchName = strShopBranchName,//门店名称
                    UserName = item.Applicant,//用户名
                    ContactPerson = item.ContactPerson,
                    ContactCellPhone = item.ContactCellPhone,
                    ApplyDate = item.ApplyDate.ToString(),
                    SellerRemark = item.SellerRemark,
                    Amount = item.Amount.ToString("f2"),
                    RefundStatus = item.RefundStatus,
                    RefundPayType = item.RefundPayType.ToDescription(),//退款支付方式
                    Reason = item.Reason,
                    CertPic1 = Core.MallIO.GetImagePath(item.CertPic1),
                    CertPic2 = Core.MallIO.GetImagePath(item.CertPic2),
                    CertPic3 = Core.MallIO.GetImagePath(item.CertPic3),
                    ReasonDetail = item.ReasonDetail,
                    ManagerRemark = item.ManagerRemark,
                    RefundMode = item.RefundMode.GetHashCode(),//退款方式
                    ReturnQuantity = item.ReturnQuantity,//退货数量
                };
                orderResults.Add(refundModel);
            }
            #endregion

            return orderResults;
        }

        /// <summary>
        /// 检查是否可以退款
        /// </summary>
        /// <param name="refundId"></param>
        /// <returns></returns>
        public static bool HasMoneyToRefund(long refundId)
        {
            return Service.HasMoneyToRefund(refundId);
        }

        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static List<OrderRefundInfo> GetOrderRefundList(long orderId)
        {
            return Service.GetOrderRefundList(orderId);
        }

        /// <summary>
        /// 管理员确认退款/退货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="managerRemark"></param>
        /// <param name="managerName"></param>
        /// <param name="notifyurl">导步通知地址</param>
        public static string ConfirmRefund(long id, string managerRemark, string managerName, string notifyurl)
        {
            return Service.ConfirmRefund(id, managerRemark, managerName, notifyurl);
        }
        /// <summary>
        /// 异步通知确认退款
        /// </summary>
        /// <param name="batchno"></param>
        public static void NotifyRefund(string batchNo)
        {
            Service.NotifyRefund(batchNo);
        }

        /// <summary>
        /// 买家确定退回商品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sellerName">用户名</param>
        /// <param name="expressCompanyName">快递公司名</param>
        /// <param name="shipOrderNumber">快递号码</param>
        public static void UserConfirmRefundGood(long id, string sellerName, string expressCompanyName, string shipOrderNumber)
        {
            Service.UserConfirmRefundGood(id, sellerName, expressCompanyName, shipOrderNumber);
        }

        /// <summary>
        /// 商家处理退款退货申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="auditStatus"></param>
        /// <param name="sellerRemark"></param>
        /// <param name="sellerName"></param>
        public static void SellerDealRefund(long id, Entities.OrderRefundInfo.OrderRefundAuditStatus auditStatus, string sellerRemark, string sellerName)
        {
            Service.SellerDealRefund(id, auditStatus, sellerRemark, sellerName);
        }

        /// <summary>
        /// 商家确认收到退货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sellerName"></param>
        public static void SellerConfirmRefundGood(long id, string sellerName, string remark = "")
        {
            Service.SellerConfirmRefundGood(id, sellerName, remark);
        }

        /// <summary>
        /// 是否可以申请退款
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderItemId"></param>
        /// <param name="isAllOrderRefund">是否为整笔退 null 所有 true 整笔退 false 货品售后</param>
        /// <returns></returns>
        public static bool CanApplyRefund(long orderId, long orderItemId, bool? isAllOrderRefund = null)
        {
            return Service.CanApplyRefund(orderId, orderItemId, isAllOrderRefund);
        }
        /// <summary>
        /// 添加或修改售后原因
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        public static void UpdateAndAddRefundReason(string reason, long id)
        {
            Service.UpdateAndAddRefundReason(reason, id);
        }
        /// <summary>
        /// 获取售后原因列表
        /// </summary>
        /// <returns></returns>
        public static List<RefundReasonInfo> GetRefundReasons()
        {
            return Service.GetRefundReasons();
        }

        /// <summary>
        /// 获取售后日志
        /// </summary>
        /// <param name="refundId">售后编号</param>
        /// <returns></returns>
        public static List<OrderRefundlog> GetRefundLogs(long refundId, int currentApplyNumber = 0, bool haveCurrentApplyNumber = true)
        {
            return Service.GetRefundLogs(refundId, currentApplyNumber, haveCurrentApplyNumber).Map<List<OrderRefundlog>>();
        }

        /// <summary>
        /// 删除售后原因
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteRefundReason(long id)
        {
            Service.DeleteRefundReason(id);
        }
        /// <summary>
        /// 激活售后
        /// </summary>
        /// <param name="info"></param>
        public static void ActiveRefund(OrderRefundInfo info)
        {
            Service.ActiveRefund(info);
        }
        ///// <summary>
        ///// 自动审核退款(job)
        ///// </summary>
        //public static void AutoAuditRefund()
        //{
        //    _iRefundService.AutoAuditRefund();
        //}
        ///// <summary>
        ///// 自动关闭过期未寄货退款(job)
        ///// </summary>
        //public static void AutoCloseByDeliveryExpired()
        //{
        //    _iRefundService.AutoCloseByDeliveryExpired();
        //}
        ///// <summary>
        ///// 自动商家确认到货(job)
        ///// </summary>
        //public static void AutoShopConfirmArrival()
        //{
        //    _iRefundService.AutoShopConfirmArrival();
        //}
    }
}
