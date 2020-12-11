using Mall.Application;
using Mall.CommonModel;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class NearShopBranchController : BaseAdminController
    {
        IMobileHomeTopicService _iMobileHomeTopicService;
        ISlideAdsService _iSlideAdsService;
        private ITopicService _iTopicService;
        public NearShopBranchController(IMobileHomeTopicService iMobileHomeTopicService, ISlideAdsService iSlideAdsService, ITopicService iTopicService)
        {
            _iMobileHomeTopicService = iMobileHomeTopicService;
            _iSlideAdsService = iSlideAdsService;
            _iTopicService = iTopicService;
        }

        public ActionResult NearShopBranchSetting()
        {
            var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> tagList = new List<SelectListItem>{new SelectListItem
            {
                Selected = true,
                Value = 0.ToString(),
                Text = "请选择..."
            }};
            foreach (var item in shopBranchTagInfos)
            {
                tagList.Add(new SelectListItem
                {
                    Selected = false,
                    Value = item.Id.ToString(),
                    Text = item.Title
                });
            }
            ViewBag.ShopBranchTags = tagList;
            //轮播图
            ViewBag.imageAds = _iSlideAdsService.GetSlidAdsOrInit(0, Entities.SlideAdInfo.SlideAdType.NearShopBranchSpecial).ToList();
            //门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.SiteSettings.IsOpenStore;
            return View(SiteSettingApplication.SiteSettings);
        }

        public JsonResult AddSlideImage(string id, string description, string imageUrl, string url, int slideTypeId)
        {
            Result result = new Result();
            var slideAdInfo = new Entities.SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = (Entities.SlideAdInfo.SlideAdType)slideTypeId;
            slideAdInfo.Url = url.ToLower();
            slideAdInfo.Description = description;
            slideAdInfo.ShopId = 0;
            if (slideAdInfo.Id > 0)
                _iSlideAdsService.UpdateSlidAd(slideAdInfo);
            else
                _iSlideAdsService.AddSlidAd(slideAdInfo);
            result.success = true;
            return Json(result);
        }

        public JsonResult GetSlideImages(int id)
        {
            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, (Entities.SlideAdInfo.SlideAdType)id).ToArray();
            var slideImageService = _iSlideAdsService;
            var slideModel = slideImageSettings.Select(item =>
            {
                //var slideImage = slideImageService.GetSlidAd(0, item.Id);

                return new
                {
                    id = item.Id,
                    imgUrl = Core.MallIO.GetImagePath(item.ImageUrl),
                    displaySequence = item.DisplaySequence,
                    url = item.Url + "," + GetLinkName(item.Url),
                    description = item.Description
                };
            });
            return Json(new { rows = slideModel, total = 100 });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SlideImageChangeSequence(int id, int oriRowNumber, int newRowNumber)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, (Entities.SlideAdInfo.SlideAdType)id);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteSlideImage(string id)
        {
            Result result = new Result();
            _iSlideAdsService.DeleteSlidAd(0, Convert.ToInt64(id));
            result.success = true;
            return Json(result);
        }

        public JsonResult GetNearShopBranchIcons()
        {
            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.NearShopBranchIcon).ToArray();
            var slideImageService = _iSlideAdsService;
            var slideModel = slideImageSettings.Select(item =>
            {
                //var slideImage = slideImageService.GetSlidAd(0, item.Id);
                return new
                {
                    id = item.Id,
                    imgUrl = Core.MallIO.GetImagePath(item.ImageUrl),
                    displaySequence = item.DisplaySequence,
                    url = item.Url + "," + GetLinkName(item.Url),
                    description = item.Description
                };
            });
            return Json(new { rows = slideModel, total = 100 });
        }

        public JsonResult AddNearShopBranchIcon(string id, string description, string imageUrl, string url)
        {
            Result result = new Result();
            var slideAdInfo = new Entities.SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = Entities.SlideAdInfo.SlideAdType.NearShopBranchIcon;
            slideAdInfo.Url = url.ToLower();
            slideAdInfo.Description = description;
            slideAdInfo.ShopId = 0;
            if (slideAdInfo.Id > 0)
                _iSlideAdsService.UpdateSlidAd(slideAdInfo);
            else
                _iSlideAdsService.AddSlidAd(slideAdInfo);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        public JsonResult DeleteNearShopBranchIcon(string id)
        {
            Result result = new Result();
            _iSlideAdsService.DeleteSlidAd(0, Convert.ToInt64(id));
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult NSBIconChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, Entities.SlideAdInfo.SlideAdType.NearShopBranchIcon);
            return Json(new Result { success = true });
        }

        [HttpPost]
        public JsonResult ShopList(int page, int rows, string titleKeyword, string tagsId, string addressId, bool? isRecommend)
        {
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = page;
            query.PageSize = rows;
            if (!string.IsNullOrEmpty(titleKeyword))
                query.ShopBranchName = titleKeyword;
            if (!string.IsNullOrEmpty(addressId))
            {
                var regionid = Convert.ToInt32(addressId);
                var region = RegionApplication.GetRegion(regionid);
                switch (region.Level)
                {
                    case Region.RegionLevel.Province: query.ProvinceId = regionid; break;
                    case Region.RegionLevel.City: query.CityId = regionid; break;
                }

            }
            if (!string.IsNullOrEmpty(tagsId))
                query.ShopBranchTagId = Convert.ToInt64(tagsId);
            if (isRecommend.HasValue) query.IsRecommend = isRecommend;

            var shopBranchs = ShopBranchApplication.GetShopBranchs(query);
            var curUrl = CurrentUrlHelper.CurrentUrlNoPort();
            var list = new
            {
                rows = shopBranchs.Models.Select(item => new
                {
                    id = item.Id,
                    name = item.ShopBranchName,
                    imgUrl = item.ShopImages,
                    url = "/" + item.Id,
                    tags = string.IsNullOrWhiteSpace(item.ShopBranchInTagNames) ? "" : item.ShopBranchInTagNames.Replace(",", " ")
                }),
                total = shopBranchs.Total
            };
            return Json(list);
        }

        [HttpPost]
        public JsonResult ShopTagList(int page, int rows, string titleKeyword)
        {
            var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            if (!string.IsNullOrEmpty(titleKeyword)) { shopBranchTagInfos = shopBranchTagInfos.Where(n => n.Title.Contains(titleKeyword)).ToList(); }
            var list = new
            {
                rows = shopBranchTagInfos.Skip((page - 1) * rows).Take(rows).Select(item => new
                {
                    id = item.Id,
                    name = item.Title,
                    imgUrl = "",
                    url = "/" + item.Id,
                    tags = ""
                }),
                total = shopBranchTagInfos.Count
            };
            return Json(list);
        }

        public JsonResult GetImageAd(long id)
        {
            var model = _iSlideAdsService.GetSlidAd(0, id);
            if (null != model)
            {
                return Json(new { success = true, imageUrl = Core.MallIO.GetImagePath(model.ImageUrl), url = model.Url });
            }
            return Json(new { success = false, msg = "未获取到项目" });
        }

        public JsonResult UpdateImageAd(long id, string pic, string url)
        {
            var image = _iSlideAdsService.GetSlidAd(0, id);
            if (!string.IsNullOrWhiteSpace(pic) && (!image.ImageUrl.Equals(pic)))
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
            var slidAd = new Entities.SlideAdInfo { ShopId = 0, Url = url, ImageUrl = pic, Id = id };
            var ret = _iSlideAdsService.UpdateSlidAd(slidAd);
            if (null != ret)
            {
                ret.ImageUrl = Core.MallIO.GetImagePath(ret.ImageUrl);
                return Json(new Result { success = true, data = ret });
            }
            else
            {
                return Json(new Result { success = false, msg = "更新失败" });
            }
        }

        public JsonResult GetRecommendShopBranch()
        {
            var query = new ShopBranchQuery
            {
                IsRecommend = true
            };
            var shopbranchs = ShopBranchApplication.GetAllShopBranchs(query).OrderBy(n => n.RecommendSequence);

            return Json(new { rows = shopbranchs });
        }

        [HttpPost]
        public JsonResult RecommendShopBranch(long[] ids)
        {
            var flag = ShopBranchApplication.RecommendShopBranch(ids);
            return Json(new Result { success = flag, msg = flag ? "" : "未知错误,请重试" });
        }

        public JsonResult RecommendChangeSequence(long oriShopBranchId, long newShopBranchId)
        {
            var flag = ShopBranchApplication.RecommendChangeSequence(oriShopBranchId, newShopBranchId);
            return Json(new Result { success = flag, msg = flag ? "" : "未知错误,请重新排序" });
        }

        public JsonResult ResetShopBranchRecommend(long id)
        {
            var flag = ShopBranchApplication.ResetShopBranchRecommend(id);
            return Json(new Result { success = flag, msg = flag ? "" : "未知错误,请重试" });
        }

        ///// <summary>
        ///// 修改同步小程序状态
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="status"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult ShopBranchSettingUpdateAppletSyncStatus(int type, bool status)
        //{
        //    var setting = SiteSettingApplication.SiteSettings;
        //    switch (type)
        //    {
        //        case 1:
        //            setting.O2OApplet_IsUseTopSlide = status;
        //            break;
        //        case 2:
        //            setting.O2OApplet_IsUseIconArea = status;
        //            break;
        //        case 3:
        //            setting.O2OApplet_IsUseAdArea = status;
        //            break;
        //        case 4:
        //            setting.O2OApplet_IsUseMiddleSlide = status;
        //            break;
        //        default:
        //            return Json(new Result { success = false, status = -1, msg = "错误的操作参数" });
        //    }
        //    SiteSettingApplication.SaveChanges();
        //    return Json(new Result { success = true, status = 1, msg = "操作成功" });
        //}

        private string GetLinkName(string url)
        {
            var arr = url.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 2)
            {
                var strID = arr[1].Substring(arr[1].LastIndexOf('/') + 1);
                long id = 0;
                long.TryParse(strID, out id);
                switch (arr[0])
                {
                    case "1":
                        {
                            var model = ShopBranchApplication.GetShopBranchTagInfo(id, false);
                            if (null != model) return model.Title;
                        }
                        break;
                    case "2":
                        {
                            var model = ShopBranchApplication.GetShopBranchById(id);
                            if (null != model) return model.ShopBranchName;
                        }
                        break;
                    case "3":
                        {
                            var model = TopicApplication.GetTopic(id);
                            if (null != model) return model.Name;
                        }
                        break;
                }
            }
            return "";
        }
    }
}