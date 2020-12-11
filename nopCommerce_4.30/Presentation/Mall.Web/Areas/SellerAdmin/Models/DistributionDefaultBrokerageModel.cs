using Mall.DTO.Distribution;
using Mall.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class DistributionDefaultBrokerageModel
    {
        /// <summary>
        /// 一级分佣比
        ///</summary>
        [Range(0.1, 100, ErrorMessage = "最高分佣比值需在0.1%-100%之间")]
        public decimal BrokerageRate1 { get; set; }
        /// <summary>
        /// 二级分佣比
        ///</summary>
        public decimal BrokerageRate2 { get; set; }
        /// <summary>
        /// 三级分佣比
        ///</summary>
        public decimal BrokerageRate3 { get; set; }

        /// <summary>
        /// 分销最大级数
        /// <para>最大3级</para>
        /// </summary>
        public int DistributionMaxLevel { get; set; } = 1;
        /// <summary>
        /// 最高分佣比
        /// </summary>
        public decimal DistributionMaxBrokerageRate { get; set; }
        /// <summary>
        /// 是否配置正确
        /// </summary>
        public bool IsConfigRight { get; set; }
    }
}