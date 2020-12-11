using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    [APPAuthorization]
    public class APPShopController : BaseAdminController
    {
        // GET: Admin/APPShop
        //APP首页配置共用于安卓和IOS，这里的平台类型写的为IOS，安卓调用首页接口数据时平台类型也选IOS
        public ActionResult HomePageSetting()
        {
            var homeTopicInfos = MobileHomeTopicApplication.GetMobileHomeTopicInfos(PlatformType.IOS);

            //专题
            ViewBag.imageAds = SlideApplication.GetImageAds(0).Where(p => p.TypeId == ImageAdsType.APPSpecial).ToList();
            //门店授权
            ViewBag.IsOpenStore =  SiteSettingApplication.SiteSettings.IsOpenStore;
            var topics = TopicApplication.GetTopics(homeTopicInfos.Select(p => p.TopicId).ToList());
            var models = homeTopicInfos.Select(item =>
            {
                var topic = topics.FirstOrDefault(p => p.Id == item.TopicId);
                return new TopicModel()
                {
                    FrontCoverImage = topic.FrontCoverImage,
                    Id = item.Id,
                    Name = topic.Name,
                    Tags = topic.Tags,
                    Sequence = item.Sequence
                };
            });
            return View(models);
        }

        //APP首页配置共用于安卓和IOS，这里的平台类型写的为IOS，安卓调用首页接口数据时平台类型也选IOS
        public ActionResult AppHomePageSetting()
        {
            VTemplateEditModel model = new Models.VTemplateEditModel();
            model.ClientType = VTemplateClientTypes.AppIndex;
            model.Name = "APP";
            //门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            VTemplateHelper.DownloadTemplate("", VTemplateClientTypes.AppIndex, 0);
            return View(model);
        }

        #region 轮播图

        public JsonResult AddSlideImage(string id, string description, string imageUrl, string url)
        {
            Result result = new Result();
            var slideAdInfo = new Entities.SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = Entities.SlideAdInfo.SlideAdType.IOSShopHome;
            slideAdInfo.Url = url.ToLower();
            slideAdInfo.Description = description;
            slideAdInfo.ShopId = 0;
            if (slideAdInfo.Id > 0)
                SlideApplication.UpdateSlidAd(slideAdInfo);
            else
                SlideApplication.AddSlidAd(slideAdInfo);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        public JsonResult DeleteSlideImage(string id)
        {
            Result result = new Result();
            SlideApplication.DeleteSlidAd(0, Convert.ToInt64(id));
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SlideImageChangeSequence(int oriRowNumber, int newRowNumber)
        {
            SlideApplication.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, Entities.SlideAdInfo.SlideAdType.IOSShopHome);

            return Json(new Result { success = true });
        }
        public JsonResult GetSlideImages()
        {
            //轮播图
            var slideImageSettings = SlideApplication.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.IOSShopHome).ToArray();
            var slideModel = slideImageSettings.Select(item =>
            {
                var slideImage = SlideApplication.GetSlidAd(0, item.Id);
                return new
                {
                    id = item.Id,
                    imgUrl = Core.MallIO.GetImagePath(item.ImageUrl),
                    displaySequence = item.DisplaySequence,
                    url = item.Url,
                    description = item.Description
                };
            });
            return Json(new { rows = slideModel, total = 100 });
        }

        public JsonResult GetImageAd(long id)
        {
            var models = SlideApplication.GetImageAd(0, id);
            return Json(new { success = true, imageUrl = Core.MallIO.GetImagePath(models.ImageUrl), url = models.Url });
        }
        public JsonResult UpdateImageAd(long id, string pic, string url)
        {
            var image = SlideApplication.GetImageAd(0, id);
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
            var imageAd = new Entities.ImageAdInfo { ShopId = 0, Url = url, ImageUrl = pic, Id = id };
            SlideApplication.UpdateImageAd(imageAd);
            return Json(new Result { success = true });
        }

        #endregion


        public ActionResult APPGuidePages()
        {
            var slideImage = SlideApplication.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.AppGuide).OrderBy(a => a.DisplaySequence).ToList();
            var model = slideImage.Select(a => new Mall.DTO.SlideAdModel() { Id = a.Id, DisplaySequence = a.DisplaySequence, ImageUrl = a.ImageUrl }).ToList();
            return View(model);
        }

        [HttpPost]
        public JsonResult APPGuidePages(List<string> pics)
        {
            List<Mall.DTO.SlideAdModel> list = new List<DTO.SlideAdModel>();
            foreach (var p in pics)
            {
                if (!string.IsNullOrWhiteSpace(p))
                {
                    list.Add(new DTO.SlideAdModel() { ImageUrl = p });
                }
            }
            if (list.Count == 0)
            {
                throw new MallException("至少上传一张引导页图");
            }
            SlideApplication.AddGuidePages(list);
            return Json(new Result { success = true });
        }





        #region App图标
        public JsonResult AddAppIcon(string id, string description, string imageUrl, string url)
        {
            Result result = new Result();
            var slideAdInfo = new Entities.SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = Entities.SlideAdInfo.SlideAdType.APPIcon;
            slideAdInfo.Url = url.ToLower().Replace("/m-wap", "/m-ios").Replace("/m-weixin", "/m-ios");
            slideAdInfo.Description = description;
            slideAdInfo.ShopId = 0;
            if (slideAdInfo.Id > 0)
                SlideApplication.UpdateSlidAd(slideAdInfo);
            else
                SlideApplication.AddSlidAd(slideAdInfo);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult AppIconChangeSequence(int oriRowNumber, int newRowNumber)
        {
            SlideApplication.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, Entities.SlideAdInfo.SlideAdType.APPIcon);
            return Json(new Result { success = true });
        }

        public JsonResult GetAPPIcons()
        {
            //轮播图
            var slideImageSettings = SlideApplication.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.APPIcon).ToArray();
            var slideModel = slideImageSettings.Select(item =>
            {
                var slideImage = SlideApplication.GetSlidAd(0, item.Id);
                return new
                {
                    id = item.Id,
                    imgUrl = Core.MallIO.GetImagePath(item.ImageUrl),
                    displaySequence = item.DisplaySequence,
                    url = item.Url,
                    description = item.Description
                };
            });
            return Json(new { rows = slideModel, total = 100 });
        }

        [HttpPost]
        public JsonResult DeleteAppIcon(string id)
        {
            Result result = new Result();
            SlideApplication.DeleteSlidAd(0, Convert.ToInt64(id));
            result.success = true;
            return Json(result);
        }

        #endregion

        #region
        public ActionResult Management()
        {
            var AgreementTypes = Entities.AgreementInfo.AgreementTypes.APP;
            return View(GetManagementModel(AgreementTypes));
        }

        [HttpPost]
        public JsonResult GetManagement(int agreementType)
        {
            return Json(GetManagementModel((Entities.AgreementInfo.AgreementTypes)agreementType));
        }

        public AgreementModel GetManagementModel(Entities.AgreementInfo.AgreementTypes type)
        {
            AgreementModel model = new AgreementModel();
            model.AgreementType = type;
            var agreement = SystemAgreementApplication.GetAgreement(type);
            if (agreement != null)
            {
                model.AgreementContent = agreement.AgreementContent;
            }
            return model;
        }

        [HttpPost]
     
        public JsonResult UpdateAgreement(int agreementType, string agreementContent)
        {
            var model = SystemAgreementApplication.GetAgreement((Entities.AgreementInfo.AgreementTypes)agreementType);
            bool isSuccess = false;
            if (model == null)
            {//第一次修改，则新增
                model = new Entities.AgreementInfo();
                model.AgreementType = agreementType.ToEnum<Entities.AgreementInfo.AgreementTypes>(Entities.AgreementInfo.AgreementTypes.Buyers);
                model.AgreementContent = ProcessHtml(agreementContent);
                SystemAgreementApplication.AddAgreement(model);
                isSuccess = true;
            }
            else
            {
                model.AgreementType = agreementType.ToEnum<Entities.AgreementInfo.AgreementTypes>(Entities.AgreementInfo.AgreementTypes.Buyers);
                model.AgreementContent = ProcessHtml(agreementContent);
                isSuccess = SystemAgreementApplication.UpdateAgreement(model);
            }
            if (isSuccess)
                return Json(new Result() { success = true, msg = "更新协议成功！" });
            else
                return Json(new Result() { success = false, msg = "更新协议失败！" });
        }
        /// <summary>
        /// 处理图片地址
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ProcessHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            var descriptionImagePath = "/Storage/Plat/APP/About";
            html = Core.Helper.HtmlContentHelper.TransferToLocalImage(html, "/", descriptionImagePath, Core.MallIO.GetImagePath(descriptionImagePath) + "/");
            html = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(html);
            return html;
        }
        #endregion

        #region 商品配置
        public ActionResult ProductSetting()
        {
            return View();
        }
        #endregion
    }
}