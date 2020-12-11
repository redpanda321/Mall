using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.CommonModel;

namespace Mall.DTO.Distribution
{
    public class DistributorListDTO
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long MemberId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string MemberName { get; set; }
        /// <summary>
        /// 上级编号
        /// </summary>

        public long SuperiorId { get; set; }
        /// <summary>
        /// 上级用户名
        /// </summary>
        public string SuperiorMemberName { get; set; }
        /// <summary>
        /// 所属等级
        /// </summary>
        public long GradeId { get; set; }
        /// <summary>
        /// 等级名称
        /// </summary>
        public string GradeName { get; set; }
        /// <summary>
        /// 分销订单数
        /// </summary>
        public int OrderCount { get; set; }
        /// <summary>
        /// 小店名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 小店图标
        /// </summary>
        public string ShopLogo { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public DistributorStatus DistributionStatus { get; set; }
        public string ShowDistributionStatus
        {
            get
            {
                return DistributionStatus.ToDescription();
            }
        }
        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyTime { get; set; }
        public string ShowApplyTime
        {
            get
            {
                return ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 通过时间
        /// </summary>
        public DateTime? PassTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 直接下级数
        /// </summary>
        public int SubNumber { get; set; }
        /// <summary>
        /// 二级下级数
        /// </summary>
        public int SubNumber2 { get; set; }
        /// <summary>
        /// 三级下级数
        /// </summary>
        public int SubNumber3 { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 总结算收入
        /// </summary>
        public decimal SettlementAmount { get; set; }
        /// <summary>
        /// 冻结金额
        /// </summary>
        public decimal FreezeAmount { get; set; }
        /// <summary>
        /// 已提现
        /// </summary>
        public decimal WithdrawalsAmount { get; set; }
        /// <summary>
        /// 分销成交商品数
        /// </summary>
        public int ProductCount { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? RegDate { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
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
        /// 开启了几级分销，现只导出数据用到
        /// </summary>
        public int DistributionMaxLevel { get; set; }
    }
}
