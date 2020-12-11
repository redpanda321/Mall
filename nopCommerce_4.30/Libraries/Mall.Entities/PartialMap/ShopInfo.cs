using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace Mall.Entities
{
    public partial class ShopInfo
    {
        /// <summary>
        /// 显示店铺状态
        /// </summary>
        [ResultColumn]
        public ShopAuditStatus ShowShopAuditStatus
        {
            get
            {
                ShopAuditStatus result = ShopAuditStatus.Unusable;
                if (this != null)
                {
                    result = this.ShopStatus;
                    if (this.EndDate != null && this.ShopStatus == ShopInfo.ShopAuditStatus.Open)
                    {
                        DateTime endd = this.EndDate.Date.AddDays(1).AddSeconds(-1);
                        if ((endd - DateTime.Now).TotalSeconds < 0)
                        {
                            result = ShopInfo.ShopAuditStatus.HasExpired;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 店铺状态
        /// </summary>
        public enum ShopAuditStatus
        {
            /// <summary>
            /// 默认
            /// </summary>
            [Description("默认")]
            Default = 0,
            /// <summary>
            /// 不可用
            /// </summary>
            [Description("不可用")]
            Unusable = 1,

            /// <summary>
            /// 待审核
            /// </summary>
            [Description("待审核")]
            WaitAudit = 2,

            /// <summary>
            /// 待付款
            /// </summary>
            [Description("待付款")]
            WaitPay = 3,

            /// <summary>
            /// 被拒绝
            /// </summary>
            [Description("被拒绝")]
            Refuse = 4,

            /// <summary>
            /// 待确认
            /// </summary>
            [Description("待确认")]
            WaitConfirm = 5,

            /// <summary>
            /// 冻结
            /// </summary>
            [Description("冻结")]
            Freeze = 6,

            /// <summary>
            /// 开启
            /// </summary>
            [Description("开启")]
            Open = 7,
            /// <summary>
            /// 已过期
            /// </summary>
            [Description("已过期")]
            HasExpired = -1
        }

        [ResultColumn]
        public string ShopAccount { get; set; }

        [ResultColumn]
        public Dictionary<long, decimal> BusinessCategory { get; set; }

     

        [ResultColumn]
        public int Sales
        {
            get;
            set;
        }

        [ResultColumn]
        public string ProductAndDescription
        {
            get;
            set;
        }

        [ResultColumn]
        public string SellerServiceAttitude
        {
            get;
            set;
        }

        [ResultColumn]
        public string SellerDeliverySpeed
        {
            get;
            set;
        }

        /// <summary>
        /// 店铺进度
        /// </summary>
        public enum ShopStage
        {
            /// <summary>
            /// 许可协议
            /// </summary>
            [Description("许可协议")]
            Agreement,

            /// <summary>
            /// 公司信息
            /// </summary>
            [Description("公司信息")]
            CompanyInfo,

            /// <summary>
            /// 财务信息
            /// </summary>
            [Description("财务信息")]
            FinancialInfo,

            /// <summary>
            /// 店铺信息
            /// </summary>
            [Description("店铺信息")]
            ShopInfo,

            /// <summary>
            /// 上传支付凭证
            /// </summary>
            [Description("上传支付凭证")]
            UploadPayOrder,

            /// <summary>
            /// 完成
            /// </summary>
            [Description("完成")]
            Finish
        }

        public class ShopVistis
        {
            public decimal VistiCounts { get; set; }

            public decimal SaleCounts { get; set; }

            public decimal SaleAmounts { get; set; }

            public decimal OrderCounts { get; set; }

            public decimal OrderPayCount { get; set; }
        }

        /// <summary>
        /// 商家商品分类与产品Id实体
        /// </summary>
        public class ShopCategoryAndProductIdModel
        {
            /// <summary>
            /// 商品ID
            /// </summary>
            public long ProductId { get; set; }

            /// <summary>
            /// 商家分类ID
            /// </summary>
            public long ShopCategoryId { get; set; }

            /// <summary>
            /// 商家分类名称
            /// </summary>
            public string CategoryName { get; set; }
        }
    }
}
