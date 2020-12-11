using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins.Payment;
using Mall.IServices;
using Mall.Web.App_Code.Common;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;


namespace Mall.Web.Areas.Web.Controllers
{
    public class PayController : BaseWebController
    {
        private IOrderService _iOrderService;
        private ICashDepositsService _iCashDepositsService;
        private IMemberCapitalService _iMemberCapitalService;
        private IRefundService _iRefundService;
        private IShopService _iShopService;
        private IFightGroupService _iFightGroupService;


        private IHttpContextAccessor _httpContextAccessor;

        private IWebHostEnvironment _hostingEnvironment;

        public PayController(IOrderService iOrderService, ICashDepositsService iCashDepositsService, IMemberCapitalService iMemberCapitalService
            , IRefundService iRefundService, IShopService iShopService, IFightGroupService iFightGroupService,
              IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment
            )
        {
            _iOrderService = iOrderService;
            _iCashDepositsService = iCashDepositsService;
            _iMemberCapitalService = iMemberCapitalService;
            _iRefundService = iRefundService;
            _iShopService = iShopService;
            _iFightGroupService = iFightGroupService;

            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = environment;
        }
        /// <summary>
        /// 支付跳转页
        /// <para>完成支付前参数整理工作，并跳转到支付页面</para>
        /// </summary>
        /// <param name="pmtid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult Index(string pmtid, string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return RedirectToAction("index", "userCenter", new { url = "/userOrder", tar = "userOrder" });
            }
            if (string.IsNullOrWhiteSpace(pmtid))
            {
                return RedirectToAction("Pay", "Order", new { orderIds = ids });
            }
            var orderIdArr = ids.Split(',').Select(item => long.Parse(item));
            //获取待支付的所有订单
            var orders = _iOrderService.GetOrders(orderIdArr).Where(item => item.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitPay && item.UserId == CurrentUser.Id).ToList();

            if (orders == null || orders.Count == 0) //订单状态不正确
            {
                return RedirectToAction("index", "userCenter", new { url = "/userOrder", tar = "userOrder" });
            }

            decimal total = orders.Sum(a => a.OrderTotalAmount - a.CapitalAmount);
            if (total == 0)
            {
                return RedirectToAction("Pay", "Order", new { orderIds = ids });
            }

            foreach (var item in orders)
            {
                if (item.OrderType == Entities.OrderInfo.OrderTypes.FightGroup)
                {
                    if (!_iFightGroupService.OrderCanPay(item.Id))
                    {
                        throw new MallException("有拼团订单为不可付款状态");
                    }
                }
            }

