using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Entities;

namespace Mall.DTO.QueryModel
{
    public partial class ShopQuery : QueryBase
    {
        public long? ShopGradeId { get; set; }
        public ShopInfo.ShopAuditStatus? Status { get; set; }
        /// <summary>
        /// 多个状态
        /// <para>补充Status</para>
        /// </summary>
        public List<ShopInfo.ShopAuditStatus> MoreStatus { get; set; } = new List<Entities.ShopInfo.ShopAuditStatus>();
        public string ShopName { get; set; }
        public string ShopAccount { get; set; }
        public long CategoryId { get; set; }
        public long BrandId { get; set; }

        public DateTime? CreateDateBegin { get; set; }
        public DateTime? CreateDateEnd { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpiredDateEnd { get; set; }


        public ShopInfo.ShopStage? Stage { get; set; }
    }
}

