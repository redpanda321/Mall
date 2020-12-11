using Mall.CommonModel;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class FightGroupActiveInfo
    {
        /// <summary>
        /// 店铺名
        /// <para>手动补充</para>
        /// </summary>
        [ResultColumn]
        public string ShopName { get; set; }
        /// <summary>
        /// 商品图片地址
        /// </summary>
        [ResultColumn]
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 拼团活动状态
        /// </summary>
        [ResultColumn]
        public FightGroupActiveStatus ActiveStatus
        {
            get
            {
                FightGroupActiveStatus result = FightGroupActiveStatus.Ending;
                if (EndTime < DateTime.Now)
                {
                    result = FightGroupActiveStatus.Ending;
                }
                else
                {
                    if (StartTime > DateTime.Now)
                    {
                        result = FightGroupActiveStatus.WillStart;
                    }
                    else
                    {
                        result = FightGroupActiveStatus.Ongoing;
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// 活动项
        /// <para>手动维护</para>
        /// </summary>
        [ResultColumn]
        public List<FightGroupActiveItemInfo> ActiveItems { get; set; }
        /// <summary>
        /// 火拼价
        /// </summary>
        [ResultColumn]
        public decimal MiniGroupPrice { get; set; }
        /// <summary>
        /// 最低售价
        /// </summary>
        [ResultColumn]
        public decimal MiniSalePrice { get; set; }
        /// <summary>
        /// 运费模板
        /// </summary>
        [ResultColumn]
        public long FreightTemplateId { get; set; }
        /// <summary>
        /// 商品广告语
        /// </summary>
        [ResultColumn]
        public string ProductShortDescription { get; set; }
        /// <summary>
        /// 商品评价数
        /// </summary>
        [ResultColumn]
        public int ProductCommentNumber { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        [ResultColumn]
        public string ProductCode { get; set; }
        /// <summary>
        /// 商品单位
        /// </summary>
        [ResultColumn]
        public string MeasureUnit { get; set; }
        /// <summary>
        /// 商品是否可购买
        /// </summary>
        [ResultColumn]
        public bool CanBuy { get; set; }
        /// <summary>
        /// 商品是否还有库存
        /// </summary>
        [ResultColumn]
        public bool HasStock { get; set; }

        /// <summary>
        /// 管理审核状态
        /// </summary>
        [ResultColumn]
        public FightGroupManageAuditStatus FightGroupManageAuditStatus
        {
            get
            {
                FightGroupManageAuditStatus result = FightGroupManageAuditStatus.Normal;
                if (ManageAuditStatus == -1)
                {
                    result = FightGroupManageAuditStatus.SoldOut;
                }
                return result;
            }
        }

        /// <summary>
        /// 主图视频
        /// </summary>
        public string VideoPath { get; set; }
    }
}
