using Mall.DTO;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.Web.Models
{
    public class UserCapitalChargeModel
    {
        public CapitalInfo UserCaptialInfo { get; set; }
        /// <summary>
        /// 是否己开启充值赠送
        /// </summary>
        public bool IsEnableRechargePresent { get; set; }
        public bool CanWithdraw { get; set; }
        /// <summary>
        /// 充值赠送规则
        /// </summary>
        public List<RechargePresentRule> RechargePresentRules { get; set; }
    }
}