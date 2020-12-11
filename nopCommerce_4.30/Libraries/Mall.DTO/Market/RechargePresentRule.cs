using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class RechargePresentRule
    {
        public long Id { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal PresentAmount { get; set; }
    }
}
