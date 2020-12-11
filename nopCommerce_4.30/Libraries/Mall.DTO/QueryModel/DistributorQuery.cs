using Mall.CommonModel;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public class DistributorQuery : QueryBase
    {
        public string MemberName { get; set; }
        public string ShopName { get; set; }
        public long? GradeId { get; set; }
        public string SuperiorMemberName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DistributorStatus? Status { get; set; }
        public List<long> ExcludeMemberIds { get; set; }
        /// <summary>
        /// 上级编号
        /// <para>不可单独使用，与Level搭配使用</para>
        /// </summary>
        public long? SuperiorMemberId { get; set; }
        /// <summary>
        /// 等级
        /// <para>不可单独使用，与SuperiorMemberId搭配使用</para>
        /// </summary>
        public int? Level { get; set; }
        /// <summary>
        /// 是否包含会员
        /// <para>不包含会员将只获取销售员</para>
        /// </summary>
        public bool IsIncludeMember { get; set; }
    }
}
