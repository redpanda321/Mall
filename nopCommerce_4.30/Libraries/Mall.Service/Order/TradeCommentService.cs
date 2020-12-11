using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    public class TradeCommentService : ServiceBase, ITradeCommentService
    {
        public QueryPageModel<OrderCommentInfo> GetOrderComments(OrderCommentQuery query)
        {
            var orderComments = DbFactory.Default.Get<OrderCommentInfo>();

            #region 条件组合
            if (query.OrderId.HasValue)
            {
                orderComments.Where(item => query.OrderId == item.OrderId);
            }
            if (query.StartDate.HasValue)
            {
                orderComments.Where(item => item.CommentDate >= query.StartDate.Value);
            }
            if (query.EndDate.HasValue)
            {

                var end = query.EndDate.Value.Date.AddDays(1);
                orderComments.Where(item => item.CommentDate < end);
            }
            if (query.ShopId.HasValue)
            {
                orderComments.Where(item => query.ShopId == item.ShopId);
            }
            if (query.UserId.HasValue)
            {
                orderComments.Where(item => query.UserId == item.UserId);
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                orderComments.Where(item => item.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                orderComments.Where(item => item.UserName.Contains(query.UserName));
            }
            #endregion

            var rst = orderComments.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);

            QueryPageModel<OrderCommentInfo> pageModel = new QueryPageModel<OrderCommentInfo>() { Models = rst, Total = rst.TotalRecordCount };
            return pageModel;
        }

        public void DeleteOrderComment(long Id)
        {
            OrderCommentInfo ociobj = DbFactory.Default.Get<OrderCommentInfo>().Where(p => p.Id == Id).FirstOrDefault();
            if (ociobj != null)
            {
                //删除相关信息
                List<long> orditemid = DbFactory.Default
                    .Get<OrderItemInfo>()
                    .Where(d => d.OrderId == ociobj.OrderId)
                    .Select(d => d.Id)
                    .ToList<long>();
                DbFactory.Default.InTransaction(() =>
                {
                    DbFactory.Default.Del<ProductCommentInfo>(d => d.SubOrderId.ExIn(orditemid));
                    //删除订单评价
                    DbFactory.Default.Del(ociobj);
                });
            }
        }

        public void AddOrderComment(OrderCommentInfo info, int productNum)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == info.OrderId && a.UserId == info.UserId).FirstOrDefault();
            if (order == null)
            {
                throw new MallException("该订单不存在，或者不属于该用户！");
            }
            var orderComment = DbFactory.Default.Get<OrderCommentInfo>().Where(a => a.OrderId == info.OrderId && a.UserId == info.UserId);
            if (orderComment.Count() > 0)
                throw new MallException("您已经评论过该订单！");
            info.ShopId = order.ShopId;
            info.ShopName = order.ShopName;
            info.UserName = order.UserName;
            info.CommentDate = DateTime.Now;
            info.OrderId = order.Id;
            DbFactory.Default.Add(info);

            Mall.Entities.MemberIntegralRecordInfo record = new Mall.Entities.MemberIntegralRecordInfo();
            record.UserName = info.UserName;
            record.ReMark = "订单号:" + info.OrderId;
            record.MemberId = info.UserId;
            record.RecordDate = DateTime.Now;
            record.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Comment;
            Mall.Entities.MemberIntegralRecordActionInfo action = new Mall.Entities.MemberIntegralRecordActionInfo();
            action.VirtualItemTypeId = Mall.Entities.MemberIntegralInfo.VirtualItemType.Comment;
            action.VirtualItemId = info.OrderId;
            record.MemberIntegralRecordActionInfo.Add(action);
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Comment);
            if (memberIntegral != null)
            {
                record.Integral = productNum * memberIntegral.ConversionIntegral();
            }
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, null);
        }

        public OrderCommentInfo GetOrderCommentInfo(long orderId, long userId)
        {
            return DbFactory.Default.Get<OrderCommentInfo>().Where(a => a.UserId == userId && a.OrderId == orderId).FirstOrDefault();
        }
        public List<OrderCommentInfo> GetOrderCommentsByOrder(long orderId)
        {
            return DbFactory.Default.Get<OrderCommentInfo>().Where(a => a.OrderId == orderId).ToList();
        }



      
    }
}
