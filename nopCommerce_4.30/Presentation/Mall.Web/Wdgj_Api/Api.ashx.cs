using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.Web.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace Mall.Web.Wdgj_Api
{
    /// <summary>
    /// WdgjApiHandler 的摘要说明
    /// </summary>
    public class Api : IHttpHandler
    {
        private long CurShopId;
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            DateTime startTime = DateTime.Now;  //开始进入时间
            var appkey = context.Request["appkey"];
            var token = context.Request["token"];
            var method = context.Request["method"];
            var sign = context.Request["sign"];
            //TODO:[DZY]记录参数日志，用于调试
            /*
            string pa = "";
            foreach (var item in context.Request.Form.AllKeys)
            {
                pa += item + "=" + context.Request.Form[item] + "&";
            }
            Log.Debug("[PA]" + pa);
            Log.Debug("[PAU]" + context.Request.RawUrl);
            */

            if (string.IsNullOrWhiteSpace(token))
            {
                ResponseError(context, 40000, "错误的接入码", "GSE.SYSTEM_ERROR", "错误的接入码：token");
                return;
            }
            if (string.IsNullOrWhiteSpace(method))
            {
                ResponseError(context, 40000, "错误的参数", "GSE.SYSTEM_ERROR", "错误的参数：method");
                return;
            }
            var shopwdinfo = ShopApplication.GetshopWdgjInfoByCode(token);
            string appSecret = "";
            if (shopwdinfo == null)
            {
                ResponseError(context, 40000, "错误的接入码", "GSE.SYSTEM_ERROR", "错误的接入码：token");
                return;
            }
            appSecret = shopwdinfo.uSign;
            CurShopId = shopwdinfo.ShopId;

            if (string.IsNullOrWhiteSpace(sign) || !CheckSign(context, appSecret, sign))
            {
                ResponseError(context, 40000, "签名错误", "GSE.SYSTEM_ERROR", "签名错误.");
                return;
            }
            try
            {
                switch (method)
                {
                    case "Differ.JH.Business.GetOrder"://订单下载
                        context.Response.Write(GetOrder(context));
                        return;
                    case "Differ.JH.Business.CheckRefundStatus"://退款检测
                        context.Response.Write(CheckRefundStatus(context));
                        return;
                    case "Differ.JH.Business.Send"://订单发货
                        context.Response.Write(Send(context));
                        return;
                    case "Differ.JH.Business.DownloadProduct"://商品下载
                        context.Response.Write(DownloadProduct(context));
                        return;
                    case "Differ.JH.Business.SyncStock"://商品库存同步
                        context.Response.Write(SyncStock(context));
                        return;
                    case "Differ.JH.Business.Consume":
                        context.Response.Write(Consume(context));  //订单核销
                        return;
                    case "Differ.JH.Business.GetWebSite":
                        context.Response.Write(GetStores(context));  //获取门店列表
                        return;
                }
            }
            catch (MallApiException ex)
            {
                ResponseError(context, 40000, ex.Message, "GSE.SYSTEM_ERROR", ex.Message);
                return;
            }
            catch (MallException ex)
            {
                ResponseError(context, 40000, ex.Message, "GSE.SYSTEM_ERROR", ex.Message);
                return;
            }
            catch
            {
                ResponseError(context, 40000, "系统异常", "GSE.SYSTEM_ERROR", "系统发生异常，请与管理员联系！");
                return;
            }
        }

        #region 查询订单
        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetOrder(HttpContext context)
        {
            string bizcontent = context.Request["bizcontent"];
            JObject jo = (JObject)JsonConvert.DeserializeObject(bizcontent);
            string OrderStatus = GetJObjectValue<string>(jo, "OrderStatus");
            string PlatOrderNo = GetJObjectValue<string>(jo, "PlatOrderNo");
            string StartTime = GetJObjectValue<string>(jo, "StartTime"); ;
            string EndTime = GetJObjectValue<string>(jo, "EndTime");
            string TimeType = GetJObjectValue<string>(jo, "TimeType");  //未使用，所有都是按订单创建时间搜索
            int PageIndex = GetJObjectValue<int>(jo, "PageIndex");
            int PageSize = GetJObjectValue<int>(jo, "PageSize");
            if (PageIndex <= 0) { PageIndex = 1; }
            if (PageSize <= 0) { PageSize = 10; }
            OrderQuery query = new OrderQuery { ShopId = CurShopId };
            switch (OrderStatus)
            {
                case "JH_01":
                    query.Status = Entities.OrderInfo.OrderOperateStatus.WaitPay;
                    break;
                case "JH_02":
                    query.Status = Entities.OrderInfo.OrderOperateStatus.WaitDelivery;
                    if (query.MoreStatus == null)
                    {
                        query.MoreStatus = new List<OrderInfo.OrderOperateStatus>() { };
                    }
                    query.MoreStatus.Add(OrderInfo.OrderOperateStatus.WaitSelfPickUp);
                    break;
                case "JH_03":
                    query.Status = Entities.OrderInfo.OrderOperateStatus.WaitReceiving;
                    break;
                case "JH_04":
                    query.Status = Entities.OrderInfo.OrderOperateStatus.Finish;
                    break;
                case "JH_05":
                    query.Status = Entities.OrderInfo.OrderOperateStatus.Close;
                    break;
                case "JH_99":
                    query.Status = null;  //所有
                    break;
            }
            if (!string.IsNullOrEmpty(PlatOrderNo) && PlatOrderNo != "")
            {
                query.OrderId = PlatOrderNo;
            }
            else
            {
                //检测参数
                if (string.IsNullOrWhiteSpace(StartTime)
                    || string.IsNullOrWhiteSpace(EndTime)
                    || string.IsNullOrWhiteSpace(OrderStatus)
                    )
                {
                    throw new MallApiException("参数错误，有必填参数未给值");
                }
                query.StartDate = DateTime.Parse(StartTime);
                query.EndDate = DateTime.Parse(EndTime);
            }
            query.PageNo = PageIndex;
            query.PageSize = PageSize;
            var orders = OrderApplication.GetFullOrders(query);
            bool ishasnextpage = false;
            if (orders.Total > 0)
            {
                ishasnextpage = orders.Models.Count == PageSize;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"code\":" + "\"" + "10000" + "\"");
            sb.Append(",\"message\":" + "\"" + "SUCCESS" + "\"");
            sb.Append(",\"numtotalorder\":" + "\"" + orders.Total + "\"");
            sb.Append(",\"ishasnextpage\":" + "\"" + (ishasnextpage ? "1" : "0") + "\"");
            sb.Append(",\"orders\":");
            sb.Append("[");
            int curordernum = 0;
            foreach (var order in orders.Models)
            {
                if (curordernum > 0)
                {
                    sb.Append(",");
                }
                sb.Append("{");
                sb.Append("\"PlatOrderNo\":" + "\"" + order.OrderId + "\"");
                string tradeStatus = "";
                string tradeStatusName = "";
                string itemStatus = "";
                switch (order.OrderStatus)
                {
                    case Entities.OrderInfo.OrderOperateStatus.WaitPay:
                        tradeStatus = "JH_01";
                        tradeStatusName = "等待买家付款";
                        itemStatus = "JH_01";
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.WaitDelivery:
                        tradeStatus = "JH_02";
                        tradeStatusName = "等待卖家发货";
                        itemStatus = "JH_02";
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.WaitReceiving:
                        tradeStatus = "JH_03";
                        tradeStatusName = "等待买家确认收货";
                        itemStatus = "JH_03";
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.Close:
                        tradeStatus = "JH_05";
                        tradeStatusName = "交易关闭";
                        itemStatus = "JH_05";
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.Finish:
                        tradeStatus = "JH_04";
                        tradeStatusName = "交易完成";
                        itemStatus = "JH_04";
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp:
                        tradeStatus = "JH_02";
                        tradeStatusName = "等待卖家发货";
                        itemStatus = "JH_02";
                        break;
                    default:
                        tradeStatus = "JH_99";
                        tradeStatusName = "其他";
                        itemStatus = "JH_99";
                        break;
                }
                sb.Append(",\"tradeStatus\":" + "\"" + tradeStatus + "\"");
                sb.Append(",\"tradeStatusdescription\":" + "\"" + tradeStatusName + "\"");
                sb.Append(",\"tradetime\":" + "\"" + order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss") + "\"");
                sb.Append(",\"payorderno\":" + "\"" + "" + "\"");
                sb.Append(",\"country\":" + "\"" + "CN" + "\"");
                string[] ShippingRegion = order.RegionFullName.Split(' ');
                string province = "";
                if (ShippingRegion.Length > 0)
                {
                    province = ShippingRegion[0];
                }
                string city = "";
                if (ShippingRegion.Length > 1)
                {
                    city = ShippingRegion[1];
                }
                string area = "";
                if (ShippingRegion.Length > 2)
                {
                    area = ShippingRegion[2];
                }
                string town = "";
                if (ShippingRegion.Length > 3)
                {
                    town = ShippingRegion[3];
                }

                sb.Append(",\"province\":" + "\"" + province + "\"");
                sb.Append(",\"city\":" + "\"" + city + "\"");
                sb.Append(",\"area\":" + "\"" + area + "\"");
                sb.Append(",\"town\":" + "\"" + town + "\"");
                sb.Append(",\"address\":" + "\"" + order.Address + "\"");
                sb.Append(",\"zip\":" + "\"\"");
                sb.Append(",\"phone\":" + "\"\"");
                sb.Append(",\"mobile\":" + "\"" + order.CellPhone + "\"");
                sb.Append(",\"email\":" + "\"\"");
                sb.Append(",\"customerremark\":" + "\"" + order.OrderRemarks + "\"");
                sb.Append(",\"sellerremark\":" + "\"" + order.SellerRemark + "\"");
                sb.Append(",\"postfee\":" + "\"" + order.Freight.ToString("F2") + "\"");
                sb.Append(",\"goodsfee\":" + "\"" + order.ProductTotalAmount.ToString("F2") + "\"");
                sb.Append(",\"totalmoney\":" + "\"" + order.OrderTotalAmount.ToString("F2") + "\"");
                sb.Append(",\"favourablemoney\":" + "\"" + (order.DiscountAmount + order.FullDiscount + order.IntegralDiscount).ToString("F2") + "\"");
                sb.Append(",\"commissionvalue\":" + "\"" + "" + "\"");
                sb.Append(",\"taxamount\":" + "\"0\"");
                sb.Append(",\"tariffamount\":" + "\"" + "" + "\"");
                sb.Append(",\"addedvalueamount\":" + "\"" + order.Tax + "\"");
                sb.Append(",\"consumptiondutyamount\":" + "\"" + "" + "\"");
                sb.Append(",\"sendstyle\":" + "\"" + order.DeliveryType.ToDescription() + "\"");
                sb.Append(",\"paytime\":" + "\"" + GetDateTimeString(order.PayDate, "yyyy-MM-dd HH:mm:ss") + "\"");
                sb.Append(",\"invoicetitle\":" + "\"" + order.InvoiceTitle + "\"");
                sb.Append(",\"taxpayerident\":" + "\"" + order.InvoiceCode + "\"");
                sb.Append(",\"codservicefee\":" + "\"" + "" + "\"");
                sb.Append(",\"receivername\":" + "\"" + order.ShipTo + "\"");
                sb.Append(",\"nick\":" + "\"" + order.UserName + "\"");
                sb.Append(",\"whsecode\":" + "\"" + "" + "\"");
                sb.Append(",\"IsHwgFlag\":" + "\"" + "0" + "\"");
                sb.Append(",\"ShouldPayType\":" + "\"" + GetShouldPayType(order.PaymentType) + "\"");
                sb.Append(",\"StoreName\":" + "\"" + order.ShopBranchName + "\"");
                sb.Append(",\"StoreId\":" + "\"" + order.ShopBranchId + "\"");
                sb.Append(",\"VerifyCode\":" + "\"" + order.PickupCode + "\"");
                //订单项
                sb.Append(",\"goodinfos\":");
                sb.Append("[");
                int curitemnum = 0;
                foreach (var item in order.OrderItems)
                {
                    var sku = ProductManagerApplication.GetSKU(item.SkuId);
                    if (curitemnum > 0)
                    {
                        sb.Append(",");
                    }
                    var skuproid = item.SkuId;
                    if (!string.IsNullOrWhiteSpace(sku.Sku))
                    {
                        skuproid = sku.Sku;
                    }
                    if (skuproid.IndexOf("_0_0_0")>-1 || string.IsNullOrWhiteSpace(skuproid))
                    {
                        var _pro = ProductManagerApplication.GetProduct(item.ProductId);
                        skuproid = _pro.ProductCode;
                    }
                    sb.Append("{");
                    sb.Append("\"ProductId\":" + "\"" + item.SkuId + "\"");
                    sb.Append(",\"suborderno\":" + "\"" + "" + "\"");
                    sb.Append(",\"tradegoodsno\":" + "\"" + skuproid + "\"");
                    sb.Append(",\"tradegoodsname\":" + "\"" + item.ProductName + "\"");
                    sb.Append(",\"tradegoodsspec\":" + "\"" + item.SKU + "\"");
                    sb.Append(",\"goodscount\":" + "\"" + item.Quantity + "\"");
                    sb.Append(",\"price\":" + "\"" + item.Price.ToString("F2") + "\"");
                    sb.Append(",\"discountmoney\":" + "\"" + (item.FullDiscount + item.CouponDiscount + item.DiscountAmount).ToString("F2") + "\"");
                    sb.Append(",\"taxamount\":" + "\"0\"");
                    sb.Append(",\"addedvalueamount\":" + "\"0\"");
                    sb.Append(",\"consumptiondutyamount\":" + "\"0\"");
                    string refundStatus = "JH_07";
                    switch (item.RefundStats)
                    {
                        case 1:
                            refundStatus = "JH_01";
                            break;
                        case 2:
                            refundStatus = "JH_02";
                            break;
                        case 3:
                            refundStatus = "JH_03";
                            break;
                        case 4:
                            refundStatus = "JH_04";
                            break;
                        case 5:
                        case 6:
                            refundStatus = "JH_99";
                            break;
                        case 7:
                            refundStatus = "JH_07";
                            break;
                    }
                    sb.Append(",\"refundStatus\":" + "\"" + refundStatus + "\"");
                    sb.Append(",\"status\":" + "\"" + itemStatus + "\"");
                    sb.Append(",\"remark\":" + "\"" + "" + "\"");
                    sb.Append("}");
                    curitemnum++;
                }
                sb.Append("]");
                //优惠
                sb.Append(",\"coupondetails\":");
                sb.Append("[");
                curitemnum = 0;
                foreach (var item in order.OrderItems)
                {
                    if (item.FullDiscount > 0)
                    {
                        if (curitemnum > 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(AddCouponDetail(item.SkuId, "JH_OffFreeCoupon", "满减", item.FullDiscount.ToString("F2")));
                        curitemnum++;
                    }
                    if (item.CouponDiscount > 0)
                    {
                        if (curitemnum > 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(AddCouponDetail(item.SkuId, "JH_Coupon", "优惠券", item.CouponDiscount.ToString("F2")));
                        curitemnum++;
                    }
                    if (item.EnabledRefundIntegral > 0)
                    {
                        if (curitemnum > 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(AddCouponDetail(item.SkuId, "JH_Point", "积分抵扣", item.EnabledRefundIntegral.Value.ToString("F2")));
                        curitemnum++;
                    }
                    if (item.DiscountAmount > 0)
                    {
                        if (curitemnum > 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(AddCouponDetail(item.SkuId, "JH_PeopleCoupon", "人工优惠", item.EnabledRefundIntegral.Value.ToString("F2")));
                        curitemnum++;
                    }
                }
                sb.Append("]");

                sb.Append("}");
                curordernum++;
            }
            sb.Append("]");
            sb.Append("}");

            return sb.ToString();
        }

        private string AddCouponDetail(string sku_id, string coupontype, string type, string price)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"sku_id\":" + "\"" + sku_id + "\"");
            sb.Append(",\"coupontype\":" + "\"" + coupontype + "\"");
            sb.Append(",\"type\":" + "\"" + type + "\"");
            sb.Append(",\"price\":" + "\"" + price + "\"");
            sb.Append("}");
            return sb.ToString();
        }

        private string GetShouldPayType(OrderInfo.PaymentTypes payType)
        {
            string result = "客户预存款";
            switch (payType)
            {
                case OrderInfo.PaymentTypes.CashOnDelivery:
                    result = "货到付款";
                    break;
                case OrderInfo.PaymentTypes.Offline:
                    result = "现金收款";
                    break;
                case OrderInfo.PaymentTypes.Online:
                    result = "担保交易";
                    break;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 查询退款状态
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string CheckRefundStatus(HttpContext context)
        {
            string bizcontent = context.Request["bizcontent"];
            JObject jo = (JObject)JsonConvert.DeserializeObject(bizcontent);
            long orderid = GetJObjectValue<long>(jo, "PlatOrderNo");
            if (orderid == 0)
            {
                throw new MallApiException("错误的参数");
            }
            RefundQuery query = new RefundQuery();
            query.OrderId = orderid;
            query.PageNo = 1;
            query.PageSize = int.MaxValue;
            var data = RefundApplication.GetOrderRefunds(query);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            if (data.Models.Count() > 0)
            {
                var ordRefund = data.Models.FirstOrDefault(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);

                string refundStatus = "";
                string refundStatusName = "";
                if (ordRefund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund && ordRefund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                {
                    switch (ordRefund.SellerAuditStatus)
                    {
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitAudit:
                            refundStatus = "JH_01";
                            refundStatusName = "已申请";
                            break;
                        case OrderRefundInfo.OrderRefundAuditStatus.UnAudit:
                            refundStatus = "JH_04";
                            refundStatusName = "拒绝申请";
                            break;
                        case OrderRefundInfo.OrderRefundAuditStatus.Audited:
                            if (ordRefund.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed)
                            {
                                refundStatus = "JH_06";
                                refundStatusName = "已退款";
                            }
                            else
                            {
                                refundStatus = "JH_99";
                                refundStatusName = "其他";
                            }
                            break;
                    }
                    //订单退款
                    sb.Append("\"refundStatus\":" + "\"" + refundStatus + "\"");
                    sb.Append(",\"refundStatusdescription\":" + "\"" + refundStatusName + "\"");
                    sb.Append(",\"childrenrefundStatus\":" + "\"" + "" + "\"");
                    sb.Append(",\"code\":" + "\"" + "10000" + "\"");
                    sb.Append(",\"message\":" + "\"" + "SUCCESS" + "\"");
                    sb.Append(",\"submessage\":" + "\"" + "退款成功" + "\"");
                }
                else
                {
                    var itemRefund = data.Models.Where(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund).ToList();
                    if (itemRefund.Count > 0)
                    {
                        sb.Append("\"refundStatus\":" + "\"JH_99\"");
                        sb.Append(",\"refundStatusdescription\":" + "\"其他\"");
                        sb.Append(",\"code\":" + "\"" + "10000" + "\"");
                        sb.Append(",\"message\":" + "\"" + "SUCCESS" + "\"");
                        sb.Append(",\"submessage\":" + "\"" + "退款成功" + "\"");
                        sb.Append(",\"childrenrefundStatus\":[");
                        int curitemno = 0;
                        foreach (var item in itemRefund)
                        {
                            if (curitemno > 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append("{");
                            switch (item.SellerAuditStatus)
                            {
                                case OrderRefundInfo.OrderRefundAuditStatus.WaitAudit:
                                    refundStatus = "JH_01";
                                    refundStatusName = "已申请";
                                    break;
                                case OrderRefundInfo.OrderRefundAuditStatus.UnAudit:
                                    refundStatus = "JH_04";
                                    refundStatusName = "拒绝申请";
                                    break;
                                case OrderRefundInfo.OrderRefundAuditStatus.Audited:
                                    if (item.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed)
                                    {
                                        refundStatus = "JH_06";
                                        refundStatusName = "已退款";
                                    }
                                    else
                                    {
                                        refundStatus = "JH_99";
                                        refundStatusName = "其他";
                                    }
                                    break;
                            }
                            var orditem = OrderApplication.GetOrderItem(item.OrderItemId);
                            if (orditem != null)
                            {
                                sb.Append("\"refundno\":\"" + item.Id + "\"");
                                sb.Append("\"productname\":\"" + orditem.ProductName + "\"");
                                sb.Append("\"refundstatus\":\"" + refundStatus + "\"");
                                sb.Append("\"refundstatusdescription\":\"" + refundStatusName + "\"");
                                sb.Append("}");
                                curitemno++;
                            }
                        }
                        sb.Append("]");

                    }
                }
            }
            else
            {
                sb.Append("\"code\":" + "\"" + "10000" + "\"");
                sb.Append(",\"message\":" + "\"" + "Succes" + "\"");
                sb.Append(",\"submessage\":" + "\"" + "没有退款" + "\"");
                sb.Append(",\"refundStatus\":" + "\"JH_07\"");
                sb.Append(",\"refundStatusdescription\":" + "\"没有退款\"");
                sb.Append(",\"childrenrefundStatus\":" + "\"" + "" + "\"");
            }
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string Send(HttpContext context)
        {
            string bizcontent = context.Request["bizcontent"];
            JObject jo = (JObject)JsonConvert.DeserializeObject(bizcontent);
            string SendType = GetJObjectValue<string>(jo, "SendType");
            string LogisticName = GetJObjectValue<string>(jo, "LogisticName");
            string LogisticType = GetJObjectValue<string>(jo, "LogisticType");
            string LogisticNo = GetJObjectValue<string>(jo, "LogisticNo");
            long OrderId = GetJObjectValue<long>(jo, "PlatOrderNo");
            int? IsSplit = GetJObjectValue<int?>(jo, "IsSplit");
            string SenderName = GetJObjectValue<string>(jo, "SenderName");
            string SenderTel = GetJObjectValue<string>(jo, "SenderTel");
            string SenderAddress = GetJObjectValue<string>(jo, "SenderAddress");
            int IsHwgFlag = GetJObjectValue<int>(jo, "IsHwgFlag");
            if (OrderId <= 0)
            {
                throw new MallApiException("订单号错误");
            }

            Entities.OrderInfo order = OrderApplication.GetOrderInfo(OrderId);

            if (order == null)
            {
                throw new MallApiException("未找到此订单");
            }

            if (order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new MallApiException("当前订单状态没有付款或不是等待发货的订单，所以不能发货！");
            }


            if (string.IsNullOrEmpty(LogisticNo))
            {
                throw new MallApiException("运单号码不能为空！");
            }

            OrderApplication.SellerSendGood(OrderId, "网店管家", LogisticName, LogisticNo);

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"code\":" + "\"10000\"");
            sb.Append(",\"message\":" + "\"SUCCESS\"");
            sb.Append("}");
            return sb.ToString();
        }

        public string DownloadProduct(HttpContext context)
        {
            string bizcontent = context.Request["bizcontent"];
            JObject jo = (JObject)JsonConvert.DeserializeObject(bizcontent);
            int PageIndex = GetJObjectValue<int>(jo, "PageIndex");
            if (PageIndex < 1)
            {
                PageIndex = 1;
            }
            int PageSize = GetJObjectValue<int>(jo, "PageSize");
            if (PageSize < 1)
            {
                PageSize = 10;
            }

            ProductQuery query = new ProductQuery
            {
                PageSize = PageSize,
                PageNo = PageIndex,
                ShopId = CurShopId
            };

            long ProductId = 0;
            if (jo["ProductId"] != null)
            {
                long.TryParse("", out ProductId);
            }
            if (ProductId > 0)
            {
                query.Ids = new long[] { ProductId };
            }
            string ProductName = GetJObjectValue<string>(jo, "ProductName");
            if (!string.IsNullOrWhiteSpace(ProductName))
            {
                query.KeyWords = ProductName;
            }
            string QStatus = GetJObjectValue<string>(jo, "Status");
            switch (QStatus)
            {
                case "JH_01":
                    //已上架商品
                    query.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
                    query.AuditStatus = new ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.Audited };
                    break;
                case "JH_02":
                    //已下架商品(违规下架)
                    query.AuditStatus = new ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.InfractionSaleOff };
                    break;
                case "JH_03":
                    //已售罄商品(超过警界库存)
                    query.OverSafeStock = true;
                    break;
                case "JH_04":
                    //未通过审核
                    query.AuditStatus = new ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.AuditFailed };
                    break;
                case "JH_05":
                    //审核中商品
                    query.AuditStatus = new ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.WaitForAuditing };
                    break;
                case "JH_06":
                    //即将上架商品(仓库)
                    query.SaleStatus = ProductInfo.ProductSaleStatus.InStock;
                    break;

            }
            var _iTypeService = ObjectContainer.Current.Resolve<Mall.IServices.ITypeService>();

            var data =ProductManagerApplication.GetProducts(query);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"code\":" + "\"" + "10000" + "\"");
            sb.Append(",\"message\":" + "\"" + "SUCCESS" + "\"");
            sb.Append(",\"totalcount\":" + "\"" + data.Total + "\"");
            sb.Append(",\"goodslist\":");
            sb.Append("[");
            int curitemnum = 0; 
            var allskus = ProductManagerApplication.GetSKUByProducts(data.Models.Select(d => d.Id));
            foreach (var item in data.Models)
            {
                var skus = allskus.Where(p => p.ProductId == item.Id);
                if (curitemnum > 0)
                {
                    sb.Append(",");
                }
                sb.Append("{");
                sb.Append("\"PlatProductID\":" + "\"" + item.Id + "\"");
                sb.Append(",\"name\":" + "\"" + item.ProductName + "\"");
                sb.Append(",\"OuterID\":" + "\"" + item.Id + "\"");
                sb.Append(",\"price\":" + "\"" + item.MinSalePrice + "\"");
                sb.Append(",\"num\":" + "\"" + skus.Sum(d => d.Stock) + "\"");
                sb.Append(",\"pictureurl\":" + "\"" + MallIO.GetProductSizeImage(item.ImagePath, 1, ImageSize.Size_400.GetHashCode()) + "\"");
                sb.Append(",\"whsecode\":" + "\"" + "" + "\"");
                sb.Append(",\"attrbutes\":");
                sb.Append("[");
                #region 商品属性填充
                var prodAttrs = ProductManagerApplication.GetProductAttributes(item.Id);
                var prodAttrids = prodAttrs.Select(d => d.AttributeId).Distinct().ToList();
                
                if (prodAttrids.Count > 0)
                {
                    List<string> propslst = new List<string>();
                    List<string> propsvallst = new List<string>();
                    int curattrnum = 0;
                    foreach (var curattid in prodAttrids)
                    {
                        var attr = prodAttrs.FirstOrDefault(d => d.AttributeId == curattid);
                        propsvallst.Clear();
                        var values = _iTypeService.GetAttributeValues(curattid);
                        var attribute = _iTypeService.GetAttribute(curattid);
                        foreach (var attrV in values)
                        {
                            if (prodAttrs.Any(p => p.ValueId == attrV.Id))
                            {
                                if (curattrnum > 0)
                                {
                                    sb.Append(",");
                                }
                                sb.Append("{");
                                sb.Append("\"name\":\"" + attribute.Name + "\"");
                                sb.Append(",\"value\":\"" + attrV.Value + "\"");
                                sb.Append(",\"attributeid\":\"" + attrV.Id + "\"");
                                sb.Append(",\"iscustom\":\"1\"");
                                sb.Append("}");
                                propsvallst.Add(attrV.Value);
                                curattrnum++;
                            }
                        }
                    }
                }
                #endregion
                sb.Append("]");

                sb.Append(",\"skus\":");
                sb.Append("[");
                #region SKUS

                var typeInfo = TypeApplication.GetProductType(item.TypeId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                if (item != null)
                {
                    colorAlias = !string.IsNullOrWhiteSpace(item.ColorAlias) ? item.ColorAlias : colorAlias;
                    sizeAlias = !string.IsNullOrWhiteSpace(item.SizeAlias) ? item.SizeAlias : sizeAlias;
                    versionAlias = !string.IsNullOrWhiteSpace(item.VersionAlias) ? item.VersionAlias : versionAlias;
                }
                int curskunum = 0;
                foreach (var skuitem in skus)
                {

                    if (curskunum > 0)
                    {
                        sb.Append(",");
                    }
                    string sku_properties_all_name = colorAlias + ":" + skuitem.Color + " " + sizeAlias + ":" + skuitem.Size + versionAlias + " " + ":" + skuitem.Version;
                    string sku_properties_name = skuitem.Color + " " + skuitem.Size + " " + skuitem.Version;
                    string skuOuterID = skuitem.Sku;
                    if (string.IsNullOrWhiteSpace(skuOuterID))
                    {
                        skuOuterID = item.ProductCode;
                    }
                    sb.Append("{");
                    sb.Append("\"SkuID\":" + "\"" + skuitem.Id + "\"");
                    sb.Append(",\"skuOuterID\":" + "\"" + skuOuterID + "\"");
                    sb.Append(",\"skuprice\":" + "\"" + skuitem.SalePrice + "\"");
                    sb.Append(",\"skuQuantity\":" + "\"" + skuitem.Stock + "\"");
                    sb.Append(",\"skuname\":" + "\"" + skuitem.Sku + "\"");
                    sb.Append(",\"skuproperty\":" + "\"" + "" + "\"");
                    sb.Append(",\"skupictureurl\":" + "\"" + "" + "\"");
                    sb.Append("}");
                    curskunum++;
                }
                #endregion
                sb.Append("]");
                sb.Append("}");
                curitemnum++;

            }
            sb.Append("]");
            sb.Append("}");
            return sb.ToString();
        }
        /// <summary>
        /// 同步库存
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string SyncStock(HttpContext context)
        {
            string bizcontent = context.Request["bizcontent"];
            JObject jo = (JObject)JsonConvert.DeserializeObject(bizcontent);
            long? ProductId = GetJObjectValue<long?>(jo, "PlatProductID");
            string SkuId = GetJObjectValue<string>(jo, "SkuID");
            string OuterID = GetJObjectValue<string>(jo, "OuterID");
            int Quantity = GetJObjectValue<int>(jo, "Quantity");
            string OutSkuID = GetJObjectValue<string>(jo, "OutSkuID");
            string SyncStockType = GetJObjectValue<string>(jo, "SyncStockType");
            string ids = SkuId;
            if (string.IsNullOrWhiteSpace(SkuId))
            {
                ids = string.Format("{0}_0", ProductId);
            }

            if (!ProductId.HasValue || ProductId <= 0)
            {
                throw new MallApiException("商品ID不存在");
            }
            var stype = StockOptionType.Normal;
            if (SyncStockType == "JH_01")
                stype = StockOptionType.Normal;
            if (SyncStockType == "JH_02")
                stype = StockOptionType.Add;

            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(SkuId))
            {
                ProductManagerApplication.SetProductStock(new List<long> { ProductId.Value }, Quantity, stype);
                sb.Append("{");
                sb.Append("\"code\":" + "\"10000\"");
                sb.Append(",\"message\":" + "\"SUCCESS\"");
                sb.Append(",\"Quantity\":" + "\"" + Quantity + "\"");
                sb.Append("}");
            }
            else
            {
                var changes = new Dictionary<string, long>() { { SkuId, Quantity } };
                ProductManagerApplication.SetSkuStock(stype, changes);

                sb.Append("{");
                sb.Append("\"code\":" + "\"10000\"");
                sb.Append(",\"message\":" + "\"SUCCESS\"");
                sb.Append(",\"Quantity\":" + "\"" + Quantity + "\"");
                sb.Append("}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 订单核销
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string Consume(HttpContext context)
        {
            string bizcontent = context.Request["bizcontent"];
            JObject jo = (JObject)JsonConvert.DeserializeObject(bizcontent);
            string orderId = GetJObjectValue<string>(jo, "PlatOrderNo");
            string pickcode = GetJObjectValue<string>(jo, "VerifyCode");
            string brachId = GetJObjectValue<string>(jo, "StoreId");
            string branchName = GetJObjectValue<string>(jo, "HXVerifyWay");
            string userName = "";
            if (string.IsNullOrWhiteSpace(pickcode))
                throw new MallApiException("该核销码无效");
            var order = Application.OrderApplication.GetOrderByPickCode(pickcode);
            if (order == null)
                throw new MallApiException("该核销码无效");
            var manager = ShopBranchApplication.GetShopBranchManagerByShopBranchId(order.ShopBranchId);
            if (manager != null) userName = manager.UserName;
            if (order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.Finish && order.DeliveryType == CommonModel.DeliveryType.SelfTake)
                throw new MallApiException("该提货码于" + order.FinishDate.ToString() + "已核销");
            if (order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                throw new MallApiException("只有待自提的订单才能进行核销");
            Application.OrderApplication.ShopBranchConfirmOrder(order.Id, order.ShopBranchId, userName + "【网店管家】");
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"code\":" + "\"10000\"");
            sb.Append(",\"message\":" + "\"SUCCESS\"");
            sb.Append("}");
            return sb.ToString();
        }

        public string GetStores(HttpContext context)
        {
            string bizcontent = context.Request["bizcontent"];
            JObject jo = (JObject)JsonConvert.DeserializeObject(bizcontent);
            int pageIndex = GetJObjectValue<int>(jo, "PageIndex");
            int pageSize = GetJObjectValue<int>(jo, "PageSize");
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 30;
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageIndex;
            query.PageSize = pageSize;
            query.ShopId = CurShopId;
            var shops = ShopBranchApplication.GetShopBranchs(query);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"code\":" + "\"" + "10000" + "\"");
            sb.Append(",\"message\":" + "\"" + "SUCCESS" + "\"");
            sb.Append(",\"totalcount\":" + "\"" + shops.Total + "\"");
            sb.Append(",\"sites\":");
            sb.Append("[");
            var curIndex = 0;
            foreach (var item in shops.Models)
            {
                string[] adr = item.AddressFullName.Split(',');
                if (curIndex > 0) sb.Append(",");
                sb.Append("{");
                sb.Append("\"shopid\":" + "\"" + item.Id + "\"");
                sb.Append(",\"name\":" + "\"" + (string.IsNullOrEmpty(item.ShopBranchName) ? "" : item.ShopBranchName.Replace("\"", "")) + "\"");
                sb.Append(",\"sitetype\":\"JH_01\"");
                sb.Append(",\"phone\":" + "\"" + item.ContactPhone + "\"");
                sb.Append(",\"province\":" + "\"" + (string.IsNullOrWhiteSpace(adr[0]) ? "" : adr[0]) + "\"");
                sb.Append(",\"city\":" + "\"" + (string.IsNullOrWhiteSpace(adr[1]) ? "" : adr[1]) + "\"");
                sb.Append(",\"area\":" + "\"" + (string.IsNullOrWhiteSpace(adr[2]) ? "" : adr[2]) + "\"");
                sb.Append(",\"address\":" + "\"" + (string.IsNullOrEmpty(item.AddressDetail) ? "" : item.AddressDetail.Replace("\"", "")) + "\"");
                sb.Append("}");
                curIndex++;
            }
            sb.Append("]");
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="hasUrlDecode">是否转码</param>
        /// <returns></returns>
        protected string GetParameter(HttpContext context, string name, bool hasUrlDecode = false)
        {
            string result = "";
            if (context.Request[name] != null)
                result = context.Request[name].ToString();
            if (string.IsNullOrWhiteSpace(result))
            {
                var routedata = context.Request.RequestContext.RouteData;
                if (routedata.Values[name] != null)
                    result = routedata.Values[name].ToString();
            }
            if (hasUrlDecode && string.IsNullOrWhiteSpace(result))
            {
                result = System.Web.HttpUtility.UrlDecode(result);
            }
            return result;
        }
        /// <summary>
        /// 输出错误并中断
        /// </summary>
        /// <param name="context"></param>
        protected void ResponseError(HttpContext context, int code, string message, string subcode, string submessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"code\":" + "\"" + code + "\"");
            sb.Append(",\"message\":" + "\"" + message + "\"");
            sb.Append(",\"subcode\":" + "\"" + subcode + "\"");
            sb.Append(",\"submessage\":" + "\"" + submessage + "\"");
            sb.Append("}");
            context.Response.Write(sb.ToString());
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="uCode"></param>
        /// <param name="mType"></param>
        /// <param name="Secret"></param>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        private bool CheckSign(HttpContext context, string appSecret, string sign)
        {
            bool result = false;

            NameValueCollection Parameters = new NameValueCollection { context.Request.Form, context.Request.QueryString };
            SortedDictionary<string, string> sortparmas = new SortedDictionary<string, string>();
            foreach (var item in Parameters.AllKeys)
            {
                if (!string.IsNullOrWhiteSpace(item) && item.ToLower() != "sign")
                {
                    sortparmas.Add(item.ToLower(), Parameters[item]);
                }
            }
            var newSign = Sign(sortparmas, appSecret);
            if (sign == newSign)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 生成签名。
        /// </summary>
        /// <returns></returns>
        private string Sign(SortedDictionary<string, string> dic, string appSecret)
        {
            //构建待签名的字符串。
            var beSignText = string.Empty;
            foreach (string _key in dic.Keys)
                beSignText += _key + dic[_key];

            //转小写生成签名。
            return this.MD5((appSecret + beSignText + appSecret).ToLower());
        }

        /// <summary>
        /// 字符串生成MD5码。
        /// </summary>
        /// <param name="data">待MD5字符</param>
        /// <returns></returns>
        private string MD5(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var hashmd5 = new MD5CryptoServiceProvider();
            byte[] byteOriginal = hashmd5.ComputeHash(bytes);
            StringBuilder ciphertext = new StringBuilder(32);
            for (int i = 0; i < byteOriginal.Length; i++)
            {
                ciphertext.Append(byteOriginal[i].ToString("x").PadLeft(2, '0'));
            }

            return ciphertext.ToString();
        }
        /// <summary>
        /// 获取对应值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private T GetJObjectValue<T>(JObject data, string key)
        {
            T result = default(T);
            var jo = data[key];
            if (jo != null)
            {
                result = jo.Value<T>();
            }
            return result;
        }
        /// <summary>
        /// 获取时间文本
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private string GetDateTimeString(DateTime? dt, string format)
        {
            string result = "";
            if (dt.HasValue)
            {
                result = dt.Value.ToString(format);
            }
            return result;
        }
    }
}