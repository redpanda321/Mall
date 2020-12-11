using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;

namespace Mall.SmallProgAPI.Model
{
    public class HomeGetShopBranchInfoModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 所属商家店铺ID
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopBranchName { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string ShopBranchInTagNames { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string ShopBranchTagId { get; set; }
        /// <summary>
        /// 门店所在地址ID
        /// </summary>
        public int AddressId { get; set; }
        /// <summary>
        /// 门店所在详细地址
        /// </summary>
        public string AddressDetail { get; set; }
        /// <summary>
        /// 门店地址中文
        /// </summary>
        public string AddressFullName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string ContactUser { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactPhone { get; set; }
        /// <summary>
        /// 门店状态
        /// </summary>
        public Mall.CommonModel.ShopBranchStatus Status { get; set; }
        /// <summary>
        /// 门店状态
        /// </summary>
        public string ShowStatus
        {
            get
            {
                return Status.ToDescription();
            }
        }
        /// <summary>
        /// 地址ID PATH
        /// </summary>
        public string RegionIdPath { get; set; }

        /// <summary>
        /// 门店服务半径/配送半径
        /// </summary>
        public int ServeRadius { get; set; }

        /// <summary>
        /// 门店经度
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// 门店维度
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// 门店Banner
        /// </summary>
        public string ShopImages { get; set; }
        /// <summary>
        /// 经纬度距离
        /// </summary>
        public double Distance { get; set; }
        /// <summary>
        /// 经纬度距离
        /// </summary>
        public string DistanceUnit { get; set; }
        /// <summary>
        /// 是否有商品且有库存
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// 门店地址全路径
        /// </summary>
        public string AddressPath { get; set; }

        /// <summary>
        /// 是否门店配送
        /// </summary>
        public bool IsStoreDelive { get; set; }
        /// <summary>
        /// 是否自提
        /// </summary>
        public bool IsAboveSelf { get; set; }
        /// <summary>
        /// 配送费
        /// </summary>
        public decimal DeliveFee { get; set; }
        /// <summary>
        /// 包邮金额
        /// </summary>
        public decimal FreeMailFee { get; set; }
        /// <summary>
        /// 起送费
        /// </summary>
        public decimal DeliveTotalFee { get; set; }
        /// <summary>
        /// 营业开始时间
        /// </summary>
        public TimeSpan StoreOpenStartTime { get; set; }
        /// <summary>
        /// 营业结束时间
        /// </summary>
        public TimeSpan StoreOpenEndTime { get; set; }

        /// <summary>
        /// 是否包邮
        /// </summary>
        public bool IsFreeMail { get; set; }
    }
}
