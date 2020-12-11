namespace Mall.SmallProgAPI.Model
{
    public class ShippingAddressDtoModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int RegionId { get; set; }
        public string ShipTo { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsDefault { get; set; }
        public bool IsQuick { get; set; }
    }
}
