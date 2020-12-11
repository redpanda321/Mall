using Mall.API.Model;
using Mall.API.Model.ParamsModel;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.API
{
    public class CommentController : BaseApiController
    {

        [HttpGet("GetComment")]
        public object GetComment(long orderId)
        {
            CheckUserLogin();
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId);
            var comments = OrderApplication.GetOrderComment(orderId);

            if (order != null && comments.Count == 0)
            {
                var model = ServiceProvider.Instance<ICommentService>.Create.GetProductEvaluationByOrderId(orderId, CurrentUser.Id).Select(item => new
                {
                    item.ProductId,
                    item.ProductName,
                    Image = Core.MallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_220) //商城App评论时获取商品图片
                });

                var orderEvaluation = ServiceProvider.Instance<ITradeCommentService>.Create.GetOrderCommentInfo(orderId, CurrentUser.Id);
                var orderitems = OrderApplication.GetOrderItems(order.Id);
                var isVirtual = order.OrderType == Mall.Entities.OrderInfo.OrderTypes.Virtual ? 1 : 0;
                return new { success = true, Product = model, orderItemIds = orderitems.Select(item => item.Id), isVirtual = isVirtual };
            }
            else
            {
                throw new MallApiException("该订单不存在或者已评论过");
            }
        }
        [HttpPost("PostAddComment")]
        public object PostAddComment(CommentAddCommentModel value)
        {
            CheckUserLogin();
            string Jsonstr = value.Jsonstr;
            bool result = false;
            var orderComment = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderCommentModel>(Jsonstr);
            if (orderComment != null)
            {
                AddOrderComment(orderComment);//添加订单评价
                AddProductsComment(orderComment.OrderId, orderComment.ProductComments);//添加商品评论
                result = true;
            }
            return new { success = result };
        }

        void AddOrderComment(OrderCommentModel comment)
        {
            ServiceProvider.Instance<ITradeCommentService>.Create.AddOrderComment(new Mall.Entities.OrderCommentInfo()
            {
                OrderId = comment.OrderId,
                DeliveryMark = comment.DeliveryMark,
                ServiceMark = comment.ServiceMark,
                PackMark = comment.PackMark,
                UserId = CurrentUser.Id,

            }, comment.ProductComments.Count());
        }

        void AddProductsComment(long orderId, IEnumerable<ProductCommentModel> productComments)
        {
            var commentService = ServiceProvider.Instance<ICommentService>.Create;
            foreach (var productComment in productComments)
            {
                Entities.ProductCommentInfo model = new Entities.ProductCommentInfo();
                model.ReviewDate = DateTime.Now;
                model.ReviewContent = productComment.Content;
                model.UserId = CurrentUser.Id;
                model.UserName = CurrentUser.UserName;
                model.Email = CurrentUser.Email;
                model.SubOrderId = productComment.OrderItemId;
                model.ReviewMark = productComment.Mark;
                model.ProductId = productComment.ProductId;
                if (productComment.Images != null && productComment.Images.Length > 0)
                {
                    model.ProductCommentImageInfo = productComment.Images.Select(item => new Entities.ProductCommentImageInfo
                    {
                        CommentType = 0,//0代表默认的表示评论的图片
                        CommentImage = MoveImages(item, CurrentUser.Id)
                    }).ToList();
                }
                commentService.AddComment(model, true);
            }
        }

        private string MoveImages(string image, long userId)
        {
            string OriUrl = Core.Helper.IOHelper.GetMapPath(image);
            var oldname = new System.IO.FileInfo(OriUrl).Name;
            string ImageDir = string.Empty;

            //转移图片
            ImageDir = Core.Helper.IOHelper.GetMapPath("/Storage/Plat/Comment");
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = userId + oldname;
            if (!System.IO.Directory.Exists(ImageDir))
                System.IO.Directory.CreateDirectory(ImageDir);//创建图片目录

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                Core.Helper.IOHelper.CopyFile(OriUrl, ImageDir, false, fileName);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                return image;
            }
        }


        [HttpGet("GetAppendComment")]
        public object GetAppendComment(long orderid)
        {
            CheckUserLogin();
            var model = ServiceProvider.Instance<ICommentService>.Create.GetProductEvaluationByOrderIdNew(orderid, CurrentUser.Id);

            if (model.FirstOrDefault().AppendTime.HasValue)
                return new { success = false, msg = "追加评论时，获取数据异常" };
            else
            {
                var listResult = model.Select(item => new
                {
                    Id = item.Id,
                    CommentId = item.CommentId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    //ThumbnailsUrl = item.ThumbnailsUrl,
                    ThumbnailsUrl = Core.MallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_220), //商城App追加评论时获取商品图片
                    BuyTime = item.BuyTime,
                    EvaluationStatus = item.EvaluationStatus,
                    EvaluationContent = item.EvaluationContent,
                    AppendContent = item.AppendContent,
                    AppendTime = item.AppendTime,
                    EvaluationTime = item.EvaluationTime,
                    ReplyTime = item.ReplyTime,
                    ReplyContent = item.ReplyContent,
                    ReplyAppendTime = item.ReplyAppendTime,
                    ReplyAppendContent = item.ReplyAppendContent,
                    EvaluationRank = item.EvaluationRank,
                    OrderId = item.OrderId,
                    CommentImages = item.CommentImages.Select(r => new
                    {
                        CommentImage = r.CommentImage,
                        CommentId = r.CommentId,
                        CommentType = r.CommentType
                    }).ToList(),
                    Color = item.Color,
                    Size = item.Size,
                    Version = item.Version
                }).ToList();
                return new { success = true, List = listResult };
            }
        }
        [HttpPost("PostAppendComment")]
        public object PostAppendComment(CommentAppendCommentModel value)
        {
            CheckUserLogin();
            string productCommentsJSON = value.productCommentsJSON;
            var commentService = ServiceProvider.Instance<ICommentService>.Create;
            var productComments = JsonConvert.DeserializeObject<List<AppendCommentModel>>(productCommentsJSON);

            foreach (var m in productComments)
            {
                m.UserId = CurrentUser.Id;
                ;
            }
            commentService.AppendComment(productComments);
            return new { success = true };
        }
    }
}
