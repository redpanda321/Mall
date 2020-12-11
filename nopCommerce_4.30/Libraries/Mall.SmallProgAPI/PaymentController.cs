using Mall.Application;
using Mall.Core.Plugins.Message;
using Mall.Core.Plugins.Payment;
using Mall.IServices;
using Mall.SmallProgAPI.Model.ParamsModel;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.SmallProgAPI
{
    public class PaymentController : BaseApiController
    {
        public object GetPaymentList(string orderId)
        {
            CheckUserLogin();
            var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.WeiXinSmallProg));
            string webRoot = Core.Helper.WebHelper.GetScheme() + "://" + Core.Helper.WebHelper.GetHost();
            string urlPre = webRoot + "/m-" + Core.PlatformType.Android + "/Payment/";

            //获取待支付的所有订单
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var orders = orderService.GetOrders(orderId.Split(',').Select(t => long.Parse(t))).Where(r => r.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitPay);
            var totalAmount = orders.Sum(t => t.OrderTotalAmount - t.CapitalAmount);

            //获取所有订单中的商品名称
            string productInfos = GetProductNameDescriptionFromOrders(orders);


            string[] strIds = orderId.Split(',');
            string notifyPre = urlPre + "Notify/", returnPre = webRoot + "/m-" + Core.PlatformType.Android + "/Member/PaymentToOrders?ids=" + orderId;
            var orderPayModel = strIds.Select(item => new Entities.OrderPayInfo
            {
                PayId = 0,
                OrderId = long.Parse(item)
            });
            //保存支付订单
            var payid = orderService.SaveOrderPayInfo(orderPayModel, Core.PlatformType.WeiXinSmallProg);
            var ids = payid.ToString();

            var models = mobilePayments.ToArray().Select(item =>
            {
                string url = string.Empty;
                try
                {
                    url = item.Biz.GetRequestUrl(returnPre, notifyPre + item.PluginInfo.PluginId.Replace(".", "-") + "/", ids, totalAmount, productInfos, CurrentUserOpenId);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("获取支付方式错误：", ex);
                }
                //适配小程序接口，从支付插件里解析出相应参数
                //字符串格式：prepayId:234320480,partnerid:32423489,nonceStr=dslkfjsld
                #region 适配小程序接口，从支付插件里解析出相应参数
                var prepayId = string.Empty;
                var nonceStr = string.Empty;
                var timeStamp = string.Empty;
                var sign = string.Empty;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var paras = url.Split(',');
                    foreach (var str in paras)
                    {
                        var keyValuePair = str.Split(':');
                        if (keyValuePair.Length == 2)
                        {
                            switch (keyValuePair[0])
                            {
                                case "prepayId":
                                    prepayId = keyValuePair[1];
                                    break;
                                case "nonceStr":
                                    nonceStr = keyValuePair[1];
                                    break;
                                case "timeStamp":
                                    timeStamp = keyValuePair[1];
                                    break;
                                case "sign":
                                    sign = keyValuePair[1];
                                    break;
                            }
                        }
                    }
                }
                #endregion 
                return new
                {
                    prepayId = prepayId,
                    nonceStr = nonceStr,
                    timeStamp = timeStamp,
                    sign = sign
                };
            });
            var model = models.FirstOrDefault();
            if (null == model) return Json(new int[0]);

            if (!string.IsNullOrEmpty(model.prepayId))
            {
                Entities.WXAppletFormDataInfo info = new Entities.WXAppletFormDataInfo();
                info.EventId = Convert.ToInt64(MessageTypeEnum.OrderPay);
                info.EventTime = DateTime.Now;
                info.EventValue = orderId;
                info.ExpireTime = DateTime.Now.AddDays(7);
                info.FormId = model.prepayId;
                WXMsgTemplateApplication.AddWXAppletFromData(info);
            }
            return Json(model);
        }

        string GetProductNameDescriptionFromOrders(IEnumerable<Entities.OrderInfo> orders)
        {
            var items = ServiceProvider.Instance<IOrderService>.Create.GetOrderItemsByOrderId(orders.Select(p => p.Id));
            var productNames = items.Select(p => p.ProductName);
            var productInfos = productNames.Count() > 1 ? (productNames.ElementAt(0) + " 等" + productNames.Count() + "种商品") : productNames.ElementAt(0);
            return productInfos;
        }

        /// <summary>
        /// 预付款支付
        /// </summary>
        /// <param name="pmtidpmtid"></param>
        /// <param name="ids"></param>
        /// <param name="payPwd"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetPayByCapital")]
        public object GetPayByCapital(string ids, string payPwd)
        {
            CheckUserLogin();
            var curUrl = Request.Scheme + "://" + Request.Path;
            OrderApplication.PayByCapital(CurrentUser.Id, ids, payPwd, curUrl);
            return Json(new { msg = "支付成功" });
        }

        [HttpGet("GetPayByCapitalIsOk")]
        public object GetPayByCapitalIsOk(string ids)
        {
            CheckUserLogin();
            var result = OrderApplication.PayByCapitalIsOk(CurrentUser.Id, ids);
            return Json(ApiResult(result, data: result));
        }

        /// <summary>
        /// 判断是否设置支付密码
        /// </summary>
        /// 
        [HttpGet("GetHasSetPayPwd")]
        public object GetHasSetPayPwd()
        {
            CheckUserLogin();
            bool result = false;
            result = OrderApplication.GetPayPwd(CurrentUser.Id);
            return Json(ApiResult(result, data: result));
        }
        /// <summary>
        /// 判断支付密码是否正确
        /// </summary>
        /// 
        [HttpGet("CheckPayPwd")]
        public  object CheckPayPwd(PostSetPayPwdModel papa)
        {
            CheckUserLogin();
            bool result = false;
            if (papa != null && !string.IsNullOrWhiteSpace(papa.pwd))
            {
                result = MemberApplication.VerificationPayPwd(CurrentUser.Id, papa.pwd);
            }
            return Json(ApiResult(result, data: result));
        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostSetPayPwd")]
        public object PostSetPayPwd(PostSetPayPwdModel model)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(model.pwd))
            {
                return Json(ErrorResult<int>("支付密码不能为空"));
            }
            MemberCapitalApplication.SetPayPwd(CurrentUser.Id, model.pwd);
            return Json(new { msg = "设置成功" });
        }


    }
}
