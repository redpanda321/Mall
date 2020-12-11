using Mall.API.Model;
using Mall.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Mall.API
{
    public class ShopCategoryController : BaseShopLoginedApiController
    {
        IShopCategoryService _ishopCategoryService;
        public ShopCategoryController()
        {
            _ishopCategoryService = ServiceProvider.Instance<IShopCategoryService>.Create;
        }

        /// <summary>
        /// 商家分类
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetShopCategories")]
        public object GetShopCategories()
        {
            CheckUserLogin();
            var categories = _ishopCategoryService.GetMainCategory(CurrentUser.ShopId);
            var model = categories
                .Select(item => new CategoryModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    SubCategories = GetShopSubCategories(item.Id, 1),
                    Depth = 0,
                    DisplaySequence = item.DisplaySequence
                }).OrderBy(c => c.DisplaySequence);
            return new { success = true, Category = model };
        }

        IEnumerable<CategoryModel> GetShopSubCategories(long categoryId, int depth)
        {
            var categories = _ishopCategoryService.GetCategoryByParentId(categoryId)
                .Select(item =>
                {
                    return new CategoryModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        SubCategories = GetShopSubCategories(item.Id, depth + 1),
                        Depth = 1,
                        DisplaySequence = item.DisplaySequence
                    };
                })
                   .OrderBy(c => c.DisplaySequence);
            return categories;
        }
    }
}
