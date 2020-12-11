using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core.Plugins.Message
{
    /// <summary>
    /// 虚拟订单
    /// </summary>
    public class MessageVirtualOrderInfo
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderId { set; get; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { set; get; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }
        /// <summary>
        /// 门店/商家地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 核销码集合
        /// </summary>
        public string VerificationCodes { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 到期时间
        /// </summary>
        public string DueTime { get; set; }
        /// <summary>
        /// 商城名称
        /// </summary>
        public string SiteName { set; get; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { set; get; }
        /// <summary>
        /// 核销码生效类型（1=立即生效，2=付款完成X小时后生效，3=次日生效）
        /// </summary>
        public sbyte EffectiveType { set; get; }
        /// <summary>
        /// 小时
        /// </summary>
        public int Hour { get; set; }

    }

    /// <summary>
    /// 虚拟订单核销
    /// </summary>
    public class MessageVirtualOrderVerificationInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { set; get; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 核销码集合
        /// </summary>
        public string VerificationCodes { get; set; }

        /// <summary>
        /// 核销时间
        /// </summary>
        public string VerificationTime { get; set; }
        /// <summary>
        /// 核销门店/商家名称
        /// </summary>
        public string ShopBranchName { get; set; }
        /// <summary>
        /// 商城名称
        /// </summary>
        public string SiteName { set; get; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { set; get; }
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; }
    }
}
