namespace Mall.SmallProgAPI.Model
{
    public class MemberOrderGetStatusModel
    {
        public long orderId { get; set; }
        public int status { get; set; }
        public long activeId { get; set; }
        public long groupId { get; set; }
    }
}
