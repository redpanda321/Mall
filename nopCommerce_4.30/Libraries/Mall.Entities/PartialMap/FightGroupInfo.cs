using Mall.CommonModel;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class FightGroupInfo
    {
        /// <summary>
        /// 店铺名称
        /// </summary>
        [ResultColumn]
        public string ShopName { get; set; }
        /// <summary>
        /// 商品名称
        ///</summary>
        [ResultColumn]
        public string ProductName { get; set; }
        /// <summary>
        /// 商品图片目录
        /// </summary>
        [ResultColumn]
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 商品默认图片
        /// </summary>
        [ResultColumn]
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// 团长用户名
        /// </summary>
        [ResultColumn]
        public string HeadUserName { get; set; }
        /// <summary>
        /// 团长头像
        /// </summary>
        [ResultColumn]
        public string HeadUserIcon { get; set; }
        /// <summary>
        /// 团长头像显示
        /// <para>默认头像值补充</para>
        /// </summary>
        [ResultColumn]
        public string ShowHeadUserIcon
        {
            get
            {
                string defualticon = "";
                string result = HeadUserIcon;
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = defualticon;
                }
                return result;
            }
        }
        /// <summary>
        /// 数据状态 成团中  成功   失败
        ///</summary>
        [ResultColumn]
        public FightGroupBuildStatus BuildStatus
        {
            get
            {
                return (FightGroupBuildStatus)this.GroupStatus;
            }
        }
        /// <summary>
        /// 拼团订单集
        /// </summary>

        [ResultColumn]
        public List<FightGroupOrderInfo> GroupOrders { get; set; }
        /// <summary>
        /// 团组时限（秒）
        /// </summary>
        [ResultColumn]
        public int? Seconds { get; set; }
    }
}
