using Mall.Entities;

namespace Mall.DTO
{
    /// <summary>
    /// 充值流水
    /// </summary>
    public class ChargeDetail
    {
        long _id;

        /// <summary>
        /// 主键ID，订单号
        /// </summary>
        public long Id { get { return _id; } set { _id = value; } }
        /// <summary>
        /// 会员ID
        /// </summary>
        public long MemId { get; set; }

        /// <summary>
        /// 充值支付时间
        /// </summary>
        public System.DateTime? ChargeTime { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        public decimal ChargeAmount { get; set; }

        /// <summary>
        /// 充值方式
        /// </summary>
        public string ChargeWay { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public ChargeDetailInfo.ChargeDetailStatus ChargeStatus { get; set; }

        /// <summary>
        /// 支付申请时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 充值赠送
        /// </summary>
        public decimal? PresentAmount { get; set; }
    }
}
