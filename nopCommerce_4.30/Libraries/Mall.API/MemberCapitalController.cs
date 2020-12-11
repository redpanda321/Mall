using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using System;
using System.Linq;

namespace Mall.API
{
    public class MemberCapitalController : BaseApiController
    {
        private const string PLUGIN_OAUTH_WEIXIN = "Mall.Plugin.OAuth.WeiXin";

        [HttpGet("Get")]
        public object Get()
        {
            CheckUserLogin();
            var capital = MemberCapitalApplication.GetCapitalInfo(CurrentUserId);
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
            return new
            {
                success = true,
                Balance = capital.Balance,
                RedPacketAmount = redPacketAmount,
                PresentAmount =  capital.PresentAmount,
                ChargeAmount = capital.ChargeAmount,
                WithDrawMinimum = sitesetting.WithDrawMinimum,
                WithDrawMaximum = sitesetting.WithDrawMaximum,
                canWithDraw= canWithDraw,
                isOpen = sitesetting.IsOpenRechargePresent,
                rule = RechargePresentRuleApplication.GetRules()
            };
        }
        [HttpGet("GetList")]
        public object GetList(int pageNo = 1, int pageSize = 10)
        {
            CheckUserLogin();
            var capitalService = ServiceApplication.Create<IMemberCapitalService>();

            var query = new CapitalDetailQuery
            {
                memberId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo
            };
            var pageMode = capitalService.GetCapitalDetails(query);
            var model = pageMode.Models.ToList().Select(e => new CapitalDetailModel
            {
                Id = e.Id,
                Amount = e.Amount +  e.PresentAmount,
                PresentAmount = e.PresentAmount,
                CapitalID = e.CapitalID,
                CreateTime = e.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                SourceData = e.SourceData,
                SourceType = e.SourceType,
                Remark = e.SourceType == CapitalDetailInfo.CapitalDetailType.Brokerage ? GetBrokerageRemark(e) : e.Remark,
                PayWay = e.Remark
            });
            dynamic result = SuccessResult();
            result.rows = model;
            result.total = pageMode.Total;

            return result;
        }
        /// <summary>
        /// 获取分销备注
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet("GetBrokerageRemark")]
        string GetBrokerageRemark(CapitalDetailInfo data)
        {
            var remark = data.Remark;
            if (remark.IndexOf(',') > -1)
            {
                remark = remark.Replace("Id", "号").Split(',')[1];
            }
            return data.SourceType.ToDescription() + " " + remark;
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
            dynamic result = new Result();
            result.success = canWithDraw && (canWeiXin || canAlipay);
            result.canWeiXin = canWeiXin;
            result.canAlipay = canAlipay;
            return result;
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
            if (string.IsNullOrEmpty(para.pwd))
            {
                throw new MallException("请输入密码！");
            }
            if (para.amount <= 0)
                throw new MallException("提现金额不能小于等于0！");
            var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, para.pwd);
            var sitesetting = SiteSettingApplication.SiteSettings;
            if (para.applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode() && !sitesetting.Withdraw_AlipayEnable)
            {
                throw new MallException("不支持支付宝提现方式！");
            }
            var _iMemberCapitalService = ServiceApplication.Create<IMemberCapitalService>();
            if (!success)
            {
                throw new MallException("支付密码不对，请重新输入！");
            }
            var balance = MemberCapitalApplication.GetBalanceByUserId(CurrentUser.Id);
            if (para.amount > balance)
                throw new MallException("提现金额不能超出可用金额！");

            if (string.IsNullOrWhiteSpace(para.openid) && para.applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                var mo = MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, Entities.MemberOpenIdInfo.AppIdTypeEnum.Payment, PLUGIN_OAUTH_WEIXIN);
                if (mo != null && !string.IsNullOrWhiteSpace(mo.OpenId))
                {
                    para.openid = mo.OpenId;
                }
            }
            if (string.IsNullOrWhiteSpace(para.nickname) && para.applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode())
            {
                throw new MallException("数据异常,真实姓名不可为空！");
            }
            if (!string.IsNullOrWhiteSpace(para.openid) && para.applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                //para.openid = Core.Helper.SecureHelper.AESDecrypt(para.openid, "Mobile");
                if (!(string.IsNullOrWhiteSpace(sitesetting.WeixinAppId) || string.IsNullOrWhiteSpace(sitesetting.WeixinAppSecret)))
                {
                    string token = AccessTokenContainer.TryGetAccessToken(sitesetting.WeixinAppId, sitesetting.WeixinAppSecret);
                    var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, para.openid);
                    if (userinfo != null)
                    {
                        para.nickname = userinfo.nickname;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(para.openid))
            {
                throw new MallException("数据异常,OpenId或收款账号不可为空！");
            }

            Mall.Entities.ApplyWithdrawInfo model = new Mall.Entities.ApplyWithdrawInfo()
            {
                ApplyAmount = para.amount,
                ApplyStatus = Mall.Entities.ApplyWithdrawInfo.ApplyWithdrawStatus.WaitConfirm,
                ApplyTime = DateTime.Now,
                MemId = CurrentUser.Id,
                OpenId = para.openid,
                NickName = para.nickname,
                ApplyType = (CommonModel.UserWithdrawType)para.applyType
            };
            _iMemberCapitalService.AddWithDrawApply(model);
            return SuccessResult();
        }
    }
}