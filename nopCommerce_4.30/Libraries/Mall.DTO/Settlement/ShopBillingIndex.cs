﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 商家财务总览
    /// </summary>
    public class ShopBillingIndex
    {

        /// <summary>
        /// 店铺帐户
        /// </summary>
        public ShopAccount ShopAccout { set; get; }

        /// <summary>
        /// 昨日销售额
        /// </summary>
        public decimal YesterDaySaleAmount { set; get; }

        /// <summary>
        /// 昨日下单总数
        /// </summary>
        public int YesterDayOrders { set; get; }

        /// <summary>
        /// 昨日付款订单数
        /// </summary>
        public int YesterDayPayOrders { set; get; }

    }
}
