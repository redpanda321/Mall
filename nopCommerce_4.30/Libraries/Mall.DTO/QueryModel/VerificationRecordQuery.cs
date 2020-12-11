using Mall.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mall.DTO.QueryModel
{
    public class VerificationRecordQuery : QueryBase
    {
        public long? ShopId { get; set; }

        public long? ShopBranchId { get; set; }

        public string OrderId { get; set; }

        public OrderInfo.VerificationCodeStatus? Status { get; set; }

        public DateTime? PayDateStart { get; set; }
        public DateTime? PayDateEnd { get; set; }

        public string VerificationCode { get; set; }

        public DateTime? VerificationTimeStart { get; set; }
        public DateTime? VerificationTimeEnd { get; set; }
        public bool IsShop { get; set; }
        public bool IsAll { get; set; }
        public sbyte? Type { get; set; }
        /// <summary>
        /// 门店或商家名称
        /// </summary>
        public string ShopBranchName { get; set; }
        /// <summary>
        /// 门店或商家ID
        /// </summary>
        public long SearchId { get; set; }
    }
}
