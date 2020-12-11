using Mall.Application;
using Mall.Core;
using Mall.Core.Plugins.Payment;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.SmallProgAPI.Model;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using System;
using System.Linq;


namespace Mall.SmallProgAPI
{
    public class MemberCapitalController : BaseApiController
    {

        private const string PLUGIN_OAUTH_WEIXIN = "Mall.Plugin.OAuth.WeiXin";

        [HttpGet("GetCapital")]
        public object GetCapital()
        {
            CheckUserLogin();
            var capital = MemberCapitalApplication.GetCapitalInfo(CurrentUser.Id);
            var sitesetting = SiteSettingApplication.SiteSettings;
            var redPacketAmount = 0M;
            if (capital != null)
            {
                //redPacketAmount = capital.Mall_CapitalDetail.Where(e => e.SourceType == Mall.Model.CapitalDetailInfo.CapitalDetailType.RedPacket).Sum(e => e.Amount);
                redPacketAmount = MemberCapitalApplication.GetSumRedPacket(capital.Id);
            }
            else
            {
                capital = new CapitalInfo();
            }
            bool canWithDraw = MemberApplication.CanWithdraw(CurrentUser.Id);
            return Json(new
            {
                success = true,
                Balance = capital?.Balance ?? 0,
                RedPacketAmount = redPacketAmount,
                PresentAmount =  capital?.PresentAmount ?? 0,
                ChargeAmount = capital?.ChargeAmount ?? 0,
                WithDrawMinimum = sitesetting.WithDrawMinimum,
                WithDrawMaximum = sitesetting.WithDrawMaximum,
                canWithDraw = canWithDraw,
                isOpen = sitesetting.IsOpenRechargePresent,
                rule = RechargePresentRuleApplication.GetRules()
            });
        }


        [HttpGet("GetList")]
        public object GetList(int pageNo = 1, int pageSize = 10)
        {
            CheckUserLogin();
            var query = new CapitalDetailQuery
            {
                memberId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo
            };
            var pageMode = MemberCapitalApplication.GetCapitalDetails(query);
            var model = pageMode.Models.ToList().Select(e => new CapitalDetailModel
            {
                Id = e.Id,
                Amount = e.Amount + e.PresentAmount,
                PresentAmount = e.PresentAmount,
                CapitalID = e.CapitalID,
                CreateTime = e.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                SourceData = e.SourceData,
                SourceType = e.SourceType,
                Remark = GetRemark(e),
                PayWay = e.Remark
            });

            return Json(new { rows = model, total = pageMode.Total });
        }

        private string GetRemark(CapitalDetailInfo data)
        {
            string result = "";
            if (data == null) return result;
            result = data.SourceType.ToDescription() + "(单号 " + (string.IsNullOrWhiteSpace(data.SourceData) ? data.Id.ToString() : data.SourceData) + ")";
            switch (data.SourceType)
            {
                case CapitalDetailInfo.CapitalDetailType.ChargeAmount:
                    if (data.PresentAmount > 0)
                    {
                        result = data.SourceType.ToDescription() + "，充" + data.Amount + "送" + data.PresentAmount + "元";
                    }
                    break;
                case CapitalDetailInfo.CapitalDetailType.Brokerage:
                    var remark = data.Remark;
                    if (remark.IndexOf(',') > -1)
                    {
                        remark = remark.Replace("Id", "号").Split(',')[1];
                    }
                    result = data.SourceType.ToDescription() +" "+ remark;
                    break;
            }

            return result;
        }

        /// <summary>
        /// 是否可以提现
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetCanWithDraw")]
        public object GetCanWithDraw()
        {
            CheckUserLogin();
            bool canWeiXin = false;
            bool canAlipay = false;
            var sitesetting = SiteSettingApplication.SiteSettings;
            //判断是否有微信openid
            var mo = MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, Entities.MemberOpenIdInfo.AppIdTypeEnum.Payment, PLUGIN_OAUTH_WEIXIN);
            if (mo != null && !string.IsNullOrWhiteSpace(mo.OpenId)) { canWeiXin = true; }
            //判断是否开启支付宝
            if (sitesetting.Withdraw_AlipayEnable)
            {
                canAlipay = true;
            }
            bool canWithDraw = MemberApplication.CanWithdraw(CurrentUser.Id);
            return Json(ApiResult<dynamic>(canWithDraw && (canWeiXin || canAlipay), data: new { canWeiXin = canWeiXin, canAlipay = canAlipay }));
        }
        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="nickname"></param>
        /// <param name="amount"></param>
        /// <param name="pwd"></param>
        /// <param name="applyType"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostApplyWithDraw")]
        public object PostApplyWithDraw(MemberCapitalApplyWithDrawModel para)
        {
            CheckUserLogin();
            if (para == null)
            {
                para = new MemberCapitalApplyWithDrawModel();
            }
            var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, para.pwd);
            var sitesetting = SiteSettingApplication.SiteSettings;
            if (para.applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode() && !sitesetting.Withdraw_AlipayEnable)
                return Json(ErrorResult<bool>("不支持支付宝提现方式"));

