using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 满减数量聚合
    /// </summary>
    public class FullDiscountProductCountAggregate
    {
        public long ActiveId { get; set; }
        public long ProductCount { get; set; }
    }
}
