using Mall.Entities;
using System;

namespace Mall.DTO
{
    public class OrderRefundExportModel
    {
        /// <summary>
        /// 售后编号
        /// </summary>
        public long RefundId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// 商铺名称
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public long ReturnQuantity { get; set; }

        /// <summary>
        /// 门店/商家
        /// </summary>
        public string ShopBranchName { get; set; }

        /// <summary>
        /// 买家
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public string ContactPerson { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string ContactCellPhone { get; set; }

        /// <summary>
        /// 申请日期
        /// </summary>
        public string ApplyDate { get; set; }

        /// <summary>
        /// 商家处理
        /// </summary>
        public string SellerRemark { get; set; }

        /// <summary>
        /// 退款金额
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// 退款状态
        /// </summary>
        public string RefundStatus { get; set; }

        /// <summary>
        /// 退款方式
        /// </summary>
        public string RefundPayType { get; set; }

        /// <summary>
        /// 买家退款理由
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 退款凭证1
        /// </summary>
        public string CertPic1 { get; set; }

        /// <summary>
        /// 退款凭证2
        /// </summary>
        public string CertPic2 { get; set; }

        /// <summary>
        /// 退款凭证3
        /// </summary>
        public string CertPic3 { get; set; }

        /// <summary>
        /// 退款详细
        /// </summary>
        public string ReasonDetail { get; set; }

        /// <summary>
        /// 平台注释
        /// </summary>
        public string ManagerRemark { get; set; }

        /// <summary>
        /// 退款方式
        /// </summary>
        public int RefundMode { get; set; }

    }
}