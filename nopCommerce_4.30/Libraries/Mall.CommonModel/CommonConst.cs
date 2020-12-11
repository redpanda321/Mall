using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 通用常量
    /// </summary>
    public class CommonConst
    {
        /// <summary>
        /// 地址分隔符
        /// </summary>
        public const string ADDRESS_PATH_SPLIT = ",";

        /// <summary>
        /// 门店签名KEY
        /// </summary>
        public const string SHOPBRANCH_ENCRYP_KEY = "KEWOPSC";

        #region 统计相关常量
        /// <summary>
        /// 平台访问记录cookie key
        /// </summary>
        public const string MALL_PLAT_VISIT_COUNT = "PlatVisitCount";
        public const int MALL_PLAT_VISIT_COUNT_MAX = 2;

        /// <summary>
        /// 店铺访问记录cookie key
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        public static string MALL_SHOP_VISIT_COUNT(string shop)
        {
            return string.Format("ShopVisitCount-{0}", shop);
        }
        public const int MALL_SHOP_VISIT_COUNT_MAX = 2;

        /// <summary>
        /// 门店访问记录cookie key
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        public static string MALL_SHOPBRANCH_VISIT_COUNT(string shop)
        {
            return string.Format("ShopBranchVisitCount-{0}", shop);
        }
        public const int MALL_SHOPBRANCH_VISIT_COUNT_MAX = 2;

        /// <summary>
        /// 商品访问用户数
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static string MALL_PRODUCT_VISITUSER_COUNT(string pid)
        {
            return string.Format("ProductVisitUserCount-{0}", pid);
        }
        public const int MALL_PRODUCT_VISITUSER_COUNT_MAX = 2;
        /// <summary>
        /// 商品访问数
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static string MALL_PRODUCT_VISIT_COUNT(string pid)
        {
            return string.Format("ProductVisitCount-{0}", pid);
        }
        public const int MALL_PRODUCT_VISIT_COUNT_MAX = 2;
        public const string MALL_PRODUCT_VISIT_COUNT_COOKIE = "ProductVisitCount";

        #endregion 统计相关常量

        /// <summary>
		/// 退款成功消息
		/// </summary>
		public const string MESSAGEQUEUE_REFUNDSUCCESSED="MessageQueue_RefundSuccessed";

		/// <summary>
		/// 付款成功消息
		/// </summary>
		public const string MESSAGEQUEUE_PAYSUCCESSED="MessageQueue_PaySuccessed";
        /// <summary>
        /// 站点名最大长度
        /// </summary>
        public const int SITENAME_LENGTH = 15;
        /// <summary>
        /// 会员名最大长度
        /// </summary>
        public const int MEMBERNAME_LENGTH = 12;
        /// <summary>
        /// 默认REGION id
        /// </summary>
        public const int DEFAULT_REGION_ID = 3653;
        /// <summary>
        /// QQ地图接口key
        /// </summary>
        public const string QQMapKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
        /// <summary>
        /// 快递公司面单背景图片路径
        /// </summary>
        public const string ExpressImagePath = "/Storage/Plat/Express/";

        /// <summary>
        /// 默认京东地址库版本
        /// </summary>
        public const string DefaultMALLJDVersion = "3.0.0";
    }
}
