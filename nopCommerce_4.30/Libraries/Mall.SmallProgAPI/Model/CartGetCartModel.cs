using System;
using System.Collections.Generic;

namespace Mall.SmallProgAPI.Model
{
    public class CartGetCartModel
    {
        public CartGetCartModel()
        {
            Store = new List<CartStoreModel>();
        }
        public bool success { get; set; }
        public List<CartStoreModel> Store { get; set; }
    }
    public class CartStoreModel
    {
        public long ShopId { get; set; }
        public long ShopBranchId { get; set; }
        public string ShopBranchName { get; set; }
        /// <summary>
        /// 状态 0:正常;1:冻结
        /// </summary>
        public int Status { get; set; }
        public decimal Amount { get; set; }
        public int TotalCount { get; set; }
        public int DeliveFee { get; set; }
        public int DeliveTotalFee { get; set; }
        public int FreeMailFee { get; set; }
        public List<CartStoreProduct> Products { get; set; }
    }
    public class CartStoreProduct
    {
        public long Id { get; set; }
        public long ShopBranchId { get; set; }
        public long CartItemId { get; set; }
        public string SkuId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public int Stock { get; set; }
        /// <summary>
        /// 状态 0:正常;1:冻结;2:库存不足
        /// </summary>
        public int Status { get; set; }
        public int ProductStatus { get; set; }
        public string SkuDetails { get; set; }
        public DateTime AddTime { get; set; }
        public string DefaultImage { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Version { get; set; }
        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
    }
}
