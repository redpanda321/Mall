using System.Linq;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Mall.CommonModel;

namespace Mall.Web.Areas.SellerAdmin.Components
{
    public class BottomViewComponent : ViewComponent
    {

        
        public async Task<IViewComponentResult> InvokeAsync(ISellerManager CurrentSellerManager)
        {
            
            ViewBag.Rights = string.Join(",", CurrentSellerManager.SellerPrivileges.Select(a => (int)a).OrderBy(a => a));

            return View();
        }

        
        
    }
}