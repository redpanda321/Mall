using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public class ChargeQuery : QueryBase
    {
        public ChargeDetailInfo.ChargeDetailStatus? ChargeStatus{get;set;}

        public long? memberId{get;set;}

        public long? ChargeNo { get; set; }

    }
}
