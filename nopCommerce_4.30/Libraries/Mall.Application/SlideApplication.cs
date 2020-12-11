using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Mall.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class SlideApplication
    {
        private static ISlideAdsService _iSlideAdsService =  EngineContext.Current.Resolve<ISlideAdsService>();

        public static void AddGuidePages(List<SlideAdModel> model)
        {
            var guides = model.Where(a => !string.IsNullOrWhiteSpace(a.ImageUrl)).ToList();
            List<SlideAdInfo> Ads = new List<SlideAdInfo>();
            var index = 1;
            foreach (var m in model)
            {
                var ad = new SlideAdInfo();
                var pic = m.ImageUrl;
                if (!string.IsNullOrWhiteSpace(pic))
                {
                    //转移图片
                    if (pic.Contains("/temp/"))
                    {
                        string source = pic.Substring(pic.LastIndexOf("/temp"));
                        string dest = @"/Storage/Plat/ImageAd/";
                        pic = Path.Combine(dest, Path.GetFileName(source));
                        Core.MallIO.CopyFile(source, pic, true);
                    }
                    else if (pic.Contains("/Storage/"))
                    {
                        pic = pic.Substring(pic.LastIndexOf("/Storage"));
                    }
                }
                ad.ImageUrl = pic;
                ad.TypeId = SlideAdInfo.SlideAdType.AppGuide;
                ad.ShopId = 0;
                ad.DisplaySequence = index;
                ad.Url = string.Empty;
                index++;
                Ads.Add(ad);
            }
            _iSlideAdsService.AddGuidePages(Ads);
        }


        public static List<Mall.DTO.SlideAdModel> GetGuidePages()
        {
            var models = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.AppGuide).OrderBy(a => a.DisplaySequence).ToList();
            var m = models.Select(a => new Mall.DTO.SlideAdModel()
            {
                Id = a.Id,
                ShopId = a.ShopId,
                DisplaySequence = a.DisplaySequence,
                ImageUrl = a.ImageUrl,
                TypeId = a.TypeId
            }).ToList();
            return m;
        }
        /// <summary>
        /// 取周边门店轮播图、图标
        /// </summary>
        /// <returns></returns>
        public static List<Mall.DTO.SlideAdModel> GetShopBranchListSlide()
        {
            List<SlideAdInfo.SlideAdType> slideTypes = new List<SlideAdInfo.SlideAdType>() { };
            slideTypes.Add(SlideAdInfo.SlideAdType.NearShopBranchHome);//轮播图1
            slideTypes.Add(SlideAdInfo.SlideAdType.NearShopBranchIcon);//图标
            slideTypes.Add(SlideAdInfo.SlideAdType.NearShopBranchSpecial);//广告图
            slideTypes.Add(SlideAdInfo.SlideAdType.NearShopBranchHome2);//轮播图2

            var models = _iSlideAdsService.GetSlidAdsByTypes(0, slideTypes).OrderBy(a => a.DisplaySequence);
            var m = models.Select(a => new Mall.DTO.SlideAdModel()
            {
                Id = a.Id,
                ShopId = a.ShopId,
                DisplaySequence = a.DisplaySequence,
                ImageUrl = Mall.Core.MallIO.GetRomoteImagePath(a.ImageUrl),
                TypeId = a.TypeId,
                Url = a.Url,
                Description = a.Description
            }).ToList();
            return m;
        }

        public static IEnumerable<ImageAdInfo> GetImageAds(long shopId, Mall.CommonModel.ImageAdsType? ImageAdsType = Mall.CommonModel.ImageAdsType.Initial)
        {
            return _iSlideAdsService.GetImageAds(shopId, ImageAdsType);
        }

        public static ImageAdInfo GetImageAd(long shopId, long id)
        {
            return _iSlideAdsService.GetImageAd(shopId, id);
        }

        public static void UpdateImageAd(ImageAdInfo model)
        {
            _iSlideAdsService.UpdateImageAd(model);
        }
        /// <summary>
        /// 更新一个自动轮播广告图片
        /// </summary>
        /// <param name="models"></param>
        public static SlideAdInfo UpdateSlidAd(SlideAdInfo models)
        {
            return _iSlideAdsService.UpdateSlidAd(models);
        }

        /// <summary>
        /// 添加一个自动轮播广告图片
        /// </summary>
        /// <param name="model"></param>
        public static void AddSlidAd(SlideAdInfo model)
        {
            _iSlideAdsService.AddSlidAd(model);
        }

        /// <summary>
        /// 删除一个自动轮播广告图片
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteSlidAd(long shopId, long id)
        {
            _iSlideAdsService.DeleteSlidAd(shopId, id);
        }

        /// <summary>
        /// 排序上下移
        /// </summary>
        /// <param name="sourceSequence"></param>
        /// <param name="destiSequence"></param>
        public static void UpdateWeixinSlideSequence(long shopId, long sourceSequence, long destiSequence, SlideAdInfo.SlideAdType type)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(shopId, sourceSequence, destiSequence, type);
        }

        /// <summary>
        /// 获取自动轮播广告图片
        /// </summary>
        /// <returns></returns>
        public static IList<SlideAdInfo> GetSlidAds(long shopId, SlideAdInfo.SlideAdType type)
        {
            var slidAds = _iSlideAdsService.GetSlidAds(shopId, type).ToList();
            return slidAds;
        }

        /// <summary>
        /// 获取一个自动轮播广告图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SlideAdInfo GetSlidAd(long shopId, long id)
        {
            return _iSlideAdsService.GetSlidAd(shopId, id);
        }
        public static bool DelSlideAdByType(long shopId, SlideAdInfo.SlideAdType type)
        {
            return _iSlideAdsService.DelSlideAdByType(shopId, type);
        }
        /// <summary>
        /// 保存店铺首页轮播图
        /// </summary>
        /// <param name="slides"></param>
        /// <param name="shopId"></param>
        public static void BatchAddShopHome(List<SlideAdInfo> slides, long shopId)
        {
            _iSlideAdsService.BatchAddShopHome(slides, shopId);
        }

    }
}
