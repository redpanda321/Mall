using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mall.Entities;
using Mall.DTO;

namespace Mall.Web.Areas.Admin.Models
{
    public class DistributorManagementModel
    {
        public List<DistributorGradeInfo> Grades { get; set; }
        public int DistributionMaxLevel { get; set; }
        public SiteSettings SiteSetting { get; set; }

        public long? GradeId { get; set; }
    }
}