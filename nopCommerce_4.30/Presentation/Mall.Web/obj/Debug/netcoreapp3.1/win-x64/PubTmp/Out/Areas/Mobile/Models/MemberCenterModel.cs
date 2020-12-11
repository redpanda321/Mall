namespace Mall.Web.Areas.Mobile.Models
{
    public class MemberCenterModel
    {
        public int WaitingForComments { get; set; }

        public Entities.MemberInfo Member { get; set; }

        public int AllOrders { get; set; }

        public int WaitingForRecieve { get; set; }

        public int WaitingForPay { get; set; }
        /// <summary>
        /// 待发货
        /// </summary>
        public int WaitingForDelivery { get; set; }

        public int RefundOrders { get; set; }

        public decimal Capital { get; set; }

        public string GradeName { get; set; }

        public int CouponsCount { get; set; }

        public int CollectionShop { get; set; }
        /// <summary>
        /// 是否已开启签到功能
        /// </summary>
        public bool SignInIsEnable { get; set; }
        /// <summary>
        /// 是否可以签到
        /// </summary>
        public bool CanSignIn { get; set; }
        
        /// <summary>
        /// 是否开启拼团
        /// </summary>
        public bool CanFightGroup { get; set; }
        /// <summary>
        /// 组团数
        /// </summary>
        public int BulidFightGroupNumber { get; set; }
        /// <summary>
        /// 用户可用积分
        /// </summary>
        public int MemberAvailableIntegrals { get; set; }
        public Mall.Entities.MemberInfo userMemberInfo { get; set; }
        public bool IsOpenRechargePresent { get; set; }
        /// <summary>
        /// 收藏商品数
        /// </summary>
        public int FavoriteProductCount { get; set; }
        /// <summary>
        /// 是否显示我要开店
        /// </summary>
        public bool IsShowDistributionOpenMyShop { set; get; }
        /// <summary>
        /// 是否显示我的小店
        /// </summary>
        public bool IsShowDistributionMyShop { get; set; }
        public string DistributionMyShopShow { get; internal set; }
        public string DistributionOpenMyShopShow { get; internal set; }
    }
}