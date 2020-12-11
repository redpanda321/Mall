using Mall.Application;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Mobile.Controllers
{
    public class CategoryController : BaseMobileTemplatesController
	{
		// GET: Mobile/Category
		public ActionResult Index()
		{
			var model = CategoryApplication.GetSubCategories();
			return View(model);
		}
	}
}