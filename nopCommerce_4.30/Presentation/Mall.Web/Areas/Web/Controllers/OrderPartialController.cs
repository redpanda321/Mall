using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Web.Controllers
{
    public class OrderPartialController : BaseMemberController
    {
        public ActionResult TopBar()
        {
            InitTopBar();
            return PartialView("~/Areas/Web/Views/Shared/OrderTopBar.cshtml");
        }


        void InitTopBar()
        {
            //会员信息
            ViewBag.Member = CurrentUser;
        }
    }
}