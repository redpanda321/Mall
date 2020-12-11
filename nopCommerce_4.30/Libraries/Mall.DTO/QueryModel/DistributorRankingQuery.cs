using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class DistributorRankingQuery : QueryBase
    {
        /// <summary>
        /// 批次(必填参数)
        /// </summary>
        public long BatchId { get; set; }
    }
}
