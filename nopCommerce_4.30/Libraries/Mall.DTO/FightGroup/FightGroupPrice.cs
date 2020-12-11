using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class FightGroupPrice
    {
        public long ProductId { get; set; }

        public decimal ActivePrice { get; set; }

        public long ActiveId { get; set; }
    }
}
