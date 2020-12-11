using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Web.Controllers
{

    [Area("Web")]
    public class ErrorController : BaseController
    {
        // GET: Web/Common
        public ActionResult Error404()
        {
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }

        public ActionResult DefaultError()
        {
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }
    }
}