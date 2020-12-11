using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class CashDeposit
    {
        public long Id { get; set; }
        public string ShopName { get; set; }
        public string Type { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime Date { get; set; }
        public decimal NeedPay { get; set; }
        public bool EnableLabels { get; set; }
    }
}
