﻿using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Linq;

namespace Mall.IServices
{
    /// <summary>
    /// 退款/退货服务接口
    /// </summary>
    public interface IComplaintService : IService
    {
        /// <summary>
        /// 获取投诉列表
        /// </summary>
        /// <param name="complaintQuery"></param>
        /// <returns></returns>
        QueryPageModel<OrderComplaintInfo> GetOrderComplaints(ComplaintQuery complaintQuery);

        /// <summary>
        /// 获取投诉数量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetOrderComplaintCount(ComplaintQuery query);

        /// <summary>
        /// 平台处理投诉
        /// </summary>
        /// <param name="id"></param>
        void DealComplaint(long id);

        /// <summary>
        /// 平台处理投诉
        /// </summary>
        /// <param name="id"></param>
        void DealComplaint(long id, string reply);

        /// <summary>
        /// 商家处理投诉
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reply"></param>
        void SellerDealComplaint(long id, string reply);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        void AddComplaint(OrderComplaintInfo info);

        /// <summary>
        /// 用户申请仲裁
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        void UserApplyArbitration(long id, long userId);
        /// <summary>
        /// 用户处理投诉
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        void UserDealComplaint(long id, long userId);

    }
}
