using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mall.Service
{
    public class TopicService : ServiceBase, ITopicService
    {

        public QueryPageModel<TopicInfo> GetTopics(int pageNo, int pageSize, PlatformType platformType = PlatformType.PC)
        {
            var topic = new QueryPageModel<TopicInfo>();
            var pageList = DbFactory.Default.Get<TopicInfo>().Where(item => item.PlatForm == platformType).OrderByDescending(a => a.Id).ToPagedList(pageNo, pageSize);

            topic.Models = pageList;
            topic.Total = pageList.TotalRecordCount;
            return topic;
        }

        public List<TopicInfo> GetTopics(List<long> topics)
        {
            return DbFactory.Default.Get<TopicInfo>(p => p.Id.ExIn(topics)).ToList();
        }

        public List<TopicModuleInfo> GetModules(long topic) {
            return DbFactory.Default.Get<TopicModuleInfo>(p => p.TopicId == topic).ToList();
        }
        public List<ModuleProductInfo> GetModuleProducts(List<long> modules) {
            return DbFactory.Default.Get<ModuleProductInfo>(p => p.ModuleId.ExIn(modules)).ToList();
        }
        public TopicModuleInfo GetTopicModule(long moduleId)
        {
            return DbFactory.Default.Get<TopicModuleInfo>(p => p.Id == moduleId).FirstOrDefault();
        }
       

        public QueryPageModel<TopicInfo> GetTopics(TopicQuery topicQuery)
        {
            var topic = new QueryPageModel<Entities.TopicInfo>();
            var topics = DbFactory.Default.Get<Entities.TopicInfo>();
            if (topicQuery.MorePlatForm != null)
            {
                topics.Where(item => item.PlatForm.ExIn(topicQuery.MorePlatForm));
            }
            else
            {
                topics.Where(item => item.PlatForm == topicQuery.PlatformType);
            }
            #region 条件组合
            if (topicQuery.ShopId > 0)
            {
                topics.Where(item => item.ShopId == topicQuery.ShopId);
            }
            else
            {
                topics.Where(item => item.ShopId == 0);
            }
            if (topicQuery.IsRecommend.HasValue)
            {
                topics.Where(item => item.IsRecommend == topicQuery.IsRecommend.Value);
            }
            if (!string.IsNullOrWhiteSpace(topicQuery.Name))
            {
                topicQuery.Name = topicQuery.Name.Trim();
                topics.Where(item => item.Name.Contains(topicQuery.Name));
            }
            if (!string.IsNullOrWhiteSpace(topicQuery.Tags))
            {
                topicQuery.Tags = topicQuery.Tags.Trim();
                topics.Where(item => item.Tags.Contains(topicQuery.Tags));
            }
            #endregion

            topics = topics.OrderByDescending(d => d.Id);
            if (topicQuery.IsAsc)
            {
                topics.OrderBy(d => d.Id);
            }
            else
            {
                topics.OrderByDescending(d => d.Id);
            }

            var data = topics.ToPagedList(topicQuery.PageNo, topicQuery.PageSize);
            topic.Models = data;
            topic.Total = data.TotalRecordCount;
            return topic;
        }


        public void DeleteTopic(long id)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<Entities.TopicInfo>().Where(n => n.Id == id).Succeed();
                //删除数据            
                if (DbFactory.Default.Get<Entities.MobileHomeTopicInfo>().Where(item => item.TopicId == id).Count() > 0)
                    throw new MallException("你的微信首页推荐专题选择了该专题，请先解除选定再删除！");

                //删除对应图片目录。。。。。。。。
                string topicDir = "/Storage/Plat/Topic" + "/" + id;
                if (Core.MallIO.ExistDir(topicDir))
                    Core.MallIO.DeleteDir(topicDir, true);
            });
        }


        public void AddTopic(Topic topic)
        {
            if (string.IsNullOrWhiteSpace(topic.Name))
                throw new ArgumentNullException("专题名称不能为空");

            bool moduleName = topic.TopicModuleInfo.Any(a => a.Name.Length > 25);

            if (moduleName)
            {
                throw new MallException("模块名称不能超过25个字符");
            }
            DbFactory.Default.InTransaction(() =>
            {
                var info = new TopicInfo
                {
                    Name = topic.Name,
                    FrontCoverImage = topic.FrontCoverImage,
                    TopImage = topic.TopImage,
                    BackgroundImage = topic.BackgroundImage,
                    PlatForm = topic.Platform,
                    Tags = topic.Tags,
                    ShopId = topic.ShopId,
                    IsRecommend = topic.IsRecommend,
                    SelfDefineText = topic.SelfDefineText,
                };
                //保存数据
                DbFactory.Default.Add(info);

                var modules = topic.TopicModuleInfo
                .Select(item => new TopicModuleInfo
                {
                    TopicId = info.Id,
                    Name = item.Name,
                    TitleAlign = item.TitleAlign,
                }).ToList();
                DbFactory.Default.AddRange(modules);


                var products = new List<ModuleProductInfo>();
                for (int i = 0; i < modules.Count; i++)
                {
                    products.AddRange(topic.TopicModuleInfo[i].ModuleProductInfo
                        .Select(item => new ModuleProductInfo
                        {
                            ModuleId = modules[i].Id,
                            ProductId = item.ProductId,
                            DisplaySequence = item.DisplaySequence
                        }));
                }
                DbFactory.Default.AddRange(products);
            });
        }


        public TopicInfo GetTopicInfo(long id)
        {
            return DbFactory.Default.Get<TopicInfo>().Where(p => p.Id == id).FirstOrDefault();
        }


        public void UpdateTopic(Topic topic)
        {
            bool moduleName = topic.TopicModuleInfo.Any(a => a.Name.Length > 25);
            if (moduleName)
                throw new MallException("模块名称不能超过25个字符");
            try
            {
                DbFactory.Default.InTransaction(() =>
                {
                    //基本信息
                    var model = DbFactory.Default.Get<TopicInfo>(p => p.Id == topic.Id).FirstOrDefault();
                    model.Name = topic.Name;
                    model.BackgroundImage = !string.IsNullOrEmpty(topic.BackgroundImage) ? topic.BackgroundImage : model.BackgroundImage;
                    model.TopImage = !string.IsNullOrEmpty(topic.TopImage) ? topic.TopImage : model.TopImage;
                    model.FrontCoverImage = !string.IsNullOrEmpty(topic.FrontCoverImage) ? topic.FrontCoverImage : model.FrontCoverImage;
                    model.Tags = topic.Tags;
                    model.IsRecommend = topic.IsRecommend;
                    model.SelfDefineText = topic.SelfDefineText;
                    DbFactory.Default.Update(model);

                    var ids = DbFactory.Default.Get<TopicModuleInfo>(p => p.TopicId == topic.Id).Select(p => p.Id).ToList<long>();
                    DbFactory.Default.Del<TopicModuleInfo>(p => p.TopicId == topic.Id);
                    var modules = topic.TopicModuleInfo
                        .Select(item => new TopicModuleInfo
                        {
                            TopicId = model.Id,
                            Name = item.Name,
                            TitleAlign = item.TitleAlign,
                        }).ToList();
                    DbFactory.Default.AddRange(modules);

                    DbFactory.Default.Del<ModuleProductInfo>(p => p.ModuleId.ExIn(ids));
                    var products = new List<ModuleProductInfo>();
                    for (int i = 0; i < modules.Count; i++)
                    {
                        products.AddRange(topic.TopicModuleInfo[i].ModuleProductInfo
                            .Select(item => new ModuleProductInfo
                            {
                                ModuleId = modules[i].Id,
                                ProductId = item.ProductId,
                                DisplaySequence = item.DisplaySequence
                            }));
                    }
                    DbFactory.Default.AddRange(products);
                });
            }
            catch (Exception ex)
            {
                throw new MallException("修改专题失败", ex);
            }

        }

        public void AddTopicInfo(TopicInfo topic)
        {
            DbFactory.Default.Add(topic);
        }
        public void UpdateTopicInfo(TopicInfo topic)
        {
            DbFactory.Default.Set<TopicInfo>()
                .Set(p => p.Name, topic.Name)
                .Set(p => p.Tags, topic.Tags)
                .Set(p => p.PlatForm, topic.PlatForm)
                .Set(p => p.TopImage, topic.TopImage)
                .Where(e => e.Id == topic.Id)
                .Succeed();
        }

        /// <summary>
        /// 图片转移
        /// </summary>
        /// <param name="topicId">专题编号</param>
        /// <param name="backGroundImage">临时背景图地址，返回正式地址</param>
        /// <param name="topImage">临时top图地址，返回正式地址</param>
        /// <returns>专题图片目录</returns>
        string MoveImages(long topicId, ref string backGroundImage, ref string topImage, ref string frontImage)
        {
            string topicImageDir = string.Empty;

            //转移图片
            string relativeDir = "/Storage/Plat/Topic/" + topicId + "/";

            if (backGroundImage.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string backgroundOriUrl = backGroundImage.Substring(backGroundImage.LastIndexOf("/temp"));
                topicImageDir = relativeDir;
                Core.MallIO.CopyFile(backgroundOriUrl, topicImageDir + Path.GetFileName(backGroundImage), true);
                //目标地址
                backGroundImage = topicImageDir + Path.GetFileName(backGroundImage); ;
            }

            if (topImage.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string topImageOriUrl = topImage.Substring(topImage.LastIndexOf("/temp"));

                Core.MallIO.CopyFile(topImageOriUrl, relativeDir + Path.GetFileName(topImage), true);
                topImage = relativeDir + Path.GetFileName(topImage);
            }

            if (!string.IsNullOrWhiteSpace(frontImage) && frontImage.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string frontImageOriUrl = frontImage.Substring(frontImage.LastIndexOf("/temp"));

                Core.MallIO.CopyFile(frontImageOriUrl, relativeDir + Path.GetFileName(frontImage), true);

                frontImage = relativeDir + Path.GetFileName(frontImage);
            }

            return topicImageDir;
        }



    }
}
