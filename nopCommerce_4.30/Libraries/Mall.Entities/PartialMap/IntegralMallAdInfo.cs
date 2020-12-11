using NPoco;
using System.ComponentModel;

namespace Mall.Entities
{
    public partial class IntegralMallAdInfo
    {
        /// <summary>
        /// 活动类型
        /// </summary>
        public enum AdActivityType
        {
            /// <summary>
            /// 刮刮卡
            /// </summary>
            [Description("刮刮卡")]
            ScratchCard = 1,

            /// <summary>
            /// 大转盘
            /// </summary>
            [Description("大转盘")]
            Roulette = 2

        }
        /// <summary>
        /// 广告显示状态
        /// </summary>
        public enum AdShowStatus
        {
            /// <summary>
            /// 显示
            /// </summary>
            [Description("显示")]
            Show = 1,
            /// <summary>
            /// 隐藏
            /// </summary>
            [Description("隐藏")]
            Hide = 2,
        }
        /// <summary>
        /// 广告显示平台
        /// </summary>
        public enum AdShowPlatform
        {
            /// <summary>
            /// 电脑
            /// </summary>
            PC = 1,
            /// <summary>
            /// APP
            /// </summary>
            APP = 2,
        }
        /// <summary>
        /// 活动类型
        /// </summary>
        [ResultColumn]
        public AdActivityType ShowActivityType
        {
            get
            {
                AdActivityType result = (AdActivityType)this.ActivityType;
                return result;
            }
            set
            {
                this.ActivityType = value.GetHashCode();
            }
        }
        /// <summary>
        /// 广告显示状态
        /// </summary>
        [ResultColumn]
        public AdShowStatus? ShowAdStatus
        {
            get
            {
                return (AdShowStatus)this.ShowStatus;
            }
            set
            {

                this.ShowStatus = (int)value;
             
            }
        }
        /// <summary>
        /// 广告显示平台
        /// </summary>
        [ResultColumn]
        public AdShowPlatform? ShowAdPlatform
        {
            get
            {
                return (AdShowPlatform?)this.ShowPlatform;
            }
            set
            {
                this.ShowPlatform = (int)value;
            }
        }

        /// <summary>
        /// 链接地址
        /// </summary>
        [ResultColumn]
        public string LinkUrl { get; set; }
    }
}
