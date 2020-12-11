using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using NetRube.Data;
using Mall.DTO;
using Nop.Core.Infrastructure;

namespace Mall.Service
{
    public class HomeCategoryService : ServiceBase, IHomeCategoryService
    {

        #region 首页分类设置

        /// <summary>
        /// 默认分类组数
        /// </summary>
        const int HOME_CATEGORY_SET_COUNT = 15;


        public int TotalRowsCount
        {
            get { return HOME_CATEGORY_SET_COUNT; }
        }

        public List<HomeCategorySet> GetHomeCategorySets()
        {
            var homeCategorySet = new HomeCategorySet[HOME_CATEGORY_SET_COUNT];

            if (Cache.Exists(CacheKeyCollection.HomeCategory)) 
            {
                homeCategorySet = Core.Cache.Get<HomeCategorySet[]>(CacheKeyCollection.HomeCategory);
            }
            else
            {
                var cc = DbFactory.Default.Get<HomeCategoryInfo>()
                    .LeftJoin<CategoryInfo>((hci, ci) => hci.CategoryId == ci.Id)
                    .Where<CategoryInfo>(p => p.IsDeleted == false && p.IsShow == true)
                    .OrderBy<CategoryInfo>(x => x.DisplaySequence)
                    .ToList();

                

                foreach (var group in cc.GroupBy(p=>p.RowId))
                {
                    homeCategorySet[group.Key - 1] = new HomeCategorySet()
                    {
                        RowNumber = group.Key,
                        HomeCategories = group.ToList()
                    };
                }
                var _HomeCategoryRowInfo = DbFactory.Default.Get<HomeCategoryRowInfo>().ToList();
                for (int i = 0; i < HOME_CATEGORY_SET_COUNT; i++)
                {
                    if (homeCategorySet[i] == null)
                    {
                        homeCategorySet[i] = new HomeCategorySet()
                        {
                            RowNumber = i + 1,
                            HomeCategories = new List<HomeCategoryInfo>()
                        };
                    }
                    var homeCategoryRows = new List<HomeCategorySet.HomeCategoryTopic>();
                    var categoryRowInfo = _HomeCategoryRowInfo.Where(item => item.RowId == i + 1).FirstOrDefault();
                    if (categoryRowInfo != null)
                    {
                        homeCategoryRows.Add(new HomeCategorySet.HomeCategoryTopic() { Url = categoryRowInfo.Url1, ImageUrl = categoryRowInfo.Image1 });
                        homeCategoryRows.Add(new HomeCategorySet.HomeCategoryTopic() { Url = categoryRowInfo.Url2, ImageUrl = categoryRowInfo.Image2 });
                    }
                    homeCategorySet[i].HomeCategoryTopics = homeCategoryRows;

                }

               var brandService =  EngineContext.Current.Resolve<IBrandService>();


              
                var categories =  EngineContext.Current.Resolve<ICategoryService>().GetCategories();

                for (int i = 0; i < homeCategorySet.Count(); i++)
                {
              
                    homeCategorySet[i].HomeCategories = homeCategorySet[i].HomeCategories.Select(t => {
                        return new HomeCategoryInfo()
                        {
                            CategoryId = t.CategoryId,
                            Depth = t.Depth,
                            Id = t.Id,
                            RowId = t.RowId,
                        };
                    }).ToList();
                    homeCategorySet[i].HomeCategoryTopics = homeCategorySet[i].HomeCategoryTopics.Select(t => new HomeCategorySet.HomeCategoryTopic()
                    {
                        ImageUrl = t.ImageUrl,
                        Url = t.Url
                    }).ToList();


                    const int MAX_DISPLAY_BRANDS = 8;//最多显示推荐品牌个数
                    //获取推荐的品牌
                    var singleRowBrand = brandService.GetBrandsByCategoryIds(homeCategorySet[i].HomeCategories.Where(item => item.Depth == 1).Select(item => item.CategoryId).ToArray()).Take(MAX_DISPLAY_BRANDS);

                    homeCategorySet[i].HomeBrand = singleRowBrand.Select(item => new BrandInfo
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Logo = item.Logo,
                    }).ToList();
                   
                }

                Cache.Insert<HomeCategorySet[]>(CacheKeyCollection.HomeCategory, homeCategorySet, 5);
            }
            return homeCategorySet.ToList();
        }

