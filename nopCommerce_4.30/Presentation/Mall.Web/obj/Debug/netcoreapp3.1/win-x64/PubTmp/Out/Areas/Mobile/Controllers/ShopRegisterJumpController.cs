using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class ShopRegisterJumpController : BaseMobileTemplatesController
    {
        /// <summary>
        /// APP转跳
        /// </summary>
        /// <param name="userkey"></param>
        /// <param name="toUrl"></param>
        /// <returns></returns>
        public ActionResult AppJump(string userkey,string toUrl)
        {
            var userId = UserCookieEncryptHelper.Decrypt(userkey, CookieKeysCollection.USERROLE_USER);
            if (userId > 0)
            {
                var member = MemberApplication.GetMember(userId);
                WriteLogin(member);
            }
            return View();
        }      
        
        /// <summary>
        /// 小程序转跳
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="toUrl"></param>
        /// <returns></returns>
        public ActionResult SmallProgJump(string openid,string toUrl)
        {
            var SmallProgServiceProvider = "WeiXinSmallProg";
            if (!string.IsNullOrWhiteSpace(openid))
            {
                var userInfo = Application.MemberApplication.GetMemberByOpenId(SmallProgServiceProvider, openid);
                WriteLogin(userInfo);
            }
            return Redirect(toUrl);
        }

        /// <summary>
        /// 写入登录cookie
        /// </summary>
        /// <param name="member"></param>
        private void WriteLogin(Entities.MemberInfo member)
        {
            if (member != null)
            {
                base.SetUserLoginCookie(member.Id, DateTime.MaxValue);
                Core.Helper.WebHelper.SetCookie(CookieKeysCollection.Mall_ACTIVELOGOUT, "0", DateTime.MaxValue);
                var sell = ManagerApplication.GetSellerManager(member.UserName);
                if (sell != null)
                    base.SetSellerAdminLoginCookie(sell.Id, DateTime.MaxValue);
                else
                {
                    //多个账号切换时，商家之前登录的cookie可能没有清除掉，当当前账号不存在时再清除下
                    Core.Helper.WebHelper.DeleteCookie(CookieKeysCollection.SELLER_MANAGER);
                }
            }
        }
    }
}