            //获取所有订单中的商品名称
            var productInfos = GetProductNameDescriptionFromOrders(orders);
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
            //获取同步返回地址
            string returnUrl = webRoot + "/Pay/Return/{0}";
            //获取异步通知地址
            string payNotify = webRoot + "/Pay/Notify/{0}/";
            var payment = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).FirstOrDefault(d => d.PluginInfo.PluginId == pmtid);
            if (payment == null)
            {
                throw new MallException("错误的支付方式");
            }

            #region 支付流水获取
            var orderPayModel = orders.Select(item => new Entities.OrderPayInfo
            {
                PayId = 0,
                OrderId = item.Id
            });
            //保存支付订单
            long payid = _iOrderService.SaveOrderPayInfo(orderPayModel, PlatformType.PC);
            #endregion

            //组织返回Model
            Mall.Web.Models.PayJumpPageModel model = new Mall.Web.Models.PayJumpPageModel();
            model.PaymentId = pmtid;
            model.OrderIds = ids;
            model.TotalPrice = total;
            model.UrlType = payment.Biz.RequestUrlType; ;
            model.PayId = payid;
            try
            {
                model.RequestUrl = payment.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(payment.PluginInfo.PluginId)), string.Format(payNotify, EncodePaymentId(payment.PluginInfo.PluginId)), payid.ToString(), total, productInfos);
            }
            catch (Exception ex)
            {
                Core.Log.Error("支付页面加载支付插件出错", ex);
                model.IsErro = true;
                model.ErroMsg = ex.Message + ",请检查平台支付配置";
            }
            if (string.IsNullOrWhiteSpace(model.RequestUrl))
            {
                model.IsErro = true;
                model.ErroMsg = "获取支付地址为空,请检查平台支付配置";
            }
            switch (model.UrlType)
            {
                case UrlType.Page:
                    return Redirect(model.RequestUrl);
                case UrlType.QRCode:
                    return Redirect("/Pay/QRPay/?id=" + pmtid + "&url=" + HttpUtility.UrlEncode(model.RequestUrl) + "&orderIds=" + ids);
            }
            ViewBag.Keyword = SiteSettings.Keyword;
            //form提交在页面组织参数并自动提交
            return View(model);
        }

        /// <summary>
        /// 获取订单内商品名称
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        private string GetProductNameDescriptionFromOrders(IEnumerable<Entities.OrderInfo> orders)
        {
            var orderitems = _iOrderService.GetOrderItemsByOrderId(orders.Select(p => p.Id));
            var productNames = orderitems.Select(p => p.ProductName).ToList();
            var productInfos = productNames.Count() > 1 ? (productNames.ElementAt(0) + " 等" + productNames.Count() + "种商品") : productNames.ElementAt(0);
            return productInfos;
        }

        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        private string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }


        [ActionName("Notify")]
       
        public ContentResult PayNotify_Post(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessNotify(_httpContextAccessor);
                var payTime = payInfo.TradeTime;
                var orderid = payInfo.OrderIds.FirstOrDefault();
                var orderIds = _iOrderService.GetOrderPay(orderid).Select(item => item.OrderId).ToList();
                try
                {
                    _iOrderService.PaySucceed(orderIds, id, payInfo.TradeTime.Value, payInfo.TradNo, payId: orderid);
                    response = payment.Biz.ConfirmPayResult();
                    //写入支付状态缓存
                    string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
                    Cache.Insert(payStateKey, true, 15);//标记为已支付
                    #region TOD:ZYF 注释，统一在service的方法里实现
                    //限时购销量
                    //PaymentHelper.IncreaseSaleCount(orderIds);
                    #endregion
                    //红包
                    PaymentHelper.GenerateBonus(orderIds, Request.Host.ToString());
                }
                catch (Exception e)
                {
                    string logErr = id + " " + orderid.ToString();
                    if (payInfo.TradeTime.HasValue)
                    {
                        logErr += " TradeTime:" + payInfo.TradeTime.Value.ToString();
                    }
                    logErr += " TradNo:" + payInfo.TradNo;
                    Log.Error(logErr, e);
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Log.Error("PayNotify_Post", ex);
            }
            return Content(response);
        }
        #region 退款异步通知
        [ActionName("RefundNotify")]
       
        public ContentResult RefundNotify_Post(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;

            Log.Info("[异步退款]开始：" + id);
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessRefundNotify(_httpContextAccessor);

                if (payInfo != null)
                {
                    string refund_batch_no = payInfo.TradNo;
                    DateTime? refund_time = payInfo.TradeTime;

                    try
                    {
                        _iRefundService.NotifyRefund(refund_batch_no);
                        response = payment.Biz.ConfirmPayResult();
                    }
                    catch (Exception e)
                    {
                        string logErr = id + " 退款异常";
                        if (payInfo.TradeTime.HasValue)
                        {
                            logErr += " RefundTime:" + refund_time.ToString();
                        }
                        logErr += " RefundBatchNo:" + refund_batch_no;
                        Log.Error(logErr, e);
                    }
                }
                else
                {
                    Log.Info("[异步退款]失败：" + id + " - 插件实例失败");
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Log.Info("[异步退款]失败：" + id);
                Log.Error("RefundNotify_Post", ex);
            }
            return Content(response);
        }
        #endregion

        #region 缴纳保证金回调处理
        [ActionName("CashNotify")]
       
        public ContentResult CashPayNotify_Post(string id, string str)
        {
            #region 如微信扫描支付参数特别处理
            if (!string.IsNullOrEmpty(id) && string.IsNullOrEmpty(str))
            {
                if (id.ToLower().IndexOf("weixinpay_native") != -1 && id.IndexOf("~") != -1)
                {
                    str = id.Split('~')[1];
                    id = id.Split('~')[0];
                }
            }
            #endregion

            //Log.Error("id:" + id + "--str:" + str);

            decimal balance = decimal.Parse(str.Split('-')[0]);
            string userName = str.Split('-')[1];
            long shopId = long.Parse(str.Split('-')[2]);
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessNotify(_httpContextAccessor);
                string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                bool result = Cache.Get<bool>(payStateKey);
                if (!result)
                {
                    var accountService = _iCashDepositsService;
                    Entities.CashDepositDetailInfo model = new Entities.CashDepositDetailInfo();

                    model.AddDate = DateTime.Now;
                    model.Balance = balance;
                    model.Description = "充值";
                    model.Operator = userName;

                    var list = new List<Entities.CashDepositDetailInfo>();
                    list.Add(model);
                    if (accountService.GetCashDepositByShopId(shopId) == null)
                    {
                        var cashDeposit = new Entities.CashDepositInfo()
                        {
                            CurrentBalance = balance,
                            Date = DateTime.Now,
                            ShopId = shopId,
                            TotalBalance = balance,
                            EnableLabels = true,
                        };
                        accountService.AddCashDeposit(cashDeposit,list);
                    }
                    else
                    {
                        model.CashDepositId = accountService.GetCashDepositByShopId(shopId).Id;
                        _iCashDepositsService.AddCashDepositDetails(model);
                    }
                    response = payment.Biz.ConfirmPayResult();

                    Cache.Insert(payStateKey, true);//标记为已支付
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Log.Error("CashPayNotify_Post", ex);
            }
            return Content(response);
        }
        #endregion 保证金

        //TODO:[LLY]增加预存款充值回调
        #region 预存款充值回调
        [ActionName("CapitalChargeNotify")]
       
        public ContentResult PayNotify_Charge(string id)
        {
            string result = string.Empty;
            try
            {
                id = DecodePaymentId(id);
                var payPlugin = PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                if (payPlugin != null)
                {
                    var paymentInfo = payPlugin.Biz.ProcessNotify(_httpContextAccessor);
                    var service = _iMemberCapitalService;
                    var chargeInfo = service.GetChargeDetail(paymentInfo.OrderIds.FirstOrDefault());
                    if (chargeInfo != null && chargeInfo.ChargeStatus != Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess)
                    {
                        //chargeInfo.ChargeWay = payPlugin.PluginInfo.DisplayName;
                        chargeInfo.ChargeWay = PaymentApplication.GetForeGroundPaymentName(payPlugin.PluginInfo.Description);
                        chargeInfo.ChargeStatus = Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
                        chargeInfo.ChargeTime = paymentInfo.TradeTime.HasValue ? paymentInfo.TradeTime.Value : DateTime.Now;
                        service.UpdateChargeDetail(chargeInfo);
                        result = payPlugin.Biz.ConfirmPayResult();
                        string payStateKey = CacheKeyCollection.PaymentState(chargeInfo.Id.ToString());//获取支付状态缓存键
                        Cache.Insert(payStateKey, true, 15);//标记为已支付
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("预存款充值回调出错：" + ex.Message);
            }
            return Content(result);
        }

        [ActionName("CapitalChargeReturn")]
       
        public ActionResult PayReturn_Charge(string id)
        {
            string result = string.Empty;
            try
            {
                id = DecodePaymentId(id);
                var payPlugin = PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                if (payPlugin != null)
                {
                    var paymentInfo = payPlugin.Biz.ProcessReturn(_httpContextAccessor);
                    var service = _iMemberCapitalService;
                    var chargeInfo = service.GetChargeDetail(paymentInfo.OrderIds.FirstOrDefault());
                    if (chargeInfo != null && chargeInfo.ChargeStatus != Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess)
                    {
                        //chargeInfo.ChargeWay = payPlugin.PluginInfo.DisplayName;
                        chargeInfo.ChargeWay = PaymentApplication.GetForeGroundPaymentName(payPlugin.PluginInfo.Description);
                        chargeInfo.ChargeStatus = Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
                        chargeInfo.ChargeTime = paymentInfo.TradeTime.HasValue ? paymentInfo.TradeTime.Value : DateTime.Now;
                        service.UpdateChargeDetail(chargeInfo);
                        result = payPlugin.Biz.ConfirmPayResult();
                        string payStateKey = CacheKeyCollection.PaymentState(chargeInfo.Id.ToString());//获取支付状态缓存键
                        Cache.Insert(payStateKey, true, 15);//标记为已支付
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("预存款充值回调出错：" + ex.Message);
            }
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }
        #endregion

        #region 企业付款回调
        /// <summary>
        /// 用户提现
        /// </summary>
        /// <param name="id"></param>
        /// <param name="outid"></param>
        /// <returns></returns>
        [ActionName("EnterpriseNotify")]
       
        public ContentResult EnterpriseNotify_Post(string id, string outid)
        {
            Log.Info("[ENP]" + Core.Helper.WebHelper.GetRawUrl());
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;
            var _iOperationLogService = ServiceApplication.Create<IOperationLogService>();
            var withdrawId = long.Parse(outid);
            var withdrawData = MemberCapitalApplication.GetApplyWithDrawInfo(withdrawId);
            if (withdrawData == null)
            {
                Log.Info("[EnterpriseNotify]" + id + " ^ " + outid);
                throw new MallException("参数错误");
            }
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessEnterprisePayNotify(_httpContextAccessor);
                if (withdrawData.ApplyStatus == Mall.Entities.ApplyWithdrawInfo.ApplyWithdrawStatus.PayPending)
                {
                    withdrawData.ApplyStatus = Mall.Entities.ApplyWithdrawInfo.ApplyWithdrawStatus.WithDrawSuccess;
                    withdrawData.PayTime = DateTime.Now;
                    MemberCapitalApplication.ConfirmApplyWithDraw(withdrawData);
                }
                response = payment.Biz.ConfirmPayResult();
            }
            catch (Exception ex)
            {
                //支付失败
                withdrawData.ApplyStatus = Mall.Entities.ApplyWithdrawInfo.ApplyWithdrawStatus.PayFail;
                withdrawData.Remark = "异步通知失败，请查看日志";
                withdrawData.ConfirmTime = DateTime.Now;
                MemberCapitalApplication.ConfirmApplyWithDraw(withdrawData);
                //操作日志
                _iOperationLogService.AddPlatformOperationLog(new Entities.LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("会员提现审核失败，提现编号：{0}", outid),
                    IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
                    PageUrl = "/Pay/EnterpriseNotify",
                    UserName = "异步回调",
                    ShopId = 0

                });
                errorMsg = ex.Message;
                Log.Error("EnterpriseNotify_Post", ex);
            }
            return Content(response);
        }
        /// <summary>
        /// 商家提现
        /// </summary>
        /// <param name="id"></param>
        /// <param name="outid"></param>
        /// <returns></returns>
        [ActionName("ShopEnterpriseNotify")]
       
        public ContentResult ShopEnterpriseNotify_Post(string id, string outid)
        {
            Log.Info("[SENP]" + Core.Helper.WebHelper.GetRawUrl());
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;
            var _iOperationLogService = ServiceApplication.Create<IOperationLogService>();
            var withdrawId = long.Parse(outid);
            var withdrawData = BillingApplication.GetShopWithDrawInfo(withdrawId);
            if (withdrawData == null)
            {
                Log.Info("[ShopEnterpriseNotify_Post]" + id + " ^ " + outid);
                throw new MallException("参数错误");
            }
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessEnterprisePayNotify(_httpContextAccessor);
                if (withdrawData.Status == Mall.CommonModel.WithdrawStaus.PayPending)
                {
                    BillingApplication.ShopApplyWithDrawCallBack(withdrawId, Mall.CommonModel.WithdrawStaus.Succeed, payInfo, "支付宝提现成功",  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(), "异步回调");
                }
                response = payment.Biz.ConfirmPayResult();
            }
            catch (Exception ex)
            {
                BillingApplication.ShopApplyWithDrawCallBack(withdrawId, Mall.CommonModel.WithdrawStaus.Fail, null, "支付宝提现失败",  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(), "异步回调");
                errorMsg = ex.Message;
                Log.Error("ShopEnterpriseNotify_Post", ex);
            }
            return Content(response);
        }
        #endregion

        #region 店铺续费支付回调处理

        [ActionName("ReNewPayNotify")]
       
        public ContentResult ReNewPayNotify_Post(string id, string str)
        {
            #region 如微信扫描支付参数特别处理
            if (!string.IsNullOrEmpty(id) && string.IsNullOrEmpty(str))
            {
                if (id.ToLower().IndexOf("weixinpay_native") != -1 && id.IndexOf("~") !=-1)
                {
                    str = id.Split('~')[1];
                    id = id.Split('~')[0];
                }
            }
            #endregion

            //Log.Error("id:" + id + "--str:" + str);
            decimal balance = decimal.Parse(str.Split('-')[0]);
            string userName = str.Split('-')[1];
            long shopId = long.Parse(str.Split('-')[2]);
            int type = int.Parse(str.Split('-')[3]);
            int value = int.Parse(str.Split('-')[4]);
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;

            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessNotify(_httpContextAccessor);
                //Entities.ShopRenewRecordInfo model = new Entities.ShopRenewRecordInfo();
                //model.TradeNo = payInfo.TradNo;
                string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                bool result = Cache.Get<bool>(payStateKey);
                if (!result)
                {
                    #region TDO:ZYF移至公共方法
                    ShopApplication.ShopReNewPayNotify(payInfo.TradNo, shopId, userName, balance, type, value);
                    ////添加店铺续费记录
                    //model.ShopId = shopId;
                    //model.OperateDate = DateTime.Now;
                    //model.Operator = userName;
                    //model.Amount = balance;
                    ////续费操作
                    //if (type == 1)
                    //{
                    //    model.OperateType = Entities.ShopRenewRecordInfo.EnumOperateType.ReNew;
                    //    var shopInfo = _iShopService.GetShop(shopId);
                    //    DateTime beginTime = shopInfo.EndDate.Value;
                    //    if (beginTime < DateTime.Now)
                    //        beginTime = DateTime.Now;
                    //    string strNewEndTime = beginTime.AddYears(value).ToString("yyyy-MM-dd");
                    //    model.OperateContent = "续费 " + value + " 年至 " + strNewEndTime;
                    //    _iShopService.AddShopRenewRecord(model);
                    //    //店铺续费
                    //    _iShopService.ShopReNew(shopId, value);
                    //}
                    ////升级操作
                    //else
                    //{
                    //    model.ShopId = shopId;
                    //    model.OperateType = Entities.ShopRenewRecordInfo.EnumOperateType.Upgrade;
                    //    var shopInfo = _iShopService.GetShop(shopId);
                    //    var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
                    //    var newshopGrade = _iShopService.GetShopGrades().Where(c => c.Id == (long)value).FirstOrDefault();
                    //    model.OperateContent = "将套餐‘" + shopGrade.Name + "'升级为套餐‘" + newshopGrade.Name + "'";
                    //    _iShopService.AddShopRenewRecord(model);
                    //    //店铺升级
                    //    _iShopService.ShopUpGrade(shopId, (long)value);
                    //}
                    #endregion
                    response = payment.Biz.ConfirmPayResult();

                    Cache.Insert(payStateKey, true);//标记为已支付
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Log.Error("ReNewPayNotify_Post", ex);
            }
            return Content(response);
        }

        #endregion

        public ActionResult QRPay(string url, string id)
        {
            ViewBag.Logo = SiteSettings.Logo;//获取Logo


            var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
            //ViewBag.Title = payment.PluginInfo.DisplayName + "支付";
            //ViewBag.Name = payment.PluginInfo.DisplayName;
            ViewBag.Title = PaymentApplication.GetForeGroundPaymentName(payment.PluginInfo.DisplayName) + "支付";
            ViewBag.Name = PaymentApplication.GetForeGroundPaymentName(payment.PluginInfo.DisplayName);

            //生成二维码
            var qrCode = Core.Helper.QRCodeHelper.Create(url);
            string fileName = DateTime.Now.ToString("yyMMddHHmmssffffff") + ".jpg";
            var qrCodeImagePath = "/temp/" + fileName;
            //qrCode.Save(Server.MapPath("~/temp/") + fileName);
            MemoryStream ms = new MemoryStream();
            qrCode.Save(ms, ImageFormat.Gif);
            ms.Flush();
            MemoryStream mem = new MemoryStream(ms.ToArray());
            //通过IO策略来保存图片
            Core.MallIO.CreateFile(qrCodeImagePath, mem, FileCreateType.Create);
            ms.Dispose();
            mem.Dispose();
            //通过IO策略返回图片绝对地址
            ViewBag.QRCode = Core.MallIO.GetFilePath(qrCodeImagePath);

            ViewBag.HelpImage = "/Plugins/Payment/" + payment.PluginInfo.ClassFullName.Split(',')[1] + "/" + payment.Biz.HelpImage;

            ViewBag.Step = 2;//支付第二步
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }

        public ActionResult ReturnSuccess(string id)
        {
            ViewBag.OrderIds = Request.Query[id];
            ViewBag.Logo = SiteSettings.Logo;//获取Logo
            ViewBag.Keyword = SiteSettings.Keyword;
            return View("Return");
        }


        public ActionResult Return(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(_httpContextAccessor);
                var payTime = payInfo.TradeTime;

                var orderid = payInfo.OrderIds.FirstOrDefault();
                var orderIds = _iOrderService.GetOrderPay(orderid).Select(item => item.OrderId).ToList();

                ViewBag.OrderIds = string.Join(",", orderIds);
                _iOrderService.PaySucceed(orderIds, id, payInfo.TradeTime.Value, payInfo.TradNo, payId: orderid);

                //写入支付状态缓存
                string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
                Cache.Insert(payStateKey, true, 15);//标记为已支付

                //红包
                PaymentHelper.GenerateBonus(orderIds, Request.Host.ToString());

                //Dictionary<long , ShopBonusInfo> bonusGrantIds = new Dictionary<long , ShopBonusInfo>();
                //string url = "http://" + Request.Url.Host.ToString() + "/m-weixin/shopbonus/index/";
                //var bonusService = ServiceApplication.Create<IShopBonusService>();
                //var buyOrders = _iOrderService.GetOrders( orderIds.AsEnumerable() );
                //foreach( var o in buyOrders )
                //{
                //    var shopBonus = bonusService.GetByShopId( o.ShopId );
                //    if( shopBonus != null && shopBonus.GrantPrice <= o.OrderTotalAmount )
                //    {
                //        long grantid = bonusService.GenerateBonusDetail( shopBonus , o.UserId , o.Id , url );
                //        bonusGrantIds.Add( grantid , shopBonus );
                //    }
                //}
            }
            catch (Exception ex)
            {
                Log.Error("pay Return:" + ex.Message);
                errorMsg = ex.Message;
                Log.Error(errorMsg);
            }
            ViewBag.Error = errorMsg;
            ViewBag.Logo = SiteSettings.Logo;//获取Logo
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }


        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }


       
        public JsonResult dadaOrderNotify(long order_id, int order_status, string client_id, string cancel_reason
            , int? cancel_from, int? update_time, string signature, int? dm_id, string dm_name, string dm_mobile)
        {
            Core.Log.Error("快递信息回调：" + order_id + "_" + order_status + "_" + client_id);
            var order = OrderApplication.GetOrder(order_id);
            if (order != null || order_status != 0)
            {
                if (order_status == (int)DadaStatus.Cancel)
                {
                    ExpressDaDaApplication.SetOrderCancel(order_id, cancel_reason);
                }
                else if (order_status == (int)DadaStatus.WaitTake)
                {
                    ExpressDaDaApplication.SetOrderWaitTake(order_id, client_id);
                    OrderApplication.SendMessageOnOrderShipping(order_id);  //发送发货消息
                }
                else if (order_status == (int)DadaStatus.Finished)
                {
                    ExpressDaDaApplication.SetOrderFinished(order_id);
                }
                //Core.Log.Error("快递信息回调已进入调用处理：" + order_id + "_" + order_status + "_" + client_id);
                ExpressDaDaApplication.SetOrderDadaStatus(order_id, order_status, client_id);
            }
            return Json(new { });
        }

    }
}