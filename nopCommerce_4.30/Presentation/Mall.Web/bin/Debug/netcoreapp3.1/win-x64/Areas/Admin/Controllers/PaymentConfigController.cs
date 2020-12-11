using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    public class PaymentConfigController : BaseAdminController
    {
        private IRegionService _iRegionService;
        private IPaymentConfigService _iPaymentConfigService;

        public PaymentConfigController(IRegionService iRegionService, IPaymentConfigService iPaymentConfigService)
        {
            _iRegionService = iRegionService;
            _iPaymentConfigService = iPaymentConfigService;
        } 

        public ActionResult Index()
        {
            var p = _iRegionService.GetAllRegions().Where(a => a.Level == CommonModel.Region.RegionLevel.Province && a.Sub != null).ToList();
            ViewBag.Address = _iPaymentConfigService.GetAddressId();
            ViewBag.AddressCity = _iPaymentConfigService.GetAddressIdCity();
            return View(p);
        }
        [HttpPost]
        public ActionResult GetRegion(long id)
        {
            return Json(_iRegionService.GetRegion(id));
        }

        [HttpPost]
        public ActionResult Save(string addressIds, string addressIds_city)
        {
            _iPaymentConfigService.Save(addressIds, addressIds_city);
            return Json(new Result() { success = true, msg = "保存成功！" });
        }
    }
}