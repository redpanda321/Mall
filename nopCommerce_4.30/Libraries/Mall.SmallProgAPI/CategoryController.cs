using Mall.Application;
using Mall.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.SmallProgAPI
{
    public class CategoryController : BaseApiController
    {

        [HttpGet("GetAllCategories")]
        public object GetAllCategories()
        {
            var categories = CategoryApplication.GetMainCategory();
            var model = categories.Where(p => p.IsShow).OrderBy(c => c.DisplaySequence).Select(c => new
            {
                cid = c.Id,
                name = c.Name,
                subs = CategoryApplication.GetCategoryByParentId(c.Id).Select(a => new
                {
                    cid = a.Id,
                    name = a.Name
                })
            }).ToList();
            var result = SuccessResult<dynamic>(data: model);
            return Json(result);
        }
    }
}
