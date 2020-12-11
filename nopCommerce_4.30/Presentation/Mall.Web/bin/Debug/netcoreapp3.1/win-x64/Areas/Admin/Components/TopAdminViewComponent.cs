using System.Configuration;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using Mall.Web.Framework;
using System.Threading.Tasks;

namespace Mall.Web.Areas.Admin.Components
{
    public class TopAdminViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult>  InvokeAsync()
        {
           
            return View();
        }

       
        

    }
}