using Mall.Application;
using Mall.IServices;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    public class ArticleController : BaseAdminController
    {
        private IArticleService _iArticleService;
        private IArticleCategoryService _iArticleCategoryService;
        public ArticleController(IArticleService iArticleService, IArticleCategoryService iArticleCategoryService)
        {
            _iArticleService = iArticleService;
            _iArticleCategoryService = iArticleCategoryService;
        }

        // GET: Admin/Article
        public ActionResult Management()
        {
            return View();
        }



        public ActionResult Add(long? id, long? cid = null)
        {
            ArticleModel articleModel;
            if (id.HasValue)
            {
                var article = _iArticleService.FindById(id.Value);
                articleModel = new ArticleModel()
                {
                    CategoryId = article.CategoryId,
                    Content = article.Content,
                    IconUrl = article.IconUrl,
                    Id = article.Id,
                    IsRelease = article.IsRelease,
                    Meta_Description = article.Meta_Description,
                    Meta_Keywords = article.Meta_Keywords,
                    Meta_Title = article.Meta_Title,
                    Title = article.Title,
                    ArticleCategoryFullPath = _iArticleCategoryService.GetFullPath(article.CategoryId)
                };
            }
            else
            {
                articleModel = new ArticleModel() { IsRelease = true };
                if (cid.HasValue)
                {
                    articleModel.CategoryId = cid;
                    articleModel.ArticleCategoryFullPath = _iArticleCategoryService.GetFullPath(cid.Value);
                }
            }
            return View(articleModel);
        }

       
        [HttpPost]
        public JsonResult Add(ArticleModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new Result { success = false });
            }
            Entities.ArticleInfo article = new Entities.ArticleInfo()
            {
                Title = model.Title,
                Meta_Title = model.Meta_Title,
                Meta_Keywords = model.Meta_Keywords,
                Meta_Description = model.Meta_Description,
                IsRelease = model.IsRelease,
                CategoryId = model.CategoryId.GetValueOrDefault(),
                Content = model.Content,
                IconUrl = model.IconUrl,
                Id = model.Id
            };
            if (article.Id > 0)
                _iArticleService.UpdateArticle(article);
            else
                _iArticleService.AddArticle(article);
            return Json(new Result { success = true });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(long? categoryId, string titleKeyWords, int rows, int page, string Sort, bool? isAsc)
        {
            var articles = _iArticleService.Find(categoryId, titleKeyWords, rows, page, Sort: Sort, isAsc: isAsc);
            string host = CurrentUrlHelper.CurrentUrl();
            string urltmp = string.Format(@"{0}{1}/", host, Url.Action("Details", "Article", new { Area = "Web" }));

            ////如果未开启PC端授权，则只保留平台公告分类数据(FG:无平台公告分类)
            //if (!SiteSettingApplication.SiteSettings.IsOpenPC )
            //{
            //    articles.Models.RemoveAll(p => !p.ArticleCategoryInfo.Name.Contains("平台公告"));
            //}
            var categories = _iArticleCategoryService.GetCategories();
            var articleModels = articles.Models.Select(item => new
            {
                id = item.Id,
                categoryId = item.CategoryId,
                categoryName = categories.FirstOrDefault(p=>p.Id==item.CategoryId)?.Name??string.Empty,
                isShow = item.IsRelease,
                title = item.Title,
                displaySequence = item.DisplaySequence,
                showurl = urltmp + item.Id.ToString(),
                addDate = item.AddDate.ToString("yyyy-MM-dd HH:mm:ss")
            });
            var data = new { rows = articleModels, total = articles.Total };
            return Json(data);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult Delete(long id)
        {
            _iArticleService.DeleteArticle(id);
            return Json(new Result { success = true });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchDelete(string ids)
        {
            long[] ids_long = ids.Split(',').Select(item => long.Parse(item)).ToArray();
            _iArticleService.DeleteArticle(ids_long);
            return Json(new Result { success = true });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateDisplaySequence(long id, long displaySequence)
        {
            _iArticleService.UpdateArticleDisplaySequence(id, displaySequence);
            return Json(new Result { success = true });
        }
    }
}