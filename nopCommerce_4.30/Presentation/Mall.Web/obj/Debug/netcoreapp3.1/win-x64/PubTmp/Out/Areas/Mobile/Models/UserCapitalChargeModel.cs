using Mall.DTO;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.Mobile.Models
{
    public class CapitalIndexChargeModel
    {
        public CapitalInfo UserCaptialInfo { get; set; }
        /// <summary>
        /// 是否已开启充值赠送
        /// </summary>
        public bool IsEnableRechargePresent { get; set; }
        /// <summary>
        /// 充值赠送规则
        /// </summary>
        public List<RechargePresentRule> RechargePresentRules { get; set; }

        /// <summary>
        /// 提现最低金额
        /// </summary>
        public int WithDrawMinimum { get; set; }

        /// <summary>
        /// 提现最高金额
        /// </summary>
        public int WithDrawMaximum { get; set; }
        public decimal RedPacketAmount { get; set; }
        public bool IsSetPwd { get; set; }
        public bool CanWithDraw { get; set; }
        public List<CapitalDetailInfo> CapitalDetails { get; set; }
    }
}