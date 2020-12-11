using Mall.CommonModel;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public class ApplyWithDrawQuery : QueryBase
    {
        public ApplyWithdrawInfo.ApplyWithdrawStatus? withDrawStatus { get; set; }

        public long? MemberId { get; set; }

        public long? WithDrawNo { get; set; }
        public UserWithdrawType? ApplyType { get; set; }
    }
}
