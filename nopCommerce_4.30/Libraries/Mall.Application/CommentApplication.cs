using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{
    public class CommentApplication:BaseApplicaion<ICommentService>
    {
        /// <summary>
        /// 获取用户订单中商品的评价列表
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<ProductEvaluation> GetProductEvaluationByOrderId(long orderId, long userId)
        {
            return Service.GetProductEvaluationByOrderId(orderId, userId);
        }

        public static List<ProductEvaluation> GetProductEvaluationByOrderIdNew(long orderId, long userId)
        {
            return Service.GetProductEvaluationByOrderIdNew(orderId, userId);
        }
        /// <summary>
        /// 根据评论ID取评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Entities.ProductCommentInfo GetComment(long id)
        {
            return Service.GetComment(id);
        }
        public static int GetCommentCountByProduct(long product)
        {
            //TODO:FG 此方法调用需要优化
            var query = new CommentQuery
            {
                ProductID = product,
                IsHidden = false,
            };
            return Service.GetCommentCount(query);
        }

        public static List<ProductCommentInfo> GetCommentsByProduct(long product, long shopbranchId = 0)
        {
            return Service.GetCommentByProduct(product, shopbranchId).Where(p => !p.IsHidden).ToList();
        }

        public static List<ProductCommentInfo> GetCommentsByProduct(IEnumerable<long> products)
        {
            return Service.GetCommentByProduct(products.ToList()).Where(p => !p.IsHidden).ToList();
        }


        public static List<ProductCommentInfo> GetCommentss(IEnumerable<long> ids)
        {
            return Service.GetCommentsByIds(ids).ToList();
        }
        public static void Add(ProductComment comment)
        {
            var info = comment.Map<ProductCommentInfo>();
            Service.AddComment(info);
            comment.Id = info.Id;
        }
        public static void AddComment(ProductCommentInfo model)
        {
            Service.AddComment(model);
        }

        public static void Add(IEnumerable<ProductComment> comments)
        {
            var list = comments.ToList().Map<List<ProductCommentInfo>>();
            Service.AddComment(list);
        }

        /// <summary>
        /// 追加评论
        /// </summary>
        public static void Append(List<AppendCommentModel> models)
        {
            Service.AppendComment(models);
        }

        /// <summary>
        /// 获取商品评价列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductComment> GetProductComments(ProductCommentQuery query)
        {
            var datalist = Service.GetProductComments(query);
            foreach(var item in datalist.Models)
            {
                item.ProductCommentImageInfo = GetProductCommentImagesByCommentIds(new List<long> { item.Id });
            }
            return new QueryPageModel<ProductComment>
            {
                Total = datalist.Total,
                //   Models = AutoMapper.Mapper.Map<List<ProductCommentInfo>, List<ProductComment>>(datalist.Models)
                Models = datalist.Models.Map<List<ProductComment>>()
            
            };
        }

        public static QueryPageModel<ProductCommentInfo> GetComments(ProductCommentQuery query)
        {
            return Service.GetProductComments(query);
        }

        public static List<ProductCommentImageInfo> GetProductCommentImagesByCommentIds(IEnumerable<long> commentIds)
        {
            return Service.GetProductCommentImagesByCommentIds(commentIds);
        }

        public static int GetCommentCount(ProductCommentQuery query) {
            return Service.GetCommentCount(query);
        }
        /// <summary>
        /// 获取商品评价数量聚合
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static ProductCommentCountAggregateModel GetProductCommentStatistic(long? productId = null, long? shopId = null, long? shopBranchId = null,bool iso2o = false)
        {
            return Service.GetProductCommentStatistic(productId, shopId, shopBranchId, iso2o: iso2o);
        }
        /// <summary>
        /// 获取商品平均评分
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public static decimal GetProductAverageMark(long product)
        {
            return Service.GetProductAverageMark(product);
        }

        public static int GetProductCommentCount(long product) {
            return Service.GetProductCommentCount(product);
        }
        /// <summary>
        /// 获取商品评价好评数
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static int GetProductHighCommentCount(long? productId = null, long? shopId = null, long? shopBranchId = null)
        {
            return Service.GetProductHighCommentCount(productId, shopId, shopBranchId);
        }
        /// <summary>
        /// 订单列表项判断有没有追加评论
        /// </summary>
        /// <param name="subOrderId"></param>
        /// <returns></returns>
        public static bool HasAppendComment(long subOrderId)
        {
            return Service.HasAppendComment(subOrderId);
        }

        public static List<OrderCommentInfo> GetOrderCommentByOrder(IEnumerable<long> orders) {
           return  GetService<ICommentService>().GetOrderCommentsByOrder(orders);
        }
    }
}
