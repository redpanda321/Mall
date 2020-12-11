using Mall.CommonModel;
using Mall.DTO;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    public class FloorService : ServiceBase, IFloorService
    {
        #region 首页楼层设置

        /// <summary>
        /// 显示顺序锁
        /// </summary>
        static object DisplaySequenceLocker = new object();

        public HomeFloorInfo AddHomeFloorBasicInfo(string name, IEnumerable<long> topLevelCategoryIds)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("楼层名称不能为空");
            if (topLevelCategoryIds == null || topLevelCategoryIds.Count() == 0)
                throw new ArgumentNullException("至少要选择一个商品分类");

            var homeFloorInfo = new HomeFloorInfo()
            {
                FloorName = name,
            };

            lock (DisplaySequenceLocker)//防止添加重复的顺序号
            {
                homeFloorInfo.DisplaySequence = GetMaxHomeFloorSequence() + 1;//设置显示序号
                DbFactory.Default.Add(homeFloorInfo);
                var categories = topLevelCategoryIds.Select(item => new FloorCategoryInfo
                {
                    CategoryId = item,
                    Depth = 1, //基本信息只保存一级分类
                    FloorId = homeFloorInfo.Id
                });
                DbFactory.Default.Add(categories);
            }
            return homeFloorInfo;
        }

        public void UpdateFloorBasicInfo(long homeFloorId, string name, IEnumerable<long> topLevelCategoryIds)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("楼层名称不能为空");
            if (topLevelCategoryIds == null || topLevelCategoryIds.Count() == 0)
                throw new ArgumentNullException("至少要选择一个商品分类");

            var homeFloorBasicInfo = DbFactory.Default.Get<HomeFloorInfo>().Where(p => p.Id == homeFloorId).FirstOrDefault();

            homeFloorBasicInfo.FloorName = name;

            topLevelCategoryIds = topLevelCategoryIds.Distinct();

            
            var categories = DbFactory.Default.Get<FloorCategoryInfo>(p => p.FloorId == homeFloorBasicInfo.Id)
                .Select(p => p.CategoryId)
                .ToList<long>();//当前楼层所有一级商品分类id
            var deletedTopLevelCategoryIds = categories.Where(item => !topLevelCategoryIds.Contains(item));//当前楼层待删除的一级商品分类id
            var newTopLevelCategoryIds = topLevelCategoryIds.Where(item => !categories.Contains(item));//待添加的一级商品分类id

            var newFloorCategories = new FloorCategoryInfo[newTopLevelCategoryIds.Count()];

            //构造新的楼层分类
            int i = 0;
            foreach (var categoryId in newTopLevelCategoryIds)
            {
                newFloorCategories[i++] = new FloorCategoryInfo()
                {
                    FloorId = homeFloorId,
                    CategoryId = categoryId,
                    Depth = 1
                };
            }
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<FloorCategoryInfo>(item => item.FloorId == homeFloorId && item.CategoryId.ExIn(deletedTopLevelCategoryIds));//删除待删除的
                DbFactory.Default.Add<FloorCategoryInfo>(newFloorCategories);
                DbFactory.Default.Update(homeFloorBasicInfo);
            });
        }

        public void UpdateHomeFloorSequence(long sourceSequence, long destiSequence)
        {
            var source = DbFactory.Default.Get<HomeFloorInfo>().Where(item => item.DisplaySequence == sourceSequence).FirstOrDefault();
            var destination = DbFactory.Default.Get<HomeFloorInfo>().Where(item => item.DisplaySequence == destiSequence).FirstOrDefault();
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Set<HomeFloorInfo>().Set(n => n.DisplaySequence, destiSequence).Where(p => p.Id == source.Id).Succeed();
                DbFactory.Default.Set<HomeFloorInfo>().Set(n => n.DisplaySequence, sourceSequence).Where(p => p.Id == destination.Id).Succeed();
            });
        }

        public void EnableHomeFloor(long homeFloorId, bool enable)
        {
            DbFactory.Default.Set<HomeFloorInfo>()
                .Set(n => n.IsShow, enable)
                .Where(p => p.Id == homeFloorId)
                .Succeed();
        }

        public List<HomeFloorInfo> GetAllHomeFloors()
        {
            return DbFactory.Default
                .Get<HomeFloorInfo>()
                .OrderBy(a => a.DisplaySequence)
                .ToList();
        }


        public HomeFloorInfo GetHomeFloor(long id)
        {
            return DbFactory.Default.Get<HomeFloorInfo>().Where(p => p.Id == id).FirstOrDefault();
        }

        public List<FloorBrandInfo> GetFloorBrands(long floor)
        {
            return DbFactory.Default.Get<FloorBrandInfo>(p => p.FloorId == floor).ToList();
        }

        /// <summary>
        /// 获取最大首页楼层序号
        /// </summary>
        /// <returns></returns>
        private long GetMaxHomeFloorSequence()
        {
            return DbFactory.Default.Get<HomeFloorInfo>().Max<long>(item => item.DisplaySequence.ExIfNull(0));
        }



        public void DeleteHomeFloor(long id)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var tabs = DbFactory.Default.Get<FloorTablInfo>().Where(p => p.FloorId == id).Select(p => p.Id);
                DbFactory.Default.Del<FloorTablDetailInfo>(p => p.TabId.ExIn(tabs));
                DbFactory.Default.Del<FloorTablInfo>(p => p.FloorId == id);
                DbFactory.Default.Del<HomeFloorInfo>(n => n.Id == id);
            });
            RemoveImage(id);//删除图片
        }

        public void UpdateHomeFloorDetail(HomeFloor floor)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var info = floor.Info;
                if (floor.Id == 0)
                {
                    var max = DbFactory.Default.Get<HomeFloorInfo>().Max<long>(p => p.DisplaySequence);
                    info.DisplaySequence = max + 1;
                    info.IsShow = true;
                    DbFactory.Default.Add(info);
                }
                else
                {
                    var model = DbFactory.Default.Get<HomeFloorInfo>().Where(p => p.Id == floor.Id).FirstOrDefault();
                    model.DefaultTabName = info.DefaultTabName;
                    model.FloorName = info.FloorName;
                    model.SubName = info.SubName;
                    model.StyleLevel = info.StyleLevel;
                    model.CommodityStyle = info.CommodityStyle;
                    model.DisplayMode = info.DisplayMode;
                    DbFactory.Default.Update(model);
                }
                
                if ((new List<uint>{ 1,4,5,6,7}).Contains(info.StyleLevel))
                {
                    floor.Tabs.ForEach(item=> {
                        item.FloorId = floor.Id;
                    });
                    UpdateFloorTablInfo(info.Id, floor.Tabs);
                }

                if (floor.Brands != null)
                    floor.Brands.ForEach(a => a.FloorId = floor.Id);
                UpdateBrand(floor.Id, floor.Brands);

                floor.Topics.ForEach(a => a.FloorId = floor.Id);
                UpdateTextLink(floor.Id, floor.Topics.Where(item => item.TopicType == Position.Top));
                UpdataProductLink(floor.Id, floor.Topics.Where(item => item.TopicType != Position.Top));

                DbFactory.Default.InTransaction(() =>
                {
                    DbFactory.Default.Del<FloorProductInfo>(item => item.FloorId == floor.Id);
                    floor.Products.ForEach(p => p.FloorId = floor.Id);
                    DbFactory.Default.Add<FloorProductInfo>(floor.Products);
                });
            });
        }

        private void UpdateFloorTablInfo(long floorId, List<FloorTablInfo> tabs)
        {
            DbFactory.Default.InTransaction(() =>
            {
                //获取该删除的所有选项卡
                var ids = DbFactory.Default.Get<FloorTablInfo>().Where(item => item.FloorId == floorId).Select(item => item.Id);
                //先删除所有选项卡的关联商品
                DbFactory.Default.Del<FloorTablDetailInfo>(item => item.TabId.ExIn(ids));
                //再删除选项卡
                DbFactory.Default.Del<FloorTablInfo>(item => item.FloorId == floorId);
                DbFactory.Default.Add<FloorTablInfo>(tabs);
                tabs.ForEach(p => p.FloorTablDetailInfo.ForEach(x => x.TabId = p.Id));
                var details = tabs.SelectMany(p => p.FloorTablDetailInfo);
                DbFactory.Default.Add(details);
            });
        }

        private void UpdataProductLink(long floorId, IEnumerable<FloorTopicInfo> productLink)
        {
            //删除原文字链接
            DbFactory.Default.Del<FloorTopicInfo>(item => item.FloorId == floorId && item.TopicType != Position.Top);
            int num = 0;
            foreach (var item in productLink)
            {
                num++;
                item.TopicImage = TransferImage(item.TopicImage, floorId, item.TopicType.ToString(), num);
            }
            //重新添加
            DbFactory.Default.Add(productLink);
        }

        /// <summary>
        /// 保存品牌
        /// </summary>
        /// <param name="floorBrands"></param>
        void UpdateBrand(long floorId, List<FloorBrandInfo> floorBrands)
        {
            //var oriBrands = DbFactory.Default.Get<FloorBrandInfo>().Where(item => item.FloorId == floorId).ToList();//查询该楼层下所有品牌
            //var oriBrandIds = oriBrands.Select(item => item.BrandId);//原所有品牌id
            //if (floorBrands == null)
            //{
            //    floorBrands = new List<FloorBrandInfo>();
            //}
            //var newBrandIds = floorBrands.Select(item => item.BrandId);//新的品牌id
            ////查出要删除的分类并删除 
            //var needToDelete = oriBrands.Where(item => !newBrandIds.Contains(item.BrandId));
            ////查出要添加的分类并添加
            //var needToAdd = floorBrands.Where(item => !oriBrandIds.Contains(item.BrandId));
            //DbFactory.Default.InTransaction(() =>
            //{
            //    DbFactory.Default.Del(needToDelete);
            //    DbFactory.Default.Add(needToAdd);
            //});

            var oriBrands = DbFactory.Default.Get<FloorBrandInfo>().Where(item => item.FloorId == floorId).ToList();//查询该楼层下所有品牌

            var oriBrandIds = oriBrands.Select(item => item.BrandId);//原所有品牌id
            var newBrandIds = floorBrands.Select(item => item.BrandId);//新的品牌id

            //查出要删除的分类并删除 
            var needToDelete = oriBrands.Where(item => !newBrandIds.Contains(item.BrandId));
            //查出要添加的分类并添加
            var needToAdd = floorBrands.Where(item => !oriBrandIds.Contains(item.BrandId));
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del(needToDelete);
                DbFactory.Default.Add(needToAdd);
            });
        }

        /// <summary>
        /// 移动图片
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        string TransferImage(string sourceFile, long floorId, string type, int num = 0)
        {
            if (!string.IsNullOrWhiteSpace(sourceFile) && !sourceFile.Contains("/Storage/Plat/"))
            {
                string newDir = "/Storage/Plat/PageSettings/HomeFloor/" + floorId + "/";

                string ext = sourceFile.Substring(sourceFile.LastIndexOf('.'));//得到扩展名
                string newName = "floor_" + type + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmssfff") + num + ext;//得到新的文件名

                if (!string.IsNullOrWhiteSpace(sourceFile))
                {
                    if (sourceFile.Contains("/temp/"))
                    {
                        string logoname = sourceFile.Substring(sourceFile.LastIndexOf('/') + 1);
                        string oldlogo = sourceFile.Substring(sourceFile.LastIndexOf("/temp"));
                        string newLogo = newDir + newName;
                        Core.MallIO.CopyFile(oldlogo, newLogo, true);
                        sourceFile = newLogo;
                        return sourceFile;//返回新的文件路径
                    }
                    else if (sourceFile.Contains("/Storage/"))
                    {
                        sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
                    }
                }
            }
            else if (sourceFile.Contains("/Storage/"))
            {
                sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
            }

            return sourceFile;
        }

        void RemoveImage(long id)
        {
            try
            {
                string path = "/Storage/Plat/PageSettings/HomeFloor/" + id + "/";

                Core.MallIO.DeleteDir(path, true);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 保存文字链接
        /// </summary>
        /// <param name="floorId"></param>
        /// <param name="textLinks"></param>
        void UpdateTextLink(long floorId, IEnumerable<FloorTopicInfo> textLinks)
        {
            //删除原文字链接
            DbFactory.Default.Del<FloorTopicInfo>(item => item.FloorId == floorId && item.TopicType == Position.Top);
            //重新添加
            DbFactory.Default.Add(textLinks);

        }


        #endregion

        public List<FloorProductInfo> GetProducts(long floor) {
            return DbFactory.Default.Get<FloorProductInfo>().Where(p => p.FloorId == floor).ToList();
        }

        public List<FloorTopicInfo> GetTopics(long floor) {
            return DbFactory.Default.Get<FloorTopicInfo>().Where(p=>p.FloorId==floor).ToList();
        }

        public List<FloorTablInfo> GetTabs(long floor)
        {
            return DbFactory.Default.Get<FloorTablInfo>().Where(p => p.FloorId == floor).ToList();
        }

        public List<FloorTablDetailInfo> GetDetails(long tab)
        {
            return DbFactory.Default.Get<FloorTablDetailInfo>().Where(p => p.TabId == tab).ToList();
        }
    }
}
