using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using Mall.DTO;
using Mall.Application;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class AdvancePaymentController : BaseAdminController
    {
        // GET: Admin/AdvancePayment
        public ActionResult Edit()
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            var siteSettingModel = new SiteSettingModel()
            {
                AdvancePaymentPercent = siteSetting.AdvancePaymentPercent,
                AdvancePaymentLimit = siteSetting.AdvancePaymentLimit,
                UnpaidTimeout = siteSetting.UnpaidTimeout,
                NoReceivingTimeout = siteSetting.NoReceivingTimeout,
                OrderCommentTimeout = siteSetting.OrderCommentTimeout,
                SalesReturnTimeout = siteSetting.SalesReturnTimeout,
                AS_ShopConfirmTimeout = siteSetting.AS_ShopConfirmTimeout,
                AS_SendGoodsCloseTimeout = siteSetting.AS_SendGoodsCloseTimeout,
                AS_ShopNoReceivingTimeout = siteSetting.AS_ShopNoReceivingTimeout
            };
            return View(siteSettingModel);
        }

        [UnAuthorize]
        [HttpPost]
       
        public JsonResult Edit(SiteSettingModel post)
        {
            var message = string.Empty;
            if (post.UnpaidTimeout < 1)
                message = "错误的未付款超时，必需大于0";

            if (post.NoReceivingTimeout < 1)
                message = "错误的确认收货超时，必需大于0";

            if (post.OrderCommentTimeout < 1)
                message = "错误的关闭评价通道时限，必需大于0";

            if (post.SalesReturnTimeout < 1)
                message = "错误的订单退货期限，必需大于0";

            if (post.AS_ShopConfirmTimeout < 1)
                message = "错误的商家自动确认售后时限，必需大于0";

            if (post.AS_SendGoodsCloseTimeout < 1)
                message = "错误的用户发货限时，必需大于0";

            if (post.AS_ShopNoReceivingTimeout < 1)
                message = "错误的商家确认到货时限，必需大于0";

            if (!string.IsNullOrEmpty(message))
                return Json(new { success = false, msg = message });

            var setting = SiteSettingApplication.SiteSettings;
            setting.UnpaidTimeout = post.UnpaidTimeout;
            setting.NoReceivingTimeout = post.NoReceivingTimeout;
            setting.OrderCommentTimeout = post.OrderCommentTimeout;
            setting.SalesReturnTimeout = post.SalesReturnTimeout;
            setting.AS_ShopConfirmTimeout = post.AS_ShopConfirmTimeout;
            setting.AS_SendGoodsCloseTimeout = post.AS_SendGoodsCloseTimeout;
            setting.AS_ShopNoReceivingTimeout = post.AS_ShopNoReceivingTimeout;
            SiteSettingApplication.SaveChanges();
            return Json(new { success = true });
        }

        #region 售后原因设置
        public ActionResult RefundReason()
        {
            var datalist = RefundApplication.GetRefundReasons();
            return View(datalist);
        }
        [HttpPost]
        public JsonResult SaveRefundReason(string reason, long id = 0)
        {
            Result r = new Result { msg = "售后原因处理成功", status = 0, success = true };
            bool isdataok = true;
            if (isdataok)
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    isdataok = false;
                    r = new Result { msg = "请填写售后原因", status = -2, success = false };
                }
            }
            if (isdataok)
            {
                if (reason.Length > 20)
                {
                    isdataok = false;
                    r = new Result { msg = "售后原因限20字符", status = -2, success = false };
                }
            }
            RefundApplication.UpdateAndAddRefundReason(reason, id);
            return Json(r);
        }
        [HttpPost]
        public JsonResult DeleteRefundReason(long id)
        {
            Result r = new Result { msg = "删除成功", status = 0, success = true };
            RefundApplication.DeleteRefundReason(id);
            return Json(r);
        }

        public ActionResult CustomerServicesEdit()
        {
            var css = CustomerServiceApplication.GetPlatformCustomerService();

            var count = 3;//客服个数，可设计成从站点配置取
            var models = new PlatformCustomerServiceModel[count];
            for (int i = 0; i < css.Count; i++)
            {
                models[i] = new PlatformCustomerServiceModel();
                css[i].Map(models[i]);
            }

            for (int i = css.Count; i < models.Length; i++)
            {
                models[i] = new PlatformCustomerServiceModel();
                models[i].CreateId = Guid.NewGuid();
                models[i].Tool = Entities.CustomerServiceInfo.ServiceTool.QQ;
            }

            if (!models.Any(p => p.Tool == Entities.CustomerServiceInfo.ServiceTool.MeiQia))
                models.First(p => p.Id == 0).Tool = Entities.CustomerServiceInfo.ServiceTool.MeiQia;//设置其中一个客服为美洽客服

            return View(models);
        }

        [HttpPost]
        public JsonResult CustomerServicesEdit(PlatformCustomerServiceModel[] css)
        {
            var count = 3;//客服个数，可设计成从站点配置取
            css = css.Where(p => !string.IsNullOrEmpty(p.Name) && !string.IsNullOrEmpty(p.AccountCode)).Take(count).ToArray();

            foreach (var item in css)
            {
                item.ShopId = 0;
                if (!string.IsNullOrEmpty(item.Name))
                    item.Name = item.Name.Trim();
                if (!string.IsNullOrEmpty(item.AccountCode))
                    item.AccountCode = item.AccountCode.Trim();
            }

            var newIds = new Dictionary<Guid, long>();
            if (css.Any(p => p.Id == 0))
            {
                foreach (var item in css.Where(p => p.Id == 0))
                {
                    var newId = CustomerServiceApplication.AddPlateCustomerService(item.Map<CustomerService>());
                    newIds.Add(item.CreateId, newId);
                }
            }
            if (css.Any(p => p.Id > 0))
                Application.CustomerServiceApplication.UpdatePlatformService(css.Where(p => p.Id > 0));

            return Json(new
            {
                Success = true,
                NewIds = newIds
            }, true);
        }
        #endregion
    }
}