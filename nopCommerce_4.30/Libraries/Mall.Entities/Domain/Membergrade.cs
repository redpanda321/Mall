using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberGradeInfo
    {
        public long Id { get; set; }
        public string GradeName { get; set; }
        public int Integral { get; set; }
        public string Remark { get; set; }
        public decimal Discount { get; set; }
    }
}
