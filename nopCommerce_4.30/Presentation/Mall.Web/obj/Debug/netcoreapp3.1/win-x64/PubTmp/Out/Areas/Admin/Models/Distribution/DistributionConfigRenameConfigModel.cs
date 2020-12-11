using Mall.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Web.Areas.Admin.Models.Distribution
{
    public class DistributionConfigRenameConfigModel
    {
        /// <summary>
        /// 分销重命名-我要开店
        /// </summary>
        [Required(ErrorMessage = "请填写我要开店重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameOpenMyShop { get; set; } = "我要开店";
        /// <summary>
        /// 分销重命名-我的小店
        /// </summary>
        [Required(ErrorMessage = "请填写我的小店重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameMyShop { get; set; } = "我的小店";
        /// <summary>
        /// 分销重命名-推广小店
        /// </summary>
        [Required(ErrorMessage = "请填写推广小店重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameSpreadShop { get; set; } = "推广小店";
        /// <summary>
        /// 分销重命名-佣金
        /// </summary>
        [Required(ErrorMessage = "请填写佣金重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameBrokerage { get; set; } = "佣金";
        /// <summary>
        /// 分销重命名-分销市场
        /// </summary>
        [Required(ErrorMessage = "请填写分销市场重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameMarket { get; set; } = "分销市场";
        /// <summary>
        /// 分销重命名-小店订单
        /// </summary>
        [Required(ErrorMessage = "请填写小店订单重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameShopOrder { get; set; } = "小店订单";
        /// <summary>
        /// 分销重命名-我的佣金
        /// </summary>
        [Required(ErrorMessage = "请填写我的佣金重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameMyBrokerage { get; set; } = "我的佣金";
        /// <summary>
        /// 分销重命名-我的下级
        /// </summary>
        [Required(ErrorMessage = "请填写我的下级重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameMySubordinate { get; set; } = "我的下级";
        /// <summary>
        /// 分销重命名-一级会员
        /// </summary>
        [Required(ErrorMessage = "请填写一级会员重命名")]
        [StringLength(4, ErrorMessage = "不可大于4个字符")]
        public string DistributorRenameMemberLevel1 { get; set; } = "一级会员";
        /// <summary>
        /// 分销重命名-二级会员
        /// </summary>
        [Required(ErrorMessage = "请填写二级会员重命名")]
        [StringLength(4, ErrorMessage = "不可大于4个字符")]
        public string DistributorRenameMemberLevel2 { get; set; } = "二级会员";
        /// <summary>
        /// 分销重命名-三级会员
        /// </summary>
        [Required(ErrorMessage = "请填写三级会员重命名")]
        [StringLength(4, ErrorMessage = "不可大于4个字符")]
        public string DistributorRenameMemberLevel3 { get; set; } = "三级会员";
        /// <summary>
        /// 分销重命名-小店设置
        /// </summary>
        [Required(ErrorMessage = "请填写小店设置重命名")]
        [StringLength(8, ErrorMessage = "不可大于8个字符")]
        public string DistributorRenameShopConfig { get; set; } = "小店设置";
    }
}