namespace Mall.DTO
{
    public class CashDepositsObligation
    {
        /// <summary>
        /// 七天无理由退换货
        /// </summary>
        public bool IsSevenDayNoReasonReturn { get; set; }

        /// <summary>
        /// 急速发货
        /// </summary>
        public bool IsTimelyShip { get; set; }

        /// <summary>
        /// 消费者保障
        /// </summary>
        public bool IsCustomerSecurity { get; set; }

		public bool CanSelfTake { get; set; }
	}
}
