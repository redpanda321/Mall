using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;

namespace Mall.Service
{
    public class ConsultationService : ServiceBase, IConsultationService
    {
        public void AddConsultation(ProductConsultationInfo model)
        {
            var product = DbFactory.Default.Get<ProductInfo>().Where(a => a.Id == model.ProductId && a.IsDeleted == false).FirstOrDefault();
            if (product != null)
            {
                model.ShopId = product.ShopId;
                model.ShopName = DbFactory.Default.Get<ShopInfo>().Where(a => a.Id == model.ShopId).Select(p => p.ShopName).FirstOrDefault<string>();
            }
            else
            {
                throw new Mall.Core.MallException("咨询的商品不存在，或者已删除");
            }
            DbFactory.Default.Add(model);
        }


        public void DeleteConsultation(long id)
        {
            DbFactory.Default.Del<ProductConsultationInfo>(n => n.Id == id);
        }

        public QueryPageModel<ProductConsultationInfo> GetConsultations(ConsultationQuery query)
        {
            var consultation = WhereBuilder(query);
            var data = consultation.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<ProductConsultationInfo>() { Models = data, Total = data.TotalRecordCount };
        }

        public int GetConsultationCount(ConsultationQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        private GetBuilder<ProductConsultationInfo> WhereBuilder(ConsultationQuery query) {

            var db = DbFactory.Default.Get<ProductConsultationInfo>();
            if (query.IsReply.HasValue)
            {
                if (query.IsReply.Value)
                    db.Where(item => item.ReplyDate.ExIsNotNull());
                else
                    db.Where(item => item.ReplyDate.ExIsNull());
            }

            if (!string.IsNullOrWhiteSpace(query.KeyWords))
                db.Where(item => item.ConsultationContent.Contains(query.KeyWords));

            if (query.ShopID > 0)
                db.Where(item => query.ShopID == item.ShopId);

            if (query.ProductID > 0)
                db.Where(item => query.ProductID == item.ProductId);

            if (query.UserID > 0)
                db.Where(item => query.UserID == item.UserId);
            return db;
        }

        public ProductConsultationInfo GetConsultation(long id)
        {
            return DbFactory.Default.Get<ProductConsultationInfo>().Where(p => p.Id == id).FirstOrDefault();
        }
        public void ReplyConsultation(long id, string replyContent, long shopId)
        {
            var flag = DbFactory.Default.Set<ProductConsultationInfo>().Set(n => n.ReplyContent, replyContent).Set(n => n.ReplyDate, DateTime.Now).Where(item => item.Id == id && item.ShopId == shopId).Succeed();
            if (!flag || shopId == 0) throw new Mall.Core.MallException("不存在该商品评论");
        }


        public List<ProductConsultationInfo> GetConsultations(long pid)
        {
            return DbFactory.Default.Get<ProductConsultationInfo>().Where(c => c.ProductId.Equals(pid)).ToList();
        }
    }
}
