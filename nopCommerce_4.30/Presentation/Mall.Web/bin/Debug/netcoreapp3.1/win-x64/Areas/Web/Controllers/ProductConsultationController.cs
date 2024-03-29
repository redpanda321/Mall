﻿using Mall.Application;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class ProductConsultationController : BaseWebController
    {
        private ICommentService _iCommentService;
        private IConsultationService _iConsultationService;
        private IProductService _iProductService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        public ProductConsultationController(
            ICommentService iCommentService ,
            IConsultationService iConsultationService ,
            IProductService iProductService ,
            ILimitTimeBuyService iLimitTimeBuyService
            )
        {
            _iCommentService = iCommentService;
            _iConsultationService = iConsultationService;
            _iProductService = iProductService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
        }
        // GET: Web/ProductConsultation
        public ActionResult Index( long id = 0 )
        {
            var productMark = CommentApplication.GetProductAverageMark(id);
            ViewBag.CommentCount = CommentApplication.GetCommentCountByProduct(id);
            ViewBag.productMark = productMark;
            var productinfo = _iProductService.GetProduct(id);
            List<FlashSalePrice> falseSalePrice = _iLimitTimeBuyService.GetPriceByProducrIds( new List<long> { id } );
            if( falseSalePrice != null && falseSalePrice.Count == 1 )
            {
                productinfo.MinSalePrice = falseSalePrice[ 0 ].MinPrice;
            }
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(productinfo);
        }

        [HttpPost]
        public JsonResult AddConsultation( string Content , long productId = 0 )
        {
            if( productId == 0 )
            {
                return Json( new Result() { success = false , msg = "咨询失败，该商品不存在或已经删除！" } );
            }
            if( CurrentUser == null )
            {
                return Json( new Result() { success = false , msg = "登录超时，请重新登录！" } );
            }
            Mall.Entities.ProductConsultationInfo model = new Entities.ProductConsultationInfo();
            model.ConsultationContent = Content;
            model.ConsultationDate = DateTime.Now;
            model.ProductId = productId;
            model.UserId = CurrentUser.Id;
            model.UserName = CurrentUser.UserName;
            model.Email = CurrentUser.Email;
            _iConsultationService.AddConsultation( model );
            return Json( new Result() { success = true , msg = "咨询成功" } );
        }
    }
}