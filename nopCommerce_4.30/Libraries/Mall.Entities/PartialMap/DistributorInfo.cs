using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.CommonModel.Enum;
using Mall.CommonModel;

namespace Mall.Entities
{
    public partial class DistributorInfo
    {
        /// <summary>
        /// 未结算金额(按需查询)
        /// </summary>
        [ResultColumn]
        public decimal NoSettlementAmount { get; set; }
        /// <summary>
        /// 用户名(后补)
        /// </summary>
        [ResultColumn]
        public string MemberName { get; set; }
        /// <summary>
        /// 上级用户名(后补)
        /// </summary>
        [ResultColumn]
        public string SuperiorMemberName { get; set; }
        /// <summary>
        /// 等级名称(后补)
        /// </summary>
        [ResultColumn]
        public string GradeName { get; set; }
        /// <summary>
        /// 二级下级数(后补)
        /// </summary>
        [ResultColumn]
        public int SubNumber2 { get; set; }
        /// <summary>
        /// 三级下级数(后补)
        /// </summary>
        [ResultColumn]
        public int SubNumber3 { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        [ResultColumn]
        public DateTime? RegDate { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        [ResultColumn]
        public string ShowRegDate
        {
            get
            {
                string result = "";
                if (RegDate.HasValue)
                {
                    result = RegDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return result;
            }
        }
        /// <summary>
        /// 是否仅为普通会员
        /// </summary>
        [ResultColumn]
        public bool IsOnlyMember
        {
            get
            {
                return DistributionStatus == DistributorStatus.UnApply.GetHashCode();
            }
        }
        /// <summary>
        /// 是否为正常销售员
        /// </summary>
        [ResultColumn]
        public bool IsNormalDistributor
        {
            get
            {
                return DistributionStatus == DistributorStatus.Audited.GetHashCode();
            }
        }
    }
}
