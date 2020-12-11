using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mall.Entities;
using Mall.DTO;

namespace Mall.Web.Areas.Mobile.Models
{
    public class DistributionMyShopViewModel : DistributorInfo
    {
        /// <summary>
        /// 离一下级差多少
        /// </summary>
        public decimal UpgradeNeedAmount { get; set; }
        /// <summary>
        /// 下一级
        /// </summary>
        public string NextGradeName { get; set; }
        /// <summary>
        /// 未结算佣金
        /// </summary>
        public decimal NoSettlementAmount { get; set; }
        public SiteSettings CurrentSiteSettings { get; internal set; }
    }
}