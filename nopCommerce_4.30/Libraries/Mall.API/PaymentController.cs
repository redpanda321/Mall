using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.Core.Plugins.Payment;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.API
{
    public class PaymentController : BaseApiController
    {

        [HttpGet("GetEnablePaymentList")]
        public object GetEnablePaymentList()
        {
            var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).OrderByDescending(d => d.PluginInfo.PluginId).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.Android));
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
            string urlPre = webRoot + "/m-" + Core.PlatformType.Android + "/Payment/";
            var model = mobilePayments.ToArray().Select(item =>
            {
                string url = string.Empty;
                return new
                {
                    id = item.PluginInfo.PluginId,
                    name = PaymentApplication.GetForeGroundPaymentName(item.PluginInfo.DisplayName),
                    logo = item.Biz.Logo,
                    url = url
                };
            }).OrderByDescending(d => d.id);
            return new { success = true, data = model };
        }
        [HttpGet("GetPaymentList")]
        public object GetPaymentList(string orderIds, string typeid)
        {
            var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).OrderByDescending(d => d.PluginInfo.PluginId).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.Android));
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
            string urlPre = webRoot + "/m-" + Core.PlatformType.Android + "/Payment/";

            //获取待支付的所有订单
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var orders = orderService.GetOrders(orderIds.Split(',').Select(t => long.Parse(t))).Where(r => r.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitPay);
            var totalAmount = orders.Sum(t => t.OrderTotalAmount - t.CapitalAmount);

            //获取所有订单中的商品名称
            string productInfos = GetProductNameDescriptionFromOrders(orders);


            string[] strIds = orderIds.Split(',');
            string notifyPre = urlPre + "Notify/", returnPre = webRoot + "/m-" + Core.PlatformType.Android + "/Member/PaymentToOrders?ids=" + orderIds;
            var orderPayModel = strIds.Select(item => new Entities.OrderPayInfo
            {
                PayId = 0,
                OrderId = long.Parse(item)
            });
            //保存支付订单
            var payid = orderService.SaveOrderPayInfo(orderPayModel, Core.PlatformType.Android);
            var ids = payid.ToString();

            var model = mobilePayments.ToArray().Select(item =>
            {
                string url = string.Empty;
                try
                {
                    url = item.Biz.GetRequestUrl(returnPre, notifyPre + item.PluginInfo.PluginId.Replace(".", "-") + "/", ids, totalAmount, productInfos);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("获取支付方式错误：", ex);
                }
                return new
                {
                    id = item.PluginInfo.PluginId,
                    //name = item.PluginInfo.DisplayName,
                    name = PaymentApplication.GetForeGroundPaymentName(item.PluginInfo.DisplayName),
                    logo = item.Biz.Logo,
                    url = url
                };
            });
            model = model.Where(item => !string.IsNullOrWhiteSpace(item.url) && item.id.Contains(typeid)).OrderByDescending(d => d.id);
            return new { success = true, data = model };
        }

        [HttpGet("GetProductNameDescriptionFromOrders")]
        string GetProductNameDescriptionFromOrders(IEnumerable<Entities.OrderInfo> orders)
        {
            var orderitems = OrderApplication.GetOrderItemsByOrderId(orders.Select(p => p.Id));
            var productNames = orderitems.Select(p => p.ProductName);
            var productInfos = productNames.Count() > 1 ? (productNames.ElementAt(0) + " 等" + productNames.Count() + "种商品") : productNames.ElementAt(0);
            return productInfos;
        }

        /// <summary>
        /// 预存款支付
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
            var curUrl = HttpContext.Request.Scheme + "://" + Request.Host.ToString();
            OrderApplication.PayByCapital(CurrentUser.Id, ids, payPwd, curUrl);
            return new Result { success = true, msg = "支付成功" };
        }

        [HttpGet("GetPayByCapitalIsOk")]
        public object GetPayByCapitalIsOk(string ids)
        {
            CheckUserLogin();
            var result = OrderApplication.PayByCapitalIsOk(CurrentUser.Id, ids);
            return new Result { success = result };
        }

        /// <summary>
        /// 判断是否设置支付密码
        /// </summary>
        /// 
        [HttpGet("GetPayPwd")]
        public object GetPayPwd()
        {
            CheckUserLogin();
            bool result = false;
            result = OrderApplication.GetPayPwd(CurrentUser.Id);
            return new { success = result };
        }
        /// <summary>
        /// 判断支付密码是否正确
        /// </summary>
        /// 
        [HttpGet("VerificationPayPwd")]
        public object VerificationPayPwd(PaymentVerificationPayPwdModel papa)
        {
            CheckUserLogin();
            bool result = false;
            if (papa != null && !string.IsNullOrWhiteSpace(papa.pwd))
            {
                result = MemberApplication.VerificationPayPwd(CurrentUser.Id, papa.pwd);
            }
            return new { success = result };
        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostSetPayPwd")]
        public object PostSetPayPwd(LoginModPwdModel model)
        {
            CheckUserLogin();
            MemberCapitalApplication.SetPayPwd(CurrentUser.Id, model.Password);
            return SuccessResult("设置成功");
        }

        /// <summary>
        /// 预账户充值接口
        /// </summary>
        /// <param name="pluginId">支付插件Id</param>
        /// <param name="amount">充值金额</param>
        /// <returns></returns>
        /// 
        [HttpGet("Charge")]
        public object Charge(PaymentChargeModel para)
        {
            para.amount = Math.Round(para.amount, 2);
            if (para.amount <= 0)
                return ErrorResult("请输入正确的金额");

            try
            {
                //获取支付插件
                var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).OrderByDescending(d => d.PluginInfo.PluginId).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.Android));
                var plugin = mobilePayments.Where(x => x.PluginInfo.PluginId.Contains(para.typeid)).FirstOrDefault();
                //添加充值明细
                var chargeDetail = new DTO.ChargeDetail();
                chargeDetail.ChargeAmount = para.amount;
                chargeDetail.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.WaitPay;
                //chargeDetail.ChargeWay = plugin.PluginInfo.DisplayName;
                chargeDetail.ChargeWay = PaymentApplication.GetForeGroundPaymentName(plugin.PluginInfo.Description);
                chargeDetail.CreateTime = DateTime.Now;
                chargeDetail.MemId = CurrentUser.Id;
                var sitesetting = SiteSettingApplication.SiteSettings;
                if (para.ispresent && sitesetting.IsOpenRechargePresent)
                {
                    var rule = RechargePresentRuleApplication.GetRules().FirstOrDefault(d => d.ChargeAmount == para.amount);
                    if (rule != null)
                    {
                        chargeDetail.PresentAmount = rule.PresentAmount;
                    }
                }
                var id = MemberCapitalApplication.AddChargeApply(chargeDetail);

                string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
                string urlPre = webRoot + "/m-" + Core.PlatformType.Android + "/Payment/";
                string notifyPre = urlPre + "CapitalChargeNotify/";
                string returnPre = webRoot + "/m-" + Core.PlatformType.Android;

                var model = mobilePayments.ToArray().Select(item =>
                {
                    string url = string.Empty;
                    try
                    {
                        url = item.Biz.GetRequestUrl(returnPre, notifyPre + item.PluginInfo.PluginId.Replace(".", "-") + "/", id.ToString(), para.amount, "会员充值");
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error("获取支付方式错误：", ex);
                    }
                    return new
                    {
                        id = item.PluginInfo.PluginId,
                        //name = item.PluginInfo.DisplayName,
                        name = PaymentApplication.GetForeGroundPaymentName(item.PluginInfo.DisplayName),
                        logo = item.Biz.Logo,
                        url = url
                    };
                });
                model = model.Where(item => !string.IsNullOrWhiteSpace(item.url) && item.id.Contains(para.typeid)).OrderByDescending(d => d.id);
                dynamic result = SuccessResult();
                result.data = model;
                return result;
            }
            catch (Exception ex)
            {
                Core.Log.Error("预账户充值报错：", ex);
                return ErrorResult("预账户充值报错");
            }
        }
    }
}
