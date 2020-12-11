using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class Distributor : DistributorInfo
    {
        public MemberInfo Member { get; set; }
        public DistributionAchievement Achievement { get; set; }
        public DistributorGradeInfo Grade { get; set; }
       
    }
}
