using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FreightAreaDetailInfo
    {
        public long Id { get; set; }
        public long FreightTemplateId { get; set; }
        public long FreightAreaId { get; set; }
        public int ProvinceId { get; set; }
        public int CityId { get; set; }
        public int CountyId { get; set; }
        public string TownIds { get; set; }
    }
}
