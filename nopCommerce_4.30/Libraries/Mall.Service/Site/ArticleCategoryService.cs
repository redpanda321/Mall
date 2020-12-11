using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mall.Service
{
    public class ArticleCategoryService : ServiceBase, IArticleCategoryService
    {
        public List<ArticleCategoryInfo> GetArticleCategoriesByParentId(long parentId, bool recursive = false)
        {
            //var articleCategories = Context.ArticleCategoryInfo.Where(item => item.ParentCategoryId == parentId);
            var articleCategories = DbFactory.Default.Get<ArticleCategoryInfo>().Where(item => item.ParentCategoryId == parentId).ToList();
            if (recursive)
            {
                var subIds = articleCategories.Select(item => item.Id).ToList<long>();
                foreach (var subId in subIds)
                    articleCategories.AddRange(GetArticleCategoriesByParentId(subId, true));
            }
            articleCategories = articleCategories.OrderBy(d=>d.DisplaySequence).ToList();
            return articleCategories;
        }

        public void AddArticleCategory(ArticleCategoryInfo articleCategory)
        {
            if (string.IsNullOrWhiteSpace(articleCategory.Name))
                throw new MallException("未指定文章分类名称");

            //检查文章分类是否存在,排除0的父分类，0为根级
            //if (articleCategory.ParentCategoryId != 0 && Context.ArticleCategoryInfo.Count(item => item.Id == articleCategory.ParentCategoryId) == 0)
            if (articleCategory.ParentCategoryId != 0 && !DbFactory.Default.Get<ArticleCategoryInfo>().Where(item => item.Id == articleCategory.ParentCategoryId).Exist())
                throw new MallException("不存在父级为" + articleCategory.ParentCategoryId + "的文章分类");

            articleCategory.IsDefault = false;//默认项禁止用户添加
            articleCategory.DisplaySequence = 1;//设置默认顺序为1
            //articleCategory = Context.ArticleCategoryInfo.Add(articleCategory);
            //Context.SaveChanges();
            DbFactory.Default.Add(articleCategory);
        }

        public void UpdateArticleCategoryName(long id, string name)
        {
            //ArticleCategoryInfo articleCategory = Context.ArticleCategoryInfo.FirstOrDefault(p => p.Id == id);
            ArticleCategoryInfo articleCategory = DbFactory.Default.Get<ArticleCategoryInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (articleCategory != null)
            {
                //articleCategory.Name = name;
                //Context.SaveChanges();
                DbFactory.Default.Set<ArticleCategoryInfo>().Set(n => n.Name, name).Where(p => p.Id == id).Succeed();
            }
            else
                throw new MallException("未找到id为" + id + "的对象");
        }

        public void UpdateArticleCategoryDisplaySequence(long id, long displaySequence)
        {
            //ArticleCategoryInfo articleCategory = Context.ArticleCategoryInfo.FirstOrDefault(p => p.Id == id);
            ArticleCategoryInfo articleCategory = DbFactory.Default.Get<ArticleCategoryInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (articleCategory != null)
            {
                //articleCategory.DisplaySequence = displaySequence;
                //Context.SaveChanges();
                DbFactory.Default.Set<ArticleCategoryInfo>().Set(n => n.DisplaySequence, displaySequence).Where(p => p.Id == id).Succeed();
            }
            else
                throw new MallException("未找到id为" + id + "的对象");
        }


        public void UpdateArticleCategory(ArticleCategoryInfo articleCategory)
        {
            if (articleCategory == null)
                throw new MallException("未指定ArticleCategoryInfo实例");
            if (string.IsNullOrWhiteSpace(articleCategory.Name))
                throw new MallException("未指定文章分类名称");

            //检查文章分类是否存在,排除0的父分类，0为根级
            //if (articleCategory.ParentCategoryId != 0 && Context.ArticleCategoryInfo.Count(item => item.Id == articleCategory.ParentCategoryId) == 0)
            if (articleCategory.ParentCategoryId != 0 && !DbFactory.Default.Get<ArticleCategoryInfo>().Where(item => item.Id == articleCategory.ParentCategoryId).Exist())
                throw new MallException("不存在父级为" + articleCategory.ParentCategoryId + "的文章分类");
            //ArticleCategoryInfo oriArticleCategory = Context.ArticleCategoryInfo.FirstOrDefault(p => p.Id == articleCategory.Id);
            ArticleCategoryInfo oriArticleCategory = DbFactory.Default.Get<ArticleCategoryInfo>().Where(p => p.Id == articleCategory.Id).FirstOrDefault();
            if (oriArticleCategory == null)
                throw new MallException("未找到id为" + articleCategory.Id + "的对象");

            //修改
            oriArticleCategory.Name = articleCategory.Name;
            oriArticleCategory.ParentCategoryId = articleCategory.ParentCategoryId;
            //Context.SaveChanges();//保存更改
            DbFactory.Default.Update(oriArticleCategory);

        }


        public void DeleteArticleCategory(params long[] ids)
        {
            var articleCategories = DbFactory.Default.Get<ArticleCategoryInfo>().Where(item => item.Id.ExIn(ids)).ToList();
            foreach (long id in ids)
                articleCategories.Concat(GetArticleCategoriesByParentId(id, true));

            if (articleCategories.Count(item => item.IsDefault) > 0)
                throw new MallException("系统内置分类不能删除");

            foreach (var item in articleCategories)
            {
                if (DbFactory.Default.Get<ArticleInfo>().Where(p => p.CategoryId == item.Id).Exist())
                    throw new MallException("请先删除分类'" + item.Name + "'下的文章！");
            }
            DbFactory.Default.Del<ArticleCategoryInfo>(articleCategories);
        }



        public ArticleCategoryInfo GetArticleCategory(long id)
        {
            return DbFactory.Default.Get<ArticleCategoryInfo>().Where(p => p.Id == id).FirstOrDefault();
        }


        public string GetFullPath(long id, string seperator = ",")
        {
            StringBuilder path = new StringBuilder(id.ToString());
            long parentId = id;
            do
            {
                var articleCategory = GetArticleCategory(parentId);
                parentId = articleCategory.ParentCategoryId;
                path.Insert(0, parentId + seperator);
            } while (parentId != 0);
            return path.ToString();
        }


        public ArticleCategoryInfo GetSpecialArticleCategory(SpecialCategory categoryType)
        {
            return GetArticleCategory((int)categoryType);
        }

        public List<ArticleCategoryInfo> GetCategories()
        {
            return DbFactory.Default.Get<ArticleCategoryInfo>().OrderBy(d=>d.DisplaySequence).ToList();
        }
        /// <summary>
        /// 检测是否存在重名栏目
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckHaveRename(long id, string name)
        {
            //bool result = false;
            //result = (Context.ArticleCategoryInfo.Count(d=>d.Id!=id && d.Name==name)>0);
            //return result;
            return DbFactory.Default.Get<ArticleCategoryInfo>().Where(d => d.Id != id && d.Name == name).Count() > 0;
        }
    }
}
