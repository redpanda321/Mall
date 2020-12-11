using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class ArticleController : BaseWebController
    {
        IArticleCategoryService _iArticleCategoryService;
        IArticleService _iArticleService;
        public ArticleController(IArticleCategoryService iArticleCategoryService, IArticleService iArticleService)
        {
            _iArticleCategoryService = iArticleCategoryService;
            _iArticleService = iArticleService;
        }

        private List<CategoryJsonModel> GetArticleCate()
        {
            List<CategoryJsonModel> articleCate = new List<CategoryJsonModel>();
            #region 文章分类

            var categories = _iArticleCategoryService.GetCategories().ToArray();
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
                    };

                    topC.SubCategory.Add(secondC);
                }
                articleCate.Add(topC);
            }
            return articleCate;
            #endregion
        }

        private string GetCateNameBySecond(long id)
        {
            var pid = _iArticleCategoryService.GetCategories().FirstOrDefault(c => c.Id.Equals(id)).ParentCategoryId;
            return _iArticleCategoryService.GetArticleCategory(pid).Name;
        }

        // GET: Web/Article
        public ActionResult Category(string id = "1", int pageNo = 1)
        {
            int pageSize = 20;
            long cid = 0;
            if (!long.TryParse(id, out cid))
            {
            }
            var articles = _iArticleService.Find(cid, "", pageSize, pageNo, isShowAll:false);
            List<CategoryJsonModel> cate = GetArticleCate();
            var currCate = cate.FirstOrDefault(c => c.Id == cid.ToString());
            ViewBag.Cate = cate;

            ViewBag.ArticleCateId = cid;
            ViewBag.Articles = articles.Models;

            ViewBag.FirstPath = currCate != null ? currCate.Name :
                GetCateNameBySecond(cid);
            ViewBag.SecondPath = currCate == null ? _iArticleCategoryService.GetArticleCategory(cid).Name : "";

            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = articles.Total
            };
            ViewBag.pageInfo = info;
            #endregion

            ViewBag.Keyword = SiteSettings.Keyword;
            return View();
        }

        public ActionResult Details(long id)
        {
            //TODO:FG 异常查询 MysqlExecuted: 125,耗时: 750.0168毫秒 /Article/Details/92 
            ViewBag.Cate = GetArticleCate();
            var model = _iArticleService.GetArticle(id);
            var category = _iArticleCategoryService.GetArticleCategory(model.CategoryId);

            ViewBag.ArticleCateId = model.CategoryId;
            ViewBag.FirstPath = category.ParentCategoryId == 0 ? category.Name : _iArticleCategoryService.GetArticleCategory(category.ParentCategoryId).Name;
            ViewBag.SecondPath = category.ParentCategoryId != 0 ? category.Name : "";
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(model);
        }
    }
}