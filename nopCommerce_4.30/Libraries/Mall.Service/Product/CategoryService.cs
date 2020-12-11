using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mall.Service
{
    public class CategoryService : ServiceBase, ICategoryService
    {
        #region 静态字段
        private static object _id_locker = new object();
        #endregion

        #region 常量
        /// <summary>
        /// 分类路径分隔符
        /// </summary>
        const char CATEGORY_PATH_SEPERATOR = '|';
        #endregion

        #region 平台

        public IEnumerable<CategoryInfo> GetMainCategory()
        {
            return GetCategories().Where(t => t.ParentCategoryId == 0).ToList();
        }

        /// <summary>
        /// 获取所有分类并缓存
        /// </summary>
        /// <returns></returns>
		public List<CategoryInfo> GetCategories()
        {
            List<CategoryInfo> categories = null;

            if (Cache.Exists(CacheKeyCollection.Category))
                categories = Core.Cache.Get<List<CategoryInfo>>(CacheKeyCollection.Category);

            if (categories == null)
            {
                categories = DbFactory.Default.Get<CategoryInfo>().Where(p => p.IsDeleted == false).ToList();
                Cache.Insert(CacheKeyCollection.Category, categories);
            }

            return categories;
        }

        public void AddCategory(CategoryInfo model)
        {
            if (null == model)
                throw new ArgumentNullException("model", "添加一个分类时，Model为空");
            DbFactory.Default.Add(model);
            if (model.ParentCategoryId == 0)
            {
                Log.Info("加一级分类：ParentCategoryId：" + model.Id);//这里加个日志记录
                DbFactory.Default.Add(new CategoryCashDepositInfo() { Id = 0, CategoryId = model.Id });
            }
            Cache.Remove(CacheKeyCollection.Category);
        }

        public Entities.CategoryInfo GetCategory(long id)
        {
            if (id <= 0)
                return null;
            var model = GetCategories().Where(t => t.Id == id).FirstOrDefault();
            return model;

        }

        public void UpdateCategoryName(long id, string name)
        {
            if (id <= 0)
                throw new ArgumentNullException("id", string.Format("更新一个分类的名称时，id={0}", id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "更新一个分类的名称时，name为空");

            var category = DbFactory.Default.Get<Entities.CategoryInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (null == category || category.Id != id)
                throw new Exception(string.Format("更新一个分类的名称时，找不到id={0} 的分类", id));

            if (null != category)
            {
                var obja = DbFactory.Default.Get<Entities.CategoryInfo>().Where(r => r.ParentCategoryId == category.ParentCategoryId && r.Name == name && r.Id != category.Id);
                if (obja.Count() >= 1)
                    throw new Exception(string.Format("分类名称重复", id));
            }

            category.Name = name;
            DbFactory.Default.Update(category);

            Cache.Remove(CacheKeyCollection.Category);
        }
        public void UpdateCategoryCommis(long id, decimal commis)
        {
            if (id <= 0)
                throw new ArgumentNullException("id", string.Format("更新一个分类的分佣比率时，id={0}", id));
            if (commis < 0)
                throw new ArgumentNullException("commis", "更新一个分类的分佣比率时，commis小于0");

            var category = DbFactory.Default.Get<Entities.CategoryInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (null == category || category.Id != id)
                throw new Exception(string.Format("更新一个分类的名称时，找不到id={0} 的分类", id));

            category.CommisRate = commis;
            DbFactory.Default.Update(category);

            Cache.Remove(CacheKeyCollection.Category);
        }
        public void UpdateCategoryDisplaySequence(long id, long displaySequence)
        {
            if (id <= 0)
                throw new ArgumentNullException("id", string.Format("更新一个分类的显示顺序时，id={0}", id));
            if (0 >= displaySequence)
                throw new ArgumentNullException("displaySequence", "更新一个分类的显示顺序时，displaySequence小于等于零");

            var category = DbFactory.Default.Get<Entities.CategoryInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (null == category || category.Id != id)
                throw new Exception(string.Format("更新一个分类的显示顺序时，找不到id={0} 的分类", id));

            category.DisplaySequence = displaySequence;
            DbFactory.Default.Update(category);

            Cache.Remove(CacheKeyCollection.Category);
        }

        public void UpdateCategorysShow(bool isShow, List<long> ids)
        {
            DbFactory.Default.Set<CategoryInfo>()
               .Set(n => n.IsShow, isShow).Where(e => e.Id.ExIn(ids)).Succeed();

            Cache.Remove(CacheKeyCollection.Category);
            Cache.Remove(CacheKeyCollection.HomeCategory);
        }


        public IEnumerable<CategoryInfo> GetCategoryByParentId(long id, bool justIsShow = true)
        {
            if (id < 0)
                throw new ArgumentNullException("id", string.Format("获取子级分类时，id={0}", id));

            if (id == 0)
            {
                if (justIsShow)
                {
                    return GetCategories().OrderBy(c => c.DisplaySequence).Where(c => c.ParentCategoryId == 0 && c.IsShow);
                }
                else
                {
                    return GetCategories().OrderBy(c => c.DisplaySequence).Where(c => c.ParentCategoryId == 0);
                }

            }
            else
            {
                IEnumerable<CategoryInfo> category = null;
                if (justIsShow)
                {
                    category = GetCategories().Where(c => c.ParentCategoryId == id && c.IsShow);
                }
                else
                {
                    category = GetCategories().Where(c => c.ParentCategoryId == id);
                }
                if (category == null) return null;
                return category.OrderBy(c => c.DisplaySequence).ToList();
            }
        }
        /// <summary>
        /// 获取分类所有下级
        /// </summary>
        /// <returns></returns>
        public List<CategoryInfo> GetAllCategoryByParent(long id)
        {
            var categroies = GetCategories();
            return GetCategroiesByParent(categroies, id);
        }

        private List<CategoryInfo> GetCategroiesByParent(List<CategoryInfo> source, long id)
        {
            var result = new List<CategoryInfo>();
            var list = source.Where(p => p.ParentCategoryId == id).ToList();
            result.AddRange(list);
            foreach (var item in list)
                result.AddRange(GetCategroiesByParent(source, item.Id));
            return result;
        }


        public IEnumerable<CategoryInfo> GetFirstAndSecondLevelCategories()
        {
            var list = GetCategories();
            var result = list.Where(c => list.Any(cc => cc.ParentCategoryId == c.Id) || c.Depth < 3).ToList();
            return result;
        }


        public long GetMaxCategoryId()
        {
            lock (_id_locker)
            {
                return DbFactory.Default.Get<Entities.CategoryInfo>().Max<long>(p => p.Id);
            }
        }

        public void UpdateCategory(Entities.CategoryInfo model)
        {
            var category = DbFactory.Default.Get<Entities.CategoryInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
            category.Icon = model.Icon;
            category.Meta_Description = model.Meta_Description;
            category.Meta_Keywords = model.Meta_Keywords;
            category.Meta_Title = model.Meta_Title;
            category.Name = model.Name;
            category.RewriteName = model.RewriteName;
            category.TypeId = model.TypeId;
            category.CommisRate = model.CommisRate;
            category.SupportVirtualProduct = model.SupportVirtualProduct;
            DbFactory.Default.Update(category);

            Cache.Remove(CacheKeyCollection.Category);
        }

        public void DeleteCategory(long id)
        {
            var path = DbFactory.Default.Get<Entities.CategoryInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (path == null)
            {
                Cache.Remove(CacheKeyCollection.Category);
                return;
            }

            var pathStart = path.Path + CATEGORY_PATH_SEPERATOR;
            var allChildIds = DbFactory.Default.Get<Entities.CategoryInfo>().Where(p => p.Path.StartsWith(pathStart)).Select(p => p.Id).ToList<long>();

            var existProduct = DbFactory.Default.Get<Entities.ProductInfo>().Where(p => (p.CategoryId == id || p.CategoryId.ExIn(allChildIds)) && p.IsDeleted == false).Exist();
            if (existProduct)
                throw new MallException("删除失败，因为有商品与该分类或子分类关联");

            allChildIds.Add(id);
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Set<CategoryInfo>().Set(n => n.IsDeleted, true).Where(n => n.Id == id || n.Id.ExIn(allChildIds)).Succeed();
                DbFactory.Default.Del<BusinessCategoryInfo>(n => n.CategoryId == id || n.CategoryId.ExIn(allChildIds));
                var category = DbFactory.Default.Get<CategoryInfo>().Where(n => n.IsDeleted == true).Where<HomeCategoryInfo>((ci, hci) => ci.Id == hci.CategoryId).Select(n => n.Id);
                DbFactory.Default.Del<HomeCategoryInfo>(n => n.ExExists(category));
            });

            Cache.Remove(CacheKeyCollection.Category);
        }


        public IEnumerable<Entities.CategoryInfo> GetTopLevelCategories(IEnumerable<long> categoryIds)
        {
            var categories = GetCategories().Where(item => categoryIds.Contains(item.Id));
            List<long> topLevelIds = new List<long>();
            foreach (var cateogry in categories)
            {
                if (cateogry.Depth == 1)
                    topLevelIds.Add(cateogry.Id);
                else
                {
                    var path = cateogry.Path;
                    var topLevelId = long.Parse(path.Split(CATEGORY_PATH_SEPERATOR)[0]);//取全路径的第一级转换，即所属一级分类id
                    topLevelIds.Add(topLevelId);
                }
            }
            return GetCategories().Where(item => topLevelIds.Contains(item.Id));
        }


        public IEnumerable<Entities.CategoryInfo> GetSecondAndThirdLevelCategories(params long[] ids)
        {
            var categoies = GetCategories().Where(item => ids.Contains(item.ParentCategoryId));
            var categoryList = new List<Entities.CategoryInfo>(categoies);

            foreach (var categoryId in categoies.Select(item => item.Id).ToList())
            {
                var category = GetCategories().Where(item => item.ParentCategoryId == categoryId);
                categoryList.AddRange(category);
            }
            return categoryList;
        }

        public IEnumerable<Entities.CategoryInfo> GetParentCategoryById(long id, bool isshow)
        {
            List<Entities.CategoryInfo> categoryList = new List<Entities.CategoryInfo>();
            var parentId = id;
            do
            {
                var pInfo = GetCategories().Where(item => item.Id == parentId).FirstOrDefault();
                if (pInfo != null)
                {
                    categoryList.Add(pInfo);
                    if (isshow)
                    {
                        var sameCount = GetCategories().Where(item => item.Depth == pInfo.Depth && item.ParentCategoryId == pInfo.ParentCategoryId && item.IsShow == isshow).Count();

                        if (sameCount == 1)
                        {
                            parentId = pInfo.ParentCategoryId;
                        }
                        else
                        {
                            parentId = 0;
                        }
                    }
                    else
                    {
                        parentId = pInfo.ParentCategoryId;
                    }
                }
                else
                {
                    parentId = 0;
                }
            }
            while (parentId > 0);
            return categoryList;
        }

        public string GetEffectCategoryName(long shopId, long typeId)
        {
            StringBuilder name = new StringBuilder();
            var cates = DbFactory.Default.Get<Entities.CategoryInfo>().Where(c => c.TypeId == typeId && c.IsDeleted == false).ToList();
            var businessCate = DbFactory.Default.Get<Entities.BusinessCategoryInfo>().Where(b => b.ShopId == shopId).ToList();
            foreach (var item in cates)
            {
                if (businessCate.Any(b => b.CategoryId == item.Id))
                {
                    name.Append(item.Name);
                    name.Append(',');
                }
            }
            return name.ToString().TrimEnd(',');
        }

        #endregion

        public List<CategoryInfo> GetValidBusinessCategoryByParentId(long id)
        {
            var allCategories = GetCategories();
            var categories = allCategories.Where(item => item.ParentCategoryId == id);
            if (id == 0)//表示第一级，只筛选出包含第三级分类的一级分类
            {
                var topCateogryIds = allCategories.Where(item => item.Path.Split('|').Length == 3).Select(item => long.Parse(item.Path.Split('|')[0]));
                categories = categories.Where(item => topCateogryIds.Contains(item.Id));
            }
            else//只筛选出包含第三级分类的二级分类
            {
                var currentCategory = allCategories.FirstOrDefault(item => item.Id == id);
                if (currentCategory != null && currentCategory.Depth == 1)//判断是否第一级，如果是第一级则表示获取第二级数据
                {
                    var secondCateogryIds = allCategories.Where(item => item.Path.Split('|').Length == 3).Select(item => long.Parse(item.Path.Split('|')[1]));
                    categories = categories.Where(item => secondCateogryIds.Contains(item.Id));
                }
            }
            return categories.ToList();
        }

        /// <summary>
        /// 获取分类列表
        /// </summary>
        /// <param name="ids">id集合</param>
        /// <returns></returns>
        public List<CategoryInfo> GetCateogryListByIds(List<long> ids)
        {
            if (ids == null || ids.Count <= 0)
                return null;

            var result = DbFactory.Default.Get<CategoryInfo>().Where(e => e.Id.ExIn(ids)).ToList();
            return result;
        }


    }
}
