namespace Mall.Web.Areas.Web.Models
{
    public class CalcFreightparameter
    {
        public long ShopId { get; set; }

        public long ProductId { get; set; }

        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}