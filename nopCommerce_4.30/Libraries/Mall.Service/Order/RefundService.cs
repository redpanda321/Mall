using Mall.Application;
using Mall.CommonModel;
using Mall.CommonModel.Delegates;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.Core.Plugins.Payment;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mall.Service
{
    public class RefundService : ServiceBase, IRefundService
    {
        #region 属性
        /// <summary>
        /// 退款成功
        /// </summary>
        public event RefundSuccessed OnRefundSuccessed;
        #endregion

        #region 方法
        public QueryPageModel<OrderRefundInfo> GetOrderRefunds(RefundQuery refundQuery)
        {
            var refunds = WhereBuilder(refundQuery);

            var rets = refunds.OrderByDescending(o => o.Id).ToPagedList(refundQuery.PageNo, refundQuery.PageSize);
            var ordidlst = rets.Select(r => r.OrderId).ToList();
            var orders = DbFactory.Default.Get<OrderInfo>(d => d.Id.ExIn(ordidlst)).ToList();
            var itemIds = rets.Select(p => p.OrderItemId).ToList();
            var orderitems = DbFactory.Default.Get<OrderItemInfo>(p => p.Id.ExIn(itemIds)).ToList();
            var ordser = ServiceProvider.Instance<IOrderService>.Create;
            foreach (var item in rets)
            {
                var orderitem = orderitems.FirstOrDefault(p => p.Id == item.OrderItemId);
                if (item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    var order = orders.FirstOrDefault(d => d.Id == item.OrderId);
                    if (order != null)
                    {
                        item.EnabledRefundAmount = order.ProductTotalAmount + order.Freight - order.DiscountAmount - order.FullDiscount;
                        item.IsVirtual = order.OrderType == OrderInfo.OrderTypes.Virtual;
                    }
                }
                else
                {
                    item.EnabledRefundAmount = (orderitem.EnabledRefundAmount == null ? 0 : orderitem.EnabledRefundAmount.Value);
                }
                //处理订单售后期
                item.IsOrderRefundTimeOut = ordser.IsRefundTimeOut(item.OrderId);
                TypeInfo typeInfo = DbFactory.Default.Get<TypeInfo>().InnerJoin<ProductInfo>((ti, pi) => ti.Id == pi.TypeId && pi.Id == orderitem.ProductId).FirstOrDefault();
                ProductInfo prodata = DbFactory.Default.Get<ProductInfo>().Where(pi => pi.Id == orderitem.ProductId).FirstOrDefault();
                orderitem.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                orderitem.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                orderitem.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                if (prodata != null)
                {
                    orderitem.ColorAlias = !string.IsNullOrWhiteSpace(prodata.ColorAlias) ? prodata.ColorAlias : orderitem.ColorAlias;
                    orderitem.SizeAlias = !string.IsNullOrWhiteSpace(prodata.SizeAlias) ? prodata.SizeAlias : orderitem.SizeAlias;
                    orderitem.VersionAlias = !string.IsNullOrWhiteSpace(prodata.VersionAlias) ? prodata.VersionAlias : orderitem.VersionAlias;
                }
            }
            var pageModel = new QueryPageModel<OrderRefundInfo>() { Models = rets, Total = rets.TotalRecordCount };
            return pageModel;
        }
        public int GetOrderRefundCount(RefundQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }
        private GetBuilder<OrderRefundInfo> WhereBuilder(RefundQuery query)
        {
            var db = DbFactory.Default
                .Get<OrderRefundInfo>()
                .InnerJoin<OrderItemInfo>((ori, oii) => ori.OrderItemId == oii.Id)
                .InnerJoin<OrderInfo>((ori, oi) => ori.OrderId == oi.Id);

            if (query.StartDate.HasValue)
                db.Where(item => item.ApplyDate >= query.StartDate);

            if (query.OrderId.HasValue)
            {
                var orderIdRange = GetOrderIdRange(query.OrderId.Value);
                var min = orderIdRange[0];
                if (orderIdRange.Length == 2)
                {
                    var max = orderIdRange[1];
                    db.Where(item => item.OrderId >= min && item.OrderId <= max);
                }
                else
                    db.Where(item => item.OrderId == min);
            }

            if (query.EndDate.HasValue)
            {
                var enddate = query.EndDate.Value.Date.AddDays(1);
                db.Where(item => item.ApplyDate < enddate);
            }

            if (query.ConfirmStatus.HasValue)
                db.Where(item => item.ManagerConfirmStatus == query.ConfirmStatus.Value);

            if (query.ShopId.HasValue)
                db.Where(item => query.ShopId == item.ShopId);

            if (query.UserId.HasValue)
                db.Where(item => item.UserId == query.UserId);

            if (!string.IsNullOrWhiteSpace(query.ProductName))
                db.Where<OrderItemInfo>(item => item.ProductName.Contains(query.ProductName));

            if (!string.IsNullOrWhiteSpace(query.ShopName))
                db.Where(item => item.ShopName.Contains(query.ShopName));

            if (!string.IsNullOrWhiteSpace(query.UserName))
                db.Where(item => item.Applicant.Contains(query.UserName));

            //多订单结果集查询
            if (query.MoreOrderId != null && query.MoreOrderId.Count > 0)
            {
                query.MoreOrderId = query.MoreOrderId.Distinct().ToList();
                db.Where(d => d.OrderId.ExIn(query.MoreOrderId));
            }
            if (query.ShowRefundType.HasValue)
            {
                switch (query.ShowRefundType)
                {
                    case 1:
                        db.Where(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                        break;
                    case 2:
                        db.Where(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderItemRefund || d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                        break;
                    case 3:
                        db.Where(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund);
                        break;
                    case 4:
                        db.Where(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderItemRefund);
                        break;
                }
            }
            if (query.RefundModes != null && query.RefundModes.Count > 0)
                db.Where(p => p.RefundMode.ExIn(query.RefundModes));

            if (query.AuditStatus.HasValue)
            {
                if (query.AuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
                    db.Where(item => item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit || item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving);
                else
                    db.Where(item => item.SellerAuditStatus == query.AuditStatus);
            }

            //商家审核状态
            if (query.AuditStatusList != null && query.AuditStatusList.Count > 0)
                db.Where(p => p.SellerAuditStatus.ExIn(query.AuditStatusList));

            if (query.ShopBranchId.HasValue)
            {
                if (query.ShopBranchId.Value > 0)
                {
                    var sbId = query.ShopBranchId.Value;
                    db.Where<OrderInfo>(p => p.ShopBranchId.ExIfNull(0) == sbId);
                }
                else
                {
                    db.Where<OrderInfo>(p => p.ShopBranchId.ExIfNull(0) == 0);
                }
            }
            if (query.IsOngoing)
            {
                db.Where<OrderRefundInfo>(d => d.ManagerConfirmStatus != OrderRefundInfo.OrderRefundConfirmStatus.Confirmed && d.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit);
            }
            if (query.IsFilterVirtual.HasValue && query.IsFilterVirtual.Value)
            {
                db.Where<OrderInfo>(p => p.OrderType != OrderInfo.OrderTypes.Virtual);
            }
            return db;
        }

        /// <summary>
        /// 获取退款列表(忽略分页)
        /// </summary>
        /// <param name="refundQuery"></param>
        /// <returns></returns>
        public List<OrderRefundInfo> GetAllOrderRefunds(RefundQuery refundQuery)
        {
            var refunds = WhereBuilder(refundQuery);

            return refunds.OrderByDescending(o => o.Id).ToList();
        }

        private long[] GetOrderIdRange(long orderId)
        {
            var temp = 16;
            var length = orderId.ToString().Length;
            if (length < temp)
            {
                var len = temp - length;
                orderId = orderId * (long)Math.Pow(10, len);
                var max = orderId + long.Parse(string.Join("", new int[len].Select(p => 9)));
                return new[] { orderId, max };
            }
            else if (length == temp)
                return new[] { orderId };
            return null;
        }

        #region 退款方式方法体
        private object lockobj = new object();
        /// <summary>
        /// 生成一个新的退款批次号
        /// </summary>
        /// <returns></returns>
        private string GetNewRefundBatchNo()
        {
            string result = "";
            lock (lockobj)
            {
                int rand;
                char code;
                result = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 5; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    result += code.ToString();
                }
                result = DateTime.Now.ToString("yyyyMMddfff") + result;
            }
            return result;
        }

        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }
        /// <summary>
        /// 原路退回
        /// </summary>
        /// <param name="refund"></param>
        /// <returns>异步请求的地址，如果同步请返回空</returns>
        private string RefundBackOut(OrderRefundInfo refund, string notifyurl)
        {
            string result = "";
            if (refund.RefundPayStatus != OrderRefundInfo.OrderRefundPayStatus.PaySuccess)
            {
                var Order = DbFactory.Default.Get<OrderInfo>(p => p.Id == refund.OrderId).FirstOrDefault();
                var OrderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == refund.OrderItemId).FirstOrDefault();
                var payWay = Order.PaymentTypeGateway;

                var paymentPlugins = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.PluginInfo.PluginId == payWay);
                if (paymentPlugins.Count() > 0)
                {
                    var OrderPayInfo = DbFactory.Default.Get<OrderPayInfo>().Where(e => e.PayState && e.OrderId == refund.OrderId).FirstOrDefault();
                    if (OrderPayInfo != null)
                    {
                        var orderIds = DbFactory.Default.Get<OrderPayInfo>().Where(item => item.PayId == OrderPayInfo.PayId && item.PayState == true).Select(e => e.OrderId).ToList<long>();
                        //var amount = DbFactory.Default.Get<OrderInfo>().Where(o => o.Id.ExIn(orderIds)).Sum<decimal>(e => e.OrderTotalAmount);
                        var amount = DbFactory.Default.Get<OrderInfo>().Where(o => o.Id.ExIn(orderIds)).ToList().Sum(e => e.OrderTotalAmount);
                        var firstOrderid = orderIds.FirstOrDefault();
                        var order = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == firstOrderid).FirstOrDefault();
                        string paytradeno = string.Empty;
                        if (order != null)
                        {
                            paytradeno = order.GatewayOrderId;
                        }
                        if (string.IsNullOrEmpty(paytradeno))
                        {
                            throw new MallException("未找到支付流水号！");
                        }
                        notifyurl = string.Format(notifyurl, EncodePaymentId(payWay));
                        decimal refundfee = refund.Amount;
                        if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                        {
                            if (OrderItem.EnabledRefundIntegral > 0 && OrderItem.EnabledRefundAmount > 0)
                            {
                                if (refundfee > (OrderItem.EnabledRefundAmount - OrderItem.EnabledRefundIntegral))
                                {
                                    refundfee = OrderItem.EnabledRefundAmount.Value - OrderItem.EnabledRefundIntegral.Value;
                                }
                            }
                        }
                        else
                        {
                            if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                            {
                                refundfee = order.OrderTotalAmount;
                            }
                            else
                            {
                                var totalrefundmoney = OrderItem.EnabledRefundAmount.Value - OrderItem.EnabledRefundIntegral.Value - OrderItem.RefundPrice;
                                if (totalrefundmoney > refund.Amount)
                                    refundfee = refund.Amount;
                                else
                                    refundfee = totalrefundmoney;
                            }
                        }
                        if (refundfee > 0)
                        {
                            string refund_batch_no = GetNewRefundBatchNo();
                            //退款流水号处理
                            if (!refund.RefundPostTime.HasValue)
                            {
                                refund.RefundPostTime = DateTime.Now.AddDays(-2);
                            }
                            //支付宝一天内可共用同一个流水号
                            if (refund.RefundPostTime.Value.Date == DateTime.Now.Date && !string.IsNullOrWhiteSpace(refund.RefundBatchNo))
                            {
                                refund_batch_no = refund.RefundBatchNo;
                            }
                            else
                            {
                                refund.RefundBatchNo = refund_batch_no;
                            }
                            refund.RefundPostTime = DateTime.Now;
                            PaymentPara para = new PaymentPara()
                            {
                                out_refund_no = refund_batch_no,
                                out_trade_no = OrderPayInfo.PayId.ToString(),
                                pay_trade_no = paytradeno,
                                refund_fee = refundfee,
                                total_fee = amount,
                                notify_url = notifyurl
                            };

                            var refundResult = paymentPlugins.FirstOrDefault().Biz.ProcessRefundFee(para);
                            if (refundResult.RefundResult == RefundState.Success)
                            {
                                if (refundResult.RefundMode == RefundRunMode.Sync)
                                {
                                    refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.PaySuccess;
                                }
                                if (refundResult.RefundMode == RefundRunMode.Async)
                                {
                                    result = refundResult.ResponseContentWhenFinished;
                                    refund.RefundBatchNo = refundResult.RefundNo;
                                    refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.Payed;
                                }
                                DbFactory.Default.Update(refund);
                            }
                            else
                            {
                                throw new MallException("退款插件工作未完成！");
                            }
                        }
                        else
                        {
                            refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.PaySuccess;
                        }
                    }
                    else
                    {
                        throw new MallException("退款时，未找到原支付订单信息！");
                    }
                }
                else
                {
                    throw new MallException("退款时，未找到支付方式！");
                }
            }
            return result;
        }
        /// <summary>
        /// 退到预付款
        /// </summary>
        /// <param name="refund"></param>
        private void RefundBackCapital(OrderRefundInfo refund)
        {
            IMemberCapitalService capitalServicer = Mall.ServiceProvider.Instance<IMemberCapitalService>.Create;
            if (refund.RefundPayType == OrderRefundInfo.OrderRefundPayType.BackCapital)
            {
                if (refund.RefundPayStatus != OrderRefundInfo.OrderRefundPayStatus.PaySuccess)
                {
                    decimal refundfee = refund.Amount;

                    var order = DbFactory.Default.Get<OrderInfo>(p => p.Id == refund.OrderId).FirstOrDefault();
                    var orderitem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == refund.OrderItemId).FirstOrDefault();

                    if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        if (orderitem.EnabledRefundIntegral > 0 && orderitem.EnabledRefundAmount > 0)
                        {
                            if (refundfee > (orderitem.EnabledRefundAmount - orderitem.EnabledRefundIntegral))
                            {
                                refundfee = orderitem.EnabledRefundAmount.Value - orderitem.EnabledRefundIntegral.Value;
                            }
                        }
                    }
                    else
                    {
                        if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                        {
                            refundfee = order.OrderTotalAmount;
                        }
                        else
                        {
                            var totalrefundmoney = orderitem.EnabledRefundAmount.Value - orderitem.EnabledRefundIntegral.Value - orderitem.RefundPrice;
                            if (totalrefundmoney < refund.Amount)
                                refundfee = totalrefundmoney;

                            //if (orderitem.EnabledRefundIntegral > 0 && orderitem.EnabledRefundAmount > 0)
                            //{
                            //    if (refundfee > (orderitem.EnabledRefundAmount - orderitem.EnabledRefundIntegral))
                            //    {
                            //        refundfee = orderitem.EnabledRefundAmount.Value - orderitem.EnabledRefundIntegral.Value;
                            //    }
                            //}
                        }
                    }
                    refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.PaySuccess;
                    if (refundfee > 0)
                    {
                        CapitalDetailModel capita = new CapitalDetailModel
                        {
                            UserId = refund.UserId,
                            Amount = refundfee,
                            SourceType = CapitalDetailInfo.CapitalDetailType.Refund,
                            SourceData = refund.OrderId.ToString(),
                            CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        capitalServicer.AddCapital(capita, false);
                    }
                }
            }
        }
        #endregion

        #region 退款成功处理
        /// <summary>
        /// 退款成功后的处理
        /// </summary>
        /// <param name="refund"></param>
        private bool RefundSuccessed(OrderRefundInfo refund, string managerName)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == refund.OrderId).FirstOrDefault();
            var orderItem = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.Id == refund.OrderItemId).FirstOrDefault();
            var member = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == refund.UserId).FirstOrDefault();
            if (refund.ManagerConfirmStatus != OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm)
                throw new MallException("只有未确认状态的退款/退货才能进行确认操作！");
            var ret = DbFactory.Default.InTransaction(() =>
            {
                if (refund.RefundPayStatus == OrderRefundInfo.OrderRefundPayStatus.PaySuccess)
                {
                    if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        var orditemlist = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == refund.OrderId).ToList();
                        foreach (var i in orditemlist)
                        {
                            if (order.OrderType == OrderInfo.OrderTypes.Virtual)//虚拟订单退款很特殊，是按核销码去退的
                            {
                                //一个虚拟订单可以分多次退
                                i.ReturnQuantity += refund.VerificationCodeIds.Split(',').Count();//退款核销码
                                var refundprice = refund.Amount;
                                var totalrefundmoney = i.EnabledRefundAmount.Value - i.RefundPrice;
                                if (totalrefundmoney < refund.Amount)
                                    refundprice = totalrefundmoney;
                                i.RefundPrice += refundprice;//退款金额
                            }
                            else
                            {
                                i.ReturnQuantity = i.Quantity;
                                if (i.EnabledRefundAmount == null)
                                    i.EnabledRefundAmount = 0;
                                i.RefundPrice = i.EnabledRefundAmount.Value;
                            }
                            if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)  //待发货退库存
                            {
                                ReturnStock(i, order, i.Quantity);
                            }
                            //虚拟订单退款，按核销码个数退
                            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                            {
                                ReturnStock(i, order, refund.VerificationCodeIds.Split(',').Count());
                            }

                            DbFactory.Default.Update(i);
                        }
                    }
                    else
                    {
                        orderItem.RefundPrice = refund.Amount;
                        orderItem.ReturnQuantity = refund.ShowReturnQuantity;
                        DbFactory.Default.Update(orderItem);
                    }

                    //实付(金额不含运费)
                    decimal orderRealPay = (order.OrderTotalAmount - order.Freight);
                    //可退(金额)
                    decimal orderCanRealRefund = orderRealPay;
                    //实退(金额不含积分)
                    /*
                     decimal amountPay = (orderRealPay == 0 ? 1 : orderRealPay);//计算实际退款金额使用
                    if (orderRealPay < refund.Amount)
                    {
                        amountPay = refund.Amount;
                    }
                    //decimal realRefundAmount = (refund.Amount - order.IntegralDiscount * (refund.Amount / amountPay));
                    */
                    decimal realRefundAmount = refund.Amount;
                    if (orderItem.EnabledRefundIntegral > 0 && orderItem.EnabledRefundAmount > 0)
                    {
                        if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                        {
                            var re = orderItem.EnabledRefundAmount.Value - orderItem.EnabledRefundIntegral.Value - orderItem.RefundPrice;
                            //修正负数，为负数，说明没有可退的实付金额（当只剩积分未退时，可能为负）
                            re = re > 0 ? re : 0.00M;
                            if (realRefundAmount > re)
                                realRefundAmount = re;
                        }
                        else
                        {
                            if (realRefundAmount > orderItem.EnabledRefundAmount - orderItem.EnabledRefundIntegral)
                            {
                                realRefundAmount = orderItem.EnabledRefundAmount.Value - orderItem.EnabledRefundIntegral.Value;
                            }
                        }
                    }

                    realRefundAmount = Math.Round(realRefundAmount, 2);
                    decimal refundActualPayAmount = 0;
                    if (realRefundAmount > 0)
                    {
                        //修改实收金额
                        order.ActualPayAmount -= realRefundAmount;
                        refundActualPayAmount = realRefundAmount;
                        if (order.OrderType != OrderInfo.OrderTypes.Virtual)//虚拟订单只会订单退款，这里不处理退款金额
                        {
                            order.RefundTotalAmount += refund.Amount;
                            if (order.RefundTotalAmount > orderRealPay)
                            {
                                order.RefundTotalAmount = orderRealPay;
                            }
                        }
                    }
                    //修正整笔退
                    if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        orderRealPay = order.OrderTotalAmount;
                        orderCanRealRefund = order.OrderTotalAmount;
                        if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                        {
                            realRefundAmount = order.OrderTotalAmount;
                            order.RefundTotalAmount = realRefundAmount;//普通订单整笔退，为订单实付总金额
                        }
                        else
                        {
                            order.RefundTotalAmount += realRefundAmount;//一个虚拟订单可以分多次退，故累计
                        }
                    }
                    DbFactory.Default.Update(order);

                    var integralExchange = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralChangeRule();

                    #region 扣除订单产生的积分
                    //积分获取方式己改这过售后期获得积分
                    /*
                    if (integralExchange != null && integralExchange.MoneyPerIntegral > 0)
                    {

                        if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                        {
                            //只处理有兑换规则的积分处理
                            var MoneyPerIntegral = integralExchange.MoneyPerIntegral;
                            decimal? refIntAmount = refund.OrderItemInfo.EnabledRefundAmount - refund.OrderItemInfo.EnabledRefundIntegral;
                            if (refIntAmount > refund.Amount)
                            {
                                refIntAmount = refund.Amount;
                            }
                            if (!refIntAmount.HasValue)
                            {
                                refIntAmount = 0;
                            }
                            if (refIntAmount > 0)
                            {
                                int DeductIntegral = (int)Math.Floor(refIntAmount.Value / MoneyPerIntegral);
                                var _tmp = member.MemberIntegralInfo.FirstOrDefault();
                                var _curuintg = _tmp == null ? 0 : _tmp.AvailableIntegrals;
                                if (DeductIntegral > 0 && _curuintg > 0 && order.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
                                {
                                    //扣除订单产生的积分
                                    MemberIntegralRecordInfo info = new MemberIntegralRecordInfo();
                                    info.UserName = member.UserName;
                                    info.MemberId = member.Id;
                                    info.RecordDate = DateTime.Now;
                                    info.TypeId = MemberIntegralInfo.IntegralType.Others;
                                    info.ReMark = "售后编号【" + refund.Id + "】退款应扣除积分" + DeductIntegral.ToString();
                                    DeductIntegral = DeductIntegral > _curuintg ? _curuintg : DeductIntegral;      //超出当前用户积分，直接扣除用户所有积分
                                    DeductIntegral = -DeductIntegral;
                                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegralInfo.IntegralType.Others, DeductIntegral);
                                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                                }
                            }
                        }
                    }
                    */
                    #endregion

                    #region 积分抵扣补回
                    //decimal refundallfee = refund.Amount;
                    //decimal refundfee = refundallfee;
                    //if (orderItem.EnabledRefundIntegral > 0 && orderItem.EnabledRefundAmount > 0)
                    //{
                    //    if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                    //    {
                    //        var re = orderItem.EnabledRefundAmount.Value - orderItem.EnabledRefundIntegral.Value - orderItem.RefundPrice;
                    //        if (refundfee > re)
                    //            refundfee = re;
                    //    }
                    //    else
                    //    {
                    //        if (refundfee > (orderItem.EnabledRefundAmount - orderItem.EnabledRefundIntegral))
                    //        {
                    //            refundfee = orderItem.EnabledRefundAmount.Value - orderItem.EnabledRefundIntegral.Value;
                    //        }
                    //    }
                    //}

                    // 说明：
                    // 退款的逻辑是先退实付金额，再退积分，realRefundAmount字段为已经退的实付金额
                    // realRefundAmount 为负数或为零，说明实付金额在之前的退款中已经退完，且退了部分积分
                    // 所以当次退款金额全部退积分，否则减掉要退的实付金额（实付金额在前面的逻辑已经处理）
                    decimal refundinfee = realRefundAmount > 0 ? (refund.Amount - realRefundAmount) : refund.Amount;
                    //虚拟订单退积分不能直接用积分优惠金额，要实时计算
                    if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund && order.OrderType != OrderInfo.OrderTypes.Virtual)
                    {
                        refundinfee = order.IntegralDiscount;
                    }
                    if (refundinfee > 0)
                    {
                        if (integralExchange != null && integralExchange.IntegralPerMoney > 0)
                        {
                            //只处理有兑换规则的积分处理
                            int IntegralPerMoney = integralExchange.IntegralPerMoney;
                            int BackIntegral = (int)Math.Floor(refundinfee * IntegralPerMoney);
                            var _curuintg = MemberIntegralApplication.GetAvailableIntegral(member.Id);
                            if (BackIntegral > 0)
                            {
                                //补充订单退款的积分
                                MemberIntegralRecordInfo info = new MemberIntegralRecordInfo();
                                info.UserName = member.UserName;
                                info.MemberId = member.Id;
                                info.RecordDate = DateTime.Now;
                                info.TypeId = MemberIntegralInfo.IntegralType.Others;
                                info.ReMark = "售后编号【" + refund.Id + "】退款时退还抵扣积分" + BackIntegral.ToString();
                                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegralInfo.IntegralType.Others, BackIntegral);
                                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                            }
                        }
                    }
                    #endregion

                    //数据持久
                    refund.ManagerConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.Confirmed;
                    refund.ManagerConfirmDate = DateTime.Now;

                    //销量退还(店铺、商品)
                    if (order.PayDate.HasValue)
                    {
                        // 修改店铺访问量
                        UpdateShopVisti(refund, order.PayDate.Value);

                        // 修改商品销量
                        UpdateProductVisti(refund, order.PayDate.Value);

                        //会员服务
                        var memberService = ServiceProvider.Instance<IMemberService>.Create;

                        memberService.UpdateNetAmount(refund.UserId, -refundActualPayAmount);//减少用户的净消费额

                        //下单量即消费次数，退款不做处理
                        #region 
                        /*
                        if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                        {
                            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                            {
                                if (orderItem.Quantity == orderItem.ReturnQuantity)//虚拟订单只会有一个订单项
                                {
                                    memberService.DecreaseMemberOrderNumber(refund.UserId);//如果虚拟订单全部核销码退光，则减少用户的下单量
                                }
                            }
                            else
                            {
                                memberService.DecreaseMemberOrderNumber(refund.UserId);//减少用户的下单量
                            }
                        }
                        */
                        #endregion
                        //减少限时抢购销量
                        ILimitTimeBuyService limitServicer = Mall.ServiceProvider.Instance<ILimitTimeBuyService>.Create;
                        limitServicer.ReduceSaleCount(refund);
                    }
                    DbFactory.Default.Update(refund);

                    #region 处理分销分佣
                    if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        Mall.ServiceProvider.Instance<IDistributionService>.Create.TreatedOrderDistributionBrokerage(refund.OrderId, false);
                    }
                    else
                    {
                        Mall.ServiceProvider.Instance<IDistributionService>.Create.TreatedOrderDistributionBrokerage(refund.OrderId, false, refund.OrderItemId);
                    }
                    #endregion

                    #region 全部退货后关闭订单
                    bool isCloseOrder = true;
                    var orderItems = DbFactory.Default.Get<OrderItemInfo>(p => p.OrderId == order.Id).ToList();
                    var refunds = DbFactory.Default.Get<OrderRefundInfo>(p => p.OrderId == order.Id).ToList();
                    if (orderItems.Any(p => p.Quantity > p.ReturnQuantity)
                    || refunds.Any(p => p.ManagerConfirmStatus != OrderRefundInfo.OrderRefundConfirmStatus.Confirmed))
                        isCloseOrder = false;

                    if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                    {
                        //虚拟订单状态是随核销码变化而变化
                        ServiceProvider.Instance<IOrderService>.Create.UpdateOrderVerificationCodeStatusByCodes(refund.VerificationCodeIds.Split(',').ToList(), order.Id, OrderInfo.VerificationCodeStatus.RefundComplete, null);
                    }

                    if (order.OrderType != OrderInfo.OrderTypes.Virtual && isCloseOrder)
                    {
                        order.CloseReason = "已退货，订单自动关闭";
                        order.OrderStatus = OrderInfo.OrderOperateStatus.Close;
                        DbFactory.Default.Update(order);
                        //发生退款时重新计算待付结算订单
                        RefundSettlement(refund.OrderId, refund.Id, refund.VerificationCodeIds, true);
                    }
                    else
                    {
                        //发生退款时重新计算待付结算订单
                        RefundSettlement(refund.OrderId, refund.Id, refund.VerificationCodeIds, false);
                    }
                    #endregion
                }
            });
            return ret;
        }

        private void SaveSuccessMessage(OrderRefundInfo refund, string managerName, long orderId)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            var orderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == refund.OrderItemId).FirstOrDefault();


            //日志记录            
            if (string.IsNullOrEmpty(managerName))
                managerName = "系统";
            string strOperateContent = "确认退款/退货";
            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                strOperateContent = "虚拟商品自动确认退款/退货";

            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = managerName;
            orderOperationLog.OrderId = refund.OrderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = strOperateContent;

            DbFactory.Default.Add(orderOperationLog);

            //消息通知
            var orderMessage = new MessageOrderInfo();
            orderMessage.OrderId = order.Id.ToString();
            orderMessage.ShopId = order.ShopId;
            orderMessage.ShopName = order.ShopName;
            orderMessage.RefundMoney = refund.Amount;
            orderMessage.UserName = order.UserName;
            orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            orderMessage.TotalMoney = order.OrderTotalAmount;
            orderMessage.ProductName = orderItem.ProductName;
            orderMessage.RefundTime = refund.ApplyDate;
            if (order.Platform == PlatformType.WeiXinSmallProg)
            {
                orderMessage.MsgOrderType = MessageOrderType.Applet;
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderRefund(order.UserId, orderMessage, refund.Id, refund.RefundPayType.ToDescription()));

            //发布退款成功消息
            //MessageQueue.PublishTopic(CommonConst.MESSAGEQUEUE_REFUNDSUCCESSED, refund.Id);
            try
            {
                if (OnRefundSuccessed != null)
                    OnRefundSuccessed(refund.Id);
            }
            catch
            {
                //Log.Error("OnRefundSuccessed=" + e.Message);
            }
            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, OrderRefundStep.Confirmed, refund.RefundStatus, managerName, refund.ManagerRemark);
        }


        /// <summary>
        /// 检查是否可以退款
        /// </summary>
        /// <param name="refundId"></param>
        /// <returns></returns>
        public bool HasMoneyToRefund(long refundId)
        {
            var model = DbFactory.Default.Get<OrderRefundInfo>().Where(a => a.Id == refundId).FirstOrDefault();
            var shopAccount = DbFactory.Default.Get<ShopAccountInfo>().Where(a => a.ShopId == model.ShopId).FirstOrDefault();
            var IsSettlement = DbFactory.Default.Get<AccountDetailInfo>().Where(a => a.OrderId == model.OrderId).FirstOrDefault();
            var result = true;
            if (IsSettlement != null && model.Amount > shopAccount.Balance)
            {
                return false;
            }
            return result;
        }


        private void RefundSettlement(long orderId, long refundId, string verificationCodeIds, bool isClose)
        {
            //获取该订单详情
            var orderInfo = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == orderId).FirstOrDefault();
            if (orderInfo == null)
            {
                return; //如果没有订单
            }
            //获取该订单下所有的退款
            var list = GetOrderRefundList(orderId);

            //根据订单号获取待结算的订单
            var model = DbFactory.Default.Get<PendingSettlementOrderInfo>().Where(a => a.OrderId == orderId).FirstOrDefault();
            decimal platCommissionReturn = 0;
            decimal distributorCommissionReturn = 0;
            decimal refundAmountTotal = 0; //总退款金额
            var refund = list.FirstOrDefault(a => a.Id == refundId);//单个项目退款
            if (!string.IsNullOrWhiteSpace(verificationCodeIds))
            {//有核销码
                refund = list.FirstOrDefault(e => e.VerificationCodeIds == verificationCodeIds);
            }
            var AccountNo = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + refund.Id;
            if (model != null) //如果没结算，更新待结算订单
            {
                foreach (var m in list)
                {
                    platCommissionReturn += m.ReturnPlatCommission;
                    refundAmountTotal += m.Amount;
                }
                if (isClose)
                {//退款、退货关闭订单时，更新完成时间
                    model.OrderFinshTime = DateTime.Now;
                }
                model.PlatCommissionReturn = platCommissionReturn;
                model.RefundAmount = refundAmountTotal;
                model.RefundDate = DateTime.Now;
                //平台佣金=平台佣金-退的平台佣金
                if (refund.ReturnPlatCommission > model.PlatCommission)//防止溢出，多次退款四舍五入的影响
                {
                    refund.ReturnPlatCommission = model.PlatCommission;
                    model.PlatCommission = 0;
                }
                else
                    model.PlatCommission -= refund.ReturnPlatCommission;
                if(orderInfo.RefundTotalAmount == orderInfo.TotalAmount)
                {
                    model.PlatCommission = 0;
                }
                //最新的分销佣金（此部分的处理逻辑在此之前的分销那里已经处理）
                var distributorCommission_new = ServiceProvider.Instance<IDistributionService>.Create.GetDistributionBrokerageAmount(orderId);
                //本次退还分销佣金=退之前分销佣金-最新的分销佣金
                distributorCommissionReturn = model.DistributorCommission - distributorCommission_new;
                model.DistributorCommissionReturn += distributorCommissionReturn;
                model.DistributorCommission = distributorCommission_new;
                //结算金额-本次退的金额+本次返回的平台佣金和分销佣金
                model.SettlementAmount = model.SettlementAmount - refund.Amount + refund.ReturnPlatCommission + distributorCommissionReturn;
                //归0处理
                model.SettlementAmount = model.SettlementAmount < 0 ? 0.00M : model.SettlementAmount;
                DbFactory.Default.Update(model);
            }
        }


        /// <summary>
        /// 修改店铺访问量
        /// </summary>
        /// <param name="refund"></param>
        /// <param name="payDate"></param>
        void UpdateShopVisti(OrderRefundInfo refund, DateTime payDate)
        {
            //退款不影响金额、数量
            //ShopVistiInfo shopVisti = Context.ShopVistiInfo.FindBy(
            //    item => item.ShopId == refund.ShopId && item.Date == payDate.Date).FirstOrDefault();
            //if (shopVisti != null)
            //{
            //    if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            //    {
            //        //整笔退
            //        var orditemlist = Context.OrderItemInfo.Where(d => d.OrderId == refund.OrderId).ToList();
            //        foreach (var item in orditemlist)
            //        {
            //            shopVisti.SaleCounts -= item.Quantity;
            //        }
            //    }
            //    else
            //    {
            //        if (refund.IsReturn)
            //            shopVisti.SaleCounts -= refund.OrderItemInfo.ReturnQuantity;
            //    }

            //    shopVisti.SaleAmounts = shopVisti.SaleAmounts - refund.Amount;
            //    Context.SaveChanges();
            //}

        }
        /// <summary>
        /// 修改商品访问量
        /// </summary>
        /// <param name="refund"></param>
        /// <param name="payDate"></param>
        void UpdateProductVisti(OrderRefundInfo refund, DateTime payDate)
        {
            var orderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == refund.OrderItemId).FirstOrDefault();
            var orderInfo = DbFactory.Default.Get<OrderInfo>(p => p.Id == refund.OrderId).FirstOrDefault();

            var product = new ProductInfo();
            var productVisti = new ProductVistiInfo();
            var _iFightGroupService = ServiceProvider.Instance<IFightGroupService>.Create;
            var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(refund.OrderId);
            DbFactory.Default.InTransaction(() =>
            {
                //处理成交量
                if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    //整笔退
                    var orditemlist = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == refund.OrderId).ToList();
                    foreach (var item in orditemlist)
                    {
                        product = DbFactory.Default.Get<ProductInfo>().Where(d => d.Id == item.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            if (orderInfo != null)
                            {
                                if (orderInfo.OrderType != OrderInfo.OrderTypes.Virtual)
                                {
                                    product.SaleCounts -= item.Quantity;
                                }
                                else
                                {
                                    product.SaleCounts -= refund.ReturnQuantity;
                                }

                                var searchProduct = DbFactory.Default.Get<SearchProductInfo>().Where(r => r.ProductId == item.ProductId).FirstOrDefault();
                                if (searchProduct != null)
                                {
                                    if (orderInfo.OrderType != OrderInfo.OrderTypes.Virtual)
                                    {
                                        searchProduct.SaleCount -= (int)item.Quantity;
                                    }
                                    else
                                    {
                                        searchProduct.SaleCount -= (int)refund.ReturnQuantity;
                                    }
                                }
                                if (searchProduct.SaleCount < 0)
                                {
                                    searchProduct.SaleCount = 0;
                                }
                                if (product.SaleCounts < 0)
                                {
                                    product.SaleCounts = 0;
                                }
                                DbFactory.Default.Update(product);
                                DbFactory.Default.Update(searchProduct);
                            }
                        }


                        //退拼团库存
                        if (fgord != null)
                        {
                            _iFightGroupService.UpdateActiveStock(fgord.ActiveId, item.SkuId, item.Quantity);
                        }
                        //productVisti = Context.ProductVistiInfo.FindBy(
                        //d => d.ProductId == item.ProductId && d.Date == payDate.Date).FirstOrDefault();

                        //if (null != productVisti)
                        //{
                        //    productVisti.SaleCounts -= orderItem.Quantity;
                        //    productVisti.SaleAmounts -= refund.Amount;
                        //}
                    }
                }
                else if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund)
                {
                    if (refund.IsReturn)
                    {
                        //判断是否有会员发货，没有会员发货视为弃货，不扣销量,不退库存
                        if (!string.IsNullOrEmpty(refund.ExpressCompanyName) && !string.IsNullOrEmpty(refund.ShipOrderNumber))
                        {
                            var productid = orderItem.ProductId;
                            product = DbFactory.Default.Get<ProductInfo>().Where(d => d.Id == productid).FirstOrDefault();
                            var returnQuantity = refund.ReturnQuantity;
                            if (product != null)
                            {
                                //product.SaleCounts -= refund.OrderItemInfo.ReturnQuantity;//这个时候事务还没提交，订单项表里的ReturnQuantity未更新，还是0
                                product.SaleCounts -= returnQuantity;
                                var searchProduct = DbFactory.Default.Get<SearchProductInfo>().Where(r => r.ProductId == product.Id).FirstOrDefault();
                                if (searchProduct != null)
                                    //searchProduct.SaleCount -= (int)refund.OrderItemInfo.ReturnQuantity;
                                    searchProduct.SaleCount -= (int)returnQuantity;
                                if (searchProduct.SaleCount < 0)
                                {
                                    searchProduct.SaleCount = 0;
                                }
                                if (product.SaleCounts < 0)
                                {
                                    product.SaleCounts = 0;
                                }
                                DbFactory.Default.Update(product);
                                DbFactory.Default.Update(searchProduct);
                            }

                            //productVisti = Context.ProductVistiInfo.FindBy(
                            //    item => item.ProductId == orderItem.ProductId && item.Date == payDate.Date).FirstOrDefault();

                            //if (null != productVisti)
                            //{
                            //    productVisti.SaleCounts -= orderItem.Quantity;
                            //    productVisti.SaleAmounts -= refund.Amount;
                            //}

                            //退拼团库存
                            if (fgord != null)
                            {
                                _iFightGroupService.UpdateActiveStock(fgord.ActiveId, orderItem.SkuId, returnQuantity);
                            }
                        }
                    }
                }
            });
        }

        #endregion
        /// <summary>
        /// 退款处理
        /// </summary>
        /// <param name="refundId"></param>
        /// <param name="managerRemark"></param>
        /// <param name="managerName"></param>
        /// <param name="notifyurl"></param>
        /// <returns></returns>
        public string ConfirmRefund(long refundId, string managerRemark, string managerName, string notifyurl)
        {
            string result = "";
            //退款信息与状态
            var refund = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.Id == refundId).FirstOrDefault();
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == refund.OrderId).FirstOrDefault();
            var orderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == refund.OrderItemId).FirstOrDefault();
            var member = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == refund.UserId).FirstOrDefault();
            if (refund.ManagerConfirmStatus != OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm)
                throw new MallException("只有未确认状态的退款/退货才能进行确认操作！");


            var ret = DbFactory.Default.InTransaction(() =>
            {
                switch (refund.RefundPayType)
                {
                    case OrderRefundInfo.OrderRefundPayType.BackOut:
                        result = RefundBackOut(refund, notifyurl);
                        break;
                    case OrderRefundInfo.OrderRefundPayType.BackCapital:
                        result = "";
                        RefundBackCapital(refund);
                        break;
                }
                refund.ManagerRemark = managerRemark;
                DbFactory.Default.Update(refund);

                if (refund.RefundPayStatus == OrderRefundInfo.OrderRefundPayStatus.PaySuccess)
                {
                    RefundSuccessed(refund, managerName);
                }

            }, failedAction: (ex) =>
            {
                throw ex;
            });
            if (ret)
            {
                SaveSuccessMessage(refund, managerName, order.Id);
            }
            #region 退款失败发送消息
            if (!string.IsNullOrWhiteSpace(result))
            {
                if (order != null)
                {
                    //消息通知
                    var orderMessage = new MessageOrderInfo();
                    orderMessage.UserName = order.UserName;
                    orderMessage.OrderId = order.Id.ToString();
                    orderMessage.ShopId = order.ShopId;
                    orderMessage.ShopName = order.ShopName;
                    orderMessage.RefundMoney = refund.Amount;
                    orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                    orderMessage.TotalMoney = order.OrderTotalAmount;
                    orderMessage.ProductName = orderItem.ProductName;
                    orderMessage.RefundAuditTime = DateTime.Now;
                    orderMessage.Remark = string.IsNullOrEmpty(refund.SellerRemark) ? "退款失败" : refund.SellerRemark;
                    if (order.Platform == PlatformType.WeiXinSmallProg)
                    {
                        orderMessage.MsgOrderType = MessageOrderType.Applet;
                    }
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderRefundFail(order.UserId, orderMessage, refund.RefundMode.GetHashCode(), refund.Id));
                }
            }
            #endregion
            return result;
        }
        /// <summary>
        /// 异步通知确认退款
        /// </summary>
        /// <param name="batchno"></param>
        public void NotifyRefund(string batchNo)
        {
            if (string.IsNullOrWhiteSpace(batchNo))
            {
                throw new MallException("错误的批次号");
            }

            OrderRefundInfo refund = DbFactory.Default.Get<OrderRefundInfo>().Where(d => d.RefundBatchNo == batchNo).FirstOrDefault();
            if (refund != null)
            {
                refund.RefundPayStatus = OrderRefundInfo.OrderRefundPayStatus.PaySuccess;
                var ret = RefundSuccessed(refund, "系统异步退款");
                if (ret)
                {
                    SaveSuccessMessage(refund, "系统异步退款", refund.OrderId);
                }
            }
        }
        /// <summary>
        /// 商家审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="auditStatus"></param>
        /// <param name="sellerRemark"></param>
        /// <param name="sellerName"></param>
        public void SellerDealRefund(long id, OrderRefundInfo.OrderRefundAuditStatus auditStatus, string sellerRemark, string sellerName)
        {
            OrderRefundInfo refund = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
            {
                if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitAudit
                    && refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery   //自动任务
                    && refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving  //商家收到的货有问题
                    )
                    throw new MallException("只有待审核状态的退款/退货才能进行处理，自动任务时需要状态为待买家寄货");
            }
            else
            {
                if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
                    throw new MallException("只有待审核状态的退款/退货才能进行处理");
            }
            if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                //订单退款无需发货
                if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery)
                {
                    //直接转换为商家审核通过
                    auditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
                    ServiceProvider.Instance<IOrderService>.Create.AgreeToRefundBySeller(refund.OrderId);        //关闭订单
                }
            }
            else
            {
                if (refund.IsReturn == false)
                {
                    if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery)
                    {
                        //直接转换为商家审核通过
                        auditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
                    }
                }
            }


            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery && !refund.IsReturn)
                refund.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
            else
                refund.SellerAuditStatus = auditStatus;

            refund.SellerAuditDate = DateTime.Now;
            refund.SellerRemark = sellerRemark;
            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                refund.ManagerConfirmDate = DateTime.Now;

            DbFactory.Default.Update(refund);

            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = sellerName;
            orderOperationLog.OrderId = refund.OrderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = "商家处理退款退货申请";

            DbFactory.Default.Add(orderOperationLog);

            var stepMap = new Dictionary<OrderRefundInfo.OrderRefundAuditStatus, OrderRefundStep>();
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.Audited, OrderRefundStep.UnConfirm);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.UnAudit, OrderRefundStep.UnAudit);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitAudit, OrderRefundStep.WaitAudit);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery, OrderRefundStep.WaitDelivery);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving, OrderRefundStep.WaitReceiving);

            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, stepMap[auditStatus], refund.RefundStatus, sellerName, refund.SellerRemark);

            #region 发送售后发货消息
            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery)
            {
                if (refund != null)
                {
                    var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == refund.OrderId).FirstOrDefault();
                    var orderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == refund.OrderItemId).FirstOrDefault();
                    if (order != null)
                    {
                        //消息通知
                        var orderMessage = new MessageOrderInfo();
                        orderMessage.UserName = order.UserName;
                        orderMessage.OrderId = order.Id.ToString();
                        orderMessage.ShopId = order.ShopId;
                        orderMessage.ShopName = order.ShopName;
                        orderMessage.RefundMoney = refund.Amount;
                        orderMessage.RefundQuantity = refund.ReturnQuantity;
                        orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                        orderMessage.TotalMoney = order.OrderTotalAmount;
                        orderMessage.ProductName = orderItem.ProductName;
                        orderMessage.RefundAuditTime = DateTime.Now;
                        orderMessage.Remark = string.IsNullOrWhiteSpace(sellerRemark) ? "请及时登录系统确认寄货并填写快递信息" : sellerRemark;
                        if (order.Platform == PlatformType.WeiXinSmallProg)
                        {
                            orderMessage.MsgOrderType = MessageOrderType.Applet;
                        }
                        Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnRefundDeliver(order.UserId, orderMessage, refund.Id));
                    }
                }
            }
            #endregion
            #region 拒绝退款后发送消息
            if (auditStatus == OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
            {
                if (refund != null)
                {
                    Core.Log.Info("[模板消息]ConfirmRefund----");
                    var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == refund.OrderId).FirstOrDefault();
                    var orderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == refund.OrderItemId).FirstOrDefault();
                    if (order != null)
                    {
                        //消息通知
                        var orderMessage = new MessageOrderInfo();
                        orderMessage.UserName = order.UserName;
                        orderMessage.OrderId = order.Id.ToString();
                        orderMessage.ShopId = order.ShopId;
                        orderMessage.ShopName = order.ShopName;
                        orderMessage.RefundMoney = refund.Amount;
                        orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                        orderMessage.TotalMoney = order.OrderTotalAmount;
                        orderMessage.ProductName = orderItem.ProductName;
                        orderMessage.RefundAuditTime = DateTime.Now;
                        orderMessage.Remark = string.IsNullOrWhiteSpace(refund.SellerRemark) ? "商家拒绝退款" : refund.SellerRemark;
                        if (order.Platform == PlatformType.WeiXinSmallProg)
                        {
                            orderMessage.MsgOrderType = MessageOrderType.Applet;
                        }
                        Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderRefundFail(order.UserId, orderMessage, refund.RefundMode.GetHashCode(), refund.Id));
                    }
                }
            }
            #endregion
        }
        /// <summary>
        /// 商家确认到货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sellerName"></param>
        public void SellerConfirmRefundGood(long id, string sellerName, string remark = "")
        {
            OrderRefundInfo refund = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving)
                throw new MallException("只有待收货状态的退货才能进行确认收货操作");
            refund.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
            refund.SellerConfirmArrivalDate = DateTime.Now;
            refund.ManagerConfirmDate = DateTime.Now;
            if (!string.IsNullOrEmpty(remark))
                refund.ManagerRemark = remark;
            DbFactory.Default.Update(refund);

            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = sellerName;
            orderOperationLog.OrderId = refund.OrderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = "商家确认收到退货";
            DbFactory.Default.Add(orderOperationLog);

            if (refund.OrderItemId > 0 && refund.ReturnQuantity > 0)
            {
                var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == refund.OrderId).FirstOrDefault();
                var orderItem = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.Id == refund.OrderItemId).FirstOrDefault();
                if (order != null && orderItem != null)
                {
                    ReturnStock(orderItem, order, refund.ReturnQuantity);
                    // 限购还原活动库存
                    if (order.OrderType == OrderInfo.OrderTypes.LimitBuy)
                    {
                        var flashSaleDetailInfo = DbFactory.Default.Get<FlashSaleDetailInfo>().Where(a => a.SkuId == orderItem.SkuId && a.FlashSaleId == orderItem.FlashSaleId).FirstOrDefault();
                        if (flashSaleDetailInfo != null)
                        {
                            flashSaleDetailInfo.TotalCount += (int)refund.ReturnQuantity;
                            DbFactory.Default.Update(flashSaleDetailInfo);
                        }
                    }
                }
            }

            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, OrderRefundStep.UnConfirm, refund.RefundStatus, sellerName, refund.ManagerRemark);

            //ServiceProvider.Instance<IOrderService>.Create.UpdateMemberOrderInfo(refund.UserId,-refund.Amount,-refund.ReturnQuantity); 不维护此冗余金额
        }
        /// <summary>
        /// 用户发货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sellerName"></param>
        /// <param name="expressCompanyName"></param>
        /// <param name="shipOrderNumber"></param>
        public void UserConfirmRefundGood(long id, string sellerName, string expressCompanyName, string shipOrderNumber)
        {
            OrderRefundInfo refund = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery)
                throw new MallException("只有待等待发货状态的能进行发货操作");
            refund.ShipOrderNumber = shipOrderNumber;
            refund.ExpressCompanyName = expressCompanyName;
            refund.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving;
            refund.BuyerDeliverDate = DateTime.Now;
            DbFactory.Default.Update(refund);

            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = sellerName;
            orderOperationLog.OrderId = refund.OrderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = "买家确认发回商品";
            DbFactory.Default.Add(orderOperationLog);
            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, OrderRefundStep.WaitReceiving, refund.RefundStatus, sellerName, refund.ExpressCompanyName + "：" + refund.ShipOrderNumber);
            //发送售后消息
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(refund.OrderId, refund.UserId);
            SendRefundAppMessage(refund, order);


        }

        public List<OrderRefundInfo> GetAllOrderRefunds()
        {
            return DbFactory.Default.Get<OrderRefundInfo>().ToList();
        }


        /// <summary>
        /// 根据订单ID获取退款成功的列表
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        public List<OrderRefundInfo> GetOrderRefundList(long orderId)
        {
            var list = DbFactory.Default.Get<OrderRefundInfo>().Where(a => a.OrderId == orderId && a.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed).OrderByDescending(a => a.Id).ToList();
            return list;
        }
        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="info"></param>
        public void AddOrderRefund(OrderRefundInfo info)
        {
            var ordser = ServiceProvider.Instance<IOrderService>.Create;
            var _iFightGroupService = ServiceProvider.Instance<IFightGroupService>.Create;
            var order = ordser.GetOrder(info.OrderId, info.UserId);
            if (order == null)
                throw new Mall.Core.MallException("该订单已删除或不属于该用户");

            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
            {
                var orderVerificationCodes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id });
                long num = orderVerificationCodes.Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).Count();
                if (num == 0)
                {
                    throw new Mall.Core.MallException("该商品没有可退的核销码");
                }

                var orderitem = OrderApplication.GetOrderItem(info.OrderItemId);
                if (orderitem == null)
                    throw new Mall.Core.MallException("该订单条目已删除或不属于该用户");

                if (info.Amount > (orderitem.EnabledRefundAmount - orderitem.RefundPrice))
                    throw new Mall.Core.MallException("退款金额不能超过订单的可退金额");

                if (info.ReturnQuantity > (orderitem.Quantity - orderitem.ReturnQuantity))
                    throw new Mall.Core.MallException("退货数量不可以超出可退数量");

                //虚拟订单退款不需要平台和商家操作，直接自动审核并退款；如果退款出现异常，可在平台后台手动操作退款
                info.ShopId = order.ShopId;
                info.ShopName = order.ShopName;
                info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                info.IsReturn = false;
                info.SellerAuditDate = DateTime.Now;
                info.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited;
                info.ManagerConfirmDate = DateTime.Now;
                info.ManagerConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm;
                info.OrderItemId = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == info.OrderId).FirstOrDefault().Id;
                info.ApplyNumber = 1;

                #region 处理佣金
                SetCommission(info, order.OrderType);
                #endregion

                DbFactory.Default.Add(info);
                ordser.UpdateOrderVerificationCodeStatusByCodes(info.VerificationCodeIds.Split(',').ToList(), order.Id, OrderInfo.VerificationCodeStatus.Refund, null);

                #region 处理消息和日志
                SendMessage(info, order);
                #endregion

            }
            else
            {
                if ((int)order.OrderStatus < 2)
                    throw new Mall.Core.MallException("错误的售后申请,订单状态有误");
                info.ShopId = order.ShopId;
                info.ShopName = order.ShopName;

                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    info.ReturnQuantity = 0;
                }
                //售后时间限制
                if (ordser.IsRefundTimeOut(info.OrderId))
                {
                    throw new Mall.Core.MallException("订单已超过售后期");
                }
                if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(order.Id);
                    if (!fgord.CanRefund)
                    {
                        throw new Mall.Core.MallException("拼团订单处于不可售后状态");
                    }
                }
                if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.Finish)
                {
                    throw new Mall.Core.MallException("货到付款订单未完成前不可售后");
                }
                var orderitem = OrderApplication.GetOrderItem(info.OrderItemId);
                if (orderitem == null && info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                    throw new Mall.Core.MallException("该订单条目已删除或不属于该用户");
                if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                        throw new Mall.Core.MallException("错误的订单退款申请,订单状态有误");
                    info.IsReturn = false;
                    info.ReturnQuantity = 0;
                    if (info.Amount > order.OrderEnabledRefundAmount)
                        throw new Mall.Core.MallException("退款金额不能超过订单的实际支付金额");
                }
                else
                {
                    if (info.Amount > (orderitem.EnabledRefundAmount - orderitem.RefundPrice))
                        throw new Mall.Core.MallException("退款金额不能超过订单的可退金额");
                    if (info.ReturnQuantity > (orderitem.Quantity - orderitem.ReturnQuantity))
                        throw new Mall.Core.MallException("退货数量不可以超出可退数量");
                }
                if (info.ReturnQuantity < 0)
                    throw new Mall.Core.MallException("错误的退货数量");
                bool isOrderRefund = false;    //是否整笔订单退款
                if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    isOrderRefund = true;
                }

                var isCanApply = CanApplyRefund(info.OrderId, info.OrderItemId, isOrderRefund);

                if (!isCanApply)
                    throw new Mall.Core.MallException("您已申请过售后，不可重复申请");
                if (!isOrderRefund)
                {
                    if (info.ReturnQuantity > 0)
                    {
                        info.RefundMode = OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund;
                    }
                    else
                    {
                        info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderItemRefund;
                    }
                }
                info.SellerAuditDate = DateTime.Now;
                info.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitAudit;
                info.ManagerConfirmDate = DateTime.Now;
                info.ManagerConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm;
                if (isOrderRefund == true)
                {
                    info.OrderItemId = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == info.OrderId).FirstOrDefault().Id;
                }

                var orditemlist = new List<OrderItemInfo>();
                //订单退款、退货都要计算退款佣金
                var model = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.Id == info.OrderItemId).FirstOrDefault();
                //计算佣金
                SetCommission(info, order.OrderType);

                if (!isOrderRefund)
                {
                    if (info.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund)
                    {
                        if (info.ReturnQuantity <= 0 || info.ReturnQuantity > (model.Quantity - model.ReturnQuantity))
                            info.ReturnQuantity = model.Quantity - model.ReturnQuantity;
                    }
                    else
                        info.ReturnQuantity = 0;
                }
                else
                {
                    info.ReturnQuantity = 0;
                }

                info.ApplyNumber = 1;

                DbFactory.Default.Add(info);

                var user = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == info.UserId).FirstOrDefault();
                var reason = info.Reason;
                if (!string.IsNullOrEmpty(info.ReasonDetail))
                    reason += ":" + info.ReasonDetail;
                //退款日志
                AddRefundLog(info.Id, info.ApplyNumber, OrderRefundStep.WaitAudit, info.RefundStatus, user.UserName, reason);


                //新增小程序推送Form数据
                if (!string.IsNullOrEmpty(info.formId))
                {
                    WXAppletFormDataInfo wxInfo = new WXAppletFormDataInfo();
                    wxInfo.EventId = Convert.ToInt64(MessageTypeEnum.OrderRefundSuccess);
                    wxInfo.EventTime = DateTime.Now;
                    wxInfo.EventValue = info.OrderId.ToString();
                    wxInfo.ExpireTime = DateTime.Now.AddDays(7);
                    wxInfo.FormId = info.formId;
                    ServiceProvider.Instance<IWXMsgTemplateService>.Create.AddWXAppletFromData(wxInfo);
                }

                //发送售后消息
                SendRefundAppMessage(info, order);

                var orderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == info.OrderItemId).FirstOrDefault();
                //新增微信短信邮件消息推送
                var orderMessage = new MessageOrderInfo();
                orderMessage.UserName = order.UserName;
                orderMessage.OrderId = order.Id.ToString();
                orderMessage.ShopId = order.ShopId;
                orderMessage.ShopName = order.ShopName;
                orderMessage.RefundMoney = info.Amount;
                orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                orderMessage.TotalMoney = order.OrderTotalAmount;
                orderMessage.ProductName = orderItem.ProductName;
                orderMessage.RefundAuditTime = DateTime.Now;
                if (order.Platform == PlatformType.WeiXinSmallProg)
                {
                    orderMessage.MsgOrderType = MessageOrderType.Applet;
                }
                Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnRefundApply(order.UserId, orderMessage, info.RefundMode.GetHashCode(), info.Id));

            }
        }

        private static void SetCommission(OrderRefundInfo info, OrderInfo.OrderTypes ordertype)
        {
            var returnPlatCommission = 0.00M;
            if (ordertype != OrderInfo.OrderTypes.Virtual)
            {//非虚拟 订单
                if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    var itemlist = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == info.OrderId).ToList();
                    foreach (var c in itemlist)
                    {
                        var refundPrice = (c.RealTotalPrice - c.CouponDiscount - c.FullDiscount);
                        returnPlatCommission += calcReturnCommission(refundPrice, c.CommisRate);
                    }
                }
                else
                {
                    var model = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.Id == info.OrderItemId).FirstOrDefault();
                    decimal itemRealTotalMoney = model.RealTotalPrice - model.CouponDiscount - model.FullDiscount;  //实付金额
                    if ((model.Quantity - model.ReturnQuantity) < info.ReturnQuantity || (itemRealTotalMoney - model.RefundPrice) < info.Amount)
                    {
                        throw new MallException("退货和退款数量不能超过订单的实际数量和金额！");
                    }
                    if (model.CommisRate > 0)
                    {
                        //计算退还佣金
                        decimal unitPrice = Math.Round((itemRealTotalMoney / model.Quantity), 2);
                        int rnum = unitPrice > 0 ? (int)Math.Ceiling(info.Amount / unitPrice) : 0;
                        decimal refundPrice = (unitPrice * rnum);
                        if (refundPrice > itemRealTotalMoney)
                        {
                            refundPrice = itemRealTotalMoney;
                        }
                        returnPlatCommission = calcReturnCommission(refundPrice, model.CommisRate); //((int)(refundPrice * model.CommisRate * 100)) / (decimal)100;

                    }
                }
            }
            else
            {//虚拟订单退还佣金计算
                var model = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.Id == info.OrderItemId).FirstOrDefault();
                returnPlatCommission = calcReturnCommission(info.Amount, model.CommisRate);
            }
            // 加上对退款表的维护
            info.ReturnPlatCommission = returnPlatCommission;
        }

        private void SendMessage(OrderRefundInfo info, OrderInfo order)
        {
            var user = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == info.UserId).FirstOrDefault();
            var reason = info.Reason;
            if (!string.IsNullOrEmpty(info.ReasonDetail))
                reason += ":" + info.ReasonDetail;
            //退款日志
            AddRefundLog(info.Id, info.ApplyNumber, OrderRefundStep.WaitAudit, info.RefundStatus, user.UserName, reason);


            //新增小程序推送Form数据
            if (!string.IsNullOrEmpty(info.formId))
            {
                WXAppletFormDataInfo wxInfo = new WXAppletFormDataInfo();
                wxInfo.EventId = Convert.ToInt64(MessageTypeEnum.OrderRefundSuccess);
                wxInfo.EventTime = DateTime.Now;
                wxInfo.EventValue = info.OrderId.ToString();
                wxInfo.ExpireTime = DateTime.Now.AddDays(7);
                wxInfo.FormId = info.formId;
                ServiceProvider.Instance<IWXMsgTemplateService>.Create.AddWXAppletFromData(wxInfo);
            }

            if (order.OrderType != OrderInfo.OrderTypes.Virtual)
            {
                //发送售后消息
                SendRefundAppMessage(info, order);
            }

            var orderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.Id == info.OrderItemId).FirstOrDefault();
            //新增微信短信邮件消息推送
            var orderMessage = new MessageOrderInfo();
            orderMessage.UserName = order.UserName;
            orderMessage.OrderId = order.Id.ToString();
            orderMessage.ShopId = order.ShopId;
            orderMessage.ShopName = order.ShopName;
            orderMessage.RefundMoney = info.Amount;
            orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            orderMessage.TotalMoney = order.OrderTotalAmount;
            orderMessage.ProductName = orderItem.ProductName;
            orderMessage.RefundAuditTime = DateTime.Now;
            if (order.Platform == PlatformType.WeiXinSmallProg)
            {
                orderMessage.MsgOrderType = MessageOrderType.Applet;
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnRefundApply(order.UserId, orderMessage, info.RefundMode.GetHashCode(), info.Id));
        }
        /// <summary>
        /// 计算退款佣金
        /// </summary>
        /// <param name="refundAmount"></param>
        /// <param name="commRate"></param>
        /// <returns></returns>
        static decimal calcReturnCommission(decimal refundAmount, decimal commRate)
        {
            return Math.Round(refundAmount * commRate, 2, MidpointRounding.AwayFromZero);
        }
        /// <summary>
        /// 激活售后
        /// </summary>
        /// <param name="info"></param>
        public void ActiveRefund(OrderRefundInfo info)
        {
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(info.OrderId, info.UserId);
            var refund = DbFactory.Default.Get<OrderRefundInfo>().Where(d => d.Id == info.Id).FirstOrDefault();
            if (refund == null)
            {
                throw new MallException("错误的售后记录");
            }
            if (refund.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit && refund.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm)
            {
                throw new MallException("您已提交过申请，请不要频繁操作");
            }
            if (refund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
            {
                throw new MallException("售后记录状态有误，不可激活");
            }

            //info数据值转换给refund
            refund.Applicant = info.Applicant;
            refund.ContactPerson = info.ContactPerson;
            refund.ContactCellPhone = info.ContactCellPhone;
            refund.RefundAccount = info.RefundAccount;
            refund.ApplyDate = info.ApplyDate;
            refund.Amount = info.Amount;
            refund.Reason = info.Reason;
            refund.SellerAuditStatus = info.SellerAuditStatus;
            refund.SellerAuditDate = info.SellerAuditDate;
            //refund.SellerRemark = info.SellerRemark;
            refund.ManagerConfirmStatus = info.ManagerConfirmStatus;
            refund.ManagerConfirmDate = info.ManagerConfirmDate;
            //refund.ManagerRemark = info.ManagerRemark;
            refund.IsReturn = info.IsReturn;
            refund.ExpressCompanyName = info.ExpressCompanyName;
            refund.ShipOrderNumber = info.ShipOrderNumber;
            refund.Payee = info.Payee;
            refund.PayeeAccount = info.PayeeAccount;
            refund.RefundPayStatus = info.RefundPayStatus;
            refund.RefundPayType = info.RefundPayType;
            refund.BuyerDeliverDate = info.BuyerDeliverDate;
            refund.SellerConfirmArrivalDate = info.SellerConfirmArrivalDate;
            refund.RefundBatchNo = info.RefundBatchNo;
            refund.RefundPostTime = info.RefundPostTime;
            refund.ReturnQuantity = info.ReturnQuantity;
            if (!string.IsNullOrEmpty(info.ReasonDetail))
                refund.ReasonDetail = info.ReasonDetail;
            refund.CertPic1 = info.CertPic1;
            refund.CertPic2 = info.CertPic2;
            refund.CertPic3 = info.CertPic3;
            if (refund.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                refund.RefundMode = info.RefundMode;
            }

            bool isOrderRefund = false;
            if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                isOrderRefund = true;
            }

            if (!isOrderRefund)
            {
                if (refund.ReturnQuantity > 0)
                {
                    refund.RefundMode = OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund;
                }
                else
                {
                    refund.RefundMode = OrderRefundInfo.OrderRefundMode.OrderItemRefund;
                }
            }
            refund.SellerAuditDate = DateTime.Now;
            refund.SellerAuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitAudit;
            refund.ManagerConfirmDate = DateTime.Now;
            refund.ManagerConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm;
            if (isOrderRefund == true)
            {
                refund.OrderItemId = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == refund.OrderId).FirstOrDefault().Id;
            }


            List<OrderItemInfo> orditemlist = new List<OrderItemInfo>();
            if (!isOrderRefund)
            {
                var model = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.Id == refund.OrderItemId).FirstOrDefault();
                decimal itemRealTotalMoney = model.RealTotalPrice - model.FullDiscount - model.CouponDiscount;   //实付金额
                if ((model.Quantity - model.ReturnQuantity) < refund.ReturnQuantity || (itemRealTotalMoney - model.RefundPrice) < refund.Amount)
                {
                    throw new MallException("退货和退款数量不能超过订单的实际数量和金额！");
                }

                if (model.CommisRate > 0)
                {
                    //计算退还佣金
                    decimal unitPrice = Math.Round((itemRealTotalMoney / model.Quantity), 2);
                    int rnum = (int)Math.Ceiling(refund.Amount / unitPrice);
                    decimal refundPrice = (unitPrice * rnum);
                    if (refundPrice > itemRealTotalMoney)
                    {
                        refundPrice = itemRealTotalMoney;
                    }
                    var returnPlatCommission = calcReturnCommission(refundPrice, model.CommisRate);//((int)(refundPrice * model.CommisRate * 100)) / (decimal)100;
                    // 加上对退款表的维护
                    refund.ReturnPlatCommission = returnPlatCommission;
                }

                if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund)
                {
                    if (refund.ReturnQuantity <= 0 || refund.ReturnQuantity > (model.Quantity - model.ReturnQuantity))
                        refund.ReturnQuantity = model.Quantity - model.ReturnQuantity;
                }
                else
                    refund.ReturnQuantity = 0;
            }
            else
            {
                refund.ReturnQuantity = 0;
            }

            if (refund.ApplyNumber == null)
            {
                refund.ApplyNumber = 1;
            }
            refund.ApplyNumber += 1;

            DbFactory.Default.Update(refund);

            var user = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == refund.UserId).FirstOrDefault();

            var reason = info.Reason;
            if (!string.IsNullOrEmpty(info.ReasonDetail))
                reason += ":" + info.ReasonDetail;
            //退款日志
            AddRefundLog(refund.Id, refund.ApplyNumber, OrderRefundStep.WaitAudit, refund.RefundStatus, user.UserName, reason);


            //新增小程序推送Form数据
            if (!string.IsNullOrEmpty(info.formId))
            {
                WXAppletFormDataInfo wxInfo = new WXAppletFormDataInfo();
                wxInfo.EventId = Convert.ToInt64(MessageTypeEnum.OrderRefundSuccess);
                wxInfo.EventTime = DateTime.Now;
                wxInfo.EventValue = info.OrderId.ToString();
                wxInfo.ExpireTime = DateTime.Now.AddDays(7);
                wxInfo.FormId = info.formId;
                ServiceProvider.Instance<IWXMsgTemplateService>.Create.AddWXAppletFromData(wxInfo);
            }

            //发送售后消息
            SendRefundAppMessage(info, order);
        }



        /// <summary>
        /// 通过订单编号获取整笔退款
        /// </summary>
        /// <param name="id">订单编号</param>
        /// <returns></returns>
        public OrderRefundInfo GetOrderRefundByOrderId(long id)
        {
            return DbFactory.Default.Get<OrderRefundInfo>().Where(a => a.OrderId == id && a.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund).FirstOrDefault();
        }


        public List<OrderRefundInfo> GetOrderRefundsByOrder(IEnumerable<long> orders)
        {
            return DbFactory.Default.Get<OrderRefundInfo>(p => p.OrderId.ExIn(orders)).ToList();
        }
        public List<OrderRefundInfo> GetOrderRefundsByOrder(long order)
        {
            return DbFactory.Default.Get<OrderRefundInfo>(p => p.OrderId == order).ToList();
        }

        public OrderRefundInfo GetOrderRefund(long id, long? userId = null, long? shopId = null)
        {
            var model = DbFactory.Default.Get<OrderRefundInfo>().Where(a => a.Id == id).FirstOrDefault();

            if (model == null || userId.HasValue && userId.Value != model.UserId || shopId.HasValue && shopId.Value != model.ShopId)
                return null;
            return model;
        }

        public OrderRefundInfo GetOrderRefundById(long id)
        {
            return DbFactory.Default.Get<OrderRefundInfo>().Where(a => a.OrderId == id).FirstOrDefault();
        }
        /// <summary>
        /// 是否可以申请退款
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderItemId"></param>
        /// <param name="isAllOrderRefund">是否为整笔退 null 所有 true 整笔退 false 货品售后</param>
        /// <returns></returns>
        public bool CanApplyRefund(long orderId, long orderItemId, bool? isAllOrderRefund = null)
        {
            bool result = false;
            var sql = DbFactory.Default.Get<OrderRefundInfo>().Where(d => d.OrderId == orderId && d.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit);
            if (DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId
            && d.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery && d.OrderStatus != OrderInfo.OrderOperateStatus.Finish).Exist())
            {
                //货到付款订单在未完成订单前不可售后
                return false;
            }
            if (isAllOrderRefund == true)
            {
                sql.Where(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
            }
            else
            {
                sql.Where(d => d.OrderItemId == orderItemId);
                if (isAllOrderRefund == false)
                {
                    sql.Where(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                }
            }
            result = (sql.Count() < 1);
            return result;
        }

        /// <summary>
        /// 添加或修改售后原因
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        public void UpdateAndAddRefundReason(string reason, long id)
        {
            if (DbFactory.Default.Get<RefundReasonInfo>().Where(d => d.Id != id && d.AfterSalesText == reason).Exist())
            {
                throw new MallException("售后原因重复");
            }
            RefundReasonInfo data = DbFactory.Default.Get<RefundReasonInfo>().Where(d => d.Id == id).FirstOrDefault();
            if (id == 0)
            {
                data = new RefundReasonInfo();
            }
            if (data == null)
            {
                throw new MallException("售后原因为空");
            }
            data.AfterSalesText = reason;
            data.Sequence = 100;   //默认排序100

            DbFactory.Default.Save(data);
        }
        /// <summary>
        /// 获取售后原因列表
        /// </summary>
        /// <returns></returns>
        public List<RefundReasonInfo> GetRefundReasons()
        {
            return DbFactory.Default.Get<RefundReasonInfo>().ToList();
        }
        /// <summary>
        /// 删除售后原因
        /// </summary>
        /// <param name="id"></param>
        public void DeleteRefundReason(long id)
        {
            //var data = Context.RefundReasonInfo.FirstOrDefault(d => d.Id == id);
            //if (data != null)
            //{
            //    Context.RefundReasonInfo.Remove(data);
            //    Context.SaveChanges();
            //}
            DbFactory.Default.Del<RefundReasonInfo>().Where(n => n.Id == id).Succeed();
        }
        /// <summary>
        /// 获取售后日志
        /// </summary>
        /// <param name="refundId"></param>
        /// <returns></returns>
        public List<OrderRefundLogInfo> GetRefundLogs(long refundId, int currentApplyNumber = 0, bool haveCurrentApplyNumber = true)
        {
            var sql = DbFactory.Default.Get<OrderRefundLogInfo>().Where(d => d.RefundId == refundId);
            if (currentApplyNumber > 0)
            {
                int getappnum = currentApplyNumber - 1;
                if (haveCurrentApplyNumber)
                {
                    getappnum++;
                }

                sql.Where(d => d.ApplyNumber <= getappnum);
            }
            sql.OrderByDescending(d => d.OperateDate).OrderByDescending(d => d.Id);
            var list = sql.ToList();

            #region 填充Step和Remark
            //step和remark是后来添加的，为了适应老数据，所以需要根据OperateContent填充Step和Remark
            var stepMap = new Dictionary<string, OrderRefundStep>();
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.Audited.ToDescription(), OrderRefundStep.UnConfirm);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.UnAudit.ToDescription(), OrderRefundStep.UnAudit);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitAudit.ToDescription(), OrderRefundStep.WaitAudit);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery.ToDescription(), OrderRefundStep.WaitDelivery);
            stepMap.Add(OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving.ToDescription(), OrderRefundStep.WaitReceiving);
            stepMap.Add(OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm.ToDescription(), OrderRefundStep.UnConfirm);
            stepMap.Add(OrderRefundInfo.OrderRefundConfirmStatus.Confirmed.ToDescription(), OrderRefundStep.Confirmed);

            foreach (var item in list)
            {
                if (item.Step > 0)
                    continue;

                var match = System.Text.RegularExpressions.Regex.Match(item.OperateContent, "【(.+)】(.+)$");
                if (match.Success)
                {
                    var refundState = match.Groups[1].Value;
                    if (stepMap.ContainsKey(refundState))
                        item.Step = stepMap[refundState];
                    item.Remark = match.Groups[2].Value;
                }
            }
            #endregion

            return list;
        }
        /// <summary>
        /// 写入售后日志
        /// <para>写入日志的内容为：[状态]日志说明</para>
        /// </summary>
        /// <param name="RefundId"></param>
        /// <param name="LogContent"></param>
        public void AddRefundLog(long refundId, int? applyNumber, OrderRefundStep step, string refundState, string userName, string remark)
        {
            applyNumber = applyNumber.HasValue ? applyNumber.Value : 1;
            var data = new OrderRefundLogInfo();
            data.RefundId = refundId;
            data.ApplyNumber = applyNumber ?? 0;
            data.Operator = userName;
            data.OperateDate = DateTime.Now;
            data.OperateContent = "【" + refundState + "】" + remark;
            data.Remark = remark;
            data.Step = step;
            DbFactory.Default.Add(data);
        }
        /// <summary>
        /// 自动审核退款(job)
        /// </summary>
        //public void AutoAuditRefund()
        //{
        //    var sitesetser = ServiceProvider.Instance<ISiteSettingService>.Create;
        //    // var siteSetting = sitesetser.GetSiteSettings();
        //    var siteSetting = sitesetser.GetSiteSettingsByObjectCache();
        //    if (siteSetting.AS_ShopConfirmTimeout > 0)
        //    {
        //        DateTime stime = DateTime.Now.AddDays(-siteSetting.AS_ShopConfirmTimeout);
        //        var rflist = DbFactory.Default.Get<OrderRefundInfo>().Where(d => d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit && d.ApplyDate < stime).Select(d => d.Id).ToList<long>();
        //        if (rflist.Count > 0)
        //        {
        //            Mall.Core.Log.Debug("RefundJob : AutoAuditRefund Number=" + rflist.Count);
        //        }
        //        foreach (var item in rflist)
        //        {
        //            try
        //            {
        //                SellerDealRefund(item, OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery, "卖家超时未处理，系统自动同意售后", "系统Job");
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Debug("RefundJob : AutoAuditRefund [有错误]编号：" + item.ToString(), ex);
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// 自动关闭过期未寄货退款(job)
        /// </summary>
        //public void AutoCloseByDeliveryExpired()
        //{
        //    var sitesetser = ServiceProvider.Instance<ISiteSettingService>.Create;
        //    //  var siteSetting = sitesetser.GetSiteSettings();
        //    //windows服务调用此处不报错
        //    var siteSetting = sitesetser.GetSiteSettingsByObjectCache();
        //    if (siteSetting.AS_SendGoodsCloseTimeout > 0)
        //    {
        //        DateTime stime = DateTime.Now.AddDays(-siteSetting.AS_SendGoodsCloseTimeout);
        //        var rflist = DbFactory.Default.Get<OrderRefundInfo>().Where(d => d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery && d.SellerAuditDate < stime).Select(d => d.Id).ToList<long>();
        //        if (rflist.Count > 0)
        //        {
        //            Mall.Core.Log.Debug("RefundJob : AutoCloseByDeliveryExpired Number=" + rflist.Count);
        //        }
        //        foreach (var item in rflist)
        //        {
        //            try
        //            {
        //                SellerDealRefund(item, OrderRefundInfo.OrderRefundAuditStatus.UnAudit, "买家超时未寄货，系统自动拒绝售后", "系统Job");
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Debug("RefundJob : AutoCloseByDeliveryExpired [有错误]编号：" + item.ToString(), ex);
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// 自动商家确认到货(job)
        /// </summary>
        //public void AutoShopConfirmArrival()
        //{
        //    var sitesetser = ServiceProvider.Instance<ISiteSettingService>.Create;
        //    //  var siteSetting = sitesetser.GetSiteSettings();
        //    //windows服务得用此缓存
        //    var siteSetting = sitesetser.GetSiteSettingsByObjectCache();
        //    if (siteSetting.AS_ShopNoReceivingTimeout > 0)
        //    {
        //        DateTime stime = DateTime.Now.AddDays(-siteSetting.AS_ShopNoReceivingTimeout);
        //        var rflist = DbFactory.Default.Get<OrderRefundInfo>().Where(d => d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving && d.BuyerDeliverDate < stime).Select(d => d.Id).ToList<long>();
        //        if (rflist.Count > 0)
        //        {
        //            Mall.Core.Log.Debug("RefundJob : AutoShopConfirmArrival Number=" + rflist.Count);
        //        }
        //        foreach (var item in rflist)
        //        {
        //            try
        //            {
        //                SellerConfirmRefundGood(item, "系统Job");
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Debug("RefundJob : AutoShopConfirmArrival [有错误]编号：" + item.ToString(), ex);
        //            }
        //        }
        //    }
        //}
        #endregion

        /// <summary>
        /// 售后发送app消息
        /// </summary>
        /// <param name="orderInfo"></param>
        public void SendRefundAppMessage(OrderRefundInfo refundInfo, OrderInfo orderInfo)
        {
            IAppMessageService _iAppMessageService = Mall.ServiceProvider.Instance<IAppMessageService>.Create;
            var app = new AppMessageInfo()
            {
                IsRead = false,
                sendtime = DateTime.Now,
                SourceId = refundInfo.Id,
                TypeId = (int)AppMessagesType.AfterSale,
                OrderPayDate = Core.Helper.TypeHelper.ObjectToDateTime(orderInfo.PayDate),
                ShopId = 0,
                ShopBranchId = 0
            };
            if (refundInfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
            {
                app.Content = string.Format("{0} 等待您审核", orderInfo.Id);
                app.Title = "您有新的售后申请";
            }
            else if (refundInfo.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving)
            {
                app.Content = string.Format("{0} 等待您收货", orderInfo.Id);
                app.Title = "您有买家寄回的商品";
            }
            if (orderInfo.ShopBranchId > 0)
            {
                app.ShopBranchId = orderInfo.ShopBranchId;
            }
            else
            {
                app.ShopId = refundInfo.ShopId;
            }
            if (!string.IsNullOrEmpty(app.Title))
                _iAppMessageService.AddAppMessages(app);
        }

        /// <summary>
        /// 确认收货后，处理库存
        /// </summary>
        /// <param name="order"></param>
        private void ReturnStock(OrderItemInfo orderItem, OrderInfo order, long returnQuantity)
        {
            SKUInfo sku = DbFactory.Default.Get<SKUInfo>().Where(p => p.Id == orderItem.SkuId).FirstOrDefault();
            if (sku != null)
            {
                if (order.ShopBranchId > 0)
                {
                    var sbSku = DbFactory.Default.Get<ShopBranchSkuInfo>().Where(p => p.SkuId == sku.Id && p.ShopBranchId == order.ShopBranchId).FirstOrDefault();
                    if (sbSku != null)
                    {
                        sbSku.Stock += (int)returnQuantity;
                        DbFactory.Default.Update(sbSku);

                        // 限购还原活动库存
                        if (order.OrderType == OrderInfo.OrderTypes.LimitBuy)
                        {
                            var flashSaleDetailInfo = DbFactory.Default.Get<FlashSaleDetailInfo>().Where(a => a.SkuId == orderItem.SkuId && a.FlashSaleId == orderItem.FlashSaleId).FirstOrDefault();
                            if (flashSaleDetailInfo != null)
                            {
                                flashSaleDetailInfo.TotalCount += (int)returnQuantity;
                                DbFactory.Default.Update(flashSaleDetailInfo);
                            }
                        }

                        //还原商品销量
                        //var productid = orderItem.ProductId;
                        //var product = DbFactory.Default.Get<ProductInfo>().Where(d => d.Id == productid).FirstOrDefault();
                        //if (product != null)
                        //{
                        //    product.SaleCounts -= returnQuantity;
                        //    var searchProduct = DbFactory.Default.Get<SearchProductInfo>().Where(r => r.ProductId == product.Id).FirstOrDefault();
                        //    if (searchProduct != null)
                        //        searchProduct.SaleCount -= (int)returnQuantity;
                        //    if (searchProduct.SaleCount < 0)
                        //    {
                        //        searchProduct.SaleCount = 0;
                        //    }
                        //    if (product.SaleCounts < 0)
                        //    {
                        //        product.SaleCounts = 0;
                        //    }
                        //    DbFactory.Default.Update(product);
                        //    DbFactory.Default.Update(searchProduct);
                        //}
                    }
                }
                else
                {
                    sku.Stock += returnQuantity;
                    DbFactory.Default.Update(sku);
                }

            }

        }
    }
}
