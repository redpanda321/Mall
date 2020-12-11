using Mall.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Web.Areas.Admin.Models.Distribution
{
    public class DistributionConfigBaseConfigModel
    {
        /// <summary>
        /// 是否启用分销功能
        /// </summary>
        public bool DistributionIsEnable { get; set; }
        /// <summary>
        /// 分销最大级数
        /// <para>最大3级</para>
        /// </summary>
        [Required(ErrorMessage = "请选择分销等级")]
        [Range(0, 3, ErrorMessage = "请选择分销等级")]
        public int DistributionMaxLevel { get; set; } = 1;
        /// <summary>
        /// 最高分佣比
        /// </summary>
        [Range(0.1, 100, ErrorMessage = "最高分佣比值需在0.1%-100%之间")]
        //[RegularExpression("^(100|(\\d{1,2}(\\.\\d){0,1}))$", ErrorMessage = "最高分佣比值需在0.1%-100%之间")]
        public decimal DistributionMaxBrokerageRate { get; set; }
        /// <summary>
        /// 非销售员-商品详情页分佣显示提示
        /// </summary>
        public bool DistributionIsProductShowTips { get; set; }
        /// <summary>
        /// 是否可以销售员自购
        /// </summary>
        public bool DistributionCanSelfBuy { get; set; }
        /// <summary>
        /// 销售员是否需要审核
        /// </summary>
        public bool DistributorNeedAudit { get; set; }
        /// <summary>
        /// 销售员申请条件
        /// <para>0表示无条件要求</para>
        /// </summary>
        [Range(0, 99999, ErrorMessage = "您输入的信息有误，此处只能输入大于0 小于 99999的整数")]
        public int DistributorApplyNeedQuota { get; set; } = 0;
    }
}