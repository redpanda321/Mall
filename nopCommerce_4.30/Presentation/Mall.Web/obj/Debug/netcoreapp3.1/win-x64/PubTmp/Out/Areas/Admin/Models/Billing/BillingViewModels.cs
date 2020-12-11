using System.Collections.Generic;

namespace Mall.Web.Areas.Admin.Models
{
    public class SettlementListModel
	{
		public long Id { get; set; }

		public long ShopId { get; set; }

		public string ShopName { get; set; }

		public decimal Amount { get; set; }

		public string CreateTime { get; set; }

		public string Cycle { get; set; }

		public string DetailId { get; set; }
	}

	public class ExportPendingSettlementListModel
	{ public List<DTO.StatisticsPendingSettlement> Data { get; set; }
	public Dictionary<long, string> ShopNames { get; set; }

	public DTO.SettlementCycle SettmentCycle { get; set; }
	}
}