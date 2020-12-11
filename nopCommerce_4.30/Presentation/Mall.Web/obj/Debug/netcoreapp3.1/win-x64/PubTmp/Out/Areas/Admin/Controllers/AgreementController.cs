using Mall.Application;
using Mall.Web.Framework;
using Mall.Web.Models;
using Mall.Core;
using System;
using System.Drawing;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class AgreementController : BaseAdminController
    {
        // GET: Admin/Agreement
        public ActionResult Management()
        {
            var AgreementTypes = Entities.AgreementInfo.AgreementTypes.Buyers;
            if (!string.IsNullOrEmpty(Request.Query["type"]) && Request.Query["type"].Equals("Seller"))
            {
                AgreementTypes = Entities.AgreementInfo.AgreementTypes.Seller;
            }
            //初始化默认返回买家注册协议
            return View(GetManagementModel(AgreementTypes));
        }
        /// <summary>
        /// 入驻链接
        /// </summary>
        /// <returns></returns>
        public ActionResult SettledLink()
        {
            #region 商家入驻链接和二维码
            string LinkUrl = String.Format("{0}/m-weixin/shopregister/step1", CurrentUrlHelper.CurrentUrlNoPort());
            ViewBag.LinkUrl = LinkUrl;
            string qrCodeImagePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(LinkUrl))
            {
                Bitmap map;
                map = Core.Helper.QRCodeHelper.Create(LinkUrl);
                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                qrCodeImagePath = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray()); // 将图片内存流转成base64,图片以DataURI形式显示  
                ms.Dispose();
            }
            ViewBag.Imgsrc = qrCodeImagePath;
            #endregion
            return View();
        }

        public ActionResult EnterSet()
        {
            //入驻参数设置
            return View();
        }


        [HttpPost]
        public JsonResult GetManagement(int agreementType)
        {
            return Json(GetManagementModel((Entities.AgreementInfo.AgreementTypes)agreementType));
        }

        public AgreementModel GetManagementModel(Entities.AgreementInfo.AgreementTypes type)
        {
            AgreementModel model = new AgreementModel();
            var iAgreement = SystemAgreementApplication.GetAgreement(type);
            if (iAgreement != null)
            {
                model.AgreementType = iAgreement.AgreementType;
                model.AgreementContent = iAgreement.AgreementContent;
            }
            return model;

        }

        [HttpPost]
       
        public JsonResult UpdateAgreement(int agreementType, string agreementContent)
        {
            var model = SystemAgreementApplication.GetAgreement((Entities.AgreementInfo.AgreementTypes)agreementType);
            model.AgreementType = agreementType.ToEnum<Entities.AgreementInfo.AgreementTypes>(Entities.AgreementInfo.AgreementTypes.Buyers);
            model.AgreementContent = agreementContent;
            if (SystemAgreementApplication.UpdateAgreement(model))
                return Json(new Result() { success = true, msg = "更新协议成功！" });
            else
                return Json(new Result() { success = false, msg = "更新协议失败！" });
        }

        #region 入驻设置
        public ActionResult Settled()
        {
            var model = ShopApplication.GetSettled();
            return View(model);
        }


        /// <summary>
        /// 商家入驻设置
        /// </summary>
        /// <param name="mSettled"></param>
        /// <returns></returns>
        public JsonResult setSettled(Mall.DTO.Settled mSettled)
        {
            ShopApplication.Settled(mSettled);
            return Json(new Result() { success = true, msg = "设置成功！" });
        }

        #endregion
    }
}