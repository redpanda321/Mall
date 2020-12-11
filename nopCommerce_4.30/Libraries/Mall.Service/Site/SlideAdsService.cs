using Mall.IServices;
using System.Collections.Generic;
using System.Linq;
using Mall.Entities;
using NetRube.Data;

namespace Mall.Service
{
    public class SlideAdsService : ServiceBase, ISlideAdsService
    {
        #region 手动轮播广告图片

        private void ResetHandSlideAdIndexFrom(long index)
        {
            var data = DbFactory.Default.Get<HandSlideAdInfo>().Where(s => s.DisplaySequence > index).OrderByDescending(s => s.DisplaySequence).ToList();
            DbFactory.Default.InTransaction(() =>
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (i == data.Count - 1)
                    {
                        data[i].DisplaySequence = index;
                    }
                    else
                    {
                        data[i].DisplaySequence = data[i + 1].DisplaySequence;
                    }
                    DbFactory.Default.Update(data[i]);
                }
            });
        }
        public List<HandSlideAdInfo> GetHandSlidAds()
        {
            return DbFactory.Default.Get<HandSlideAdInfo>().OrderBy(s => s.DisplaySequence).ToList();
        }

        public void AddHandSlidAd(HandSlideAdInfo model)
        {
            var data = DbFactory.Default.Get<HandSlideAdInfo>().Max<long>(s => s.DisplaySequence);

            //修改Model的Index
            model.DisplaySequence = data + 1;

            DbFactory.Default.Add(model);
        }

        public void DeleteHandSlidAd(long id)
        {
            var index = DbFactory.Default.Get<HandSlideAdInfo>().Where(s => s.Id == id).FirstOrDefault().DisplaySequence;
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<HandSlideAdInfo>(s => s.Id == id);
                ResetHandSlideAdIndexFrom(index);
            });
        }

        public void UpdateHandSlidAd(HandSlideAdInfo models)
        {
          
            DbFactory.Default.Set<HandSlideAdInfo>()
                .Set(n => n.ImageUrl, models.ImageUrl)
                .Set(n => n.Url, models.Url)
                .Where(n => n.Id == models.Id)
                .Succeed();
        }

        public void AdjustHandSlidAdIndex(long id, bool direction)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var slide = DbFactory.Default.Get<HandSlideAdInfo>().Where(s => s.Id == id).FirstOrDefault();
                if (direction)
                {
                    var prev = DbFactory.Default.Get<HandSlideAdInfo>().Where(s => s.DisplaySequence == (slide.DisplaySequence - 1)).FirstOrDefault();
                    slide.DisplaySequence = slide.DisplaySequence - 1;
                    prev.DisplaySequence = prev.DisplaySequence + 1;
                    DbFactory.Default.Update(slide);
                    DbFactory.Default.Update(prev);

                }
                else
                {
                    var next = DbFactory.Default.Get<HandSlideAdInfo>().Where(s => s.DisplaySequence == (slide.DisplaySequence + 1)).FirstOrDefault();
                    slide.DisplaySequence = slide.DisplaySequence + 1;
                    next.DisplaySequence = next.DisplaySequence - 1;
                    DbFactory.Default.Update(slide);
                    DbFactory.Default.Update(next);
                }
            });
        }


        public HandSlideAdInfo GetHandSlidAd(long id)
        {
            return DbFactory.Default.Get<HandSlideAdInfo>().Where(s => s.Id == id).FirstOrDefault();
        }

        #endregion

        #region 自动轮播广告图片
        public List<SlideAdInfo> GetSlidAds(long shopId, SlideAdInfo.SlideAdType type)
        {
            return DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == shopId && s.TypeId == type).OrderBy(t => t.DisplaySequence).ToList();
        }
        public IEnumerable<SlideAdInfo> GetSlidAdsByTypes(long shopId, IEnumerable<SlideAdInfo.SlideAdType> types)
        {
            return DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == shopId && s.TypeId.ExIn(types)).OrderBy(t => t.DisplaySequence).ToList();
        }
        public IEnumerable<SlideAdInfo> GetSlidAdsOrInit(long shopId, SlideAdInfo.SlideAdType? slidAdsType = SlideAdInfo.SlideAdType.Initial)
        {
            var sliAdInfos = DbFactory.Default.Get<SlideAdInfo>().Where(i => i.ShopId == shopId).OrderBy(i => i.Id);
            if (slidAdsType != SlideAdInfo.SlideAdType.Initial)
            {
                sliAdInfos.Where(item => item.TypeId == slidAdsType);
            }
            var rets = sliAdInfos.ToList();
            if (rets.Count == 0)
            {
                DbFactory.Default.InTransaction(() =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var newSlideAdInfo = new SlideAdInfo { ImageUrl = "", ShopId = shopId, Url = "", TypeId = slidAdsType.Value };
                        DbFactory.Default.Add<SlideAdInfo>(newSlideAdInfo);
                        rets.Add(newSlideAdInfo);
                    }
                });
            }
            return rets;
        }

        public void AddSlidAd(SlideAdInfo model)
        {
            string imgUrl = string.Empty;
            var data = DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == model.ShopId).Max<long>(s => s.DisplaySequence);
            if ((model.TypeId == SlideAdInfo.SlideAdType.VShopHome || model.TypeId == SlideAdInfo.SlideAdType.WeixinHome)
                && DbFactory.Default.Get<SlideAdInfo>().Where(item => item.ShopId == model.ShopId && item.TypeId == model.TypeId).Count() + 1 > 5)
                throw new Mall.Core.MallException("最多只能添加5张轮播图");
            DbFactory.Default.InTransaction(() =>
            {
                //修改Model的Index
                model.DisplaySequence = data + 1;

                DbFactory.Default.Add(model);
                imgUrl = model.ImageUrl;
                imgUrl = MoveImages(ref imgUrl, model.TypeId, model.ShopId);
                DbFactory.Default.Set<SlideAdInfo>().Set(n => n.ImageUrl, imgUrl).Where(n => n.Id == model.Id).Succeed();
            });
        }

        /// <summary>
        /// 添加APP引导页
        /// </summary>
        /// <param name="models"></param>
        public void AddGuidePages(List<SlideAdInfo> models)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<SlideAdInfo>(p => p.TypeId == SlideAdInfo.SlideAdType.AppGuide);
                DbFactory.Default.Add<SlideAdInfo>(models);
            });
        }

        private void ResetSlideAdIndexFrom(long shopId, long index)
        {
            var data = DbFactory.Default.Get<SlideAdInfo>().Where(s => s.DisplaySequence > index && s.ShopId == shopId).
                OrderByDescending(s => s.DisplaySequence).ToList();
            DbFactory.Default.InTransaction(() =>
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    if (i == data.Count - 1)
                    {
                        item.DisplaySequence = index;
                    }
                    else
                    {
                        item.DisplaySequence = data[i + 1].DisplaySequence;
                    }
                    DbFactory.Default.Update(item);
                }
            });
        }

        public void DeleteSlidAd(long shopId, long id)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var index = DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == shopId && s.Id == id).FirstOrDefault().DisplaySequence;
                DbFactory.Default.Del<SlideAdInfo>(s => s.ShopId == shopId && s.Id == id);
                ResetSlideAdIndexFrom(shopId, index);
            });
        }

        public SlideAdInfo UpdateSlidAd(SlideAdInfo models)
        {
            string imgUr = string.Empty;
            var slide = DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == models.ShopId && s.Id == models.Id).FirstOrDefault();
            slide.Description = models.Description;
            slide.Url = models.Url;

            imgUr = models.ImageUrl;
            imgUr = MoveImages(ref imgUr, models.TypeId, models.ShopId);
            slide.ImageUrl = imgUr;
            var count = DbFactory.Default.Update(slide);
            if (count > 0)
            {
                return slide;
            }
            return null;
        }

        public void AdjustSlidAdIndex(long shopId, long id, bool direction, SlideAdInfo.SlideAdType type)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var slide = DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == shopId && s.Id == id && s.TypeId == type).FirstOrDefault();
                if (direction)
                {
                    var prev = DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == shopId && s.TypeId == type).OrderByDescending(s => s.DisplaySequence).Where(s => s.DisplaySequence < slide.DisplaySequence).FirstOrDefault();
                    if (null != prev)
                    {
                        var temp = slide.DisplaySequence;
                        slide.DisplaySequence = prev.DisplaySequence;
                        prev.DisplaySequence = temp;
                        DbFactory.Default.Update(slide);
                        DbFactory.Default.Update(prev);
                    }

                }
                else
                {
                    var next = DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == shopId && s.TypeId == type).OrderBy(s => s.DisplaySequence).Where(s => s.DisplaySequence > slide.DisplaySequence).FirstOrDefault();

                    if (null != next)
                    {
                        var temp = slide.DisplaySequence;
                        slide.DisplaySequence = next.DisplaySequence;
                        next.DisplaySequence = temp;
                    }
                    DbFactory.Default.Update(slide);
                    DbFactory.Default.Update(next);
                }
            });
            //Context.SaveChanges();
        }


        public SlideAdInfo GetSlidAd(long shopId, long id)
        {
            return DbFactory.Default.Get<SlideAdInfo>().Where(s => s.ShopId == shopId && s.Id == id).FirstOrDefault();
        }

        #endregion


        #region 普通广告图片

        public void UpdateImageAd(ImageAdInfo model)
        {
            //var imageAd = Context.ImageAdInfo.FirstOrDefault(i => i.ShopId == model.ShopId && i.Id == model.Id);
            var imageAd = DbFactory.Default.Get<ImageAdInfo>().Where(i => i.ShopId == model.ShopId && i.Id == model.Id).FirstOrDefault();
            if (null != imageAd && imageAd.Id == model.Id)
            {
                imageAd.ImageUrl = model.ImageUrl;
                imageAd.Url = model.Url;
                DbFactory.Default.Update(imageAd);
            }
            //Context.SaveChanges();


        }

        public ImageAdInfo GetImageAd(long shopId, long id)
        {
            //return Context.ImageAdInfo.FirstOrDefault(item => item.Id == id && item.ShopId == shopId);
            return DbFactory.Default.Get<ImageAdInfo>().Where(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
        }

        public IEnumerable<ImageAdInfo> GetImageAds(long shopId, Mall.CommonModel.ImageAdsType? ImageAdsType = Mall.CommonModel.ImageAdsType.Initial)
        {
            //IEnumerable<ImageAdInfo> imageAdInfos = Context.ImageAdInfo.FindBy(i => i.ShopId == shopId).OrderBy(i => i.Id);
            var imageAdInfos = DbFactory.Default.Get<ImageAdInfo>().Where(i => i.ShopId == shopId).OrderBy(i => i.Id);
            if (ImageAdsType != Mall.CommonModel.ImageAdsType.Initial)
            {
                imageAdInfos.Where(item => item.TypeId == ImageAdsType);
            }
            var rets = imageAdInfos.ToList();
            if (rets.Count == 0)
            {
                var newImageAdInfo = new ImageAdInfo() { ImageUrl = "", ShopId = shopId, Url = "", IsTransverseAD = true };
                //Context.ImageAdInfo.AddRange(newImageAdInfos);
                //Context.SaveChanges();

                DbFactory.Default.Add<ImageAdInfo>(newImageAdInfo);
                rets.Add(newImageAdInfo);

                for (int i = 0; i < 4; i++)
                {
                    newImageAdInfo = new ImageAdInfo { ImageUrl = "", ShopId = shopId, Url = "", IsTransverseAD = false };

                    DbFactory.Default.Add<ImageAdInfo>(newImageAdInfo);
                    rets.Add(newImageAdInfo);
                }

            }
            if (!rets.Any(p => p.IsTransverseAD))
            {
                ImageAdInfo info = new ImageAdInfo { ImageUrl = "", ShopId = shopId, Url = "", IsTransverseAD = true };
                //Context.ImageAdInfo.Add(info);
                //Context.SaveChanges();
                DbFactory.Default.Add(info);
            }
            return rets;
        }

        #endregion


        public void UpdateWeixinSlideSequence(long shopId, long sourceSequence, long destiSequence, SlideAdInfo.SlideAdType type)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var souce = DbFactory.Default.Get<SlideAdInfo>().Where(item => item.DisplaySequence == sourceSequence && item.ShopId == shopId && item.TypeId == type).FirstOrDefault();
                var destination = DbFactory.Default.Get<SlideAdInfo>().Where(item => item.DisplaySequence == destiSequence && item.ShopId == shopId && item.TypeId == type).FirstOrDefault();
                souce.DisplaySequence = destiSequence;
                destination.DisplaySequence = sourceSequence;
                DbFactory.Default.Update(souce);
                DbFactory.Default.Update(destination);
            });
        }
        public bool DelSlideAdByType(long shopId, SlideAdInfo.SlideAdType type)
        {
            return DbFactory.Default.Del<SlideAdInfo>(s => s.ShopId == shopId && s.TypeId == type) > 0;
        }
        public void BatchAddShopHome(List<SlideAdInfo> slides, long shopId)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DelSlideAdByType(shopId, SlideAdInfo.SlideAdType.ShopHome);
                DbFactory.Default.Add<SlideAdInfo>(slides);
            });
        }


        /// <summary>
        /// 图片转移
        /// </summary>
        /// <param name="topicId">专题编号</param>
        /// <param name="backGroundImage">临时背景图地址，返回正式地址</param>
        /// <param name="topImage">临时top图地址，返回正式地址</param>
        /// <returns>专题图片目录</returns>
        string MoveImages(ref string backGroundImage, SlideAdInfo.SlideAdType type, long shopId)
        {
            string imageDir = string.Empty;
            string relativeDir = string.Empty;
            if (type == SlideAdInfo.SlideAdType.WeixinHome)
            {
                //转移图片
                imageDir = "/Storage/Plat/Weixin/SlidAd/";
                relativeDir = "/Storage/Plat/Weixin/SlidAd/";
            }

            if (type == SlideAdInfo.SlideAdType.VShopHome)
            {
                imageDir = "/Storage/Shop/" + shopId + "/VShop/";
                relativeDir = "/Storage/Shop/" + shopId + "/VShop/";

            }
            if (type == SlideAdInfo.SlideAdType.IOSShopHome)
            {
                imageDir = "/Storage/Plat/APP/SlidAd/";
                relativeDir = "/Storage/Plat/APP/SlidAd/";
            }
            backGroundImage = backGroundImage.Replace("\\", "/");

            if (!string.IsNullOrWhiteSpace(imageDir))//只有在临时目录中的图片才需要复制
            {
                if (backGroundImage.Contains("/temp/"))
                {
                    if (!string.IsNullOrWhiteSpace(backGroundImage))
                    {
                        string logoname = backGroundImage.Substring(backGroundImage.LastIndexOf('/') + 1);
                        string oldlogo = backGroundImage.Substring(backGroundImage.LastIndexOf("/temp"));
                        string newLogo = imageDir + logoname;
                        Core.MallIO.CopyFile(oldlogo, newLogo, true);
                        backGroundImage = newLogo;
                    }
                }
                else if (backGroundImage.Contains("/Storage/"))
                {
                    backGroundImage = backGroundImage.Substring(backGroundImage.LastIndexOf("/Storage"));
                }
            }

            return backGroundImage;
        }
    }
}
