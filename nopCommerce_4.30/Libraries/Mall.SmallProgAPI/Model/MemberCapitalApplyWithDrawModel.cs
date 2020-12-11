namespace Mall.SmallProgAPI.Model
{
    public class MemberCapitalApplyWithDrawModel
    {
        public MemberCapitalApplyWithDrawModel()
        {
            this.applyType = 1;
        }
        public string openId { get; set; }
        public string nickname { get; set; }
        public decimal amount { get; set; }
        public string pwd { get; set; }
        public int applyType { get; set; }
    }
}
