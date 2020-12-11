using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public class WeiActivityQuery : QueryBase
    {
        public string Name { get; set; }
        public WeiActivityType Type { get; set; }
        public bool? IsIntegralActivity { get; set; }
        public bool IsShowAll { get; set; } = true;
    }
}
