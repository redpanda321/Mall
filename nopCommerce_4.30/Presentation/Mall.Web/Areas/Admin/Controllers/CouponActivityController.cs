using Mall.Web.Framework;
using System;

using System.IO;
using Mall.Application;
using System.Linq;
using Mall.CommonModel;
using Mall.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    [MarketingAuthorization]
    public class CouponActivityController : BaseAdminController
    {
        public CouponActivityController()
        {
        }

        public ActionResult Index()
        {
            string link = string.Format("{0}/m-Wap/RegisterActivity/Gift", CurrentUrlHelper.CurrentUrlNoPort());
            var map = Core.Helper.QRCodeHelper.Create(link);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            ViewBag.QR = strUrl;
            ViewBag.link = link;


            var model = CouponApplication.GetCouponSendByRegister();
            return View(model);
        }

        /// <summary>
        /// 修改注册赠送优惠券设置
        /// </summary>
        /// <param name="CouponRegisterId">设置主键ID</param>
        /// <param name="status">状态</param>
        /// <param name="couponIds">优惠券ID，用','隔开</param>
        /// <returns></returns>
        public JsonResult Update(long CouponRegisterId, CouponSendByRegisterStatus status, string couponIds)
        {
            var coupons = couponIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => long.Parse(p))
                .ToList();
            if (status ==  CouponSendByRegisterStatus.Open && coupons.Count == 0)
                return Json(new Result() { success = false, msg = "请选择优惠券" });

            var model = new CouponSendByRegisterModel()
            {
                Id = CouponRegisterId,
                Link = "#",
                Status = status,
                CouponIds = coupons.Select(p => new CouponModel { Id = p }).ToList()
            };
            CouponApplication.SetCouponSendByRegister(model);
            return Json(new Result() { success = true, msg = "设置成功" });

        }


    }
}