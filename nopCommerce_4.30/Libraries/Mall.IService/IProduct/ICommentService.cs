using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface ICommentService : IService
    {
        /// <summary>
        /// 添加一个产品评论
        /// </summary>
        /// <param name="model"></param>
        /// <param name="IsStartTransaction">调用当前方法时是否开启了事物，如开启参数设为true(如两个事物嵌套以为未关闭会抛异常)</param>
        void AddComment(Entities.ProductCommentInfo model, bool IsStartTransaction = false);
		void AddComment(IEnumerable<Entities.ProductCommentInfo> models);


        /// <summary>
        /// 回复评论或者追加评论
        void ReplyComment(long id, long shopId, string replyContent = "", string appendContent = "");

        /// <summary>
        /// 追加评论
        /// </summary>
        void AppendComment(List<AppendCommentModel> model);

        /// <summary>
        /// 自动评论
        /// </summary>
        /// <param name="userid"></param>
        //void AutoComment(long? userid=null);

        /// <summary>
        /// 删除产品评论
        /// </summary>
        /// <param name="id">评论ID</param>
        void HiddenComment(long id);

        /// <summary>
        /// 查询评论列表
        /// </summary>
        /// <param name="query">查询条件实体</param>
        /// <returns></returns>
        QueryPageModel<Entities.ProductCommentInfo> GetComments(CommentQuery query);
        int GetCommentCount(CommentQuery query);


        /// <summary>
        /// 订单列表项判断有没有追加评论
        /// </summary>
        /// <param name="subOrderId"></param>
        /// <returns></returns>
        bool HasAppendComment(long subOrderId);

        /// <summary>
        /// 获取单个评论内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Entities.ProductCommentInfo GetComment(long id);
        /// <summary>
        /// 根据IDS取多个评论
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<Entities.ProductCommentInfo> GetCommentsByIds(IEnumerable<long> ids);
        List<ProductCommentInfo> GetCommentByProduct(long product, long shopbranchId = 0);
        List<ProductCommentInfo> GetCommentByProduct(List<long> products);


        /// <summary>
        /// 获取用户订单评价的列表
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        QueryPageModel<UserOrderCommentModel> GetOrderComment(OrderCommentQuery query);


        /// <summary>
        /// 获取用户订单中商品的评价列表
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<ProductEvaluation> GetProductEvaluationByOrderId(long orderId, long userId);

        /// <summary>
        /// 获取用户订单中商品的评价列表
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<ProductEvaluation> GetProductEvaluationByOrderIdNew(long orderId, long userId);

        void SetCommentEmpty( long id );

        /// <summary>
        /// 获取用户未评论的商品
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topN"></param>
        /// <returns></returns>
         List<Entities.OrderItemInfo> GetUnEvaluatProducts(long userId);
        decimal GetProductMark(long id);
        /// <summary>
        /// 获取商品评价列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<Entities.ProductCommentInfo> GetProductComments(ProductCommentQuery query);

        /// <summary>
        /// 获取商品评价数
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetCommentCount(ProductCommentQuery query);

        /// <summary>
        /// 获取商品评价数量聚合
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ProductCommentCountAggregateModel GetProductCommentStatistic(long? productId = null, long? shopId = null, long? shopBranchId = null, bool iso2o = false);

        /// <summary>
        /// 获取商品平均评分
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        decimal GetProductAverageMark(long product);
        /// <summary>
        /// 获得商品评论数
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        int GetProductCommentCount(long product);
        List<OrderCommentInfo> GetOrderCommentsByOrder(IEnumerable<long> orders);
        /// <summary>
        /// 获取商品评价好评数
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        int GetProductHighCommentCount(long? productId = null, long? shopId = null, long? shopBranchId = null);

        /// <summary>
        /// 获取评论图片
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        List<ProductCommentImageInfo> GetProductCommentImagesByCommentIds(IEnumerable<long> commentIds);
    }
}
