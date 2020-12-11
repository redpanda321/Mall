using Mall.Application;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class OrderEvaluationController : BaseMemberController
    {
        // GET: Web/OrderEvaluation
        public ActionResult Index(long id)
        {
            var model = CommentApplication.GetProductEvaluationByOrderId(id, CurrentUser.Id);
            var orderEvaluation = TradeCommentApplication.GetOrderComment(id, CurrentUser.Id);
            if (orderEvaluation != null)
                ViewBag.Mark = Math.Round((orderEvaluation.PackMark + orderEvaluation.ServiceMark + orderEvaluation.DeliveryMark) / 3D);
            else
                ViewBag.Mark = 0;
            ViewBag.OrderId = id;
            //检查当前产品是否产自官方自营店
            ViewBag.IsSellerAdminProdcut = OrderApplication.GetOrder(id).ShopId.Equals(1L);
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            var orderInfo = OrderApplication.GetOrder(id);
            if (orderInfo != null)
            {
                ViewBag.IsVirtual = orderInfo.OrderType == Mall.Entities.OrderInfo.OrderTypes.Virtual ? 1 : 0;
            }
            return View(model);
        }

        public ActionResult Satisfied(int pageSize = 15, int pageNo = 1)
        {
            var query = new OrderQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = Entities.OrderInfo.OrderOperateStatus.Finish;
            query.UserId = CurrentUser.Id;
            query.Sort = "CommentCount";
            query.IsAsc = true;

            var model = OrderApplication.GetOrders(query);
            ViewBag.Comments = CommentApplication.GetOrderCommentByOrder(model.Models.Select(p => p.Id));

            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            #endregion
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(model.Models);
        }

        //没任何地方调用了
        //public JsonResult AddOrderEvaluation(DTO.OrderComment model)
        //{
        //    model.UserId = CurrentUser.Id;
        //    TradeCommentApplication.Add(model);
        //    return Json(new Result() { success = true, msg = "评价成功" });
        //}

        private string MoveImages(string image, long userId)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var oldname = Path.GetFileName(image);
            string ImageDir = string.Empty;

            //转移图片
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = userId + oldname;
            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var de = image.Substring(image.LastIndexOf("/temp/"));
                Core.MallIO.CopyFile(de, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else if (image.Contains("/Storage"))
            {
                return image.Substring(image.LastIndexOf("/Storage"));
            }
            return image;
        }


        [HttpPost]
        public JsonResult AddOrderEvaluationAndComment(int packMark, int deliveryMark, int serviceMark, long orderId, string productCommentsJSON)
        {
            var productComments = JsonConvert.DeserializeObject<List<ProductCommentsModel>>(productCommentsJSON);

            if (packMark != 0 || deliveryMark != 0 || serviceMark == 0)
            {
                var info = new DTO.OrderComment();
                info.UserId = CurrentUser.Id;
                info.PackMark = packMark;
                info.DeliveryMark = deliveryMark;
                info.ServiceMark = serviceMark;
                info.OrderId = orderId;
                TradeCommentApplication.Add(info, productComments.Count);
            }

            var list = new List<DTO.ProductComment>();
            foreach (var productComment in productComments)
            {
                var model = new ProductComment();
                model.ReviewDate = DateTime.Now;
                model.ReviewContent = (productComment.content.Equals("无") || string.IsNullOrEmpty(productComment.content)) ? "好评！" : productComment.content;
                model.UserId = CurrentUser.Id;
                model.UserName = CurrentUser.UserName;
                model.Email = CurrentUser.Email;
                model.SubOrderId = productComment.subOrderId;
                model.ReviewMark = productComment.star;
                if (productComment.proimages != null && productComment.proimages.Length > 0)
                {
                    model.Images = new List<ProductCommentImage>();
                    foreach (var img in productComment.proimages)
                    {
                        var p = new ProductCommentImage();
                        p.CommentType = 0;//0代表默认的表示评论的图片
                        p.CommentImage = MoveImages(img, CurrentUser.Id);
                        model.Images.Add(p);
                    }
                }

                list.Add(model);
            }
            var comments = CommentApplication.GetProductEvaluationByOrderIdNew(orderId, CurrentUser.Id);
            foreach (var item in comments)
            {
                var addComment = productComments.FirstOrDefault(e => e.subOrderId == item.Id);
                if (addComment != null)
                {
                    return Json(new Result() { success = false, msg = "您已进行过评价！", status = -1 });
                }
            }
            CommentApplication.Add(list);

            return Json(new Result() { success = true, msg = "评价成功" });
        }

        public ActionResult Details(long orderId)
        {
            var orderComment = TradeCommentApplication.GetOrderComment(orderId, CurrentUser.Id);
            ViewBag.PackMark = orderComment != null ? orderComment.PackMark - 1 : -1;
            ViewBag.DeliveryMark = orderComment != null ? orderComment.DeliveryMark - 1 : -1;
            ViewBag.ServiceMark = orderComment != null ? orderComment.ServiceMark - 1 : -1;
            var model = CommentApplication.GetProductEvaluationByOrderIdNew(orderId, CurrentUser.Id);
            var orderInfo = OrderApplication.GetOrder(orderId);
            bool isVirtual = false;
            if (orderInfo != null)
            {
                ViewBag.IsSellerAdminProdcut = orderInfo.ShopId.Equals(1L);
                isVirtual = orderInfo.OrderType == Entities.OrderInfo.OrderTypes.Virtual;
            }
            ViewBag.IsVirtual = isVirtual;
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(model);
        }

        /// <summary>
        /// 追加评论
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult AppendComment(long orderId)
        {
            var model = CommentApplication.GetProductEvaluationByOrderIdNew(orderId, CurrentUser.Id);
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(model);
        }


        public ActionResult AppendProductComment(string productCommentsJSON)
        {
            var productComments = JsonConvert.DeserializeObject<List<AppendCommentModel>>(productCommentsJSON);
            var comments = CommentApplication.GetCommentss(productComments.Select(e => e.Id));
            foreach (var m in productComments)
            {
                var comment = comments.FirstOrDefault(e => e.Id == m.Id && !string.IsNullOrWhiteSpace(e.AppendContent));
                if (comment != null)
                {
                    return Json(new Result() { success = false, msg = "您已追加过评价，不需要再重复操作！", status = -1 });
                }
                m.UserId = CurrentUser.Id;
            }

            CommentApplication.Append(productComments);
            return Json(new Result() { success = true, msg = "追加成功" });
        }
    }
}