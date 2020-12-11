using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public class CapitalDetailQuery:QueryBase
    {
        public CapitalDetailInfo.CapitalDetailType? capitalType { get; set; }

        public long memberId { get; set; }

        public DateTime? startTime { get;set;}

        public DateTime? endTime{get;set;}
    }
}
