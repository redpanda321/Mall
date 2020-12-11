using System.Configuration;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using Mall.Web.Framework;
using System.Threading.Tasks;
using Mall.Entities;
using Mall.CommonModel;

namespace Mall.Web.Areas.Admin.Components
{
    public class  BottomAdminViewComponent : ViewComponent
    {
        
       
        public async  Task<IViewComponentResult> InvokeAsync(IPaltManager CurrentManager)
        {
            ViewBag.Rights  = string.Join(",", CurrentManager.AdminPrivileges.Select(a => (int)a).OrderBy(a => a));
            return View();
        }

    }
}