using Mall.Application;
using Mall.Core;
using Mall.Core.Plugins.Payment;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using System;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class CapitalController : BaseMobileMemberController
    {
        IMemberCapitalService _iMemberCapitalService;
        private const string PLUGIN_OAUTH_WEIXIN = "Mall.Plugin.OAuth.WeiXin";


        private IHttpContextAccessor _httpContextAccessor;
        public CapitalController(IMemberCapitalService iMemberCapitalService, IHttpContextAccessor httpContextAccessor)
        {
            _iMemberCapitalService = iMemberCapitalService;

            this._httpContextAccessor = httpContextAccessor;
        }
        // GET: Mobile/Capital
        public ActionResult Index()
        {
            CapitalIndexChargeModel result = new CapitalIndexChargeModel();
            //判断是否需要跳转到支付地址
            if (this.Request.GetDisplayUrl().EndsWith("/Capital/Index", StringComparison.OrdinalIgnoreCase) || this.Request.GetDisplayUrl().EndsWith("/Capital", StringComparison.OrdinalIgnoreCase))
                return Redirect(Url.RouteUrl("PayRoute") + "?area=mobile&platform=" + this.PlatformType.ToString() + "&controller=Capital&action=Index");

            var model = MemberCapitalApplication.GetCapitalInfo(CurrentUser.Id);
            var redPacketAmount = 0M;
            if (model != null)
            {
                //redPacketAmount = model.Mall_CapitalDetail.Where(e => e.SourceType == Model.CapitalDetailInfo.CapitalDetailType.RedPacket).Sum(e => e.Amount);
                redPacketAmount = MemberCapitalApplication.GetSumRedPacket(model.Id);
                result.CapitalDetails = MemberCapitalApplication.GetTopCapitalDetailList(model.Id, 15);
            }
            else
            {
                model = new CapitalInfo
                {
                    Balance = 0,
                    ChargeAmount = 0,
                    FreezeAmount = 0,
                    MemId = CurrentUser.Id,
                    PresentAmount = 0
                };
            }
            result.UserCaptialInfo = model;
            result.IsEnableRechargePresent = SiteSettings.IsOpenRechargePresent;
            if (result.IsEnableRechargePresent)
            {
                result.RechargePresentRules = RechargePresentRuleApplication.GetRules();
            }

            result.RedPacketAmount = redPacketAmount;
            result.IsSetPwd = string.IsNullOrWhiteSpace(CurrentUser.PayPwd) ? false : true;
            var siteSetting = SiteSettingApplication.SiteSettings;
            result.WithDrawMinimum = siteSetting.WithDrawMinimum;
            result.WithDrawMaximum = siteSetting.WithDrawMaximum;
            result.CanWithDraw = MemberApplication.CanWithdraw(CurrentUser.Id);
            return View(result);
        }
        public JsonResult List(int page, int rows)
        {
            var capitalService = _iMemberCapitalService;

            var query = new CapitalDetailQuery
            {
                memberId = CurrentUser.Id,
                PageSize = rows,
                PageNo = page
            };
            var pageMode = capitalService.GetCapitalDetails(query);
            var model = pageMode.Models.ToList().Select(e => new CapitalDetailModel
            {
                Id = e.Id,
                Amount = e.Amount,
                CapitalID = e.CapitalID,
                CreateTime = e.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                SourceData = e.SourceData,
                SourceType = e.SourceType,
                Remark = e.SourceType== CapitalDetailInfo.CapitalDetailType.Brokerage? GetBrokerageRemark(e) : e.Remark,
                PayWay = e.Remark,
                PresentAmount = e.PresentAmount,
                SourceTypeName = e.SourceType.ToDescription()
            });

            return Json(new { model = model, total = pageMode.Total });
        }
        /// <summary>
        /// 获取分销备注
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string GetBrokerageRemark(CapitalDetailInfo data)
        {
            var remark = data.Remark;
            if (remark.IndexOf(',') > -1)
            {
                remark = remark.Replace("Id", "号").Split(',')[1];
            }
            return data.SourceType.ToDescription() + " " + remark;
        }
        public JsonResult SetPayPwd(string pwd)
        {
            _iMemberCapitalService.SetPayPwd(CurrentUser.Id, pwd);
            return Json(new { success = true, msg = "设置成功" });
        }
        /// <summary>
        /// 是否可以提现
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CanWithDraw()
        {
            bool canWeiXin = false;
            bool canAlipay = false;
            if (PlatformType == PlatformType.WeiXin)
            {
                canWeiXin = true;
            }
            else
            {
                //判断是否有微信openid
                //var mo = MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, MemberOpenIdInfo.AppIdTypeEnum.Payment, PLUGIN_OAUTH_WEIXIN);
                //if (mo != null && !string.IsNullOrWhiteSpace(mo.OpenId)) { canWeiXin = true;}
            }
            //判断是否开启支付宝
            if (SiteSettings.Withdraw_AlipayEnable)
            {
                canAlipay = true;
            }
            return Json(new { success = canWeiXin || canAlipay, canWeiXin = canWeiXin, canAlipay = canAlipay });
        }
        public JsonResult ApplyWithDrawSubmit(string openid, string nickname, decimal amount, string pwd, int applyType = 1)
        {
            var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, pwd);
            if (!success)
            {
                throw new MallException("支付密码不对，请重新输入！");
            }
            if (applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode() && !SiteSettings.Withdraw_AlipayEnable)
            {
                throw new MallException("不支持支付宝提现方式！");
            }

            var balance = MemberCapitalApplication.GetBalanceByUserId(UserId);
            if (amount > balance)
            {
                throw new MallException("提现金额不能超出可用金额！");
            }
            if (amount <= 0)
            {
                throw new MallException("提现金额不能小于等于0！");
            }
            if (string.IsNullOrWhiteSpace(openid) && applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                openid = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.Mall_USER_OpenID);
            }
            if (string.IsNullOrWhiteSpace(nickname) && applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode())
            {
                throw new MallException("数据异常,真实姓名不可为空！");
            }
            if (!string.IsNullOrWhiteSpace(openid) && applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                openid = Core.Helper.SecureHelper.AESDecrypt(openid, "Mobile");
                var siteSetting = SiteSettingApplication.SiteSettings;
                if (!(string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret)))
                {
                    string token = AccessTokenContainer.TryGetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                    var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, openid);
                    if (userinfo != null)
                    {
                        nickname = userinfo.nickname;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(openid))
            {
                throw new MallException("数据异常,OpenId或收款账号不可为空！");
            }

            Mall.Entities.ApplyWithdrawInfo model = new Mall.Entities.ApplyWithdrawInfo()
            {
                ApplyAmount = amount,
                ApplyStatus = Mall.Entities.ApplyWithdrawInfo.ApplyWithdrawStatus.WaitConfirm,
                ApplyTime = DateTime.Now,
                MemId = CurrentUser.Id,
                OpenId = openid,
                NickName = nickname,
                ApplyType = (CommonModel.UserWithdrawType)applyType
            };
            _iMemberCapitalService.AddWithDrawApply(model);
            return Json(new { success = true });
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Charge(string pluginId, decimal amount, bool ispresent = false)
        {
            amount = Math.Round(amount, 2);
            if (amount <= 0)
                return Json(new { success = false, msg = "请输入正确的金额" });

            var plugin = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(pluginId);

            var chargeDetail = new DTO.ChargeDetail();
            chargeDetail.ChargeAmount = amount;
            chargeDetail.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.WaitPay;
            chargeDetail.ChargeWay = PaymentApplication.GetForeGroundPaymentName(plugin.PluginInfo.Description);
            chargeDetail.CreateTime = DateTime.Now;
            chargeDetail.MemId = CurrentUser.Id;
            if (ispresent && SiteSettings.IsOpenRechargePresent)
            {
                var rule = RechargePresentRuleApplication.GetRules().FirstOrDefault(d => d.ChargeAmount == amount);
                if (rule != null)
                {
                    chargeDetail.PresentAmount = rule.PresentAmount;
                }
            }
            var id = MemberCapitalApplication.AddChargeApply(chargeDetail);

            string openId = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.Mall_USER_OpenID);
            if (!string.IsNullOrWhiteSpace(openId))
            {
                openId = Core.Helper.SecureHelper.AESDecrypt(openId, "Mobile");
            }
            else
            {
                var openUserInfo = Application.MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, Entities.MemberOpenIdInfo.AppIdTypeEnum.Payment);
                if (openUserInfo != null)
                    openId = openUserInfo.OpenId;
            }

            string webRoot = Request.Scheme + "://" + Request.Host.ToString();
            string notifyUrl = webRoot + "/m-" + PlatformType + "/Payment/CapitalChargeNotify/" + plugin.PluginInfo.PluginId.Replace(".", "-") + "/";
            string returnUrl = webRoot + "/m-" + PlatformType + "/Capital/Index";
            var requestUrl = plugin.Biz.GetRequestUrl(returnUrl, notifyUrl, id.ToString(), amount, "会员充值", openId);
            return Json(new
            {
                href = requestUrl,
                success = true
            });
        }

        public ActionResult ChargeSuccess(string id)
        {
            Log.Info("pluginId:" + id);
            var plugin = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id.Replace("-", "."));
            var payInfo = plugin.Biz.ProcessNotify(_httpContextAccessor);
            if (payInfo != null)
            {

                var chargeApplyId = payInfo.OrderIds.FirstOrDefault();
                Log.Info("chargeApplyId:" + chargeApplyId);
                MemberCapitalApplication.ChargeSuccess(chargeApplyId);
                var response = plugin.Biz.ConfirmPayResult();
                return Content(response);
            }
            Log.Info("payInfo:为空");
            return Content(string.Empty);
        }
    }
}