using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;

using System.Linq;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class BrandController : BaseSellerController
    {

        private IBrandService _iBrandService;
        public BrandController(IBrandService iBrandService)
        {
            _iBrandService = iBrandService;
        }

        public ActionResult Management()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }
        public ActionResult Edit(long id)
        {
            var data = _iBrandService.GetBrandApply(id);
            if (data.ShopId != CurrentShop.Id)
            {
                throw new MallException("错误的参数");
            }
            return View(data);
        }
        public ActionResult Show(long id)
        {
            var model = _iBrandService.GetBrandApply(id);
            return View(model);
        }


        [Description("分页获取品牌列表JSON数据")]
        [UnAuthorize]
        [HttpPost]
        public JsonResult List(int page, int rows)
        {
            var shopId = CurrentSellerManager.ShopId;
            var result = _iBrandService.GetShopBrandApplys(shopId, null, page, rows, "");
            var brands = result.Models.ToArray().Select(item => new BrandApplyModel()
            {
                Id = item.Id,
                BrandId = item.BrandId,
                ShopId = item.ShopId,
                BrandName = item.BrandName,
                BrandLogo = MallIO.GetImagePath(item.Logo),
                BrandDesc = item.Description == null ? "" : item.Description,
                BrandAuthPic = item.AuthCertificate,
                Remark = item.Remark,
                BrandMode = item.ApplyMode,
                PlatRemark = item.PlatRemark,
                AuditStatus = item.AuditStatus,
            });
            DataGridModel<BrandApplyModel> model = new DataGridModel<BrandApplyModel>() { rows = brands, total = result.Total };
            return Json(model);
        }
        [HttpPost]
        [UnAuthorize]
        [ShopOperationLog(Message = "重新编辑申请品牌")]
        public JsonResult EditApply(BrandApplyModel brand)
        {
            var shopId = CurrentSellerManager.ShopId;
            Entities.ShopBrandApplyInfo model = _iBrandService.GetBrandApply(brand.Id);
            if (model == null)
            {
                throw new Mall.Core.MallException("该品牌审核不通过，请重新编辑");
            }
            if (model.ApplyMode == 1)
            {
                var m = _iBrandService.GetBrand(brand.BrandId);
                if (m == null)
                {
                    throw new Mall.Core.MallException("品牌不存在");
                }
                model.BrandName = m.Name;
                model.Logo = m.Logo;
                model.Description = m.Description;
                model.BrandId = brand.BrandId;
            }
            else
            {
                if (brand.BrandDesc.Length > 200)
                {
                    return Json(new Result() { success = false, msg = "简介过长！" });
                }
                model.BrandId = 0;
                model.BrandName = brand.BrandName.Trim();
                model.Logo = brand.BrandLogo;
                model.Description = brand.BrandDesc;
            }

            model.Remark = brand.Remark;
            model.AuthCertificate = brand.BrandAuthPic;
            model.ShopId = shopId;
            model.ApplyTime = DateTime.Now;            

            var oldapply = _iBrandService.GetExistApply(shopId, model.BrandName);
            if (oldapply == null)
            {
                model.AuditStatus = 0;
                _iBrandService.UpdateApplyBrand(model);
            }
            else
            {
                Entities.BrandInfo orderband = null;
                if ( oldapply.BrandId>0)
                    orderband = _iBrandService.GetBrand(oldapply.BrandId);
                if (orderband != null && orderband.IsDeleted)
                {
                    model.Id = oldapply.Id;
                }
                else
                {
                    if (oldapply.AuditStatus == (int)ShopBrandApplyInfo.BrandAuditStatus.Audited || oldapply.AuditStatus == (int)ShopBrandApplyInfo.BrandAuditStatus.UnAudit)
                    {
                        throw new Mall.Core.MallException("该品牌申请已存在，请选择申请其他品牌");
                    }
                    model.AuditStatus = 0;
                    _iBrandService.UpdateApplyBrand(model);
                }
            }
            return Json(new Result() { success = true, msg = "重新编辑品牌申请成功！" });
        }


        [HttpPost]
        [UnAuthorize]
        [ShopOperationLog(Message = "申请品牌")]
        public JsonResult Apply(BrandApplyModel brand)
        {
            var shopId = CurrentSellerManager.ShopId;
            var model = new Entities.ShopBrandApplyInfo();

            model.BrandId = brand.BrandId;
            model.ApplyMode = brand.BrandMode == 1 ? (int)Mall.Entities.ShopBrandApplyInfo.BrandApplyMode.Exist : (int)Mall.Entities.ShopBrandApplyInfo.BrandApplyMode.New;
            if (brand.BrandMode == 1)
            {
                var m = _iBrandService.GetBrand(brand.BrandId);
                if (m == null)
                {
                    throw new Mall.Core.MallException("品牌不存在，请刷新页面重新申请");
                }
                model.BrandName = m.Name;
                model.Logo = m.Logo;
                model.Description = m.Description;
            }
            else
            {
                if (brand.BrandDesc.Length > 200)
                {
                    return Json(new Result() { success = false, msg = "简介过长！" });
                }
                model.BrandId = 0;
                model.BrandName = brand.BrandName.Trim();
                model.Logo = brand.BrandLogo;
                model.Description = brand.BrandDesc;
            }

            model.Remark = brand.Remark;
            model.AuthCertificate = brand.BrandAuthPic;
            model.ShopId = shopId;
            model.ApplyTime = DateTime.Now;

            var oldapply = _iBrandService.GetExistApply(shopId, model.BrandName);
          
            if (oldapply == null)
            {
                _iBrandService.ApplyBrand(model);
                return Json(new Result() { success = true, msg = "申请成功！" });
            }
            else
            {
                var oBrand = BrandApplication.GetBrand(oldapply.BrandId);
                if (oBrand != null && oBrand.IsDeleted)
                {
                    model.Id = oldapply.Id;
                }
                else
                {
                    if (oldapply.AuditStatus == (int)Entities.ShopBrandApplyInfo.BrandAuditStatus.UnAudit)
                    {
                        return Json(new Result() { success = false, msg = "该品牌正在审核中！" });
                    }
                    else if (oldapply.AuditStatus == (int)Entities.ShopBrandApplyInfo.BrandAuditStatus.Audited)
                    {
                        return Json(new Result() { success = false, msg = "该品牌已存在，请选择申请已有品牌！" });
                    }
                }
                model.AuditStatus = 0;
                _iBrandService.UpdateApplyBrand(model);
                return Json(new Result() { success = true, msg = "申请成功！" });
            }
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateSellerBrand(BrandModel brand)
        {
            Entities.BrandInfo model = new Entities.BrandInfo()
            {
                Id = brand.ID,
                Name = brand.BrandName,
                Description = brand.BrandDesc,
                Logo = brand.BrandLogo,
                //AuditStatus = BrandInfo.BrandAuditStatus.UnAudit
            };
            
            //_iBrandService.UpdateSellerBrand(model);
            return Json(new Result() { success = true, msg = "更新成功" });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult IsExist(string name)
        {
            bool flag = _iBrandService.IsExistBrand(name);
            if (flag == false)
            {
                return Json(new Result() { success = false, msg = "该品牌不存在！" });
            }
            else
                return Json(new Result() { success = true, msg = "该品牌已存在！" });
        }

        [UnAuthorize]
        public JsonResult GetBrandsAjax(long? id,string action)
        {
            var brands = _iBrandService.GetBrands("", CurrentSellerManager.ShopId, action);
            var data = new List<Mall.Web.Areas.Admin.Models.Product.BrandViewModel>();
            foreach (var brand in brands)
            {
                data.Add(new Mall.Web.Areas.Admin.Models.Product.BrandViewModel
                {
                    id = brand.Id,
                    isChecked = null == id ? false : id.Equals(brand.Id),
                    value = brand.Name
                });
            }
            return Json(new { data = data });
        }
    }
}