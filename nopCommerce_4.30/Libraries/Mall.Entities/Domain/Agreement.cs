using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AgreementInfo
    {
        public long Id { get; set; }
        public int AgreementType { get; set; }
        public string AgreementContent { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
