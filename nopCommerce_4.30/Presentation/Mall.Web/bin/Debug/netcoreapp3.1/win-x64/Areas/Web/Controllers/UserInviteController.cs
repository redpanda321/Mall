using Mall.DTO;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;


namespace Mall.Web.Areas.Web.Controllers
{
    public class UserInviteController : BaseMemberController
    {
        private IMemberInviteService _iMemberInviteService;
        public UserInviteController(IMemberInviteService iMemberInviteService)
        {
            _iMemberInviteService = iMemberInviteService;
        }

        public ActionResult Index()
        {
            var userId = CurrentUser.Id;
            var model = _iMemberInviteService.GetMemberInviteInfo(userId);
            var rule = _iMemberInviteService.GetInviteRule();

            string host = Request.Host.ToString();
            string scheme = Request.Scheme;
            //host += Request.Url.Port != 80 ? ":"+Request.Url.Port.ToString() : "";
              model.InviteLink = String.Format("{0}://{1}/Register/index/{2}", scheme, host, userId);
              var map = Core.Helper.QRCodeHelper.Create(model.InviteLink);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            model.QR = strUrl;
            var  m = new Tuple<UserInviteModel, Entities.InviteRuleInfo, Entities.MemberInfo>(model, rule,CurrentUser);
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(m);
        }
    }
}