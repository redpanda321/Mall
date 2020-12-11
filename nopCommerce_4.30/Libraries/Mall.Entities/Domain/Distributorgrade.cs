using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributorGradeInfo
    {
        public long Id { get; set; }
        public string GradeName { get; set; }
        public decimal Quota { get; set; }
    }
}
