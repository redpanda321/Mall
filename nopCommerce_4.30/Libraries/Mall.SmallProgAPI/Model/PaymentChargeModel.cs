namespace Mall.SmallProgAPI.Model
{
    public class PaymentChargeModel
    {
        public string openId { get; set; }
        public string typeId { get; set; }
        public decimal amount { get; set; }
        public bool ispresent { get; set; }
    }
}
