using Mall.Application;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Web.Controllers
{

    [Area("Web")]
    public class RegisterActivityController : BaseController
    {
      
        /// <summary>
        /// 注册有礼
        /// </summary>
        /// <returns></returns>
        public ActionResult Gift()
        {
            if (IsMobileTerminal)
            {
                Response.Redirect("/m-Wap/RegisterActivity/Gift");
            }
            var model = CouponApplication.GetCouponSendByRegister();
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(model);
        }
    }
}