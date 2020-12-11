using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class ProductLadderPrice
    {
        public new long Id { get; set; }
        public long ProductId { get; set; }
        public int MinBath { get; set; }
        public int MaxBath { get; set; }
        public decimal Price { get; set; }
    }
}
