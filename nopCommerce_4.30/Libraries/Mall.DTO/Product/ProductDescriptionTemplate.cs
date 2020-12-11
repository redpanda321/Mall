using Mall.Entities;

namespace Mall.DTO
{
    public class ProductDescriptionTemplate
	{
		public long Id { get; set; }
		public long ShopId { get; set; }
		public string Name { get; set; }
		public ProductDescriptionTemplateInfo.TemplatePosition Position { get; set; }
		public string Content { get; set; }
		public string MobileContent { get; set; }
	}
}