        public HomeCategorySet GetHomeCategorySet(int rowNumber)
        {
            if (rowNumber > HOME_CATEGORY_SET_COUNT || rowNumber < 0)
                throw new ArgumentNullException("行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            var homeCategorySet = new HomeCategorySet();
            homeCategorySet.RowNumber = rowNumber;
            //homeCategorySet.HomeCategories = Context.HomeCategoryInfo.FindBy(item => item.RowNumber == rowNumber);
            homeCategorySet.HomeCategories = DbFactory.Default.Get<HomeCategoryInfo>().Where(item => item.RowId == rowNumber).ToList();
            //var categoryRowInfo = Context.HomeCategoryRowInfo.FindBy(item => item.RowId == rowNumber).FirstOrDefault();
            var categoryRowInfo = DbFactory.Default.Get<HomeCategoryRowInfo>().Where(item => item.RowId == rowNumber).FirstOrDefault();

            var homeCategoryRows = new List<HomeCategorySet.HomeCategoryTopic>();
            homeCategoryRows.Add(new HomeCategorySet.HomeCategoryTopic() { Url = categoryRowInfo.Url1, ImageUrl = categoryRowInfo.Image1 });
            homeCategoryRows.Add(new HomeCategorySet.HomeCategoryTopic() { Url = categoryRowInfo.Url2, ImageUrl = categoryRowInfo.Image2 });
            homeCategorySet.HomeCategoryTopics = homeCategoryRows;

            return homeCategorySet;
        }

        public void UpdateHomeCategorySet(HomeCategorySet homeCategorySet)
        {
            if (homeCategorySet.HomeCategories == null)
                throw new ArgumentNullException("传入的分类不能为null，但可以是空集合");

            //int rowNumber = homeCategorySet.HomeCategories.FirstOrDefault().RowNumber;
            int rowNumber = homeCategorySet.HomeCategories.FirstOrDefault().RowId;
            if (rowNumber > HOME_CATEGORY_SET_COUNT || rowNumber < 0)
                throw new ArgumentNullException("行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            DbFactory.Default.InTransaction(() =>
            {
                //Context.HomeCategoryInfo.Remove(item => item.RowNumber == rowNumber);//移除原所有首页分类
                DbFactory.Default.Del<HomeCategoryInfo>(item => item.RowId == rowNumber);//移除原所有首页分类

                //填充行号
                foreach (var homeCategory in homeCategorySet.HomeCategories)
                {
                    //homeCategory.RowNumber = rowNumber;
                    homeCategory.RowId = rowNumber;
                    DbFactory.Default.Add(homeCategory);
                }
            });
            //Context.HomeCategoryInfo.AddRange(homeCategorySet.HomeCategories);//添加新的首页分类
            //Context.SaveChanges();
            //DbFactory.Default.Add(homeCategorySet.HomeCategories);

        }


        public void UpdateHomeCategorySet(int rowNumber, IEnumerable<long> categoryIds)
        {
            if (rowNumber > HOME_CATEGORY_SET_COUNT || rowNumber < 0)
                throw new ArgumentNullException("行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            var categoryService = new CategoryService();
            var categoriesCount = categoryIds.Count();
            //var homeCategories = new HomeCategoryInfo[categoriesCount];
            long categoryId;

            DbFactory.Default.InTransaction(() =>
            {
                //Context.HomeCategoryInfo.Remove(item => item.RowNumber == rowNumber);//移除原所有首页分类
                DbFactory.Default.Del<HomeCategoryInfo>(item => item.RowId == rowNumber);//移除原所有首页分类

                for (var i = 0; i < categoriesCount; i++)
                {
                    categoryId = categoryIds.ElementAt(i);
                    DbFactory.Default.Add(new HomeCategoryInfo()
                    {
                        //RowNumber = rowNumber,
                        RowId = rowNumber,
                        CategoryId = categoryId,
                        Depth = categoryService.GetCategory(categoryId).Depth
                    });
                }
            });
            //Context.HomeCategoryInfo.AddRange(homeCategories);
            //DbFactory.Default.Add<HomeCategoryInfo>(homeCategories);
            //Context.SaveChanges();
        }


        public void UpdateHomeCategorySetSequence(int sourceRowNumber, int destiRowNumber)
        {
            if (sourceRowNumber > HOME_CATEGORY_SET_COUNT || sourceRowNumber < 0)
                throw new ArgumentNullException("原行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);
            if (destiRowNumber > HOME_CATEGORY_SET_COUNT || destiRowNumber < 0)
                throw new ArgumentNullException("新行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            List<long> sources, destinations;
            //sources = Context.HomeCategoryInfo.FindBy(item => item.RowNumber == sourceRowNumber);
            sources = DbFactory.Default.Get<HomeCategoryInfo>().Where(item => item.RowId == sourceRowNumber).Select(n => n.Id).ToList<long>();
            //destinations = Context.HomeCategoryInfo.FindBy(item => item.RowNumber == destiRowNumber);
            destinations = DbFactory.Default.Get<HomeCategoryInfo>().Where(item => item.RowId == destiRowNumber).Select(n => n.Id).ToList<long>();
            //foreach (var source in sources)
            //{
            //source.RowNumber = destiRowNumber;
            //source.RowId = destiRowNumber;
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Set<HomeCategoryInfo>().Set(n => n.RowId, destiRowNumber).Where(p => p.Id.ExIn(sources)).Succeed();
                //}
                //foreach (var destination in destinations)
                //{
                //destination.RowId = sourceRowNumber;
                DbFactory.Default.Set<HomeCategoryInfo>().Set(n => n.RowId, sourceRowNumber).Where(p => p.Id.ExIn(destinations)).Succeed();
                //}
            });
            //Context.SaveChanges();

        }


        public void UpdateHomeCategorySet(int rowNumber, IEnumerable<HomeCategorySet.HomeCategoryTopic> homeCategoryTopics)
        {
            if (rowNumber > HOME_CATEGORY_SET_COUNT || rowNumber < 0)
                throw new ArgumentNullException("行号不在取值范围内！取值必须大于0且小于" + HOME_CATEGORY_SET_COUNT);

            HomeCategoryRowInfo rowInfo;
            //var oldRowInfo = Context.HomeCategoryRowInfo.FindBy(item => item.RowId == rowNumber).FirstOrDefault();
            var oldRowInfo = DbFactory.Default.Get<HomeCategoryRowInfo>().Where(item => item.RowId == rowNumber).FirstOrDefault();
            if (oldRowInfo == null)
                rowInfo = new HomeCategoryRowInfo() { RowId = rowNumber };
            else
                rowInfo = oldRowInfo;

            int i = 0;
            string[] needToDeleteFiles = new string[2];
            foreach (var item in homeCategoryTopics)
            {
                if (!string.IsNullOrWhiteSpace(item.Url) && !string.IsNullOrWhiteSpace(item.ImageUrl))
                {
                    if (i++ == 0)
                    {
                        if (rowInfo.Image1 != item.ImageUrl)//当图片有修改时，删除原图片
                            needToDeleteFiles[0] = rowInfo.Image1;

                        rowInfo.Image1 = item.ImageUrl;
                        rowInfo.Url1 = item.Url;
                    }
                    else
                    {
                        if (rowInfo.Image2 != item.ImageUrl)//当图片有修改时，删除原图片
                            needToDeleteFiles[1] = rowInfo.Image2;

                        rowInfo.Image2 = item.ImageUrl;
                        rowInfo.Url2 = item.Url;
                    }
                    if (!string.IsNullOrWhiteSpace(item.ImageUrl))
                        TransferImages(item.ImageUrl);
                }
            }
            if (oldRowInfo == null)
                //Context.HomeCategoryRowInfo.Add(rowInfo);
                DbFactory.Default.Add(rowInfo);
            //Context.SaveChanges();

            //删除原图片
            foreach (var file in needToDeleteFiles)
            {
                if (!string.IsNullOrWhiteSpace(file))
                {
                    MallIO.DeleteFile(file);
                }
            }
        }

        string TransferImages(string oriImageUrl)
        {
            string newDir = "/Storage/Plat/PageSettings/HomeCategory";
            string newFileName = oriImageUrl;
            if (!string.IsNullOrWhiteSpace(newFileName))
            {
                if (newFileName.Contains("/temp/"))
                {
                    string ext = oriImageUrl.Substring(newFileName.LastIndexOf('.'));
                    string newName = Guid.NewGuid().ToString("N") + ext;
                    string oldlogo = newFileName.Substring(newFileName.LastIndexOf("/temp"));
                    string newLogo = newDir + newName;
                    Core.MallIO.CopyFile(oldlogo, newLogo, true);
                    newFileName = newLogo;
                }
                else if (newFileName.Contains("/Storage/"))
                {
                    newFileName = newFileName.Substring(newFileName.LastIndexOf("/Storage"));
                }
            }
            return newFileName;
        }





        #endregion



    }
}
