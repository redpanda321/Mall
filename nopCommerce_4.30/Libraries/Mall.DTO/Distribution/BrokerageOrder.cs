using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mall.Entities.OrderInfo;
using Mall.Core;
namespace Mall.DTO
{
    public class BrokerageOrder
    {
        public long OrderId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public OrderOperateStatus OrderStatus { get; set; }
        public string OrderStatusText { get { return OrderStatus.ToDescription(); } }
        public DateTime? SettlementTime { get; set; }
        public DistributionBrokerageStatus Status { get; set; }
        public string StatusText { get { return Status.ToDescription(); } }
        public long SuperiorId1 { get; set; }
        public string SuperiorName1 { get; set; }
        public decimal Brokerage1 { get; set; }
        public long SuperiorId2 { get; set; }
        public string SuperiorName2 { get; set; }
        public decimal Brokerage2 { get; set; }
        public long SuperiorId3 { get; set; }
        public string SuperiorName3 { get; set; }
        public decimal Brokerage3 { get; set; }
        public decimal TotalBrokerage
        {
            get
            {
                decimal result;
                result = decimal.Parse(Brokerage1.ToString("f2"));
                result += decimal.Parse(Brokerage2.ToString("f2"));
                result += decimal.Parse(Brokerage3.ToString("f2"));
                return result;
            }
        }



    }
}
