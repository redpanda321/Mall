using System.Collections.Generic;

namespace Mall.Web.Areas.Web.Models
{
    public class AmountModel
    {
        public decimal totalProductAmount { get; set; }

        public decimal totalFreightAmount { get; set; }

        public decimal totalAmount { get; set; }

        public List<decimal> freightAmounts { get; set; }
    }
}