using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using System.Linq;

using Mall.Core;

using System.Collections.Generic;
using Mall.DTO;
using System.IO;
using System.Web;
using Mall.Web.Models;
using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class ShopHomeModuleController : BaseSellerController
    {
        private IShopHomeModuleService _iShopHomeModuleService;
        public ShopHomeModuleController(IShopHomeModuleService iShopHomeModuleService)
        {
            _iShopHomeModuleService = iShopHomeModuleService;
        }

        // GET: SellerAdmin/ShopHomeModule
        public ActionResult Management()
        {
            var shopHomeModules = _iShopHomeModuleService.GetAllShopHomeModuleInfos(CurrentSellerManager.ShopId).ToArray();

            var models = shopHomeModules.Select(item => new ShopHomeModuleBasicModel()
            {
                Id = item.Id,
                Name = item.Name,
                DisplaySequence = item.DisplaySequence,
                IsEnable = item.IsEnable
            });
            return View(models);
        }


        [HttpPost]
        public JsonResult SaveName(long id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidPropertyException("名称不能为空");
            if (id <= 0)
                throw new InvalidPropertyException("商品模块id必须大于0");

            name = name.Trim();
            _iShopHomeModuleService.UpdateShopProductModuleName(CurrentSellerManager.ShopId, id, name);
            ClearCache();
            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult AddShopHomeModule(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidPropertyException("名称不能为空");

            var shopHomeModule = new Entities.ShopHomeModuleInfo()
            {
                ShopId = CurrentSellerManager.ShopId,
                Name = name.Trim()
            };

            _iShopHomeModuleService.AddShopProductModule(shopHomeModule);
            ClearCache();
            return Json(new { success = true, name = shopHomeModule.Name, id = shopHomeModule.Id });
        }


        [HttpPost]
        [ShopOperationLog(Message = "删除商品模块")]
        public JsonResult Delelte(long id)
        {
            if (id <= 0)
                throw new InvalidPropertyException("商品模块id必须大于0");

            _iShopHomeModuleService.Delete(CurrentSellerManager.ShopId, id);
            ClearCache();
            return Json(new { success = true });
        }



        [HttpPost]
        [ShopOperationLog(Message = "添加商品模块")]
        public JsonResult SaveShopModuleProducts(long id, string productIds)
        {
            if (id <= 0)
                throw new InvalidPropertyException("商品模块id必须大于0");
            IEnumerable<long> productIds_long;
            try
            {
                productIds_long = productIds.Split(',').Select(item => long.Parse(item));
            }
            catch (System.FormatException)
            {
                throw new InvalidPropertyException("商品编号不合法，请使用半角逗号连接各商品id");
            }

            _iShopHomeModuleService.UpdateShopProductModuleProducts(CurrentSellerManager.ShopId, id, productIds_long);
            ClearCache();
            return Json(new { success = true });
        }

        public void ClearCache()
        {
            //HttpResponse.RemoveOutputCacheItem(string.Format("/Shop/Home/{0}", this.CurrentShop.Id)); //移除页面缓存
            //HttpResponse.RemoveOutputCacheItem(string.Format("/shop/home/{0}", this.CurrentShop.Id));
        }


        #region by zjt

        public ActionResult EditFooter()
        {
            string footer = _iShopHomeModuleService.GetFooter(CurrentSellerManager.ShopId);
            ViewBag.Footer = footer;
            return View();
        }

        [HttpPost]
       
        public ActionResult EditFooter(string footer)
        {
            _iShopHomeModuleService.EditFooter(CurrentSellerManager.ShopId, footer);
            ClearCache();
            return Json(new Result { success = true });
        }


        public ActionResult AddFloor(long? id = 0)
        {
            var model = new ShopModuleViewModel();

            if (id.HasValue && id > 0)
            {
                model.Module = _iShopHomeModuleService.GetShopHomeModuleInfo(CurrentSellerManager.ShopId, id.Value);
                model.ModuleProducts = _iShopHomeModuleService.GetProducts(new List<long> { id.Value });
                model.Images = _iShopHomeModuleService.GetImages(new List<long> { id.Value });
                model.Products = ProductManagerApplication.GetProducts(model.ModuleProducts.Select(p => p.ProductId));
            }

            return View(model);
        }

        public ActionResult EditFloor(long? id = 0)
        {
            Entities.ShopHomeModuleInfo info = null;
            if (id != null && id > 0)
            {
                info = _iShopHomeModuleService.GetShopHomeModuleInfo(CurrentSellerManager.ShopId, (int)id);
            }
            else
            {
                info = new Entities.ShopHomeModuleInfo();
            }
            return View(info);
        }

        public ActionResult AddAcion(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return Json(new Result { success = false });
            }

            AddShopHomeModuleModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<AddShopHomeModuleModel>(args);

            foreach (var img in model.TopImgs)
            {
                if (!img.ImgPath.Contains("/temp"))
                {
                    continue;
                }
                string source = img.ImgPath.Substring(img.ImgPath.LastIndexOf("/temp"));
                string dest = string.Format(@"/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);
                string fullDir = dest;
                Core.MallIO.CopyFile(source, fullDir + Path.GetFileName(source), true);
                img.ImgPath = Path.Combine(dest, Path.GetFileName(source));
            }

            model.ShopId = CurrentSellerManager.ShopId;
            _iShopHomeModuleService.SaveFloor(model);
            ClearCache();
            return Json(new Result { success = true });
        }

        [HttpPost]
        public JsonResult FloorEnable(long id, bool enable)
        {
            _iShopHomeModuleService.Enable(id, enable);
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        public JsonResult FloorChangeSequence(int oriRowNumber, int newRowNumber, string direction)
        {
            _iShopHomeModuleService.UpdateFloorSequence(CurrentSellerManager.ShopId, oriRowNumber, newRowNumber, direction);
            ClearCache();
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        public ActionResult DelFloor(long id)
        {
            _iShopHomeModuleService.DelFloor(id);
            return Json(new
            {
                success = true
            });
        }
        #endregion
    }
}