            if (!success)
                return Json(ErrorResult<bool>("支付密码不对，请重新输入"));

            var balance = MemberCapitalApplication.GetBalanceByUserId(CurrentUser.Id);
            if (para.amount > balance)
                return Json(ErrorResult<bool>("提现金额不能超出可用金额！"));

            if (para.amount <= 0)
            {
                return Json(ErrorResult<bool>("提现金额不能小于等于0！"));
            }
            if (string.IsNullOrWhiteSpace(para.openId) && para.applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                var mo = MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, Entities.MemberOpenIdInfo.AppIdTypeEnum.Payment, PLUGIN_OAUTH_WEIXIN);
                if (mo != null && !string.IsNullOrWhiteSpace(mo.OpenId))
                {
                    para.openId = mo.OpenId;
                }
            }
            if (string.IsNullOrWhiteSpace(para.nickname) && para.applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode())
            {
                return Json(ErrorResult<bool>("数据异常,真实姓名不可为空！"));
            }
            if (!string.IsNullOrWhiteSpace(para.openId) && para.applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                //para.openid = Core.Helper.SecureHelper.AESDecrypt(para.openid, "Mobile");
                if (!string.IsNullOrWhiteSpace(sitesetting.WeixinAppletId) && !string.IsNullOrWhiteSpace(sitesetting.WeixinAppletSecret))
                {
                    string token = AccessTokenContainer.TryGetAccessToken(sitesetting.WeixinAppletId, sitesetting.WeixinAppletSecret);
                    var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, para.openId);
                    if (userinfo != null)
                    {
                        para.nickname = userinfo.nickname;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(para.openId))
            {
                return Json(ErrorResult<bool>("数据异常,OpenId或收款账号不可为空！"));
            }

            Mall.Entities.ApplyWithdrawInfo model = new Mall.Entities.ApplyWithdrawInfo()
            {
                ApplyAmount = para.amount,
                ApplyStatus = Mall.Entities.ApplyWithdrawInfo.ApplyWithdrawStatus.WaitConfirm,
                ApplyTime = DateTime.Now,
                MemId = CurrentUser.Id,
                OpenId = para.openId,
                NickName = para.nickname,
                ApplyType = (CommonModel.UserWithdrawType)para.applyType
            };
            MemberCapitalApplication.AddWithDrawApply(model);
            return Json(true);
        }

        /// <summary>
        /// 预账户充值接口
        /// </summary>
        /// <param name="pluginId">支付插件Id</param>
        /// <param name="amount">充值金额</param>
        /// <returns></returns>
        /// 
        [HttpPost("PostCharge")]
        public object PostCharge(PaymentChargeModel para)
        {
            para.amount = Math.Round(para.amount, 2);
            if (para.amount <= 0)
                return Json(ErrorResult<dynamic>("请输入正确的金额"));
            if (string.IsNullOrWhiteSpace(para.openId))
            {
                return Json(ErrorResult<dynamic>("缺少OpenId"));
            }
            try
            {
                //获取支付插件
                var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.WeiXinSmallProg));

                if (mobilePayments.Any())
                {
                    var plugin = mobilePayments.Where(x => x.PluginInfo.PluginId.Contains(para.typeId)).FirstOrDefault();
                    //添加充值明细
                    var chargeDetail = new DTO.ChargeDetail();
                    chargeDetail.ChargeAmount = para.amount;
                    chargeDetail.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.WaitPay;
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
                    string returnPre = "";

                    var models = mobilePayments.ToArray().Select(item =>
                    {
                        string url = string.Empty;
                        try
                        {
                            url = item.Biz.GetRequestUrl(returnPre, notifyPre + item.PluginInfo.PluginId.Replace(".", "-") + "/", id.ToString(), para.amount, "会员充值", openId: para.openId);
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
                    if (null == model) return Json(ErrorResult<dynamic>("获取支付方式失败，请与管理员联系"));

                    return Json(model);
                }
                else
                {
                    Core.Log.Error("暂未配置支付方式");
                    return Json(ErrorResult<dynamic>("暂未配置支付方式"));
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("预账户充值报错：", ex);
                return Json(ErrorResult<dynamic>("预账户充值报错"));
            }
        }
    }
}
