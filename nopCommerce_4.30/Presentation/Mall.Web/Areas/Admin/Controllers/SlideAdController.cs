using Mall.IServices;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    public class SlideAdController : BaseAdminController
    {
        ISlideAdsService _iSlideAdsService;
        public SlideAdController(ISlideAdsService iSlideAdsService)
        {
            _iSlideAdsService = iSlideAdsService;
        }
        #region 手动轮播图片
        // GET: Admin/SlideAd
        public ActionResult HandSlideManagement()
        {
            return View();
        }

        [UnAuthorize]
        public JsonResult GetHandSlideJson()
        {
            var data = _iSlideAdsService.GetHandSlidAds();
            IEnumerable<HandSlideModel> slide = data.ToArray().Select(item => new HandSlideModel()
            {
                Id = item.Id,
                Pic = Core.MallIO.GetImagePath(item.ImageUrl),
                URL = item.Url,
                Index = item.DisplaySequence
            });

            DataGridModel<HandSlideModel> dataGrid = new DataGridModel<HandSlideModel>() { rows = slide, total = slide.Count() };
            return Json(dataGrid);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult AddHandSlideAd(string pic, string url)
        {
            var slide = new Entities.HandSlideAdInfo() { ImageUrl = pic, Url = url, DisplaySequence = 0 };
            if (!string.IsNullOrWhiteSpace(pic))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = Path.Combine(dest, Path.GetFileName(source));
                    Core.MallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }

                slide.ImageUrl = pic;
            }
            _iSlideAdsService.AddHandSlidAd(slide);
            return Json(new Result{ success = true });
        }

        [UnAuthorize]
        public JsonResult DeleteHandSlide(long Id)
        {
            _iSlideAdsService.DeleteHandSlidAd(Id);
            return Json(new Result { success = true });
        }

        [UnAuthorize]
        public JsonResult EditHandSlideAd(long id, string pic, string url)
        {
            var slide = _iSlideAdsService.GetHandSlidAd(id);

            if (!string.IsNullOrWhiteSpace(pic) && (!slide.ImageUrl.Equals(pic)))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = Path.Combine(dest, Path.GetFileName(source));
                    Core.MallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }
            }

            _iSlideAdsService.UpdateHandSlidAd(new Entities.HandSlideAdInfo
            {
                Id = id,
                ImageUrl = pic,
                Url = url
            });
            return Json(new Result { success = true });
        }

        [HttpPost]
        [UnAuthorize]
        public ActionResult AdjustHandSlideIndex(long id, int direction)
        {
            _iSlideAdsService.AdjustHandSlidAdIndex(id, direction == 1);
            return Json(new Result { success = true });
        }

        #endregion

        #region 自动轮播图

        [UnAuthorize]
        public JsonResult GetSlideJson()
        {
            var data = _iSlideAdsService.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.PlatformHome);
            IEnumerable<HandSlideModel> slide = data.ToArray().Select(item => new HandSlideModel()
            {
                Id = item.Id,
                Pic = Core.MallIO.GetImagePath(item.ImageUrl),
                URL = item.Url,
                Index = item.DisplaySequence
            });

            DataGridModel<HandSlideModel> dataGrid = new DataGridModel<HandSlideModel>() { rows = slide, total = slide.Count() };
            return Json(dataGrid);
        }
        public ActionResult SlideManagement()
        {
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult AddSlideAd(string pic, string url)
        {
            var slide = new Entities.SlideAdInfo() { ImageUrl = pic, Url = url, ShopId = 0, DisplaySequence = 0, TypeId = Entities.SlideAdInfo.SlideAdType.PlatformHome };
            if (!string.IsNullOrWhiteSpace(pic))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = Path.Combine(dest, Path.GetFileName(source));
                    Core.MallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }

                slide.ImageUrl = pic;
            }
            _iSlideAdsService.AddSlidAd(slide);
            return Json(new Result{ success = true });
        }

        [UnAuthorize]
        public JsonResult DeleteSlide(long Id)
        {
            _iSlideAdsService.DeleteSlidAd(0, Id);
            return Json(new Result { success = true });
        }

        [UnAuthorize]
        public JsonResult EditSlideAd(long id, string pic, string url)
        {
            var slide = _iSlideAdsService.GetSlidAd(0, id);

            if (!string.IsNullOrWhiteSpace(pic) && (!slide.ImageUrl.Equals(pic)))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = Path.Combine(dest, Path.GetFileName(source));
                    Core.MallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }
            }

            _iSlideAdsService.UpdateSlidAd(new Entities.SlideAdInfo
            {
                Id = id,
                ImageUrl = pic,
                Url = url
            });
            return Json(new Result { success = true });
        }

        [UnAuthorize]
        [HttpPost]
        public ActionResult AdjustSlideIndex(long id, int direction)
        {
            _iSlideAdsService.AdjustSlidAdIndex(0, id, direction == 1, Entities.SlideAdInfo.SlideAdType.PlatformHome);
            return Json(new Result { success = true });
        }

        #endregion

        #region 普通广告图片

        [HttpPost]
        [UnAuthorize]
        public ActionResult UpdateImageAd(long id, string pic, string url)
        {
            var image = _iSlideAdsService.GetImageAd(0, id);

            if (!string.IsNullOrWhiteSpace(pic) && (!image.ImageUrl.Equals(pic)))
            {
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = Path.Combine(dest, Path.GetFileName(source));
                    Core.MallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage/"));
                }
            }
            var imageAd = new Entities.ImageAdInfo { ShopId = 0, Url = url, ImageUrl = pic, Id = id };
            _iSlideAdsService.UpdateImageAd(imageAd);
            return Json(new Result { success = true });
        }

        #endregion

    }
}