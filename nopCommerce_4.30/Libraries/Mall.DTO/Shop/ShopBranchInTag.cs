using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class ShopBranchInTag
    {
        public long Id { get; set; }
        public long ShopBranchId { get; set; }
        public long TagId { get; set; }
        public string Title { get; set; }
    }
}
