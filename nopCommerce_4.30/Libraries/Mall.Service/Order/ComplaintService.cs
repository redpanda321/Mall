using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Mall.Entities;
using NetRube.Data;

namespace Mall.Service
{
    public class ComplaintService : ServiceBase, IComplaintService
    {
        public QueryPageModel<OrderComplaintInfo> GetOrderComplaints(ComplaintQuery query)
        {
            var db = WhereBuilder(query);
            var data = db.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<OrderComplaintInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        public int GetOrderComplaintCount(ComplaintQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        private GetBuilder<OrderComplaintInfo> WhereBuilder(ComplaintQuery query)
        {
            var db = DbFactory.Default.Get<OrderComplaintInfo>();
            if (query.OrderId.HasValue)
                db.Where(item => query.OrderId == item.OrderId);
            if (query.StartDate.HasValue)
                db.Where(item => item.ComplaintDate >= query.StartDate.Value);
            if (query.EndDate.HasValue)
            {
                var endDay = query.EndDate.Value.Date.AddDays(1);
                db.Where(item => item.ComplaintDate < endDay);
            }
            if (query.Status.HasValue)
                db.Where(item => query.Status.Value == item.Status);
            if (query.ShopId.HasValue)
                db.Where(item => query.ShopId == item.ShopId);
            if (query.UserId.HasValue)
                db.Where(item => item.UserId == query.UserId);
            if (!string.IsNullOrWhiteSpace(query.ShopName))
                db.Where(item => item.ShopName.Contains(query.ShopName));
            if (!string.IsNullOrWhiteSpace(query.UserName))
                db.Where(item => item.UserName.Contains(query.UserName));
            return db;
        }

        public void DealComplaint(long id)
        {
            //OrderComplaintInfo orderComplaint = DbFactory.Default.Get<OrderComplaintInfo>().Where(p => p.Id == id).FirstOrDefault();
            //orderComplaint.Status = OrderComplaintInfo.ComplaintStatus.End;
            //DbFactory.Default.Update(orderComplaint);
            DbFactory.Default.Set<OrderComplaintInfo>().Set(n => n.Status, OrderComplaintInfo.ComplaintStatus.End).Where(n => n.Id == id).Succeed();
        }

        public void DealComplaint(long id, string reply)
        {
            DbFactory.Default.Set<OrderComplaintInfo>().Set(n => n.Status, OrderComplaintInfo.ComplaintStatus.End).Set(n => n.PlatRemark, reply).Where(n => n.Id == id).Succeed();
        }

        public void SellerDealComplaint(long id, string reply)
        {
            DbFactory.Default.Set<OrderComplaintInfo>()
                .Set(n => n.Status, OrderComplaintInfo.ComplaintStatus.Dealed)
                .Set(n => n.SellerReply, reply)
                .Where(n => n.Id == id)
                .Succeed();
        }

        public void UserDealComplaint(long id, long userId)
        {
            OrderComplaintInfo orderComplaint = DbFactory.Default.Get<OrderComplaintInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (orderComplaint != null && orderComplaint.UserId != userId)
            {
                throw new MallException("该投诉不属于此用户！");
            }
            orderComplaint.Status = OrderComplaintInfo.ComplaintStatus.End;
            DbFactory.Default.Update(orderComplaint);
        }

        public void UserApplyArbitration(long id, long userId)
        {
            OrderComplaintInfo orderComplaint = DbFactory.Default.Get<OrderComplaintInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (orderComplaint.UserId != userId)
            {
                throw new MallException("该投诉不属于此用户！");
            }
            orderComplaint.Status = OrderComplaintInfo.ComplaintStatus.Dispute;
            DbFactory.Default.Update(orderComplaint);
        }


        public List<OrderComplaintInfo> GetAllComplaint()
        {
            return DbFactory.Default.Get<OrderComplaintInfo>().ToList();
        }

        //添加一个用户投诉
        public void AddComplaint(OrderComplaintInfo model)
        {
            var exist = DbFactory.Default.Get<OrderComplaintInfo>().Where(a => a.OrderId == model.OrderId).Exist();
            if (exist)
            {
                throw new MallException("你已经投诉过了，请勿重复投诉！");
            }
            if (string.IsNullOrEmpty(model.SellerReply))
            {
                model.SellerReply = string.Empty;
            }
            DbFactory.Default.Add(model);
        }
    }
}
