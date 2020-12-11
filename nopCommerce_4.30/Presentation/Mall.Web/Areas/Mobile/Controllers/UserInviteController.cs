using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class UserInviteController : BaseMobileMemberController
    {
        private IMemberInviteService _iMemberInviteService;
        private IMemberIntegralService _iMemberIntegralService;
        public UserInviteController(IMemberInviteService iMemberInviteService,IMemberIntegralService iMemberIntegralService)
        {
            _iMemberInviteService = iMemberInviteService;
            _iMemberIntegralService = iMemberIntegralService;
        }
        public ActionResult Index()
        {
            var userId = CurrentUser.Id;
            var model = _iMemberInviteService.GetMemberInviteInfo(userId);
            var rule = _iMemberInviteService.GetInviteRule();
            var Integral = _iMemberIntegralService.GetIntegralChangeRule() ;
            if (Integral != null && Integral.IntegralPerMoney > 0)
            {
                ViewBag.IntergralMoney = (rule.InviteIntegral /Integral.IntegralPerMoney).ToString("f2");
            }
            string host = CurrentUrlHelper.CurrentUrlNoPort();
            model.InviteLink = String.Format("{0}/Register/index/{1}", host, userId);
            //rule.ShareIcon = string.Format("http://{0}{1}", host, rule.ShareIcon);
            rule.ShareIcon = !string.IsNullOrWhiteSpace(rule.ShareIcon) ? MallIO.GetRomoteImagePath(rule.ShareIcon) : "";
            var map = Core.Helper.QRCodeHelper.Create(model.InviteLink);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            model.QR = strUrl;
            ViewBag.WeiXin = PlatformType == PlatformType.WeiXin;
            var m = new Tuple<UserInviteModel, Entities.InviteRuleInfo, Entities.MemberInfo>(model, rule, CurrentUser);
            return View(m);
        }
    }
}