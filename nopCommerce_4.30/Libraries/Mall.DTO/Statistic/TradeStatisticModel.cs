
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 交易统计
    /// </summary>
    public class TradeStatisticModel
    {
        public System.DateTime Date { get; set; }
        public long VisitCounts { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        public long OrderUserCount { get; set; }
        public long OrderCount { get; set; }
        public long OrderProductCount { get; set; }
        public decimal OrderAmount { get; set; }
        public long OrderPayUserCount { get; set; }
        public long OrderPayCount { get; set; }
        /// <summary>
        /// 今日销售额（付款金额，不包括积分）
        /// </summary>
        public decimal TodaySaleAmount { get; set; }
        /// <summary>
        /// 退款订单数
        /// </summary>
        public long OrderRefundCount { get; set; }

        /// <summary>
        /// 退款件数
        /// </summary>
        public long OrderRefundProductCount { get; set; }

        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal OrderRefundAmount { get; set; }

        /// <summary>
        /// 件单价
        /// </summary>
        public decimal UnitPrice {
            get {
                if (SaleCounts > 0)
                {
                    return Math.Round(Convert.ToDecimal(this.SaleAmounts / this.SaleCounts), 2);
                }
                return 0;
            }
        }

        /// <summary>
        /// 连带率
        /// </summary>
        public decimal JointRate {
            get {
                if (OrderPayCount > 0)
                {
                    return Math.Round((Convert.ToDecimal(this.SaleCounts) / this.OrderPayCount) * 100, 2);
                }
                return 0;
            }
        }

        public bool StatisticFlag { get; set; }

        /// <summary>
        /// 退款率
        /// </summary>
        public decimal OrderRefundRate {
            get {
                if(OrderPayCount>0)
                {
                    return Math.Round((Convert.ToDecimal(this.OrderRefundCount) / this.OrderPayCount) * 100, 2);
                }
                return 0;
            }
        }

        /// <summary>
        /// 订单转化率
        /// </summary>
        public decimal OrderConversionsRates
        {
            get {
                if (VisitCounts > 0)
                {                    
                    return Math.Round((Convert.ToDecimal(this.OrderUserCount) / this.VisitCounts) * 100, 2);
                }
                else
                {
                    if (OrderUserCount > 0)
                    {//有订单，没浏览人数，默认为100%
                        return Math.Round((Convert.ToDecimal(this.OrderUserCount) / this.OrderUserCount) * 100, 2);
                    }
                    return 0;
                }
            }
        }
        /// <summary>
        /// 付款转化率
        /// </summary>
        public decimal PayConversionsRates
        {
            get
            {
                if (this.OrderUserCount > 0)
                {
                    if (this.OrderPayUserCount > this.OrderUserCount)
                        return 100;
                    return Math.Round((Convert.ToDecimal(this.OrderPayUserCount) / this.OrderUserCount) * 100, 2);
                }                    
                else
                {
                    if (this.OrderPayUserCount > 0)
                    {//有付款，没浏览人数，默认为100%
                        return 100;
                    }
                    return 0;
                }
            }
        }
        /// <summary>
        /// 成交转化率
        /// </summary>
        public decimal TransactionConversionRate
        {
            get
            {
                if (this.VisitCounts > 0)
                    return Math.Round((Convert.ToDecimal(this.OrderPayUserCount) / this.VisitCounts) * 100, 2);
                else
                {
                    if (this.OrderPayUserCount > 0)
                    {//有付款，没浏览人数，默认为100%
                        return Math.Round((Convert.ToDecimal(this.OrderPayUserCount) / this.OrderPayUserCount) * 100, 2);
                    }
                    return 0;
                }
            }
        }

        /// <summary>
        /// 导出时添加百分号
        /// </summary>
        public string OrderConversionsRatesString
        {
            get
            {
                return OrderConversionsRates + "%";
            }
        }
        /// <summary>
        /// 导出时添加百分号
        /// </summary>
        public string PayConversionsRatesString
        {
            get
            {
                return PayConversionsRates + "%";
            }
        }
        /// <summary>
        /// 导出时添加百分号
        /// </summary>
        public string TransactionConversionRateString
        {
            get
            {
                return TransactionConversionRate + "%";
            }
        }

        /// <summary>
        /// 付款金额线
        /// </summary>
        public LineChartDataModel<decimal> ChartModelPayAmounts { get; set; }
        /// <summary>
        /// 付款人数线
        /// </summary>
        public LineChartDataModel<long> ChartModelPayUsers { get; set; }
        /// <summary>
        /// 付款件数线
        /// </summary>
        public LineChartDataModel<long> ChartModelPayPieces { get; set; }
        /// <summary>
        /// 下单转化率线
        /// </summary>
        public LineChartDataModel<decimal> ChartModelOrderConversionsRates { get; set; }
        /// <summary>
        /// 付款转化率线
        /// </summary>
        public LineChartDataModel<decimal> ChartModelPayConversionsRates { get; set; }
        /// <summary>
        /// 成交转化率线
        /// </summary>
        public LineChartDataModel<decimal> ChartModelTransactionConversionRate { get; set; }

    }
}

