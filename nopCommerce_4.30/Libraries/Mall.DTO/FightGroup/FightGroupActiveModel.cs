using Mall.CommonModel;
using Mall.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mall.DTO
{
    /// <summary>
    /// 拼团活动
    /// </summary>
    public class FightGroupActiveModel
    {
        public FightGroupActiveModel()
        {
            ProductImages = new List<string>();
        }
        /// <summary>
        /// 是否已初始商品图片信息
        /// </summary>
        public bool isInitImagesed { get; set; }

        #region 属性
        /// <summary>
        /// 编号
        ///</summary>
        public long Id { get; set; }
        /// <summary>
        /// 店铺编号
        ///</summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 商品编号
        ///</summary>
        [Required(ErrorMessage = "请选择商品")]
        [Display(Name = "商品编号")]
        public long ProductId { get; set; }
        /// <summary>
        /// 商品名称
        ///</summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 图片
        ///</summary>
        [Required(ErrorMessage = "请上传列表图片")]
        [Display(Name = "列表图片")]
        public string IconUrl { get; set; }
        /// <summary>
        /// 开始时间
        ///</summary>
        [Required(ErrorMessage = "请填写活动开始时间")]
        [DataType(DataType.Date, ErrorMessage = "错误的开始时间")]
        [Display(Name = "开始时间")]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        ///</summary>
        [Required(ErrorMessage = "请填写活动结束时间")]
        [DataType(DataType.Date, ErrorMessage = "错误的结束时间")]
        [Display(Name = "结束时间")]
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 参团人数限制
        ///</summary>
        [Range(2, 200, ErrorMessage = "成团人数只能在2-200人之间")]
        [Required(ErrorMessage = "请填写成团人数")]
        [Display(Name = "参团人数")]
        public int? LimitedNumber { get; set; }
        /// <summary>
        /// 成团时限
        ///</summary>
        [Range(0.1d, 2400d, ErrorMessage = "0.1-2400之间")]
        [Required(ErrorMessage = "请填写成团时限")]
        [RegularExpression("^[+-]?\\d+(\\.\\d+)?$", ErrorMessage = "成团时限不正确")]
        [Display(Name = "成团时限")]
        public decimal? LimitedHour { get; set; }
        /// <summary>
        /// 数量限制
        ///</summary>
        [Range(1, 1000, ErrorMessage = "错误的数量限制")]
        [Required(ErrorMessage = "请填写数量限制")]
        [Display(Name = "数量限制")]
        public int? LimitQuantity { get; set; }
        /// <summary>
        /// 组团数量
        /// </summary>
        public int? GroupCount { get; set; }
        /// <summary>
        /// 成团数量
        /// </summary>
        public int? OkGroupCount { get; set; }
        /// <summary>
        /// 活动添加时间
        ///</summary>
        public DateTime? AddTime { get; set; }
        /// <summary>
        /// 活动项
        /// </summary>
        public List<FightGroupActiveItemModel> ActiveItems { get; set; }

        /// <summary>
        ///活动是否结束
        /// </summary>
        public bool IsEnd { get; set; }
        /// <summary>
        /// 管理审核状态
        /// </summary>
        public FightGroupManageAuditStatus FightGroupManageAuditStatus { get; set; }
        /// <summary>
        /// 下架原因
        /// </summary>
        public string ManageRemark { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public Nullable<System.DateTime> ManageDate { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public Nullable<long> ManagerId { get; set; }

        /// <summary>
        /// 商品是否还有库存
        /// </summary>
        public bool HasStock { get; set; }
        /// <summary>
        /// 拼团活动状态
        /// </summary>
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
        /// 活动状态显示名称
        /// </summary>
        public string ShowActiveStatus
        {
            get
            {
                return this.ActiveStatus.ToDescription();
            }
        }

        #region 商品信息补充
        /// <summary>
        /// 商品图片目录
        /// </summary>
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 商品默认图片
        /// </summary>
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// 商品其他图片
        /// </summary>
        public List<string> ProductImages { get; set; }
        /// <summary>
        /// 运费模板
        /// </summary>
        public long FreightTemplateId { get; set; }
        /// <summary>
        /// 商品评价数
        /// </summary>
        public int ProductCommentNumber { get; set; }
        /// <summary>
        /// 商品广告语
        /// </summary>
        public string ProductShortDescription { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 商品单位
        /// </summary>
        public string MeasureUnit { get; set; }
        /// <summary>
        /// 商品是否可购买
        /// </summary>
        public bool CanBuy { get; set; }
        #endregion

        /// <summary>
        /// 火拼价
        /// </summary>
        public decimal MiniGroupPrice
        {
            get
            {
                decimal result = 0;
                if (this.ActiveItems != null)
                {
                    if (this.ActiveItems.Count > 0)
                    {
                        result = this.ActiveItems.Min(d => d.ActivePrice);
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// 最低售价
        /// </summary>
        public decimal MiniSalePrice
        {
            get
            {
                decimal result = 0;
                if (this.ActiveItems != null)
                {
                    if (this.ActiveItems.Count > 0)
                    {
                        result = this.ActiveItems.Min(d => d.ProductPrice);
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// 主图视频
        /// </summary>
        public string VideoPath { get; set; }

        /// <summary>
        /// 是否显示销量
        /// </summary>
        public bool IsSaleCountOnOff { get; set; }

        /// <summary>
        /// 拼团成功销量
        /// </summary>
        public int? SaleVolume { get; set; }

        /// <summary>
        /// 拼团对应商品的运费模板
        /// </summary>
        public Entities.FreightTemplateInfo FreightTemplate { get; set; }

        /// <summary>
        /// 商品运费文本
        /// </summary>
        public string FreightStr { get; set; }
        #endregion

        #region 方法
        /// <summary>
        /// 验证信息有效性
        /// </summary>
        public void CheckValidation()
        {
            if ( this.ProductId < 1)
            {
                throw new MallException("请选择活动商品");
            }
            if (string.IsNullOrWhiteSpace(this.IconUrl))
            {
                throw new MallException("请上传活动列表图片");
            }
            if (this.LimitedHour < 0.1m)
            {
                throw new MallException("错误的成团时限");
            }
            if (this.LimitedNumber < 1)
            {
                throw new MallException("错误的参团人数限制");
            }
            if (this.LimitQuantity < 1)
            {
                throw new MallException("错误的限购数量");
            }
            if (ActiveItems == null)
            {
                throw new MallException("错误的规格信息");
            }
            bool isstockok = false;   //最少需要一个有库存规格
            long stocksum = 0;
            foreach (var item in ActiveItems)
            {
                item.CheckValidation();
                if (item.ActiveStock > 0)
                {
                    isstockok = true;
                    stocksum += item.ActiveStock.Value;
                }
            }

            if(stocksum == 0)
            {
                throw new MallException("总活动库存必须大于0");
            }

            if(stocksum< LimitedNumber)
            {
                throw new MallException("总活动库存不可以小于成团人数限制");
            }
            if (!isstockok)
            {
                throw new MallException("最少需要一个规格有库存可用");
            }
        }
        /// <summary>
        /// 初始产品图片列表
        /// </summary>
        public void InitProductImages()
        {
            if (!isInitImagesed)
            {
                CompelInitProductImages();
            }
        }
        /// <summary>
        /// 强制初始产品图片列表
        /// </summary>
        private void CompelInitProductImages()
        {
            if (this.ProductImages == null)
            {
                this.ProductImages = new List<string>();
            }
            if (!string.IsNullOrWhiteSpace(this.ProductImgPath))
            {
                //补充图片地址
                for (var n = 2; n < 6; n++)
                {
                    var _imgurl = MallIO.GetProductSizeImage(this.ProductImgPath, n, (int)Mall.CommonModel.ImageSize.Size_350);
                    if (MallIO.ExistFile(_imgurl))
                    {
                        this.ProductImages.Add(_imgurl);
                    }
                }
            }
            isInitImagesed = true;
        }

        /// <summary>
        /// 显示时间(小于1小时显示分钟)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string ShowHourOrMinute(decimal num)
        {
            string result = num.ToString("#.##") + "小时";
            if (num < 1)
            {
                result = ((int)(num * 60)).ToString() + "分钟";
            }
            return result;
        }
        #endregion
    }
}
