using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    public class AdminPublicController : BaseAdminController
    {
        public ActionResult Top()
        {
            //var t = ConfigurationManager.AppSettings["IsInstalled"];


            //    if (!(null == t || bool.Parse(t)))

            bool t = configuration.GetValue<bool>("Mall:IsInstalled");

            if(!t)
            {
                return RedirectToAction("Agreement", "Installer", new { area = "Web" });
            }
            return View();
        }

   
        public ActionResult Bottom()
        {
            ViewBag.Rights  = string.Join(",", CurrentManager.AdminPrivileges.Select(a => (int)a).OrderBy(a => a));
            return View();
        }

    }
}