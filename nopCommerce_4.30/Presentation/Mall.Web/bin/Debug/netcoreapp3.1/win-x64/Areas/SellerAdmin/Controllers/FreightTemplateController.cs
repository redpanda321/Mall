﻿
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Linq;
using Mall.Core;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class FreightTemplateController : BaseSellerController
    {
        public ActionResult Index()
        {
            return View();
        }
        [UnAuthorize]
        [HttpPost]
        public JsonResult List(int page, int rows)
        {
            var result = Mall.Application.FreightTemplateApplication.GetShopFreightTemplate(CurrentSellerManager.ShopId)
                .Select(e => new FreightTemplateInfoModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    ShopID = e.ShopID,
                    ValuationMethod = e.ValuationMethod.ToDescription(),
                    IsFree = (e.IsFree == CommonModel.FreightTemplateType.Free)
                });
            DataGridModel<FreightTemplateInfoModel> model = new DataGridModel<FreightTemplateInfoModel>() { rows = result.ToArray(), total = result.Count() };
            return Json(model, true);
        }

        public ActionResult Add(long? id)
        {
            long templateId = id.HasValue ? id.Value : 0;
            var template = new DTO.FreightTemplate()
            {
                ShopID = CurrentSellerManager.ShopId
            };
            ViewBag.IsUsed = 0;
            if (id.HasValue)
            {
                template = Mall.Application.FreightTemplateApplication.GetFreightTemplate(templateId, CurrentSellerManager.ShopId);
                
                //var template = _iFreightTemplateService.GetFreightTemplate(templateId);
                //if (template.ShopID != CurrentSellerManager.ShopId)
                //{
                //    throw new Core.MallException("该运费模板不存在" + id);
                //}
                //var regionService = _iRegionService;
                //if (template.SourceAddress.HasValue)
                //{
                //    templateExtend.SourceAddressStr = regionService.GetRegionPath(template.SourceAddress.Value);
                //}
                //templateExtend.Id = template.Id;
                //templateExtend.IsFree = template.IsFree;
                //templateExtend.Name = template.Name;
                //templateExtend.ShopID = template.ShopID;
                //templateExtend.SendTime = template.SendTime;
                //templateExtend.SourceAddress = template.SourceAddress;
                //templateExtend.ValuationMethod = template.ValuationMethod;
                //templateExtend.FreightAreaContent = template.Mall_FreightAreaContent.Select(e => new Mall.Web.Areas.SellerAdmin.Models.FreightAreaContentInfoExtend()
                //{
                //    AreaContent = e.AreaContent,
                //    AreaContentCN = regionService.GetRegionByName(e.AreaContent).Name,
                //    AccumulationUnit = e.AccumulationUnit,
                //    AccumulationUnitMoney = e.AccumulationUnitMoney,
                //    FirstUnit = e.FirstUnit,
                //    FirstUnitMonry = e.FirstUnitMonry,
                //    IsDefault = e.IsDefault,
                //    FreightTemplateId = e.FreightTemplateId,
                //    Id = e.Id
                //});
                if (Mall.Application.FreightTemplateApplication.IsProductUseFreightTemp(templateId))
                {
                    ViewBag.IsUsed = 1;
                }
            }
            return View(template);
        }

        //public ActionResult Detials(long id)
        //{
        //    long templateId = id;
        //    var templateExtend = new Mall.Web.Areas.SellerAdmin.Models.FreightTemplateInfoExtend()
        //    {
        //        ShopID = CurrentSellerManager.ShopId
        //    };

        //    var template = _iFreightTemplateService.GetFreightTemplate(templateId);
        //    if (template.ShopID != CurrentSellerManager.ShopId)
        //    {
        //        throw new Core.MallException("该运费模板不存在" + id);
        //    }
        //    var regionService = _iRegionService;
        //    if (template.SourceAddress.HasValue)
        //    {
        //        templateExtend.SourceAddressStr = regionService.GetRegionPath(template.SourceAddress.Value);
        //    }
        //    templateExtend.Id = template.Id;
        //    templateExtend.IsFree = template.IsFree;
        //    templateExtend.Name = template.Name;
        //    templateExtend.ShopID = template.ShopID;
        //    templateExtend.SendTime = template.SendTime;
        //    templateExtend.SourceAddress = template.SourceAddress;
        //    templateExtend.ValuationMethod = template.ValuationMethod;
        //    templateExtend.FreightAreaContent = template.Mall_FreightAreaContent.Select(e => new Mall.Web.Areas.SellerAdmin.Models.FreightAreaContentInfoExtend()
        //    {
        //        AreaContent = e.AreaContent,
        //        AreaContentCN = regionService.GetRegionByName(e.AreaContent).Name,
        //        AccumulationUnit = e.AccumulationUnit,
        //        AccumulationUnitMoney = e.AccumulationUnitMoney,
        //        FirstUnit = e.FirstUnit,
        //        FirstUnitMonry = e.FirstUnitMonry,
        //        IsDefault = e.IsDefault,
        //        FreightTemplateId = e.FreightTemplateId,
        //        Id = e.Id
        //    });

        //    ViewBag.IsUsed = 0;
        //    return View(templateExtend);
        //}

        public JsonResult GetFreightTemplateInfo(long templateid)
        {
            var templateInfo = FreightTemplateApplication.GetFreightTemplate(templateid, CurrentSellerManager.ShopId);
            return Json(new { model = templateInfo, success = true });
        }

		public JsonResult GetFreightAreaDetail(long templateId)
		{
			var template = Mall.Application.FreightTemplateApplication.GetFreightAreaDetail(templateId, CurrentSellerManager.ShopId);
            return Json(new { model = template.FreightArea, modelfree = template.FreeTempContent }, true);
        }

       
        [HttpPost]
        public JsonResult SaveTemplate(DTO.FreightTemplate templateinfo)
        {
			if (templateinfo.SourceAddress == null || templateinfo.SourceAddress.Value == 0)
				return Json(new { success = false });
            if (templateinfo.Name.Length > 20)
            {
                return Json(new { success = false });
            }
            foreach(var item in templateinfo.FreightArea)
            {
                item.FirstUnitMonry = (float)Math.Round(item.FirstUnitMonry.Value, 2);
                item.AccumulationUnitMoney= (float)Math.Round(item.AccumulationUnitMoney.Value, 2);
            }
            templateinfo.ShopID = CurrentSellerManager.ShopId;
            FreightTemplateApplication.AddOrUpdateFreightTemplate(templateinfo);
            return Json(new { success = true });
        }

        public JsonResult DeleteTemplate(long id)
        {
            Mall.Application.FreightTemplateApplication.DeleteFreightTemplate(id, CurrentShop.Id);
            return Json(new { success = true, msg = "删除成功" });
        }


        public JsonResult CopyTemplate(long id)
        {
            Mall.Application.FreightTemplateApplication.CopyFreightTemplate(id, CurrentShop.Id);
            return Json(new { success = true, msg = "复制成功" });
        }
    }
}