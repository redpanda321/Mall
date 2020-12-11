using Mall.Entities;
using System.Collections.Generic;

namespace Mall.DTO
{
    public class CartItemModel
    {
        /// <summary>
        /// 库存ID
        /// </summary>
        public string skuId { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public string size { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public string color { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string version { get; set; }

        public string skuDetails { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string imgUrl { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string shopName { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long shopId { set; get; }
        public long vshopId { set; get; }
        /// <summary>
        /// 是否自营店
        /// </summary>
        public bool IsSelf { set; get; }
        public string productCode { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string unit { get; set; }
        public List<CouponRecordInfo> UserCoupons { set; get; }
        public bool isDis { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
        public long collpid { get; set; }
        public bool IsLimit { get; set; }
        /// <summary>
        /// 门店ID
        /// </summary>
        public long ShopBranchId { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopBranchName { get; set; }

        /// <summary>
        /// 是否开启阶梯价
        /// </summary>
        public bool IsOpenLadder { get; set; }
        public long FreightTemplateId { get; set; }

        /// <summary>
        /// 分摊的满减金额
        /// </summary>
        public decimal fullDiscount { get; set; }
        public sbyte ProductType { get; set; }
    }
}