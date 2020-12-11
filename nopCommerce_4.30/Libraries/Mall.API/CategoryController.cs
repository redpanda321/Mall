using Mall.API.Model;
using Mall.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Mall.API
{
    public class CategoryController : BaseApiController
    {
        /// <summary>
        /// 平台分类
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetCategories")]
        public object GetCategories()
        {
            var categories = ServiceProvider.Instance<ICategoryService>.Create.GetCategories();
            var model = categories
                .Where(item => item.ParentCategoryId == 0 && item.IsShow)
                .Select(item => new CategoryModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    SubCategories = GetSubCategories(categories, item.Id, 1),
                    Depth = 0,
                    DisplaySequence = item.DisplaySequence
                }).OrderBy(c => c.DisplaySequence);
            return new { success = true, Category = model };
        }

        [HttpGet("GetSubCategories")]
        IEnumerable<CategoryModel> GetSubCategories(IEnumerable<Entities.CategoryInfo> allCategoies, long categoryId, int depth)
        {
            var categories = allCategoies
                .Where(item => item.ParentCategoryId == categoryId && item.IsShow)
                .Select(item =>
                {
                    string image = string.Empty;
                    if (depth == 2)
                    {
                        //image ="http://" + Url.Request.RequestUri.Host + item.Icon;
                        if (!string.IsNullOrWhiteSpace(item.Icon))
                            image = Core.MallIO.GetRomoteImagePath(item.Icon);
                    }
                    return new CategoryModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Image = image,
                        SubCategories = GetSubCategories(allCategoies, item.Id, depth + 1),
                        Depth = 1,
                        DisplaySequence = item.DisplaySequence
                    };
                })
                   .OrderBy(c => c.DisplaySequence);
            return categories;
        }
    }
}
