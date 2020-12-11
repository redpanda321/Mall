using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Mall.Service
{
    public class CommentService : ServiceBase, ICommentService
    {
        public void AddComment(ProductCommentInfo model, bool IsStartTransaction = false)
        {
            var m = DbFactory.Default.Get<OrderItemInfo>().InnerJoin<OrderInfo>((item, o) => item.OrderId == o.Id).Where<OrderItemInfo, OrderInfo>((item, o) => item.Id == model.SubOrderId && o.UserId == model.UserId).FirstOrDefault<OrderItemInfo>();
            if (m == null)
            {
                throw new Mall.Core.MallException("不能对此商品进行评价！");
            }
            model.ShopId = m.ShopId;
            model.ProductId = m.ProductId;
            model.ShopName = DbFactory.Default.Get<ShopInfo>().Where(a => a.Id == m.ShopId).Select(a => a.ShopName).FirstOrDefault<string>();
            model.IsHidden = false;

            //是否外面已开始了事物，如开启了事物这里不要再嵌套事物(嵌套是否会报错)
            if (IsStartTransaction)
            {
                DbFactory.Default.Add(model);
                //保存评价图片
                if (model.ProductCommentImageInfo != null)
                {
                    model.ProductCommentImageInfo.ForEach(p => p.CommentId = model.Id);
                    DbFactory.Default.AddRange(model.ProductCommentImageInfo);
                }
                //更新搜索商品评论数
                DbFactory.Default.Set<SearchProductInfo>().Set(n => n.Comments, n => n.Comments + 1).Where(n => n.ProductId == m.ProductId).Succeed();
            }
            else
            {
                DbFactory.Default.InTransaction(() =>
                {
                    DbFactory.Default.Add(model);
                    //保存搜索图片
                    if (model.ProductCommentImageInfo != null)
                    {
                        model.ProductCommentImageInfo.ForEach(p => p.CommentId = model.Id);
                        DbFactory.Default.AddRange(model.ProductCommentImageInfo);
                    }
                    DbFactory.Default.Set<SearchProductInfo>().Set(n => n.Comments, n => n.Comments + 1).Where(n => n.ProductId == m.ProductId).Succeed();
                });
            }
        }

        public void AddComment(IEnumerable<ProductCommentInfo> models)
        {
            DbFactory.Default.InTransaction(() =>
            {
                foreach (var model in models)
                {
                    var m = DbFactory.Default.Get<OrderItemInfo>().InnerJoin<OrderInfo>((item, o) => item.Id == model.SubOrderId && o.UserId == model.UserId && item.OrderId == o.Id).FirstOrDefault<OrderItem>();
                    if (m == null)
                    {
                        throw new Mall.Core.MallException("不能对此商品进行评价！");
                    }
                    model.ShopId = m.ShopId;
                    model.ProductId = m.ProductId;
                    model.ShopName = DbFactory.Default.Get<ShopInfo>().Where(a => a.Id == m.ShopId).Select(a => a.ShopName).FirstOrDefault<string>();
                    model.IsHidden = false;
                    DbFactory.Default.Add(model);
                    if (model.ProductCommentImageInfo != null)
                    {
                        model.ProductCommentImageInfo.ForEach(p => p.CommentId = model.Id);
                        DbFactory.Default.AddRange(model.ProductCommentImageInfo);
                    }
                    DbFactory.Default.Set<SearchProductInfo>().Set(n => n.Comments, n => n.Comments + 1).Where(n => n.ProductId == m.ProductId).Succeed();
                }
            });
        }

        

        public void HiddenComment(long id)
        {
            //var model = DbFactory.Default.Get<ProductCommentInfo>().Where(item => item.Id == id).FirstOrDefault();
            //model.IsHidden = true;

            //DbFactory.Default.Update(model);
            DbFactory.Default.Set<ProductCommentInfo>().Set(n => n.IsHidden, true).Where(item => item.Id == id).Succeed();
        }

        public void SetCommentEmpty(long id)
        {
           
            DbFactory.Default.Set<ProductCommentInfo>().Set(n => n.ReviewContent, "").Set(n => n.ReplyContent, "").Where(n => n.Id == id).Succeed();
        }

        public QueryPageModel<ProductCommentInfo> GetComments(CommentQuery query)
        {
            var db = WhereBuilder(query);
            var data = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<ProductCommentInfo>() { Models = data, Total = data.TotalRecordCount };
        }

        public int GetCommentCount(CommentQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        public GetBuilder<ProductCommentInfo> WhereBuilder(CommentQuery query) {
            var db = DbFactory.Default.Get<ProductCommentInfo>();
            #region 条件组合
            if (query.IsReply.HasValue)
            {
                if (!query.IsReply.Value)
                    db.Where(item => item.ReplyDate.ExIsNull() || (item.ReplyAppendDate.ExIsNull() && item.AppendDate.ExIsNotNull()));
                else
                    db.Where(item => item.ReplyDate.ExIsNotNull() && item.ReplyAppendDate.ExIsNotNull());
            }
            if (query.ShopID > 0)
                db.Where(item => query.ShopID == item.ShopId);

            if (query.ProductID > 0)
                db.Where(item => query.ProductID == item.ProductId);

            if (query.Rank >=0)
                switch (query.Rank)
                {
                    case 0: //好评
                        db.Where(item => item.ReviewMark == 4 || item.ReviewMark == 5);
                        break;
                    case 1: //中评
                        db.Where(item => item.ReviewMark == 3);
                        break;
                    case 2: //差评
                        db.Where(item => item.ReviewMark <= 2);
                        break;
                }

            if (query.HasAppend)
                db.Where(item => item.AppendDate.ExIsNotNull());


            if (query.UserID > 0)
                db.Where(item => query.UserID == item.UserId);

            if (!string.IsNullOrWhiteSpace(query.KeyWords))
                db.Where(item => item.ReviewContent.Contains(query.KeyWords));

            if (!string.IsNullOrWhiteSpace(query.ProductName))
            {
                var childSql = DbFactory.Default.Get<ProductInfo>()
                    .Where<ProductCommentInfo>((b, a) => b.ProductName.Contains(query.ProductName) && a.ProductId == b.Id).Select(a => a.Id);
                db.Where(a => a.ExExists(childSql));
            }
            if (query.HasAppend)
                db.Where(b => b.AppendDate.ExIsNotNull());

            if (query.IsHidden.HasValue)
                db.Where(p => p.IsHidden == query.IsHidden);
            #endregion

            return db;
        }

        public ProductCommentInfo GetComment(long id)
        {
            return DbFactory.Default.Get<ProductCommentInfo>().Where(p => p.Id == id).FirstOrDefault();
        }


        public List<ProductCommentInfo> GetCommentByProduct(long product, long shopbranchId = 0) {
            var comments = DbFactory.Default.Get<ProductCommentInfo>().Where(p => p.ProductId == product);
            if(shopbranchId > 0)
            {
                //TODO:FG 查询待优化
                var ordsql = DbFactory.Default.Get<OrderInfo>().Where(d => d.ShopBranchId == shopbranchId).Select(d => d.Id).ToList<long>();
                var orditemsql = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId.ExIn(ordsql)).Select(d => d.Id).ToList<long>();
                comments.Where(d => d.SubOrderId.ExIsNotNull() && d.SubOrderId.ExIn(orditemsql));
            }
            return comments.ToList();
        }
        public List<ProductCommentInfo> GetCommentByProduct(List<long> products)
        {
            return DbFactory.Default.Get<ProductCommentInfo>().Where(p => p.ProductId.ExIn(products)).ToList();
        }
        public IEnumerable<ProductCommentInfo> GetCommentsByIds(IEnumerable<long> ids)
        {
            return DbFactory.Default.Get<ProductCommentInfo>().Where(e => e.Id.ExIn(ids)).ToList();
        }
        public QueryPageModel<UserOrderCommentModel> GetOrderComment(OrderCommentQuery query)
        {
            var model = DbFactory.Default.Get<OrderCommentInfo>().Where(a => a.UserId == query.UserId).OrderByDescending(item => item.Id).ToPagedList(query.PageNo, query.PageSize);
            var OrderCommentModel = model.Select(a => new UserOrderCommentModel { CommentTime = a.CommentDate, OrderId = a.OrderId }).ToList();
            QueryPageModel<UserOrderCommentModel> pageModel = new QueryPageModel<UserOrderCommentModel>()
            {
                Models = OrderCommentModel,
                Total = model.TotalRecordCount
            };
            return pageModel;
        }


        /// <summary>
        /// 根据订单ID获取订单商品的评价
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ProductEvaluation> GetProductEvaluationByOrderId(long orderId, long userId)
        {
            var items = DbFactory.Default.Get<OrderItemInfo>()
                .InnerJoin<OrderInfo>((item, o) => item.OrderId == o.Id && item.OrderId == orderId && o.UserId == userId && o.OrderStatus == OrderInfo.OrderOperateStatus.Finish);
            var order = DbFactory.Default.Get<OrderInfo>(p => p.Id == orderId).FirstOrDefault();
            var childOrder = DbFactory.Default.Get<ProductCommentInfo>().Where<OrderItemInfo>((p, o) => p.ProductId == o.ProductId && p.UserId == userId && p.SubOrderId == o.Id).Select(a => a.Id);
            items.OrderBy(a => a.ExExists(childOrder));
            var data = items.Select<OrderItemInfo>().ToList();
            var orderDate = DateTime.Now;
            if (null == items || items.Count() <= 0)
            {
                var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
                if (flag)
                {
                    var o = DbFactory.MongoDB.AsQueryable<OrderInfo>().FirstOrDefault(a => a.Id == orderId && a.UserId == userId && a.OrderStatus == OrderInfo.OrderOperateStatus.Finish);
                    if (null != o)
                    {
                        orderDate = o.OrderDate;
                        var mongoorder = DbFactory.MongoDB.AsQueryable<OrderItemInfo>().Where(a => a.OrderId == orderId).ToList();
                        var productcomments = DbFactory.Default.Get<ProductCommentInfo>().Where(p => p.UserId == userId).ToList();
                        data = mongoorder.OrderBy(a => productcomments.Any(b => b.SubOrderId == a.Id && b.ProductId == a.ProductId)).ToList();
                    }
                }
            }
            var model = data.Select(a => new ProductEvaluation
            {
                ProductId = a.ProductId,
                ThumbnailsUrl = a.ThumbnailsUrl,
                ProductName = a.ProductName,
                BuyTime = order == null ? orderDate : order.OrderDate,
                EvaluationStatus = DbFactory.Default.Get<ProductCommentInfo>().Where(b => b.ProductId == a.ProductId && b.UserId == userId && b.SubOrderId == a.Id).Exist(),
                Id = a.Id,
                OrderId = a.OrderId,
                Color = a.Color,
                Size = a.Size,
                Version = a.Version,
                SkuId = a.SkuId,
                Price = a.SalePrice
            }).ToList();
            foreach (var pe in model)
            {
                TypeInfo typeInfo = DbFactory.Default.Get<TypeInfo>().InnerJoin<ProductInfo>((t, p) => t.Id == p.TypeId && p.Id == pe.ProductId).FirstOrDefault();
                ProductInfo productInfo = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == pe.ProductId).FirstOrDefault();
                pe.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                pe.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                pe.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                if (productInfo != null)
                {
                    if (!string.IsNullOrWhiteSpace(productInfo.ColorAlias))//如果商品有自定义规格名则用
                    {
                        pe.ColorAlias = productInfo.ColorAlias;
                    }
                    if (!string.IsNullOrWhiteSpace(productInfo.SizeAlias))
                    {
                        pe.SizeAlias = productInfo.SizeAlias;
                    }
                    if (!string.IsNullOrWhiteSpace(productInfo.VersionAlias))
                    {
                        pe.VersionAlias = productInfo.VersionAlias;
                    }
                }
            }
            return model;
        }

        public List<OrderItemInfo> GetUnEvaluatProducts(long userId)
        {
            var orderitem = DbFactory.Default
                .Get<OrderItemInfo>()
                .LeftJoin<OrderInfo>((oii, oi) => oii.OrderId == oi.Id)
                .Where<OrderInfo>((item, o) => o.UserId == userId && o.OrderStatus == OrderInfo.OrderOperateStatus.Finish);

            var childSql = DbFactory.Default
                .Get<ProductCommentInfo>()
                .Where<OrderItemInfo>((c, item) => c.ProductId == item.ProductId && c.UserId == userId && c.SubOrderId == item.Id)
                .Select(a => a.Id);

            orderitem.Where(o => o.ExExists(childSql) == false);
            return orderitem.OrderByDescending(n => n.Id).ToList();
            //var order = Context.OrderItemInfo.Where(a => a.OrderInfo.UserId == userId && a.OrderInfo.OrderStatus == Mall.Model.OrderInfo.OrderOperateStatus.Finish && !Context.ProductCommentInfo.Any(b => b.ProductId == a.ProductId && b.UserId == userId && b.SubOrderId == a.Id)).OrderByDescending(a => a.Id);
            //return order;
        }

        public decimal GetProductMark(long id)
        {
            string cacheKey = CacheKeyCollection.CACHE_PRODUCTMARK(id);
            if (Cache.Exists(cacheKey))
                return Cache.Get<decimal>(cacheKey);
            var result = DbFactory.Default.Get<ProductCommentInfo>().Where(n => n.ProductId == id && n.IsHidden != true).Avg<decimal>(n => n.ReviewMark);

            //默认商品评价
            if (result <= 0)
                result = 5;

            Cache.Insert(cacheKey, result, 600);
            return result;
        }

        /// <summary>
        /// 有否有追加评论
        /// </summary>
        /// <param name="subOrderId"></param>
        /// <returns></returns>
        public bool HasAppendComment(long subOrderId)
        {
            return DbFactory.Default.Get<ProductCommentInfo>().Where(a => a.SubOrderId == subOrderId && a.AppendDate.ExIsNotNull()).Exist();
        }


        public List<ProductEvaluation> GetProductEvaluationByOrderIdNew(long orderId, long userId)
        {
            var model = DbFactory.Default.Get<ProductCommentInfo>().InnerJoin<OrderItemInfo>((c, item) => item.OrderId == orderId && c.SubOrderId == item.Id && c.UserId == userId)
                .InnerJoin<ProductInfo>((c, p) => c.ProductId == p.Id)
                .InnerJoin<OrderInfo, OrderItemInfo>((o, item) => item.OrderId == o.Id)
                .Select(c => new
                {
                    ProductId = c.ProductId,
                    CommentId = c.Id,
                    EvaluationStatus = true,
                    Id = c.SubOrderId,
                    EvaluationRank = c.ReviewMark - 1,
                    EvaluationContent = c.ReviewContent,
                    EvaluationTime = c.ReviewDate,
                    ReplyContent = c.ReplyContent,
                    ReplyTime = c.ReplyDate.Value,
                    ReplyAppendContent = c.ReplyAppendContent,
                    ReplyAppendTime = c.ReplyAppendDate,
                    AppendContent = c.AppendContent,
                    AppendTime = c.AppendDate
                }).Select<OrderItemInfo>(item => new
                {
                    ThumbnailsUrl = item.ThumbnailsUrl,
                    OrderId = item.OrderId,
                    Color = item.Color,
                    Size = item.Size,
                    Version = item.Version
                }).Select<ProductInfo>(p => new
                {
                    ProductName = p.ProductName
                }).Select<OrderInfo>(o => new
                {
                    BuyTime = o.OrderDate
                });
            var ret = model.ToList<ProductEvaluation>();
            foreach (var pe in ret)
            {
                pe.CommentImages = DbFactory.Default.Get<ProductCommentImageInfo>().Where(a => a.CommentId == pe.CommentId).ToList();
                var typeInfo = DbFactory.Default.Get<TypeInfo>().InnerJoin<ProductInfo>((t, p) => t.Id == p.TypeId && p.Id == pe.ProductId).FirstOrDefault();
                var prodata = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == pe.ProductId).FirstOrDefault();
                //ProductTypeInfo typeInfo = (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == pe.ProductId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
                pe.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                pe.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                pe.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                if (prodata != null)
                {
                    pe.ColorAlias = !string.IsNullOrWhiteSpace(prodata.ColorAlias) ? prodata.ColorAlias : pe.ColorAlias;
                    pe.SizeAlias = !string.IsNullOrWhiteSpace(prodata.SizeAlias) ? prodata.SizeAlias : pe.SizeAlias;
                    pe.VersionAlias = !string.IsNullOrWhiteSpace(prodata.VersionAlias) ? prodata.VersionAlias : pe.VersionAlias;
                }
            }

            return ret;
        }


        public void ReplyComment(long id, long shopId, string replyContent = "", string appendContent = "")
        {
            var model = DbFactory.Default.Get<ProductCommentInfo>().Where(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (shopId == 0 || model == null)
            {
                throw new Mall.Core.MallException("不存在该商品评论");
            }
            if (!string.IsNullOrEmpty(replyContent))
            {
                model.ReplyContent = replyContent;
                model.ReplyDate = DateTime.Now;
            }
            if (!string.IsNullOrEmpty(appendContent))
            {
                model.ReplyAppendContent = appendContent;
                model.ReplyAppendDate = DateTime.Now;
            }
            DbFactory.Default.Update(model);
        }


        private string MoveImages(string image, long userId)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return string.Empty;
            }
            string ImageDir = string.Empty;
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = userId + Path.GetFileName(image);

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(temp, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                return relativeDir + fileName;
            }
        }

        private string DownloadWxImage(string mediaId)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            var token = AccessTokenContainer.TryGetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
            var address = string.Format("https://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);
            Random ra = new Random();
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ra.Next(10) + ".jpg";
            var ImageDir = "/Storage/Plat/Comment/";
            WebClient wc = new WebClient();
            try
            {
                string fullPath = Path.Combine(ImageDir, fileName);
                var data = wc.DownloadData(address);
                MemoryStream stream = new MemoryStream(data);
                Core.MallIO.CreateFile(fullPath, stream, FileCreateType.Create);
                return fullPath;
            }
            catch (Exception ex)
            {
                Log.Error("下载图片发生异常" + ex.Message);
                return string.Empty;
            }
        }



        public void AppendComment(List<AppendCommentModel> list)
        {
            var userid = 0L;
            var userName = "";
            long orderId = 0;
            var addImageList = new List<ProductCommentImageInfo>();

            DbFactory.Default.InTransaction(() =>
            {

                foreach (var model in list)
                {
                    var m = DbFactory.Default.Get<ProductCommentInfo>().Where(item => item.Id == model.Id && item.UserId == model.UserId).FirstOrDefault();
                    if (model.UserId == 0 || m == null)
                    {
                        throw new Mall.Core.MallException("该商品尚未评论，请先评论。");
                    }
                    if (m.AppendDate.HasValue)
                    {
                        throw new Mall.Core.MallException("您已追加评价过了，不需再重复操作。");
                    }
                    userid = m.UserId;
                    userName = m.UserName;
                    var orderItem = OrderApplication.GetOrderItem(m.SubOrderId);
                    orderId = orderItem.OrderId;
                    m.AppendContent = model.AppendContent;
                    m.AppendDate = DateTime.Now;
                    if (model.Images != null && model.Images.Length > 0)
                    {
                        foreach (var img in model.Images)
                        {
                            var p = new ProductCommentImageInfo();
                            p.CommentType = 1;//1代表表示追加评论的图片
                            p.CommentImage = MoveImages(img, model.UserId);
                            p.CommentId = model.Id;
                            addImageList.Add(p);
                        }
                    }
                    else if (model.WXmediaId != null && model.WXmediaId.Length > 0)
                    {
                        foreach (var img in model.WXmediaId)
                        {
                            var p = new ProductCommentImageInfo();
                            p.CommentType = 1;//1表示追加的图片
                            p.CommentImage = DownloadWxImage(img);
                            p.CommentId = model.Id;
                            if (!string.IsNullOrEmpty(p.CommentImage))
                            {
                                addImageList.Add(p);
                            }
                        }
                    }
                    DbFactory.Default.Update(m);
                }
                DbFactory.Default.Add<ProductCommentImageInfo>(addImageList);
            });

          

        }
        /// <summary>
        /// 获取商品评价列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ProductCommentInfo> GetProductComments(ProductCommentQuery query)
        {
            var sql = WhereBuilder(query);
            sql.OrderByDescending(d => d.ReviewDate);
            var data = sql.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<ProductCommentInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        public int GetCommentCount(ProductCommentQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        /// <summary>
        /// 获取商品评价数量聚合
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ProductCommentCountAggregateModel GetProductCommentStatistic(long? productId = null, long? shopId = null, long? shopBranchId = null,bool iso2o = false)
        {
            var query = new ProductCommentQuery
            {
                ProductId = productId
            };
            if (shopId.HasValue && shopId > 0)
            {
                query.ShopId = shopId.Value;
            }
            if (shopBranchId.HasValue && shopBranchId > 0)
            {
                query.ShopBranchId = shopBranchId.Value;
            }
            query.IsO2O = iso2o;
            ProductCommentCountAggregateModel result = new ProductCommentCountAggregateModel();
            result.AllComment = WhereBuilder(query).Count();
            result.LowComment = WhereBuilder(query).Where(d => d.ReviewMark <= (int)ProductCommentMarkType.Low).Count();
            result.MediumComment = WhereBuilder(query).Where(d => d.ReviewMark == (int)ProductCommentMarkType.Medium).Count();
            result.HighComment = WhereBuilder(query).Where(d => d.ReviewMark >= (int)ProductCommentMarkType.High).Count();
            result.AppendComment = WhereBuilder(query).Where(d => d.AppendDate.ExIsNotNull()).Count();
            var childSql = DbFactory.Default.Get<ProductCommentImageInfo>().Where<ProductCommentInfo>((img, c) => img.CommentId == c.Id).Select(a => a.Id);
            result.HasImageComment = WhereBuilder(query).Where(a => a.ExExists(childSql)).Count();
            return result;
        }

        public decimal GetProductAverageMark(long product)
        {
            return DbFactory.Default.Get<ProductCommentInfo>()
                .Where(p => p.ProductId == product &&  p.IsHidden == false)
                .Avg<decimal>(p => p.ReviewMark);
        }

        public int GetProductCommentCount(long product)
        {
            return DbFactory.Default.Get<ProductCommentInfo>()
               .Where(p => p.ProductId == product && p.IsHidden == false)
               .Count();
        }



        public List<OrderCommentInfo> GetOrderCommentsByOrder(IEnumerable<long> orders) {
            return DbFactory.Default.Get<OrderCommentInfo>()
                .Where(p => p.OrderId.ExIn(orders))
                .ToList();
        }

        /// <summary>
        /// 获取商品评价好评数
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public int GetProductHighCommentCount(long? productId = null, long? shopId = null, long? shopBranchId = null)
        {
            ProductCommentQuery query = new ProductCommentQuery()
            {
                ProductId = productId
            };
            if (shopId.HasValue && shopId > 0)
            {
                query.ShopId = shopId.Value;
            }
            if (shopBranchId.HasValue && shopBranchId > 0)
            {
                query.ShopBranchId = shopBranchId.Value;
            }
            var sql = WhereBuilder(query);
            return sql.Where(d => d.ReviewMark >= (int)ProductCommentMarkType.High).Count();
        }

        private GetBuilder<ProductCommentInfo> WhereBuilder(ProductCommentQuery query)
        {
            var db = DbFactory.Default.Get<ProductCommentInfo>();
            if (query.ProductId.HasValue)
                db.Where(d => d.ProductId == query.ProductId.Value);

            if (!query.ShowHidden)
                db.Where(d => d.IsHidden == false);

            if (query.IsReply.HasValue)
            {
                if (query.IsReply.Value)
                    db.Where(p => p.ReplyDate.ExIsNotNull());
                else
                    db.Where(p => p.ReplyDate.ExIsNull() || (p.ReplyAppendDate.ExIsNull() && p.AppendDate.ExIsNotNull()));//初是为评论或追加未评论
            }
            if (query.ShopId.HasValue)
                db.Where(d => d.ShopId == query.ShopId.Value);

            if (query.CommentType.HasValue)
            {
                switch (query.CommentType)
                {
                    case ProductCommentMarkType.Low:
                        db.Where(d => d.ReviewMark <= (int)ProductCommentMarkType.Low);
                        break;
                    case ProductCommentMarkType.Medium:
                        db.Where(d => d.ReviewMark == (int)ProductCommentMarkType.Medium);
                        break;
                    case ProductCommentMarkType.High:
                        db.Where(d => d.ReviewMark >= (int)ProductCommentMarkType.High);
                        break;
                    case ProductCommentMarkType.HasImage:
                        var childSql = DbFactory.Default.Get<ProductCommentImageInfo>().Where<ProductCommentInfo>((img, p) => img.CommentId == p.Id).Select(a => a.Id);
                        db.Where(d => d.ExExists(childSql));
                        break;
                    case ProductCommentMarkType.Append:
                        db.Where(d => d.AppendDate.ExIsNotNull());
                        break;
                }
            }

            if (query.ShopBranchId.HasValue)
            {
                //TODO:FG 查询待优化
                var ordsql = DbFactory.Default.Get<OrderInfo>().Where(d => d.ShopBranchId == query.ShopBranchId.Value).Select(d => d.Id).ToList<long>();
                var orditemsql = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId.ExIn(ordsql)).Select(d => d.Id).ToList<long>();
                db.Where(d => d.SubOrderId.ExIsNotNull() && d.SubOrderId.ExIn(orditemsql));
                
            }
            if(query.IsO2O)
            {
                //TODO:ZYF O2O门店评价排除虚拟商品
                var productsql = DbFactory.Default.Get<ProductInfo>().Where(p => p.ProductType == 1).Select(p => p.Id).ToList<long>();
                db.Where(d => d.ProductId.ExNotIn(productsql));
            }
            return db;
        }

        /// <summary>
        /// 获取评论图片
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public List<ProductCommentImageInfo> GetProductCommentImagesByCommentIds(IEnumerable<long> commentIds)
        {
            return DbFactory.Default.Get<ProductCommentImageInfo>().Where(a => a.CommentId.ExIn(commentIds)).ToList();
        }
    }
}
