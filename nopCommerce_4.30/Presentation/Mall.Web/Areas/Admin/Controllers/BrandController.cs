using Mall.Web.Framework;
using Mall.Web.Models;
using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Application;
using Mall.DTO.QueryModel;

using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class BrandController : BaseAdminController
    {
        private IBrandService _iBrandService;
        private ISearchProductService _iSearchProductService;
        public BrandController(IBrandService iBrandService, ISearchProductService iSearchProductService)
        {
            _iBrandService = iBrandService;
            _iSearchProductService = iSearchProductService;
        }

        [Description("品牌管理页面")]
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult Verify()
        {
            return View();
        }

        public ActionResult Show(long id)
        {
            var model = _iBrandService.GetBrandApply(id);
            return View(model);
        }

        [Description("分页获取品牌列表JSON数据")]
        [HttpPost]
        // GET: Admin/Brand
        public JsonResult List(int page, int rows, string keyWords)
        {
            keyWords = keyWords.Trim();
            var result = _iBrandService.GetBrands(keyWords, page, rows);
            IEnumerable<BrandModel> brands = result.Models.ToArray().Select(item => {
                string shopName = string.Empty;
                var brandapply = BrandApplication.GetShopBrandApply(item.Name);
                if(brandapply != null)
                {
                    //var shop = ShopApplication.GetShop(brandapply.ShopId);
                    shopName = "（商家新增品牌）";
                }
                return new BrandModel()
                {
                    BrandName = item.Name,
                    BrandLogo = Core.MallIO.GetImagePath(item.Logo),
                    ID = item.Id,
                    BrandDesc = item.Name + shopName
                    //AuditStatus = (int)item.AuditStatus
                };
            });
            DataGridModel<BrandModel> model = new DataGridModel<BrandModel>() { rows = brands, total = result.Total };
            return Json(model);
        }

        public JsonResult ApplyList(int page, int rows, string keyWords)
        {
            var data = _iBrandService.GetShopBrandApplys(null, 0, page, rows, keyWords.Trim());
            var brands = data.Models;
            var shops = ShopApplication.GetShops(brands.Select(p => p.ShopId).ToList());
            var list = brands.Select(item => new BrandApplyModel()
            {
                Id = item.Id,
                BrandId = item.BrandId,
                ShopId = item.ShopId,
                BrandName = item.BrandName,
                BrandLogo = item.Logo,
                BrandDesc = item.Description == null ? "" : item.Description,
                BrandAuthPic = item.AuthCertificate,
                Remark = item.Remark,
                BrandMode = item.ApplyMode,
                AuditStatus = item.AuditStatus,
                ApplyTime = item.ApplyTime.ToString("yyyy-MM-dd"),
                ShopName = shops.FirstOrDefault(p => p.Id == item.ShopId)?.ShopName ?? string.Empty
            });
            var result = new DataGridModel<BrandApplyModel>() { rows = list, total = data.Total };
            return Json(result);
        }

        [HttpPost]
        [Description("删除品牌")]
        public JsonResult Delete(int id)
        {
            _iBrandService.DeleteBrand(id);
            return Json(new Result() { success = true, msg = "删除成功！" });

        }
        //获取品牌下的商品数量
        [HttpPost]
        public JsonResult GetProductNum(string brandName)
        {
            var query = new ProductQuery { BrandName = brandName };
            var count = ProductManagerApplication.GetProductCount(query);
            return SuccessResult<dynamic>(data: new { ProductNum = count });
        }
        [HttpPost]
        [Description("删除品牌申请")]
        public JsonResult DeleteApply(int id)
        {
            _iBrandService.DeleteApply(id);

            return Json(new Result() { success = true, msg = "删除成功！" });

        }

        [HttpPost]
        public JsonResult Audit(int id)
        {
            _iBrandService.AuditBrand(id, Mall.Entities.ShopBrandApplyInfo.BrandAuditStatus.Audited, "");
            return Json(new Result() { success = true, msg = "审核成功！" });
        }

        [HttpPost]
        public JsonResult Refuse(int id, string remark)
        {
            _iBrandService.AuditBrand(id, Mall.Entities.ShopBrandApplyInfo.BrandAuditStatus.Refused, remark);
            return Json(new Result() { success = true, msg = "拒绝成功！" });
        }

        [HttpPost]
        // GET: Admin/Brand
        public JsonResult GetBrands(string keyWords, int? AuditStatus = 2)
        {
            var after = _iBrandService.GetBrands(keyWords);
            var values = after.Select(item => new { key = item.Id, value = item.Name, envalue = item.RewriteName });
            return Json(values);
        }

        public ActionResult Edit(long id)
        {
            var brand = _iBrandService.GetBrand(id);

            BrandModel model = new BrandModel()
            {
                ID = brand.Id,
                BrandName = brand.Name,
                BrandDesc = brand.Description,
                BrandLogo = brand.Logo,
                //BrandEnName = brand.RewriteName,
                MetaDescription = brand.Meta_Description,
                MetaKeyWord = brand.Meta_Keywords,
                MetaTitle = brand.Meta_Title,
                //ShopID = brand.ShopId,
                IsRecommend = brand.IsRecommend,
                //AuditStatus = (int)brand.AuditStatus
            };
            return View(model);
        }


        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                Entities.BrandInfo model = new Entities.BrandInfo()
                {
                    Name = brand.BrandName.Trim(),
                    Description = brand.BrandDesc,
                    Logo = brand.BrandLogo,
                    Meta_Description = brand.MetaDescription,
                    Meta_Keywords = brand.MetaKeyWord,
                    Meta_Title = brand.MetaTitle,
                    //RewriteName = brand.BrandEnName,
                    //ShopId = 0,
                    IsRecommend = brand.IsRecommend,
                    //AuditStatus = BrandInfo.BrandAuditStatus.Audited
                };
                bool flag = _iBrandService.IsExistBrand(brand.BrandName.Trim());
                if (flag == false)
                {
                    _iBrandService.AddBrand(model);
                }
                return RedirectToAction("Management");
            }
            return View(brand);
        }

        [HttpPost]
        public JsonResult IsExist(string name)
        {
            bool flag = _iBrandService.IsExistBrand(name);
            if (flag == false)
            {
                return Json(new Result() { success = false, msg = null });
            }
            else
                return Json(new Result() { success = true, msg = "该品牌已存在，请不要重复添加！" });
        }

        [HttpPost]
        [OperationLog(Message = "编辑品牌")]
        public ActionResult Edit(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                Entities.BrandInfo model = new Entities.BrandInfo()
                {
                    Id = brand.ID,
                    Name = brand.BrandName.Trim(),
                    Description = brand.BrandDesc,
                    Logo = brand.BrandLogo,
                    Meta_Description = brand.MetaDescription,
                    Meta_Keywords = brand.MetaKeyWord,
                    Meta_Title = brand.MetaTitle,
                    //RewriteName = brand.BrandEnName,
                    IsRecommend = brand.IsRecommend,
                };
                _iBrandService.UpdateBrand(model);
                //更新商品搜索冗余数据
                Task.Factory.StartNew(() =>
                {
                    _iSearchProductService.UpdateBrand(model);
                });
                return RedirectToAction("Management");
            }
            return View(brand);
        }

        [HttpPost]
        public JsonResult IsInUse(long id)
        {
            bool flag = _iBrandService.BrandInUse(id);
            if (flag == false)
            {
                return Json(new Result() { success = false, msg = "该品牌尚未使用！" });
            }
            else
                return Json(new Result() { success = true, msg = "该品牌已使用！" });
        }
    }
}












