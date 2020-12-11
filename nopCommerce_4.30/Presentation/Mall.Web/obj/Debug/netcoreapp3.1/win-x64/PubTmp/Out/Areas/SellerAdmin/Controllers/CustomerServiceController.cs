using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class CustomerServiceController : BaseSellerController
    {
        private ICustomerService _iCustomerService;
        public CustomerServiceController(ICustomerService iCustomerService)
        {
            _iCustomerService = iCustomerService;
        }

        // GET: SellerAdmin/CustomerService
        public ActionResult Management()
        {
            var customerServices = _iCustomerService.GetCustomerService(CurrentSellerManager.ShopId).OrderByDescending(item => item.Id).ToArray();
            var model = new CustomerServiceManagementViewModel();
            model.CustomerServices = customerServices.Select(
                item => new CustomerServiceModel()
                {
                    Id = item.Id,
                    Account = item.AccountCode,
                    Name = item.Name,
                    Tool = item.Tool,
                    Type = item.Type
                }).ToList();

            var mobileService = _iCustomerService.GetCustomerServiceForMobile(CurrentSellerManager.ShopId);

            var hasMobileService = mobileService != null ? true : false;
            model.HasMobileService = hasMobileService;
            model.MobileService = mobileService;
            var sitesetting = SiteSettingApplication.SiteSettings;
            if (sitesetting != null)
            {
                ViewBag.IsOpenPC = sitesetting.IsOpenPC;
                ViewBag.IsOpenH5 = sitesetting.IsOpenH5;
            }
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Delete(long id)
        {
            _iCustomerService.DeleteCustomerService(CurrentSellerManager.ShopId, id);
            return Json(new { success = true });
        }

        public ActionResult Add(long? id)
        {
            var service = _iCustomerService;
            Entities.CustomerServiceInfo customerServiceInfo;
            if (id.HasValue && id > 0)
                customerServiceInfo = service.GetCustomerService(CurrentSellerManager.ShopId, id.Value);
            else
                customerServiceInfo = new Entities.CustomerServiceInfo();

            var customerServiceModels = new CustomerServiceModel()
            {
                Id = customerServiceInfo.Id,
                Account = customerServiceInfo.AccountCode,
                Name = customerServiceInfo.Name,
                Tool = customerServiceInfo.Tool,
                Type = customerServiceInfo.Type
            };
            return View(customerServiceModels);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Add(CustomerServiceModel customerServiceModel)
        {
            var service = _iCustomerService;
            Entities.CustomerServiceInfo customerServiceInfo = new Entities.CustomerServiceInfo()
            {
                Id = customerServiceModel.Id,
                Type = customerServiceModel.Type.GetValueOrDefault(Entities.CustomerServiceInfo.ServiceType.PreSale),
                Tool = customerServiceModel.Tool,
                Name = customerServiceModel.Name,
                AccountCode = customerServiceModel.Account,
                ShopId = CurrentSellerManager.ShopId,
                TerminalType = Mall.Entities.CustomerServiceInfo.ServiceTerminalType.PC,
                ServerStatus = Mall.Entities.CustomerServiceInfo.ServiceStatusType.Open
            };

            if (customerServiceInfo.Id > 0)
                service.UpdateCustomerService(customerServiceInfo);
            else
                service.AddCustomerService(customerServiceInfo);

            return Json(new { success = true });

        }

        public ActionResult addMobile()
        {
            var service = _iCustomerService;
            Entities.CustomerServiceInfo customerServiceInfo;
            customerServiceInfo = service.GetCustomerServiceForMobile(CurrentSellerManager.ShopId);
            if (customerServiceInfo == null)
                customerServiceInfo = new Entities.CustomerServiceInfo();
            var customerServiceModels = new CustomerServiceModel()
            {
                Id = customerServiceInfo.Id,
                Account = customerServiceInfo.AccountCode,
                Name = customerServiceInfo.Name,
                Tool = customerServiceInfo.Tool,
                Type = customerServiceInfo.Type
            };
            return View(customerServiceModels);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult addMobile(CustomerServiceModel customerServiceModel)
        {
            var service = _iCustomerService;
            Entities.CustomerServiceInfo customerServiceInfo = new Entities.CustomerServiceInfo()
            {
                Id = customerServiceModel.Id,
                Type = customerServiceModel.Type.GetValueOrDefault(Entities.CustomerServiceInfo.ServiceType.PreSale),
                Tool = Entities.CustomerServiceInfo.ServiceTool.QQ,
                Name = customerServiceModel.Name,
                AccountCode = customerServiceModel.Account,
                ShopId = CurrentSellerManager.ShopId,
                TerminalType = Entities.CustomerServiceInfo.ServiceTerminalType.Mobile,
                ServerStatus = Entities.CustomerServiceInfo.ServiceStatusType.Open
            };

            if (customerServiceInfo.Id > 0)
                service.UpdateCustomerService(customerServiceInfo);
            else
                service.AddCustomerService(customerServiceInfo);

            return Json(new { success = true });
        }

        public ActionResult AddMeiQia(long? id)
        {
            return Add(id);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult deleteMobile()
        {
            _iCustomerService.DeleteCustomerServiceForMobile(CurrentSellerManager.ShopId);
            return Json(new { success = true });
        }
    }
}