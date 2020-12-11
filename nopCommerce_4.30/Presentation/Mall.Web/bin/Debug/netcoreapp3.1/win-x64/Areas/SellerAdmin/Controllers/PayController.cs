using Mall.Core;
using Mall.Core.Plugins.Payment;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using Mall.DTO.QueryModel;

using Mall.IServices;
using Mall.Application;
using Mall.CommonModel;
using Mall.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Nop.Core.Infrastructure;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class PayController : BaseController
    {

        private IHttpContextAccessor _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();


        /// <summary>
        /// 平台充值同步回调
        /// </summary>
        /// <param name="id">充值订单号</param>
        /// <param name="balance">充值金额</param>
        /// <returns></returns>

        public ActionResult CapitalChargeReturn(string id)
        {
            id = DecodePaymentId(id);
            Log.Info("商家充值同步回调key：" + id);
            string error = string.Empty;

            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(_httpContextAccessor);
                CashDepositDetailInfo model = new CashDepositDetailInfo();
                string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                bool result = Cache.Get<bool>(payStateKey);//记录缓存，不重复处理
                if (!result)
                {
                    long orderIds = payInfo.OrderIds.FirstOrDefault();
                    Log.Info("商家充值同步回调订单号：" + orderIds);
                    BillingApplication.ShopRecharge(orderIds, payInfo.TradNo, id);

                    //写入支付状态缓存
                    Cache.Insert(payStateKey, true);//标记为已支付
                }
            }
            catch (Exception ex)
            {
                Log.Error("商家充值同步回调错误：" + ex.Message);
                error = ex.Message;
            }
            ViewBag.Error = error;
            return View();
        }


        /// <summary>
        /// 平台充值同步异步
        /// </summary>
        /// <param name="id">充值订单号</param>
        /// <param name="balance">充值金额</param>
        /// <returns></returns>
       
        public ContentResult CapitalChargeNotify(string id)
        {
            id = DecodePaymentId(id);
            Log.Info("商家充值异步回调key：" + id);
            string str = string.Empty;

            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(_httpContextAccessor);
                CashDepositDetailInfo model = new CashDepositDetailInfo();
                string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                bool result = Cache.Get<bool>(payStateKey);//记录缓存，不重复处理
                if (!result)
                {
                    long orderIds = payInfo.OrderIds.FirstOrDefault();
                    Log.Info("商家充值异步回调订单号：" + orderIds);
                    BillingApplication.ShopRecharge(orderIds, payInfo.TradNo, id);

                    str = payment.Biz.ConfirmPayResult();
                    //写入支付状态缓存
                    Cache.Insert(payStateKey, true);//标记为已支付
                }
            }
            catch (Exception ex)
            {
                Log.Error("商家充值异步回调错误：" + ex.Message);
            }
            return Content(str);
        }

        /// <summary>
        /// 订单解密处理方法
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }
    }
}