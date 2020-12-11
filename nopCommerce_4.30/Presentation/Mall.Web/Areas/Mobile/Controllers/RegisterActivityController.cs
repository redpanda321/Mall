using Mall.Application;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class RegisterActivityController : BaseMobileTemplatesController
    {
        const string CHECK_CODE_KEY = "checkCode";

        private IMemberInviteService _iMemberInviteService;
        private IMemberIntegralService _iMemberIntegralService;
        private IMessageService _iMessageService;
        public RegisterActivityController(
            IMemberService iMemberService,
            IMemberInviteService iMemberInviteService,
            IMemberIntegralService iMemberIntegralService,
            IMemberIntegralConversionFactoryService iMemberIntegralConversionFactoryService,
            IBonusService iBonusService,
            IMessageService iMessageService
            )
        {
            _iMessageService = iMessageService;
            _iMemberInviteService = iMemberInviteService;
            _iMemberIntegralService = iMemberIntegralService;
        }

        #region 注册有礼
        /// <summary>
        /// 注册有礼
        /// </summary>
        /// <returns></returns>
        public ActionResult Gift()
        {
            if (!IsMobileTerminal)
            {
                Response.Redirect("/RegisterActivity/Gift");
            }
            var model = CouponApplication.GetCouponSendByRegister();
            if (model != null && model.Status.Equals(Mall.CommonModel.CouponSendByRegisterStatus.Open) && model.total > 0) { }
            else
                Response.Redirect("end");
            return View(model);
        }

        /// <summary>
        /// 老用户
        /// </summary>
        /// <returns></returns>
        public ActionResult Share()
		{
			var userId = base.UserId;
            if (userId != 0)
            {
                var model = _iMemberInviteService.GetMemberInviteInfo(userId);
                var rule = _iMemberInviteService.GetInviteRule();
                var Integral = _iMemberIntegralService.GetIntegralChangeRule();
                if (Integral != null && Integral.IntegralPerMoney > 0)
                {
                    ViewBag.IntergralMoney = (rule.InviteIntegral / Integral.IntegralPerMoney).ToString("f2");
                }
                string host = CurrentUrlHelper.CurrentUrlNoPort();
                model.InviteLink = String.Format("{0}/Register/index/{1}", host, userId);
                //rule.ShareIcon = string.Format("http://{0}{1}", host, rule.ShareIcon);
                //var map = Core.Helper.QRCodeHelper.Create(model.InviteLink);
                //MemoryStream ms = new MemoryStream();
                //map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                ////  将图片内存流转成base64,图片以DataURI形式显示  
                //string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
                //ms.Dispose();
                //model.QR = strUrl;
                var m = new Tuple<UserInviteModel, Entities.InviteRuleInfo, Entities.MemberInfo>(model, rule, CurrentUser);

                return View(m);
            }
            else
            {
                Response.Redirect("/m-Wap/Login/Entrance?returnUrl=" + HttpUtility.UrlEncode(Request.GetDisplayUrl().ToString()));
                return View();
            }
        }

        /// <summary>
        /// 活动结束
        /// </summary>
        /// <returns></returns>
        public ActionResult End()
        {
            return View();
        }
        #endregion
    }
}