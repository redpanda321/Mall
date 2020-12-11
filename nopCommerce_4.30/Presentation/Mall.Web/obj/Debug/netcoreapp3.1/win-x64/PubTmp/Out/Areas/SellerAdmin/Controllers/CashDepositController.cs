using Mall.Core;
using Mall.Core.Plugins.Payment;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using Mall.DTO.QueryModel;
using Mall.CommonModel;
using Mall.DTO;
using Mall.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class CashDepositController : BaseSellerController
    {
        private ICashDepositsService _iCashDepositsService;

        private IHttpContextAccessor _httpContextAccessor;

        public CashDepositController(ICashDepositsService iCashDepositsService
              , IHttpContextAccessor httpContextAccessor)
        {
            this._iCashDepositsService = iCashDepositsService;
            _httpContextAccessor = httpContextAccessor;
        }

        public ActionResult Management()
        {
            var model = _iCashDepositsService.GetCashDepositByShopId(CurrentSellerManager.ShopId);
            ViewBag.NeedPayCashDeposit = _iCashDepositsService.GetNeedPayCashDepositByShopId(CurrentSellerManager.ShopId);
            var shopAccount = ShopApplication.GetShopAccount(CurrentSellerManager.ShopId);
            ViewBag.ShopAccountAmount = shopAccount.Balance.ToString("F2");
            if (ViewBag.NeedPayCashDeposit == -1)
            {
                return View("UnSet");
            }
            return View(model);
        }

        public JsonResult CashDepositDetail(long cashDepositId, int pageNo = 1, int pageSize = 100000)
        {
            CashDepositDetailQuery query = new CashDepositDetailQuery()
            {
                CashDepositId = cashDepositId,
                PageNo = pageNo,
                PageSize = pageSize
            };
            QueryPageModel<Entities.CashDepositDetailInfo> cashDepositDetails = _iCashDepositsService.GetCashDepositDetails(query);
            var cashDepositDetailModel = cashDepositDetails.Models.ToArray().Select(item => new
            {
                Id = item.Id,
                Date = item.AddDate.ToString("yyyy-MM-dd HH:mm"),
                Balance = item.Balance,
                Operator = item.Operator,
                Description = item.Description
            });
            return Json(new { rows = cashDepositDetailModel, total = cashDepositDetails.Total });

        }
        public JsonResult PaymentList(decimal balance)
        {
            var needPayCashDeposit = _iCashDepositsService.GetNeedPayCashDepositByShopId(CurrentSellerManager.ShopId);
            if (balance < needPayCashDeposit)
            {
                throw new MallException("缴纳保证金必须大于应缴保证金");
            }
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();

            //获取同步返回地址
            string returnUrl = webRoot + "/SellerAdmin/CashDeposit/Return/{0}?balance={1}";

            //获取异步通知地址
            string payNotify = webRoot + "/pay/CashNotify/{0}/?str={1}";

            var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

            const string RELATEIVE_PATH = "/Plugins/Payment/";

            //不重复数字
            string ids = DateTime.Now.ToString("yyyyMMddmmss") + CurrentSellerManager.ShopId.ToString();

            var models = payments.Select(item =>
            {
                string requestUrl = string.Empty;
                try
                {
                    string strPaymentId = EncodePaymentId(item.PluginInfo.PluginId);//异步是Id参数
                    string strstr = balance + "-" + CurrentSellerManager.UserName + "-" + CurrentSellerManager.ShopId;//异步是str参数
                    string strreturnUrl = string.Format(returnUrl, strPaymentId, balance);
                    string strpayNotify = string.Format(payNotify, strPaymentId, strstr);

                    #region 当微信扫描支付特殊处理参数：//在微信扫描支付用“?”参数传不过来，则第二个参数用“~”隔开传
                    if (strPaymentId.ToLower().IndexOf("weixinpay_native") != -1)
                    {
                        //在微信扫描支付?后参数不传过来，则第二个参数用“~”分开传
                        strpayNotify = string.Format(webRoot + "/pay/CashNotify/{0}", strPaymentId + "~" + strstr);
                    }
                    //Log.Error("strreturnUrl:" + strreturnUrl);
                    //Log.Error("strpayNotify:" + strpayNotify);
                    #endregion

                    requestUrl = item.Biz.GetRequestUrl(strreturnUrl, strpayNotify, ids, balance, "保证金充值");

                    #region //微信扫描支付它原本requestUrl是作为url参数，在url参数：加上了orderids参数特殊处理下
                    if (strPaymentId.ToLower().IndexOf("weixinpay_native") != -1)
                    {
                        requestUrl = HttpUtility.UrlEncode(requestUrl) + "&orderIds=" + ids + "&type=shopcashdeposit";
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Core.Log.Error("支付页面加载支付插件出错", ex);
                }
                return new PaymentModel()
                {
                    Logo = RELATEIVE_PATH + item.PluginInfo.ClassFullName.Split(',')[1] + "/" + item.Biz.Logo,
                    RequestUrl = requestUrl,
                    UrlType = item.Biz.RequestUrlType,
                    Id = item.PluginInfo.PluginId
                };
            }).OrderByDescending(d=>d.Id).AsEnumerable();
            models = models.Where(item => !string.IsNullOrEmpty(item.RequestUrl) && item.Id != "Mall.Plugin.Payment.WeiXinPay");//只选择正常加载的插件  && item.Id != "Mall.Plugin.Payment.WeiXinPay_Native"
            return Json(models);
        }

        string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }

        [ActionName("CashNotify")]
       
        public ContentResult CashPayNotify_Post(string id, string str)
        {
            decimal balance = decimal.Parse(str.Split('-')[0]);
            string userName = str.Split('-')[1];
            long shopId = long.Parse(str.Split('-')[2]);
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(_httpContextAccessor);
                string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                bool result = Cache.Get<bool>(payStateKey);
                if (!result)
                {
                    var accountService = _iCashDepositsService;
                    var model = new Entities.CashDepositDetailInfo();

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
                        accountService.AddCashDeposit(cashDeposit, list);
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
            }
            return Content(response);
        }

        public ActionResult Return(string id, decimal balance)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;

            try
            {

                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(_httpContextAccessor);
                var accountService = _iCashDepositsService;
                Entities.CashDepositDetailInfo model = new Entities.CashDepositDetailInfo();
                string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                bool result = Cache.Get<bool>(payStateKey);
                if (!result)
                {
                    model.AddDate = DateTime.Now;
                    model.Balance = balance;
                    model.Description = "充值";
                    model.Operator = CurrentSellerManager.UserName;

                    var list = new List<Entities.CashDepositDetailInfo>();
                    list.Add(model);
                    if (accountService.GetCashDepositByShopId(CurrentSellerManager.ShopId) == null)
                    {
                        var cashDeposit = new Entities.CashDepositInfo()
                        {
                            CurrentBalance = balance,
                            Date = DateTime.Now,
                            ShopId = CurrentSellerManager.ShopId,
                            TotalBalance = balance,
                            EnableLabels = true,
                        };
                        accountService.AddCashDeposit(cashDeposit, list);
                    }

                    else
                    {
                        model.CashDepositId = accountService.GetCashDepositByShopId(CurrentSellerManager.ShopId).Id;
                        _iCashDepositsService.AddCashDepositDetails(model);
                    }

                    //写入支付状态缓存
                    Cache.Insert(payStateKey, true);//标记为已支付
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            ViewBag.Error = errorMsg;
            ViewBag.Logo = SiteSettingApplication.SiteSettings.Logo;//获取Logo
            return View();
        }

        public JsonResult AccountBalancePay(decimal balance)
        {
            if (balance <= 0)
                return Json(new Result() { success = false, msg = "应缴金额不正确" });

            var shopAccount = ShopApplication.GetShopAccount(CurrentSellerManager.ShopId);
            if (shopAccount == null)
                return Json(new Result() { success = false, msg = "店铺账户信息异常" });

            if (shopAccount.Balance < balance) //店铺余额不足以支付费用
            {
                return Json(new Result() { success = false, msg = "您的店铺余额为：" + shopAccount.Balance + "元,不足以支付此次费用，请先充值。" });
            }

            var userName = CurrentSellerManager.UserName;
            var shopId = CurrentSellerManager.ShopId;
            var accountService = _iCashDepositsService;
            if (accountService.ShopAccountRecord(shopId,balance,"充值保证金","")) {

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
            }
            return Json(new Result() { success = true });
        }
    }
}