using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberIntegralExchangeRuleInfo
    {
        public long Id { get; set; }
        public int IntegralPerMoney { get; set; }
        public int MoneyPerIntegral { get; set; }
    }
}
