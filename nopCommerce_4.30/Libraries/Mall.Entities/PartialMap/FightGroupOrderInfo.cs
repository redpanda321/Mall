using Mall.CommonModel;
using NPoco;
using System;
using System.Collections.Generic;

namespace Mall.Entities
{
    public partial class FightGroupOrderInfo
    {
        /// <summary>
        /// 真实姓名
        /// </summary>
        [ResultColumn]
        public string RealName { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        [ResultColumn]
        public string Photo { get; set; }
        /// <summary>
        /// 订单用户名
        /// </summary>
        [ResultColumn]
        public string UserName { get; set; }
        /// <summary>
        /// 组团状态
        /// </summary>
        [ResultColumn]
        public FightGroupBuildStatus GroupStatus { get; set; }
        /// <summary>
        /// 参团人数限制
        /// </summary>

        [ResultColumn]
        public int LimitedNumber { get; set; }
        /// <summary>
        /// 时间限制
        /// </summary>
        [ResultColumn]
        public decimal LimitedHour { get; set; }
        /// <summary>
        /// 已参团人数
        /// </summary>
        [ResultColumn]
        public int JoinedNumber { get; set; }
        /// <summary>
        /// 参团时间
        /// </summary>
        [ResultColumn]
        public DateTime AddGroupTime { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        [ResultColumn]
        public string ProductName { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        [ResultColumn]
        public string IconUrl { get; set; }

        /// <summary>
        /// 分享商品图片
        /// </summary>
        [ResultColumn]
        public string thumbs { get; set; }
        /// <summary>
        /// 火拼价格
        /// </summary>
        [ResultColumn]
        public decimal MiniGroupPrice { get; set; }
        /// <summary>
        /// 已团成功用户
        /// </summary>
        [ResultColumn]
        public List<UserInfo> UserInfo { get; set; }
        /// <summary>
        /// 团长姓名
        /// </summary>
        [ResultColumn]
        public string HeadUserName { get; set; }
        /// <summary>
        /// 团长头像
        /// </summary>
        [ResultColumn]
        public string HeadUserIcon { get; set; }
        /// <summary>
        /// 团长头像显示（默认头像值补充）
        /// </summary>
        [ResultColumn]
        public string ShowHeadUserIcon { get; set; }
        /// <summary>
        /// 团组时限（秒）
        /// </summary>
        [ResultColumn]
        public int? Seconds { get; set; }
        /// <summary>
        /// 参团状态
        /// </summary>
        [ResultColumn]
        public FightGroupOrderJoinStatus GetJoinStatus
        {
            get
            {
                return (FightGroupOrderJoinStatus)this.JoinStatus;
            }
        }
        /// <summary>
        /// 是否可以退款
        /// </summary>
        [ResultColumn]
        public bool CanRefund
        {
            get
            {
                bool result = false;
                switch (GetJoinStatus)
                {
                    case FightGroupOrderJoinStatus.BuildSuccess:
                        result = true;
                        break;
                    case FightGroupOrderJoinStatus.BuildFailed:
                        result = true;
                        break;
                    case FightGroupOrderJoinStatus.JoinFailed:
                        result = true;
                        break;
                }
                return result;
            }
        }
        /// <summary>
        /// 是否可以发货
        /// </summary>
        [ResultColumn]
        public bool CanSendGood
        {
            get
            {
                bool result = false;
                switch (GetJoinStatus)
                {
                    case FightGroupOrderJoinStatus.BuildSuccess:
                        result = true;
                        break;
                }
                return result;
            }
        }
        [ResultColumn]
        public bool IsCurrentDay { get; set; }
    }

    public class UserInfo
    {
        /// <summary>
        /// 用户头像
        /// </summary>
        [ResultColumn]
        public string Photo { get; set; }
        /// <summary>
        /// 订单用户名
        /// </summary>
        [ResultColumn]
        public string UserName { get; set; }
        /// <summary>
        /// 参团时间
        /// </summary>
        [ResultColumn]
        public DateTime? JoinTime { get; set; }

    }
}
