using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class PublicInfrastructureController : BaseMemberController
    {
        private ICategoryService _iCategoryService;
        public PublicInfrastructureController(ICategoryService iCategoryService)
        {
            _iCategoryService = iCategoryService;
        }

        // GET: Web/PublicInfrastructure
        public ActionResult Allbrand()
        {
            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }

        private List<CategoryJsonModel> GetCategoryJson()
        {
            var categories = _iCategoryService.GetFirstAndSecondLevelCategories();
            var json = new List<CategoryJsonModel>();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id))
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                        SubCategory = new List<ThirdLevelCategoty>()
                    };
                    var thridCates = _iCategoryService.GetCategoryByParentId(secondItem.Id);
                    foreach (var thrid in thridCates)
                    {
                        var thridC = new ThirdLevelCategoty()
                        {
                            Name = thrid.Name,
                            Id = thrid.Id.ToString()
                        };
                        secondC.SubCategory.Add(thridC);
                    }
                    topC.SubCategory.Add(secondC);
                }
                json.Add(topC);
            }
            return json;
        }

        public ActionResult AllCategory()
        {
            var cate = GetCategoryJson();
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(cate);
        }

    }
}