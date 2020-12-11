using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mall.Entities;

namespace Mall.Web.Areas.Mobile.Models
{
    public class DistributionMarketViewModel
    {
        public List<CategoryInfo> AllTopCategories { get; set; }
        /// <summary>
        /// 分享商品基础URI
        /// </summary>
        public string ShareProductUrlTMP { get; set; }
    }
}