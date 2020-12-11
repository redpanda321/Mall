using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mall.Entities.ProductInfo;

namespace Mall.DTO.QueryModel
{
    public class FightGroupActiveQuery : QueryBase
    {
        public string ProductName { get; set; }
        public FightGroupActiveStatus? ActiveStatus { get; set; }
        public List<FightGroupActiveStatus> ActiveStatusList { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ShopName { get; set; }
        public long? ShopId { get; set; }

        /// <summary>
        /// 产品销售状态
        /// </summary>
        public ProductSaleStatus? SaleStatus { get; set; }
    }
}
