namespace Mall.DTO.QueryModel
{
    public partial class CollocationQuery : QueryBase
    {
        public string Title { get; set; }
        public long ? ShopId { get; set; }
        public int Status { get; set; }
    }
}
