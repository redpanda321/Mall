using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class StatisticOrderComment
    {
       public long ShopId { get; set; }
        /// <summary>
        /// 宝贝与描述相符 商家得分
        /// </summary>
       public decimal ProductAndDescription { get; set; }

            /// <summary>
            /// 宝贝与描述相符 同行业平均分
            /// </summary>
        public decimal ProductAndDescriptionPeer { get; set; }

        /// <summary>
        /// 宝贝与描述相符 同行业商家最高得分
        /// </summary>
        public decimal ProductAndDescriptionMax { get; set; }

        /// <summary>
        /// 宝贝与描述相符 同行业商家最低得分
        /// </summary>
        public decimal ProductAndDescriptionMin { get; set; }
        /// <summary>
        /// 卖家发货速度 商家得分
        /// </summary>
        public decimal SellerDeliverySpeed { get; set; }

        /// <summary>
        /// 卖家发货速度 同行业平均分
        /// </summary>
        public decimal SellerDeliverySpeedPeer { get; set; }

        /// <summary>
        /// 卖家发货速度 同行业商家最高得分
        /// </summary>
        public decimal SellerDeliverySpeedMax { get; set; }

        /// <summary>
        /// 卖家发货速度 同行业商家最低得分
        /// </summary>
        public decimal SellerDeliverySpeedMin { get; set; }

        /// <summary>
        /// 卖家服务态度 商家得分
        /// </summary>
        public decimal SellerServiceAttitude { get; set; }

        /// <summary>
        /// 卖家服务态度 同行业平均分
        /// </summary>
        public decimal SellerServiceAttitudePeer { get; set; }

        /// <summary>
        /// 卖家服务态度 同行业商家最高得分
        /// </summary>
        public decimal SellerServiceAttitudeMax { get; set; }

        /// <summary>
        /// 卖家服务态度 同行业商家最低得分
        /// </summary>
        public decimal SellerServiceAttitudeMin { get; set; }
    }
}
