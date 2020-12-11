using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
	/// <summary>
	/// 包括额外相关信息的订单
	/// </summary>
	public class FullOrder:Order
	{
		public List<OrderItem> OrderItems { get; set; }

        public List<OrderRefundInfo> Refunds { get; set; }

		/// <summary>
		/// 订单包含的所有商品总件数
		/// </summary>
		public long ProductCount
		{
			get
			{
				if (OrderItems == null || OrderItems.Count == 0)
					return 0;
				return OrderItems.Sum(p => p.Quantity);
			}
		}
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopBranchName { get; set; }


        /// <summary>
        /// 订单发票记录
        /// </summary>
        public OrderInvoiceInfo OrderInvoice { get; set; }

        /// <summary>
        /// 平台佣金
        /// </summary>
        public decimal PlatCommission { get; set; }

        /// <summary>
        /// 分销员佣金
        /// </summary>
        public decimal DistributorCommission { get; set; }
    }
}
