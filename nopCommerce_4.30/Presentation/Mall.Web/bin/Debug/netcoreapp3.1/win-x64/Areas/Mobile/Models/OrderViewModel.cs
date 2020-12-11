namespace Mall.Web.Areas.Mobile.Models
{
    public class ShopBranchModel
	{
		public int ShopId{get;set;}
		public int RegionId{get;set;}
		public string[] SkuIds{get;set;}
		public int[] Counts { get; set; }
	}
}