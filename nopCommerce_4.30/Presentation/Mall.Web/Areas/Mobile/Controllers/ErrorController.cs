using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Mobile.Controllers
{
    public class ErrorController : BaseMobileTemplatesController
    {
        // GET: Mobile/Error
        public ActionResult Error()
        {
            return View();
        }
    }
}