using Mall.Entities;

namespace Mall.DTO
{
    public class MemberOpenId
	{
		public new long Id { get; set; }
		public long UserId { get; set; }
		public string OpenId { get; set; }
		public string ServiceProvider { get; set; }
		public MemberOpenIdInfo.AppIdTypeEnum AppIdType { get; set; }
		public string UnionId { get; set; }
		public string UnionOpenId { get; set; }
	}
}
