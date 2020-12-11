using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mall.Service
{
    public class ArticleService : ServiceBase, IArticleService
    {
        public void AddArticle(ArticleInfo article)
        {

            article.AddDate = DateTime.Now;
            article.DisplaySequence = 0;
            //Context.ArticleInfo.Add(article);
            //Context.SaveChanges();
            DbFactory.Default.Add(article);
            string url = TransferImage(article.IconUrl, article.Id);
            article.IconUrl = url;

            string imageRealtivePath = "/Storage/Plat/Article/" + article.Id;
            article.IconUrl = TransferImage(article.IconUrl, article.Id);
            article.Content = HTMLProcess(article.Content, article.Id);//转移外站图片，去除script脚本,防止注入
            //Context.SaveChanges();
            DbFactory.Default.Update(article);

        }

        public void UpdateArticle(ArticleInfo article)
        {
            //var localArticle = Context.ArticleInfo.FirstOrDefault(p => p.Id == article.Id);
            var localArticle = DbFactory.Default.Get<ArticleInfo>().Where(p => p.Id == article.Id).FirstOrDefault();
            localArticle.CategoryId = article.CategoryId;
            localArticle.IconUrl = article.IconUrl;
            localArticle.IsRelease = article.IsRelease;
            localArticle.Meta_Description = article.Meta_Description;
            localArticle.Meta_Keywords = article.Meta_Keywords;
            localArticle.Meta_Title = article.Meta_Title;
            localArticle.Title = article.Title;
            localArticle.IconUrl = TransferImage(localArticle.IconUrl, article.Id);
            localArticle.Content = HTMLProcess(article.Content, article.Id);//转移外站图片，去除script脚本,防止注入
            //Context.SaveChanges();
            DbFactory.Default.Update(localArticle);
        }


        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        string HTMLProcess(string content, long id)
        {
            string imageRealtivePath = "/Storage/Plat/Article/" + id;
            content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.MallIO.GetImagePath(imageRealtivePath) + "/");
            content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            return content;
        }


        /// <summary>
        /// 图片转移
        /// </summary>
        /// <param name="tempUrl">临时图片地址</param>
        /// <returns>实际图片路径（相对于网站根目录）</returns>
        string TransferImage(string tempUrl, long id)
        {
            if (!string.IsNullOrWhiteSpace(tempUrl) && !tempUrl.Contains("/Storage/"))
            {
                string dir = "/Storage/Plat/Article/";
                dir = dir + id;


                string ext = tempUrl.Substring(tempUrl.LastIndexOf('.'));
                //return dir + "/image" + ext;

                string resulturl = tempUrl;
                if (!string.IsNullOrWhiteSpace(resulturl))
                {
                    if (resulturl.Contains("/temp/"))
                    {
                        string logoname = resulturl.Substring(resulturl.LastIndexOf('/') + 1);
                        string oldlogo = resulturl.Substring(resulturl.LastIndexOf("/temp"));
                        string newLogo = dir + logoname;
                        Core.MallIO.CopyFile(oldlogo, newLogo, true);
                        resulturl = newLogo;
                    }
                    else if (resulturl.Contains("/Storage/"))
                    {
                        resulturl = resulturl.Substring(resulturl.LastIndexOf("/Storage"));
                    }
                }
                return resulturl;
            }
            else
                return tempUrl;
        }



        public void DeleteArticle(params long[] ids)
        {
            //var artiles = Context.ArticleInfo.FindBy(item => ids.Contains(item.Id));
            //Context.ArticleInfo.RemoveRange(artiles);
            //Context.SaveChanges();
            DbFactory.Default.Del<ArticleInfo>(p => p.Id.ExIn(ids));
            foreach (long id in ids)
            {
                string path = "/Storage/Plat/Article/" + id;
                if (MallIO.ExistDir(path))
                    MallIO.DeleteDir(path, true);
            }

        }

        public QueryPageModel<ArticleInfo> Find(long? articleCategoryId, string titleKeyWords, int pageSize, int pageNumber, bool isShowAll = true, string Sort = "", bool? isAsc = false)
        {
            //var articles = (from p in Context.ArticleInfo
            //                join o in Context.ArticleCategoryInfo on p.CategoryId equals o.Id
            //                select p
            //                );
            var articles = DbFactory.Default.Get<ArticleInfo>().LeftJoin<ArticleCategoryInfo>((ai, aci) => ai.CategoryId == aci.Id);
            if (!isShowAll) articles.Where(d => d.IsRelease == true);
            if (articleCategoryId.HasValue)
            {
                var categoryIds = (new ArticleCategoryService()).GetArticleCategoriesByParentId(articleCategoryId.Value, true).Select(item => item.Id).ToList();
                categoryIds.Add(articleCategoryId.Value);
                //articles = articles.Where(item => categoryIds.Contains(item.CategoryId));
                articles.Where(item => item.CategoryId.ExIn(categoryIds));
            }
            if (!string.IsNullOrWhiteSpace(titleKeyWords))
                articles.Where(item => item.Title.Contains(titleKeyWords));
            var pageModel = new QueryPageModel<ArticleInfo>();
            //int total;
            //pageModel.Models = articles.FindBy(item => true, pageNumber, pageSize, out total, item => item.Id, false).ToList();
            if (!string.IsNullOrWhiteSpace(Sort))
            {
                Sort = Sort.ToLower();
            }
            switch (Sort)
            {
                case "displaysequence":
                    if (isAsc==true)
                    {
                        articles = articles.OrderBy(item => item.DisplaySequence);
                    }
                    else
                    {
                        articles = articles.OrderByDescending(item => item.DisplaySequence);
                    }
                    break;
                case "adddate":
                    if (isAsc == true)
                    {
                        articles = articles.OrderBy(item => item.AddDate);
                    }
                    else
                    {
                        articles = articles.OrderByDescending(item => item.AddDate);
                    }
                    break;
                default:
                    articles = articles.OrderByDescending(item => item.DisplaySequence).OrderByDescending(item => item.Id);
                    break;
            }
            var models = articles.Select().ToPagedList(pageNumber, pageSize);
            pageModel.Models = models;
            pageModel.Total = models.TotalRecordCount;

            return pageModel;
        }


        public ArticleInfo FindById(long id)
        {
            //return Context.ArticleInfo.FirstOrDefault(p => p.Id == id);
            return DbFactory.Default.Get<ArticleInfo>().Where(p => p.Id == id).FirstOrDefault();
        }


        public void UpdateArticleDisplaySequence(long id, long displaySequence)
        {
            //var article = Context.ArticleInfo.FirstOrDefault(p => p.Id == id);
            var article = DbFactory.Default.Get<ArticleInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (article == null)
                throw new MallException("未找到id为" + id + "的文章");
            //article.DisplaySequence = displaySequence;
            //Context.SaveChanges();
            DbFactory.Default.Set<ArticleInfo>().Set(n => n.DisplaySequence, displaySequence).Where(p => p.Id == id).Succeed();
        }


        public List<ArticleInfo> GetTopNArticle<T>(int num, long categoryId)
        {
            var cate = DbFactory.Default.Get<ArticleCategoryInfo>().Where(a => a.ParentCategoryId == categoryId).Select(a => a.Id);
            var query = DbFactory.Default.Get<ArticleInfo>().Where(a => (a.CategoryId == categoryId || a.CategoryId.ExIn(cate)) && a.IsRelease == true);
            query.OrderByDescending(d=>d.DisplaySequence).OrderByDescending(d=>d.Id).Take(num);
            return query.ToList();
        }

        public List<ArticleInfo> GetArticleByArticleCategoryId(long articleCategoryId, bool isShowAll = true)
        {
            var articles = DbFactory.Default.Get<ArticleInfo>().Where(item => item.CategoryId == articleCategoryId);
            if (!isShowAll) articles.Where(d => d.IsRelease == true);
            articles.OrderByDescending(d => d.DisplaySequence).OrderByDescending(d => d.Id);
            return articles.ToList();
        }
        public List<ArticleInfo> GetArticleByArticleCategoryIds(List<long> articleCategoryIds, bool isShowAll = true)
        {
            var articles = DbFactory.Default.Get<ArticleInfo>().Where(item => item.CategoryId.ExIn(articleCategoryIds));
            if (!isShowAll) articles.Where(d => d.IsRelease == true);
            articles.OrderByDescending(d => d.DisplaySequence).OrderByDescending(d => d.Id);
            return articles.ToList();
        }

        public ArticleInfo GetArticle(long id)
        {
            return DbFactory.Default.Get<ArticleInfo>().Where(p => p.Id == id).FirstOrDefault();
        }
    }
}
