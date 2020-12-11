using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 门店公司基本信息
    /// </summary>
    public class ShopCompanyInfo
    {
        public long ShopId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public int CompanyRegionId { get; set; }
        public CompanyEmployeeCount CompanyEmployeeCount { get; set; }
    }
}
