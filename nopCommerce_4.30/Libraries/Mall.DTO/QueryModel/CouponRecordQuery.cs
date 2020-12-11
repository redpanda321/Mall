namespace Mall.DTO.QueryModel
{
    public partial class CouponRecordQuery : QueryBase
    {
        public string UserName { get; set; }

        public long? UserId { set; get; }
        public long? ShopId { get; set; }

        public int? Status { set; get; }

        public long? CouponId { set; get; }
    }
}
