using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Mall.Web.Models;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using Mall.Application;
using Mall.CommonModel;

using Mall.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class ProductCommentController : BaseSellerController
    {
        private IOrderService _iOrderService;
        private ICommentService _iCommentService;
        private ITypeService _iTypeService;
        public ProductCommentController(IOrderService iOrderService, ICommentService iCommentService, ITypeService iTypeService)
        {
            _iOrderService = iOrderService;
            _iCommentService = iCommentService;
            _iTypeService = iTypeService;
        }
        public ActionResult Management()
        {
            bool isReply = Request.GetDisplayUrl().ToLower().Contains("selleradmin/home/console");
            ViewBag.isReply = isReply ? 1 : 0;
            return View();
        }

        public JsonResult List(int page, int rows, string productName, bool? isReply = null, int Rank = -1, bool hasAppend = false)
        {
            if (!string.IsNullOrEmpty(productName))
            {
                productName = productName.Trim();
            }
            var orderItemService = _iOrderService;
            var iTypeService = _iTypeService;
            var query = new CommentQuery() { PageNo = page, PageSize = rows, HasAppend = hasAppend, ProductName = productName, Rank = Rank, ShopID = base.CurrentSellerManager.ShopId, IsReply = isReply };
            var result = _iCommentService.GetComments(query);
            var orderItemIds = result.Models.Select(a => a.SubOrderId).ToList();
            
            var orderItems = orderItemService.GetOrderItemsByOrderItemId(orderItemIds).ToDictionary(item=>item.Id,item=>item);
            var comments = result.Models.Select(item =>
            {
                var product = ProductManagerApplication.GetProduct(item.ProductId);
                return new ProductCommentModel()
                {
                    CommentContent = item.ReviewContent,
                    CommentDate = item.ReviewDate,
                    ReplyContent = item.ReplyContent,
                    AppendContent = item.AppendContent,
                    AppendDate = item.AppendDate,
                    ReplyAppendDate = item.ReplyAppendDate,
                    CommentMark = item.ReviewMark,
                    ReplyDate = item.ReplyDate,
                    Id = item.Id,
                    ProductName = (product == null) ? "" : product.ProductName,
                    ProductId = item.ProductId,
                    ImagePath = orderItems[item.SubOrderId].ThumbnailsUrl,
                    UserName = item.UserName,
                    OderItemId = item.SubOrderId,
                    Color = "",
                    Version = "",
                    Size = "",
                    UserId = item.UserId
                };
            }).ToList();
            //TODO LRL 2015/08/06 从评价信息添加商品的规格信息
            foreach (var item in comments)
            {
                item.ImagePath = Core.MallIO.GetProductSizeImage(item.ImagePath, 1, (int)ImageSize.Size_100);
                if (item.OderItemId.HasValue)
                {
                    var obj = orderItemService.GetOrderItem(item.OderItemId.Value);
                    if (obj != null)
                    {
                        item.Color = obj.Color;
                        item.Size = obj.Size;
                        item.Version = obj.Version;
                        item.OrderId = obj.OrderId;

                        var member = MemberApplication.GetMember(item.UserId);
                        if (member != null)
                        {
                            item.UserName = member.UserName;
                            item.UserPhone = member.CellPhone;
                        }
                    }
                }
                Entities.TypeInfo typeInfo = iTypeService.GetTypeByProductId(item.ProductId);
                item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                var productInfo = ProductManagerApplication.GetProduct(item.ProductId);
                if (productInfo != null)
                {
                    item.ColorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : item.ColorAlias;
                    item.SizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : item.SizeAlias;
                    item.VersionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : item.VersionAlias;
                }
            }
            DataGridModel<ProductCommentModel> model = new DataGridModel<ProductCommentModel>() { rows = comments, total = result.Total };
            return Json(model);
        }

        [HttpPost]
        public JsonResult Detail(long id)
        {
            var model = _iCommentService.GetComment(id);
            return Json(new { ConsulationContent = model.ReviewContent, ReplyContent = model.ReplyContent });
        }

        [HttpPost]
        public JsonResult ReplyComment(long id, string replycontent, string appendContent)
        {
            var shopid = base.CurrentSellerManager.ShopId;
            _iCommentService.ReplyComment(id, shopid, replycontent, appendContent);
            return Json(new Result() { success = true, msg = "回复成功！" });
        }


        [HttpPost]
        public ActionResult GetComment(long Id)
        {
            var model = _iCommentService.GetComment(Id);
            var commentImages = _iCommentService.GetProductCommentImagesByCommentIds(new List<long> { Id });
            foreach (var item in commentImages)
                item.CommentImage = Mall.Core.MallIO.GetImagePath(item.CommentImage);
            ViewBag.Images = commentImages;
            return View(model);
        }
    }
}
