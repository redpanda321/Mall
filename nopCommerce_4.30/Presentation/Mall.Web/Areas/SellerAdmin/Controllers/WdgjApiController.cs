
using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class WdgjApiController : BaseSellerController
    {
        private IShopService _iShopService;
        private long CurShopId;

        public WdgjApiController(IShopService iShopService)
        {
            _iShopService = iShopService;
            if (CurrentSellerManager != null)
            {//退出登录后，直接进入controller异常处理
                CurShopId = CurrentSellerManager.ShopId;
            }
        }

        public ActionResult Index()
        {
            var data = _iShopService.GetshopWdgjInfoById(CurShopId);
            var models = new WdgjApiModel()
            {
                Id = data != null ? data.Id : 0,
                uCode = data != null ? data.uCode : "",
                uSign = data != null ? data.uSign : ""
            };
            return View(models);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Add(WdgjApiModel wdgj)
        {
            var service = _iShopService;
            Entities.ShopWdgjSettingInfo shopwdgjInfo = new Entities.ShopWdgjSettingInfo()
            {
                Id = wdgj.Id,
                ShopId = CurShopId,
                uCode = wdgj.uCode,
                uSign = wdgj.uSign
            };
            if (shopwdgjInfo.Id > 0)
                service.UpdateShopWdgj(shopwdgjInfo);
            else
                service.AddShopWdgj(shopwdgjInfo);
            return Json(new { success = true });
        }
    }
}