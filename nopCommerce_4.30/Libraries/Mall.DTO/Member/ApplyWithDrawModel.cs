
namespace Mall.DTO
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplyWithDrawModel
    {
        public long Id { get; set; }
        public long MemId { get; set; }

        public string MemberName { get; set; }
        public string NickName { get; set; }
        public string OpenId { get; set; }
        public Mall.Entities.ApplyWithdrawInfo.ApplyWithdrawStatus ApplyStatus { get; set; }
        /// <summary>
        /// 提现申请的处理状态
        /// </summary>
        public string ApplyStatusDesc { get; set; }
        public decimal ApplyAmount { get; set; }
        public string ApplyTime { get; set; }
        public string ConfirmTime { get; set; }
        public string PayTime { get; set; }
        public string PayNo { get; set; }
        public string OpUser { get; set; }
        public string Remark { get; set; }
        public Mall.CommonModel.UserWithdrawType? ApplyType { get; set; }
        public decimal? Poundage { get; set; }
        public decimal PayAmount
        {
            get
            {
                return ApplyAmount - (Poundage ?? 0);
            }
        }
    }
}