using Mall.CommonModel;
using Mall.Core;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class ProductInfo
    {
        protected string ImageServerUrl = "";
        /// <summary>
        /// 销售状态
        /// </summary>
        public enum ProductSaleStatus
        {
            /// <summary>
            /// 原始状态
            /// <para>此状态不可入库，需要取出原数据的销售状态重新补充数据</para>
            /// </summary>
            [Description("原始状态")]
            RawState = 0,
            /// <summary>
            /// 出售中
            /// </summary>
            [Description("出售中")]
            OnSale = 1,

            /// <summary>
            /// 仓库中
            /// </summary>
            [Description("仓库中")]
            InStock = 2,
            /// <summary>
            /// 草稿箱
            /// </summary>
            [Description("草稿箱")]
            InDraft = 3
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        public enum ProductAuditStatus
        {
            /// <summary>
            /// 待审核
            /// </summary>
            [Description("待审核")]
            WaitForAuditing = 1,

            /// <summary>
            /// 销售中
            /// </summary>
            [Description("销售中")]
            Audited,

            /// <summary>
            /// 未通过(审核失败)
            /// </summary>
            [Description("未通过")]
            AuditFailed,

            /// <summary>
            /// 违规下架
            /// </summary>
            [Description("违规下架")]
            InfractionSaleOff,

            /// <summary>
            /// 未审核
            /// </summary>
            [Description("未审核")]
            UnAudit
        }

        /// <summary>
        /// 修改状态
        /// </summary>
        public enum ProductEditStatus
        {
            /// <summary>
            /// 正常
            /// <para>修改已生效</para>
            /// </summary>
            [Description("正常")]
            Normal = 0,

            /// <summary>
            /// 已修改
            /// </summary>
            [Description("已修改")]
            Edited = 1,

            /// <summary>
            /// 待审核
            /// </summary>
            [Description("待审核")]
            PendingAudit = 2,

            /// <summary>
            /// 已修改待审核
            /// <para>已修改+待审核</para>
            /// </summary>
            [Description("已修改待审核")]
            EditedAndPending = 3,
            /// <summary>
            /// 强制待审核
            /// <para>免审上架也需要审核</para>
            /// </summary>
            [Description("强制待审核")]
            CompelPendingAudit = 4,
            /// <summary>
            /// 强制待审已修改
            /// <para>免审上架也需要审核</para>
            /// </summary>
            [Description("强制待审已修改")]
            CompelPendingHasEdited = 5,

        }


        [ResultColumn]
        public long ConcernedCount { get; set; }

        /// <summary>
        /// 获取图片地址 YZY修改获取不同尺寸图片的方法
        /// </summary>
        /// <param name="imageIndex">图片序号</param>
        /// <param name="imageSize">图片尺寸</param>
        /// <returns></returns>
        public string GetImage(ImageSize imageSize, int imageIndex = 1)
        {
            return Core.MallIO.GetProductSizeImage(ImagePath, imageIndex, (int)imageSize);
        }


        /// <summary>
        /// 图片路径
        /// </summary>
        [ResultColumn]
        public string ImagePathUrl
        {
            //  get { return ImageServerUrl + imagePath; }

            get { return Core.MallIO.GetImagePath(ImagePath); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    ImagePath = value.Replace(ImageServerUrl, "");
                else
                    ImagePath = value;
            }
        }

        [ResultColumn]
        public string RelativePath
        {
            get { return ImagePath; }
        }

        /// <summary>
        /// 成交数
        /// </summary>
        [ResultColumn]
        public long OrderCounts { get; set; }

        /// <summary>
        /// 商品地址
        /// </summary>
        [ResultColumn]
        public string Address { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        [ResultColumn]
        public string ShopName { get; set; }

        [ResultColumn]
        public string BrandName
        {
            get;
            set;
        }

        #region 表单传参用
        [ResultColumn]
        public string CategoryNames { get; set; }

        [ResultColumn]
        public int IsCategory { get; set; }

        [ResultColumn]
        public long TopId { get; set; }

        [ResultColumn]
        public long BottomId { get; set; }
        #endregion

        [ResultColumn]
        public string ShowProductState
        {
            get
            {
                string result = "错误数据";
                if (this != null)
                {
                    if (this.AuditStatus == ProductInfo.ProductAuditStatus.WaitForAuditing)
                    {
                        result = (this.SaleStatus == ProductInfo.ProductSaleStatus.OnSale ? ProductInfo.ProductAuditStatus.WaitForAuditing.ToDescription() :
                ProductInfo.ProductSaleStatus.InStock.ToDescription());
                    }
                    else
                    {
                        result = this.AuditStatus.ToDescription();
                    }
                }
                return result;
            }
        }

        [ResultColumn]
        public bool IsFavorite
        {
            get;
            set;
        }
        [ResultColumn]
        public long ShopBranchSaleCounts
        {
            get; set;
        }

        /// <summary>
        /// 商品有效期类型(0=长期有效，1=自定义日期)
        /// </summary>
        public bool ValidityType { get; set; }
        /// <summary>
        /// 自定义开始日期
        /// </summary>
        public string StartDate { get; set; }
        /// <summary>
        /// 自定义结束日期
        /// </summary>
        public string EndDate { get; set; }
        /// <summary>
        /// 核销码生效类型
        /// </summary>
        public Entities.ProductInfo.VirtualProductEffectiveType EffectiveType { get; set; }
        /// <summary>
        /// 付款完成X小时有效
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// 虚拟商品退款类型
        /// </summary>
        public Entities.ProductInfo.SupportVirtualRefundType SupportRefundType { get; set; }

        /// <summary>
        /// 使用须知
        /// </summary>
        public string UseNotice { get; set; }
        public Entities.VirtualProductItemInfo[] VirtualProductItemInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 核销码生效类型
        /// </summary>
        public enum VirtualProductEffectiveType
        {
            /// <summary>
            /// 立即生效
            /// </summary>
            [Description("立即生效")]
            Immediately = 1,

            [Description("付款完成X小时后生效")]
            AfterPayment = 2,

            [Description("次日生效")]
            Morrow = 3
        }

        /// <summary>
        /// 虚拟商品退款类型
        /// </summary>
        public enum SupportVirtualRefundType
        {
            /// <summary>
            /// 支持有效期内退款
            /// </summary>
            [Description("支持有效期内退款")]
            SupportValidity = 1,

            /// <summary>
            /// 支持随时退款
            /// </summary>
            [Description("支持随时退款")]
            SupportAnyTime = 2,

            /// <summary>
            /// 不支持退款
            /// </summary>
            [Description("不支持退款")]
            NonSupport = 3
        }
        /// <summary>
        /// 虚拟商品信息项类型
        /// </summary>
        public enum VirtualProductItemType
        {
            /// <summary>
            /// 文本格式
            /// </summary>
            [Description("文本格式")]
            Text = 1,

            /// <summary>
            /// 日期
            /// </summary>
            [Description("日期")]
            Date = 2,

            /// <summary>
            /// 时间
            /// </summary>
            [Description("时间")]
            Time = 3,

            /// <summary>
            /// 身份证
            /// </summary>
            [Description("身份证")]
            IdCard = 4,

            /// <summary>
            /// 数字格式
            /// </summary>
            [Description("数字格式")]
            Digital = 5,

            /// <summary>
            /// 图片
            /// </summary>
            [Description("图片")]
            Picture = 6

        }

        List<ProductAttributeInfo> _ProductAttributeInfo = null;
        /// <summary>
        /// 关联属性
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<ProductAttributeInfo> ProductAttributeInfo { get; set; }

        List<ProductShopCategoryInfo> _ProductShopCategoryInfo = null;
        /// <summary>
        /// 
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<ProductShopCategoryInfo> ProductShopCategoryInfo { get; set; }
    }
}
